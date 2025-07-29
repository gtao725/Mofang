using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SerialNumberInController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };
            return View();
        }

        //序列号采集列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.SerialNumberInSearch entity = new WCF.RootService.SerialNumberInSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["ClientCode"];
            entity.ReceiptId = Request["txt_receiptId"].Trim();
            entity.SoNumber = Request["txt_soNumber"].Trim();
            entity.CustomerPoNumber = Request["txt_customerNumber"].Trim();
            entity.AltItemNumber = Request["txt_altItemNumber"].Trim();
            entity.CartonId = Request["txt_cartonId"].Trim();
            entity.HuId = Request["txt_huId"].Trim();

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
            List<WCF.RootService.SerialNumberInOut> list = cf.SerialNumberInList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("PoId", "PoId");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("CartonId", "序列号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,ReceiptId:130,CreateDate:130,CartonId:150,CreateUser:80,UpdateUser:80,UpdateDate:130,default:100"));
        }


        //序列号采集列表
        [HttpGet]
        public ActionResult SerialNumberDetail()
        {

            WCF.RootService.SerialNumberDetailSearch entity = new WCF.RootService.SerialNumberDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.HeadId = Convert.ToInt32(Request["HeadId"]);
            entity.UPC = Request["UPC"];

            int total = 0;
            List<WCF.RootService.SerialNumberDetailOut> list = cf.SerialNumberDetailList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("SNType", "进出类型");
            fieldsName.Add("PCS", "数量");
            fieldsName.Add("UPC", "UPC号");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "SNType:100,UPC:130,CreateDate:130,default:100"));
        }

        //序列号信息修改
        [HttpGet]
        public ActionResult SerialNumberEdit()
        {
            WCF.RootService.SerialNumberIn entity = new WCF.RootService.SerialNumberIn();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.CartonId = Request["cartonId"].Trim();
            entity.UpdateUser = Session["userName"].ToString();

            int result = cf.SerialNumberEdit(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "序列号已存在！", null, "");
            }
        }

        //删除序列号
        [HttpGet]
        public ActionResult SerialNumberDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.SerialNumberDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //按照托盘删除序列号
        [HttpGet]
        public ActionResult SerialNumberDelByHuId()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.RootService.SerialNumberIn entity = new WCF.RootService.SerialNumberIn();
            entity.Id = id;

            int result = cf.SerialNumberDelByHuId(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //新增序列号信息
        [HttpGet]
        public ActionResult SerialNumberAdd()
        {
            WCF.RootService.SerialNumberIn entity = new WCF.RootService.SerialNumberIn();
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receiptId"].Trim();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.ClientCode = Request["clientCode"].Trim();
            entity.SoNumber = Request["soNumber"].Trim();
            entity.CustomerPoNumber = Request["customerPoNumber"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.PoId = Convert.ToInt32(Request["poId"]);
            entity.ItemId = Convert.ToInt32(Request["itemId"]);
            entity.HuId = Request["huId"].Trim();
            entity.CartonId = Request["add_cartonId"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            int result = cf.SerialNumberAdd(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "序列号已存在，添加失败！", null, "");
            }
        }
    }
}
