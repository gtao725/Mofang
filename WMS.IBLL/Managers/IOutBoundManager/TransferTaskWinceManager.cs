using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ITransferTaskWinceManager
    {

        //获取交接单号明细
        List<TransferDetailCe> GetTransferDetailCe(string transferNumber, string whCode, int pageSize, int pageIndex, out int total);

        string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode);

        string TransferTaskLoadInsert(string transferNumber, string userName, string loadId, string whCode);

        string BeginTransferTask(int transferId, string userName, int fayunQty);
        string TransferTaskDelete(int transferId);
    }
}
