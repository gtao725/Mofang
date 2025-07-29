using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.Express;
using WMS.IBLL;

namespace WMS.BLL
{
    public class PackWinceManager : IPackWinceManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();

        //验证 分拣框号是否正确
        public List<SortTaskDetailResult> CheckPackSortGroupNumber(string sortGroupNumber, string whCode)
        {

            List<PackTask> sql = idal.IPackTaskDAL.SelectBy(u => u.SortGroupNumber == sortGroupNumber && u.WhCode == whCode && (u.Status == 10 || u.Status == 20)).ToList();
            if (sql.Count > 0)
            {
                PackTask packTask = sql.First();
                string loadId = packTask.LoadId;
                int? groupId = packTask.SortGroupId;
                string groupNumber = packTask.SortGroupNumber;
                return (from a in idal.ISortTaskDetailDAL.SelectAll()
                        where a.WhCode == whCode && a.LoadId == loadId && a.GroupId == groupId && a.GroupNumber == groupNumber
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
                        }).ToList(); ;
            }
            else
                return null;

        }

        public string PackTaskInsert(PackTaskInsert packTaskInsert)
        {
            PackTaskManager PT = new PackTaskManager();
            return PT.PackTaskInsert(packTaskInsert);
        }

        public string UpdatePackHead(int packHeadId, decimal? weight, string PackCartonNo, decimal? longth, decimal? width, decimal? height, string UpdateUser)
        {

            PackHead entity = new PackHead();
            entity.Id = packHeadId;
            entity.Weight = weight;
            entity.Length = longth;
            entity.Width = width;
            entity.Height = height;
            entity.PackCarton = PackCartonNo;
            entity.UpdateUser = UpdateUser;

            PackTaskManager PT = new PackTaskManager();
            string res = PT.UpdatePackHead(entity);
            if (res == "Y")
                return "Y";
            else
                return "N";


        }

        public bool ChecPackNumber(string WhCode, string LoadId, string SortGroupNumber, string PackNumber)
        {
            return (from a in idal.IPackTaskDAL.SelectAll()
                    join b in idal.IPackHeadDAL.SelectAll() on a.Id equals b.PackTaskId
                    where a.LoadId == a.LoadId && a.WhCode == WhCode && a.SortGroupNumber == SortGroupNumber && b.PackNumber == PackNumber
                    select a.Id).Count() == 0;
        }



    }
}
