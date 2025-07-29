using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.IDAL
{
    public partial interface IDALSession
    {
        int SaveChanges();
        List<T> ExecSqlToList<T>(string sql, SqlParameter[] sqlParameter);
        string ExecSqlToString(string sql, SqlParameter[] sqlParameter);
        void ExecSql(string sql, SqlParameter[] sqlParameter);
        void ExecSqlAsync(string sql, SqlParameter[] sqlParameter);
    }
}
