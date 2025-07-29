using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IGMSManager
    {

        //WMS登记后添加车辆
        string WmsCreateGms(string WhCode, string ReceiptId);
        //放车
        string ReleaseTruck(QueueParam queueParam);


        //收货登记修改表头后更改对应GMS信息
        string UpdateTruckInfoByReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames);
        //收货登记删除后调用删除GMS对应信息
        string DeleteTruckInfoByReceiptRegister(string ReceiptId, string WhCode);

        //删除车辆排队信息
        string DeleteTruckInfo(QueueParam queueParam);
        //删除车牌排队明细信息
        string DeleteTruckDetail(QueueParam queueParam);
        //新增车辆表头信息
        string AddTruckQueueHead(TruckQueueHeadParam truckQueueHeadParam);
        //预约通道获取
        IEnumerable<LookUp> BookChannelSelect(string whCode);
        //新增车辆明细信息
        string AddTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam);
        //修改车辆表头信息
        string UpdateTruckQueueHeader(TruckQueueHeadParam truckQueueHeadParam);
        //修改明细
        string UpdateTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam);
        //车辆排队信息接收接口
        string GetTruckQueueInfo(List<TruckQueueInfo> truckQueueInfoList);
        ////车辆排队查询列表
        List<TruckQueueListDetail> getTruckQueueList(QueueParam queueParam);


        ////车辆出入库查询列表
        List<TruckQueueListDetail> getTruckQueueListTruck(QueueParam queueParam);

        //超时锁定车辆解锁
        string unlockTruck(QueueParam queueParam);
        //车辆入场抬杆
        string getInGate(ReceiptParam receiptParam);
        //车辆离库抬杆
        string leaveGate(ReceiptParam receiptParam);
        //小库区下拉列表
        IEnumerable<WhZoneResult> GetSmallLoadAreaList(string whCode, string UnloadingArea);
    }
}
