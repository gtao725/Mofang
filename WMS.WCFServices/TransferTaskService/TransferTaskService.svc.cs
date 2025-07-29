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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“TransferTaskService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 TransferTaskService.svc 或 TransferTaskService.svc.cs，然后开始调试。
    public class TransferTaskService : ITransferTaskService
    {
        IBLL.ITransferTaskManager transfer = new BLL.TransferTaskManager();

        public List<TransferHeadResult> GetExpressNumberList(string transferNumber, string whCode)
        {
            return transfer.GetExpressNumberList(transferNumber, whCode);
        }

        //获取交接框号已扫描的出库单列表
        public List<TransferHeadResult> GetCustomerOutPoNumberList(string transferNumber, string whCode)
        {
            return transfer.GetCustomerOutPoNumberList(transferNumber, whCode);
        }

        //交接工作台
        //工作台中 扫描快递单号后 验证是否存在 及插入数据等
        public string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode)
        {
            return transfer.TransferTaskInsert(transferNumber, userName, expressNumber, whCode);
        }

        //确认交接
        //交接时 减去对应库存 更新订单状态
        public string BeginTransferTask(int transferId, string userName, int fayunQty)
        {
            return transfer.BeginTransferTask(transferId, userName, fayunQty);
        }

        //交接工作台
        //出库订单交接
        public string TransferTaskOutBoundOrderInsert(string transferNumber, string userName, string customerOutPoNumber, string whCode)
        {
            return transfer.TransferTaskOutBoundOrderInsert(transferNumber, userName, customerOutPoNumber, whCode, "work");
        }

        //查看交接任务
        public List<TransferTaskResult> GetTransferTaskList(TransferTaskSearch entity, out int total)
        {
            return transfer.GetTransferTaskList(entity, out total);
        }

        //查询交接任务
        public List<TransferTaskResult> GetTransferTaskOrderList(TransferTaskSearch entity, out int total)
        {
            return transfer.GetTransferTaskOrderList(entity, out total);
        }

        //查询 交接任务的交接明细
        public List<TransferHeadDetailResult> GetTransferTaskDetailList(TransferHeadDetailSearch searchEntity, out int total)
        {
            return transfer.GetTransferTaskDetailList(searchEntity, out total);
        }



        //根据交接框号 删除交接
        public string TransferTaskDelete(int transferTaskId)
        {
            return transfer.TransferTaskDelete(transferTaskId);
        }

        //删除交接头信息
        public string TransferHeadDelete(int id)
        {
            return transfer.TransferHeadDelete(id);
        }

        //根据客户出库单号 删除交接信息
        public string TransferHeadDeleteByOrder(int transferTaskId, string customerOutPoNumber)
        {
            return transfer.TransferHeadDeleteByOrder(transferTaskId, customerOutPoNumber);
        }


        //交接单报表查询
        public List<TransferReportResult> GetTransferReportList(int tid)
        {
            return transfer.GetTransferReportList(tid);
        }


        //快递单未交接查询
        public List<UnTransferExpressNumberResult> GetUnTransferExpressNumberList(UnTransferExpressNumberSearch entity, out int total)
        {
            return transfer.GetUnTransferExpressNumberList(entity, out total);
        }
    }
}
