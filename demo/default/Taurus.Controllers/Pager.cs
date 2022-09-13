using System;
using System.Web;
using CYQ.Data.Xml;
using System.Text;
using System.Xml;

namespace Taurus.Logic
{
    public class Pager
    {
        #region 分页属性设置
        private string _PagerID = "pager";
        public string PagerID
        {
            get { return _PagerID; }
            set { _PagerID = value; }
        }

        private int _PageIndex;
        ///<summary>
        ///当前索引页
        /// <summary>
        public int PageIndex
        {
            get
            {
                if (_PageIndex == 0)
                {
                    int.TryParse(System.Web.HttpContext.Current.Request["page"], out _PageIndex);
                }
                return _PageIndex;
            }
            set
            {
                _PageIndex = value;
            }
        }

        private int _RecordCount;
        ///<summary>
        /// 总记录数
        /// </summary>
        public int RecordCount
        {
            get
            {
                return _RecordCount;
            }
            set
            {
                _RecordCount = value;
            }
        }
        private int _PageSize;
        /// <summary>
        /// 每页显示的大小条数
        /// </summary>
        public int PageSize
        {
            get
            {
                if (_PageSize == 0)
                {
                    if (!int.TryParse(System.Web.HttpContext.Current.Request["rows"], out _PageSize))
                    {
                        _PageSize = 5;
                    }
                }
                return _PageSize;
            }
            set
            {
                _PageSize = value;
            }
        }

        private string _URPara;
        /// <summary>
        /// URL重写模式时外部的参数组，如：list_isgood_{0}.html、list_type_{0}.html或{0}.html等，其中的“{0}”为数字型的页码
        /// </summary>
        public string URLPara
        {
            get
            {
                if (string.IsNullOrEmpty(_URPara))
                {
                    string url = HttpContext.Current.Request.Url.LocalPath;
                    string para = HttpContext.Current.Request.Url.PathAndQuery;
                    if (string.IsNullOrEmpty(para))
                    {
                        _URPara = url + "?page={0}";
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(url + "?page={0}");
                        foreach (string key in HttpContext.Current.Request.QueryString)
                        {
                            if (key.ToLower() != "page")
                            {
                                sb.Append("&" + key + "=" + HttpContext.Current.Request.QueryString[key]);
                            }
                        }
                        _URPara = sb.ToString();
                    }
                }
                return _URPara;
            }
            set
            {
                _URPara = value;
            }
        }
        private XHtmlAction doc;
        #endregion
        private string _InnerXml;
        public string InnerXml
        {
            get
            {
                if (string.IsNullOrEmpty(_InnerXml))
                {
                    _InnerXml = "<div class=\"pager\"><a id=\"labFirst\" >FirstPage</a> | <a id=\"labPrev\" >PrePage</a>";
                    _InnerXml += "<span id=\"labForNum\" active=\"current\"><a id=\"labNum\" href=\"#\"></a></span><a id=\"labNext\" >NextPage</a> | <a id=\"labLast\" >LastPage</a></div>";
                }
                return _InnerXml;
            }
            set
            {
                _InnerXml = value;
            }
        }

        public Pager(XHtmlAction view)
        {
            doc = view;
        }
        public void Bind(int recordCount)
        {
            _RecordCount = recordCount;
            XmlNode pagerNode = doc.Get(PagerID);
            if (pagerNode == null || _RecordCount <= _PageSize || (_RecordCount <= 0 && _PageIndex <= 1))
            {
                return;
            }
            
            int pageCount = (RecordCount % PageSize) == 0 ? RecordCount / PageSize : RecordCount / PageSize + 1;//页数
            if (PageIndex > pageCount)
            {
                return;
            }
            if (pagerNode.InnerXml == "")
            {
                pagerNode.InnerXml = InnerXml;
            }
            //if (_RecordCount <= _PageSize) //  总数不够一页。
            //{
            //    if (onlyRemoveChild.Length > 0 && onlyRemoveChild[0])
            //    {
            //        doc.RemoveAllChild(IDKey.Node_Pager);
            //        doc.RemoveAllChild(IDKey.Node_Pager2);
            //    }
            //    else
            //    {
            //        doc.Remove(IDKey.Node_Pager);
            //        doc.Remove(IDKey.Node_Pager2);//一个页面允许出现上下两个分页。
            //    }
            //    return;
            //}
            //if (_RecordCount <= 0 && _PageIndex <= 1)
            //{
            //    return;
            //}
           
            FormatFourNum(pageCount);
            FormatNum(pageCount);
            XmlNode xNode2 = doc.Get(PagerID + "2");
            if (xNode2 != null)
            {
                doc.Set(xNode2, SetType.InnerText, pagerNode.InnerXml);
            }
        }
        private void FormatFourNum(int pageCount)
        {
            if (PageIndex > 1)
            {
                doc.Set("labFirst", SetType.Href, URLPara.Replace("{0}", "1"));
                doc.Set("labPrev", SetType.Href, URLPara.Replace("{0}", (PageIndex - 1).ToString()));
            }

            if (PageIndex < pageCount)
            {
                doc.Set("labNext", SetType.Href, URLPara.Replace("{0}", (_PageIndex + 1).ToString()));
                doc.Set("labLast", SetType.Href, URLPara.Replace("{0}", pageCount.ToString()));
            }
        }
        protected void FormatNum(int pageCount)
        {
            int start = 1, end = 10;
            if (pageCount < end)//页数小于10
            {
                end = pageCount;
            }
            else
            {
                start = (PageIndex > 5) ? PageIndex - 5 : start;
                int result = (start + 9) - pageCount;//是否超过最后面的页数
                if (result > 0)
                {
                    end = pageCount;
                    start -= result;//超过后,补差
                }
                else
                {
                    end = start + 9;
                }
            }
            SetNumLink(start, end);
        }
        private void SetNumLink(int start, int end)
        {
            string numLinks = string.Empty;
            System.Xml.XmlNode node = doc.GetByID("labForNum");
            if (node != null)
            {
                string activeCss = doc.GetAttrValue(node, "active");
                System.Xml.XmlNode newNode = null;
                for (int i = end; i >= start; i--)
                {
                    doc.Set("labNum", SetType.A, i.ToString(), URLPara.Replace("{0}", i.ToString()));
                    newNode = node.Clone();
                    if (i == PageIndex && activeCss.Length > 0)
                    {
                        doc.Set(newNode, SetType.Class, activeCss);
                    }
                    doc.InsertAfter(newNode, node);
                }
                doc.Remove(node);
            }
        }

    }
}
