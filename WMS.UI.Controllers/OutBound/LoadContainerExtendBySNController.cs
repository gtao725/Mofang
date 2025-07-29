using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LoadContainerExtendBySNController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["CreateUserName"] = from r in cf.CreateUserSelect(Session["whCode"].ToString())
                                         select new SelectListItem()
                                         {
                                             Text = r.CreateUserName,     //text
                                             Value = r.CreateUser
                                         };

            ViewData["LoadContainerTypeSelect"] = from r in cf.LoadContainerTypeSelect()
                                                  select new SelectListItem()
                                                  {
                                                      Text = r.ContainerName,     //text
                                                      Value = r.ContainerType
                                                  };
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };


            ViewData["DaiDianSelect"] = from r in cf.DaiDianSelectList()
                                        select new SelectListItem()
                                        {
                                            Text = r.DaiDianName,     //text
                                            Value = r.Id.ToString()
                                        };

            ViewData["LoadContainerCodeTypeSelect"] = from r in cf.LoadContainerTypeSelect()
                                                      select new SelectListItem()
                                                      {
                                                          Text = r.ContainerName,     //text
                                                          Value = r.ContainerCodeType
                                                      };

            ViewData["LoadChargeRuleSelect"] = from r in cf.LoadChargeRuleSelectList()
                                               select new SelectListItem()
                                               {
                                                   Text = r.FunctionName,     //text
                                                   Value = r.Id.ToString()
                                               };

            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.LoadId = Request["load"].Trim();
            entity.ClientCode = Request["WhClientCode"].Trim();
            entity.BillNumber = Request["billNumber"].Trim().Replace(" ", "");
            entity.SealNumber = Request["sealNumber"].Trim().Replace(" ", "");

            entity.CreateUser = Request["createUser"].Trim();
            entity.ChuCangFS = Request["shipMode"].Trim();

            string poNumber = Request["containerNumber"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            if (Request["BeginETD"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["BeginETD"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["EndETD"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["EndETD"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
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
            entity.Status0 = Request["Status0"];
            entity.Status1 = Request["Status1"];
            entity.Status3 = Request["Status3"];
            entity.ShipStatus = Request["ShipStatus"];

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerResult> list = cf.LoadContainerList(entity, poNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Action1", "出库SN调整");
            fieldsName.Add("LoadMasterId", "loadMasterId");
            fieldsName.Add("LoadId", "Load");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("Status0", "释放状态");
            fieldsName.Add("Status1", "备货状态");
            fieldsName.Add("Status3", "装箱状态");
            fieldsName.Add("ShipStatus", "封箱状态");

            fieldsName.Add("SumQty", "释放数量");
            fieldsName.Add("SumCBM", "释放立方");
            fieldsName.Add("PlanQty", "计划总数量");
            fieldsName.Add("ShippingOrigin", "发货地");
            fieldsName.Add("Remark", "出货备注");

            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("SealNumber", "封号");

            fieldsName.Add("ProcessId", "ProcessId");
            fieldsName.Add("ProcessName", "所选出货流程");

            fieldsName.Add("ReleaseDate", "释放时间");
            fieldsName.Add("BeginPickDate", "开始备货时间");
            fieldsName.Add("EndPickDate", "结束备货时间");
            fieldsName.Add("BeginPackDate", "开始装箱时间");
            fieldsName.Add("EndPackDate", "结束装箱时间");
            fieldsName.Add("ShipDate", "封箱时间");

            fieldsName.Add("ContainerSource", "箱单来源");

            fieldsName.Add("CreateUserName", "创建者");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("LoadContainerHuDetailId", "已选库存");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:156,Action1:75,LoadId:140,ClientCode:110,ContainerName:120,ProcessName:130,VesselName:120,Port:170,BillNumber:110,ContainerNumber:90,SealNumber:75,CreateDate:130,ETD:130,ContainerType:60,Status0:60,Status1:60,Status3:60,ShipStatus:60,SumQty:60,DSSumQty:60,EchQty:50,SumCBM:50,SumWeight:50,ChuCangFS:70,WeightFlag:70,ReleaseDate:130,BeginPickDate:130,EndPickDate:130,BeginPackDate:130,EndPackDate:130,ShipDate:130,default:90", null, "", 50, str));
        }

        [HttpGet]
        public ActionResult LoadToOutBoundOrderList()
        {
            WCF.OutBoundService.OutBoundOrderSearch entity = new WCF.OutBoundService.OutBoundOrderSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.LoadMasterId = Convert.ToInt32(Request["LoadMasterId"]);

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.OutBoundOrderDetailResult> list = cf.LoadToOutBoundOrderList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("TotalCbm", "实装立方");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            return Content(EIP.EipListJson(list, total, fieldsName, "ClientCode:100,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,Sequence:40,default:60", null, "", 200, str));
        }

        //创建Load
        [HttpGet]
        public ActionResult LoadContainerAdd()
        {
            WCF.OutBoundService.LoadContainerExtend entity = new WCF.OutBoundService.LoadContainerExtend();
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = "";
            entity.VesselName = Request["add_VesselName"].ToString().Trim();
            entity.VesselNumber = Request["add_VesselNumber"].ToString().Trim();
            entity.CarriageName = Request["add_CarriageName"].Trim();
            if (Request["add_ETD"] != "")
            {
                entity.ETD = Convert.ToDateTime(Request["add_ETD"]);
            }
            entity.ChuCangFS = Request["txt_shipMode"].Trim();
            entity.ContainerType = Request["add_ContainerType"].Trim();
            entity.Port = Request["add_Port"].Trim();
            entity.DeliveryPlace = Request["add_DeliveryPlace"].Trim();
            entity.BillNumber = Request["add_BillNumber"].Trim();
            entity.ContainerNumber = Request["add_ContainerNumber"].Trim().Replace(" ", "");
            entity.SealNumber = Request["add_SealNumber"].Trim().Replace(" ", "");
            entity.CreateUser = Session["userName"].ToString();
            entity.WeightFlag = Convert.ToInt32(Request["add_WeightFlag"].Trim());
            entity.ContainerSource = "WMS";

            WCF.OutBoundService.LoadContainerExtend result = cf.LoadContainerAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "创建成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，创建失败！", null, "");
            }
        }

        //Load 维护箱封号
        [HttpGet]
        public ActionResult EditContainerNumber()
        {
            WCF.OutBoundService.LoadContainerExtend entity = new WCF.OutBoundService.LoadContainerExtend();
            entity.Id = Convert.ToInt32(Request["id"]);

            entity.VesselName = Request["edit_VesselName"].Trim();
            entity.VesselNumber = Request["edit_VesselNumber"].Trim();
            entity.CarriageName = Request["edit_CarriageName"].Trim();

            if (Request["edit_ETD"] != "")
            {
                entity.ETD = Convert.ToDateTime(Request["edit_ETD"]);
            }
            entity.ContainerType = Request["edit_ContainerType"].Trim();
            entity.Port = Request["edit_Port"].Trim();
            entity.DeliveryPlace = Request["edit_DeliveryPlace"].Trim();
            entity.BillNumber = Request["edit_BillNumber"].Trim();
            entity.ContainerNumber = Request["edit_ContainerNumber"].Trim().Replace(" ", "");
            entity.SealNumber = Request["edit_SealNumber"].Trim().Replace(" ", "");
            entity.CreateUser = Session["userName"].ToString();

            string result = cf.LoadContainerEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除箱单
        [HttpGet]
        public ActionResult LoadContainerDelById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            string result = cf.LoadContainerDelete(id);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //装箱单查询库存 按SO查询
        [HttpPost]
        public ActionResult SelectInventory()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_clientCode"].Trim();
            entity.SoNumber = Request["so_like"].Trim();

            string so_area = Request["so_area"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult> list = cf.LoadContainerHuDetailList(entity, soNumberList, out total,out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("AcQty", "可用库存");
            fieldsName.Add("SelectQty", "已选数量");
            fieldsName.Add("PlanQty", "锁定数量");
            fieldsName.Add("Date", "库存天数");
            fieldsName.Add("ContainerNumber", "被选箱号");

            return Content(EIP.EipListJson(list, total, fieldsName, "ClientCode:110,SoNumber:130,ContainerNumber:150,default:70", null, "", 50, str));
        }


        //装箱单查询库存 按SOPOSKU查询
        [HttpGet]
        public ActionResult SelectInventoryByPOSKU()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_clientCode"].Trim();
            entity.SoNumber = Request["byposku_so_number"].Trim();
            entity.PoNumber = Request["byposku_po_number"].Trim();
            entity.AltNumber = Request["byposku_alt_number"].Trim();


            string so_area = Request["so_area"];
            string[] soNumberList = null;
            List<string> List1 = new List<string>();
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in soNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List1.Add(item);
                    }
                }
            }

            string po_area = Request["po_area"];
            string[] poNumberList = null;
            List<string> List2 = new List<string>();
            if (!string.IsNullOrEmpty(po_area))
            {
                string so_temp = po_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in poNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List2.Add(item);
                    }
                }
            }

            string sku_area = Request["sku_area"];
            string[] skuNumberList = null;
            List<string> List3 = new List<string>();
            if (!string.IsNullOrEmpty(sku_area))
            {
                string so_temp = sku_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                skuNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in skuNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List3.Add(item);
                    }
                }
            }

            string style_area = Request["style1_area"];
            string[] style1List = null;
            List<string> List4 = new List<string>();
            if (!string.IsNullOrEmpty(style_area))
            {
                string so_temp = style_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                style1List = so_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in style1List)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List4.Add(item);
                    }
                }
            }

            string style2_area = Request["style2_area"];
            string[] style2List = null;
            List<string> List5 = new List<string>();
            if (!string.IsNullOrEmpty(style2_area))
            {
                string so_temp = style2_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                style2List = so_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in style2List)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List5.Add(item);
                    }
                }
            }

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult> list = cf.LoadContainerHuDetailListByPOSKU(entity, List1.ToArray(), List2.ToArray(), List3.ToArray(), List4.ToArray(), List5.ToArray(), out total,out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("ItemId", "款号ID");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("AcQty", "可用库存");
            fieldsName.Add("SelectQty", "已选数量");
            fieldsName.Add("PlanQty", "锁定数量");
            fieldsName.Add("Date", "库存天数");
            fieldsName.Add("ContainerNumber", "被选箱号");

            return Content(EIP.EipListJson(list, total, fieldsName, "ClientCode:110,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ContainerNumber:150,default:70", null, "", 50, str));
        }

        //装箱单选择库存后 验证出货流程是否只有一个
        [HttpPost]
        public void LoadContainerCheckProcessName()
        {
            string[] clientCode = Request.Form.GetValues("clientCode");

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
                if (sql.Count() == 1)
                {
                    Response.Write("Y$" + sql.First().Value + "$" + sql.First().Text);
                }
                else
                {
                    Response.Write("N");
                }
            }
            else
            {
                Response.Write("N");
            }
        }

        //装箱单选择库存后 生成明细
        [HttpPost]
        public ActionResult LoadContainerHuDetailCheck()
        {
            string[] clientCode = Request.Form.GetValues("clientCode");

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

        //装箱单选择库存后 生成明细
        [HttpPost]
        public ActionResult LoadContainerHuDetailAdd()
        {
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);
            string[] soNumber = Request.Form.GetValues("soNumber");
            string[] clientCode = Request.Form.GetValues("clientCode");
            int processId = Convert.ToInt32(Request["processId"]);
            string processName = Request["processName"];

            string result = cf.LoadContainerHuDetailAdd(Session["whCode"].ToString(), loadContainerId, soNumber, clientCode, processId, processName, Session["userName"].ToString());

            WCF.OutBoundService.LoadContainerResult entity = new WCF.OutBoundService.LoadContainerResult();
            entity.Id = loadContainerId;

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "保存成功！", entity, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //装箱单选择库存后 生成明细  ByPOSKU
        [HttpPost]
        public ActionResult LoadContainerHuDetailAddByPOSKU()
        {
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);
            string[] soNumber = Request.Form.GetValues("soNumber");
            string[] poNumber = Request.Form.GetValues("poNumber");
            string[] altNumber = Request.Form.GetValues("altNumber");
            string[] clientCode = Request.Form.GetValues("clientCode");
            int processId = Convert.ToInt32(Request["processId"]);
            string processName = Request["processName"];

            string result = cf.LoadContainerHuDetailAddByPOSKU(Session["whCode"].ToString(), loadContainerId, soNumber, poNumber, altNumber, clientCode, processId, processName, Session["userName"].ToString());

            WCF.OutBoundService.LoadContainerResult entity = new WCF.OutBoundService.LoadContainerResult();
            entity.Id = loadContainerId;

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "保存成功！", entity, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //装箱单所选库存 按SO查询
        [HttpGet]
        public ActionResult SelectLoadContainerHuDetailBySo()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadContainerHuDetailId = Convert.ToInt32(Request["loadContainerId"]);

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult2> list = cf.SelectLoadContainerHuDetailBySo(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,SoNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
        }

        //装箱单所选库存 按SOP查询
        [HttpGet]
        public ActionResult SelectLoadContainerHuDetailBySoPo()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadContainerHuDetailId = Convert.ToInt32(Request["loadContainerId"]);
            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult2> list = cf.SelectLoadContainerHuDetailBySoPo(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,SoNumber:130,CustomerPoNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
        }

        //装箱单所选库存 按SO查询
        [HttpGet]
        public ActionResult SelectLoadContainerHuDetailBySoPoSku()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadContainerHuDetailId = Convert.ToInt32(Request["loadContainerId"]);
            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult2> list = cf.SelectLoadContainerHuDetailBySoPoSku(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
        }

        //装箱单所选库存 按款号属性查询
        [HttpGet]
        public ActionResult SelectLoadContainerHuDetailBySoPoSkuStyle()
        {
            WCF.OutBoundService.LoadContainerHuDetailSearch entity = new WCF.OutBoundService.LoadContainerHuDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadContainerHuDetailId = Convert.ToInt32(Request["loadContainerId"]);

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult2> list = cf.SelectLoadContainerHuDetailBySoPoSkuStyle(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Id", "id");
            fieldsName.Add("Sequence", "顺序");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("ItemId", "属性ID");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "Lot时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:55", null, "", 200, str));
        }

        //修改装箱单所选的库存明细的顺序
        [HttpGet]
        public ActionResult LoadContainerHuDetailEdit()
        {
            WCF.OutBoundService.LoadContainerExtendHuDetail entity = new WCF.OutBoundService.LoadContainerExtendHuDetail();
            if (Request["Id"] != "undefined" && Request["Id"] != "")
            {
                entity.Id = Convert.ToInt32(Request["Id"]);
            }
            else
            {
                entity.Id = 0;
            }
            if (Request["Qty"] != "undefined" && Request["Qty"] != "")
            {
                if (Request["Qty"] != "")
                {
                    entity.Qty = Convert.ToInt32(Request["Qty"]);
                }
            }
            if (Request["sku"] != "undefined")
            {
                entity.AltItemNumber = Request["sku"];
            }
            else
            {
                entity.AltItemNumber = "";
            }
            if (Request["po"] != "undefined")
            {
                entity.CustomerPoNumber = Request["po"];
            }
            else
            {
                entity.CustomerPoNumber = "";
            }
            if (Request["Sequence"] != "undefined")
            {
                if (Request["Sequence"] != "")
                {
                    entity.Sequence = Convert.ToInt32(Request["Sequence"]);
                }
                else
                {
                    entity.Sequence = 0;
                }
            }
            else
                entity.Sequence = 0;
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["so"];
            entity.LoadContainerId = Convert.ToInt32(Request["loadContainerId"]);
            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.LoadContainerHuDetailEdit(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "明细修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除装箱单所选的库存明细
        [HttpGet]
        public ActionResult LoadContainerHuDetailDel()
        {
            WCF.OutBoundService.LoadContainerExtendHuDetail entity = new WCF.OutBoundService.LoadContainerExtendHuDetail();
            if (Request["Id"] != "undefined")
            {
                entity.Id = Convert.ToInt32(Request["Id"]);
            }
            else
            {
                entity.Id = 0;
            }
            if (Request["sku"] != "undefined")
            {
                entity.AltItemNumber = Request["sku"];
            }
            else
            {
                entity.AltItemNumber = "";
            }
            if (Request["po"] != "undefined")
            {
                entity.CustomerPoNumber = Request["po"];
            }
            else
            {
                entity.CustomerPoNumber = "";
            }

            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["so"];
            entity.LoadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            string result = cf.LoadContainerHuDetailDel(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //导入装箱单顺序
        [HttpPost]
        public ActionResult ImportsSequenceBySo()
        {
            string[] so = Request.Form.GetValues("SO号");
            string[] seq = Request.Form.GetValues("顺序");
            string whCode = Session["whCode"].ToString();
            string userName = Session["userName"].ToString();
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            if (so == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }

            if (so.Count() != seq.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < so.Length; i++)
            {
                if (!data.ContainsValue(so[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, so[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + so[i].ToString().Trim() + "-" + seq[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = cf.ImportsSequenceBySo(whCode, loadContainerId, so, seq, userName);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //导入装箱单顺序
        [HttpPost]
        public ActionResult ImportsSequenceBySoPo()
        {
            string[] so = Request.Form.GetValues("SO号");
            string[] po = Request.Form.GetValues("PO");
            string[] seq = Request.Form.GetValues("顺序");
            string whCode = Session["whCode"].ToString();
            string userName = Session["userName"].ToString();
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            if (so == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }

            if (so.Count() != seq.Count() || seq.Count() != po.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < so.Length; i++)
            {
                if (!data.ContainsValue(so[i].ToString().Trim() + "-" + po[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, so[i].ToString().Trim() + "-" + po[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + seq[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = cf.ImportsSequenceBySoPo(whCode, loadContainerId, so, po, seq, userName);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //导入装箱单顺序
        [HttpPost]
        public ActionResult ImportsSequenceBySoPoSku()
        {
            string[] so = Request.Form.GetValues("SO号");
            string[] po = Request.Form.GetValues("PO");
            string[] sku = Request.Form.GetValues("款号");
            string[] seq = Request.Form.GetValues("顺序");
            string whCode = Session["whCode"].ToString();
            string userName = Session["userName"].ToString();
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            if (so == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }

            if (so.Count() != seq.Count() || seq.Count() != po.Count() || po.Count() != sku.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < so.Length; i++)
            {
                if (!data.ContainsValue(so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim() + "-" + seq[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = cf.ImportsSequenceBySoPoSku(whCode, loadContainerId, so, po, sku, seq, userName);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //导入装箱单顺序
        [HttpPost]
        public ActionResult ImportsSequenceBySoPoSkuStyle()
        {
            string[] so = Request.Form.GetValues("SO号");
            string[] po = Request.Form.GetValues("PO");
            string[] sku = Request.Form.GetValues("款号");
            string[] itemId = Request.Form.GetValues("属性ID");
            string[] seq = Request.Form.GetValues("顺序");
            string whCode = Session["whCode"].ToString();
            string userName = Session["userName"].ToString();
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            if (so == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }

            if (so.Count() != seq.Count() || seq.Count() != po.Count() || po.Count() != sku.Count() || sku.Count() != itemId.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < so.Length; i++)
            {
                if (!data.ContainsValue(so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim() + "-" + itemId[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim() + "-" + itemId[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + so[i].ToString().Trim() + "-" + po[i].ToString().Trim() + "-" + sku[i].ToString().Trim() + "-" + itemId[i].ToString().Trim() + "-" + seq[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = cf.ImportsSequenceBySoPoSkuStyle(whCode, loadContainerId, so, po, sku, itemId, seq, userName);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //确认生成CLP
        [HttpPost]
        public ActionResult ConfirmLoadMasterAdd()
        {
            string whCode = Session["whCode"].ToString();
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);
            string userName = Session["userName"].ToString();

            string result = cf.ConfirmLoadMasterAdd(whCode, userName, loadContainerId);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "创建成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "创建失败！", null, "");
            }
        }

        //箱型警戒立方
        [HttpGet]
        public void CheckLoadContainerCBMToRealease()
        {
            string result = cf.CheckLoadContainerCBMToRealease(Convert.ToInt32(Request["loadContainerId"]));
            Response.Write(result);
        }

        //确认保存前 提示数量及立方
        [HttpGet]
        public void CheckLoadContainerDetailToRealease()
        {
            string result = cf.CheckLoadContainerDetailToRealease(Convert.ToInt32(Request["loadContainerId"]));
            Response.Write(result);
        }


        //验证客户流程
        [HttpPost]
        public ActionResult CheckImportsSNCreateFlowRule()
        {
            string[] clientCode = Request.Form.GetValues("客户名");

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
        public ActionResult ImportsOutBoundOrderByExtend()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("出库单号");
            string[] snNumber = Request.Form.GetValues("SN");
            string[] qty = Request.Form.GetValues("数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }

            if (clientCode.Count() != soNumber.Count())
            {
                return Helper.RedirectAjax("err", "请检查客户名与SO号行数是否一致！", null, "");
            }

            string result1 = "";
            if (snNumber != null)
            {
                if (snNumber.Count() != qty.Count())
                {
                    return Helper.RedirectAjax("err", "请检查SN与数量行数是否一致！", null, "");
                }

                for (int i = 0; i < snNumber.Length; i++)
                {
                    if (qty[i].ToString() != "1")
                    {
                        result1 = "导入SN时数量必须为1！";
                        break;
                    }
                }
            }

            if (result1 != "")
            {
                return Helper.RedirectAjax("err", result1, null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorstring = "";
            int k = 0;
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + snNumber[i].ToString().Trim() + "-" + qty[i].ToString().Trim()))
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + snNumber[i].ToString().Trim() + "-" + qty[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorstring = "数据:" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + snNumber[i].ToString().Trim() + "-" + qty[i].ToString().Trim();
                }
            }

            if (errorstring != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorstring, null, "");
            }

            List<WCF.OutBoundService.OutBoundOrderExtendInsert> entityList = new List<WCF.OutBoundService.OutBoundOrderExtendInsert>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (entityList.Where(u => u.ClientCode == clientCode[i].Trim() && u.SoNumber == soNumber[i].Trim()).Count() == 0)
                {
                    WCF.OutBoundService.OutBoundOrderExtendInsert entity = new WCF.OutBoundService.OutBoundOrderExtendInsert();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ClientCode = clientCode[i].Trim();
                    entity.SoNumber = soNumber[i].Trim();
                    entity.Qty = Convert.ToInt32(qty[i].Trim());
                    entity.CreateUser = Session["userName"].ToString();

                    if (snNumber.Count() > 0)
                    {
                        List<WCF.OutBoundService.OutBoundOrderDetailExtendModel> orderDetailList = new List<WCF.OutBoundService.OutBoundOrderDetailExtendModel>();

                        WCF.OutBoundService.OutBoundOrderDetailExtendModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailExtendModel();
                        orderDetail.SNNumber = snNumber[i].ToString().Trim();
                        orderDetailList.Add(orderDetail);

                        entity.OutBoundOrderDetailExtendModel = orderDetailList.ToArray();
                    }

                    entityList.Add(entity);
                }
                else
                {
                    WCF.OutBoundService.OutBoundOrderExtendInsert oldentity = entityList.Where(u => u.ClientCode == clientCode[i].Trim() && u.SoNumber == soNumber[i].Trim()).First();
                    entityList.Remove(oldentity);

                    WCF.OutBoundService.OutBoundOrderExtendInsert newentity = oldentity;
                    newentity.Qty = oldentity.Qty + Convert.ToInt32(qty[i].ToString());

                    List<WCF.OutBoundService.OutBoundOrderDetailExtendModel> orderDetailList = oldentity.OutBoundOrderDetailExtendModel.ToList();

                    if (snNumber.Count() > 0)
                    {
                        WCF.OutBoundService.OutBoundOrderDetailExtendModel orderDetail = new WCF.OutBoundService.OutBoundOrderDetailExtendModel();
                        orderDetail.SNNumber = snNumber[i].ToString().Trim();
                        orderDetailList.Add(orderDetail);
                    }

                    newentity.OutBoundOrderDetailExtendModel = orderDetailList.ToArray();
                    entityList.Add(newentity);
                }
            }

            string result = cf.ImportsOutBoundOrderExtendBySN(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpGet]
        public ActionResult ChangeLoadContainerExtendBySNList()
        {
            WCF.OutBoundService.PickTaskDetailSearch entity = new WCF.OutBoundService.PickTaskDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["loadId"];
            entity.HuId = Request["txt_huId_sn"];

            int total = 0;
            List<WCF.OutBoundService.PickTaskDetailResult2> list = cf.PickTaskDetailList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action1", "删除");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("HuId", "SN");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "SKU");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("Status", "备货状态");
            fieldsName.Add("Status1", "装箱状态");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action1:40,Action2:50,LoadId:140,HuId:130,ClientCode:100,SoNumber:100,CustomerPoNumber:100,AltItemNumber:100,Status:70,Status1:70,default:50", null, "", 200, ""));
        }


        [HttpGet]
        public ActionResult ChangeLoadContainerExtendBySNDel()
        {
            WCF.OutBoundService.PickTaskDetailResult2 entity = new WCF.OutBoundService.PickTaskDetailResult2();
            entity.WhCode = Session["whCode"].ToString();
            entity.CreateUser = Session["userName"].ToString();
            entity.LoadId = Request["load"];
            entity.HuId = Request["huid"];
            entity.ClientCode = Request["clientcode"];
            entity.SoNumber = Request["sonumber"];
            entity.CustomerPoNumber = Request["ponumber"];
            entity.AltItemNumber = Request["itemnumber"];
            entity.UnitName = Request["unitname"];
            entity.ItemId = Convert.ToInt32(Request["itemid"]);
            if (Request["lotnumber1"] == null || Request["lotnumber1"] == "null")
            {
                entity.LotNumber1 = "";
            }
            else
            {
                entity.LotNumber1 = Request["lotnumber1"];
            }

            if (Request["lotnumber2"] == null || Request["lotnumber2"] == "null")
            {
                entity.LotNumber2 = "";
            }
            else
            {
                entity.LotNumber2 = Request["lotnumber2"];
            }

            string result = cf.PickTaskDetailHuIdDel(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult ChangeLoadContainerExtendBySNAdd()
        {
            WCF.OutBoundService.PickTaskDetailResult2 entity = new WCF.OutBoundService.PickTaskDetailResult2();
            entity.WhCode = Session["whCode"].ToString();
            entity.CreateUser = Session["userName"].ToString();
            entity.LoadId = Request["load"];
            entity.HuId = Request["add_changesn"];

            string result = cf.PickTaskDetailHuIdAdd(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "新增成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}
