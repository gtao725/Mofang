using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WMS.IDAL;

namespace WMS.BLL
{
   public  class BLLHelper
    {
     public static IDAL.IDALSession GetDal(){


          IDAL.IDALSessionFactory iDALSessionFactory = DI.SpringHelper.GetObject<WMS.IDAL.IDALSessionFactory>("DBSessFactory");

          DAL.DALSession dALSession = (DAL.DALSession)iDALSessionFactory.GetDALSession();

          return dALSession;
      }
 

    }
}
