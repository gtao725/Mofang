using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class BusinessObjectsModel
    {

        public int Id;
        public string ObjectName;
        public string ObjectValue;
        public string ObjectDes;
        public string ObjectType;
        public List<BusinessObjectItemsModel> ListBusinessObjectItems;
    }

    public class BusinessObjectItemsModel
    {

        public int Id;
        public int ObjectId;
        public string MustAttributeName;
        public string MustAttributeNameCN;
        public int? ParaObjectId;
        public BusinessObjectsModel ParaObject;
    }

    //public class BusinessObjectsEModel
    //{
    //    public int Id;
    //    public string ObjectName;
    //    public string ObjectDes;
    //    public string ObjectType;
    //    public IEnumerable<BusinessObjectItemsEModel> ListBusinessObjectItems;
    //   // public object ListBusinessObjectItems;
    //}

    //public class BusinessObjectItemsEModel
    //{

    //    public int Id;
    //    public int ObjectId;
    //    public string MustAttributeName;
    //    public string MustAttributeNameCN;
    //    public int? ParaObjectId;
    //   // public string ProcDBName;
    //}
}
 