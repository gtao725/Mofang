using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IRecReportManager
    {
        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        //收货查询
        //对应 C_ReceiveController 中的 List 方法
        List<ReceiptReportResult> C_ReceiveList(ReceiptReportSearch searchEntity, out int total);
    }
}
