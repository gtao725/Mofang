using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IShipLoadManager
    {


        //检查Load及托盘 是否拆托
        string CheckPickingLoad(string LoadId, string whCode, string HuId);

        //备货 
        string PickingLoad(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location);

        //BTB 装箱
        string PackingLoad(string LoadId, string whCode, string huId, string userName);

        //装箱插入装卸工 工作量
        string PackingLoad(string LoadId, string whCode, string huId, string Position, string userName, List<WorkloadAccountModel> WorkloadAccountModel, List<HuDetailRemained> HuDetailRemained);

        //集装箱 封箱
        string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber, int baQty);

        //集装箱 封箱
        string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber);

        //DC备货后自动装箱
        string PickingLoadDC(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location);

        //备货时 自动分拣-备货-包装
        string PickingSortingPackingByOrerBegin(List<PickTaskDetailResult> entityList, string whCode, string packGroupNumber, string Location, string userName);

        //备货时 自动分拣
        string PickingSortingByOrerBegin(List<PickTaskDetailResult> entityList, string whCode, string packGroupNumber, string Location, string userName);


    }
}
