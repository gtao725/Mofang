using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WMS.EIP
{
    public class ReportRequestDataModel
    {
 
        //执行数据库DBNAME
        public string ExecDbName { get; set; }      
        //执行的SQL的名称
        public string SqlName { get; set; }
        public string Sql { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public Hashtable SqlParameter { get; set; }
        public string SumPara { get; set; }
        public Dictionary<string, string> OrderByPara { get; set; }

        
        public bool IfReturnSql { get; set; }
        public bool ExecType { get; set; }

    }

}