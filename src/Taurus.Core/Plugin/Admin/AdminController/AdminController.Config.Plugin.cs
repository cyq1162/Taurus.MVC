using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;
using Taurus.Plugin.CORS;
using Taurus.Plugin.Metric;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
        private void ConfigPlugin()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
            if (type.StartsWith("plugin-limit"))
            {
                if (type == "plugin-limit")
                {
                    Sets(dt, "Limit.IsIgnoreLAN", LimitConfig.IsIgnoreLAN, "limit : Ignore LAN (Local Area Network) IP address.");
                    Sets(dt, "Limit.IsIgnoreAdmin", LimitConfig.IsIgnoreAdmin, "limit : Ignore /admin path.");
                    Sets(dt, "Limit.IsIgnoreDoc", LimitConfig.IsIgnoreDoc, "limit : Ignore /doc path.");
                    Sets(dt, "Limit.IsIgnoreMicroService", LimitConfig.IsIgnoreMicroService, "limit : Ignore /microservice path.");
                    Sets(dt, "Limit.IsUseXRealIP", LimitConfig.IsUseXRealIP, "limit : Use X-Real-IP to obtain the client IP address.");
                    dt.NewRow(true);
                    Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                    Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                    Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                }
                else if (type == "plugin-limit-ip")
                {
                    Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                }
                else if (type == "plugin-limit-rate")
                {
                    Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                    Sets(dt, "Limit.Rate.MaxConcurrentConnections", LimitConfig.Rate.MaxConcurrentConnections, "Rate limit : Maximum number of open connections..");
                    Sets(dt, "Limit.Rate.Period", LimitConfig.Rate.Period + " (s)", "Rate limit : Interval period (second).");
                    Sets(dt, "Limit.Rate.Limit", LimitConfig.Rate.Limit, "Rate limit : Maximum number of requests within an interval time.");
                    Sets(dt, "Limit.Rate.Key", LimitConfig.Rate.Key, "Rate limit : Customize a key to replace IP.");

                }
                else if (type == "plugin-limit-ack")
                {
                    Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                    Sets(dt, "Limit.Ack.Key", LimitConfig.Ack.Key, "Ack limit : Secret key.");
                    Sets(dt, "Limit.Ack.IsVerifyDecode", LimitConfig.Ack.IsVerifyDecode, "Ack limit : Ack must be decode and valid.");
                    Sets(dt, "Limit.Ack.IsVerifyUsed", LimitConfig.Ack.IsVerifyUsed, "Ack limit : Ack use once only.");
                    if (IsAdmin)
                    {
                        string tip = @"        
        public static string CreateAck()<br/>
        {<br/>
            //1、key + random string. <br/>
            string rndKey = LimitConfig.Ack.Key + DateTime.Now.Ticks;<br/>
            //2、to bytes<br/>
            byte[] bytes = Encoding.ASCII.GetBytes(rndKey);<br/>
            //3、reverse<br/>
            Array.Reverse(bytes);<br/>
            //4、to Base64(replace = to #)<br/>
            string base64Key = Convert.ToBase64String(bytes);<br/>
                   base64Key = base64Key.Replace(""="", ""#"");<br/>
            //5、return # + any char + Base64<br/>
            return ""#"" + (char)(DateTime.Now.Second + 65) + base64Key;<br/>
        }";
                        Sets(dt, "Limit.Ack CreateAck()", tip, "Ack limit : Ack algorithm.");
                    }
                }
            }
            else if (type == "plugin-admin")
            {
                Sets(dt, "Admin.IsEnable", AdminConfig.IsEnable, "Admin plugin : Backend visual management plugin.");
                Sets(dt, "Admin.Path", AdminConfig.Path, "Admin url path.");
                Sets(dt, "Admin.HtmlFolderName", AdminConfig.HtmlFolderName, "Mvc view folder name for admin.");
                dt.NewRow(true);
                Sets(dt, "Admin.UserName", AdminConfig.UserName + GetOnlineText(true), "Admin account.");
                Sets(dt, "Admin.Password", string.IsNullOrEmpty(AdminConfig.Password) ? "" : "******", "Admin password.");

                string[] items = AppDataIO.Read(AdminConst.AccountPath).Split(',');
                if (items.Length == 2)
                {
                    dt.NewRow(true);
                    Sets(dt, "Admin.UserName - Setting ", items[0] + GetOnlineText(false), "Readonly account by setting.");
                    Sets(dt, "Admin.Password - Setting", items[1], "Readonly password by setting.");
                }
            }
            else if (type == "plugin-doc")
            {
                Sets(dt, "Doc.IsEnable", DocConfig.IsEnable, "Doc plugin : API automation testing plugin.");
                Sets(dt, "Doc.Path", DocConfig.Path, "Doc url path.");
                Sets(dt, "Doc.HtmlFolderName", DocConfig.HtmlFolderName, "Mvc view folder name for doc.");
                Sets(dt, "Doc.DefaultImg", DocConfig.DefaultImg, "Default images path for doc auto test,as :/App_Data/xxx.jpg");
                Sets(dt, "Doc.DefaultParas", DocConfig.DefaultParas, "Global para for doc auto test,as :ack,token");
            }
            else if (type == "plugin-metric")
            {
                Sets(dt, "Metric.IsEnable", MetricConfig.IsEnable, "Metric plugin : API metric plugin.");
                Sets(dt, "Metric.IsMvcOnly", MetricConfig.IsMvcOnly, "Metric : Ignoring requests outside of MVC.");
                Sets(dt, "Metric.IsIgnorePluginUrl", MetricConfig.IsIgnorePluginUrl, "Metric : Ignore plugin url.");
                Sets(dt, "Metric.IsDurable", MetricConfig.IsDurable, "Durable : Metric write to file.");
                Sets(dt, "Metric.DurableInterval", MetricConfig.DurableInterval+" (s)", "Durable: Interval period (second).");
                Sets(dt, "Metric.DurablePath", MetricConfig.DurablePath, "Durable : Metric【write to file】 folder name or path.");
            }
            else if (type == "plugin-cors")
            {
                Sets(dt, "CORS.IsEnable", CORSConfig.IsEnable, "Application is allow cross-origin resource sharing.");
                Sets(dt, "CORS.Methods", CORSConfig.Methods, "CORS Header：Access-Control-Allow-Methods.");
                Sets(dt, "CORS.Origin", CORSConfig.Origin, "CORS Header：Access-Control-Allow-Origin.");
                Sets(dt, "CORS.Expose", CORSConfig.Expose, "CORS Header：Access-Control-Expose-Headers.");
                Sets(dt, "CORS.Credentials", CORSConfig.Credentials, "CORS Header：Access-Control-Allow-Credentials.");
                Sets(dt, "CORS.MaxAge", CORSConfig.MaxAge + " (s)", "CORS Header：Access-Control-Max-Age.");
            }
            dt.Bind(View);
        }
    }
}
