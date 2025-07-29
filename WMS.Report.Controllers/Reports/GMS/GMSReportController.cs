using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.Report.Controllers
{
    public class GMSReportController : Controller
    {
        EIP.EIP EIP = new EIP.EIP();

        [HttpPost]
       
        public ActionResult GMSList()
        {
            string whCode = HttpContext.Request["whCode"];
            string TruckNumber = HttpContext.Request["TruckNumber"];
            string PhoneNumber = HttpContext.Request["PhoneNumber"];
            string ReceiptId = HttpContext.Request["ReceiptId"];
            string ClientCode = HttpContext.Request["ClientCode"];
            string UnloadingArea = HttpContext.Request["UnloadingArea"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string EndCreateDate = HttpContext.Request["EndCreateDate"];
            string TruckStatus = HttpContext.Request["TruckStatus"];
            string ReStatus = HttpContext.Request["ReStatus"];

            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@TruckNumber", TruckNumber);
            SqlParameters.Add("@PhoneNumber", PhoneNumber);
            SqlParameters.Add("@ReceiptId", ReceiptId);
            SqlParameters.Add("@ClientCode", ClientCode);
            SqlParameters.Add("@UnloadingArea", UnloadingArea);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            SqlParameters.Add("@EndCreateDate", EndCreateDate);
            SqlParameters.Add("@TruckStatus", TruckStatus);
            SqlParameters.Add("@ReStatus", ReStatus);
            return Content(EIP.ReportList("车辆管理列表", null, SqlParameters, "seq:30,卸货区:50,客户:110,有效预约:65,绿色通道:65,创建时间:133,default:80,货物立方:67,登记时间:134,放车时间:134,预约开始时间:135,预约结束时间:135,司机手机号码:90,放车人:70,业务确认时间:133,进场时间:133,离库时间:133,货物数量:67,状态:70,操作:130", null, "WMS_PROD_DBAddress"));
        }

        [HttpGet]
        public ActionResult GMSDetailList()
        {
            
            string HeadId = HttpContext.Request["HeadId"];
           
            Hashtable SqlParameters = new Hashtable();

            SqlParameters.Add("@HeadId", HeadId);

            return Content(EIP.ReportList("车辆货物明细", null, SqlParameters, "预录入数量:70,已登记数量:70,创建时间:130,default:100", null, "WMS_PROD_DBAddress"));
        }
        [HttpPost]

        public ActionResult GMSLockList()
        {
            string whCode = HttpContext.Request["whCode"];
            string TruckNumber = HttpContext.Request["TruckNumber"];
            string PhoneNumber = HttpContext.Request["PhoneNumber"];
            string BeginCreateDate = HttpContext.Request["BeginCreateDate"];
            string EndCreateDate = HttpContext.Request["EndCreateDate"];
            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", whCode);
            SqlParameters.Add("@TruckNumber", TruckNumber);
            SqlParameters.Add("@PhoneNumber", PhoneNumber);
            SqlParameters.Add("@BeginCreateDate", BeginCreateDate);
            SqlParameters.Add("@EndCreateDate", EndCreateDate);
            return Content(EIP.ReportList("车辆锁定管理列表", null, SqlParameters, "预录入数量:70,已登记数量:70,创建时间:130,default:100", null, "WMS_PROD_DBAddress"));
        }
    }
}
