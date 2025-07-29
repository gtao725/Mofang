using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;

namespace WMS.UI.Controllers
{
    public class InCFSPhotoKController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        WCF.RecService.RecServiceClient recService = new WCF.RecService.RecServiceClient();

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
                                           Value = r.ClientCode.ToString()
                                       };

            WCF.RecService.HoldMasterSearch entity = new WCF.RecService.HoldMasterSearch();
            entity.pageIndex = 1;
            entity.pageSize = 200;
            entity.WhCode = Session["whCode"].ToString();
            int total = 0;

            ViewData["HoldReasonList"] = from r in recService.HoldMasterListByRec(entity, out total)
                                         select new SelectListItem()
                                         {
                                             Text = r.HoldReason,     //text
                                             Value = r.HoldReason.ToString()
                                         };

            return View();
        }

        //收货照片列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.PhotoMasterSearch entity = new WCF.RootService.PhotoMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["ClientCode"];
            entity.Number = Request["number"];
            entity.Number2 = Request["number2"];
            entity.HuId = Request["huid"];
            entity.TCRStatus = Request["TCRStatus"];
            entity.HoldReason = Request["holdReason"];
            entity.CheckStatus1 = Request["checkStatus1"];
            entity.KRemark1 = Request["KRemark1"];
            entity.HoldReason1 = Request["holdreason1"];

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

            if (Request["HoldReasonType"] != "")
            {
                if (Request["HoldReasonType"] == "非标签")
                {
                    entity.HoldReasonTypeNot = "标签";
                }
                else
                {
                    entity.HoldReasonType = Request["HoldReasonType"].ToString();
                }
            }

            int total = 0;
            List<WCF.RootService.PhotoMasterResult> list = cf.InCFSPhotoMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("PhotoId", "照片ID");
            fieldsName.Add("Number", "收货批次号");
            fieldsName.Add("Number2", "SO号");
            fieldsName.Add("Number3", "PO");
            fieldsName.Add("Number4", "SKU");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "理货员");
            fieldsName.Add("RegQty", "登记数量");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("HoldReason", "TCR类型");
            fieldsName.Add("TCRStatus", "TCR状态");
            fieldsName.Add("TCRCheckUser", "TCR处理人");
            fieldsName.Add("TCRCheckDate", "处理时间");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");
            fieldsName.Add("SettlementMode", "结算方式");
            fieldsName.Add("SumPrice", "金额");
            fieldsName.Add("DeliveryDate", "收货时间");
            fieldsName.Add("Status", "是否上传");
            fieldsName.Add("UploadDate", "上传时间");
            fieldsName.Add("CheckStatus1", "客服审核状态");
            fieldsName.Add("CheckUser1", "客服审核人");
            fieldsName.Add("CheckDate1", "客服审核时间");
            fieldsName.Add("KRemark1", "客服备注");
            fieldsName.Add("CheckStatus2", "仓库审核状态");
            fieldsName.Add("CheckUser2", "仓库审核人");
            fieldsName.Add("CheckDate2", "仓库审核时间");
            fieldsName.Add("CRemark1", "仓库备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("OrderSource", "来源");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:140,PhotoId:60,Number2:130,Number3:130,Number4:130,Status:60,Qty:60,RegQty:55,UnitName:50,HuId:80,HoldReason:100,TCRStatus:65,TCRProcessMode:160,SumPrice:65,SettlementMode:60,LocationId:80,CreateUser:60,OrderSource:90,TCRCheckUser:80,CheckStatus1:90,CheckUser1:80,CheckStatus2:90,CheckUser2:80,UserCode:50,UserNameCN:50,TCRCheckDate:120,Style1:70,Style2:70,Style3:70,default:110"));
        }


        [HttpPost]
        //查询客户异常原因
        public ActionResult GetHoldMaster()
        {
            WCF.RootService.HoldMasterSearch entity = new WCF.RootService.HoldMasterSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_ClientCode"];
            int total = 0;
            WCF.RootService.HoldMaster[] list = cf.HoldMasterList(entity, out total);

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

        //新增TCR
        [HttpPost]
        public ActionResult PhotoMasterAdd()
        {
            List<WCF.RootService.PhotoMaster> list = new List<WCF.RootService.PhotoMaster>();
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();
            try
            {
                if (Request["txt_Qty"] != "")
                {
                    entity.Qty = Convert.ToInt32(Request["txt_Qty"]);
                    if (entity.Qty < 1)
                    {
                        return Helper.RedirectAjax("err", "数量必须大于0！", null, "");
                    }
                }
                else
                {
                    entity.Qty = 0;
                }
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "数量必须为数字！", null, "");
            }

            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_ClientCode"];
            entity.Number = Request["txt_ReceiptId"].Trim();
            entity.Number2 = Request["txt_SoNumber"].Trim();
            entity.Number3 = Request["txt_CustomerPoNumber"].Trim();
            entity.Number4 = Request["txt_AltNumber"].Trim();

            entity.HuId = Request["txt_HuId"].Trim();
            entity.HoldReason = Request["txt_HoldReason"].Trim();
            entity.KRemark1 = Request["txt_Kremark1"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.PhotoMasterAdd(list.ToArray(), Session["whCode"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改TCR
        [HttpGet]
        public ActionResult PhotoMasterEdit()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();

            try
            {
                if (Request["edit_SumPrice"] != "")
                {
                    entity.SumPrice = Convert.ToDecimal(Request["edit_SumPrice"]);

                    if (entity.SumPrice < 0)
                    {
                        return Helper.RedirectAjax("err", "金额必须大于0！", null, "");
                    }
                }
                else
                {
                    entity.SumPrice = 0;
                }
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "金额必须为数字！", null, "");
            }

            entity.Id = Convert.ToInt32(Request["id"]);
            entity.TCRProcessMode = Request["edit_TCRProcessMode"];
            entity.SettlementMode = Request["edit_SettlementMode"];
            entity.KRemark1 = Request["edit_KRemark1"];
            string result = cf.PhotoMasterEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改TCR
        [HttpGet]
        public ActionResult EditTCRStatus()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();

            entity.Id = Convert.ToInt32(Request["id"]);
            entity.TCRStatus = "处理中";
            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.EditTCRStatus(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "处理成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除TCR
        [HttpGet]
        public ActionResult DelTCR()
        {
            int result = cf.PhotoMasterDel(Convert.ToInt32(Request["id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


        //新增TCR处理方式
        [HttpGet]
        public ActionResult TCRProcessModeAdd()
        {
            WCF.RootService.TCRProcess entity = new WCF.RootService.TCRProcess();

            entity.WhCode = Session["whCode"].ToString();
            entity.TCRProcessMode = Request["add_TCRProcessMode"];
            entity.CreateUser = Session["userName"].ToString();

            string result = cf.TCRProcessModeAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除TCR处理方式
        [HttpGet]
        public ActionResult TCRProcessModeDel()
        {
            int result = cf.TCRProcessModeDel(Convert.ToInt32(Request["Id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //TCR处理方式列表
        [HttpGet]
        public ActionResult TCRProcessModeSearch()
        {
            WCF.RootService.PhotoMasterSearch entity = new WCF.RootService.PhotoMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;
            List<WCF.RootService.TCRProcessResult> list = cf.TCRProcessModeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:50,TCRProcessMode:300,CreateDate:120,default:100"));
        }

        //TCR处理方式下拉列表
        [HttpPost]
        public ActionResult TCRProcessModeSelect()
        {
            WCF.RootService.PhotoMasterSearch entity = new WCF.RootService.PhotoMasterSearch();
            entity.pageIndex = 1;
            entity.pageSize = 200;
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;

            List<WCF.RootService.TCRProcessResult> list1 = cf.TCRProcessModeList(entity, out total).ToList();

            if (list1 != null)
            {
                var sql = from r in list1
                          select new
                          {
                              Value = r.TCRProcessMode.ToString(),
                              Text = r.TCRProcessMode    //text
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
        public void InCFSPhotoKComplete()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();
            if (Request["fileId"] != null)
            {
                entity.PhotoId = Convert.ToInt32(Request["fileId"]);
            }
            else
            {
                entity.PhotoId = 0;
            }
            entity.WhCode = Request["whCode"];
            entity.Number = Request["number"];
            entity.Number2 = Request["number2"];
            entity.CreateUser = Request["user"];
            entity.Id = Convert.ToInt32(Request["Id"] == null ? "0" : Request["Id"]);
            string result = cf.InCFSPhotoCComplete(entity);

        }

        //照片上传完成
        [HttpGet]
        public void InCFSPhotoKShenheComplete()
        {
            WCF.RootService.PhotoMaster entity = new WCF.RootService.PhotoMaster();
            entity.PhotoId = Convert.ToInt32(Request["photoid"]);
            entity.WhCode = Request["whCode"];
            entity.Number = Request["number"];
            entity.Number2 = Request["number2"];
            entity.CheckUser1 = Request["user"];
            entity.Id = Convert.ToInt32(Request["Id"] == null ? "0" : Request["Id"]);
            string result = cf.CFSPhotoCShenheComplete(entity);

        }

        //清除照片ID
        [HttpGet]
        public ActionResult PhotoMasterDelPhotoId()
        {
            string result = cf.PhotoMasterDelPhotoId(Convert.ToInt32(Request["Id"]), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "清除照片ID成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "清除失败！", null, "");
            }
        }

    }
}
