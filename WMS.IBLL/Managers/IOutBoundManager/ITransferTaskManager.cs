using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ITransferTaskManager
    {

        //获取交接框号已扫描的快递单列表
        List<TransferHeadResult> GetExpressNumberList(string transferNumber, string whCode);

        //获取交接框号已扫描的出库单列表
        List<TransferHeadResult> GetCustomerOutPoNumberList(string transferNumber, string whCode);

        //交接工作台
        //工作台中 扫描快递单号后 验证是否存在 及插入数据等
        string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode);

        //确认交接
        //交接时 减去对应库存 更新订单状态
        string BeginTransferTask(int transferId, string userName, int fayunQty);

        //交接工作台
        //出库订单交接
        string TransferTaskOutBoundOrderInsert(string transferNumber, string userName, string customerOutPoNumber, string whCode, string workFlag);



        //查看交接任务
        List<TransferTaskResult> GetTransferTaskList(TransferTaskSearch entity, out int total);

        //查询交接任务
        List<TransferTaskResult> GetTransferTaskOrderList(TransferTaskSearch entity, out int total);

        //查询 交接任务的交接明细
        List<TransferHeadDetailResult> GetTransferTaskDetailList(TransferHeadDetailSearch searchEntity, out int total);


        //根据交接框号 删除交接
        string TransferTaskDelete(int transferTaskId);


        //删除交接信息
        string TransferHeadDeleteByOrder(int transferTaskId, string customerOutPoNumber);

        //删除交接头信息
        string TransferHeadDelete(int id);


        TransferTaskResultEcl GetTransferTaskEclResult(string whCode, string transferId);


        //交接单报表查询
        List<TransferReportResult> GetTransferReportList(int tid);

        //快递单未交接查询
        List<UnTransferExpressNumberResult> GetUnTransferExpressNumberList(UnTransferExpressNumberSearch entity, out int total);
    }
}
