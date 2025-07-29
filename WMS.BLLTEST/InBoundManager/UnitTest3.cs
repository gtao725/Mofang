using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;
using System.Linq;

namespace WMS.BLLTEST.InBoundManager
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void TestMethod1()
        {

            InVentoryManager inv = new InVentoryManager();
            List<EditHuDetailLotEntity> list = new List<EditHuDetailLotEntity>();

            EditHuDetailLotEntity en = new EditHuDetailLotEntity();
            en.WhCode = "02";
            en.ClientCode = "DM";
            en.AltItemNumber = "2380200150";
            en.LotNumber1 = "DA";
            en.Qty = 10;
            en.NewLotNumber1 = "A";
            en.UserName = "1761";
            list.Add(en);

            EditHuDetailLotEntity en1 = new EditHuDetailLotEntity();
            en1.WhCode = "02";
            en1.ClientCode = "DM";
            en1.AltItemNumber = "2352121150";
            en1.LotNumber1 = "DA";
            en1.Qty = 10;
            en1.NewLotNumber1 = "A";
            en1.UserName = "1761";
            list.Add(en1);

            List<EditHuDetailLotEntity> sss = inv.EditHuIdLot(list);

            EditHuDetailLotEntity ss = sss.First();

            string jg = "Y";

            Assert.AreEqual(jg, "");


        }
    }
}
