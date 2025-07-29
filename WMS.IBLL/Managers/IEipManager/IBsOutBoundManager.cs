using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;


namespace WMS.IBLL
{
    public interface IBsOutBoundManager
    {
        string OutBoundLoadAddBs(BsLoadModel entity);

        #region 检查出库订单是否存在
        string OutBoundOrderCheck(string CustomerOutPoNumber);

        string EditLoadShipMode(string LoadId, string WhCode, string ShipMode);
        #endregion
    }
}
