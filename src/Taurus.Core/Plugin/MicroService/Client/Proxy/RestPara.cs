using CYQ.Data.Json;
using CYQ.Data.Emit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace Taurus.Plugin.MicroService.Proxy
{
    /// <summary>
    /// Rpc Rest 调用参数基类
    /// </summary>
    public abstract class RestParaBase
    {
        /// <summary>
        /// 请求头参数
        /// </summary>
        [JsonIgnore]
        public WebHeaderCollection Headers = new WebHeaderCollection();

        /// <summary>
        /// 编码
        /// </summary>
        [JsonIgnore]
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// 获取查询字符串
        /// </summary>
        /// <returns></returns>
        public virtual string GetQueryString()
        {
            var dic = EntityToDictionary.Invoke(this);
            if (dic != null && dic.Count > 0)
            {
                return "?" + GetQueryString(dic);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取需要提交的数据。
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBytes()
        {
            var dic = EntityToDictionary.Invoke(this);
            if (dic != null && dic.Count > 0)
            {
                bool hasFile = false;
                foreach (var item in dic)
                {
                    var value = item.Value;
                    if (value != null)
                    {
                        if (value is HttpPostedFile || value is HttpFileCollection)
                        {
                            hasFile = true;
                            break;
                        }
                    }
                }
                if (hasFile)
                {
                    string boundary = "--" + DateTime.Now.Ticks.ToString("x");
                    Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
                    return GetFileBytes(dic, boundary);
                }
                else
                {
                    return GetFormBytes(dic);
                }
            }
            return null;
        }


        #region 中间过程实现 Get
        private string GetQueryString(Dictionary<string, object> dic)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in dic)
            {
                sb.Append(item.Key);
                sb.Append("=");
                var value = item.Value;
                if (value != null)
                {
                    if (value is string || value is ValueType)
                    {
                        sb.Append(value.ToString());
                    }
                    else
                    {
                        sb.Append(JsonHelper.ToJson(value));
                    }
                }
            }
            return sb.ToString();
        }

        #endregion

        #region 中间过程实现 Post
        private byte[] GetFormBytes(Dictionary<string, object> dic)
        {
            var value = GetQueryString(dic);
            return Encoding.GetBytes(value);
        }
        private byte[] GetFileBytes(Dictionary<string, object> dic, string boundary)
        {
            List<byte> bytes = new List<byte>();

            foreach (var item in dic)
            {
                var key = item.Key;
                var value = item.Value;
                if (value == null) { continue; }

                if (value is HttpPostedFile)
                {
                    AddFile(boundary, key, value as HttpPostedFile, bytes);
                }
                else if (value is HttpFileCollection)
                {
                    foreach (HttpPostedFile file in value as HttpFileCollection)
                    {
                        AddFile(boundary, key, file, bytes);
                    }
                }
                else
                {
                    if (value is string || value is ValueType)
                    {
                        AddForm(boundary, key, value.ToString(), bytes);
                    }
                    else
                    {
                        AddForm(boundary, key, JsonHelper.ToJson(value), bytes);
                    }
                }

            }
            string endBoundary = "--" + boundary + "--\r\n";
            bytes.AddRange(Encoding.GetBytes(endBoundary));
            return bytes.ToArray();
        }
        private void AddForm(string boundary, string name, string value, List<byte> bytes)
        {
            string format = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
            bytes.AddRange(Encoding.GetBytes(String.Format(format, name, value)));
        }

        private void AddFile(string boundary, string name, HttpPostedFile file, List<byte> bytes)
        {
            AddFileHeader(boundary, name, file.FileName, file.ContentType, bytes);
            using (BinaryReader reader = new BinaryReader(file.InputStream))
            {
                bytes.AddRange(reader.ReadBytes((int)file.InputStream.Length));
            }

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    byte[] buffer = new byte[file.InputStream.Length];
            //    int bytesRead;
            //    while ((bytesRead = file.InputStream.Read(buffer, 0, buffer.Length)) > 0)
            //    {
            //        ms.Write(buffer, 0, bytesRead);
            //    }

            //   // file.InputStream.CopyTo(ms);
            //    bytes.AddRange(ms.ToArray());
            //}
            AddFileEnd(bytes);
        }

        private void AddFileHeader(string boundary, string name, string fileName, string contentType, List<byte> bytes)
        {
            string format = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            byte[] headerBytes = Encoding.GetBytes(String.Format(format, name, fileName, contentType));
            bytes.AddRange(headerBytes);
        }
        private void AddFileEnd(List<byte> bytes)
        {
            string end = "\r\n";
            bytes.AddRange(Encoding.GetBytes(end));
        }

        #endregion
    }

    /// <summary>
    /// 默认无参类型
    /// </summary>
    public class RestDefaultPara : RestParaBase
    {

    }
}
