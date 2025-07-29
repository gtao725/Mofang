using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.BLL
{
    public class InventoryHelper
    {
        IDAL.IDALSession idal = BLLHelper.GetDal();

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

        //验证库位是否存在
        public string CheckLocationId(string locationId, string whCode)
        {
            List<WhLocation> list = idal.IWhLocationDAL.SelectBy(u => u.LocationId == locationId && u.WhCode == whCode && (u.LocationTypeId == 1 || u.LocationTypeId == 5 || u.LocationTypeId == 6));
            if (list.Count > 0)
            {
                if (list.First().Status == "H")
                {
                    return "库位被冻结！";
                }
                else
                {
                    return "Y";
                }
            }
            else
            {
                return "库位不存在！";
            }
        }


        /// <summary>
        /// 检测托盘是否锁定
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns>Y是锁定,N是未锁定,否则就是异常原因</returns>
        public string IfHuIdLocked(string WhCode, string HuId)
        {
            IQueryable<string> res = from a in idal.IHuMasterDAL.SelectAll()
                                     where a.HuId == HuId && a.WhCode == WhCode
                                     select a.Status;


            if (res.Count() == 0)
                return "托盘无库存!";
            else if (res.Count() > 0)
                if (res.First() == "H")
                    return "Y";
                else if (res.First() == "A")
                    return "N";
                else
                    return "库存状态异常";
            else
                return "数据异常";
        }
        /// <summary>
        /// 门区是否存在
        /// </summary>
        /// <param name="whCode"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public bool IfLocation(string whCode, string locationId)
        {
            List<WhLocation> list = idal.IWhLocationDAL.SelectBy(u => u.LocationId == locationId && u.WhCode == whCode && u.Status == "A");
            return list.Count > 0;
        }
        /// <summary>
        /// 验证门区是否存在,并且类型
        /// </summary>
        /// <param name="whCode"></param>
        /// <param name="locationId"></param>
        /// <param name="LocationTypeId">收货门区类型</param>
        /// <returns></returns>
        public bool IfLocation(string whCode, string locationId, int LocationTypeId)
        {
            List<WhLocation> list = idal.IWhLocationDAL.SelectBy(u => u.LocationId == locationId && u.WhCode == whCode && u.LocationTypeId == LocationTypeId);
            return list.Count > 0;
        }
        public string PltHoldEdit(HuMaster huMaster)
        {
            //验证是否存在
            if (!IfPlt(huMaster.WhCode, huMaster.HuId))
                return "托盘不存在!";
            //验证是否有货
            if (!IfPltHaveStock(huMaster.WhCode, huMaster.HuId))
                return "托盘无库存!";
            idal.IHuMasterDAL.UpdateBy(huMaster, u => u.HuId == huMaster.HuId && u.WhCode == huMaster.WhCode, new string[] { "Status", "HoldId", "HoldReason", "UpdateUser", "UpdateDate" });

            if (huMaster.Status == "A")
            {
                TranLog tranLog = new TranLog();
                tranLog.TranType = "151";
                tranLog.Description = "解冻操作";
                tranLog.TranDate = DateTime.Now;
                tranLog.TranUser = huMaster.UpdateUser;
                tranLog.WhCode = huMaster.WhCode;
                tranLog.HuId = huMaster.HuId;
                tranLog.ReceiptId = huMaster.ReceiptId;
                tranLog.Location = huMaster.Location;
                tranLog.HoldId = huMaster.HoldId;
                tranLog.HoldReason = huMaster.HoldReason;
                idal.ITranLogDAL.Add(tranLog);
            }
            else
            {
                TranLog tranLog = new TranLog();
                tranLog.TranType = "152";
                tranLog.Description = "冻结操作";
                tranLog.TranDate = DateTime.Now;
                tranLog.TranUser = huMaster.UpdateUser;
                tranLog.WhCode = huMaster.WhCode;
                tranLog.HuId = huMaster.HuId;
                tranLog.ReceiptId = huMaster.ReceiptId;
                tranLog.Location = huMaster.Location;
                tranLog.HoldId = huMaster.HoldId;
                tranLog.HoldReason = huMaster.HoldReason;
                idal.ITranLogDAL.Add(tranLog);
            }

            idal.IHuMasterDAL.SaveChanges();
            return "Y";
        }
        /// <summary>
        /// 获取托盘的库位
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns>没有话返回null</returns>
        public string GetPltLocation(string WhCode, string HuId)
        {
            if (IfPltHaveStock(WhCode, HuId))
                return (from a in idal.IHuMasterDAL.SelectAll()
                        where a.HuId == HuId && a.WhCode == WhCode
                        select a.Location).First();
            else
                return null;
        }

        //验证仓库是否是无托盘的库存管理,如果是无托盘管理 库存中托盘号变更为库位号管理
        public bool CheckWhCodeIsNoHuIdFlag(string WhCode)
        {
            List<WhInfo> list = idal.IWhInfoDAL.SelectBy(u => u.WhCode == WhCode && u.NoHuIdFlag > 0);
            return list.Count > 0;
        }
    }
}
