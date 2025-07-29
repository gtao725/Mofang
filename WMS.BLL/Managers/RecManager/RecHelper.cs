using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.BLL
{
    public class RecHelper
    {
        IDAL.IDALSession idal = BLLHelper.GetDal();

        public bool CheckReceiptId(string ReceiptId, string WhCode)
        {
            //验证收货批次号
            return (from a in idal.IReceiptRegisterDAL.SelectAll()
                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.Status != "C" && a.Status != "N"
                    select a.Id).Count() > 0;
        }

        //验证SOPO是否存在及有误
        public bool CheckSoPo(string ReceiptId, string WhCode, string SoNumber, string CustomerPoNumber)
        {
            List<InBoundOrder> inorlist = (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                                           join b in idal.IInBoundOrderDetailDAL.SelectAll() on a.InOrderDetailId equals b.Id
                                           join c in idal.IInBoundOrderDAL.SelectAll() on b.PoId equals c.Id
                                           where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
                                           select c).ToList();
            if (inorlist.Count == 0)
            {
                return false;
            }
            InBoundOrder inorder = inorlist.First();
            if (inorder.SoId != null)
            {
                //如果SOID不为空 但收货SO为空 返回假（错误）
                if (string.IsNullOrEmpty(SoNumber) == true)
                {
                    return false;
                }
                else
                {
                    return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                            join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
                            join c in idal.IInBoundSODAL.SelectAll() on b.SoId equals c.Id
                            where c.SoNumber == SoNumber && a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
                            select a.ReceiptId).Count() > 0;
                }
            }
            else
            {
                return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                        join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
                        where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
                        select a.ReceiptId).Count() > 0;
            }

            //if (string.IsNullOrEmpty(SoNumber))
            //    return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
            //            join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
            //            where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
            //            select a.ReceiptId).Count() > 0;
            //else
            //    return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
            //            join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
            //            join c in idal.IInBoundSODAL.SelectAll() on b.SoId equals c.Id
            //            where c.SoNumber == SoNumber && a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
            //            select a.ReceiptId).Count() > 0;
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

        /// <summary>
        /// 验证是否是同一个库位类型
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location"></param>
        /// <param name="DestLoc"></param>
        /// <returns></returns>
        public bool CheckLocationTypes(string WhCode, string Location,string DestLoc)
        {
            return (from a in idal.IWhLocationDAL.SelectAll()
                    where a.WhCode == WhCode && (a.LocationId == Location || a.LocationId == DestLoc)
                    select a.LocationTypeId).Distinct().Count()==1;
        }


        public bool CheckSku(string ReceiptId, string WhCode, int ItemId, string CustomerPoNumber)
        {
            //验证SKU
            return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.ItemId == ItemId && a.CustomerPoNumber == CustomerPoNumber
                    select a.Id).Count() > 0;
        }
        public string GetSkuAltItemNumber(int ItemId)
        {

            var sql = (from a in idal.IItemMasterDAL.SelectAll()
                       where a.Id == ItemId
                       select a.AltItemNumber);
            if (sql.Count() > 0)
                return sql.First();
            else
                return "";

        }

        /// <summary>
        /// ItemNumber转换为ITEMID,可能是sku,可能是itemId,可能是EAN
        /// </summary>
        /// <param name="ItemNumber"></param>
        /// <returns></returns>
        public string RecItemNumberToId(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber)
        {

            int Item = -1;
            int.TryParse(ItemNumber, out Item);

            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll() on new { A = a.WhCode, B = a.ItemId } equals new { A = b.WhCode, B = b.Id }
                      join c in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals c.Id
                      join d in idal.IInBoundSODAL.SelectAll() on c.SoId equals d.Id into c_join
                      from dd in c_join.DefaultIfEmpty()
                      where a.ReceiptId == ReceiptId && a.WhCode == WhCode && a.CustomerPoNumber == CustomerPoNumber
                      && (b.AltItemNumber == ItemNumber || b.EAN == ItemNumber || b.Id == Item)
                      && (dd.SoNumber == SoNumber || dd.SoNumber == (SoNumber == null ? "" : SoNumber))
                      select new { b.Id, b.AltItemNumber, b.EAN };
            if (SoNumber == null) {
                  sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                          join b in idal.IItemMasterDAL.SelectAll() on new { A = a.WhCode, B = a.ItemId } equals new { A = b.WhCode, B = b.Id }
                          where a.ReceiptId == ReceiptId && a.WhCode == WhCode && a.CustomerPoNumber == CustomerPoNumber
                          && (b.AltItemNumber == ItemNumber || b.EAN == ItemNumber || b.Id == Item)
                          select new { b.Id, b.AltItemNumber, b.EAN };
            }
 

            ////说明ItemNumber是字符
            //if (Item == 0)
            //    sql = sql.Where(u => u.AltItemNumber== ItemNumber || u.EAN == ItemNumber);
            //else
            //    sql = sql.Where(u => u.Id == Item);

            if (sql.ToList().Distinct().Count() == 1)
                return "Y$"+sql.First().Id.ToString();
            else if(sql.Count()>1)
                return "N$" + ItemNumber + "有" + sql.Count() + "个,请扫描收货操作单条码";
            else
                return "N$"+ ItemNumber +"不存在";

        }


        /// <summary>
        /// ItemNumber转换为ITEMID,可能是sku,可能是itemId,可能是EAN
        /// </summary>
        /// <param name="ItemNumber"></param>
        /// <returns></returns>
        public List<int> RecItemNumberToIds(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber)
        {

 
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll() on new { A = a.WhCode, B = a.ItemId } equals new { A = b.WhCode, B = b.Id }
                      join c in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals c.Id
                      join d in idal.IInBoundSODAL.SelectAll() on c.SoId equals d.Id into c_join
                      from dd in c_join.DefaultIfEmpty()
                      where a.ReceiptId == ReceiptId && a.WhCode == WhCode && a.CustomerPoNumber == CustomerPoNumber
                      && (b.AltItemNumber == ItemNumber || b.EAN == ItemNumber )
                      && (dd.SoNumber == SoNumber || dd.SoNumber == (SoNumber == null ? "" : SoNumber))
                      select   b.Id  ;
            if (SoNumber == null)
            {
                sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll() on new { A = a.WhCode, B = a.ItemId } equals new { A = b.WhCode, B = b.Id }
                      where a.ReceiptId == ReceiptId && a.WhCode == WhCode && a.CustomerPoNumber == CustomerPoNumber
                      && (b.AltItemNumber == ItemNumber || b.EAN == ItemNumber)
                      select   b.Id  ;
            }


 
                return sql.ToList();

        }






        #region
        /// <summary>
        /// 检查SKU是否存在收货批次号中
        /// </summary>
        /// <param name="ReceiptId">收货批次号</param>
        /// <param name="WhCode">WhCode</param>
        /// <param name="ItemId">款号ID</param>
        /// <param name="CustomerPoNumber">客户PO</param>
        /// <returns>真或假</returns>
        public bool CheckSku(string ReceiptId, string WhCode, List<int> ItemId, string CustomerPoNumber)
        {
            //验证SKU
            return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.CustomerPoNumber == CustomerPoNumber
                    && (ItemId).Contains(a.ItemId)
                    select a.Id).Count() >= ItemId.Count;
        }
        #endregion

        #region
        /// <summary>
        /// 检查SKU是否存在款号基础表中
        /// </summary>
        /// <param name="WhCode">WhCode</param>
        /// <param name="ItemId">款号ID</param>
        /// <returns></returns>
        public bool CheckSku(string WhCode, List<int> ItemId)
        {
            //验证SKU
            return (from a in idal.IItemMasterDAL.SelectAll()
                    where a.WhCode == WhCode
                    && (ItemId).Contains(a.Id)
                    select a.Id).Count() >= ItemId.Count;
        }
        #endregion



        #region
        /// <summary>
        /// 检查SKU 对应的款号ID 是否正确
        /// </summary>
        /// <param name="WhCode">WhCode</param>
        /// <param name="ItemId">款号ID</param>
        /// <returns></returns>
        public bool CheckSkuId(string WhCode, List<RecModeldetail> RecModeldetail, int ClientId)
        {
            bool result = true;
            foreach (var item in RecModeldetail.Distinct())
            {
                List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == WhCode && u.ClientId == ClientId && u.AltItemNumber == item.AltItemNumber).OrderBy(u => u.Id).ToList();
                if (itemMasterList.Count == 0)
                {
                    result = false;
                    break;
                }
                else
                {
                    int[] id = (from a in itemMasterList select a.Id).ToArray();
                    if (id.Contains(item.ItemId) == false)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
        #endregion


        public bool CheckUnit(string WhCode, int ItemId, string UnitName)
        {
            //验证Unit
            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      where a.WhCode == WhCode && a.Id == ItemId
                      select new RecSkuUnit
                      {
                          UnitFlag = a.UnitFlag,
                          UnitName = a.UnitName,
                          WhCode = a.WhCode,
                          ClientCode = a.ClientCode
                      };
            if (sql.Count() > 0)
            {
                RecSkuUnit recSkuUnit = sql.First();
                if (recSkuUnit.UnitFlag == 1)
                {
                    return (from a in idal.IUnitDAL.SelectAll()
                            where a.ClientCode == recSkuUnit.ClientCode
                                && a.ItemId == ItemId
                                && a.UnitName == UnitName
                                && a.WhCode == recSkuUnit.WhCode
                            select a.Id).Count() > 0;
                }
                else
                {
                    if (recSkuUnit.UnitName == "none")
                    {
                        return true;
                    }
                    else
                    {
                        return UnitName == recSkuUnit.UnitName;
                    }
                }
            }
            else
                return false;
        }


        #region
        /// <summary>
        /// 检查SKU对应的单位是否有误
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="ItemId"></param>
        /// <param name="recModel"></param>
        /// <returns></returns>
        public bool CheckUnit(string WhCode, List<int> ItemId, ReceiptInsert entity)
        {
            //首先得到款号是否有单位为none的情况
            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      where ItemId.Contains(a.Id)
                      select new RecSkuUnit
                      {
                          ItemId = a.Id,
                          UnitFlag = a.UnitFlag,
                          UnitName = a.UnitName
                      };

            var sql1 = from a in idal.IUnitDAL.SelectAll()
                       where ItemId.Contains(a.ItemId)
                       select new RecSkuUnit
                       {
                           ItemId = a.ItemId,
                           UnitId = a.Id,
                           UnitName = a.UnitName
                       };

            //循环已验证过的 款号ID
            foreach (var item in sql)
            {
                if (item.UnitFlag == 0)
                {
                    List<RecModeldetail> listRec = entity.RecModeldetail.Where(u => u.ItemId == item.ItemId).ToList();

                    if (listRec.Count() != 1)
                    {
                        if ((from a in listRec where a.ItemId == item.ItemId select a.UnitName).Distinct().Count() > 1)
                        {
                            return false;
                        }
                    }

                    string unitNameResult = sql.Where(u => u.ItemId == item.ItemId).First().UnitName;
                    if (unitNameResult == "none")
                    {
                        return true;
                    }
                    else
                    {
                        string unitName = listRec.First().UnitName;
                        if ((sql.Where(u => u.ItemId == item.ItemId && u.UnitName == unitName).Distinct().Count() == 0))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //如果款号flag为1 但没有查询到单位
                    if (sql1.Count() == 0)
                    {
                        return false;
                    }

                    List<RecModeldetail> listRec = entity.RecModeldetail.Where(u => u.ItemId == item.ItemId).ToList();

                    foreach (var item1 in listRec)
                    {
                        if (sql1.Where(u => u.ItemId == item.ItemId && u.UnitName == item1.UnitName && u.UnitId == item1.UnitId).Count() == 0)
                        {
                            return false;
                        }
                    }

                }
            }
            return true;
        }

        #endregion

        #region
        /// <summary>
        /// 验证收货门区是否正确
        /// </summary>
        /// <param name="WhCode">WhCode</param>
        /// <param name="Location">预收门区</param>
        /// <returns></returns>
        public bool CheckRecLocation(string WhCode, string Location)
        {

            return (from a in idal.IWhLocationDAL.SelectAll()
                    where a.LocationId == Location && a.WhCode == WhCode
                    join b in idal.ILocationTypeDAL.SelectAll() on a.LocationTypeId equals b.Id
                    where b.TypeName == "S" && a.Status == "A"
                    select a.Id).Count() > 0;
        }
        #endregion

        /// <summary>
        /// 验证退货门区是否正确
        /// </summary>
        /// <param name="WhCode">WhCode</param>
        /// <param name="Location">退货门区</param>
        /// <returns></returns>
        public bool CheckReturnLocation(string WhCode, string Location)
        {

            return (from a in idal.IWhLocationDAL.SelectAll()
                    where a.LocationId == Location && a.WhCode == WhCode
                    join b in idal.ILocationTypeDAL.SelectAll() on a.LocationTypeId equals b.Id
                    where   b.TypeName == "AB" && a.Status == "A"
                    select a.Id).Count() > 0;
        }


        #region

        /// <summary>
        /// 验证收货批次是否完全收货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location"></param>
        /// <returns></returns>
        public bool CheckRecComplete(string WhCode, string ReceiptId)
        {
            return (from a in (
                     (from a0 in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IReceiptDAL.SelectAll()
                           on new { a0.ReceiptId, a0.WhCode, a0.PoId, a0.ItemId, a0.UnitName }
                       equals new { b.ReceiptId, b.WhCode, b.PoId, b.ItemId, b.UnitName } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where
                       a0.ReceiptId == ReceiptId && a0.WhCode == WhCode
                      group new { a0, b } by new
                      {
                          a0.Id,
                          a0.ReceiptId,
                          a0.WhCode,
                          a0.PoId,
                          a0.ItemId,
                          a0.RegQty,
                          a0.UnitName
                      } into g
                      select new
                      {
                          g.Key.Id,
                          g.Key.ReceiptId,
                          g.Key.WhCode,
                          PoId = (Int32?)g.Key.PoId,
                          ItemId = (Int32?)g.Key.ItemId,
                          g.Key.UnitName,
                          RegQty = (Int32?)g.Key.RegQty,
                          qty = g.Sum(p => ((Int32?)p.b.Qty ?? (Int32?)0))
                      }))
                    where a.qty == 0 || a.qty != a.RegQty
                    select a).Count() == 0;
        }
        #endregion



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
        #endregion
    }
}
