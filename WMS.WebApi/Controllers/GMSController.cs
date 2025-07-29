
using MODEL_MSSQL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class GMSController : ApiController
    {
        Helper Helper = new Helper();
        //车辆排队信息接收接口
        [HttpGet, HttpPost]
        public object GetTruckQueueInfo(List<TruckQueueInfo> truckQueueInfoList)
        {
            string res = "";
            IGMSManager iGMSManager = new GMSManager();
            res = iGMSManager.GetTruckQueueInfo(truckQueueInfoList);
            if (res != "接收成功！")
            {
                return Helper.ResultData("N", res, new { });
            }
            return Helper.ResultData("Y", res, new { });
        }
        //放车接口
        [HttpGet, HttpPost]
        public object ReleaseTruck(QueueParam queueParam)
        {
            string res = "";
            IGMSManager iGMSManager = new GMSManager();
            res = iGMSManager.ReleaseTruck(queueParam);
            return Helper.ResultData("Y", res, new { });
        }
        //车辆排队查询列表
        [HttpGet, HttpPost]
        public object getTruckQueueList(QueueParam queueParam)
        {
            //QueueParam queueParam1 = new QueueParam();
            //queueParam1.WhCode = "10";
            IGMSManager iGMSManager = new GMSManager();
            var truckQueueListDetail = iGMSManager.getTruckQueueList(queueParam);
            return Helper.ResultData("Y", "", truckQueueListDetail);
        }
        //车辆出入库查询列表
        [HttpGet, HttpPost]
        public object getTruckQueueListTruck(QueueParam queueParam)
        {
            //QueueParam queueParam1 = new QueueParam();
            //queueParam1.WhCode = "10";
            IGMSManager iGMSManager = new GMSManager();
            var truckQueueListDetail = iGMSManager.getTruckQueueListTruck(queueParam);
            return Helper.ResultData("Y", "", truckQueueListDetail);
        }
        //获取库区
        [HttpGet,HttpPost]
        public object getLoadingAreaList(QueueParam queueParam)
        {
            RootManager rootManager = new RootManager();
            var LoadingAreaList = rootManager.WhZoneParentSelect(queueParam.WhCode).ToList().Where(u => u.RegFlag == 1).Select(u=>u.ZoneName).ToList();
            return Helper.ResultData("Y", "", LoadingAreaList);
        }

        //获取库区
        [HttpGet, HttpPost]
        public object getSmallLoadAreaList(QueueParam queueParam)
        {
            GMSManager gms = new GMSManager();
            var SmallLoadAreaList = gms.GetSmallLoadAreaList(queueParam.WhCode, queueParam.UnloadingArea).Select(u => u.ZoneName).ToList();
            return Helper.ResultData("Y", "", SmallLoadAreaList);
        }

        
        //车辆排队表头信息更新接口
        [HttpGet,HttpPost]
        public object UpdateTruckQueueHeader(TruckQueueHeadParam truckQueueHeadParam)
        {
            IGMSManager iGMSManager = new GMSManager();
            string result = iGMSManager.UpdateTruckQueueHeader(truckQueueHeadParam);
            if (result == "修改成功！")
            {
                return Helper.ResultData("Y", result, new { });
            }
            else
            {
                return Helper.ResultData("N", result, new { });
            }
        }
        //抬杆接口
        [HttpGet, HttpPost]
        public object getInGate(ReceiptParam receiptParam)
        {
            string res = "";
            IGMSManager iGMSManager = new GMSManager();
            res = iGMSManager.getInGate(receiptParam);
            return Helper.ResultData("Y", res, new { });
        }


        //抬杆离库接口
        [HttpGet, HttpPost]
        public object leaveGate(ReceiptParam receiptParam)
        {
            string res = "";
            IGMSManager iGMSManager = new GMSManager();
            res = iGMSManager.leaveGate(receiptParam);
            return Helper.ResultData("Y", res, new { });
        }
    }
}
