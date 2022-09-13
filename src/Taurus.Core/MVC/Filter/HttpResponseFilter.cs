﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CYQ.Data;

namespace Taurus.Mvc
{
    /// <summary>
    /// 处理本项目部署成子应用程序时，多了一个目录的问题。
    /// </summary>
    internal class HttpResponseFilter : Stream
    {
        Stream filterStream;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stream">参数为：HttpContext.Current.Response.Filter</param>
        public HttpResponseFilter(Stream stream)
        {
            filterStream = stream;
        }
        public override bool CanRead
        {
            get { return filterStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return filterStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return filterStream.CanWrite; }
        }

        public override void Flush()
        {
            filterStream.Flush();
        }

        public override long Length
        {
            get { return filterStream.Length; }
        }

        public override long Position
        {
            get
            {
                return filterStream.Position;
            }
            set
            {
                filterStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return filterStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return filterStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            filterStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            string ct = HttpContext.Current.Response.ContentType;
            if (ct.IndexOf("image", StringComparison.OrdinalIgnoreCase) != -1)
            {
                filterStream.Write(buffer, offset, count);
                return;
            }
            //读出写的文字
            byte[] data = new byte[count];

            Buffer.BlockCopy(buffer, offset, data, 0, count);

            string html = Encoding.UTF8.GetString(data);
            //开始替换
            html = ReplaceText.Replace(html);

            //将替换后的写入response
            byte[] newdata = Encoding.UTF8.GetBytes(html);
            filterStream.Write(newdata, 0, newdata.Length);
        }
    }
    class ReplaceText
    {
        internal static string Replace(string html)
        {
            string ui = AppConfig.GetApp("UI", string.Empty);
            if (ui != string.Empty)
            {
                ui = ui.ToLower();
                html = html.Replace(" src=\"/", " src=\"" + ui + "/").Replace(" src = \"/", " src = \"" + ui + "/").Replace(" src = '/", " src = '/" + ui + "/").Replace(" src='/", " src ='/" + ui + "/"); ;
                html = html.Replace(" href=\"/", " href=\"" + ui + "/").Replace(" href = \"/", " href = \"" + ui + "/").Replace(" href='/", " href='" + ui + "/").Replace(" href = '/", " href = '" + ui + "/");
            }
            return html;
        }

    }
}
