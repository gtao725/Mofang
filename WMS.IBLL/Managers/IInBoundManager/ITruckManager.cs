using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface ITruckManager
    {
        #region 车辆排队管理

        //货代下拉菜单列表
        IEnumerable<WhAgent> WhAgentListSelect(string whCode);

        //车辆排队列表
        List<ReceiptRegisterResult> TruckQueryList(ReceiptRegisterSearch searchEntity, out int total);

        //车辆放车
        string EditTruckStatus(string whCode, string receiptId, string userName);
        //车辆插队
        string QueueUpTruck(string whCode, string receiptId, string userName, string remark);

        //删除收货登记 排队系统使用
        string DelReceiptRegisterByTruck(ReceiptRegister entity);


        #endregion

    }
}
