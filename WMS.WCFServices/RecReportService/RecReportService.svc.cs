using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.RecReportService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“RecReportService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 RecReportService.svc 或 RecReportService.svc.cs，然后开始调试。
    public class RecReportService : IRecReportService
    {
        IBLL.IRecReportManager recReport = new BLL.RecReportManager();
        //客户下拉菜单列表
        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {
            return recReport.WhClientListSelect(whCode);
        }

        //收货查询
        public List<ReceiptReportResult> C_ReceiveList(ReceiptReportSearch searchEntity, out int total)
        {
            return recReport.C_ReceiveList(searchEntity, out total);
        }
    }
}
