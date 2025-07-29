using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IBsInBoundManager
    {


        #region 1.保税区EIP订单导入
        string InBoundOrderListAddBs(InBoundOrderInsert entity);
        #endregion

        #region 2.保税区EIP收货登记
        string AddReceiptBs(string[] SO, string ClientCode, string LocationId, string WhCode, string User);

        #endregion

    }
}
