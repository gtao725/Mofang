using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_EditProcessNameController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        [DefaultRequest]
        public ActionResult Index1()
        {
            return View();
        }


        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.ReceiptRegisterSearch entity = new WCF.RootService.ReceiptRegisterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"].Trim();
            entity.TruckNumber = Request["TruckNumber"].Trim();
            entity.PhoneNumber = Request["PhoneNumber"].Trim();
            entity.Status = Request["status"].Trim();
         
            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }

            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.RootService.ReceiptRegisterResult> list = cf.GetFlowHeadListByRec(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientId", "客户Id");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("SumQty", "原登记数量");
            fieldsName.Add("ProcessName", "所选流程");

            return Content(EIP.EipListJson(list, total, fieldsName, "ReceiptId:110,Id:50,Status:50,SumQty:70,ProcessName:110,default:80"));
        }

        [HttpPost]
        public ActionResult GetFlowNameList()
        {
            var sql = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]), "InBound")
                      select new
                      {
                          Value = r.Id.ToString(),
                          Text = r.FlowName    //text
                      };
            return Json(sql);
        }

        //修改流程
        [HttpGet]
        public ActionResult EditProcessName()
        {
            string number = Request["number"];
            string whCode = Session["whCode"].ToString();
            int processId = Convert.ToInt32(Request["processId"]);
            string processName = Request["processName"];
            string type = Request["type"];

            string result = cf.EditProcessName(number, whCode, processId, processName, type, Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult List1()
        {
            WCF.RootService.LoadMasterSearch entity = new WCF.RootService.LoadMasterSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
         
            entity.LoadId = Request["load"].Trim();
       
            entity.Status0 = Request["Status0"];
            entity.Status1 = Request["Status1"];
         
            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }
            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.RootService.LoadMasterResult> list = cf.GetFlowHeadListByLoad(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("ClientId", "客户Id");
            fieldsName.Add("ClientCode", "客户名");
       
            fieldsName.Add("Status0", "释放状态");
            fieldsName.Add("Status1", "备货状态");
           
            fieldsName.Add("ProcessId", "ProcessId");
            fieldsName.Add("ProcessName", "所选流程");

            fieldsName.Add("SumQty", "出货数量");
            fieldsName.Add("DSSumQty", "直装数量");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:50,ClientCode:110,LoadId:135,ProcessName:130,SumQty:60,DSSumQty:60,default:70"));
        }

        [HttpPost]
        public ActionResult GetFlowNameList1()
        {
            var sql = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]), "OutBound")
                      select new
                      {
                          Value = r.Id.ToString(),
                          Text = r.FlowName    //text
                      };
            return Json(sql);
        }

    }
}
