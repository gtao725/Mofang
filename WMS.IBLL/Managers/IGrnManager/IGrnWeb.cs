using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.IBLL
{
    public interface IGrnWeb
    {
        List<GrnHeadResult> GrnHeadList(GrnHeadSearch search, out int total);

        List<DamcoGRNDetail> GrnSOList(GrnHeadSearch search,out int total);
    }
}
