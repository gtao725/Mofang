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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IRootService”。
    [ServiceContract]
    public interface IRootService
    {

        #region 1.代理管理

        [OperationContract]
        List<WhAgent> WhAgentList(WhAgentSearch search, out int total);

        [OperationContract]
        WhAgent WhAgentAdd(WhAgent entity);

        [OperationContract]
        int WhAgentEdit(WhAgent entity, params string[] modifiedProNames);

        #endregion


        #region 2.客户管理

        [OperationContract]
        List<WhClientResult> WhClientList(WhClientSearch search, out int total);

        [OperationContract]
        //客户释放规则列表
        List<WhClientResult> WhClientReleaseRuleList(WhClientSearch searchEntity, out int total);

        [OperationContract]
        WhClient WhClientAdd(WhClient entity);

        [OperationContract]
        int WhClientEdit(WhClient entity, params string[] modifiedProNames);

        [OperationContract]
        List<WhAgentResult> WhAgentUnselected(WhAgentSearch searchEntity, out int total);

        [OperationContract]
        List<WhAgentResult> WhAgentSelected(WhAgentSearch searchEntity, out int total);

        [OperationContract]
        int WhAgentWhClientListAdd(List<R_WhClient_WhAgent> entity);

        [OperationContract]
        int WhAgentWhClientDel(R_WhClient_WhAgent entity);

        [OperationContract]
        //客户异常原因列表
        List<HoldMaster> HoldMasterListByClient(HoldMasterSearch searchEntity, out int total);

        [OperationContract]
        //新增客户异常原因
        HoldMaster HoldMasterAdd(HoldMaster entity);

        [OperationContract]
        //客户异常原因修改
        int HoldMasterEdit(HoldMaster entity, params string[] modifiedProNames);

        [OperationContract]
        //客户异常原因删除
        int HoldMasterDelById(int id);


        [OperationContract]
        //删除收出货流程与RF的流程关系
        int ClientFlowNameDel(int id);

        [OperationContract]
        //根据当前客户查询出未选择的流程
        List<FlowHeadResult> FlowNameUnselected(FlowHeadSearch searchEntity, out int total);

        [OperationContract]
        //根据当前客户查询出已选择的流程
        List<FlowHeadResult> FlowNameSelected(FlowHeadSearch searchEntity, out int total);

        [OperationContract]
        //新增客户流程关系
        //对应ClientController中的 AddFlowRule 方法
        int AddFlowRule(List<R_Client_FlowRule> entity);


        //删除客户与流程之间的关系
        [OperationContract]
        int ClientFlowRuleDel(int id);



        #endregion


        #region 3.储位管理

        [OperationContract]
        List<WhLocationResult> WhLocationList(WhLocationSearch searchEntity, out int total);

        [OperationContract]
        string LocationImports(List<LocationResult> entity);

        [OperationContract]
        IEnumerable<WhLocationTypeResult> LocationTypeSelect();

        [OperationContract]
        IEnumerable<WhZoneResult> ZoneSelect(string whCode);

        [OperationContract]
        IEnumerable<WhLocationResult> LocationSelect(string whCode);

        [OperationContract]
        WhLocation LocationAdd(WhLocation entity);

        [OperationContract]
        //按照规则生成储位
        int AddLocation(string beginLocationArray, string beginLocationColumn, string endLocationColumn, string beginLocationColumn2, string endLocationColumn2, int beginLocationRow, int endLocationRow, int LocationFloor, int LocationPcs, int CheckBegin, string whCode, string userName);


        [OperationContract]
        //批量修改储位
        string LocationEdit(List<WhLocation> list);

        [OperationContract]
        //批量删除库位
        string WhLocationBatchDel(int?[] idarr);

        [OperationContract]
        int WhLocationEdit(WhLocation entity);

        [OperationContract]
        //取得默认异常库位
        string GetWhLocationLookUp(string whCode);

        [OperationContract]
        //异常库位默认设置
        string SetWhLocationLookUp(string whCode, string abLocationId);

        #endregion


        #region 4.托盘管理

        [OperationContract]
        List<WhPallateResult> WhPallateList(WhPallateSearch searchEntity, out int total);

        [OperationContract]
        int WhPallateListAdd(List<Pallate> entity);

        [OperationContract]
        IEnumerable<WhPallateTypeResult> PallateTypeSelect();

        [OperationContract]
        string PallateImports(List<WhPallateResult> entity);

        [OperationContract]
        string PallateBatchDel(int?[] idarr);


        #endregion


        #region 5.区域管理

        [OperationContract]
        List<WhZoneResult> WhZoneList(WhZoneSearch searchEntity, out int total);

        [OperationContract]
        Zone WhZoneAdd(Zone entity);

        [OperationContract]
        List<ZoneLocationResult> LocationUnselected(ZoneLocationSearch searchEntity, out int total);

        [OperationContract]
        List<ZoneLocationResult> LocationSelected(ZoneLocationSearch searchEntity, out int total);


        [OperationContract]
        int ZoneLocationAdd(List<ZoneLocation> entity);

        [OperationContract]
        int ZoneLocationDelById(int Id);

        [OperationContract]
        //区域父级菜单下拉列表
        IEnumerable<WhZoneResult> WhZoneParentSelect(string whCode);


        [OperationContract]
        //区域信息修改
        int WhZoneParentEdit(Zone entity, params string[] modifiedProNames);

        [OperationContract]
        //批量删除区域
        string WhZoneBatchDel(int?[] idarr);

        [OperationContract]
        string ZoneImports(List<ZoneResult> entity);

        #endregion


        #region 6.款号管理

        [OperationContract]
        //款号列表
        List<WhItemResult> ItemMasterList(WhItemSearch searchEntity, out int total);

        [OperationContract]
        //批量导入款号
        string ItemImports(string[] clientCode, string[] altItemNumber, string[] style1, string[] style2, string[] style3, string[] unitName, string whCode, string userName);

        [OperationContract]
        //批量导入款号品名
        string ItemImportsItemName(string[] clientCode, string[] altItemNumber, string[] itemName, string whCode, string userName);

        [OperationContract]
        //修改款号基础信息
        string ItemMasterEdit(ItemMaster im);


        //OMS款号信息新增或更新接口JSON列表
        [OperationContract]
        string ItemMasterExtendOMSAdd(List<WhItemExtendOMS> entity);

        #endregion


        #region 7.仓库管理

        [OperationContract]
        //仓库异常原因列表
        List<HoldMaster> WarehouseHoldMasterList(HoldMasterSearch searchEntity, out int total);



        [OperationContract]
        //箱号采集列表
        List<SerialNumberInOut> SerialNumberInList(SerialNumberInSearch searchEntity, out int total);

        [OperationContract]
        List<SerialNumberDetailOut> SerialNumberDetailList(SerialNumberDetailSearch searchEntity, out int total);
        [OperationContract]
        //箱号信息修改
        int SerialNumberEdit(SerialNumberIn entity);

        [OperationContract]
        //箱号删除
        int SerialNumberDel(int id);

        [OperationContract]
        //批量删除序列号
        int SerialNumberDelByHuId(SerialNumberIn entity);

        [OperationContract]
        //新增箱号信息
        int SerialNumberAdd(SerialNumberIn entity);

        [OperationContract]
        //出货箱号采集管理 查询列表
        List<SerialNumberOut> SerialNumberOutList(SerialNumberOutSearch searchEntity, out int total);

        [OperationContract]
        //出货采集序列号 修改
        string SerialNumberOutEdit(SerialNumberOut entity);

        [OperationContract]
        //出货序列号删除
        string SerialNumberOutDel(SerialNumberOut entity);

        [OperationContract]
        //新增出货序列号信息
        string SerialNumberOutAdd(SerialNumberOut entity);

        [OperationContract]
        //新增出货序列号信息
        string SerialNumberAddOther(SerialNumberOut entity);


        [OperationContract]
        //收货箱号采集管理 查询列表
        List<HeportSerialNumberIn> HeportSerialNumberInList(SerialNumberInSearch searchEntity, out int total);

        [OperationContract]
        //箱号信息修改
        int HeportSerialNumberEdit(HeportSerialNumberIn entity);

        [OperationContract]
        //箱号删除
        int HeportSerialNumberDel(int id);

        [OperationContract]
        int HeportSerialNumberDelByHuId(HeportSerialNumberIn entity);



        [OperationContract]
        //盘点任务列表
        List<CycleCountMasterResult> CycleCountMasterList(CycleCountMasterSearch searchEntity, out int total);

        [OperationContract]
        //新增盘点任务
        string CycleCountMasterAdd(CycleCountMasterInsert entity);

        [OperationContract]
        //盘点任务明细列表
        List<CycleCountDetailResult> CycleCountDetailList(CycleCountDetailSearch searchEntity, out int total);

        [OperationContract]
        //删除盘点任务
        string CycleCountMasterDel(CycleCountMaster entity);

        [OperationContract]
        //盘点任务开始
        string CycleCountInsertComplex(CycleCountInsertComplex entity);

        [OperationContract]
        //盘点任务结果列表
        List<CycleCountCheckResult> CycleCountCheckList(CycleCountCheckSearch searchEntity, out int total);

        [OperationContract]
        //盘点任务差异EAN验证款号
        //有差异的 款号匹配EAN验证是否是EAN 如果是EAN 更新盘点结果
        string CheckCycleResultSkuByEAN(string taskNumber, string whCode, string userName);

        [OperationContract]
        //盘点任务再次生成
        string CycleCountMasterAddAgain(string taskNumber, string whCode, string userName);

        [OperationContract]
        //创建盘点任务 按照客户和款号
        string CycleCountMasterAddByClientCodeSku(CycleCountMasterInsert entity, string[] itemNumberList, string clientCode);

        [OperationContract]
        //创建盘点任务 按照库位变更时间
        string CycleCountMasterAddByLocationChangeTime(CycleCountMasterInsert entity, CycleCountMasterSeacrh searchEntity);

        [OperationContract]
        //修改实际盘点结果
        string EditCycleCheckDetail(CycleCountCheck entity, CycleCountCheck oldEntity, string userName);

        [OperationContract]
        //添加实际盘点结果
        string AddCycleCheckDetail(List<CycleCountCheck> entity, string userName);

        [OperationContract]
        //删除实际盘点结果
        string DelCycleCheckDetail(CycleCountCheck entity, string userName);

        #endregion


        #region 8.款号单位信息管理

        [OperationContract]
        //仓库异常原因列表
        List<UnitsResult> UnitsList(UnitsSearch searchEntity, out int total);

        [OperationContract]
        //添加款号
        int AddUnit(Unit unit);


        [OperationContract]
        List<ItemUnitResult> ItemUnitList(ItemUnitSearch searchEntity, out int total);

        #endregion


        #region 9.RF流程管理

        [OperationContract]
        //流程列表
        List<BusinessFlowGroupResult> ClientRFFlowList(BusinessFlowGroupSearch searchEntity, out int total);

        [OperationContract]
        //添加流程
        BusinessFlowGroup AddClientRFFlow(BusinessFlowGroup entity);

        [OperationContract]
        //流程修改
        int ClientRFFlowEdit(BusinessFlowGroup entity, params string[] modifiedProNames);

        [OperationContract]
        //流程明细列表
        List<BusinessFlowHeadResult> RFFlowRuleDetailList(BusinessFlowHeadSearch searchEntity, out int total);

        [OperationContract]
        //添加流程配置
        string AddClientFlowDetail(BusinessFlowHeadInsert entity);


        #endregion


        #region 10.收出货等流程管理

        [OperationContract]
        //流程列表
        List<FlowHeadResult> ClientFlowRuleList(FlowHeadSearch searchEntity, out int total);

        [OperationContract]
        //添加流程
        FlowHead AddClientFlowRule(FlowHead entity);

        [OperationContract]
        //流程修改
        int ClientFlowRuleEdit(FlowHead entity, params string[] modifiedProNames);

        [OperationContract]
        //流程明细列表
        List<FlowDetailResult> FlowRuleDetailList(FlowDetailSearch searchEntity, out int total);

        [OperationContract]
        //添加出货流程配置
        string AddClientOutFlowDetail(FlowHeadInsert entity);

        [OperationContract]
        //根据当前客户查询出未选择的流程
        List<BusinessFlowGroupResult> ClientFlowNameUnselected(BusinessFlowGroupSearch searchEntity, out int total);

        [OperationContract]
        //根据当前客户查询出已选择的流程
        List<BusinessFlowGroupResult> ClientFlowNameSelected(BusinessFlowGroupSearch searchEntity, out int total);

        [OperationContract]
        //新增客户流程关系
        int AddClientFlowRuleDetail(List<FlowDetail> entity);

        [OperationContract]
        //报表字段排序下拉列表
        IEnumerable<FieldOrderByResult> FieldOrderBySelect(string whCode);

        [OperationContract]
        //打印报表名称下拉列表
        IEnumerable<CRTempResult> TempSelect(string type, string whCode);

        #endregion


        #region 11.Load生成规则管理

        [OperationContract]
        List<LoadCreateRuleResult> GetLoadCreateRuleList(LoadCreateRuleSearch searchEntity, out int total);

        [OperationContract]
        //新增生成规则
        LoadCreateRule LoadCreateRuleAdd(LoadCreateRule entity);

        [OperationContract]
        //信息修改
        int LoadCreateRuleEdit(LoadCreateRule entity);

        [OperationContract]
        List<BusinessFlowGroupResult> LoadCreateFlowNameUnselected(LoadCreateRuleSearch searchEntity, out int total);

        [OperationContract]
        List<BusinessFlowGroupResult> LoadCreateFlowNameSelected(LoadCreateRuleSearch searchEntity, out int total);

        [OperationContract]
        int R_LoadRule_FlowHeadListAdd(List<R_LoadRule_FlowHead> entity);

        [OperationContract]
        int R_LoadRule_FlowHeadDel(int id);

        [OperationContract]
        //自动生成Load规则名称下拉列表
        IEnumerable<LoadCreateRule> LoadCreateRuleSelect(string whCode);

        [OperationContract]
        //自动批量生成Load
        string BeginLoadCreate(LoadCreateRuleInsert entity);

        [OperationContract]
        //根据客户名、订单来源、创建时间 查询出货流程对应得订单数
        List<LoadCreateRuleResult> GetOrderQtyList(LoadCreateRuleSearch searchEntity, out int total);

        [OperationContract]
        //出货订单类型下拉列表
        IEnumerable<LoadCreateRuleInsert> OutBoundOrderSourceSelect(string whCode);

        #endregion


        #region 12.补货任务管理

        [OperationContract]
        //补货信息列表
        List<R_Location_ItemResult> R_Location_ItemList(R_Location_ItemSearch searchEntity, out int total);

        [OperationContract]
        //新增补货库位信息
        string R_Location_ItemAdd(List<R_Location_Item> entity);

        [OperationContract]
        int R_Location_ItemEdit(R_Location_Item entity);

        [OperationContract]
        //释放补货任务
        string ReleaseSupplementTask(string whCode, string userName, int count, string[] altItemNumber);

        [OperationContract]
        //释放补货任务 博士定制
        string BoschReleaseSupplementTask(string whCode, string userName, int count);

        [OperationContract]
        //补货任务表查询
        List<SupplementTaskResult> SupplementTaskResultList(SupplementTaskSearch searchEntity, out int total);

        [OperationContract]
        //补货任务明细查询
        List<SupplementTaskDetailResult> SupplementTaskDetailResultList(SupplementTaskDetailSearch searchEntity, out int total);

        [OperationContract]
        //删除补货任务明细
        string SupplementTaskDetailDel(string whCode, string supplementNumber, string userName);

        [OperationContract]
        List<WhLocationResult> SupplementLocationList(WhLocationSearch searchEntity, out int total);

        //批量导入捡货库位
        [OperationContract]
        int ImportSupplementLocation(List<WhLocation> entity);

        [OperationContract]
        //批量删除捡货库位
        string SupplementLocationDel(List<WhLocation> entity);

        [OperationContract]
        //批量删除捡货库位对应款号信息
        string R_Location_Item_Del(List<R_Location_Item> entity);

        #endregion


        #region 13.耗材管理
        [OperationContract]
        //耗材列表
        List<Loss> LossList(LossSearch searchEntity, out int total);

        [OperationContract]
        //新增耗材
        string LossAdd(Loss entity);

        [OperationContract]
        //修改耗材
        int LossEdit(Loss entity);

        [OperationContract]
        //删除耗材
        int LossDel(int id);

        #endregion


        #region 14.照片管理

        [OperationContract]
        //新增TCR处理方式
        string TCRProcessModeAdd(TCRProcess entity);

        [OperationContract]
        //TCR处理方式列表
        List<TCRProcessResult> TCRProcessModeList(PhotoMasterSearch searchEntity, out int total);

        [OperationContract]
        //新增TCR处理方式
        int TCRProcessModeDel(int id);

        [OperationContract]
        //修改TCR信息
        string PhotoMasterEdit(PhotoMaster entity);

        [OperationContract]
        //修改TCR信息
        string PhotoMasterEdit1(PhotoMaster entity);

        [OperationContract]
        //处理TCR
        string EditTCRStatus(PhotoMaster entity);

        [OperationContract]
        //新增照片信息
        string PhotoMasterAdd(List<PhotoMaster> entity, string whCode);

        [OperationContract]
        //删除照片信息
        int PhotoMasterDel(int id);

        [OperationContract]
        //CFS收货照片查询
        List<PhotoMasterResult> InCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total);

        [OperationContract]
        //CFS出货照片查询
        List<PhotoMasterResult> OutCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total);

        [OperationContract]
        //照片上传
        string InCFSPhotoCComplete(PhotoMaster entity);

        [OperationContract]
        //审核收货照片
        string CFSPhotoCShenheComplete(PhotoMaster entity);


        [OperationContract]
        //出货照片上传
        string OutCFSPhotoCComplete(PhotoMaster entity);

        [OperationContract]
        //审核照片百分比
        decimal CheckCountPercent(PhotoMasterSearch searchEntity);

        [OperationContract]
        //TCR类型
        List<HoldMaster> HoldMasterList(HoldMasterSearch searchEntity, out int total);

        [OperationContract]
        //清除照片ID
        string PhotoMasterDelPhotoId(int id, string userName);

        [OperationContract]
        //删除PhotoMaster照片
        string PhotoMasterDelById(int id, string userName);

        #endregion


        #region 15.工作量管理

        [OperationContract]
        //收货工作量
        List<WorkloadAccountResult> InWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total);

        [OperationContract]
        //出货工作量
        List<WorkloadAccountResult> OutWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total);

        [OperationContract]
        //工人种类
        List<WorkloadAccountResult> WorkTypeList(string whCode);

        [OperationContract]
        //修改工人工号
        string EditWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]
        //批量修改收货工人
        string EditWorkloadAccountList(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]
        //批量删除工人工号
        string DelWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]

        //新增工人工号
        string AddWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);


        [OperationContract]
        //修改工人工号
        string EditWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]
        //批量修改工人工号
        string EditWorkloadAccount1List(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]
        //批量删除工人工号
        string DelWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        [OperationContract]

        //新增工人工号
        string AddWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        #endregion


        #region 16.收出货订单流程修改管理

        [OperationContract]
        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string type);

        [OperationContract]
        List<ReceiptRegisterResult> GetFlowHeadListByRec(ReceiptRegisterSearch searchEntity, out int total);

        [OperationContract]
        List<LoadMasterResult> GetFlowHeadListByLoad(LoadMasterSearch searchEntity, out int total);

        [OperationContract]
        string EditProcessName(string number, string whCode, int processId, string processName, string type, string userCode);

        #endregion


        #region 17.单据管理

        [OperationContract]
        List<CRTemplate> GetCRTemplate(CRReportSearch searchEntity, out int total);

        [OperationContract]
        String CrystallReportEdit(CRTemplate entity);

        [OperationContract]
        string CrystallReportAdd(CRTemplate entity);

        [OperationContract]
        int CrystallReportdel(int Id);
        #endregion


        #region 18.Edi任务基础数据管理

        [OperationContract]
        //得到仓库下的所有Edi基础名称
        IEnumerable<UrlEdi> UrlEdiSelect(string whCode);

        [OperationContract]
        //查询列表
        List<UrlEdi> UrlEdiList(UrlEdiSearch searchEntity, out int total);

        [OperationContract]
        //修改
        String UrlEdiEdit(UrlEdi entity);

        [OperationContract]
        //新增
        string UrlEdiAdd(UrlEdi entity);

        [OperationContract]
        int UrlEdidel(int Id);

        #endregion


        #region 19.highcharts数据管理(分析柱状图)
        //库位使用率
        [OperationContract]
        List<Highcharts> LocRateList(String WhCode);

        //库存使用率
        [OperationContract]
        List<Highcharts> InvRateList(String WhCode);

        [OperationContract]
        List<HighchartClient> ClientInvRateList(String WhCode);

        #endregion


        #region 20.客户类型管理

        //客户类型下拉菜单列表
        [OperationContract]
        IEnumerable<WhClientType> WhClientTypeListSelect(string whCode);


        //客户类型查询列表
        [OperationContract]
        List<WhClientType> WhClientTypeList(WhClientTypeSearch searchEntity, out int total);

        //客户类型修改
        [OperationContract]
        string WhClientTypeEdit(WhClientType entity);

        //新增客户类型
        [OperationContract]
        string WhClientTypeAdd(WhClientType entity);

        //删除客户类型
        [OperationContract]
        int WhClientTypeDel(int id);

        #endregion


        #region 21.夜班区间管理

        [OperationContract]
        //客户类型查询列表
        List<NightTime> NightTimeList(NightTimeSearch searchEntity, out int total);

        [OperationContract]
        //新增夜班区间
        string NightTimeAdd(NightTime entity);

        [OperationContract]
        //删除夜班区间
        int NightTimeDel(int id);

        [OperationContract]
        //修改夜班区间
        string NightTimeEdit(NightTime entity);

        [OperationContract]
        //夜班区间下拉菜单列表
        IEnumerable<NightTime> NightTimeListSelect(string whCode);

        #endregion


        #region 22.道口收费节假日管理

        [OperationContract]
        List<Holiday> HolidayList(HolidaySearch searchEntity, out int total);

        [OperationContract]
        string HolidayImports(string[] holiday, string[] dayBegin, string whCode, string userName);

        [OperationContract]
        //修改信息
        string HolidayEdit(Holiday entity);

        [OperationContract]
        int HolidayDel(int id);

        #endregion


        #region 23.收货合同管理

        [OperationContract]
        //查询
        List<ContractForm> ContractFormList(ContractFormSearch searchEntity, out int total);

        [OperationContract]
        //合同导入
        string ContractFormImports(List<ContractForm> entityList);

        [OperationContract]
        //删除
        int ContractFormDeleteAll(string contractName, string whCode, string userName);

        [OperationContract]
        //删除
        int ContractFormDel(int id);

        [OperationContract]
        //修改信息
        string ContractFormEdit(ContractForm entity);


        [OperationContract]
        //合同下拉菜单列表
        IEnumerable<string> ContractNameListSelect(string whCode);

        #endregion


        #region 24.TCR费用管理

        [OperationContract]
        //查询
        List<FeeMaster> FeeMaseterList(FeeMasterSearch searchEntity, string[] soNumberList, out int total, out string str);

        [OperationContract]
        //添加正常TCR费用
        FeeMaster FeeMaseterAdd(FeeMaster entity);

        [OperationContract]
        //添加收货特殊费用
        string FeeMaseterSpecialAdd(FeeMaster entity, int type);

        [OperationContract]
        //添加出货提箱特殊费用
        string FeeMasterOutLoadAdd(FeeMaster entity);

        [OperationContract]
        //客服确认状态未预收款
        string FeeMaseterEditStatus(FeeMaster entity);

        [OperationContract]
        //客服撤销确认状态未预收款
        string FeeMaseterEditStatus1(FeeMaster entity);

        [OperationContract]
        //客服订单确认作废 
        string FeeMaseterEditStatus2(FeeMaster entity);

        [OperationContract]
        //修改信息
        string FeeMaseterEdit(FeeMaster entity);

        [OperationContract]
        //修改费用备注
        string FeeMasterRemarkEdit(FeeMaster entity);

        [OperationContract]
        //删除全部费用信息
        string FeeMaseterDel(string feeNumber, string whCode, string userName);

        [OperationContract]
        //添加费用明细
        string FeeDetailAdd(FeeDetail entity);

        [OperationContract]
        //删除费用明细
        string FeeDetailDel(int id, string userName);

        [OperationContract]
        //费用明细列表
        List<FeeDetail> FeeDetailList(FeeDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //修改费用明细信息
        string FeeDetailEdit(FeeDetail entity);

        [OperationContract]
        //修改信息
        string FeeMaseterDKEdit(FeeMaster entity);

        [OperationContract]
        //修改费用明细信息
        string FeeDetailCKEdit(FeeDetail entity);

        [OperationContract]
        //仓库修改操作时间
        string FeeMasterCKEditBeginEndDate(FeeMaster entity);

        [OperationContract]
        //仓库确认状态
        string ConfirmFeeMasterCK(FeeMaster entity);

        [OperationContract]
        //仓库添加耗材费用明细
        string FeeDetailAddCKLoss(FeeDetail entity);

        [OperationContract]
        //删除费用明细
        string FeeDetailDelCKLoss(int id, string userName);

        [OperationContract]
        //得到实际操作费用
        string getOperationFee(string feeNumber, string whCode);

        [OperationContract]
        //得到实际操作费用列表 
        List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total);

        [OperationContract]
        //道口费用结算
        string FeeMaseterDKJiesuanEdit(FeeMaster entity, int type);

        [OperationContract]
        //道口修改发票信息
        string FeeMaseterInvoiceEdit(FeeMaster entity);

        [OperationContract]
        //查询库存TCR且不在费用明细内
        List<FeeDetailHuDetailListResult> HuDetailListByPOSKU(FeeDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] huList, out int total, out string str);

        [OperationContract]
        //添加费用明细
        string FeeDetailAddList(List<FeeDetail> entityList);

        [OperationContract]
        //查询库存TCR根据托盘条件检索
        FeeDetailHuDetailListResult GetHuDetailByHuId(string huId, string whCode, string clientCode);

        [OperationContract]
        //重新计算TCR费用
        string AgainTCRFeeCalculate(FeeMaster entity);

        #endregion


        #region 25.TCR收费节假日管理

        [OperationContract]
        //查询
        List<FeeHoliday> FeeHolidayList(HolidaySearch searchEntity, out int total);

        [OperationContract]
        //节假日导入
        string FeeHolidayImports(string[] holiday, string[] dayBegin, string[] type, string whCode, string userName);

        [OperationContract]
        //修改信息
        string FeeHolidayEdit(FeeHoliday entity);

        [OperationContract]
        int FeeHolidayDel(int id);

        #endregion


        #region 26.DamcoGrnRule管理

        [OperationContract]
        //DamcoGrnRule查询列表
        List<DamcoGrnRule> DamcoGrnRuleList(DamcoGrnRuleSearch searchEntity, out int total);

        [OperationContract]
        //新增
        string DamcoGrnRuleAdd(DamcoGrnRule entity);

        [OperationContract]
        //删除
        int DamcoGrnRuleDel(int id);

        [OperationContract]
        //修改
        string DamcoGrnRuleEdit(DamcoGrnRule entity);

        #endregion


        #region 27.出货合同管理

        [OperationContract]
        //查询
        List<ContractFormOut> ContractFormOutList(ContractFormOutSearch searchEntity, out int total);

        [OperationContract]
        //合同导入
        string ContractFormOutImports(List<ContractFormOut> entityList);

        [OperationContract]
        //删除
        int ContractFormOutDeleteAll(string contractName, string whCode);

        [OperationContract]
        //修改信息
        string ContractFormOutEdit(ContractFormOut entity);

        [OperationContract]
        //合同下拉菜单列表
        IEnumerable<string> ContractFormOutListSelect(string whCode);


        #endregion


        #region 28.账单管理

        [OperationContract]
        //账单查询
        List<BillMaster> BillMasterList(BillMasterSearch searchEntity, out int total);

        [OperationContract]
        //添加账单
        BillMaster BillMasterAdd(BillMaster entity);

        [OperationContract]
        //查询Load列表 显示是否已确认费用状态
        List<BillDetailResult> GetLoadMasterList(BillDetailSearch searchEntity, out int total, string[] loadIdList, string[] containerNumberList);

        [OperationContract]
        //根据Load号查询出货箱单费用并添加至账单
        string BillDetailAdd(string whCode, string billNumber, string userName, string[] loadId);

        [OperationContract]
        //显示装箱进港费明细
        List<BillDetailRepostResult> DamcoBillDetailList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total);


        [OperationContract]
        //删除账单
        string BillFeeDetailDelAll(string whCode, string billNumber);

        [OperationContract]
        //根据Load号删除账单明细，并撤销费用状态
        string BillDetailDelByLoad(string whCode, string billNumber, string userName, string loadId);

        [OperationContract]
        //修改账单状态
        string BillMasterEdit(string whCode, string billNumber, string status);

        [OperationContract]
        //根据SO号查询出货SO特费并添加至账单
        string BillDetailAddSO(string whCode, string billNumber, string userName, int[] idList);


        [OperationContract]
        //根据Id删除账单明细，并撤销费用状态
        string BillDetailDelBySO(string whCode, string billNumber, string userName, int id);


        [OperationContract]
        //丹马士SO账单显示
        List<BillDetailRepostResult> DamcoBillDetailSOList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total);



        #endregion


        #region 29.区域与库位拓展管理

        [OperationContract]
        //区域与库位扩展列表
        List<ZoneExtendResult> ZonesExtendList(WhZoneSearch searchEntity, out int total);

        [OperationContract]
        //新增区域扩展
        ZonesExtend ZonesExtendAdd(ZonesExtend entity);

        [OperationContract]
        //区域扩展信息修改
        int ZonesExtendEdit(ZonesExtend entity);

        [OperationContract]
        int ZonesExtendDel(int id);

        [OperationContract]
        string ZonesExtendBatchDel(int?[] idarr);

        [OperationContract]
        string ZoneExtendImports(List<ZoneExtendResult> entity);

        #endregion


        #region 30.客户扩展管理

        [OperationContract]
        //列表
        List<WhClientExtendResult> WhClientExtendList(WhClientSearch searchEntity, out int total);

        [OperationContract]
        //新增
        WhClientExtend WhClientExtendAdd(WhClientExtend entity);

        [OperationContract]
        //修改
        int WhClientExtendEdit(WhClientExtend entity);

        [OperationContract]
        //批量删除
        string WhClientExtendBatchDel(int?[] idarr);

        #endregion


        #region 31.部分收货原因登记管理

        [OperationContract]
        //列表
        List<ReceiptPartialRegisterResult> ReceiptPartialRegisterList(ReceiptPartialRegisterSearch searchEntity, out int total);

        [OperationContract]
        List<ReceiptPartialUnReceiptResult> ReceiptPartialUnReceiptList(ReceiptPartialUnPreceiptSearch searchEntity, out int total);

        [OperationContract]
        List<ReceiptPartialRegisteredDetailResult> RegisteredList(ReceiptPartialUnPreceiptSearch searchEntity, out int total);

        [OperationContract]
        string UnReceiptRegister(ReceiptPartialRegisterDetail entity, int UnQty);

        [OperationContract]
        string ReceiptPartialDeleteDetail(int Id, string rediptId, string WhCode);

        [OperationContract]
        string ReceiptPartialComplete(string ReceiptId, string WhCode);

        [OperationContract]
        string ReceiptPartialReBack(string ReceiptId, string WhCode);

        [OperationContract]
        //拒收登记异常原因下拉列表
        List<HoldMaster> HoldMasterListByReceiptPart(HoldMasterSearch searchEntity);

        [OperationContract]
        //拒收登记照片上传
        string ReceiptPartPhotoUpload(ReceiptPartialRegister entity);


        #endregion


        #region 32.收货收费特殊项目收费管理

        [OperationContract]
        //查询
        List<ContractFormExtend> ContractFormExtendList(ContractFormExtendSearch searchEntity, out int total);

        [OperationContract]
        //修改信息
        string ContractFormExtendEdit(ContractFormExtend entity);

        #endregion



        #region 33.托盘称重管理

        [OperationContract]
        //列表查询
        List<HuMasterResult1> HuMasterHeavyPalletList(HuMasterSearch1 searchEntity, string[] soNumber, out int total);

        [OperationContract]
        //托盘修改长宽高
        string HuMasterHeavyPalletEdit(HuMasterResult1 entity);


        #endregion


        #region 34.款号Code管理

        [OperationContract]
        //款号Code列表
        List<ItemMasterColorCode> ItemMasterColorCodeList(WhItemSearch searchEntity, out int total);

        [OperationContract]
        //批量导入款号Code
        string ItemMasterColorCodeImports(string[] clientCode, string[] colorCode, string[] colorDescription, string whCode, string userName);

        [OperationContract]
        string ItemColorCodeEdit(ItemMasterColorCode im);

        #endregion


        #region 35.退货上架库位管理

        [OperationContract]
        List<WhLocationResult> ReturnGoodLocationList(WhLocationSearch searchEntity, out int total);

        [OperationContract]
        int ImportReturnGoodLocation(List<WhLocation> entity);

        [OperationContract]
        //批量删除退货上架库位
        string ReturnGoodLocationDel(List<WhLocation> entity);

        [OperationContract]
        //退货上架库位款号信息列表
        List<R_Location_ItemResult> R_Location_ItemRGList(R_Location_ItemSearch searchEntity, out int total);

        [OperationContract]
        //新增退货上架库位信息
        string R_Location_ItemRGAdd(List<R_Location_Item_RG> entity);

        [OperationContract]
        int R_Location_ItemRGEdit(R_Location_Item_RG entity);

        [OperationContract]
        //批量删除信息
        string R_Location_Item_RG_Del(List<R_Location_Item_RG> entity);


        #endregion


    }
}
