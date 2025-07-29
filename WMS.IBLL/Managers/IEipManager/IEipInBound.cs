using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IEipInBound
    {


        #region 1.CFS订单导入调用收货批次
        string EipInsertInBound(List<InBoundOrderInsert> entity);
        #endregion


        #region 2 删除LOAD
        string LoadMasterDel(string LoadId, string Whcode, string User);
        #endregion

        #region 3 转换导入对象
        List<InBoundOrderInsert> ImportsInBoundOrderTransformation(List<ImportsInBoundOrderInsert> entity);
        #endregion

        #region 4 验证SO是否已经登记
        string CheckRegInBoundSo(string SoNumber, string Whcode,string ClientCode);
        #endregion

        #region 5 GWI导入WMS DAMCOGRE表
        string ImportsGWI(List<GwiDetailInsert> entity);
        #endregion

    }
}
