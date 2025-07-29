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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“InBoundService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 InBoundService.svc 或 InBoundService.svc.cs，然后开始调试。
    public class InBoundService : IInBoundService
    {
        IBLL.IInBoundOrderManager inBoundOrder = new BLL.InBoundOrderManager();
        IBLL.IInBoundOrderDetailService inBoundOrderDetail = new BLL.InBoundOrderDetailService();
        IBLL.IRegInBoundOrderManager regInBound = new BLL.RegInBoundOrderManager();
        IBLL.ITruckManager truck = new BLL.TruckManager();

        #region 1.预录入

        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {
            return inBoundOrder.WhClientListSelect(whCode);
        }

        //客户拥有流程 下拉列表
        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string soNumber, string customerPo, string orderType)
        {
            return inBoundOrder.ClientFlowNameSelect(whCode, clientId, soNumber, customerPo, orderType);
        }

        public string InBoundOrderListAdd(InBoundOrderInsert entity)
        {
            return inBoundOrder.InBoundOrderListAdd(entity);
        }

        public List<InBoundOrderDetailResult> InBoundOrderDetailList(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] itemNumberList, out int total, out string str)
        {
            return inBoundOrder.InBoundOrderDetailList(searchEntity, poNumberList, itemNumberList, out total, out str);
        }

        public string InBoundOrderDetailEdit(InBoundOrderDetail entity, params string[] modifiedProNames)
        {
            return inBoundOrder.InBoundOrderDetailEdit(entity, modifiedProNames);
        }


        //预录入修改客户名
        public string InBoundOrderEditClientCode(EditInBoundResult entity, string[] soList, string[] clientCodeList)
        {
            return inBoundOrder.InBoundOrderEditClientCode(entity, soList, clientCodeList);
        }

        public List<InBoundOrderResult> InBoundListDetail(InBoundOrderSearch searchEntity, string[] soList, out int total)
        {
            return inBoundOrder.InBoundListDetail(searchEntity, soList, out total);
        }

        public string InBoundOrderDetailDel(int id, string userName)
        {
            return inBoundOrder.InBoundOrderDetailDel(id, userName);
        }

        //删除预录入
        public string DeleteInBound(string whCode, string clientCode, string customerPoNumber, string soNumber)
        {
            return inBoundOrder.DeleteInBound(whCode, clientCode, customerPoNumber, soNumber);
        }

        //一键删除SO预录入
        public string DelInBoundBySO(string whCode, string clientCode, string soNumber)
        {
            return inBoundOrder.DelInBoundBySO(whCode, clientCode, soNumber);
        }

        #endregion

        #region 2.DC预录入
        //DC预录入明细列表
        public List<InBoundOrderDetailResult> InBoundOrderDetailListDC(InBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total, out string str)
        {
            return inBoundOrder.InBoundOrderDetailListDC(searchEntity, itemNumberList, out total, out str);
        }

        //DC批量添加预录入
        public string InBoundOrderListAddDC(InBoundOrderInsert entity)
        {
            return inBoundOrder.InBoundOrderListAddDC(entity);
        }
        #endregion

        #region 3.通用预录入

        //DC批量添加预录入
        public string InBoundOrderListAddCommon(InBoundOrderInsert entity)
        {
            return inBoundOrder.InBoundOrderListAddCommon(entity);
        }

        //通过款号得到单位
        public IEnumerable<UnitsResult> GetUnitSelList(UnitsSearch entity)
        {
            return inBoundOrder.GetUnitSelList(entity);
        }

        //通过款号 验证单位 是否有误
        public string CheckUnitName(InBoundOrderInsert entity)
        {
            return inBoundOrder.CheckUnitName(entity);
        }

        #endregion

        #region 4.电商入库订单导入

        public string InBoundOrderListAddEcl(InBoundOrderInsert entity)
        {
            return inBoundOrder.InBoundOrderListAddEcl(entity);
        }


        #endregion

        #region 5.收货登记 

        //收货门区下拉列表
        public IEnumerable<WhZoneResult> RecZoneSelect(string whCode, int clientId)
        {
            return regInBound.RecZoneSelect(whCode, clientId);
        }

        //客户流程下拉列表
        public IEnumerable<BusinessFlowGroupResult> RegClientFlowNameSelect(int clientId)
        {
            return regInBound.RegClientFlowNameSelect(clientId);
        }

        //收货登记查询
        public List<ReceiptRegisterResult> ReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total)
        {
            return regInBound.ReceiptRegisterList(searchEntity, out total);
        }

        //添加进仓单(整出)  
        public List<InBoundOrderDetailResult> RegInBoundOrderListCFS(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            return regInBound.RegInBoundOrderListCFS(searchEntity, soNumberList, poNumberList, skuNumberList, out total, out str);
        }

        public ReceiptRegister AddReceiptRegister(ReceiptRegister entity)
        {
            return regInBound.AddReceiptRegister(entity);
        }

        public ReceiptRegister AddReceiptRegister1(ReceiptRegister entity)
        {
            return regInBound.AddReceiptRegister1(entity);
        }

        //收货登记添加明细
        public string AddReceiptRegisterDetail(List<ReceiptRegisterInsert> listEntity)
        {
            return regInBound.AddReceiptRegisterDetail1(listEntity);

        }

        //收货批次明细查询
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            return regInBound.ReceiptRegisterDetailList(searchEntity, out total, out str);
        }


        //修改收货登记明细 
        public string EditReceiptRegisterDetail(ReceiptRegisterDetailEdit entity)
        {
            return regInBound.EditReceiptRegisterDetail(entity);
        }


        //删除收货登记明细 
        public string DelReceiptRegisterDetail(ReceiptRegisterDetailEdit entity)
        {
            return regInBound.DelReceiptRegisterDetail(entity);
        }

        //删除收货登记 
        public string DelReceiptRegister(ReceiptRegister entity)
        {
            return regInBound.DelReceiptRegister(entity);
        }

        //修改收货登记 
        public string EditReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames)
        {
            return regInBound.EditReceiptRegister1(entity, modifiedProNames);
        }


        //验证收货批次流程是否选择有误
        public string CheckReceiptProssName(ReceiptRegisterInsert entity)
        {
            return regInBound.CheckReceiptProssName(entity);
        }


        //添加进仓单(非整出)  
        public List<InBoundOrderDetailResult> RegInBoundOrderListDC(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            return regInBound.RegInBoundOrderListDC(searchEntity, poNumberList, skuNumberList, out total, out str);
        }

        //显示收货批次明细(非整出)
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailPartList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            return regInBound.ReceiptRegisterDetailPartList(searchEntity, out total, out str);
        }

        //添加进仓单 查询(通用)  
        public List<InBoundOrderDetailResult> RegInBoundOrderListCom(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            return regInBound.RegInBoundOrderListCom(searchEntity, soNumberList, poNumberList, skuNumberList, out total, out str);
        }


        //显示收货批次明细 （通用）
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailListCom(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            return regInBound.ReceiptRegisterDetailListCom(searchEntity, out total, out str);
        }


        //释放收货批次号 验证 是否有直装
        public string ReleaseReceipt(string whCode, string receiptId)
        {
            return regInBound.ReleaseReceipt(whCode, receiptId);
        }

        //释放成功后 根据车牌号查询列表
        public List<ReceiptRegisterResult> DSReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total)
        {
            return regInBound.DSReceiptRegisterList(searchEntity, out total);
        }

        //撤销释放  WMS使用
        public string RollbackReceiptId(ReceiptRegister entity)
        {
            return regInBound.RollbackReceiptId(entity);
        }

        //更新收货批次联单数
        public string UpdateRegReceiptBillCount(ReceiptRegister entity)
        {
            return regInBound.UpdateRegReceiptBillCount(entity);
        }


        #endregion

        #region 7.批量导入预录入
        //验证客户收货流程是否一致
        public List<FlowHeadResult> CheckClientCodeRule(string whCode, List<WhClient> client)
        {
            return inBoundOrder.CheckClientCodeRule(whCode, client);
        }

        //批量导入预录入
        public string ImportsInBoundOrder(List<InBoundOrderInsert> entityList)
        {
            return inBoundOrder.ImportsInBoundOrder(entityList);
        }

        public string ImportsInBoundOrderExtend(List<InBoundOrderInsert> entityList)
        {
            return inBoundOrder.ImportsInBoundOrderExtend(entityList);
        }

        #endregion

        #region 8.修改客户名SO等基础信息

        //查询列表，显示客户名、SO
        public List<EditInBoundResult> EditInBoundClientCodeList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundClientCodeList(searchEntity, out total);
        }

        //查询列表，显示SO\PO
        public List<EditInBoundResult> EditInBoundCustomerPoList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundCustomerPoList(searchEntity, out total);
        }

        //查询列表，显示SO\PO\款号
        public List<EditInBoundResult> EditInBoundAltItemNumberList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundAltItemNumberList(searchEntity, out total);
        }


        //查询列表，显示收货明细及单位
        public List<ReceiptResult> EditUnitNameList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditUnitNameList(searchEntity, out total);
        }

        //查询列表，显示收货PO含托盘
        public List<ReceiptResult> EditInBoundCustPoByHuList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundCustPoByHuList(searchEntity, out total);
        }

        //查询列表，显示收货POSKU含托盘 
        public List<ReceiptResult> EditInBoundAltItemByHuList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundAltItemByHuList(searchEntity, out total);
        }

        //查询列表，显示收货POSKU属性含托盘
        public List<ReceiptResult> EditInBoundAltItemStyleByHuList(EditInBoundSearch searchEntity, out int total)
        {
            return inBoundOrder.EditInBoundAltItemStyleByHuList(searchEntity, out total);
        }

        //修改收货客户名
        public string EditDetailClientCode(EditInBoundResult entity, string[] soList)
        {
            return inBoundOrder.EditDetailClientCode(entity, soList);
        }

        //修改SO
        public string EditDetailSoNumber(EditInBoundResult entity, string[] soList)
        {
            return inBoundOrder.EditDetailSoNumber(entity, soList);
        }

        //修改PO
        public string EditDetailCustomerPoNumber(EditInBoundResult entity, string[] soList, string[] poList)
        {
            return inBoundOrder.EditDetailCustomerPoNumber(entity, soList, poList);
        }

        //修改款号
        public string EditDetailAltItemNumber(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList)
        {
            return inBoundOrder.EditDetailAltItemNumber(entity, soList, poList, itemList);
        }

        //修改单位
        public string EditDetailUnitName(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] unitNameList)
        {
            return inBoundOrder.EditDetailUnitName(entity, soList, poList, itemList, unitNameList);
        }

        //修改SO By托盘
        public string EditDetailSoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList)
        {
            return inBoundOrder.EditDetailSoNumberByHu(entity, soList, poList, huList);
        }

        //修改PO By托盘
        public string EditDetailCustomerPoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList)
        {
            return inBoundOrder.EditDetailCustomerPoNumberByHu(entity, soList, poList, huList);
        }

        //修改款号 By托盘
        public string EditDetailAltItemByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] huList)
        {
            return inBoundOrder.EditDetailAltItemByHu(entity, soList, poList, itemList, huList);
        }

        //修改款号的属性 By托盘
        public string EditDetailAltItemStyleByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] style1List, string[] style2List, string[] style3List, string[] huList)
        {
            return inBoundOrder.EditDetailAltItemStyleByHu(entity, soList, poList, itemList, style1List, style2List, style3List, huList);
        }

        #endregion


        #region 9.车辆排队管理


        //货代下拉菜单列表
        public IEnumerable<WhAgent> WhAgentListSelect(string whCode)
        {
            return truck.WhAgentListSelect(whCode);
        }

        //车辆排队列表
        public List<ReceiptRegisterResult> TruckQueryList(ReceiptRegisterSearch searchEntity, out int total)
        {
            return truck.TruckQueryList(searchEntity, out total);
        }

        //车辆放车
        public string EditTruckStatus(string whCode, string receiptId, string userName)
        {
            return truck.EditTruckStatus(whCode, receiptId, userName);
        }

        //车辆插队
        public string QueueUpTruck(string whCode, string receiptId, string userName, string remark)
        {
            return truck.QueueUpTruck(whCode, receiptId, userName, remark);
        }

        //删除收货登记 排队系统使用
        public string DelReceiptRegisterByTruck(ReceiptRegister entity)
        {
            return truck.DelReceiptRegisterByTruck(entity);
        }

        #endregion


        #region 10.入库导入SN

        public List<SerialNumberInOutExtend> InBoundOrderExtendList(InBoundOrderExntedSearch searchEntity, out int total)
        {
            return inBoundOrder.InBoundOrderExtendList(searchEntity, out total);
        }


        //收货操作单模版
        public string PrintInTempalte(string whCode, string receiptId)
        {
            return regInBound.PrintInTempalte(whCode, receiptId);
        }

        //收货单位下拉菜单列表
        public IEnumerable<UnitDefault> UnitDefaultListSelect(string whCode)
        {
            return inBoundOrder.UnitDefaultListSelect(whCode);
        }

        #endregion


        #region 11.forecast
        public List<ExcelImportInBound> ExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            return inBoundOrder.ExcelImportInBoundList(searchEntity, out total, out str);
        }


        public string checkSONumber(List<ExcelImportInBoundSo> checkSoList)
        {
            return inBoundOrder.checkSONumber(checkSoList);
        }

        //forecast 导入
        public string importForecast(List<ExcelImportInBound> checkSoList)
        {
            return inBoundOrder.importForecast(checkSoList);
        }

        public string ExcelImportInBoundEdit(ExcelImportInBound entity)
        {
            return inBoundOrder.ExcelImportInBoundEdit(entity);
        }

        //删除预录入
        public string ExcelImportInBoundDelete(string whCode, string clientCode, string soNumber)
        {
            return inBoundOrder.ExcelImportInBoundDelete(whCode, clientCode, soNumber);
        }

        #endregion


        #region 12.Kamrt

        public List<KmartReceiptDelay> KmartReceiptDelayList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            return inBoundOrder.KmartReceiptDelayList(searchEntity, out total, out str);
        }


        //Kmart 收货延时原因维护
        public string KmartDelayReason(ReceiptRegisterExtend entity)
        {
            return inBoundOrder.KmartDelayReason(entity);
        }

        public IEnumerable<KmartWaitingReason> KmartReasontSelect()
        {
            return inBoundOrder.KmartReasontSelect();
        }


        //Kmart打托列表查询
        public List<KmartHeavyPalletResult> KmartHeavyPalletList(KmartHeavyPalletSearch searchEntity, string[] soNumber, out int total)
        {
            return inBoundOrder.KmartHeavyPalletList(searchEntity, soNumber, out total);
        }

        //Kmart打托修改长宽高
        public string KmartHeavyPalletEdit(KmartHeavyPalletResult entity)
        {
            return inBoundOrder.KmartHeavyPalletEdit(entity);
        }

        #endregion

        #region 13.Cotton导入Forecast

        //forecast 列表查询
        public List<ExcelImportInBoundCotton> CottonExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            return inBoundOrder.CottonExcelImportInBoundList(searchEntity, out total, out str);
        }

        public string ExcelImportForecastCotton(List<ExcelImportInBoundCotton> entityList)
        {
            return inBoundOrder.ExcelImportForecastCotton(entityList);
        }


        public string EditForecastCotton(ExcelImportInBoundCotton entityCotton)
        {
            return inBoundOrder.EditForecastCotton(entityCotton);
        }


        #endregion


        #region 14.Mosaic导入Forecast

        //forecast 列表查询
        public List<ExcelImportInBoundCom> MosaicExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int
total, out string str)
        {
            return inBoundOrder.MosaicExcelImportInBoundList(searchEntity, out total, out str);
        }

        //Excel导入Forecast通用方法
        public string ExcelImportForecastCommon(List<ExcelImportInBoundCom> entityList)
        {
            return inBoundOrder.ExcelImportForecastCommon(entityList);
        }


        #endregion

    }
}
