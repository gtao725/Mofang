using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LoadContainerController : Controller
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

            ViewData["LoadChargeDaiDianSelect"] = from r in cf.LoadChargeDaiDianSelectList()
                                                  select new SelectListItem()
                                                  {
                                                      Text = r.DaiDianName,     //text
                                                      Value = r.Id.ToString()
                                                  };

            ViewData["userName"] = Session["userName"].ToString();

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


            if (Request["createUser"] != "undefined")
            {
                entity.CreateUser = Request["createUser"];
            }
            else
            {
                entity.CreateUser = "";
            }

            entity.ChuCangFS = Request["shipMode"].Trim();
            entity.VesselName = Request["VesselName"].Trim();

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
            entity.LoadChargeStatus = Request["LoadChargeStatus"];

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerResult> list = cf.LoadContainerList(entity, poNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Action1", "指定托盘");
            fieldsName.Add("Action2", "附加直装");
            fieldsName.Add("Action3", "费用管理");
            fieldsName.Add("Action4", "提货费用");
            fieldsName.Add("LoadMasterId", "loadMasterId");
            fieldsName.Add("LoadId", "Load");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("Status0", "释放状态");
            fieldsName.Add("Status1", "备货状态");
            fieldsName.Add("Status3", "装箱状态");
            fieldsName.Add("ShipStatus", "封箱状态");

            fieldsName.Add("SumQty", "出货数量");
            fieldsName.Add("SumCBM", "总立方");
            fieldsName.Add("DSSumQty", "直装数量");
            fieldsName.Add("EchQty", "挂衣数");
            fieldsName.Add("SumWeight", "总重量");

            fieldsName.Add("BillNumber", "提单号");
            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("SealNumber", "封号");
            fieldsName.Add("ContainerName", "箱型名称");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("ChuCangFS", "出仓方式");
            fieldsName.Add("ProcessId", "ProcessId");
            fieldsName.Add("ProcessName", "所选出货流程");

            fieldsName.Add("VesselName", "船名");
            fieldsName.Add("VesselNumber", "航次");
            fieldsName.Add("CarriageName", "船公司");
            fieldsName.Add("ETDShow", "ETD时间");
            fieldsName.Add("Port", "港区");
            fieldsName.Add("PortSuticase", "提箱进港");
            fieldsName.Add("DeliveryPlace", "交货地");
            fieldsName.Add("WeightFlag", "是否称重");

            fieldsName.Add("ReleaseDate", "释放时间");
            fieldsName.Add("BeginPickDate", "开始备货时间");
            fieldsName.Add("EndPickDate", "结束备货时间");
            fieldsName.Add("BeginPackDate", "开始装箱时间");
            fieldsName.Add("EndPackDate", "结束装箱时间");
            fieldsName.Add("ShipDate", "封箱时间");

            fieldsName.Add("ContainerSource", "箱单来源");
            fieldsName.Add("Remark", "出货备注");

            fieldsName.Add("CreateUserName", "创建者");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("LoadContainerHuDetailId", "已选库存");
            fieldsName.Add("LoadChargeStatus", "费用状态");
            fieldsName.Add("ClientContractNameOut", "出货合同");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:146,Action1:55,Action2:55,Action3:55,Action4:55,LoadId:140,ClientCode:110,ContainerName:120,ProcessName:130,VesselName:120,Port:170,BillNumber:110,ContainerNumber:90,SealNumber:75,CreateDate:130,ETDShow:130,ContainerType:60,Status0:60,Status1:60,Status3:60,ShipStatus:60,SumQty:60,DSSumQty:60,EchQty:50,SumCBM:50,SumWeight:50,ChuCangFS:70,WeightFlag:70,ReleaseDate:130,BeginPickDate:130,EndPickDate:130,BeginPackDate:130,EndPackDate:130,ShipDate:130,default:90", null, "", 50, str));
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

            int total = 0;
            string str = "";
            List<WCF.OutBoundService.LoadContainerHuDetailResult> list = cf.LoadContainerHuDetailList(entity, List1.ToArray(), out total, out str).ToList();

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

            List<WCF.OutBoundService.LoadContainerHuDetailResult> list = cf.LoadContainerHuDetailListByPOSKU(entity, List1.ToArray(), List2.ToArray(), List3.ToArray(), List4.ToArray(), List5.ToArray(), out total, out str).ToList();

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


        //装箱单选择库存后 生成明细  ByPOSKU
        [HttpPost]
        public ActionResult LoadContainerHuDetailAddByImportPOSKU()
        {
            int loadContainerId = Convert.ToInt32(Request["loadContainerId"]);
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("SO号");
            string[] poNumber = Request.Form.GetValues("PO号");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] qty = Request.Form.GetValues("库存数量");
            string[] sequence = Request.Form.GetValues("顺序");

            int[] qtyArr = Array.ConvertAll(qty, int.Parse);



            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != soNumber.Count() || poNumber.Count() != soNumber.Count() || poNumber.Count() != altItemNumber.Count() || altItemNumber.Count() != qty.Count() || qty.Count() != sequence.Count() || sequence.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() ))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString() );
                    k++;
                }
                else
                {
                    errorItemNumber = "第" + (i + 1) + "行" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + poNumber[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + qty[i].ToString() + "-" + sequence[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = "";
            if (sequence.Count() > 0)
            {
                if (!string.IsNullOrEmpty(sequence[0]))
                {
                    int[] sequenceArr = Array.ConvertAll(sequence, int.Parse);
                    result = cf.LoadContainerHuDetailAddByImportPOSKU(Session["whCode"].ToString(), loadContainerId, soNumber, poNumber, altItemNumber, clientCode, qtyArr, sequenceArr, Session["userName"].ToString());
                }
                else
                {
                    result = cf.LoadContainerHuDetailAddByImportPOSKU(Session["whCode"].ToString(), loadContainerId, soNumber, poNumber, altItemNumber, clientCode, qtyArr, null, Session["userName"].ToString());
                }
            }
            else
            {
                result = cf.LoadContainerHuDetailAddByImportPOSKU(Session["whCode"].ToString(), loadContainerId, soNumber, poNumber, altItemNumber, clientCode, qtyArr, null, Session["userName"].ToString());
            }

            WCF.OutBoundService.LoadContainerResult entity = new WCF.OutBoundService.LoadContainerResult();
            entity.Id = loadContainerId;

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", entity, "");
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
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Sequence:60,SoNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
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
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Sequence:60,SoNumber:130,CustomerPoNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
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
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessName", "出货流程");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Sequence:60,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:70", null, "", 200, str));
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
            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Sequence:50,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ClientCode:100,ProcessName:100,CreateDate:130,default:55", null, "", 200, str));
        }

        //批量修改顺序及数量
        [HttpPost]
        public ActionResult BatchLoadContainerHuDetailEdit()
        {
            string[] Id = Request.Form.GetValues("check_to4");
            string[] SO = Request.Form.GetValues("check_so");
            string[] PO = Request.Form.GetValues("check_po");
            string[] SKU = Request.Form.GetValues("check_sku");

            int LoadContainerId = Convert.ToInt32(Request["loadContainerId"]);

            string[] Qty = Request.Form.GetValues("edit_Qty");
            string[] Sequence = Request.Form.GetValues("edit_Sequence");

            string result = "";
            for (int i = 0; i < Id.Length; i++)
            {
                WCF.OutBoundService.LoadContainerExtendHuDetail entity = new WCF.OutBoundService.LoadContainerExtendHuDetail();
                if (Id[i] != "undefined" && Id[i] != "")
                {
                    entity.Id = Convert.ToInt32(Id[i]);
                }
                else
                {
                    entity.Id = 0;
                }
                if (Qty[i] != "undefined" && Qty[i] != "")
                {
                    entity.Qty = Convert.ToInt32(Qty[i]);
                }
                if (SKU[i] != "undefined")
                {
                    entity.AltItemNumber = SKU[i];
                }
                else
                {
                    entity.AltItemNumber = "";
                }
                if (PO[i] != "undefined")
                {
                    entity.CustomerPoNumber = PO[i];
                }
                else
                {
                    entity.CustomerPoNumber = "";
                }
                if (Sequence[i] != "undefined")
                {
                    if (Sequence[i] != "")
                    {
                        entity.Sequence = Convert.ToInt32(Sequence[i]);
                    }
                    else
                    {
                        entity.Sequence = 0;
                    }
                }
                else
                {
                    entity.Sequence = 0;
                }
                entity.WhCode = Session["whCode"].ToString();
                entity.SoNumber = SO[i];
                entity.LoadContainerId = Convert.ToInt32(LoadContainerId);
                entity.UpdateUser = Session["userName"].ToString();

                result = cf.LoadContainerHuDetailEdit(entity);
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


        [HttpGet]
        public ActionResult LoadChargeRuleUnselected()
        {
            WCF.OutBoundService.LoadChargeRuleSearch whPositionSearch = new WCF.OutBoundService.LoadChargeRuleSearch();

            whPositionSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPositionSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPositionSearch.SOFeeFlag = 0;
            whPositionSearch.TypeName = Request["TypeName"];

            int total = 0;
            WCF.OutBoundService.LoadChargeRuleResult[] list = cf.LoadChargeRuleUnselected(whPositionSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FunctionName", "收费科目");
            fieldsName.Add("FunctionUnitName", "计费单位");
            fieldsName.Add("CustomerFlag", "客服计件");
            fieldsName.Add("WarehouseFlag", "仓库计件");
            fieldsName.Add("Description", "说明");
            fieldsName.Add("BargainingFlag", "议价");
            fieldsName.Add("BargainingDescription", "是否议价");
            fieldsName.Add("DaiDianId", "代垫ID");
            fieldsName.Add("TypeName", "科目类型");
            fieldsName.Add("ClientAutoCharge", "列表客户系统计算");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:45,FunctionName:140,FunctionUnitName:60,BargainingFlag:45,BargainingDescription:60,default:85"));
        }

        [HttpGet]
        public ActionResult LoadChargeRuleSelected()
        {
            WCF.OutBoundService.LoadChargeRuleSearch whPositionSearch = new WCF.OutBoundService.LoadChargeRuleSearch();

            whPositionSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPositionSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPositionSearch.WhCode = Session["whCode"].ToString();
            whPositionSearch.LoadId = Request["LoadId"].ToString();

            int total = 0;
            WCF.OutBoundService.LoadChargeDetailResult[] list = cf.LoadChargeRuleSelected(whPositionSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ChargeName", "收费科目");
            fieldsName.Add("QtyCbm", "数量或立方");
            fieldsName.Add("ChargeUnitName", "计费单位");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:45,ChargeName:160,ChargeUnitName:60,default:80"));
        }

        [HttpPost]
        public ActionResult GetFunctionUnitNameList()
        {
            string unitName = Request["unitName"];

            string[] itemNumberList = null;
            if (!string.IsNullOrEmpty(unitName))
            {
                string item_temp = unitName.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                itemNumberList = item_temp.Split(',');           //把SO 按照@分割，放在数组
            }

            if (itemNumberList != null)
            {
                var sql = from r in itemNumberList
                          select new
                          {
                              Value = r.ToString(),
                              Text = r    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //得到科目类型列表
        [HttpPost]
        public ActionResult GetLoadChangeRuleTypeName()
        {
            List<WCF.OutBoundService.LoadChargeRuleResult> list1 = cf.GetLoadChangeRuleTypeName().ToList();

            if (list1 != null)
            {
                WCF.OutBoundService.LoadChargeRuleResult first = new WCF.OutBoundService.LoadChargeRuleResult();
                first.FunctionName = "";
                list1.Add(first);

                var sql = from r in list1.OrderBy(u => u.FunctionName)
                          select new
                          {
                              Value = r.FunctionName,
                              Text = r.FunctionName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult LoadChargeRuleCheck()
        {
            string loadId = Request["Rule_loadId"];
            string whCode = Session["whCode"].ToString();
            int RuleAdd_id = Convert.ToInt32(Request["RuleAdd_id"]);
            string RuleAdd_QtyCBM = Request["RuleAdd_QtyCBM"];
            string RuleAdd_UnitName = Request["RuleAdd_UnitName"];
            decimal RuleAdd_Price = Convert.ToDecimal(Request["RuleAdd_Price"]);


            if (RuleAdd_UnitName == "立方")
            {
                try
                {
                    decimal s = Convert.ToDecimal(Request["RuleAdd_QtyCBM"]);
                }
                catch
                {
                    return Helper.RedirectAjax("err", "立方输入格式有误！", null, "");
                }
            }
            else
            {
                try
                {
                    int s = Convert.ToInt32(Request["RuleAdd_QtyCBM"]);
                }
                catch
                {
                    return Helper.RedirectAjax("err", "数量输入格式有误！", null, "");
                }
            }

            WCF.OutBoundService.LoadChargeDetailInsert entity = new WCF.OutBoundService.LoadChargeDetailInsert();
            entity.loadChargeRuleId = RuleAdd_id;
            entity.qtycbm = RuleAdd_QtyCBM;
            entity.unitName = RuleAdd_UnitName;
            entity.price = RuleAdd_Price;
            entity.loadId = loadId;
            entity.whCode = whCode;
            entity.userName = Session["userName"].ToString();
            entity.soNumber = Request["RuleAdd_SoNumber"];

            if (!string.IsNullOrEmpty(Request["RuleAdd_DaiDian"]))
            {
                entity.daiDianId = Convert.ToInt32(Request["RuleAdd_DaiDian"]);
            }
            else
            {
                entity.daiDianId = 0;
            }

            string result = cf.LoadChargeRuleCheck(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public void LoadChargeClientAutoCharge()
        {
            WCF.OutBoundService.LoadChargeDetailResult entity = new WCF.OutBoundService.LoadChargeDetailResult();
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["client_loadId"];
            entity.ChargeName = Request["client_chargeName"];
            entity.ClientCode = Request["client_clientCode"];
            entity.CreateUser = Session["userName"].ToString();

            string result = cf.LoadChargeClientAutoCharge(entity);

            Response.Write(result);
        }



        [HttpGet]
        public ActionResult LoadChargeDetailDelById()
        {
            string result = cf.LoadChargeDetailDelById(Convert.ToInt32(Request["Id"]));
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //验证出货费用是否已录入完成且未遗漏必选项
        [HttpGet]
        public void LoadChargeDetailCheck()
        {
            string result = cf.LoadChargeDetailCheck(Session["whCode"].ToString(), Request["LoadId"]);
            Response.Write(result);
        }

        public ActionResult FeeDetailList()
        {
            WCF.OutBoundService.LoadChargeRuleSearch entity = new WCF.OutBoundService.LoadChargeRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["LoadId"];

            int total = 0;
            List<WCF.OutBoundService.LoadChargeDetailResult> list = cf.LoadChargeDetailList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ChargeType", "类型");
            fieldsName.Add("ChargeName", "收费科目");
            fieldsName.Add("LadderNumber", "阶梯数值");
            fieldsName.Add("QtyCbm", "数量或立方");
            fieldsName.Add("ChargeUnitName", "计费单位");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("PriceTotal", "总价");
            fieldsName.Add("Remark", "计费方式");
            fieldsName.Add("Description", "说明");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:45,ChargeName:130,QtyCbm:70,ChargeUnitName:60,Price:60,PriceTotal:60,Remark:60,default:80"));
        }

        [HttpGet]
        public ActionResult EditFeeDetail()
        {
            string result = cf.LoadChargeEdit(Session["whCode"].ToString(), Request["LoadId"], "C");
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "状态确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult LoadChargeEditReload()
        {
            string result = cf.LoadChargeEdit(Session["whCode"].ToString(), Request["LoadId"], "U");
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "状态撤销成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult AgainLoadCharge()
        {
            string result = cf.AAgainLoadCharge(Session["whCode"].ToString(), Request["LoadId"], Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "重新计算基础费用成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //创建SO特费
        [HttpPost]
        public ActionResult AddLoadChargeDetailBySO()
        {
            WCF.OutBoundService.LoadChargeDetail entity = new WCF.OutBoundService.LoadChargeDetail();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_clientCode1"];
            if (Request["txt_ETD"] != "")
            {
                entity.ETD = Convert.ToDateTime(Request["txt_ETD"]);
            }
            entity.LoadId = "";
            entity.SoNumber = Request["txt_SoNumber"].Trim();
            entity.UnitName = Request["txt_BillNumber"].Trim();

            entity.ContainerNumber = Request["txt_ContainerNumber"].Trim().Replace(" ", "");
            entity.ContainerType = Request["txt_ContainerType"];
            entity.CBM = Convert.ToDecimal(Request["txt_Quantity"].Trim());
            entity.LoadChargeRuleId = Convert.ToInt32(Request["txt_LoadChargeRuleId"].Trim());
            entity.Price = Convert.ToDecimal(Request["txt_Price"].Trim());
            entity.PriceTotal = entity.CBM * entity.Price;
            entity.ChargeUnitName = Request["txt_CaoZuoUser"].Trim();
            entity.NotLoadFlag = 1;
            entity.SoStatus = "U";
            entity.LadderNumber = Request["txt_FCR"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CustomerFlag = 1;
            entity.BargainingFlag = 1;
            entity.Remark = Request["txt_Remark"].Trim();
            entity.TaxInclusiveFlag = Convert.ToInt32(Request["txt_sea_taxInclusiveFlag"]);

            string result = cf.LoadChargeDetailAddBySO(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }


        //编辑SO特费
        [HttpGet]
        public ActionResult LoadChargeDetailSoEdit()
        {
            WCF.OutBoundService.LoadChargeDetail entity = new WCF.OutBoundService.LoadChargeDetail();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            if (Request["edit_txt_ETD"] != "")
            {
                entity.ETD = Convert.ToDateTime(Request["edit_txt_ETD"]);
            }

            entity.SoNumber = Request["edit_txt_SoNumber"].Trim();
            entity.UnitName = Request["edit_txt_BillNumber"].Trim();

            entity.ContainerNumber = Request["edit_txt_ContainerNumber"].Trim().Replace(" ", "");
            entity.ContainerType = Request["edit_txt_ContainerType"];
            entity.CBM = Convert.ToDecimal(Request["edit_txt_Quantity"].Trim());
            entity.LoadChargeRuleId = Convert.ToInt32(Request["edit_txt_LoadChargeRuleId"].Trim());
            entity.Price = Convert.ToDecimal(Request["edit_txt_Price"].Trim());
            entity.PriceTotal = entity.CBM * entity.Price;
            entity.ChargeUnitName = Request["edit_txt_CaoZuoUser"].Trim();
            entity.LadderNumber = Request["edit_txt_FCR"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.Remark = Request["edit_txt_Remark"].Trim();
            entity.TaxInclusiveFlag = Convert.ToInt32(Request["edi_txt_sea_taxInclusiveFlag"]);


            string result = cf.LoadChargeDetailEditBySO(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //查询SO特费列表
        public ActionResult GetLoadChargeSOList()
        {
            WCF.OutBoundService.LoadChargeDetailSearch entity = new WCF.OutBoundService.LoadChargeDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.BillNumber = Request["txt_sea_BillNumber"];
            entity.ContainerNumber = Request["txt_sea_ContainerNumber"];
            entity.ClientCode = Request["txt_sea_clientCode"];

            if (Request["txt_sea_ETD_begin"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["txt_sea_ETD_begin"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["txt_sea_ETD_end"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["txt_sea_ETD_end"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
            }
            if (Request["txt_sea_createUser"] != "")
            {
                entity.CreateUser = Session["userName"].ToString();
            }

            string so_area = Request["txt_sea_SoNumber_area"];
            string[] soList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.OutBoundService.LoadChargeDetailResult> list = cf.LoadChargeDetailSOList(entity, soList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ETDShow", "ETD时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("UnitName", "提单号");
            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("LoadChargeRuleId", "LoadChargeRuleId");
            fieldsName.Add("ChargeName", "科目");
            fieldsName.Add("CBM", "数量");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("PriceTotal", "总价");
            fieldsName.Add("ChargeUnitName", "操作员");
            fieldsName.Add("LadderNumber", "FCR号");
            fieldsName.Add("SoStatus", "状态");
            fieldsName.Add("TaxInclusiveFlag", "是否含税");
            fieldsName.Add("Remark", "备注");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,ClientCode:130,ETD:80,SoNumber:130,CBM:45,Price:65,PriceTotal:65,Remark:150,default:90"));
        }

        [HttpGet]
        public ActionResult LoadChargeDetailSoDel()
        {
            string result = cf.LoadChargeDetailSoDel(Convert.ToInt32(Request["Id"]), Session["userName"].ToString());
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
        public ActionResult LoadChargeDetailSoBatchDel()
        {
            string[] Id = Request.Form.GetValues("idarr");

            string result = "";
            for (int i = 0; i < Id.Length; i++)
            {
                result = cf.LoadChargeDetailSoDel(Convert.ToInt32(Id[i]), Session["userName"].ToString());
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
        public ActionResult LoadChargeBatchEdit()
        {
            string[] loadId = Request.Form.GetValues("loadId");

            int sucCount = 0;
            int errCount = 0;
            for (int i = 0; i < loadId.Length; i++)
            {
                string result = cf.LoadChargeEdit(Session["whCode"].ToString(), loadId[i].ToString(), "C");
                if (result == "Y")
                {
                    sucCount++;
                }
                else
                {
                    errCount++;
                }
            }

            if (errCount > 0)
            {
                return Helper.RedirectAjax("err", "本次一共确认：" + (sucCount + errCount) + "票。其中成功：" + sucCount + "，失败：" + errCount + "，当前有失败的，需查看明细手动确认！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "本次一共确认：" + (sucCount + errCount) + "票。其中成功：" + sucCount + "，无确认失败！", null, "");
            }

        }


        [HttpGet]
        public void CheckCount()
        {
            WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;
            cf.CheckLoadChangeErrorCount(entity, out total);
            Response.Write(total + "个");
        }

        public ActionResult GetLoadChargeErrorList()
        {
            WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["txt_err_LoadId"];
            entity.ClientCode = Request["txt_err_clientCode"];

            int total = 0;
            List<WCF.OutBoundService.LoadChargeDetailResult> list = cf.CheckLoadChangeErrorCount(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("Description", "异常说明");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:40,Description:470,default:140"));
        }


        [HttpPost]
        public ActionResult ImportsLoadChargeDetailBySO()
        {
            string[] clientCode = Request.Form.GetValues("客户");
            string[] etd = Request.Form.GetValues("ETD");
            string[] soNumber = Request.Form.GetValues("SO号");
            string[] billNumber = Request.Form.GetValues("提单号");
            string[] containerNumber = Request.Form.GetValues("箱号");
            string[] loadChargeRule = Request.Form.GetValues("科目");
            string[] quantity = Request.Form.GetValues("数量");
            string[] price = Request.Form.GetValues("单价");
            string[] caoZuoUser = Request.Form.GetValues("操作员");
            string[] fcr = Request.Form.GetValues("FCR号");
            string[] remark = Request.Form.GetValues("备注");
            string[] taxInclusive = Request.Form.GetValues("是否含税");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != etd.Count() || etd.Count() != soNumber.Count() || soNumber.Count() != billNumber.Count() || billNumber.Count() != containerNumber.Count() || containerNumber.Count() != loadChargeRule.Count() || loadChargeRule.Count() != quantity.Count() || quantity.Count() != price.Count() || price.Count() != caoZuoUser.Count() || caoZuoUser.Count() != fcr.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            List<WCF.InBoundService.WhClient> clientCodeList = inboundcf.WhClientListSelect(Session["whCode"].ToString()).ToList();
            List<WCF.OutBoundService.LoadChargeRule> loadChargeRuleList = cf.LoadChargeRuleSelectList().ToList();

            string result = "";
            for (int i = 0; i < clientCode.Length; i++)
            {
                string client = clientCode[i].Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                if (clientCodeList.Where(u => u.ClientCode == client).Count() == 0)
                {
                    result = "客户名：" + client + "有误！";
                    break;
                }

                string chargeName = loadChargeRule[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                if (loadChargeRuleList.Where(u => u.FunctionName == chargeName).Count() == 0)
                {
                    result = "科目：" + chargeName + "有误！";
                    break;
                }

                if (!string.IsNullOrEmpty(etd[i]))
                {
                    try
                    {
                        DateTime ted = Convert.ToDateTime(etd[i]);
                    }
                    catch (Exception)
                    {
                        result = "ETD：" + etd[i] + "格式有误！";
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(taxInclusive[i]))
                {
                    if (taxInclusive[i] != "是" && taxInclusive[i] != "否")
                    {
                        result = "是否含税列可填写：是/否或不填写任何内容！";
                        break;
                    }
                }
            }

            if (result != "")
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                WCF.OutBoundService.LoadChargeDetail entity = new WCF.OutBoundService.LoadChargeDetail();
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientCode = clientCode[i].Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");

                entity.LoadId = "";
                entity.SoNumber = soNumber[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                entity.UnitName = billNumber[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                entity.ContainerNumber = containerNumber[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");

                string chargeName = loadChargeRule[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                entity.LoadChargeRuleId = loadChargeRuleList.Where(u => u.FunctionName == chargeName).First().Id;
                entity.ETD = Convert.ToDateTime(etd[i]);
                entity.ContainerType = "";

                if (!string.IsNullOrEmpty(taxInclusive[i]))
                {
                    if (taxInclusive[i] == "是")
                    {
                        entity.TaxInclusiveFlag = 1;
                    }
                    else
                    {
                        entity.TaxInclusiveFlag = 0;
                    }
                }
                else
                {
                    entity.TaxInclusiveFlag = 0;
                }
                entity.Remark = remark[i];

                entity.CBM = Convert.ToDecimal(quantity[i]);
                entity.Price = Convert.ToDecimal(price[i]);
                entity.PriceTotal = entity.CBM * entity.Price;
                entity.ChargeUnitName = caoZuoUser[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                entity.NotLoadFlag = 1;
                entity.SoStatus = "U";
                entity.LadderNumber = fcr[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                entity.CreateUser = Session["userName"].ToString();
                entity.CustomerFlag = 1;
                entity.BargainingFlag = 1;

                cf.LoadChargeDetailAddBySO(entity);
            }

            if (result != "")
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
            else
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }

        }


        [HttpGet]
        [Check]
        public ActionResult LoadChargeRuleListCheck()
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

        [HttpGet]
        public ActionResult GetLoadChargeRuleList()
        {
            List<WCF.OutBoundService.LoadChargeRule> list = cf.LoadChargeRuleSelectList().OrderBy(u => u.FunctionId).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FunctionName", "科目名称");
            fieldsName.Add("ChargeCode", "账单ChargeCode");
            fieldsName.Add("ChargeItem", "账单ChargeItem");

            return Content(EIP.EipListJson(list, list.Count, fieldsName, "Id:70,default:140", null, "", 200, ""));
        }

        //添加SO特费科目
        [HttpGet]
        public ActionResult AddLoadChargeRule()
        {
            WCF.OutBoundService.LoadChargeRule entity = new WCF.OutBoundService.LoadChargeRule();

            entity.FunctionName = Request["add_functionName"].Trim();
            entity.DaiDianId = Convert.ToInt32(Request["addt_DaiDianId"]);
            entity.ChargeCode = Request["add_ChargeCode"].Trim();
            entity.ChargeItem = Request["add_ChargeItem"].Trim();

            string result = cf.LoadChargeRuleAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        //编辑SO特费科目
        [HttpGet]
        public ActionResult EditLoadChargeRule()
        {
            WCF.OutBoundService.LoadChargeRule entity = new WCF.OutBoundService.LoadChargeRule();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            entity.FunctionName = Request["edit_functionName"].Trim();
            entity.DaiDianId = Convert.ToInt32(Request["edit_DaiDianId"]);
            entity.ChargeCode = Request["edit_ChargeCode"].Trim();
            entity.ChargeItem = Request["edit_ChargeItem"].Trim();

            string result = cf.LoadChargeRuleEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }



        [HttpPost]
        public ActionResult ImportsLoadContainerExtendHuDetail()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("SO号");
            string[] poNumber = Request.Form.GetValues("PO号");
            string[] itemNumber = Request.Form.GetValues("款号");
            string[] qty = Request.Form.GetValues("库存数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 3000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过3000条！", null, "");
            }



            string result = "";

            if (result != "")
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
            else
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }

        }


    }
}
