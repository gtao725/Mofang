using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.IBLL
{
    public interface IBaseBLL<T>
        where T : class, new()
        // where SeatchT : class,new()
    {
        //添加新实体对象
        T Add(T entity);

        //更新实体对象
        int Update(T entity, params string[] proNames);

        //删除单个实体对象
        int DeleteById(int id);

        //批量删除实体对象
        int DeleteByListId(List<int> delId);



    }
}
