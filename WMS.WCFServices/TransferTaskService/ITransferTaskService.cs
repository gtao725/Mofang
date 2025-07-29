using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.TransferTaskService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ITransferTaskService”。
    [ServiceContract]
    public interface ITransferTaskService
    {

        [OperationContract]
        //获取交接框号已扫描的快递单列表
        List<TransferHeadResult> GetExpressNumberList(string transferNumber, string whCode);

        [OperationContract]
        //获取交接框号已扫描的出库单列表
        List<TransferHeadResult> GetCustomerOutPoNumberList(string transferNumber, string whCode);

        [OperationContract]
        //交接工作台
        //工作台中 扫描快递单号后 验证是否存在 及插入数据等
        string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode);

        [OperationContract]
        //确认交接
        //交接时 减去对应库存 更新订单状态
        string BeginTransferTask(int transferId, string userName, int fayunQty);

        [OperationContract]
        //交接工作台
        //出库订单交接
        string TransferTaskOutBoundOrderInsert(string transferNumber, string userName, string customerOutPoNumber, string whCode);

        [OperationContract]
        //查看交接任务
        List<TransferTaskResult> GetTransferTaskList(TransferTaskSearch entity, out int total);

        [OperationContract]
        //查询交接任务
        List<TransferTaskResult> GetTransferTaskOrderList(TransferTaskSearch entity, out int total);

        [OperationContract]
        //查询 交接任务的交接明细
        List<TransferHeadDetailResult> GetTransferTaskDetailList(TransferHeadDetailSearch searchEntity, out int total);


        [OperationContract]
        //根据交接框号 删除交接
        string TransferTaskDelete(int transferTaskId);

        [OperationContract]
        //删除交接头信息
        string TransferHeadDelete(int id);

        [OperationContract]
        //删除交接信息
        string TransferHeadDeleteByOrder(int transferTaskId, string customerOutPoNumber);

        [OperationContract]
        //交接单报表查询
        List<TransferReportResult> GetTransferReportList(int tid);

        [OperationContract]
        //快递单未交接查询
        List<UnTransferExpressNumberResult> GetUnTransferExpressNumberList(UnTransferExpressNumberSearch entity, out int total);

    }
}
