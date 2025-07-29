using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;

namespace WMS.BLLTEST
{
    [TestClass]
    public class picktest
    {

        [TestMethod]
        public void picktest2()
        {
            ShipLoadManager eom = new ShipLoadManager();
            eom.UrlEdiTaskInsert("LD210603105033062", "02", "1761");
            
            Assert.AreEqual("Y", "");
        }

        [TestMethod]
        public void picktest3()
        {
            ShipLoadManager rm = new ShipLoadManager();

            List<PickTaskDetailResult> entityList = new List<PickTaskDetailResult>();
            PickTaskDetailResult entity = new PickTaskDetailResult();
            entity.LoadId = "LD180918140124101";
            entity.Id = 5480;
            entity.OutBoundOrderId = 5066;
            entity.PickQty = 1;

            entityList.Add(entity);

            PickTaskDetailResult entity1 = new PickTaskDetailResult();
            entity1.LoadId = "LD180918140124101";
            entity1.Id = 5484;
            entity1.OutBoundOrderId = 5067;
            entity1.PickQty = 3;

            List<PackScanNumberInsert> PackScanNumberList = new List<PackScanNumberInsert>();
            PackScanNumberInsert scan = new PackScanNumberInsert();
            scan.ScanNumber = "ABC1234567";
            PackScanNumberList.Add(scan);
            entity1.PackScanNumber = PackScanNumberList;

            entityList.Add(entity1);

            string jg = rm.PickingSortingPackingByOrerBegin(entityList, "10", "T18091804", "A02", "1761");

            Assert.AreEqual(jg, "");
        }



    }
}
