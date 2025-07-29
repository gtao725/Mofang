using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest5
    {
        [TestMethod]
        public void TestMethod5()
        {
            RecManager rm = new RecManager();
 
            string jg;
            Grn grn = new Grn();
            jg = grn.AutoSendGRN("EI25061209361210888", "10", "1761");

            Assert.AreEqual(jg, "");
        }
    }
}
