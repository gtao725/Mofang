using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class InterceptOrderController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();


        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            return View();
        }


        [HttpGet]
        public ActionResult List()
        {
            WCF.OutBoundService.OutBoundOrderSearch1 entity = new WCF.OutBoundService.OutBoundOrderSearch1();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            if (Request["WhClientId"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
            else
            {
                entity.ClientId = 0;
            }
            entity.CustomerOutPoNumber = Request["CustomerOutPoNumber"].Trim();
            entity.OutPoNumber = Request["OutPoNumber"].Trim();
            entity.StatusName = Request["StatusName"].Trim();
            entity.ProcessName = Request["ProcessName"].Trim();
            entity.AltItemNumber = Request["AltItemNumber"].Trim();
            entity.StatusId2 = Convert.ToInt32(Request["StatusId2"]);

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
            List<WCF.OutBoundService.OutBoundOrderResult1> list = cf.InterceptOutBoundOrderList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "ClientId");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("OutPoNumber", "系统出库单号");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("SumQty", "数量");
            fieldsName.Add("ReceiptId", "回库点");
            fieldsName.Add("StatusId1", "状态说明");
            fieldsName.Add("OrderSource", "订单来源");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("FlowName", "出库订单流程");
            fieldsName.Add("ProcessId", "ProcessId");

            fieldsName.Add("StatusId", "StatusId");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,ClientCode:120,SumQty:45,OrderSource:65,StatusName:90,default:150"));
        }


        //通过输入的款号 属性 得到单位列表
        [HttpPost]
        public ActionResult GetFlowNameList()
        {
            var sql = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]))
                      select new
                      {
                          Value = r.Id.ToString(),
                          Text = r.FlowName    //text
                      };
            return Json(sql);

        }

        [HttpPost]
        //重新生成收货批次号
        public ActionResult AddReceiptIdByInterceptOrderList()
        {
            string[] outBoundOrderList = Request.Form.GetValues("outBoundOrderId");

            int[] outBoundOrderList1 = Array.ConvertAll(outBoundOrderList, int.Parse);
            string abLocation = Request["abLocation"];

            if (outBoundOrderList1.Count() > 10)
            {
                return Helper.RedirectAjax("err", "批量处理10个订单以内效率更高，请更换为10个订单批量处理！", null, "");
            }

            string result = "";
            foreach (var item in outBoundOrderList1)
            {
                result = cf.InterceptOrderBatchReturnToWarehouse(item, Session["whCode"].ToString(), Session["userName"].ToString(), abLocation);
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量处理订单成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //[HttpPost]
        ////重新生成收货批次号
        //public ActionResult AddReceiptIdByInterceptOrder()
        //{

        //    string result = cf.AddReceiptIdByInterceptOrder(Convert.ToInt32(Request["outBoundOrderId"]), Convert.ToInt32(Request["sel_flowName"]), Request["processName"], Request["recLocationId"], Session["userName"].ToString());
        //    if (result == "Y")
        //    {
        //        return Helper.RedirectAjax("ok", "重新生成成功！", null, "");
        //    }
        //    else
        //    {
        //        return Helper.RedirectAjax("err", result, null, "");
        //    }

        //}

        //订单确认拦截处理已完成
        [HttpGet]
        public ActionResult CheckInterceptOrder()
        {
            string result = cf.CheckInterceptOrder(Convert.ToInt32(Request["outBoundOrderId"]), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功，订单处理完成！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public void GetInterceptOrderReceiptId()
        {
            string result = cf.GetInterceptOrderReceiptId(Request["OutPoNumber"], Session["whCode"].ToString());
            Response.Write(result);
        }

    }
}
