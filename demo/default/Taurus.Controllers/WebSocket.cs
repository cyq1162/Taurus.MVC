using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Taurus.Mvc;
using Taurus.Mvc.Attr;
using CYQ.Data.Tool;
namespace Taurus.Controllers
{
    public class Msg
    {
        public Guid ID { get; set; }

        public string Url { get; set; }

        public string Header { get; set; }
        public string Body { get; set; }

    }
    public class WS : Taurus.Mvc.Controller
    {
        [WebSocket]
        public void Connection()
        {
            //if (Context.WebSockets.IsWebSocketRequest)
            //{
            try
            {
                var socketAsync = Context.WebSockets.AcceptWebSocketAsync();
                socketAsync.Wait();
                var webSocket = socketAsync.Result;
                var id = Guid.NewGuid().ToString("N");
                var buffer = ArrayPool<byte>.Shared.Rent(1024);
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var resultAsync = webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        resultAsync.Wait();
                        var result = resultAsync.Result;
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break;
                        }
                        string text = Encoding.UTF8.GetString(buffer.AsSpan(0, result.Count));

                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") +" 收到：" + text);
                        if (text.StartsWith("{") && text.EndsWith("}"))
                        {
                            Msg msg = JsonHelper.ToEntity<Msg>(text);
                            msg.Body = "ok";
                            text = JsonHelper.ToJson(msg);
                        }
                        webSocket.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None);
                        System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") +" 已返回.");
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch (Exception)
            {
            }
            // }
        }
        public void Hello(string id)
        {
            Write(id);
        }
    }

}
