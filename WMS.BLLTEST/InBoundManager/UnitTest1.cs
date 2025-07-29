using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WMS.BLL;
using WMS.BLLClass;
using System.Collections.Generic;

namespace WMS.BLLTEST
{
    [TestClass]
    public class UnitTestinbound
    {
        [TestMethod]
        public void eclinbound()
        {

                GMSManager gm = new GMSManager();
            //    List<TruckQueueInfo> a = new List<TruckQueueInfo>();
            //    TruckQueueInfo ti = new TruckQueueInfo();
            //    ti.truckQueueDetailParamList = new List<TruckQueueDetailParam>();
            //    ti.truckQueueHeadParam = new TruckQueueHeadParam();
            //    //ti.truckQueueHeadParam.Id            = 0;
            //    ti.truckQueueHeadParam.WhCode        = "10";
            //    ti.truckQueueHeadParam.TruckNumber   = "沪TES1111";
            //    ti.truckQueueHeadParam.PhoneNumber   = "13636668696";
            //    ti.truckQueueHeadParam.TruckStatus   = "1";
            //    ti.truckQueueHeadParam.TruckLength   = "大于17米";
            //    ti.truckQueueHeadParam.AllowTime     = null;
            //    ti.truckQueueHeadParam.AllowUser     = null;
            //    ti.truckQueueHeadParam.JumpingRemark = null;
            //    ti.truckQueueHeadParam.EntryTime     = null;
            //    ti.truckQueueHeadParam.DepartureTime = null;
            //    ti.truckQueueHeadParam.DepartureType = null;
            //    ti.truckQueueHeadParam.GreenPassFlag = 1;
            //    ti.truckQueueHeadParam.CreateUser    = "1012";
            //    ti.truckQueueHeadParam.CreateDate    = null;
            //    ti.truckQueueHeadParam.UpdateUser    = null;
            //    ti.truckQueueHeadParam.UpdateDate    = null;
            //    ti.truckQueueHeadParam.BookOrigin    = "WMS";
            //    ti.truckQueueHeadParam.WMSWhCode = "CC库";
            //    TruckQueueDetailParam td = new TruckQueueDetailParam();
            //    //td.Id = null;
            //    //td.HeadId = null;
            //    td.WhCode = "10";
            //    td.ReceiptId = "EI00012";
            //    td.UnloadingArea = "CC库";
            //    td.ClientCode = "TEST";
            //    td.BkNumber = "YU1111";
            //    td.Qty = 55;
            //    td.CBM = 12;
            //    td.Weight = 0;
            //    td.GoodsType = null;
            //    td.BkDateBegin = "2025-1-8 14:00:00.000";
            //    td.BkDateEnd = "2025-1-8 16:00:00.000";
            //    td.RegisterDate = "2025-1-8";
            //    td.BkIsValid = 1;
            //    td.SeeFlag = 0;
            //    td.SeeTime = null;
            //    td.SeeUser = null;
            //    td.OverSizeFlag = 0;
            //    td.FeesStatus = 0;
            //    td.BookOrigin = null;
            //    td.CreateUser = "1012";
            //    td.CreateDate = null;
            //    td.UpdateUser = null;
            //    td.UpdateDate = null;
            //    td.NoticeFlag = 1;
            //    td.BookChannel = "CC库";
            //    ti.truckQueueDetailParamList.Add(td);
            //    a.Add(ti);

            //string aa = gm.GetTruckQueueInfo(a);


           string aa = gm.WmsCreateGms("10", "EI25010815305610600");


            Assert.AreEqual(aa,"Y");
        }
    }
}
