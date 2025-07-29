using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IInterceptManager
    {
        string OutBoundOrderIntercept(string whCode, string customerOutPoNumber, string clientCode, string userName);

        //拦截订单查询 
        List<OutBoundOrderResult1> InterceptOutBoundOrderList(OutBoundOrderSearch1 searchEntity, out int total);

        //得到客户下的所有收货流程
        IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId);

        //重新生成收货操作单
        string AddReceiptIdByInterceptOrder(int outBoundOrderId, int processId, string processName, string recLocationId, string userName, string abLocation);

        //批量生成拦截收货操作单
        string AddReceiptIdByInterceptOrderList(int[] outBoundOrderIdList, string whCode, string userName, string abLocation);

        //订单处理完成 确认
        string CheckInterceptOrder(int outBoundOrderId, string userName);

        //通过系统出库订单号查找收货批次号
        string GetInterceptOrderReceiptId(string OutPoNumber, string whCode);

        //拦截或退货收货方法
        string ReceiptInsertByOther(ReceiptInsert entity);

        //Location为当前库位,DestLoc为上架库位
        string InterceptNoHuIdByHuDetail(string WhCode, string Location, string DestLoc, string HuId, string User);


        //拦截订单批量回库
        string InterceptOrderBatchReturnToWarehouse(int outBoundOrderIdList, string whCode, string userName, string abLocation);

    }
}
