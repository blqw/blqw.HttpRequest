﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using blqw.IOC;

namespace blqw.Web
{
    /// <summary>
    /// 表示一个 HTTP 请求
    /// </summary>
    public class HttpRequest : IHttpRequest
    {
        /// <summary>
        /// 参数容器
        /// </summary>
        private readonly IHttpParameterContainer _paramContainer;

        /// <summary>
        /// 用于为 <see cref="Cookies" />属性提供数据
        /// </summary>
        private CookieContainer _cookies;

        /// <summary>
        /// 用于为 <see cref="HttpMethod" />属性提供数据
        /// </summary>
        private string _httpMethod;

        /// <summary>
        /// 用于为 <see cref="Method" />属性提供数据
        /// </summary>
        private HttpRequestMethod _method;


        /// <summary>
        /// 用于为 <see cref="Response" />属性提供数据
        /// </summary>
        private IHttpResponse _response;


        /// <summary>
        /// 用于为 <see cref="Trackings" />属性提供数据
        /// </summary>
        private List<IHttpTracking> _trackings;

        /// <summary>
        /// 初始化 HTTP 请求
        /// </summary>
        public HttpRequest()
        {
            var @params = new HttpParameterContainer();
            Body = new HttpBody(@params);
            Headers = new HttpHeaders(@params);
            Query = new HttpStringParams(@params, HttpParamLocation.Query);
            PathParams = new HttpStringParams(@params, HttpParamLocation.Path);
            Params = new HttpParams(@params);
            _paramContainer = @params;
            Timeout = new TimeSpan(0, 0, 15);
            Logger = Httpdoer.DefaultLogger;
        }

        /// <summary>
        /// 初始化 HTTP 请求,并设定基路径
        /// </summary>
        /// <param name="baseUrl"> 基路径 </param>
        public HttpRequest(string baseUrl)
            : this()
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }
            if ((baseUrl.Length <= 8) || (baseUrl[0] == ':'))
            {
                baseUrl = "http://" + baseUrl;
            }
            else if ((baseUrl[3] != ':') && (baseUrl[4] != ':') && (baseUrl[5] != ':'))
            {
                baseUrl = "http://" + baseUrl;
            }

