using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.SortTaskService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ISortTaskService”。
    [ServiceContract]
    public interface ISortTaskService
    {

        //分拣工作台--------------------------------------------------
        //拉取 分拣头信息

        [OperationContract]
        List<SortTask> GetSortTaskList(string loadId, string whCode);

        //拉取 分拣明细信息

        [OperationContract]
        List<SortTaskDetail> GetSortTaskDetailList(string loadId, string whCode);

        [OperationContract]
        //拉取 分拣明细信息
        List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode);

        [OperationContract]
        //实时拉取分拣冻结数量
        List<SortTaskDetailResult> GetHoldQtySortTaskDetailList(string loadId, string whCode);


        [OperationContract]
        //分拣工作台
        string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty);


        [OperationContract]
        //更新分拣订单号状态 及 框号信息
        string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName);

        [OperationContract]
        //分拣工作台 查询分拣明细
        List<SortTaskDetailSelectResult> SelectSortTaskDetailList(SortTaskDetailSearch searchEntity, out int total);

        [OperationContract]
        //更新框号信息
        string UpdateSortTaskDetail(string loadId, string whCode, int groupId, string groupNumber, string userName);
    }
}
