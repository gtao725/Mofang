using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IRootManager
    {
        #region 1.代理管理

        //代理列表
        //对应AgentController中的 List 方法
        List<WhAgent> WhAgentList(WhAgentSearch search, out int total);

        //新增代理
        //对应AgentController中的 AddAgent 方法
        WhAgent WhAgentAdd(WhAgent entity);

        //代理信息修改
        //对应AgentController中的 WhAgentEdit 方法
        int WhAgentEdit(WhAgent entity, params string[] modifiedProNames);

        #endregion


        #region 2.客户管理

        //客户列表
        //对应ClientController中的 List 方法
        List<WhClientResult> WhClientList(WhClientSearch searchEntity, out int total);

        //客户释放规则列表
        List<WhClientResult> WhClientReleaseRuleList(WhClientSearch searchEntity, out int total);

        //新增客户
        //对应ClientController中的 AddClient 方法
        WhClient WhClientAdd(WhClient entity);

        //客户信息修改
        //对应ClientController中的 WhClientEdit 方法
        int WhClientEdit(WhClient entity, params string[] modifiedProNames);

        //根据当前客户查询出未选择的货代
        //对应ClientController中的 WhAgentUnselected 方法
        List<WhAgentResult> WhAgentUnselected(WhAgentSearch searchEntity, out int total);

        //根据当前客户查询出已选择的货代
        //对应ClientController中的 WhAgentSelected 方法
        List<WhAgentResult> WhAgentSelected(WhAgentSearch searchEntity, out int total);

        //批量添加客户代理关系
        //对应ClientController中的 WhAgentWhClientListAdd 方法
        int WhAgentWhClientListAdd(List<R_WhClient_WhAgent> entity);

        //删除客户的某个代理
        //对应ClientController中的 WhAgentWhClientDelByClientId 方法
        int WhAgentWhClientDel(R_WhClient_WhAgent entity);


        //客户异常原因列表
        //对应ClientController中的 HoldMasterListByClient 方法
        List<HoldMaster> HoldMasterListByClient(HoldMasterSearch searchEntity, out int total);


        //新增客户异常原因
        //对应ClientController中的 HoldMasterAdd 方法
        HoldMaster HoldMasterAdd(HoldMaster entity);


        //客户异常原因修改
        //对应ClientController中的 HoldMasterEdit 方法
        int HoldMasterEdit(HoldMaster entity, params string[] modifiedProNames);


        //根据当前客户查询出未选择的流程
        //对应ClientController中的 ClientOutFlowNameUnselected 方法
        List<FlowHeadResult> FlowNameUnselected(FlowHeadSearch searchEntity, out int total);

        //根据当前客户查询出已选择的流程
        //对应ClientController中的 FlowNameSelected 方法
        List<FlowHeadResult> FlowNameSelected(FlowHeadSearch searchEntity, out int total);

        //新增客户流程关系
        //对应ClientController中的 AddFlowRule 方法
        int AddFlowRule(List<R_Client_FlowRule> entity);





        //验证托盘移动目的地的合法性
        int CheckClientZone(string HuId, string Loaction, string WhCode);

        #endregion


        #region 3.储位管理

        //客户列表
        List<WhLocationResult> WhLocationList(WhLocationSearch searchEntity, out int total);

        //批量导入储位
        string LocationImports(List<LocationResult> entity);

        //按照规则生成储位
        int AddLocation(string beginLocationArray, string beginLocationColumn, string endLocationColumn, string beginLocationColumn2, string endLocationColumn2, int beginLocationRow, int endLocationRow, int LocationFloor, int LocationPcs, int CheckBegin, string whCode, string userName);

        //储位类型下拉列表
        IEnumerable<WhLocationTypeResult> LocationTypeSelect();

        //区域类型下拉列表
        IEnumerable<WhZoneResult> ZoneSelect(string whCode);

        IEnumerable<WhLocationResult> LocationSelect(string whCode);

        //新增储位
        WhLocation LocationAdd(WhLocation entity);

        //批量修改储位
        string LocationEdit(List<WhLocation> list);

        //批量删除库位
        string WhLocationBatchDel(int?[] idarr);

        int WhLocationEdit(WhLocation entity);

        //取消区域关系
        int ZoneLocationDelById(int Id);

        //取得默认异常库位
        string GetWhLocationLookUp(string whCode);

        //异常库位默认设置
        string SetWhLocationLookUp(string whCode, string abLocationId);

        #endregion


        #region 4.托盘管理

        //托盘列表
        //对应PallateController中的 List 方法
        List<WhPallateResult> WhPallateList(WhPallateSearch searchEntity, out int total);


        //批量导入托盘
        //对应PallateController中的 AddPallate 方法
        int WhPallateListAdd(List<Pallate> entity);


        //托盘类型下拉列表
        IEnumerable<WhPallateTypeResult> PallateTypeSelect();


        string PallateImports(List<WhPallateResult> entity);

        string PallateBatchDel(int?[] idarr);

        #endregion


        #region 5.区域管理

        //区域列表
        List<WhZoneResult> WhZoneList(WhZoneSearch searchEntity, out int total);

        //新增区域
        Zone WhZoneAdd(Zone entity);

        //根据当前区域查询出未选择的库位信息
        List<ZoneLocationResult> LocationUnselected(ZoneLocationSearch searchEntity, out int total);

        //根据当前区域查询出已选择的库位信息
        List<ZoneLocationResult> LocationSelected(ZoneLocationSearch searchEntity, out int total);

        //新增区域
        int ZoneLocationAdd(List<ZoneLocation> entity);


        //区域父级菜单下拉列表
        IEnumerable<WhZoneResult> WhZoneParentSelect(string whCode);


        //区域信息修改
        int WhZoneParentEdit(Zone entity, params string[] modifiedProNames);


        //批量删除区域
        string WhZoneBatchDel(int?[] idarr);

        string ZoneImports(List<ZoneResult> entity);

        #endregion


        #region 6.款号管理

        //款号列表
        //对应ItemController中的 List 方法
        List<WhItemResult> ItemMasterList(WhItemSearch searchEntity, out int total);

        //批量导入款号
        //对应ItemController中的 imports 方法
        string ItemImports(string[] clientCode, string[] altItemNumber, string[] style1, string[] style2, string[] style3, string[] unitName, string whCode, string userName);


        //批量导入款号品名
        string ItemImportsItemName(string[] clientCode, string[] altItemNumber, string[] itemName, string whCode, string userName);

        //添加款号
        string ItemMaterAdd(ItemMaster im);

        //修改款号基础信息
        string ItemMasterEdit(ItemMaster im);

        //更新款号
        string ItemMaterUpdate(ItemMaster im);

        //OMS款号信息新增或更新接口JSON列表
        string ItemMasterExtendOMSAdd(List<WhItemExtendOMS> entity);

        #endregion


        #region 7.仓库管理

        //仓库异常原因列表
        //对应WarehouseController中的 List 方法
        List<HoldMaster> WarehouseHoldMasterList(HoldMasterSearch searchEntity, out int total);


        //箱号采集信息管理--------------------------------


        //箱号采集列表
        //对应 C_SerialNumberController 中的 List 方法
        List<SerialNumberInOut> SerialNumberInList(SerialNumberInSearch searchEntity, out int total);
        //箱号采集明细列表
        List<SerialNumberDetailOut> SerialNumberDetailList(SerialNumberDetailSearch searchEntity, out int total);
        //批量删除序列号
        int SerialNumberDelByHuId(SerialNumberIn entity);


        //箱号信息修改
        //对应 C_SerialNumberController 中的 SerialNumberEdit 方法
        int SerialNumberEdit(SerialNumberIn entity);

        //新增箱号信息
        //对应 C_SerialNumberController 中的 SerialNumberAdd 方法
        int SerialNumberAdd(SerialNumberIn entity);

        //出货箱号采集管理 查询列表
        List<SerialNumberOut> SerialNumberOutList(SerialNumberOutSearch searchEntity, out int total);

        //出货采集序列号 修改
        string SerialNumberOutEdit(SerialNumberOut entity);

        //新增出货序列号信息
        string SerialNumberOutAdd(SerialNumberOut entity);

        //新增出货序列号信息
        string SerialNumberAddOther(SerialNumberOut entity);

        //出货序列号删除
        string SerialNumberOutDel(SerialNumberOut entity);



        //收货箱号采集管理 查询列表
        List<HeportSerialNumberIn> HeportSerialNumberInList(SerialNumberInSearch searchEntity, out int total);

        //箱号信息修改
        int HeportSerialNumberEdit(HeportSerialNumberIn entity);

        int HeportSerialNumberDelByHuId(HeportSerialNumberIn entity);

        //盘点任务信息管理--------------------------------

        //盘点任务列表
        //对应 C_CycleCountController 中的 List 方法
        List<CycleCountMasterResult> CycleCountMasterList(CycleCountMasterSearch searchEntity, out int total);

        //新增盘点任务
        //对应 C_CycleCountController 中的 CycleCountMasterAdd 方法
        string CycleCountMasterAdd(CycleCountMasterInsert entity);

        //盘点任务明细列表
        //对应 C_CycleCountController 中的 CycleCountDetailList 方法
        List<CycleCountDetailResult> CycleCountDetailList(CycleCountDetailSearch searchEntity, out int total);

        //删除盘点任务
        //对应 C_CycleCountController 中的 CycleCountMasterDel 方法
        string CycleCountMasterDel(CycleCountMaster entity);

        //盘点任务开始
        string CycleCountInsertComplex(CycleCountInsertComplex entity);

        string CycleCountInsertComplex(CycleCountInsertComplexAddPo entity);


        //盘点任务结果列表
        //对应 C_CycleCountController 中的 CycleCountChecklList 方法
        List<CycleCountCheckResult> CycleCountCheckList(CycleCountCheckSearch searchEntity, out int total);

        //完成盘点任务
        string CycleCountComplete(string taskNumber, string whCode, string userName);

        //盘点任务差异EAN验证款号
        //有差异的 款号匹配EAN验证是否是EAN 如果是EAN 更新盘点结果
        string CheckCycleResultSkuByEAN(string taskNumber, string whCode, string userName);

        //盘点任务再次生成
        string CycleCountMasterAddAgain(string taskNumber, string whCode, string userName);


        //提交盘点结果时 验证是否与库存盘点结果一致
        string CheckCycleResult(CycleCountInsertComplexAddPo searchEntity);


        //提交盘点结果时 验证是否与库存盘点结果一致
        string CheckCycleResult(CycleCountInsertComplex searchEntity);


        //创建盘点任务 按照客户和款号
        string CycleCountMasterAddByClientCodeSku(CycleCountMasterInsert entity, string[] itemNumberList, string clientCode);

        //创建盘点任务 按照库位变更时间
        string CycleCountMasterAddByLocationChangeTime(CycleCountMasterInsert entity, CycleCountMasterSeacrh searchEntity);

        //修改实际盘点结果
        string EditCycleCheckDetail(CycleCountCheck entity, CycleCountCheck oldEntity, string userName);

        //添加实际盘点结果
        string AddCycleCheckDetail(List<CycleCountCheck> entity, string userName);

        //删除实际盘点结果
        string DelCycleCheckDetail(CycleCountCheck entity, string userName);

        #endregion


        #region 8.款号单位信息管理
        List<UnitsResult> UnitsList(UnitsSearch searchEntity, out int total);

        //新增单位
        int AddUnit(Unit unit);

        List<ItemUnitResult> ItemUnitList(ItemUnitSearch searchEntity, out int total);
        #endregion


        #region 9.RF流程管理

        //流程列表
        //对应ClientFlowController中的 List 方法
        List<BusinessFlowGroupResult> ClientRFFlowList(BusinessFlowGroupSearch searchEntity, out int total);


        //添加流程
        //对应ClientFlowController中的 AddClientFlow 方法
        BusinessFlowGroup AddClientRFFlow(BusinessFlowGroup entity);

        //流程修改
        //对应ClientFlowController中的 ClientFlowEdit 方法
        int ClientRFFlowEdit(BusinessFlowGroup entity, params string[] modifiedProNames);


        //流程明细列表
        //对应ClientFlowController中的 FlowRuleDetailList 方法
        List<BusinessFlowHeadResult> RFFlowRuleDetailList(BusinessFlowHeadSearch searchEntity, out int total);


        //添加流程配置
        //对应ClientFlowController中的 AddClientFlowDetail 方法
        string AddClientFlowDetail(BusinessFlowHeadInsert entity);

        #endregion


        #region 10.收出货等流程管理

        //流程列表
        //对应ClientOutFlowController中的 List 方法
        List<FlowHeadResult> ClientFlowRuleList(FlowHeadSearch searchEntity, out int total);

        //添加流程
        //对应ClientOutFlowController中的 AddClientFlowRule 方法
        FlowHead AddClientFlowRule(FlowHead entity);

        //流程修改
        //对应ClientOutFlowController中的 ClientOutFlowEdit 方法
        int ClientFlowRuleEdit(FlowHead entity, params string[] modifiedProNames);


        //流程明细列表
        //对应ClientOutFlowController中的 FlowRuleDetailList 方法
        List<FlowDetailResult> FlowRuleDetailList(FlowDetailSearch searchEntity, out int total);


        //添加出货流程配置
        //对应ClientOutFlowController中的 AddClientOutFlowDetail 方法
        string AddClientOutFlowDetail(FlowHeadInsert entity);



        //对应ClientController中的 ClientFlowNameUnselected 方法
        List<BusinessFlowGroupResult> ClientFlowNameUnselected(BusinessFlowGroupSearch searchEntity, out int total);


        //对应ClientController中的 ClientFlowNameSelected 方法
        List<BusinessFlowGroupResult> ClientFlowNameSelected(BusinessFlowGroupSearch searchEntity, out int total);

        //新增收货流程对RF流程的关系
        //对应ClientController中的 AddClientFlowRule 方法
        int AddClientFlowRuleDetail(List<FlowDetail> entity);


        //报表字段排序下拉列表
        IEnumerable<FieldOrderByResult> FieldOrderBySelect(string whCode);

        //打印报表名称下拉列表
        IEnumerable<CRTempResult> TempSelect(string type, string whCode);

        #endregion


        #region 11.Load生成规则管理

        List<LoadCreateRuleResult> GetLoadCreateRuleList(LoadCreateRuleSearch searchEntity, out int total);

        //新增生成规则
        LoadCreateRule LoadCreateRuleAdd(LoadCreateRule entity);

        //信息修改
        int LoadCreateRuleEdit(LoadCreateRule entity);

        List<BusinessFlowGroupResult> LoadCreateFlowNameUnselected(LoadCreateRuleSearch searchEntity, out int total);

        List<BusinessFlowGroupResult> LoadCreateFlowNameSelected(LoadCreateRuleSearch searchEntity, out int total);


        int R_LoadRule_FlowHeadListAdd(List<R_LoadRule_FlowHead> entity);


        //自动生成Load规则名称下拉列表
        IEnumerable<LoadCreateRule> LoadCreateRuleSelect(string whCode);

        //自动批量生成Load
        string BeginLoadCreate(LoadCreateRuleInsert entity);

        //根据客户名、订单来源、创建时间 查询出货流程对应得订单数
        List<LoadCreateRuleResult> GetOrderQtyList(LoadCreateRuleSearch searchEntity, out int total);

        //出货订单类型下拉列表
        IEnumerable<LoadCreateRuleInsert> OutBoundOrderSourceSelect(string whCode);

        #endregion


        #region 12.补货任务管理

        //补货信息列表
        List<R_Location_ItemResult> R_Location_ItemList(R_Location_ItemSearch searchEntity, out int total);

        //新增补货库位信息
        string R_Location_ItemAdd(List<R_Location_Item> entity);

        int R_Location_ItemEdit(R_Location_Item entity);

        //释放补货任务
        string ReleaseSupplementTask(string whCode, string userName, int count, string[] altItemNumber);

        //释放补货任务 博士定制
        string BoschReleaseSupplementTask(string whCode, string userName, int count);


        //补货任务表查询
        List<SupplementTaskResult> SupplementTaskResultList(SupplementTaskSearch searchEntity, out int total);

        //补货任务明细查询
        List<SupplementTaskDetailResult> SupplementTaskDetailResultList(SupplementTaskDetailSearch searchEntity, out int total);

        //删除补货任务明细
        string SupplementTaskDetailDel(string whCode, string supplementNumber, string userName);

        List<WhLocationResult> SupplementLocationList(WhLocationSearch searchEntity, out int total);

        //批量导入捡货库位
        int ImportSupplementLocation(List<WhLocation> entity);

        //批量删除捡货库位
        string SupplementLocationDel(List<WhLocation> entity);

        //批量删除捡货库位对应款号信息
        string R_Location_Item_Del(List<R_Location_Item> entity);

        #endregion


        #region 13.耗材管理

        //耗材列表
        List<Loss> LossList(LossSearch searchEntity, out int total);

        //新增耗材
        string LossAdd(Loss entity);

        //修改耗材
        int LossEdit(Loss entity);

        //删除耗材
        int LossDel(int id);

        #endregion


        #region 14.照片管理

        //新增TCR处理方式
        string TCRProcessModeAdd(TCRProcess entity);

        //TCR处理方式列表
        List<TCRProcessResult> TCRProcessModeList(PhotoMasterSearch searchEntity, out int total);

        //修改TCR信息
        string PhotoMasterEdit(PhotoMaster entity);

        //修改TCR信息
        string PhotoMasterEdit1(PhotoMaster entity);

        //处理TCR
        string EditTCRStatus(PhotoMaster entity);

        //新增照片信息
        string PhotoMasterAdd(List<PhotoMaster> entity, string whCode);

        //CFS收货照片查询
        List<PhotoMasterResult> InCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total);

        //CFS出货照片查询
        List<PhotoMasterResult> OutCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total);

        //照片上传
        string InCFSPhotoCComplete(PhotoMaster entity);

        //审核收货照片
        string CFSPhotoCShenheComplete(PhotoMaster entity);

        //出货照片上传
        string OutCFSPhotoCComplete(PhotoMaster entity);

        //审核照片百分比
        decimal CheckCountPercent(PhotoMasterSearch searchEntity);

        //TCR类型
        List<HoldMaster> HoldMasterList(HoldMasterSearch searchEntity, out int total);

        string PhotoMasterDelPhotoId(int id, string userName);

        //删除PhotoMaster照片
        string PhotoMasterDelById(int id, string userName);

        #endregion


        #region 15.工作量管理

        //收货工作量
        List<WorkloadAccountResult> InWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total);

        //出货工作量
        List<WorkloadAccountResult> OutWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total);

        //工人种类
        List<WorkloadAccountResult> WorkTypeList(string whCode);

        //修改工人工号
        string EditWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);

        //批量修改收货工人
        string EditWorkloadAccountList(List<WorkloadAccountResult> entity, string userCode);

        //批量删除工人工号
        string DelWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);

        //新增工人工号
        string AddWorkloadAccount(List<WorkloadAccountResult> entity, string userCode);


        //修改工人工号
        string EditWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        //批量修改工人工号
        string EditWorkloadAccount1List(List<WorkloadAccountResult> entity, string userCode);


        //批量删除工人工号
        string DelWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        //新增工人工号
        string AddWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode);

        #endregion


        #region 16.收出货订单流程修改管理

        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string type);

        List<ReceiptRegisterResult> GetFlowHeadListByRec(ReceiptRegisterSearch searchEntity, out int total);

        List<LoadMasterResult> GetFlowHeadListByLoad(LoadMasterSearch searchEntity, out int total);

        string EditProcessName(string number, string whCode, int processId, string processName, string type, string userCode);

        #endregion


        #region 17.单据管理
        List<CRTemplate> GetCRTemplate(CRReportSearch searchEntity, out int total);


        String CrystallReportEdit(CRTemplate entity);

        string CrystallReportAdd(CRTemplate entity);
        #endregion


        #region 18.Edi任务基础数据管理
        //得到仓库下的所有Edi基础名称
        IEnumerable<UrlEdi> UrlEdiSelect(string whCode);

        //查询列表
        List<UrlEdi> UrlEdiList(UrlEdiSearch searchEntity, out int total);

        //修改
        String UrlEdiEdit(UrlEdi entity);

        //新增
        string UrlEdiAdd(UrlEdi entity);

        #endregion


        #region 19.highcharts数据管理(分析柱状图)

        //库位使用率
        List<Highcharts> LocRateList(String WhCode);

        List<Highcharts> InvRateList(String WhCode);

        List<HighchartClient> ClientInvRateList(String WhCode);


        #endregion


        #region 20.客户类型管理

        //客户类型查询列表
        List<WhClientType> WhClientTypeList(WhClientTypeSearch searchEntity, out int total);

        //客户类型修改
        string WhClientTypeEdit(WhClientType entity);

        //新增客户类型
        string WhClientTypeAdd(WhClientType entity);

        //删除客户类型
        int WhClientTypeDel(int id);

        //客户类型下拉菜单列表
        IEnumerable<WhClientType> WhClientTypeListSelect(string whCode);

        #endregion


        #region 21.夜班区间管理

        //客户类型查询列表
        List<NightTime> NightTimeList(NightTimeSearch searchEntity, out int total);

        //新增夜班区间
        string NightTimeAdd(NightTime entity);

        //删除夜班区间
        int NightTimeDel(int id);

        //修改夜班区间
        string NightTimeEdit(NightTime entity);


        //夜班区间下拉菜单列表
        IEnumerable<NightTime> NightTimeListSelect(string whCode);

        #endregion


        #region 22.道口收费节假日管理

        List<Holiday> HolidayList(HolidaySearch searchEntity, out int total);
        string HolidayImports(string[] holiday, string[] dayBegin, string whCode, string userName);

        //修改信息
        string HolidayEdit(Holiday entity);

        #endregion


        #region 23.收货合同管理

        //查询
        List<ContractForm> ContractFormList(ContractFormSearch searchEntity, out int total);

        //合同导入
        string ContractFormImports(List<ContractForm> entityList);

        //删除
        int ContractFormDeleteAll(string contractName, string whCode, string userName);

        //修改信息
        string ContractFormEdit(ContractForm entity);

        //合同下拉菜单列表
        IEnumerable<string> ContractNameListSelect(string whCode);

        #endregion


        #region 24.TCR费用管理

        //查询
        List<FeeMaster> FeeMaseterList(FeeMasterSearch searchEntity, string[] soNumberList, out int total, out string str);

        //添加正常TCR费用
        FeeMaster FeeMaseterAdd(FeeMaster entity);

        //添加收货特殊费用
        string FeeMaseterSpecialAdd(FeeMaster entity, int type);

        //添加出货提箱特殊费用
        string FeeMasterOutLoadAdd(FeeMaster entity);

        //客服确认状态未预收款
        string FeeMaseterEditStatus(FeeMaster entity);

        //客服撤销确认状态未预收款
        string FeeMaseterEditStatus1(FeeMaster entity);

        //客服订单确认作废 
        string FeeMaseterEditStatus2(FeeMaster entity);

        //修改信息
        string FeeMaseterEdit(FeeMaster entity);

        //修改费用备注
        string FeeMasterRemarkEdit(FeeMaster entity);

        //删除全部费用信息
        string FeeMaseterDel(string feeNumber, string whCode, string userName);


        //添加费用明细
        string FeeDetailAdd(FeeDetail entity);

        //删除费用明细
        string FeeDetailDel(int id, string userName);

        //费用明细列表
        List<FeeDetail> FeeDetailList(FeeDetailSearch searchEntity, out int total, out string str);

        //修改费用明细信息
        string FeeDetailEdit(FeeDetail entity);

        //修改信息
        string FeeMaseterDKEdit(FeeMaster entity);

        //修改费用明细信息
        string FeeDetailCKEdit(FeeDetail entity);

        //仓库修改操作时间
        string FeeMasterCKEditBeginEndDate(FeeMaster entity);

        //仓库确认状态
        string ConfirmFeeMasterCK(FeeMaster entity);

        //仓库添加耗材费用明细
        string FeeDetailAddCKLoss(FeeDetail entity);

        //删除费用明细
        string FeeDetailDelCKLoss(int id, string userName);


        //得到实际操作费用
        string getOperationFee(string feeNumber, string whCode);

        //得到实际操作费用列表 
        List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total);

        //道口费用结算
        string FeeMaseterDKJiesuanEdit(FeeMaster entity, int type);

        //道口修改发票信息
        string FeeMaseterInvoiceEdit(FeeMaster entity);

        //查询库存TCR且不在费用明细内
        List<FeeDetailHuDetailListResult> HuDetailListByPOSKU(FeeDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] huList, out int total, out string str);

        //添加费用明细
        string FeeDetailAddList(List<FeeDetail> entityList);

        //查询库存TCR根据托盘条件检索
        FeeDetailHuDetailListResult GetHuDetailByHuId(string huId, string whCode, string clientCode);

        //重新计算TCR费用
        string AgainTCRFeeCalculate(FeeMaster entity);

        #endregion


        #region 25.TCR收费节假日管理

        //查询
        List<FeeHoliday> FeeHolidayList(HolidaySearch searchEntity, out int total);

        //节假日导入
        string FeeHolidayImports(string[] holiday, string[] dayBegin, string[] type, string whCode, string userName);

        //修改信息
        string FeeHolidayEdit(FeeHoliday entity);


        #endregion


        #region 26.DamcoGrnRule管理

        //DamcoGrnRule查询列表 
        List<DamcoGrnRule> DamcoGrnRuleList(DamcoGrnRuleSearch searchEntity, out int total);

        //新增
        string DamcoGrnRuleAdd(DamcoGrnRule entity);

        //删除
        int DamcoGrnRuleDel(int id);

        //修改
        string DamcoGrnRuleEdit(DamcoGrnRule entity);

        #endregion


        #region 27.出货合同管理

        //查询
        List<ContractFormOut> ContractFormOutList(ContractFormOutSearch searchEntity, out int total);

        //合同导入
        string ContractFormOutImports(List<ContractFormOut> entityList);

        //删除
        int ContractFormOutDeleteAll(string contractName, string whCode);

        //修改信息
        string ContractFormOutEdit(ContractFormOut entity);

        //合同下拉菜单列表
        IEnumerable<string> ContractFormOutListSelect(string whCode);


        #endregion


        #region 28.账单管理

        //账单查询
        List<BillMaster> BillMasterList(BillMasterSearch searchEntity, out int total);

        //添加账单
        BillMaster BillMasterAdd(BillMaster entity);

        //查询Load列表 显示是否已确认费用状态
        List<BillDetailResult> GetLoadMasterList(BillDetailSearch searchEntity, out int total, string[] loadIdList, string[] containerNumberList);

        //根据Load号查询出货箱单费用并添加至账单
        string BillDetailAdd(string whCode, string billNumber, string userName, string[] loadId);

        //丹马士账单显示
        List<BillDetailRepostResult> DamcoBillDetailList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total);


        //删除账单
        string BillFeeDetailDelAll(string whCode, string billNumber);

        //根据Load号删除账单明细，并撤销费用状态
        string BillDetailDelByLoad(string whCode, string billNumber, string userName, string loadId);

        //修改账单状态
        string BillMasterEdit(string whCode, string billNumber, string status);


        //根据SO号查询出货SO特费并添加至账单
        string BillDetailAddSO(string whCode, string billNumber, string userName, int[] idList);

        //根据Id删除账单明细，并撤销费用状态
        string BillDetailDelBySO(string whCode, string billNumber, string userName, int id);

        //丹马士SO账单显示
        List<BillDetailRepostResult> DamcoBillDetailSOList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total);

        #endregion


        #region 29.区域与库位拓展管理

        //区域与库位扩展列表
        List<ZoneExtendResult> ZonesExtendList(WhZoneSearch searchEntity, out int total);

        ZonesExtend ZonesExtendAdd(ZonesExtend entity);

        //区域扩展信息修改
        int ZonesExtendEdit(ZonesExtend entity);

        string ZonesExtendBatchDel(int?[] idarr);


        string ZoneExtendImports(List<ZoneExtendResult> entity);


        #endregion


        #region 30.客户扩展管理

        //列表
        List<WhClientExtendResult> WhClientExtendList(WhClientSearch searchEntity, out int total);

        //新增
        WhClientExtend WhClientExtendAdd(WhClientExtend entity);

        //修改
        int WhClientExtendEdit(WhClientExtend entity);

        //批量删除
        string WhClientExtendBatchDel(int?[] idarr);

        #endregion


        #region 31.部分收货原因登记管理

        //列表
        List<ReceiptPartialRegisterResult> ReceiptPartialRegisterList(ReceiptPartialRegisterSearch searchEntity, out int total);

        List<ReceiptPartialUnReceiptResult> ReceiptPartialUnReceiptList(ReceiptPartialUnPreceiptSearch searchEntity, out int total);

        List<ReceiptPartialRegisteredDetailResult> RegisteredList(ReceiptPartialUnPreceiptSearch searchEntity, out int total);
        string UnReceiptRegister(ReceiptPartialRegisterDetail entity, int UnQty);

        string ReceiptPartialDeleteDetail(int Id, string rediptId, string WhCode);

        string ReceiptPartialComplete(string ReceiptId, string WhCode);

        string ReceiptPartialReBack(string ReceiptId, string WhCode);


        //拒收登记异常原因下拉列表
        List<HoldMaster> HoldMasterListByReceiptPart(HoldMasterSearch searchEntity);


        //拒收登记照片上传
        string ReceiptPartPhotoUpload(ReceiptPartialRegister entity);

        #endregion


        #region 32.收货收费特殊项目收费管理

        //查询
        List<ContractFormExtend> ContractFormExtendList(ContractFormExtendSearch searchEntity, out int total);


        //修改信息
        string ContractFormExtendEdit(ContractFormExtend entity);

        #endregion



        #region 33.托盘称重管理

        //列表查询
        List<HuMasterResult1> HuMasterHeavyPalletList(HuMasterSearch1 searchEntity, string[] soNumber, out int total);


        //托盘修改长宽高
        string HuMasterHeavyPalletEdit(HuMasterResult1 entity);


        #endregion


        #region 34.款号Code管理
        //款号Code列表
        List<ItemMasterColorCode> ItemMasterColorCodeList(WhItemSearch searchEntity, out int total);


        //批量导入款号Code
        string ItemMasterColorCodeImports(string[] clientCode, string[] colorCode, string[] colorDescription, string whCode, string userName);

        string ItemColorCodeEdit(ItemMasterColorCode im);

        #endregion



        #region 35.退货上架库位管理

        List<WhLocationResult> ReturnGoodLocationList(WhLocationSearch searchEntity, out int total);

        int ImportReturnGoodLocation(List<WhLocation> entity);

        //批量删除退货上架库位
        string ReturnGoodLocationDel(List<WhLocation> entity);


        //退货上架库位款号信息列表
        List<R_Location_ItemResult> R_Location_ItemRGList(R_Location_ItemSearch searchEntity, out int total);


        //新增退货上架库位信息
        string R_Location_ItemRGAdd(List<R_Location_Item_RG> entity);


        int R_Location_ItemRGEdit(R_Location_Item_RG entity);

        //批量删除信息
        string R_Location_Item_RG_Del(List<R_Location_Item_RG> entity);

        #endregion


    }
}
