using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class InBoundOrderDCController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            return View();
        }

        [DefaultRequest]
        public ActionResult AddIndex()
        {
            ViewData["client_id"] = Request["clientId"];
            ViewData["client_name"] = Request["clientName"];
            ViewData["po_number"] = Request["add_po"].Trim();

            ViewData["ClientFLowNameList"] = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]), "", Request["add_po"].Trim(), "DC")
                                             select new SelectListItem()
                                             {
                                                 Text = r.FlowName,     //text
                                                 Value = r.Id.ToString()
                                             };

            return View();
        }

        //预录入查询明细列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.InBoundService.InBoundOrderDetailSearch entity = new WCF.InBoundService.InBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.CustomerPoNumber = Request["poNumber"].Trim();
            entity.OrderType = "DC";

            string[] itemNumberList = null;
            string itemNumber = Request["sku_area"];
            if (!string.IsNullOrEmpty(itemNumber))
            {
                string item_temp = itemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                itemNumberList = item_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.InBoundService.InBoundOrderDetailResult> list = cf.InBoundOrderDetailListDC(entity, itemNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "SKU");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Qty", "预录入数量");
            fieldsName.Add("RegQty", "已登记数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "体积");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CustomerPoNumber:150,AltItemNumber:150,default:65", null, "", 200, str));
        }

        //验证数组款号 属性 对应的单位是否有误
        [HttpPost]
        public void CheckUnitName()
        {
            string[] Id = Request.Form.GetValues("Id");
            string[] alt_item_number = Request.Form.GetValues("alt_item_number");
            string[] style1 = Request.Form.GetValues("style1");
            string[] style2 = Request.Form.GetValues("style2");
            string[] style3 = Request.Form.GetValues("style3");
            string[] unitId = Request.Form.GetValues("unitId");
            string[] unitName = Request.Form.GetValues("unitName");
            string[] qty = Request.Form.GetValues("qty");
            string[] weight = Request.Form.GetValues("weight");
            string[] cbm = Request.Form.GetValues("cbm");

            if (Id == null)
            {
                Response.Write("数组有误无法保存！");
                return;
            }

            WCF.InBoundService.InBoundOrderInsert entity = new WCF.InBoundService.InBoundOrderInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["ClientId"]);
            entity.ClientCode = Request["ClientName"].Trim();
            entity.OrderType = "DC";
            entity.ProcessId = Convert.ToInt32(Request["ProcessId"]);
            entity.ProcessName = Request["ProcessName"];

            List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

            for (int i = 0; i < Id.Length; i++)
            {
                WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                orderDetail.JsonId = Convert.ToInt32(Id[i].ToString());
                orderDetail.CustomerPoNumber = Request["poNumber"].Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.AltItemNumber = alt_item_number[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.Style1 = style1[i].Trim().ToString();
                orderDetail.Style2 = style2[i].Trim().ToString();
                orderDetail.Style3 = style3[i].Trim().ToString();
                if (unitId[i].ToString() == "")
                {
                    orderDetail.UnitId = 0;
                }
                else
                {
                    orderDetail.UnitId = Convert.ToInt32(unitId[i].ToString());
                }
                if (unitName[i].ToString() == "")
                {
                    orderDetail.UnitName = "none";
                }
                else
                {
                    orderDetail.UnitName = unitName[i].ToString();
                }
                orderDetail.Qty = Convert.ToInt32(qty[i].ToString());
                orderDetail.Weight = Convert.ToDecimal(weight[i].ToString());
                orderDetail.CBM = Convert.ToDecimal(cbm[i].ToString());
                orderDetail.CreateUser = Session["userName"].ToString();
                orderDetailList.Add(orderDetail);
            }

            entity.InBoundOrderDetailInsert = orderDetailList.ToArray();
            string result = cf.CheckUnitName(entity);
            if (result.Substring(0, 1) == "N")
            {
                Response.Write(result.Substring(1, result.Length - 1));
            }
            else
            {
                AddInBoundOrder(entity);
            }
        }

        //添加预录入
        [HttpPost]
        public void AddInBoundOrder(WCF.InBoundService.InBoundOrderInsert entity)
        {
            string result = cf.InBoundOrderListAddCommon(entity);
            if (result.Substring(0, 1) == "N")
            {
                Response.Write(result.Substring(1, result.Length - 1));
            }
            else
            {
                Response.Write("Y$" + result);
            }
        }

        //修改预录入明细信息
        [HttpGet]
        public ActionResult InBoundOrderDetailEdit()
        {
            WCF.InBoundService.InBoundOrderDetail entity = new WCF.InBoundService.InBoundOrderDetail();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Qty = Convert.ToInt32(Request["edit_qty"].Trim());

            if (Request["edit_weight"] == "")
            {
                entity.Weight = 0;
            }
            else
            {
                entity.Weight = Convert.ToDecimal(Request["edit_weight"]);
            }
            if (Request["edit_cbm"] == "")
            {
                entity.CBM = 0;
            }
            else
            {
                entity.CBM = Convert.ToDecimal(Request["edit_cbm"]);
            }

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            string result = cf.InBoundOrderDetailEdit(entity, new string[] { "Qty", "Weight", "CBM", "UpdateUser", "UpdateDate" });
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //预录入明细删除
        [HttpGet]
        public ActionResult InBoundOrderDetailDel()
        {
            string result = cf.InBoundOrderDetailDel(Convert.ToInt32(Request["Id"]), Session["userName"].ToString());
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
