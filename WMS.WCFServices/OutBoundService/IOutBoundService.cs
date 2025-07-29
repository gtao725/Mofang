using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.OutBoundService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IOutBoundService”。
    [ServiceContract]
    public interface IOutBoundService
    {

        #region  1.出库订单管理

        [OperationContract]
        //出库订单查询
        List<OutBoundOrderResult> OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);


        [OperationContract]
        //根据选择的客户获得出货流程
        IEnumerable<FlowHead> OutFlowHeadListByClientId(string whCode, int clientId);

        [OperationContract]
        //出库流程下拉列表
        IEnumerable<FlowHead> OutFlowHeadListSelect(string whCode);

        [OperationContract]
        //状态查询下拉列表
        IEnumerable<LookUp> FlowRuleStatusListSelect();

        [OperationContract]
        //出库订单添加
        OutBoundOrder OutBoundOrderAdd(OutBoundOrder entity);

        [OperationContract]
        //确认出库订单
        string ConfirmOutBoundOrder(OutBoundOrder eneity);

        [OperationContract]
        //回滚出库订单
        string RollbackOutBoundOrder(OutBoundOrder eneity);

        [OperationContract]
        //验证款号、款号对应的单位 是否有误
        string OutBoundCheckUnitName(OutBoundOrderDetailInsert entity);

        [OperationContract]
        //出库订单明细添加
        string OutBoundOrderDetailAdd(OutBoundOrderDetailInsert entity);

        [OperationContract]
        //出库订单明细查询
        List<OutBoundOrderDetailResult> OutBoundOrderDetailList(OutBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total);

        [OperationContract]
        //出库订单录入信息修改
        int OutBoundOrderDetailEdit(OutBoundOrderDetail entity, params string[] modifiedProNames);

        [OperationContract]
        //出库订单明细删除
        int OutBoundOrderDetailDel(int id);

        [OperationContract]
        //出库订单删除
        string OutBoundOrderDel(int id);


        //出库订单导入
        [OperationContract]
        string ImportsOutBoundOrder(List<ImportOutBoundOrder> entityList);

        #endregion


        #region 2.Load管理


        [OperationContract]
        //Load查询
        List<LoadMasterResult> LoadMasterList(LoadMasterSearch searchEntity, out int total);

        [OperationContract]
        //Load明细查询 直接显示订单明细
        List<OutBoundOrderDetailResult> LoadToOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total, out string str);

        [OperationContract]
        LoadMaster LoadMasterAdd(LoadMaster entity);


        [OperationContract]
        //Load创建
        string LoadContainerExtendAdd(LoadContainerExtend entity);


        [OperationContract]
        //箱型下拉列表
        IEnumerable<LoadContainerType> LoadContainerTypeSelect();

        [OperationContract]
        //出库订单查询
        List<OutBoundOrderResult> Load_OutBoundOrderList(OutBoundOrderSearch searchEntity, string[] customerOutPo, out int total);

        [OperationContract]
        //添加Load明细
        int LoadDetailAdd(List<LoadDetail> entity);

        [OperationContract]
        //Load删除 同时删除明细
        string LoadMasterDel(int id);

        [OperationContract]
        //Load订单 明细查询
        List<OutBoundOrderResult> LoadSecond_OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);

        [OperationContract]
        //Load删除明细
        string LoadDetailDel(int id);

        [OperationContract]
        //释放Load
        string CheckReleaseLoad(string loadId, string whCode, string userName);

        [OperationContract]
        //撤销释放
        string RollbackLoad(string loadId, string whCode, string userName);

        [OperationContract]
        //释放Load
        string CheckReleaseLoadGetType(string loadId, string whCode, string userName, string getType);

        [OperationContract]
        //查询Load批量导入出货托盘列表
        List<LoadHuIdExtend> LoadHuIdExtendSearch(LoadHuIdExtendSearch searchEntity, out int total);


        [OperationContract]
        //Load批量导入出货托盘 释放时优先该托盘
        string LoadHuIdExtendImport(List<LoadHuIdExtend> entity);


        [OperationContract]
        //删除导入的出货托盘
        int LoadHuIdExtendDelete(int id);


        [OperationContract]
        //修改Load备注
        string LoadMasterEditRemark(LoadMaster entity);

        [OperationContract]
        //装箱单列表查询
        List<LoadContainerResult> LoadContainerList(LoadContainerSearch searchEntity, string[] containerNumber, out int total, out string str);

        [OperationContract]
        //装箱单创建人下拉列表
        List<CreateUserResult> CreateUserSelect(string whCode);

        [OperationContract]
        //添加装箱单
        LoadContainerExtend LoadContainerAdd(LoadContainerExtend entity);

        [OperationContract]
        //修改箱单信息
        string LoadContainerEdit(LoadContainerExtend entity);

        [OperationContract]
        //删除装箱单
        string LoadContainerDelete(int loadContainerId);

        [OperationContract]
        //装箱单查询库存 按SO查询
        List<LoadContainerHuDetailResult> LoadContainerHuDetailList(LoadContainerHuDetailSearch searchEntity, string[] soList, out int total, out string str);


        [OperationContract]
        //装箱单查询库存  按SOPOSKU查询
        List<LoadContainerHuDetailResult> LoadContainerHuDetailListByPOSKU(LoadContainerHuDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] style1List, string[] style2List, out int total, out string str);

        [OperationContract]
        //验证所选库存的客户出货流程是否一致
        List<FlowHeadResult> CheckLoadContainerClientCodeRule(string whCode, List<WhClient> client);

        [OperationContract]
        //装箱单选择库存生成明细
        string LoadContainerHuDetailAdd(string whCode, int loadContainerId, string[] soList, string[] clientCodeList, int processId, string processName, string userName);


        [OperationContract]
        //装箱单  选择库存生成明细  ByPOSKU
        string LoadContainerHuDetailAddByPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int processId, string processName, string userName);

        [OperationContract]
        //装箱单  批量导入出货明细
        string LoadContainerHuDetailAddByImportPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int[] qtyList, int[] sequenceList, string userName);


        [OperationContract]
        //装箱单所选库存 按SO查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySo(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //装箱单所选库存 按SOPO查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPo(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //装箱单所选库存 按SOPOSKU查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSku(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //装箱单所选库存 按款号属性查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSkuStyle(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //修改装箱单选择的库存明细的顺序
        string LoadContainerHuDetailEdit(LoadContainerExtendHuDetail entity);

        [OperationContract]
        //删除装箱单选择的库存明细
        string LoadContainerHuDetailDel(LoadContainerExtendHuDetail entity);

        [OperationContract]
        //导入装箱单选择库存的顺序
        string ImportsSequenceBySo(string whCode, int loadContainerId, string[] soList, string[] sequenceList, string userName);

        [OperationContract]
        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPo(string whCode, int loadContainerId, string[] soList, string[] poList, string[] sequenceList, string userName);

        [OperationContract]
        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPoSku(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] sequenceList, string userName);

        [OperationContract]
        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPoSkuStyle(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] itemIdList, string[] sequenceList, string userName);

        [OperationContract]
        //装箱单生成CLP
        string ConfirmLoadMasterAdd(string whCode, string userName, int loadContainerId);

        [OperationContract]
        //保存前 验证装箱单数量与立方 和实际释放的做比对
        string CheckLoadContainerDetailToRealease(int loadContainerId);

        [OperationContract]
        //验证 装箱单所选数量 和 已保存的Load数量做比对
        string CheckLoadContainerQtyToOutDetail(string loadId, string whCode);

        [OperationContract]
        //验证箱型 和 所选立方
        string CheckLoadContainerCBMToRealease(int loadContainerId);

        [OperationContract]
        //直装订单列表
        List<OutBoundOrderResult> DSOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);

        [OperationContract]
        //直装出库订单导入
        string DSOutBoundImport(List<ImportOutBoundOrder> entityList, string loadId);


        [OperationContract]
        //直装订单删除
        string DSLoadDetailDel(int id);

        [OperationContract]
        //出货操作单模版
        string PrintOutTempalte(string whCode, string loadId);

        [OperationContract]
        //出货费用列表选择
        List<LoadChargeRuleResult> LoadChargeRuleUnselected(LoadChargeRuleSearch searchEntity, out int total);

        [OperationContract]
        List<LoadChargeDetailResult> LoadChargeRuleSelected(LoadChargeRuleSearch searchEntity, out int total);

        [OperationContract]
        //Load出货规则验证
        string LoadChargeRuleCheck(LoadChargeDetailInsert entity);

        [OperationContract]
        //删除
        string LoadChargeDetailDelById(int id);

        [OperationContract]
        //仓库收费科目列表显示
        List<LoadContainerResult> LoadChargeDetailWarehouseList(LoadContainerSearch searchEntity, string[] containerNumber, out int total);

        [OperationContract]
        //仓库收费科目输入数量列表显示
        List<LoadChargeDetailResult> LoadChargeRuleWarehouseSelected(LoadChargeRuleSearch searchEntity, out int total);

        [OperationContract]
        //仓库录入数量操作
        string LoadContainerWarehouseEdit(string whCode, string loadId, string[] id, string[] qtycbm);

        [OperationContract]
        //检测出货项目是否全部录入完成，必选项是否遗漏
        string LoadChargeDetailCheck(string whCode, string loadId);

        [OperationContract]
        //根据客户自动计算某些科目
        string LoadChargeClientAutoCharge(LoadChargeDetailResult entity);

        [OperationContract]
        //显示load下的所有费用科目及数量
        List<LoadChargeDetailResult> LoadChargeDetailList(LoadChargeRuleSearch searchEntity, out int total);

        [OperationContract]
        //修改出货费用状态为确认
        string LoadChargeEdit(string whCode, string loadId, string status);

        [OperationContract]
        //重新计算基础费用
        string AAgainLoadCharge(string whCode, string loadId, string userName);

        [OperationContract]
        //代垫列表
        List<LoadChargeDaiDian> DaiDianSelectList();

        [OperationContract]
        //特费科目列表
        List<LoadChargeRule> LoadChargeRuleSelectList();

        [OperationContract]
        //代垫列表
        List<LoadChargeDaiDian> LoadChargeDaiDianSelectList();

        [OperationContract]
        //SO特费科目添加
        string LoadChargeRuleAdd(LoadChargeRule entity);

        [OperationContract]
        //编辑SO特费科目
        string LoadChargeRuleEdit(LoadChargeRule entity);

        [OperationContract]
        //添加SO特费
        string LoadChargeDetailAddBySO(LoadChargeDetail entity);

        [OperationContract]
        //编辑SO特费
        string LoadChargeDetailEditBySO(LoadChargeDetail entity);

        [OperationContract]
        //查询SO特费列表
        List<LoadChargeDetailResult> LoadChargeDetailSOList(LoadChargeDetailSearch searchEntity, string[] soList, out int total);

        [OperationContract]
        //删除SO特费
        string LoadChargeDetailSoDel(int id, string userName);

        [OperationContract]
        //科目类型下拉列表
        IEnumerable<LoadChargeRuleResult> GetLoadChangeRuleTypeName();

        [OperationContract]
        //得到确认箱单费用异常数量
        List<LoadChargeDetailResult> CheckLoadChangeErrorCount(LoadContainerSearch searchEntity, out int total);

        [OperationContract]
        //查询释放异常列表
        List<ReleaseLoadDetail> ReleaseLoadDetailList(LoadHuIdExtendSearch searchEntity, out int total);

     

        #endregion


        #region 3.拦截订单管理

        [OperationContract]
        //订单拦截
        string OutBoundOrderIntercept(string whCode, string customerOutPoNumber, string clientCode, string userName);

        [OperationContract]
        //拦截订单查询 
        List<OutBoundOrderResult1> InterceptOutBoundOrderList(OutBoundOrderSearch1 searchEntity, out int total);

        [OperationContract]
        //得到客户下的所有收货流程
        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId);

        [OperationContract]
        //重新生成收货操作单
        string AddReceiptIdByInterceptOrder(int outBoundOrderId, int processId, string processName, string recLocationId, string userName, string abLocation);

        [OperationContract]
        //批量生成拦截收货操作单
        string AddReceiptIdByInterceptOrderList(int[] outBoundOrderIdList, string whCode, string userName, string abLocation);

        [OperationContract]
        //订单处理完成 确认
        string CheckInterceptOrder(int outBoundOrderId, string userName);

        [OperationContract]
        //通过系统出库订单号查找收货批次号
        string GetInterceptOrderReceiptId(string OutPoNumber, string whCode);

        [OperationContract]
        //拦截订单批量回库
        string InterceptOrderBatchReturnToWarehouse(int outBoundOrderIdList, string whCode, string userName, string abLocation);


        #endregion


        #region 特殊功能：SN批量导入一键创建Load并释放 更换、删除、新增其它托盘

        [OperationContract]
        //SN批量导入一键创建Load并释放
        string ImportsOutBoundOrderExtendBySN(List<OutBoundOrderExtendInsert> entityList);

        [OperationContract]
        //备货单信息显示
        List<PickTaskDetailResult2> PickTaskDetailList(PickTaskDetailSearch searchEntity, out int total);

        [OperationContract]
        //删除托盘
        string PickTaskDetailHuIdDel(PickTaskDetailResult2 entity);

        [OperationContract]
        //新增托盘
        string PickTaskDetailHuIdAdd(PickTaskDetailResult2 entity);

        #endregion



        #region 客户装箱单数据处理

        [OperationContract]
        List<ExcelImportOutBound> CottonExcelImportOutBoundList(ExcelImportOutBoundSearch searchEntity, out int total, out string str);

        [OperationContract]
        string ExcelImportOutBoundCotton(List<ExcelImportOutBound> entity);

        #endregion

    }
}
