using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.EIP
{
    public class EipDataGridModel
    {



        //    string aa = "{\"excelhidden\":[],\"rows\":[{\"aa\":\"-252\",\"bb\":\"2013-08-01\"}],\"mid\":\"3a462c08-e1c8-11e5-b3ac-0050569e52bf\",\"eip_sorts\":\"[]\",\"total\":1,\"page\":1,\"columns\":[{{\"column\":\"bb\",\"width\":70,\"type\":\"datetime\",\"osort\":1,\"sort\":1},{\"column\":\"aa\",\"width\":80,\"type\":\"string\",\"osort\":2,\"sort\":2}]}";

    
        public object excelhidden { get; set; }

        //string detail;
        public object rows { get; set; }

        //"total":"290"
        public int total { get; set; }
        //当前页
        public int page { get; set; }

        //  "mid": "3a462c08-e1c8-11e5-b3ac-0050569e52bf", 
        public object sum { get; set; }
        public string mid { get; set; }

        //   "eip_sorts": "[{\"name\":\"来源\",\"value\":\"DESC\"},{\"name\":\"A/R\",\"value\":\"ASC\"}]", 
        public object eip_sorts { get; set; }

        //"columns": [{"column":"批号",  "width": 50,"type": "string","osort": 0, "sort": 0},{"column":"批号",  "width": 50,"type": "string","osort": 0, "sort": 0}"

        public object columns { get; set; }

    }

    public class EipDataGridModelColumns
    {
        public string column { get; set; }
        public string width { get; set; }
        public string type { get; set; }
        public int osort { get; set; }
        public int sort { get; set; }
    }

    public class eip_sorts {
        public  string name { get; set; }
        public string value { get; set; }
    }
}
