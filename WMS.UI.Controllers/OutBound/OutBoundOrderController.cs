using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class OutBoundOrderController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();
        WCF.RootService.RootServiceClient rootcf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            //ViewData["OutFlowHeadSelect"] = from r in cf.OutFlowHeadListSelect(Session["whCode"].ToString())
            //                                select new SelectListItem()
            //                                {
            //                                    Text = r.FlowName,     //text
            //                                    Value = r.Id.ToString()
            //                                };
            ViewData["FlowRuleStatusSelect"] = from r in cf.FlowRuleStatusListSelect()
                                               select new SelectListItem()
                                               {
                                                   Text = r.Description,     //text
                                                   Value = r.ColumnKey.ToString()
                                               };

            ViewData["OutOrderSourceList"] = from r in rootcf.OutBoundOrderSourceSelect(Session["whCode"].ToString())
                                             select new SelectListItem()
                                             {
                                                 Text = r.OrderSource,     //text
                                                 Value = r.OrderSource
                                             };

            return View();
        }

        [DefaultRequest]
        public ActionResult AddIndex()
        {
            ViewData["outBoundOrderId"] = Request["outBoundOrderId"];
            ViewData["customerOutPoNumber"] = Request["customerOutPoNumber"].Trim();
            ViewData["clientId"] = Request["clientId"];
            ViewData["clientCode"] = Request["clientCode"];
            ViewData["outPoNumber"] = Request["outPoNumber"].Trim();
            return View();
        }

        //通过选择的客户得到出货流程
        [HttpPost]
        public ActionResult GetFlowNameList()
        {
            IEnumerable<WCF.OutBoundService.FlowHead> list = cf.OutFlowHeadListByClientId(Session["whCode"].ToString(), Convert.ToInt32(Request["txt_WhClient"]));
            if (list != null)
            {
                var sql = from r in list
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

        [HttpGet]
        public ActionResult List()
        {
            WCF.OutBoundService.OutBoundOrderSearch entity = new WCF.OutBoundService.OutBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            if (Request["WhClientId"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
            else
            {
                entity.ClientId = 0;
            }
            if (Request["FlowRuleStatus"] != "")
            {
                entity.StatusId = Convert.ToInt32(Request["FlowRuleStatus"]);
            }
            else
            {
                entity.StatusId = 0;
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
            entity.AltCustomerOutPoNumber = Request["AltCustomerOutPoNumber"].Trim();
            entity.CustomerOutPoNumber = Request["CustomerOutPoNumber"].Trim();
            entity.OutPoNumber = Request["OutPoNumber"].Trim();
            entity.OrderSource = Request["txt_orderSource"].Trim();

            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderResult> list = cf.OutBoundOrderList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "ClientId");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("OutPoNumber", "系统出库单号");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("AltCustomerOutPoNumber", "平台单号");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("FlowName", "出库订单流程");
            fieldsName.Add("ProcessId", "ProcessId");
            fieldsName.Add("NowProcessId", "NowProcessId");
            fieldsName.Add("NowProcessName", "当前所属步骤");
            fieldsName.Add("StatusId", "StatusId");
            fieldsName.Add("DSShow", "订单类型");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("OrderSource", "订单来源");
            fieldsName.Add("PlanOutTime", "计划出库时间");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("RollbackFlag", "RollbackFlag");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,DSShow:65,OutPoNumber:130,PlanOutTime:130,CreateDate:130,LoadId:130,default:100"));
        }

        //出库订单添加
        [HttpGet]
        public ActionResult OutBoundOrderAdd()
        {
            WCF.OutBoundService.OutBoundOrder entity = new WCF.OutBoundService.OutBoundOrder();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["txt_WhClient"]);
            entity.ClientCode = Request["clientCode"].Trim();
            entity.CustomerOutPoNumber = Request["txt_CustomerOutPoNumber"].Trim();
            entity.ProcessId = Convert.ToInt32(Request["txt_OutFlowName"]);
            entity.OrderSource = "WMS录入";
            entity.LoadFlag = 0;
            entity.PlanOutTime = Convert.ToDateTime(Request["txt_PlanOutTime"]);
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            WCF.OutBoundService.OutBoundOrder result = cf.OutBoundOrderAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "出库订单创建成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，该订单号创建失败或已存在！", null, "");
            }
        }

        //出库订单删除
        [HttpGet]
        public ActionResult OutBoundOrderDel()
        {
            string result = cf.OutBoundOrderDel(Convert.ToInt32(Request["Id"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //确认出库订单
        [HttpGet]
        public ActionResult ConfirmOutBoundOrder()
        {
            WCF.OutBoundService.OutBoundOrder entity = new WCF.OutBoundService.OutBoundOrder();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.WhCode = Session["whCode"].ToString();
            string result = cf.ConfirmOutBoundOrder(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //回滚出库订单
        [HttpGet]
        public ActionResult RollbackOutBoundOrder()
        {
            WCF.OutBoundService.OutBoundOrder entity = new WCF.OutBoundService.OutBoundOrder();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.WhCode = Session["whCode"].ToString();
            string result = cf.RollbackOutBoundOrder(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "回滚成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }



        //验证数组款号 属性 对应的单位是否有误
        [HttpPost]
        public void CheckUnitName()
        {
            string[] Id = Request.Form.GetValues("Id");
            string[] so_number = Request.Form.GetValues("so_number");
            string[] customer_po = Request.Form.GetValues("customer_po");
            string[] alt_item_number = Request.Form.GetValues("alt_item_number");
            string[] style1 = Request.Form.GetValues("style1");
            string[] style2 = Request.Form.GetValues("style2");
            string[] style3 = Request.Form.GetValues("style3");
            string[] lot1 = Request.Form.GetValues("lot1");
            string[] lot2 = Request.Form.GetValues("lot2");
            string[] lotDate = Request.Form.GetValues("lotDate");
            string[] unitId = Request.Form.GetValues("unitId");
            string[] unitName = Request.Form.GetValues("unitName");
            string[] qty = Request.Form.GetValues("qty");

            if (Id == null)
            {
                Response.Write("数组有误无法保存！");
                return;
            }

            WCF.OutBoundService.OutBoundOrderDetailInsert entity = new WCF.OutBoundService.OutBoundOrderDetailInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.OutBoundOrderId = Convert.ToInt32(Request["outBoundOrderId"]);
            entity.ClientId = Convert.ToInt32(Request["ClientId"]);
            entity.DSFlag = 0;
            List<WCF.OutBoundService.OutBoundOrderDetailModel> orderDetailList = new List<WCF.OutBoundService.OutBoundOrderDetailModel>();

            for (int i = 0; i < Id.Length; i++)
            {
                WCF.OutBoundService.OutBoundOrderDetailModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailModel();
                orderDetail.JsonId = Convert.ToInt32(Id[i].ToString());
                orderDetail.SoNumber = so_number[i].ToString().Trim();
                orderDetail.CustomerPoNumber = customer_po[i].ToString().Trim();
                orderDetail.AltItemNumber = alt_item_number[i].ToString().Trim();
                orderDetail.Style1 = style1[i].ToString().Trim();
                orderDetail.Style2 = style2[i].ToString().Trim();
                orderDetail.Style3 = style3[i].ToString().Trim();
                orderDetail.LotNumber1 = lot1[i].ToString().Trim();
                orderDetail.LotNumber2 = lot2[i].ToString().Trim();
                if (lotDate[i].ToString().Trim() == "")
                {
                    orderDetail.LotDate = null;
                }
                else
                {
                    orderDetail.LotDate = Convert.ToDateTime(lotDate[i].ToString());
                }
                if (unitId[i].ToString() == "")
                {
                    orderDetail.UnitId = 0;
                }
                else
                {
                    orderDetail.UnitId = Convert.ToInt32(unitId[i].ToString());
                }

                orderDetail.UnitName = unitName[i].ToString();
                orderDetail.Qty = Convert.ToInt32(qty[i].ToString());
                orderDetail.CreateUser = Session["userName"].ToString();
                orderDetailList.Add(orderDetail);
            }

            entity.OutBoundOrderDetailModel = orderDetailList.ToArray();
            string result = cf.OutBoundCheckUnitName(entity);
            if (result.Substring(0, 1) == "N")
            {
                Response.Write(result.Substring(1, result.Length - 1));
            }
            else
            {
                OutBoundOrderDetailAdd(entity);
            }
        }

        //添加预录入
        [HttpPost]
        public void OutBoundOrderDetailAdd(WCF.OutBoundService.OutBoundOrderDetailInsert entity)
        {
            string result = cf.OutBoundOrderDetailAdd(entity);
            if (result.Substring(0, 1) == "N")
            {
                Response.Write(result.Substring(1, result.Length - 1));
            }
            else
            {
                Response.Write("Y$" + result);
            }

        }

        //出库明细查询
        [HttpGet]
        public ActionResult OutBoundOrderDetailList()
        {
            WCF.OutBoundService.OutBoundOrderDetailSearch entity = new WCF.OutBoundService.OutBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.OutBoundOrderId = Convert.ToInt32(Request["outBoundOrderId"]);

            string itemNumber = Request["sku_area"];
            string[] itemNumberList = null;
            if (!string.IsNullOrEmpty(itemNumber))
            {
                string item_temp = itemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                itemNumberList = item_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderDetailResult> list = cf.OutBoundOrderDetailList(entity, itemNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "Lotdate");
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:80,CustomerPoNumber:80,AltItemNumber:80,LotDate:130,default:60"));
        }



        //出库订单明细修改
        [HttpGet]
        public ActionResult OutBoundOrderDetailEdit()
        {
            WCF.OutBoundService.OutBoundOrderDetail entity = new WCF.OutBoundService.OutBoundOrderDetail();
            entity.Id = Convert.ToInt32(Request["Id"]);
            if (Request["Sequence"] != "")
                entity.Sequence = Convert.ToInt32(Request["Sequence"]);
            else
                entity.Sequence = 0;
            entity.UpdateUser = Session["userName"].ToString();

            int result = 0;

            if (Request["Qty"] != "undefined")
            {
                entity.Qty = Convert.ToInt32(Request["Qty"]);
                result = cf.OutBoundOrderDetailEdit(entity, new string[] { "Qty", "Sequence", "UpdateUser", "UpdateDate" });
            }
            else
            {
                result = cf.OutBoundOrderDetailEdit(entity, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "明细修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //预录入明细删除
        [HttpGet]
        public ActionResult OutBoundOrderDetailDel()
        {
            int result = cf.OutBoundOrderDetailDel(Convert.ToInt32(Request["Id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，删除失败！", null, "");
            }
        }


        //格式化系统出库单号 显示订单明细
        [HttpGet]
        public ActionResult Second_OutBoundOrderDetailList()
        {
            WCF.OutBoundService.OutBoundOrderDetailSearch entity = new WCF.OutBoundService.OutBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.OutBoundOrderId = Convert.ToInt32(Request["OutBoundOrderId"]);

            entity.CustomerPoNumber = Request["se_customerPoNumber"];
            entity.AltItemNumber = Request["se_altCustomerPoNumber"];

            string[] itemNumberList = null;
            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderDetailResult> list = cf.OutBoundOrderDetailList(entity, itemNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("StowPosition", "装箱位置");  
            fieldsName.Add("LotDate", "Lotdate");
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:100,CustomerPoNumber:100,AltItemNumber:100,LotDate:110,default:60"));
        }




        //订单拦截
        [HttpGet]
        public ActionResult InterceptOrder()
        {
            string result = cf.OutBoundOrderIntercept(Session["whCode"].ToString(), Request["customerOutPoNumber"], Request["clientCode"], Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "订单拦截成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //验证客户流程
        [HttpPost]
        public ActionResult CheckImportsOutBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] customerOutPoNumber = Request.Form.GetValues("客户出库订单号");
            string[] soNumber = Request.Form.GetValues("入库SO");
            string[] poNumber = Request.Form.GetValues("入库PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] lotNumber1 = Request.Form.GetValues("LotNumber1");
            string[] lotNumber2 = Request.Form.GetValues("LotNumber2");
            string[] qty = Request.Form.GetValues("数量");

            List<WCF.OutBoundService.WhClient> list = new List<WCF.OutBoundService.WhClient>();
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (list.Where(u => u.ClientCode == clientCode[i]).Count() == 0)
                {
                    WCF.OutBoundService.WhClient client = new WCF.OutBoundService.WhClient();
                    client.ClientCode = clientCode[i];
                    list.Add(client);
                }
            }

            List<WCF.OutBoundService.FlowHeadResult> list1 = cf.CheckLoadContainerClientCodeRule(Session["whCode"].ToString(), list.ToArray()).ToList();

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
        public ActionResult ImportsOutBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] customerOutPoNumber = Request.Form.GetValues("客户出库订单号");
            string[] soNumber = Request.Form.GetValues("入库SO");
            string[] poNumber = Request.Form.GetValues("入库PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] lotNumber1 = Request.Form.GetValues("LotNumber1");
            string[] lotNumber2 = Request.Form.GetValues("LotNumber2");
            string[] qty = Request.Form.GetValues("数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != customerOutPoNumber.Count() || customerOutPoNumber.Count() != soNumber.Count() || soNumber.Count() != poNumber.Count() || poNumber.Count() != altItemNumber.Count() || altItemNumber.Count() != lotNumber1.Count() || lotNumber1.Count() != lotNumber2.Count() || lotNumber2.Count() != qty.Count() || qty.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + customerOutPoNumber[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + lotNumber1[i].ToString().Trim() + "-" + lotNumber2[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + customerOutPoNumber[i].ToString() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lotNumber1[i].ToString() + "-" + lotNumber2[i].ToString());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + customerOutPoNumber[i].ToString() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lotNumber1[i].ToString() + "-" + lotNumber2[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.OutBoundService.ImportOutBoundOrder> entityList = new List<WCF.OutBoundService.ImportOutBoundOrder>();
            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (entityList.Where(u => u.ClientCode == clientCode[i].Trim() && u.CustomerOutPoNumber == customerOutPoNumber[i].Trim()).Count() == 0)
                {
                    WCF.OutBoundService.ImportOutBoundOrder entity = new WCF.OutBoundService.ImportOutBoundOrder();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ProcessId= Convert.ToInt32(Request["processId"]);
                    entity.ProcessName= Request["processName"];
                    entity.ClientCode = clientCode[i].Trim();
                    entity.CustomerOutPoNumber = customerOutPoNumber[i].Trim();

                    List<WCF.OutBoundService.OutBoundOrderDetailModel> orderDetailList = new List<WCF.OutBoundService.OutBoundOrderDetailModel>();

                    WCF.OutBoundService.OutBoundOrderDetailModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailModel();
                    orderDetail.JsonId = i;
                    orderDetail.SoNumber = soNumber[i].ToString().Trim();
                    orderDetail.CustomerPoNumber = poNumber[i].ToString().Trim();
                    orderDetail.AltItemNumber = altItemNumber[i].ToString().Trim();
                    orderDetail.Qty = Convert.ToInt32(qty[i].ToString());
                    orderDetail.LotNumber1 = lotNumber1[i].Trim();
                    orderDetail.LotNumber2 = lotNumber2[i].Trim();
                    orderDetail.CreateUser = Session["userName"].ToString();
                    orderDetailList.Add(orderDetail);

                    entity.OutBoundOrderDetailModel = orderDetailList.ToArray();
                    entityList.Add(entity);
                }
                else
                {
                    WCF.OutBoundService.ImportOutBoundOrder oldentity = entityList.Where(u => u.ClientCode == clientCode[i].Trim() && u.CustomerOutPoNumber == customerOutPoNumber[i].Trim()).First();
                    entityList.Remove(oldentity);

                    WCF.OutBoundService.ImportOutBoundOrder newentity = oldentity;

                    List<WCF.OutBoundService.OutBoundOrderDetailModel> orderDetailList = oldentity.OutBoundOrderDetailModel.ToList();

                    if (orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim() && u.AltItemNumber == altItemNumber[i].Trim() && u.SoNumber == soNumber[i].Trim() && u.LotNumber1 == lotNumber1[i].Trim() && u.LotNumber2 == lotNumber2[i].Trim()).Count() == 0)
                    {
                        WCF.OutBoundService.OutBoundOrderDetailModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailModel();
                        orderDetail.JsonId = i;
                        orderDetail.SoNumber = soNumber[i].ToString().Trim();
                        orderDetail.CustomerPoNumber = poNumber[i].ToString().Trim();
                        orderDetail.AltItemNumber = altItemNumber[i].ToString().Trim();
                        orderDetail.Qty = Convert.ToInt32(qty[i].ToString());
                        orderDetail.LotNumber1 = lotNumber1[i].Trim();
                        orderDetail.LotNumber2 = lotNumber2[i].Trim();
                        orderDetail.CreateUser = Session["userName"].ToString();
                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        WCF.OutBoundService.OutBoundOrderDetailModel oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim() && u.AltItemNumber == altItemNumber[i].Trim() && u.SoNumber == soNumber[i].Trim() && u.LotNumber1 == lotNumber1[i].Trim() && u.LotNumber2 == lotNumber2[i].Trim()).First();

                        orderDetailList.Remove(oldorderDetail);

                        WCF.OutBoundService.OutBoundOrderDetailModel neworderDetail = oldorderDetail;
                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(qty[i].ToString());
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.OutBoundOrderDetailModel = orderDetailList.ToArray();
                    entityList.Add(newentity);
                }
            }

            string result = cf.ImportsOutBoundOrder(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }




    }
}
