using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IEclOutBoundManager
    {

        string OMSOutBoundOrderDel(string whCode, string customerOutPoNumber, string clientCode, string userName);
        string OutBoundLoadAddEcl(EclLoadModel entity);

        string OutBoundOrderAddOMS(EclOutOrderModel entity);

        string BoschOutBoundOrderAddOMS(EclOutOrderModel entity);
        void UrlEdiTaskInsertOut(string TransferCode, string WhCode, string CreateUser);

        //批量导入OMS订单
        List<EclOutOrderModelResult> OutBoundOrderAddOMSBatch(List<EclOutOrderModel> entityList);

        //手动获取快递单
        string GetExpressNumber(string express_code, SFExpressModel sfModel, YTExpressModel ytModel, string whCode, ZTOExpressModel ztModel);

    }
}
