using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class ShipWinceManger : IShipWinceManger
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        ShipHelper shipHelper = new ShipHelper();

        #region CE备货方法
        public bool CheckLoadStatus(string WhCode, string LoadId)
        {
            return shipHelper.CheckLoadStatus(WhCode, LoadId);
        }

        public string CheckPickingLoad(string WhCode, string LoadId, string HuId)
        {
            ShipLoadManager shipLoadManager = new ShipLoadManager();
            return shipLoadManager.CheckPickingLoad(LoadId, WhCode, HuId);
        }

        public int GetLoadProcessId(string WhCode, string LoadId)
        {
            return (from a in idal.ILoadMasterDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode
                    select a.ProcessId).First();
        }
        public string GetSysPlt(string WhCode)
        {
            return shipHelper.GetSysPlt(WhCode);
        }

        public ShipPickDesModel GetPickingRes(string WhCode, string LoadId)
        {

            List<PickTaskDetail> pickTaskDetail = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == WhCode);
            if (pickTaskDetail.Count() > 0)
            {
                ShipPickDesModel shipPickDes = new ShipPickDesModel();
                //未备货托盘数量
                shipPickDes.UnPickPltQty = pickTaskDetail.Where(u => u.Status == "U").Select(t => t.HuId).Distinct().Count();
                //单位数量不一致的话显示...,否则就显示求和数量
                shipPickDes.UnPickQty = pickTaskDetail.Where(u => u.Status == "U").Select(t => t.UnitId).Distinct().Count() > 1
                                        ? "..." : pickTaskDetail.Where(u => u.Status == "U").Select(u => u.Qty).Sum().ToString();
                //已备货托盘数量
                shipPickDes.PickPltQty = pickTaskDetail.Where(u => u.Status != "U").Select(t => t.HuId).Distinct().Count();
                //单位数量不一致的话显示...,否则就显示求和数量
                shipPickDes.PickQty = pickTaskDetail.Where(u => u.Status != "U").Select(t => t.UnitId).Distinct().Count() > 1
                                        ? "..." : pickTaskDetail.Where(u => u.Status != "U").Select(u => u.Qty).Sum().ToString();
                //建议托盘和库位对象,按照库位排序 
                // PickTaskDetail PickedDetailList = pickTaskDetail.Where(u => u.Status == "U").OrderBy(s => s.AltItemNumber).ThenBy(t => t.Location).First();
                PickTaskDetail PickedDetailList = pickTaskDetail.Where(u => u.Status == "U").OrderBy(s => s.PickingSequence).ThenBy(t => t.Location).First();


                shipPickDes.SugPlt = PickedDetailList.HuId;
                shipPickDes.SugLocation = PickedDetailList.Location;
                //返回建议托盘是否需要拆托 
                ShipLoadManager shipLoadManager = new ShipLoadManager();
                shipPickDes.SplitFlag = shipLoadManager.CheckPickingLoad(LoadId, WhCode, PickedDetailList.HuId) == "N" ? "Y" : "N";

                return shipPickDes;
            }
            else
                return null;
        }

        public ShipPickDesModel GetPickingRes(string WhCode, string LoadId, int index)
        {


            List<PickTaskDetail> pickTaskDetail = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == WhCode);
            if (pickTaskDetail.Count() > 0 && pickTaskDetail.Where(u => u.Status == "U").Count() > index)
            {
                ShipPickDesModel shipPickDes = new ShipPickDesModel();
                //未备货托盘数量
                shipPickDes.UnPickPltQty = pickTaskDetail.Where(u => u.Status == "U").Select(t => t.HuId).Distinct().Count();
                //单位数量不一致的话显示...,否则就显示求和数量
                shipPickDes.UnPickQty = pickTaskDetail.Where(u => u.Status == "U").Select(t => t.UnitId).Distinct().Count() > 1
                                        ? "..." : pickTaskDetail.Where(u => u.Status == "U").Select(u => u.Qty).Sum().ToString();
                //已备货托盘数量
                shipPickDes.PickPltQty = pickTaskDetail.Where(u => u.Status != "U").Select(t => t.HuId).Distinct().Count();
                //单位数量不一致的话显示...,否则就显示求和数量
                shipPickDes.PickQty = pickTaskDetail.Where(u => u.Status != "U").Select(t => t.UnitId).Distinct().Count() > 1
                                        ? "..." : pickTaskDetail.Where(u => u.Status != "U").Select(u => u.Qty).Sum().ToString();
                //建议托盘和库位对象,按照库位排序 
                // PickTaskDetail PickedDetailList = pickTaskDetail.Where(u => u.Status == "U").OrderBy(s => s.AltItemNumber).ThenBy(t => t.Location).First();

                List<PickTaskDetail> pickTaskDetailOderBy = pickTaskDetail.Where(u => u.Status == "U").OrderBy(s => s.PickingSequence).ThenBy(u => u.Location).ToList();


                //  // PickTaskDetail PickedDetailList = pickTaskDetail.Where(u => u.Status == "U").OrderBy(s => s.Location).First();

                shipPickDes.SugPlt = pickTaskDetailOderBy[index].HuId;
                shipPickDes.SugLocation = pickTaskDetailOderBy[index].Location;
                //返回建议托盘是否需要拆托 
                ShipLoadManager shipLoadManager = new ShipLoadManager();
                shipPickDes.SplitFlag = shipLoadManager.CheckPickingLoad(LoadId, WhCode, pickTaskDetailOderBy[index].HuId) == "N" ? "Y" : "N";

                return shipPickDes;
            }
            else
                return null;
        }
        public List<ShipPickDesModel> GetPickingResList(string WhCode, string LoadId, int count)
        {

            List<ShipPickDesModel> pickingResList = new List<ShipPickDesModel>();
            var sql = from a in idal.IPickTaskDetailDAL.SelectAll()
                      where a.LoadId == LoadId && a.WhCode == WhCode && a.Status == "U"
                      select new ShipPickDesModel { SugLocation = a.Location, SugPlt = a.HuId };
            pickingResList = sql.OrderBy(s => s.SugLocation).Take(count).ToList();
            return pickingResList;

        }

        public ShipPickDesModel GetPickingPltRes(string WhCode, string HuId)
        {

            List<HuDetail> huDetail = idal.IHuDetailDAL.SelectBy(u => u.WhCode == WhCode && u.HuId == HuId);
            if (huDetail.Count() > 0)
            {
                ShipPickDesModel shipPickDes = new ShipPickDesModel();
                //单位数量不一致的话显示...,否则就显示求和数量
                shipPickDes.PltQty = huDetail.Select(t => t.UnitId).Distinct().Count() > 1
                                        ? "..." : huDetail.Select(u => u.Qty).Sum().ToString();
                return shipPickDes;
            }
            else
                return null;
        }

        //备货是否全部完成,true就是全部完成了
        public bool CheckPickingComplete(string WhCode, string LoadId)
        {
            return (from a in idal.ILoadMasterDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode && a.Status1 == "C"
                    select a.ProcessId).Count() == 1;
        }

        // 
        public List<ShipPickSplitDesModel> GetPickingSplitDes(string WhCode, string LoadId, string HuId)
        {
            //取出库存表和备货表的数据
            List<ShipPickSplitDesModel> sql = (from a in idal.IHuDetailDAL.SelectAll()
                                               join b in idal.IPickTaskDetailDAL.SelectAll()
                                               on new { A = a.Id, B = a.WhCode, C = a.HuId } equals new { A = (int)b.HuDetailId, B = b.WhCode, C = b.HuId } into temp
                                               from tt in temp.DefaultIfEmpty()
                                               where a.WhCode == WhCode && a.HuId == HuId
                                               select new ShipPickSplitDesModel { LoadId = tt.LoadId, AltItemNumber = a.AltItemNumber, UnitName = a.UnitName, Qty = a.Qty, SplitQty = tt.Qty }).ToList();
            //返回求和数据
            return (sql.Where(u => u.LoadId == LoadId).GroupBy(u => new { u.AltItemNumber, u.UnitName })
                                         .Select(g => new ShipPickSplitDesModel
                                         {
                                             AltItemNumber = g.Key.AltItemNumber,
                                             UnitName = g.Key.UnitName,
                                             Qty = g.Sum(u => u.Qty),
                                             SplitQty = g.Sum(u => u.SplitQty)
                                         })).ToList();

        }

        public List<string> GetSerialNumberOut(string WhCode, string LoadId, string HuId)
        {
            return (from a in idal.ISerialNumberOutDAL.SelectAll()
                    where a.WhCode == WhCode && a.LoadId == LoadId && a.HuId == HuId
                    select a.CartonId).ToList();
        }
        public string CheckPickSplitPlt(string WhCode, string LoadId, string HuId, string PutHuId)
        {
            string mess = CheckPickingLoad(WhCode, LoadId, HuId);
            //既不是拆托 也不是整出 表示有误
            if (mess != "N" && mess != "Y")
            {
                return mess;
            }
            //拆托还要验证新托盘
            if (mess == "N")
            {
                if (!shipHelper.CheckPlt(WhCode, PutHuId))
                {
                    return "错误！托盘" + PutHuId + "不存在或已使用！";
                }
            }
            return "Y";
        }
        public string SerialNumberChange(string WhCode, string LoadId, string HuId, string PutHuId, string UserName)
        {
            idal.ISerialNumberOutDAL.UpdateByExtended(u => u.WhCode == WhCode && u.LoadId == LoadId && u.HuId == HuId, t => new SerialNumberOut { HuId = PutHuId, UpdateUser = UserName, UpdateDate = DateTime.Now });
            return "Y";
        }



        #endregion

        #region 封箱方法
        public bool CheckDeliveryLoadStatus(string WhCode, string LoadId)
        {

            //备货完成即可封箱,太阳能项目自动装箱
            return (from a in idal.IPickTaskDetailDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode && a.Status == "U"
                    select a.Id).Count() == 0;
        }
        //public string GetContainerNumberLoadId(string WhCode, string ContainerNumber)
        //{
        //    var sql = from a in idal.ILoadMasterDAL.SelectAll()
        //              join b in idal.ILoadContainerExtendDAL.SelectAll() on new { A = a.LoadId, B = a.WhCode } equals new { A = b.LoadId, B = b.WhCode }
        //              where b.ContainerNumber == ContainerNumber && b.WhCode == WhCode && a.ShipDate == null
        //              select a.LoadId   ;
        //    if (sql.Count() == 1)
        //        return sql.First();
        //    else
        //        return null;
        //}
        //public bool CheckDeliverySealNumber(string WhCode, string LoadId, string SealNumber)
        //{
        //    var sql = from a in idal.ILoadContainerExtendDAL.SelectAll()
        //               where a.LoadId == LoadId && a.WhCode == WhCode && (a.SealNumber == SealNumber|| a.SealNumber==null|| a.SealNumber=="")
        //              select a.LoadId;
        //    return sql.Count() == 1;

        //}




        public ShipLoadDesModel GetShipLoadDesHead(string WhCode, string LoadId)
        {
            LoadWinceManger loadWinceManger = new LoadWinceManger();
            return loadWinceManger.GetShipLoadDesHead(WhCode, LoadId);

        }
        public string ShippingLoad(string loadId, string whCode, string userName)
        {
            ShipLoadManager shipLoadManager = new ShipLoadManager();
            return shipLoadManager.ShippingLoad(loadId, whCode, userName);
        }
        public string ShippingLoadCustomer(string loadId, string DeliveryOrderNumber, string whCode, string userName)
        {

            ShipLoadManager shipLoadManager = new ShipLoadManager();
            //自动装箱
            string res = shipLoadManager.adminSetPackingLoad(loadId, whCode, userName);

            if (res == "Y")
            {
                res = shipLoadManager.ShippingLoad(loadId, whCode, userName);
                if (res == "Y")
                {

                    LoadMaster ld = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).First();
                    ld.Remark = ld.Remark + " 发运单号:" + DeliveryOrderNumber;
                    ld.UpdateUser = userName;
                    ld.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(ld, u => u.WhCode == whCode && u.LoadId == loadId, new string[] { "Remark", "UpdateUser", "UpdateDate" });
                    idal.ILoadMasterDAL.SaveChanges();
                    return "Y";
                }
                else
                    return res;
            }
            else
                return "装箱失败!数据异常";
        }


        public string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber)
        {
            ShipLoadManager shipLoadManager = new ShipLoadManager();
            return shipLoadManager.ShippingLoad(loadId, whCode, userName, containerNumber, sealNumber);
        }

        public string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber, int baQty)
        {
            ShipLoadManager shipLoadManager = new ShipLoadManager();
            return shipLoadManager.ShippingLoad(loadId, whCode, userName, containerNumber, sealNumber, baQty);
        }


        public bool DeliveryQtyCheck(string loadId, string whCode)
        {
            return (from a in idal.IPickTaskDetailDAL.SelectAll()
                    where a.LoadId == loadId && a.WhCode == whCode && a.UnitName.Contains("ECH")
                    select a.Id).Count() > 0;
        }

        #endregion
    }
}
