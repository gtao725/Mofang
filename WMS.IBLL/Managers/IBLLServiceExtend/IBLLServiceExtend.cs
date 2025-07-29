using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;


namespace WMS.IBLL
{
    public partial interface IWhUserPositionService : IBaseBLL<WhUserPosition>
    {
        WhUserPosition WhUserPositionAdd(WhUserPosition entity);

    }

}
