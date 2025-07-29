using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.IBLL
{
    public interface IBusiness
    {

        // IEnumerable<BusinessObjectHeadModel> BusinessDetailGet(int objectHeadId);
        BusinessFlowDetailList GetFlowDetail(int FlowId,string FlowDetailType);
        BusinessFlowDetailList GetFlowDetailAPP(int FlowId, string FlowDetailType);
    }
}
