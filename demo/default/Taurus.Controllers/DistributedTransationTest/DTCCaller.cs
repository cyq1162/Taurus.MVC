using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CYQ.Data.Tool;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.DistributedTransaction;
using System.Diagnostics;
using CYQ.Data.Json;

namespace Taurus.Controllers.Test
{
    /// <summary>
    /// 分布式事务测试 - 调用端，即客户端【Client】
    /// </summary>
    public class DTCCaller : Taurus.Mvc.Controller
    {
        public void Start()
        {
            //DTC.Client.DoTaskAsync("666");//通知添加3个商品
            //DTC.Client.DoTaskAsync("333", "DTC.Goods.Add");//通知添加3个商品

            RpcTask task = Rpc.StartPostAsync("http://localhost:5555/dtcprovider/AddGoods", null);//添加一行商品
            bool isOK = task.Result.IsSuccess;
            Write("OK", false);
            DTC.Client.RollBackAsync(1, "OnRollBack");//回滚
            return;

            //业务调用成功
            if (task.Result.IsSuccess)
            {
                string json = task.Result.ResultText;
                if (!JsonHelper.IsSuccess(json))
                {
                    DTC.Client.RollBackAsync(1);//回滚
                    return;
                }
                string goodsID = JsonHelper.GetValue(json, "msg");
                RpcTask task2 = Rpc.StartPostAsync(Mvc.MvcConfig.RunUrl + "/dtcprovider/ReduceGoods", Encoding.UTF8.GetBytes("goodsID=" + goodsID + "&count=1"));//扣减库存
                if (task2.Result.IsSuccess)
                {
                    DTC.Client.CommitAsync(2, "Start2.CallBack");//提交
                    Write("OK", true);
                    return;
                }
            }
            DTC.Client.RollBackAsync(2);//回滚
            Response.StatusCode = 404;
            Write("Fail", false);
        }
        [DTCClientCallBack("OnRollBack")]
        public static void OnRollBack(DTCClientCallBackPara para)
        {
            Debug.WriteLine("Client 触发回调函数：" + para.CallBackKey);
        }

        public void Start2()
        {
            Debug.WriteLine("Client 调用：Start2()，发起DoTask任务。");
            //纯消息发送
            DTC.Client.PublishTaskAsync("3", "DTC.Goods.Add", "DoTask");//通知添加3个商品
            Write("OK2");
        }
        [DTCClientCallBack("DoTask")]
        public void OnDoTask(DTCClientCallBackPara para)
        {
            Debug.WriteLine("Client 触发回调函数：" + para.CallBackKey);
        }

        public void Start3()
        {
            try
            {
                RpcTask task = Rpc.StartGetAsync("http://localhost:5112/WeatherForecast");//添加一行商品
                bool isOK = task.Result.IsSuccess;
                //纯消息发送
                DTC.Client.CommitAsync(1, "OnCommitOK");//通知添加3个商品
                Write("OK3");
            }
            catch (Exception err)
            {
                
               
            }
           
        }
        static int i = 0;
        [DTCClientCallBack("OnCommitOK")]
        public void OnCommitOK(DTCClientCallBackPara para)
        {

            i++;
            Debug.WriteLine("Client 触发回调函数：" + para.ExeType + " - " + i +" - " + para.MsgID);
            //if (para.ExeType == ExeType.Commit.ToString())
            //{
            //    DTC.Client.DoTaskAsync("abc", "callback", para.CallBackSubKey);
            //}
        }

    }
}
