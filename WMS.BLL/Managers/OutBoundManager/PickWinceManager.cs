using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.Express;
using WMS.IBLL;

namespace WMS.BLL
{
    public class PickWinceManager : IPickWinceManger
    {

        IDAL.IDALSession idal = BLLHelper.GetDal();

        //获取拣货LOAD明细,流程ProcessId对才行
        public List<string> PickLoadList(string WhCode, string LoadId, string UserName)
        {
            return idal.ILoadMasterDAL.SelectBy(u => (u.LoadId.Contains(LoadId)|| LoadId==null) && u.WhCode == WhCode&&u.ProcessId==39 && u.Status0=="C"&& new string[] { "U", "A" }.Contains(u.Status1)).Select(u => u.LoadId).ToList();
           
        }

        //获取拣货订单
        public List<string> GetPickTaskOrder(string WhCode, string LoadId)
        {

            return (from a in idal.IPickTaskDetailDAL.SelectAll()
                    join b in idal.IOutBoundOrderDAL.SelectAll() on a.OutBoundOrderId equals b.Id
                    where a.LoadId == LoadId && a.WhCode == WhCode&& new string[] { "U", "A" }.Contains(a.Status)
                    select b.AltCustomerOutPoNumber+"$"+b.Id).Distinct().ToList();

        }


        //获取拣货明细
        public List<PickTaskDetailWince> GetPickTaskDetail(string WhCode, string LoadId,int OutBoundOrderId)
        {

            return (from a in idal.IPickTaskDetailDAL.SelectAll()
                    join b in idal.IItemMasterDAL.SelectAll() on a.ItemId equals b.Id
                    where a.LoadId == LoadId && a.WhCode == WhCode && a.OutBoundOrderId == OutBoundOrderId && new string[] { "U", "A" }.Contains(a.Status)
                    select new PickTaskDetailWince
                    {
                        Id = a.Id,
                        WhCode = a.WhCode,
                        LoadId = a.LoadId,
                        OutBoundOrderId = OutBoundOrderId,
                        HuId = a.HuId,
                        Location = a.Location,
                        SoNumber = a.SoNumber,
                        CustomerPoNumber = a.CustomerPoNumber,
                        AltItemNumber = a.AltItemNumber,
                        ItemId = a.ItemId,
                        Qty = a.Qty,
                        PickQty = a.PickQty,
                        Sequence = a.Sequence==null?0: a.Sequence,
                        HandFlag = b.HandFlag,
                        ScanFlag = (b.ScanFlag==null?0: b.ScanFlag),
                        ScanRule = b.ScanRule,
                        EAN = b.EAN
                    }).ToList();
        }




         
    }
}
