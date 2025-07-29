using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ILoadManager
    {
        //Load查询
        //对应 LoadController 中的 List 方法
        List<LoadMasterResult> LoadMasterList(LoadMasterSearch searchEntity, out int total);

        //Load明细查询 直接显示订单明细
        List<OutBoundOrderDetailResult> LoadToOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total, out string str);

        //Load创建
        //对应 LoadController 中的 LoadMasterAdd 方法
        LoadMaster LoadMasterAdd(LoadMaster entity);

        //维护箱封号
        string LoadContainerExtendAdd(LoadContainerExtend entity);

        //箱型下拉列表
        IEnumerable<LoadContainerType> LoadContainerTypeSelect();

        //出库订单查询
        //对应 LoadController 中的 OutBoundOrderList 方法
        List<OutBoundOrderResult> Load_OutBoundOrderList(OutBoundOrderSearch searchEntity, string[] customerOutPo, out int total);

        //添加Load明细
        //对应 LoadController 中的 LoadDetailAdd 方法
        int LoadDetailAdd(List<LoadDetail> entity);

        //Load删除 同时删除明细
        string LoadMasterDel(int id);

        //Load订单 明细查询
        //对应 LoadController 中的 LoadSecond_OutBoundOrderList 方法
        List<OutBoundOrderResult> LoadSecond_OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);

        //Load删除明细
        string LoadDetailDel(int id);

        //查询Load批量导入出货托盘列表
        List<LoadHuIdExtend> LoadHuIdExtendSearch(LoadHuIdExtendSearch searchEntity, out int total);

        //Load批量导入出货托盘 释放时优先该托盘
        string LoadHuIdExtendImport(List<LoadHuIdExtend> entity);


        //修改Load备注
        string LoadMasterEditRemark(LoadMaster entity);

        //装箱单列表查询
        List<LoadContainerResult> LoadContainerList(LoadContainerSearch searchEntity, string[] containerNumber, out int total, out string str);

        //装箱单创建人下拉列表
        List<CreateUserResult> CreateUserSelect(string whCode);

        //添加装箱单
        LoadContainerExtend LoadContainerAdd(LoadContainerExtend entity);

        //修改箱单信息
        string LoadContainerEdit(LoadContainerExtend entity);

        //装箱单查询库存 按SO查询
        List<LoadContainerHuDetailResult> LoadContainerHuDetailList(LoadContainerHuDetailSearch searchEntity, string[] soList, out int total, out string str);

        //装箱单查询库存  按SOPOSKU查询
        List<LoadContainerHuDetailResult> LoadContainerHuDetailListByPOSKU(LoadContainerHuDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] style1List, string[] style2List, out int total, out string str);

        //验证所选库存的客户出货流程是否一致
        List<FlowHeadResult> CheckLoadContainerClientCodeRule(string whCode, List<WhClient> client);

        //装箱单选择库存生成明细
        string LoadContainerHuDetailAdd(string whCode, int loadContainerId, string[] soList, string[] clientCodeList, int processId, string processName, string userName);

        //装箱单  选择库存生成明细  ByPOSKU
        string LoadContainerHuDetailAddByPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int processId, string processName, string userName);

        //装箱单  批量导入出货明细
        string LoadContainerHuDetailAddByImportPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int[] qtyList, int[] sequenceList, string userName);


        //装箱单所选库存 按SO查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySo(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        //装箱单所选库存 按SOPO查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPo(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        //装箱单所选库存 按SOPOSKU查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSku(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        //装箱单所选库存 按款号属性查询
        List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSkuStyle(LoadContainerHuDetailSearch searchEntity, out int total, out string str);

        //修改装箱单选择的库存明细的顺序
        string LoadContainerHuDetailEdit(LoadContainerExtendHuDetail entity);

        //删除装箱单选择的库存明细
        string LoadContainerHuDetailDel(LoadContainerExtendHuDetail entity);

        //导入装箱单选择库存的顺序
        string ImportsSequenceBySo(string whCode, int loadContainerId, string[] soList, string[] sequenceList, string userName);

        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPo(string whCode, int loadContainerId, string[] soList, string[] poList, string[] sequenceList, string userName);

        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPoSku(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] sequenceList, string userName);

        //导入装箱单选择库存的顺序
        string ImportsSequenceBySoPoSkuStyle(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] itemIdList, string[] sequenceList, string userName);

        //装箱单生成CLP
        string ConfirmLoadMasterAdd(string whCode, string userName, int loadContainerId);

        //保存前 验证装箱单数量与立方 和实际释放的做比对
        string CheckLoadContainerDetailToRealease(int loadContainerId);

        //验证 装箱单所选数量 和 已保存的Load数量做比对
        string CheckLoadContainerQtyToOutDetail(string loadId, string whCode);

        //验证箱型 和 所选立方
        string CheckLoadContainerCBMToRealease(int loadContainerId);

        //直装订单列表
        List<OutBoundOrderResult> DSOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);

        //直装出库订单导入
        string DSOutBoundImport(List<ImportOutBoundOrder> entityList, string loadId);

        //直装订单删除
        string DSLoadDetailDel(int id);

        //出货操作单模版
        string PrintOutTempalte(string whCode, string loadId);


        //删除装箱单
        string LoadContainerDelete(int loadContainerId);

        //出货费用列表选择
        List<LoadChargeRuleResult> LoadChargeRuleUnselected(LoadChargeRuleSearch searchEntity, out int total);
        List<LoadChargeDetailResult> LoadChargeRuleSelected(LoadChargeRuleSearch searchEntity, out int total);

        //Load出货规则验证
        string LoadChargeRuleCheck(LoadChargeDetailInsert entity);

        //删除出货箱单明细
        string LoadChargeDetailDelById(int id);

        //仓库收费科目列表显示
        List<LoadContainerResult> LoadChargeDetailWarehouseList(LoadContainerSearch searchEntity, string[] containerNumber, out int total);

        //仓库收费科目输入数量列表显示
        List<LoadChargeDetailResult> LoadChargeRuleWarehouseSelected(LoadChargeRuleSearch searchEntity, out int total);

        //仓库录入数量操作
        string LoadContainerWarehouseEdit(string whCode, string loadId, string[] id, string[] qtycbm);

        //检测出货项目是否全部录入完成，必选项是否遗漏
        string LoadChargeDetailCheck(string whCode, string loadId);


        //根据客户自动计算某些科目
        string LoadChargeClientAutoCharge(LoadChargeDetailResult entity);


        //显示load下的所有费用科目及数量
        List<LoadChargeDetailResult> LoadChargeDetailList(LoadChargeRuleSearch searchEntity, out int total);

        //修改出货费用状态为确认
        string LoadChargeEdit(string whCode, string loadId, string status);

        //重新计算基础费用
        string AgainLoadCharge(string whCode, string loadId, string userName);

        //代垫列表
        List<LoadChargeDaiDian> DaiDianSelectList();

        //特费科目列表
        List<LoadChargeRule> LoadChargeRuleSelectList();

        //代垫列表
        List<LoadChargeDaiDian> LoadChargeDaiDianSelectList();

        //SO特费科目添加
        string LoadChargeRuleAdd(LoadChargeRule entity);

        //编辑SO特费科目
        string LoadChargeRuleEdit(LoadChargeRule entity);

        //添加SO特费
        string LoadChargeDetailAddBySO(LoadChargeDetail entity);

        //编辑SO特费
        string LoadChargeDetailEditBySO(LoadChargeDetail entity);

        //查询SO特费列表
        List<LoadChargeDetailResult> LoadChargeDetailSOList(LoadChargeDetailSearch searchEntity, string[] soList, out int total);

        //删除SO特费
        string LoadChargeDetailSoDel(int id, string userName);

        //科目类型下拉列表
        IEnumerable<LoadChargeRuleResult> GetLoadChangeRuleTypeName();

        //得到确认箱单费用异常数量
        List<LoadChargeDetailResult> CheckLoadChangeErrorCount(LoadContainerSearch searchEntity, out int total);

        //查询释放异常列表
        List<ReleaseLoadDetail> ReleaseLoadDetailList(LoadHuIdExtendSearch searchEntity, out int total);


        #region  特殊功能：SN批量导入一键创建Load并释放 更换、删除、新增其它托盘
        //SN批量导入一键创建Load并释放
        string ImportsOutBoundOrderExtendBySN(List<OutBoundOrderExtendInsert> entityList);


        //备货单信息显示
        List<PickTaskDetailResult2> PickTaskDetailList(PickTaskDetailSearch searchEntity, out int total);


        //删除托盘
        string PickTaskDetailHuIdDel(PickTaskDetailResult2 entity);

        //新增托盘
        string PickTaskDetailHuIdAdd(PickTaskDetailResult2 entity);

        #endregion


        #region 客户装箱单数据处理

        List<ExcelImportOutBound> CottonExcelImportOutBoundList(ExcelImportOutBoundSearch searchEntity, out int total, out string str);


        string ExcelImportOutBoundCotton(List<ExcelImportOutBound> entity);


        #endregion

    }
}
