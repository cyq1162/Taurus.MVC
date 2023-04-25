using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Taurus.Controllers
{
    class TestModule:IHttpModule
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            //throw new NotImplementedException();

            TestA a = new TestA();
            a.A();
            //a.B();
        }

       
    }

    public static class AA
    {
        //public static void B(this TestA builder)
        //{

        //}
    }

    public class TestA
    {
        public void A()
        {
            A(1);
        }
        public void A(int a)
        {
            //A(a, "bbb");
        }
        public void A(int a, int b)
        {
       //....
        }
    }
}
