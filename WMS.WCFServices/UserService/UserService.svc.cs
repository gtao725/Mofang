using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;

namespace WMS.WCFServices.UserService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“UserService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 UserService.svc 或 UserService.svc.cs，然后开始调试。
    public class UserService : IUserService
    {
        IBLL.IUserManager iUserManager = new BLL.UserManager();

        public string WhAgentWhClientAddComplex(WhAgent whAgent, WhClient whClient)
        {
            return iUserManager.WhAgentWhClientAddComplex(whAgent, whClient);
        }
    }
}
