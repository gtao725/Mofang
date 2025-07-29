using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ISortTaskManager
    {


        //分拣工作台--------------------------------------------------
        //拉取 分拣头信息
        List<SortTask> GetSortTaskList(string loadId, string whCode);

        //拉取 分拣明细信息
        List<SortTaskDetail> GetSortTaskDetailList(string loadId, string whCode);

        //拉取 分拣明细信息
        List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode);


        //实时拉取分拣冻结数量
        List<SortTaskDetailResult> GetHoldQtySortTaskDetailList(string loadId, string whCode);

        //分拣工作台
        string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty);

        //更新分拣订单号状态 及 框号信息
        string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName);

        //分拣工作台 查询分拣明细
        List<SortTaskDetailSelectResult> SelectSortTaskDetailList(SortTaskDetailSearch searchEntity, out int total);

        //更新框号信息
        string UpdateSortTaskDetail(string loadId, string whCode, int groupId, string groupNumber, string userName);
    }
}
