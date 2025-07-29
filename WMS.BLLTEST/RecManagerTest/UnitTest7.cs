using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;
using System.Threading;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest7
    {

        

    [TestMethod]
        public void TestMethod7()
        {

        RecManager rm = new RecManager();


            Thread th;
            object iii;
            for (int i = 16001; i < 16002; i++)
            {
                th = new Thread(new ParameterizedThreadStart(add));
                iii = (object)i;
                th.IsBackground = true;
                Thread.Sleep(100);
                th.Start(iii);
            }


            Thread.Sleep(10 * 1000);
            string a = "";

            Assert.AreEqual(a,"Y");
        }



        [TestMethod]
        public void add(object i)
        {
            int ii = (int)i;
            RecManager rm1=  new RecManager();

            RecModeldetail recm1 = new RecModeldetail();
            recm1.AltItemNumber = "SKU1";
            recm1.ItemId = 1;
            //recm1.UnitId= null;
            recm1.UnitName = "EA";
            recm1.UnitId = 2;
            recm1.Qty = 1;
            recm1.Length = 10;
            recm1.Width = 20;
            recm1.Height = 30;
            recm1.Weight = 0;
            recm1.LotNumber1 = "";
            recm1.LotNumber2 = "";
            recm1.LotDate = null;
            recm1.SerialNumberInModel = null;

            RecModeldetail recm2 = new RecModeldetail();
            recm2.AltItemNumber = "SKU1";
            recm2.ItemId = 1;
            //recm1.UnitId= null;
            recm2.UnitName = "BOX";
            recm2.UnitId = 1;
            recm2.Qty = 2;
            recm2.Length = 10;
            recm2.Width = 20;
            recm2.Height = 30;
            recm2.Weight = 0;
            recm2.LotNumber1 = "A";
            recm2.LotNumber2 = "";
            recm2.LotDate = null;
            recm2.SerialNumberInModel = null;


            RecModeldetail recm3 = new RecModeldetail();
            recm3.AltItemNumber = "SKU2";
            recm3.ItemId = 2;
            //recm1.UnitId= null;
            recm3.UnitName = "ECH";
            recm3.UnitId = 0;
            recm3.Qty = 3;
            recm3.Length = 10;
            recm3.Width = 10;
            recm3.Height = 10;
            recm3.Weight = 0;
            recm3.LotNumber1 = "A";
            recm3.LotNumber2 = "";
            recm3.LotDate = null;
            recm3.SerialNumberInModel = null;

            RecModeldetail recm4 = new RecModeldetail();
            recm4.AltItemNumber = "SKU2";
            recm4.ItemId = 2;
            //recm1.UnitId= null;
            recm4.UnitName = "ECH";
            recm4.UnitId = 0;
            recm4.Qty = 3;
            recm4.Length = 10;
            recm4.Width = 50;
            recm4.Height = 10;
            recm4.Weight = 0;
            recm4.LotNumber1 = "";
            recm4.LotNumber2 = "";
            recm4.LotDate = null;
            recm4.SerialNumberInModel = null;


            //List<SerialNumberInModel> SerialNumberInModel=NewsStyleUriParser;

            List<RecModeldetail> rmd = new List<RecModeldetail>();
            rmd.Add(recm1);
            rmd.Add(recm2);
            rmd.Add(recm3);
            rmd.Add(recm4);

            ReceiptInsert ri = new ReceiptInsert();

            ri.ClientCode = "TEST";
            ri.ClientId = 1;
            ri.CreateUser = "1012";
            ri.CustomerPoNumber = "PO1";
            ri.HuId = "L"+i;
            ri.Location = "A01";
            ri.LotFlag = 0;
            ri.PoId = 1;
            ri.ProcessId = 1;
            ri.ReceiptId = "EI161118152910003";
            ri.RegId = 0;
            ri.SoNumber = "SO12";
            ri.Status = "A";
            ri.WhCode = "01";
            ri.RecModeldetail = rmd;
            ri.WorkloadAccountModel = new List<WorkloadAccountModel>();

            //ri.HuId = "L14009";

            string aa=rm1.ReceiptInsert(ri);

            //Console.WriteLine("不带参数的线程函数");
            //int aaa = 1;
            Assert.AreEqual(aa, "Y");
        }


    
    }
}
