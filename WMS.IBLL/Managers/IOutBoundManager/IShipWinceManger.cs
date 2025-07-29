using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IShipWinceManger
    {
        bool CheckLoadStatus(string WhCode, string LoadId);
        int GetLoadProcessId(string WhCode, string LoadId);
        ShipPickDesModel GetPickingRes(string WhCode, string LoadId);
        ShipPickDesModel GetPickingRes(string WhCode, string LoadId,int index);
        List<ShipPickDesModel> GetPickingResList(string WhCode, string LoadId, int count);
        ShipPickDesModel GetPickingPltRes(string WhCode,string HuId);
        ShipLoadDesModel GetShipLoadDesHead(string WhCode, string ContainerNumber);
        string CheckPickingLoad(string WhCode, string LoadId, string HuId);

        string  CheckPickSplitPlt(string WhCode, string LoadId, string HuId,string PutHuId);

        string SerialNumberChange(string WhCode, string LoadId, string HuId, string PutHuId, string UserName);
        List<string> GetSerialNumberOut(string WhCode, string LoadId, string HuId);
        bool CheckPickingComplete(string WhCode, string LoadId);
        List<ShipPickSplitDesModel> GetPickingSplitDes(string WhCode, string LoadId, string HuId);

        bool CheckDeliveryLoadStatus(string WhCode, string LoadId);
        //string GetContainerNumberLoadId(string WhCode, string ContainerNumber);
        //bool CheckDeliverySealNumber(string WhCode, string LoadId, string SealNumber);
        string ShippingLoad(string loadId, string whCode, string userName);
        string ShippingLoadCustomer(string loadId,string DeliveryOrderNumber, string whCode, string userName);
        
        string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber);
        string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber,int baQty);
        string GetSysPlt(string WhCode);

        bool DeliveryQtyCheck(string loadId, string whCode);
    }
}
