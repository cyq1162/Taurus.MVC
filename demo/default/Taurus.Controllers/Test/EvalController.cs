using CYQ.Data.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Taurus.Controllers
{
    public class EvalController: Taurus.Mvc.Controller
    {
        public void TestEval()
        {
         
            //  string html="<a href=\"<%# 'dir'=='dir'?'/list/${type}':'${src}' %>\"></a>";

            //if (html.IndexOf("<%#") > -1)//js eval 执行
            //{
            //    MatchCollection matchs = Regex.Matches(html, @"<%#([\S\s]*?)%>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //    if (matchs != null && matchs.Count > 0)
            //    {
            //        foreach (Match match in matchs)
            //        {
            //            //原始的占位符
            //            string value = match.Groups[0].Value;//${txt#name:xx#xx}，${'aaa'+txt#name:xx#xx+'bbb'}
            //            string value1 = match.Groups[1].Value;
            //            string evalValue = null;
            //            try
            //            {
            //                evalValue = Convert.ToString(Microsoft.JScript.Eval.JScriptEvaluate(value1, Microsoft.JScript.Vsa.VsaEngine.CreateEngine()));
            //            }
            //            catch(Exception err) 
            //            {
            //               // Log.WriteLogToTxt(err);
            //            }
            //            html = html.Replace(value, evalValue);
            //        }
            //    }
            //}

        }
    }
}
