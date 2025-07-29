using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IPackWinceManager
    {
        //验证 分拣框号是否正确
        List<SortTaskDetailResult> CheckPackSortGroupNumber(string sortGroupNumber, string whCode);
        bool ChecPackNumber(string WhCode, string LoadId, string SortGroupNumber, string PackNumber);
        string PackTaskInsert(PackTaskInsert packTaskInsert);
        string UpdatePackHead(int packHeadId, decimal? weight, string PackCartonNo, decimal? longth, decimal? width, decimal? height, string UpdateUser);
 
    }
}
