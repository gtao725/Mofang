using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.GMSService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IPackTaskService”。
    [ServiceContract]
    public interface IGMSService
    {

        [OperationContract]
        //放车
        string ReleaseTruck(QueueParam queueParam);
        [OperationContract]
        //删除车辆排队信息
        string DeleteTruckInfo(QueueParam queueParam);
        [OperationContract]
        //删除车牌排队明细信息
        string DeleteTruckDetail(QueueParam queueParam);
        [OperationContract]
        //新增车辆表头信息
        string AddTruckQueueHead(TruckQueueHeadParam truckQueueHeadParam);
        [OperationContract]
        ////预约通道获取
        IEnumerable<LookUp> BookChannelSelect(string whCode);
        [OperationContract]
        //新增车辆明细信息
        string AddTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam);
        [OperationContract]
        //修改车辆表头信息
        string UpdateTruckQueueHeader(TruckQueueHeadParam truckQueueHeadParam);
        [OperationContract]
        //修改明细
        string UpdateTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam);
        [OperationContract]
        //超时锁定车辆解锁
        string unlockTruck(QueueParam queueParam);
        [OperationContract]
        //车辆入场抬杆
        string getInGate(ReceiptParam receiptParam);
        [OperationContract]
        //车辆入场抬杆
        string leaveGate(ReceiptParam receiptParam);
        [OperationContract]
        //小库区下拉列表
        IEnumerable<WhZoneResult> GetSmallLoadAreaList(string whCode, string UnloadingArea);

    }
}
