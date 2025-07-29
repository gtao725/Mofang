using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class InBoundOrderController : Controller
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
        public ActionResult EditIndex()
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
            ViewData["so_number"] = Request["add_so"].Trim();
            ViewData["po_number"] = Request["add_po"].Trim();

            if (Request["add_so"] != "")
            {
                ViewData["ClientFLowNameList"] = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]), Request["add_so"].Trim(), "", "CFS")
                                                 select new SelectListItem()
                                                 {
                                                     Text = r.FlowName,     //text
                                                     Value = r.Id.ToString()
                                                 };
            }
            else
            {
                ViewData["ClientFLowNameList"] = from r in cf.ClientFlowNameSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["clientId"]), "", Request["add_po"].Trim(), "DC")
                                                 select new SelectListItem()
                                                 {
                                                     Text = r.FlowName,     //text
                                                     Value = r.Id.ToString()
                                                 };
            }

            return View();
        }

        //编辑预录入查询列表
        [HttpGet]
        public ActionResult ListDetail()
        {
            WCF.InBoundService.InBoundOrderSearch entity = new WCF.InBoundService.InBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.CustomerPoNumber = Request["edit_customer_po"].Trim();
            entity.ClientCode = Request["clientCode"].Trim();

            string poNumber = Request["edit_so_number"];
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
            List<WCF.InBoundService.InBoundOrderResult> list = cf.InBoundListDetail(entity, poNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Action1", "修改客户名");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "进仓单");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("TotalQty", "预录数量");
            fieldsName.Add("TotalRegQty", "登记数量");
            fieldsName.Add("ProcessName", "流程名称");
            fieldsName.Add("OrderType", "OrderType");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Action1:70,TotalQty:60,TotalRegQty:60,ProcessName:100,default:130"));
        }

        //预录入查询明细列表
        [HttpGet]
        public ActionResult List()
        {
            if (Request["soNumber"] != "")
            {
                WCF.InBoundService.InBoundOrderDetailSearch entity = new WCF.InBoundService.InBoundOrderDetailSearch();
                entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
                entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientId = Convert.ToInt32(Request["clientId"]);
                entity.SoNumber = Request["soNumber"].Trim();
                entity.OrderType = "CFS";

                string poNumber = Request["po_area"];
                string[] poNumberList = null;
                string[] itemNumberList = null;
                if (!string.IsNullOrEmpty(poNumber))
                {
                    string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                    poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
                }

                string itemNumber = Request["sku_area"];
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    string item_temp = itemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                    itemNumberList = item_temp.Split('@');           //把SO 按照@分割，放在数组
                }

                int total = 0;
                string str = "";
                List<WCF.InBoundService.InBoundOrderDetailResult> list = cf.InBoundOrderDetailList(entity, poNumberList, itemNumberList, out total, out str).ToList();

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
                fieldsName.Add("Weight", "重量");
                fieldsName.Add("CBM", "体积");
                fieldsName.Add("UnitName", "单位名");
                fieldsName.Add("UnitId", "单位ID");

                return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CustomerPoNumber:150,AltItemNumber:150,default:65", null, "", 200, str));
            }
            else
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
                fieldsName.Add("Weight", "重量");
                fieldsName.Add("CBM", "体积");

                return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CustomerPoNumber:150,AltItemNumber:150,default:65", null, "", 200, str));
            }

        }

        //验证数组款号 属性 对应的单位是否有误
        [HttpPost]
        public void CheckUnitName()
        {
            string[] Id = Request.Form.GetValues("Id");
            string[] customer_po = Request.Form.GetValues("customer_po");
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
            entity.ClientCode = Request["ClientName"];
            entity.ProcessId = Convert.ToInt32(Request["ProcessId"]);
            entity.ProcessName = Request["ProcessName"];

            if (Request["SoNumber"] != "")
            {
                entity.SoNumber = Request["SoNumber"].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entity.OrderType = "CFS";
            }
            else
            {
                entity.SoNumber = "";
                entity.OrderType = "DC";
            }

            List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

            for (int i = 0; i < Id.Length; i++)
            {
                WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                orderDetail.JsonId = Convert.ToInt32(Id[i].ToString());
                orderDetail.CustomerPoNumber = customer_po[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.AltItemNumber = alt_item_number[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.Style1 = style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.Style2 = style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                orderDetail.Style3 = style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
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
                    orderDetail.UnitName = unitName[i].Trim().ToString();
                }
                orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                orderDetail.Weight = Convert.ToDecimal(weight[i].Trim().ToString());
                orderDetail.CBM = Convert.ToDecimal(cbm[i].Trim().ToString());
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
            entity.Qty = Convert.ToInt32(Request["edit_qty"]);

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

        //修改预录入客户名
        [HttpPost]
        public ActionResult InBoundOrderEditClientCode()
        {
            string[] soList = Request.Form.GetValues("soNumber");
            string[] clientCodeList = Request.Form.GetValues("clientCode");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.ClientCode = Request["clientCode"];
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            string result = cf.InBoundOrderEditClientCode(entity, soList.ToArray(), clientCodeList.ToArray());

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
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

        [HttpPost]
        public ActionResult BatchInBoundOrderDetailDel()
        {
            string[] Id = Request.Form.GetValues("idarr");

            string result = "";
            for (int i = 0; i < Id.Length; i++)
            {
                result = cf.InBoundOrderDetailDel(Convert.ToInt32(Id[i]), Session["userName"].ToString());
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        [HttpPost]
        public ActionResult BatchInBoundOrderDetailEdit()
        {
            string[] Id = Request.Form.GetValues("check_to");
            string[] Qty = Request.Form.GetValues("edit_qty");
            string[] Weight = Request.Form.GetValues("edit_weight");
            string[] CBM = Request.Form.GetValues("edit_cbm");

            string result = "";
            for (int i = 0; i < Id.Length; i++)
            {
                WCF.InBoundService.InBoundOrderDetail entity = new WCF.InBoundService.InBoundOrderDetail();
                entity.Id = Convert.ToInt32(Id[i]);
                entity.Qty = Convert.ToInt32(Qty[i]);

                if (string.IsNullOrEmpty(Weight[i]))
                {
                    entity.Weight = 0;
                }
                else
                {
                    entity.Weight = Convert.ToDecimal(Weight[i]);
                }
                if (string.IsNullOrEmpty(CBM[i]))
                {
                    entity.CBM = 0;
                }
                else
                {
                    entity.CBM = Convert.ToDecimal(CBM[i]);
                }

                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;
                result = cf.InBoundOrderDetailEdit(entity, new string[] { "Qty", "Weight", "CBM", "UpdateUser", "UpdateDate" });
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量保存成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //通过输入的款号 属性 得到单位列表
        [HttpPost]
        public ActionResult GetUnitSelList()
        {
            WCF.InBoundService.UnitsSearch entity = new WCF.InBoundService.UnitsSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.AltItemNumber = Request["alt_item_number"].Trim();
            entity.Style1 = Request["style1"].Trim();
            entity.Style2 = Request["style2"].Trim();
            entity.Style3 = Request["style3"].Trim();

            IEnumerable<WCF.InBoundService.UnitsResult> list = cf.GetUnitSelList(entity);
            if (list != null)
            {
                var sql = from r in cf.GetUnitSelList(entity)
                          select new
                          {
                              Value = r.Id.ToString(),
                              Text = r.UnitName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //验证客户流程
        [HttpPost]
        public ActionResult CheckImportsInBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("SO");
            string[] poNumber = Request.Form.GetValues("PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] style1 = Request.Form.GetValues("属性1");
            string[] style2 = Request.Form.GetValues("属性2");
            string[] style3 = Request.Form.GetValues("属性3");
            string[] qty = Request.Form.GetValues("数量");

            List<WCF.InBoundService.WhClient> list = new List<WCF.InBoundService.WhClient>();
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (list.Where(u => u.ClientCode == clientCode[i]).Count() == 0)
                {
                    WCF.InBoundService.WhClient client = new WCF.InBoundService.WhClient();
                    client.ClientCode = clientCode[i].Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    list.Add(client);
                }
            }

            List<WCF.InBoundService.FlowHeadResult> list1 = cf.CheckClientCodeRule(Session["whCode"].ToString(), list.ToArray()).ToList();

            if (list1 != null)
            {
                var sql = from r in list1
                          select new
                          {
                              Value = r.Id.ToString(),
                              Text = r.FlowName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }

        }

        [HttpPost]
        public ActionResult ImportsInBoundOrderSOPO()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("SO");
            string[] poNumber = Request.Form.GetValues("PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] style1 = Request.Form.GetValues("属性1");
            string[] style2 = Request.Form.GetValues("属性2");
            string[] style3 = Request.Form.GetValues("属性3");
            string[] qty = Request.Form.GetValues("数量");
            string[] weight = Request.Form.GetValues("重量");
            string[] cbm = Request.Form.GetValues("立方");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 300)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过300条！", null, "");
            }

            if (clientCode.Count() != soNumber.Count() || soNumber.Count() != poNumber.Count() || altItemNumber.Count() != poNumber.Count() || qty.Count() != clientCode.Count() || style1.Count() != clientCode.Count() || style2.Count() != clientCode.Count() || style3.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add 
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.InBoundService.InBoundOrderInsert> entityList = new List<WCF.InBoundService.InBoundOrderInsert>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (entityList.Where(u => u.SoNumber == soNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.ClientCode == clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).Count() == 0)
                {
                    WCF.InBoundService.InBoundOrderInsert entity = new WCF.InBoundService.InBoundOrderInsert();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ClientCode = clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    entity.SoNumber = soNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    entity.OrderType = "CFS";
                    entity.ProcessId = Convert.ToInt32(Request["processId"]);
                    entity.ProcessName = Request["processName"];

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

                    WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                    orderDetail.JsonId = i;
                    if (string.IsNullOrEmpty(poNumber[i].Trim().ToString()) == true)
                    {
                        orderDetail.CustomerPoNumber = entity.SoNumber;
                    }
                    else
                    {
                        orderDetail.CustomerPoNumber = poNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    if (string.IsNullOrEmpty(altItemNumber[i].Trim().ToString()) == true)
                    {
                        orderDetail.AltItemNumber = orderDetail.CustomerPoNumber;
                    }
                    else
                    {
                        orderDetail.AltItemNumber = altItemNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    orderDetail.Style1 = style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Style2 = style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Style3 = style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                    if (string.IsNullOrEmpty(weight[i].Trim().ToString()) == true)
                    {
                        orderDetail.Weight = 0;
                    }
                    else
                    {
                        orderDetail.Weight = Convert.ToDecimal((weight[i].Trim().ToString() ?? "0"));
                    }

                    if (string.IsNullOrEmpty(cbm[i].Trim().ToString()) == true)
                    {
                        orderDetail.CBM = 0;
                    }
                    else
                    {
                        orderDetail.CBM = Convert.ToDecimal((cbm[i].Trim().ToString() ?? "0"));
                    }

                    orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                    orderDetail.CreateUser = Session["userName"].ToString();
                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                    entityList.Add(entity);
                }
                else
                {
                    WCF.InBoundService.InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == soNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.ClientCode == clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).First();
                    entityList.Remove(oldentity);

                    WCF.InBoundService.InBoundOrderInsert newentity = oldentity;

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                    if (orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == altItemNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style2 == style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style3 == style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).Count() == 0)
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                        orderDetail.JsonId = i;
                        if (string.IsNullOrEmpty(poNumber[i].Trim().ToString()) == true)
                        {
                            orderDetail.CustomerPoNumber = newentity.SoNumber;
                        }
                        else
                        {
                            orderDetail.CustomerPoNumber = poNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        }
                        if (string.IsNullOrEmpty(altItemNumber[i].Trim().ToString()) == true)
                        {
                            orderDetail.AltItemNumber = orderDetail.CustomerPoNumber;
                        }
                        else
                        {
                            orderDetail.AltItemNumber = altItemNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        }
                        orderDetail.Style1 = style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Style2 = style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Style3 = style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                        if (string.IsNullOrEmpty(weight[i].Trim().ToString()) == true)
                        {
                            orderDetail.Weight = 0;
                        }
                        else
                        {
                            orderDetail.Weight = Convert.ToDecimal((weight[i].Trim().ToString() ?? "0"));
                        }

                        if (string.IsNullOrEmpty(cbm[i].Trim().ToString()) == true)
                        {
                            orderDetail.CBM = 0;
                        }
                        else
                        {
                            orderDetail.CBM = Convert.ToDecimal((cbm[i].Trim().ToString() ?? "0"));
                        }

                        orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetail.CreateUser = Session["userName"].ToString();
                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == altItemNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style2 == style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style3 == style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).First();

                        orderDetailList.Remove(oldorderDetail);

                        WCF.InBoundService.InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                    entityList.Add(newentity);
                }
            }

            string result = cf.ImportsInBoundOrder(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpPost]
        public ActionResult ImportsInBoundOrderPO()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] poNumber = Request.Form.GetValues("PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] style1 = Request.Form.GetValues("属性1");
            string[] style2 = Request.Form.GetValues("属性2");
            string[] style3 = Request.Form.GetValues("属性3");
            string[] qty = Request.Form.GetValues("数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 300)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过300条！", null, "");
            }

            if (clientCode.Count() != poNumber.Count() || altItemNumber.Count() != poNumber.Count() || qty.Count() != clientCode.Count() || style1.Count() != clientCode.Count() || style2.Count() != clientCode.Count() || style3.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.InBoundService.InBoundOrderInsert> entityList = new List<WCF.InBoundService.InBoundOrderInsert>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (entityList.Where(u => u.ClientCode == clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).Count() == 0)
                {
                    WCF.InBoundService.InBoundOrderInsert entity = new WCF.InBoundService.InBoundOrderInsert();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ClientCode = clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    entity.OrderType = "DC";
                    entity.ProcessId = Convert.ToInt32(Request["processId"]);
                    entity.ProcessName = Request["processName"];

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

                    WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                    orderDetail.JsonId = i;
                    orderDetail.CustomerPoNumber = poNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.AltItemNumber = altItemNumber[i].Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Style1 = style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Style2 = style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Style3 = style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                    orderDetail.CreateUser = Session["userName"].ToString();
                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                    entityList.Add(entity);
                }
                else
                {
                    WCF.InBoundService.InBoundOrderInsert oldentity = entityList.Where(u => u.ClientCode == clientCode[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).First();
                    entityList.Remove(oldentity);

                    WCF.InBoundService.InBoundOrderInsert newentity = oldentity;

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                    if (orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == altItemNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style2 == style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style3 == style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).Count() == 0)
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                        orderDetail.JsonId = i;
                        orderDetail.CustomerPoNumber = poNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.AltItemNumber = altItemNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Style1 = style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Style2 = style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Style3 = style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetail.CreateUser = Session["userName"].ToString();
                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == altItemNumber[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == style1[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style2 == style2[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style3 == style3[i].Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).First();

                        orderDetailList.Remove(oldorderDetail);

                        WCF.InBoundService.InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                    entityList.Add(newentity);
                }
            }

            string result = cf.ImportsInBoundOrder(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //删除预录入
        public ActionResult DeleteInBound()
        {
            string whCode = Session["whCode"].ToString();
            string clientCode = Request["clientCode"];
            string customerPoNumber = Request["customerPoNumber"];
            string soNumber = Request["soNumber"];

            string result = cf.DeleteInBound(whCode, clientCode, customerPoNumber, soNumber);
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
