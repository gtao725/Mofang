using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace WMS.IDAL
{
    public interface IBaseDAL<T> where T : class, new()
    {
        //查询全部
        IQueryable<T> SelectAll();

        //  //单行数据对象添加方法接口
        // T Add(T entity);

        // //多行数据对象添加方法接口
        // int Add(List<T> listEntity);

        // //单行数据对象更新
        // int Update(T entity);

        // //多行数据对象更新
        // int Update(List<T> listEntity);

        // //单行数据对象删除
        // int Delete(T entity);

        // //多行数据对象更新
        // int Delete(List<T> listEntity);

        //  //数据查询接口
        // IQueryable<T> Select(Expression<Func<T, bool>> whereLambda);
        //  //数据分页查询接口
        // IQueryable<T> SelectPage<S>(int pageSize, int pageIndex, out int total, Func<T, bool> whereLambda, Func<T, S> orderbyLambda, bool isAsc);
        //// List<T> SelectPage<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy);

        #region 1.0 新增 实体 +int Add(T model)
        /// <summary>
        /// 新增 实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        T Add(T model);
        #endregion


        IEnumerable<T> Add(IEnumerable<T> modelList);

        #region 2.0 根据 id 删除 +int Del(T model)
        /// <summary>
        /// 根据 id 删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        int Delete(T model);
        #endregion

        #region 3.0 根据条件删除 +int DelBy(Expression<Func<T, bool>> delWhere)
        /// <summary>
        /// 3.0 根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        int DeleteBy(Expression<Func<T, bool>> delWhere);
        #endregion
        int DeleteByExtended(Expression<Func<T, bool>> delWhere);

        #region 4.0 修改 +int Update(T model, params string[] proNames)
        /// <summary>
        /// 4.0 修改，如：
        /// T u = new T() { uId = 1, uLoginName = "asdfasdf" };
        /// this.Modify(u, "uLoginName");
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        int Update(T model, params string[] proNames);
        #endregion

        #region 4.0 批量修改 +int UpdateBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
        /// <summary>
        /// 4.0 批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <returns></returns>
        int UpdateBy(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames);
        #endregion
        int UpdateByExtended(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression);

        #region 5.0 根据条件查询 +List<T> SelectBy(Expression<Func<T,bool>> whereLambda)
        /// <summary>
        /// 5.0 根据条件查询 +List<T> GetListBy(Expression<Func<T,bool>> whereLambda)
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        List<T> SelectBy(Expression<Func<T, bool>> whereLambda);
        #endregion

        #region 5.1 根据条件 排序 和查询 + List<T> SelectListBy<TKey>
        /// <summary>
        /// 5.1 根据条件 排序 和查询
        /// </summary>
        /// <typeparam name="TKey">排序字段类型</typeparam>
        /// <param name="whereLambda">查询条件 lambda表达式</param>
        /// <param name="orderLambda">排序条件 lambda表达式</param>
        /// <returns></returns>
        List<T> SelectListBy<TKey>(Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderLambda);
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
        List<T> SelectPagedList<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> whereLambda, Expression<Func<T, TKey>> orderBy);
        #endregion


        int SaveChanges();

    }
}
