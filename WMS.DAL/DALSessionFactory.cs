using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using WMS.IDAL;

namespace WMS.DAL
{
    public class DALSessionFactory : IDALSessionFactory
    {
        public  IDALSession GetDALSession()
        {
            //从当前线程中 获取 DBContext 数据仓储 对象
            IDALSession dalSesion = (DALSession)CallContext.GetData(typeof(DALSessionFactory).Name);

            if (dalSesion == null)
            {
                dalSesion = new DALSession();
                CallContext.SetData(typeof(DALSessionFactory).Name, dalSesion);
            }
            return dalSesion;
        }
    }
}
