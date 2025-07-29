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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IRecReportService”。
    [ServiceContract]
    public interface IRecReportService
    {
        [OperationContract]
        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        [OperationContract]
        //收货查询
        List<ReceiptReportResult> C_ReceiveList(ReceiptReportSearch searchEntity, out int total);
    }
}
