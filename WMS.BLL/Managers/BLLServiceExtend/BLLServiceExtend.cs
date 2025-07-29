using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;
using System.Collections;

namespace WMS.BLL
{

    public partial class WhUserPositionService : IBaseBLL<WhUserPosition>
    {
        public WhUserPosition WhUserPositionAdd(WhUserPosition entity)
        {
            //if (idal.SelectBy(u => u.WhCode == entity.WhCode && u.UserId == entity.UserId && u.PositionId == entity.PositionId).Count() == 0)
            //{
            //    idal.Add(entity);
            //    idal.SaveChanges();
            //    return entity;
            //}
            //else
             return null;
        }
    }

    //public partial class WhPositionPowerService : IBaseBLL<WhPositionPower>
    //{
    //    public WhPositionPower WhPositionPowerAdd(WhPositionPower entity)
    //    {
    //        if (idal.SelectBy(u => u.WhCode == entity.WhCode && u.PositionId == entity.PositionId && u.PowerId == entity.PowerId).Count() == 0)
    //        {
    //            idal.Add(entity);
    //            idal.SaveChanges();
    //            return entity;
    //        }
    //        else
    //            return null;
    //    }

    //}


    //public partial class InBoundOrderDetailService : BaseBLL<InBoundOrderDetail>, IInBoundOrderDetailService
    //{
    //    public List<InBoundOrderDetail> InBoundOrderDetailPageSelect(InBoundOrderDetailSearch searchEntity, out int total)
    //    {
    //        var sql = from a in idal.SelectAll()
    //                  select a;
    //        total = 0;


    //        sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
    //        return sql.ToList();

    //    }
    //}

    //public partial class InBoundOrderService : BaseBLL<InBoundOrder>, IInBoundOrderService
    //{

    //    IDAL.IDALSession dal = BLL.BLLHelper.GetDal();

    //    public List<InBoundOrderResult> InBoundOrderPageSelect(InBoundOrderSearch searchEntity, out int total)
    //    {
    //        //var sql = from a in idal.SelectAll()
    //        //          join b in dal.IInBoundOrderDetailDAL.SelectAll()
    //        //          on new { A = a.Id, B = a.WhCode, C = a.PoNumber, D = a.CustomerPoNumber, E = a.SoNumber } equals new { A = b.PoId, B = b.WhCode, C = b.PoNumber, D = b.CustomerPoNumber, E = b.SoNumber }
    //        //          group new { b.Qty, b.RegQty } by new { a.Id, a.WhCode, a.ClientCode, a.ForwarderCode, a.SoNumber, a.CustomerPoNumber, a.ProcessId, a.ProcessName, a.CreateUser, a.CreateDate, a.PlanInTime, a.UpdateUser, a.UpdateDate } into tempa
    //        //          select new InBoundOrderResult
    //        //          {
    //        //              Id = tempa.Key.Id,
    //        //              WhCode = tempa.Key.WhCode,
    //        //              ClientCode = tempa.Key.ClientCode,
    //        //              ForwarderCode = tempa.Key.ForwarderCode,
    //        //              SoNumber = tempa.Key.SoNumber,
    //        //              CustomerPoNumber = tempa.Key.CustomerPoNumber,
    //        //              ProcessId = (int)tempa.Key.ProcessId,
    //        //              ProcessName = tempa.Key.ProcessName,
    //        //              CreateUser = tempa.Key.CreateUser,
    //        //              CreateDate = (DateTime)tempa.Key.CreateDate,
    //        //              PlanInTime = (DateTime)tempa.Key.PlanInTime,
    //        //              UpdateUser = tempa.Key.UpdateUser,
    //        //              UpdateDate = (DateTime)tempa.Key.UpdateDate,
    //        //              TotalRegQty = tempa.Sum(u => u.RegQty),
    //        //              TotalQty = tempa.Sum(u => u.Qty)

    //        //          };

    //        total = 0;
    //        return null;

    //    }


    //}

    //public partial class InBoundOrderDetailService : BaseBLL<InBoundOrderDetail>, IInBoundOrderDetailService
    //{
    //    public string Delete(InBoundOrderDetail entity)
    //    {
    //        int result = idal.SelectBy(u => u.Id == entity.Id && u.RegQty != 0).Count;
    //        if (result > 0)
    //        {
    //            return "当前明细已做登记，无法删除！";
    //        }
    //        else
    //        {
    //            idal.Delete(entity);
    //            idal.SaveChanges();
    //            return "删除成功！";
    //        }
    //    }
    //}


    //public partial class WhClientService : BaseBLL<WhClient>, IWhClientService
    //{

    //    //检查客户超时0为失败,1未成功
    //    //int CLinetCheckOverTime(int Id);
    //    public int CLinetCheckOverTime(int Id)
    //    {
    //        return 1;
    //    }
    //}

    //public partial class WhInfoService : BaseBLL<WhInfo>, IWhInfoService
    //{

    //    //检查客户超时0为失败,1未成功
    //    public int CLinetWhOverTime(int Id)
    //    {
    //        return 1;
    //    }

    //}

}
