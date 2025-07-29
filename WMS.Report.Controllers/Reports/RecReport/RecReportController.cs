
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class RecReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();

        #region 1.收货查询
        public ActionResult Receipt()
        {
            ViewData["agentCode"] = EIP.ReportSelect("select AgentCode,Id from WhAgent with(nolock) where WhCode='" + Session["whCode"].ToString() + "' order by AgentCode", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);

            ViewData["regZone"] = EIP.ReportSelect("select ZoneName,ZoneName from Zones with(nolock) where RegFlag=1 and WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewData["recStatus"] = EIP.ReportSelect("select '全部未完成' union all select '完成收货' union all select '未收货' union all select '正在收货'", null, null);

            ViewData["clientType"] = EIP.ReportSelect("select ClientType,ClientType from WhClientType with(nolock) where  WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptList()
        {
            string agentId = HttpContext.Request["agentId"];
            string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];

            string regZone = HttpContext.Request["regZone"];
            string recStatus = HttpContext.Request["recStatus"];

            string soWhere = HttpContext.Request["soWhere"];
            string hebingWhere = HttpContext.Request["hebingWhere"];

            string orderBy = HttpContext.Request["orderBy"];
            string clientType = HttpContext.Request["clientType"];
            string isBK = HttpContext.Request["isBK"];

            Hashtable SqlParameters = new Hashtable();
            if (Request["RegisterDateEnd"] != "")
            {
                DateTime RegisterDateEnd = Convert.ToDateTime(Request["RegisterDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
            }
            else
            {
                SqlParameters.Add("@RegisterDateEnd", "");
            }
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            SqlParameters.Add("@ClientId", WhClientId);
            SqlParameters.Add("@AgentId", agentId);

            SqlParameters.Add("@Location", regZone);
            SqlParameters.Add("@ClientType", clientType);

            SqlParameters.Add("@isBK", isBK);

            if (hebingWhere == "1")
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }

                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();

                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询合并", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,车型:50,是否超长:60,预约时间:150,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }
            else if (soWhere == "1")
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }

                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();

                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询按SO", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,SO:130,车型:50,是否超长:60,预约时间:150,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }
            else
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }


                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();
                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,预约时间:150,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,SO:130,PO:130,车型:50,是否超长:60,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }

            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        public ActionResult WarehouseReceipt()
        {
            ViewData["whInfoCode"] = EIP.ReportSelect("select distinct wi.WhName,wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "'  or '" + Session["isAdmin"].ToString() + "'='Y'  order by wi.WhName ", null, null);
            ViewData["clientCode"] = EIP.ReportSelect(" select distinct  ClientCode,a.ClientCode Code from WhClient a inner join WhInfo b on a.WhCode=b.WhCode  where    a.Status='Active'  and (a.WhCode in (select wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "')or '" + Session["isAdmin"].ToString() + "'='Y') order by ClientCode", null, null);

            ViewData["regZone"] = EIP.ReportSelect("select distinct ZoneName,ZoneName from Zones  a inner join WhInfo b on a.WhCode=b.WhCode where a.RegFlag=1 and (a.WhCode in (select wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "')or  '" + Session["isAdmin"].ToString() + "'='Y') ", null, null);

            ViewData["recStatus"] = EIP.ReportSelect("select '全部未完成' union all select '完成收货' union all select '未收货' union all select '正在收货'", null, null);

            if (Session["isAdmin"].ToString() == "Y")
                ViewBag.CompanyId = "0";
            else
                ViewBag.CompanyId = Session["whCompany"].ToString();

            ViewBag.UserName = Session["userName"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult WarehouseReceiptList()
        {

            string clientCode = HttpContext.Request["clientCode"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];

            string regZone = HttpContext.Request["regZone"];
            string recStatus = HttpContext.Request["recStatus"];

            string soWhere = HttpContext.Request["soWhere"];
            string hebingWhere = HttpContext.Request["hebingWhere"];

            string orderBy = HttpContext.Request["orderBy"];

            string isBK = HttpContext.Request["isBK"];

            Hashtable SqlParameters = new Hashtable();
            if (Request["RegisterDateEnd"] != "")
            {
                DateTime RegisterDateEnd = Convert.ToDateTime(Request["RegisterDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
            }
            else
            {
                SqlParameters.Add("@RegisterDateEnd", "");
            }
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            SqlParameters.Add("@clientCode", clientCode);
            string CompanyId = HttpContext.Request["CompanyId"];
            string UserName = HttpContext.Request["UserName"];
            SqlParameters.Add("@CompanyId", CompanyId);
            SqlParameters.Add("@UserName", UserName);
            SqlParameters.Add("@Location", regZone);
            //  SqlParameters.Add("@ClientType", clientType);

            SqlParameters.Add("@isBK", isBK);

            if (hebingWhere == "1")
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }

                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();

                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询合并(公司)", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,车型:50,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }
            else if (soWhere == "1")
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }

                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();

                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询按SO(公司)", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,SO:130,车型:50,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }
            else
            {
                if (recStatus == "全部未完成")
                {
                    SqlParameters.Add("@recStatus", "");
                    SqlParameters.Add("@recStatus1", recStatus);
                }
                else
                {
                    SqlParameters.Add("@recStatus", recStatus);
                    SqlParameters.Add("@recStatus1", "");
                }


                Dictionary<string, string> OrderByPara = new Dictionary<string, string>();
                if (orderBy == "结束收货时间")
                {
                    OrderByPara.Add("结束收货时间", " desc");

                }
                else if (orderBy == "登记时间")
                {
                    OrderByPara.Add("登记时间", " asc");
                }
                else
                {
                    OrderByPara.Add("登记时间", " asc");
                }

                return Content(EIP.ReportList("收货查询(公司)", null, SqlParameters, "操作:40,收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,预约时间:150,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,卸货区域:60,计划数量:60,实收数量:60,是否预约:60,分票数量:60,货损数量:60,挂衣数:60,立方:60,重量:60,SO:130,PO:130,车型:50,default:80", OrderByPara, null, 50, false, "计划数量,实收数量,分票数量,货损数量,挂衣数,计划立方,立方"));
            }

            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }




        #endregion

        #region 2.收货明细查询
        public ActionResult ReceiptDetail()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptDetailList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string Custom1 = HttpContext.Request["Custom1"];
            string HuId = HttpContext.Request["HuId"];

            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];
            Hashtable SqlParameters = new Hashtable();
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }
            string nightWhere = HttpContext.Request["nightWhere"];

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@CustomerPoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@AltItemNumber", string.IsNullOrEmpty(altItemNumber) ? "" : altItemNumber);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            SqlParameters.Add("@ClientId", WhClientId);

            SqlParameters.Add("@Custom1", Custom1);
            SqlParameters.Add("@HuId", HuId);

            if (nightWhere == "1")
            {
                SqlParameters.Add("@nightWhere", nightWhere);

                return Content(EIP.ReportList("收货明细查询", null, SqlParameters, "收货批次号:135,收货时间:125,状态:50,单位:45,SO:130,PO:130,款号:130,理货员:60,工号:50,实收数量:60,长:50,宽:50,高:50,重量:50,是否分票:60,default:80", null, null, 50, false, "实收数量,立方"));
            }
            else
            {
                return Content(EIP.ReportList("收货明细查询1", null, SqlParameters, "收货批次号:135,收货时间:125,状态:50,单位:45,SO:130,PO:130,款号:130,理货员:60,工号:50,实收数量:60,长:50,宽:50,高:50,重量:50,是否分票:60,default:80", null, null, 50, false, "实收数量,立方"));
            }


            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 3.收货状态查询
        public ActionResult ReceiptStatus()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptStatusList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altNumber = HttpContext.Request["AltNumber"];
            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];
            Hashtable SqlParameters = new Hashtable();
            if (Request["RegisterDateEnd"] != "")
            {
                DateTime RegisterDateEnd = Convert.ToDateTime(Request["RegisterDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
            }
            else
            {
                SqlParameters.Add("@RegisterDateEnd", "");
            }
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", ReceiptId == null ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", soNumber == null ? "" : soNumber);
            SqlParameters.Add("@PoNumber", poNumber == null ? "" : poNumber);
            SqlParameters.Add("@AltNumber", altNumber == null ? "" : altNumber);
            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("收货状态查询", null, SqlParameters, "收货批次号:135,登记时间:120,车辆到达时间:120,放车时间:120,入库时间:120,离库时间:120,开始收货时间:120,结束收货时间:120,SO:130,PO:130,计划数量:60,实收数量:60,分票数量:60,货损数量:60,立方:60,重量:60,单位:60,default:80", null, null, 50, false, "计划数量,实收数量,货损数量,立方"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }


        #endregion

        #region 4.收货扫描序列号查询

        public ActionResult ReceiptScanNumber()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptScanNumberList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string scanNumber = HttpContext.Request["ScanNumber"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];
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
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@SoNumber", so_number_result);
            SqlParameters.Add("@PoNumber", poNumber);
            SqlParameters.Add("@AltItemNumber", altItemNumber);
            SqlParameters.Add("@ScanNumber", scan_number_result);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            SqlParameters.Add("@HuId", huId);
            SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("收货扫描序列号查询", null, SqlParameters, "收货批次号:135,收货时间:125,托盘:80,理货员:80,序列号:130,default:100"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 5.收货CottonOn查询
        public ActionResult ReceiptCottonOn()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult ReceiptCottonOnList()
        {
           
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["SoNumber"];
            string poNumber = HttpContext.Request["PoNumber"];
            string itemNumber = HttpContext.Request["ItemNumber"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];

            Hashtable SqlParameters = new Hashtable();   
            if (Request["ReceiptDateEnd"] != "")
            {
                DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ReceiptId", string.IsNullOrEmpty(ReceiptId) ? "" : ReceiptId);
            SqlParameters.Add("@SoNumber", string.IsNullOrEmpty(soNumber) ? "" : soNumber);
            SqlParameters.Add("@PoNumber", string.IsNullOrEmpty(poNumber) ? "" : poNumber);
            SqlParameters.Add("@ItemNumber", string.IsNullOrEmpty(itemNumber) ? "" : itemNumber);

            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);

            return Content(EIP.ReportList("收货CottonOn查询", null, SqlParameters, "default:110", null, null, 50, false, "实收数量,立方"));

        }


        #endregion

    }
}
