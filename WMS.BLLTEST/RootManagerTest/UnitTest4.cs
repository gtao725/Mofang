using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using System.Collections.Generic;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest4
    {
        [TestMethod]
        public void TestMethod4()
        {
        RootManager rm = new RootManager();

            string jg;
            List<string> ClientCode = new List<string>();
            List<string> AltItemNumber = new List<string>();
            List<string> sys1 = new List<string>();
            List<string> sys2 = new List<string>();
            List<string> sys3 = new List<string>();
            List<string> UnitName = new List<string>();

            for(int i = 101000; i < 102000; i++)
            {
                ClientCode.Add("TEST");
                AltItemNumber.Add("S"+i);
                sys1.Add("");
                sys2.Add("");
                sys3.Add("");
                UnitName.Add("none");
            }

            string[] _ClientCode = ClientCode.ToArray();
            string[] _AltItemNumber = AltItemNumber.ToArray();
            string[] _sys1 = sys1.ToArray();
            string[] _sys2 = sys2.ToArray();
            string[] _sys3 = sys3.ToArray();
            string[] _UnitName = UnitName.ToArray();


            jg =rm.ItemImports(_ClientCode, _AltItemNumber, _sys1, _sys2, _sys3, _UnitName, "01","1012");
            //jg =rm.WhPallateListAdd(pallate);
         Assert.AreEqual(jg, "");
        }
    }
}
