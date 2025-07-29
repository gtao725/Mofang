using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;
using System.Threading;

namespace WMS.BLLTEST
{
    [TestClass]
    public class picktest2
    {

        //请点击运行测试  不要使用调试测试
        [TestMethod]
        public void feetest()
        {
            SortTaskManager rm = new SortTaskManager();

            //string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location
            //后台执行备货 
            string jg = "";
            jg = rm.AddPackTask("LD25051308223610347", "02", "1761");

            Assert.AreEqual(jg, "");
        }

        //请点击运行测试  不要使用调试测试
        [TestMethod]
        public void picktest()
        {
            ShipLoadManager rm = new ShipLoadManager();

            //后台执行备货 
            string jg = "";
            jg = rm.PickingSortingByLoad1("LD25051308223510346", "02", "1761", "PLTD810201", "SYSPLTD002", "D02");

            Assert.AreEqual(jg, "");
        }

        //请点击运行测试  不要使用调试测试
        [TestMethod]
        public void picktest1()
        {
            ShipLoadManager rm = new ShipLoadManager();

            //后台执行装箱    
            string jg = "";
            jg = rm.adminSetPackingLoad("LD190910161257371", "02", "1761");

            Assert.AreEqual(jg, "");
        }

        //请点击运行测试  不要使用调试测试
        [TestMethod]
        public void picktest3()
        {
            ShipLoadManager rm = new ShipLoadManager();

            //后台执行封箱    
            string jg = "";
            jg = rm.ShippingLoad("LD210606160758825", "10", "1761", "MSKU5861481", "CN8967936", 0);

            Assert.AreEqual(jg, "");
        }


        //请点击运行测试  不要使用调试测试
        [TestMethod]
        public void picktest4()
        {
            TransferTaskManager rm = new TransferTaskManager();

            //后台执行封箱    
            string jg = "";
            jg = rm.BeginTransferTask(23974, "1536", 101);

            Assert.AreEqual(jg, "");
        }

        [TestMethod]
        public void picktest5()
        {
            ReleaseLoadManager rel = new ReleaseLoadManager();

            string jg = rel.EditPickingSequence("LD22082313145719719", "10", 0);

            Assert.AreEqual(jg, "");
        }


        [TestMethod]
        public void picktest6()
        {
            PackTaskManager pack = new PackTaskManager();
            string jg = pack.GetSFExpressDownUrlAPI();

            Assert.AreEqual(jg, "");
        }
    }
}
