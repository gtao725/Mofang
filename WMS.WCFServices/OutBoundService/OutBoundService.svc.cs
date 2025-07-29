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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“OutBoundService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 OutBoundService.svc 或 OutBoundService.svc.cs，然后开始调试。
    public class OutBoundService : IOutBoundService
    {
        IBLL.IOutBoundOrderManager outBound = new BLL.OutBoundOrderManager();
        IBLL.IOutBoundOrderDetailService outBoundDetail = new BLL.OutBoundOrderDetailService();
        IBLL.ILoadManager load = new BLL.LoadManager();
        IBLL.IReleaseLoadManager releaseLoad = new BLL.ReleaseLoadManager();
        IBLL.IInterceptManager intercept = new BLL.InterceptManager();
        IBLL.ILoadContainerExtendService loadContainer = new BLL.LoadContainerExtendService();
        IBLL.ILoadHuIdExtendService loadHuIdExtend = new BLL.LoadHuIdExtendService();


        #region  1.出库订单管理
        public List<OutBoundOrderResult> OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            return outBound.OutBoundOrderList(searchEntity, out total);
        }

        //根据选择的客户获得出货流程
        public IEnumerable<FlowHead> OutFlowHeadListByClientId(string whCode, int clientId)
        {
            return outBound.OutFlowHeadListByClientId(whCode, clientId);
        }

        //出库流程下拉列表
        public IEnumerable<FlowHead> OutFlowHeadListSelect(string whCode)
        {
            return outBound.OutFlowHeadListSelect(whCode);
        }

        //状态查询下拉列表
        public IEnumerable<LookUp> FlowRuleStatusListSelect()
        {
            return outBound.FlowRuleStatusListSelect();
        }

        //出库订单添加
        public OutBoundOrder OutBoundOrderAdd(OutBoundOrder entity)
        {
            return outBound.OutBoundOrderAdd(entity);
        }

        //确认出库订单
        public string ConfirmOutBoundOrder(OutBoundOrder eneity)
        {
            return outBound.ConfirmOutBoundOrder(eneity);
        }

        //回滚出库订单
        public string RollbackOutBoundOrder(OutBoundOrder eneity)
        {
            return outBound.RollbackOutBoundOrder(eneity);
        }

        //验证款号、款号对应的单位 是否有误
        public string OutBoundCheckUnitName(OutBoundOrderDetailInsert entity)
        {
            return outBound.OutBoundCheckUnitName(entity);
        }

        //出库订单明细添加
        public string OutBoundOrderDetailAdd(OutBoundOrderDetailInsert entity)
        {
            return outBound.OutBoundOrderDetailAdd(entity);
        }

        //出库订单明细查询
        public List<OutBoundOrderDetailResult> OutBoundOrderDetailList(OutBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total)
        {
            return outBound.OutBoundOrderDetailList(searchEntity, itemNumberList, out total);
        }

        //出库订单录入信息修改
        public int OutBoundOrderDetailEdit(OutBoundOrderDetail entity, params string[] modifiedProNames)
        {
            return outBound.OutBoundOrderDetailEdit(entity, modifiedProNames);
        }

        //出库订单明细删除
        public int OutBoundOrderDetailDel(int id)
        {
            return outBoundDetail.DeleteById(id);
        }

        //出库订单删除
        public string OutBoundOrderDel(int id)
        {
            return outBound.OutBoundOrderDel(id);
        }

        //出库订单导入
        public string ImportsOutBoundOrder(List<ImportOutBoundOrder> entityList)
        {
            return outBound.ImportsOutBoundOrder(entityList);
        }

        #endregion


        #region 2.Load管理

        //Load查询
        public List<LoadMasterResult> LoadMasterList(LoadMasterSearch searchEntity, out int total)
        {
            return load.LoadMasterList(searchEntity, out total);
        }

        //Load明细查询 直接显示订单明细
        public List<OutBoundOrderDetailResult> LoadToOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total, out string str)
        {
            return load.LoadToOutBoundOrderList(searchEntity, out total, out str);
        }


        public LoadMaster LoadMasterAdd(LoadMaster entity)
        {
            return load.LoadMasterAdd(entity);
        }

        //Load创建
        public string LoadContainerExtendAdd(LoadContainerExtend entity)
        {
            return load.LoadContainerExtendAdd(entity);
        }

        //箱型下拉列表
        public IEnumerable<LoadContainerType> LoadContainerTypeSelect()
        {
            return load.LoadContainerTypeSelect();
        }

        //出库订单查询
        public List<OutBoundOrderResult> Load_OutBoundOrderList(OutBoundOrderSearch searchEntity, string[] customerOutPo, out int total)
        {
            return load.Load_OutBoundOrderList(searchEntity, customerOutPo, out total);
        }

        //添加Load明细
        public int LoadDetailAdd(List<LoadDetail> entity)
        {
            return load.LoadDetailAdd(entity);
        }

        //Load删除 同时删除明细
        public string LoadMasterDel(int id)
        {
            return load.LoadMasterDel(id);
        }


        //Load订单 明细查询
        public List<OutBoundOrderResult> LoadSecond_OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            return load.LoadSecond_OutBoundOrderList(searchEntity, out total);
        }

        //Load删除明细
        public string LoadDetailDel(int id)
        {
            return load.LoadDetailDel(id);
        }

        //释放Load
        public string CheckReleaseLoad(string loadId, string whCode, string userName)
        {
            return releaseLoad.CheckReleaseLoad(loadId, whCode, userName);
        }


        //撤销释放
        public string RollbackLoad(string loadId, string whCode, string userName)
        {
            return releaseLoad.RollbackLoad(loadId, whCode, userName);
        }

        //释放Load
        public string CheckReleaseLoadGetType(string loadId, string whCode, string userName, string getType)
        {
            return releaseLoad.CheckReleaseLoad(loadId, whCode, userName, getType);
        }

        //查询Load批量导入出货托盘列表
        public List<LoadHuIdExtend> LoadHuIdExtendSearch(LoadHuIdExtendSearch searchEntity, out int total)
        {
            return load.LoadHuIdExtendSearch(searchEntity, out total);
        }

        //Load批量导入出货托盘 释放时优先该托盘
        public string LoadHuIdExtendImport(List<LoadHuIdExtend> entity)
        {
            return load.LoadHuIdExtendImport(entity);
        }

        //删除导入的出货托盘
        public int LoadHuIdExtendDelete(int id)
        {
            return loadHuIdExtend.DeleteById(id);
        }


        //修改Load备注
        public string LoadMasterEditRemark(LoadMaster entity)
        {
            return load.LoadMasterEditRemark(entity);
        }

        //装箱单列表查询
        public List<LoadContainerResult> LoadContainerList(LoadContainerSearch searchEntity, string[] containerNumber, out int total, out string str)
        {
            return load.LoadContainerList(searchEntity, containerNumber, out total, out str);
        }


        //装箱单创建人下拉列表
        public List<CreateUserResult> CreateUserSelect(string whCode)
        {
            return load.CreateUserSelect(whCode);
        }


        //添加装箱单
        public LoadContainerExtend LoadContainerAdd(LoadContainerExtend entity)
        {
            return load.LoadContainerAdd(entity);
        }

        //修改箱单信息
        public string LoadContainerEdit(LoadContainerExtend entity)
        {
            return load.LoadContainerEdit(entity);
        }

        public string LoadContainerDelete(int loadContainerId)
        {
            return load.LoadContainerDelete(loadContainerId);
        }

        //装箱单查询库存 按SO
        public List<LoadContainerHuDetailResult> LoadContainerHuDetailList(LoadContainerHuDetailSearch searchEntity, string[] soList, out int total, out string str)
        {
            return load.LoadContainerHuDetailList(searchEntity, soList, out total, out str);
        }

        //装箱单查询库存  按SOPOSKU查询
        public List<LoadContainerHuDetailResult> LoadContainerHuDetailListByPOSKU(LoadContainerHuDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] style1List, string[] style2List, out int total, out string str)
        {
            return load.LoadContainerHuDetailListByPOSKU(searchEntity, soList, poList, skuList, style1List, style2List, out total, out str);
        }

        //验证所选库存的客户出货流程是否一致
        public List<FlowHeadResult> CheckLoadContainerClientCodeRule(string whCode, List<WhClient> client)
        {
            return load.CheckLoadContainerClientCodeRule(whCode, client);
        }

        //装箱单选择库存生成明细
        public string LoadContainerHuDetailAdd(string whCode, int loadContainerId, string[] soList, string[] clientCodeList, int processId, string processName, string userName)
        {
            return load.LoadContainerHuDetailAdd(whCode, loadContainerId, soList, clientCodeList, processId, processName, userName);
        }

        //装箱单  选择库存生成明细  ByPOSKU
        public string LoadContainerHuDetailAddByPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int processId, string processName, string userName)
        {
            return load.LoadContainerHuDetailAddByPOSKU(whCode, loadContainerId, soList, poList, altList, clientCodeList, processId, processName, userName);
        }


        //装箱单  批量导入出货明细
        public string LoadContainerHuDetailAddByImportPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int[] qtyList, int[] sequenceList, string userName)
        {
            return load.LoadContainerHuDetailAddByImportPOSKU(whCode, loadContainerId, soList, poList, altList, clientCodeList, qtyList, sequenceList, userName);
        }

        //装箱单所选库存 按SO查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySo(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            return load.SelectLoadContainerHuDetailBySo(searchEntity, out total, out str);
        }

        //装箱单所选库存 按SOPO查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPo(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            return load.SelectLoadContainerHuDetailBySoPo(searchEntity, out total, out str);
        }

        //装箱单所选库存 按SOPOSKU查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSku(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            return load.SelectLoadContainerHuDetailBySoPoSku(searchEntity, out total, out str);
        }

        //装箱单所选库存 按款号属性查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSkuStyle(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            return load.SelectLoadContainerHuDetailBySoPoSkuStyle(searchEntity, out total, out str);
        }

        //修改装箱单选择的库存明细的顺序
        public string LoadContainerHuDetailEdit(LoadContainerExtendHuDetail entity)
        {
            return load.LoadContainerHuDetailEdit(entity);
        }

        //删除装箱单选择的库存明细
        public string LoadContainerHuDetailDel(LoadContainerExtendHuDetail entity)
        {
            return load.LoadContainerHuDetailDel(entity);
        }


        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySo(string whCode, int loadContainerId, string[] soList, string[] sequenceList, string userName)
        {
            return load.ImportsSequenceBySo(whCode, loadContainerId, soList, sequenceList, userName);
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPo(string whCode, int loadContainerId, string[] soList, string[] poList, string[] sequenceList, string userName)
        {
            return load.ImportsSequenceBySoPo(whCode, loadContainerId, soList, poList, sequenceList, userName);
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPoSku(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] sequenceList, string userName)
        {
            return load.ImportsSequenceBySoPoSku(whCode, loadContainerId, soList, poList, skuList, sequenceList, userName);
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPoSkuStyle(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] itemIdList, string[] sequenceList, string userName)
        {
            return load.ImportsSequenceBySoPoSkuStyle(whCode, loadContainerId, soList, poList, skuList, itemIdList, sequenceList, userName);
        }

        //装箱单生成CLP
        public string ConfirmLoadMasterAdd(string whCode, string userName, int loadContainerId)
        {
            return load.ConfirmLoadMasterAdd(whCode, userName, loadContainerId);
        }

        //保存前 验证装箱单数量与立方 和实际释放的做比对
        public string CheckLoadContainerDetailToRealease(int loadContainerId)
        {
            return load.CheckLoadContainerDetailToRealease(loadContainerId);
        }

        //验证 装箱单所选数量 和 已保存的Load数量做比对
        public string CheckLoadContainerQtyToOutDetail(string loadId, string whCode)
        {
            return load.CheckLoadContainerQtyToOutDetail(loadId, whCode);
        }

        //验证箱型 和 所选立方
        public string CheckLoadContainerCBMToRealease(int loadContainerId)
        {
            return load.CheckLoadContainerCBMToRealease(loadContainerId);
        }

        //直装订单列表
        public List<OutBoundOrderResult> DSOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            return load.DSOutBoundOrderList(searchEntity, out total);
        }

        //直装出库订单导入
        public string DSOutBoundImport(List<ImportOutBoundOrder> entityList, string loadId)
        {
            return load.DSOutBoundImport(entityList, loadId);
        }


        //直装订单删除
        public string DSLoadDetailDel(int id)
        {
            return load.DSLoadDetailDel(id);
        }

        //出货操作单模版
        public string PrintOutTempalte(string whCode, string loadId)
        {
            return load.PrintOutTempalte(whCode, loadId);
        }

        public List<LoadChargeRuleResult> LoadChargeRuleUnselected(LoadChargeRuleSearch searchEntity, out int total)
        {
            return load.LoadChargeRuleUnselected(searchEntity, out total);
        }

        public List<LoadChargeDetailResult> LoadChargeRuleSelected(LoadChargeRuleSearch searchEntity, out int total)
        {
            return load.LoadChargeRuleSelected(searchEntity, out total);
        }

        //Load出货规则验证
        public string LoadChargeRuleCheck(LoadChargeDetailInsert entity)
        {
            return load.LoadChargeRuleCheck(entity);
        }

        public string LoadChargeDetailDelById(int id)
        {
            return load.LoadChargeDetailDelById(id);
        }

        //仓库收费科目列表显示
        public List<LoadContainerResult> LoadChargeDetailWarehouseList(LoadContainerSearch searchEntity, string[] containerNumber, out int total)
        {
            return load.LoadChargeDetailWarehouseList(searchEntity, containerNumber, out total);
        }

        //仓库收费科目输入数量列表显示
        public List<LoadChargeDetailResult> LoadChargeRuleWarehouseSelected(LoadChargeRuleSearch searchEntity, out int total)
        {
            return load.LoadChargeRuleWarehouseSelected(searchEntity, out total);
        }

        //仓库录入数量操作
        public string LoadContainerWarehouseEdit(string whCode, string loadId, string[] id, string[] qtycbm)
        {
            return load.LoadContainerWarehouseEdit(whCode, loadId, id, qtycbm);
        }

        //检测出货项目是否全部录入完成，必选项是否遗漏
        public string LoadChargeDetailCheck(string whCode, string loadId)
        {
            return load.LoadChargeDetailCheck(whCode, loadId);
        }


        //根据客户自动计算某些科目
        public string LoadChargeClientAutoCharge(LoadChargeDetailResult entity)
        {
            return load.LoadChargeClientAutoCharge(entity);
        }

        //显示load下的所有费用科目及数量
        public List<LoadChargeDetailResult> LoadChargeDetailList(LoadChargeRuleSearch searchEntity, out int total)
        {
            return load.LoadChargeDetailList(searchEntity, out total);
        }

        //修改出货费用状态为确认
        public string LoadChargeEdit(string whCode, string loadId, string status)
        {
            return load.LoadChargeEdit(whCode, loadId, status);
        }

        //重新计算基础费用
        public string AAgainLoadCharge(string whCode, string loadId, string userName)
        {
            return load.AgainLoadCharge(whCode, loadId, userName);
        }

        //代垫列表
        public List<LoadChargeDaiDian> DaiDianSelectList()
        {
            return load.DaiDianSelectList();
        }

        //特费科目列表
        public List<LoadChargeRule> LoadChargeRuleSelectList()
        {
            return load.LoadChargeRuleSelectList();
        }


        //代垫列表
        public List<LoadChargeDaiDian> LoadChargeDaiDianSelectList()
        {
            return load.LoadChargeDaiDianSelectList();
        }

        //SO特费科目添加
        public string LoadChargeRuleAdd(LoadChargeRule entity)
        {
            return load.LoadChargeRuleAdd(entity);
        }

        //编辑SO特费科目
        public string LoadChargeRuleEdit(LoadChargeRule entity)
        {
            return load.LoadChargeRuleEdit(entity);
        }

        //添加SO特费
        public string LoadChargeDetailAddBySO(LoadChargeDetail entity)
        {
            return load.LoadChargeDetailAddBySO(entity);
        }

        //编辑SO特费
        public string LoadChargeDetailEditBySO(LoadChargeDetail entity)
        {
            return load.LoadChargeDetailEditBySO(entity);
        }


        //查询SO特费列表
        public List<LoadChargeDetailResult> LoadChargeDetailSOList(LoadChargeDetailSearch searchEntity, string[] soList, out int total)
        {
            return load.LoadChargeDetailSOList(searchEntity, soList, out total);
        }

        //删除SO特费
        public string LoadChargeDetailSoDel(int id, string userName)
        {
            return load.LoadChargeDetailSoDel(id, userName);
        }

        //科目类型下拉列表
        public IEnumerable<LoadChargeRuleResult> GetLoadChangeRuleTypeName()
        {
            return load.GetLoadChangeRuleTypeName();
        }

        //得到确认箱单费用异常数量
        public List<LoadChargeDetailResult> CheckLoadChangeErrorCount(LoadContainerSearch searchEntity, out int total)
        {
            return load.CheckLoadChangeErrorCount(searchEntity, out total);
        }

        public List<ReleaseLoadDetail> ReleaseLoadDetailList(LoadHuIdExtendSearch searchEntity, out int total)
        {
            return load.ReleaseLoadDetailList(searchEntity, out total);
        }


        #endregion


        #region 3.拦截订单管理

        //订单拦截
        public string OutBoundOrderIntercept(string whCode, string customerOutPoNumber, string clientCode, string userName)
        {
            return intercept.OutBoundOrderIntercept(whCode, customerOutPoNumber, clientCode, userName);
        }

        //拦截订单查询 
        public List<OutBoundOrderResult1> InterceptOutBoundOrderList(OutBoundOrderSearch1 searchEntity, out int total)
        {
            return intercept.InterceptOutBoundOrderList(searchEntity, out total);
        }

        //得到客户下的所有收货流程
        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId)
        {
            return intercept.ClientFlowNameSelect(whCode, clientId);
        }

        //重新生成收货操作单
        public string AddReceiptIdByInterceptOrder(int outBoundOrderId, int processId, string processName, string recLocationId, string userName, string abLocation)
        {
            return intercept.AddReceiptIdByInterceptOrder(outBoundOrderId, processId, processName, recLocationId, userName, abLocation);
        }


        //批量生成拦截收货操作单
        public string AddReceiptIdByInterceptOrderList(int[] outBoundOrderIdList, string whCode, string userName, string abLocation)
        {
            return intercept.AddReceiptIdByInterceptOrderList(outBoundOrderIdList, whCode, userName, abLocation);
        }

        //订单处理完成 确认
        public string CheckInterceptOrder(int outBoundOrderId, string userName)
        {
            return intercept.CheckInterceptOrder(outBoundOrderId, userName);
        }


        //通过系统出库订单号查找收货批次号
        public string GetInterceptOrderReceiptId(string OutPoNumber, string whCode)
        {
            return intercept.GetInterceptOrderReceiptId(OutPoNumber, whCode);
        }


        //拦截订单批量回库
        public string InterceptOrderBatchReturnToWarehouse(int outBoundOrderIdList, string whCode, string userName, string abLocation)
        {
            return intercept.InterceptOrderBatchReturnToWarehouse(outBoundOrderIdList, whCode, userName, abLocation);
        }

        #endregion



        #region 特殊功能：SN批量导入一键创建Load并释放 更换、删除、新增其它托盘

        //SN批量导入一键创建Load并释放
        public string ImportsOutBoundOrderExtendBySN(List<OutBoundOrderExtendInsert> entityList)
        {
            return load.ImportsOutBoundOrderExtendBySN(entityList);
        }

        //备货单信息显示
        public List<PickTaskDetailResult2> PickTaskDetailList(PickTaskDetailSearch searchEntity, out int total)
        {
            return load.PickTaskDetailList(searchEntity, out total);
        }


        //删除托盘
        public string PickTaskDetailHuIdDel(PickTaskDetailResult2 entity)
        {
            return load.PickTaskDetailHuIdDel(entity);
        }

        //新增托盘
        public string PickTaskDetailHuIdAdd(PickTaskDetailResult2 entity)
        {
            return load.PickTaskDetailHuIdAdd(entity);
        }

        #endregion


        #region 客户装箱单数据处理

        public List<ExcelImportOutBound> CottonExcelImportOutBoundList(ExcelImportOutBoundSearch searchEntity, out int total, out string str)
        {
            return load.CottonExcelImportOutBoundList(searchEntity, out total, out str);
        }


        public string ExcelImportOutBoundCotton(List<ExcelImportOutBound> entity)
        {
            return load.ExcelImportOutBoundCotton(entity);
        }


        #endregion

    }
}
