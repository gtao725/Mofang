using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LoadContainerWarehouseController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf1 = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
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
            WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.LoadId = Request["load"].Trim();
            entity.ClientCode = Request["clientCode"].Trim();
            
            entity.ContainerNumberSix = Request["containerNumberSix"].Trim();
            entity.SealNumber = Request["sealNumber"].Trim();
  
            string poNumber = Request["containerNumber"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }
  
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
            List<WCF.OutBoundService.LoadContainerResult> list = cf.LoadChargeDetailWarehouseList(entity, poNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action1", "操作");
            fieldsName.Add("LoadMasterId", "loadMasterId");
            fieldsName.Add("Status0", "状态");
            fieldsName.Add("LoadId", "Load");
            fieldsName.Add("ClientCode", "客户名");

            fieldsName.Add("SumQty", "出货数量");
            fieldsName.Add("SumCBM", "总立方");

            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("SealNumber", "封号");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("ChuCangFS", "出仓方式");   
  
            fieldsName.Add("ETD", "ETD时间");
            fieldsName.Add("Port", "港区");

            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action1:55,Status0:60,LoadId:140,ClientCode:110,Port:170,ContainerNumber:90,SealNumber:75,CreateDate:130,ETD:130,ContainerType:60,SumQty:60,SumCBM:50,ChuCangFS:70,default:90"));
        }

        public ActionResult FeeDetailList()
        {
            WCF.OutBoundService.LoadChargeRuleSearch entity = new WCF.OutBoundService.LoadChargeRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["LoadId"];

            int total = 0;
            List<WCF.OutBoundService.LoadChargeDetailResult> list = cf.LoadChargeRuleWarehouseSelected(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ChargeName", "收费科目");
            fieldsName.Add("QtyCbm", "数量或立方");
            fieldsName.Add("ChargeUnitName", "计费单位");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:45,ChargeName:160,QtyCbm:170,default:90"));
        }

        [HttpPost]
        public ActionResult LoadContainerWarehouseEdit()
        {
            string loadid = Request["LoadId"];
            string whcode = Session["whCode"].ToString();
            string[] id = Request.Form.GetValues("addid");
            string[] qtycbm = Request.Form.GetValues("qtycbm");

            string result = cf.LoadContainerWarehouseEdit(whcode, loadid, id, qtycbm);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "保存成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}
