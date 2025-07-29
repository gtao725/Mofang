using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ReceiptPartialRegisterController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewBag.PhotoServerAddress = ConfigurationManager.AppSettings["PhotoServerAddress"].ToString();
            ViewBag.PhotoSaveAddress = ConfigurationManager.AppSettings["PhotoSaveAddress"].ToString();
            ViewBag.WMSUrl = ConfigurationManager.AppSettings["WMSUrl"].ToString();
            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewBag.userName = Session["userName"].ToString();
            ViewBag.whCode = Session["whCode"].ToString();

            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            return View();
        }

        //列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.ReceiptPartialRegisterSearch entity = new WCF.RootService.ReceiptPartialRegisterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.ClientCode = Request["clientCode"];

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }

            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }


            int total = 0;
            List<WCF.RootService.ReceiptPartialRegisterResult> list = cf.ReceiptPartialRegisterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("PhotoId", "照片ID");
            fieldsName.Add("UploadDate", "上传时间");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Qty", "拒收数量");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:120,UploadDate:130,ClientCode:120,ReceiptId:135,CreateDate:130,default:80"));
        }


        //列表
        [HttpGet]
        public ActionResult UnReceiptList()
        {
            WCF.RootService.ReceiptPartialUnPreceiptSearch entity = new WCF.RootService.ReceiptPartialUnPreceiptSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.ClientCode = Request["clientCode"];
            int total = 0;
            List<WCF.RootService.ReceiptPartialUnReceiptResult> list = cf.ReceiptPartialUnReceiptList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");

            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("PoNumber", "PO");
            fieldsName.Add("itemNumber", "款号");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("UnQty", "未收货数量");
            fieldsName.Add("RegisteredQty", "已登记数量");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,UploadDate:130,ClientCode:120,ReceiptId:135,CreateDate:130,default:80"));
        }


        //列表
        [HttpGet]
        public ActionResult RegisteredList()
        {
            WCF.RootService.ReceiptPartialUnPreceiptSearch entity = new WCF.RootService.ReceiptPartialUnPreceiptSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.ClientCode = Request["clientCode"];
            int total = 0;
            List<WCF.RootService.ReceiptPartialRegisteredDetailResult> list = cf.RegisteredList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("PoNumber", "PO");
            fieldsName.Add("itemNumber", "款号");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("RegisteredQty", "已登记数量");
            fieldsName.Add("Reason", "原因");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,UploadDate:130,ClientCode:120,ReceiptId:135,CreateDate:130,default:80"));
            //return null;
        }



        [HttpGet]
        public ActionResult UnReceiptRegister()
        {
            WCF.RootService.ReceiptPartialRegisterDetail entity = new WCF.RootService.ReceiptPartialRegisterDetail();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.ReceiptId = Request["ReceiptId"];
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["SoNumber"];
            entity.PoNumber = Request["PoNumber"];
            entity.ItemNumber = Request["ItemNumber"];
            entity.ItemId = int.Parse(Request["ItemId"]);
            entity.Qty = int.Parse(Request["edit_qty"]);
            entity.Reason = Request["edit_Reason"];
            entity.CreateDate = DateTime.Now;
            entity.CreateUser = Session["userName"].ToString();
            string result = cf.UnReceiptRegister(entity, int.Parse(Request["unQty"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }



        }


        [HttpGet]
        public ActionResult DeleteDetail()
        {
            string Id = Request["Id"];
            string rediptId = Request["receiptId"];
            string WhCode = Session["whCode"].ToString();
            //return null;

            string result = cf.ReceiptPartialDeleteDetail(int.Parse(Id), rediptId, WhCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        public ActionResult ReceiptPartialComplete()
        {
            string Id = Request["Id"];
            string rediptId = Request["receiptId"];
            string WhCode = Session["whCode"].ToString();
            //return null;

            string result = cf.ReceiptPartialComplete(rediptId, WhCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        public ActionResult ReceiptPartialReBack()
        {
            string Id = Request["Id"];
            string rediptId = Request["receiptId"];
            string WhCode = Session["whCode"].ToString();
            //return null;

            string result = cf.ReceiptPartialReBack(rediptId, WhCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "状态回撤成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        [HttpPost]
        public ActionResult GetHoldMasterListByReceiptPart()
        {
            WCF.RootService.HoldMasterSearch entity = new WCF.RootService.HoldMasterSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"];

            List<WCF.RootService.HoldMaster> list = cf.HoldMasterListByReceiptPart(entity).ToList();
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.HoldReason.ToString(),
                              Text = r.HoldReason    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }


        //照片上传完成
        [HttpGet]
        public void ReceiptPartPhotoUpload()
        {
            WCF.RootService.ReceiptPartialRegister entity = new WCF.RootService.ReceiptPartialRegister();
            if (Request["fileId"] != null)
            {
                entity.PhotoId = Convert.ToInt32(Request["fileId"]);
            }
            else
            {
                entity.PhotoId = 0;
            }
            entity.Id = Convert.ToInt32(Request["Id"] == null ? "0" : Request["Id"]);

            string result = cf.ReceiptPartPhotoUpload(entity);

        }

    }
}
