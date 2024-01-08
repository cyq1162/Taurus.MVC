using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Plugin.DistributedTransaction;
using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Json;

namespace Taurus.Controllers.DistributedTransationTest
{
    /// <summary>
    /// 分布式事务提供端【即服务端】
    /// </summary>
    public class DTCProvider : Taurus.Mvc.Controller
    {
        /// <summary>
        /// 添加商品
        /// </summary>
        public void AddGoods()
        {
            //追加事务回滚机制
            //DTC.Server.SubscribeAsync("1", "DTC.Goods.Add.CallBack");
            //Write("1", true);//添加成功返回商品ID
            //return;
            //1、正常执行添加商品逻辑
            using (Goods gd = new Goods())
            {
                gd.GoodsName = DateTime.Now.Ticks.ToString();
                gd.GoodsNum = DateTime.Now.Second;
                if (gd.Insert(InsertOp.ID))
                {
                    //追加事务回滚机制
                    DTC.Server.Subscribe(gd.ID.ToString(), "DTC.Goods.Add.CallBack");
                    Write(gd.ID.ToString(), true);//添加成功返回商品ID
                }
                else
                {
                    Write("商品添加失败!", false);
                }
            }
        }
        [DTCServerSubscribe("DTC.Goods.Add.CallBack")]
        private bool AddGoods_CallBack(DTCServerSubscribePara msg)
        {
            msg.ReturnContent = DateTime.Now.Ticks.ToString();
            return true;
            //if (msg.ExeType == ExeType.Commit.ToString()) { return true; }
            //if (string.IsNullOrEmpty(msg.Content)) { return false; }
            //using (Goods gd = new Goods())
            //{
            //    return gd.Delete(msg.Content);
            //}
        }

        /// <summary>
        /// 扣减商品库存
        /// </summary>
        public void ReduceGoods(int goodsID, int count)
        {
            using (Goods gd = new Goods())
            {
                gd.SetExpression("GoodsNum=GoodsNum-" + count);
                if (gd.Update("GoodsNum>0 and ID=" + goodsID))
                {
                    //追加事务回滚机制
                    DTC.Server.Subscribe(goodsID + "," + count, "DTC.Goods.Reduce.CallBack");
                    Write("商品库存扣减成功！", true);
                }
                else
                {
                    Write("商品库存扣减失败!", false);
                }
            }
        }
        [DTCServerSubscribe("DTC.Goods.Reduce.CallBack")]
        private bool ReduceGoods_CallBack(DTCServerSubscribePara msg)
        {
            if (msg.ExeType == ExeType.Commit || string.IsNullOrEmpty(msg.Content)) { return true; }
            using (Goods gd = new Goods())
            {
                string[] items = msg.Content.Split(',');
                gd.SetExpression("GoodsNum=GoodsNum+" + items[1]);
                return gd.Update(items[0]);
            }
        }
        /// <summary>
        /// 删除商品
        /// </summary>
        public void DeleteGoods(int goodsID)
        {
            using (Goods gd = new Goods())
            {
                if (gd.Fill(goodsID))
                {
                    string json = JsonHelper.ToJson(gd);
                    if (gd.Delete(goodsID))
                    {  //追加事务回滚机制
                        DTC.Server.Subscribe(json, "DTC.Goods.Delete.CallBack");
                        Write("商品删除成功！", true);
                        return;
                    }
                }
            }
            Write("商品添加失败!", false);
        }

        [DTCServerSubscribe("DTC.Goods.Delete.CallBack")]
        private bool DeleteGoods_CallBack(DTCServerSubscribePara msg)
        {
            if (msg.ExeType == ExeType.Commit) { return true; }
            using (Goods gd = new Goods())
            {
                gd.LoadFrom(msg.Content);
                gd.AllowInsertID = true;
                return gd.Insert(InsertOp.None);
            }
        }

        /// <summary>
        /// 提供另一种消息通知方法调用
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [DTCServerSubscribe("DTC.Goods.Add")]
        public static bool Goods_Add(DTCServerSubscribePara msg)
        {
            using (Goods gd = new Goods())
            {
                gd.GoodsName = DateTime.Now.Ticks.ToString();
                gd.GoodsNum = DateTime.Now.Second;
                if (gd.Insert(InsertOp.ID))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [DTCServerSubscribe("DTC.Goods.Add")]
        public static bool Goods_Add_Default(DTCServerSubscribePara msg)
        {
            msg.ReturnContent = "return value to you.";
            return true;
        }
    }

    internal class Goods : CYQ.Data.Orm.SimpleOrmBase
    {
        public Goods()
        {
            base.SetInit(this, "DTC_Demo_Goods", "MsConn");
        }
        public int? ID { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string GoodsName { get; set; }
        /// <summary>
        /// 库存数量
        /// </summary>
        public int? GoodsNum { get; set; }
    }
}
