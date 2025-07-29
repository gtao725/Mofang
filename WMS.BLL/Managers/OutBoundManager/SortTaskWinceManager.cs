using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class SortTaskWinceManager : ISortTaskWinceManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
 

        //获取分拣表头
        public List<SortTask> GetSortTaskList(string LoadId, string WhCode)
        {
            SortTaskManager sortTaskManager = new SortTaskManager();
            return sortTaskManager.GetSortTaskList(LoadId, WhCode);
        }
        //获取分拣明细
        public List<SortTaskDetailResult> GetSortTaskDetailList(string LoadId, string WhCode)
        {
            return (from a in idal.ISortTaskDetailDAL.SelectAll()
                   where a.WhCode == WhCode && a.LoadId == LoadId
                   select new SortTaskDetailResult
                   {
                       GroupId = a.GroupId,
                       WhCode = a.WhCode,
                       LoadId = a.LoadId,
                       GroupNumber = a.GroupNumber,
                       OutPoNumber = a.OutPoNumber,
                       AltItemNumber = a.AltItemNumber,
                       ItemId = a.ItemId,
                       EAN = a.EAN,
                       Qty = a.Qty,
                       PackQty = a.PackQty,
                       PlanQty = a.PlanQty,
                       ScanFlag = a.ScanFlag,
                       HandFlag = a.HandFlag,
                       ScanRule = a.ScanRule
                   }).ToList();
        }
        //分拣SKU
        public string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty) {

            SortTaskManager sortTaskManager = new SortTaskManager();
            return sortTaskManager.SortTaskScanning( loadId,  whCode,  userName,  itemString,  Qty);
        }

        //完成分拣后  更新分拣订单号状态 及 框号信息
        //增加包装数据
        public string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName)
        {

            SortTaskManager sortTaskManager = new SortTaskManager();
            return sortTaskManager.UpdateSortTaskGroupNumber(loadId, whCode, groupId, groupNumber, userName);
        }
 

    }
}
