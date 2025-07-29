using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class GrnWeb : IGrnWeb
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        public List<GrnHeadResult> GrnHeadList(GrnHeadSearch search, out int total)
        {
            var sql = from a in idal.IDamcoGRNHeadDAL.SelectAll()
                      join b in idal.IDamcoGRNDetailDAL.SelectAll()
                            on new { a.WhCode, a.SoNumber, a.ClientCode }
                        equals new { b.WhCode, b.SoNumber, b.ClientCode } into b_join
                      from b in b_join.DefaultIfEmpty() 
                      //&& a.ClientCode == "ADEO"
                      group new { a, b } by new
                      {
                          a.Id,
                          a.WhCode,
                          a.ClientCode,
                          a.SoNumber,
                          a.WmsQty,
                          a.WmsCbm,
                          a.SendType,
                          a.Status,
                          a.SendTime,
                          a.CreateDate
                      } into g
                      select new GrnHeadResult
                      {
                          Id=g.Key.Id,
                          WhCode=g.Key.WhCode,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          SendType = g.Key.SendType,
                          Status = g.Key.Status,
                          SendTime = (DateTime?)g.Key.SendTime,
                          GWI_Qty = (Int32?)g.Sum(p => p.b.GWI_Qty),
                          GWI_Cbm = (Double?)Math.Round((double)g.Sum(p => p.b.GWI_Cbm), 2),
                          GWI_Kgs = (Double?)Math.Round((double)g.Sum(p => p.b.GWI_Kgs), 2),
                          WmsQty = (Int32?)g.Key.WmsQty,
                          WmsCbm = (Double?)Math.Round((double)g.Key.WmsCbm,2),
                          //WMS_Kgs = (Double?)Math.Round((double)g.Sum(p => p.b.WMS_Kgs), 2),
                          GRN_Qty = (Int32?)g.Sum(p => p.b.GRN_Qty),
                          GRN_Cbm = (Double?)Math.Round((double)g.Sum(p => p.b.GRN_Cbm), 2),
                          GRN_Kgs = (Double?)Math.Round((double)g.Sum(p => p.b.GRN_Kgs), 2),
                          CreateDate = (DateTime?)g.Key.CreateDate
                      };

            if (search.SoL != null)
            {
                sql = sql.Where(u => search.SoL.Contains(u.SoNumber));
            }
            if (search.BeginCreateDate != null)
                sql = sql.Where(u => u.CreateDate >= search.BeginCreateDate);
            if (search.EndCreateDate != null)
                sql = sql.Where(u => u.CreateDate <= search.EndCreateDate);
            if (!string.IsNullOrEmpty(search.ClientCode))
                sql = sql.Where(u => u.ClientCode == search.ClientCode);
            //if (search.receiptid != null)
            //    sql = sql.Where(u => u. == search.receiptid);
            if (!string.IsNullOrEmpty(search.WhCode))
                sql = sql.Where(u => u.WhCode == search.WhCode);
            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(search.pageSize * (search.pageIndex - 1)).Take(search.pageSize);

            List<GrnHeadResult> res = sql.ToList();


            return res;
;
        }

        public List<DamcoGRNDetail> GrnSOList(GrnHeadSearch search, out int total) {
            var sql = idal.IDamcoGRNDetailDAL.SelectBy(u => u.ClientCode == search.ClientCode && u.SoNumber == search.So && u.WhCode==search.WhCode);
            total = sql.Count();
            return sql.ToList();
        }
    }
}
