using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace WMS.Reports.Services.Helper
{
    public class OpenDataHelper
    {
        private  string db_name;
        private  SqlConnection connS;
        public  void OpenData(string dbName)
        {
 
            if (!string.IsNullOrEmpty(db_name))
            {
                CloseData();
            }
            connS = new SqlConnection();
            connS.ConnectionString = connStr(dbName).ToString();
            connS.Open();
            db_name = dbName.ToUpper();
      
        }

        private  SqlConnectionStringBuilder connStr(string dbName)
        {
            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder();
            if (!string.IsNullOrEmpty(db_name))
            {
                CloseData();
            }
            string Ip = dbName.Replace("#", ".").Substring(Convert.ToInt32("0"), dbName.LastIndexOf(".")), ud = "", pw = "", db = "";


            ud = ConfigurationManager.AppSettings["WMSDBUser"].ToString(); ;
            pw = ConfigurationManager.AppSettings["WMSDBPW"].ToString(); ;
            db = dbName.Substring(dbName.LastIndexOf(".") + 1, dbName.Length - dbName.LastIndexOf(".") - 1);

            //switch (Ip)
            //{
            //    case "10.88.88.97":
            //        ud = "sa";
            //        pw = "1qaz2wsx,";
            //        db = dbName.Substring(dbName.LastIndexOf(".") + 1, dbName.Length - dbName.LastIndexOf(".") - 1);
            //        break;
            //    case "10.88.88.103":
            //        ud = "sa";
            //        pw = "1qaz2wsx,";
            //        db = dbName.Substring(dbName.LastIndexOf(".") + 1, dbName.Length - dbName.LastIndexOf(".") - 1);
            //        break;
            //}

            connStr.DataSource = Ip;
            connStr.UserID = ud;
            connStr.Password = pw;
            connStr.InitialCatalog = db;
            connStr.MaxPoolSize = 1000;

            return connStr;// "Max Pool Size=1000 ;data source=" + Ip + ";user id=" + ud + ";password=" + pw + ";initial catalog=" + db + ";";
        }
        public  void CloseData()
        {
 
            if (!string.IsNullOrEmpty(db_name))
            {
                connS.Close();
                connS.Dispose();
                connS = null;
            }
            db_name =null;
        }

        public  DataSet ExecSQL(string sql, Hashtable sqlParameters)
        {
            DataSet dataSet = new DataSet();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sql, connS);
            if (sqlParameters != null)
            {
                foreach (DictionaryEntry item in sqlParameters)
                {
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue(item.Key.ToString(), item.Value);
                }
            }
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        /// <summary>
        /// 不带返回行数的DateTable的Json
        /// </summary>
        /// <param name="SqlName">sql名称</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="ExecDbName">执行DB 例如 10#88#88#97.WMS</param>
        /// <returns>返回DataTable类型的Json</returns>
        public string SqlNameToJson(string SqlName, Hashtable sqlParameters, string ExecDbName) {

            return DataTableToJson(SqlToDataTable(GetSql(SqlName, ExecDbName), sqlParameters, ExecDbName));
        }

        #region 带总行数的DataSet的Json
        /// <summary>
        /// 带总行数的DataSet的Json,datable名称分别为Result,Count
        /// </summary>
        /// <param name="SqlName">sql名称</param>
        /// <param name="Sql">Sql内容</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="ExecDbName">执行DB 例如 10#88#88#97.WMS</param>
        /// <param name="Page">返回页数</param>
        /// <param name="Size">返回行数 如果行是0 则返回全部数据</param>
        /// <param name="OrderByPara">排序的参数</param>
        /// <returns>返回DataSet类型的Json,datable名称分别为Result,Count</returns>
        public  string SqlToJson(string SqlName,string Sql, Hashtable sqlParameters, string ExecDbName,int Page, int Size, Dictionary<string, string> OrderByPara, string SumPara,bool ExecType=false)
        {
            string sql = string.Empty;
            //返回行数结束 如果结束行是0 则返回全部数据,走datable
            if (Size != 0)
            { 
 
                sql = SqlReturn(SqlName, Sql, sqlParameters, ExecDbName, Page, Size, OrderByPara,false, SumPara, ExecType);
                return DataSetToJson(SqlToDataSet(sql, sqlParameters, ExecDbName));
            }
            else {
                //说如果SQL名称不为空,并且Sql为空,读取SqlName
                if (!string.IsNullOrEmpty(SqlName) && string.IsNullOrEmpty(Sql))
                    sql = GetSql(SqlName, ExecDbName);
                else
                    sql = Sql;
                return DataTableToJson(SqlToDataTable(sql, sqlParameters, ExecDbName));

            }



            ////返回行数结束 如果结束行是0 则返回全部数据,走datable
            //if (Size != 0)
            //{

            //    sql = SqlReturn(SqlName, Sql, sqlParameters, ExecDbName, Page, Size, OrderByPara, false, SumPara);
            //    //返回JSON
            //    return DataSetToJson(SqlToDataSet(sql, sqlParameters, ExecDbName));
            //}
            //else
            //{
            //    //说如果SQL名称不为空,并且Sql为空,读取SqlName
            //    if (!string.IsNullOrEmpty(SqlName) && string.IsNullOrEmpty(Sql))
            //        sql = GetSql(SqlName, ExecDbName);
            //    else
            //        sql = Sql;
            //    return DataTableToJson(SqlToDataTable(sql, sqlParameters, ExecDbName));
            //}



        }
        #endregion

        public string SqlReturn(string SqlName, string Sql, Hashtable sqlParameters, string ExecDbName, int Page, int Size, Dictionary<string, string> OrderByPara,bool IfSqlPara=false, string SumPara=null,bool ExecType=false) {

            string sql = string.Empty;
            if (sqlParameters == null) sqlParameters = new Hashtable();
            //添加sql的中页数和行数的参数
            sqlParameters.Add("@Page", Page);
            sqlParameters.Add("@Size", Size);
            #region 获取变形后的带返回总数的sql
            //说如果SQL名称不为空,并且Sql为空,并且不需要复杂的执行SQL,读取SqlName
            if (!string.IsNullOrEmpty(SqlName) && string.IsNullOrEmpty(Sql)&& !ExecType) sql = GetReportSqls(GetSql(SqlName, ExecDbName), Page, Size, OrderByPara, SumPara);
            //说如果SQL内容不为空或者要立即执行SQL或者要返回SQL  
            if (!string.IsNullOrEmpty(Sql) || ExecType || IfSqlPara)
            {
                //复杂执行SQL或者直接返回SQL的时候没有Sql只有SqlName
                if (!string.IsNullOrEmpty(SqlName)&&string.IsNullOrEmpty(Sql)&& (ExecType|| IfSqlPara))
                    Sql = GetSql(SqlName, ExecDbName);
                //替换排序及求和
                sql = GetReportSqlQueryOnly(Sql, Page, Size, OrderByPara, SumPara);
                //替换参数
                sql = SqlParaReplace(sql, sqlParameters);
            }
            #endregion
            return sql;
        }

        public string SqlParaReplace(string sql, Hashtable sqlParameters)
        {
            if (sqlParameters != null)
            {
                foreach (DictionaryEntry item in sqlParameters)
                {
                    if(item.Value is int)
                        sql = sql.Replace(item.Key.ToString(),  item.Value.ToString());
                    else
                        sql = sql.Replace(item.Key.ToString(), "'"+item.Value.ToString()+"'");
                }
            }
            return sql;
        }


        #region sql返回DataTable
        /// <summary>
        /// sql返回DataTable
        /// </summary>
        /// <param name="SqlName">sql名称</param>
        /// <param name="Sql">Sql内容</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="ExecDbName">执行DB 例如 10#88#88#97.WMS</param>
        /// <param name="OrderByPara">排序的参数</param>
        /// <returns>sql返回DataTable</returns>
        public  DataTable SqlToDataTable(string SqlName, string Sql, Hashtable sqlParameters, string ExecDbName, Dictionary<string, string> OrderByPara,bool ExecType)
        {



            #region 获取变形后的sql
            //说如果SQL名称不为空 读取SqlName
            if (!string.IsNullOrEmpty(SqlName)) Sql = GetSql(SqlName, ExecDbName);
            StringBuilder returnSql = new StringBuilder();
            if (!ExecType)
            {
                //将原SQL包含在内
                returnSql.Append("select * from (");
                returnSql.Append(Sql);
                returnSql.Append(" ) Result ");
            }
            else
            {
                Sql = Sql.Replace("##SqlQueryBegin", "select * from (");
                Sql = Sql.Replace("##SqlQueryEnd", " ) Result ");
                returnSql.Append(Sql);
            }
            //添加排序字段
            returnSql.Append(OrderByParaToStr(OrderByPara));
            #endregion
            return SqlToDataTable(returnSql.ToString(), sqlParameters, ExecDbName);
           
        }
        #endregion

        public  string StringToJson(string str)
        {
            DataSet ds = new DataSet();

            //添加查询结果DataTable
            DataTable dt = new DataTable();
            dt.Columns.Add("查询结果");
            DataRow newRow= dt.NewRow();
            newRow[0] = str;
            dt.Rows.Add(newRow);
            dt.TableName = "Result";
            ds.Tables.Add(dt);

            //添加查询行数DataTable
            DataTable dtCount = new DataTable();
            dtCount.Columns.Add("Counts");
            DataRow newDtCountRow = dtCount.NewRow();
            newDtCountRow[0] = 0;
            dtCount.Rows.Add(newDtCountRow);
            dtCount.TableName = "Count";
            ds.Tables.Add(dtCount);

            return DataSetToJson(ds);
        }
        public  string GetSql(string SqlName, string ExecDbName)
        {
            Hashtable ht = new Hashtable();
            ht.Add("@SqlName", SqlName);
            return SqlToDataTable("select Sql from Sqls where SqlName=@SqlName and InactiveFlag='N' ", ht, ExecDbName).Rows[0][0].ToString();
        }


        #region 获取报表的sql,带RowsCount
        /// <summary>
        /// 获取报表的sql,带RowsCount
        /// </summary>
        /// <param name="oldSql">sql内容</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="Page">返回页数</param>
        /// <param name="Size">返回行数 如果行是0 则返回全部数据</param>
        /// <param name="OrderByPara">排序的参数</param>
        /// <param name="SumPara">求和参数</param>
        /// 
        /// <returns>获取报表的sql,带RowsCount</returns>
        public string GetReportSqls(string oldSql, int Page, int Size, Dictionary<string, string> OrderByPara, string SumPara)
        {
           // string oldSql = GetSql(SqlName, ExecDbName);
            StringBuilder sql = new StringBuilder();

            //只得到部分行数时,需要返回总数行
            if (Size != 0)
            {
                //将原SQL包含在内
                sql.Append("select * from (");
                sql.Append(oldSql);
                sql.Append(" ) Result ");
                //添加排序字段
                sql.Append(OrderByParaToStr(OrderByPara));
                //添加行数
                sql.Append(" OFFSET (@Page-1)*@Size ROWS FETCH NEXT @Size ROWS ONLY ; ");
                //添加总数及求和查询
                sql.Append("select count(*) Counts ");
                if (!string.IsNullOrEmpty(SumPara))
                {
                    string[] SumParaArry = SumPara.Split(',');
                    foreach (string item in SumParaArry)
                    {
                        sql.Append(",sum(");
                        sql.Append(item);
                        sql.Append(") ");
                        sql.Append(item);

                    }
                }

                sql.Append(" from ( ");
                sql.Append(oldSql);
                sql.Append(" ) EipCount ");
                return sql.ToString();
            }else
                 return oldSql;
        }
        #endregion

        #region 报表不使用参数,自己拼接的SQL,不添加 Result,与Counts
        /// <summary>
        /// 获取报表的sql,带RowsCount
        /// </summary>
        /// <param name="oldSql">sql内容</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="Page">返回页数</param>
        /// <param name="Size">返回行数 如果行是0 则返回全部数据</param>
        /// <param name="OrderByPara">排序的参数</param>
        /// <param name="SumPara">求和参数</param>
        /// 
        /// <returns>获取报表的sql,带RowsCount</returns>
        public string GetReportSqlQueryOnly(string oldSql, int Page, int Size, Dictionary<string, string> OrderByPara, string SumPara)
        {
             
 
             
            string SqlContet = oldSql,SqlCount= oldSql;
 
            //拼写查询翻页结果
            string SqlQueryEnd = "";
            SqlContet = SqlContet.Replace("##SqlQueryBegin", "select * from (");
            SqlQueryEnd = " ) Result";
            SqlQueryEnd += OrderByParaToStr(OrderByPara);
            SqlQueryEnd += " OFFSET (" + Page + "-1)*" + Size + " ROWS FETCH NEXT " + Size + " ROWS ONLY ;";
            SqlContet = SqlContet.Replace("##SqlQueryEnd", SqlQueryEnd);

            //添加总数及求和查询
            string SqlQueryBegin = "  select count(*) Counts ";
            if (!string.IsNullOrEmpty(SumPara))
            {
                string[] SumParaArry = SumPara.Split(',');
                foreach (string item in SumParaArry)
                {
                    SqlQueryBegin += ",sum("+ item+")"+ item;
                }
            }
            SqlQueryBegin += " from ( ";
            SqlCount = SqlCount.Replace("##SqlQueryBegin", SqlQueryBegin);
            SqlCount = SqlCount.Replace("##SqlQueryEnd", " ) EipCount ");
 
            return SqlContet + SqlCount;
 
        }
        #endregion

        /// <summary>
        /// 组织排序参数返回STR
        /// </summary>
        /// <param name="OrderByPara"></param>
        /// <returns></returns>
        public string OrderByParaToStr(Dictionary<string, string> OrderByPara) {
            StringBuilder str = new StringBuilder();
            if (OrderByPara != null)
            {
                int i = 0;
                foreach (KeyValuePair<string, string> item in OrderByPara)
                {
                    if (i == 0)
                        str.Append(" order by ");
                    else
                        str.Append(",");
                    str.Append(item.Key);
                    str.Append(" ");
                    str.Append(item.Value);
                    i++;
                }
            }else
                str.Append(" order by 1");
            return str.ToString();
        }


        public  DataTable SqlToDataTable(string sql, Hashtable sqlParameters, string ExecDbName)
        {
            DataSet ds = new DataSet();
            ds = SqlToDataSet(sql, sqlParameters, ExecDbName);
            DataTable dt = new DataTable() ;
            if (ds.Tables != null)
                dt = ds.Tables[0];
            return dt;
        }

        #region 带返回count数的DataSet,datable名称分别为Result,Count
        /// <summary>
        /// 带返回count数的DataSet,datable名称分别为Result,Count
        /// </summary>
        /// <param name="sql">sql名称 最少是2个datatable</param>
        /// <param name="sqlParameters">参数</param>
        /// <param name="ExecDbName">执行DB 例如 10#88#88#97.WMS</param>
        /// <returns>带返回count数的DataSet,datable名称分别为Result,Count</returns>
        private  DataSet SqlToDataSet(string sql, Hashtable sqlParameters, string ExecDbName)
        {
            //打开数据库
            OpenData(ExecDbName);
            DataSet ds = new DataSet();
            //填充DT
            ds = ExecSQL(sql, sqlParameters);
            //关闭数据库
            CloseData();
            ds.Tables[0].TableName = "Result";
            if (ds.Tables.Count > 1) ds.Tables[1].TableName = "Count";
            return ds;
        }
        #endregion

        private  string DataTableToJson(DataTable dt)
        {
       
            return JsonConvert.SerializeObject(dt, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }); 
        }
        private  string DataSetToJson(DataSet ds)
        {
            return JsonConvert.SerializeObject(ds, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }

    }
}