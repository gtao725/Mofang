using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.IBLL;

namespace WMS.BLL
{
    public abstract class BaseBLL<T> : IBLL.IBaseBLL<T> where T : class, new()
    {
        protected IDAL.IBaseDAL<T> idal;

        //构造函数给子类调用
        public BaseBLL()
        {
            //子类的方法给idal赋值
            Setidal();
        }
        public abstract void Setidal();

        public T Add(T entity)
        {
            idal.Add(entity);
            idal.SaveChanges();
            return entity;
        }

        public int Update(T entity, params string[] proNames)
        {
            idal.Update(entity, proNames);
            return idal.SaveChanges();
        }

        public abstract int DeleteById(int id);

        public abstract int DeleteByListId(List<int> delId);

    }
}
