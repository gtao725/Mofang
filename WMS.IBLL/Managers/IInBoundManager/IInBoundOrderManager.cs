using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IInBoundOrderManager
    {
        #region 1.预录入

        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        //客户拥有流程 下拉列表
        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string soNumber, string customerPo, string orderType);

        //CFS批量添加预录入
        string InBoundOrderListAdd(InBoundOrderInsert entity);

        //CFS预录入明细列表
        //对应InBoundOrderController中的 List 方法
        List<InBoundOrderDetailResult> InBoundOrderDetailList(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] itemNumberList, out int total, out string str);

        //CFS预录入信息修改
        string InBoundOrderDetailEdit(InBoundOrderDetail entity, params string[] modifiedProNames);

        //CFS预录入信息删除
        string InBoundOrderDetailDel(int id,string userName);

        //预录入修改客户名
        string InBoundOrderEditClientCode(EditInBoundResult entity, string[] soList, string[] clientCodeList);

        //编辑预录入查询列表
        //对应InBoundOrderController中的 ListDetail 方法
        List<InBoundOrderResult> InBoundListDetail(InBoundOrderSearch searchEntity, string[] soList, out int total);

        //删除预录入
        string DeleteInBound(string whCode, string clientCode, string customerPoNumber, string soNumber);

        //一键删除SO预录入
        string DelInBoundBySO(string whCode, string clientCode, string soNumber);

        #endregion



        #region 2.DC预录入

        //DC预录入明细列表
        //对应InBoundOrderDCController中的 List 方法
        List<InBoundOrderDetailResult> InBoundOrderDetailListDC(InBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total, out string str);

        //DC批量添加预录入
        //对应InBoundOrderDCController中的 AddInBoundOrder 方法
        string InBoundOrderListAddDC(InBoundOrderInsert entity);


        #endregion



        #region 3.通用预录入

        //DC批量添加预录入
        //对应InBoundOrderDCController中的 AddInBoundOrder 方法
        string InBoundOrderListAddCommon(InBoundOrderInsert entity);

        //通过款号得到单位
        IEnumerable<UnitsResult> GetUnitSelList(UnitsSearch entity);

        //通过款号 验证单位 是否有误
        string CheckUnitName(InBoundOrderInsert entity);

        #endregion


        #region 4.电商入库单导入
        string InBoundOrderListAddEcl(InBoundOrderInsert entity);
        #endregion


        #region 6.删除入库订单
        string DeleteInorderBySO(string SoNumber, string WhCode, string ClientCode);


        #endregion


        #region 7.批量导入预录入
        //验证客户收货流程是否一致
        List<FlowHeadResult> CheckClientCodeRule(string whCode, List<WhClient> client);

        //批量导入预录入
        string ImportsInBoundOrder(List<InBoundOrderInsert> entityList);

        string ImportsInBoundOrderExtend(List<InBoundOrderInsert> entityList);


        #endregion


        #region 8.SO修改客户名基础信息

        //查询列表，显示客户名、SO
        List<EditInBoundResult> EditInBoundClientCodeList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示SO\PO
        List<EditInBoundResult> EditInBoundCustomerPoList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示SO\PO\款号
        List<EditInBoundResult> EditInBoundAltItemNumberList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示收货明细及单位
        List<ReceiptResult> EditUnitNameList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示收货PO含托盘 
        List<ReceiptResult> EditInBoundCustPoByHuList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示收货POSKU含托盘
        List<ReceiptResult> EditInBoundAltItemByHuList(EditInBoundSearch searchEntity, out int total);

        //查询列表，显示收货POSKU属性含托盘
        List<ReceiptResult> EditInBoundAltItemStyleByHuList(EditInBoundSearch searchEntity, out int total);

        //修改收货客户名
        string EditDetailClientCode(EditInBoundResult entity, string[] soList);

        //修改SO
        string EditDetailSoNumber(EditInBoundResult entity, string[] soList);

        //修改PO
        string EditDetailCustomerPoNumber(EditInBoundResult entity, string[] soList, string[] poList);

        //修改款号
        string EditDetailAltItemNumber(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList);


        //修改单位
        string EditDetailUnitName(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] unitNameList);

        //修改SO By托盘
        string EditDetailSoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList);

        //修改PO By托盘
        string EditDetailCustomerPoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList);

        //修改款号 By托盘
        string EditDetailAltItemByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] huList);

        //修改款号的属性 By托盘
        string EditDetailAltItemStyleByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] style1List, string[] style2List, string[] style3List, string[] huList);

        #endregion


        #region 9.预约单批量导入预录入 同时生成收货操作单

        string ImportsInBoundOrderAndReceiptByOrder(List<InBoundOrderInsert> entityList);

        //收货单位下拉菜单列表
        IEnumerable<UnitDefault> UnitDefaultListSelect(string whCode);


        #endregion


        #region 10.入库导入SN

        List<SerialNumberInOutExtend> InBoundOrderExtendList(InBoundOrderExntedSearch searchEntity, out int total);


        #endregion


        #region 11.forecast
        List<ExcelImportInBound> ExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        string checkSONumber(List<ExcelImportInBoundSo> checkSoList);
        //导入excel
        string importForecast(List<ExcelImportInBound> entity);
        //编辑
        string ExcelImportInBoundEdit(ExcelImportInBound entity);

        string ExcelImportInBoundDelete(string whCode, string clientCode, string soNumber);
        #endregion


        #region 12.Kmart 

        //收货延时报表
        List<KmartReceiptDelay> KmartReceiptDelayList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        //Kmart 收货延时原因维护
        string KmartDelayReason(ReceiptRegisterExtend entity);

        IEnumerable<KmartWaitingReason> KmartReasontSelect();

        //Kmart打托列表查询
        List<KmartHeavyPalletResult> KmartHeavyPalletList(KmartHeavyPalletSearch searchEntity, string[] soNumber, out int total);

        //Kmart打托修改长宽高
        string KmartHeavyPalletEdit(KmartHeavyPalletResult entity);


        #endregion



        #region 13.Cotton导入Forecast

        //forecast 列表查询
        List<ExcelImportInBoundCotton> CottonExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str);

        string ExcelImportForecastCotton(List<ExcelImportInBoundCotton> entityList);

        string EditForecastCotton(ExcelImportInBoundCotton entityCotton);


        #endregion



        #region 14.Mosaic导入Forecast

        //forecast 列表查询
        List<ExcelImportInBoundCom> MosaicExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int
total, out string str);

        //Excel导入Forecast通用方法
        string ExcelImportForecastCommon(List<ExcelImportInBoundCom> entityList);

        #endregion

    }
}
