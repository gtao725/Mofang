using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IOutBoundOrderManager
    {
        #region  1.出库订单录入

        //出库订单查询
        //对应 OutBoundOrderController 中的 List 方法
        List<OutBoundOrderResult> OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total);

        //根据选择的客户获得出货流程
        IEnumerable<FlowHead> OutFlowHeadListByClientId(string whCode, int clientId);

        //出库流程下拉列表
        IEnumerable<FlowHead> OutFlowHeadListSelect(string whCode);

        //状态查询下拉列表
        IEnumerable<LookUp> FlowRuleStatusListSelect();

        //出库订单添加
        //对应 OutBoundOrderController 中的 OutBoundOrderAdd 方法
        OutBoundOrder OutBoundOrderAdd(OutBoundOrder entity);

        //确认出库订单
        string ConfirmOutBoundOrder(OutBoundOrder eneity);

        //回滚出库订单
        string RollbackOutBoundOrder(OutBoundOrder eneity);

        //验证款号、款号对应的单位 是否有误
        string OutBoundCheckUnitName(OutBoundOrderDetailInsert entity);

        //出库订单明细添加
        //对应 OutBoundOrderController 中的 OutBoundOrderAdd 方法
        string OutBoundOrderDetailAdd(OutBoundOrderDetailInsert entity);

        //出库订单明细查询
        //对应 OutBoundOrderController 中的 OutBoundOrderDetailList 方法
        List<OutBoundOrderDetailResult> OutBoundOrderDetailList(OutBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total);

        //出库订单录入信息修改
        int OutBoundOrderDetailEdit(OutBoundOrderDetail entity, params string[] modifiedProNames);

        //出库订单删除 同时删除明细
        string OutBoundOrderDel(int id);

        #endregion


        string ImportsOutBoundOrder(List<ImportOutBoundOrder> entityList);

        //出库发货揽件异常登记
        string DeliveryExceptionRegister(string WhCode, string ExpressNumber, string user);
    }
}
