using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.BLL
{
    public class ShipHelper
    {
        IDAL.IDALSession idal = BLLHelper.GetDal();

        #region 验证备货门区是否正确
        /// <summary>
        /// 验证备货门区是否正确
        /// </summary>
        /// <param name="WhCode">WhCode</param>
        /// <param name="Location">备货门区</param>
        /// <returns></returns>
        public bool CheckOutLocation(string WhCode, string Location)
        {
            return (from a in idal.IWhLocationDAL.SelectAll()
                    where a.LocationId == Location && a.WhCode == WhCode
                    join b in idal.ILocationTypeDAL.SelectAll() on a.LocationTypeId equals b.Id
                    where b.TypeName == "D" && a.Status == "A"
                    select a.Id).Count() > 0;
        }

        //验证托盘状态
        public bool CheckHuStatus(string WhCode, string HuId)
        {
            return (from a in idal.IHuMasterDAL.SelectAll()
                    where a.HuId == HuId && a.WhCode == WhCode && a.Type == "M" && a.Status == "A"
                    select a.Id).Count() > 0;
        }

        //验证托盘的库位是否正常
        public bool CheckLocationStatusByHuId(string WhCode, string HuId)
        {
            return (from a in idal.IHuMasterDAL.SelectAll()
                    join b in idal.IWhLocationDAL.SelectAll()
                    on new { A = a.Location, B = a.WhCode } equals new { A = b.LocationId, B = b.WhCode }
                    where a.HuId == HuId && a.WhCode == WhCode && a.Type == "M" && a.Status == "A" && b.Status == "A"
                    select b.Id).Count() > 0;
        }

        //验证托盘状态
        public bool CheckHuStatusByPickTask(string WhCode, string HuId, string LoadId)
        {
            return (from a in idal.IPickTaskDetailDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode && a.HuId == HuId && a.Status == "U"
                    select a.Id).Count() > 0;
        }

        public bool CheckPlt(string WhCode, string HuId)
        {
            ////验证托盘
            if (IfPlt(WhCode, HuId))
                //验证是否有货
                return !IfPltHaveStock(WhCode, HuId);
            else
                return false;
        }

        /// <summary>
        /// 托盘是否存在
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public bool IfPlt(string WhCode, string HuId)
        {
            return (from a in idal.IPallateDAL.SelectAll()
                    where a.HuId == HuId && a.WhCode == WhCode
                    select a.Id).Count() > 0;
        }

        /// <summary>
        /// 托盘是否有库存
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public bool IfPltHaveStock(string WhCode, string HuId)
        {
            return (from a in idal.IHuMasterDAL.SelectAll()
                    where a.HuId == HuId && a.WhCode == WhCode
                    select a.Id).Count() > 0;
        }

        //验证Load状态
        public bool CheckLoadStatus(string WhCode, string LoadId)
        {
            return (from a in idal.ILoadMasterDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode && (a.Status1 == "U" || a.Status1 == "A")&&a.Status0=="C"
                    select a.Id).Count() > 0;
        }

        //验证客户是否存在
        public bool CheckClientCode(string whCode, string clientCode)
        {
            if (clientCode == "" || clientCode == null)
            {
                return false;
            }

            if (idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == clientCode).Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //获取无库存的系统模拟托盘
        public string GetSysPlt(string WhCode) {

            List<string> sql= (from a in idal.IPallateDAL.SelectAll()
                    join b in idal.IHuMasterDAL.SelectAll()
                    on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode } into temp
                    from tt in temp.DefaultIfEmpty()
                    where a.HuId.StartsWith("SYSPLTD") && a.WhCode == WhCode&&a.Status=="U" && tt.HuId==null
                    select a.HuId).ToList() ;
            if (sql.Count() > 0)
                return sql.First();
            else
                return null;
        }
        
        #endregion
    }
}
