using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    internal class DTCDebug
    {
        public static void WriteLine(string msg)
        {
            return;
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
