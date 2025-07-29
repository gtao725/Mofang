using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.DAL
{
    /// <summary>
    /// DbSession:本质就是一个简单工厂，就是这么一个简单工厂，但从抽象意义上来说，它就是整个数据库访问层的统一入口。
    /// 因为拿到了DbSession就能够拿到整个数据库访问层所有的dal。
    /// </summary>
    public partial class DALSession : IDAL.IDALSession
    {
        /// <summary>
        /// 跨世纪的。
        /// 能实现一个设计模式：  UnitWork
        /// 能够做到让开发人员可以随意在BLL层空数据库访问进行批量的提交。
        /// </summary>
        /// <returns></returns>

        public int SaveChanges()
        {
            //这里只需要调用当前线程内部的上下文SaveChange就完事了。
            DbContext dbContext = EFDbContextFactory.GetCurrentDbContext();
            return dbContext.SaveChanges();
        }
        public List<T> ExecSqlToList<T>(string sql, SqlParameter[] sqlParameter)
        {

            DbContext dbContext = EFDbContextFactory.GetCurrentDbContext();
            List<T> res = new List<T>();
            res = dbContext.Database.SqlQuery<T>(sql, sqlParameter).ToList();
            return res;
        }
        public string ExecSqlToString(string sql, SqlParameter[] sqlParameter)
        {

            DbContext dbContext = EFDbContextFactory.GetCurrentDbContext();

            return dbContext.Database.SqlQuery(typeof(string), sql, sqlParameter).Cast<string>().First();
        }
        public void ExecSql(string sql, SqlParameter[] sqlParameter)
        {

            DbContext dbContext = EFDbContextFactory.GetCurrentDbContext();
            dbContext.Database.ExecuteSqlCommand(sql, sqlParameter);
        }
        public void ExecSqlAsync(string sql, SqlParameter[] sqlParameter)
        {
            DbContext dbContext = EFDbContextFactory.GetCurrentDbContext();
            dbContext.Database.ExecuteSqlCommandAsync(sql, sqlParameter);
        }


    }
}
