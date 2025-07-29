using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ISortTaskWinceManager
    {

        List<SortTask> GetSortTaskList(string LoadId, string WhCode);
        List<SortTaskDetailResult> GetSortTaskDetailList(string LoadId, string WhCode);
        string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty);

        string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName);

        //  bool ChecPackNumber(string WhCode, string LoadId, string SortGroupNumber, string PackNumber);
        //   string PackTaskInsert(PackTaskInsert packTaskInsert);
        //   string UpdatePackHead(int packHeadId, decimal? weight, string PackCartonNo, decimal? longth, decimal? width, decimal? height, string UpdateUser);

    }
}
