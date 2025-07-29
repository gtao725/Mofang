using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.IBLL
{
    public interface IReleaseLoadManager
    {

        //释放Load
        string CheckReleaseLoad(string loadId, string whCode, string userName);

        //撤销释放
        string RollbackLoad(string loadId, string whCode, string userName);

        //释放Load
        string CheckReleaseLoad(string loadId, string whCode, string userName, string getType);

    }
}
