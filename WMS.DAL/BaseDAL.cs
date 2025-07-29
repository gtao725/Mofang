using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WMS.IDAL;
using EntityFramework.Extensions;
namespace WMS.DAL
{
   public class BaseDAL<T> : IBaseDAL<T> where T : class,new()
    {
       DbContext db = EFDbContextFactory.GetCurrentDbContext();
            //new MODEL_MSSQL.WMS_Entities();





       #region 1.0 新增 实体 +int Add(T model)
       /// <summary>
       /// 新增 实体
       /// </summary>
       /// <param name="model"></param>
       /// <returns></returns>
       public T Add(T model)
       {
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Set<T>().Add(model);
           return model;//保存成功后，会将自增的id设置给 model的 主键属性，并返回受影响行数
       }
        #endregion

        public IEnumerable<T> Add(IEnumerable<T> modelList)
        {
            db.Configuration.ValidateOnSaveEnabled = false;
            db.Set<T>().AddRange(modelList);
            return modelList;//保存成功后，会将自增的id设置给 model的 主键属性，并返回受影响行数
        }

        #region 2.0 根据 id 删除 +int Delete(T model)
        /// <summary>
        /// 根据 id 删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        public int Delete(T model)
       {

           db.Set<T>().Attach(model);
           db.Set<T>().Remove(model);
           return 1;
       }

       #endregion

       #region 3.0 根据条件删除 +int DeleteBy(Expression<Func<T, bool>> delWhere)
       /// <summary>
       /// 3.0 根据条件删除
       /// </summary>
       /// <param name="delWhere"></param>
       /// <returns></returns>
       public int DeleteBy(Expression<Func<T, bool>> delWhere)
       {
           //3.1查询要删除的数据
           List<T> listDeleting = db.Set<T>().Where(delWhere).ToList();
           //3.2将要删除的数据 用删除方法添加到 EF 容器中
           listDeleting.ForEach(u =>
           {
               db.Set<T>().Attach(u);//先附加到 EF容器
               db.Set<T>().Remove(u);//标识为 删除 状态
           });
           //3.3一次性 生成sql语句到数据库执行删除
           return 1;
       }
        #endregion

        #region 3.1 根据条件删除 +int DeleteBy(Expression<Func<T, bool>> delWhere)
        /// <summary>
        /// 3.1 根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        public int DeleteByExtended(Expression<Func<T, bool>> delWhere)
        {
            
            return SelectAll().Where(delWhere).Delete(); ;
        }
 
        //public int DeleteAsyncByExtended(Expression<Func<T, bool>> delWhere)
        //{

        //    return SelectAll().Where(delWhere).DeleteAsync(); ;
        //}
        #endregion
        #region 4.0 修改 +int Update(T model, params string[] proNames)
        /// <summary>
        /// 4.0 修改，如：
        /// T u = new T() { uId = 1, uLoginName = "asdfasdf" };
        /// this.Modify(u, "uLoginName");
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        public int Update(T model, params string[] proNames)
       {
            db.Configuration.ValidateOnSaveEnabled = false;
            //4.1将 对象 添加到 EF中
            DbEntityEntry entry = db.Entry<T>(model);
           //4.2先设置 对象的包装 状态为 Unchanged
           entry.State = System.Data.Entity.EntityState.Unchanged;
           //4.3循环 被修改的属性名 数组
           foreach (string proName in proNames)
           {
               //4.4将每个 被修改的属性的状态 设置为已修改状态;后面生成update语句时，就只为已修改的属性 更新
               entry.Property(proName).IsModified = true;
           }
           //4.4一次性 生成sql语句到数据库执行
           return 1;
       }
       #endregion

       #region 4.0 批量修改 +int UpdateBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
       /// <summary>
       /// 4.0 批量修改
       /// </summary>
       /// <param name="model">要修改的实体对象</param>
       /// <param name="whereLambda">查询条件</param>
       /// <param name="proNames">要修改的 属性 名称</param>
       /// <returns></returns>
       public int UpdateBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
       {
            //4.1查询要修改的数据
            List<T> listModifing = db.Set<T>().Where(whereLambda).ToList();
           
           //获取 实体类 类型对象
           Type t = typeof(T); // model.GetType();
           //获取 实体类 所有的 公有属性
           List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
           //创建 实体属性 字典集合
           Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
           //将 实体属性 中要修改的属性名 添加到 字典集合中 键：属性名  值：属性对象
           proInfos.ForEach(p =>
           {
               if (modifiedProNames.Contains(p.Name))
               {
                   dictPros.Add(p.Name, p);
               }
           });

           //4.3循环 要修改的属性名
           foreach (string proName in modifiedProNames)
           {
               //判断 要修改的属性名是否在 实体类的属性集合中存在
               if (dictPros.ContainsKey(proName))
               {
                   //如果存在，则取出要修改的 属性对象
                   PropertyInfo proInfo = dictPros[proName];
                   //取出 要修改的值
                   object newValue = proInfo.GetValue(model, null); //object newValue = model.uName;

                   //4.4批量设置 要修改 对象的 属性
                   foreach (T usrO in listModifing)
                   {
                       //为 要修改的对象 的 要修改的属性 设置新的值
                       proInfo.SetValue(usrO, newValue, null); //usrO.uName = newValue;
                   }
               }
           }
           //4.4一次性 生成sql语句到数据库执行
           return 1;
       }
        #endregion

