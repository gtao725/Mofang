using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;
using System.Transactions;

namespace WMS.BLL
{
    public class InVentoryManager : IInVentoryManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        InventoryHelper inventoryHelper = new InventoryHelper();

        //客户下拉菜单列表
        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {

            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.Status == "Active" && a.WhCode == whCode
                      select a;
            return sql.AsEnumerable();
        }

        //库存查询
        //对应 C_InVentoryController 中的 List 方法
        public List<InVentoryResult> C_InVentoryList(InVentorySearch searchEntity, out int total)
        {
            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IHuDetailDAL.SelectAll()
                      on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { A = b.WhCode, B = b.ItemId } equals new { A = c.WhCode, B = c.Id } into temp1
                      from bc in temp1.DefaultIfEmpty()
                      join d in idal.IWhLocationDAL.SelectAll()
                      on new { A = a.WhCode, B = a.Location } equals new { A = d.WhCode, B = d.LocationId }
                      select new InVentoryResult
                      {
                          WhCode = a.WhCode,
                          ClientCode = b.ClientCode,
                          ClientId = b.ClientId,
                          ReceiptDate = a.ReceiptDate,
                          HuId = a.HuId,
                          Location = a.Location,
                          Type = a.Type,
                          Status = a.Status,
                          HoldId = a.HoldId,
                          HoldReason = a.HoldReason,
                          SoNumber = b.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          Style1 = bc.Style1,
                          Style2 = bc.Style2,
                          Style3 = bc.Style3,
                          Qty = b.Qty,
                          UnitName = b.UnitName,
                          CBM = b.UnitName.Contains("ECH") ? 0 : b.Qty * b.Length * b.Width * b.Height,
                          LotNumber1 = b.LotNumber1 ?? "",
                          LotNumber2 = b.LotNumber2 ?? "",
                          LotDate = b.LotDate

                          //Doi = (TimeSpan)(DateTime.Now - a.ReceiptDate)

                      };

            if (searchEntity.ClientId != 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));

