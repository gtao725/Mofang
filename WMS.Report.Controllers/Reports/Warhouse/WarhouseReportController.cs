using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class WarhouseReportController : Controller
    {

        EIP.EIP EIP = new EIP.EIP();


        #region 1.盘点查询

        public ActionResult CycleCount()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult CycleCountList()
        {

            string whCode = HttpContext.Request["whCode"];
            string HuId = HttpContext.Request["HuId"];
            string TaskNumber = HttpContext.Request["TaskNumber"];
            string LocationId = HttpContext.Request["LocationId"];
            string CustomerPoNumber = HttpContext.Request["CustomerPoNumber"];
            string AltItemNumber = HttpContext.Request["AltItemNumber"];
            string whetherDifference = HttpContext.Request["whetherDifference"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            int eip_page_size = Convert.ToInt32(HttpContext.Request["eip_page_size"]);
            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", HuId);
            SqlParameters.Add("@TaskNumber", TaskNumber);
            SqlParameters.Add("@LocationId", LocationId);
            SqlParameters.Add("@CustomerPoNumber", CustomerPoNumber);
            SqlParameters.Add("@AltItemNumber", AltItemNumber);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
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


            if (whetherDifference == "1")
            {
                return Content(EIP.ReportList("已盘任务错误信息查询", null, SqlParameters, "盘点号码:130,盘点时间:130,库存PO:100,实盘PO:100,实盘款号:100,库存款号:100,库存数量:60,实盘数量:60,盘点结果:70,Lot1:60,default:80", null, null, eip_page_size, false, null, true));
            }
            else
            {
                return Content(EIP.ReportList("盘点明细查询", null, SqlParameters, "盘点号码:130,盘点时间:130,库存PO:100,实盘PO:100,实盘款号:100,库存款号:100,库存数量:60,实盘数量:60,盘点结果:70,Lot1:60,default:80", null, null, eip_page_size, false, null, true));
            }

            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 2.收出货动态库位查询

        public ActionResult RecOutDyanmic()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult RecOutDyanmicList()
        {
            string whCode = HttpContext.Request["whCode"];
            string TranDate = HttpContext.Request["TranDate"];
            string ClientCode = HttpContext.Request["ClientCode"];

            string TranDate1 = HttpContext.Request["TranDate1"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@TranDate", TranDate);

            SqlParameters.Add("@TranDate1", TranDate1);

            SqlParameters.Add("@ClientCode", ClientCode);
            return Content(EIP.ReportList("出货动态库位查询", null, SqlParameters, "default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 3.系统记录查询

        public ActionResult Tranlog()
        {
            ViewData["tranType"] = EIP.ReportSelect("select Description,ColumnKey from LookUp with(nolock) where TableName='TranLog' and ColumnName='TranType' order by 2", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult TranlogList()
        {
            string clientCode = HttpContext.Request["clientCode"];
            string whCode = HttpContext.Request["whCode"];
            string HuId = HttpContext.Request["HuId"];
            string Load = HttpContext.Request["Load"];
            string Receipt = HttpContext.Request["Receipt"];
            string SoNumber = HttpContext.Request["SoNumber"];
            string PoNumber = HttpContext.Request["PoNumber"];
            string AltNumber = HttpContext.Request["AltNumber"];
            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];
            string UserCode = HttpContext.Request["UserCode"];
            string tranType = HttpContext.Request["tranType"];
            string Remark = HttpContext.Request["Remark"];
            string ExpressNumber = HttpContext.Request["ExpressNumber"];

            string CustomerOutPoNumber = HttpContext.Request["CustomerOutPoNumber"];
            string Location = HttpContext.Request["LocationId"];

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
            SqlParameters.Add("@ClientCode", clientCode);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@HuId", HuId);
            SqlParameters.Add("@LoadId", Load);
            SqlParameters.Add("@ReceiptId", Receipt);
            SqlParameters.Add("@SoNumber", SoNumber);
            SqlParameters.Add("@CustomerPoNumber", PoNumber);
            SqlParameters.Add("@AltItemNumber", AltNumber);
            SqlParameters.Add("@UserCode", UserCode);
            SqlParameters.Add("@TranType", tranType);
            SqlParameters.Add("@Remark", Remark);
            SqlParameters.Add("@ExpressNumber", ExpressNumber);

            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);
            SqlParameters.Add("@CustomerOutPoNumber", CustomerOutPoNumber);
            SqlParameters.Add("@Location", Location);
            
            return Content(EIP.ReportList("系统记录查询", null, SqlParameters, "类型:80,操作时间:113,收货批次号:113,原始数量:60,修改数量:60,SO:130,PO:130,款号:130,长:60,宽:60,高:60,重量:60,单位:60,LotDate:110,Load:135,操作人:60,客户:110,Lot1:60,Lot2:60,收货时间:110,default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 4.工作量基础查询

        public ActionResult WorkAccount()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["workType"] = EIP.ReportSelect("select '理货员' WorkType union all select '叉车工' WorkType union all select '装卸工' WorkType union all select '电车工' WorkType ", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            return View();
        }

        [HttpPost]
        public ActionResult WorkAccountList()
        {
            string type = HttpContext.Request["type"];

            string whCode = HttpContext.Request["whCode"];
            string LoadId = HttpContext.Request["LoadId"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string workType = HttpContext.Request["workType"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];
            DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);

            string UserCode = HttpContext.Request["UserCode"];
            string UserNameCN = HttpContext.Request["UserNameCN"];
            string ClientCode = HttpContext.Request["ClientCode"];

            if (type == "In")
            {
                Hashtable SqlParameters = new Hashtable();
                SqlParameters.Add("@WhCode", whCode);
                SqlParameters.Add("@WorkType", workType);
                SqlParameters.Add("@ReceiptId", ReceiptId);
                SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);

                SqlParameters.Add("@ClientCode", ClientCode);
                SqlParameters.Add("@UserCode", UserCode);
                SqlParameters.Add("@UserNameCN", UserNameCN);

                return Content(EIP.ReportList("收货工作量查询", null, SqlParameters, "收货批次号:110,时间:120,客户:110,default:70", null, null, 50, false, "数量,立方"));
            }
            else
            {
                Hashtable SqlParameters = new Hashtable();
                SqlParameters.Add("@WhCode", whCode);
                SqlParameters.Add("@WorkType", workType);
                SqlParameters.Add("@LoadId", LoadId);
                SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);

                SqlParameters.Add("@ClientCode", ClientCode);
                SqlParameters.Add("@UserCode", UserCode);
                SqlParameters.Add("@UserNameCN", UserNameCN);

                return Content(EIP.ReportList("出货工作量查询", null, SqlParameters, "Load号:120,时间:120,客户:110,箱号:100,default:70", null, null, 50, false, "数量,立方"));
            }

            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 5.每日收出货查询

        public ActionResult EveryDayRecOut()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult EveryDayRecOutList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string whCode = HttpContext.Request["whCode"];
            string BeginDate = HttpContext.Request["BeginDate"];

            Hashtable SqlParameters = new Hashtable();
            if (Request["EndDate"] != "")
            {
                DateTime EndDate = Convert.ToDateTime(Request["EndDate"].ToString()).AddDays(1);
                SqlParameters.Add("@EndDate", EndDate);
            }
            else
            {
                SqlParameters.Add("@EndDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@BeginDate", BeginDate);
            SqlParameters.Add("@ClientId", WhClientId);

            return Content(EIP.ReportList("每日收出货查询", null, SqlParameters, "日期:125,货代:125,客户:125,类型:60,default:80"));
        }



        #endregion

        #region 6.仓库收货工作量统计

        public ActionResult WarhouseRecWorkAccount()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["workType"] = EIP.ReportSelect("select '理货员' WorkType union all select '叉车工' WorkType union all select '装卸工' WorkType union all select '电车工' WorkType ", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            return View();
        }

        [HttpPost]
        public ActionResult WarhouseRecWorkAccountList()
        {

            string whCode = HttpContext.Request["whCode"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string workType = HttpContext.Request["workType"];
            string UserCode = HttpContext.Request["UserCode"];
            string UserNameCN = HttpContext.Request["UserNameCN"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];

            DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);

            Hashtable SqlParameters = new Hashtable();

            SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@WorkType", workType);
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);

            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@UserCode", UserCode);
            SqlParameters.Add("@UserNameCN", UserNameCN);

            return Content(EIP.ReportList("仓库收货工作量统计", null, SqlParameters, "收货批次号:110,收货日期:120,default:80", null, null, 50, false, "数量,立方"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 7.仓库出货工作量统计

        public ActionResult WarhouseOutWorkAccount()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["workType"] = EIP.ReportSelect("select '理货员' WorkType union all select '叉车工' WorkType union all select '装卸工' WorkType union all select '电车工' WorkType ", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            return View();
        }

        [HttpPost]
        public ActionResult WarhouseOutWorkAccountList()
        {

            string whCode = HttpContext.Request["whCode"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string LoadId = HttpContext.Request["LoadId"];
            string ContainerNumber = HttpContext.Request["ContainerNumber"];
            string workType = HttpContext.Request["workType"];
            string UserCode = HttpContext.Request["UserCode"];
            string UserNameCN = HttpContext.Request["UserNameCN"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["ReceiptDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string ReceiptDateEnd = HttpContext.Request["ReceiptDateEnd"];
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@WorkType", workType);
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@ContainerNumber", ContainerNumber);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);

            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@UserCode", UserCode);
            SqlParameters.Add("@UserNameCN", UserNameCN);

            return Content(EIP.ReportList("仓库出货工作量统计", null, SqlParameters, "Load号:120,出货日期:120,库区:50,箱型:50,工号:50,员工姓名:60,工作类型:60,立方:50,数量:50,把数:50,客户名:100,default:130", null, null, 50, false, "数量,立方"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 8.工作量统计（出仓方式）

        public ActionResult WorkAccountChucangfangshi()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            return View();
        }

        [HttpPost]
        public ActionResult WorkAccountChucangfangshiList()
        {

            string whCode = HttpContext.Request["whCode"];
            string LoadId = HttpContext.Request["LoadId"];
            string ContainerNumber = HttpContext.Request["ContainerNumber"];
            string Status3 = HttpContext.Request["Status3"];
            string shipMode = HttpContext.Request["shipMode"];

            string ETDBegin = HttpContext.Request["ETDBegin"];
            string PackDateBegin = HttpContext.Request["PackDateBegin"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["ETDEnd"].ToString() != "")
            {
                try
                {
                    DateTime ETDEnd = Convert.ToDateTime(Request["ETDEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@ETDEnd", ETDEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string ETDEnd = HttpContext.Request["ETDEnd"];
                SqlParameters.Add("@ETDEnd", ETDEnd);
            }

            if (Request["PackDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime PackDateEnd = Convert.ToDateTime(Request["PackDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@PackDateEnd", PackDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string PackDateEnd = HttpContext.Request["PackDateEnd"];
                SqlParameters.Add("@PackDateEnd", PackDateEnd);
            }

            if (Status3 == "全部未完成")
            {
                SqlParameters.Add("@Status3", "");
                SqlParameters.Add("@Status33", Status3);
            }
            else
            {
                SqlParameters.Add("@Status3", Status3);
                SqlParameters.Add("@Status33", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@ContainerNumber", ContainerNumber);
            SqlParameters.Add("@shipMode", shipMode);

            SqlParameters.Add("@ETDBegin", ETDBegin);
            SqlParameters.Add("@PackDateBegin", PackDateBegin);

            return Content(EIP.ReportList("出仓方式工作量统计", null, SqlParameters, "Load号:115,开始装箱时间:110,结束装箱时间:110,开航日期:110,创建时间:110,发货时间:110,default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 9.收货绩效统计

        public ActionResult WorkAccountRecJixiao()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            return View();
        }

        [HttpPost]
        public ActionResult WorkAccountRecJixiaoList()
        {

            string whCode = HttpContext.Request["whCode"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string team = HttpContext.Request["team"];
            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["RegisterDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime RegisterDateEnd = Convert.ToDateTime(Request["RegisterDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string RegisterDateEnd = HttpContext.Request["RegisterDateEnd"];
                SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@Team", team);
            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);

            return Content(EIP.ReportList("收货绩效统计", null, SqlParameters, "收货批次号:110,登记时间:110,车牌号:80,客户名:100,SO号:100,开始收货时间:110,结束收货时间:110,default:60"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 10.客户堆存期查询

        public ActionResult ClientDuicunqi()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewData["agentCode"] = EIP.ReportSelect("select AgentCode,AgentCode from WhAgent with(nolock) where WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);
            return View();
        }

        [HttpPost]
        public ActionResult ClientDuicunqiList()
        {

            string whCode = HttpContext.Request["whCode"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string agentId = HttpContext.Request["agentId"];

            string RegisterDateBegin = HttpContext.Request["RegisterDateBegin"];
            string showDetail = HttpContext.Request["showDetail"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["RegisterDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime RegisterDateEnd = Convert.ToDateTime(Request["RegisterDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string RegisterDateEnd = HttpContext.Request["RegisterDateEnd"];
                SqlParameters.Add("@RegisterDateEnd", RegisterDateEnd);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@AgentCode", agentId);
            SqlParameters.Add("@ClientCode", ClientCode);

            SqlParameters.Add("@RegisterDateBegin", RegisterDateBegin);

            if (showDetail == "1")
            {
                return Content(EIP.ReportList("客户堆存期查询", null, SqlParameters, "出货数量:70,出货立方:70,囤存天数:70,default:110"));
            }
            else
            {
                return Content(EIP.ReportList("客户堆存期查询汇总", null, SqlParameters, "default:110"));
            }

            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 11.收货采集差异

        public ActionResult C_SerialNumberInDiff()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult C_SerialNumberInDiffList()
        {
            string ClientId = HttpContext.Request["ClientId"];
            string whCode = HttpContext.Request["whCode"];

            string ReceiptId = HttpContext.Request["ReceiptId"];
            string SoNumber = HttpContext.Request["SoNumber"];
            string PoNumber = HttpContext.Request["PoNumber"];
            string ALtItemNumber = HttpContext.Request["ALtItemNumber"];
            string tranType = HttpContext.Request["tranType"];
            string ReceiptDateBegin = HttpContext.Request["ReceiptDateBegin"];
            string ReceiptDateEnd = HttpContext.Request["ReceiptDateEnd"];

            string solist = HttpContext.Request["SoNumberArr"];
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
            if (Request["ReceiptDateEnd"] != "")
            {
                ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"].ToString()).AddDays(1).ToString();
                SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            }
            else
            {
                SqlParameters.Add("@ReceiptDateEnd", "");
            }
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@ClientId", ClientId);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@SoNumber", SoNumber);
            SqlParameters.Add("@PoNumber", PoNumber);
            SqlParameters.Add("@AltItemNumber", ALtItemNumber);
            SqlParameters.Add("@ReceiptDateBegin", ReceiptDateBegin);
            //SqlParameters.Add("@ReceiptDateEnd", ReceiptDateEnd);
            SqlParameters.Add("@SoNumber1", scan_number_result);

            return Content(EIP.ReportList("收货扫描差异查询", null, SqlParameters, "收货批次号:130,客户:110,开始收货时间:130,结束收货时间:130,default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }
        [HttpPost]
        // [HttpGet]
        public ActionResult C_SnInDiffDetail()
        {

            string WhCode = HttpContext.Request["WhCode"];
            string ReceiptId = HttpContext.Request["ReceiptId"];

            string SoNumber = HttpContext.Request["detail_sonumber"];
            string PoNumber = HttpContext.Request["detail_ponumber"];
            string AltItemNumber = HttpContext.Request["detail_altitemnumber"];
            string HuId = HttpContext.Request["detail_huid"];

            string solist = HttpContext.Request["Detail_SoNumberArr"];
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
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@WhCode", WhCode);

            SqlParameters.Add("@SoNumber", SoNumber);
            SqlParameters.Add("@PoNumber", PoNumber);
            SqlParameters.Add("@AltItemNumber", AltItemNumber);
            SqlParameters.Add("@HuId", HuId);
            SqlParameters.Add("@SoNumber1", scan_number_result);

            return Content(EIP.ReportList("收货扫描差异明细", null, SqlParameters, "收货批次号:130,客户:110,PO:110,SO:110,default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }
        [HttpPost]
        // [HttpGet]
        public ActionResult C_SnInDetail()
        {

            string WhCode = HttpContext.Request["WhCode"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@WhCode", WhCode);

            return Content(EIP.ReportList("收货扫描差异明细1", null, SqlParameters, "收货批次号:130,SO号:110,PO号:110,SO:110,款号:90,CartonId:120,default:80"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }



        #endregion

        #region 12.工人采集箱号查询

        public ActionResult WorkScanNumber()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewData["workMonth"] = EIP.ReportSelect("select '1' workMonth union all select '2' workMonth union all select '3' workMonth union all select '4' workMonth union all select '5' workMonth union all select '6' workMonth union all select '7' workMonth union all select '8' workMonth union all select '9' workMonth union all select '10' workMonth union all select '11' workMonth union all select '12' workMonth", null, null);

            ViewData["workYear"] = EIP.ReportSelect("select  YEAR(GETDATE()) workYear union select  YEAR(GETDATE())-1 workYear  order by 1 desc", null, null);

            return View();
        }

        [HttpPost]
        public ActionResult WorkScanNumberList()
        {
            string whCode = HttpContext.Request["whCode"];
            string workYear = HttpContext.Request["workYear"];
            string workMonth = HttpContext.Request["workMonth"];

            string UserCode = HttpContext.Request["UserCode"];
            string UserNameCN = HttpContext.Request["UserNameCN"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@CreateYear", workYear);
            SqlParameters.Add("@CreateMouth", workMonth);
            SqlParameters.Add("@UserCode", UserCode);
            SqlParameters.Add("@UserNameCN", UserNameCN);

            return Content(EIP.ReportList("采集箱号工号统计", null, SqlParameters, "年:60,月:30,default:90", null, null, 50));

        }

        #endregion


        #region 13.补货任务明细查询

        public ActionResult SupplementTaskDetail()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        [HttpPost]
        public ActionResult SupplementTaskDetailList() 
        {
            string whCode = HttpContext.Request["whCode"];   
            string SupplementNumber = HttpContext.Request["SupplementNumber"];
            string AltItemNumber = HttpContext.Request["AltItemNumber"];
            string CreateDateBegin = HttpContext.Request["CreateDateBegin"];

            Hashtable SqlParameters = new Hashtable();

            if (Request["CreateDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime CreateDateEnd = Convert.ToDateTime(Request["CreateDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@CreateDateEnd", CreateDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string CreateDateEnd = HttpContext.Request["CreateDateEnd"];
                SqlParameters.Add("@CreateDateEnd", CreateDateEnd);
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@SupplementNumber", SupplementNumber);
            SqlParameters.Add("@AltItemNumber", AltItemNumber);
            SqlParameters.Add("@CreateDateBegin", CreateDateBegin);

            return Content(EIP.ReportList("补货任务明细查询", null, SqlParameters, "补货任务号:140,default:120", null, null, 50));

        }

        #endregion


        #region 14.超重箱统计

        public ActionResult WorkContainerChaoZhong()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewData["agentCode"] = EIP.ReportSelect("select AgentCode,AgentCode from WhAgent with(nolock) where WhCode='" + Session["whCode"].ToString() + "'", null, null);

            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,ClientCode from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by 1", null, null);

            return View();
        }

        [HttpPost]
        public ActionResult WorkContainerChaoZhongList()
        {
            string ClientCode = HttpContext.Request["ClientCode"];
            string agentId = HttpContext.Request["agentId"];

            string whCode = HttpContext.Request["whCode"];
            string LoadId = HttpContext.Request["LoadId"]; 
            string PackDateBegin = HttpContext.Request["PackDateBegin"];

            string scanNumber = HttpContext.Request["ContainerNumber"];
            string showChaoZhong = HttpContext.Request["showChaoZhong"];

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

            if (Request["PackDateEnd"].ToString() != "")
            {
                try
                {
                    DateTime PackDateEnd = Convert.ToDateTime(Request["PackDateEnd"].ToString()).AddDays(1);
                    SqlParameters.Add("@EndPackDate", PackDateEnd);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                string PackDateEnd = HttpContext.Request["PackDateEnd"];
                SqlParameters.Add("@EndPackDate", PackDateEnd);
            }

            SqlParameters.Add("@AgentCode", agentId);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@LoadId", LoadId);
            SqlParameters.Add("@ContainerNumber", scan_number_result);
            SqlParameters.Add("@BeginPackDate", PackDateBegin);

            if (showChaoZhong == "1")
            {
                return Content(EIP.ReportList("超重箱统计查询-超重", null, SqlParameters, "Load:140,default:110"));
            }
            else
            {
                return Content(EIP.ReportList("超重箱统计查询", null, SqlParameters, "Load:140,default:110"));
            }  

        }

        #endregion

    }
}
