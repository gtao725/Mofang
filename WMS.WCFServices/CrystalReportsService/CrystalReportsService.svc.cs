using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;
using WMS.BLLClass;
using System.Data;

namespace WMS.WCFServices.CrystalReportsService
{
      public class CrystalReportsService : ICrystalReportsService
    {
        public DataTable WhClientListSelect(string whCode)
        {
            IBLL.IInBoundOrderManager inBoundOrder = new BLL.InBoundOrderManager();
            return null;
        }

    }
}
