using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LoadController : Controller
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
            ViewData["OutFlowHeadSelect"] = from r in cf.OutFlowHeadListSelect(Session["whCode"].ToString())
                                            select new SelectListItem()
                                            {
                                                Text = r.FlowName,     //text
                                                Value = r.Id.ToString()
                                            };


            ViewData["LoadContainerTypeSelect"] = from r in cf.LoadContainerTypeSelect()
                                                  select new SelectListItem()
                                                  {
                                                      Text = r.ContainerName,     //text
                                                      Value = r.ContainerType
                                                  };

            ViewData["LoadCreateRuleSelect"] = from r in rootcf.LoadCreateRuleSelect(Session["whCode"].ToString())
                                               select new SelectListItem()
                                               {
                                                   Text = r.RuleName,     //text
                                                   Value = r.Id.ToString()
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
            ViewData["clientCode"] = Request["clientCode"].Trim();
            ViewData["outPoNumber"] = Request["outPoNumber"].Trim();
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.OutBoundService.LoadMasterSearch entity = new WCF.OutBoundService.LoadMasterSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.CustomerOutPoNumber = Request["CustomerOutPoNumber"].Trim();
            entity.LoadId = Request["load"].Trim();
            entity.ShipMode = Request["shipMode"];
            entity.Status0 = Request["Status0"];
            entity.Status1 = Request["Status1"];
            entity.Status3 = Request["Status3"];
            entity.ShipStatus = Request["ShipStatus"];
            entity.ContainerNumber = Request["containerNumber"].Trim();

            entity.ClientCode = Request["clientCode"];
            if (Request["BeginShipDate"] != "")
            {
                entity.BeginShipDate = Convert.ToDateTime(Request["BeginShipDate"]);
            }
            else
            {
                entity.BeginShipDate = null;
            }
            if (Request["EndShipDate"] != "")
            {
                entity.EndShipDate = Convert.ToDateTime(Request["EndShipDate"]).AddDays(1);
            }
            else
            {
                entity.EndShipDate = null;
            }
            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                return Helper.RedirectAjax("err", "创建开始时间必须选择！", null, "");
            }
            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                return Helper.RedirectAjax("err", "创建结束时间必须选择！", null, "");
            }

            int total = 0;
            List<WCF.OutBoundService.LoadMasterResult> list = cf.LoadMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Action1", "指定托盘");
            fieldsName.Add("Action2", "附加直装");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LoadId", "Load");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("SumQty", "出货数量");
            fieldsName.Add("DSSumQty", "直装数量");
            fieldsName.Add("EchQty", "挂衣数");
            fieldsName.Add("SumCBM", "总立方");
            fieldsName.Add("SumWeight", "总重量");
            fieldsName.Add("Remark", "出货备注");

            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("SealNumber", "封号");
            fieldsName.Add("ContainerType", "箱型");

            fieldsName.Add("ShipMode", "出库方式");
            fieldsName.Add("Status0", "释放状态");
            fieldsName.Add("Status1", "备货状态");
            fieldsName.Add("Status2", "分拣状态");
            fieldsName.Add("Status3", "装箱状态");
            fieldsName.Add("ShipStatus", "封箱状态");
            fieldsName.Add("ProcessId", "ProcessId");
            fieldsName.Add("ProcessName", "所选出货流程");

            fieldsName.Add("VesselName", "船名");
            fieldsName.Add("VesselNumber", "航次");
            fieldsName.Add("CarriageName", "船公司");
            fieldsName.Add("ETDShow", "ETD时间");
            fieldsName.Add("ContainerName", "箱型名称");
            fieldsName.Add("Port", "港区");
            fieldsName.Add("DeliveryPlace", "交货地");
            fieldsName.Add("BillNumber", "提单号");

            fieldsName.Add("ReleaseDate", "释放时间");
            fieldsName.Add("BeginPickDate", "开始备货时间");
            fieldsName.Add("EndPickDate", "结束备货时间");
            fieldsName.Add("BeginPackDate", "开始装箱时间");
            fieldsName.Add("EndPackDate", "结束装箱时间");
            fieldsName.Add("BeginSortDate", "开始分拣时间");
            fieldsName.Add("EndSortDate", "结束分拣时间");
            fieldsName.Add("ShipDate", "封箱时间");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:130,Action1:55,Action2:55,ClientCode:110,LoadId:140,CustomerOutPoNumber:120,ContainerName:100,SealNumber:80,ProcessName:130,VesselName:120,Port:170,BillNumber:120,ContainerNumber:120,ReleaseDate:130,BeginPickDate:130,EndPickDate:130,BeginPackDate:130,EndPackDate:130,BeginSortDate:130,EndSortDate:130,ShipDate:130,CreateDate:130,ETDShow:130,ContainerType:60,Remark:110,SumQty:60,DSSumQty:60,EchQty:50,SumCBM:50,SumWeight:50,default:70"));
        }

        //创建Load
        [HttpGet]
        public ActionResult LoadMasterAdd()
        {
            WCF.OutBoundService.LoadMaster entity = new WCF.OutBoundService.LoadMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ShipMode = Request["txt_shipMode"];
            entity.ProcessId = Convert.ToInt32(Request["txt_FlowName"].ToString());
            entity.ProcessName = Request["processName"].ToString();

            entity.Remark = Request["txt_remark"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            WCF.OutBoundService.LoadMaster result = cf.LoadMasterAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "Load创建成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，创建失败！", null, "");
            }
        }

        [HttpPost]
        //批量释放Load
        public ActionResult BatchReleaseLoad()
        {
            string[] outBoundOrderList = Request.Form.GetValues("loadArr");

            string result = "";
            foreach (var item in outBoundOrderList)
            {
                result = cf.CheckReleaseLoadGetType(item, Session["whCode"].ToString(), Session["userName"].ToString(), "");
                if (result != "Y")
                {
                    result = "Load号：" + item + result;
                    break;
                }
            }

            if (result != "Y")
            {
                return Helper.RedirectAjax("err", "Load释放异常！" + result, null, "");
            }
            else
            {
                return Helper.RedirectAjax("ok", "批量释放成功！", null, "");
            }

        }

        //查询客户出库订单信息
        [HttpGet]
        public ActionResult OutBoundOrderList()
        {
            WCF.OutBoundService.OutBoundOrderSearch entity = new WCF.OutBoundService.OutBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadMasterId = Convert.ToInt32(Request["loadMasterId"]);

            if (Request["txt_detail_clientCode"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["txt_detail_clientCode"]);
            }
            else
            {
                entity.ClientId = 0;
            }

            if (Request["Hid_processId"] != "")
            {
                entity.ProcessId = Convert.ToInt32(Request["Hid_processId"]);
            }
            else
            {
                entity.ProcessId = 0;
            }
            string customerPoNumber = Request["customerOutPo_area"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(customerPoNumber))
            {
                string po_temp = customerPoNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderResult> list = cf.Load_OutBoundOrderList(entity, poNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("FlowName", "出库订单流程");
            fieldsName.Add("OrderSource", "订单来源");
            fieldsName.Add("PlanOutTime", "计划出库时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,ClientCode:120,default:150"));
        }


        //添加Load明细
        [HttpPost]
        public ActionResult LoadDetailAdd()
        {
            string[] outBoundOrderId = Request.Form.GetValues("outBoundOrderId");

            List<WCF.OutBoundService.LoadDetail> orderDetailList = new List<WCF.OutBoundService.LoadDetail>();

            for (int i = 0; i < outBoundOrderId.Length; i++)
            {
                WCF.OutBoundService.LoadDetail entity = new WCF.OutBoundService.LoadDetail();
                entity.LoadMasterId = Convert.ToInt32(Request["loadMasterId"]);
                entity.OutBoundOrderId = Convert.ToInt32(outBoundOrderId[i].ToString());
                entity.CreateUser = Session["userName"].ToString();
                entity.CreateDate = DateTime.Now;
                orderDetailList.Add(entity);
            }

            int result = cf.LoadDetailAdd(orderDetailList.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，创建失败！", null, "");
            }
        }

        //Load删除
        [HttpGet]
        public ActionResult LoadMasterDel()
        {
            string result = cf.LoadMasterDel(Convert.ToInt32(Request["Id"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //释放Load前验证装箱单所选数量与Load订单数量是否一致
        [HttpGet]
        public void CheckLoadContainerQtyToOutDetail()
        {
            string result = cf.CheckLoadContainerQtyToOutDetail(Request["LoadId"], Session["whCode"].ToString());
            Response.Write(result);
        }

        //释放Load
        [HttpGet]
        public ActionResult ReleaseLoad()
        {
            string result = cf.CheckReleaseLoadGetType(Request["LoadId"], Session["whCode"].ToString(), Session["userName"].ToString(), "");
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "释放成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "Load释放异常！" + result, null, "");
            }
        }


        //撤销释放
        [HttpGet]
        public ActionResult RollbackLoad()
        {
            string result = cf.RollbackLoad(Request["Load"], Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "撤销释放成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //查看Load 明细
        [HttpGet]
        public ActionResult LoadSecond_OutBoundOrderList()
        {
            WCF.OutBoundService.OutBoundOrderSearch entity = new WCF.OutBoundService.OutBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.LoadMasterId = Convert.ToInt32(Request["LoadMasterId"]);

            entity.AltCustomerOutPoNumber = Request["loadSecond_altCustomerPoNumber"].Trim();
            entity.CustomerOutPoNumber = Request["loadSecond_customerPoNumber"].Trim();

            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderResult> list = cf.LoadSecond_OutBoundOrderList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("OutPoNumber", "系统出库单号");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("AltCustomerOutPoNumber", "平台单号");
            fieldsName.Add("FlowName", "出库订单流程");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("SumQty", "订单数量");

            fieldsName.Add("OutBoundOrderId", "OutBoundOrderId");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:50,ClientCode:120,StatusName:70,SumQty:60,default:150"));
        }


        //Load明细删除
        [HttpGet]
        public ActionResult LoadDetailDel()
        {
            string result = cf.LoadDetailDel(Convert.ToInt32(Request["Id"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //Load 维护箱封号
        [HttpGet]
        public ActionResult EditContainerNumber()
        {
            WCF.OutBoundService.LoadContainerExtend entity = new WCF.OutBoundService.LoadContainerExtend();
            entity.LoadId = Request["edit_LoadId"];
            entity.WhCode = Session["whCode"].ToString();

            entity.VesselName = Request["edit_VesselName"].Trim();
            entity.VesselNumber = Request["edit_VesselNumber"].Trim();
            entity.CarriageName = Request["edit_CarriageName"].Trim();
            entity.ETD = Convert.ToDateTime(Request["edit_ETD"]);
            entity.ContainerType = Request["edit_ContainerType"].Trim();
            entity.Port = Request["edit_Port"].Trim();
            entity.DeliveryPlace = Request["edit_DeliveryPlace"].Trim();
            entity.BillNumber = Request["edit_BillNumber"].Trim();
            entity.ContainerNumber = Request["edit_ContainerNumber"].Trim();
            entity.SealNumber = Request["edit_SealNumber"].Trim();
            entity.CreateUser = Session["userName"].ToString();

            string result = cf.LoadContainerExtendAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加/修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //Load 修改出货备注
        [HttpGet]
        public ActionResult EditLoadRemark()
        {
            WCF.OutBoundService.LoadMaster entity = new WCF.OutBoundService.LoadMaster();
            entity.Id = Convert.ToInt32(Request["edit_LoadMasterId"]);
            entity.Remark = Request["edit_remark"].Trim();

            string result = cf.LoadMasterEditRemark(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //导入出货托盘列表
        [HttpGet]
        public ActionResult LoadHuIdExtendSearch()
        {
            WCF.OutBoundService.LoadHuIdExtendSearch entity = new WCF.OutBoundService.LoadHuIdExtendSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["LoadId"].Trim();

            int total = 0;
            List<WCF.OutBoundService.LoadHuIdExtend> list = cf.LoadHuIdExtendSearch(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:40,default:130"));
        }

        //删除导入出货托盘 
        [HttpGet]
        public ActionResult LoadHuIdExtendDel()
        {
            int result = cf.LoadHuIdExtendDelete(Convert.ToInt32(Request["Id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //批量导入出货托盘
        public ActionResult LoadHuIdExtendImport()
        {
            string[] location = Request.Form.GetValues("托盘");
            string loadId = Request["loadId"].Trim();

            Hashtable hash = new Hashtable();
            string mess = "";

            if (location == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (location.Length > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            List<WCF.OutBoundService.LoadHuIdExtend> list = new List<WCF.OutBoundService.LoadHuIdExtend>();
            for (int i = 0; i < location.Length; i++)
            {
                if (!hash.ContainsValue(location[i].ToString()))
                {
                    hash.Add(i, location[i].ToString());
                    WCF.OutBoundService.LoadHuIdExtend entity = new WCF.OutBoundService.LoadHuIdExtend();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.LoadId = loadId;
                    entity.HuId = location[i].Trim().ToString();
                    entity.CreateUser = Session["userName"].ToString();

                    list.Add(entity);
                }
                else
                {
                    mess += "托盘重复：" + location[i].ToString() + "<br/>";
                }
            }

            if (mess != "")
            {
                return Helper.RedirectAjax("err", "导入失败！<br/>" + mess, null, "");
            }

            string result = cf.LoadHuIdExtendImport(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，导入失败！", null, "");
            }

        }

        //直装订单列表
        [HttpGet]
        public ActionResult DSOutBoundOrderList()
        {
            WCF.OutBoundService.OutBoundOrderSearch entity = new WCF.OutBoundService.OutBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.LoadMasterId = Convert.ToInt32(Request["LoadMasterId"]);

            int total = 0;
            List<WCF.OutBoundService.OutBoundOrderResult> list = cf.DSOutBoundOrderList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("OutPoNumber", "系统出库单号");
            fieldsName.Add("CustomerOutPoNumber", "客户出库单号");
            fieldsName.Add("FlowName", "出库订单流程");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("SumQty", "订单数量");
            fieldsName.Add("OutBoundOrderId", "OutBoundOrderId");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:50,SumQty:65,default:110"));
        }

        //直装订单 验证客户出货流程
        [HttpPost]
        public ActionResult CheckImportsOutBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");

            int loadPorcessId = Convert.ToInt32(Request["loadPorcessId"]);

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
                if (list1.Where(u => u.Id == loadPorcessId).Count() == 0)
                {
                    return null;
                }
                else
                {
                    var sql = from r in list1.Where(u => u.Id == loadPorcessId)
                              select new
                              {
                                  Value = r.Id.ToString(),
                                  Text = r.FlowName    //text
                              };
                    return Json(sql);
                }
            }
            else
            {
                return null;
            }
        }

        //直装订单导入
        [HttpPost]
        public ActionResult DSOutBoundImport()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("入库SO");
            string[] poNumber = Request.Form.GetValues("入库PO");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] style1 = Request.Form.GetValues("属性1");
            string[] style2 = Request.Form.GetValues("属性2");
            string[] style3 = Request.Form.GetValues("属性3");

            string[] qty = Request.Form.GetValues("数量");

            string loadId = Request["loadId"];

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != soNumber.Count() || soNumber.Count() != poNumber.Count() || poNumber.Count() != altItemNumber.Count() || altItemNumber.Count() != qty.Count() || qty.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<string> checkClientCode = clientCode.ToList();
            if (checkClientCode.Distinct().Count() > 1)
            {
                return Helper.RedirectAjax("err", "导入订单一票只能有一个客户！", null, "");
            }

            List<WCF.OutBoundService.ImportOutBoundOrder> entityList = new List<WCF.OutBoundService.ImportOutBoundOrder>();

            WCF.OutBoundService.ImportOutBoundOrder entity = new WCF.OutBoundService.ImportOutBoundOrder();
            entity.WhCode = Session["whCode"].ToString();
            entity.ProcessId = Convert.ToInt32(Request["processId"]);
            entity.ProcessName = Request["processName"];
            entity.ClientCode = clientCode[0].Trim();

            List<WCF.OutBoundService.OutBoundOrderDetailModel> orderDetailList = new List<WCF.OutBoundService.OutBoundOrderDetailModel>();
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim() && u.AltItemNumber == altItemNumber[i].Trim() && u.SoNumber == soNumber[i].Trim() && u.Style1 == style1[i].ToString().Trim() && u.Style2 == style2[i].ToString().Trim() && u.Style3 == style3[i].ToString().Trim()).Count() == 0)
                {
                    WCF.OutBoundService.OutBoundOrderDetailModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailModel();
                    orderDetail.JsonId = i;
                    orderDetail.SoNumber = soNumber[i].ToString().Trim();
                    orderDetail.CustomerPoNumber = poNumber[i].ToString().Trim();
                    orderDetail.AltItemNumber = altItemNumber[i].ToString().Trim();
                    orderDetail.Qty = Convert.ToInt32(qty[i].ToString());
                    orderDetail.Style1 = style1[i].ToString().Trim();
                    orderDetail.Style2 = style2[i].ToString().Trim();
                    orderDetail.Style3 = style3[i].ToString().Trim();
                    orderDetail.LotNumber1 = "";
                    orderDetail.LotNumber2 = "";
                    orderDetail.CreateUser = Session["userName"].ToString();
                    orderDetailList.Add(orderDetail);
                }
                else
                {
                    WCF.OutBoundService.OutBoundOrderDetailModel oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == poNumber[i].Trim() && u.AltItemNumber == altItemNumber[i].Trim() && u.SoNumber == soNumber[i].Trim() && u.Style1 == style1[i].ToString().Trim() && u.Style2 == style2[i].ToString().Trim() && u.Style3 == style3[i].ToString().Trim()).First();

                    orderDetailList.Remove(oldorderDetail);

                    WCF.OutBoundService.OutBoundOrderDetailModel neworderDetail = oldorderDetail;
                    neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(qty[i].ToString());
                    orderDetailList.Add(neworderDetail);
                }
            }

            entity.OutBoundOrderDetailModel = orderDetailList.ToArray();
            entityList.Add(entity);

            string result = cf.DSOutBoundImport(entityList.ToArray(), loadId);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //直装订单删除
        [HttpGet]
        public ActionResult DSLoadDetailDel()
        {
            string result = cf.DSLoadDetailDel(Convert.ToInt32(Request["Id"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //打印出货操作单
        [HttpGet]
        public void PrintLoadId()
        {
            string result = cf.PrintOutTempalte(Session["whCode"].ToString(), Request["LoadId"]);
            Response.Write(result);
        }

        [HttpGet]
        [Check]
        public ActionResult WhclientReleaseRuleCheck()
        {
            string WhCode = Session["whCode"].ToString();
            if (!string.IsNullOrEmpty(WhCode))
            {
                return Helper.RedirectAjax("ok", "可访问！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，没有权限访问该功能！", null, "");
            }
        }

        //客户释放规则列表

        [HttpGet]
        public ActionResult WhclientReleaseRuleList()
        {
            WCF.RootService.WhClientSearch entity = new WCF.RootService.WhClientSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.ClientCode = Request["clientCode"];
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;
            List<WCF.RootService.WhClientResult> list = rootcf.WhClientReleaseRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ReleaseRule", "ReleaseRule");
            fieldsName.Add("ReleaseRuleShow", "释放规则");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:100,default:140"));
        }


        //修改客户释放规则

        [HttpGet]
        public ActionResult WhclientReleaseRuleEdit()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.ReleaseRule = Convert.ToInt32(Request["edit_ReleaseRule"]);

            int result = rootcf.WhClientEdit(entity, new string[] { "ReleaseRule" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "客户释放规则修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }



        //查看Load 明细
        [HttpGet]
        public ActionResult showReleaseLoadDetailList()
        {
            WCF.OutBoundService.LoadHuIdExtendSearch entity = new WCF.OutBoundService.LoadHuIdExtendSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.LoadId = Request["LoadId"];
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;
            List<WCF.OutBoundService.ReleaseLoadDetail> list = cf.ReleaseLoadDetailList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ReleaseMark", "释放条件");
            fieldsName.Add("Description", "异常说明");
            fieldsName.Add("Qty", "有效库存数");
            fieldsName.Add("OutQty", "出货数量");
            fieldsName.Add("OutAltItemNumber", "出货SKU");
            fieldsName.Add("OutItemId", "ItemId");
            fieldsName.Add("OutUnitName", "出货单位");
            fieldsName.Add("OutSoNumber", "出货SO");
            fieldsName.Add("OutCustomerPoNumber", "出货PO");
            fieldsName.Add("OutLotNumber1", "出货Lot1");
            fieldsName.Add("OutLotNumber2", "出货Lot2");
            fieldsName.Add("OutLotDate", "出货LotDate");
            fieldsName.Add("ClientCode", "客户名");

            return Content(EIP.EipListJson(list, total, fieldsName, "ReleaseMark:130,Description:240,Qty:72,OutUnitName:70,Id:50,OutQty:62,default:110"));
        }

    }
}