            Uri uri;
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out uri) == false)
            {
                throw new UriFormatException(nameof(baseUrl) + " 不是一个有效的Url字符串");
            }
            BaseUrl = uri;
        }

        /// <summary>
        /// 本地 Cookies 缓存
        /// </summary>
        public static CookieContainer LocalCookies { get; } = new CookieContainer();

        /// <summary>
        /// 请求操作中最后一次异常
        /// </summary>
        public Exception Exception => Response?.Exception;

        /// <summary>
        /// HTTP 请求正文
        /// </summary>
        public HttpBody Body { get; }

        /// <summary>
        /// HTTP 头信息
        /// </summary>
        public HttpHeaders Headers { get; }

        /// <summary>
        /// HTTP 请求路径参数
        /// </summary>
        public HttpStringParams PathParams { get; }

        /// <summary>
        /// HTTP 请求查询参数
        /// </summary>
        public HttpStringParams Query { get; }

        /// <summary>
        /// HTTP 参数,根据 Method 和 Path 来确定参数位置
        /// </summary>
        public HttpParams Params { get; }

        /// <summary>
        /// 基路径
        /// </summary>
        public Uri BaseUrl { get; set; }

        /// <summary>
        /// Cookie
        /// </summary>
        public CookieContainer Cookies
        {
            get { return _cookies ?? (_cookies = new CookieContainer()); }
            set { _cookies = value; }
        }

        /// <summary>
        /// 请求编码
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        public HttpRequestMethod Method
        {
            get { return _method; }
            set
            {
                _method = value;
                switch (_method)
                {
                    case HttpRequestMethod.Custom:
                    case HttpRequestMethod.Get:
                        HttpMethod = "GET";
                        _method = HttpRequestMethod.Get;
                        break;
                    case HttpRequestMethod.Post:
                        HttpMethod = "POST";
                        if (_paramContainer.Contains(HttpParamLocation.Header, "Content-Type") == false)
                        {
                            Body.ContentType = HttpContentType.Form;
                        }
                        break;
                    case HttpRequestMethod.Head:
                        HttpMethod = "Head";
                        break;
                    case HttpRequestMethod.Trace:
                        HttpMethod = "Trace";
                        break;
                    case HttpRequestMethod.Put:
                        HttpMethod = "PUT";
                        break;
                    case HttpRequestMethod.Delete:
                        HttpMethod = "DELETE";
                        break;
                    case HttpRequestMethod.Options:
                        HttpMethod = "OPTIONS";
                        break;
                    case HttpRequestMethod.Connect:
                        HttpMethod = "CONNECT";
                        break;
                    default:
                        HttpMethod = _method.ToString();
                        break;
                }
            }
        }

        /// <summary>
        /// 请求方式的字符串形式
        /// </summary>
        public string HttpMethod
        {
            get { return _httpMethod; }
            set
            {
                _httpMethod = value?.ToUpperInvariant();
                switch (_httpMethod)
                {
                    case "GET":
                        _method = HttpRequestMethod.Get;
                        break;
                    case "POST":
                        _method = HttpRequestMethod.Post;
                        if (_paramContainer.Contains(HttpParamLocation.Header, "Content-Type") == false)
                        {
                            Body.ContentType = HttpContentType.Form;
                        }
                        break;
                    case "HEAD":
                        _method = HttpRequestMethod.Head;
                        break;
                    case "TRACE":
                        _method = HttpRequestMethod.Trace;
                        break;
                    case "PUT":
                        _method = HttpRequestMethod.Put;
                        break;
                    case "DELETE":
                        _method = HttpRequestMethod.Delete;
                        break;
                    case "OPTIONS":
                        _method = HttpRequestMethod.Options;
                        break;
                    case "CONNECT":
                        _method = HttpRequestMethod.Connect;
                        break;
                    default:
                        _method = HttpRequestMethod.Custom;
                        break;
                }
            }
        }

        /// <summary>
        /// 基路径的相对路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// 获取或设置 HTTP 消息版本。默认值为 1.1。
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 枚举所有请求参数
        /// </summary>
        public IEnumerator<HttpParamValue> GetEnumerator() => _paramContainer.GetEnumerator();

        /// <summary>
        /// 最后一次响应
        /// </summary>
        public IHttpResponse Response
        {
            get { return _response; }
            set
            {
                _response = value;
                if (value?.IsSuccessStatusCode == false)
                {
                    Logger?.Write(TraceEventType.Information, $"状态码:{(int)_response.StatusCode}");
                }
            }
        }


        /// <summary>
        /// 是否使用 Cookie
        /// </summary>
        [Obsolete("使用新属性CookieMode来设置,默认为 HttpCookieMode.CustomOrCache ")]
        public bool UseCookies
        {
            get { return CookieMode != HttpCookieMode.None; }
            set { CookieMode = value ? HttpCookieMode.CustomOrCache : HttpCookieMode.None; }
        }

        /// <summary>
        /// 缓存模式
        /// </summary>
        public HttpCookieMode CookieMode { get; set; }

        /// <summary>
        /// 自动302跳转
        /// </summary>
        public bool AutoRedirect { get; set; } = true;


        /// <summary>
        /// 获取或设置日志记录器
        /// </summary>
        public TraceSource Logger { get; set; }

        /// <summary>
        /// 获取用于触发一系列事件的跟踪对象
        /// </summary>
        public List<IHttpTracking> Trackings => _trackings ?? (_trackings = new List<IHttpTracking>());

        /// <summary>
        /// 完整路径
        /// </summary>
        public Uri FullUrl => BaseUrl.Combine(Path);


        /// <summary>
        /// 返回当前请求的完成路径
        /// </summary>
        /// <returns> </returns>
        public override string ToString() => FullUrl?.ToString() ?? "http://";

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="location">参数位置</param>
        protected void SetParam(string name, object value, HttpParamLocation location)
        {
            _paramContainer.SetValue(location, name, value);
        }
    }
}