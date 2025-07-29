using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //WhUserDAL WH = new WhUserDAL();

            //WhUser wu=new WhUser();
            //wu.CompanyId = 99;
            //wu.PassWord = "999";
            //wu.UserName = "test1";
            //wu.Status = "Active";
            //wu = WH.Add(wu);
            //WH.SaveChanges();

            RecManager rm = new RecManager();

          

            //RecModeldetail recm2 = new RecModeldetail();
            //recm2.AltItemNumber = "206712-01";
            //recm2.ItemId = 102014;
            //recm1.UnitId= null;
            //recm2.UnitName = "CTN";
            //recm2.UnitId = 102014;
            //recm2.Qty = 2;
            //recm2.Length = 10;
            //recm2.Width = 40;
            //recm2.Height = 30;
            //recm2.Weight = 0;
            //recm2.LotNumber1 = "";
            //recm2.LotNumber2 = "";
            //recm2.LotDate = null;
            //recm2.SerialNumberInModel = null;



            WorkloadAccountModel w1 = new WorkloadAccountModel();
            w1.WorkType = "装卸工";
            w1.UserCode = "123";

            WorkloadAccountModel w2 = new WorkloadAccountModel();
            w2.WorkType = "装卸工";
            w2.UserCode = "124";

            //    //List<SerialNumberInModel> SerialNumberInModel=NewsStyleUriParser;

         

            List<WorkloadAccountModel> work = new List<WorkloadAccountModel>();
            work.Add(w1);
            work.Add(w2);

            ReceiptInsert ri = new ReceiptInsert();
            ri.WhCode = "02";
            ri.ReceiptId = "EI180131110730159";
            ri.Location = "D01";
            ri.ClientCode = "TEST";
            ri.ClientId = 3;
            ri.CustomerPoNumber = "SO20170910046";
            ri.HuId = "PLTD118391";

            RecModeldetail recm1 = new RecModeldetail();
            recm1.AltItemNumber = "CCC";
            recm1.ItemId = 79116;
            recm1.UnitId = 0;
            recm1.UnitName = "EA";
            recm1.Qty = 5;
            recm1.Length = 10;
            recm1.Width = 20;
            recm1.Height = 30;
            recm1.Weight = 0;
            recm1.LotNumber1 = "";
            recm1.LotNumber2 = "";
            recm1.LotDate = null;
            recm1.SerialNumberInModel = null;

            List<RecModeldetail> rmd = new List<RecModeldetail>();
            rmd.Add(recm1);
            ri.RecModeldetail = rmd;
            ri.WorkloadAccountModel = work;
            ri.CreateUser = "1761";

            string a = rm.ReceiptInsert(ri);



            Assert.AreEqual(a, "Y");
        }
    }
}
