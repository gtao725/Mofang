using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SerialNumberOutController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();

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
            WCF.RootService.SerialNumberOutSearch entity = new WCF.RootService.SerialNumberOutSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["ClientCode"];
            entity.LoadId = Request["txt_receiptId"].Trim();
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
            List<WCF.RootService.SerialNumberOut> list = cf.SerialNumberOutList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("CartonId", "序列号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:120,LoadId:130,CreateDate:130,UpdateDate:130,CartonId:150,CreateUser:80,UpdateUser:80,default:100"));
        }


        //序列号信息修改
        [HttpGet]
        public ActionResult SerialNumberEdit()
        {
            WCF.RootService.SerialNumberOut entity = new WCF.RootService.SerialNumberOut();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.CartonId = Request["cartonId"].Trim();
            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.SerialNumberOutEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除序列号
        [HttpGet]
        public ActionResult SerialNumberDel()
        {
            WCF.RootService.SerialNumberOut entity = new WCF.RootService.SerialNumberOut();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.SerialNumberOutDel(entity);
            if (result == "Y")
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
            WCF.RootService.SerialNumberOut entity = new WCF.RootService.SerialNumberOut();
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["receiptId"].Trim();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.ClientCode = Request["clientCode"].Trim();
            entity.SoNumber = Request["soNumber"].Trim();
            entity.CustomerPoNumber = Request["customerPoNumber"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.ItemId = Convert.ToInt32(Request["itemId"]);
            entity.HuId = Request["huId"].Trim();
            entity.CartonId = Request["add_cartonId"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            string result = cf.SerialNumberOutAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //新增序列号信息
        [HttpGet]
        public ActionResult SerialNumberAddOther()
        {
            WCF.RootService.SerialNumberOut entity = new WCF.RootService.SerialNumberOut();
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["receiptId"].Trim();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.ClientCode = Request["clientCode"].Trim();

            entity.SoNumber = Request["add_soNumber"].Trim();
            entity.CustomerPoNumber = Request["add_poNumber"].Trim();
            entity.AltItemNumber = Request["add_altItemNumber"].Trim();
            entity.HuId = Request["add_huId"].Trim();
            entity.CartonId = Request["add_cartonId"].Trim();

            entity.CreateUser = Session["userName"].ToString();

            string result = cf.SerialNumberAddOther(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }
    }
}
