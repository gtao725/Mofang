using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;
using System.Linq;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTest3
    {

        [TestMethod]
        public void TestMethod2()
        {
            RootManager rm = new RootManager();
            int total = 0;
            List<FeeDetailResult1> list = rm.getOperationFeeList("FM24032616140610893", "10",out total);

            decimal? sumFee = list.Last().HSTotalPrice;
            string a = sumFee.ToString();

            Assert.AreEqual(a, "Y");
        }

        [TestMethod]
        public void TestMethod3()
        {
            RootManager rm = new RootManager();

            //CycleCountInsertComplex entity = new CycleCountInsertComplex();
            //entity.TaskNumber = "PD170209105010001";
            //entity.WhCode = "02";
            //entity.LocationId = "101";
            //entity.CreateUser = "1761";

            //HuIdModel hu = new HuIdModel();
            //hu.HuId = "PLTD130454";
            //hu.Qty = 50;

            //List<HuIdModel> list = new List<HuIdModel>();
            //list.Add(hu);

            //entity.HuIdModel = list;


            //string a = rm.CycleCountInsertComplex(entity);

            int aa = rm.CheckClientZone("L01", "LOC001", "10");

            //List<Pallate> pallate = new List<Pallate>();
            //    for (int i = 10000; i < 20000; i++)
            //    {
            //        pallate.Add(new Pallate() { WhCode = "01", HuId = "L"+i, TypeId = 1, Status = "U" });
            //    }
            //    int jg;
            //    jg=rm.WhPallateListAdd(pallate);
            Assert.AreEqual(aa, 1112);


        }
    }
}
