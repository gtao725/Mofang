using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WMS.WCFServices.CommonService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ICommonService”。
    [ServiceContract]
    public interface ICommonService
    {
        [OperationContract]
        string PageFunc();

        [OperationContract]
        string ServiceFunc();
        [OperationContract]
        string ServiceAutoFunc();
    }
}
