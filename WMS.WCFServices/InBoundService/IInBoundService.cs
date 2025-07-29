using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.InBoundService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IInBoundService”。
    [ServiceContract]
    public interface IInBoundService
    {
        #region 1.预录入

        [OperationContract]
        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        [OperationContract]
        //客户拥有流程 下拉列表
        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string soNumber, string customerPo, string orderType);

        [OperationContract]
        //CFS批量添加预录入
        string InBoundOrderListAdd(InBoundOrderInsert entity);

        [OperationContract]
        //CFS预录入明细列表
        List<InBoundOrderDetailResult> InBoundOrderDetailList(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] itemNumberList, out int total, out string str);

        [OperationContract]
        //CFS预录入信息修改
        string InBoundOrderDetailEdit(InBoundOrderDetail entity, params string[] modifiedProNames);

        [OperationContract]
        //预录入修改客户名
        string InBoundOrderEditClientCode(EditInBoundResult entity, string[] soList, string[] clientCodeList);

        [OperationContract]
        //编辑预录入查询列表
        List<InBoundOrderResult> InBoundListDetail(InBoundOrderSearch searchEntity, string[] soList, out int total);

        [OperationContract]
        //删除预录入明细
        string InBoundOrderDetailDel(int id, string userName);

        [OperationContract]
        //删除预录入
        string DeleteInBound(string whCode, string clientCode, string customerPoNumber, string soNumber);

        [OperationContract]
        //一键删除SO预录入
        string DelInBoundBySO(string whCode, string clientCode, string soNumber);

        #endregion


        #region 2.DC预录入

        [OperationContract]
        //DC预录入明细列表
        List<InBoundOrderDetailResult> InBoundOrderDetailListDC(InBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total, out string str);

        [OperationContract]
        //DC批量添加预录入
        string InBoundOrderListAddDC(InBoundOrderInsert entity);

        #endregion


        #region 3.通用预录入

        [OperationContract]
        //DC批量添加预录入
        string InBoundOrderListAddCommon(InBoundOrderInsert entity);

        [OperationContract]
        //通过款号得到单位
        IEnumerable<UnitsResult> GetUnitSelList(UnitsSearch entity);

        [OperationContract]
        //通过款号 验证单位 是否有误
        string CheckUnitName(InBoundOrderInsert entity);

        #endregion


        #region 4.通用预录入

        [OperationContract]
        //DC批量添加预录入
        string InBoundOrderListAddEcl(InBoundOrderInsert entity);

        #endregion


        #region 5.收货登记 

        [OperationContract]
        //收货登记区域下拉列表
        IEnumerable<WhZoneResult> RecZoneSelect(string whCode, int clientId);

        [OperationContract]
        //客户流程下拉列表
        IEnumerable<BusinessFlowGroupResult> RegClientFlowNameSelect(int clientId);

        [OperationContract]
        //收货登记查询
        List<ReceiptRegisterResult> ReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total);

        [OperationContract]
        //添加进仓单(整出)  
        List<InBoundOrderDetailResult> RegInBoundOrderListCFS(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        [OperationContract]
        //CFS 收货登记
        ReceiptRegister AddReceiptRegister(ReceiptRegister entity);

        [OperationContract]
        ReceiptRegister AddReceiptRegister1(ReceiptRegister entity);

        [OperationContract]
        //CFS 收货登记添加明细
        string AddReceiptRegisterDetail(List<ReceiptRegisterInsert> listEntity);


        [OperationContract]
        //收货批次明细
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //修改收货登记明细 
        string EditReceiptRegisterDetail(ReceiptRegisterDetailEdit entity);

        [OperationContract]
        //删除收货登记明细 
        string DelReceiptRegisterDetail(ReceiptRegisterDetailEdit entity);

        [OperationContract]
        //删除收货登记 
        string DelReceiptRegister(ReceiptRegister entity);

        [OperationContract]
        //修改收货登记 
        string EditReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames);

        [OperationContract]
        //验证收货批次流程是否选择有误
        string CheckReceiptProssName(ReceiptRegisterInsert entity);



        [OperationContract]
        //添加进仓单(非整出)  
        List<InBoundOrderDetailResult> RegInBoundOrderListDC(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        [OperationContract]
        //显示收货批次明细(非整出)
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailPartList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);

        [OperationContract]
        //添加进仓单 查询(通用)  
        List<InBoundOrderDetailResult> RegInBoundOrderListCom(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        [OperationContract]
        //显示收货批次明细 （通用）
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailListCom(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);


        //释放收货批次号 验证 是否有直装
        [OperationContract]
        string ReleaseReceipt(string whCode, string receiptId);

        [OperationContract]
        //释放成功后 根据车牌号查询列表
        List<ReceiptRegisterResult> DSReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total);

        [OperationContract]
        //撤销释放  WMS使用
        string RollbackReceiptId(ReceiptRegister entity);

        [OperationContract]
        //更新收货批次联单数
        string UpdateRegReceiptBillCount(ReceiptRegister entity);

        #endregion


        #region 7.批量导入预录入

        [OperationContract]
        //验证客户收货流程是否一致
        List<FlowHeadResult> CheckClientCodeRule(string whCode, List<WhClient> client);

        //批量导入预录入
        [OperationContract]
        string ImportsInBoundOrder(List<InBoundOrderInsert> entityList);

        [OperationContract]
        string ImportsInBoundOrderExtend(List<InBoundOrderInsert> entityList);

        #endregion


        #region 8.修改客户名SO等信息

        [OperationContract]
        //查询列表，显示客户名、SO
        List<EditInBoundResult> EditInBoundClientCodeList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示SO\PO
        List<EditInBoundResult> EditInBoundCustomerPoList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示SO\PO\款号
        List<EditInBoundResult> EditInBoundAltItemNumberList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示收货明细及单位
        List<ReceiptResult> EditUnitNameList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示收货PO含托盘
        List<ReceiptResult> EditInBoundCustPoByHuList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示收货POSKU含托盘
        List<ReceiptResult> EditInBoundAltItemByHuList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //查询列表，显示收货POSKU属性含托盘
        List<ReceiptResult> EditInBoundAltItemStyleByHuList(EditInBoundSearch searchEntity, out int total);

        [OperationContract]
        //修改收货客户名
        string EditDetailClientCode(EditInBoundResult entity, string[] soList);

        [OperationContract]
        //修改SO
        string EditDetailSoNumber(EditInBoundResult entity, string[] soList);

        [OperationContract]
        //修改PO
        string EditDetailCustomerPoNumber(EditInBoundResult entity, string[] soList, string[] poList);

        [OperationContract]
        //修改款号
        string EditDetailAltItemNumber(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList);

        [OperationContract]
        //修改单位
        string EditDetailUnitName(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] unitNameList);

        [OperationContract]
        //修改SO By托盘
        string EditDetailSoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList);

        [OperationContract]
        //修改PO By托盘
        string EditDetailCustomerPoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList);

        [OperationContract]
        //修改款号 By托盘
        string EditDetailAltItemByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] huList);

        [OperationContract]
        //修改款号的属性 By托盘
        string EditDetailAltItemStyleByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] style1List, string[] style2List, string[] style3List, string[] huList);

        #endregion


        #region 9.车辆排队管理

        [OperationContract]
        //货代下拉菜单列表
        IEnumerable<WhAgent> WhAgentListSelect(string whCode);

        [OperationContract]
        //车辆排队列表
        List<ReceiptRegisterResult> TruckQueryList(ReceiptRegisterSearch searchEntity, out int total);

        [OperationContract]
        //车辆放车
        string EditTruckStatus(string whCode, string receiptId, string userName);

        [OperationContract]
        //车辆插队
        string QueueUpTruck(string whCode, string receiptId, string userName, string remark);

        [OperationContract]
        //删除收货登记 排队系统使用
        string DelReceiptRegisterByTruck(ReceiptRegister entity);

        #endregion


        #region 10.入库导入SN

        [OperationContract]
        List<SerialNumberInOutExtend> InBoundOrderExtendList(InBoundOrderExntedSearch searchEntity, out int total);

        [OperationContract]
        //收货单位下拉菜单列表
        IEnumerable<UnitDefault> UnitDefaultListSelect(string whCode);

        [OperationContract]
        //收货操作单模版
        string PrintInTempalte(string whCode, string receiptId);

        #endregion


        #region 11.forecast

        [OperationContract]
        List<ExcelImportInBound> ExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        [OperationContract]
        string checkSONumber(List<ExcelImportInBoundSo> checkSoList);


        [OperationContract]
        //forecast 导入

        string importForecast(List<ExcelImportInBound> checkSoList);
        #endregion


        #region 12.Kmart

        [OperationContract]
        //编辑
        string ExcelImportInBoundEdit(ExcelImportInBound entity);

        [OperationContract]
        //删除
        string ExcelImportInBoundDelete(string whCode, string clientCode, string soNumber);


        [OperationContract]
        List<KmartReceiptDelay> KmartReceiptDelayList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        [OperationContract]
        //Kmart 收货延时原因维护
        string KmartDelayReason(ReceiptRegisterExtend entity);

        [OperationContract]
        IEnumerable<KmartWaitingReason> KmartReasontSelect();

        [OperationContract]
        //Kmart打托列表查询
        List<KmartHeavyPalletResult> KmartHeavyPalletList(KmartHeavyPalletSearch searchEntity, string[] soNumber, out int total);

        [OperationContract]
        //Kmart打托修改长宽高
        string KmartHeavyPalletEdit(KmartHeavyPalletResult entity);


        #endregion


        #region 13.Cotton导入Forecast

        [OperationContract]
        //forecast 列表查询
        List<ExcelImportInBoundCotton> CottonExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        [OperationContract]
        string ExcelImportForecastCotton(List<ExcelImportInBoundCotton> entityList);

        [OperationContract]
        string EditForecastCotton(ExcelImportInBoundCotton entityCotton);

        #endregion


        #region 14.Mosaic导入Forecast

        [OperationContract]
        //forecast 列表查询
        List<ExcelImportInBoundCom> MosaicExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int
total, out string str);

        [OperationContract]
        //Excel导入Forecast通用方法
        string ExcelImportForecastCommon(List<ExcelImportInBoundCom> entityList);

        #endregion
    }
}
