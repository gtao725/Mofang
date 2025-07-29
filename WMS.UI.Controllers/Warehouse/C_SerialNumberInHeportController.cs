using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SerialNumberInHeportController : Controller
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

        //序列号采集列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.SerialNumberInSearch entity = new WCF.RootService.SerialNumberInSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
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
            List<WCF.RootService.HeportSerialNumberIn> list = cf.HeportSerialNumberInList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
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

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,ReceiptId:130,CreateDate:130,CartonId:150,CreateUser:80,UpdateUser:80,UpdateDate:130,default:100"));
        }




        //序列号信息修改
        [HttpGet]
        public ActionResult SerialNumberEdit()
        {
            WCF.RootService.HeportSerialNumberIn entity = new WCF.RootService.HeportSerialNumberIn();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.CartonId = Request["cartonId"].Trim();
            entity.UpdateUser = Session["userName"].ToString();

            int result = cf.HeportSerialNumberEdit(entity);
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
            int result = cf.HeportSerialNumberDel(id);
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
            WCF.RootService.HeportSerialNumberIn entity = new WCF.RootService.HeportSerialNumberIn();
            entity.Id = id;

            int result = cf.HeportSerialNumberDelByHuId(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


    }
}
