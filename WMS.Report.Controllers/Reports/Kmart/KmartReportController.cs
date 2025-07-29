
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class KmartReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();




        #region 1.收货异常查询

        public ActionResult ReceiptExcept()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptExceptList()
        {
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
            Hashtable SqlParameters = new Hashtable();


            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            //SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("Kmart收货异常报表", null, SqlParameters, "default:130", null, null, 50, false, ""));
        }
        #endregion


        #region 2.收货查询


        public ActionResult Receipt()
        {


            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult ReceiptList()
        {

            //string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
            Hashtable SqlParameters = new Hashtable();

            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            //SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("Kmart收货报表", null, SqlParameters, "default:130", null, null, 50, false, "ReceivedCarton实收箱数,ReceivedCBM体积"));
        }
        #endregion


        #region 3.库存查询


        public ActionResult Inventory()
        {


            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult InventoryList()
        {

            //string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
            Hashtable SqlParameters = new Hashtable();

            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            //SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("Kmart库存报表", null, SqlParameters, "default:130", null, null, 50, false, "ReceivedCarton实收箱数,ReceivedCBM体积"));
        }
        #endregion


        #region 4.未完成收货状态


        public ActionResult UnReceipt()
        {


            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult UnReceiptList()
        {

            //string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
            Hashtable SqlParameters = new Hashtable();

            //if (Request["ReceiptDateEnd"] != "")
            //{
            //    DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
            //    SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            //}
            //else
            //{
            //    SqlParameters.Add("@ReceiptDateEnd", "");
            //}
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            //SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("Kmart未完成卸货状态报表", null, SqlParameters, "default:130", null, null, 50, false, ""));
        }
        #endregion


        #region 5.收货等待报表


        public ActionResult ReceiptWait()
        {


            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult ReceiptWaitList()
        {

            //string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
            Hashtable SqlParameters = new Hashtable();

            //if (Request["ReceiptDateEnd"] != "")
            //{
            //    DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
            //    SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            //}
            //else
            //{
            //    SqlParameters.Add("@ReceiptDateEnd", "");
            //}

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            //SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("Kmart卸货等待时间报表", null, SqlParameters, "default:130", null, null, 50, false, ""));
        }
        #endregion


        #region 6.收货异常状态处理查询

        public ActionResult ReceiptExceptStatus()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult ReceiptExceptStatusList()
        {
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string EndReceiptDate = HttpContext.Request["EndReceiptDate"];

            string Status1 = HttpContext.Request["Status1"];
            Hashtable SqlParameters = new Hashtable();

            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            SqlParameters.Add("@Status1", Status1);

            return Content(EIP.ReportList("Kmart收货异常状态处理报表", null, SqlParameters, "default:130", null, null, 50, false, ""));
        }
        #endregion


        #region 7.货损及分票TCR查询

        public ActionResult CargoDamage()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult CargoDamageList()
        {
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];

            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];

            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string BeginConsDate = HttpContext.Request["BeginConsDate"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndReceiptDate"] != "")
            {
                DateTime EndReceiptDate = Convert.ToDateTime(Request["EndReceiptDate"].ToString()).AddDays(1);
                SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            }
            else
            {
                SqlParameters.Add("@EndReceiptDate", "");
            }

            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(1);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@BeginCostDate", BeginConsDate);

            return Content(EIP.ReportList("Kmart货损异常报表", null, SqlParameters, "default:130", null, null, 50, false, ""));
        }
        #endregion


        #region 8.库存释放状态查询

        public ActionResult InventoryReleaseStatus()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult InventoryReleaseStatusList()
        {

            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string CLPStatus= HttpContext.Request["CLPStatus"];


            Hashtable SqlParameters = new Hashtable();
            string BeginConsDate = HttpContext.Request["BeginConsDate"];

            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
            SqlParameters.Add("@CLPStatus", string.IsNullOrEmpty(CLPStatus) ? "" : CLPStatus);
            


            return Content(EIP.ReportList("Kmart库存释放状态报表", null, SqlParameters, "default:130", null, null, 50, false, "CLP箱数,库存箱数,weight_KGS_重量,CLP体积,库存CBM"));
        }
        #endregion


        #region 9.贴标费用查询

        public ActionResult LabelingCost()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult LabelingCostList() 
        {
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            string Labeling = HttpContext.Request["Labeling"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(1);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@SoNumber", soNumber);
            SqlParameters.Add("@PoNumber", poNumber);
            SqlParameters.Add("@BeginCostDate", BeginConsDate);
            SqlParameters.Add("@Labeling", Labeling);

            return Content(EIP.ReportList("Kmart贴标费用查询", null, SqlParameters, "default:130", null, null, 50, false, "箱数"));
        }
        #endregion


        #region 10.库存查询


        public ActionResult InventoryWeight()
        {


            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult InventoryWeightList()
        {

            //string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string ItemNumber = HttpContext.Request["ItemNumber"];
            string Exceed1480 = HttpContext.Request["Exceed1480"];
            
            Hashtable SqlParameters = new Hashtable();

            string BeginConsDate = HttpContext.Request["BeginConsDate"];
            SqlParameters.Add("@BeginConsDate", BeginConsDate);
            if (Request["EndConsDate"] != "")
            {
                DateTime EndConsDate = Convert.ToDateTime(Request["EndConsDate"].ToString()).AddDays(0);
                SqlParameters.Add("@EndConsDate", EndConsDate);
            }
            else
            {
                SqlParameters.Add("@EndConsDate", "");
            }

            if (Exceed1480 != "1")
            {
                SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
                SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(ItemNumber) ? "" : ItemNumber);
                
            }
            else
            {
                SqlParameters.Add("@Exceed1480", string.IsNullOrEmpty(Exceed1480) ? "" : Exceed1480);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);






            //SqlParameters.Add("@ClientId", WhClientId);

            if (Exceed1480 == "1")
            {
                return Content(EIP.ReportList("Kmart库存重量报表-合并托盘", null, SqlParameters, "default:130", null, null, 50, false, ""));
                
            }
            else
            {
                return Content(EIP.ReportList("Kmart库存重量报表", null, SqlParameters, "default:130", null, null, 50, false, ""));

            }
               
        }
        #endregion
    }
}
