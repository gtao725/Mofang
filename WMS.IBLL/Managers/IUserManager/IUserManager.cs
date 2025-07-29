using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;

namespace WMS.IBLL
{
    public interface IUserManager
    {
        //复合的一次性添加货代客户及关系
        string WhAgentWhClientAddComplex(WhAgent whAgent, WhClient whClient);


    }
}
