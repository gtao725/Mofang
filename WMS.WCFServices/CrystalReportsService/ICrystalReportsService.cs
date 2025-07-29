using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WMS.WCFServices.CrystalReportsService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“CrystalReportsService”。
    [ServiceContract]
    public interface ICrystalReportsService
    {
        [OperationContract]
        //客户下拉菜单列表
        System.Data.DataTable WhClientListSelect(string whCode);
    }
}
