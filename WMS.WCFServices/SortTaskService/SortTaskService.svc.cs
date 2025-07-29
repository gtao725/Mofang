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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“SortTaskService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 SortTaskService.svc 或 SortTaskService.svc.cs，然后开始调试。
    public class SortTaskService : ISortTaskService
    {

        IBLL.ISortTaskManager sortTask = new BLL.SortTaskManager();

        //分拣工作台--------------------------------------------------
        //拉取 分拣头信息
        public List<SortTask> GetSortTaskList(string loadId, string whCode)
        {
            return sortTask.GetSortTaskList(loadId, whCode);
        }

        //拉取 分拣明细信息
        public List<SortTaskDetail> GetSortTaskDetailList(string loadId, string whCode)
        {
            return sortTask.GetSortTaskDetailList(loadId, whCode);
        }

        //拉取 分拣明细信息
        public List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode)
        {
            return sortTask.GetSumSortTaskDetailList(loadId, whCode);
        }

        //实时拉取分拣冻结数量
        public List<SortTaskDetailResult> GetHoldQtySortTaskDetailList(string loadId, string whCode)
        {
            return sortTask.GetHoldQtySortTaskDetailList(loadId, whCode);
        }

        //分拣工作台
        public string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty)
        {
            return sortTask.SortTaskScanning(loadId, whCode, userName, itemString, Qty);
        }


        //更新分拣订单号状态 及 框号信息
        public string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName)
        {
            return sortTask.UpdateSortTaskGroupNumber(loadId, whCode, groupId, groupNumber, userName);
        }


        //分拣工作台 查询分拣明细
        public List<SortTaskDetailSelectResult> SelectSortTaskDetailList(SortTaskDetailSearch searchEntity, out int total)
        {
            return sortTask.SelectSortTaskDetailList(searchEntity, out total);
        }


        //更新框号信息
        public string UpdateSortTaskDetail(string loadId, string whCode, int groupId, string groupNumber, string userName)
        {
            return sortTask.UpdateSortTaskDetail(loadId, whCode, groupId, groupNumber, userName);
        }

    }
}
