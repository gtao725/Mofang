using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
   public interface IRecOuterManager
    {
        bool CheckRecLocation(string WhCode, string Location);
        ReceiptData GetReceiptData(ReceiptParam receiptParam);
    }
}
