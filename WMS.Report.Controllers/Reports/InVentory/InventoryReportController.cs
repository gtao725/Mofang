
using System;
using System.Collections;
using System.Data;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class InventoryReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();

        #region 1.库存查询
        public ActionResult Inventory()
        {
            ViewData["agentCode"] = EIP.ReportSelect("select AgentCode,Id from WhAgent with(nolock) where WhCode='" + Session["whCode"].ToString() + "' order by AgentCode", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);

            ViewData["clientType"] = EIP.ReportSelect("select ClientType,ClientType from WhClientType with(nolock) where  WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewData["zoneName"] = EIP.ReportSelect("select ZoneName, Id from Zones with(nolock) where  WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }
        public ActionResult WarehouseInventory()
        {
            ViewData["whInfoCode"] = EIP.ReportSelect("select distinct wi.WhName,wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "'  or '" + Session["isAdmin"].ToString() + "'='Y'  order by wi.WhName ", null, null);
            ViewData["clientCode"] = EIP.ReportSelect(" select distinct  ClientCode,a.ClientCode Code from WhClient a inner join WhInfo b on a.WhCode=b.WhCode  where    a.Status='Active'  and (a.WhCode in (select wi.WhCode from R_WhInfo_WhUser a inner join WhUser b on a.UserId=b.Id inner join WhInfo wi on a.WhCodeId=wi.Id where b.UserName='" + Session["userName"].ToString() + "')or  '" + Session["isAdmin"].ToString() + "'='Y') order by ClientCode", null, null);

            if (Session["isAdmin"].ToString() == "Y")
                ViewBag.CompanyId = "0";
            else
                ViewBag.CompanyId = Session["whCompany"].ToString();

            ViewBag.UserName = Session["userName"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult InventoryList()
        {
            string agentId = HttpContext.Request["agentId"];
            string WhClientId = HttpContext.Request["WhClientId"];

            string whCode = HttpContext.Request["whCode"];
            string receiptId = HttpContext.Request["ReceiptId"];
            string soNumber = HttpContext.Request["so_like"];

            string lot1 = HttpContext.Request["LotNumber1"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string holdReason = HttpContext.Request["holdReason"];

            string hebingWhere = HttpContext.Request["hebingWhere"];

            string locationId = HttpContext.Request["locationId"];
            string locationId1 = HttpContext.Request["locationId1"];

            string solist = HttpContext.Request["so_number"];
            string clientType = HttpContext.Request["clientType"];
            string zoneId = HttpContext.Request["zoneId"];

            string type = HttpContext.Request["typeSelect"];

            string scan_number_result = "";   //定义so最终值
            if (solist + "" != "")
            {
                string scan_temp = solist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                if (scan_number_result != "")
                {
                    scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
                }
            }

            string HuIdlist = HttpContext.Request["hu_id"];
            string huid_result = "";   //定义so最终值
            if (HuIdlist + "" != "")
            {
                string scan_temp = HuIdlist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    huid_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                huid_result = huid_result.Substring(0, huid_result.Length - 1);  //最后 so最终值需减去1位
            }

            string polist = HttpContext.Request["customer_po"];
            string po_result = "";   //定义so最终值
            if (polist + "" != "")
            {
                string scan_temp = polist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        po_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                if (po_result != "")
                {
                    po_result = po_result.Substring(0, po_result.Length - 1);  //最后 so最终值需减去1位
                }
            }

            string itemlist = HttpContext.Request["altItemNumber"];
            string item_result = "";   //定义so最终值
            if (itemlist + "" != "")
            {
                string scan_temp = itemlist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    item_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                item_result = item_result.Substring(0, item_result.Length - 1);  //最后 so最终值需减去1位
            }

            string style1list = HttpContext.Request["style1"];
            string style1_result = "";   //定义so最终值
            if (style1list + "" != "")
            {
                string scan_temp = style1list.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    style1_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                style1_result = style1_result.Substring(0, style1_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndReceiptDate"].ToString() != "")
            {
                try
                {
                    DateTime EndReceiptDate = Convert.ToDateTime(Request["EndReceiptDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
                SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            }

            if (Request["locationTypeId"] != "")
            {
                string locationTypeId = HttpContext.Request["locationTypeId"];
                SqlParameters.Add("@LocationTypeId", locationTypeId);
            }
            else
            {
                SqlParameters.Add("@LocationTypeId", "");
            }

            SqlParameters.Add("@AgentId", agentId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", huid_result);
            SqlParameters.Add("@ReceiptId", receiptId == null ? "" : receiptId);
            SqlParameters.Add("@SoNumber", soNumber == null ? "" : soNumber);

            SqlParameters.Add("@SoNumber1", scan_number_result);

            SqlParameters.Add("@CustomerPoNumber1", po_result);
            SqlParameters.Add("@AltItemNumber1", item_result);
            SqlParameters.Add("@Style1", style1_result);

            SqlParameters.Add("@LotNumber1", lot1 == null ? "" : lot1);
            SqlParameters.Add("@ClientId", WhClientId);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@holdReason", holdReason);
            SqlParameters.Add("@Location", locationId);
            SqlParameters.Add("@Location1", locationId1);

            SqlParameters.Add("@ClientType", clientType);
            SqlParameters.Add("@zoneId", zoneId);
            SqlParameters.Add("@type", type);

            if (hebingWhere == "1")
            {
                return Content(EIP.ReportList("库存查询无托盘", null, SqlParameters, "托盘:80,库位状态:100,收货时间:110,SO:130,PO:130,款号:130,锁定数量:60,收货批次号:115,库存数量:60,库存天数:60,长:60,宽:60,高:60,重量:60,default:70", null, null, 50, false, "库存数量,立方"));
            }
            else
            {
                return Content(EIP.ReportList("库存查询", null, SqlParameters, "托盘:80,库位状态:100,收货时间:110,SO:130,PO:130,款号:130,锁定数量:60,收货批次号:115,库存数量:60,库存天数:60,长:60,宽:60,高:60,重量:60,default:70", null, null, 50, false, "库存数量,立方"));
            }

        }
        [HttpPost]
        public ActionResult WarehouseInventoryList()
        {

            string ClientCode = HttpContext.Request["ClientCode"];

            string whCode = HttpContext.Request["whCode"];
            string receiptId = HttpContext.Request["ReceiptId"];
            string soNumber = HttpContext.Request["so_like"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string lot1 = HttpContext.Request["LotNumber1"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];
            string holdReason = HttpContext.Request["holdReason"];

            string hebingWhere = HttpContext.Request["hebingWhere"];

            string locationId = HttpContext.Request["locationId"];
            string locationId1 = HttpContext.Request["locationId1"];

            string solist = HttpContext.Request["so_number"];
            // string clientType = HttpContext.Request["clientType"];

            string scan_number_result = "";   //定义so最终值
            if (solist + "" != "")
            {
                string scan_temp = solist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                if (scan_number_result != "")
                {
                    scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
                }
            }

            string HuIdlist = HttpContext.Request["hu_id"];
            string huid_result = "";   //定义so最终值
            if (HuIdlist + "" != "")
            {
                string scan_temp = HuIdlist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    huid_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                huid_result = huid_result.Substring(0, huid_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndReceiptDate"].ToString() != "")
            {
                try
                {
                    DateTime EndReceiptDate = Convert.ToDateTime(Request["EndReceiptDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
                SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            }

            if (Request["locationTypeId"] != "")
            {
                string locationTypeId = HttpContext.Request["locationTypeId"];
                SqlParameters.Add("@LocationTypeId", locationTypeId);
            }
            else
            {
                SqlParameters.Add("@LocationTypeId", "");
            }
            string CompanyId = HttpContext.Request["CompanyId"];
            string UserName = HttpContext.Request["UserName"];
            SqlParameters.Add("@CompanyId", CompanyId);
            SqlParameters.Add("@UserName", UserName);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", huid_result);
            SqlParameters.Add("@ReceiptId", receiptId == null ? "" : receiptId);
            SqlParameters.Add("@SoNumber", soNumber == null ? "" : soNumber);

            SqlParameters.Add("@SoNumber1", scan_number_result);

            SqlParameters.Add("@CustomerPoNumber", poNumber == null ? "" : poNumber);
            SqlParameters.Add("@AltItemNumber", altItemNumber == null ? "" : altItemNumber);
            SqlParameters.Add("@LotNumber1", lot1 == null ? "" : lot1);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);
            SqlParameters.Add("@holdReason", holdReason);
            SqlParameters.Add("@Location", locationId);
            SqlParameters.Add("@Location1", locationId1);

            //SqlParameters.Add("@ClientType", clientType);

            if (hebingWhere == "1")
            {
                return Content(EIP.ReportList("库存查询无托盘(公司)", null, SqlParameters, "托盘:130,库位状态:100,收货时间:110,SO:130,PO:130,款号:130,锁定数量:60,收货批次号:115,库存数量:60,库存天数:60,长:60,宽:60,高:60,重量:60,default:70", null, null, 50, false, "库存数量,立方"));
            }
            else
            {
                return Content(EIP.ReportList("库存查询(公司)", null, SqlParameters, "托盘:130,库位状态:100,收货时间:110,SO:130,PO:130,款号:130,锁定数量:60,收货批次号:115,库存数量:60,库存天数:60,长:60,宽:60,高:60,重量:60,default:70", null, null, 50, false, "库存数量,立方"));
            }

        }
        #endregion


        #region 2.超俩小时 未摆货 库存查询
        public ActionResult InventoryOutOfTime()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult InventoryOutOfTimeList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string HuId = HttpContext.Request["HuId"];
            string whCode = HttpContext.Request["whCode"];
            string receiptId = HttpContext.Request["ReceiptId"];
            string soNumber = HttpContext.Request["so_like"];
            string poNumber = HttpContext.Request["PoNumber"];
            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string lot1 = HttpContext.Request["LotNumber1"];

            string holdReason = HttpContext.Request["holdReason"];

            string locationId = HttpContext.Request["locationId"];

            string solist = HttpContext.Request["so_number"];

            string scan_number_result = "";   //定义so最终值
            if (solist + "" != "")
            {
                string scan_temp = solist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                if (scan_number_result != "")
                {
                    scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
                }
            }

            Hashtable SqlParameters = new Hashtable();

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", HuId == null ? "" : HuId);
            SqlParameters.Add("@ReceiptId", receiptId == null ? "" : receiptId);
            SqlParameters.Add("@SoNumber", soNumber == null ? "" : soNumber);

            SqlParameters.Add("@SoNumber1", scan_number_result);

            SqlParameters.Add("@CustomerPoNumber", poNumber == null ? "" : poNumber);
            SqlParameters.Add("@AltItemNumber", altItemNumber == null ? "" : altItemNumber);
            SqlParameters.Add("@LotNumber1", lot1 == null ? "" : lot1);
            SqlParameters.Add("@ClientId", WhClientId);
            SqlParameters.Add("@holdReason", holdReason);
            SqlParameters.Add("@Location", locationId);

            return Content(EIP.ReportList("库存查询超俩小时", null, SqlParameters, "托盘:100,库位状态:100,收货时间:125,SO:130,PO:130,款号:130,收货批次号:120,锁定数量:60,库存数量:60,长:60,宽:60,高:60,重量:60,default:80", null, null, 50, false, "库存数量,立方"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 3.库存库位对比
        public ActionResult InventoryLocationCompare()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult InventoryLocationCompareList()
        {

            string WhClientId = HttpContext.Request["WhClientId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["so_like"];

            string altItemNumber = HttpContext.Request["ALtItemNumber"];
            string locationId = HttpContext.Request["locationId"];
            string locationId1 = HttpContext.Request["locationId1"];
            string solist = HttpContext.Request["so_number"];
            string flag = HttpContext.Request["flag"];
            string LocationAreas = HttpContext.Request["LocationAreas"];


            string scan_number_result = "";   //定义so最终值
            if (solist + "" != "")
            {
                string scan_temp = solist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    if (!string.IsNullOrEmpty(scan_strings[i]))
                    {
                        scan_number_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                    }
                }
                if (scan_number_result != "")
                {
                    scan_number_result = scan_number_result.Substring(0, scan_number_result.Length - 1);  //最后 so最终值需减去1位
                }
            }

            string HuIdlist = HttpContext.Request["hu_id"];
            string huid_result = "";   //定义so最终值
            if (HuIdlist + "" != "")
            {
                string scan_temp = HuIdlist.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                string[] scan_strings = scan_temp.Split('@');           //把SO 按照@分割，放在数组

                for (int i = 0; i < scan_strings.Length; i++)         //循环数组
                {
                    huid_result += "" + scan_strings[i] + ",";           //取得so,并且前后加上单引号
                }
                huid_result = huid_result.Substring(0, huid_result.Length - 1);  //最后 so最终值需减去1位
            }

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", huid_result);
            SqlParameters.Add("@SoNumber", soNumber == null ? "" : soNumber);

            SqlParameters.Add("@SoNumber1", scan_number_result);
            SqlParameters.Add("@AltItemNumber", altItemNumber == null ? "" : altItemNumber);

            SqlParameters.Add("@ClientId", WhClientId);
            SqlParameters.Add("@Location", locationId);
            SqlParameters.Add("@Location1", locationId1);
            SqlParameters.Add("@LocationAreas", LocationAreas);
            SqlParameters.Add("@flag", flag);

            return Content(EIP.ReportList("库存库位对比", null, SqlParameters, "托盘号:180,款号:120,数量:60,default:80", null, null, 50, false, "数量"));


        }

        #endregion


        #region 4.客户库存量查询

        public ActionResult InventoryClientCBM()
        {
            ViewData["agentCodeList"] = EIP.ReportSelect("select AgentCode,Id from WhAgent with(nolock) where WhCode='" + Session["whCode"].ToString() + "' order by AgentCode", null, null);

            ViewData["clientCodeList"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            ViewData["zoneNameList"] = EIP.ReportSelect("select left(ZoneName,1) ZoneName, left(ZoneName,1) ZoneName from Zones with(nolock) where  WhCode='" + Session["whCode"].ToString() + "' and RegFlag=1 ", null, null);

            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult InventoryClientCBMList()
        {
            string agentId = HttpContext.Request["agentCode"];
            string ClientCode = HttpContext.Request["clientCode"];

            string whCode = HttpContext.Request["whCode"];
            string zoneName = HttpContext.Request["zoneName"];
            string BeginReceiptDate = HttpContext.Request["BeginReceiptDate"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["EndReceiptDate"].ToString() != "")
            {
                try
                {
                    DateTime EndReceiptDate = Convert.ToDateTime(Request["EndReceiptDate"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string EndReceiptDate = HttpContext.Request["EndReceiptDate"];
                SqlParameters.Add("@EndReceiptDate", EndReceiptDate);
            }

            SqlParameters.Add("@AgentId", agentId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@zoneId", zoneName);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@BeginReceiptDate", BeginReceiptDate);

            return Content(EIP.ReportList("客户库存量查询", null, SqlParameters, "库区:70,default:130", null, null, 50, false, "库存量"));

        }

        #endregion

    }
}
