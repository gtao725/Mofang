using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_RecQuestionController : Controller
    {
        WCF.RecService.RecServiceClient cf = new WCF.RecService.RecServiceClient();
        WCF.InBoundService.InBoundServiceClient incf = new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in incf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };

            ViewData["LotFlagSelect"] = from r in cf.LotFlagListSelect()
                                             select new SelectListItem()
                                             {
                                                 Text = r.Description,     //text
                                                 Value = r.ColumnKey
                                             };

            return View();

        }

        [DefaultRequest]
        public ActionResult Index1()
        {
            ViewData["WhClientList"] = from r in incf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };

            ViewData["LotFlagSelect"] = from r in cf.LotFlagListSelect()
                                        select new SelectListItem()
                                        {
                                            Text = r.Description,     //text
                                            Value = r.ColumnKey
                                        };

            return View();

        }

        public ActionResult List()
        {
            WCF.RecService.ReceiptSearch entity = new WCF.RecService.ReceiptSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode= Request["ClientCode"].Trim();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.HuId = Request["hu_id"].Trim();
            entity.SoNumber = Request["so_number"].Trim();
            entity.CustomerPoNumber = Request["customer_po"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();

            if (Request["ReceiptDateBegin"] != "")
            {
                entity.ReceiptDateBegin = Convert.ToDateTime(Request["ReceiptDateBegin"]);
            }
            else
            {
                entity.ReceiptDateBegin = null;
            }
            if (Request["ReceiptDateEnd"] != "")
            {
                entity.ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"]).AddDays(1);
            }
            else
            {
                entity.ReceiptDateEnd = null;
            }

            int total = 0;
            string str = "";
            List<WCF.RecService.ReceiptResult> list = cf.C_RecQuestionList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("TransportType", "车型");
            fieldsName.Add("TransportTypeExtend", "是否超长");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LotFlagShow", "分票");        
            fieldsName.Add("HoldReason", "异常原因");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("UnitId", "单位ID");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("UnitNameShow", "单位名");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("Custom1", "客户定制1");
            fieldsName.Add("Custom2", "客户定制2");
            fieldsName.Add("Custom3", "客户定制3");
            fieldsName.Add("CreateUser", "理货员");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,ReceiptId:120,ReceiptDate:120,HuId:70,LotFlagShow:40,Qty:60,Length:60,Width:60,Height:60,Weight:60,CBM:60,UnitName:60,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,Style1:60,default:90", null, "", 50, str));
        }

        public ActionResult List1()
        {
            WCF.RecService.ReceiptSearch entity = new WCF.RecService.ReceiptSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["ClientCode"].Trim();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.HuId = Request["hu_id"].Trim();
            entity.SoNumber = Request["so_number"].Trim();
            entity.CustomerPoNumber = Request["customer_po"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();

            if (Request["createUser"].Trim() != "")
            {
                entity.CreateUser = Session["userName"].ToString();
            }

            if (Request["ReceiptDateBegin"] != "")
            {
                entity.ReceiptDateBegin = Convert.ToDateTime(Request["ReceiptDateBegin"]);
            }
            else
            {
                entity.ReceiptDateBegin = null;
            }
            if (Request["ReceiptDateEnd"] != "")
            {
                entity.ReceiptDateEnd = Convert.ToDateTime(Request["ReceiptDateEnd"]).AddDays(1);
            }
            else
            {
                entity.ReceiptDateEnd = null;
            }

            int total = 0;
            string str = "";
            List<WCF.RecService.ReceiptResult> list = cf.C_RecQuestionList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("TransportType", "车型");
            fieldsName.Add("TransportTypeExtend", "是否超长");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LotFlagShow", "分票");
            fieldsName.Add("HoldReason", "异常原因");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("UnitId", "单位ID");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("UnitNameShow", "单位名");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("Custom1", "客户定制1");
            fieldsName.Add("Custom2", "客户定制2");
            fieldsName.Add("Custom3", "客户定制3");
            fieldsName.Add("CreateUser", "理货员");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,ReceiptId:120,ReceiptDate:120,HuId:70,LotFlagShow:40,Qty:60,Length:60,Width:60,Height:60,Weight:60,CBM:60,UnitName:60,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,Style1:60,default:90", null, "", 50, str));
        }

        [HttpGet]
        public ActionResult EditDetail()
        {
            int id = Convert.ToInt32(Request["edit_id"]);
            WCF.RecService.Receipt entity = new WCF.RecService.Receipt();
            entity.Id = id;
            entity.WhCode = Session["whCode"].ToString();
            entity.Qty = Convert.ToInt32(Request["edit_qty"]);
            entity.Length = Convert.ToDecimal(Request["edit_length"]);
            entity.Width = Convert.ToDecimal(Request["edit_width"]);
            entity.Height = Convert.ToDecimal(Request["edit_height"]);
            entity.Weight = Convert.ToDecimal(Request["edit_weight"]);
            entity.UnitName = Request["edit_UnitName"];

            if (string.IsNullOrEmpty(entity.UnitName))
            {
                return Helper.RedirectAjax("err", "收货单位异常，请重新选择！", null, "");
            }
            else
            {
                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;
                string result = cf.ReceiptEdit(entity);
                if (result == "Y")
                {
                    return Helper.RedirectAjax("ok", "修改成功！", null, "");
                }
                else
                {
                    return Helper.RedirectAjax("err", result, null, "");
                }
            }
        }

        [HttpGet]
        public ActionResult EditDetailCustom() 
        {
            int id = Convert.ToInt32(Request["edit_id"]);
            WCF.RecService.Receipt entity = new WCF.RecService.Receipt();
            entity.Id = id;
            entity.WhCode = Session["whCode"].ToString();
            entity.Custom1 = Request["edit_custom1"];
            entity.Custom2 = Request["edit_custom2"];
            entity.Custom3 = Request["edit_custom3"];

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            string result = cf.ReceiptEditDetailCustom(entity); 
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpPost]
        public void CheckClientCode()
        {
            string[] id = Request.Form.GetValues("id");
            string[] clientCode = Request.Form.GetValues("clientCode");

            string result = "";
            List<string> list = new List<string>();
            int count = 0;
            foreach (var item in clientCode)
            {
                if (count == 0)
                {
                    result = "Y$" + item;
                    list.Add(item);
                    count++;
                }
                if (list.Contains(item) == false)
                {
                    result = "N$";
                    break;
                }
            }

            Response.Write(result);


        }

        [HttpPost]
        //托盘批量修改TCR
        public ActionResult FrozenEditList()
        {
            string[] id = Request.Form.GetValues("id");
            string holdReason = Request["holdReason"].ToString();
            holdReason = holdReason.Substring(0, holdReason.Length - 1);
            string username = Session["userName"].ToString();

            string result = cf.FrozenEditList(id, Session["whCode"].ToString(), holdReason, username);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpPost]
        //托盘批量清除TCR
        public ActionResult FrozenDeleteList()
        {
            string[] id = Request.Form.GetValues("id");
            string username = Session["userName"].ToString();

            string result = cf.FrozenDeleteList(id, Session["whCode"].ToString(), username);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量清除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpPost]
        //托盘批量修改分票
        public ActionResult LotFlagEditList()
        {
            string[] id = Request.Form.GetValues("id");
            string username = Session["userName"].ToString();
            string lotFlag = Request["lotFlag"];

            string result = cf.LotFlagEditList(id, Convert.ToInt32(lotFlag), Session["whCode"].ToString(), username);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpPost]
        //托盘批量清除分票
        public ActionResult LotFlagDeleteList()
        {
            string[] id = Request.Form.GetValues("id");
            string username = Session["userName"].ToString();

            string result = cf.LotFlagEditList(id, 0, Session["whCode"].ToString(), username);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量清除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpGet]
        //查询客户异常原因
        public ActionResult HoldMasterListByRec()
        {
            WCF.RecService.HoldMasterSearch entity = new WCF.RecService.HoldMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"];
            int total = 0;
            WCF.RecService.HoldMaster[] list = cf.HoldMasterListByRec(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("HoldReason", "异常原因");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "HoldReason:200,default:120"));
        }


        [HttpPost]
        public ActionResult GetUnitDefaultList()
        {
            IEnumerable<WCF.InBoundService.UnitDefault> list = incf.UnitDefaultListSelect(Session["whCode"].ToString());
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.UnitName.ToString(),
                              Text = r.UnitNameCN    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }


        [HttpPost]
        public ActionResult EditRegTransportType()
        {
            string[] receiptId = Request.Form.GetValues("receiptId");
            string editTransportType = Request["editTransportType"].ToString();
            string editTransportTypeExtend = Request["editTransportTypeExtend"].ToString();

            string username = Session["userName"].ToString();

            string receipt_id = receiptId[0].ToString();

            string result = cf.ReceiptEditRegTransportType(receipt_id, Session["whCode"].ToString(), editTransportType, editTransportTypeExtend, username);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

    }
}
