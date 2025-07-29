using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.RootService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“RootService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 RootService.svc 或 RootService.svc.cs，然后开始调试。
    public class RootService : IRootService
    {
        IBLL.IRootManager root = new BLL.RootManager();
        IBLL.IWhAgentService agent = new BLL.WhAgentService();
        IBLL.IWhClientService client = new BLL.WhClientService();

        IBLL.IZoneLocationService zoneLocation = new BLL.ZoneLocationService();
        IBLL.IHoldMasterService holdMaster = new BLL.HoldMasterService();

        IBLL.ISerialNumberInService serial = new BLL.SerialNumberInService();
        IBLL.IFlowDetailService flowDetail = new BLL.FlowDetailService();
        IBLL.IR_Client_FlowRuleService r_client_flowRule = new BLL.R_Client_FlowRuleService();

        IBLL.IR_LoadRule_FlowHeadService r_loadRule_flowHead = new BLL.R_LoadRule_FlowHeadService();
        IBLL.IPhotoMasterService pho = new BLL.PhotoMasterService();
        IBLL.ITCRProcessService tcr = new BLL.TCRProcessService();
        IBLL.ICRTemplateService cr = new BLL.CRTemplateService();
        IBLL.IUrlEdiService urledi = new BLL.UrlEdiService();

        IBLL.IHolidayService holiday = new BLL.HolidayService();
        IBLL.IFeeHolidayService feeholiday = new BLL.FeeHolidayService();
        IBLL.IContractFormService contract = new BLL.ContractFormService();
        IBLL.IZonesExtendService zoneExtend = new BLL.ZonesExtendService();

        IBLL.IHeportSerialNumberInService heportserial = new BLL.HeportSerialNumberInService();

        #region 1.代理管理

        public List<WhAgent> WhAgentList(WhAgentSearch search, out int total)
        {
            return root.WhAgentList(search, out total);
        }

        public WhAgent WhAgentAdd(WhAgent entity)
        {
            return root.WhAgentAdd(entity);
        }


        public int WhAgentEdit(WhAgent entity, params string[] modifiedProNames)
        {
            return root.WhAgentEdit(entity, modifiedProNames);
        }

        #endregion


        #region 2.客户管理


        public List<WhClientResult> WhClientList(WhClientSearch search, out int total)
        {
            return root.WhClientList(search, out total);
        }

        //客户释放规则列表
        public List<WhClientResult> WhClientReleaseRuleList(WhClientSearch searchEntity, out int total)
        {
            return root.WhClientReleaseRuleList(searchEntity, out total);
        }


        public WhClient WhClientAdd(WhClient entity)
        {
            return root.WhClientAdd(entity);
        }

        public int WhClientEdit(WhClient entity, params string[] modifiedProNames)
        {
            return root.WhClientEdit(entity, modifiedProNames);
        }

        public List<WhAgentResult> WhAgentUnselected(WhAgentSearch searchEntity, out int total)
        {
            return root.WhAgentUnselected(searchEntity, out total);
        }

        public List<WhAgentResult> WhAgentSelected(WhAgentSearch searchEntity, out int total)
        {
            return root.WhAgentSelected(searchEntity, out total);
        }

        public int WhAgentWhClientListAdd(List<R_WhClient_WhAgent> entity)
        {
            return root.WhAgentWhClientListAdd(entity);
        }

        public int WhAgentWhClientDel(R_WhClient_WhAgent entity)
        {
            return root.WhAgentWhClientDel(entity);
        }

        //客户异常原因列表
        public List<HoldMaster> HoldMasterListByClient(HoldMasterSearch searchEntity, out int total)
        {
            return root.HoldMasterListByClient(searchEntity, out total);
        }

        //新增客户异常原因
        public HoldMaster HoldMasterAdd(HoldMaster entity)
        {
            return root.HoldMasterAdd(entity);
        }

        //客户异常原因修改
        public int HoldMasterEdit(HoldMaster entity, params string[] modifiedProNames)
        {
            return root.HoldMasterEdit(entity, modifiedProNames);
        }

        //客户异常原因删除
        public int HoldMasterDelById(int id)
        {
            return holdMaster.DeleteById(id);
        }


        //删除收出货流程与RF流程的关系
        public int ClientFlowNameDel(int id)
        {
            return flowDetail.DeleteById(id);
        }

        //根据当前客户查询出未选择的流程
        public List<FlowHeadResult> FlowNameUnselected(FlowHeadSearch searchEntity, out int total)
        {
            return root.FlowNameUnselected(searchEntity, out total);
        }

        //根据当前客户查询出已选择的流程
        public List<FlowHeadResult> FlowNameSelected(FlowHeadSearch searchEntity, out int total)
        {
            return root.FlowNameSelected(searchEntity, out total);
        }

        //新增客户流程关系
        //对应ClientController中的 AddFlowRule 方法
        public int AddFlowRule(List<R_Client_FlowRule> entity)
        {
            return root.AddFlowRule(entity);
        }

        //删除客户与流程之间的关系
        public int ClientFlowRuleDel(int id)
        {
            return r_client_flowRule.DeleteById(id);
        }



        #endregion


        #region 3.储位管理

        public List<WhLocationResult> WhLocationList(WhLocationSearch searchEntity, out int total)
        {
            return root.WhLocationList(searchEntity, out total);
        }

        public string LocationImports(List<LocationResult> entity)
        {
            return root.LocationImports(entity);
        }

        public IEnumerable<WhLocationTypeResult> LocationTypeSelect()
        {
            return root.LocationTypeSelect();
        }

        public IEnumerable<WhZoneResult> ZoneSelect(string whCode)
        {
            return root.ZoneSelect(whCode);
        }

        public IEnumerable<WhLocationResult> LocationSelect(string whCode)
        {
            return root.LocationSelect(whCode);
        }

        public WhLocation LocationAdd(WhLocation entity)
        {
            return root.LocationAdd(entity);
        }


        //按照规则生成储位
        public int AddLocation(string beginLocationArray, string beginLocationColumn, string endLocationColumn, string beginLocationColumn2, string endLocationColumn2, int beginLocationRow, int endLocationRow, int LocationFloor, int LocationPcs, int CheckBegin, string whCode, string userName)
        {
            return root.AddLocation(beginLocationArray, beginLocationColumn, endLocationColumn, beginLocationColumn2, endLocationColumn2, beginLocationRow, endLocationRow, LocationFloor, LocationPcs, CheckBegin, whCode, userName);
        }

        //批量修改储位
        public string LocationEdit(List<WhLocation> list)
        {
            return root.LocationEdit(list);
        }

        //批量删除库位
        public string WhLocationBatchDel(int?[] idarr)
        {
            return root.WhLocationBatchDel(idarr);
        }

        public int WhLocationEdit(WhLocation entity)
        {
            return root.WhLocationEdit(entity);
        }


        //取得默认异常库位
        public string GetWhLocationLookUp(string whCode)
        {
            return root.GetWhLocationLookUp(whCode);
        }

        //异常库位默认设置
        public string SetWhLocationLookUp(string whCode, string abLocationId)
        {
            return root.SetWhLocationLookUp(whCode, abLocationId);
        }

        #endregion


        #region 4.托盘管理

        public List<WhPallateResult> WhPallateList(WhPallateSearch searchEntity, out int total)
        {
            return root.WhPallateList(searchEntity, out total);
        }

        public int WhPallateListAdd(List<Pallate> entity)
        {
            return root.WhPallateListAdd(entity);
        }

        public IEnumerable<WhPallateTypeResult> PallateTypeSelect()
        {
            return root.PallateTypeSelect();
        }

        public string PallateImports(List<WhPallateResult> entity)
        {
            return root.PallateImports(entity);
        }

        public string PallateBatchDel(int?[] idarr)
        {
            return root.PallateBatchDel(idarr);
        }

        #endregion



        #region 5.区域管理

        public List<WhZoneResult> WhZoneList(WhZoneSearch searchEntity, out int total)
        {
            return root.WhZoneList(searchEntity, out total);
        }

        public Zone WhZoneAdd(Zone entity)
        {
            return root.WhZoneAdd(entity);
        }

        public List<ZoneLocationResult> LocationUnselected(ZoneLocationSearch searchEntity, out int total)
        {
            return root.LocationUnselected(searchEntity, out total);
        }

        public List<ZoneLocationResult> LocationSelected(ZoneLocationSearch searchEntity, out int total)
        {
            return root.LocationSelected(searchEntity, out total);
        }

        public int ZoneLocationAdd(List<ZoneLocation> entity)
        {
            return root.ZoneLocationAdd(entity);
        }

        public int ZoneLocationDelById(int Id)
        {
            return root.ZoneLocationDelById(Id);
        }

        //区域父级菜单下拉列表
        public IEnumerable<WhZoneResult> WhZoneParentSelect(string whCode)
        {
            return root.WhZoneParentSelect(whCode);
        }

        //区域信息修改
        public int WhZoneParentEdit(Zone entity, params string[] modifiedProNames)
        {
            return root.WhZoneParentEdit(entity, modifiedProNames);
        }

        //批量删除区域
        public string WhZoneBatchDel(int?[] idarr)
        {
            return root.WhZoneBatchDel(idarr);
        }


        public string ZoneImports(List<ZoneResult> entity)
        {
            return root.ZoneImports(entity);
        }


        #endregion


        #region 6.款号管理

        //款号列表
        public List<WhItemResult> ItemMasterList(WhItemSearch searchEntity, out int total)
        {
            return root.ItemMasterList(searchEntity, out total);
        }

        //批量导入款号
        public string ItemImports(string[] clientCode, string[] altItemNumber, string[] style1, string[] style2, string[] style3, string[] unitName, string whCode, string userName)
        {
            return root.ItemImports(clientCode, altItemNumber, style1, style2, style3, unitName, whCode, userName);
        }

        //批量导入款号品名
        public string ItemImportsItemName(string[] clientCode, string[] altItemNumber, string[] itemName, string whCode, string userName)
        {
            return root.ItemImportsItemName(clientCode, altItemNumber, itemName, whCode, userName);
        }

        //修改款号基础信息
        public string ItemMasterEdit(ItemMaster im)
        {
            return root.ItemMasterEdit(im);
        }


        //OMS款号信息新增或更新接口JSON列表
        public string ItemMasterExtendOMSAdd(List<WhItemExtendOMS> entity)
        {
            return root.ItemMasterExtendOMSAdd(entity);
        }

        #endregion


        #region 7.仓库管理

        //仓库异常原因列表
        public List<HoldMaster> WarehouseHoldMasterList(HoldMasterSearch searchEntity, out int total)
        {
            return root.WarehouseHoldMasterList(searchEntity, out total);
        }


        //箱号采集列表
        public List<SerialNumberInOut> SerialNumberInList(SerialNumberInSearch searchEntity, out int total)
        {
            return root.SerialNumberInList(searchEntity, out total);
        }
        public List<SerialNumberDetailOut> SerialNumberDetailList(SerialNumberDetailSearch searchEntity, out int total)
        {
            return root.SerialNumberDetailList(searchEntity, out total);
        }
        //箱号信息修改
        public int SerialNumberEdit(SerialNumberIn entity)
        {
            return root.SerialNumberEdit(entity);
        }

        //箱号删除
        public int SerialNumberDel(int id)
        {
            SerialNumberIn serialNumberIn = serial.Select(id);
            //检查有没有已经出货的扫描序列号,如果状态是1表示没有,可以删除
            if (serialNumberIn.ToOutStatus == 1)
                return serial.DeleteById(id);
            else
                return 0;

        }

        //批量删除序列号
        public int SerialNumberDelByHuId(SerialNumberIn entity)
        {
            return root.SerialNumberDelByHuId(entity);
        }

        //新增箱号信息
        public int SerialNumberAdd(SerialNumberIn entity)
        {
            return root.SerialNumberAdd(entity);
        }


        //出货箱号采集管理 查询列表
        public List<SerialNumberOut> SerialNumberOutList(SerialNumberOutSearch searchEntity, out int total)
        {
            return root.SerialNumberOutList(searchEntity, out total);
        }


        //出货采集序列号 修改
        public string SerialNumberOutEdit(SerialNumberOut entity)
        {
            return root.SerialNumberOutEdit(entity);
        }

        //出货序列号删除
        public string SerialNumberOutDel(SerialNumberOut entity)
        {
            return root.SerialNumberOutDel(entity);
        }

        //新增出货序列号信息
        public string SerialNumberOutAdd(SerialNumberOut entity)
        {
            return root.SerialNumberOutAdd(entity);
        }

        //新增出货序列号信息
        public string SerialNumberAddOther(SerialNumberOut entity)
        {
            return root.SerialNumberAddOther(entity);
        }


        //收货箱号采集管理 查询列表
        public List<HeportSerialNumberIn> HeportSerialNumberInList(SerialNumberInSearch searchEntity, out int total)
        {
            return root.HeportSerialNumberInList(searchEntity, out total);
        }

        //箱号信息修改
        public int HeportSerialNumberEdit(HeportSerialNumberIn entity)
        {
            return root.HeportSerialNumberEdit(entity);
        }

        //采集箱号删除
        public int HeportSerialNumberDel(int id)
        {
            return heportserial.DeleteById(id);
        }

        //采集箱号按托盘删除
        public int HeportSerialNumberDelByHuId(HeportSerialNumberIn entity)
        {
            return root.HeportSerialNumberDelByHuId(entity);
        }

        //盘点任务列表
        public List<CycleCountMasterResult> CycleCountMasterList(CycleCountMasterSearch searchEntity, out int total)
        {
            return root.CycleCountMasterList(searchEntity, out total);
        }

        //新增盘点任务
        public string CycleCountMasterAdd(CycleCountMasterInsert entity)
        {
            return root.CycleCountMasterAdd(entity);
        }

        //盘点任务明细列表
        public List<CycleCountDetailResult> CycleCountDetailList(CycleCountDetailSearch searchEntity, out int total)
        {
            return root.CycleCountDetailList(searchEntity, out total);
        }

        //删除盘点任务
        public string CycleCountMasterDel(CycleCountMaster entity)
        {
            return root.CycleCountMasterDel(entity);
        }

        //盘点任务开始
        public string CycleCountInsertComplex(CycleCountInsertComplex entity)
        {
            return root.CycleCountInsertComplex(entity);
        }

        //盘点任务结果列表
        public List<CycleCountCheckResult> CycleCountCheckList(CycleCountCheckSearch searchEntity, out int total)
        {
            return root.CycleCountCheckList(searchEntity, out total);
        }

        //盘点任务差异EAN验证款号
        //有差异的 款号匹配EAN验证是否是EAN 如果是EAN 更新盘点结果
        public string CheckCycleResultSkuByEAN(string taskNumber, string whCode, string userName)
        {
            return root.CheckCycleResultSkuByEAN(taskNumber, whCode, userName);
        }

        //盘点任务再次生成
        public string CycleCountMasterAddAgain(string taskNumber, string whCode, string userName)
        {
            return root.CycleCountMasterAddAgain(taskNumber, whCode, userName);
        }

        //创建盘点任务 按照客户和款号
        public string CycleCountMasterAddByClientCodeSku(CycleCountMasterInsert entity, string[] itemNumberList, string clientCode)
        {
            return root.CycleCountMasterAddByClientCodeSku(entity, itemNumberList, clientCode);
        }


        //创建盘点任务 按照库位变更时间
        public string CycleCountMasterAddByLocationChangeTime(CycleCountMasterInsert entity, CycleCountMasterSeacrh searchEntity)
        {
            return root.CycleCountMasterAddByLocationChangeTime(entity, searchEntity);
        }



        //修改实际盘点结果
        public string EditCycleCheckDetail(CycleCountCheck entity, CycleCountCheck oldEntity, string userName)
        {
            return root.EditCycleCheckDetail(entity, oldEntity, userName);
        }

        //添加实际盘点结果
        public string AddCycleCheckDetail(List<CycleCountCheck> entity, string userName)
        {
            return root.AddCycleCheckDetail(entity, userName);
        }

        //删除实际盘点结果
        public string DelCycleCheckDetail(CycleCountCheck entity, string userName)
        {
            return root.DelCycleCheckDetail(entity, userName);
        }

        #endregion


        #region 8.款号单位信息管理
        public List<UnitsResult> UnitsList(UnitsSearch searchEntity, out int total)
        {
            return root.UnitsList(searchEntity, out total);

        }

        public int AddUnit(Unit unit)
        {
            return root.AddUnit(unit);

        }


        public List<ItemUnitResult> ItemUnitList(ItemUnitSearch searchEntity, out int totol)
        {

            return root.ItemUnitList(searchEntity, out totol);


        }
        #endregion


        #region 9.RF流程管理

        //流程列表
        public List<BusinessFlowGroupResult> ClientRFFlowList(BusinessFlowGroupSearch searchEntity, out int total)
        {
            return root.ClientRFFlowList(searchEntity, out total);
        }


        //添加流程
        public BusinessFlowGroup AddClientRFFlow(BusinessFlowGroup entity)
        {
            return root.AddClientRFFlow(entity);
        }

        //流程修改
        public int ClientRFFlowEdit(BusinessFlowGroup entity, params string[] modifiedProNames)
        {
            return root.ClientRFFlowEdit(entity, modifiedProNames);
        }

        //流程明细列表
        public List<BusinessFlowHeadResult> RFFlowRuleDetailList(BusinessFlowHeadSearch searchEntity, out int total)
        {
            return root.RFFlowRuleDetailList(searchEntity, out total);
        }

        //添加流程配置
        public string AddClientFlowDetail(BusinessFlowHeadInsert entity)
        {
            return root.AddClientFlowDetail(entity);
        }

        #endregion


        #region 10.收出货等流程管理

        //流程列表

        public List<FlowHeadResult> ClientFlowRuleList(FlowHeadSearch searchEntity, out int total)
        {
            return root.ClientFlowRuleList(searchEntity, out total);
        }

        //添加流程

        public FlowHead AddClientFlowRule(FlowHead entity)
        {
            return root.AddClientFlowRule(entity);
        }

        //流程修改

        public int ClientFlowRuleEdit(FlowHead entity, params string[] modifiedProNames)
        {
            return root.ClientFlowRuleEdit(entity, modifiedProNames);
        }


        //流程明细列表
        public List<FlowDetailResult> FlowRuleDetailList(FlowDetailSearch searchEntity, out int total)
        {
            return root.FlowRuleDetailList(searchEntity, out total);
        }


        //添加出货流程配置
        public string AddClientOutFlowDetail(FlowHeadInsert entity)
        {
            return root.AddClientOutFlowDetail(entity);
        }

        //根据当前客户查询出未选择的流程
        public List<BusinessFlowGroupResult> ClientFlowNameUnselected(BusinessFlowGroupSearch searchEntity, out int total)
        {
            return root.ClientFlowNameUnselected(searchEntity, out total);
        }

        //根据当前客户查询出已选择的流程
        public List<BusinessFlowGroupResult> ClientFlowNameSelected(BusinessFlowGroupSearch searchEntity, out int total)
        {
            return root.ClientFlowNameSelected(searchEntity, out total);
        }

        //新增客户流程关系
        public int AddClientFlowRuleDetail(List<FlowDetail> entity)
        {
            return root.AddClientFlowRuleDetail(entity);
        }

        //报表字段排序下拉列表
        public IEnumerable<FieldOrderByResult> FieldOrderBySelect(string whCode)
        {
            return root.FieldOrderBySelect(whCode);
        }


        //打印报表名称下拉列表
        public IEnumerable<CRTempResult> TempSelect(string type, string whCode)
        {
            return root.TempSelect(type, whCode);
        }

        #endregion


        #region 11.Load生成规则管理

        public List<LoadCreateRuleResult> GetLoadCreateRuleList(LoadCreateRuleSearch searchEntity, out int total)
        {
            return root.GetLoadCreateRuleList(searchEntity, out total);
        }

        //新增生成规则
        public LoadCreateRule LoadCreateRuleAdd(LoadCreateRule entity)
        {
            return root.LoadCreateRuleAdd(entity);
        }


        //信息修改
        public int LoadCreateRuleEdit(LoadCreateRule entity)
        {
            return root.LoadCreateRuleEdit(entity);
        }



        public List<BusinessFlowGroupResult> LoadCreateFlowNameUnselected(LoadCreateRuleSearch searchEntity, out int total)
        {
            return root.LoadCreateFlowNameUnselected(searchEntity, out total);
        }
        public List<BusinessFlowGroupResult> LoadCreateFlowNameSelected(LoadCreateRuleSearch searchEntity, out int total)
        {
            return root.LoadCreateFlowNameSelected(searchEntity, out total);
        }


        public int R_LoadRule_FlowHeadListAdd(List<R_LoadRule_FlowHead> entity)
        {
            return root.R_LoadRule_FlowHeadListAdd(entity);
        }


        public int R_LoadRule_FlowHeadDel(int id)
        {
            return r_loadRule_flowHead.DeleteById(id);
        }

        //自动生成Load规则名称下拉列表
        public IEnumerable<LoadCreateRule> LoadCreateRuleSelect(string whCode)
        {
            return root.LoadCreateRuleSelect(whCode);
        }

        //自动批量生成Load
        public string BeginLoadCreate(LoadCreateRuleInsert entity)
        {
            return root.BeginLoadCreate(entity);
        }

        //根据客户名、订单来源、创建时间 查询出货流程对应得订单数
        public List<LoadCreateRuleResult> GetOrderQtyList(LoadCreateRuleSearch searchEntity, out int total)
        {
            return root.GetOrderQtyList(searchEntity, out total);
        }

        //出货订单类型下拉列表
        public IEnumerable<LoadCreateRuleInsert> OutBoundOrderSourceSelect(string whCode)
        {
            return root.OutBoundOrderSourceSelect(whCode);
        }

        #endregion


        #region 12.补货任务管理

        //补货信息列表
        public List<R_Location_ItemResult> R_Location_ItemList(R_Location_ItemSearch searchEntity, out int total)
        {
            return root.R_Location_ItemList(searchEntity, out total);
        }

        //新增补货库位信息
        public string R_Location_ItemAdd(List<R_Location_Item> entity)
        {
            return root.R_Location_ItemAdd(entity);
        }

        public int R_Location_ItemEdit(R_Location_Item entity)
        {
            return root.R_Location_ItemEdit(entity);
        }

        //释放补货任务
        public string ReleaseSupplementTask(string whCode, string userName, int count, string[] altItemNumber)
        {
            return root.ReleaseSupplementTask(whCode, userName, count, altItemNumber);
        }

        //释放补货任务 博士定制
        public string BoschReleaseSupplementTask(string whCode, string userName, int count)
        {
            return root.BoschReleaseSupplementTask(whCode, userName, count);
        }


        //补货任务表查询
        public List<SupplementTaskResult> SupplementTaskResultList(SupplementTaskSearch searchEntity, out int total)
        {
            return root.SupplementTaskResultList(searchEntity, out total);
        }


        //补货任务明细查询
        public List<SupplementTaskDetailResult> SupplementTaskDetailResultList(SupplementTaskDetailSearch searchEntity, out int total)
        {
            return root.SupplementTaskDetailResultList(searchEntity, out total);
        }


        //删除补货任务明细
        public string SupplementTaskDetailDel(string whCode, string supplementNumber, string userName)
        {
            return root.SupplementTaskDetailDel(whCode, supplementNumber, userName);
        }

        public List<WhLocationResult> SupplementLocationList(WhLocationSearch searchEntity, out int total)
        {
            return root.SupplementLocationList(searchEntity, out total);
        }


        //批量导入捡货库位
        public int ImportSupplementLocation(List<WhLocation> entity)
        {
            return root.ImportSupplementLocation(entity);
        }


        //批量删除捡货库位
        public string SupplementLocationDel(List<WhLocation> entity)
        {
            return root.SupplementLocationDel(entity);
        }

        //批量删除捡货库位对应款号信息
        public string R_Location_Item_Del(List<R_Location_Item> entity)
        {
            return root.R_Location_Item_Del(entity);
        }

        #endregion


        #region 13.耗材管理

        //耗材列表
        public List<Loss> LossList(LossSearch searchEntity, out int total)
        {
            return root.LossList(searchEntity, out total);
        }

        //新增耗材
        public string LossAdd(Loss entity)
        {
            return root.LossAdd(entity);
        }

        //修改耗材
        public int LossEdit(Loss entity)
        {
            return root.LossEdit(entity);
        }

        //删除耗材
        public int LossDel(int id)
        {
            return root.LossDel(id);
        }

        #endregion


        #region 14.照片管理


        //新增TCR处理方式
        public string TCRProcessModeAdd(TCRProcess entity)
        {
            return root.TCRProcessModeAdd(entity);
        }


        //删除TCR处理方式
        public int TCRProcessModeDel(int id)
        {
            return tcr.DeleteById(id);
        }

        //TCR处理方式列表
        public List<TCRProcessResult> TCRProcessModeList(PhotoMasterSearch searchEntity, out int total)
        {
            return root.TCRProcessModeList(searchEntity, out total);
        }

        //修改TCR信息
        public string PhotoMasterEdit(PhotoMaster entity)
        {
            return root.PhotoMasterEdit(entity);
        }

        //修改TCR信息
        public string PhotoMasterEdit1(PhotoMaster entity)
        {
            return root.PhotoMasterEdit1(entity);
        }

        //处理TCR
        public string EditTCRStatus(PhotoMaster entity)
        {
            return root.EditTCRStatus(entity);
        }


        //新增照片信息
        public string PhotoMasterAdd(List<PhotoMaster> entity, string whCode)
        {
            return root.PhotoMasterAdd(entity, whCode);
        }

        //删除照片信息
        public int PhotoMasterDel(int id)
        {
            return pho.DeleteById(id);
        }

        //CFS收货照片查询
        public List<PhotoMasterResult> InCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total)
        {
            return root.InCFSPhotoMasterList(searchEntity, out total);
        }

        //CFS出货照片查询
        public List<PhotoMasterResult> OutCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total)
        {
            return root.OutCFSPhotoMasterList(searchEntity, out total);
        }

        //照片上传
        public string InCFSPhotoCComplete(PhotoMaster entity)
        {
            return root.InCFSPhotoCComplete(entity);
        }

        //审核照片
        public string CFSPhotoCShenheComplete(PhotoMaster entity)
        {
            return root.CFSPhotoCShenheComplete(entity);
        }

        //出货照片上传
        public string OutCFSPhotoCComplete(PhotoMaster entity)
        {
            return root.OutCFSPhotoCComplete(entity);
        }

        //审核照片百分比
        public decimal CheckCountPercent(PhotoMasterSearch searchEntity)
        {
            return root.CheckCountPercent(searchEntity);
        }


        //TCR类型
        public List<HoldMaster> HoldMasterList(HoldMasterSearch searchEntity, out int total)
        {
            return root.HoldMasterList(searchEntity, out total);
        }

        public string PhotoMasterDelPhotoId(int id, string userName)
        {
            return root.PhotoMasterDelPhotoId(id, userName);
        }

        //删除PhotoMaster照片
        public string PhotoMasterDelById(int id, string userName)
        {
            return root.PhotoMasterDelById(id, userName);
        }

        #endregion


        #region 15.工作量管理

        //收货工作量
        public List<WorkloadAccountResult> InWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total)
        {
            return root.InWorkloadAccountList(searchEntity, out total);
        }

        //出货工作量
        public List<WorkloadAccountResult> OutWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total)
        {
            return root.OutWorkloadAccountList(searchEntity, out total);
        }

        //工人种类
        public List<WorkloadAccountResult> WorkTypeList(string whCode)
        {
            return root.WorkTypeList(whCode);
        }

        //修改工人工号
        public string EditWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.EditWorkloadAccount(entity, userCode);
        }

        //批量修改收货工人
        public string EditWorkloadAccountList(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.EditWorkloadAccountList(entity, userCode);
        }

        //批量删除工人工号
        public string DelWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.DelWorkloadAccount(entity, userCode);
        }

        //新增工人工号
        public string AddWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.AddWorkloadAccount(entity, userCode);
        }

        //修改工人工号
        public string EditWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.EditWorkloadAccount1(entity, userCode);
        }

        //批量修改工人工号
        public string EditWorkloadAccount1List(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.EditWorkloadAccount1List(entity, userCode);
        }

        //批量删除工人工号
        public string DelWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.DelWorkloadAccount1(entity, userCode);
        }

        //新增工人工号
        public string AddWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            return root.AddWorkloadAccount1(entity, userCode);
        }

        #endregion


        #region 16.收出货订单流程修改管理

        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string type)
        {
            return root.ClientFlowNameSelect(whCode, clientId, type);
        }
        public List<ReceiptRegisterResult> GetFlowHeadListByRec(ReceiptRegisterSearch searchEntity, out int total)
        {
            return root.GetFlowHeadListByRec(searchEntity, out total);
        }

        public List<LoadMasterResult> GetFlowHeadListByLoad(LoadMasterSearch searchEntity, out int total)
        {
            return root.GetFlowHeadListByLoad(searchEntity, out total);
        }

        public string EditProcessName(string number, string whCode, int processId, string processName, string type, string userCode)
        {
            return root.EditProcessName(number, whCode, processId, processName, type, userCode);
        }

        #endregion


        #region 17.单据管理

        public List<CRTemplate> GetCRTemplate(CRReportSearch searchEntity, out int total)
        {
            return root.GetCRTemplate(searchEntity, out total);
        }

        public String CrystallReportEdit(CRTemplate entity)
        {
            return root.CrystallReportEdit(entity);
        }

        public string CrystallReportAdd(CRTemplate entity)
        {
            return root.CrystallReportAdd(entity);
        }

        public int CrystallReportdel(int Id)
        {
            return cr.DeleteById(Id);
        }

        #endregion


        #region 18.Edi任务基础数据管理
        //得到仓库下的所有Edi基础名称
        public IEnumerable<UrlEdi> UrlEdiSelect(string whCode)
        {
            return root.UrlEdiSelect(whCode);
        }

        //查询列表
        public List<UrlEdi> UrlEdiList(UrlEdiSearch searchEntity, out int total)
        {
            return root.UrlEdiList(searchEntity, out total);
        }

        //修改
        public String UrlEdiEdit(UrlEdi entity)
        {
            return root.UrlEdiEdit(entity);
        }

        //新增
        public string UrlEdiAdd(UrlEdi entity)
        {
            return root.UrlEdiAdd(entity);
        }

        public int UrlEdidel(int Id)
        {
            return urledi.DeleteById(Id);
        }

        #endregion


        #region 19.highcharts数据管理(分析柱状图)
        //库位使用率

        public List<Highcharts> LocRateList(String WhCode)
        {

            return root.LocRateList(WhCode);

        }

        public List<Highcharts> InvRateList(String WhCode)
        {

            return root.InvRateList(WhCode);

        }

        public List<HighchartClient> ClientInvRateList(String WhCode)
        {
            return root.ClientInvRateList(WhCode);
        }

        #endregion


        #region 20.客户类型管理

        //客户类型查询列表
        public List<WhClientType> WhClientTypeList(WhClientTypeSearch searchEntity, out int total)
        {
            return root.WhClientTypeList(searchEntity, out total);
        }

        //客户类型修改
        public string WhClientTypeEdit(WhClientType entity)
        {
            return root.WhClientTypeEdit(entity);
        }

        //新增客户类型
        public string WhClientTypeAdd(WhClientType entity)
        {
            return root.WhClientTypeAdd(entity);
        }

        //删除客户类型
        public int WhClientTypeDel(int id)
        {
            return root.WhClientTypeDel(id);
        }

        //客户类型下拉菜单列表
        public IEnumerable<WhClientType> WhClientTypeListSelect(string whCode)
        {
            return root.WhClientTypeListSelect(whCode);
        }

        #endregion


        #region 21.夜班区间管理

        //客户类型查询列表
        public List<NightTime> NightTimeList(NightTimeSearch searchEntity, out int total)
        {
            return root.NightTimeList(searchEntity, out total);
        }

        //新增夜班区间
        public string NightTimeAdd(NightTime entity)
        {
            return root.NightTimeAdd(entity);
        }

        //删除夜班区间
        public int NightTimeDel(int id)
        {
            return root.NightTimeDel(id);
        }

        //修改夜班区间
        public string NightTimeEdit(NightTime entity)
        {
            return root.NightTimeEdit(entity);

        }

        //夜班区间下拉菜单列表
        public IEnumerable<NightTime> NightTimeListSelect(string whCode)
        {
            return root.NightTimeListSelect(whCode);
        }

        #endregion


        #region 22.道口收费节假日管理


        public List<Holiday> HolidayList(HolidaySearch searchEntity, out int total)
        {
            return root.HolidayList(searchEntity, out total);
        }


        public string HolidayImports(string[] holiday, string[] dayBegin, string whCode, string userName)
        {
            return root.HolidayImports(holiday, dayBegin, whCode, userName);
        }

        //修改信息
        public string HolidayEdit(Holiday entity)
        {
            return root.HolidayEdit(entity);
        }

        public int HolidayDel(int id)
        {
            return holiday.DeleteById(id);
        }

        #endregion


        #region 23.收货合同管理

        //查询
        public List<ContractForm> ContractFormList(ContractFormSearch searchEntity, out int total)
        {
            return root.ContractFormList(searchEntity, out total);
        }

        //合同导入
        public string ContractFormImports(List<ContractForm> entityList)
        {
            return root.ContractFormImports(entityList);
        }

        //删除
        public int ContractFormDeleteAll(string contractName, string whCode, string userName)
        {
            return root.ContractFormDeleteAll(contractName, whCode, userName);
        }

        public int ContractFormDel(int id)
        {
            return contract.DeleteById(id);
        }

        //修改信息
        public string ContractFormEdit(ContractForm entity)
        {
            return root.ContractFormEdit(entity);
        }

        //合同下拉菜单列表
        public IEnumerable<string> ContractNameListSelect(string whCode)
        {
            return root.ContractNameListSelect(whCode);
        }

        #endregion


        #region 24.TCR费用管理

        //查询
        public List<FeeMaster> FeeMaseterList(FeeMasterSearch searchEntity, string[] soNumberList, out int total, out string str)
        {
            return root.FeeMaseterList(searchEntity, soNumberList, out total, out str);
        }

        //添加正常TCR费用
        public FeeMaster FeeMaseterAdd(FeeMaster entity)
        {
            return root.FeeMaseterAdd(entity);
        }

        //添加收货特殊费用
        public string FeeMaseterSpecialAdd(FeeMaster entity, int type)
        {
            return root.FeeMaseterSpecialAdd(entity, type);
        }

        //添加出货提箱特殊费用
        public string FeeMasterOutLoadAdd(FeeMaster entity)
        {
            return root.FeeMasterOutLoadAdd(entity);
        }

        //客服确认状态未预收款
        public string FeeMaseterEditStatus(FeeMaster entity)
        {
            return root.FeeMaseterEditStatus(entity);
        }

        //客服撤销确认状态未预收款
        public string FeeMaseterEditStatus1(FeeMaster entity)
        {
            return root.FeeMaseterEditStatus1(entity);
        }

        //客服订单确认作废 
        public string FeeMaseterEditStatus2(FeeMaster entity)
        {
            return root.FeeMaseterEditStatus2(entity);
        }

        //修改信息
        public string FeeMaseterEdit(FeeMaster entity)
        {
            return root.FeeMaseterEdit(entity);
        }


        //修改费用备注
        public string FeeMasterRemarkEdit(FeeMaster entity)
        {
            return root.FeeMasterRemarkEdit(entity);
        }

        //删除全部费用信息
        public string FeeMaseterDel(string feeNumber, string whCode, string userName)
        {
            return root.FeeMaseterDel(feeNumber, whCode, userName);
        }


        //添加费用明细
        public string FeeDetailAdd(FeeDetail entity)
        {
            return root.FeeDetailAdd(entity);
        }

        //删除费用明细
        public string FeeDetailDel(int id, string userName)
        {
            return root.FeeDetailDel(id, userName);
        }

        //费用明细列表
        public List<FeeDetail> FeeDetailList(FeeDetailSearch searchEntity, out int total, out string str)
        {
            return root.FeeDetailList(searchEntity, out total, out str);
        }


        //修改费用明细信息
        public string FeeDetailEdit(FeeDetail entity)
        {
            return root.FeeDetailEdit(entity);
        }

        //修改信息
        public string FeeMaseterDKEdit(FeeMaster entity)
        {
            return root.FeeMaseterDKEdit(entity);
        }

        //修改费用明细信息
        public string FeeDetailCKEdit(FeeDetail entity)
        {
            return root.FeeDetailCKEdit(entity);
        }

        //仓库修改操作时间
        public string FeeMasterCKEditBeginEndDate(FeeMaster entity)
        {
            return root.FeeMasterCKEditBeginEndDate(entity);
        }

        //仓库确认状态
        public string ConfirmFeeMasterCK(FeeMaster entity)
        {
            return root.ConfirmFeeMasterCK(entity);
        }

        //仓库添加耗材费用明细
        public string FeeDetailAddCKLoss(FeeDetail entity)
        {
            return root.FeeDetailAddCKLoss(entity);
        }

        //删除费用明细
        public string FeeDetailDelCKLoss(int id, string userName)
        {
            return root.FeeDetailDelCKLoss(id, userName);
        }

        //得到实际操作费用
        public string getOperationFee(string feeNumber, string whCode)
        {
            return root.getOperationFee(feeNumber, whCode);
        }

        //得到实际操作费用列表 
        public List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total)
        {
            return root.getOperationFeeList(feeNumber, whCode, out total);
        }

        //道口费用结算
        public string FeeMaseterDKJiesuanEdit(FeeMaster entity, int type)
        {
            return root.FeeMaseterDKJiesuanEdit(entity, type);
        }

        //道口修改发票信息
        public string FeeMaseterInvoiceEdit(FeeMaster entity)
        {
            return root.FeeMaseterInvoiceEdit(entity);
        }

        //查询库存TCR且不在费用明细内
        public List<FeeDetailHuDetailListResult> HuDetailListByPOSKU(FeeDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] huList, out int total, out string str)
        {
            return root.HuDetailListByPOSKU(searchEntity, soList, poList, skuList, huList, out total, out str);
        }

        //添加费用明细
        public string FeeDetailAddList(List<FeeDetail> entityList)
        {
            return root.FeeDetailAddList(entityList);
        }

        //查询库存TCR根据托盘条件检索
        public FeeDetailHuDetailListResult GetHuDetailByHuId(string huId, string whCode, string clientCode)
        {
            return root.GetHuDetailByHuId(huId, whCode, clientCode);
        }

        //重新计算TCR费用
        public string AgainTCRFeeCalculate(FeeMaster entity)
        {
            return root.AgainTCRFeeCalculate(entity);
        }

        #endregion


        #region 25.TCR收费节假日管理

        //查询
        public List<FeeHoliday> FeeHolidayList(HolidaySearch searchEntity, out int total)
        {
            return root.FeeHolidayList(searchEntity, out total);
        }

        //节假日导入
        public string FeeHolidayImports(string[] holiday, string[] dayBegin, string[] type, string whCode, string userName)
        {
            return root.FeeHolidayImports(holiday, dayBegin, type, whCode, userName);
        }

        //修改信息
        public string FeeHolidayEdit(FeeHoliday entity)
        {
            return root.FeeHolidayEdit(entity);
        }

        public int FeeHolidayDel(int id)
        {
            return feeholiday.DeleteById(id);
        }

        #endregion


        #region 26.DamcoGrnRule管理


        //DamcoGrnRule查询列表
        public List<DamcoGrnRule> DamcoGrnRuleList(DamcoGrnRuleSearch searchEntity, out int total)
        {
            return root.DamcoGrnRuleList(searchEntity, out total);
        }

        //新增
        public string DamcoGrnRuleAdd(DamcoGrnRule entity)
        {
            return root.DamcoGrnRuleAdd(entity);
        }

        //删除
        public int DamcoGrnRuleDel(int id)
        {
            return root.DamcoGrnRuleDel(id);
        }

        //修改
        public string DamcoGrnRuleEdit(DamcoGrnRule entity)
        {
            return root.DamcoGrnRuleEdit(entity);
        }

        #endregion


        #region 27.出货合同管理

        //查询
        public List<ContractFormOut> ContractFormOutList(ContractFormOutSearch searchEntity, out int total)
        {
            return root.ContractFormOutList(searchEntity, out total);
        }

        //合同导入
        public string ContractFormOutImports(List<ContractFormOut> entityList)
        {
            return root.ContractFormOutImports(entityList);
        }

        //删除
        public int ContractFormOutDeleteAll(string contractName, string whCode)
        {
            return root.ContractFormOutDeleteAll(contractName, whCode);
        }


        //修改信息
        public string ContractFormOutEdit(ContractFormOut entity)
        {
            return root.ContractFormOutEdit(entity);
        }

        //合同下拉菜单列表
        public IEnumerable<string> ContractFormOutListSelect(string whCode)
        {
            return root.ContractFormOutListSelect(whCode);
        }


        #endregion


        #region 28.账单管理

        //账单查询
        public List<BillMaster> BillMasterList(BillMasterSearch searchEntity, out int total)
        {
            return root.BillMasterList(searchEntity, out total);
        }

        //添加账单
        public BillMaster BillMasterAdd(BillMaster entity)
        {
            return root.BillMasterAdd(entity);
        }

        //查询Load列表 显示是否已确认费用状态
        public List<BillDetailResult> GetLoadMasterList(BillDetailSearch searchEntity, out int total, string[] loadIdList, string[] containerNumberList)
        {
            return root.GetLoadMasterList(searchEntity, out total, loadIdList, containerNumberList);
        }

        //根据Load号查询出货箱单费用并添加至账单
        public string BillDetailAdd(string whCode, string billNumber, string userName, string[] loadId)
        {
            return root.BillDetailAdd(whCode, billNumber, userName, loadId);
        }


        //显示装箱进港费明细
        public List<BillDetailRepostResult> DamcoBillDetailList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total)
        {
            return root.DamcoBillDetailList(searchEntity, chargeName, clientCode, clientCodeNotIn, out total);
        }


        //删除账单
        public string BillFeeDetailDelAll(string whCode, string billNumber)
        {
            return root.BillFeeDetailDelAll(whCode, billNumber);
        }

        //根据Load号删除账单明细，并撤销费用状态
        public string BillDetailDelByLoad(string whCode, string billNumber, string userName, string loadId)
        {
            return root.BillDetailDelByLoad(whCode, billNumber, userName, loadId);
        }

        //修改账单状态
        public string BillMasterEdit(string whCode, string billNumber, string status)
        {
            return root.BillMasterEdit(whCode, billNumber, status);
        }

        //根据SO号查询出货SO特费并添加至账单
        public string BillDetailAddSO(string whCode, string billNumber, string userName, int[] idList)
        {
            return root.BillDetailAddSO(whCode, billNumber, userName, idList);
        }

        //根据Id删除账单明细，并撤销费用状态
        public string BillDetailDelBySO(string whCode, string billNumber, string userName, int id)
        {
            return root.BillDetailDelBySO(whCode, billNumber, userName, id);
        }

        //丹马士SO账单显示
        public List<BillDetailRepostResult> DamcoBillDetailSOList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total)
        {
            return root.DamcoBillDetailSOList(searchEntity, chargeName, clientCode, clientCodeNotIn, out total);
        }

        #endregion


        #region 29.区域与库位拓展管理

        //区域与库位扩展列表
        public List<ZoneExtendResult> ZonesExtendList(WhZoneSearch searchEntity, out int total)
        {
            return root.ZonesExtendList(searchEntity, out total);
        }

        //新增区域扩展
        public ZonesExtend ZonesExtendAdd(ZonesExtend entity)
        {
            return root.ZonesExtendAdd(entity);
        }

        //区域扩展信息修改
        public int ZonesExtendEdit(ZonesExtend entity)
        {
            return root.ZonesExtendEdit(entity);
        }

        public int ZonesExtendDel(int id)
        {
            return zoneExtend.DeleteById(id);
        }

        public string ZonesExtendBatchDel(int?[] idarr)
        {
            return root.ZonesExtendBatchDel(idarr);
        }

        public string ZoneExtendImports(List<ZoneExtendResult> entity)
        {
            return root.ZoneExtendImports(entity);
        }

        #endregion


        #region 30.客户扩展管理

        //列表
        public List<WhClientExtendResult> WhClientExtendList(WhClientSearch searchEntity, out int total)
        {
            return root.WhClientExtendList(searchEntity, out total);
        }

        //新增
        public WhClientExtend WhClientExtendAdd(WhClientExtend entity)
        {
            return root.WhClientExtendAdd(entity);
        }

        //修改
        public int WhClientExtendEdit(WhClientExtend entity)
        {
            return root.WhClientExtendEdit(entity);
        }

        //批量删除
        public string WhClientExtendBatchDel(int?[] idarr)
        {
            return root.WhClientExtendBatchDel(idarr);
        }


        #endregion


        #region 31.部分收货原因登记管理

        //列表
        public List<ReceiptPartialRegisterResult> ReceiptPartialRegisterList(ReceiptPartialRegisterSearch searchEntity, out int total)
        {
            return root.ReceiptPartialRegisterList(searchEntity, out total);
        }

        public List<ReceiptPartialUnReceiptResult> ReceiptPartialUnReceiptList(ReceiptPartialUnPreceiptSearch searchEntity, out int total)
        {
            return root.ReceiptPartialUnReceiptList(searchEntity, out total);
        }
        public List<ReceiptPartialRegisteredDetailResult> RegisteredList(ReceiptPartialUnPreceiptSearch searchEntity, out int total)
        {
            return root.RegisteredList(searchEntity, out total);
        }



        public string UnReceiptRegister(ReceiptPartialRegisterDetail entity, int UnQty)
        {
            return root.UnReceiptRegister(entity, UnQty);
        }

        public string ReceiptPartialDeleteDetail(int Id, string rediptId, string WhCode)
        {
            return root.ReceiptPartialDeleteDetail(Id, rediptId, WhCode);
        }

        public string ReceiptPartialComplete(string ReceiptId, string WhCode)
        {
            return root.ReceiptPartialComplete(ReceiptId, WhCode);
        }

        public string ReceiptPartialReBack(string ReceiptId, string WhCode)
        {
            return root.ReceiptPartialReBack(ReceiptId, WhCode);
        }


        //拒收登记异常原因下拉列表
        public List<HoldMaster> HoldMasterListByReceiptPart(HoldMasterSearch searchEntity)
        {
            return root.HoldMasterListByReceiptPart(searchEntity);
        }

        //拒收登记照片上传
        public string ReceiptPartPhotoUpload(ReceiptPartialRegister entity)
        {
            return root.ReceiptPartPhotoUpload(entity);
        }


        #endregion



        #region 32.收货收费特殊项目收费管理

        //查询
        public List<ContractFormExtend> ContractFormExtendList(ContractFormExtendSearch searchEntity, out int total)
        {
            return root.ContractFormExtendList(searchEntity, out total);
        }

        //修改信息
        public string ContractFormExtendEdit(ContractFormExtend entity)
        {
            return root.ContractFormExtendEdit(entity);
        }


        #endregion


        #region 33.托盘称重管理

        //列表查询
        public List<HuMasterResult1> HuMasterHeavyPalletList(HuMasterSearch1 searchEntity, string[] soNumber, out int total)
        {
            return root.HuMasterHeavyPalletList(searchEntity, soNumber, out total);
        }

        //托盘修改长宽高
        public string HuMasterHeavyPalletEdit(HuMasterResult1 entity)
        {
            return root.HuMasterHeavyPalletEdit(entity);
        }

        #endregion


        #region 34.款号Code管理
        //款号Code列表
        public List<ItemMasterColorCode> ItemMasterColorCodeList(WhItemSearch searchEntity, out int total)
        {
            return root.ItemMasterColorCodeList(searchEntity, out total);
        }


        //批量导入款号Code
        public string ItemMasterColorCodeImports(string[] clientCode, string[] colorCode, string[] colorDescription, string whCode, string userName)
        {
            return root.ItemMasterColorCodeImports(clientCode, colorCode, colorDescription, whCode, userName);
        }


        public string ItemColorCodeEdit(ItemMasterColorCode im)
        {
            return root.ItemColorCodeEdit(im);
        }

        #endregion


        #region 35.退货上架库位管理


        public List<WhLocationResult> ReturnGoodLocationList(WhLocationSearch searchEntity, out int total)
        {
            return root.ReturnGoodLocationList(searchEntity, out total);
        }

        public int ImportReturnGoodLocation(List<WhLocation> entity)
        {
            return root.ImportReturnGoodLocation(entity);
        }


        //批量删除退货上架库位
        public string ReturnGoodLocationDel(List<WhLocation> entity)
        {
            return root.ReturnGoodLocationDel(entity);
        }


        //退货上架库位款号信息列表
        public List<R_Location_ItemResult> R_Location_ItemRGList(R_Location_ItemSearch searchEntity, out int total)
        {
            return root.R_Location_ItemRGList(searchEntity, out total);
        }

        //新增退货上架库位信息
        public string R_Location_ItemRGAdd(List<R_Location_Item_RG> entity)
        {
            return root.R_Location_ItemRGAdd(entity);
        }

        public int R_Location_ItemRGEdit(R_Location_Item_RG entity)
        {
            return root.R_Location_ItemRGEdit(entity);
        }

        //批量删除信息
        public string R_Location_Item_RG_Del(List<R_Location_Item_RG> entity)
        {
            return root.R_Location_Item_RG_Del(entity);
        }

        #endregion
    }
}
