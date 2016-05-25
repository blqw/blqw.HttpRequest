﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.Web
{
    /// <summary>
    /// 表示一个正文解析器
    /// </summary>
    public interface IHttpBodyParser : ICustomFormatter
    {
        /// <summary>
        /// 将正文格式化为字节流
        /// </summary>
        /// <param name="format">包含格式规范的格式字符串</param>
        /// <param name="body">请求或响应正文</param>
        /// <param name="formatProvider">它提供有关当前实例的格式信息</param>
        /// <returns></returns>
        byte[] Format(string format, IEnumerable<KeyValuePair<string, object>> body, IFormatProvider formatProvider);
    }
}
