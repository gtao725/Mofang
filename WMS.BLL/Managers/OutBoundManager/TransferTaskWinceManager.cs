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
    public class TransferTaskWinceManager : ITransferTaskWinceManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();


        //获取交接单号明细
        public List<TransferDetailCe> GetTransferDetailCe(string transferNumber, string whCode, int pageSize, int pageIndex, out int total)
        {
            var list = (from a in idal.ITransferTaskDAL.SelectAll()
                        join b in idal.ITransferHeadDAL.SelectAll()
                              on new { a.Id, a.WhCode }
                          equals new { Id = (Int32)b.TransferTaskId, b.WhCode }
                        where a.TransferNumber == transferNumber && a.WhCode == whCode && a.Status != 30
                        select new TransferDetailCeModel
                        {
                            TransferTaskId = a.Id,
                            TransferNumber = a.TransferNumber,
                            LoadId = b.LoadId,
                            CustomerOutPoNumber = b.CustomerOutPoNumber,
                            OutPoNumber = b.OutPoNumber,
                            PackNumber = b.PackNumber,
                            express_code = a.express_code,
                            express_type_zh = a.express_type_zh,
                            ExpressNumber = b.ExpressNumber,
                            StatusDes = b.Status == -10 ? "被拦截" : b.Status == 10 ? "正常" : null,
                            TransferHeadId = b.Id
                        }).ToList();
            total = list.Count();
            List<TransferDetailCeModel> transferDetailCeModelList = list.OrderByDescending(u => u.TransferHeadId).Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            var aa = from a in transferDetailCeModelList
                     select new TransferDetailCe
                     {
                         TransferTaskId = a.TransferTaskId,
                         TransferNumber = a.TransferNumber,
                         LoadId = a.LoadId,
                         CustomerOutPoNumber = a.CustomerOutPoNumber,
                         OutPoNumber = a.OutPoNumber,
                         PackNumber = a.PackNumber,
                         express_code = a.express_code,
                         express_type_zh = a.express_type_zh,
                         ExpressNumber = a.ExpressNumber,
                         StatusDes = a.StatusDes,
                         TransferHeadId = a.TransferHeadId

                     };
            return aa.ToList();
        }



        public string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode)
        {
            TransferTaskManager transferTaskManager = new TransferTaskManager();
            return transferTaskManager.TransferTaskInsert(transferNumber, userName, expressNumber, whCode);
        }
        public string TransferTaskLoadInsert(string transferNumber, string userName, string loadId, string whCode)
        {
            TransferTaskManager transferTaskManager = new TransferTaskManager();
            return transferTaskManager.TransferTaskOutBoundOrderInsert(transferNumber, userName, loadId, whCode, "");
        }

        public string BeginTransferTask(int transferId, string userName, int fayunQty)
        {
            TransferTaskManager transferTaskManager = new TransferTaskManager();
            return transferTaskManager.BeginTransferTask(transferId, userName, fayunQty);
        }
        public string TransferTaskDelete(int transferId)
        {
            TransferTaskManager transferTaskManager = new TransferTaskManager();
            return transferTaskManager.TransferTaskDelete(transferId);
        }


    }
}
