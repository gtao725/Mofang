using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class PackTaskController : Controller
    {
        WCF.PackTaskService.PackTaskServiceClient cf = new WCF.PackTaskService.PackTaskServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.PackTaskService.PackTaskSearch entity = new WCF.PackTaskService.PackTaskSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.SortGroupNumber = Request["packNumber"].Trim();

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
            List<WCF.PackTaskService.PackTaskSearchResult> list = cf.GetPackTaskSearchResult(entity,null,null, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("PackTaskId", "操作");
            fieldsName.Add("SortGroupNumber", "分拣框号");
            fieldsName.Add("CustomerOutPoNumber", "客户订单号");
            fieldsName.Add("ExpressCode", "快递公司");
            fieldsName.Add("PackHeadId", "packHeadId");
            fieldsName.Add("PackGroupId", "包装组号");
            fieldsName.Add("PackNumber", "包装托盘");
            fieldsName.Add("Status", "状态");

            fieldsName.Add("ExpressNumber", "快递单号");
            fieldsName.Add("ExpressStatus", "快递状态");
            fieldsName.Add("planQty", "计划总数量");
            fieldsName.Add("packQty", "包装总数量");
            fieldsName.Add("packNowQty", "包装数量");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("PackCarton", "包装耗材");

            fieldsName.Add("ExpressMessage", "快递单获取结果");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "PackTaskId:110,SortGroupNumber:120,CustomerOutPoNumber:120,PackNumber:90,ExpressNumber:120,PackCarton:120,ExpressMessage:120,CreateDate:120,default:70"));
        }

        [HttpGet]
        public ActionResult EditPackHead()
        {
            WCF.PackTaskService.PackHead packHead = new WCF.PackTaskService.PackHead();
            packHead.Id = Convert.ToInt32(Request["edit_headId"]);
            packHead.PackCarton = Request["edit_packCarton"].Trim();
            packHead.Weight = Convert.ToDecimal(Request["edit_weight"]);

            if (!string.IsNullOrEmpty(Request["edit_length"]) && Request["edit_length"] != "null")
            {
                packHead.Length = Convert.ToDecimal(Request["edit_length"]);
            }
            if (!string.IsNullOrEmpty(Request["edit_width"]) && Request["edit_width"] != "null")
            {
                packHead.Width = Convert.ToDecimal(Request["edit_width"]);
            }
            if (!string.IsNullOrEmpty(Request["edit_height"]) && Request["edit_height"] != "null")
            {
                packHead.Height = Convert.ToDecimal(Request["edit_height"]);
            }

            packHead.UpdateUser = Session["userName"].ToString();

            string result = cf.UpdatePackHead(packHead);
            if (result =="Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败！", null, "");
            }
        }

        //Load删除
        [HttpGet]
        public ActionResult DeletePackHead()
        {
            WCF.PackTaskService.PackHead packHead = new WCF.PackTaskService.PackHead();

            packHead.Id = Convert.ToInt32(Request["Id"]);
            packHead.PackTaskId = Convert.ToInt32(Request["packTaskId"]);
            int result = cf.DeletePackHead(packHead);

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


        //查看明细
        [HttpGet]
        public ActionResult PackDetailList()
        {
            int total = 0;
            List<WCF.PackTaskService.PackDetailSearchResult> list = cf.GetPackDetailSearchResult(Convert.ToInt32(Request["packHeadId"]), out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("PackDetailId", "操作");
            fieldsName.Add("PackNumber", "包装托盘");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("PlanQty", "计划数量");
            fieldsName.Add("Qty", "包装数量");
            fieldsName.Add("CreateUser", "操作人");
            fieldsName.Add("CreateDate", "操作时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "PackDetailId:50,PackNumber:100,CreateDate:120,AltItemNumber:120,default:80"));
        }


        //查看明细
        [HttpGet]
        public ActionResult ScanNumberDetailList()
        {
            int total = 0;
            List<WCF.PackTaskService.PackPackScanNumberResult> list = cf.ScanNumberDetailList(Convert.ToInt32(Request["PackDetailId"]), out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("PackScanNumberId", "packScanNumberId");
            fieldsName.Add("ScanNumber", "扫描序列号");

            return Content(EIP.EipListJson(list, total, fieldsName, "PackDetailId:50,PackNumber:100,CreateDate:120,default:80"));
        }

        //Load明细删除
        [HttpGet]
        public ActionResult LoadDetailDel()
        {
            string result = null;
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }





    }
}
