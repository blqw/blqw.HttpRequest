﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blqw.Web
{
    /// <summary>
    /// HttpRequest 扩展方法
    /// </summary>
    public static class HttpRequestExtensions
    {

        #region 同步
        /// <summary>
        /// 同步客户端
        /// </summary>
        public static readonly IHttpClient SyncClient = new HttpClientSync();

        /// <summary>
        /// 同步发送请求,返回响应体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static IHttpResponse Send(this IHttpRequest request)
        {
            return SyncClient.Send(request);
        }

        /// <summary>
        /// 同步发送请求,返回字符串
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static string GetString(this IHttpRequest request)
        {
            var res = SyncClient.Send(request);
            var str = res.Body?.ToString();
            (request as IHttpLogger)?.Debug(str);
            return str;
        }

        /// <summary>
        /// 同步发送请求,返回字节数组
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static byte[] GetBytes(this IHttpRequest request)
        {
            var res = SyncClient.Send(request);
            return res.Body?.ResponseBody;
        }

        /// <summary>
        /// 同步发送请求,返回实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static T GetObject<T>(this IHttpRequest request)
        {
            var res = SyncClient.Send(request);
            if (res.Body == null)
            {
                return default(T);
            }
            return res.Body.ToObject<T>();
        }
        #endregion

        #region 异步
        /// <summary>
        /// 异步客户端
        /// </summary>
        public static readonly IHttpClient AsyncClient = new HttpClientAsync();

        /// <summary>
        /// 异步发送请求,返回响应体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static Task<IHttpResponse> SendAsync(this IHttpRequest request)
        {
            return SendAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// 异步发送请求,设置超时时间,返回响应体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static Task<IHttpResponse> SendAsync(this IHttpRequest request, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                return SendAsync(request, tokenSource.Token);
            }
        }

        /// <summary>
        /// 异步发送请求,设置取消标识,返回响应体
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="cancellationToken">取消标识</param>
        /// <returns></returns>
        public static Task<IHttpResponse> SendAsync(this IHttpRequest request, CancellationToken cancellationToken)
        {
            return AsyncClient.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// 异步发送请求,返回字符串
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(this IHttpRequest request)
        {
            var res = await AsyncClient.SendAsync(request, CancellationToken.None);
            var str = res.Body?.ToString();
            (request as IHttpLogger)?.Debug(str);
            return str;
        }

        /// <summary>
        /// 异步发送请求,返回字节数组
        /// </summary>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytesAsync(this IHttpRequest request)
        {
            var res = await AsyncClient.SendAsync(request, CancellationToken.None);
            return res.Body?.ResponseBody;
        }

        /// <summary>
        /// 异步发送请求,返回实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">请求体</param>
        /// <returns></returns>
        public static async Task<T> GetObjectAsync<T>(this IHttpRequest request)
        {
            var res = await AsyncClient.SendAsync(request, CancellationToken.None);
            if (res.Body == null)
            {
                return default(T);
            }
            return res.Body.ToObject<T>();
        }

        /// <summary>
        /// 异步发送请求,设置超时时间,返回字符串
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(this IHttpRequest request, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                var res = await AsyncClient.SendAsync(request, tokenSource.Token);
                var str = res.Body?.ToString();
                (request as IHttpLogger)?.Debug(str);
                return str;
            }
        }

        /// <summary>
        /// 异步发送请求,设置超时时间,返回字节数组
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytesAsync(this IHttpRequest request, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                var res = await AsyncClient.SendAsync(request, tokenSource.Token);
                return res.Body?.ResponseBody;
            }
        }

        /// <summary>
        /// 异步发送请求,设置超时时间,返回实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">请求体</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        public static async Task<T> GetObjectAsync<T>(this IHttpRequest request, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                var res = await AsyncClient.SendAsync(request, tokenSource.Token);
                if (res.Body == null)
                {
                    return default(T);
                }
                return res.Body.ToObject<T>();
            }
        }

        /// <summary>
        /// 异步发送请求,设置取消标识,返回字符串
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="cancellationToken">取消标识</param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(this IHttpRequest request, CancellationToken cancellationToken)
        {
            var res = await AsyncClient.SendAsync(request, cancellationToken);
            var str = res.Body?.ToString();
            (request as IHttpLogger)?.Debug(str);
            return str;
        }

        /// <summary>
        /// 异步发送请求,设置取消标识,返回字节数组
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="cancellationToken">取消标识</param>
        /// <returns></returns>
        public static async Task<byte[]> GetBytesAsync(this IHttpRequest request, CancellationToken cancellationToken)
        {
            var res = await AsyncClient.SendAsync(request, cancellationToken);
            return res.Body?.ResponseBody;
        }

        /// <summary>
        /// 异步发送请求,设置取消标识,返回实体对象
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="cancellationToken">取消标识</param>
        /// <returns></returns>
        public static async Task<T> GetObjectAsync<T>(this IHttpRequest request, CancellationToken cancellationToken)
        {
            var res = await AsyncClient.SendAsync(request, cancellationToken);
            if (res.Body == null)
            {
                return default(T);
            }
            return res.Body.ToObject<T>();
        }
        #endregion

        #region Begin...End
        /// <summary>
        /// 异步回调客户端
        /// </summary>
        public static readonly IHttpClient CallbackClient = new HttpClientSync();

        /// <summary>
        /// 异步发送请求,并使用回调函数处理回调逻辑,也可以使用EndSend方法来接收返回值
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="callback">回调方法</param>
        /// <param name="state">需要状态回调方法的参数</param>
        /// <returns></returns>
        public static IAsyncResult BeginSend(this IHttpRequest request, AsyncCallback callback, object state)
        {
            return CallbackClient.BeginSend(request, callback, state);
        }

        /// <summary>
        /// 阻塞当前线程,直到异步操作接收到返回值
        /// </summary>
        /// <param name="request">请求体</param>
        /// <param name="asyncResult">表示一个异步操作</param>
        /// <returns></returns>
        public static IHttpResponse EndSend(this IHttpRequest request, IAsyncResult asyncResult)
        {
            return CallbackClient.EndSend(asyncResult);
        }

        #endregion
    }
}