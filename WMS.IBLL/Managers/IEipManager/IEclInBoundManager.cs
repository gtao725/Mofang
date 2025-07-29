using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IEclInBoundManager
    {

         
        string InBoundOrderListAddEcl(InBoundOrderInsert entity);

        string InBoundOrderListAddOms(InBoundOrderInsert entity);

        string InBoundOrderListAddOmsSSID(InBoundOrderInsert entity);


        string ItemMasterAddOms(List<ItemMaster> iml);
        string ItemMasterUpdateOms(ItemMaster im);
        void UrlEdiTaskInsertRec(string ReceiptId, string WhCode, string CreateUser);
 
    }
}
