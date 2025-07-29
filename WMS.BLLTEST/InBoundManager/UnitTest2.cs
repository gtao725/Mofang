using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;

namespace WMS.BLLTEST.InBoundManager
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
 
            Grn rm = new Grn();

            string jg;
            jg = rm.AutoSendGRN("EI210422041734281", "10", "WmsAuto");
            //jg = rm.UpdateGrnWmsData("SGH5691187", "NIKE", "10", "WmsAuto1");

            Assert.AreEqual(jg, "Y");



        }
    }
}
