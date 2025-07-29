using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.Report.Controllers
{
    public class InBoundReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();

        #region 1.预录入明细查询

        public ActionResult InBound()
        {
            ViewData["clientCode"] = EIP.ReportSelect("select ClientCode,Id from WhClient with(nolock) where WhCode='" + Session["whCode"].ToString() + "' and Status='Active' order by ClientCode", null, null);
            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult InBoundList()
        {
            string WhClientId = HttpContext.Request["WhClientId"];
            string whCode = HttpContext.Request["whCode"];
            string soNumber = HttpContext.Request["soNumber"];
            string poNumber = HttpContext.Request["poNumber"];
            string altItemNumber = HttpContext.Request["altItemNumber"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            Hashtable SqlParameters = new Hashtable();
            if (Request["EndCreateDate"] != "")
            {
                DateTime EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"].ToString()).AddDays(1);
                SqlParameters.Add("@EndCreateDate", EndCreateDate);
            }
            else
            {
                SqlParameters.Add("@EndCreateDate", "");
            }

            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@soNumber", soNumber == null ? "" : soNumber);
            SqlParameters.Add("@poNumber", poNumber == null ? "" : poNumber);
            SqlParameters.Add("@altItemNumber", altItemNumber == null ? "" : altItemNumber);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            SqlParameters.Add("@ClientId", WhClientId);

             
            return Content(EIP.ReportList("预录入明细查询", null, SqlParameters, "预录入数量:70,已登记数量:70,创建时间:130,default:100"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion

        #region 2.道口工作量查询

        public ActionResult DaoKouWork()
        {
            ViewData["YearCreateDate"] = EIP.ReportSelect("select  YEAR(GETDATE()) union select  YEAR(GETDATE())-1 ", null, null);
            ViewData["MonthCreateDate"] = EIP.ReportSelect(" select 1 union select 2 union select 3 union select 4 union select 5 union select 6 union select 7 union select 8 union select 9 union select 10 union select 11 union select 12  ", null, null);

            ViewBag.whCode = Session["whCode"].ToString();
            return View();
        }

        [HttpPost]
        public ActionResult DaoKouWorkList()
        {
            string whCode = HttpContext.Request["whCode"];

            Hashtable SqlParameters = new Hashtable();
            string YearCreateDate = HttpContext.Request["YearCreateDate"];
            string MonthCreateDate = HttpContext.Request["MonthCreateDate"];

            SqlParameters.Add("@YearCreateDate", YearCreateDate);

            SqlParameters.Add("@MonthCreateDate", MonthCreateDate);

            SqlParameters.Add("@WhCode", whCode);

            return Content(EIP.ReportList("道口工作量查询", null, SqlParameters, "预录入数量:70,已登记数量:70,创建时间:130,default:100"));
            //  return Content(HttpContext.Request.Url.AbsoluteUri);
        }

        #endregion
    }
}
