using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SupplementTaskController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

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
            WCF.RootService.SupplementTaskSearch entity = new WCF.RootService.SupplementTaskSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SupplementNumber = Request["SupplementNumber"].Trim();
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
            List<WCF.RootService.SupplementTaskResult> list = cf.SupplementTaskResultList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("SupplementNumber", "补货任务号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SupplementNumber:120,CreateDate:120,UpdateDate:120,default:100"));
        }

        [HttpGet]
        public ActionResult ReleaseSupplementTask()
        {
            int count = Convert.ToInt32(Request["txt_Qty"]);

            string poNumber = Request["AltItemNumber"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            if (count > 100)
            {
                return Helper.RedirectAjax("err", "单次生成补货任务个数不能超过100个！", null, "");
            }

            string result = cf.ReleaseSupplementTask(Session["whCode"].ToString(), Session["userName"].ToString(), count, poNumberList);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "生成成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult BoschReleaseSupplementTask()
        {
            int count = Convert.ToInt32(Request["bosch_txt_Qty"]);

            if (count > 100)
            {
                return Helper.RedirectAjax("err", "单次生成补货任务个数不能超过100个！", null, "");
            }

            string result = cf.BoschReleaseSupplementTask(Session["whCode"].ToString(), Session["userName"].ToString(), count);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "生成成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        [HttpGet]
        public ActionResult SupplementTaskDetailList()
        {
            WCF.RootService.SupplementTaskDetailSearch entity = new WCF.RootService.SupplementTaskDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.SupplementNumber = Request["SupplementNumber"].Trim();
            entity.LocationId = Request["detail_LocationId"].ToString();
            entity.PutLocationId = Request["detail_PutLocationId"].ToString();
            entity.AltItemNumber = Request["detail_AltItemNumber"].ToString();

            int total = 0;
            List<WCF.RootService.SupplementTaskDetailResult> list = cf.SupplementTaskDetailResultList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("SupplementNumber", "补货任务号");
            fieldsName.Add("HuId", "托盘号");
            fieldsName.Add("LocationId", "原始库位");
            fieldsName.Add("PutLocationId", "目标库位");
            fieldsName.Add("GroupNumber", "下架容器");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "LotDate");
            fieldsName.Add("UpdateUser", "操作人");
            fieldsName.Add("UpdateDate", "操作时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,UnitName:60,Qty:60,Status:60,UpdateDate:120,default:100"));
        }


        [HttpGet]
        public ActionResult SupplementTaskDetailDel()
        {
            string supplementNumber = Request["SupplementNumber"].Trim();
            string result = cf.SupplementTaskDetailDel(Session["whCode"].ToString(), supplementNumber, Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else if (result == "Y1")
            {
                return Helper.RedirectAjax("ok", "部分明细删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}
