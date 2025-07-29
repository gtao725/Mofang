using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod2()
        {

            RecManager rm = new RecManager();
            string s = rm.DelReceiptCharge("EI25070708091010309", "10","1761");

            //Grn grn = new Grn();
            //string s = grn.AutoSendGRN("EI25063013334914875", "10", "WmsAuto");

            Assert.AreEqual(s, "Y");
        }

        [TestMethod]
        public void TestMethod3()
        {
            InventoryWinceManager rm = new InventoryWinceManager();
            string a = rm.RecStockMove("10", "A01", "ARA010101", "L000001", "1761");

            //Grn grn = new Grn();
            //string s=grn.AutoSendGRN("EI200928010829737", "10", "1761");
            //string a1 = rm.RecComplete("EI191202134606108", "02", "1761");
            Assert.AreEqual(a, "Y");
        }

        void add()
        {
            RecManager rm1 = new RecManager();
            int aaa = 1;
        }
    }
}