            total = sql.Count();
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //库存信息更改 查询
        public List<InVentoryResult> C_InVentoryQuestionList(InVentorySearch searchEntity, string[] soNumber, string[] poNumber, string[] altNumber, string[] style1, string[] huId, out int total, out string str)
        {
            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IHuDetailDAL.SelectAll()
                      on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId } into temp2
                      from b in temp2.DefaultIfEmpty()
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { A = b.WhCode, B = b.ItemId } equals new { A = c.WhCode, B = c.Id } into temp1
                      from bc in temp1.DefaultIfEmpty()
                      join d in idal.IWhLocationDAL.SelectAll()
                       on new { A = a.Location, B = a.WhCode } equals new { A = d.LocationId, B = d.WhCode } into temp3
                      from d in temp3.DefaultIfEmpty()
                      join e in idal.IWhLocationDAL.SelectAll()
                     on new { A = a.WhCode, B = a.Location } equals new { A = e.WhCode, B = e.LocationId } into temp4
                      from e in temp4.DefaultIfEmpty()
                      join g in idal.ILocationTypeDAL.SelectAll()
                      on e.LocationTypeId equals g.Id
                      join f in idal.IUnitDefaultDAL.SelectAll()
                     on new { A = b.WhCode, B = b.UnitName } equals new { A = f.WhCode, B = f.UnitName } into temp5
                      from f in temp5.DefaultIfEmpty()
                      select new InVentoryResult
                      {
                          HuMasterId = a.Id,
                          HuDetailId = b.Id,
                          ReceiptId = b.ReceiptId,
                          HuId = a.HuId,
                          WhCode = a.WhCode,
                          Location = a.Location,
                          LocationTypeId = d.LocationTypeId,
                          ClientId = b.ClientId,
                          ClientCode = b.ClientCode,
                          ReceiptDate = b.ReceiptDate,
                          SoNumber = b.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          ItemId = b.ItemId,
                          Style1 = bc.Style1 ?? "",
                          Style2 = bc.Style2 ?? "",
                          Style3 = bc.Style3 ?? "",
                          Length = b.Length,
                          Width = b.Width,
                          Height = b.Height,
                          Weight = b.Weight,
                          Qty = b.Qty,
                          PlanQty = b.PlanQty ?? 0,
                          CBM = b.UnitName.Contains("ECH") ? 0 : b.Qty * b.Length * b.Width * b.Height,
                          UnitNameShow = f.UnitNameCN,
                          UnitName = b.UnitName,
                          Type = a.Type,
                          Status = a.Status,
                          TypeShow =
                          a.Type == "M" ? "在库" :
                          a.Type == "R" ? "收货中" :
                          a.Type == "O" ? "出货中" : null,
                          StatusShow =
                          a.Status == "A" ? "正常" :
                          a.Status == "H" ? "冻结" : null,
                          LocationShow =
                          g.Description == "收货门区" ? "收货门区需上架" : g.Description,
                          PickDetailShow = a.Status == "H" ? "不可释放" : g.Description == "收货门区" ? "不可释放" : b.PlanQty > 0 ? "已释放" : a.Type == "O" ? "已释放" : e.LocationTypeId == 3 ? "已释放" : "正常",
                          HoldReason = a.HoldReason,
                          LotNumber1 = b.LotNumber1 ?? "",
                          LotNumber2 = b.LotNumber2 ?? "",
                          LotDate = b.LotDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.Type == searchEntity.Type);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));

            if (soNumber != null)
            {
                soNumber = soNumber.Where(u => u != "").Distinct().ToArray();
                if (soNumber.Count() > 0)
                {
                    sql = sql.Where(u => soNumber.Contains(u.SoNumber));
                }
            }
            if (poNumber != null)
            {
                poNumber = poNumber.Where(u => u != "").Distinct().ToArray();
                if (poNumber.Count() > 0)
                {
                    sql = sql.Where(u => poNumber.Contains(u.CustomerPoNumber));
                }
            }
            if (altNumber != null)
            {
                altNumber = altNumber.Where(u => u != "").Distinct().ToArray();
                if (altNumber.Count() > 0)
                {
                    sql = sql.Where(u => altNumber.Contains(u.AltItemNumber));
                }
            }
            if (style1 != null)
            {
                style1 = style1.Where(u => u != "").Distinct().ToArray();
                if (style1.Count() > 0)
                {
                    sql = sql.Where(u => style1.Contains(u.Style1));
                }
            }
            if (huId != null)
            {
                huId = huId.Where(u => u != "").Distinct().ToArray();
                if (huId.Count() > 0)
                {
                    sql = sql.Where(u => huId.Contains(u.HuId));
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));
            if (!string.IsNullOrEmpty(searchEntity.LotNumber1))
                sql = sql.Where(u => u.LotNumber1 == searchEntity.LotNumber1);
            if (!string.IsNullOrEmpty(searchEntity.LotNumber2))
                sql = sql.Where(u => u.LotNumber2 == searchEntity.LotNumber2);
            if (searchEntity.LotDate != null)
                sql = sql.Where(u => u.LotDate == searchEntity.LotDate);

            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.Location == searchEntity.LocationId);
            if (!string.IsNullOrEmpty(searchEntity.LocationId1))
                sql = sql.Where(u => u.Location.Contains(searchEntity.LocationId1));

            if (searchEntity.HoldReason == "1")
            {
                sql = sql.Where(u => u.Status == "H");
            }
            if (searchEntity.HoldReason == "0")
            {
                sql = sql.Where(u => u.Status == "A");
            }
            if (searchEntity.ClientId != 0 && searchEntity.ClientId != null)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (searchEntity.LocationTypeId != 0)
            {
                sql = sql.Where(u => u.LocationTypeId == searchEntity.LocationTypeId);
            }
            if (searchEntity.BeginReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.BeginReceiptDate);
            }
            if (searchEntity.EndReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.EndReceiptDate);
            }

            List<InVentoryResult> list = new List<InVentoryResult>();
            foreach (var item in sql)
            {
                TimeSpan d3 = DateTime.Now.Subtract(Convert.ToDateTime(item.ReceiptDate == null ? DateTime.Now : item.ReceiptDate));
                item.Doi = d3.Days;
                list.Add(item);
            }

            total = list.Count;
            str = "";
            if (total > 0)
            {
                str = "{\"库存数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql.Sum(u => u.CBM).ToString() + "\"}";
            }

            list = list.OrderBy(u => u.ReceiptDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }



        //修改库存信息
        //对应 C_InVentoryQuestionController 中的 EditDetail 方法
        public int HuMasterEdit(HuDetailResult entity, params string[] modifiedProNames)
        {
            HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode).First();
            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode);
            List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
            foreach (var huDetail in huDetailList)
            {
                //得到原始数据 进行日志添加
                TranLog tranLog = new TranLog();
                tranLog.TranType = "105";
                tranLog.Description = "摆货操作";
                tranLog.TranDate = DateTime.Now;
                tranLog.TranUser = entity.UserName;
                tranLog.WhCode = entity.WhCode;
                tranLog.ClientCode = huDetail.ClientCode;
                tranLog.SoNumber = huDetail.SoNumber;
                tranLog.CustomerPoNumber = huDetail.CustomerPoNumber;
                tranLog.AltItemNumber = huDetail.AltItemNumber;
                tranLog.ItemId = huDetail.ItemId;
                tranLog.UnitID = huDetail.UnitId;
                tranLog.UnitName = huDetail.UnitName;
                tranLog.TranQty = huDetail.Qty;
                tranLog.TranQty2 = huDetail.Qty;
                tranLog.HuId = huDetail.HuId;
                tranLog.Length = huDetail.Length;
                tranLog.Width = huDetail.Width;
                tranLog.Height = huDetail.Height;
                tranLog.Weight = huDetail.Weight;
                tranLog.LotNumber1 = huDetail.LotNumber1;
                tranLog.LotNumber2 = huDetail.LotNumber2;
                tranLog.LotDate = huDetail.LotDate;
                tranLog.ReceiptId = huDetail.ReceiptId;
                tranLog.ReceiptDate = huDetail.ReceiptDate;
                tranLog.Location = huMaster.Location;
                tranLog.Location2 = entity.Location;
                tranLog.HoldId = huMaster.HoldId;
                tranLog.HoldReason = huMaster.HoldReason;
                idal.ITranLogDAL.Add(tranLog);

                //WorkloadAccount work = new WorkloadAccount();
                //work.WhCode = entity.WhCode; 
                //work.ReceiptId = huMaster.ReceiptId;
                //work.ClientId = huDetail.ClientId;
                //work.ClientCode = huDetail.ClientCode;
                //work.HuId = huMaster.HuId;
                //work.WorkType = "叉车工";
                //work.UserCode = entity.UserName;
                //work.LotFlag = 0;
                //work.EchFlag = (huDetail.UnitName == "ECH" ? 1 : 0);
                //work.Qty = (Int32)huDetail.Qty;
                //work.CBM = huDetail.Length * huDetail.Width * huDetail.Height * huDetail.Qty;
                //work.Weight = huDetail.Weight;
                //work.ReceiptDate = DateTime.Now;
                //idal.IWorkloadAccountDAL.Add(work);

                //插入工人工作量
                if (addWorkList.Where(u => u.WhCode == entity.WhCode && u.HuId == huDetail.HuId).Count() == 0)
                {
                    WorkloadAccount work = new WorkloadAccount();
                    work.WhCode = entity.WhCode;
                    work.ReceiptId = huDetail.ReceiptId;
                    work.ClientId = huDetail.ClientId;
                    work.ClientCode = huDetail.ClientCode;
                    work.HuId = huDetail.HuId;
                    work.WorkType = "叉车工";
                    work.UserCode = entity.UserName;
                    work.LotFlag = 0;
                    work.EchFlag = (huDetail.UnitName.Contains("ECH") ? 1 : 0);
                    work.Qty = (Int32)huDetail.Qty;
                    work.CBM = huDetail.Length * huDetail.Width * huDetail.Height * huDetail.Qty;
                    work.Weight = huDetail.Weight;
                    work.ReceiptDate = DateTime.Now;
                    addWorkList.Add(work);
                }
                else
                {
                    WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == entity.WhCode && u.HuId == huDetail.HuId).First();
                    addWorkList.Remove(getModel);

                    WorkloadAccount work = new WorkloadAccount();
                    work.WhCode = entity.WhCode;
                    work.ReceiptId = huDetail.ReceiptId;
                    work.ClientId = huDetail.ClientId;
                    work.ClientCode = huDetail.ClientCode;
                    work.HuId = huDetail.HuId;
                    work.WorkType = "叉车工";
                    work.UserCode = entity.UserName;
                    work.LotFlag = 0;

                    if (getModel.EchFlag == 1)
                    {
                        work.EchFlag = 1;
                    }
                    else
                    {
                        work.EchFlag = (huDetail.UnitName.Contains("ECH") ? 1 : 0);
                    }

                    work.Qty = (Int32)huDetail.Qty + getModel.Qty;
                    work.CBM = (huDetail.Length * huDetail.Width * huDetail.Height * huDetail.Qty) + getModel.CBM;
                    work.Weight = huDetail.Weight + getModel.Weight;
                    work.ReceiptDate = DateTime.Now;
                    addWorkList.Add(work);
                }
            }
            idal.IWorkloadAccountDAL.Add(addWorkList);

            //库位变更时间
            WhLocationEditChangeTime(entity.WhCode, huMaster.Location);

            huMaster.Location = entity.Location;
            huMaster.UpdateUser = entity.UserName;
            huMaster.UpdateDate = DateTime.Now;
            idal.IHuMasterDAL.UpdateBy(huMaster, u => u.HuId == entity.HuId && u.WhCode == entity.WhCode, new string[] { "Location", "UpdateUser", "UpdateDate" });
            idal.IHuMasterDAL.SaveChanges();
            return 1;
        }

        //动态盘点所用-库位变更时间
        public void WhLocationEditChangeTime(string whCode, string locationId)
        {
            WhLocation whLocation = new WhLocation();
            whLocation.ChangeTime = DateTime.Now;
            whLocation.WhCode = whCode;
            whLocation.LocationId = locationId;
            idal.IWhLocationDAL.UpdateBy(whLocation, u => u.WhCode == whCode && u.LocationId == locationId && u.LocationTypeId == 1, new string[] { "ChangeTime" });
            idal.IWhLocationDAL.SaveChanges();
        }

        //修改库存信息
        //对应 C_InVentoryQuestionController 中的 HuMasterHuDetailEdit 方法
        public string HuMasterHuDetailEdit(HuDetailResult entity)
        {
            if (entity.Qty != 0)
            {
                string result = inventoryHelper.CheckLocationId(entity.Location, entity.WhCode);
                if (result != "Y")
                {
                    return result;
                }
            }

            HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.Id == entity.HuMasterId).First();
            HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == entity.Id).First();

            if (entity.Qty == 0)
            {
                if ((huDetail.PlanQty == null ? 0 : huDetail.PlanQty) != 0)
                {
                    return "当前库存已有锁定数量，无法删除！";
                }
            }

            if (entity.Qty != 0)
            {
                if ((huDetail.PlanQty == null ? 0 : huDetail.PlanQty) != 0)
                {
                    if (entity.Qty < huDetail.PlanQty)
                    {
                        return "库存数量必须大于等于已锁定数量！";
                    }
                }
            }

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.UserName;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = huDetail.ClientCode;
            tranLog.SoNumber = huDetail.SoNumber;
            tranLog.CustomerPoNumber = huDetail.CustomerPoNumber;
            tranLog.AltItemNumber = huDetail.AltItemNumber;
            tranLog.ItemId = huDetail.ItemId;
            tranLog.UnitID = huDetail.UnitId;
            tranLog.UnitName = huDetail.UnitName;
            tranLog.TranQty = huDetail.Qty;
            tranLog.TranQty2 = entity.Qty;
            tranLog.HuId = huDetail.HuId;
            tranLog.Length = huDetail.Length;
            tranLog.Width = huDetail.Width;
            tranLog.Height = huDetail.Height;
            tranLog.Weight = huDetail.Weight;
            tranLog.LotNumber1 = huDetail.LotNumber1;
            tranLog.LotNumber2 = huDetail.LotNumber2;
            tranLog.LotDate = huDetail.LotDate;
            tranLog.ReceiptId = huMaster.ReceiptId;
            tranLog.ReceiptDate = huMaster.ReceiptDate;
            tranLog.Location = huMaster.Location;
            tranLog.Location2 = entity.Location;
            tranLog.HoldId = huMaster.HoldId;
            tranLog.HoldReason = huMaster.HoldReason;

            //库位变更时间
            WhLocationEditChangeTime(huMaster.WhCode, huMaster.Location);

            if (entity.Qty != 0)
            {
                if (huMaster.Type == "M")
                {
                    OMSInvChange omsInv = new OMSInvChange();
                    omsInv.TranDate = DateTime.Now;
                    omsInv.TranUser = entity.UserName;
                    omsInv.WhCode = entity.WhCode;
                    omsInv.Status = 0;
                    omsInv.HuId = huDetail.HuId;
                    omsInv.ClientCode = huDetail.ClientCode;
                    omsInv.SoNumber = huDetail.SoNumber;
                    omsInv.CustomerPoNumber = huDetail.CustomerPoNumber;
                    omsInv.AltItemNumber = huDetail.AltItemNumber;
                    omsInv.ItemId = huDetail.ItemId;
                    omsInv.UnitID = huDetail.UnitId;
                    omsInv.UnitName = huDetail.UnitName;
                    omsInv.Qty = huDetail.Qty;
                    omsInv.LotNumber1 = huDetail.LotNumber1 ?? "";
                    omsInv.LotNumber2 = huDetail.LotNumber2 ?? "";
                    omsInv.LotDate = huDetail.LotDate;
                    omsInv.TranType = "150";
                    omsInv.Description = "库存修改";
                    omsInv.ChangeQty = entity.Qty;
                    omsInv.ChangeLotNumber1 = entity.LotNumber1 ?? "";
                    omsInv.ChangeLotNumber2 = entity.LotNumber2 ?? "";
                    omsInv.ChangeLotDate = entity.LotDate;
                    if (omsInv.Qty != omsInv.ChangeQty || omsInv.LotNumber1 != omsInv.ChangeLotNumber1 || omsInv.LotNumber2 != omsInv.ChangeLotNumber2 || omsInv.LotDate != omsInv.ChangeLotDate)
                    {
                        idal.IOMSInvChangeDAL.Add(omsInv);
                    }
                }

                tranLog.TranType = "150";
                tranLog.Description = "库存修改";

                huDetail.Qty = Convert.ToInt32(entity.Qty);
                huDetail.Length = entity.Length;
                huDetail.Width = entity.Width;
                huDetail.Height = entity.Height;
                huDetail.Weight = entity.Weight;
                huDetail.LotNumber1 = entity.LotNumber1;
                huDetail.LotNumber2 = entity.LotNumber2;
                huDetail.LotDate = entity.LotDate;
                huDetail.UpdateUser = entity.UserName;
                huDetail.UpdateDate = DateTime.Now;
                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == entity.Id, new string[] { "Qty", "Length", "Width", "Height", "Weight", "LotNumber1", "LotNumber2", "UpdateUser", "UpdateDate", "LotDate" });

                huMaster.Location = entity.Location;
                huMaster.UpdateUser = entity.UserName;
                huMaster.UpdateDate = DateTime.Now;
                idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == entity.HuMasterId, new string[] { "Location", "UpdateUser", "UpdateDate" });
            }
            else
            {
                OMSInvChange omsInv = new OMSInvChange();
                omsInv.TranType = "450";
                omsInv.Description = "库存删除";
                omsInv.TranDate = DateTime.Now;
                omsInv.TranUser = entity.UserName;
                omsInv.Status = 0;
                omsInv.WhCode = entity.WhCode;
                omsInv.HuId = huDetail.HuId;
                omsInv.ClientCode = huDetail.ClientCode;
                omsInv.SoNumber = huDetail.SoNumber;
                omsInv.CustomerPoNumber = huDetail.CustomerPoNumber;
                omsInv.AltItemNumber = huDetail.AltItemNumber;
                omsInv.ItemId = huDetail.ItemId;
                omsInv.UnitID = huDetail.UnitId;
                omsInv.UnitName = huDetail.UnitName;
                omsInv.Qty = huDetail.Qty;
                omsInv.LotNumber1 = huDetail.LotNumber1 ?? "";
                omsInv.LotNumber2 = huDetail.LotNumber2 ?? "";
                omsInv.LotDate = huDetail.LotDate;
                omsInv.ChangeQty = 0;
                idal.IOMSInvChangeDAL.Add(omsInv);

                tranLog.TranType = "450";
                tranLog.Description = "库存删除";

                //得到托盘明细行数
                List<HuDetail> GetHuDetailCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == huDetail.HuId);
                if (GetHuDetailCount.Count == 1)
                {
                    if (GetHuDetailCount.First().Id == entity.Id)
                    {
                        idal.IHuMasterDAL.Delete(huMaster);
                    }
                }

                idal.IHuDetailDAL.Delete(huDetail);
            }

            idal.ITranLogDAL.Add(tranLog);

            idal.IHuDetailDAL.SaveChanges();

            return "Y";
        }

        //托盘移库
        public string HuIdRemoveLocation(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            RootManager rm = new RootManager();
            RecHelper recHelper = new RecHelper();

            List<WhLocation> checkLocation = idal.IWhLocationDAL.SelectBy(u => u.WhCode == WhCode && (u.LocationId == DestLoc || u.LocationId == Location));
            if (checkLocation.Count != 2)
            {
                return "库位" + DestLoc + "不存在，请更换！";
            }

            //判断托盘上架目标库位是否合法
            if (rm.CheckClientZone(HuId, DestLoc, WhCode) == 0)
                return "非保货物不允许上架到保税库位！";
            if (Location == DestLoc)
                return "目标库位与当前库位相同！";
            //检测是否有该托盘
            if (!recHelper.IfPlt(WhCode, HuId))
                return "系统未检测到此托盘！";
            //起始库位是收货门区,走上架流程
            if (checkLocation.Where(u => u.LocationId == Location).First().LocationTypeId == 2)
            {
                IInventoryWinceManager aa = new InventoryWinceManager();
                return aa.RecStockMove(WhCode, Location, DestLoc, HuId, User);
            }

            //库位变更时间
            WhLocationEditChangeTime(WhCode, Location);

            TranLog tranLog = new TranLog();
            tranLog.TranType = "108";
            tranLog.Description = "移库网页版";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = User;
            tranLog.WhCode = WhCode;
            tranLog.HuId = HuId;
            tranLog.Location = Location;
            tranLog.Location2 = DestLoc;
            idal.ITranLogDAL.Add(tranLog);

            HuMaster huMaster = new HuMaster();
            huMaster.Location = DestLoc;
            huMaster.UpdateUser = User;
            huMaster.UpdateDate = DateTime.Now;
            idal.IHuMasterDAL.UpdateBy(huMaster, u => u.WhCode == WhCode && u.HuId == HuId, new string[] { "Location", "UpdateUser", "UpdateDate" });
            idal.IHuMasterDAL.SaveChanges();

            return "Y";
        }



        //新增库存
        //对应 C_AddInventoryController 中的 AddInventory 方法
        public string AddInventory(HuDetailInsert entity)
        {
            if (entity.HuId == null || entity.WhCode == null || entity.Qty == null)
            {
                return "数据有误，请重新操作！";
            }

            List<Pallate> checkPallateList = idal.IPallateDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (checkPallateList.Count == 0)
            {
                Pallate p = new Pallate(); ;
                p.WhCode = entity.WhCode;
                p.HuId = entity.HuId;
                p.TypeId = 1;
                p.Status = "U";
                p.CreateUser = entity.UserName;
                p.CreateDate = DateTime.Now;
                idal.IPallateDAL.Add(p);
                idal.IPallateDAL.SaveChanges();
            }

            if (!inventoryHelper.CheckPlt(entity.WhCode, entity.HuId))
            {
                return "错误！托盘已使用！";
            }
            string result = inventoryHelper.CheckLocationId(entity.Location, entity.WhCode);
            if (result != "Y")
            {
                return result;
            }

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == entity.AltItemNumber && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();
            ItemMaster itemMaster = new ItemMaster();
            if (listItemMaster.Count == 0)
            {
                return "错误！款号不存在或有误！";
            }
            else
            {
                itemMaster = listItemMaster.First();
            }

            List<HuMaster> HuMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (HuMasterList.Count == 0)
            {
                HuMaster huMaster = new HuMaster();
                huMaster.WhCode = entity.WhCode;
                huMaster.HuId = entity.HuId;
                huMaster.Type = "M";
                huMaster.Status = "A";
                huMaster.Location = entity.Location;
                huMaster.ReceiptId = entity.ReceiptId;
                huMaster.ReceiptDate = DateTime.Now;
                huMaster.CreateUser = entity.UserName;
                huMaster.CreateDate = DateTime.Now;
                idal.IHuMasterDAL.Add(huMaster);

                //库位变更时间
                WhLocationEditChangeTime(huMaster.WhCode, huMaster.Location);
            }
            else
            {
                HuMaster huMasterFirst = HuMasterList.First();
                //库位变更时间
                WhLocationEditChangeTime(huMasterFirst.WhCode, huMasterFirst.Location);
            }

            HuDetail huDetail = new HuDetail();
            huDetail.WhCode = entity.WhCode;
            huDetail.HuId = entity.HuId;
            huDetail.ClientId = entity.ClientId;
            huDetail.ClientCode = entity.ClientCode;
            huDetail.SoNumber = entity.SoNumber;
            huDetail.CustomerPoNumber = entity.CustomerPoNumber;
            huDetail.AltItemNumber = itemMaster.AltItemNumber;
            huDetail.ItemId = itemMaster.Id;
            huDetail.UnitId = entity.UnitId;
            huDetail.UnitName = entity.UnitName;
            huDetail.ReceiptId = entity.ReceiptId;
            huDetail.Qty = Convert.ToInt32(entity.Qty);
            huDetail.ReceiptDate = DateTime.Now;
            huDetail.Length = entity.Length / 100;
            huDetail.Width = entity.Width / 100;
            huDetail.Height = entity.Height / 100;
            huDetail.Weight = entity.Weight;
            huDetail.LotNumber1 = entity.LotNumber1;
            huDetail.LotNumber2 = entity.LotNumber2;
            huDetail.LotDate = entity.LotDate;
            huDetail.CreateUser = entity.UserName;
            huDetail.CreateDate = DateTime.Now;
            idal.IHuDetailDAL.Add(huDetail);

            TranLog tranLog = new TranLog();
            tranLog.TranType = "160";
            tranLog.Description = "新增库存";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.UserName;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = entity.ClientCode;
            tranLog.SoNumber = entity.SoNumber;
            tranLog.CustomerPoNumber = entity.CustomerPoNumber;
            tranLog.AltItemNumber = entity.AltItemNumber;
            tranLog.ItemId = itemMaster.Id;
            tranLog.UnitID = entity.UnitId;
            tranLog.UnitName = entity.UnitName;
            tranLog.TranQty2 = entity.Qty;
            tranLog.HuId = entity.HuId;
            tranLog.Length = entity.Length / 100;
            tranLog.Width = entity.Width / 100;
            tranLog.Height = entity.Height / 100;
            tranLog.Weight = entity.Weight;
            tranLog.LotNumber1 = entity.LotNumber1;
            tranLog.LotNumber2 = entity.LotNumber2;
            tranLog.LotDate = entity.LotDate;
            tranLog.ReceiptId = entity.ReceiptId;
            tranLog.ReceiptDate = DateTime.Now;
            tranLog.Location = entity.Location;
            idal.ITranLogDAL.Add(tranLog);

            OMSInvChange omsInv = new OMSInvChange();
            omsInv.TranType = "160";
            omsInv.Description = "新增库存";
            omsInv.TranDate = DateTime.Now;
            omsInv.TranUser = entity.UserName;
            omsInv.Status = 0;
            omsInv.WhCode = entity.WhCode;
            omsInv.HuId = entity.HuId;
            omsInv.ClientCode = entity.ClientCode;
            omsInv.SoNumber = entity.SoNumber;
            omsInv.CustomerPoNumber = entity.CustomerPoNumber;
            omsInv.AltItemNumber = entity.AltItemNumber;
            omsInv.ItemId = itemMaster.Id;
            omsInv.UnitID = entity.UnitId;
            omsInv.UnitName = entity.UnitName;
            omsInv.Qty = entity.Qty;
            omsInv.LotNumber1 = entity.LotNumber1;
            omsInv.LotNumber2 = entity.LotNumber2;
            omsInv.LotDate = entity.LotDate;
            idal.IOMSInvChangeDAL.Add(omsInv);

            idal.IHuDetailDAL.SaveChanges();

            return "Y";
        }

        //托盘解冻
        public string PltHoldEdit(HuMaster entity)
        {
            return inventoryHelper.PltHoldEdit(entity);
        }

        public string InventoryTCR(HuMaster entity)
        {
            //插入TCR记录


            List<HuDetail> HuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (HuDetailList.Count > 0)
            {
                //如果有TCR 批量插入
                List<PhotoMaster> photoList = new List<PhotoMaster>();
                foreach (var item in HuDetailList)
                {
                    PhotoMaster photoMaster = new PhotoMaster();

                    List<PhotoMaster> checkPhotoMaster = idal.IPhotoMasterDAL.SelectBy(u => u.WhCode == item.WhCode && u.Number == item.ReceiptId && u.Number2 == item.SoNumber && (u.PhotoId ?? 0) != 0);
                    if (checkPhotoMaster.Count > 0)
                    {
                        PhotoMaster getfirst = checkPhotoMaster.First();
                        photoMaster.PhotoId = getfirst.PhotoId;
                    }

                    photoMaster.WhCode = item.WhCode;
                    photoMaster.ClientCode = item.ClientCode;
                    photoMaster.Number = item.ReceiptId;
                    photoMaster.Number2 = item.SoNumber;
                    photoMaster.Number3 = item.CustomerPoNumber;
                    photoMaster.Number4 = item.AltItemNumber;

                    photoMaster.ItemId = item.ItemId;
                    photoMaster.UnitName = item.UnitName;
                    photoMaster.Qty = item.Qty;
                    photoMaster.RegQty = 0;
                    photoMaster.HuId = item.HuId;
                    photoMaster.HoldReason = entity.HoldReason;
                    photoMaster.TCRStatus = "未处理";
                    photoMaster.TCRProcessMode = "";

                    photoMaster.SettlementMode = "";
                    photoMaster.SumPrice = 0;
                    photoMaster.DeliveryDate = item.ReceiptDate;
                    photoMaster.Type = "in";

                    photoMaster.Status = 0;

                    photoMaster.CheckStatus1 = "N";
                    photoMaster.CheckStatus2 = "N";
                    photoMaster.CreateUser = item.CreateUser;
                    photoMaster.CreateDate = DateTime.Now;
                    photoList.Add(photoMaster);
                }

                idal.IPhotoMasterDAL.Add(photoList);
                idal.IPhotoMasterDAL.SaveChanges();
            }

            return "Y";
        }


        //托盘移库修改
        public bool HuMasterEdit(HuDetailResult entity)
        {
            HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode).First();
            HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode).First();

            //库位变更时间
            WhLocationEditChangeTime(huMaster.WhCode, huMaster.Location);

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "109";
            tranLog.Description = "移库操作";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.UserName;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = huDetail.ClientCode;
            tranLog.SoNumber = huDetail.SoNumber;
            tranLog.CustomerPoNumber = huDetail.CustomerPoNumber;
            tranLog.AltItemNumber = huDetail.AltItemNumber;
            tranLog.ItemId = huDetail.ItemId;
            tranLog.UnitID = huDetail.UnitId;
            tranLog.UnitName = huDetail.UnitName;
            tranLog.TranQty = huDetail.Qty;
            tranLog.TranQty2 = huDetail.Qty;
            tranLog.HuId = huDetail.HuId;
            tranLog.Length = huDetail.Length;
            tranLog.Width = huDetail.Width;
            tranLog.Height = huDetail.Height;
            tranLog.Weight = huDetail.Weight;
            tranLog.LotNumber1 = huDetail.LotNumber1;
            tranLog.LotNumber2 = huDetail.LotNumber2;
            tranLog.LotDate = huDetail.LotDate;
            tranLog.ReceiptId = huMaster.ReceiptId;
            tranLog.ReceiptDate = huMaster.ReceiptDate;
            tranLog.Location = huMaster.Location;
            tranLog.Location2 = entity.Location;
            tranLog.HoldId = huMaster.HoldId;
            tranLog.HoldReason = huMaster.HoldReason;
            idal.ITranLogDAL.Add(tranLog);

            huMaster.Location = entity.Location;
            huMaster.UpdateUser = entity.UserName;
            huMaster.UpdateDate = DateTime.Now;
            idal.IHuMasterDAL.UpdateBy(huMaster, u => u.HuId == entity.HuId && u.WhCode == entity.WhCode, new string[] { "Location", "UpdateUser", "UpdateDate" });
            idal.IHuMasterDAL.SaveChanges();
            return true;
        }


        //修改SN
        public string EditInventoryExtendHuId(int huDetailId, int huMasterId, string huId)
        {
            List<HuDetail> huDetaiList = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailId);
            List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.Id == huMasterId);

            if (huDetaiList.Count == 0 || huMasterList.Count == 0)
            {
                return "修改SN信息异常，请重新查询后再操作！";
            }

            HuDetail huDetail = huDetaiList.First();
            List<Receipt> receiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == huDetail.WhCode && u.HuId == huDetail.HuId && u.SoNumber == huDetail.SoNumber && u.AltItemNumber == huDetail.AltItemNumber && u.Qty == huDetail.Qty && u.ClientCode == huDetail.ClientCode);

            List<Pallate> pallateList = idal.IPallateDAL.SelectBy(u => u.WhCode == huDetail.WhCode && u.HuId == huId);
            if (pallateList.Count == 0)
            {
                Pallate pa = new Pallate();
                pa.WhCode = huDetail.WhCode;
                pa.HuId = huId;
                pa.TypeId = 1;
                pa.Status = "U";
                pa.CreateDate = DateTime.Now;
                idal.IPallateDAL.Add(pa);
                idal.IPallateDAL.SaveChanges();
            }

            idal.ISerialNumberInOutExtendDAL.UpdateByExtended(u => u.WhCode == huDetail.WhCode && u.ClientCode == huDetail.ClientCode && u.SoNumber == huDetail.SoNumber && u.AltItemNumber == huDetail.AltItemNumber && u.CartonId == huDetail.HuId, t => new SerialNumberInOutExtend { CartonId = huId });

            if (receiptList.Count > 0)
            {
                foreach (var item in receiptList)
                {
                    idal.IReceiptDAL.UpdateByExtended(u => u.Id == item.Id, t => new Receipt { HuId = huId });
                }
            }

            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == huDetailId, t => new HuDetail { HuId = huId });
            idal.IHuMasterDAL.UpdateByExtended(u => u.Id == huMasterId, t => new HuMaster { HuId = huId });

            return "Y";
        }
        //创建库存调整单
        public string CreateInvMove(string WhCode, string User)
        {
            //库存,算所有的正常库存
            var sqlOnHand = from a in (
                                           (from a in idal.IHuMasterDAL.SelectAll()
                                            join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                                            join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                                            join e in idal.IWhClientExtendDAL.SelectAll() on new { A = (Int32)b.ClientId, B = b.WhCode } equals new { A = e.ClientId, B = e.WhCode }
                                            //OnlySkuFlag=1混合区库存,LocationTypeId=1 正常库位,InvClearUpSkuMaxQty数量有且大于0
                                            where a.WhCode == WhCode && c.ZoneId != null && c.LocationTypeId == 1 && e.InvClearUpSkuMaxQty > 0
                                            select new { b.ClientCode, e.InvClearUpSkuMaxQty, b.ItemId, b.Qty })
                                 )
                            group a by new { a.ItemId, a.InvClearUpSkuMaxQty } into g
                            //库存数大于库存整理的警戒值
                            where g.Sum(u => u.Qty) > g.Max(u => u.InvClearUpSkuMaxQty)
                            select new ZoneNameSearch { ItemId = g.Key.ItemId, InvClearUpSkuMaxQty = g.Key.InvClearUpSkuMaxQty, SumQty = (g.Sum(u => u.Qty) - g.Key.InvClearUpSkuMaxQty) };




            ////库存,只算正常库存
            //var sqlOnHandAll1 = (from a in (
            //                               (from a in idal.IHuMasterDAL.SelectAll()
            //                                join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
            //                                join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
            //                                join d in idal.IZonesExtendDAL.SelectAll() on c.ZoneId equals d.ZoneId
            //                                join e in idal.IWhClientExtendDAL.SelectAll() on new { A = (Int32)b.ClientId, B = b.WhCode } equals new { A = e.ClientId, B = e.WhCode }
            //                                // LocationTypeId=1 正常库位,InvClearUpSkuMaxQty数量有且大于0
            //                                where a.WhCode == WhCode && c.ZoneId != null  && c.LocationTypeId == 1 && e.InvClearUpSkuMaxQty > 0
            //                                  &&sqlOnHand.ToList().Contains(b.ItemId)
            //                                select new { c.ZoneId, d.MaxPallateQty, b.ClientCode, e.InvClearUpSkuMaxQty, b.ItemId, b.Qty })
            //                     )
            //                     group a by new { a.ZoneId, a.MaxPallateQty, a.ItemId, a.ClientCode, a.InvClearUpSkuMaxQty } into g
            //                     //库存数大于库存整理的警戒值
            //                   //  where g.Sum(u => u.Qty) >= g.Max(u => u.InvClearUpSkuMaxQty)
            //                     select new ZoneNameSearch { ZoneId = g.Key.ZoneId.ToString(), MaxPallateQty = g.Key.MaxPallateQty, ItemId = g.Key.ItemId, InvClearUpSkuMaxQty = g.Key.InvClearUpSkuMaxQty, SumQty = (g.Sum(u => u.Qty)) });



            var sqlOnHandAll = sqlOnHand.OrderBy(u => u.SumQty).ToList();

            if (sqlOnHandAll.Count() > 0)
            {

                List<int> zoneArry = new List<int> { 0 };
                string moveNum = "M" + DI.IDGenerator.NewId;

                List<InvMoveDetail> detailAdd = new List<InvMoveDetail>();
                int InvClearUpSkuMaxQty = (Int32)sqlOnHandAll.First().InvClearUpSkuMaxQty;


                InventoryWinceManager invm = new InventoryWinceManager();
                foreach (var zs in sqlOnHandAll)
                {


                    //var aa= from a in idal.IHuMasterDAL.SelectAll()
                    //        join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                    //        join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                    //        join d in idal.IZonesExtendDAL.SelectAll() on c.ZoneId equals d.ZoneId
                    //        join e in idal.IZoneDAL.SelectAll() on c.ZoneId equals e.Id
                    //        //OnlySkuFlag=1混合区库存,LocationTypeId=1 正常库位
                    //        where a.WhCode == WhCode && c.ZoneId != null && d.OnlySkuFlag == 1 && c.LocationTypeId == 1 && b.ItemId == zs.ItemId
                    //        select new InvMoveDetailAdd { MoveNum = moveNum, Location = a.Location, WhCode = a.WhCode, ClientCode = b.ClientCode, SoNumber = b.SoNumber, CustomerPoNumber = b.CustomerPoNumber, AltItemNumber = b.AltItemNumber, ItemId = b.ItemId, HuId = b.HuId, ZoneId = (Int32)c.ZoneId, ZoneName = e.ZoneName, DesZoneId = 0, DesZoneName = "", CreateDate = DateTime.Now, CreateUser = User, UpZoneId = e.UpId };


                    List<InvMoveDetailAdd> onHandDetail = (from a in idal.IHuMasterDAL.SelectAll()
                                                           join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                                                           join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                                                           join d in idal.IZonesExtendDAL.SelectAll() on c.ZoneId equals d.ZoneId
                                                           join e in idal.IZoneDAL.SelectAll() on c.ZoneId equals e.Id
                                                           //OnlySkuFlag=1混合区库存,LocationTypeId=1 正常库位
                                                           where a.WhCode == WhCode && c.ZoneId != null && d.OnlySkuFlag == 1 && c.LocationTypeId == 1 && b.ItemId == zs.ItemId
                                                           select new InvMoveDetailAdd { MoveNum = moveNum, Location = a.Location, WhCode = a.WhCode, ClientCode = b.ClientCode, SoNumber = b.SoNumber, CustomerPoNumber = b.CustomerPoNumber, AltItemNumber = b.AltItemNumber, ItemId = b.ItemId, HuId = b.HuId, ZoneId = (Int32)c.ZoneId, ZoneName = e.ZoneName, DesZoneId = 0, DesZoneName = "", CreateDate = DateTime.Now, CreateUser = User, UpZoneId = e.UpId }).ToList();


                    if (onHandDetail.Count > 0)
                    {
                        //需要循环的次数
                        int forCount = onHandDetail.Count / InvClearUpSkuMaxQty;
                        int modCount = onHandDetail.Count % InvClearUpSkuMaxQty;
                        //如果余数大于0,循环次数+1
                        if (modCount > 0)
                            forCount = forCount + 1;

                        //需要更新的行数
                        int doCount = 0;

                        //当前zone剩余可分配的数量
                        int zoneMaxPallateQtyRemain = 0;

                        int oldZoneId = 0;


                        for (int i = 0; i < forCount; i++)
                        {
                            //混合区库位的上级Id,UpZoneId
                            int ZoneId = onHandDetail.First().UpZoneId;
                            string HuId = onHandDetail.First().HuId;


                            //剩余需要分配的库存数量
                            int RemainingOnHandQty = onHandDetail.Count;


                            ZoneNameSearch zn = invm.EasternChinaRecStockNotDoZone(ZoneId, HuId, WhCode, zoneArry);

                            //这个数量已经减去建议的这次数量了
                            int OnHandQty = Convert.ToInt32(zn.OnHandQty);


                            ////建议到已有库存的库位RemainingQty!=0,此次执行的行数为剩余库位数
                            if (OnHandQty != 0)
                            {
                                //如果需要分配的库存数量(RemainingOnHandQty)与库位剩余数 MaxPallateQty-OnHandQty,谁小取谁
                                if (RemainingOnHandQty > Convert.ToInt32(zn.MaxPallateQty) - OnHandQty)
                                    doCount = Convert.ToInt32(zn.MaxPallateQty) - OnHandQty;
                                else
                                    doCount = RemainingOnHandQty;

                            }
                            else
                            {
                                //如果需要分配的库存数量(RemainingOnHandQty)与库位剩余数 MaxPallateQty,谁小取谁
                                if (RemainingOnHandQty > Convert.ToInt32(zn.MaxPallateQty) && Convert.ToInt32(zn.MaxPallateQty) > 0)
                                    doCount = Convert.ToInt32(zn.MaxPallateQty);
                                else
                                    doCount = RemainingOnHandQty;
                            }


                            for (int j = 0; j < doCount; j++)
                            {
                                onHandDetail[j].DesZoneId = Convert.ToInt32(zn.ZoneId);
                                //把异常结果写进DesZoneName
                                if (zn.ZoneId != 0 && zn.ZoneId != null)
                                {
                                    onHandDetail[j].DesZoneName = zn.ZoneName;
                                }
                                else
                                {
                                    onHandDetail[j].DesZoneName = zn.res;
                                }
                                //添加到实体中
                                detailAdd.Add(SetInvMoveDetailValue(onHandDetail[j]));

                            }
                            onHandDetail = onHandDetail.Where(u => u.DesZoneName == "").ToList();

                            //循环到最后一轮了,如果未分配数量还有forCount+1,如果没有了,不forCount=i,退出循环
                            if (i >= forCount - 1 && onHandDetail.Count > 0)
                                forCount = forCount + 1;
                            else if (onHandDetail.Count == 0)
                            {

                                forCount = i;
                            }


                            // int MaxPallateQty = Convert.ToInt32(zn.MaxPallateQty);
                            //第一次oldZoneId=0或者换了ZoneId 更新为建议的zn.ZoneId
                            if (oldZoneId == 0 || oldZoneId != Convert.ToInt32(zn.ZoneId))
                            {
                                oldZoneId = Convert.ToInt32(zn.ZoneId);
                                zoneMaxPallateQtyRemain = Convert.ToInt32(zn.MaxPallateQty) - OnHandQty;
                            }

                            ////oldZoneId与新的zn.ZoneId不同,zone剩余可分配数zoneMaxPallateQtyRemain重置为最大托盘数 zn.MaxPallateQty
                            //if (oldZoneId != Convert.ToInt32(zn.ZoneId) && Convert.ToInt32(zn.ZoneId) != 0) {

                            //    zoneMaxPallateQtyRemain = Convert.ToInt32(zn.MaxPallateQty);
                            //}

                            //zone剩余可分配数,等于上次剩余的数减去分配数
                            zoneMaxPallateQtyRemain = zoneMaxPallateQtyRemain - doCount;

                            //zoneMaxPallateQtyRemain 没有可用数量或者说换ZoneId,都添加到不再分配的名单中
                            if (zoneMaxPallateQtyRemain == 0 || onHandDetail.Count == 0)
                            {
                                zoneArry.Add(Convert.ToInt32(zn.ZoneId));
                            }
                        }

                    }

                }
                if (detailAdd.Count > 0)
                {
                    InvMove im = new InvMove { };
                    im.MoveNum = moveNum;
                    im.WhCode = WhCode;
                    im.CreateDate = DateTime.Now;
                    im.CreateUser = User;
                    idal.IInvMoveDAL.Add(im);
                    idal.IInvMoveDAL.SaveChanges();

                    idal.IInvMoveDetailDAL.Add(detailAdd);
                    idal.IInvMoveDetailDAL.SaveChanges();
                    return "Y$" + moveNum;
                }
                else
                    return "N$无混合区库存需要移库";
            }
            else
                return "N$无混合区库存需要移库";
        }


        public InvMoveDetail SetInvMoveDetailValue(InvMoveDetailAdd OldObj)
        {
            InvMoveDetail NewObj = new InvMoveDetail();
            NewObj.WhCode = OldObj.WhCode;
            NewObj.HuId = OldObj.HuId;
            NewObj.ClientCode = OldObj.ClientCode;
            NewObj.ItemId = OldObj.ItemId;
            NewObj.Location = OldObj.Location;
            NewObj.MoveNum = OldObj.MoveNum;
            NewObj.SoNumber = OldObj.SoNumber;
            NewObj.CustomerPoNumber = OldObj.CustomerPoNumber;
            NewObj.ZoneId = OldObj.ZoneId;
            NewObj.ZoneName = OldObj.ZoneName;
            NewObj.DesZoneId = OldObj.DesZoneId;
            NewObj.DesZoneName = OldObj.DesZoneName;
            NewObj.AltItemNumber = OldObj.AltItemNumber;
            NewObj.CreateUser = OldObj.CreateUser;
            NewObj.CreateDate = OldObj.CreateDate;
            return NewObj;
        }

        //库存整理单查询
        public List<InvMoveDetailResult> ListInvMove(InvMoveDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.IInvMoveDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new InvMoveDetailResult
                      {
                          WhCode = a.WhCode,
                          MoveNum = a.MoveNum,
                          MoveNumEdit = a.MoveNum,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (searchEntity.DetailFlag == 1)
            {
                sql = from a in idal.IInvMoveDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new InvMoveDetailResult
                      {
                          WhCode = a.WhCode,
                          MoveNum = a.MoveNum,
                          MoveNumEdit = a.MoveNum,
                          ClientCode = a.ClientCode,
                          CustomerPoNumber = a.CustomerPoNumber,
                          SoNumber = a.SoNumber,
                          AltItemNumber = a.AltItemNumber,
                          HuId = a.HuId,
                          Location = a.Location,
                          ZoneId = a.ZoneId,
                          ZoneName = a.ZoneName,
                          DesZoneId = a.DesZoneId,
                          DesZoneName = a.DesZoneName,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate

                      };
                if (!string.IsNullOrEmpty(searchEntity.MoveNum))
                    sql = sql.Where(u => u.MoveNum.Contains(searchEntity.MoveNum));
                if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                    sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
                if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                    sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));
                if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                    sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));
                if (!string.IsNullOrEmpty(searchEntity.HuId))
                    sql = sql.Where(u => u.HuId.Contains(searchEntity.HuId));
                if (searchEntity.BeginDate != null)
                    sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
                if (searchEntity.EndDate != null)
                    sql = sql.Where(u => u.CreateDate <= searchEntity.EndDate);
                sql = sql.OrderByDescending(u => u.MoveNum).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            }
            else
            {

                if (!string.IsNullOrEmpty(searchEntity.MoveNum))
                    sql = sql.Where(u => u.MoveNum == searchEntity.SoNumber);
                if (searchEntity.BeginDate != null)
                    sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
                if (searchEntity.EndDate != null)
                    sql = sql.Where(u => u.CreateDate <= searchEntity.EndDate);
                sql = sql.OrderByDescending(u => u.MoveNum).ThenBy(u => u.CreateDate);

            }


            total = sql.Count();
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //调整库存Lot
        public string EditHuIdLot(EditHuDetailLotEntity entity, string locationId, int qty, string lot1, string lot2, string lotdate)
        {
            List<HuDetail> checkHuDetaiList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId && u.ClientCode == entity.ClientCode && u.AltItemNumber == entity.AltItemNumber && (u.LotNumber1 == entity.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (entity.LotNumber1 == null ? "" : entity.LotNumber1)) && (u.LotNumber2 == entity.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (entity.LotNumber2 == null ? "" : entity.LotNumber2)) && u.LotDate == entity.LotDate && (u.PlanQty ?? 0) == 0);
            if (checkHuDetaiList.Count == 0)
            {
                return "未找到托盘库存明细！";
            }

            List<HuMaster> checkHuMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (checkHuMasterList.Count == 0)
            {
                return "未找到托盘库存明细！";
            }

            HuMaster firstHuMaster = checkHuMasterList.First();
            if (firstHuMaster.Type != "M")
            {
                return "库存托盘需为正常状态！";
            }
            else if (firstHuMaster.Status != "A")
            {
                return "库存托盘需为正常状态！";
            }

            int sumQty = checkHuDetaiList.Sum(u => u.Qty);
            if (sumQty < qty)
            {
                return "托盘库存数小于修改数量！";
            }

            int qtyResult = qty;

            List<HuDetail> huDetailList = new List<HuDetail>();
            foreach (var item in checkHuDetaiList)
            {
                int invQty = item.Qty;
                if (huDetailList.Where(u => u.Id == item.Id).Count() > 0)
                {
                    List<HuDetail> gethuDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id);
                    if (gethuDetailList.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        invQty = gethuDetailList.First().Qty;
                    }
                }

                if (qtyResult == invQty)
                {
                    List<int> idList = new List<int>();
                    idList.Add(item.Id);
                    BatchEditHuIdLot(idList.ToArray(), item.WhCode, entity.HuId, qtyResult, lot1, lot2, lotdate, entity.UserName);
                    break;
                }
                else if (qtyResult < invQty)
                {
                    List<int> idList = new List<int>();
                    idList.Add(item.Id);
                    BatchEditHuIdLot(idList.ToArray(), item.WhCode, entity.HuId, qtyResult, lot1, lot2, lotdate, entity.UserName);
                    break;
                }
                else if (qtyResult > invQty)
                {
                    List<int> idList = new List<int>();
                    idList.Add(item.Id);
                    BatchEditHuIdLot(idList.ToArray(), item.WhCode, entity.HuId, invQty, lot1, lot2, lotdate, entity.UserName);

                    huDetailList.Add(item);
                    qtyResult = qtyResult - invQty;
                }

            }

            return "Y";
        }


        //调整库存Lot
        public List<EditHuDetailLotEntity> EditHuIdLot(List<EditHuDetailLotEntity> entityList)
        {
            //返回结果列表
            List<EditHuDetailLotEntity> resultList = new List<EditHuDetailLotEntity>();

            //库存修改列表
            List<HuDetail> editHuDetailList = new List<HuDetail>();

            foreach (var item in entityList)
            {
                editHuDetailList.Clear();

                List<HuDetail> list = (from a in idal.IHuMasterDAL.SelectAll()
                                       join b in idal.IHuDetailDAL.SelectAll()
                                       on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                                       join c in idal.IItemMasterDAL.SelectAll()
                                       on new { A = b.WhCode, B = b.ItemId } equals new { A = c.WhCode, B = c.Id } into temp1
                                       from bc in temp1.DefaultIfEmpty()
                                       join d in idal.IWhLocationDAL.SelectAll()
                                       on new { A = a.WhCode, B = a.Location } equals new { A = d.WhCode, B = d.LocationId }
                                       where a.WhCode == item.WhCode && b.ClientCode == item.ClientCode && b.AltItemNumber == item.AltItemNumber && (b.LotNumber1 == item.LotNumber1 || (b.LotNumber1 == null ? "" : b.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (b.LotNumber2 == item.LotNumber2 || (b.LotNumber2 == null ? "" : b.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && (b.Qty - (b.PlanQty ?? 0)) > 0
                                       && a.Type == "M" && a.Status == "A" && d.LocationTypeId == 1
                                       select b).OrderBy(u => u.ReceiptDate).ToList();

                //如果库存没有符合条件的 修改数量返回0
                if (list.Count == 0)
                {
                    EditHuDetailLotEntity result = new EditHuDetailLotEntity();
                    result.WhCode = item.WhCode;
                    result.ClientCode = item.ClientCode;
                    result.ClientSystemNumber = item.ClientSystemNumber;
                    result.AltItemNumber = item.AltItemNumber;
                    result.LotNumber1 = item.LotNumber1;
                    result.LotNumber2 = item.LotNumber2;
                    result.LotDate = item.LotDate;
                    result.NewLotDate = item.NewLotDate;
                    result.NewLotNumber1 = item.NewLotNumber1;
                    result.NewLotNumber2 = item.NewLotNumber2;
                    result.UserName = item.UserName;
                    result.Qty = 0;
                    resultList.Add(result);

                    TranLog tranLog = new TranLog();
                    tranLog.TranType = "860";
                    tranLog.Description = "EDI库存调整Lot款号汇总记录";
                    tranLog.TranDate = DateTime.Now;
                    tranLog.TranUser = item.UserName;
                    tranLog.WhCode = item.WhCode;
                    tranLog.ClientCode = item.ClientCode;
                    tranLog.SoNumber = "";
                    tranLog.CustomerPoNumber = "";
                    tranLog.AltItemNumber = item.AltItemNumber;
                    tranLog.ItemId = 0;
                    tranLog.UnitID = 0;
                    tranLog.UnitName = "";
                    tranLog.TranQty = item.Qty;
                    tranLog.TranQty2 = 0;
                    tranLog.HuId = "";
                    tranLog.Length = 0;
                    tranLog.Width = 0;
                    tranLog.Height = 0;
                    tranLog.Weight = 0;
                    tranLog.LotNumber1 = item.LotNumber1;
                    tranLog.LotNumber2 = item.NewLotNumber1;
                    tranLog.LotDate = null;
                    tranLog.ReceiptId = "";
                    tranLog.ReceiptDate = null;
                    tranLog.Location = "";
                    tranLog.Location2 = "";
                    tranLog.HoldId = 0;
                    tranLog.HoldReason = "";
                    tranLog.CustomerOutPoNumber = item.ClientSystemNumber;
                    tranLog.Remark = item.Qty + "件原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "未匹配到正常库存调整失败";
                    idal.ITranLogDAL.Add(tranLog);
                    idal.ITranLogDAL.SaveChanges();
                }
                else
                {
                    int qtyResult = (Int32)item.Qty;
                    foreach (var sqlDetail in list)
                    {
                        if (qtyResult <= 0)
                        {
                            break;
                        }
                        else
                        {
                            int? sqlDetailQty = sqlDetail.Qty - (sqlDetail.PlanQty ?? 0);

                            if (editHuDetailList.Where(u => u.Id == sqlDetail.Id).Count() > 0)
                            {
                                int getQty = editHuDetailList.Where(u => u.Id == sqlDetail.Id).Sum(u => u.Qty);
                                sqlDetailQty = sqlDetail.Qty - getQty;
                            }
                            if (sqlDetailQty <= 0)
                            {
                                continue;
                            }

                            HuDetail detail = sqlDetail;
                            detail.LotNumber1 = item.NewLotNumber1;
                            detail.LotNumber2 = item.NewLotNumber2 ?? "";
                            detail.LotDate = null;

                            if (qtyResult >= sqlDetailQty)
                            {
                                detail.Qty = (Int32)sqlDetailQty;
                                qtyResult = qtyResult - detail.Qty;
                            }
                            else
                            {
                                detail.Qty = qtyResult;
                                qtyResult = 0;
                            }

                            if (detail.Qty <= 0)
                            {
                                continue;
                            }

                            detail.UpdateDate = DateTime.Now;
                            detail.ReceiptDate = sqlDetail.ReceiptDate;
                            editHuDetailList.Add(detail);
                        }
                    }

                    EditHuDetailLotEntity result = new EditHuDetailLotEntity();
                    result.WhCode = item.WhCode;
                    result.ClientCode = item.ClientCode;
                    result.ClientSystemNumber = item.ClientSystemNumber;
                    result.AltItemNumber = item.AltItemNumber;
                    result.LotNumber1 = item.LotNumber1;
                    result.LotNumber2 = item.LotNumber2;
                    result.LotDate = item.LotDate;
                    result.NewLotDate = item.NewLotDate;
                    result.NewLotNumber1 = item.NewLotNumber1;
                    result.NewLotNumber2 = item.NewLotNumber2;
                    result.UserName = item.UserName;
                    result.Qty = 0;

                    //当一个修改循环完成后，开始执行修改
                    foreach (var item1 in editHuDetailList)
                    {
                        string getResult = BatchEditHuIdLot(item1.Id, item1.WhCode, item1.HuId, item1.Qty, item1.LotNumber1, item1.LotNumber2, "", item.UserName, item.ClientSystemNumber);

                        if (getResult == "Y")
                        {
                            result.Qty = result.Qty + item1.Qty;
                        }
                    }

                    TranLog tranLog = new TranLog();
                    tranLog.TranType = "860";
                    tranLog.Description = "EDI库存调整Lot款号汇总记录";
                    tranLog.TranDate = DateTime.Now;
                    tranLog.TranUser = item.UserName;
                    tranLog.WhCode = item.WhCode;
                    tranLog.ClientCode = item.ClientCode;
                    tranLog.SoNumber = "";
                    tranLog.CustomerPoNumber = "";
                    tranLog.AltItemNumber = item.AltItemNumber;
                    tranLog.ItemId = 0;
                    tranLog.UnitID = 0;
                    tranLog.UnitName = "";
                    tranLog.TranQty = item.Qty;
                    tranLog.TranQty2 = result.Qty;
                    tranLog.HuId = "";
                    tranLog.Length = 0;
                    tranLog.Width = 0;
                    tranLog.Height = 0;
                    tranLog.Weight = 0;
                    tranLog.LotNumber1 = item.LotNumber1;
                    tranLog.LotNumber2 = item.NewLotNumber1;
                    tranLog.LotDate = null;
                    tranLog.ReceiptId = "";
                    tranLog.ReceiptDate = null;
                    tranLog.Location = "";
                    tranLog.Location2 = "";
                    tranLog.HoldId = 0;
                    tranLog.HoldReason = "";
                    tranLog.CustomerOutPoNumber = item.ClientSystemNumber;
                    tranLog.Remark = item.Qty + "件原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "成功调整" + result.Qty + "件为Lot1:" + item.NewLotNumber1 + "-Lot2:" + item.NewLotNumber2 + "-LotDate:" + item.NewLotDate;
                    idal.ITranLogDAL.Add(tranLog);
                    idal.ITranLogDAL.SaveChanges();

                    resultList.Add(result);
                }
            }

            return resultList;
        }

        //调整库存Lot
        public string BatchEditHuIdLot(int[] huDetailId, string whCode, string locationId, int qty, string lot1, string lot2, string lotdate, string uesrName)
        {
            List<HuDetail> list = idal.IHuDetailDAL.SelectBy(u => huDetailId.Contains(u.Id));
            if (list.Count == 0)
            {
                return "没有需要调整的信息！";
            }

            List<HuMaster> checkHuMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
            if (checkHuMaster.Count == 0)
            {
                List<Pallate> checkHuId = idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
                List<WhLocation> checkLocationId = idal.IWhLocationDAL.SelectBy(u => u.WhCode == whCode && u.LocationId == locationId);
                if (checkLocationId.Count == 0)
                {
                    return "未找到调整后库位号！";
                }
                else
                {
                    if (checkHuId.Count == 0)
                    {
                        Pallate pallate = new Pallate();
                        pallate.WhCode = whCode;
                        pallate.HuId = locationId;
                        pallate.TypeId = 1;
                        pallate.Status = "U";
                        idal.IPallateDAL.Add(pallate);
                    }

                    HuMaster hu = new HuMaster();
                    hu.WhCode = whCode;
                    hu.HuId = locationId;
                    hu.Type = "M";
                    hu.Status = "A";
                    hu.Location = locationId;
                    hu.TransactionFlag = 0;
                    hu.ReceiptId = "";
                    hu.ReceiptDate = DateTime.Now;
                    hu.CreateUser = "";
                    hu.CreateDate = DateTime.Now;
                    idal.IHuMasterDAL.Add(hu);
                }
            }
            else
            {
                HuMaster firstHuMaster = checkHuMaster.First();
                if (firstHuMaster.Type != "M")
                {
                    return "库存托盘需为正常状态！";
                }
                else if (firstHuMaster.Status != "A")
                {
                    return "库存托盘需为正常状态！";
                }
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    foreach (var item in list)
                    {
                        if (item.PlanQty > 0)
                        {
                            continue;
                        }

                        if (item.Qty < qty)
                        {
                            continue;
                        }
                        else if (item.Qty == qty)
                        {
                            DateTime? lotd = null;
                            if (!string.IsNullOrEmpty(lotdate))
                            {
                                lotd = Convert.ToDateTime(lotdate);
                            }

                            List<HuDetail> getHuDetailListCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == locationId && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId && (u.LotNumber1 == lot1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (lot1 == null ? "" : lot1)) && (u.LotNumber2 == lot2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (lot2 == null ? "" : lot2)) && u.LotDate == lotd);
                            if (getHuDetailListCount.Count == 0)
                            {
                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "850";
                                tranLog.Description = "库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = item.LotNumber2;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = "";
                                tranLog.Location2 = "";
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为:" + lot1 + "-" + lot2 + "-" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                HuDetail huDetail = item;
                                huDetail.HuId = locationId;
                                huDetail.LotNumber1 = lot1;
                                huDetail.LotNumber2 = lot2;
                                if (!string.IsNullOrEmpty(lotdate))
                                {
                                    huDetail.LotDate = Convert.ToDateTime(lotdate);
                                }

                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { HuId = locationId, LotNumber1 = lot1, LotNumber2 = lot2, LotDate = huDetail.LotDate });
                            }
                            else
                            {
                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "850";
                                tranLog.Description = "库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = item.LotNumber2;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = "";
                                tranLog.Location2 = "";
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为:" + lot1 + "-" + lot2 + "-" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                HuDetail first = getHuDetailListCount.First();

                                first.Qty = first.Qty + qty;
                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == first.Id, t => new HuDetail { Qty = first.Qty });

                                idal.IHuDetailDAL.DeleteByExtended(u => u.Id == item.Id);
                            }

                        }
                        else if (item.Qty > qty)
                        {
                            DateTime? lotd = null;
                            if (!string.IsNullOrEmpty(lotdate))
                            {
                                lotd = Convert.ToDateTime(lotdate);
                            }

                            List<HuDetail> getHuDetailListCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == locationId && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId && (u.LotNumber1 == lot1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (lot1 == null ? "" : lot1)) && (u.LotNumber2 == lot2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (lot2 == null ? "" : lot2)) && u.LotDate == lotd);
                            if (getHuDetailListCount.Count == 0)
                            {
                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "850";
                                tranLog.Description = "库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = item.LotNumber2;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = "";
                                tranLog.Location2 = "";
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + item.LotNumber2 + "原LotDate:" + item.LotDate + "调整" + qty + "件为:" + lot1 + "-" + lot2 + "-" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                HuDetail huDetail = new HuDetail();
                                huDetail.WhCode = item.WhCode;
                                huDetail.HuId = locationId;
                                huDetail.ClientId = item.ClientId;
                                huDetail.ClientCode = item.ClientCode;
                                huDetail.SoNumber = item.SoNumber;
                                huDetail.CustomerPoNumber = item.CustomerPoNumber;
                                huDetail.AltItemNumber = item.AltItemNumber;
                                huDetail.ItemId = item.ItemId;
                                huDetail.UnitId = item.UnitId;
                                huDetail.UnitName = item.UnitName;
                                huDetail.ReceiptId = item.ReceiptId;
                                huDetail.Qty = qty;
                                huDetail.PlanQty = item.PlanQty ?? 0;
                                huDetail.ReceiptDate = item.ReceiptDate;
                                huDetail.Length = item.Length;
                                huDetail.Width = item.Width;
                                huDetail.Height = item.Height;
                                huDetail.Weight = item.Weight;
                                huDetail.LotNumber1 = lot1;
                                huDetail.LotNumber2 = lot2;
                                if (!string.IsNullOrEmpty(lotdate))
                                {
                                    huDetail.LotDate = Convert.ToDateTime(lotdate);
                                }
                                huDetail.Attribute1 = item.Attribute1;
                                huDetail.CreateUser = item.CreateUser;
                                huDetail.CreateDate = item.CreateDate;
                                idal.IHuDetailDAL.Add(huDetail);
                            }
                            else
                            {
                                HuDetail first = getHuDetailListCount.First();

                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "850";
                                tranLog.Description = "库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = item.LotNumber2;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = "";
                                tranLog.Location2 = "";
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + item.LotNumber2 + "原LotDate:" + item.LotDate + "调整" + qty + "件为:" + lot1 + "-" + lot2 + "-" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                first.Qty = first.Qty + qty;
                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == first.Id, t => new HuDetail { Qty = first.Qty });
                            }

                            HuDetail huDetail1 = item;
                            huDetail1.Qty = item.Qty - qty;
                            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { Qty = huDetail1.Qty });
                        }

                        List<HuDetail> checkHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == item.HuId && u.WhCode == item.WhCode).ToList();
                        if (checkHuDetailList.Count() == 0)
                        {
                            idal.IHuMasterDAL.DeleteByExtended(u => u.HuId == item.HuId && u.WhCode == item.WhCode);
                        }
                        idal.IHuDetailDAL.SaveChanges();
                    }

                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "批量调整库存Lot异常，请重试！";
                }
            }
        }



        //调整库存Lot
        public string BatchEditHuIdLot(int huDetailId, string whCode, string locationId, int qty, string lot1, string lot2, string lotdate, string uesrName, string clientSystemNumber)
        {
            List<HuDetail> list = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailId);
            if (list.Count == 0)
            {
                return "没有需要调整的信息！";
            }

            HuMaster firstHuMaster = new HuMaster();

            List<HuMaster> checkHuMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
            if (checkHuMaster.Count == 0)
            {
                List<Pallate> checkHuId = idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
                List<WhLocation> checkLocationId = idal.IWhLocationDAL.SelectBy(u => u.WhCode == whCode && u.LocationId == locationId);
                if (checkLocationId.Count == 0)
                {
                    return "未找到调整后库位号！";
                }
                else
                {
                    if (checkHuId.Count == 0)
                    {
                        Pallate pallate = new Pallate();
                        pallate.WhCode = whCode;
                        pallate.HuId = locationId;
                        pallate.TypeId = 1;
                        pallate.Status = "U";
                        idal.IPallateDAL.Add(pallate);
                    }

                    HuMaster hu = new HuMaster();
                    hu.WhCode = whCode;
                    hu.HuId = locationId;
                    hu.Type = "M";
                    hu.Status = "A";
                    hu.Location = locationId;
                    hu.TransactionFlag = 0;
                    hu.ReceiptId = "";
                    hu.ReceiptDate = DateTime.Now;
                    hu.CreateUser = "";
                    hu.CreateDate = DateTime.Now;
                    idal.IHuMasterDAL.Add(hu);
                }
            }
            else
            {
                firstHuMaster = checkHuMaster.First();
                if (firstHuMaster.Type != "M")
                {
                    return "库存托盘需为正常状态！";
                }
                else if (firstHuMaster.Status != "A")
                {
                    return "库存托盘需为正常状态！";
                }
            }



            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    string result = "";

                    foreach (var item in list)
                    {
                        #region 
                        if (item.PlanQty > 0)
                        {
                            if (item.Qty < qty)
                            {
                                result = "库存数量小于调整数量！";
                                break;
                            }

                            List<HuDetail> getHuDetailListCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == locationId && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId && (u.LotNumber1 == lot1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (lot1 == null ? "" : lot1)) && (u.LotNumber2 == lot2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (lot2 == null ? "" : lot2)));
                            if (getHuDetailListCount.Count == 0)
                            {
                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "860";
                                tranLog.Description = "EDI库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = lot1;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = firstHuMaster.Location;
                                tranLog.Location2 = firstHuMaster.Location;
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.CustomerOutPoNumber = clientSystemNumber;
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                HuDetail huDetail = new HuDetail();
                                huDetail.WhCode = item.WhCode;
                                huDetail.HuId = locationId;
                                huDetail.ClientId = item.ClientId;
                                huDetail.ClientCode = item.ClientCode;
                                huDetail.SoNumber = item.SoNumber;
                                huDetail.CustomerPoNumber = item.CustomerPoNumber;
                                huDetail.AltItemNumber = item.AltItemNumber;
                                huDetail.ItemId = item.ItemId;
                                huDetail.UnitId = item.UnitId;
                                huDetail.UnitName = item.UnitName;
                                huDetail.ReceiptId = item.ReceiptId;
                                huDetail.Qty = qty;
                                huDetail.PlanQty = 0;
                                huDetail.ReceiptDate = item.ReceiptDate;
                                huDetail.Length = item.Length;
                                huDetail.Width = item.Width;
                                huDetail.Height = item.Height;
                                huDetail.Weight = item.Weight;
                                huDetail.LotNumber1 = lot1;
                                huDetail.LotNumber2 = lot2;
                                if (!string.IsNullOrEmpty(lotdate))
                                {
                                    huDetail.LotDate = Convert.ToDateTime(lotdate);
                                }
                                huDetail.Attribute1 = item.Attribute1;
                                huDetail.CreateUser = item.CreateUser;
                                huDetail.CreateDate = item.CreateDate;
                                huDetail.UpdateUser = uesrName;
                                huDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.Add(huDetail);
                            }
                            else
                            {
                                HuDetail first = getHuDetailListCount.First();

                                //得到原始数据 进行日志添加
                                TranLog tranLog = new TranLog();
                                tranLog.TranType = "860";
                                tranLog.Description = "EDI库存调整Lot";
                                tranLog.TranDate = DateTime.Now;
                                tranLog.TranUser = uesrName;
                                tranLog.WhCode = whCode;
                                tranLog.ClientCode = item.ClientCode;
                                tranLog.SoNumber = item.SoNumber;
                                tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                tranLog.AltItemNumber = item.AltItemNumber;
                                tranLog.ItemId = item.ItemId;
                                tranLog.UnitID = item.UnitId;
                                tranLog.UnitName = item.UnitName;
                                tranLog.TranQty = item.Qty;
                                tranLog.TranQty2 = qty;
                                tranLog.HuId = item.HuId;
                                tranLog.Length = item.Length;
                                tranLog.Width = item.Width;
                                tranLog.Height = item.Height;
                                tranLog.Weight = item.Weight;
                                tranLog.LotNumber1 = item.LotNumber1;
                                tranLog.LotNumber2 = lot1;
                                tranLog.LotDate = item.LotDate;
                                tranLog.ReceiptId = item.ReceiptId;
                                tranLog.ReceiptDate = item.ReceiptDate;
                                tranLog.Location = firstHuMaster.Location;
                                tranLog.Location2 = firstHuMaster.Location;
                                tranLog.HoldId = 0;
                                tranLog.HoldReason = "";
                                tranLog.CustomerOutPoNumber = clientSystemNumber;
                                tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                idal.ITranLogDAL.Add(tranLog);

                                first.Qty = first.Qty + qty;
                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == first.Id, t => new HuDetail { Qty = first.Qty, UpdateUser = uesrName, UpdateDate = DateTime.Now });
                            }

                            HuDetail huDetail1 = item;
                            huDetail1.Qty = item.Qty - qty;
                            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { Qty = huDetail1.Qty, UpdateUser = uesrName, UpdateDate = DateTime.Now });

                            idal.IHuDetailDAL.SaveChanges();
                        }
                        else
                        {
                            if (item.Qty < qty)
                            {
                                result = "库存数量小于调整数量！";
                                break;
                            }
                            else if (item.Qty == qty)
                            {
                                DateTime? lotd = null;
                                if (!string.IsNullOrEmpty(lotdate))
                                {
                                    lotd = Convert.ToDateTime(lotdate);
                                }

                                List<HuDetail> getHuDetailListCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == locationId && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId && (u.LotNumber1 == lot1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (lot1 == null ? "" : lot1)) && (u.LotNumber2 == lot2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (lot2 == null ? "" : lot2)) && u.LotDate == lotd);
                                if (getHuDetailListCount.Count == 0)
                                {
                                    //得到原始数据 进行日志添加
                                    TranLog tranLog = new TranLog();
                                    tranLog.TranType = "860";
                                    tranLog.Description = "EDI库存调整Lot";
                                    tranLog.TranDate = DateTime.Now;
                                    tranLog.TranUser = uesrName;
                                    tranLog.WhCode = whCode;
                                    tranLog.ClientCode = item.ClientCode;
                                    tranLog.SoNumber = item.SoNumber;
                                    tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                    tranLog.AltItemNumber = item.AltItemNumber;
                                    tranLog.ItemId = item.ItemId;
                                    tranLog.UnitID = item.UnitId;
                                    tranLog.UnitName = item.UnitName;
                                    tranLog.TranQty = item.Qty;
                                    tranLog.TranQty2 = qty;
                                    tranLog.HuId = item.HuId;
                                    tranLog.Length = item.Length;
                                    tranLog.Width = item.Width;
                                    tranLog.Height = item.Height;
                                    tranLog.Weight = item.Weight;
                                    tranLog.LotNumber1 = item.LotNumber1;
                                    tranLog.LotNumber2 = lot1;
                                    tranLog.LotDate = item.LotDate;
                                    tranLog.ReceiptId = item.ReceiptId;
                                    tranLog.ReceiptDate = item.ReceiptDate;
                                    tranLog.Location = firstHuMaster.Location;
                                    tranLog.Location2 = firstHuMaster.Location;
                                    tranLog.HoldId = 0;
                                    tranLog.HoldReason = "";
                                    tranLog.CustomerOutPoNumber = clientSystemNumber;
                                    tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                    idal.ITranLogDAL.Add(tranLog);

                                    HuDetail huDetail = item;
                                    huDetail.HuId = locationId;
                                    huDetail.LotNumber1 = lot1;
                                    huDetail.LotNumber2 = lot2;
                                    if (!string.IsNullOrEmpty(lotdate))
                                    {
                                        huDetail.LotDate = Convert.ToDateTime(lotdate);
                                    }

                                    idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail
                                    {
                                        HuId = locationId,
                                        LotNumber1 = lot1,
                                        LotNumber2 = lot2,
                                        LotDate = huDetail.LotDate,
                                        UpdateUser = uesrName,
                                        UpdateDate = DateTime.Now
                                    });
                                }
                                else
                                {
                                    //得到原始数据 进行日志添加
                                    TranLog tranLog = new TranLog();
                                    tranLog.TranType = "860";
                                    tranLog.Description = "EDI库存调整Lot";
                                    tranLog.TranDate = DateTime.Now;
                                    tranLog.TranUser = uesrName;
                                    tranLog.WhCode = whCode;
                                    tranLog.ClientCode = item.ClientCode;
                                    tranLog.SoNumber = item.SoNumber;
                                    tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                    tranLog.AltItemNumber = item.AltItemNumber;
                                    tranLog.ItemId = item.ItemId;
                                    tranLog.UnitID = item.UnitId;
                                    tranLog.UnitName = item.UnitName;
                                    tranLog.TranQty = item.Qty;
                                    tranLog.TranQty2 = qty;
                                    tranLog.HuId = item.HuId;
                                    tranLog.Length = item.Length;
                                    tranLog.Width = item.Width;
                                    tranLog.Height = item.Height;
                                    tranLog.Weight = item.Weight;
                                    tranLog.LotNumber1 = item.LotNumber1;
                                    tranLog.LotNumber2 = lot1;
                                    tranLog.LotDate = item.LotDate;
                                    tranLog.ReceiptId = item.ReceiptId;
                                    tranLog.ReceiptDate = item.ReceiptDate;
                                    tranLog.Location = firstHuMaster.Location;
                                    tranLog.Location2 = firstHuMaster.Location;
                                    tranLog.HoldId = 0;
                                    tranLog.HoldReason = "";
                                    tranLog.CustomerOutPoNumber = clientSystemNumber;
                                    tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                    idal.ITranLogDAL.Add(tranLog);

                                    HuDetail first = getHuDetailListCount.First();

                                    first.Qty = first.Qty + qty;
                                    idal.IHuDetailDAL.UpdateByExtended(u => u.Id == first.Id, t => new HuDetail { Qty = first.Qty, UpdateUser = uesrName, UpdateDate = DateTime.Now });

                                    idal.IHuDetailDAL.DeleteByExtended(u => u.Id == item.Id);
                                }

                            }
                            else if (item.Qty > qty)
                            {
                                DateTime? lotd = null;
                                if (!string.IsNullOrEmpty(lotdate))
                                {
                                    lotd = Convert.ToDateTime(lotdate);
                                }

                                List<HuDetail> getHuDetailListCount = idal.IHuDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == locationId && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId && (u.LotNumber1 == lot1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (lot1 == null ? "" : lot1)) && (u.LotNumber2 == lot2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (lot2 == null ? "" : lot2)) && u.LotDate == lotd);
                                if (getHuDetailListCount.Count == 0)
                                {
                                    //得到原始数据 进行日志添加
                                    TranLog tranLog = new TranLog();
                                    tranLog.TranType = "860";
                                    tranLog.Description = "EDI库存调整Lot";
                                    tranLog.TranDate = DateTime.Now;
                                    tranLog.TranUser = uesrName;
                                    tranLog.WhCode = whCode;
                                    tranLog.ClientCode = item.ClientCode;
                                    tranLog.SoNumber = item.SoNumber;
                                    tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                    tranLog.AltItemNumber = item.AltItemNumber;
                                    tranLog.ItemId = item.ItemId;
                                    tranLog.UnitID = item.UnitId;
                                    tranLog.UnitName = item.UnitName;
                                    tranLog.TranQty = item.Qty;
                                    tranLog.TranQty2 = qty;
                                    tranLog.HuId = item.HuId;
                                    tranLog.Length = item.Length;
                                    tranLog.Width = item.Width;
                                    tranLog.Height = item.Height;
                                    tranLog.Weight = item.Weight;
                                    tranLog.LotNumber1 = item.LotNumber1;
                                    tranLog.LotNumber2 = lot1;
                                    tranLog.LotDate = item.LotDate;
                                    tranLog.ReceiptId = item.ReceiptId;
                                    tranLog.ReceiptDate = item.ReceiptDate;
                                    tranLog.Location = firstHuMaster.Location;
                                    tranLog.Location2 = firstHuMaster.Location;
                                    tranLog.HoldId = 0;
                                    tranLog.HoldReason = "";
                                    tranLog.CustomerOutPoNumber = clientSystemNumber;
                                    tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                    idal.ITranLogDAL.Add(tranLog);

                                    HuDetail huDetail = new HuDetail();
                                    huDetail.WhCode = item.WhCode;
                                    huDetail.HuId = locationId;
                                    huDetail.ClientId = item.ClientId;
                                    huDetail.ClientCode = item.ClientCode;
                                    huDetail.SoNumber = item.SoNumber;
                                    huDetail.CustomerPoNumber = item.CustomerPoNumber;
                                    huDetail.AltItemNumber = item.AltItemNumber;
                                    huDetail.ItemId = item.ItemId;
                                    huDetail.UnitId = item.UnitId;
                                    huDetail.UnitName = item.UnitName;
                                    huDetail.ReceiptId = item.ReceiptId;
                                    huDetail.Qty = qty;
                                    huDetail.PlanQty = item.PlanQty ?? 0;
                                    huDetail.ReceiptDate = item.ReceiptDate;
                                    huDetail.Length = item.Length;
                                    huDetail.Width = item.Width;
                                    huDetail.Height = item.Height;
                                    huDetail.Weight = item.Weight;
                                    huDetail.LotNumber1 = lot1;
                                    huDetail.LotNumber2 = lot2;
                                    if (!string.IsNullOrEmpty(lotdate))
                                    {
                                        huDetail.LotDate = Convert.ToDateTime(lotdate);
                                    }
                                    huDetail.Attribute1 = item.Attribute1;
                                    huDetail.CreateUser = item.CreateUser;
                                    huDetail.CreateDate = item.CreateDate;
                                    huDetail.UpdateUser = uesrName;
                                    huDetail.UpdateDate = DateTime.Now;
                                    idal.IHuDetailDAL.Add(huDetail);
                                }
                                else
                                {
                                    HuDetail first = getHuDetailListCount.First();

                                    //得到原始数据 进行日志添加
                                    TranLog tranLog = new TranLog();
                                    tranLog.TranType = "860";
                                    tranLog.Description = "EDI库存调整Lot";
                                    tranLog.TranDate = DateTime.Now;
                                    tranLog.TranUser = uesrName;
                                    tranLog.WhCode = whCode;
                                    tranLog.ClientCode = item.ClientCode;
                                    tranLog.SoNumber = item.SoNumber;
                                    tranLog.CustomerPoNumber = item.CustomerPoNumber;
                                    tranLog.AltItemNumber = item.AltItemNumber;
                                    tranLog.ItemId = item.ItemId;
                                    tranLog.UnitID = item.UnitId;
                                    tranLog.UnitName = item.UnitName;
                                    tranLog.TranQty = item.Qty;
                                    tranLog.TranQty2 = qty;
                                    tranLog.HuId = item.HuId;
                                    tranLog.Length = item.Length;
                                    tranLog.Width = item.Width;
                                    tranLog.Height = item.Height;
                                    tranLog.Weight = item.Weight;
                                    tranLog.LotNumber1 = item.LotNumber1;
                                    tranLog.LotNumber2 = lot1;
                                    tranLog.LotDate = item.LotDate;
                                    tranLog.ReceiptId = item.ReceiptId;
                                    tranLog.ReceiptDate = item.ReceiptDate;
                                    tranLog.Location = firstHuMaster.Location;
                                    tranLog.Location2 = firstHuMaster.Location;
                                    tranLog.HoldId = 0;
                                    tranLog.HoldReason = "";
                                    tranLog.CustomerOutPoNumber = clientSystemNumber;
                                    tranLog.Remark = "原Lot1:" + item.LotNumber1 + "原Lot2:" + "原LotDate:" + item.LotNumber2 + "调整" + qty + "件为Lot1:" + lot1 + "-Lot2:" + lot2 + "-LotDate:" + lotdate;
                                    idal.ITranLogDAL.Add(tranLog);

                                    first.Qty = first.Qty + qty;
                                    idal.IHuDetailDAL.UpdateByExtended(u => u.Id == first.Id, t => new HuDetail { Qty = first.Qty, UpdateUser = uesrName, UpdateDate = DateTime.Now });
                                }

                                HuDetail huDetail1 = item;
                                huDetail1.Qty = item.Qty - qty;
                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { Qty = huDetail1.Qty, UpdateUser = uesrName, UpdateDate = DateTime.Now });
                            }

                            List<HuDetail> checkHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == item.HuId && u.WhCode == item.WhCode).ToList();
                            if (checkHuDetailList.Count() == 0)
                            {
                                idal.IHuMasterDAL.DeleteByExtended(u => u.HuId == item.HuId && u.WhCode == item.WhCode);
                            }
                            idal.IHuDetailDAL.SaveChanges();
                        }

                        #endregion
                    }

                    if (result != "")
                    {
                        return result;
                    }

                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "批量调整库存Lot异常，请重试！";
                }
            }
        }


        //异常库位批量上架
        public string BatchPutHuIdABLocation(int[] huDetailId, string whCode, string locationId, string uesrName)
        {
            List<HuDetail> list = idal.IHuDetailDAL.SelectBy(u => huDetailId.Contains(u.Id));
            if (list.Count == 0)
            {
                return "没有需要上架的信息，请重新查询！";
            }

            string[] CheckClientCodeList = (from a in list select a.ClientCode).ToList().Distinct().ToArray();
            if (CheckClientCodeList.Count() > 1)
            {
                return "仅允许单客户批量上架，请选择客户查询后再次操作！";
            }

            string[] CheckItemNumberList = (from a in list select a.AltItemNumber).ToList().Distinct().ToArray();
            if (CheckItemNumberList.Count() > 1)
            {
                return "仅允许单款号批量上架，请输入款号查询后再次操作！";
            }

            string[] CheckLotNumber1List = (from a in list select a.LotNumber1).ToList().Distinct().ToArray();
            if (CheckLotNumber1List.Count() > 1)
            {
                return "仅允许单款号单Lot1批量上架，请输入Lot1查询后再次操作！";
            }

            string[] CheckLotNumber2List = (from a in list select a.LotNumber2).ToList().Distinct().ToArray();
            if (CheckLotNumber2List.Count() > 1)
            {
                return "仅允许单款号单Lot2批量上架，请输入Lot2查询后再次操作！";
            }

            string[] CheckLotDateList = (from a in list select a.LotDate.ToString()).ToList().Distinct().ToArray();
            if (CheckLotDateList.Count() > 1)
            {
                return "仅允许单款号单LotDate批量上架，请输入LotDate查询后再次操作！";
            }


            List<HuMaster> checkHuMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
            if (checkHuMaster.Count == 0)
            {
                List<Pallate> checkHuId = idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == locationId);
                List<WhLocation> checkLocationId = idal.IWhLocationDAL.SelectBy(u => u.WhCode == whCode && u.LocationId == locationId);
                if (checkLocationId.Count == 0)
                {
                    return "系统未找到本次上架的正常库位，请先添加该库位！";
                }
                else
                {
                    if (checkHuId.Count == 0)
                    {
                        Pallate pallate = new Pallate();
                        pallate.WhCode = whCode;
                        pallate.HuId = locationId;
                        pallate.TypeId = 1;
                        pallate.Status = "U";
                        idal.IPallateDAL.Add(pallate);
                    }

                    HuMaster hu = new HuMaster();
                    hu.WhCode = whCode;
                    hu.HuId = locationId;
                    hu.Type = "M";
                    hu.Status = "A";
                    hu.Location = locationId;
                    hu.TransactionFlag = 0;
                    hu.ReceiptId = "";
                    hu.ReceiptDate = DateTime.Now;
                    hu.CreateUser = "";
                    hu.CreateDate = DateTime.Now;
                    idal.IHuMasterDAL.Add(hu);
                }
            }
            else
            {
                HuMaster firstHuMaster = checkHuMaster.First();
                if (firstHuMaster.Type != "M")
                {
                    return "库存存在该上架库位，上架库位需为正常状态！";
                }
                else if (firstHuMaster.Status != "A")
                {
                    return "库存存在该上架库位，上架库位需为正常状态！";
                }
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    HuDetail firstHuDetail = list.First();
                    HuMaster firstHuMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == firstHuDetail.HuId).First();

                    int sumQty = Convert.ToInt32(list.Sum(u => u.Qty).ToString());

                    //得到原始数据 进行日志添加
                    TranLog tranLog = new TranLog();
                    tranLog.TranType = "906";
                    tranLog.Description = "异常库位批量上架";
                    tranLog.TranDate = DateTime.Now;
                    tranLog.TranUser = uesrName;
                    tranLog.WhCode = whCode;
                    tranLog.ClientCode = firstHuDetail.ClientCode;
                    tranLog.SoNumber = "";
                    tranLog.CustomerPoNumber = "";
                    tranLog.AltItemNumber = firstHuDetail.AltItemNumber;
                    tranLog.ItemId = firstHuDetail.ItemId;
                    tranLog.UnitID = firstHuDetail.UnitId;
                    tranLog.UnitName = firstHuDetail.UnitName;
                    tranLog.TranQty = sumQty;
                    tranLog.TranQty2 = sumQty;
                    tranLog.HuId = firstHuMaster.Location;
                    tranLog.Length = firstHuDetail.Length;
                    tranLog.Width = firstHuDetail.Width;
                    tranLog.Height = firstHuDetail.Height;
                    tranLog.Weight = firstHuDetail.Weight;
                    tranLog.LotNumber1 = firstHuDetail.LotNumber1;
                    tranLog.LotNumber2 = firstHuDetail.LotNumber2;
                    tranLog.LotDate = firstHuDetail.LotDate;
                    tranLog.ReceiptId = "";
                    tranLog.Location = firstHuMaster.Location;
                    tranLog.Location2 = locationId;
                    tranLog.HoldId = firstHuMaster.HoldId;
                    tranLog.HoldReason = firstHuMaster.HoldReason;
                    idal.ITranLogDAL.Add(tranLog);

                    HuDetail huDetail = new HuDetail();
                    huDetail.WhCode = firstHuDetail.WhCode;
                    huDetail.HuId = locationId;
                    huDetail.ClientId = firstHuDetail.ClientId;
                    huDetail.ClientCode = firstHuDetail.ClientCode;
                    huDetail.SoNumber = "";
                    huDetail.CustomerPoNumber = "";
                    huDetail.AltItemNumber = firstHuDetail.AltItemNumber;
                    huDetail.ItemId = firstHuDetail.ItemId;
                    huDetail.UnitId = firstHuDetail.UnitId;
                    huDetail.UnitName = firstHuDetail.UnitName;
                    huDetail.ReceiptId = "";
                    huDetail.Qty = sumQty;
                    huDetail.PlanQty = 0;
                    huDetail.ReceiptDate = DateTime.Now;
                    huDetail.Length = firstHuDetail.Length;
                    huDetail.Width = firstHuDetail.Width;
                    huDetail.Height = firstHuDetail.Height;
                    huDetail.Weight = firstHuDetail.Weight;
                    huDetail.LotNumber1 = firstHuDetail.LotNumber1;
                    huDetail.LotNumber2 = firstHuDetail.LotNumber2;
                    huDetail.LotDate = firstHuDetail.LotDate;
                    huDetail.Attribute1 = firstHuDetail.Attribute1;
                    huDetail.CreateUser = uesrName;
                    huDetail.CreateDate = DateTime.Now;
                    idal.IHuDetailDAL.Add(huDetail);

                    idal.IHuDetailDAL.DeleteByExtended(u => huDetailId.Contains(u.Id));

                    idal.IHuDetailDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "异常库位批量上架异常，请重试！";
                }
            }
        }

    }
}
