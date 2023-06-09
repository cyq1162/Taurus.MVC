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
                    Sets(dt, "Limit.IsIgnoreLAN", LimitConfig.IsIgnoreLAN, "limit : is ignore LAN (Local Area Network) IP address.");
                    Sets(dt, "Limit.IsIgnoreAdmin", LimitConfig.IsIgnoreAdmin, "limit : is ignore /admin path.");
                    Sets(dt, "Limit.IsIgnoreDoc", LimitConfig.IsIgnoreDoc, "limit : is ignore /doc path.");
                    Sets(dt, "Limit.IsIgnoreMicroService", LimitConfig.IsIgnoreMicroService, "limit : is ignore /microservice path.");
                    Sets(dt, "Limit.IsUseXRealIP", LimitConfig.IsUseXRealIP, "limit : is use X-Real-IP to obtain the client IP address.");
                    dt.NewRow(true);
                    Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                    Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                    Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                }
                else if (type == "plugin-limit-ip")
                {
                    Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                    Sets(dt, "Limit.IP.IsSync", LimitConfig.IP.IsSync, "IP limit : is sync ip blackname list from register center.");
                }
                else if (type == "plugin-limit-rate")
                {
                    Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                    Sets(dt, "Limit.Rate.Period", LimitConfig.Rate.Period + " (s)", "Rate limit : interval period (second).");
                    Sets(dt, "Limit.Rate.Limit", LimitConfig.Rate.Limit, "Rate limit : maximum number of requests within an interval time.");
                    Sets(dt, "Limit.Rate.Key", LimitConfig.Rate.Key, "Rate limit : can customize a key to replace IP.");

                }
                else if (type == "plugin-limit-ack")
                {
                    Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                    Sets(dt, "Limit.Ack.Key", LimitConfig.Ack.Key, "Ack limit : secret key.");
                    Sets(dt, "Limit.Ack.IsVerifyDecode", LimitConfig.Ack.IsVerifyDecode, "Ack limit : ack must be decode and valid.");
                    Sets(dt, "Limit.Ack.IsVerifyUsed", LimitConfig.Ack.IsVerifyUsed, "Ack limit : ack use once only.");
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
                        Sets(dt, "Limit.Ack CreateAck()", tip, "Ack limit : ack algorithm.");
                    }
                }
            }
            else if (type == "plugin-admin")
            {
                Sets(dt, "Admin.IsEnable", AdminConfig.IsEnable, "Admin plugin : backend visual management plugin.");
                Sets(dt, "Admin.Path", AdminConfig.Path, "Admin url path.");
                Sets(dt, "Admin.HtmlFolderName", AdminConfig.HtmlFolderName, "Mvc view folder name for admin.");
                dt.NewRow(true);
                Sets(dt, "Admin.UserName", AdminConfig.UserName + GetOnlineText(true), "Admin account.");
                Sets(dt, "Admin.Password", string.IsNullOrEmpty(AdminConfig.Password) ? "" : "******", "Admin password.");

                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
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
                Sets(dt, "Doc.DefaultParas", DocConfig.DefaultParas, "global para for doc auto test,as :ack,token");
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
            else if (type == "plugin-microservice")
            {
                #region MicroService

                Sets(dt, "MicroService Type", GetMsTypeText(), "Type of current microservice (Show Only).");
                if (MsConfig.IsServer)
                {
                    Sets(dt, "MicroService.Server.IsEnable", MsConfig.Server.IsEnable, "Microservice server (register center, gateway) plugin.");
                    Sets(dt, "MicroService.Server.Name", MsConfig.Server.Name, "Server name.");
                    Sets(dt, "MicroService.Server.RcKey", MsConfig.Server.RcKey, "Register center secret key.");
                    Sets(dt, "MicroService.Server.RcUrl", MsConfig.Server.RcUrl, "Register center url.");
                    Sets(dt, "MicroService.Server.RcUrl - 2", Server.Host2, "Register center backup url.");
                    Sets(dt, "MicroService.Server.RcPath", MsConfig.Server.RcPath, "Register center local path.");
                    Sets(dt, "MicroService.Server.GatewayTimeout", MsConfig.Server.GatewayTimeout + " (s)", "Gateway timeout (second) for request forward.");
                    Sets(dt, "MicroService Gateway Proxy LastTime", Rpc.Gateway.LastProxyTime.ToString("yyyy-MM-dd HH:mm:ss"), "The last time the proxy forwarded the request (Show Only).");
                }

                if (MsConfig.IsClient)
                {
                    if (MsConfig.IsServer)
                    {
                        dt.NewRow(true);
                    }
                    Sets(dt, "MicroService.Client.IsEnable", MsConfig.Client.IsEnable, "Microservice client plugin.");
                    Sets(dt, "MicroService.Client.IsAllowRemoteExit", MsConfig.Client.IsAllowRemoteExit, "Client is allow remote stop by register center.");
                    Sets(dt, "MicroService.Client.Name", MsConfig.Client.Name, "Client module name.");
                    Sets(dt, "MicroService.Client.Domain", MsConfig.Client.Domain, "Client bind domain.");
                    Sets(dt, "MicroService.Client.Version", MsConfig.Client.Version, "Client web version.");
                    Sets(dt, "MicroService.Client.RcKey", MsConfig.Client.RcKey, "Register center secret key.");
                    Sets(dt, "MicroService.Client.RcUrl", MsConfig.Client.RcUrl, "Register center url.");
                    Sets(dt, "MicroService.Client.RcUrl - 2", Client.Host2, "Register center backup url.");
                    Sets(dt, "MicroService.Client.RcPath", MsConfig.Client.RcPath, "Register center local path.");

                }
                #endregion
            }
            dt.Bind(View);
        }
    }
}
