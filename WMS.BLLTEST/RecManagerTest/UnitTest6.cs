using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest6
    {
        [TestMethod]
        public void TestMethod6()
        {
            ShipLoadManager rm = new ShipLoadManager();

            string jg;
            //jg = rm.CheckPickingLoad("LD161214135310002", "01", "PLTD290887");
            // jg = rm.PickingLoadNotRemove("LD161214135310003", "01", "1012", "PLTD235687", "A04");

            jg = rm.PickingLoad("LD161214135310002", "01", "1012", "PLTD290887", "L19998", "A04");
            Assert.AreEqual(jg, "");
        }
        [TestMethod]
        public void PickingLoadNotRemoveTEST()
        {
            ShipLoadManager rm = new ShipLoadManager();

            string jg;
            //jg = rm.CheckPickingLoad("LD161214135310003", "01", "PLTD235687");
            // jg = rm.PickingLoadNotRemove("LD161214135310003", "01", "1012", "PLTD235687", "A04");

            jg = rm.PickingLoad("LD161214135310002", "01", "1012", "PLTD222278", "L19980", "A04");
            Assert.AreEqual(jg, "");
        }
        [TestMethod]
        public void shipingload()
        {
            ShipLoadManager rm = new ShipLoadManager();

            string jg;
            //jg = rm.CheckPickingLoad("LD161214135310003", "01", "PLTD235687");
            // jg = rm.PickingLoadNotRemove("LD161214135310003", "01", "1012", "PLTD235687", "A04");

            jg = rm.ShippingLoad("LD161214135310002", "01", "1012");
            Assert.AreEqual(jg, "");
        }
    }
}
