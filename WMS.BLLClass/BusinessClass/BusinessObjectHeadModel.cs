using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass {
    public class BusinessObjectHeadModel
    {
        public int Id;
        public string BusinessName;
        public List<BusinessObjectDetailModel> ListBusinessObjectDetail;
    }

    public class BusinessObjectDetailModel
    {
        public int Id;
        public int HeadId;
        public int DetailSeq;
        public int? InParaObjectId;
        public BusinessObjectsModel InParaObject;
        public int? InExecObjectId;
        public BusinessObjectsModel InExecObject;
        public string FormName;
        public int? BeforeOpenExecObjectId;
        public BusinessObjectsModel BeforeOpenExecObject;
        public string NextOpenFormName;
        public string ExecFlag;
    }
}