        #region 4.1 批量修改  
        public int UpdateByExtended(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression)
        {
            return SelectAll().Where(filterExpression).Update(updateExpression);
        }
        #endregion
        #region 5.0 根据条件查询 +List<T> SelectBy(Expression<Func<T,bool>> whereLambda)
        /// <summary>
        /// 5.0 根据条件查询 +List<T> GetListBy(Expression<Func<T,bool>> whereLambda)
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> SelectBy(Expression<Func<T, bool>> whereLambda)
       {
           return db.Set<T>().Where(whereLambda).ToList();
       }
       #endregion

       #region 5.1 根据条件 排序 和查询 + List<T> SelectListBy<TKey>
       /// <summary>
       /// 5.1 根据条件 排序 和查询
       /// </summary>
       /// <typeparam name="TKey">排序字段类型</typeparam>
       /// <param name="whereLambda">查询条件 lambda表达式</param>
       /// <param name="orderLambda">排序条件 lambda表达式</param>
       /// <returns></returns>
       public List<T> SelectListBy<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderLambda)
       {
           //List<int> listIds = new List<int>() { 1, 2, 3 };
           //new MODEL.OuOAEntities().Ou_UserInfo.Where(u => listIds.Contains(u.uId));
           return db.Set<T>().Where(whereLambda).OrderBy(orderLambda).ToList();
       }
       #endregion

       #region 6.0 分页查询 + List<T> SelectPagedList<TKey>
       /// <summary>
       /// 6.0 分页查询 + List<T> GetPagedList<TKey>
       /// </summary>
       /// <param name="pageIndex">页码</param>
       /// <param name="pageSize">页容量</param>
       /// <param name="whereLambda">条件 lambda表达式</param>
       /// <param name="orderBy">排序 lambda表达式</param>
       /// <returns></returns>
       public List<T> SelectPagedList<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy)
       {
           // 分页 一定注意： Skip 之前一定要 OrderBy
           return db.Set<T>().Where(whereLambda).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
       }
       #endregion
       //public T Add(T entity)
        //{
        //    db.Set<T>().Add(entity);
        //    //db.SaveChanges();//保存成功后，会将自增的id设置给 model的 主键属性，并返回受影响行数
        //    return entity;
        //}

        //public bool Add(List<T> listEntity)
        //{
        //    foreach (var item in listEntity)
        //    {
        //        db.Set<T>().Add(item);
        //    }
        //    //return db.SaveChanges() > 0;
        //    return true;
        //}


        //public bool Update(T entity)
        //{
        //    db.Entry(entity).State = EntityState.Modified;
        //    //return db.SaveChanges() > 0;
        //    return true;
        //}

        //public bool Update(List<T> listEntity)
        //{
        //    foreach (var item in listEntity)
        //    {
        //        db.Entry(item).State = EntityState.Modified;
        //    }
        //    //return db.SaveChanges() > 0;
        //    return true;
        //}


        //public bool Delete(T entity)
        //{
        //    //db.Set<T>().Attach(entity);//先附加到 EF容器
        //    //db.Set<T>().Remove(entity);//标识为 删除 状态
        //    db.Entry(entity).State = EntityState.Deleted;
        //   // return db.SaveChanges() > 0;
        //    return true;
        //}

        //public bool Delete(List<T> listEntity)
        //{
        //    foreach (var item in listEntity)
        //    {
        //        db.Entry(item).State = EntityState.Deleted;
        //    }
        //    //return db.SaveChanges() > 0;
        //    return true;
        //}


        //// public List<T> SelectPage<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy)
        //// {
        //// 分页 一定注意： Skip 之前一定要 OrderBy
        ////    return db.Set<T>().Where(whereLambda).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        ////  }

        //public IQueryable<T> SelectPage<S>(int pageSize, int pageIndex, out int total, Func<T, bool> whereLambda, Func<T, S> orderbyLambda, bool isAsc)
        //{
        //    total = db.Set<T>().Where(whereLambda).Count();
        //    if (isAsc)
        //    {
        //        return
        //        db.Set<T>()
        //          .Where(whereLambda)
        //          .OrderBy(orderbyLambda)
        //          .Skip(pageSize * (pageIndex - 1))
        //          .Take(pageSize)
        //          .AsQueryable();
        //    }
        //    else
        //    {
        //        return
        //       db.Set<T>()
        //         .Where(whereLambda)
        //         .OrderByDescending(orderbyLambda)
        //         .Skip(pageSize * (pageIndex - 1))
        //         .Take(pageSize)
        //         .AsQueryable();
        //    }
        //}

        //public IQueryable<T> Select(Expression<Func<T, bool>> whereLambda)
        //{
        //    return db.Set<T>().Where(whereLambda).AsQueryable(); 
        //}

    

        public IQueryable<T> SelectAll()
        {
            return db.Set<T>().AsQueryable().AsNoTracking();
        }


        public int SaveChanges()
        {
            db.Configuration.ValidateOnSaveEnabled = false;
            return db.SaveChanges();
        }
    }
}
