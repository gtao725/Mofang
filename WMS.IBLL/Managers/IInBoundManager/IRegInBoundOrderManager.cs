using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IRegInBoundOrderManager
    {

        #region 1.收货登记 

        //收货登记区域下拉列表
        IEnumerable<WhZoneResult> RecZoneSelect(string whCode, int clientId);

        //客户流程下拉列表
        IEnumerable<BusinessFlowGroupResult> RegClientFlowNameSelect(int clientId);

        //收货登记查询
        //对应RegInBoundOrderController中的 List 方法
        List<ReceiptRegisterResult> ReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total);


        //添加进仓单(整出)  
        //对应RegInBoundOrderController中的 List_CFS 方法
        List<InBoundOrderDetailResult> RegInBoundOrderListCFS(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        //CFS 收货登记
        //对应RegInBoundOrderController中的 AddReceiptRegister 方法
        ReceiptRegister AddReceiptRegister(ReceiptRegister entity);

        ReceiptRegister AddReceiptRegister1(ReceiptRegister entity);

        //CFS 收货登记添加明细
        //对应RegInBoundOrderController中的 AddReceiptRegisterDetail 方法
        string AddReceiptRegisterDetail(List<ReceiptRegisterInsert> listEntity);

        string AddReceiptRegisterDetail1(List<ReceiptRegisterInsert> listEntity);


        //显示收货批次明细 (整出)
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailList 方法
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);


        //修改收货登记明细 
        //对应RegInBoundOrderController中的 EditReceiptRegisterDetail 方法
        string EditReceiptRegisterDetail(ReceiptRegisterDetailEdit entity);


        //删除收货登记明细 
        //对应RegInBoundOrderController中的 DelReceiptRegisterDetail 方法
        string DelReceiptRegisterDetail(ReceiptRegisterDetailEdit entity);


        //删除收货登记 
        //对应RegInBoundOrderController中的 DelReceiptRegister 方法
        string DelReceiptRegister(ReceiptRegister entity);


        //修改收货登记 
        //对应RegInBoundOrderController中的 EditReceiptRegister 方法
        string EditReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames);

        string EditReceiptRegister1(ReceiptRegister entity, params string[] modifiedProNames);

        //验证收货批次流程是否选择有误
        string CheckReceiptProssName(ReceiptRegisterInsert entity);


        //添加进仓单(非整出)  
        //对应RegInBoundOrderController中的 AddInBoundOrder 方法
        List<InBoundOrderDetailResult> RegInBoundOrderListDC(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        //显示收货批次明细(非整出)
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailPartList 方法
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailPartList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);


        //添加进仓单 查询(通用)  
        //对应RegInBoundOrderController中的 List_Com 方法
        List<InBoundOrderDetailResult> RegInBoundOrderListCom(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str);

        //显示收货批次明细 （通用）
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailListCom 方法
        List<ReceiptRegisterDetailResult> ReceiptRegisterDetailListCom(ReceiptRegisterDetailSearch searchEntity, out int total, out string str);

        #endregion

        //释放收货批次号 验证 是否有直装
        string ReleaseReceipt(string whCode, string receiptId);

        //释放成功后 根据车牌号查询列表
        List<ReceiptRegisterResult> DSReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total);

        //撤销释放  WMS使用
        string RollbackReceiptId(ReceiptRegister entity);

        //收货操作单模版
        string PrintInTempalte(string whCode, string receiptId);

        //更新收货批次联单数
        string UpdateRegReceiptBillCount(ReceiptRegister entity);

    }
}
