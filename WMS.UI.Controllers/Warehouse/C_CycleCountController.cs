using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_CycleCountController : Controller
    {
        WCF.InVentoryService.InVentoryServiceClient cf1 = new WCF.InVentoryService.InVentoryServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };
            return View();
        }

        //盘点任务列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.CycleCountMasterSearch entity = new WCF.RootService.CycleCountMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.TaskNumber = Request["TaskNumber"].Trim();
            entity.Type = Request["cType"];
            if (Request["createType1"] != "")
            {
                entity.CreateType = Convert.ToInt32(Request["createType1"]);
            }
            else
            {
                entity.CreateType = 0;
            }

            entity.Status = Request["Status"];
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
            List<WCF.RootService.CycleCountMasterResult> list = cf.CycleCountMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("TaskNumber", "任务号码");
            fieldsName.Add("Type", "盘点模式");
            fieldsName.Add("CreateType", "盘点类型");
            fieldsName.Add("TypeDescription", "类型说明");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LocationNullShow", "是否去掉空库位");
            fieldsName.Add("OneByOneScanShow", "是否逐件扫描");
            fieldsName.Add("CompareStorageLocationHuShow", "是否比对托盘");
            fieldsName.Add("Description", "描述");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,TaskNumber:150,Description:150,CreateDate:130,LocationNullShow:100,OneByOneScanShow:90,default:80"));
        }


        //新增盘点任务(按库位属性)
        [HttpGet]
        public ActionResult CycleCountMasterAdd()
        {
            WCF.RootService.CycleCountMasterInsert entity = new WCF.RootService.CycleCountMasterInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.Type = Request["sel_Type"];
            entity.CreateUser = Session["userName"].ToString();
            entity.Description = Request["description"].Trim();
            entity.Location = Request["location"].Trim();
            entity.LocationColumn = Request["locationColumn"].Trim();
            entity.CreateType = Convert.ToInt32(Request["createType"]);
            entity.LocationNullFlag = Convert.ToInt32(Request["sel_LocationNullFlag"]);
            entity.OneByOneScanFlag = Convert.ToInt32(Request["oneByoneScan"]);
            entity.TypeDescription = Request["typeDescription"].Trim();

            entity.CompareStorageLocationHu = Convert.ToInt32(Request["compareStorage"]);

            if (Request["locationRowBegin"] != "")
            {
                entity.LocationRowBegin = Convert.ToInt32(Request["locationRowBegin"]);
            }
            else
            {
                entity.LocationRowBegin = 0;
            }
            if (Request["locationRowEnd"] != "")
            {
                entity.LocationRowEnd = Convert.ToInt32(Request["locationRowEnd"]);
            }
            else
            {
                entity.LocationRowEnd = 0;
            }
            string result = cf.CycleCountMasterAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //新增盘点任务(按起始库位)
        [HttpGet]
        public ActionResult CycleCountMasterAddByLocationId()
        {
            WCF.RootService.CycleCountMasterInsert entity = new WCF.RootService.CycleCountMasterInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.Type = Request["sel_Type"];
            entity.CreateUser = Session["userName"].ToString();
            entity.Description = Request["description"].Trim();
            entity.BeginLocationId = Request["BeginLocationId"].Trim();
            entity.EndLocationId = Request["EndLocationId"].Trim();
            entity.CreateType = Convert.ToInt32(Request["createType"]);
            entity.LocationNullFlag = Convert.ToInt32(Request["sel_LocationNullFlag"]);
            entity.OneByOneScanFlag = Convert.ToInt32(Request["oneByoneScan"]);
            entity.TypeDescription = Request["typeDescription"].Trim();

            entity.CompareStorageLocationHu = Convert.ToInt32(Request["compareStorage"]);

            if (Request["BeginLocationId"].ToString() != "" || Request["EndLocationId"].ToString() != "")
            {
                if (entity.BeginLocationId.Length != entity.EndLocationId.Length)
                {
                    return Helper.RedirectAjax("err", "起始库位和结束库位长度必须一致！", null, "");
                }
            }

            string result = cf.CycleCountMasterAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //新增盘点任务(按动态盘点)
        [HttpGet]
        public ActionResult CycleCountMasterAddByLocationChangeTime()
        {
            WCF.RootService.CycleCountMasterInsert entity = new WCF.RootService.CycleCountMasterInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.Type = Request["sel_Type"];
            entity.CreateUser = Session["userName"].ToString();
            entity.Description = Request["description"].Trim();
            entity.CreateType = Convert.ToInt32(Request["createType"]);
            entity.LocationNullFlag = 0;
            entity.OneByOneScanFlag = Convert.ToInt32(Request["oneByoneScan"]);
            entity.TypeDescription = Request["typeDescription"].Trim();

            entity.CompareStorageLocationHu = Convert.ToInt32(Request["compareStorage"]);

            if (Request["BeginLocationChangeTime"].ToString() == "" || Request["EndLocationChangeTime"].ToString() == "")
            {
                return Helper.RedirectAjax("err", "库位变更时间不能为空！", null, "");
            }

            WCF.RootService.CycleCountMasterSeacrh searchentity = new WCF.RootService.CycleCountMasterSeacrh();
            try
            {
                searchentity.BeginCreateDate = Convert.ToDateTime(Request["BeginLocationChangeTime"].ToString());
            }
            catch
            {
                return Helper.RedirectAjax("err", "库位变更起始时间格式有误！", null, "");
            }
            try
            {
                searchentity.EndCreateDate = Convert.ToDateTime(Request["EndLocationChangeTime"].ToString());
            }
            catch
            {
                return Helper.RedirectAjax("err", "库位变更结束时间格式有误！", null, "");
            }

            string result = cf.CycleCountMasterAddByLocationChangeTime(entity, searchentity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //盘点任务明细列表
        [HttpGet]
        public ActionResult CycleCountDetailList()
        {
            WCF.RootService.CycleCountDetailSearch entity = new WCF.RootService.CycleCountDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.TaskNumber = Request["taskNumber"].Trim();
            entity.LocationId = Request["detail_locationId"].Trim();
            entity.Action = Request["sel_ResultAction"].Trim();
            entity.Status = Request["sel_ResultStatus"].Trim();

            int total = 0;
            List<WCF.RootService.CycleCountDetailResult> list = cf.CycleCountDetailList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("TaskNumber", "任务号码");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Action1", "盘点结果");
            fieldsName.Add("CheckUser", "盘点人");
            fieldsName.Add("CheckDate", "盘点时间");
            fieldsName.Add("Action", "查看盘点结果");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,TaskNumber:150,CheckDate:130,default:80"));
        }

        //删除盘点任务
        [HttpGet]
        public ActionResult CycleCountMasterDel()
        {
            WCF.RootService.CycleCountMaster entity = new WCF.RootService.CycleCountMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.TaskNumber = Request["taskNumber"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            string rerult = cf.CycleCountMasterDel(entity);

            if (rerult == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("ok", rerult, null, "");
            }
        }


        //查询库存盘点结果
        [HttpGet]
        public ActionResult CycleCountCheckList()
        {
            WCF.RootService.CycleCountCheckSearch entity = new WCF.RootService.CycleCountCheckSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.TaskNumber = Request["taskNumber"].Trim();
            entity.LocationId = Request["locationId"].Trim();

            int total = 0;
            List<WCF.RootService.CycleCountCheckResult> list = cf.CycleCountCheckList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("WhCode", "操作");
           
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("Inv_HuId", "库存托盘");
            fieldsName.Add("Inv_CustomerPoNumber", "库存PO");
            fieldsName.Add("Inv_AltItemNumber", "库存款号");
            fieldsName.Add("Inv_LotNumber1", "库存Lot1");
            fieldsName.Add("Inv_LotNumber2", "库存Lot2");
            fieldsName.Add("Inv_Qty", "库存数量");
            fieldsName.Add("Che_HuId", "实盘托盘");
            fieldsName.Add("Che_Qty", "实盘数量");
            fieldsName.Add("Che_CustomerPoNumber", "实盘PO");
            fieldsName.Add("Che_AltItemNumber", "实盘款号");    
            fieldsName.Add("TaskNumber", "任务号码");

            return Content(EIP.EipListJson(list, total, fieldsName, "WhCode:90,TaskNumber:110,Inv_HuId:80,Inv_CustomerPoNumber:100,Inv_Qty:60,Che_HuId:70,Che_CustomerPoNumber:100,Che_Qty:60,Inv_LotNumber1:60,Inv_LotNumber2:60,default:80"));
        }

        [HttpPost]
        public ActionResult CheckCycleResultSkuByEAN()
        {
            string taskNumber = Request["taskNumber"].Trim();

            string result = cf.CheckCycleResultSkuByEAN(taskNumber, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "验证成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //盘点差异再次创建盘点任务
        [HttpPost]
        public ActionResult CycleCountMasterAddAgain()
        {

            string taskNumber = Request["taskNumber"].Trim();

            string result = cf.CycleCountMasterAddAgain(taskNumber, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "再次生成盘点任务成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //新增盘点任务(按客户和款号)
        [HttpGet]
        public ActionResult CycleCountMasterAddByClientCodeSku()
        {
            WCF.RootService.CycleCountMasterInsert entity = new WCF.RootService.CycleCountMasterInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.Type = Request["sel_Type"];
            entity.CreateUser = Session["userName"].ToString();
            entity.Description = Request["description"].Trim();
            entity.CreateType = Convert.ToInt32(Request["createType"]);
            entity.LocationNullFlag = Convert.ToInt32(Request["sel_LocationNullFlag"]);
            entity.OneByOneScanFlag = Convert.ToInt32(Request["oneByoneScan"]);
            entity.TypeDescription = Request["typeDescription"].Trim();

            entity.CompareStorageLocationHu = Convert.ToInt32(Request["compareStorage"]);

            string clientCode = Request["WhClientList"].Trim();
            string itemNumber = Request["sku_area"];
            string[] itemNumberList = null;
            if (!string.IsNullOrEmpty(itemNumber))
            {
                string item_temp = itemNumber.Replace(",", "@");    //把so中的逗号 替换为@符号
                itemNumberList = item_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string result = cf.CycleCountMasterAddByClientCodeSku(entity, itemNumberList, clientCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }




        //修改实际盘点结果
        [HttpGet]
        public ActionResult EditCycleCheckDetail()
        {
            WCF.RootService.CycleCountCheck oldentity = new WCF.RootService.CycleCountCheck();
            oldentity.WhCode = Session["whCode"].ToString();
            oldentity.TaskNumber = Request["taskNumber"].Trim();
            oldentity.LocationId = Request["locationId"].Trim();
            oldentity.HuId = Request["huId"].Trim();
            oldentity.CustomerPoNumber = Request["poNumber"].Trim();
            oldentity.AltItemNumber = Request["itemNumber"].Trim();
            oldentity.Qty = Convert.ToInt32(Request["qty"]);

            WCF.RootService.CycleCountCheck entity = new WCF.RootService.CycleCountCheck();
            entity.WhCode = Session["whCode"].ToString();
            entity.TaskNumber = Request["taskNumber"].Trim();
            entity.LocationId = Request["locationId"].Trim();
            entity.HuId = Request["edit_HuId"].Trim();
            entity.CustomerPoNumber = Request["edit_PO"].Trim();
            entity.AltItemNumber = Request["edit_Item"].Trim();
            entity.Qty = Convert.ToInt32(Request["edit_Qty"]);

            string result = cf.EditCycleCheckDetail(entity, oldentity, Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败！", null, "");
            }
        }

        //删除实际盘点结果
        [HttpGet]
        public ActionResult DelCycleCheckDetail()
        {
            WCF.RootService.CycleCountCheck oldentity = new WCF.RootService.CycleCountCheck();
            oldentity.WhCode = Session["whCode"].ToString();
            oldentity.TaskNumber = Request["taskNumber"].Trim();
            oldentity.LocationId = Request["locationId"].Trim();
            oldentity.HuId = Request["huId"].Trim();
            oldentity.CustomerPoNumber = Request["poNumber"].Trim();
            oldentity.AltItemNumber = Request["itemNumber"].Trim();
            oldentity.Qty = Convert.ToInt32(Request["qty"]);

            string result = cf.DelCycleCheckDetail(oldentity, Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //添加实际盘点结果
        [HttpGet]
        public ActionResult AddCycleCheckDetail()
        {
            List<WCF.RootService.CycleCountCheck> list = new List<WCF.RootService.CycleCountCheck>();

            WCF.RootService.CycleCountCheck oldentity = new WCF.RootService.CycleCountCheck();
            oldentity.WhCode = Session["whCode"].ToString();
            oldentity.TaskNumber = Request["taskNumber"].Trim();
            oldentity.LocationId = Request["locationId"].Trim();
            oldentity.HuId = Request["edit_HuId"].Trim();
            oldentity.CustomerPoNumber = Request["edit_PO1"].Trim();
            oldentity.AltItemNumber = Request["edit_Item1"].Trim();
            oldentity.Qty = Convert.ToInt32(Request["edit_Qty1"]);
            list.Add(oldentity);

            if (Request["edit_PO2"].Trim() != "" || Request["edit_Item2"].Trim() != "")
            {
                WCF.RootService.CycleCountCheck entity = new WCF.RootService.CycleCountCheck();
                entity.WhCode = Session["whCode"].ToString();
                entity.TaskNumber = Request["taskNumber"].Trim();
                entity.LocationId = Request["locationId"].Trim();
                entity.HuId = Request["edit_HuId"].Trim();
                entity.CustomerPoNumber = Request["edit_PO2"].Trim();
                entity.AltItemNumber = Request["edit_Item2"].Trim();
                entity.Qty = Convert.ToInt32(Request["edit_Qty2"]);
                list.Add(entity);
            }

            string result = cf.AddCycleCheckDetail(list.ToArray(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败！", null, "");
            }
        }

    }
}
