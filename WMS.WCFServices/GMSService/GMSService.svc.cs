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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“PackTaskService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 PackTaskService.svc 或 PackTaskService.svc.cs，然后开始调试。
    public class GMSService : IGMSService
    {
        IBLL.IGMSManager QueueList = new BLL.GMSManager();
        IBLL.IRootManager root = new BLL.RootManager();
        //放车
        public string ReleaseTruck(QueueParam queueParam)
        {
            return QueueList.ReleaseTruck(queueParam);
        }
        //删除车辆排队信息
        public string DeleteTruckInfo(QueueParam queueParam)
        {
            return QueueList.DeleteTruckInfo(queueParam);
        }
        //删除车牌排队明细信息
        public string DeleteTruckDetail(QueueParam queueParam)
        {
            return QueueList.DeleteTruckDetail(queueParam);
        }
        //新增车辆表头信息
        public string AddTruckQueueHead(TruckQueueHeadParam truckQueueHeadParam)
        {
            return QueueList.AddTruckQueueHead(truckQueueHeadParam);
        }
        //获取预约通道
        public IEnumerable<LookUp> BookChannelSelect(string whCode)
        {
            return QueueList.BookChannelSelect(whCode);
        }
        ////新增车辆明细信息
        public string AddTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam)
        {
            return QueueList.AddTruckQueueDetail(truckQueueDetailParam);
        }
        //修改车辆表头信息
        public string UpdateTruckQueueHeader(TruckQueueHeadParam truckQueueHeadParam)
        {
            return QueueList.UpdateTruckQueueHeader(truckQueueHeadParam);
        }
        //修改明细
        public string UpdateTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam)
        {
            return QueueList.UpdateTruckQueueDetail(truckQueueDetailParam);
        }
        //超时锁定车辆解锁
        public string unlockTruck(QueueParam queueParam)
        {
            return QueueList.unlockTruck(queueParam);
        }
        //车辆入场抬杆
        public string getInGate(ReceiptParam receiptParam)
        {
            return QueueList.getInGate(receiptParam);
        }
        //车辆离库抬杆
        public string leaveGate(ReceiptParam receiptParam)
        {
            return QueueList.leaveGate(receiptParam);
        }
        //小库区下拉列表
        public IEnumerable<WhZoneResult> GetSmallLoadAreaList(string whCode, string UnloadingArea)
        {
            return QueueList.GetSmallLoadAreaList(whCode,UnloadingArea);
        }
    }
}
