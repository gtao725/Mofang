
using System;
using System.Collections;
using System.Data;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class OutReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();

        #region 1.Load查询

        public ActionResult Load()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            ViewData["clientType"] = EIP.ReportSelect("select ClientType,ClientType from WhClientType with(nolock) where  WhCode='" + Session["whCode"].ToString() + "'", null, null);

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult LoadList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string CustomerOutPoNumber = HttpContext.Request["CustomerOutPoNumber"];
            string shipMode = HttpContext.Request["shipMode"];
            string Status0 = HttpContext.Request["Status0"];
            string Status1 = HttpContext.Request["Status1"];
            string Status3 = HttpContext.Request["Status3"];

            string ShipStatus = HttpContext.Request["ShipStatus"];
            string BeginShipDate = HttpContext.Request["BeginShipDate"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string clientType = HttpContext.Request["clientType"];

            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndShipDate"].ToString() != "")
            {
                try
                {
                    DateTime EndShipDate = Convert.ToDateTime(Request["EndShipDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndShipDate", EndShipDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndShipDate = HttpContext.Request["EndShipDate"];
                SqlParameters.Add("@EndShipDate", EndShipDate);
            }

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@CustomerOutPoNumber", CustomerOutPoNumber);
            SqlParameters.Add("@ShipMode", shipMode);
            SqlParameters.Add("@Status0", Status0);
            SqlParameters.Add("@Status1", Status1);
            SqlParameters.Add("@Status3", Status3);
            SqlParameters.Add("@ShipStatus", ShipStatus);
            SqlParameters.Add("@BeginShipDate", BeginShipDate);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);

            SqlParameters.Add("@ClientType", clientType);

            return Content(EIP.ReportList("出货Load查询", null, SqlParameters, "Load:135,客户:100,客户出库单号:130,出货数量:60,直装数量:60,挂衣数:50,总立方:55,总重量:50,ETD:110,箱号:110,箱型:50,箱型名称:150,所选流程:130,船名:130,港区:100,提单号:140,开始备货时间:120,结束备货时间:120,开始装箱时间:120,结束装箱时间:120,开始分拣时间:120,结束分拣时间:120,封箱时间:120,创建人:100,创建时间:130,default:70", null, null, 50, false, "总立方"));

        }
        public ActionResult WarehouseLoad()
        {
            ViewData["whInfoCode"] = EIP.ReportSelect("select distinct wi.WhName,wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "'  or '" + Session["isAdmin"].ToString() + "'='Y'  order by wi.WhName ", null, null);
            ViewData["clientCode"] = EIP.ReportSelect(" select distinct  ClientCode,a.ClientCode Code from WhClient a inner join WhInfo b on a.WhCode=b.WhCode  where    a.Status='Active'  and (a.WhCode in (select wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "')or  '" + Session["isAdmin"].ToString() + "'='Y') order by ClientCode", null, null);

            if (Session["isAdmin"].ToString() == "Y")
                ViewBag.CompanyId = "0";
            else
                ViewBag.CompanyId = Session["whCompany"].ToString();
            ViewBag.UserName = Session["userName"].ToString();
            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult WarehouseLoadList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string CustomerOutPoNumber = HttpContext.Request["CustomerOutPoNumber"];
            string shipMode = HttpContext.Request["shipMode"];
            string Status0 = HttpContext.Request["Status0"];
            string Status1 = HttpContext.Request["Status1"];
            string Status3 = HttpContext.Request["Status3"];

            string ShipStatus = HttpContext.Request["ShipStatus"];
            string BeginShipDate = HttpContext.Request["BeginShipDate"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string ClientCode = HttpContext.Request["ClientCode"];

            // string clientType = HttpContext.Request["clientType"];

            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndShipDate"].ToString() != "")
            {
                try
                {
                    DateTime EndShipDate = Convert.ToDateTime(Request["EndShipDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndShipDate", EndShipDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndShipDate = HttpContext.Request["EndShipDate"];
                SqlParameters.Add("@EndShipDate", EndShipDate);
            }

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }



            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@CustomerOutPoNumber", CustomerOutPoNumber);
            SqlParameters.Add("@ShipMode", shipMode);
            SqlParameters.Add("@Status0", Status0);
            SqlParameters.Add("@Status1", Status1);
            SqlParameters.Add("@Status3", Status3);
            SqlParameters.Add("@ShipStatus", ShipStatus);
            SqlParameters.Add("@BeginShipDate", BeginShipDate);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            string CompanyId = HttpContext.Request["CompanyId"];
            string UserName = HttpContext.Request["UserName"];
            SqlParameters.Add("@CompanyId", CompanyId);
            SqlParameters.Add("@UserName", UserName);
            // SqlParameters.Add("@ClientType", clientType);

            return Content(EIP.ReportList("出货Load查询(公司)", null, SqlParameters, "Load:135,出货备注:120,客户:100,客户出库单号:130,出货数量:60,直装数量:60,挂衣数:50,总立方:55,总重量:50,ETD:110,箱号:110,箱型:50,箱型名称:150,所选流程:130,船名:130,港区:100,提单号:140,开始备货时间:120,结束备货时间:120,开始装箱时间:120,结束装箱时间:120,开始分拣时间:120,结束分拣时间:120,封箱时间:120,创建人:100,创建时间:130,default:70"));

        }
        [HttpPost]
        public ActionResult OutBoundOrderList()
        {
            string LoadMasterId = HttpContext.Request["LoadMasterId"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadMasterId", LoadMasterId);
            return Content(EIP.ReportList("出货订单查询", null, SqlParameters, "Load号:123,系统出库单号:115,客户出库单号:105,客户:100,数量:50,单位:50,状态:80,Lot1:50,Lot2:50,default:100"));
        }

        [HttpPost]
        public ActionResult OutBoundOrderDetailList()
        {
            string OutBoundOrderId = HttpContext.Request["OutBoundOrderId"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@OutBoundOrderId", OutBoundOrderId);
            return Content(EIP.ReportList("出货订单明细查询", null, SqlParameters, "default:100"));
        }




        #endregion

        #region 2.Load出货进度查询

        public ActionResult LoadProcedure()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }


        [HttpPost]
        public ActionResult LoadProcedureList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string CustomerOutPoNumber = HttpContext.Request["CustomerOutPoNumber"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@CustomerOutPoNumber", CustomerOutPoNumber);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);

            return Content(EIP.ReportList("出货订单进度查询", null, SqlParameters, "Load:135,客户:110,客户出库单号:130,款号:130,操作时间:130,开始备货时间:130,结束备货时间:130,开始装箱时间:130,结束装箱时间:130,开始分拣时间:130,结束分拣时间:130,封箱时间:130,创建时间:130,default:80"));
        }

        #endregion

        #region 3.未交接包装信息查询

        public ActionResult PackTask()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult PackTaskList()
        {
            string whCode = HttpContext.Request["whCode"];
            string clientCode = HttpContext.Request["clientCode"];
            string load = HttpContext.Request["load"];
            string poNumber = HttpContext.Request["poNumber"];
            string customerOutPoNumber = HttpContext.Request["customerOutPoNumber"];
            string altcustomerOutPoNumber = HttpContext.Request["altcustomerOutPoNumber"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", clientCode);
            SqlParameters.Add("@CustomerOutPoNumber", customerOutPoNumber);
            SqlParameters.Add("@PoNumber", poNumber);
            SqlParameters.Add("@AltCustomerOutPoNumber", altcustomerOutPoNumber);

            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            return Content(EIP.ReportList("包装信息查询", null, SqlParameters, "Load:135,创建时间:130,default:130"));

        }

        [HttpPost]
        public ActionResult PackDetailList1()
        {
            string LoadId = HttpContext.Request["LoadId"];
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@WhCode", whCode);
            return Content(EIP.ReportList("包装明细查询(ByLoad)", null, SqlParameters, "default:120"));
        }

        [HttpPost]
        public ActionResult PackHeadList()
        {
            string LoadId = HttpContext.Request["LoadId"];
            string customerPoNumber = HttpContext.Request["customerPoNumber"];
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@CustomerOutPoNumber", customerPoNumber);
            return Content(EIP.ReportList("包装明细查询(ByPo)", null, SqlParameters, "default:120"));
        }


        [HttpPost]
        public ActionResult PackScanNumberList()
        {
            string LoadId = HttpContext.Request["LoadId"];
            string customerPoNumber = HttpContext.Request["customerPoNumber"];
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@CustomerOutPoNumber", customerPoNumber);
            return Content(EIP.ReportList("包装明细查询(扫描EAN)", null, SqlParameters, "default:120"));
        }

        #endregion

        #region 4.备货任务明细查询

        public ActionResult PickDetailTask()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult PickDetailTaskList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["loadId"];

            string BeginShipDate = HttpContext.Request["BeginShipDate"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string HuId = HttpContext.Request["HuId"];
            string SO = HttpContext.Request["SO"];
            string PO = HttpContext.Request["PO"];
            string SKU = HttpContext.Request["SKU"];
            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            SqlParameters.Add("@HuId", HuId);
            SqlParameters.Add("@SO", SO);
            SqlParameters.Add("@PO", PO);
            SqlParameters.Add("@SKU", SKU);
            return Content(EIP.ReportList("备货明细查询", null, SqlParameters, "Load:135,创建时间:130,封箱时间:130,流程:130,default:100"));

        }

        #endregion

        #region 5.释放有误CFS查询

        public ActionResult ReleaseLoad()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult ReleaseLoadList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["loadId"];
            string qtyWhere = HttpContext.Request["qtyWhere"];



            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            if (qtyWhere == "1")
            {
                SqlParameters.Add("@qtyWhere", qtyWhere);
            }
            else
            {
                SqlParameters.Add("@qtyWhere", "");
            }

            return Content(EIP.ReportList("释放有误CFS查询", null, SqlParameters, "Load:135,单位:50,计划数量:60,托盘:70,可用库存:60,库存数量:60,SO:130,PO:130,款号:130,LotNumber1:50,LotNumber2:50,库存类型:60,库存状态:60,库位:70,default:100"));
        }

        #endregion

        #region 6.释放有误DC查询

        public ActionResult ReleaseLoad1()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult ReleaseLoadListDC()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["loadId"];
            string qtyWhere = HttpContext.Request["qtyWhere"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            if (qtyWhere == "1")
            {
                SqlParameters.Add("@qtyWhere", qtyWhere);
            }
            else
            {
                SqlParameters.Add("@qtyWhere", "");
            }

            return Content(EIP.ReportList("释放有误DC查询", null, SqlParameters, "Load:135,单位:60,计划数量:60,可用库存:60,已锁定数量:65,库存数量:60,长:60,宽:60,高:60,重量:60,,SO:130,PO:130,款号:130,LotNumber1:60,LotNumber2:60,default:100"));

        }

        #endregion

        #region 7.交接信息查询

        public ActionResult TransferHead()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult TransferHeadList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string OutPoNumber = HttpContext.Request["OutPoNumber"];
            string CustomerOutPoNumber = HttpContext.Request["CustomerOutPoNumber"];
            string AltCustomerOutPoNumber = HttpContext.Request["AltCustomerOutPoNumber"];
            string ExpressNumber = HttpContext.Request["ExpressNumber"];
            string packCarton = HttpContext.Request["packCarton"];
            string clientCode = HttpContext.Request["clientCode"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@OutPoNumber", OutPoNumber);
            SqlParameters.Add("@CustomerOutPoNumber", CustomerOutPoNumber);
            SqlParameters.Add("@AltCustomerOutPoNumber", AltCustomerOutPoNumber);
            SqlParameters.Add("@ExpressNumber", ExpressNumber);
            SqlParameters.Add("@PackCarton", packCarton);
            SqlParameters.Add("@ClientCode", clientCode);

            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            return Content(EIP.ReportList("交接信息查询", null, SqlParameters, "分拣组号:70,包装组号:70,交接人:70,长:60,宽:60,高:60,重量:60,包装件数:70,包装立方:70,包装耗材:100,default:120"));
        }

        [HttpPost]
        public ActionResult TransferDetailList()
        {
            string LoadId = HttpContext.Request["LoadId"];
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@WhCode", whCode);
            return Content(EIP.ReportList("交接明细查询(ByLoad)", null, SqlParameters, "分拣组号:70,包装组号:70,包装数量:70,交接人:70,长:60,宽:60,高:60,重量:60,包装耗材:100,default:120"));
        }

        [HttpPost]
        public ActionResult TransferScanNumberList()
        {
            string LoadId = HttpContext.Request["LoadId"];
            string customerPoNumber = HttpContext.Request["customerPoNumber"];
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@CustomerOutPoNumber", customerPoNumber);
            return Content(EIP.ReportList("交接明细查询(扫描EAN)", null, SqlParameters, "分拣组号:70,包装组号:70,包装数量:70,交接人:70,长:60,宽:60,高:60,重量:60,包装耗材:100,default:120"));
        }

        #endregion

        #region 8.出货扫描序列号查询

        public ActionResult OutScanNumber()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult OutScanNumberList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string LoadId = HttpContext.Request["LoadId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string scanNumber = HttpContext.Request["ScanNumber"];
            string OutDateBegin = HttpContext.Request["OutDateBegin"];
            string huId = HttpContext.Request["HuId"];

            string so_number_result = "";   //定义so最终值
            if (soNumber + "" != "")
            {
                string so_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] so_strings = so_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < so_strings.Length; i++)         //循环数组
                {
                    so_number_result += "" + so_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                so_number_result = so_number_result.Substring(0, so_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["OutDateEnd"] != "")
            {
                DateTime OutDateEnd = Convert.ToDateTime(Request["OutDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@OutDateEnd", OutDateEnd);
            }
            else
            {
                SqlParameters.Add("@OutDateBegin", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@SoNumber", so_number_result);
            SqlParameters.Add("@PoNumber", poNumber);
            SqlParameters.Add("@AltItemNumber", altItemNumber);
            SqlParameters.Add("@ScanNumber", scan_number_result);
            SqlParameters.Add("@OutDateBegin", OutDateBegin);
            SqlParameters.Add("@HuId", huId);
            SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("集装箱出货扫描序列号查询", null, SqlParameters, "Load号:130,收货时间:110,托盘:80,理货员:80,序列号:130,出货时间:125,default:100"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 9.出货超期查询

        public ActionResult OutChaoQi()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }


        [HttpPost]
        public ActionResult OutChaoQiList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string ContainerNumber = HttpContext.Request["ContainerNumber"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string BeginPackDate = HttpContext.Request["BeginPackDate"];
            string tianshu = HttpContext.Request["tianshu"];

            Hashtable SqlParameters = new Hashtable();

            string scan_number_result = "";   //定义so最终值
            if (ContainerNumber + "" != "")
            {
                string scan_temp = ContainerNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }
            if (Request["EndPackDate"].ToString() != "")
            {
                try
                {
                    DateTime EndPackDate = Convert.ToDateTime(Request["EndPackDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndPackDate", EndPackDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndPackDate = HttpContext.Request["EndPackDate"];
                SqlParameters.Add("@EndPackDate", EndPackDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@Tianshu", tianshu);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            SqlParameters.Add("@BeginPackDate", BeginPackDate);

            return Content(EIP.ReportList("集装箱出货超期查询", null, SqlParameters, "Load:135,客户:110,箱号:100,箱型:50,SO号:120,PO号:120,装箱时间:120,收货时间:120,default:60"));
        }

        #endregion

        #region 10.装箱状态查询

        public ActionResult Load_Status()
        {
            ViewData["WeightFlag"] = EIP.ReportSelect("select '未称重' name,'0' flag  UNION  select '已称重' name,'1' flag ", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult Load_StatusList()
        {
            string WeightFlag = HttpContext.Request["WeightFlag"];

            string whCode = HttpContext.Request["whCode"];
            string ContainerNumber = HttpContext.Request["ContainerNumber"];
            string OutDateBegin = HttpContext.Request["OutDateBegin"];
            string OutDateEnd = HttpContext.Request["OutDateEnd"];


            string ContainerNumber_result = "";   //定义ContainerNumber最终值
            if (ContainerNumber + "" != "")
            {
                string Container_temp = ContainerNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] Container_strings = Container_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < Container_strings.Length; i++)         //循环数组
                {
                    ContainerNumber_result += "'" + Container_strings[i] + "',";           //取得so,并且前后加上单引号
                }
                ContainerNumber_result = ContainerNumber_result.Substring(0, ContainerNumber_result.Length - 1);  //最后 定义ContainerNumber最终值最终值需减去1位
            }


            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);

            string sql = @"SELECT 
                            a.LoadId,
                            a.CreateDate 创建时间,
                            ContainerNumber 箱号,
                            case when b.Status0='C' THEN '已释放'  WHEN   b.Status0 IS NULL THEN ''  else '未释放' end  释放状态,
                            case when b.Status1='C' THEN '完成备货' WHEN   b.Status1 IS NULL THEN ''   else '未完成备货' end 备货状态,
                            case when b.Status3='C' THEN '完成装箱' WHEN   b.Status3 IS NULL THEN ''   else '未完成装箱' end 装箱状态,
                            b.ShipDate 封箱时间,
                            sum(d.Qty) 货物数量,
                            case when a.WeightFlag=1 THEN '需称重'  ELSE '' end 称重需求,
                            a.WeightDate 称重时间,
                            a.NetWeight 净重,
                            a.GrossWeight 毛重
                            from 
                            LoadContainerExtend  a
                            LEFT join LoadMaster b on a.WhCode=b.WhCode AND a.LoadId=b.LoadId
                            LEFT join LoadDetail c on c.LoadMasterId=b.Id
                            LEFT join OutBoundOrderDetail d ON d.OutBoundOrderId=c.OutBoundOrderId
                            WHERE 1=1 #where#
                            group by 
                            a.LoadId,
                            ContainerNumber,
                            b.Status0,
                            b.Status1,
                            b.Status3,
                            b.ShipDate,
                            a.WeightFlag,
                            a.WeightDate,
                            a.CreateDate,
                            a.NetWeight,
                            a.GrossWeight";

            string SqlCondition = "";
            if (whCode + "" != "")
            {
                SqlCondition += " and  a.WhCode='" + whCode + "' ";
            }
            else
            {
                return null;
            }

            if (ContainerNumber + "" != "")
            {
                SqlCondition += " and a.ContainerNumber in( " + ContainerNumber_result + ")  ";
            }

            if (OutDateBegin + "" != "")
            {
                SqlCondition += " and a.CreateDate >= '" + OutDateBegin + "'  ";
            }
            if (OutDateEnd + "" != "")
            {
                SqlCondition += " and a.CreateDate <='" + OutDateEnd + "'  ";
            }
            sql = sql.Replace("#where#", SqlCondition);



            //return Content(EIP.ReportList("LOAD状态查询", null, SqlParameters, "收货批次号:110,收货时间:110,托盘:80,理货员:80,序列号:130,default:100"));
            return Content(EIP.ReportList(null, sql, SqlParameters, "收货批次号:110,收货时间:110,托盘:80,理货员:80,序列号:130,default:100"));


            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 11.已出运柜查询
        public ActionResult Cabinets()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            return View();
        }


        [HttpPost]
        public ActionResult CabinetsList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string Status = HttpContext.Request["Status"];

            string BeginShipDate = HttpContext.Request["BeginShipDate"];

            string ClientCode = HttpContext.Request["ClientCode"];

            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndShipDate"].ToString() != "")
            {
                try
                {
                    DateTime EndShipDate = Convert.ToDateTime(Request["EndShipDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndShipDate", EndShipDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndShipDate = HttpContext.Request["EndShipDate"];
                SqlParameters.Add("@EndShipDate", EndShipDate);
            }


            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@Status", Status);
            SqlParameters.Add("@BeginShipDate", BeginShipDate);
            return Content(EIP.ReportList("已出运柜查询", null, SqlParameters, "Load:137,出仓日期:120,箱号:120,客户:120,称重日期:130,装箱单箱货总重:90,差异率:70,default:80"));

        }

        #endregion

        #region 12.未出运柜查询
        public ActionResult NoCabinets()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            ViewData["zoneName"] = EIP.ReportSelect("select  b.ZoneName,b.ZoneName from WhClient a inner join Zones b on a.ZoneId = b.Id where  a.WhCode = '" + Session["whCode"].ToString() + "' and b.RegFlag = 1 group by  b.ZoneName", null, null);

            return View();
        }


        [HttpPost]
        public ActionResult NoCabinetsList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string Status = HttpContext.Request["Status"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string scanNumber = HttpContext.Request["ContainerNumber"];
            string zoneName = HttpContext.Request["zoneName"];
            string vessName = HttpContext.Request["vessName"];


            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@Status", Status);
            SqlParameters.Add("@zoneName", zoneName);
            SqlParameters.Add("@vessName", vessName);

            return Content(EIP.ReportList("未出运柜查询", null, SqlParameters, "Load:135,ETD:80,出仓方式:70,库区:50,出货数量:70,装柜数量:70,出货立方:70,是否称重:70,备货状态:70,装箱状态:70,default:120", null, null, 50, false, "出货立方"));

        }

        #endregion

        #region 13.Load1查询（已出信息查询）

        public ActionResult Load1()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult Load1List()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];

            string BeginShipDate = HttpContext.Request["BeginShipDate"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string sealNumber = HttpContext.Request["SealNumber"];

            string seal_number_result = "";   //定义so最终值
            if (sealNumber + "" != "")
            {
                string scan_temp = sealNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    seal_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                seal_number_result = seal_number_result.Substring(0, seal_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string soNumber = HttpContext.Request["SoNumber"];

            string so_number_result = "";   //定义so最终值
            if (soNumber + "" != "")
            {
                string scan_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        so_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                so_number_result = so_number_result.Substring(0, so_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string poNumber = HttpContext.Request["PoNumber"];

            string po_number_result = "";   //定义so最终值
            if (poNumber + "" != "")
            {
                string scan_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        po_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                po_number_result = po_number_result.Substring(0, po_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string itemNumber = HttpContext.Request["ItemNumber"];

            string item_number_result = "";   //定义so最终值
            if (itemNumber + "" != "")
            {
                string scan_temp = itemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    item_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                item_number_result = item_number_result.Substring(0, item_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndShipDate"].ToString() != "")
            {
                try
                {
                    DateTime EndShipDate = Convert.ToDateTime(Request["EndShipDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndShipDate", EndShipDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndShipDate = HttpContext.Request["EndShipDate"];
                SqlParameters.Add("@EndShipDate", EndShipDate);
            }

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@SoNumber", so_number_result);
            SqlParameters.Add("@PoNumber", po_number_result);
            SqlParameters.Add("@SealNumber", seal_number_result);
            SqlParameters.Add("@AltItemNumber", item_number_result);

            SqlParameters.Add("@BeginShipDate", BeginShipDate);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            return Content(EIP.ReportList("出货Load查询1", null, SqlParameters, "Load:135,客户:100,出货数量:60,直装数量:60,挂衣数:50,总立方:55,总重量:50,ETD:110,箱号:110,箱型:50,箱型名称:150,所选流程:130,船名:130,港区:100,提单号:140,开始备货时间:120,结束备货时间:120,开始装箱时间:120,结束装箱时间:120,开始分拣时间:120,结束分拣时间:120,封箱时间:120,创建人:100,创建时间:130,SO号:130,PO:130,款号:130,数量:55,单位:55,default:70", null, null, 50, false, "数量"));

        }


        #endregion

        #region 14.Load2查询（未出信息查询）

        public ActionResult Load2()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult Load2List()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];

            string BeginShipDate = HttpContext.Request["BeginShipDate"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string sealNumber = HttpContext.Request["SealNumber"];

            string seal_number_result = "";   //定义so最终值
            if (sealNumber + "" != "")
            {
                string scan_temp = sealNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    seal_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                seal_number_result = seal_number_result.Substring(0, seal_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string soNumber = HttpContext.Request["SoNumber"];

            string so_number_result = "";   //定义so最终值
            if (soNumber + "" != "")
            {
                string scan_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        so_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                so_number_result = so_number_result.Substring(0, so_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string poNumber = HttpContext.Request["PoNumber"];

            string po_number_result = "";   //定义so最终值
            if (poNumber + "" != "")
            {
                string scan_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        po_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                po_number_result = po_number_result.Substring(0, po_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            string itemNumber = HttpContext.Request["ItemNumber"];

            string item_number_result = "";   //定义so最终值
            if (itemNumber + "" != "")
            {
                string scan_temp = itemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    item_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                item_number_result = item_number_result.Substring(0, item_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndShipDate"].ToString() != "")
            {
                try
                {
                    DateTime EndShipDate = Convert.ToDateTime(Request["EndShipDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndShipDate", EndShipDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndShipDate = HttpContext.Request["EndShipDate"];
                SqlParameters.Add("@EndShipDate", EndShipDate);
            }

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@SoNumber", so_number_result);
            SqlParameters.Add("@PoNumber", po_number_result);
            SqlParameters.Add("@SealNumber", seal_number_result);
            SqlParameters.Add("@AltItemNumber", item_number_result);

            SqlParameters.Add("@BeginShipDate", BeginShipDate);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            return Content(EIP.ReportList("出货Load查询2", null, SqlParameters, "Load:135,客户:100,出货数量:60,直装数量:60,挂衣数:50,总立方:55,总重量:50,ETD:110,箱号:110,箱型:50,箱型名称:150,所选流程:130,船名:130,港区:100,提单号:140,开始备货时间:120,结束备货时间:120,开始装箱时间:120,结束装箱时间:120,开始分拣时间:120,结束分拣时间:120,封箱时间:120,创建人:100,创建时间:130,SO号:130,PO:130,款号:130,数量:55,单位:55,default:70", null, null, 50, false, "数量"));

        }


        #endregion

        #region 15.装箱单查询

        public ActionResult LoadContainer()
        {
            ViewData["CreateUserName"] = EIP.ReportSelect("select distinct b.UserNameCN,a.CreateUser from LoadContainerExtend a with(nolock) left join WhUser  b with(nolock) on a.CreateUser = b.UserName where a.WhCode = '" + Session["whCode"].ToString() + "' and b.Status='Active' order by 1 ", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult LoadContainerList()
        {
            string createUser = HttpContext.Request["createUser"];
            string clientCode = HttpContext.Request["ClientCode"];

            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string BillNumber = HttpContext.Request["BillNumber"];
            string sealNumber = HttpContext.Request["sealNumber"];

            string shipMode = HttpContext.Request["shipMode"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string BeginETD = HttpContext.Request["BeginETD"];
            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }
            if (Request["EndETD"].ToString() != "")
            {
                try
                {
                    DateTime EndETD = Convert.ToDateTime(Request["EndETD"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndETD", EndETD);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndETD = HttpContext.Request["EndETD"];
                SqlParameters.Add("@EndETD", EndETD);
            }

            SqlParameters.Add("@CreateUser", createUser);
            SqlParameters.Add("@ClientCode", clientCode);

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@SealNumber", sealNumber);
            SqlParameters.Add("@ShipMode", shipMode);
            SqlParameters.Add("@BillNumber", BillNumber);
            SqlParameters.Add("@BeginETD", BeginETD);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);

            return Content(EIP.ReportList("装箱单查询", null, SqlParameters, "Load:135,客户名:120,出货数量:60,直装数量:60,挂衣数:50,总立方:55,总重量:50,ETD:110,箱号:110,箱型:50,箱型名称:150,船名:130,港区:100,提单号:140,创建人:70,创建时间:130,default:70"));

        }

        #endregion

        #region 16.MLOG出货月报表查询

        public ActionResult MlogMonth()
        {
            ViewData["YearCreateDate"] = EIP.ReportSelect("select  YEAR(GETDATE()) union select  YEAR(GETDATE())-1 ", null, null);
            ViewData["MonthCreateDate"] = EIP.ReportSelect(" select 1 union select 2 union select 3 union select 4 union select 5 union select 6 union select 7 union select 8 union select 9 union select 10 union select 11 union select 12  ", null, null);

            ViewBag.whCode = Session["whCode"].ToString();

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult MlogMonthList()
        {
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            string YearCreateDate = HttpContext.Request["YearCreateDate"];
            int MonthCreateDate = Convert.ToInt32(HttpContext.Request["MonthCreateDate"]);

            string begin = "", end = "";
            if (MonthCreateDate < 10)
            {
                begin = YearCreateDate + "-0" + MonthCreateDate + "-01";
                if (MonthCreateDate < 9)
                {
                    end = YearCreateDate + "-0" + (MonthCreateDate + 1) + "-01";
                }
                else
                {
                    end = YearCreateDate + "-" + (MonthCreateDate + 1) + "-01";
                }
            }
            else
            {
                begin = YearCreateDate + "-" + MonthCreateDate + "-01";
                end = YearCreateDate + "-" + (MonthCreateDate + 1) + "-01";
            }

            SqlParameters.Add("@YearCreateDate", begin);
            SqlParameters.Add("@MonthCreateDate", end);
            SqlParameters.Add("@WhCode", whCode);

            return Content(EIP.ReportList("MLOG出货月报表查询", null, SqlParameters, "工人:50,工人类型:60,立方:60,数量:60,default:130", null, null, 50, false, "立方,数量"));

        }

        #endregion



        #region 17.装箱单明细查询

        public ActionResult LoadContainerDetail()
        {
            ViewData["CreateUserName"] = EIP.ReportSelect("select distinct b.UserNameCN,a.CreateUser from LoadContainerExtend a with(nolock) left join WhUser  b with(nolock) on a.CreateUser = b.UserName where a.WhCode = '" + Session["whCode"].ToString() + "' and b.Status='Active' order by 1 ", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            //return null;
            return View();
        }

        [HttpPost]
        public ActionResult LoadContainerDetailList()
        {
            string createUser = HttpContext.Request["createUser"];
            string clientCode = HttpContext.Request["ClientCode"];

            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string BillNumber = HttpContext.Request["BillNumber"];
            string sealNumber = HttpContext.Request["sealNumber"];

            string shipMode = HttpContext.Request["shipMode"];

            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string BeginETD = HttpContext.Request["BeginETD"];
            string scanNumber = HttpContext.Request["ContainerNumber"];

            string scan_number_result = "";   //定义so最终值
            if (scanNumber + "" != "")
            {
                string scan_temp = scanNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndCreateDate"].ToString() != "")
            {
                try
                {
                    DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndCreateDate", EndCreateDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndCreateDate = HttpContext.Request["EndCreateDate"];
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }
            if (Request["EndETD"].ToString() != "")
            {
                try
                {
                    DateTime EndETD = Convert.ToDateTime(Request["EndETD"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndETD", EndETD);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndETD = HttpContext.Request["EndETD"];
                SqlParameters.Add("@EndETD", EndETD);
            }

            SqlParameters.Add("@CreateUser", createUser);
            SqlParameters.Add("@ClientCode", clientCode);

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@SealNumber", sealNumber);
            SqlParameters.Add("@ShipMode", shipMode);
            SqlParameters.Add("@BillNumber", BillNumber);
            SqlParameters.Add("@BeginETD", BeginETD);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);

            return Content(EIP.ReportList("装箱单明细查询", null, SqlParameters, "Load:135,客户名:120,SO号:120,PO号:120,款号:130,数量:50,单位:50,ETD:110,箱号:110,箱型:50,箱型名称:150,船名:130,港区:100,提单号:140,创建人:70,创建时间:133,结束装箱时间:133,default:70", null, null, 50, false, "数量,实装立方"));

        }

        #endregion




        #region 18.出货超期明细查询

        public ActionResult OutChaoQiDetail()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            //Session["whCode"].ToString();
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            //return null;
            return View();
        }


        [HttpPost]
        public ActionResult OutChaoQiDetailList()
        {
            string whCode = HttpContext.Request["whCode"];
            string load = HttpContext.Request["load"];
            string ContainerNumber = HttpContext.Request["ContainerNumber"];
            string ClientCode = HttpContext.Request["ClientCode"];
 
            string BeginPackDate = HttpContext.Request["BeginPackDate"];
            string tianshu = HttpContext.Request["tianshu"];

            Hashtable SqlParameters = new Hashtable();

            string scan_number_result = "";   //定义so最终值
            if (ContainerNumber + "" != "")
            {
                string scan_temp = ContainerNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
            }

           
            if (Request["EndPackDate"].ToString() != "")
            {
                try
                {
                    DateTime EndPackDate = Convert.ToDateTime(Request["EndPackDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndPackDate", EndPackDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndPackDate = HttpContext.Request["EndPackDate"];
                SqlParameters.Add("@EndPackDate", EndPackDate);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@LoadId", load);
            SqlParameters.Add("@Tianshu", tianshu);
            SqlParameters.Add("@ContainerNumber", scan_number_result);

            SqlParameters.Add("@BeginPackDate", BeginPackDate);

            return Content(EIP.ReportList("集装箱出货超期明细查询", null, SqlParameters, "出库单号:135,出库SO:100,收货批次号:130,客户:110,箱号:100,箱型:50,SO号:120,PO号:120,款号:140,装箱时间:120,收货时间:120,default:60", null, null, 50, false, "装箱箱数,CBM"));
        }

        #endregion

    }
}
