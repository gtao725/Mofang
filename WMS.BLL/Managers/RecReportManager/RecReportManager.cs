using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class RecReportManager : IRecReportManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();

        //客户下拉菜单列表
        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.Status == "Active" && a.WhCode == whCode
                      select a;
            return sql.AsEnumerable();
        }

        //收货查询
        //对应 C_ReceiveController 中的 List 方法
        public List<ReceiptReportResult> C_ReceiveList(ReceiptReportSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IItemMasterDAL.SelectAll()
                      on new { A = a.WhCode, B = a.ItemId } equals new { A = b.WhCode, B = b.Id }
                      select new ReceiptReportResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          WhCode = a.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          ReceiptDate = a.ReceiptDate,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          Style1 = b.Style1,
                          HuId = a.HuId,
                          Qty = a.Qty,
                          UnitName = a.UnitName,
                          Length = a.Length,
                          Width = a.Width,
                          Height = a.Height,
                          Weight = a.Weight,
                          CBM = a.Length * a.Width * a.Height * a.Qty
                      };

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId.Contains(searchEntity.ReceiptId));
            if (searchEntity.ClientId != 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

    }
}
