﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blqw.Web
{
    public sealed class HttpClientAsync : IHttpClient
    {
        static readonly System.Net.Http.HttpClient _Client = GetOnlyHttpClient();

        private static System.Net.Http.HttpClient GetOnlyHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            handler.MaxAutomaticRedirections = 10;
            handler.UseCookies = false;
            handler.AutomaticDecompression = DecompressionMethods.GZip;
            handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            var www = new System.Net.Http.HttpClient(handler);
            www.Timeout = new TimeSpan(0, 0, 30);
            www.MaxResponseContentBufferSize = int.MaxValue;
            return www;
        }

        public async Task<IHttpResponse> SendAsync(IHttpRequest request, CancellationToken cancellationToken)
        {
            var timer = HttpTimer.Start();
            var data = default(HttpRequestData);
            try
            {
                request.OnInitialize();
                data = new HttpRequestData(request);
                var www = GetRequest(data);
                timer.Readied();
                request.OnSending();
                using (var source1 = new CancellationTokenSource(request.Timeout))
                using (var source2 = CancellationTokenSource.CreateLinkedTokenSource(source1.Token, cancellationToken))
                {
                    var response = await _Client.SendAsync(www, source2.Token);
                    timer.Sent();
                    request.Response = (await Transfer(request.UseCookies, response));
                    request.OnEnd(request.Response);
                }
            }
            catch (Exception ex)
            {
                timer.Error();
                if (ex is TaskCanceledException)
                {
                    ex = new TimeoutException("请求已超时");
                }
                var res = new HttpResponse();
                res.Exception = ex;
                request.Response = res;
                request.OnError(res);
            }
            finally
            {
                timer.Ending();
                request.Debug(timer.ToString());
            }
            ((HttpResponse)request.Response).RequestData = data;
            return request.Response;
        }


        private async Task<HttpResponse> Transfer(bool useCookies, HttpResponseMessage response)
        {
            if (response == null)
            {
                return new HttpResponse() { StatusCode = 0 };
            }
            var contentType = (HttpContentType)response.Content.Headers.ContentType?.ToString();
            var res = new HttpResponse()
            {
                Headers = new HttpHeaders(),
            };
            using (response)
            {
                var headers = response.Headers;
                foreach (var header in headers)
                {
                    foreach (var value in header.Value)
                    {
                        res.Headers.Add(header.Key, value);
                    }
                }
                var body = await response.Content.ReadAsByteArrayAsync();
                res.Body = new HttpBody(contentType, body);
                if (useCookies)
                {
                    IEnumerable<string> cookieHeader;
                    if (response.Headers.TryGetValues("Set-Cookie", out cookieHeader))
                    {
                        var url = response.RequestMessage.RequestUri;
                        foreach (var cookie in cookieHeader)
                        {
                            _LocalCookies.SetCookies(url, cookie);
                        }
                        res.Cookies = _LocalCookies.GetCookies(url);
                    }
                }
                res.StatusCode = response.StatusCode;
                res.Status = response.ReasonPhrase;
                res.Version = $"{response.RequestMessage.RequestUri.Scheme.ToUpperInvariant()}/{response.Version}";
                res.IsSuccessStatusCode = response.IsSuccessStatusCode;
            }
            return res;
        }

        private HttpRequestMessage GetRequest(HttpRequestData data)
        {
            var request = data.Request;
            request.Debug(data.Url);
            var www = new HttpRequestMessage(GetHttpMethod(request), data.Url);
            www.Version = data.Version;
            if (data.Body != null)
            {
                www.Content = new ByteArrayContent(data.Body ?? _BytesEmpty);
            }
            foreach (var header in data.Headers)
            {
                //防止中文引起的头信息乱码
                var transfer = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(header.Value));
                if (!www.Headers.TryAddWithoutValidation(header.Key, transfer))
                {
                    www.Content?.Headers.TryAddWithoutValidation(header.Key, transfer);
                }
            }

            if (request.UseCookies)
            {
                var cookie = request.Cookies.GetCookieHeader(data.Host);
                if (string.IsNullOrWhiteSpace(cookie) == false)
                {
                    www.Headers.Add("Cookie", cookie);
                    data.Headers.Add(new KeyValuePair<string, string>("Cookie", cookie));
                }
            }

            return www;
        }

        static readonly CookieContainer _LocalCookies = new CookieContainer();
        static readonly byte[] _BytesEmpty = new byte[0];
        static readonly HttpMethod _HttpMethod_CONNECT = new HttpMethod("CONNECT");

        /// <summary> 获取 HttpMethod
        /// </summary>
        public HttpMethod GetHttpMethod(IHttpRequest request)
        {
            switch (request.Method)
            {
                case HttpRequestMethod.Get:
                    return HttpMethod.Get;
                case HttpRequestMethod.Post:
                    return HttpMethod.Post;
                case HttpRequestMethod.Head:
                    return HttpMethod.Head;
                case HttpRequestMethod.Trace:
                    return HttpMethod.Trace;
                case HttpRequestMethod.Put:
                    return HttpMethod.Put;
                case HttpRequestMethod.Delete:
                    return HttpMethod.Delete;
                case HttpRequestMethod.Options:
                    return HttpMethod.Options;
                case HttpRequestMethod.Connect:
                    return _HttpMethod_CONNECT;
                case HttpRequestMethod.Custom:
                    return new HttpMethod(request.HttpMethod);
                default:
                    return HttpMethod.Get;
            }
        }


        #region NotImplementedException

        public IAsyncResult BeginSend(IHttpRequest request, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IHttpResponse EndSend(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public IHttpResponse Send(IHttpRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
