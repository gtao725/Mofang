using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.IBLL;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace WMS.BLL
{
    public class RecManager : IRecManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        RecHelper recHelper = new RecHelper();
        EclInBoundManager oms = new EclInBoundManager();
        //API 97
        public string APIReqUrl = "http://10.88.88.97:3100/api//";
        //public string APIReqUrl = "http://localhost:52078/api/";

        private static object o = new object();

        //收货信息更改 查询
        public List<ReceiptResult> C_RecQuestionList(ReceiptSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join d in idal.IReceiptRegisterDAL.SelectAll()
                      on new { A = a.WhCode, B = a.ReceiptId } equals new { A = d.WhCode, B = d.ReceiptId } into temp3
                      from d in temp3.DefaultIfEmpty()

                      join b in idal.IItemMasterDAL.SelectAll()
                      on new { B = a.ItemId } equals new { B = b.Id } into temp4
                      from b in temp4.DefaultIfEmpty()

                      join c in idal.IUnitDefaultDAL.SelectAll()
                      on new { A = a.WhCode, B = a.UnitName } equals new { A = c.WhCode, B = c.UnitName } into temp2
                      from c in temp2.DefaultIfEmpty()

                      join f in idal.ILookUpDAL.SelectAll()
                      on new { LotFlag = a.LotFlag.ToString() } equals new { LotFlag = f.ColumnKey } into temp1
                      from f in temp1.DefaultIfEmpty()

                      where f.TableName == "Receipt" && a.WhCode == searchEntity.WhCode
                      select new ReceiptResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          WhCode = a.WhCode,
                          ClientCode = a.ClientCode,
                          ReceiptDate = a.ReceiptDate,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          Style1 = b.Style1,
                          HuId = a.HuId,
                          Qty = a.Qty,
                          UnitId = a.UnitId,
                          UnitName = a.UnitName,
                          UnitNameShow = c.UnitNameCN,
                          LotFlagShow = f.Description,
                          Length = a.Length,
                          Width = a.Width,
                          Height = a.Height,
                          Weight = a.Weight,
                          HoldReason = a.HoldReason,
                          CBM = a.UnitName.Contains("ECH") ? 0 : a.Length * a.Width * a.Height * a.Qty,
                          Custom1 = a.Custom1 ?? "",
                          Custom2 = a.Custom2 ?? "",
                          Custom3 = a.Custom3 ?? "",
                          CreateUser = a.CreateUser,
                          TransportType = d.TransportType,
                          TransportTypeExtend = d.TransportTypeExtend
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            }
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));
            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);

            if (searchEntity.ReceiptDateBegin != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.ReceiptDateBegin);
            }
            if (searchEntity.ReceiptDateEnd != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.ReceiptDateEnd);
            }

            List<ReceiptResult> sql1 = sql.ToList();

            total = sql1.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"实收数量\":\"" + sql1.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql1.Sum(u => u.CBM).ToString() + "\"}";
            }
            sql1 = sql1.OrderBy(u => u.Id).Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return sql1.ToList();
        }

        public List<ReceiptDetailCompleteResult> C_RecCompleteList(ReceiptSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IReceiptDAL.SelectAll()
                            on new { a.WhCode, a.ReceiptId, a.PoId, a.ItemId }
                        equals new { b.WhCode, b.ReceiptId, b.PoId, b.ItemId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IInBoundOrderDAL.SelectAll() on new { PoId = a.PoId } equals new { PoId = c.Id } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)c.SoId } equals new { SoId = d.Id } into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in idal.IReceiptRegisterDAL.SelectAll()
                            on new { a.WhCode, a.ReceiptId }
                        equals new { e.WhCode, e.ReceiptId } into e_join
                      from e in e_join.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode && e.Status != "N"
                      group new { e, a, d, c, b } by new
                      {
                          e.ClientCode,
                          e.LocationId,
                          e.Status,
                          a.ReceiptId,
                          d.SoNumber,
                          c.CustomerPoNumber,
                          a.AltItemNumber,
                          a.RegQty
                      } into g
                      select new ReceiptDetailCompleteResult
                      {
                          Action = "",
                          ClientCode = g.Key.ClientCode,
                          LocationId = g.Key.LocationId,
                          Status = g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                          ReceiptId = g.Key.ReceiptId,
                          SoNumber = g.Key.SoNumber,
                          PoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          RegQty = g.Key.RegQty,
                          RecQty = g.Sum(p => ((Int32?)p.b.Qty ?? (Int32?)0))
                      };

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"实收数量\":\"" + sql.Sum(u => u.RecQty).ToString() + "\",\"登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.PoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        #region 收货信息更改 功能汇总

        //批量修改收货TCR异常原因 并冻结库存、修改TCR报表
        public string FrozenEditList(string[] idArr, string whCode, string holdReason, string userName)
        {
            List<int> idList = new List<int>();
            foreach (var item in idArr)
            {
                idList.Add(Convert.ToInt32(item));
            }
            //得到需要修改的 收货信息
            List<Receipt> receiptList = idal.IReceiptDAL.SelectBy(u => idList.Contains(u.Id));
            if (receiptList.Count == 0)
            {
                return "未找到收货数据，请重新查询！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //2.修改TCR表
                    //2.1 查询需要修改的 TCR表中的数据
                    string[] receiptArr = (from a in receiptList select a.ReceiptId).ToList().Distinct().ToArray();
                    string[] huIdArr = (from a in receiptList select a.HuId).ToList().Distinct().ToArray();

                    List<PhotoMaster> tcrList = idal.IPhotoMasterDAL.SelectBy(u => u.WhCode == whCode && receiptArr.Contains(u.Number) && huIdArr.Contains(u.HuId));

                    List<TranLog> tranLogList = new List<TranLog>();
                    List<PhotoMaster> photoList = new List<PhotoMaster>();  //TCR待新增的列表
                    foreach (var item in receiptList)
                    {
                        TranLog tranLog = new TranLog();
                        tranLog.TranType = "111";
                        tranLog.Description = "收货修改TCR原因";
                        tranLog.TranDate = DateTime.Now;
                        tranLog.TranUser = userName;
                        tranLog.WhCode = whCode;
                        tranLog.ClientCode = item.ClientCode;
                        tranLog.SoNumber = item.SoNumber;
                        tranLog.PoID = item.PoId;
                        tranLog.CustomerPoNumber = item.CustomerPoNumber;
                        tranLog.AltItemNumber = item.AltItemNumber;
                        tranLog.LotFlag = item.LotFlag;
                        tranLog.ItemId = item.ItemId;
                        tranLog.UnitID = item.UnitId;
                        tranLog.UnitName = item.UnitName;
                        tranLog.TranQty = item.Qty;
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
                        tranLog.Remark = "原原因:" + item.HoldReason + "新原因:" + holdReason;
                        if (tranLog.Remark.Length > 99)
                        {
                            tranLog.Remark = tranLog.Remark.Substring(0, 98);
                        }
                        tranLogList.Add(tranLog);

                        //1. 修改收货表
                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item.Id, t => new Receipt { HoldReason = holdReason, Status = "H", UpdateUser = userName, UpdateDate = DateTime.Now });

                        if (tcrList.Where(u => u.Number == item.ReceiptId && u.WhCode == whCode && u.Number2 == item.SoNumber && u.Number3 == item.CustomerPoNumber && u.Number4 == item.AltItemNumber && u.HuId == item.HuId).Count() == 0)
                        {
                            //2.2如果不存在 需要新增一行TCR
                            PhotoMaster photoMaster = new PhotoMaster();

                            photoMaster.PhotoId = 0;
                            photoMaster.WhCode = item.WhCode;
                            photoMaster.ClientCode = item.ClientCode;
                            photoMaster.Number = item.ReceiptId;
                            photoMaster.Number2 = item.SoNumber;
                            photoMaster.Number3 = item.CustomerPoNumber;
                            photoMaster.Number4 = item.AltItemNumber;

                            photoMaster.PoId = item.PoId;
                            photoMaster.ItemId = item.ItemId;
                            photoMaster.UnitName = item.UnitName;
                            photoMaster.Qty = item.Qty;
                            photoMaster.RegQty = 0;
                            photoMaster.HuId = item.HuId;
                            photoMaster.HoldReason = holdReason;
                            photoMaster.TCRStatus = "未处理";
                            photoMaster.TCRProcessMode = "";

                            photoMaster.SettlementMode = "";
                            photoMaster.SumPrice = 0;
                            photoMaster.DeliveryDate = item.ReceiptDate;
                            photoMaster.Type = "in";

                            photoMaster.Status = 0;

                            photoMaster.CheckStatus1 = "N";
                            photoMaster.CheckStatus2 = "N";
                            photoMaster.CreateUser = userName;
                            photoMaster.CreateDate = DateTime.Now;
                            photoList.Add(photoMaster);

                        }
                        else
                        {
                            //2.3如果托盘、SO\PO\SKU\收货批次号 已存在，直接修改TCR原因
                            idal.IPhotoMasterDAL.UpdateByExtended(u => u.Number == item.ReceiptId && u.WhCode == whCode && u.Number2 == item.SoNumber && u.Number3 == item.CustomerPoNumber && u.Number4 == item.AltItemNumber && u.HuId == item.HuId, t => new PhotoMaster { HoldReason = holdReason, UpdateUser = userName, UpdateDate = DateTime.Now });
                        }
                    }

                    //3.修改库存
                    List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && huIdArr.Contains(u.HuId) && receiptArr.Contains(u.ReceiptId));

                    List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && huIdArr.Contains(u.HuId) && receiptArr.Contains(u.ReceiptId));
                    if (huMasterList.Count > 0)
                    {
                        foreach (var item in huMasterList)
                        {
                            List<HuDetail> checkHuDetaiList = huDetailList.Where(u => u.WhCode == item.WhCode && u.HuId == item.HuId).ToList();

                            //如果没有释放数量
                            if (checkHuDetaiList.Where(u => u.PlanQty > 0).Count() == 0)
                            {
                                idal.IHuMasterDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuMaster { HoldReason = holdReason, HoldId = 0, Status = "H", UpdateUser = userName, UpdateDate = DateTime.Now });
                            }
                        }
                    }

                    idal.ITranLogDAL.Add(tranLogList);
                    if (photoList.Count > 0)
                    {
                        idal.IPhotoMasterDAL.Add(photoList);
                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改TCR原因时出现异常，请重新提交！";
                }
            }
        }

        //批量清除收货TCR异常原因
        public string FrozenDeleteList(string[] idArr, string whCode, string userName)
        {
            List<int> idList = new List<int>();
            foreach (var item in idArr)
            {
                idList.Add(Convert.ToInt32(item));
            }
            //得到需要修改的 收货信息
            List<Receipt> receiptList = idal.IReceiptDAL.SelectBy(u => idList.Contains(u.Id));
            if (receiptList.Count == 0)
            {
                return "未找到收货数据，请重新查询！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    string[] receiptArr = (from a in receiptList select a.ReceiptId).ToList().Distinct().ToArray();
                    string[] huIdArr = (from a in receiptList select a.HuId).ToList().Distinct().ToArray();

                    List<TranLog> tranLogList = new List<TranLog>();

                    foreach (var item in receiptList)
                    {
                        TranLog tranLog = new TranLog();
                        tranLog.TranType = "112";
                        tranLog.Description = "收货清除TCR原因";
                        tranLog.TranDate = DateTime.Now;
                        tranLog.TranUser = userName;
                        tranLog.WhCode = whCode;
                        tranLog.ClientCode = item.ClientCode;
                        tranLog.SoNumber = item.SoNumber;
                        tranLog.PoID = item.PoId;
                        tranLog.CustomerPoNumber = item.CustomerPoNumber;
                        tranLog.AltItemNumber = item.AltItemNumber;
                        tranLog.LotFlag = item.LotFlag;
                        tranLog.ItemId = item.ItemId;
                        tranLog.UnitID = item.UnitId;
                        tranLog.UnitName = item.UnitName;
                        tranLog.TranQty = item.Qty;
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
                        tranLog.Remark = "原原因:" + item.HoldReason;
                        if (tranLog.Remark.Length > 99)
                        {
                            tranLog.Remark = tranLog.Remark.Substring(0, 98);
                        }
                        tranLogList.Add(tranLog);

                        //1. 修改收货表
                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item.Id, t => new Receipt { HoldReason = "", Status = "A", UpdateUser = userName, UpdateDate = DateTime.Now });

                        //2.删除TCR
                        idal.IPhotoMasterDAL.DeleteByExtended(u => u.Number == item.ReceiptId && u.WhCode == whCode && u.Number2 == item.SoNumber && u.Number3 == item.CustomerPoNumber && u.Number4 == item.AltItemNumber && u.HuId == item.HuId && u.TCRStatus == "未处理");
                    }

                    //3.修改库存
                    List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && huIdArr.Contains(u.HuId) && receiptArr.Contains(u.ReceiptId));

                    List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && huIdArr.Contains(u.HuId) && receiptArr.Contains(u.ReceiptId));

                    if (huMasterList.Count > 0)
                    {
                        foreach (var item in huMasterList)
                        {
                            List<HuDetail> checkHuDetaiList = huDetailList.Where(u => u.WhCode == item.WhCode && u.HuId == item.HuId).ToList();

                            //如果没有释放数量
                            if (checkHuDetaiList.Where(u => u.PlanQty > 0).Count() == 0)
                            {
                                idal.IHuMasterDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuMaster { HoldReason = "", Status = "A", UpdateUser = userName, UpdateDate = DateTime.Now });
                            }
                        }
                    }

                    idal.ITranLogDAL.Add(tranLogList);
                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "清除TCR原因时出现异常，请重新提交！";
                }
            }
        }


        //批量修改收货分票
        public string LotFlagEditList(string[] idArr, int lotFlag, string whCode, string userName)
        {
            List<int> idList = new List<int>();
            foreach (var item in idArr)
            {
                idList.Add(Convert.ToInt32(item));
            }
            //得到需要修改的 收货信息
            List<Receipt> receiptList = idal.IReceiptDAL.SelectBy(u => idList.Contains(u.Id));
            if (receiptList.Count == 0)
            {
                return "未找到收货数据，请重新查询！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    List<TranLog> tranLogList = new List<TranLog>();
                    foreach (var item in receiptList)
                    {
                        TranLog tranLog = new TranLog();
                        if (lotFlag > 0)
                        {
                            tranLog.TranType = "113";
                            tranLog.Description = "收货修改分票";
                        }
                        else if (lotFlag == 0)
                        {
                            tranLog.TranType = "114";
                            tranLog.Description = "收货清除分票";
                        }

                        tranLog.TranDate = DateTime.Now;
                        tranLog.TranUser = userName;
                        tranLog.WhCode = whCode;
                        tranLog.ClientCode = item.ClientCode;
                        tranLog.SoNumber = item.SoNumber;
                        tranLog.PoID = item.PoId;
                        tranLog.CustomerPoNumber = item.CustomerPoNumber;
                        tranLog.AltItemNumber = item.AltItemNumber;
                        tranLog.LotFlag = item.LotFlag;
                        tranLog.ItemId = item.ItemId;
                        tranLog.UnitID = item.UnitId;
                        tranLog.UnitName = item.UnitName;
                        tranLog.TranQty = item.Qty;
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
                        tranLog.Remark = "原分票:" + (item.LotFlag) + "新分票:" + lotFlag;
                        if (tranLog.Remark.Length > 99)
                        {
                            tranLog.Remark = tranLog.Remark.Substring(0, 98);
                        }
                        tranLogList.Add(tranLog);

                        //1. 修改收货表
                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item.Id, t => new Receipt { LotFlag = lotFlag, UpdateUser = userName, UpdateDate = DateTime.Now });

                        //2.修改工作量分票
                        idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.HuId == item.HuId && u.WorkType != "叉车工", t => new WorkloadAccount { LotFlag = lotFlag });
                    }

                    idal.ITranLogDAL.Add(tranLogList);
                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改分票时出现异常，请重新提交！";
                }
            }
        }

        #endregion


        //修改收货车型
        public string ReceiptEditRegTransportType(string receiptId, string whCode, string transportType, string transportTypeExtend, string userName)
        {
            List<ReceiptRegister> list = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
            if (list.Count == 0)
            {
                return "未找到收货登记信息，请重新查询！";
            }

            ReceiptRegister reg = list.First();

            TranLog tl = new TranLog();
            tl.TranType = "75";
            tl.Description = "收货登记修改车型";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = reg.WhCode;
            tl.ReceiptId = reg.ReceiptId;
            tl.Remark = "车型修改为:" + transportType + transportTypeExtend;
            idal.ITranLogDAL.Add(tl);

            reg.TransportType = transportType;
            reg.TransportTypeExtend = transportTypeExtend;
            idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "TransportType", "TransportTypeExtend" });

            idal.IReceiptRegisterDAL.SaveChanges();

            return "Y";
        }

        //修改收货信息
        public string ReceiptEdit(Receipt entity)
        {
            Receipt rec = idal.IReceiptDAL.SelectBy(u => u.Id == entity.Id).First();
            if (entity.UnitName != rec.UnitName)
            {
                if (idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId && u.Status != "C").Count > 0)
                {
                    return "修改收货单位必须完成收货才可修改！";
                }
            }

            string checkUnitName = rec.UnitName;

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.UpdateUser;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = rec.ClientCode;
            tranLog.SoNumber = rec.SoNumber;
            tranLog.PoID = rec.PoId;
            tranLog.CustomerPoNumber = rec.CustomerPoNumber;
            tranLog.AltItemNumber = rec.AltItemNumber;
            tranLog.LotFlag = rec.LotFlag;
            tranLog.ItemId = rec.ItemId;
            tranLog.UnitID = rec.UnitId;
            tranLog.UnitName = rec.UnitName;
            tranLog.TranQty = rec.Qty;
            tranLog.HuId = rec.HuId;
            tranLog.Length = rec.Length;
            tranLog.Width = rec.Width;
            tranLog.Height = rec.Height;
            tranLog.Weight = rec.Weight;
            tranLog.LotNumber1 = rec.LotNumber1;
            tranLog.LotNumber2 = rec.LotNumber2;
            tranLog.LotDate = rec.LotDate;
            tranLog.ReceiptId = rec.ReceiptId;
            tranLog.ReceiptDate = rec.ReceiptDate;

            string result = "";

            List<HuDetail> hudetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId && u.SoNumber == rec.SoNumber && u.ClientCode == rec.ClientCode && u.CustomerPoNumber == rec.CustomerPoNumber && u.AltItemNumber == rec.AltItemNumber && u.ItemId == rec.ItemId && u.UnitName == rec.UnitName && u.Qty == rec.Qty && u.Length == rec.Length && u.Height == rec.Height && u.Weight == rec.Weight && u.Width == rec.Width && u.ReceiptId == rec.ReceiptId);
            if (hudetailList.Count > 0)
            {
                foreach (var item in hudetailList)
                {
                    if ((item.PlanQty ?? 0) > 0)
                    {
                        result = "库存已释放Load，无法修改！";
                        break;
                    }
                }
            }

            List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId);
            if (huMasterList.Count > 0)
            {
                List<WhLocation> whlocationList = idal.IWhLocationDAL.SelectBy(u => u.WhCode == rec.WhCode && u.LocationTypeId == 3);
                if (whlocationList.Count > 0)
                {
                    string[] location = (from a in whlocationList
                                         select a.LocationId).ToList().Distinct().ToArray();

                    HuMaster huMaster = huMasterList.First();
                    if (location.Contains(huMaster.Location))
                    {
                        result = "当前库存已备货，无法修改！";
                    }
                }
            }

            if (result != "")
            {
                return result;
            }


            if (hudetailList.Count > 0)
            {
                if (entity.Qty != 0)
                {
                    HuDetail editHuDetail = hudetailList.First();
                    editHuDetail.Qty = entity.Qty;
                    editHuDetail.Length = entity.Length;
                    editHuDetail.Width = entity.Width;
                    editHuDetail.Height = entity.Height;
                    editHuDetail.Weight = entity.Weight;
                    editHuDetail.UnitName = entity.UnitName;
                    editHuDetail.UpdateUser = entity.UpdateUser;
                    editHuDetail.UpdateDate = DateTime.Now;
                    idal.IHuDetailDAL.UpdateBy(editHuDetail, u => u.Id == editHuDetail.Id, new string[] { "Qty", "Length", "Width", "Height", "Weight", "UpdateUser", "UpdateDate", "UnitName" });
                }
                else
                {
                    idal.IHuDetailDAL.DeleteByExtended(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId && u.SoNumber == rec.SoNumber && u.ClientCode == rec.ClientCode && u.CustomerPoNumber == rec.CustomerPoNumber && u.AltItemNumber == rec.AltItemNumber && u.ItemId == rec.ItemId && u.UnitName == rec.UnitName && u.Qty == rec.Qty && u.Length == rec.Length && u.Height == rec.Height && u.Weight == rec.Weight && u.Width == rec.Width && u.ReceiptId == rec.ReceiptId);

                    idal.ISerialNumberInDAL.DeleteByExtended(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId && u.SoNumber == rec.SoNumber && u.ClientCode == rec.ClientCode && u.CustomerPoNumber == rec.CustomerPoNumber && u.AltItemNumber == rec.AltItemNumber && u.ItemId == rec.ItemId && u.Length == rec.Length && u.Height == rec.Height && u.Width == rec.Width && u.ReceiptId == rec.ReceiptId);

                    idal.IPhotoMasterDAL.DeleteByExtended(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId && u.Number2 == rec.SoNumber && u.ClientCode == rec.ClientCode && u.Number3 == rec.CustomerPoNumber && u.Number4 == rec.AltItemNumber && u.Qty == rec.Qty && u.HoldReason == rec.HoldReason && u.Number == rec.ReceiptId);

                    List<HuDetail> checkHuDetail = idal.IHuDetailDAL.SelectBy(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId);
                    if (checkHuDetail.Count == 0)
                    {
                        idal.IHuMasterDAL.DeleteByExtended(u => u.WhCode == rec.WhCode && u.HuId == rec.HuId);
                    }
                }
            }

            if (entity.Qty != 0)
            {
                tranLog.TranQty2 = entity.Qty;
                tranLog.TranType = "110";
                tranLog.Description = "收货修改";
                tranLog.Remark = "修改为长:" + entity.Length + "宽:" + entity.Width + "高:" + entity.Height;

                Receipt editRec = new Receipt();
                editRec.Qty = entity.Qty;
                editRec.Length = entity.Length;
                editRec.Width = entity.Width;
                editRec.Height = entity.Height;
                editRec.Weight = entity.Weight;
                editRec.UnitName = entity.UnitName;
                editRec.UpdateUser = entity.UpdateUser;
                editRec.UpdateDate = DateTime.Now;
                idal.IReceiptDAL.UpdateBy(editRec, u => u.Id == entity.Id, new string[] { "Qty", "Length", "Width", "Height", "Weight", "UnitName" });

                List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId && u.PoId == rec.PoId && u.ItemId == rec.ItemId);
                if (regDetailList.Count > 0)
                {
                    ReceiptRegisterDetail regDetail = regDetailList.First();
                    if (entity.UnitName != regDetail.UnitName)
                    {
                        if (regDetail.RegQty == entity.Qty)
                        {
                            regDetail.UnitName = entity.UnitName;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(regDetail, u => u.Id == regDetail.Id, new string[] { "UnitName" });
                        }
                    }
                }

            }
            else
            {
                tranLog.TranType = "120";
                tranLog.Description = "收货删除";
                idal.IReceiptDAL.DeleteBy(u => u.Id == entity.Id);
            }
            idal.ITranLogDAL.Add(tranLog);
            idal.IReceiptDAL.SaveChanges();

            List<Receipt> CheckReceiptList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == rec.ReceiptId && u.WhCode == rec.WhCode);
            if (CheckReceiptList.Count == 0)
            {
                idal.IReceiptRegisterDAL.UpdateByExtended(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId, u => new ReceiptRegister() { Status = "U", RecSumCBM = 0, RecSumQty = 0, RecSumWeight = 0 });
            }
            else
            {
                decimal sumCbm = Convert.ToDecimal(CheckReceiptList.Sum(u => (u.UnitName.Contains("ECH") ? 0 : u.Qty) * u.Length * u.Width * u.Height).ToString());
                int sumQty = Convert.ToInt32(CheckReceiptList.Sum(u => u.Qty).ToString());
                decimal sumWeight = Convert.ToDecimal(CheckReceiptList.Sum(u => u.Qty * u.Weight).ToString());

                List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == rec.ReceiptId && u.WhCode == rec.WhCode);

                if (regList.Count > 0)
                {
                    ReceiptRegister reg = regList.First();
                    reg.RecSumCBM = sumCbm;
                    reg.RecSumQty = sumQty;
                    reg.RecSumWeight = sumWeight;
                    idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "RecSumCBM", "RecSumQty", "RecSumWeight" });
                }
            }

            //最后调整工人工作量
            decimal? qty = 0;    //总数量
            decimal? cbm = 0;    //总体积
            decimal? weight = 0; //总重量

            List<Receipt> ReceiptList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == rec.ReceiptId && u.WhCode == rec.WhCode && u.HuId == rec.HuId);

            if (ReceiptList.Count > 0)
            {
                foreach (var item in ReceiptList)
                {
                    qty += item.Qty;
                    cbm += (item.UnitName.Contains("ECH") ? 0 : item.Qty) * (item.Length) * (item.Height) * (item.Width);
                    weight += item.Weight;
                }

                List<WorkloadAccount> WorkloadAccountList = idal.IWorkloadAccountDAL.SelectBy(u => u.ReceiptId == rec.ReceiptId && u.WhCode == rec.WhCode && u.HuId == rec.HuId);

                //先得到工种
                List<string> list = (from a in WorkloadAccountList
                                     select a.WorkType
                                     ).Distinct().ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    string work = (list[i]);

                    List<WorkloadAccount> s = WorkloadAccountList.Where(u => u.WorkType == work).ToList();
                    int count = s.Count;
                    if (count == 0)
                    {
                        count = 1;
                    }
                    decimal? avgCbm = cbm / count;
                    decimal? avgQty = qty / count;
                    decimal? avgWeight = weight / count;

                    if (checkUnitName.Contains("ECH"))
                    {
                        idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId && u.HuId == rec.HuId && u.WorkType == work, u => new WorkloadAccount() { Qty = (decimal)avgQty, CBM = (decimal)avgCbm, Weight = (decimal)avgWeight, EchFlag = 1 });
                    }
                    else
                    {
                        idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId && u.HuId == rec.HuId && u.WorkType == work, u => new WorkloadAccount() { Qty = (decimal)avgQty, CBM = (decimal)avgCbm, Weight = (decimal)avgWeight, EchFlag = 0 });
                    }

                }
            }
            else
            {
                idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == rec.WhCode && u.ReceiptId == rec.ReceiptId && u.HuId == rec.HuId);
            }

            idal.IReceiptDAL.SaveChanges();
            return "Y";
        }

        //收货修改客户定制
        public string ReceiptEditDetailCustom(Receipt entity)
        {
            Receipt rec = idal.IReceiptDAL.SelectBy(u => u.Id == entity.Id).First();

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "520";
            tranLog.Description = "收货修改客户定制";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.UpdateUser;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = rec.ClientCode;
            tranLog.SoNumber = rec.SoNumber;
            tranLog.PoID = rec.PoId;
            tranLog.CustomerPoNumber = rec.CustomerPoNumber;
            tranLog.AltItemNumber = rec.AltItemNumber;
            tranLog.LotFlag = rec.LotFlag;
            tranLog.ItemId = rec.ItemId;
            tranLog.UnitID = rec.UnitId;
            tranLog.UnitName = rec.UnitName;
            tranLog.TranQty = rec.Qty;
            tranLog.HuId = rec.HuId;
            tranLog.Length = rec.Length;
            tranLog.Width = rec.Width;
            tranLog.Height = rec.Height;
            tranLog.Weight = rec.Weight;
            tranLog.LotNumber1 = rec.LotNumber1;
            tranLog.LotNumber2 = rec.LotNumber2;
            tranLog.LotDate = rec.LotDate;
            tranLog.ReceiptId = rec.ReceiptId;
            tranLog.ReceiptDate = rec.ReceiptDate;
            tranLog.Remark = rec.Custom1 + "/" + rec.Custom2 + "/" + rec.Custom3;
            idal.ITranLogDAL.Add(tranLog);

            Receipt editRec = new Receipt();
            editRec.Custom1 = entity.Custom1;
            editRec.Custom2 = entity.Custom2;
            editRec.Custom3 = entity.Custom3;
            idal.IReceiptDAL.UpdateBy(editRec, u => u.Id == entity.Id, new string[] { "Custom1", "Custom2", "Custom3" });

            if (rec.WhCode == "02" && rec.ClientCode == "DM")
            {
                List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == 18).ToList();
                if (urlList.Count > 0)
                {
                    UrlEdi url = urlList.First();

                    UrlEdiTask uet = new UrlEdiTask();
                    uet.WhCode = rec.WhCode;
                    uet.Type = "OMS";
                    uet.Url = url.Url + "&WhCode=" + rec.WhCode;
                    uet.Field = url.Field;
                    uet.Mark = rec.ReceiptId;
                    uet.HttpType = url.HttpType;
                    uet.Status = 1;
                    uet.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet);
                }
            }

            idal.IReceiptDAL.SaveChanges();
            return "Y";
        }

        //收货异常原因列表
        public List<HoldMaster> HoldMasterListByRec(HoldMasterSearch searchEntity, out int total)
        {
            if (!string.IsNullOrEmpty(searchEntity.ClientCode) && (searchEntity.ClientCode ?? "") != "")
            {
                List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.ClientCode == searchEntity.ClientCode);
                if (clientList.Count > 0)
                {
                    searchEntity.ClientId = clientList.First().Id;
                }
            }

            var sql = from a in idal.IHoldMasterDAL.SelectAll()
                      where a.ReasonType == "TCR" && a.WhCode == searchEntity.WhCode &&
                      (a.ClientId == searchEntity.ClientId || a.ClientCode == "all")
                      select a;

            total = sql.Count();
            sql = sql.OrderBy(u => u.Sequence);
            if (searchEntity.pageSize != 0 && searchEntity.pageIndex != 0) sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //验证实收数量与登记数量
        public string CheckReceiptQty(ReceiptInsert entity, int PoId)
        {
            bool CheckResult = true;            //验证结果
            string mess = "";

            try
            {
                //得到登记数据
                var regSql = from receiptregisterdetail in idal.IReceiptRegisterDetailDAL.SelectAll()
                             where
                               receiptregisterdetail.ReceiptId == entity.ReceiptId &&
                               receiptregisterdetail.WhCode == entity.WhCode &&
                               receiptregisterdetail.PoId == PoId
                             select new CheckRecModel
                             {
                                 ItemId = receiptregisterdetail.ItemId,
                                 UnitName = receiptregisterdetail.UnitName,
                                 Qty = receiptregisterdetail.RegQty
                             };

                if (regSql.Count() == 0)
                {
                    return "错误！没有找到收货登记信息！";
                }

                List<CheckRecModel> listRegModel = new List<CheckRecModel>();    //登记数据
                List<CheckRecModel> listRfModel = new List<CheckRecModel>();     //预收数据
                List<CheckRecModel> listRecModel = new List<CheckRecModel>();    //实收数据

                //查询数据库得到实收数据
                var recSql = from receipt in idal.IReceiptDAL.SelectAll()
                             where
                               receipt.ReceiptId == entity.ReceiptId &&
                               receipt.WhCode == entity.WhCode &&
                               receipt.PoId == PoId &&
                               receipt.SoNumber == (entity.SoNumber ?? "")
                             group receipt by new
                             {
                                 receipt.ItemId,
                                 receipt.UnitName
                             } into g
                             select new CheckRecModel
                             {
                                 ItemId = g.Key.ItemId,
                                 UnitName = g.Key.UnitName,
                                 Qty = g.Sum(p => p.Qty)
                             };

                //循环实体
                foreach (var item in entity.RecModeldetail)
                {
                    if (item.Qty != 0)
                    {
                        if (item.SerialNumberInModel != null)
                        {
                            if (item.Qty != item.SerialNumberInModel.Count && item.SerialNumberInModel.Count != 0)
                            {
                                mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                break;
                            }
                        }
                    }

                    //得到登记数量
                    var sql_reg = from a in listRegModel
                                  where a.ItemId == item.ItemId
                                  select a;
                    if (sql_reg.Count() == 0)
                    {
                        var reg1 = (from a in regSql
                                    where a.ItemId == item.ItemId && a.UnitName == "none"
                                    select a);
                        CheckRecModel model = new CheckRecModel();
                        if (reg1.Count() > 0)
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == "none"
                                     select a).First();
                        }
                        else
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                     select a).First();
                        }

                        listRegModel.Add(model);
                    }

                    //得到预收数量总和
                    var sql = from a in listRfModel
                              where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                              select a;
                    if (sql.Count() == 0)
                    {
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.UnitName = item.UnitName;
                        checkRecModel.Qty = item.Qty;
                        listRfModel.Add(checkRecModel);
                    }
                    else
                    {
                        CheckRecModel model = listRfModel.Where(u => u.ItemId == item.ItemId).First();
                        listRfModel.Remove(model);
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.UnitName = item.UnitName;
                        checkRecModel.Qty = item.Qty + model.Qty;
                        listRfModel.Add(checkRecModel);
                    }

                    //得到实收数量与预收数量的总和
                    var sql_rec1 = from a in recSql
                                   where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                   select a;
                    var sql_rec2 = from a in listRecModel
                                   where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                   select a;
                    if (sql_rec2.Count() == 0)
                    {
                        if (sql_rec1.Count() > 0)
                        {
                            CheckRecModel model = sql_rec1.Where(u => u.ItemId == item.ItemId).First();
                            model.Qty = model.Qty + item.Qty;
                            listRecModel.Add(model);
                        }
                    }
                    else
                    {
                        CheckRecModel getRecModel = sql_rec2.Where(u => u.ItemId == item.ItemId).First();
                        listRecModel.Remove(getRecModel);
                        CheckRecModel model = new CheckRecModel();
                        model.ItemId = item.ItemId;
                        model.UnitName = item.UnitName;
                        model.Qty = item.Qty + getRecModel.Qty;
                        listRecModel.Add(model);
                    }
                }
                if (mess != "")
                {
                    return mess;
                }

                if (listRfModel.Where(u => u.Qty == 0).Count() > 0)
                {
                    mess = "错误！托盘所收数量必须大于0！";
                }
                if (mess != "")
                {
                    return mess;
                }

                //如果有实收List 表示有实收 
                //比较实收 与登记
                if (listRecModel.Count > 0)
                {
                    foreach (var item in listRecModel)
                    {
                        if (CheckResult)
                        {
                            int getQty = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a).First().Qty;
                            if (item.Qty > getQty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                    }
                }

                if (mess != "")
                {
                    return mess;
                }

                //比较 预收与登记
                foreach (var item in listRfModel)
                {
                    if (CheckResult)
                    {
                        var s = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == "none" select a);
                        if (s.Count() > 0)
                        {
                            if (item.Qty > s.First().Qty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                        else
                        {
                            var s1 = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a);
                            if (s1.Count() == 0)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收单位与收货登记单位不一致，请删除重收！";
                            }
                            else
                            {
                                int getQty = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a).First().Qty;
                                if (item.Qty > getQty)
                                {
                                    CheckResult = false;
                                    mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                mess = "错误！数据比较异常！";
            }
            if (mess != "")
            {
                return mess;
            }

            //直装收货 需要验证 所收Lot 是否等于直装出库单Lot



            return "";
        }


        //验证实收数量与登记数量 款号可变形
        public string CheckReceiptQtyByItemBX(ReceiptInsert entity, int PoId)
        {
            bool CheckResult = true;            //验证结果
            string mess = "";

            try
            {
                //得到登记数据
                var regSql = from receiptregisterdetail in idal.IReceiptRegisterDetailDAL.SelectAll()
                             where
                               receiptregisterdetail.ReceiptId == entity.ReceiptId &&
                               receiptregisterdetail.WhCode == entity.WhCode &&
                               receiptregisterdetail.PoId == PoId
                             select new CheckRecModel
                             {
                                 ItemId = receiptregisterdetail.ItemId,
                                 UnitName = receiptregisterdetail.UnitName,
                                 Qty = receiptregisterdetail.RegQty
                             };

                if (regSql.Count() == 0)
                {
                    return "错误！没有找到收货登记信息！";
                }

                List<CheckRecModel> listRegModel = new List<CheckRecModel>();    //登记数据
                List<CheckRecModel> listRfModel = new List<CheckRecModel>();     //预收数据
                List<CheckRecModel> listRecModel = new List<CheckRecModel>();    //实收数据

                //查询数据库得到实收数据
                var recSql = from receipt in idal.IReceiptDAL.SelectAll()
                             where
                               receipt.ReceiptId == entity.ReceiptId &&
                               receipt.WhCode == entity.WhCode &&
                               receipt.PoId == PoId &&
                               receipt.SoNumber == (entity.SoNumber ?? "")
                             group receipt by new
                             {
                                 receipt.ItemId
                             } into g
                             select new CheckRecModel
                             {
                                 ItemId = g.Key.ItemId,
                                 Qty = g.Sum(p => p.Qty)
                             };

                //循环实体
                foreach (var item in entity.RecModeldetail)
                {
                    if (item.Qty != 0)
                    {
                        if (item.SerialNumberInModel != null)
                        {
                            if (item.Qty != item.SerialNumberInModel.Count && item.SerialNumberInModel.Count != 0)
                            {
                                mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                break;
                            }
                        }
                    }

                    //得到登记数量
                    var sql_reg = from a in listRegModel
                                  where a.ItemId == item.ItemId
                                  select a;
                    if (sql_reg.Count() == 0)
                    {
                        var reg1 = (from a in regSql
                                    where a.ItemId == item.ItemId && a.UnitName == "none"
                                    select a);
                        CheckRecModel model = new CheckRecModel();
                        if (reg1.Count() > 0)
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == "none"
                                     select a).First();
                        }
                        else
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId
                                     select a).First();
                        }

                        listRegModel.Add(model);
                    }

                    //得到预收数量总和
                    var sql = from a in listRfModel
                              where a.ItemId == item.ItemId
                              select a;
                    if (sql.Count() == 0)
                    {
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.Qty = item.Qty;
                        listRfModel.Add(checkRecModel);
                    }
                    else
                    {
                        CheckRecModel model = listRfModel.Where(u => u.ItemId == item.ItemId).First();
                        listRfModel.Remove(model);
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.Qty = item.Qty + model.Qty;
                        listRfModel.Add(checkRecModel);
                    }

                    //得到实收数量与预收数量的总和
                    var sql_rec1 = from a in recSql
                                   where a.ItemId == item.ItemId
                                   select a;
                    var sql_rec2 = from a in listRecModel
                                   where a.ItemId == item.ItemId
                                   select a;
                    if (sql_rec2.Count() == 0)
                    {
                        if (sql_rec1.Count() > 0)
                        {
                            CheckRecModel model = sql_rec1.Where(u => u.ItemId == item.ItemId).First();
                            model.Qty = model.Qty + item.Qty;
                            listRecModel.Add(model);
                        }
                    }
                    else
                    {
                        CheckRecModel getRecModel = sql_rec2.Where(u => u.ItemId == item.ItemId).First();
                        listRecModel.Remove(getRecModel);
                        CheckRecModel model = new CheckRecModel();
                        model.ItemId = item.ItemId;
                        model.Qty = item.Qty + getRecModel.Qty;
                        listRecModel.Add(model);
                    }
                }
                if (mess != "")
                {
                    return mess;
                }

                if (listRfModel.Where(u => u.Qty == 0).Count() > 0)
                {
                    mess = "错误！托盘所收数量必须大于0！";
                }
                if (mess != "")
                {
                    return mess;
                }

                //如果有实收List 表示有实收 
                //比较实收 与登记
                if (listRecModel.Count > 0)
                {
                    foreach (var item in listRecModel)
                    {
                        if (CheckResult)
                        {
                            int getQty = (from a in listRegModel where a.ItemId == item.ItemId select a).First().Qty;
                            if (item.Qty > getQty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                    }
                }

                if (mess != "")
                {
                    return mess;
                }

                //比较 预收与登记
                foreach (var item in listRfModel)
                {
                    if (CheckResult)
                    {
                        var s = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == "none" select a);
                        if (s.Count() > 0)
                        {
                            if (item.Qty > s.First().Qty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                        else
                        {
                            var s1 = (from a in listRegModel where a.ItemId == item.ItemId select a);
                            if (s1.Count() == 0)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数据与收货登记数据不一致，请删除重收！";
                            }
                            else
                            {
                                int getQty = (from a in listRegModel where a.ItemId == item.ItemId select a).First().Qty;
                                if (item.Qty > getQty)
                                {
                                    CheckResult = false;
                                    mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                mess = "错误！数据比较异常！";
            }
            if (mess != "")
            {
                return mess;
            }

            //直装收货 需要验证 所收Lot 是否等于直装出库单Lot



            return "";
        }


        //插入收货信息
        public string ReceiptInsert(ReceiptInsert entity)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 收货优化
                    if (entity.RecModeldetail == null)
                    {
                        return "错误！没有货物明细！";
                    }

                    //1.首先验证全部数据是否满足
                    if (!recHelper.CheckReceiptId(entity.ReceiptId, entity.WhCode))
                    {
                        return "错误！收货批次号有误或不存在！";
                    }

                    ReceiptRegister checkReg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId).First();
                    if (checkReg.DSFLag == 1)
                    {
                        if (!recHelper.CheckOutLocation(entity.WhCode, entity.Location))
                        {
                            return "错误！直装操作单应输入备货门区！";
                        }
                    }
                    else
                    {
                        if (!recHelper.CheckRecLocation(entity.WhCode, entity.Location))
                        {
                            return "错误！收货门区有误！";
                        }
                    }

                    if (!recHelper.CheckSoPo(entity.ReceiptId, entity.WhCode, entity.SoNumber, entity.CustomerPoNumber))
                    {
                        return "错误！SO或PO输入有误！";
                    }
                    if (!recHelper.CheckPlt(entity.WhCode, entity.HuId))
                    {
                        return "错误！托盘不存在或已使用！";
                    }

                    bool recDetailResult = true;

                    List<RecModeldetail> listRecDetail = new List<RecModeldetail>();
                    foreach (var item in entity.RecModeldetail)
                    {
                        if (recDetailResult)
                        {
                            if ((from a in listRecDetail where a.ItemId == item.ItemId && a.UnitId == item.UnitId && a.Length == item.Length && a.Width == item.Width && a.Height == item.Height && a.LotNumber1 == item.LotNumber1 && a.LotNumber2 == item.LotNumber2 && a.LotDate == item.LotDate select a).Count() == 0)
                            {
                                RecModeldetail recDetail = new RecModeldetail();
                                recDetail.ItemId = item.ItemId;
                                recDetail.UnitId = item.UnitId;
                                recDetail.Length = item.Length;
                                recDetail.Width = item.Width;
                                recDetail.Height = item.Height;
                                recDetail.LotNumber1 = item.LotNumber1;
                                recDetail.LotNumber2 = item.LotNumber2;
                                recDetail.LotDate = item.LotDate;
                                recDetail.Attribute1 = item.Attribute1;
                                listRecDetail.Add(recDetail);
                            }
                            else
                            {
                                recDetailResult = false;
                            }
                        }
                    }
                    if (recDetailResult == false)
                    {
                        return "错误！货物明细重复或异常！";
                    }

                    string result = "";
                    List<int> ItemList = new List<int>();

                    List<string> checksearNumberList = new List<string>();
                    foreach (var item in entity.RecModeldetail)
                    {
                        if (!ItemList.Contains(item.ItemId))
                        {
                            ItemList.Add(item.ItemId);
                        }

                        string s = CheckSerialNumberList(entity, item, checksearNumberList);
                        if (s != "Y")
                        {
                            result = s;
                            break;
                        }
                    }

                    if (result != "")
                    {
                        return result;
                    }


                    if (!recHelper.CheckSku(entity.ReceiptId, entity.WhCode, ItemList, entity.CustomerPoNumber))
                    {
                        return "错误！款号有误或不存在！";
                    }
                    if (!recHelper.CheckSku(entity.WhCode, ItemList))
                    {
                        return "错误！款号有误或不存在！";
                    }

                    //验证该收货 是否选择 收货单位(可变)流程    
                    List<BusinessFlowHead> checkRFRule = (from c in idal.IReceiptRegisterDAL.SelectAll()
                                                          join a in idal.IFlowDetailDAL.SelectAll() on new { ProcessId = (Int32)c.ProcessId } equals new { ProcessId = a.FlowHeadId } into a_join
                                                          from a in a_join.DefaultIfEmpty()
                                                          join b in idal.IBusinessFlowHeadDAL.SelectAll() on new { BusinessObjectGroupId = (Int32)a.BusinessObjectGroupId } equals new { BusinessObjectGroupId = b.GroupId } into b_join
                                                          from b in b_join.DefaultIfEmpty()
                                                          where c.ReceiptId == entity.ReceiptId && c.WhCode == entity.WhCode
                                                          select b).ToList();

                    //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                    //绑定了后台数据的主键ID， 请勿随意更改
                    if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                    {
                        if (!recHelper.CheckUnit(entity.WhCode, ItemList, entity))
                        {
                            return "错误！款号对应的单位有误！";
                        }
                    }

                    if (recHelper.CheckSkuId(entity.WhCode, entity.RecModeldetail, entity.ClientId) == false)
                    {
                        return "错误！款号所扫描的ID有误！";
                    }

                    int SoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);
                        SoId = soList.First().Id;
                    }

                    int PoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }
                    else
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }

                    //2.验证实收数量 预收数量与登记数量是否有差异

                    //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                    //绑定了后台数据的主键ID， 请勿随意更改
                    if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                    {
                        string checkReceiptQtyResult = CheckReceiptQty(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }
                    else
                    {
                        //款号可变形的验证
                        string checkReceiptQtyResult = CheckReceiptQtyByItemBX(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }

                    //3.开始插入数据
                    List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
                    ReceiptRegister reg = regList.First();

                    Receipt rec = null;
                    decimal? qty = 0;    //总数量
                    decimal? cbm = 0;    //总体积
                    decimal? weight = 0; //总重量


                    List<InBoundOrderDetail> inboundOrderDetailList = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.PoId == PoId);
                    List<ReceiptRegisterDetail> receiptRegDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId);

                    foreach (var item in entity.RecModeldetail)
                    {
                        //插入实收表
                        rec = new Receipt();
                        rec.WhCode = entity.WhCode;
                        rec.RegId = reg.Id;
                        rec.ReceiptId = entity.ReceiptId;
                        rec.ClientId = entity.ClientId;
                        rec.ClientCode = entity.ClientCode;
                        rec.ReceiptDate = DateTime.Now;
                        if (entity.HoldMasterModel != null)
                        {
                            if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                            {
                                rec.Status = "A";
                            }
                            else
                            {
                                rec.Status = "H";
                                rec.HoldReason = entity.HoldMasterModel.HoldReason;
                            }
                        }
                        else
                        {
                            rec.Status = "A";
                        }
                        rec.SoNumber = entity.SoNumber;
                        rec.PoId = PoId;
                        rec.CustomerPoNumber = entity.CustomerPoNumber;
                        rec.HuId = entity.HuId;
                        rec.LotFlag = entity.LotFlag;
                        rec.CreateUser = entity.CreateUser;
                        rec.CreateDate = DateTime.Now;

                        rec.AltItemNumber = item.AltItemNumber;
                        rec.ItemId = item.ItemId;
                        rec.UnitId = item.UnitId;
                        rec.UnitName = item.UnitName;
                        rec.Qty = item.Qty;
                        rec.Length = item.Length / 100;
                        rec.Width = item.Width / 100;
                        rec.Height = item.Height / 100;
                        rec.Weight = item.Weight;
                        rec.LotNumber1 = item.LotNumber1;
                        rec.LotNumber2 = item.LotNumber2;
                        rec.LotDate = item.LotDate;
                        rec.Attribute1 = item.Attribute1;

                        rec.Custom1 = item.Custom1;
                        rec.Custom2 = item.Custom2;
                        rec.Custom3 = item.Custom3;

                        idal.IReceiptDAL.Add(rec);

                        qty += item.Qty;
                        cbm += (item.UnitName.Contains("ECH") ? 0 : item.Qty) * (item.Length / 100) * (item.Height / 100) * (item.Width / 100);
                        weight += item.Weight;


                        ItemMaster getItemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();
                        //2.如果选择了正常的收货单位  
                        //验证单位 且修改单位
                        if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                        {
                            if (getItemMaster.UnitFlag == 0 && getItemMaster.UnitName == "none")
                            {
                                ItemMaster item1 = new ItemMaster();
                                item1.UnitName = item.UnitName;
                                item1.UpdateUser = entity.CreateUser;
                                item1.UpdateDate = DateTime.Now;
                                idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            //InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.ItemId == item.ItemId && u.PoId == PoId && u.WhCode == entity.WhCode).First();

                            InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                            if (inOrderDetail.UnitName == "none")
                            {
                                inOrderDetail.UnitId = item.UnitId;
                                inOrderDetail.UnitName = item.UnitName;
                                inOrderDetail.UpdateUser = entity.CreateUser;
                                inOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            //ReceiptRegisterDetail receiptRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId && u.ItemId == item.ItemId).First();

                            ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                            if (receiptRegDetail.UnitName == "none")
                            {
                                receiptRegDetail.UnitId = item.UnitId;
                                receiptRegDetail.UnitName = item.UnitName;
                                receiptRegDetail.UpdateUser = entity.CreateUser;
                                receiptRegDetail.UpdateDate = DateTime.Now;
                                idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }
                        }
                        else
                        {
                            ItemMaster item1 = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                            item1.UnitName = item.UnitName;
                            item1.UpdateUser = entity.CreateUser;
                            item1.UpdateDate = DateTime.Now;
                            idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });

                            //InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.ItemId == item.ItemId && u.PoId == PoId && u.WhCode == entity.WhCode).First();

                            InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                            inOrderDetail.UnitId = item.UnitId;
                            inOrderDetail.UnitName = item.UnitName;
                            inOrderDetail.UpdateUser = entity.CreateUser;
                            inOrderDetail.UpdateDate = DateTime.Now;
                            idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });

                            //ReceiptRegisterDetail receiptRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId && u.ItemId == item.ItemId).First();

                            ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                            receiptRegDetail.UnitId = item.UnitId;
                            receiptRegDetail.UnitName = item.UnitName;
                            receiptRegDetail.UpdateUser = entity.CreateUser;
                            receiptRegDetail.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                        }


                        if (item.SerialNumberInModel != null)
                        {
                            SerialNumberInsert(entity, PoId, item);     //添加采集箱号
                        }
                    }

                    //更改收货批次号状态
                    ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
                    if (receiptRegister.Status == "U" || receiptRegister.Status == "P" || entity.TransportTypeEdit == "1")
                    {
                        if (receiptRegister.BeginReceiptDate == null)
                        {
                            ReceiptRegister receiptRegister1 = new ReceiptRegister();
                            receiptRegister1.BeginReceiptDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister1, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "BeginReceiptDate" });
                        }
                        receiptRegister.Status = "A";

                        if (receiptRegister.TransportType == null || receiptRegister.TransportType == "" || entity.TransportTypeEdit == "1")
                        {
                            receiptRegister.TransportType = entity.TransportType;
                            receiptRegister.TransportTypeExtend = entity.TransportTypeExtend;
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "TransportType", "UpdateUser", "UpdateDate", "TransportTypeExtend" });
                        }
                        else
                        {
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
                        }

                        //正在收货时 验证 是否是拦截订单
                        if (receiptRegister.HoldOutBoundOrderId != 0)
                        {
                            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == receiptRegister.HoldOutBoundOrderId);
                            if (OutBoundOrderList.Count != 0)
                            {
                                OutBoundOrder outBoundOrder = OutBoundOrderList.First();

                                outBoundOrder.StatusName = "已拦截正在收货";
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName" });
                            }
                        }

                    }

                    if (entity.WorkloadAccountModel != null)
                    {
                        WorkloadAccountInsert(entity, qty, cbm, weight);    //添加工人工作量
                    }

                    List<RecModeldetail> checkEch = entity.RecModeldetail.Where(u => u.UnitName.Contains("ECH")).Distinct().ToList();

                    //插入理货员
                    WorkloadAccount work1 = new WorkloadAccount();
                    work1.WhCode = entity.WhCode;
                    work1.ClientId = entity.ClientId;
                    work1.ClientCode = entity.ClientCode;
                    work1.ReceiptId = entity.ReceiptId;
                    work1.HuId = entity.HuId;
                    work1.WorkType = "理货员";
                    work1.UserCode = entity.CreateUser;
                    work1.LotFlag = entity.LotFlag;
                    work1.ReceiptDate = DateTime.Now;
                    work1.Qty = Convert.ToDecimal(qty);

                    if (checkEch.Count > 0)
                    {
                        work1.EchFlag = 1;
                    }
                    else
                    {
                        work1.EchFlag = 0;
                    }

                    work1.CBM = cbm;
                    work1.Weight = weight;
                    idal.IWorkloadAccountDAL.Add(work1);

                    HuMasterInsert(entity);    //添加库存主表

                    AddReceiptTranLog(entity);//添加收货TranLog
                    idal.IReceiptDAL.SaveChanges();

                    //收货时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有货损托盘
                    List<Receipt> checkReceiptTCRList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.Status == "H" && u.HuId == entity.HuId);
                    if (checkReceiptTCRList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptTCRList)
                        {
                            PhotoMaster photoMaster = new PhotoMaster();

                            photoMaster.PhotoId = 0;

                            photoMaster.WhCode = item.WhCode;
                            photoMaster.ClientCode = item.ClientCode;
                            photoMaster.Number = item.ReceiptId;
                            photoMaster.Number2 = item.SoNumber;
                            photoMaster.Number3 = item.CustomerPoNumber;
                            photoMaster.Number4 = item.AltItemNumber;

                            photoMaster.PoId = item.PoId;
                            photoMaster.ItemId = item.ItemId;
                            photoMaster.UnitName = item.UnitName;
                            photoMaster.Qty = item.Qty;
                            photoMaster.RegQty = 0;
                            photoMaster.HuId = item.HuId;
                            photoMaster.HoldReason = item.HoldReason;
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
                    }
                    #endregion
                    //TCR结束

                    #endregion

                    idal.IReceiptDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string ss = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                    return "收货异常，请重新提交！";
                }
            }
        }

        //验证实收数量与登记数量
        public string CheckReceiptQty(List<ReceiptInsert> entityList)
        {
            string mess = "";

            ReceiptInsert entity = entityList.First();

            //0.验证预收数据 是否为同一收货批次号与托盘号
            if (entityList.Count > 1)
            {
                for (int i = 1; i < entityList.Count; i++)
                {
                    if (entity.WhCode != entityList[i].WhCode || entity.ReceiptId != entityList[i].ReceiptId)
                    {
                        mess = "错误！当前多次所收的收货批次号不一样！";
                        break;
                    }
                }
            }

            if (mess != "")
            {
                return mess;
            }

            try
            {
                //1.得到收货批次的登记数据
                var regSql = from receiptregisterdetail in idal.IReceiptRegisterDetailDAL.SelectAll()
                             where
                               receiptregisterdetail.ReceiptId == entity.ReceiptId &&
                               receiptregisterdetail.WhCode == entity.WhCode
                             select new CheckRecModel
                             {
                                 PoId = receiptregisterdetail.PoId,
                                 ItemId = receiptregisterdetail.ItemId,
                                 UnitName = receiptregisterdetail.UnitName,
                                 Qty = receiptregisterdetail.RegQty
                             };

                if (regSql.Count() == 0)
                {
                    return "错误！没有找到收货登记信息！";
                }

                //2.查询数据库得到实收数据
                var recSql = from receipt in idal.IReceiptDAL.SelectAll()
                             where
                               receipt.ReceiptId == entity.ReceiptId &&
                               receipt.WhCode == entity.WhCode
                             group receipt by new
                             {
                                 receipt.PoId,
                                 receipt.ItemId,
                                 receipt.UnitName
                             } into g
                             select new CheckRecModel
                             {
                                 PoId = g.Key.PoId,
                                 ItemId = g.Key.ItemId,
                                 UnitName = g.Key.UnitName,
                                 Qty = g.Sum(p => p.Qty)
                             };

                //3.合并 SO PO 托盘 是一致的预收数据
                List<ReceiptInsert> checkReceiptInsertList = new List<ReceiptInsert>();
                foreach (var item in entityList)
                {
                    int SoId = 0;
                    if (!string.IsNullOrEmpty(item.SoNumber))
                    {
                        List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == item.SoNumber && u.WhCode == item.WhCode && u.ClientId == item.ClientId);
                        SoId = soList.First().Id;
                    }

                    int PoId = 0;
                    if (!string.IsNullOrEmpty(item.SoNumber))
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId);
                        PoId = orderList.First().Id;
                    }
                    else
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId);
                        PoId = orderList.First().Id;
                    }

                    if (checkReceiptInsertList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.HuId == item.HuId).Count() == 0)
                    {
                        ReceiptInsert addRec = new ReceiptInsert();
                        addRec.WhCode = item.WhCode;
                        addRec.ReceiptId = item.ReceiptId;
                        addRec.SoNumber = item.SoNumber;
                        addRec.CustomerPoNumber = item.CustomerPoNumber;
                        addRec.PoId = PoId;
                        addRec.HuId = item.HuId;

                        addRec.RecModeldetail = item.RecModeldetail;

                        checkReceiptInsertList.Add(addRec);
                    }
                    else
                    {
                        //如果存在SO PO 托盘一致的 需要合并预收数量和款号的数据
                        ReceiptInsert oldRec = checkReceiptInsertList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.HuId == item.HuId).First();

                        List<RecModeldetail> oldRecDetailList = oldRec.RecModeldetail;

                        checkReceiptInsertList.Remove(oldRec);

                        ReceiptInsert addRec = new ReceiptInsert();
                        addRec.WhCode = item.WhCode;
                        addRec.ReceiptId = item.ReceiptId;
                        addRec.SoNumber = item.SoNumber;
                        addRec.CustomerPoNumber = item.CustomerPoNumber;
                        addRec.PoId = PoId;
                        addRec.HuId = item.HuId;

                        List<RecModeldetail> checkRecDetailList = new List<RecModeldetail>();

                        foreach (var recDetail in oldRecDetailList)
                        {
                            if (recDetail.SerialNumberInModel != null)
                            {
                                if (recDetail.Qty != recDetail.SerialNumberInModel.Count && recDetail.SerialNumberInModel.Count > 0)
                                {
                                    mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                    break;
                                }
                            }
                            if (checkRecDetailList.Where(u => u.ItemId == recDetail.ItemId).Count() == 0)
                            {
                                RecModeldetail recModeldetail = new RecModeldetail();
                                recModeldetail.ItemId = recDetail.ItemId;
                                recModeldetail.Qty = recDetail.Qty;
                                recModeldetail.UnitName = recDetail.UnitName;
                                recModeldetail.UnitId = recDetail.UnitId;
                                checkRecDetailList.Add(recModeldetail);
                            }
                            else
                            {
                                RecModeldetail oldrecModeldetail = checkRecDetailList.Where(u => u.ItemId == recDetail.ItemId).First();
                                if (oldrecModeldetail.UnitName != recDetail.UnitName)
                                {
                                    mess = "错误！款号多次所收的单位不一样！";
                                    break;
                                }
                                checkRecDetailList.Remove(oldrecModeldetail);

                                RecModeldetail recModeldetail = new RecModeldetail();
                                recModeldetail.ItemId = recDetail.ItemId;
                                recModeldetail.Qty = recDetail.Qty + oldrecModeldetail.Qty;
                                recModeldetail.UnitName = recDetail.UnitName;
                                recModeldetail.UnitId = recDetail.UnitId;
                                checkRecDetailList.Add(recModeldetail);
                            }
                        }

                        foreach (var recDetail in item.RecModeldetail)
                        {
                            if (recDetail.SerialNumberInModel != null)
                            {
                                if (recDetail.Qty != recDetail.SerialNumberInModel.Count && recDetail.SerialNumberInModel.Count > 0)
                                {
                                    mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                    break;
                                }
                            }
                            if (checkRecDetailList.Where(u => u.ItemId == recDetail.ItemId).Count() == 0)
                            {
                                RecModeldetail recModeldetail = new RecModeldetail();
                                recModeldetail.ItemId = recDetail.ItemId;
                                recModeldetail.Qty = recDetail.Qty;
                                recModeldetail.UnitName = recDetail.UnitName;
                                recModeldetail.UnitId = recDetail.UnitId;
                                checkRecDetailList.Add(recModeldetail);
                            }
                            else
                            {
                                RecModeldetail oldrecModeldetail = checkRecDetailList.Where(u => u.ItemId == recDetail.ItemId).First();
                                if (oldrecModeldetail.UnitName != recDetail.UnitName)
                                {
                                    mess = "错误！款号多次所收的单位不一样！";
                                    break;
                                }
                                checkRecDetailList.Remove(oldrecModeldetail);

                                RecModeldetail recModeldetail = new RecModeldetail();
                                recModeldetail.ItemId = recDetail.ItemId;
                                recModeldetail.Qty = recDetail.Qty + oldrecModeldetail.Qty;
                                recModeldetail.UnitName = recDetail.UnitName;
                                recModeldetail.UnitId = recDetail.UnitId;
                                checkRecDetailList.Add(recModeldetail);
                            }
                        }
                        if (mess != "")
                        {
                            break;
                        }
                        addRec.RecModeldetail = checkRecDetailList;
                        checkReceiptInsertList.Add(addRec);
                    }
                }
                if (mess != "")
                {
                    return mess;
                }

                ///循环预收数据
                ///比较预收+实收 是否大于登记数量
                foreach (var item in checkReceiptInsertList)
                {
                    if (mess != "")
                    {
                        break;
                    }
                    foreach (var recDetail in item.RecModeldetail)
                    {
                        if (regSql.Where(u => u.PoId == item.PoId && u.ItemId == recDetail.ItemId).Count() == 0)
                        {
                            mess = "错误！款号有误或不存在！";
                            break;
                        }
                        else
                        {
                            List<CheckRecModel> regnoneList = regSql.Where(u => u.PoId == item.PoId && u.ItemId == recDetail.ItemId && u.UnitName == "none").ToList();
                            if (regnoneList.Count() > 0)
                            {
                                if (recDetail.Qty > regnoneList.First().Qty)
                                {
                                    mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                    break;
                                }
                            }
                            else
                            {
                                //得到收货登记
                                CheckRecModel regList = regSql.Where(u => u.PoId == item.PoId && u.ItemId == recDetail.ItemId).First();

                                if (recSql.Where(u => u.PoId == item.PoId && u.ItemId == recDetail.ItemId).Count() > 0)
                                {
                                    CheckRecModel recList = recSql.Where(u => u.PoId == item.PoId && u.ItemId == recDetail.ItemId).First();

                                    if (recDetail.Qty + recList.Qty > regList.Qty)
                                    {
                                        mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                        break;
                                    }
                                }
                                else
                                {
                                    if (recDetail.Qty > regList.Qty)
                                    {
                                        mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                        break;
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                mess = "错误！数据比较异常！";
            }

            if (mess != "")
            {
                return mess;
            }

            return "";
        }

        //插入收货信息
        public string ReceiptInsert(List<ReceiptInsert> entityList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 收货优化
                    string mess = "";

                    ReceiptInsert firstEntity = entityList.First();

                    string[] huIdArr = (from a in entityList
                                        select a.HuId).Distinct().ToArray();
                    if (huIdArr.Count() > 1)
                    {
                        return "错误！收货不可使用多个托盘！";
                    }

                    if (!recHelper.CheckReceiptId(firstEntity.ReceiptId, firstEntity.WhCode))
                    {
                        return "错误！收货批次号有误或不存在！";
                    }

                    ReceiptRegister checkReg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == firstEntity.WhCode && u.ReceiptId == firstEntity.ReceiptId).First();
                    if (checkReg.DSFLag == 1)
                    {
                        if (!recHelper.CheckOutLocation(firstEntity.WhCode, firstEntity.Location))
                        {
                            return "错误！直装操作单应输入备货门区！";
                        }
                    }
                    else
                    {
                        if (!recHelper.CheckRecLocation(firstEntity.WhCode, firstEntity.Location))
                        {
                            return "错误！收货门区有误！";
                        }
                    }

                    if (!recHelper.CheckPlt(firstEntity.WhCode, firstEntity.HuId))
                    {
                        return "错误！托盘不存在或已使用！";
                    }

                    string holdReason = "";
                    foreach (var item in entityList)
                    {
                        if (item.HoldMasterModel != null)
                        {
                            holdReason += item.HoldMasterModel.HoldReason + ",";
                        }
                    }
                    if (holdReason != "")
                    {
                        holdReason = holdReason.Substring(0, holdReason.Length - 1);
                        if (holdReason.Length > 100)
                        {
                            return "错误！托盘货损原因超长！";
                        }
                    }

                    List<string> checksearNumberList = new List<string>();


                    //验证该收货 是否选择 收货单位(可变)流程    
                    List<BusinessFlowHead> checkRFRule = (from c in idal.IReceiptRegisterDAL.SelectAll()
                                                          join a in idal.IFlowDetailDAL.SelectAll() on new { ProcessId = (Int32)c.ProcessId } equals new { ProcessId = a.FlowHeadId } into a_join
                                                          from a in a_join.DefaultIfEmpty()
                                                          join b in idal.IBusinessFlowHeadDAL.SelectAll() on new { BusinessObjectGroupId = (Int32)a.BusinessObjectGroupId } equals new { BusinessObjectGroupId = b.GroupId } into b_join
                                                          from b in b_join.DefaultIfEmpty()
                                                          where c.ReceiptId == checkReg.ReceiptId && c.WhCode == checkReg.WhCode
                                                          select b).ToList();

                    //1.首先验证全部数据是否满足
                    foreach (var entity in entityList)
                    {
                        if (entity.RecModeldetail == null)
                        {
                            mess = "错误！没有货物明细！";
                            break;
                        }

                        if (!recHelper.CheckSoPo(entity.ReceiptId, entity.WhCode, entity.SoNumber, entity.CustomerPoNumber))
                        {
                            mess = "错误！SO或PO输入有误！";
                            break;
                        }

                        List<RecModeldetail> listRecDetail = new List<RecModeldetail>();
                        foreach (var item in entity.RecModeldetail)
                        {
                            if (listRecDetail.Where(a => a.ItemId == item.ItemId && a.UnitId == item.UnitId && a.Length == item.Length && a.Width == item.Width && a.Height == item.Height && a.LotNumber1 == item.LotNumber1 && a.LotNumber2 == item.LotNumber2 && a.LotDate == item.LotDate).Count() == 0)
                            {
                                RecModeldetail recDetail = new RecModeldetail();
                                recDetail.ItemId = item.ItemId;
                                recDetail.UnitId = item.UnitId;
                                recDetail.Length = item.Length;
                                recDetail.Width = item.Width;
                                recDetail.Height = item.Height;
                                recDetail.LotNumber1 = item.LotNumber1;
                                recDetail.LotNumber2 = item.LotNumber2;
                                recDetail.LotDate = item.LotDate;
                                recDetail.Attribute1 = item.Attribute1;
                                listRecDetail.Add(recDetail);
                            }
                            else
                            {
                                mess = "错误！货物明细重复或异常！";
                                break;
                            }

                            //验证扫描序列号是否正确
                            string s = CheckSerialNumberList(entity, item, checksearNumberList);
                            if (s != "Y")
                            {
                                mess = s;
                                break;
                            }
                        }

                        List<int> ItemList = new List<int>();
                        foreach (var item in entity.RecModeldetail)
                        {
                            if (!ItemList.Contains(item.ItemId))
                            {
                                ItemList.Add(item.ItemId);
                            }
                        }
                        if (!recHelper.CheckSku(entity.ReceiptId, entity.WhCode, ItemList, entity.CustomerPoNumber))
                        {
                            mess = "错误！款号有误或不存在！";
                            break;
                        }
                        if (!recHelper.CheckSku(entity.WhCode, ItemList))
                        {
                            mess = "错误！款号有误或不存在！";
                            break;
                        }

                        //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                        //绑定了后台数据的主键ID， 请勿随意更改
                        if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                        {
                            if (!recHelper.CheckUnit(entity.WhCode, ItemList, entity))
                            {
                                return "错误！款号对应的单位有误！";
                            }
                        }

                        if (recHelper.CheckSkuId(entity.WhCode, entity.RecModeldetail, entity.ClientId) == false)
                        {
                            mess = "错误！款号所扫描的ID有误！";
                            break;
                        }


                    }
                    if (mess != "")
                    {
                        return mess;
                    }

                    //2.验证实收数量 预收数量与登记数量是否有差异
                    string checkReceiptQtyResult = CheckReceiptQty(entityList);
                    if (checkReceiptQtyResult != "")
                    {
                        return checkReceiptQtyResult;
                    }

                    //3.开始插入数据
                    foreach (var entity in entityList)
                    {
                        int SoId = 0;
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);
                            SoId = soList.First().Id;
                        }

                        int PoId = 0;
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                            PoId = orderList.First().Id;
                        }
                        else
                        {
                            List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                            PoId = orderList.First().Id;
                        }

                        //3.开始插入数据
                        List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
                        ReceiptRegister reg = regList.First();

                        Receipt rec = null;
                        decimal? qty = 0;    //总数量
                        decimal? cbm = 0;    //总体积
                        decimal? weight = 0; //总重量


                        List<InBoundOrderDetail> inboundOrderDetailList = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.PoId == PoId);
                        List<ReceiptRegisterDetail> receiptRegDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId);

                        foreach (var item in entity.RecModeldetail)
                        {
                            //插入实收表
                            rec = new Receipt();
                            rec.WhCode = entity.WhCode;
                            rec.RegId = reg.Id;
                            rec.ReceiptId = entity.ReceiptId;
                            rec.ClientId = entity.ClientId;
                            rec.ClientCode = entity.ClientCode;
                            rec.ReceiptDate = DateTime.Now;
                            if (entity.HoldMasterModel != null)
                            {
                                if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                                {
                                    rec.Status = "A";
                                }
                                else
                                {
                                    rec.Status = "H";
                                    rec.HoldReason = entity.HoldMasterModel.HoldReason;
                                }
                            }
                            else
                            {
                                rec.Status = "A";
                            }
                            rec.SoNumber = entity.SoNumber;
                            rec.PoId = PoId;
                            rec.CustomerPoNumber = entity.CustomerPoNumber;
                            rec.HuId = entity.HuId;
                            rec.LotFlag = entity.LotFlag;
                            rec.CreateUser = entity.CreateUser;
                            rec.CreateDate = DateTime.Now;

                            rec.AltItemNumber = item.AltItemNumber;
                            rec.ItemId = item.ItemId;
                            rec.UnitId = item.UnitId;
                            rec.UnitName = item.UnitName;
                            rec.Qty = item.Qty;
                            rec.Length = item.Length / 100;
                            rec.Width = item.Width / 100;
                            rec.Height = item.Height / 100;
                            rec.Weight = item.Weight;
                            rec.LotNumber1 = item.LotNumber1;
                            rec.LotNumber2 = item.LotNumber2;
                            rec.LotDate = item.LotDate;
                            rec.Attribute1 = item.Attribute1;

                            rec.Custom1 = item.Custom1;
                            rec.Custom2 = item.Custom2;
                            rec.Custom3 = item.Custom3;

                            idal.IReceiptDAL.Add(rec);

                            qty += item.Qty;
                            cbm += (item.UnitName.Contains("ECH") ? 0 : item.Qty) * (item.Length / 100) * (item.Height / 100) * (item.Width / 100);
                            weight += item.Weight;


                            ItemMaster getItemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();
                            //2.如果选择了正常的收货单位  
                            //验证单位 且修改单位
                            if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                            {
                                if (getItemMaster.UnitFlag == 0 && getItemMaster.UnitName == "none")
                                {
                                    ItemMaster item1 = new ItemMaster();
                                    item1.UnitName = item.UnitName;
                                    item1.UpdateUser = entity.CreateUser;
                                    item1.UpdateDate = DateTime.Now;
                                    idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });
                                }

                                InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                                if (inOrderDetail.UnitName == "none")
                                {
                                    inOrderDetail.UnitId = item.UnitId;
                                    inOrderDetail.UnitName = item.UnitName;
                                    inOrderDetail.UpdateUser = entity.CreateUser;
                                    inOrderDetail.UpdateDate = DateTime.Now;
                                    idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                                }

                                ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                                if (receiptRegDetail.UnitName == "none")
                                {
                                    receiptRegDetail.UnitId = item.UnitId;
                                    receiptRegDetail.UnitName = item.UnitName;
                                    receiptRegDetail.UpdateUser = entity.CreateUser;
                                    receiptRegDetail.UpdateDate = DateTime.Now;
                                    idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                                }
                            }
                            else
                            {
                                ItemMaster item1 = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                                item1.UnitName = item.UnitName;
                                item1.UpdateUser = entity.CreateUser;
                                item1.UpdateDate = DateTime.Now;
                                idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });

                                InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                                inOrderDetail.UnitId = item.UnitId;
                                inOrderDetail.UnitName = item.UnitName;
                                inOrderDetail.UpdateUser = entity.CreateUser;
                                inOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });

                                //ReceiptRegisterDetail receiptRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId && u.ItemId == item.ItemId).First();

                                ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                                receiptRegDetail.UnitId = item.UnitId;
                                receiptRegDetail.UnitName = item.UnitName;
                                receiptRegDetail.UpdateUser = entity.CreateUser;
                                receiptRegDetail.UpdateDate = DateTime.Now;
                                idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            if (item.SerialNumberInModel != null)
                            {
                                SerialNumberInsert(entity, PoId, item);     //添加采集箱号
                            }
                        }

                        if (entity.WorkloadAccountModel != null)
                        {
                            WorkloadAccountInsert(entity, qty, cbm, weight);    //添加工人工作量
                        }

                        List<RecModeldetail> checkEch = entity.RecModeldetail.Where(u => u.UnitName.Contains("ECH")).Distinct().ToList();

                        //插入理货员
                        WorkloadAccount work1 = new WorkloadAccount();
                        work1.WhCode = entity.WhCode;
                        work1.ClientId = entity.ClientId;
                        work1.ClientCode = entity.ClientCode;
                        work1.ReceiptId = entity.ReceiptId;
                        work1.HuId = entity.HuId;
                        work1.WorkType = "理货员";
                        work1.UserCode = entity.CreateUser;
                        work1.LotFlag = entity.LotFlag;
                        work1.ReceiptDate = DateTime.Now;
                        work1.Qty = Convert.ToDecimal(qty);

                        if (checkEch.Count > 0)
                        {
                            work1.EchFlag = 1;
                        }
                        else
                        {
                            work1.EchFlag = 0;
                        }
                        work1.CBM = cbm;
                        work1.Weight = weight;
                        idal.IWorkloadAccountDAL.Add(work1);

                        HuMasterInsert(entity, holdReason);    //添加库存主表

                        AddReceiptTranLog(entity);//添加收货TranLog

                        idal.IReceiptDAL.SaveChanges();
                    }

                    //更改收货批次号状态
                    ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == firstEntity.ReceiptId && u.WhCode == firstEntity.WhCode).First();
                    if (receiptRegister.Status == "U" || receiptRegister.Status == "P" || firstEntity.TransportTypeEdit == "1")
                    {
                        if (receiptRegister.BeginReceiptDate == null)
                        {
                            ReceiptRegister receiptRegister1 = new ReceiptRegister();
                            receiptRegister1.BeginReceiptDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister1, u => u.ReceiptId == firstEntity.ReceiptId && u.WhCode == firstEntity.WhCode, new string[] { "BeginReceiptDate" });
                        }
                        receiptRegister.Status = "A";

                        if (receiptRegister.TransportType == null || receiptRegister.TransportType == "" || firstEntity.TransportTypeEdit == "1")
                        {
                            receiptRegister.TransportType = firstEntity.TransportType;
                            receiptRegister.TransportTypeExtend = firstEntity.TransportTypeExtend;
                            receiptRegister.UpdateUser = firstEntity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == firstEntity.ReceiptId && u.WhCode == firstEntity.WhCode, new string[] { "Status", "TransportType", "UpdateUser", "UpdateDate", "TransportTypeExtend" });
                        }
                        else
                        {
                            receiptRegister.UpdateUser = firstEntity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == firstEntity.ReceiptId && u.WhCode == firstEntity.WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
                        }

                        //正在收货时 验证 是否是拦截订单
                        if (receiptRegister.HoldOutBoundOrderId != 0)
                        {
                            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == receiptRegister.HoldOutBoundOrderId);
                            if (OutBoundOrderList.Count != 0)
                            {
                                OutBoundOrder outBoundOrder = OutBoundOrderList.First();

                                outBoundOrder.StatusName = "已拦截正在收货";
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName" });
                            }
                        }

                    }

                    //收货时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有货损托盘
                    List<Receipt> checkReceiptTCRList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == firstEntity.ReceiptId && u.WhCode == firstEntity.WhCode && u.Status == "H" && u.HuId == firstEntity.HuId);
                    if (checkReceiptTCRList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptTCRList)
                        {
                            PhotoMaster photoMaster = new PhotoMaster();

                            photoMaster.PhotoId = 0;

                            photoMaster.WhCode = item.WhCode;
                            photoMaster.ClientCode = item.ClientCode;
                            photoMaster.Number = item.ReceiptId;
                            photoMaster.Number2 = item.SoNumber;
                            photoMaster.Number3 = item.CustomerPoNumber;
                            photoMaster.Number4 = item.AltItemNumber;

                            photoMaster.PoId = item.PoId;
                            photoMaster.ItemId = item.ItemId;
                            photoMaster.UnitName = item.UnitName;
                            photoMaster.Qty = item.Qty;
                            photoMaster.RegQty = 0;
                            photoMaster.HuId = item.HuId;
                            photoMaster.HoldReason = item.HoldReason;
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
                    }
                    #endregion
                    //TCR结束

                    #endregion

                    idal.IReceiptDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "收货异常，请重新提交！";
                }
            }
        }

        //验证收货扫描序列号
        private string CheckSerialNumberList(ReceiptInsert entity, RecModeldetail recDetail, List<string> searNumber)
        {
            string result = "";

            if (recDetail.SerialNumberInModel != null)
            {
                //1.先验证 是否重复
                foreach (var ser in recDetail.SerialNumberInModel)
                {
                    if (ser != null)
                    {
                        if (searNumber.Contains(ser.CartonId) == true)
                        {
                            result = "序列号扫描重复！";
                            break;
                        }
                        else
                        {
                            searNumber.Add(ser.CartonId);
                        }
                    }
                }
                if (result != "")
                {
                    return result;
                }

                //2.验证是否在同一收货批次下 存在重复
                if (searNumber.Count > 0)
                {
                    List<SerialNumberIn> list = idal.ISerialNumberInDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);

                    int errorCount = list.Where(u => searNumber.Contains(u.CartonId)).Count();
                    if (errorCount > 0)
                    {
                        result = list.Where(u => searNumber.Contains(u.CartonId)).First().CartonId + "等" + errorCount + "个SN已扫描!";
                        //result = "序列号已存在！";
                    }

                    if (result != "")
                    {
                        return result;
                    }
                }
            }
            return "Y";
        }


        //添加采集箱号
        private void SerialNumberInsert(ReceiptInsert entity, int PoId, RecModeldetail recDetail)
        {
            foreach (var ser in recDetail.SerialNumberInModel)
            {
                if (ser != null)
                {
                    //插入采集箱号表
                    SerialNumberIn serial = new SerialNumberIn();
                    serial.WhCode = entity.WhCode;
                    serial.ReceiptId = entity.ReceiptId;
                    serial.ClientId = entity.ClientId;
                    serial.ClientCode = entity.ClientCode;
                    serial.SoNumber = entity.SoNumber;
                    serial.CustomerPoNumber = entity.CustomerPoNumber;
                    serial.AltItemNumber = recDetail.AltItemNumber;
                    serial.PoId = PoId;
                    serial.ItemId = recDetail.ItemId;
                    serial.CartonId = ser.CartonId;
                    serial.HuId = entity.HuId;
                    serial.Length = recDetail.Length / 100;
                    serial.Width = recDetail.Width / 100;
                    serial.Height = recDetail.Height / 100;
                    serial.Weight = recDetail.Weight;
                    serial.LotNumber1 = recDetail.LotNumber1;
                    serial.LotNumber2 = recDetail.LotNumber2;
                    serial.LotDate = recDetail.LotDate;
                    serial.CreateUser = entity.CreateUser;
                    serial.CreateDate = DateTime.Now;
                    serial.ToOutStatus = 1;

                    idal.ISerialNumberInDAL.Add(serial);
                }
            }
        }

        private void WorkloadAccountInsert(ReceiptInsert entity, decimal? qty, decimal? cbm, decimal? weight)
        {
            //先得到工种
            List<string> list = entity.WorkloadAccountModel.Select(u => u.WorkType).Distinct().ToList();

            Hashtable hs = new Hashtable();
            for (int i = 0; i < list.Count; i++)
            {
                string workType = list[i].ToString();
                int workCount = entity.WorkloadAccountModel.Where(u => u.WorkType == workType).Count();
                hs.Add(workType, workCount);    //把工种做为唯一键
            }

            List<RecModeldetail> checkEch = entity.RecModeldetail.Where(u => u.UnitName.Contains("ECH")).Distinct().ToList();

            //插入工人工作量表
            foreach (var workItem in entity.WorkloadAccountModel)
            {
                WorkloadAccount work = new WorkloadAccount();
                work.WhCode = entity.WhCode;
                work.ClientId = entity.ClientId;
                work.ClientCode = entity.ClientCode;
                work.ReceiptId = entity.ReceiptId;
                work.HuId = entity.HuId;
                work.WorkType = workItem.WorkType;
                work.UserCode = workItem.UserCode;
                work.LotFlag = entity.LotFlag;
                work.ReceiptDate = DateTime.Now;

                if (checkEch.Count > 0)
                {
                    work.EchFlag = 1;
                }
                else
                {
                    work.EchFlag = 0;
                }

                if (hs.ContainsKey(workItem.WorkType))
                {
                    int count = Convert.ToInt32(hs[workItem.WorkType]);
                    decimal? avgCbm = cbm / count;
                    decimal? avgQty = qty / count;
                    decimal? avgWeight = weight / count;

                    work.CBM = avgCbm;
                    work.Qty = Convert.ToDecimal(avgQty);
                    work.Weight = avgWeight;
                }
                idal.IWorkloadAccountDAL.Add(work);
            }
        }

        #region 插入收货TranLog

        //对应 C_RecWebController 中的 ReceiptInsert 方法
        public void AddReceiptTranLog(ReceiptInsert entity)
        {


            foreach (var item in entity.RecModeldetail)
            {
                TranLog tl = new TranLog();
                tl.TranType = "103";
                tl.Description = "收货";
                tl.TranDate = DateTime.Now;
                tl.TranUser = entity.CreateUser;
                tl.WhCode = entity.WhCode;
                tl.ClientCode = entity.ClientCode;
                tl.SoNumber = entity.SoNumber;
                tl.CustomerPoNumber = entity.CustomerPoNumber;
                tl.PoID = entity.PoId;
                tl.AltItemNumber = item.AltItemNumber;
                tl.ItemId = item.ItemId;
                tl.UnitID = item.UnitId;
                tl.UnitName = item.UnitName;
                tl.Status = entity.Status;
                tl.TranQty2 = item.Qty;
                tl.HuId = entity.HuId;
                tl.LotFlag = entity.LotFlag;
                tl.Length = item.Length / 100;
                tl.Width = item.Width / 100;
                tl.Height = item.Height / 100;
                tl.Weight = item.Weight;
                tl.LotNumber1 = item.LotNumber1;
                tl.LotNumber2 = item.LotNumber2;
                tl.LotDate = item.LotDate;
                tl.ReceiptId = entity.ReceiptId;
                tl.ReceiptDate = DateTime.Now;
                tl.Location = entity.Location;
                tl.HoldId = entity.HoldMasterModel == null ? 0 : entity.HoldMasterModel.HoldId;
                tl.HoldReason = entity.HoldMasterModel == null ? null : entity.HoldMasterModel.HoldReason;

                idal.ITranLogDAL.Add(tl);
            }
        }
        #endregion

        #region 插入库存信息

        //对应 C_RecWebController 中的 ReceiptInsert 方法
        public void HuMasterInsert(ReceiptInsert entity)
        {
            List<HuMaster> checkHuMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (checkHuMasterList.Count == 0)
            {
                HuMaster hu = new HuMaster();
                hu.WhCode = entity.WhCode;
                hu.HuId = entity.HuId;
                hu.Type = "R";
                if (entity.HoldMasterModel != null)
                {
                    if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                    {
                        hu.Status = "A";
                    }
                    else
                    {
                        hu.Status = "H";
                        hu.HoldId = entity.HoldMasterModel.HoldId;
                        hu.HoldReason = entity.HoldMasterModel.HoldReason;
                    }
                }
                else
                {
                    hu.Status = "A";
                }
                if (entity.HuHeight != 0)
                {
                    hu.HuHeight = entity.HuHeight;
                    hu.HuLength = entity.HuLength;
                    hu.HuWidth = entity.HuWidth;
                }

                hu.Location = entity.Location;
                hu.TransactionFlag = 0;
                hu.ReceiptId = entity.ReceiptId;
                hu.ReceiptDate = DateTime.Now;
                hu.CreateUser = entity.CreateUser;
                hu.CreateDate = DateTime.Now;
                idal.IHuMasterDAL.Add(hu);
            }
            HuDetailInsert(entity);
        }


        public void HuMasterInsert(ReceiptInsert entity, string holdReason)
        {
            List<HuMaster> checkHuMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
            if (checkHuMasterList.Count == 0)
            {
                HuMaster hu = new HuMaster();
                hu.WhCode = entity.WhCode;
                hu.HuId = entity.HuId;
                hu.Type = "R";
                if (entity.HoldMasterModel != null)
                {
                    if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                    {
                        hu.Status = "A";
                    }
                    else
                    {
                        hu.Status = "H";
                        hu.HoldId = entity.HoldMasterModel.HoldId;
                        hu.HoldReason = entity.HoldMasterModel.HoldReason;
                    }
                }
                else
                {
                    hu.Status = "A";
                }

                if (holdReason != "")
                {
                    hu.Status = "H";
                    hu.HoldId = 0;
                    hu.HoldReason = holdReason;
                }
                if (entity.HuHeight != 0)
                {
                    hu.HuHeight = entity.HuHeight / 100;
                    hu.HuLength = entity.HuLength / 100;
                    hu.HuWidth = entity.HuWidth / 100;
                }
                if (entity.HuWeight != 0)
                {
                    hu.HuWeight = entity.HuWeight;
                }

                hu.Location = entity.Location;
                hu.TransactionFlag = 0;
                hu.ReceiptId = entity.ReceiptId;
                hu.ReceiptDate = DateTime.Now;
                hu.CreateUser = entity.CreateUser;
                hu.CreateDate = DateTime.Now;
                idal.IHuMasterDAL.Add(hu);
            }
            HuDetailInsert(entity);
        }

        #endregion

        #region 插入库存信息


        //对应 C_RecWebController 中的 ReceiptInsert 方法
        public void HuDetailInsert(ReceiptInsert entity)
        {
            foreach (var item in entity.RecModeldetail)
            {
                HuDetail hu = new HuDetail();
                hu.WhCode = entity.WhCode;
                hu.HuId = entity.HuId;
                hu.ClientId = entity.ClientId;
                hu.ClientCode = entity.ClientCode;
                hu.SoNumber = entity.SoNumber;
                hu.CustomerPoNumber = entity.CustomerPoNumber;
                hu.ReceiptId = entity.ReceiptId;
                hu.ReceiptDate = DateTime.Now;
                hu.CreateUser = entity.CreateUser;
                hu.CreateDate = DateTime.Now;
                hu.AltItemNumber = item.AltItemNumber;
                hu.ItemId = item.ItemId;
                hu.UnitId = item.UnitId;
                hu.UnitName = item.UnitName;
                hu.Qty = item.Qty;
                hu.PlanQty = 0;
                hu.Length = item.Length / 100;
                hu.Width = item.Width / 100;
                hu.Height = item.Height / 100;
                hu.Weight = item.Weight;
                hu.LotNumber1 = item.LotNumber1;
                hu.LotNumber2 = item.LotNumber2;
                hu.LotDate = item.LotDate;
                hu.Attribute1 = item.Attribute1;

                idal.IHuDetailDAL.Add(hu);
            }
        }
        #endregion

        #region 更新收货批次状态

        public string UpdateRecStatus(string ReceiptId, string WhCode, string CreateUser)
        {
            if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
            {
                return "收货批次号有误或不存在！";
            }
            ReceiptRegister reg = new ReceiptRegister();
            reg.Status = "A";
            reg.UpdateUser = CreateUser;
            reg.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.ReceiptId == ReceiptId && u.WhCode == WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
            idal.SaveChanges();
            return "Y";
        }
        #endregion

        #region 检查是否完成收货

        //返回Y 是全部完成  返回N是部分收货
        public string CheckRecComplete(string ReceiptId, string WhCode)
        {
            if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
            {
                return "收货批次号有误或不存在！";
            }

            List<Receipt> recList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
            if (recList.Count == 0)
            {
                return "收货批次未收入货物，无法完成收货！";
            }

            //2019年2月29日 张雨佳
            //新增 检查是否有强制全部收货流程 主键ID为32，如果有 不允许部分收货
            //验证该收货 是否选择 收货单位(可变)流程    
            List<BusinessFlowHead> checkRFRule = (from c in idal.IReceiptRegisterDAL.SelectAll()
                                                  join a in idal.IFlowDetailDAL.SelectAll() on new { ProcessId = (Int32)c.ProcessId } equals new { ProcessId = a.FlowHeadId } into a_join
                                                  from a in a_join.DefaultIfEmpty()
                                                  join b in idal.IBusinessFlowHeadDAL.SelectAll() on new { BusinessObjectGroupId = (Int32)a.BusinessObjectGroupId } equals new { BusinessObjectGroupId = b.GroupId } into b_join
                                                  from b in b_join.DefaultIfEmpty()
                                                  where c.ReceiptId == ReceiptId && c.WhCode == WhCode
                                                  select b).ToList();

            //绑定了后台数据的主键ID， 请勿随意更改
            if (checkRFRule.Where(u => u.FlowRuleId == 32).Count() > 0)
            {
                if (!recHelper.CheckRecComplete(WhCode, ReceiptId))
                {
                    return "当前流程属于强制完成收货流程，无法部分收货！";
                }
            }


            ReceiptRegister checkReg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).First();

            if (recHelper.CheckRecComplete(WhCode, ReceiptId))
            {
                return "Y";
            }
            else
            {
                if (checkReg.DSFLag == 1)
                {
                    return "直装收货操作单不能部分收货！";
                }
                else
                {
                    return "N";
                }
            }
        }



        //检查是否完成收货  返回Y 是全部完成  返回N是部分收货
        public string CheckRecComplete1(string ReceiptId, string WhCode, string userName)
        {
            if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
            {
                return "收货批次号有误或不存在！";
            }

            List<Receipt> recList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
            if (recList.Count == 0)
            {
                return "收货批次未收入货物，无法完成收货！";
            }

            ReceiptRegister checkReg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).First();

            string result = "";
            if (recHelper.CheckRecComplete(WhCode, ReceiptId))
            {
                result = RecComplete(ReceiptId, WhCode, userName);
                if (result == "Y")
                {
                    TranLog tl2 = new TranLog();
                    tl2.TranType = "125";
                    tl2.Description = "强制完成收货整收收货";
                    tl2.TranDate = DateTime.Now;
                    tl2.TranUser = userName;
                    tl2.WhCode = WhCode;
                    tl2.ReceiptId = ReceiptId;
                    idal.ITranLogDAL.Add(tl2);
                    idal.SaveChanges();
                }
                return result;
            }
            else
            {
                if (checkReg.DSFLag == 1)
                {
                    return "直装收货操作单不能部分收货！";
                }
                else
                {
                    result = PartRecComplete(ReceiptId, WhCode, userName);
                    if (result == "Y")
                    {
                        TranLog tl2 = new TranLog();
                        tl2.TranType = "126";
                        tl2.Description = "强制完成收货部分收货";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = userName;
                        tl2.WhCode = WhCode;
                        tl2.ReceiptId = ReceiptId;
                        idal.ITranLogDAL.Add(tl2);
                        idal.SaveChanges();
                    }
                    return result;
                }
            }
        }

        #endregion

        #region 部分收货

        public string PartRecComplete(string ReceiptId, string WhCode, string CreateUser)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {

                    #region 部分收货
                    if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
                    {
                        return "收货批次号有误或不存在！";
                    }

                    ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).First();
                    if (reg.DSFLag == 1)
                    {
                        return "直装收货操作单不能部分收货！";
                    }

                    if (reg.HoldOutBoundOrderId != 0)
                    {
                        return "拦截订单重收货物不能部分收货！";
                    }

                    //验证托盘是否已全部称重
                    string checkResult = CheckAllHuWeight(ReceiptId, WhCode);
                    if (checkResult != "Y")
                    {
                        return checkResult;
                    }

                    if (!recHelper.CheckRecComplete(WhCode, ReceiptId))
                    {
                        #region 验证TCR
                        var sql = from a in (
                               (from a0 in idal.IReceiptRegisterDetailDAL.SelectAll()
                                join b in idal.IReceiptDAL.SelectAll()
                                     on new { a0.ReceiptId, a0.WhCode, a0.PoId, a0.ItemId }
                                 equals new { b.ReceiptId, b.WhCode, b.PoId, b.ItemId } into b_join
                                from b in b_join.DefaultIfEmpty()
                                where
                                 a0.ReceiptId == ReceiptId && a0.WhCode == WhCode
                                group new { a0, b } by new
                                {
                                    a0.Id,
                                    a0.ReceiptId,
                                    a0.InOrderDetailId,
                                    a0.WhCode,
                                    a0.PoId,
                                    a0.ItemId,
                                    a0.RegQty
                                } into g
                                select new
                                {
                                    g.Key.Id,
                                    g.Key.ReceiptId,
                                    g.Key.InOrderDetailId,
                                    g.Key.WhCode,
                                    PoId = g.Key.PoId,
                                    ItemId = g.Key.ItemId,
                                    RegQty = g.Key.RegQty,
                                    qty = g.Sum(p => ((Int32?)p.b.Qty ?? 0))
                                }))
                                  where a.qty == 0 || a.qty != a.RegQty
                                  select a;

                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in sql)
                        {
                            InBoundOrderDetail inOrderModel = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InOrderDetailId).First();

                            var s = from a in idal.IInBoundOrderDetailDAL.SelectAll()
                                    join b in idal.IInBoundOrderDAL.SelectAll() on new { PoId = a.PoId } equals new { PoId = b.Id } into b_join
                                    from b in b_join.DefaultIfEmpty()
                                    join c in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)b.SoId } equals new { SoId = c.Id } into c_join
                                    from c in c_join.DefaultIfEmpty()
                                    join d in idal.IItemMasterDAL.SelectAll() on new { ItemId = a.ItemId } equals new { ItemId = d.Id } into d_join
                                    from d in d_join.DefaultIfEmpty()
                                    where
                                      a.Id == item.InOrderDetailId
                                    select new InBoundOrderDetailResult
                                    {
                                        ClientCode = b.ClientCode,
                                        SoNumber = c.SoNumber,
                                        CustomerPoNumber = b.CustomerPoNumber,
                                        AltItemNumber = d.AltItemNumber
                                    };

                            PhotoMaster photoMaster = new PhotoMaster();

                            if (s.Count() > 0)
                            {
                                InBoundOrderDetailResult inresult = s.First();

                                photoMaster.WhCode = item.WhCode;
                                photoMaster.ClientCode = inresult.ClientCode;
                                photoMaster.Number = item.ReceiptId;
                                photoMaster.Number2 = inresult.SoNumber;
                                photoMaster.Number3 = inresult.CustomerPoNumber;
                                photoMaster.Number4 = inresult.AltItemNumber;

                                photoMaster.PoId = item.PoId;
                                photoMaster.ItemId = item.ItemId;
                                photoMaster.UnitName = "";
                                photoMaster.Qty = 0;
                                photoMaster.RegQty = item.RegQty;
                                photoMaster.HoldReason = "部分收货";
                                photoMaster.TCRStatus = "未处理";
                                photoMaster.TCRProcessMode = "";

                                photoMaster.SettlementMode = "";
                                photoMaster.SumPrice = 0;
                                photoMaster.Type = "in";

                                photoMaster.Status = 0;

                                photoMaster.CheckStatus1 = "N";
                                photoMaster.CheckStatus2 = "N";
                                photoMaster.CreateUser = CreateUser;
                                photoMaster.CreateDate = DateTime.Now;
                            }

                            //等于0表示实收数量为0
                            if (item.qty == 0)
                            {
                                if (s.Count() > 0)
                                    photoList.Add(photoMaster);

                                inOrderModel.RegQty = inOrderModel.RegQty - item.RegQty;
                                inOrderModel.UpdateUser = CreateUser;
                                inOrderModel.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderModel, u => u.Id == item.InOrderDetailId, new string[] { "RegQty", "UpdateUser", "UpdateDate" });

                                ReceiptRegisterDetail model = new ReceiptRegisterDetail();
                                model.Id = item.Id;
                                idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }
                            else
                            {
                                if (s.Count() > 0)
                                {
                                    photoMaster.Qty = item.qty;
                                    photoList.Add(photoMaster);
                                }

                                int qtyResult = 0;
                                qtyResult = item.RegQty - item.qty;

                                inOrderModel.RegQty = inOrderModel.RegQty - qtyResult;
                                inOrderModel.UpdateUser = CreateUser;
                                inOrderModel.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderModel, u => u.Id == item.InOrderDetailId, new string[] { "RegQty", "UpdateUser", "UpdateDate" });

                                ReceiptRegisterDetail model = new ReceiptRegisterDetail();
                                model.RegQty = item.qty;
                                model.UpdateUser = CreateUser;
                                model.UpdateDate = DateTime.Now;
                                idal.IReceiptRegisterDetailDAL.UpdateBy(model, u => u.Id == item.Id, new string[] { "RegQty", "UpdateUser", "UpdateDate" });
                            }
                        }

                        #endregion

                        //部分收货登记新增
                        int? getRegQty = photoList.Sum(u => u.RegQty);
                        int? getRecQty = photoList.Sum(u => u.Qty);
                        if (getRegQty > getRecQty)
                        {
                            ReceiptPartialRegister regPart = new ReceiptPartialRegister();
                            regPart.WhCode = WhCode;
                            regPart.ReceiptId = ReceiptId;
                            regPart.ClientCode = reg.ClientCode;
                            regPart.Status = "U";
                            regPart.Qty = getRegQty - getRecQty;
                            regPart.CreateUser = CreateUser;
                            regPart.CreateDate = DateTime.Now;
                            idal.IReceiptPartialRegisterDAL.Add(regPart);

                            idal.IPhotoMasterDAL.Add(photoList);
                        }
                    }

                    //收货完成时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有分票
                    List<Receipt> checkReceiptLotFlagList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode && u.LotFlag > 0);
                    if (checkReceiptLotFlagList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptLotFlagList)
                        {
                            string soNumber = item.SoNumber + "分票";
                            if (photoList.Where(u => u.Number2 == soNumber && u.Number == item.ReceiptId).Count() > 0)
                            {
                                continue;
                            }

                            PhotoMaster photoMaster = new PhotoMaster();

                            List<PhotoMaster> checkPhotoMaster = idal.IPhotoMasterDAL.SelectBy(u => u.WhCode == item.WhCode && u.Number == item.ReceiptId && u.Number2 == soNumber);
                            if (checkPhotoMaster.Count == 0)
                            {
                                photoMaster.PhotoId = 0;
                                photoMaster.WhCode = item.WhCode;
                                photoMaster.ClientCode = item.ClientCode;
                                photoMaster.Number = item.ReceiptId;
                                photoMaster.Number2 = soNumber;

                                photoMaster.HoldReason = "货物分票";
                                photoMaster.TCRStatus = "未处理";
                                photoMaster.UnitName = "CTN";
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
                        }

                        idal.IPhotoMasterDAL.Add(photoList);
                    }
                    #endregion
                    //TCR结束

                    //by yujia 19.08.14
                    //完成收货时 更新实收数量、立方、重量
                    List<Receipt> getPickTaskDetail = idal.IReceiptDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId);

                    //计算立方
                    decimal sumCbm = Convert.ToDecimal(getPickTaskDetail.Sum(u => u.Qty * u.Length * u.Width * u.Height).ToString());
                    int sumQty = Convert.ToInt32(getPickTaskDetail.Sum(u => u.Qty).ToString());
                    decimal sumWeight = Convert.ToDecimal(getPickTaskDetail.Sum(u => u.Qty * u.Weight).ToString());

                    reg.RecSumCBM = sumCbm;
                    reg.RecSumQty = sumQty;
                    reg.RecSumWeight = sumWeight;

                    reg.Status = "C";
                    reg.EndReceiptDate = DateTime.Now;
                    reg.UpdateUser = CreateUser;
                    reg.UpdateDate = DateTime.Now;
                    idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "Status", "EndReceiptDate", "UpdateUser", "UpdateDate", "RecSumCBM", "RecSumQty", "RecSumWeight" });

                    HuMaster huMaster = new HuMaster();
                    huMaster.Type = "M";
                    huMaster.UpdateUser = CreateUser;
                    huMaster.UpdateDate = DateTime.Now;
                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.ReceiptId == ReceiptId && u.WhCode == WhCode, new string[] { "Type", "UpdateUser", "UpdateDate" });

                    //插入EDI任务数据
                    oms.UrlEdiTaskInsertRec(ReceiptId, WhCode, CreateUser);
                    #endregion

                    Task<string> task = Task.Run<string>(() =>
                    {
                        Grn grn = new Grn();
                        return grn.AutoSendGRN(ReceiptId, WhCode, "WmsAuto");
                    });

                    idal.SaveChanges();

                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "部分收货异常，请重新提交！";
                }
            }
        }

        #endregion

        #region 完成收货

        public string RecComplete(string ReceiptId, string WhCode, string CreateUser)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 完成收货
                    if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
                    {
                        return "收货批次号有误或不存在！";
                    }
                    ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).First();

                    //直装收货 不能出现货损货物被完成收货
                    if (reg.DSFLag == 1)
                    {
                        List<HuMaster> HuMasterListCheckHold = idal.IHuMasterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId);
                        if (HuMasterListCheckHold.Where(u => u.Status == "H").Count() > 0)
                        {
                            return "直装货物存在货损无法完成收货，请先处理！";
                        }
                    }

                    //验证托盘是否已全部称重
                    string checkResult = CheckAllHuWeight(ReceiptId, WhCode);
                    if (checkResult != "Y")
                    {
                        return checkResult;
                    }

                    //by yujia 19.08.14
                    //完成收货时 更新实收数量、立方、重量
                    List<Receipt> getPickTaskDetail = idal.IReceiptDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId);

                    //计算立方
                    decimal sumCbm = Convert.ToDecimal(getPickTaskDetail.Sum(u => (u.UnitName.Contains("ECH") ? 0 : u.Qty) * u.Length * u.Width * u.Height).ToString());
                    int sumQty = Convert.ToInt32(getPickTaskDetail.Sum(u => u.Qty).ToString());
                    decimal sumWeight = Convert.ToDecimal(getPickTaskDetail.Sum(u => u.Qty * u.Weight).ToString());

                    reg.RecSumCBM = sumCbm;
                    reg.RecSumQty = sumQty;
                    reg.RecSumWeight = sumWeight;

                    //完成收货时 更新收货登记状态
                    reg.Status = "C";
                    reg.UpdateUser = CreateUser;
                    reg.UpdateDate = DateTime.Now;
                    reg.EndReceiptDate = DateTime.Now;
                    idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "Status", "EndReceiptDate", "UpdateUser", "UpdateDate", "RecSumCBM", "RecSumQty", "RecSumWeight" });

                    //完成收货时 更新库存托盘类型为正常
                    HuMaster huMaster = new HuMaster();
                    huMaster.Type = "M";
                    huMaster.UpdateUser = CreateUser;
                    huMaster.UpdateDate = DateTime.Now;
                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.ReceiptId == ReceiptId && u.WhCode == WhCode, new string[] { "Type", "UpdateUser", "UpdateDate" });

                    #region 拦截订单重新收货
                    //完成收货时 验证拦截订单 并删除拦截订单的库存 分拣 包装 交接信息

                    //验证收货批次是否有 拦截订单
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);

                    string result = "";
                    if (ReceiptRegisterList.Count != 0)
                    {
                        ReceiptRegister receiptRegister = ReceiptRegisterList.First();

                        if (receiptRegister.HoldOutBoundOrderId != 0)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == receiptRegister.HoldOutBoundOrderId).First();

                            //如果收货批次是拦截订单再次生成且未处理
                            if (outBoundOrder.StatusId == -10 && (outBoundOrder.InterceptFlag ?? 0) == 0)
                            {
                                #region 
                                //得到拦截订单信息
                                List<OutBoundOrderDetail> OutBoundOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == receiptRegister.HoldOutBoundOrderId);

                                LoadDetail loadDetail = idal.ILoadDetailDAL.SelectBy(u => u.OutBoundOrderId == receiptRegister.HoldOutBoundOrderId).First();
                                LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == loadDetail.LoadMasterId).First();

                                //得到Load的所选流程
                                List<FlowDetail> flowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == loadMaster.ProcessId && u.Type == "Release");

                                if (flowDetailList.Count > 0)
                                {
                                    //得到释放的条件 是先进先出 还是后进先出等
                                    string mark = flowDetailList.First().Mark;

                                    //得到拦截订单下 Load的所有库存信息
                                    List<HuDetail> HuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.HuId == loadMaster.LoadId);

                                    //验证托盘是否已扣减，如果 已扣减，应跳过该行 继续下一行
                                    List<HuDetail> checkHuList = new List<HuDetail>();

                                    //出库订单信息
                                    foreach (var item in OutBoundOrderDetailList)
                                    {
                                        int tranQty = item.Qty;   //得到出库数量

                                        #region 根据释放条件 删除库存


                                        if (mark == "1" || mark == "2")     //带SO PO的释放条件
                                        {
                                            List<HuDetail> GetHuDetailList = HuDetailList.Where(u => u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                                    && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                                            if (GetHuDetailList.Count > 0)
                                            {
                                                foreach (var huDetail in GetHuDetailList)
                                                {
                                                    if (tranQty == 0)
                                                    {
                                                        break;
                                                    }

                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                                                    {
                                                        huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
                                                    }
                                                    if (huDetail.Qty < 1)
                                                    {
                                                        continue;
                                                    }

                                                    //插入日志
                                                    AddInerceptOrderRecLog(CreateUser, huDetail, outBoundOrder);

                                                    if (huDetail.Qty < tranQty)
                                                    {
                                                        huDetail.Qty = 0;
                                                        tranQty = tranQty - huDetail.Qty;
                                                    }
                                                    else
                                                    {
                                                        huDetail.Qty = huDetail.Qty - tranQty;
                                                        tranQty = 0;    //交接数量已扣减   
                                                    }

                                                    //记录下当前托盘 已扣除过的数量 不能在下个交接明细中继续扣除
                                                    //如：PLTA00001 SKU1 数量1 在交接1中被扣除了，但 交接2循环进来就不能再扣减它了
                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                                                    {
                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                    else
                                                    {
                                                        HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                                                        checkHuList.Remove(oldHu);

                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                }

                                                foreach (var item1 in checkHuList)
                                                {
                                                    HuDetail huDetail = new HuDetail();
                                                    huDetail.Qty = item1.Qty;
                                                    huDetail.UpdateUser = CreateUser;
                                                    huDetail.UpdateDate = DateTime.Now;
                                                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item1.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                                }
                                            }

                                        }
                                        else if (mark == "5" || mark == "6")     //无视SO 的释放条件
                                        {
                                            List<HuDetail> GetHuDetailList = HuDetailList.Where(u => u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName &&
                                   (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                                            if (GetHuDetailList.Count > 0)
                                            {
                                                foreach (var huDetail in GetHuDetailList)
                                                {
                                                    if (tranQty == 0)
                                                    {
                                                        break;
                                                    }

                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                                                    {
                                                        huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
                                                    }
                                                    if (huDetail.Qty < 1)
                                                    {
                                                        continue;
                                                    }

                                                    //插入日志
                                                    AddInerceptOrderRecLog(CreateUser, huDetail, outBoundOrder);

                                                    if (huDetail.Qty < tranQty)
                                                    {
                                                        huDetail.Qty = 0;
                                                        tranQty = tranQty - huDetail.Qty;
                                                    }
                                                    else
                                                    {
                                                        huDetail.Qty = huDetail.Qty - tranQty;
                                                        tranQty = 0;    //交接数量已扣减   
                                                    }

                                                    //记录下当前托盘 已扣除过的数量 不能在下个交接明细中继续扣除
                                                    //如：PLTA00001 SKU1 数量1 在交接1中被扣除了，但 交接2循环进来就不能再扣减它了
                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                                                    {
                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                    else
                                                    {
                                                        HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                                                        checkHuList.Remove(oldHu);

                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                }

                                                foreach (var item1 in checkHuList)
                                                {
                                                    HuDetail huDetail = new HuDetail();
                                                    huDetail.Qty = item1.Qty;
                                                    huDetail.UpdateUser = CreateUser;
                                                    huDetail.UpdateDate = DateTime.Now;
                                                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item1.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                                }
                                            }

                                        }
                                        else
                                        {
                                            List<HuDetail> GetHuDetailList = HuDetailList.Where(u => u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName &&
                                        (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                                            if (GetHuDetailList.Count > 0)
                                            {
                                                foreach (var huDetail in GetHuDetailList)
                                                {
                                                    if (tranQty == 0)
                                                    {
                                                        break;
                                                    }

                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                                                    {
                                                        huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
                                                    }
                                                    if (huDetail.Qty < 1)
                                                    {
                                                        continue;
                                                    }

                                                    //插入日志
                                                    AddInerceptOrderRecLog(CreateUser, huDetail, outBoundOrder);

                                                    if (huDetail.Qty < tranQty)
                                                    {
                                                        huDetail.Qty = 0;
                                                        tranQty = tranQty - huDetail.Qty;
                                                    }
                                                    else
                                                    {
                                                        huDetail.Qty = huDetail.Qty - tranQty;
                                                        tranQty = 0;    //交接数量已扣减   
                                                    }

                                                    //记录下当前托盘 已扣除过的数量 不能在下个交接明细中继续扣除
                                                    //如：PLTA00001 SKU1 数量1 在交接1中被扣除了，但 交接2循环进来就不能再扣减它了
                                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                                                    {
                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                    else
                                                    {
                                                        HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                                                        checkHuList.Remove(oldHu);

                                                        HuDetail newHu = new HuDetail();
                                                        newHu.Id = huDetail.Id;
                                                        newHu.Qty = huDetail.Qty;
                                                        checkHuList.Add(newHu);
                                                    }
                                                }

                                                foreach (var item1 in checkHuList)
                                                {
                                                    HuDetail huDetail = new HuDetail();
                                                    huDetail.Qty = item1.Qty;
                                                    huDetail.UpdateUser = CreateUser;
                                                    huDetail.UpdateDate = DateTime.Now;
                                                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item1.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });

                                                }
                                            }
                                        }

                                        #endregion


                                        #region 注释删除拦截订单的分拣及包装

                                        //#region 删除分拣
                                        //List<SortTaskDetail> SortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.OutPoNumber != outBoundOrder.OutPoNumber);
                                        //if (SortTaskDetailList.Count == 0)
                                        //{
                                        //    idal.ISortTaskDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId);
                                        //}
                                        //idal.ISortTaskDetailDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber);
                                        //#endregion

                                        //#region 删除包装
                                        //List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber);
                                        //foreach (var packTask in PackTaskList)
                                        //{
                                        //    List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id);

                                        //    foreach (var packHead in packHeadList)
                                        //    {
                                        //        //通过 包装头ID  获取包装明细信息
                                        //        List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHead.Id);

                                        //        foreach (var packDetail in packDetailList)
                                        //        {
                                        //            //3.删除对应的包装扫描信息
                                        //            idal.IPackScanNumberDAL.DeleteBy(u => u.PackDetailId == packDetail.Id);

                                        //            //4.删除包装明细信息
                                        //            idal.IPackDetailDAL.DeleteBy(u => u.Id == packDetail.Id);
                                        //        }

                                        //        //5.删除包装头信息
                                        //        idal.IPackHeadDAL.DeleteBy(u => u.Id == packHead.Id);
                                        //    }
                                        //    idal.IPackTaskDAL.DeleteBy(u => u.Id == packTask.Id);
                                        //}
                                        //#endregion


                                        #endregion

                                        idal.SaveChanges(); //先执行一次 删除
                                    }
                                }
                                else
                                {
                                    result = "匹配拦截订单出货流程错误无法完成收货！";
                                }

                                if (result != "")
                                {
                                    return result;
                                }
                                //再检测 库存是否存在为0的情况
                                List<HuDetail> checkHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.HuId == loadMaster.LoadId && u.Qty != 0);
                                if (checkHuDetailList.Count == 0)
                                {
                                    idal.IHuMasterDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.HuId == loadMaster.LoadId);
                                }
                                idal.IHuDetailDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.HuId == loadMaster.LoadId && u.Qty == 0);

                                outBoundOrder.InterceptFlag = 1;
                                outBoundOrder.StatusName = "已拦截已处理";
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName", "InterceptFlag" });

                                #endregion
                            }
                        }
                    }

                    if (result != "")
                    {
                        return result;
                    }
                    #endregion

                    //插入EDI任务数据
                    oms.UrlEdiTaskInsertRec(ReceiptId, WhCode, CreateUser);

                    //直装完成收货 
                    #region  直装收货
                    //插入备货任务表 完成备货 分拣表中插入备货数量
                    if (reg.DSFLag == 1)
                    {
                        LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == reg.LoadMasterId).First();
                        List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId);
                        //更新直装出库单 明细中的none单位
                        foreach (var item in regDetailList)
                        {
                            List<OutBoundOrderDetail> outorderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == reg.OutBoundOrderId && u.WhCode == reg.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == "none");
                            if (outorderDetailList.Count > 0)
                            {
                                OutBoundOrderDetail outorderDetail = outorderDetailList.First();
                                outorderDetail.UnitName = item.UnitName;
                                outorderDetail.UnitId = item.UnitId;
                                idal.IOutBoundOrderDetailDAL.UpdateBy(outorderDetail, u => u.Id == outorderDetail.Id, new string[] { "UnitName", "UnitId" });
                            }

                            List<PickTaskDetail> pickTaskDetailList = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == loadMaster.LoadId && u.WhCode == loadMaster.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == "none");
                            if (pickTaskDetailList.Count > 0)
                            {
                                PickTaskDetail pickTaskDetail = pickTaskDetailList.First();
                                pickTaskDetail.UnitName = item.UnitName;
                                pickTaskDetail.UnitId = item.UnitId;
                                idal.IPickTaskDetailDAL.UpdateBy(pickTaskDetail, u => u.Id == pickTaskDetail.Id, new string[] { "UnitName", "UnitId" });
                            }
                        }
                        idal.SaveChanges(); //执行修改单位

                        //插入备货任务表
                        List<PickTaskDetail> pickTaskList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.DSFLag == 1);

                        List<HuDetailResult> hudetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                                             join b in idal.IHuMasterDAL.SelectAll()
                                                                   on new { a.WhCode, a.HuId }
                                                               equals new { b.WhCode, b.HuId } into b_join
                                                             from b in b_join.DefaultIfEmpty()
                                                             where
                                                               a.ReceiptId == reg.ReceiptId &&
                                                               a.WhCode == reg.WhCode
                                                             select new HuDetailResult
                                                             {
                                                                 Id = a.Id,
                                                                 HuId = a.HuId,
                                                                 WhCode = a.WhCode,
                                                                 ClientId = a.ClientId,
                                                                 ClientCode = a.ClientCode,
                                                                 SoNumber = a.SoNumber,
                                                                 CustomerPoNumber = a.CustomerPoNumber,
                                                                 AltItemNumber = a.AltItemNumber,
                                                                 ReceiptDate = a.ReceiptDate,
                                                                 PlanQty = a.PlanQty ?? 0,
                                                                 Qty = a.Qty,
                                                                 ItemId = a.ItemId,
                                                                 UnitId = a.UnitId,
                                                                 UnitName = a.UnitName,
                                                                 Height = a.Height,
                                                                 Length = a.Length,
                                                                 Weight = a.Weight,
                                                                 Width = a.Width,
                                                                 LotNumber1 = a.LotNumber1,
                                                                 LotNumber2 = a.LotNumber2,
                                                                 LotDate = a.LotDate,
                                                                 Location = b.Location
                                                             }).ToList();

                        List<PickTaskDetail> addPickTaskDetailList = new List<PickTaskDetail>();
                        List<PickTaskDetail> updatePickTaskDetailList = new List<PickTaskDetail>();
                        foreach (var item in pickTaskList)
                        {
                            List<HuDetailResult> checkHudetail = hudetailList.Where(u => u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName).ToList();
                            if (checkHudetail.Count > 0)
                            {
                                foreach (var hudetail in checkHudetail)
                                {
                                    PickTaskDetail detail = new PickTaskDetail();
                                    detail.WhCode = hudetail.WhCode;
                                    detail.LoadId = item.LoadId;
                                    detail.HuDetailId = hudetail.Id;
                                    detail.HuId = hudetail.HuId;
                                    detail.Location = hudetail.Location;
                                    detail.SoNumber = hudetail.SoNumber;
                                    detail.CustomerPoNumber = hudetail.CustomerPoNumber;
                                    detail.AltItemNumber = hudetail.AltItemNumber;
                                    detail.ItemId = (Int32)hudetail.ItemId;
                                    detail.UnitId = hudetail.UnitId;
                                    detail.UnitName = hudetail.UnitName;
                                    detail.Qty = (Int32)hudetail.Qty;
                                    detail.DSFLag = 1;
                                    detail.Length = (hudetail.Length ?? 0);
                                    detail.Width = (hudetail.Width ?? 0);
                                    detail.Height = (hudetail.Height ?? 0);
                                    detail.Weight = (hudetail.Weight ?? 0);
                                    detail.LotNumber1 = hudetail.LotNumber1;
                                    detail.LotNumber2 = hudetail.LotNumber2;
                                    detail.LotDate = hudetail.LotDate;
                                    detail.Sequence = item.Sequence;
                                    detail.Status = "C";
                                    detail.Status1 = "U";
                                    detail.CreateUser = CreateUser;
                                    detail.CreateDate = DateTime.Now;
                                    addPickTaskDetailList.Add(detail);

                                    if (updatePickTaskDetailList.Where(u => u.Id == item.Id).Count() == 0)
                                    {
                                        PickTaskDetail updatePick = new PickTaskDetail();
                                        updatePick.Id = item.Id;
                                        updatePick.Qty = item.Qty - detail.Qty;
                                        updatePickTaskDetailList.Add(updatePick);
                                    }
                                    else
                                    {
                                        PickTaskDetail getPick = updatePickTaskDetailList.Where(u => u.Id == item.Id).First();
                                        updatePickTaskDetailList.Remove(getPick);

                                        PickTaskDetail updatePick = new PickTaskDetail();
                                        updatePick.Id = item.Id;
                                        updatePick.Qty = getPick.Qty - detail.Qty;
                                        updatePickTaskDetailList.Add(updatePick);
                                    }
                                }
                            }
                        }
                        //插入库存托盘
                        idal.IPickTaskDetailDAL.Add(addPickTaskDetailList);
                        //修改备货任务
                        foreach (var item in updatePickTaskDetailList)
                        {
                            idal.IPickTaskDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                        }
                        idal.SaveChanges();         //执行插入备货托盘

                        idal.IPickTaskDetailDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.DSFLag == 1 && u.Qty == 0);

                        //插入分拣表的备货数量
                        foreach (var addPick in addPickTaskDetailList)
                        {
                            List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId && u.HuId == addPick.HuId && u.DSFLag == 1);
                            foreach (var item in pickList)
                            {
                                List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                                int resultQty = item.Qty;
                                foreach (var detail in sortTaskDetailList)
                                {
                                    if (detail.PickQty + resultQty > detail.PlanQty)
                                    {
                                        resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                        SortTaskDetail entity = new SortTaskDetail();
                                        entity.PickQty = detail.PlanQty;
                                        entity.UpdateUser = CreateUser;
                                        entity.UpdateDate = DateTime.Now;
                                        idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "PickQty", "UpdateUser", "UpdateDate" });
                                        idal.IPickTaskDetailDAL.SaveChanges();
                                        continue;
                                    }
                                    else
                                    {
                                        SortTaskDetail entity = new SortTaskDetail();
                                        entity.PickQty = detail.PickQty + resultQty;
                                        entity.UpdateUser = CreateUser;
                                        entity.UpdateDate = DateTime.Now;
                                        idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "PickQty", "UpdateUser", "UpdateDate" });
                                        idal.IPickTaskDetailDAL.SaveChanges();

                                        break;
                                    }
                                }
                            }
                        }

                        //更新Load的备货状态
                        //-----------25 为已备货
                        FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == loadMaster.ProcessId && u.Type == "Picking").First();

                        //修改Load状态
                        if (loadMaster.Status1 == "U")
                        {
                            loadMaster.Status1 = "A";
                            loadMaster.UpdateUser = CreateUser;
                            loadMaster.UpdateDate = DateTime.Now;
                            loadMaster.BeginPickDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.Id == loadMaster.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate" });

                            //修改OutOrder状态
                            List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                              where a.LoadMasterId == loadMaster.Id
                                                              select new OutBoundOrderResult
                                                              {
                                                                  Id = (Int32)a.OutBoundOrderId
                                                              }).ToList();
                            foreach (var item in list)
                            {
                                OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                                if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                                {
                                    outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                    outBoundOrder.StatusId = flowDetail.StatusId;
                                    outBoundOrder.StatusName = "正在备货";
                                    outBoundOrder.UpdateUser = CreateUser;
                                    outBoundOrder.UpdateDate = DateTime.Now;
                                    idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });
                                }

                            }
                        }
                        idal.SaveChanges();

                        //检查是否全部完成备货
                        var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                                   where a.LoadId == loadMaster.LoadId && a.WhCode == loadMaster.WhCode && a.Status != "U"
                                   select a;
                        if (sql4.Count() == 0)
                        {
                            loadMaster.Status1 = "C";
                            loadMaster.UpdateUser = CreateUser;
                            loadMaster.UpdateDate = DateTime.Now;
                            loadMaster.EndPickDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.Id == loadMaster.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                            //修改OutOrder状态
                            List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                              where a.LoadMasterId == loadMaster.Id
                                                              select new OutBoundOrderResult
                                                              {
                                                                  Id = (Int32)a.OutBoundOrderId
                                                              }).ToList();
                            foreach (var item in list)
                            {
                                OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                                //只更新正在备货的订单 
                                if (outBoundOrder.StatusName == "正在备货")
                                {
                                    outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                    outBoundOrder.StatusId = flowDetail.StatusId;
                                    outBoundOrder.StatusName = flowDetail.StatusName;
                                    outBoundOrder.UpdateUser = CreateUser;
                                    outBoundOrder.UpdateDate = DateTime.Now;
                                    idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });
                                }
                            }
                        }
                    }
                    #endregion
                    //直装结束

                    //收货完成时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有分票
                    List<Receipt> checkReceiptLotFlagList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode && u.LotFlag > 0);
                    if (checkReceiptLotFlagList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptLotFlagList)
                        {
                            string soNumber = item.SoNumber + "分票";
                            if (photoList.Where(u => u.Number2 == soNumber && u.Number == item.ReceiptId).Count() > 0)
                            {
                                continue;
                            }

                            PhotoMaster photoMaster = new PhotoMaster();

                            List<PhotoMaster> checkPhotoMaster = idal.IPhotoMasterDAL.SelectBy(u => u.WhCode == item.WhCode && u.Number == item.ReceiptId && u.Number2 == soNumber);
                            if (checkPhotoMaster.Count == 0)
                            {
                                photoMaster.PhotoId = 0;
                                photoMaster.WhCode = item.WhCode;
                                photoMaster.ClientCode = item.ClientCode;
                                photoMaster.Number = item.ReceiptId;
                                photoMaster.Number2 = soNumber;

                                photoMaster.HoldReason = "货物分票";
                                photoMaster.TCRStatus = "未处理";
                                photoMaster.UnitName = "CTN";
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
                        }

                        idal.IPhotoMasterDAL.Add(photoList);
                    }
                    #endregion
                    //TCR结束

                    #endregion

                    //Grn grn = new Grn();
                    //grn.AutoSendGRN(ReceiptId, WhCode, "WmsAuto");

                    Task<string> task = Task.Run<string>(() =>
                    {
                        Grn grn = new Grn();
                        return grn.AutoSendGRN(ReceiptId, WhCode, "WmsAuto");
                    });

                    idal.SaveChanges();

                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string ss = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                    return "完成收货异常，请重新提交！";
                }
            }
        }
        public string RecCheckOverWeightList(List<ReceiptInsert> entityList)
        {


            decimal? weightTotal = 0;

            foreach (var item in entityList)
            {
                weightTotal += RecCheckOverWeight(item);
            }
            //1480
            if (weightTotal != 0 & weightTotal <= 1480)
                return "Y$" + Convert.ToInt32(weightTotal);
            else if (weightTotal == 0)
                return "N$系统重量为:0,确认收货?";
            else
                return "N$总重:" + Convert.ToInt32(weightTotal) + " 超重!确认收货?";
        }


        public decimal? RecCheckOverWeight(ReceiptInsert entity)
        {

            int SoId = 0;
            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);
                SoId = soList.First().Id;
            }

            int PoId = 0;
            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                PoId = orderList.First().Id;
            }
            else
            {
                List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                PoId = orderList.First().Id;
            }

            decimal? weightTotal = 0;
            List<InBoundOrderDetail> inboundOrderDetailList = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.PoId == PoId);

            foreach (var item in entity.RecModeldetail)
            {

                InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();
                weightTotal += (inOrderDetail.Weight / inOrderDetail.Qty) * item.Qty;

            }
            return weightTotal;
        }

        //拦截订单完成收货日志LOG
        private void AddInerceptOrderRecLog(string CreateUser, HuDetail huDetail, OutBoundOrder outboundorder)
        {
            //插入tranLog
            TranLog tl = new TranLog();
            tl.TranType = "130";
            tl.Description = "收货完成拦截订单删除";
            tl.TranDate = DateTime.Now;
            tl.TranUser = CreateUser;
            tl.WhCode = huDetail.WhCode;
            tl.ClientCode = outboundorder.ClientCode;
            tl.SoNumber = huDetail.SoNumber;
            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
            tl.AltItemNumber = huDetail.AltItemNumber;
            tl.ItemId = huDetail.ItemId;
            tl.UnitID = huDetail.UnitId;
            tl.UnitName = huDetail.UnitName;
            tl.TranQty = huDetail.Qty;
            tl.TranQty2 = huDetail.Qty;
            tl.HuId = huDetail.HuId;
            tl.Length = huDetail.Length;
            tl.Width = huDetail.Width;
            tl.Height = huDetail.Height;
            tl.Weight = huDetail.Weight;
            tl.LotNumber1 = huDetail.LotNumber1;
            tl.LotNumber2 = huDetail.LotNumber2;
            tl.LotDate = huDetail.LotDate;
            tl.ReceiptId = huDetail.ReceiptId;
            tl.ReceiptDate = huDetail.ReceiptDate;
            tl.OutPoNumber = outboundorder.OutPoNumber;
            tl.CustomerOutPoNumber = outboundorder.CustomerOutPoNumber;
            tl.LoadId = huDetail.HuId;
            tl.Remark = "拦截订单进行收货匹配库存删除";

            idal.ITranLogDAL.Add(tl);
        }

        #endregion

        #region 暂停收货

        public string PauseRec(string ReceiptId, string WhCode, string CreateUser)
        {
            if (!recHelper.CheckReceiptId(ReceiptId, WhCode))
            {
                return "收货批次号有误或不存在！";
            }
            ReceiptRegister reg = new ReceiptRegister();
            reg.Status = "P";
            reg.UpdateUser = CreateUser;
            reg.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.ReceiptId == ReceiptId && u.WhCode == WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
            idal.SaveChanges();
            return "Y";
        }
        #endregion


        //获取收货数据信息
        public List<EclRecModel> GetRecInfoEcl(string ReceiptId)
        {
            var sql = from receipt in idal.IReceiptDAL.SelectAll()
                      where
                        receipt.ReceiptId == ReceiptId
                      group receipt by new
                      {
                          receipt.SoNumber,
                          receipt.ClientCode,
                          receipt.CustomerPoNumber,
                          receipt.AltItemNumber,
                          receipt.UnitName,
                          receipt.LotNumber1,
                          receipt.LotNumber2,
                          receipt.LotDate,
                          receipt.ReceiptId
                      } into g
                      select new EclRecModel
                      {
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          UnitName = g.Key.UnitName,
                          LotNumber1 = g.Key.LotNumber1,
                          LotNumber2 = g.Key.LotNumber2,
                          LotDate = (DateTime?)g.Key.LotDate,
                          Qty = (Int32)g.Sum(p => p.Qty),
                          ClientCode = g.Key.ClientCode,
                          ReceiptId = g.Key.ReceiptId
                      };

            return sql.ToList();
        }


        //收货按款号插入
        public string ReceiptByAltItemNumberInsert(ReceiptInsert entity)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 收货优化
                    if (entity.RecModeldetail == null)
                    {
                        return "错误！没有货物明细！";
                    }

                    //1.首先验证全部数据是否满足
                    if (!recHelper.CheckReceiptId(entity.ReceiptId, entity.WhCode))
                    {
                        return "错误！收货批次号有误或不存在！";
                    }

                    if (!recHelper.CheckSoPo(entity.ReceiptId, entity.WhCode, entity.SoNumber, entity.CustomerPoNumber))
                    {
                        return "错误！SO或PO输入有误！";
                    }

                    bool recDetailResult = true;
                    string result = "";

                    List<RecModeldetail> listRecDetail = new List<RecModeldetail>();
                    foreach (var item in entity.RecModeldetail)
                    {
                        if (entity.HuId != item.AltItemNumber)
                        {
                            result = "托盘号必须与款号一致！";
                            break;
                        }
                        if (recDetailResult)
                        {
                            if ((from a in listRecDetail where a.ItemId == item.ItemId && a.UnitId == item.UnitId && a.Length == item.Length && a.Width == item.Width && a.Height == item.Height && a.LotNumber1 == item.LotNumber1 && a.LotNumber2 == item.LotNumber2 && a.LotDate == item.LotDate select a).Count() == 0)
                            {
                                RecModeldetail recDetail = new RecModeldetail();
                                recDetail.ItemId = item.ItemId;
                                recDetail.UnitId = item.UnitId;
                                recDetail.Length = item.Length;
                                recDetail.Width = item.Width;
                                recDetail.Height = item.Height;
                                recDetail.LotNumber1 = item.LotNumber1;
                                recDetail.LotNumber2 = item.LotNumber2;
                                recDetail.LotDate = item.LotDate;
                                recDetail.Attribute1 = item.Attribute1;
                                listRecDetail.Add(recDetail);
                            }
                            else
                            {
                                recDetailResult = false;
                            }
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }
                    if (recDetailResult == false)
                    {
                        return "错误！货物明细重复或异常！";
                    }


                    List<int> ItemList = new List<int>();

                    List<string> checksearNumberList = new List<string>();
                    foreach (var item in entity.RecModeldetail)
                    {
                        if (!ItemList.Contains(item.ItemId))
                        {
                            ItemList.Add(item.ItemId);
                        }

                        string s = CheckSerialNumberList(entity, item, checksearNumberList);
                        if (s != "Y")
                        {
                            result = s;
                            break;
                        }
                    }

                    if (result != "")
                    {
                        return result;
                    }


                    if (!recHelper.CheckSku(entity.ReceiptId, entity.WhCode, ItemList, entity.CustomerPoNumber))
                    {
                        return "错误！款号有误或不存在！";
                    }
                    if (!recHelper.CheckSku(entity.WhCode, ItemList))
                    {
                        return "错误！款号有误或不存在！";
                    }

                    //验证该收货 是否选择 收货单位(可变)流程    
                    List<BusinessFlowHead> checkRFRule = (from c in idal.IReceiptRegisterDAL.SelectAll()
                                                          join a in idal.IFlowDetailDAL.SelectAll() on new { ProcessId = (Int32)c.ProcessId } equals new { ProcessId = a.FlowHeadId } into a_join
                                                          from a in a_join.DefaultIfEmpty()
                                                          join b in idal.IBusinessFlowHeadDAL.SelectAll() on new { BusinessObjectGroupId = (Int32)a.BusinessObjectGroupId } equals new { BusinessObjectGroupId = b.GroupId } into b_join
                                                          from b in b_join.DefaultIfEmpty()
                                                          where c.ReceiptId == entity.ReceiptId && c.WhCode == entity.WhCode
                                                          select b).ToList();

                    //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                    //绑定了后台数据的主键ID， 请勿随意更改
                    if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                    {
                        if (!recHelper.CheckUnit(entity.WhCode, ItemList, entity))
                        {
                            return "错误！款号对应的单位有误！";
                        }
                    }

                    if (recHelper.CheckSkuId(entity.WhCode, entity.RecModeldetail, entity.ClientId) == false)
                    {
                        return "错误！款号所扫描的ID有误！";
                    }

                    int SoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);
                        SoId = soList.First().Id;
                    }

                    int PoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }
                    else
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }

                    //2.验证实收数量 预收数量与登记数量是否有差异

                    //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                    //绑定了后台数据的主键ID， 请勿随意更改
                    if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                    {
                        string checkReceiptQtyResult = CheckReceiptQty(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }
                    else
                    {
                        //款号可变形的验证
                        string checkReceiptQtyResult = CheckReceiptQtyByItemBX(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }

                    //3.开始插入数据
                    List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
                    ReceiptRegister reg = regList.First();

                    Receipt rec = null;
                    decimal? qty = 0;    //总数量
                    decimal? cbm = 0;    //总体积
                    decimal? weight = 0; //总重量


                    foreach (var item in entity.RecModeldetail)
                    {
                        //插入实收表
                        rec = new Receipt();
                        rec.WhCode = entity.WhCode;
                        rec.RegId = reg.Id;
                        rec.ReceiptId = entity.ReceiptId;
                        rec.ClientId = entity.ClientId;
                        rec.ClientCode = entity.ClientCode;
                        rec.ReceiptDate = DateTime.Now;
                        if (entity.HoldMasterModel != null)
                        {
                            if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                            {
                                rec.Status = "A";
                            }
                            else
                            {
                                rec.Status = "H";
                                rec.HoldReason = entity.HoldMasterModel.HoldReason;
                            }
                        }
                        else
                        {
                            rec.Status = "A";
                        }
                        rec.SoNumber = entity.SoNumber;
                        rec.PoId = PoId;
                        rec.CustomerPoNumber = entity.CustomerPoNumber;
                        rec.HuId = entity.HuId;
                        rec.LotFlag = entity.LotFlag;
                        rec.CreateUser = entity.CreateUser;
                        rec.CreateDate = DateTime.Now;

                        rec.AltItemNumber = item.AltItemNumber;
                        rec.ItemId = item.ItemId;
                        rec.UnitId = item.UnitId;
                        rec.UnitName = item.UnitName;
                        rec.Qty = item.Qty;
                        rec.Length = item.Length / 100;
                        rec.Width = item.Width / 100;
                        rec.Height = item.Height / 100;
                        rec.Weight = item.Weight;
                        rec.LotNumber1 = item.LotNumber1;
                        rec.LotNumber2 = item.LotNumber2;
                        rec.LotDate = item.LotDate;
                        rec.Attribute1 = item.Attribute1;

                        idal.IReceiptDAL.Add(rec);

                        qty += item.Qty;
                        cbm += (item.UnitName.Contains("ECH") ? 0 : item.Qty) * (item.Length / 100) * (item.Height / 100) * (item.Width / 100);
                        weight += item.Weight;


                        ItemMaster getItemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();
                        //2.如果选择了正常的收货单位  
                        //验证单位 且修改单位
                        if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                        {
                            if (getItemMaster.UnitFlag == 0 && getItemMaster.UnitName == "none")
                            {
                                ItemMaster item1 = new ItemMaster();
                                item1.UnitName = item.UnitName;
                                item1.UpdateUser = entity.CreateUser;
                                item1.UpdateDate = DateTime.Now;
                                idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.ItemId == item.ItemId && u.PoId == PoId && u.WhCode == entity.WhCode).First();
                            if (inOrderDetail.UnitName == "none")
                            {
                                inOrderDetail.UnitId = item.UnitId;
                                inOrderDetail.UnitName = item.UnitName;
                                inOrderDetail.UpdateUser = entity.CreateUser;
                                inOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            ReceiptRegisterDetail receiptRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId && u.ItemId == item.ItemId).First();
                            if (receiptRegDetail.UnitName == "none")
                            {
                                receiptRegDetail.UnitId = item.UnitId;
                                receiptRegDetail.UnitName = item.UnitName;
                                receiptRegDetail.UpdateUser = entity.CreateUser;
                                receiptRegDetail.UpdateDate = DateTime.Now;
                                idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }
                        }
                        else
                        {
                            ItemMaster item1 = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                            item1.UnitName = item.UnitName;
                            item1.UpdateUser = entity.CreateUser;
                            item1.UpdateDate = DateTime.Now;
                            idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });

                            InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.ItemId == item.ItemId && u.PoId == PoId && u.WhCode == entity.WhCode).First();

                            inOrderDetail.UnitId = item.UnitId;
                            inOrderDetail.UnitName = item.UnitName;
                            inOrderDetail.UpdateUser = entity.CreateUser;
                            inOrderDetail.UpdateDate = DateTime.Now;
                            idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });

                            ReceiptRegisterDetail receiptRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId && u.ItemId == item.ItemId).First();

                            receiptRegDetail.UnitId = item.UnitId;
                            receiptRegDetail.UnitName = item.UnitName;
                            receiptRegDetail.UpdateUser = entity.CreateUser;
                            receiptRegDetail.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                        }


                        if (item.SerialNumberInModel != null)
                        {
                            SerialNumberInsert(entity, PoId, item);     //添加采集箱号
                        }
                    }

                    //更改收货批次号状态
                    ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
                    if (receiptRegister.Status == "U" || receiptRegister.Status == "P" || entity.TransportTypeEdit == "1")
                    {
                        if (receiptRegister.BeginReceiptDate == null)
                        {
                            ReceiptRegister receiptRegister1 = new ReceiptRegister();
                            receiptRegister1.BeginReceiptDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister1, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "BeginReceiptDate" });
                        }
                        receiptRegister.Status = "A";
                        if (receiptRegister.TransportType == null || receiptRegister.TransportType == "" || entity.TransportTypeEdit == "1")
                        {
                            receiptRegister.TransportType = entity.TransportType;
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "TransportType", "UpdateUser", "UpdateDate" });
                        }
                        else
                        {
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
                        }

                        //正在收货时 验证 是否是拦截订单
                        if (receiptRegister.HoldOutBoundOrderId != 0)
                        {
                            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == receiptRegister.HoldOutBoundOrderId);
                            if (OutBoundOrderList.Count != 0)
                            {
                                OutBoundOrder outBoundOrder = OutBoundOrderList.First();

                                outBoundOrder.StatusName = "已拦截正在收货";
                                outBoundOrder.UpdateUser = entity.CreateUser;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName", "UpdateUser", "UpdateDate" });
                            }
                        }

                    }

                    if (entity.WorkloadAccountModel != null)
                    {
                        WorkloadAccountInsert(entity, qty, cbm, weight);    //添加工人工作量
                    }

                    List<RecModeldetail> checkEch = entity.RecModeldetail.Where(u => u.UnitName.Contains("ECH")).Distinct().ToList();

                    //插入理货员
                    WorkloadAccount work1 = new WorkloadAccount();
                    work1.WhCode = entity.WhCode;
                    work1.ClientId = entity.ClientId;
                    work1.ClientCode = entity.ClientCode;
                    work1.ReceiptId = entity.ReceiptId;
                    work1.HuId = entity.HuId;
                    work1.WorkType = "理货员";
                    work1.UserCode = entity.CreateUser;
                    work1.LotFlag = entity.LotFlag;
                    work1.ReceiptDate = DateTime.Now;
                    work1.Qty = Convert.ToDecimal(qty);

                    if (checkEch.Count > 0)
                    {
                        work1.EchFlag = 1;
                    }
                    else
                    {
                        work1.EchFlag = 0;
                    }

                    work1.CBM = cbm;
                    work1.Weight = weight;
                    idal.IWorkloadAccountDAL.Add(work1);

                    //添加库存
                    //验证库存是否存在该款号，如果存在 直接附加，如果不在 新增托盘号(款号)，新增库存

                    List<HuDetail> checkHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode && u.AltItemNumber == entity.HuId);
                    if (checkHuDetailList.Count == 0)
                    {
                        List<Pallate> pallateList = idal.IPallateDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
                        if (pallateList.Count == 0)
                        {
                            Pallate pallate = new Pallate();
                            pallate.WhCode = entity.WhCode;
                            pallate.HuId = entity.HuId;
                            pallate.Status = "U";
                            pallate.TypeId = 1;
                            pallate.CreateUser = entity.CreateUser;
                            pallate.CreateDate = DateTime.Now;
                            idal.IPallateDAL.Add(pallate);
                        }
                        HuMasterInsert(entity);    //添加库存主表

                        AddReceiptTranLog(entity);//添加收货TranLog  
                    }
                    else
                    {
                        HuMaster humaster = new HuMaster();
                        List<HuMaster> checkHuMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);
                        if (checkHuMasterList.Count == 0)
                        {
                            humaster.WhCode = entity.WhCode;
                            humaster.HuId = entity.HuId;
                            humaster.Type = "R";
                            if (entity.HoldMasterModel != null)
                            {
                                if (string.IsNullOrEmpty(entity.HoldMasterModel.HoldReason))
                                {
                                    humaster.Status = "A";
                                }
                                else
                                {
                                    humaster.Status = "H";
                                    humaster.HoldId = entity.HoldMasterModel.HoldId;
                                    humaster.HoldReason = entity.HoldMasterModel.HoldReason;
                                }
                            }
                            else
                            {
                                humaster.Status = "A";
                            }

                            humaster.Location = entity.Location;
                            humaster.TransactionFlag = 0;
                            humaster.ReceiptId = entity.ReceiptId;
                            humaster.ReceiptDate = DateTime.Now;
                            humaster.CreateUser = entity.CreateUser;
                            humaster.CreateDate = DateTime.Now;
                            idal.IHuMasterDAL.Add(humaster);
                        }
                        else
                        {
                            humaster = checkHuMasterList.First();
                        }

                        HuDetailInsert(entity);

                        foreach (var item in entity.RecModeldetail)
                        {
                            TranLog tl = new TranLog();
                            tl.TranType = "104";
                            tl.Description = "收货时上架并合并";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = entity.CreateUser;
                            tl.WhCode = entity.WhCode;
                            tl.ClientCode = entity.ClientCode;
                            tl.SoNumber = entity.SoNumber;
                            tl.CustomerPoNumber = entity.CustomerPoNumber;
                            tl.PoID = entity.PoId;
                            tl.AltItemNumber = item.AltItemNumber;
                            tl.ItemId = item.ItemId;
                            tl.UnitID = item.UnitId;
                            tl.UnitName = item.UnitName;
                            tl.Status = entity.Status;
                            tl.TranQty2 = item.Qty;
                            tl.HuId = entity.HuId;
                            tl.LotFlag = entity.LotFlag;
                            tl.Length = item.Length / 100;
                            tl.Width = item.Width / 100;
                            tl.Height = item.Height / 100;
                            tl.Weight = item.Weight;
                            tl.LotNumber1 = item.LotNumber1;
                            tl.LotNumber2 = item.LotNumber2;
                            tl.LotDate = item.LotDate;
                            tl.ReceiptId = entity.ReceiptId;
                            tl.ReceiptDate = DateTime.Now;
                            tl.Location = entity.Location;
                            tl.Location2 = humaster.Location;
                            tl.HoldId = entity.HoldMasterModel == null ? 0 : entity.HoldMasterModel.HoldId;
                            tl.HoldReason = entity.HoldMasterModel == null ? null : entity.HoldMasterModel.HoldReason;

                            idal.ITranLogDAL.Add(tl);
                        }
                    }

                    //收货时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有货损托盘
                    List<Receipt> checkReceiptTCRList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.Status == "H" && u.HuId == entity.HuId);
                    if (checkReceiptTCRList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptTCRList)
                        {
                            PhotoMaster photoMaster = new PhotoMaster();

                            photoMaster.PhotoId = 0;
                            photoMaster.WhCode = item.WhCode;
                            photoMaster.ClientCode = item.ClientCode;
                            photoMaster.Number = item.ReceiptId;
                            photoMaster.Number2 = item.SoNumber;
                            photoMaster.Number3 = item.CustomerPoNumber;
                            photoMaster.Number4 = item.AltItemNumber;

                            photoMaster.PoId = item.PoId;
                            photoMaster.ItemId = item.ItemId;
                            photoMaster.UnitName = item.UnitName;
                            photoMaster.Qty = item.Qty;
                            photoMaster.RegQty = 0;
                            photoMaster.HuId = item.HuId;
                            photoMaster.HoldReason = item.HoldReason;
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
                    }
                    #endregion
                    //TCR结束

                    #endregion

                    idal.IReceiptDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "收货异常，请重新提交！";
                }
            }
        }


        //拦截订单快捷收货,type 0代表单品，其余为多品
        public string ReceiptByOutOrderIntercept(ReceiptInsert entity, int type)
        {
            if (entity.RecModeldetail == null)
            {
                return "错误！没有货物明细！";
            }
            if (string.IsNullOrEmpty(entity.WhCode))
            {
                return "错误！请提供仓库编码！";
            }
            if (string.IsNullOrEmpty(entity.CreateUser))
            {
                return "错误！请提供创建人！";
            }

            lock (o)
            {
                List<RecModeldetail> RecModeldetail1 = new List<RecModeldetail>();

                List<ItemMaster> ItemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode);
                string result = "";

                #region 拦截收货的款号验证
                foreach (var item3 in entity.RecModeldetail)
                {
                    if (ItemMasterList.Where(u => u.AltItemNumber == item3.AltItemNumber || (u.EAN ?? "") == item3.AltItemNumber).Count() == 0)
                    {
                        result = "款号或EAN有误！" + item3.AltItemNumber;
                        break;
                    }

                    if (ItemMasterList.Where(u => u.AltItemNumber == item3.AltItemNumber || (u.EAN ?? "") == item3.AltItemNumber).Count() > 1)
                    {
                        result = "款号或EAN检索出俩条以上数据，无法选取！" + item3.AltItemNumber;
                        break;
                    }

                    ItemMaster itemMaster = ItemMasterList.Where(u => u.AltItemNumber == item3.AltItemNumber || (u.EAN ?? "") == item3.AltItemNumber).First();
                    RecModeldetail recModelDetail = item3;
                    recModelDetail.AltItemNumber = itemMaster.AltItemNumber;
                    recModelDetail.ItemId = itemMaster.Id;
                    recModelDetail.UnitId = itemMaster.UnitFlag;
                    recModelDetail.UnitName = itemMaster.UnitName;

                    recModelDetail.Length = 0;
                    recModelDetail.Width = 0;
                    recModelDetail.Height = 0;
                    recModelDetail.Weight = 0;
                    RecModeldetail1.Add(recModelDetail);

                }
                if (result != "")
                {
                    return result;
                }
                #endregion

                WorkloadAccountModel w1 = new WorkloadAccountModel();
                w1.WorkType = "理货员";
                w1.UserCode = entity.CreateUser;

                List<WorkloadAccountModel> WorkloadAccountModel = new List<BLLClass.WorkloadAccountModel>();
                WorkloadAccountModel.Add(w1);

                entity.WorkloadAccountModel = WorkloadAccountModel;
                entity.RecModeldetail = RecModeldetail1;

                List<OutBoundOrderDetail> sql = new List<OutBoundOrderDetail>();
                //type 为0代表单品
                if (type == 0)
                {
                    //单品订单拦截收货
                    #region
                    sql = (from a in idal.IOutBoundOrderDAL.SelectAll()
                           join c in idal.IOutBoundOrderDetailDAL.SelectAll()
                           on a.Id equals c.OutBoundOrderId
                           join b in idal.IFlowHeadDAL.SelectAll()
                           on a.ProcessId equals b.Id
                           where a.WhCode == entity.WhCode && a.StatusId == -10 && a.StatusName == "已拦截重新生成"
                           && b.FlowName.Contains("单品")
                           select c).OrderBy(u => u.OutBoundOrderId).ToList();

                    if (sql.Count == 0)
                    {
                        return "没有需要拦截收货的单品订单，请先生成收货批次！";
                    }

                    int count = 0;  //验证款号个数
                    int outboundOrderId = 0;

                    string altItemNumber = "";
                    foreach (var item in entity.RecModeldetail)
                    {
                        if (sql.Where(u => u.ItemId == item.ItemId).Count() > 0)
                        {
                            outboundOrderId = sql.Where(u => u.ItemId == item.ItemId).First().OutBoundOrderId;
                        }
                        count++;
                        altItemNumber += item.AltItemNumber + ",";
                    }

                    if (altItemNumber != "")
                    {
                        altItemNumber = altItemNumber.Substring(0, altItemNumber.Length - 1);
                    }
                    if (count > 1)
                    {
                        return "当前所选单品流程，但出现多个款号：" + altItemNumber;
                    }

                    if (outboundOrderId == 0)
                    {
                        return "没有找到该款号的拦截信息：" + altItemNumber;
                    }

                    List<OutBoundOrder> outBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outboundOrderId);
                    if (outBoundOrderList.Where(u => u.StatusName == "已拦截重新生成").Count() == 0)
                    {
                        return "匹配到的订单状态有误，请重新扫描！";
                    }

                    OutBoundOrder outBoundOrder = outBoundOrderList.First();
                    entity.ReceiptId = outBoundOrder.ReceiptId;
                    entity.ClientId = outBoundOrder.ClientId;
                    entity.ClientCode = outBoundOrder.ClientCode;
                    entity.CustomerPoNumber = outBoundOrder.CustomerOutPoNumber;
                    entity.LotFlag = 0;
                    entity.Status = "A";

                    ReceiptRegisterDetail receiptRegisterDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.CustomerPoNumber == entity.CustomerPoNumber).First();
                    entity.PoId = receiptRegisterDetail.PoId;

                    InBoundOrder inboundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.Id == entity.PoId).First();
                    if ((inboundOrder.SoId ?? 0) != 0)
                    {
                        entity.SoNumber = idal.IInBoundSODAL.SelectBy(u => u.Id == inboundOrder.SoId).First().SoNumber;
                    }

                    #endregion

                    //执行收货
                    string result1 = ReceiptInsert(entity);
                    if (result1 != "Y")
                    {
                        return result1;
                    }

                    Thread.Sleep(400); //停0.4秒

                    //执行完成收货
                    result1 = RecComplete(entity.ReceiptId, entity.WhCode, entity.CreateUser);
                    if (result1 != "Y")
                    {
                        return result1;
                    }

                    return "Y";
                }
                else
                {
                    //多品订单拦截收货
                    #region
                    sql = (from a in idal.IOutBoundOrderDAL.SelectAll()
                           join c in idal.IOutBoundOrderDetailDAL.SelectAll()
                           on a.Id equals c.OutBoundOrderId
                           join b in idal.IFlowHeadDAL.SelectAll()
                           on a.ProcessId equals b.Id
                           where a.WhCode == entity.WhCode && a.StatusId == -10 && a.StatusName == "已拦截重新生成"
                           && b.FlowName.Contains("多品")
                           select c).OrderBy(u => u.OutBoundOrderId).ToList();

                    if (sql.Count == 0)
                    {
                        return "没有需要拦截收货的多品订单，请先生成收货批次！";
                    }

                    int count = 0;  //验证款号个数
                    int outboundOrderId = 0;

                    string altItemNumber = "";
                    foreach (var item in entity.RecModeldetail.OrderBy(u => u.AltItemNumber))
                    {
                        count++;
                        altItemNumber += item.AltItemNumber + ",";
                    }

                    if (count == 1)
                    {
                        return "当前所选多品流程，但仅有一个款号：" + altItemNumber;
                    }

                    List<OutBoundOrderDetail> ResultList = new List<OutBoundOrderDetail>();
                    //得到订单ID
                    int[] outBoundOrderIdArr = (from a in sql
                                                select a.OutBoundOrderId).ToList().Distinct().ToArray();
                    //循环订单ID 会减少一些不必要的循环
                    foreach (var itemOrderId in outBoundOrderIdArr)
                    {
                        List<OutBoundOrderDetail> getOutBoundOrderDetailList2 = sql.Where(u => u.OutBoundOrderId == itemOrderId).ToList();

                        //万一 订单主表或订单明细表数据不一致，跳过异常订单
                        if (getOutBoundOrderDetailList2.Count == 0)
                        {
                            continue;
                        }

                        OutBoundOrderDetail outOrder1 = new OutBoundOrderDetail();

                        //把订单和款号绑定起来，后面才好根据款号来排序
                        foreach (var itemOrder in getOutBoundOrderDetailList2.OrderBy(u => u.AltItemNumber))
                        {
                            outOrder1.OutBoundOrderId = itemOrderId;
                            outOrder1.AltItemNumber += itemOrder.AltItemNumber + ",";
                        }

                        ResultList.Add(outOrder1);
                    }

                    if (ResultList.Where(u => u.AltItemNumber == altItemNumber).Count() == 0)
                    {
                        return "没有找到该款号的拦截信息：" + altItemNumber;
                    }
                    else
                    {
                        outboundOrderId = ResultList.Where(u => u.AltItemNumber == altItemNumber).First().OutBoundOrderId;
                    }

                    if (outboundOrderId == 0)
                    {
                        return "没有找到该款号的拦截信息：" + altItemNumber;
                    }

                    List<OutBoundOrder> outBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outboundOrderId);
                    if (outBoundOrderList.Where(u => u.StatusName == "已拦截重新生成").Count() == 0)
                    {
                        return "匹配到的订单状态有误，请重新扫描！";
                    }

                    OutBoundOrder outBoundOrder = outBoundOrderList.First();
                    entity.ReceiptId = outBoundOrder.ReceiptId;
                    entity.ClientId = outBoundOrder.ClientId;
                    entity.ClientCode = outBoundOrder.ClientCode;
                    entity.CustomerPoNumber = outBoundOrder.CustomerOutPoNumber;
                    entity.LotFlag = 0;
                    entity.Status = "A";

                    ReceiptRegisterDetail receiptRegisterDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.CustomerPoNumber == entity.CustomerPoNumber).First();
                    entity.PoId = receiptRegisterDetail.PoId;

                    InBoundOrder inboundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.Id == entity.PoId).First();
                    if ((inboundOrder.SoId ?? 0) != 0)
                    {
                        entity.SoNumber = idal.IInBoundSODAL.SelectBy(u => u.Id == inboundOrder.SoId).First().SoNumber;
                    }
                    #endregion

                    //执行收货
                    string result1 = ReceiptInsert(entity);
                    if (result1 != "Y")
                    {
                        return result1;
                    }

                    Thread.Sleep(400); //停0.4秒

                    //执行完成收货
                    result1 = RecComplete(entity.ReceiptId, entity.WhCode, entity.CreateUser);
                    if (result1 != "Y")
                    {
                        return result1;
                    }

                    return "Y";
                }
            }
        }


        //收货收费信息
        public string ReceiptCharge(string ReceiptId, string WhCode, string CreateUser)
        {
            List<ReceiptRegister> RegList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);

            List<ContractForm> contractList1 = new List<ContractForm>();

            if (RegList.Count == 0)
            {
                return "N";
            }

            ReceiptRegister reg = RegList.First();

            string clientCode = reg.ClientCode;
            int clientId = reg.ClientId;
            string transportType = reg.TransportType;
            int? dnCount = (reg.DNCount == null ? 1 : reg.DNCount);



            WhClient client = idal.IWhClientDAL.SelectBy(u => u.Id == clientId).First();
            if (!string.IsNullOrEmpty(client.ContractName))
            {
                contractList1 = idal.IContractFormDAL.SelectBy(u => u.WhCode == WhCode && u.ContractName == client.ContractName);
            }
            else
            {
                return "N";
            }

            List<Receipt> RecList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
            List<Holiday> holidayList = idal.IHolidayDAL.SelectAll().Distinct().ToList();

            List<WhAgent> agentList = (from a in idal.IWhAgentDAL.SelectAll()
                                       join b in idal.IR_WhClient_WhAgentDAL.SelectAll()
                                       on a.Id equals b.AgentId
                                       where b.ClientId == client.Id
                                       select a).ToList();
            try
            {
                int holidayFlag = 0;
                string holidayString = "否";
                int nightTimeFlag = 0;
                string nightTimeString = "白班";

                #region 开始计算

                string datetimeNow = "", datetimeNow1 = "", nighttimeNow = "";
                if (DateTime.Now < Convert.ToDateTime("2023-07-24"))
                {
                    //1.验证是否节假日
                    datetimeNow = Convert.ToDateTime(reg.EndReceiptDate).ToString("d");    //2020/1/1 格式化
                }
                else
                {
                    datetimeNow = Convert.ToDateTime(reg.PrintDate).ToString("d");    //2020/1/1 格式化
                }

                if (holidayList.Where(u => u.DayBegin == datetimeNow).Count() > 0)
                {
                    holidayFlag = 1;
                    holidayString = "是";
                }

                //2.验证是否夜班
                //如果打印时间为空，默认取创建时间后3分钟为打印时间
                if (reg.PrintDate == null)
                {
                    reg.PrintDate = Convert.ToDateTime(reg.RegisterDate).AddMinutes(3);
                    idal.IReceiptRegisterDAL.UpdateByExtended(u => u.Id == reg.Id, t => new ReceiptRegister { PrintDate = reg.PrintDate });
                }

                datetimeNow1 = DateTime.Now.AddDays(0).ToString("d");    //2020/1/1 格式化

                if (DateTime.Now < Convert.ToDateTime("2023-07-24"))
                {
                    nighttimeNow = datetimeNow1 + " " + Convert.ToDateTime(reg.EndReceiptDate).ToString("t"); //t 格式： 9:26
                }
                else
                {
                    nighttimeNow = datetimeNow1 + " " + Convert.ToDateTime(reg.PrintDate).ToString("t"); //t 格式： 9:26
                }

                if (!string.IsNullOrEmpty(client.NightTime))
                {
                    string nightbegin = datetimeNow1 + " " + client.NightTime.Substring(0, 5);
                    string nightend = datetimeNow1 + " " + client.NightTime.Substring(6, 5);

                    if (Convert.ToInt32(client.NightTime.Substring(0, 2)) > Convert.ToInt32(client.NightTime.Substring(6, 2)))
                    {
                        //20:00-08:00
                        if (Convert.ToDateTime(nighttimeNow) >= Convert.ToDateTime(nightbegin) || Convert.ToDateTime(nighttimeNow) < Convert.ToDateTime(nightend))
                        {
                            nightTimeFlag = 1;
                            nightTimeString = "夜班";
                        }
                    }
                    else
                    {
                        //00:00-08:00
                        if (Convert.ToDateTime(nighttimeNow) >= Convert.ToDateTime(nightbegin) && Convert.ToDateTime(nighttimeNow) < Convert.ToDateTime(nightend))
                        {
                            nightTimeFlag = 1;
                            nightTimeString = "夜班";
                        }
                    }

                }

                DateTime s = Convert.ToDateTime(datetimeNow1);

                List<ContractForm> contractList = contractList1.Where(u => u.Type == "卸货" && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList();

                string result = "";

                List<ReceiptCharge> ReceiptChargeAddList = new List<ReceiptCharge>();
                List<ReceiptChargeDetail> ReceiptChargeDetailAddList = new List<ReceiptChargeDetail>();
                List<ContractForm> getContractList = new List<ContractForm>();

                //1.计算卸货费
                #region 卸货费

                List<ReceiptResult> xiehuoList = (from a in RecList
                                                  group a by new
                                                  {
                                                      a.ReceiptId,
                                                      a.UnitName,
                                                      a.Length,
                                                      a.Width,
                                                      a.Height,
                                                      a.HuId
                                                  } into g
                                                  select new ReceiptResult
                                                  {
                                                      ReceiptId = g.Key.ReceiptId,
                                                      UnitName = g.Key.UnitName == "LP" ? "托" :
                                                                   g.Key.UnitName == "CTN" ? "箱" :
                                                                   g.Key.UnitName == "EA" ? "件" :
                                                                   g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                   g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                   g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                   g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                      CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                      Weight = g.Sum(p => p.Qty * (p.Weight ?? 0)),
                                                      Qty = g.Sum(p => p.Qty)
                                                  }).ToList();

                if (xiehuoList.Count > 0)
                {
                    int count = 1;

                    List<ContractForm> checkLadderNumber = new List<ContractForm>();
                    List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                    foreach (var item in xiehuoList)
                    {
                        //验证卸货单位
                        getContractList = contractList.Where(u => u.ChargeName.Contains(item.UnitName) || u.ChargeName.Contains("所有")).ToList();

                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + "没有找到合同明细,无法计算费用";
                            break;
                        }

                        //验证车辆类型
                        getContractList = getContractList.Where(u => u.TransportType.Contains(transportType) || u.TransportType.Contains("所有")).ToList();
                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + "/所有 没有找到合同明细,无法计算费用";
                            break;
                        }

                        //验证班次
                        getContractList = getContractList.Where(u => u.Shift.Contains(nightTimeString) || u.Shift.Contains("所有")).ToList();
                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + "/所有,班次:" + nightTimeString + "/所有 没有找到合同明细,无法计算费用";
                            break;
                        }

                        if (count == 1)
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "U";
                            recCharge.Description = "";
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            ReceiptChargeAddList.Add(recCharge);
                        }

                        count++;

                        //计算卸货费
                        if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains("所有")).Count() == 0)
                        {
                            //7种情况 新增收费明细
                            //所有 所有 夜班
                            //所有 平板 所有
                            //箱货 所有 所有

                            //所有 平板 夜班
                            //箱货 所有 夜班
                            //箱货 平板 所有

                            //箱货 平板 夜班

                            //1.所有 所有 夜班
                            if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:" + nightTimeString + "立方及重量数值：" + item.CBM + "/" + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:" + nightTimeString + " 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }


                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }

                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }

                            }
                            //2.所有 平板 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:所有 立方及重量数值：" + item.CBM + " / " + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:所有 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:所有 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }
                            }
                            //3.箱货 所有 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:所有 立方及重量数值：" + item.CBM + " / " + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:所有 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:所有 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }
                            }
                            //4.所有 平板 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:" + nightTimeString + "立方及重量数值：" + item.CBM + "/" + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:" + nightTimeString + " 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }
                            }
                            //5.箱货 所有 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:" + nightTimeString + "立方及重量数值：" + item.CBM + "/" + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:" + nightTimeString + " 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }

                            }
                            //6.箱货 平板 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:所有 立方及重量数值：" + item.CBM + " / " + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:所有 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:所有 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }
                            }
                            //7.箱货 平板 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + "立方及重量数值：" + item.CBM + "/" + item.Weight + " 重量或立方匹配合同信息异常！";
                                    break;
                                }

                                if (checkContractCount.Count > 1)
                                {
                                    if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                        break;
                                    }
                                    else
                                    {
                                        string[] groupId = (from a in checkContractCount
                                                            select a.GroupId).ToList().Distinct().ToArray();
                                        if (groupId.Length > 1)
                                        {
                                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + " 组号不同,无法计算费用";
                                            break;
                                        }
                                    }
                                }

                                foreach (var contract in checkContractCount)
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }


                                    if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                    {
                                        checkLadderNumber.Add(contract);
                                        checkReceiptChargeDetail.Add(recChargeDetail);
                                    }
                                    else
                                    {
                                        ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                        checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                        ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                        newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                        newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                        newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                        newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                        checkReceiptChargeDetail.Add(newrecChargeDetail);
                                    }
                                }
                            }
                            else
                            {
                                result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + " 没有找到合同明细,无法计算费用";
                                break;
                            }
                        }
                        else
                        {
                            List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                            if (checkContractCount.Count == 0)
                            {
                                result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:所有 重量数值：" + item.Weight + "立方数值：" + item.CBM + " 未找到合同明细,无法计算费用";
                                break;
                            }

                            if (checkContractCount.Count > 1)
                            {
                                if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:所有 存在多行合同明细,无法计算费用";
                                    break;
                                }
                                else
                                {
                                    string[] groupId = (from a in checkContractCount
                                                        select a.GroupId).ToList().Distinct().ToArray();
                                    if (groupId.Length > 1)
                                    {
                                        result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:所有 组号不同,无法计算费用";
                                        break;
                                    }
                                }
                            }

                            foreach (var contract in checkContractCount)
                            {

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                //插入收费表明细
                                recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                //验证合同阶梯数值
                                result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                if (result != "")
                                {
                                    break;
                                }
                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }


                                if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.LadderNumberBeginCBM == contract.LadderNumberBeginCBM && u.LadderNumberEndCBM == contract.LadderNumberEndCBM && u.ChargeUnitName == contract.ChargeUnitName && u.ChargeName == contract.ChargeName).Count() == 0)
                                {
                                    checkLadderNumber.Add(contract);
                                    checkReceiptChargeDetail.Add(recChargeDetail);
                                }
                                else
                                {
                                    ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM) && u.ChargeName == contract.ChargeName).First();

                                    checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                    ReceiptChargeDetail newrecChargeDetail = recChargeDetail;
                                    newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                    newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                    newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                    newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                    checkReceiptChargeDetail.Add(newrecChargeDetail);
                                }
                            }
                        }
                    }

                    decimal? sumCbm = 0;
                    foreach (var item in checkReceiptChargeDetail)
                    {
                        if (item.ChargeUnitName == "立方")
                        {
                            sumCbm += item.CBM;
                        }
                    }

                    foreach (var item in checkReceiptChargeDetail)
                    {
                        if (item.ChargeUnitName == "立方")
                        {
                            if (item.CBM > 0 && item.CBM < 1 && sumCbm < 1)
                            {
                                item.CBM = 1;
                                item.PriceTotal = item.Price * 1;
                            }
                            else if (sumCbm > 1)
                            {
                                item.CBM = Math.Ceiling(Convert.ToDecimal(item.CBM));
                                item.PriceTotal = item.Price * item.CBM;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(item);

                    }
                }

                if (result != "")
                {
                    ReceiptCharge recCharge = new ReceiptCharge();
                    recCharge.ClientCode = clientCode;
                    recCharge.ReceiptId = ReceiptId;
                    recCharge.WhCode = WhCode;
                    recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                    recCharge.Status = "N";
                    recCharge.Description = "类型:卸货-" + result;
                    recCharge.CreateUser = CreateUser;
                    recCharge.CreateDate = DateTime.Now;
                    idal.IReceiptChargeDAL.Add(recCharge);
                    idal.IReceiptChargeDAL.SaveChanges();
                    return "N";
                }
                #endregion


                //1.计算卸货费
                #region 卸货费-大件

                List<ReceiptResult> xiehuoList1 = (from a in RecList
                                                   where a.UnitName == "EA-BIG1"
                                                   group a by new
                                                   {
                                                       a.ReceiptId,
                                                       a.UnitName,
                                                       a.Weight,
                                                       a.HuId
                                                   } into g
                                                   select new ReceiptResult
                                                   {
                                                       ReceiptId = g.Key.ReceiptId,
                                                       UnitName = g.Key.UnitName == "LP" ? "托" :
                                                                    g.Key.UnitName == "CTN" ? "箱" :
                                                                    g.Key.UnitName == "EA" ? "件" :
                                                                    g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                    g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                    g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                    g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                       CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                       Weight = g.Sum(p => (p.Weight ?? 0)),
                                                       Qty = g.Sum(p => p.Qty)
                                                   }).ToList();

                if (xiehuoList1.Count > 0)
                {
                    getContractList = new List<ContractForm>();

                    List<ContractForm> checkLadderNumber = new List<ContractForm>();
                    List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                    foreach (var item in xiehuoList1)
                    {
                        //验证卸货单位
                        getContractList = contractList.Where(u => u.ChargeName.Contains(item.UnitName) || u.ChargeName.Contains("所有")).ToList();

                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + "没有找到合同明细,无法计算费用";
                            break;
                        }

                        //验证车辆类型
                        getContractList = getContractList.Where(u => u.TransportType.Contains(transportType) || u.TransportType.Contains("所有")).ToList();
                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + "/所有 没有找到合同明细,无法计算费用";
                            break;
                        }

                        //验证班次
                        getContractList = getContractList.Where(u => u.Shift.Contains(nightTimeString) || u.Shift.Contains("所有")).ToList();
                        if (getContractList.Count == 0)
                        {
                            result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + "/所有,班次:" + nightTimeString + "/所有 没有找到合同明细,无法计算费用";
                            break;
                        }

                        //计算卸货费
                        if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains("所有")).Count() == 0)
                        {
                            //7种情况 新增收费明细
                            //所有 所有 夜班
                            //所有 平板 所有
                            //箱货 所有 所有

                            //所有 平板 夜班
                            //箱货 所有 夜班
                            //箱货 平板 所有

                            //箱货 平板 夜班

                            //1.所有 所有 夜班
                            if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:所有,班次:" + nightTimeString + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }

                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //2.所有 平板 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:所有 " + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {

                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //3.箱货 所有 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:所有 " + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //4.所有 平板 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:所有,车辆类型:" + transportType + ",班次:" + nightTimeString + " " + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //5.箱货 所有 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains("所有") && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:所有,班次:" + nightTimeString + " " + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //6.箱货 平板 所有
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains("所有")).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:所有" + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            //7.箱货 平板 夜班
                            else if (getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString)).Count() > 0)
                            {
                                List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains(item.UnitName) && u.TransportType.Contains(transportType) && u.Shift.Contains(nightTimeString) && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                                if (checkContractCount.Count == 0)
                                {
                                    result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + "" + ",数值:" + item.Weight + " 没有找到合同明细,无法计算费用";
                                    break;
                                }

                                foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                    //插入收费表明细
                                    recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                    //验证合同阶梯数值
                                    result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                    if (result != "")
                                    {
                                        break;
                                    }
                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                    }

                                    if ((contract.MaxPriceTotal ?? 0) > 0)
                                    {
                                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                        {
                                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                        }
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                            else
                            {
                                result = "合同名:" + client.ContractName + ",卸货单位:" + item.UnitName + ",车辆类型:" + transportType + ",班次:" + nightTimeString + " 没有找到合同明细,无法计算费用";
                                break;
                            }
                        }
                        else
                        {
                            List<ContractForm> checkContractCount = getContractList.Where(u => u.ChargeName.Contains("所有") && u.TransportType.Contains("所有") && u.Shift.Contains("所有") && (u.LadderNumberBegin ?? 0) <= item.Weight && (u.LadderNumberEnd ?? 0) > item.Weight && (u.LadderNumberBeginCBM ?? 0) <= item.CBM && (u.LadderNumberEndCBM ?? 0) > item.CBM).ToList();

                            foreach (var contract in checkContractCount.OrderBy(u => u.LadderNumberBegin))
                            {
                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();

                                //插入收费表明细
                                recChargeDetailInsert(ReceiptId, WhCode, CreateUser, clientCode, transportType, nightTimeFlag, item, contract, recChargeDetail, holidayString);

                                //验证合同阶梯数值
                                result = checkContractLadderNumber(client, result, item, contract, recChargeDetail);

                                if (result != "")
                                {
                                    break;
                                }
                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                ReceiptChargeDetailAddList.Add(recChargeDetail);
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }
                }

                #endregion

                contractList = contractList1.Where(u => u.Type == "卸货附加" && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList();
                //2.计算卸货附加费
                #region 分票费
                //分票SO
                //分票PO
                //分票SKU
                //分票SKU + 1
                //分票SKU + 2
                //分票SKU + 3
                //分票SKU + 4
                //分票SKU + 5
                //分票SKU + 6

                List<ReceiptResult> RecLotFlagList = (from a in RecList
                                                      where a.LotFlag > 0
                                                      group a by new
                                                      {
                                                          a.ReceiptId,
                                                          a.LotFlag
                                                      } into g
                                                      select new ReceiptResult
                                                      {
                                                          ReceiptId = g.Key.ReceiptId,
                                                          LotFlag = g.Key.LotFlag,
                                                          LotFlagShow =
                                                                   g.Key.LotFlag == 2 ? "分票PO" :
                                                                   g.Key.LotFlag == 3 ? "分票SKU" :
                                                                   g.Key.LotFlag == 1 ? "分票属性1" :
                                                                   g.Key.LotFlag == 5 ? "分票属性2" :
                                                                   g.Key.LotFlag == 6 ? "分票属性3" :
                                                                   g.Key.LotFlag == 4 ? "分PO+SKU" :
                                                                   g.Key.LotFlag == 7 ? "分PO+SKU+属1" :
                                                                   g.Key.LotFlag == 8 ? "分PO+SKU+属1+属2" :
                                                                   g.Key.LotFlag == 9 ? "分PO+SKU+属1+属2+属3" : null,
                                                          CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                          Weight = g.Sum(p => p.Qty * (p.Weight ?? 0)),
                                                          Qty = g.Sum(p => p.Qty)
                                                      }).OrderBy(u => u.LotFlag).ToList();

                getContractList = new List<ContractForm>();

                foreach (var item in RecLotFlagList)
                {
                    //验证分票类型是否存在
                    getContractList = contractList.Where(u => u.ChargeName == item.LotFlagShow).ToList();

                    if (getContractList.Count == 0)
                    {
                        result = "合同名:" + client.ContractName + ",分票类型:" + item.LotFlagShow + "没有找到合同明细,无法计算费用";
                        break;
                    }
                    else if (getContractList.Count > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",分票类型:" + item.LotFlagShow + "存在多行合同明细,无法计算费用";
                            break;
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",分票类型:" + item.LotFlagShow + " 组号不同,无法计算费用";
                                break;
                            }
                        }
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = item.LotFlagShow;
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = item.UnitName;
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = item.Qty;
                        recChargeDetail.CBM = item.CBM;
                        recChargeDetail.Weight = item.Weight;
                        recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                        recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                        recChargeDetail.Price = contract.Price;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        //验证合同阶梯数值
                        result = checkContractLadderNumber1(client, result, item, contract, recChargeDetail);

                        if (result != "")
                        {
                            break;
                        }
                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }


                if (result != "")
                {
                    ReceiptCharge recCharge = new ReceiptCharge();
                    recCharge.ClientCode = clientCode;
                    recCharge.ReceiptId = ReceiptId;
                    recCharge.WhCode = WhCode;
                    recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                    recCharge.Status = "N";
                    recCharge.Description = "类型:卸货附加-" + result;
                    recCharge.CreateUser = CreateUser;
                    recCharge.CreateDate = DateTime.Now;
                    idal.IReceiptChargeDAL.Add(recCharge);
                    idal.IReceiptChargeDAL.SaveChanges();
                    return "N";
                }
                #endregion


                //3.计算传真费
                #region 传真费
                int faxInCount = reg.FaxInCount ?? 0;
                int faxOutCount = reg.FaxOutCount ?? 0;

                if (faxInCount > 0)
                {
                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("传真费-收")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",传真费-收 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",传真费-收 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",传真费-收 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "传真费-收";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = faxInCount;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "份";
                        recChargeDetail.LadderNumber = "";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * faxInCount;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                if (faxOutCount > 0)
                {

                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("传真费-发")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",传真费-发 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",传真费-发 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",传真费-发 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "传真费-发";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = faxOutCount;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "份";
                        recChargeDetail.LadderNumber = "";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * faxOutCount;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }
                #endregion


                //4.计算缠绕膜费
                #region 缠绕膜费

                //箱货托盘数

                if (RecList.Count > 0)
                {
                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("缠绕膜费")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",缠绕膜费 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",缠绕膜费 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",缠绕膜费 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    List<ContractForm> getContractList2 = new List<ContractForm>();
                    getContractList2 = contractList.Where(u => u.ChargeName.Contains("缠绕膜托盘货物数") && u.Price > 0).ToList();

                    List<ReceiptResult> ctnHuIdList = new List<ReceiptResult>();
                    if (getContractList2.Count > 0)
                    {
                        List<Receipt> RecList1 = RecList.Where(u => u.Qty > 1).ToList();
                        ctnHuIdList = (from a in RecList1
                                       where a.UnitName == "CTN"
                                       group a by new
                                       {
                                           a.ReceiptId,
                                           a.HuId
                                       } into g
                                       select new ReceiptResult
                                       {
                                           ReceiptId = g.Key.ReceiptId,
                                           HuId = g.Key.HuId,
                                           CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height)
                                       }).ToList();
                    }
                    else
                    {
                        ctnHuIdList = (from a in RecList
                                       where a.UnitName == "CTN"
                                       group a by new
                                       {
                                           a.ReceiptId,
                                           a.HuId
                                       } into g
                                       select new ReceiptResult
                                       {
                                           ReceiptId = g.Key.ReceiptId,
                                           HuId = g.Key.HuId,
                                           CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height)
                                       }).ToList();
                    }

                    decimal setCBM = Convert.ToDecimal(0.25);
                    string[] huIdList = (from a in ctnHuIdList
                                         where a.CBM >= setCBM
                                         select a.HuId).ToList().Distinct().ToArray();

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "缠绕膜费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "个";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = huIdList.Count();
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "箱货托盘数";
                        recChargeDetail.LadderNumber = "大于等于0.25立方";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * huIdList.Count();

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion


                //5.计算车辆管理费
                #region 车辆管理费

                if ((reg.TruckCount ?? 0) > 0)
                {
                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("车辆管理费")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",车辆管理费 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",车辆管理费 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",车辆管理费 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "车辆管理费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = reg.TruckCount;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "辆";

                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * reg.TruckCount;
                        recChargeDetail.LadderNumber = "";

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion



                //5.1 计算车辆管理超长加收费
                #region 车辆管理超长加收费

                if ((reg.TruckCount ?? 0) > 0 && !string.IsNullOrEmpty(reg.TransportTypeExtend))
                {

                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("车辆管理超长加收费")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",车辆管理超长加收费 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",车辆管理超长加收费 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",车辆管理超长加收费 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "车辆管理超长加收费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = reg.TruckCount;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "辆";

                        //如果有超长类 那么需附加收费，取得ContractFormExtend表中的车辆管理超长加收费 进行单价附加
                        List<ContractFormExtend> getList = idal.IContractFormExtendDAL.SelectBy(u => u.WhCode == reg.WhCode && u.ChargeName == "车辆管理费");
                        if (contract.Price == 0)
                        {
                            recChargeDetail.Price = 0;
                            recChargeDetail.PriceTotal = 0;
                            recChargeDetail.LadderNumber = "该合同不加收车辆管理超长加收费";
                        }
                        else if (getList.Count == 0)
                        {
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * reg.TruckCount;
                            recChargeDetail.LadderNumber = "超长车" + reg.TransportTypeExtend + "加收" + contract.Price + "元/辆";
                        }
                        else
                        {
                            ContractFormExtend getFirst = getList.First();

                            recChargeDetail.Price = getFirst.Price;
                            recChargeDetail.PriceTotal = (getFirst.Price) * reg.TruckCount;
                            recChargeDetail.LadderNumber = "超长车" + reg.TransportTypeExtend + "加收" + getFirst.Price + "元/辆";
                        }

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion


                //6.计算打单费
                ContractForm CheckMonthFlagContract = new ContractForm();

                #region 打单费
                //打单费存在有 根据DN数来的，也有根据SO数来的，也有根据DN算一份来的

                List<string> soCountList = (from a in RecList
                                            select a.SoNumber).ToList().Distinct().ToList();

                if (soCountList.Count > 0 && ((reg.TruckCount ?? 0) > 0 || (reg.DNCount ?? 0) > 0))
                {
                    getContractList = new List<ContractForm>();

                    //正常收费 单价*SO个数
                    getContractList = contractList.Where(u => u.ChargeName.Contains("打单费")).ToList();

                    //打单份数 NIKE在用，一个DN最多收15个SO费用，多个DN 即 打单份数单价*DN数*打单费单价=15*3*20
                    List<ContractForm> getdadanContractList = contractList.Where(u => u.ChargeName.Contains("打单份数") && u.Price > 0).ToList();

                    //按单收费 按DN收费 M&S在用，一个DN按一个SO的价格收费，即 按单收费*打单费单价
                    List<ContractForm> getandanContractList = contractList.Where(u => u.ChargeName.Contains("按单收费") && u.Price > 0).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",打单费 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",打单费 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",打单费 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "打单费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "个";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";

                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "个";
                        recChargeDetail.LadderNumber = "DN数：" + reg.DNCount;

                        //如果有按单收费 则按DN数乘以单价，否则按 SO数乘以单价
                        if (getandanContractList.Count == 0)
                        {
                            recChargeDetail.Qty = soCountList.Count;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * soCountList.Count;
                        }
                        else
                        {
                            recChargeDetail.Qty = reg.DNCount;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * (reg.DNCount ?? 0);
                        }

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        CheckMonthFlagContract = contract;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        //如果有打单份数 则本次打单费上限总价是DN数*单价*每个DN数最多按xx个SO计费，如果无打单份数则不设上限
                        if (getdadanContractList.Count == 0)
                        {
                            contract.MaxPriceTotal = contract.Price * dnCount * 30;
                        }
                        else
                        {
                            contract.MaxPriceTotal = contract.Price * dnCount * getdadanContractList.First().Price;
                        }


                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }



                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion


                //7.计算联单费
                #region 联单费

                if ((reg.BillCount ?? 0) > 0)
                {
                    getContractList = new List<ContractForm>();

                    getContractList = contractList.Where(u => u.ChargeName.Contains("联单费")).ToList();

                    if (getContractList.Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",联单费 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count() > 1)
                    {
                        if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                        {
                            result = "合同名:" + client.ContractName + ",联单费 存在多行合同明细,无法计算费用";
                        }
                        else
                        {
                            string[] groupId = (from a in getContractList
                                                select a.GroupId).ToList().Distinct().ToArray();
                            if (groupId.Length > 1)
                            {
                                result = "合同名:" + client.ContractName + ",联单费 组号不同,无法计算费用";
                            }
                        }
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }


                    int? billCount = (reg.BillCount - 5);
                    if (billCount < 0)
                    {
                        billCount = 0;
                    }

                    int? faxFeeCount = Convert.ToInt32(Math.Ceiling(((decimal)(billCount) / 5)));

                    foreach (var contract in getContractList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "联单费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = faxFeeCount;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "每5份";
                        recChargeDetail.LadderNumber = reg.BillCount + "/5=" + faxFeeCount + "打";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * faxFeeCount;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion


                //8.计算绿色通道费
                #region 绿色通道费

                if (soCountList.Count > 0 && (reg.GreenPassFLag ?? 0) > 0)
                {
                    if (contractList.Where(u => u.ChargeName.Contains("绿色通道费")).Count() == 0)
                    {
                        result = "合同名:" + client.ContractName + ",绿色通道费 没有找到合同明细,无法计算费用";
                    }

                    List<ContractForm> getContractList1 = new List<ContractForm>();
                    //验证班次
                    getContractList1 = contractList.Where(u => u.ChargeName.Contains("绿色通道费") && (u.Shift.Contains(nightTimeString) || u.Shift.Contains("所有"))).ToList();
                    if (getContractList1.Count == 0)
                    {
                        result = "合同名:" + client.ContractName + ",绿色通道费,班次:" + nightTimeString + "/所有 没有找到合同明细,无法计算费用";
                    }
                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    List<ContractForm> checkContractCount = new List<ContractForm>();
                    //1.所有 所有 夜班
                    if (getContractList1.Where(u => u.Shift.Contains("所有")).Count() > 0)
                    {
                        checkContractCount = getContractList1.Where(u => u.Shift.Contains("所有")).ToList();
                        if (checkContractCount.Count > 1)
                        {
                            if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                            {
                                result = "合同名:" + client.ContractName + ",绿色通道费,班次:所有 存在多行合同明细,无法计算费用";
                            }
                            else
                            {
                                string[] groupId = (from a in checkContractCount
                                                    select a.GroupId).ToList().Distinct().ToArray();
                                if (groupId.Length > 1)
                                {
                                    result = "合同名:" + client.ContractName + ",绿色通道费,班次:所有 组号不同,无法计算费用";
                                }
                            }
                        }
                    }
                    else if (getContractList1.Where(u => u.Shift.Contains(nightTimeString)).Count() > 0)
                    {
                        checkContractCount = getContractList1.Where(u => u.Shift.Contains(nightTimeString)).ToList();
                        if (checkContractCount.Count > 1)
                        {
                            if (checkContractCount.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                            {
                                result = "合同名:" + client.ContractName + ",绿色通道费,班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                            }
                            else
                            {
                                string[] groupId = (from a in checkContractCount
                                                    select a.GroupId).ToList().Distinct().ToArray();
                                if (groupId.Length > 1)
                                {
                                    result = "合同名:" + client.ContractName + ",绿色通道费,班次:" + nightTimeString + " 组号不同,无法计算费用";
                                }
                            }
                        }
                    }
                    else
                    {
                        result = "合同名:" + client.ContractName + ",绿色通道费,班次:" + nightTimeString + " 没有找到合同明细,无法计算费用";
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    foreach (var contract in checkContractCount)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = ((client.Passageway ?? "") == "" ? "绿色通道" : client.Passageway) + "费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = soCountList.Count;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "SO个数";

                        recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;

                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * soCountList.Count;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                        recChargeDetail.GroupId = contract.GroupId;

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        if ((contract.MaxPriceTotal ?? 0) > 0)
                        {
                            if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                            {
                                recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                            }
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);

                    }
                }

                #endregion


                //9.计算箱货超重费
                #region 箱货超重费

                getContractList = contractList.Where(u => u.ChargeName.Contains("超重费(箱货)")).ToList();

                if (getContractList.Count > 0)
                {

                    List<ReceiptResult> RecCtnWeightList = (from a in RecList
                                                            where a.UnitName == "CTN"
                                                            group a by new
                                                            {
                                                                a.ReceiptId,
                                                                a.UnitName,
                                                                a.Weight
                                                            } into g
                                                            select new ReceiptResult
                                                            {
                                                                ReceiptId = g.Key.ReceiptId,
                                                                UnitName =
                                                                       g.Key.UnitName == "LP" ? "托" :
                                                                       g.Key.UnitName == "CTN" ? "箱" :
                                                                       g.Key.UnitName == "EA" ? "件" :
                                                                       g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                       g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                       g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                       g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                                HuWeight = (g.Key.Weight ?? 0),
                                                                CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                                Weight = g.Sum(p => p.Qty * ((p.Weight ?? 0))),
                                                                Qty = g.Sum(p => p.Qty)
                                                            }).OrderBy(u => u.LotFlag).ToList();

                    if (RecCtnWeightList.Count > 0)
                    {
                        foreach (var item in RecCtnWeightList)
                        {
                            if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",超重费(箱货),数值:" + item.HuWeight + ",车型:" + transportType + ",班次:" + nightTimeString + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                                break;
                            }
                            else if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() > 1)
                            {
                                result = "合同名:" + client.ContractName + ",超重费(箱货),数值:" + item.HuWeight + ",车型:" + transportType + ",班次:" + nightTimeString + " 合同明细存在多行,无法计算费用";
                                break;
                            }
                        }

                        if (result != "")
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "N";
                            recCharge.Description = "类型:卸货附加-" + result;
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            idal.IReceiptChargeDAL.Add(recCharge);
                            idal.IReceiptChargeDAL.SaveChanges();
                            return "N";
                        }

                        List<ContractForm> checkLadderNumber = new List<ContractForm>();
                        List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();
                        foreach (var item in RecCtnWeightList.OrderBy(u => u.HuWeight))
                        {
                            ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).OrderBy(u => u.LadderNumberBegin).First();

                            if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                            {
                                checkLadderNumber.Add(contract);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超重费(箱货)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = item.HuWeight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }

                                }
                                checkReceiptChargeDetail.Add(recChargeDetail);
                            }
                            else
                            {
                                ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                                checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超重费(箱货)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = item.HuWeight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                recChargeDetail.Qty += oldrecChargeDetail.Qty;
                                recChargeDetail.CBM += oldrecChargeDetail.CBM;
                                recChargeDetail.Weight += oldrecChargeDetail.Weight;

                                recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                                checkReceiptChargeDetail.Add(recChargeDetail);

                            }
                        }

                        foreach (var item in checkReceiptChargeDetail)
                        {
                            ReceiptChargeDetailAddList.Add(item);
                        }

                    }

                }

                #endregion


                //10.计算箱货小纸箱
                #region 箱货小纸箱

                getContractList = contractList.Where(u => u.ChargeName.Contains("小纸箱(箱货)")).ToList();

                if (getContractList.Count > 0)
                {
                    List<ReceiptResult> RecCtnSmallList = (from a in RecList
                                                           where a.UnitName == "CTN"
                                                           group a by new
                                                           {
                                                               a.ReceiptId,
                                                               a.UnitName,
                                                               a.Length,
                                                               a.Width,
                                                               a.Height
                                                           } into g
                                                           select new ReceiptResult
                                                           {
                                                               ReceiptId = g.Key.ReceiptId,
                                                               UnitName =
                                                                      g.Key.UnitName == "LP" ? "托" :
                                                                      g.Key.UnitName == "CTN" ? "箱" :
                                                                      g.Key.UnitName == "EA" ? "件" :
                                                                      g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                      g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                       g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                       g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                               HuCbm = g.Key.Length * g.Key.Width * g.Key.Height,
                                                               CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                               Weight = g.Sum(p => p.Qty * p.Weight),
                                                               Qty = g.Sum(p => p.Qty)
                                                           }).OrderBy(u => u.LotFlag).ToList();

                    if (RecCtnSmallList.Count > 0)
                    {
                        foreach (var item in RecCtnSmallList)
                        {
                            if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",小纸箱(箱货),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                                break;
                            }
                            else if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() > 1)
                            {
                                result = "合同名:" + client.ContractName + ",小纸箱(箱货),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 合同明细存在多行,无法计算费用";
                                break;
                            }
                        }

                        if (result != "")
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "N";
                            recCharge.Description = "类型:卸货附加-" + result;
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            idal.IReceiptChargeDAL.Add(recCharge);
                            idal.IReceiptChargeDAL.SaveChanges();
                            return "N";
                        }

                        List<ContractForm> checkLadderNumber = new List<ContractForm>();
                        List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                        foreach (var item in RecCtnSmallList)
                        {
                            ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).OrderBy(u => u.LadderNumberBegin).First();

                            if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                            {
                                checkLadderNumber.Add(contract);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "小纸箱(箱货)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                checkReceiptChargeDetail.Add(recChargeDetail);
                            }
                            else
                            {
                                ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                                checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "小纸箱(箱货)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.HuCbm;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                recChargeDetail.Qty += oldrecChargeDetail.Qty;
                                recChargeDetail.CBM += oldrecChargeDetail.CBM;
                                recChargeDetail.Weight += oldrecChargeDetail.Weight;

                                recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                                checkReceiptChargeDetail.Add(recChargeDetail);
                            }
                        }

                        foreach (var item in checkReceiptChargeDetail)
                        {
                            ReceiptChargeDetailAddList.Add(item);
                        }

                    }
                }

                #endregion


                //11.计算托盘货超重费
                #region 托盘货超重费-不用计算(已注释)

                //List<ReceiptResult> RecLPWeightList = (from a in RecList
                //                                       where a.UnitName == "LP"
                //                                       group a by new
                //                                       {
                //                                           a.ReceiptId,
                //                                           a.UnitName,
                //                                           a.Weight
                //                                       } into g
                //                                       select new ReceiptResult
                //                                       {
                //                                           ReceiptId = g.Key.ReceiptId,
                //                                           UnitName =
                //                                                  g.Key.UnitName == "LP" ? "托" :
                //                                                  g.Key.UnitName == "CTN" ? "箱" :
                //                                                  g.Key.UnitName == "EA" ? "件" :
                //                                                   g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                //                                                   g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                //                                                   g.Key.UnitName == "EA-BIG" ? "大件" : null,
                //                                           HuWeight = g.Key.Weight,
                //                                           CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                //                                           Weight = g.Sum(p => p.Qty * p.Weight),
                //                                           Qty = g.Sum(p => p.Qty)
                //                                       }).OrderBy(u => u.LotFlag).ToList();

                //if (RecLPWeightList.Count > 0)
                //{
                //    getContractList = contractList.Where(u => u.ChargeName.Contains("超重费(托)")).ToList();

                //    if (getContractList.Count == 0)
                //    {
                //        result = "合同名:" + client.ContractName + ",超重费(托) 没有找到合同明细,无法计算费用";
                //    }
                //    foreach (var item in RecLPWeightList)
                //    {
                //        if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) >= item.HuWeight).Count() == 0)
                //        {
                //            result = "合同名:" + client.ContractName + ",超重费(托),数值:" + item.HuWeight + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                //            break;
                //        }
                //    }

                //    if (result != "")
                //    {
                //        ReceiptCharge recCharge = new ReceiptCharge();
                //        recCharge.ClientCode = clientCode;
                //        recCharge.ReceiptId = ReceiptId;
                //        recCharge.WhCode = WhCode;
                //        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                //        recCharge.Status = "N";
                //        recCharge.Description = "类型:卸货附加-" + result;
                //        recCharge.CreateUser = CreateUser;
                //        recCharge.CreateDate = DateTime.Now;
                //        idal.IReceiptChargeDAL.Add(recCharge);
                //        idal.IReceiptChargeDAL.SaveChanges();
                //        return "N";
                //    }

                //    List<ContractForm> checkLadderNumber = new List<ContractForm>();
                //    List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                //    foreach (var item in RecLPWeightList)
                //    {
                //        ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) >= item.HuWeight).OrderBy(u => u.LadderNumberBegin).First();


                //        if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                //        {
                //            checkLadderNumber.Add(contract);

                //            ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                //            recChargeDetail.WhCode = WhCode;
                //            recChargeDetail.ReceiptId = ReceiptId;
                //            recChargeDetail.ClientCode = clientCode;
                //            recChargeDetail.ChargeType = "卸货附加费";
                //            recChargeDetail.ChargeName = "超重费(托)";
                //            recChargeDetail.TransportType = transportType;
                //            recChargeDetail.UnitName = "";
                //            recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                //            recChargeDetail.Qty = item.Qty;
                //            recChargeDetail.CBM = item.CBM;
                //            recChargeDetail.Weight = item.HuWeight;
                //            recChargeDetail.ChargeUnitName = "";
                //            recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                //            recChargeDetail.Price = contract.Price;
                //            recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                //            recChargeDetail.HolidayFlag = holidayString;
                //            recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                //            recChargeDetail.GroupId = contract.GroupId;

                //            recChargeDetail.CreateUser = CreateUser;
                //            recChargeDetail.CreateDate = DateTime.Now;

                //            if (holidayFlag == 1)
                //            {
                //                recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                //            }

                //            if ((contract.MaxPriceTotal ?? 0) > 0)
                //            {
                //                if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                //                {
                //                    recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                //                }
                //            }

                //            checkReceiptChargeDetail.Add(recChargeDetail);

                //        }
                //        else
                //        {
                //            ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                //            checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                //            ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                //            recChargeDetail.WhCode = WhCode;
                //            recChargeDetail.ReceiptId = ReceiptId;
                //            recChargeDetail.ClientCode = clientCode;
                //            recChargeDetail.ChargeType = "卸货附加费";
                //            recChargeDetail.ChargeName = "超重费(托)";
                //            recChargeDetail.TransportType = transportType;
                //            recChargeDetail.UnitName = "";
                //            recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                //            recChargeDetail.Qty = item.Qty;
                //            recChargeDetail.CBM = item.CBM;
                //            recChargeDetail.Weight = item.HuWeight;
                //            recChargeDetail.ChargeUnitName = "";
                //            recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                //            recChargeDetail.Price = contract.Price;
                //            recChargeDetail.PriceTotal = contract.Price * item.CBM;

                //            recChargeDetail.HolidayFlag = holidayString;
                //            recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                //            recChargeDetail.GroupId = contract.GroupId;

                //            recChargeDetail.CreateUser = CreateUser;
                //            recChargeDetail.CreateDate = DateTime.Now;

                //            if (holidayFlag == 1)
                //            {
                //                recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                //            }

                //            if ((contract.MaxPriceTotal ?? 0) > 0)
                //            {
                //                if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                //                {
                //                    recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                //                }
                //            }

                //            recChargeDetail.Qty += oldrecChargeDetail.Qty;
                //            recChargeDetail.CBM += oldrecChargeDetail.CBM;
                //            recChargeDetail.Weight += oldrecChargeDetail.Weight;

                //            recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                //            checkReceiptChargeDetail.Add(recChargeDetail);

                //        }
                //    }

                //    foreach (var item in checkReceiptChargeDetail)
                //    {
                //        ReceiptChargeDetailAddList.Add(item);
                //    }

                //}

                #endregion


                //12.计算托盘货超立方费
                #region 托盘货超立方费

                getContractList = contractList.Where(u => u.ChargeName.Contains("超立方(托)")).ToList();

                if (getContractList.Count > 0)
                {
                    List<ReceiptResult> RecLPCBMList = (from a in RecList
                                                        where a.UnitName == "LP"
                                                        group a by new
                                                        {
                                                            a.ReceiptId,
                                                            a.UnitName,
                                                            a.Length,
                                                            a.Width,
                                                            a.Height
                                                        } into g
                                                        select new ReceiptResult
                                                        {
                                                            ReceiptId = g.Key.ReceiptId,
                                                            UnitName =
                                                                   g.Key.UnitName == "LP" ? "托" :
                                                                   g.Key.UnitName == "CTN" ? "箱" :
                                                                   g.Key.UnitName == "EA" ? "件" :
                                                                   g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                   g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                       g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                       g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                            HuCbm = g.Key.Length * g.Key.Width * g.Key.Height,
                                                            CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                            Weight = g.Sum(p => p.Qty * p.Weight),
                                                            Qty = g.Sum(p => p.Qty)
                                                        }).OrderBy(u => u.LotFlag).ToList();

                    if (RecLPCBMList.Count > 0)
                    {
                        foreach (var item in RecLPCBMList)
                        {
                            if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",超立方(托),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                                break;
                            }
                            else if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() > 1)
                            {
                                result = "合同名:" + client.ContractName + ",超立方(托),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                break;
                            }
                        }

                        if (result != "")
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "N";
                            recCharge.Description = "类型:卸货附加-" + result;
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            idal.IReceiptChargeDAL.Add(recCharge);
                            idal.IReceiptChargeDAL.SaveChanges();
                            return "N";
                        }

                        List<ContractForm> checkLadderNumber = new List<ContractForm>();
                        List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                        foreach (var item in RecLPCBMList)
                        {
                            ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).OrderBy(u => u.LadderNumberBegin).First();

                            if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                            {
                                checkLadderNumber.Add(contract);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超立方(托)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }

                                }
                                checkReceiptChargeDetail.Add(recChargeDetail);

                            }
                            else
                            {
                                ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                                checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超立方(托)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.HuCbm;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                recChargeDetail.Qty += oldrecChargeDetail.Qty;
                                recChargeDetail.CBM += oldrecChargeDetail.CBM;
                                recChargeDetail.Weight += oldrecChargeDetail.Weight;

                                recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                                checkReceiptChargeDetail.Add(recChargeDetail);
                            }
                        }

                        foreach (var item in checkReceiptChargeDetail)
                        {
                            ReceiptChargeDetailAddList.Add(item);
                        }
                    }
                }

                #endregion


                //13.计算大件货超立方费
                #region 大件货超立方费

                getContractList = contractList.Where(u => u.ChargeName.Contains("超立方(大件)")).ToList();

                if (getContractList.Count > 0)
                {
                    List<ReceiptResult> RecENBIGCBMList = (from a in RecList
                                                           where a.UnitName == "EA-BIG"
                                                           group a by new
                                                           {
                                                               a.ReceiptId,
                                                               a.UnitName,
                                                               a.Length,
                                                               a.Width,
                                                               a.Height
                                                           } into g
                                                           select new ReceiptResult
                                                           {
                                                               ReceiptId = g.Key.ReceiptId,
                                                               UnitName =
                                                                      g.Key.UnitName == "LP" ? "托" :
                                                                      g.Key.UnitName == "CTN" ? "箱" :
                                                                      g.Key.UnitName == "EA" ? "件" :
                                                                      g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                      g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                          g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                          g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                               HuCbm = g.Key.Length * g.Key.Width * g.Key.Height,
                                                               CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                               Weight = g.Sum(p => p.Qty * p.Weight),
                                                               Qty = g.Sum(p => p.Qty)
                                                           }).OrderBy(u => u.LotFlag).ToList();

                    if (RecENBIGCBMList.Count > 0)
                    {
                        foreach (var item in RecENBIGCBMList)
                        {
                            if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",超立方(大件),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                                break;
                            }
                            else if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() > 1)
                            {
                                result = "合同名:" + client.ContractName + ",超立方(大件),数值:" + item.HuCbm + ",车型:" + transportType + ",班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                break;
                            }
                        }

                        if (result != "")
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "N";
                            recCharge.Description = "类型:卸货附加-" + result;
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            idal.IReceiptChargeDAL.Add(recCharge);
                            idal.IReceiptChargeDAL.SaveChanges();
                            return "N";
                        }

                        List<ContractForm> checkLadderNumber = new List<ContractForm>();
                        List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                        foreach (var item in RecENBIGCBMList)
                        {
                            ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuCbm && (u.LadderNumberEnd ?? 0) > item.HuCbm && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).OrderBy(u => u.LadderNumberBegin).First();

                            if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                            {
                                checkLadderNumber.Add(contract);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超立方(大件)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }

                                }
                                checkReceiptChargeDetail.Add(recChargeDetail);

                            }
                            else
                            {
                                ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                                checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超立方(大件)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.HuCbm;
                                recChargeDetail.Weight = item.Weight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.HuCbm;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                recChargeDetail.Qty += oldrecChargeDetail.Qty;
                                recChargeDetail.CBM += oldrecChargeDetail.CBM;
                                recChargeDetail.Weight += oldrecChargeDetail.Weight;

                                recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                                checkReceiptChargeDetail.Add(recChargeDetail);
                            }
                        }

                        foreach (var item in checkReceiptChargeDetail)
                        {
                            ReceiptChargeDetailAddList.Add(item);
                        }
                    }
                }

                #endregion


                //14.计算大件货超重费
                #region 大件货超重费

                getContractList = contractList.Where(u => u.ChargeName.Contains("超重费(大件)")).ToList();

                if (getContractList.Count > 0)
                {

                    List<ReceiptResult> RecENBIGWeightList = (from a in RecList
                                                              where a.UnitName == "EA-BIG"
                                                              group a by new
                                                              {
                                                                  a.ReceiptId,
                                                                  a.UnitName,
                                                                  a.Weight
                                                              } into g
                                                              select new ReceiptResult
                                                              {
                                                                  ReceiptId = g.Key.ReceiptId,
                                                                  UnitName =
                                                                         g.Key.UnitName == "LP" ? "托" :
                                                                         g.Key.UnitName == "CTN" ? "箱" :
                                                                         g.Key.UnitName == "EA" ? "件" :
                                                                         g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                          g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                          g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                          g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                                  HuWeight = g.Key.Weight,
                                                                  CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                                  Weight = g.Sum(p => p.Qty * p.Weight),
                                                                  Qty = g.Sum(p => p.Qty)
                                                              }).OrderBy(u => u.LotFlag).ToList();

                    if (RecENBIGWeightList.Count > 0)
                    {
                        foreach (var item in RecENBIGWeightList)
                        {
                            if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",超重费(大件),数值:" + item.HuWeight + ",车型:" + transportType + ",班次:" + nightTimeString + " 无对应阶梯区间,没有找到合同明细,无法计算费用";
                                break;
                            }
                            else if (getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).Count() > 1)
                            {
                                result = "合同名:" + client.ContractName + ",超重费(大件),数值:" + item.HuWeight + ",车型:" + transportType + ",班次:" + nightTimeString + " 存在多行合同明细,无法计算费用";
                                break;
                            }
                        }

                        if (result != "")
                        {
                            ReceiptCharge recCharge = new ReceiptCharge();
                            recCharge.ClientCode = clientCode;
                            recCharge.ReceiptId = ReceiptId;
                            recCharge.WhCode = WhCode;
                            recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                            recCharge.Status = "N";
                            recCharge.Description = "类型:卸货附加-" + result;
                            recCharge.CreateUser = CreateUser;
                            recCharge.CreateDate = DateTime.Now;
                            idal.IReceiptChargeDAL.Add(recCharge);
                            idal.IReceiptChargeDAL.SaveChanges();
                            return "N";
                        }

                        List<ContractForm> checkLadderNumber = new List<ContractForm>();
                        List<ReceiptChargeDetail> checkReceiptChargeDetail = new List<ReceiptChargeDetail>();

                        foreach (var item in RecENBIGWeightList)
                        {
                            ContractForm contract = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.HuWeight && (u.LadderNumberEnd ?? 0) > item.HuWeight && (u.TransportType == "所有" || u.TransportType == transportType) && (u.Shift == "所有" || u.Shift == nightTimeString)).OrderBy(u => u.LadderNumberBegin).First();


                            if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd).Count() == 0)
                            {
                                checkLadderNumber.Add(contract);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超重费(大件)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = item.HuWeight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * recChargeDetail.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                checkReceiptChargeDetail.Add(recChargeDetail);

                            }
                            else
                            {
                                ReceiptChargeDetail oldrecChargeDetail = checkReceiptChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd)).First();

                                checkReceiptChargeDetail.Remove(oldrecChargeDetail);

                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超重费(大件)";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = item.HuWeight;
                                recChargeDetail.ChargeUnitName = "";
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.CBM;

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = contract.GroupId;

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                if ((contract.MaxPriceTotal ?? 0) > 0)
                                {
                                    if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                                    {
                                        recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                                    }
                                }

                                recChargeDetail.Qty += oldrecChargeDetail.Qty;
                                recChargeDetail.CBM += oldrecChargeDetail.CBM;
                                recChargeDetail.Weight += oldrecChargeDetail.Weight;

                                recChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;

                                checkReceiptChargeDetail.Add(recChargeDetail);

                            }
                        }

                        foreach (var item in checkReceiptChargeDetail)
                        {
                            ReceiptChargeDetailAddList.Add(item);
                        }

                    }

                }

                #endregion



                //15.计算收货扫描SN费
                #region 收货扫描SN费

                List<SerialNumberIn> serInList = idal.ISerialNumberInDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
                if (serInList.Count > 0)
                {
                    getContractList = contractList.Where(u => u.ChargeName.Contains("收货扫描SN")).ToList();

                    if (getContractList.Count == 0)
                    {
                        result = "合同名:" + client.ContractName + ",收货扫描SN 没有找到合同明细,无法计算费用";
                    }
                    else if (getContractList.Count > 1)
                    {
                        result = "合同名:" + client.ContractName + ",收货扫描SN 存在多行合同明细,无法计算费用";
                    }

                    if (result != "")
                    {
                        ReceiptCharge recCharge = new ReceiptCharge();
                        recCharge.ClientCode = clientCode;
                        recCharge.ReceiptId = ReceiptId;
                        recCharge.WhCode = WhCode;
                        recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                        recCharge.Status = "N";
                        recCharge.Description = "类型:卸货附加-" + result;
                        recCharge.CreateUser = CreateUser;
                        recCharge.CreateDate = DateTime.Now;
                        idal.IReceiptChargeDAL.Add(recCharge);
                        idal.IReceiptChargeDAL.SaveChanges();
                        return "N";
                    }

                    ContractForm contract = getContractList.First();

                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                    recChargeDetail.WhCode = WhCode;
                    recChargeDetail.ReceiptId = ReceiptId;
                    recChargeDetail.ClientCode = clientCode;
                    recChargeDetail.ChargeType = "卸货附加费";
                    recChargeDetail.ChargeName = "收货扫描SN";
                    recChargeDetail.TransportType = transportType;
                    recChargeDetail.UnitName = "";
                    recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                    recChargeDetail.Qty = serInList.Count;
                    recChargeDetail.CBM = 0;
                    recChargeDetail.Weight = 0;
                    recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                    recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                    recChargeDetail.Price = contract.Price;
                    recChargeDetail.PriceTotal = contract.Price * serInList.Count;

                    recChargeDetail.HolidayFlag = holidayString;
                    recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                    recChargeDetail.GroupId = contract.GroupId;

                    recChargeDetail.CreateUser = CreateUser;
                    recChargeDetail.CreateDate = DateTime.Now;

                    if (holidayFlag == 1)
                    {
                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                    }

                    if ((contract.MaxPriceTotal ?? 0) > 0)
                    {
                        if (recChargeDetail.PriceTotal > (contract.MaxPriceTotal ?? 0))
                        {
                            recChargeDetail.PriceTotal = contract.MaxPriceTotal;
                        }
                    }

                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                }

                #endregion


                //16.蛇形卸货


                //17.排队重置次数
                #region 排队重置特殊操作费
                if ((reg.ResetCount ?? 0) > 0)
                {
                    if ((reg.TruckCount ?? 0) > 0 || (reg.DNCount ?? 0) > 0)
                    {
                        if (soCountList.Count > 0)
                        {
                            getContractList = new List<ContractForm>();

                            getContractList = contractList.Where(u => u.ChargeName.Contains("特殊操作费")).ToList();

                            if (getContractList.Count() == 0)
                            {
                                result = "合同名:" + client.ContractName + ",特殊操作费 没有找到合同明细,无法计算费用";
                            }
                            else if (getContractList.Count() > 1)
                            {
                                if (getContractList.Where(u => (u.GroupId ?? "") == "").Count() > 0)
                                {
                                    result = "合同名:" + client.ContractName + ",特殊操作费 存在多行合同明细,无法计算费用";
                                }
                                else
                                {
                                    string[] groupId = (from a in getContractList
                                                        select a.GroupId).ToList().Distinct().ToArray();
                                    if (groupId.Length > 1)
                                    {
                                        result = "合同名:" + client.ContractName + ",特殊操作费 组号不同,无法计算费用";
                                    }
                                }
                            }

                            if (result != "")
                            {
                                ReceiptCharge recCharge = new ReceiptCharge();
                                recCharge.ClientCode = clientCode;
                                recCharge.ReceiptId = ReceiptId;
                                recCharge.WhCode = WhCode;
                                recCharge.EndReceiptDate = Convert.ToDateTime(nighttimeNow);
                                recCharge.Status = "N";
                                recCharge.Description = "类型:卸货附加-" + result;
                                recCharge.CreateUser = CreateUser;
                                recCharge.CreateDate = DateTime.Now;
                                idal.IReceiptChargeDAL.Add(recCharge);
                                idal.IReceiptChargeDAL.SaveChanges();
                                return "N";
                            }

                            ContractForm contract = getContractList.First();

                            ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                            recChargeDetail.WhCode = WhCode;
                            recChargeDetail.ReceiptId = ReceiptId;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "卸货附加费";
                            recChargeDetail.ChargeName = "特殊操作费";
                            recChargeDetail.TransportType = transportType;
                            recChargeDetail.UnitName = "";
                            recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                            recChargeDetail.Qty = soCountList.Count * reg.ResetCount;
                            recChargeDetail.CBM = 0;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = "次";
                            recChargeDetail.LadderNumber = "重置数:" + reg.ResetCount.ToString() + ",SO个数:" + soCountList.Count;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * soCountList.Count * reg.ResetCount;

                            recChargeDetail.HolidayFlag = holidayString;
                            recChargeDetail.MonthlyFlag = CheckMonthFlagContract.MonthlyFlag ?? "否";
                            recChargeDetail.GroupId = "";

                            recChargeDetail.CreateUser = CreateUser;
                            recChargeDetail.CreateDate = DateTime.Now;

                            ReceiptChargeDetailAddList.Add(recChargeDetail);
                        }
                    }
                }
                #endregion


                //18.计算收货耗材费-改名为打托服务费
                #region 收货耗材费--打托服务费

                if (reg.AddServiceStatus == "U" || reg.AddServiceStatus == "C")
                {
                    List<AddValueServiceResult> addValueList = (from a in idal.IAddValueServiceDAL.SelectAll()
                                                                join b in idal.IRecLossDAL.SelectAll()
                                                                on a.RecLossId equals b.Id
                                                                join c in idal.IRecLossTypeDAL.SelectAll()
                                                                on a.RecLossTypeId equals c.Id
                                                                where a.ReceiptId == ReceiptId && a.WhCode == WhCode
                                                                select new AddValueServiceResult
                                                                {
                                                                    ReceiptId = a.ReceiptId,
                                                                    WhCode = a.WhCode,
                                                                    RecLossName = b.RecLossName,
                                                                    RecLossDescription = b.RecLossDescription,
                                                                    RecLossType = c.RecLossType1,
                                                                    Price = a.Price,
                                                                    Qty = a.Qty
                                                                }).ToList();

                    foreach (var contract in addValueList)
                    {
                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "打托服务费";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = contract.RecLossName;
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = contract.Qty;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "耗材数";
                        recChargeDetail.LadderNumber = contract.RecLossDescription;
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * contract.Qty;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = "否";
                        recChargeDetail.GroupId = "";

                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        ReceiptChargeDetailAddList.Add(recChargeDetail);
                    }
                }

                #endregion


                //19.计算超长车加收立方费,钱春秀增加
                //平板加收10/立方  厢车加收5/立方
                //然后超长车-加收20元/辆的车辆管理费,扫描枪上增加超长车选项16.5米，2021年5月1日正式开始
                //2023年10月20日调整:平板加收5/立方  厢车加收5/立方
                #region 超长车加收立方费

                if (!string.IsNullOrEmpty(reg.TransportTypeExtend))
                {
                    if (transportType.Contains("集装箱"))
                    {

                    }
                    else
                    {
                        ContractForm contractList2 = contractList1.Where(u => u.Type == "卸货" && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList().First();

                        List<ReceiptChargeDetail> getList = ReceiptChargeDetailAddList.Where(u => u.ChargeType == "卸货费").ToList();

                        getContractList = contractList.Where(u => u.ChargeName.Contains("卸货超长加收") && u.Price > 0 && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList();

                        if (getContractList.Count > 0)
                        {
                            List<string> unitList = new List<string>();
                            unitList.Add("箱");
                            unitList.Add("托");
                            unitList.Add("卷");
                            unitList.Add("大件");

                            ContractForm contract = getContractList.First();

                            foreach (var recDetail in getList)
                            {
                                if (unitList.Contains(recDetail.UnitName))
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                    recChargeDetail.WhCode = WhCode;
                                    recChargeDetail.ReceiptId = ReceiptId;
                                    recChargeDetail.ClientCode = clientCode;
                                    recChargeDetail.ChargeType = "卸货费";
                                    recChargeDetail.ChargeName = "卸货超长加收";
                                    recChargeDetail.TransportType = transportType;
                                    recChargeDetail.UnitName = recDetail.UnitName;
                                    recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                    recChargeDetail.Qty = 0;
                                    recChargeDetail.CBM = recDetail.CBM;
                                    recChargeDetail.Weight = 0;
                                    recChargeDetail.ChargeUnitName = recDetail.ChargeUnitName;

                                    recChargeDetail.LadderNumber = "超长车" + reg.TransportTypeExtend + "加收" + contract.Price + "元/立方";
                                    recChargeDetail.Price = contract.Price;

                                    if (recDetail.CBM > 0 && recDetail.CBM < 1 && recDetail.ChargeUnitName == "立方")
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.Price * 1;
                                    }
                                    else
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.Price * recDetail.CBM;
                                    }

                                    recChargeDetail.HolidayFlag = holidayString;
                                    recChargeDetail.MonthlyFlag = recDetail.MonthlyFlag;
                                    recChargeDetail.GroupId = recDetail.GroupId;

                                    recChargeDetail.CreateUser = CreateUser;
                                    recChargeDetail.CreateDate = DateTime.Now;

                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contractList2.Ratio;
                                    }

                                    ReceiptChargeDetailAddList.Add(recChargeDetail);
                                }
                            }
                        }
                    }

                }

                #endregion


                //20.超50KG加收费 15/立方，钱春秀增加，2021年5月14日(超50KG加收费30/件)，5月18日变更为(15/立方),2023年7月27日 陆倩增加THD客户收费
                #region 超50KG加收费

                if (1 == 1)
                {
                    getContractList = contractList.Where(u => u.ChargeName.Contains("超50KG附加费") && u.Price > 0 && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList();

                    if (getContractList.Count > 0)
                    {
                        List<ReceiptResult> RecCtnWeightList = (from a in RecList
                                                                where (a.UnitName == "CTN" || a.UnitName == "CLOTH-ROLL") && a.Weight >= 50
                                                                group a by new
                                                                {
                                                                    a.ReceiptId,
                                                                    a.UnitName,
                                                                    a.Weight
                                                                } into g
                                                                select new ReceiptResult
                                                                {
                                                                    ReceiptId = g.Key.ReceiptId,
                                                                    UnitName =
                                                                           g.Key.UnitName == "LP" ? "托" :
                                                                           g.Key.UnitName == "CTN" ? "箱" :
                                                                           g.Key.UnitName == "EA" ? "件" :
                                                                           g.Key.UnitName == "CLOTH-ROLL" ? "卷" :
                                                                           g.Key.UnitName == "ECH-THIN" ? "薄挂衣" :
                                                                           g.Key.UnitName == "ECH-THICK" ? "厚挂衣" :
                                                                           g.Key.UnitName == "EA-BIG" ? "大件" : null,
                                                                    HuWeight = (g.Key.Weight ?? 0),
                                                                    CBM = g.Sum(p => p.Qty * p.Length * p.Width * p.Height),
                                                                    Weight = g.Sum(p => p.Qty * ((p.Weight ?? 0))),
                                                                    Qty = g.Sum(p => p.Qty)
                                                                }).OrderBy(u => u.LotFlag).ToList();

                        if (RecCtnWeightList.Count > 0)
                        {
                            ContractForm contract = getContractList.First();

                            foreach (var item in RecCtnWeightList.OrderBy(u => u.HuWeight))
                            {
                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "超50KG附加费";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = item.UnitName;
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = item.HuWeight;
                                recChargeDetail.ChargeUnitName = "立方";
                                recChargeDetail.LadderNumber = "超50KG";
                                recChargeDetail.Price = contract.Price;

                                if (item.CBM > 0 && item.CBM < 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.Price * 1;
                                }
                                else
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.Price * item.CBM;
                                }

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;

                                recChargeDetail.GroupId = "";
                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                                }

                                ReceiptChargeDetailAddList.Add(recChargeDetail);

                            }
                        }
                    }
                }

                #endregion



                //21.打单SKU加收,根据SKU个数来灵活取值
                //2024年8月1日 BBL增加
                #region 打单SKU加收

                getContractList = contractList.Where(u => u.ChargeName.Contains("打单SKU加收") && u.Price > 0).ToList();

                if (getContractList.Count > 0)
                {
                    List<ReceiptResult> RecSkuCountList = (from a in RecList
                                                           group a by new
                                                           {
                                                               a.ReceiptId,
                                                               a.AltItemNumber,
                                                               a.ItemId
                                                           } into g
                                                           select new ReceiptResult
                                                           {
                                                               ReceiptId = g.Key.ReceiptId,
                                                               AltItemNumber = g.Key.AltItemNumber
                                                           }).ToList().Distinct().ToList();

                    if (RecSkuCountList.Count > 0)
                    {
                        ContractForm contract = getContractList.First();

                        List<ContractForm> getContractList1 = contractList.Where(u => u.ChargeName.Contains("打单SKU个数设置")).ToList();

                        //得到合同配置SKU个数，默认5个
                        int setSkuCount = 0;
                        if (getContractList1.Count == 0)
                        {
                            setSkuCount = 5;
                        }
                        else
                        {
                            setSkuCount = Convert.ToInt32(getContractList1.First().Price);
                        }

                        //得到整个批次共有多少个SKU
                        int getSumCount = RecSkuCountList.Count;
                        //得到除数
                        decimal getCount = getSumCount / (decimal)setSkuCount;
                        //得到取整数
                        decimal skushu = Math.Ceiling(getCount);

                        ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                        recChargeDetail.WhCode = WhCode;
                        recChargeDetail.ReceiptId = ReceiptId;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "卸货附加费";
                        recChargeDetail.ChargeName = "打单SKU加收";
                        recChargeDetail.TransportType = transportType;
                        recChargeDetail.UnitName = "份";
                        recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                        recChargeDetail.Qty = Convert.ToInt32(skushu);
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = "份";
                        recChargeDetail.LadderNumber = getSumCount + "/" + setSkuCount + "=" + skushu + "份,每" + setSkuCount + "个SKU加收" + contract.Price + "元,依次递增";
                        recChargeDetail.Price = contract.Price;

                        recChargeDetail.PriceTotal = recChargeDetail.Price * skushu;

                        recChargeDetail.HolidayFlag = holidayString;
                        recChargeDetail.MonthlyFlag = contract.MonthlyFlag;

                        recChargeDetail.GroupId = "";
                        recChargeDetail.CreateUser = CreateUser;
                        recChargeDetail.CreateDate = DateTime.Now;

                        if (holidayFlag == 1)
                        {
                            recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contract.Ratio;
                        }

                        ReceiptChargeDetailAddList.Add(recChargeDetail);

                    }

                }

                #endregion


                #endregion


                List<ReceiptChargeDetail> ReceiptChargeDetailAddList1 = ReceiptChargeDetailAddList.Where(u => (u.GroupId ?? "") == "").ToList();
                string[] groupIdArr = (from a in ReceiptChargeDetailAddList
                                       where (a.GroupId ?? "") != ""
                                       select a.GroupId).ToList().Distinct().ToArray();

                foreach (var item in groupIdArr)
                {
                    List<ReceiptChargeDetail> getList = ReceiptChargeDetailAddList.Where(u => (u.GroupId ?? "") == item).ToList();

                    //得到 组号3的 超立方(大件)和超重量(大件) 的总列表
                    List<ReceiptChargeDetail> groupList = (from a in getList
                                                           group a by new
                                                           {
                                                               a.ChargeName
                                                           } into g
                                                           select new ReceiptChargeDetail
                                                           {
                                                               ChargeName = g.Key.ChargeName,
                                                               PriceTotal = g.Sum(p => p.PriceTotal)
                                                           }).OrderByDescending(u => u.PriceTotal).ToList();

                    //通过聚合名称和总价，得到 超立方(大件)和超重量(大件) 谁价格更高
                    //然后得到 组号、名称的列表 循环加入总集
                    foreach (var item1 in ReceiptChargeDetailAddList.Where(u => (u.GroupId ?? "") == item && u.ChargeName == groupList.First().ChargeName))
                    {
                        ReceiptChargeDetailAddList1.Add(item1);
                    }
                }

                #region 22.基础卸货费月结
                if (1 == 1)
                {
                    contractList = contractList1.Where(u => u.Type == "卸货附加" && u.ChargeName.Contains("基础卸货费") && u.Price > 0 && Convert.ToDateTime(datetimeNow1) >= u.ActiveDateBegin && Convert.ToDateTime(datetimeNow1) <= u.ActiveDateEnd).ToList();

                    if (contractList.Count > 0)
                    {
                        List<ReceiptChargeDetail> ReceiptChargeDetailAddList2 = new List<ReceiptChargeDetail>();

                        if (ReceiptChargeDetailAddList1.Where(u => u.ChargeType == "卸货费" && u.ChargeName != "卸货超长加收").Count() > 0)
                        {
                            List<ReceiptChargeDetail> getList = ReceiptChargeDetailAddList1.Where(u => u.ChargeType == "卸货费" && u.ChargeName != "卸货超长加收").ToList();

                            ContractForm contractList2 = contractList.First();

                            foreach (var recDetail in getList)
                            {
                                //删除原卸货费
                                ReceiptChargeDetailAddList1.Remove(recDetail);

                                //得到新卸货费
                                ReceiptChargeDetail newRecDetail = recDetail;
                                newRecDetail.ChargeType = "原卸货费";
                                newRecDetail.LadderNumber = "原卸货费";
                                newRecDetail.MonthlyFlag = "是";
                                ReceiptChargeDetailAddList1.Add(newRecDetail);

                                //添加基础卸货费
                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "基础卸货费";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = recDetail.UnitName;
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = recDetail.Qty;
                                recChargeDetail.CBM = recDetail.CBM;
                                recChargeDetail.Weight = recDetail.Weight;
                                recChargeDetail.ChargeUnitName = recDetail.ChargeUnitName;
                                recChargeDetail.LadderNumber = "需扣除的基础卸货费";
                                recChargeDetail.Price = contractList2.Price;

                                if (recDetail.CBM > 0 && recDetail.CBM < 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.Price * 1;
                                }
                                else
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.Price * recDetail.CBM;
                                }

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contractList2.MonthlyFlag;
                                recChargeDetail.GroupId = "";
                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * contractList2.Ratio;
                                }
                                ReceiptChargeDetailAddList1.Add(recChargeDetail);


                                //添加扣除基础卸货费后的现结卸货费
                                ReceiptChargeDetail recChargeDetail1 = new ReceiptChargeDetail();
                                recChargeDetail1.WhCode = WhCode;
                                recChargeDetail1.ReceiptId = ReceiptId;
                                recChargeDetail1.ClientCode = clientCode;
                                recChargeDetail1.ChargeType = "卸货费";
                                recChargeDetail1.ChargeName = "卸货费现结";
                                recChargeDetail1.TransportType = transportType;
                                recChargeDetail1.UnitName = newRecDetail.ChargeUnitName;
                                recChargeDetail1.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail1.Qty = newRecDetail.Qty;
                                recChargeDetail1.CBM = newRecDetail.CBM;
                                recChargeDetail1.Weight = newRecDetail.Weight;
                                recChargeDetail1.ChargeUnitName = newRecDetail.ChargeUnitName;
                                recChargeDetail1.LadderNumber = "扣除基础卸货费后的费用现结";
                                if (newRecDetail.Price >= contractList2.Price)
                                {
                                    recChargeDetail1.Price = newRecDetail.Price - contractList2.Price;
                                }
                                else
                                {
                                    recChargeDetail1.Price = 0;
                                }

                                if (recDetail.CBM > 0 && recDetail.CBM < 1)
                                {
                                    recChargeDetail1.PriceTotal = recChargeDetail1.Price * 1;
                                }
                                else
                                {
                                    recChargeDetail1.PriceTotal = recChargeDetail1.Price * recDetail.CBM;
                                }

                                recChargeDetail1.HolidayFlag = holidayString;
                                recChargeDetail1.MonthlyFlag = "否";
                                recChargeDetail1.GroupId = "";
                                recChargeDetail1.CreateUser = CreateUser;
                                recChargeDetail1.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail1.PriceTotal = recChargeDetail1.PriceTotal * contractList2.Ratio;
                                }
                                ReceiptChargeDetailAddList1.Add(recChargeDetail1);

                            }
                        }
                    }
                }

                #endregion

                idal.IReceiptChargeDAL.Add(ReceiptChargeAddList);
                idal.IReceiptChargeDetailDAL.Add(ReceiptChargeDetailAddList1);


                #region 疫情期间：燃油附加费计算，现已停止收费 功能隐藏

                //21.计算燃油附加费，4月15日新增 1.5元/立方，不满1立方按1立方算

                #region 燃油附加费

                if (reg.BeginReceiptDate < Convert.ToDateTime("2022-10-15"))
                {
                    List<ReceiptChargeDetail> ReceiptChargeDetailAddList2 = new List<ReceiptChargeDetail>();

                    if (ReceiptChargeDetailAddList1.Where(u => u.ChargeType == "卸货费" && u.ChargeName != "卸货超长加收").Count() > 0)
                    {
                        getContractList = new List<ContractForm>();
                        getContractList = contractList.Where(u => u.ChargeName.Contains("打单费")).ToList();

                        List<ReceiptChargeDetail> getList = ReceiptChargeDetailAddList1.Where(u => u.ChargeType == "卸货费" && u.ChargeName != "卸货超长加收").ToList();

                        WhAgent getAgent = new WhAgent();
                        if (agentList.Count > 0)
                        {
                            getAgent = agentList.First();
                        }

                        ContractForm contract = new ContractForm();
                        if (getContractList.Count() > 0)
                        {
                            contract = getContractList.First();
                        }
                        else
                        {
                            contract.MonthlyFlag = "否";
                        }

                        foreach (var recDetail in getList)
                        {
                            if (getAgent.AgentCode == "GuanHang")
                            {
                                continue;
                            }

                            if (getAgent.AgentCode == "MLOG")
                            {
                                //非月结客户才添加
                                if (recDetail.MonthlyFlag == "否")
                                {
                                    ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                    recChargeDetail.WhCode = WhCode;
                                    recChargeDetail.ReceiptId = ReceiptId;
                                    recChargeDetail.ClientCode = clientCode;
                                    recChargeDetail.ChargeType = "卸货附加费";
                                    recChargeDetail.ChargeName = "燃油附加费";
                                    recChargeDetail.TransportType = transportType;
                                    recChargeDetail.UnitName = "立方";
                                    recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                    recChargeDetail.Qty = 1;
                                    recChargeDetail.CBM = recDetail.CBM;
                                    recChargeDetail.Weight = 0;
                                    recChargeDetail.ChargeUnitName = "次";

                                    recChargeDetail.LadderNumber = "加收1.5元/立方";
                                    recChargeDetail.Price = Convert.ToDecimal(1.5);

                                    if (recDetail.ChargeName.Contains("挂衣"))
                                    {
                                        recChargeDetail.PriceTotal = recDetail.PriceTotal * Convert.ToDecimal(0.07);
                                        recChargeDetail.UnitName = "挂衣";
                                        recChargeDetail.Price = recDetail.PriceTotal * Convert.ToDecimal(0.07);
                                        recChargeDetail.CBM = 0;
                                        recChargeDetail.LadderNumber = "加收卸货费的7%";
                                    }
                                    else
                                    {
                                        if (recDetail.CBM > 0 && recDetail.CBM < 1)
                                        {
                                            recChargeDetail.PriceTotal = recChargeDetail.Price * 1;
                                        }
                                        else
                                        {
                                            recChargeDetail.PriceTotal = recChargeDetail.Price * recDetail.CBM;
                                        }
                                    }

                                    recChargeDetail.HolidayFlag = holidayString;
                                    recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                    recChargeDetail.GroupId = "";

                                    recChargeDetail.CreateUser = CreateUser;
                                    recChargeDetail.CreateDate = DateTime.Now;

                                    if (holidayFlag == 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * 1;
                                    }

                                    ReceiptChargeDetailAddList2.Add(recChargeDetail);
                                }
                            }
                            else
                            {
                                ReceiptChargeDetail recChargeDetail = new ReceiptChargeDetail();
                                recChargeDetail.WhCode = WhCode;
                                recChargeDetail.ReceiptId = ReceiptId;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "卸货附加费";
                                recChargeDetail.ChargeName = "燃油附加费";
                                recChargeDetail.TransportType = transportType;
                                recChargeDetail.UnitName = "立方";
                                recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
                                recChargeDetail.Qty = 1;
                                recChargeDetail.CBM = recDetail.CBM;
                                recChargeDetail.Weight = 0;
                                recChargeDetail.ChargeUnitName = "次";

                                recChargeDetail.LadderNumber = "加收1.5元/立方";
                                recChargeDetail.Price = Convert.ToDecimal(1.5);

                                if (recDetail.ChargeName.Contains("挂衣"))
                                {
                                    recChargeDetail.PriceTotal = recDetail.PriceTotal * Convert.ToDecimal(0.07);
                                    recChargeDetail.UnitName = "挂衣";
                                    recChargeDetail.Price = recDetail.PriceTotal * Convert.ToDecimal(0.07);
                                    recChargeDetail.CBM = 0;
                                    recChargeDetail.LadderNumber = "加收卸货费的7%";
                                }
                                else
                                {
                                    if (recDetail.CBM > 0 && recDetail.CBM < 1)
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.Price * 1;
                                    }
                                    else
                                    {
                                        recChargeDetail.PriceTotal = recChargeDetail.Price * recDetail.CBM;
                                    }
                                }

                                recChargeDetail.HolidayFlag = holidayString;
                                recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
                                recChargeDetail.GroupId = "";

                                recChargeDetail.CreateUser = CreateUser;
                                recChargeDetail.CreateDate = DateTime.Now;

                                if (holidayFlag == 1)
                                {
                                    recChargeDetail.PriceTotal = recChargeDetail.PriceTotal * 1;
                                }

                                ReceiptChargeDetailAddList2.Add(recChargeDetail);
                            }

                        }
                    }

                    idal.IReceiptChargeDetailDAL.Add(ReceiptChargeDetailAddList2);
                }


                #endregion

                #endregion


                idal.SaveChanges();

                return "Y";
            }
            catch (Exception e)
            {
                string ss = e.InnerException.Message;

                return "自动计算收费异常！";
            }

        }

        private static string checkContractLadderNumber(WhClient client, string result, ReceiptResult item, ContractForm contract, ReceiptChargeDetail recChargeDetail)
        {
            //如果按立方计费
            if (contract.ChargeUnitName.Contains("立方"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.CBM > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收立方:" + item.CBM + ",无法计算费用";

                    }
                    else if (item.CBM < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收立方:" + item.CBM + ",无法计算费用";

                    }
                    else if (item.CBM >= contract.LadderNumberBegin && item.CBM < contract.LadderNumberEnd)
                    {

                        recChargeDetail.PriceTotal = contract.Price * item.CBM;

                    }
                }
                else
                {

                    recChargeDetail.PriceTotal = contract.Price * item.CBM;

                }
            }
            else if (contract.ChargeUnitName.Contains("吨"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.Weight > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收重量:" + item.Weight + ",无法计算费用";

                    }
                    else if (item.Weight < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收重量:" + item.Weight + ",无法计算费用";

                    }
                    else if (item.Weight >= contract.LadderNumberBegin && item.Weight < contract.LadderNumberEnd)
                    {
                        recChargeDetail.PriceTotal = contract.Price * (item.Weight / 1000);
                    }
                }
                else
                {
                    recChargeDetail.PriceTotal = contract.Price * (item.Weight / 1000);
                }

            }
            else if (contract.ChargeUnitName.Contains("件"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.CBM > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM >= contract.LadderNumberBegin && item.CBM < contract.LadderNumberEnd)
                    {
                        recChargeDetail.PriceTotal = contract.Price * item.Qty;
                    }
                }
                else
                {
                    recChargeDetail.PriceTotal = contract.Price * item.Qty;
                }
            }
            else if (contract.ChargeUnitName.Contains("托"))
            {
                recChargeDetail.PriceTotal = contract.Price * item.Qty;
            }
            else
            {
                result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + " 没有找到合同明细,无法计算费用";
            }

            return result;
        }


        private static string checkContractLadderNumber1(WhClient client, string result, ReceiptResult item, ContractForm contract, ReceiptChargeDetail recChargeDetail)
        {
            if (contract.ChargeUnitName.Contains("件") || contract.ChargeUnitName.Contains("箱"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.CBM > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM >= contract.LadderNumberBegin && item.CBM < contract.LadderNumberEnd)
                    {
                        recChargeDetail.PriceTotal = contract.Price * item.Qty;
                    }
                }
                else
                {
                    recChargeDetail.PriceTotal = contract.Price * item.Qty;
                }
            }
            else if (contract.ChargeUnitName.Contains("立方"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.CBM > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收数量:" + item.Qty + ",无法计算费用";

                    }
                    else if (item.CBM >= contract.LadderNumberBegin && item.CBM < contract.LadderNumberEnd)
                    {

                        recChargeDetail.PriceTotal = contract.Price * item.CBM;

                    }
                }
                else
                {

                    recChargeDetail.PriceTotal = contract.Price * item.CBM;

                }
            }
            else if (contract.ChargeUnitName.Contains("吨"))
            {
                if (!string.IsNullOrEmpty(contract.LadderNumberBegin.ToString()) && !string.IsNullOrEmpty(contract.LadderNumberEnd.ToString()))
                {
                    if (item.Weight > contract.LadderNumberEnd)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "小于实收重量:" + item.Weight + ",无法计算费用";

                    }
                    else if (item.Weight < contract.LadderNumberBegin)
                    {
                        result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + ",阶梯数值:" + contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "大于实收重量:" + item.Weight + ",无法计算费用";

                    }
                    else if (item.Weight >= contract.LadderNumberBegin && item.Weight < contract.LadderNumberEnd)
                    {
                        recChargeDetail.PriceTotal = contract.Price * (item.Weight / 1000);
                    }
                }
                else
                {
                    recChargeDetail.PriceTotal = contract.Price * (item.Weight / 1000);
                }

            }
            else if (contract.ChargeUnitName.Contains("托"))
            {
                recChargeDetail.PriceTotal = contract.Price * item.Qty;
            }
            else
            {
                result = "合同名:" + client.ContractName + ",卸货单位:" + recChargeDetail.UnitName + "/所有,车辆类型:" + recChargeDetail.TransportType + "/所有,班次:" + (recChargeDetail.NightTimeFlag == "是" ? "夜班" : "白班") + "/所有,计费单位:" + contract.ChargeUnitName + " 没有找到合同明细,无法计算费用";
            }

            return result;
        }

        private static void recChargeDetailInsert(string ReceiptId, string WhCode, string CreateUser, string clientCode, string transportType, int nightTimeFlag, ReceiptResult item, ContractForm contract, ReceiptChargeDetail recChargeDetail, string holidayString)
        {
            recChargeDetail.WhCode = WhCode;
            recChargeDetail.ReceiptId = ReceiptId;
            recChargeDetail.ClientCode = clientCode;
            recChargeDetail.ChargeType = "卸货费";
            recChargeDetail.ChargeName = contract.ChargeName;
            recChargeDetail.TransportType = transportType;
            recChargeDetail.UnitName = item.UnitName;
            recChargeDetail.NightTimeFlag = nightTimeFlag == 0 ? "否" : nightTimeFlag == 1 ? "是" : "否";
            recChargeDetail.Qty = item.Qty;
            recChargeDetail.CBM = item.CBM;
            recChargeDetail.Weight = item.Weight;
            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
            recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd + "/" + contract.LadderNumberBeginCBM + "-" + contract.LadderNumberEndCBM;
            recChargeDetail.Price = contract.Price;

            recChargeDetail.HolidayFlag = holidayString;
            recChargeDetail.MonthlyFlag = contract.MonthlyFlag;
            recChargeDetail.GroupId = contract.GroupId;
            recChargeDetail.CreateUser = CreateUser;
            recChargeDetail.CreateDate = DateTime.Now;
        }


        //删除收货费用后再次获取费用
        public string DelReceiptCharge(string ReceiptId, string WhCode, string CreateUser)
        {
            List<ReceiptRegister> list = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId);
            if (list.Count == 0)
            {
                return "未找到批次信息，无法计算费用！";
            }

            ReceiptRegister fir = list.First();
            if (fir.Status != "C")
            {
                return "收货批次未完成收货，无法计算费用！";
            }

            TranLog tl2 = new TranLog();
            tl2.TranType = "530";
            tl2.Description = "重新获取收货费用";
            tl2.TranDate = DateTime.Now;
            tl2.TranUser = CreateUser;
            tl2.WhCode = WhCode;
            tl2.Remark = "收货批次号：" + ReceiptId;

            idal.ITranLogDAL.Add(tl2);
            idal.SaveChanges();

            idal.IReceiptChargeDAL.DeleteByExtended(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
            idal.IReceiptChargeDetailDAL.DeleteByExtended(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode && u.ChargeName != "现场操作费");

            ReceiptCharge(ReceiptId, WhCode, CreateUser);

            return "Y";
        }


        //分票类型下拉菜单列表
        public IEnumerable<LookUp> LotFlagListSelect()
        {
            var sql = (from a in idal.ILookUpDAL.SelectAll()
                       where a.TableName == "Receipt" && a.ColumnName == "LotFlag"
                       select a).Distinct();

            return sql.AsEnumerable();
        }


        //通过批次检测流程中是否需要验证所有托盘的重量
        public string CheckAllHuWeight(string ReceiptId, string WhCode)
        {
            List<FlowHead> flowHeadList = (from a in idal.IFlowHeadDAL.SelectAll()
                                           join b in idal.IReceiptRegisterDAL.SelectAll()
                                           on a.Id equals b.ProcessId
                                           where b.WhCode == WhCode && b.ReceiptId == ReceiptId
                                           select a).ToList();

            if (flowHeadList.Count > 0)
            {
                FlowHead flowHead = flowHeadList.First();
                if (flowHead.CheckAllHuWeightFlag > 0)
                {
                    string[] huIdArr = (from a in idal.IHuDetailDAL.SelectAll()
                                        where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                                        select a.HuId).Distinct().ToArray();

                    var sql = from a in idal.IHuMasterDAL.SelectAll()
                              where a.WhCode == WhCode && huIdArr.Contains(a.HuId) && (a.HuWeight ?? 0) == 0
                              select a;
                    if (sql.Count() > 0)
                    {
                        return "无法完成收货！托盘需全部称重！";
                    }
                }
            }

            return "Y";
        }


        #region 1.收货耗材科目

        public List<RecLossType> RecLossTypeList(RecLossTypeSearch searchEntity, out int total)
        {
            var sql = idal.IRecLossTypeDAL.SelectAll();

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.RecLossType))
                sql = sql.Where(u => u.RecLossType1.Contains(searchEntity.RecLossType));
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public RecLossType RecLossTypeAdd(RecLossType entity)
        {
            if (idal.IRecLossTypeDAL.SelectBy(u => u.RecLossType1 == entity.RecLossType1 && u.WhCode == entity.WhCode).Count() == 0)
            {
                entity.Status = "Active";
                entity.CreateDate = DateTime.Now;
                idal.IRecLossTypeDAL.Add(entity);
                idal.IRecLossTypeDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        public string RecLossTypeEdit(RecLossType entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IRecLossTypeDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
            idal.IRecLossTypeDAL.SaveChanges();
            return "Y";
        }

        #endregion


        #region 2.收货耗材

        public List<RecLossResult> RecLossList(RecLossSearch searchEntity, out int total)
        {
            var sql = from a in idal.IRecLossDAL.SelectAll()
                      join b in idal.IRecLossTypeDAL.SelectAll()
                      on a.RecLossTypeId equals b.Id into temp1
                      from ab in temp1.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      select new RecLossResult
                      {
                          Id = a.Id,
                          RecSort = a.RecSort,
                          RecLossName = a.RecLossName,
                          RecLossDescription = a.RecLossDescription,
                          RecLossType = ab.RecLossType1,
                          RecLossTypeId = a.RecLossTypeId,
                          Price = a.Price,
                          Status = a.Status,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);
            if (!string.IsNullOrEmpty(searchEntity.RecLossName))
                sql = sql.Where(u => u.RecLossName == searchEntity.RecLossName);
            if (searchEntity.RecLossTypeId > 0)
                sql = sql.Where(u => u.RecLossTypeId == searchEntity.RecLossTypeId);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public RecLoss RecLossAdd(RecLoss entity)
        {
            if (idal.IRecLossDAL.SelectBy(u => u.WhCode == entity.WhCode && u.RecLossName == entity.RecLossName).Count() == 0)
            {
                entity.Status = "Active";
                entity.CreateDate = DateTime.Now;
                idal.IRecLossDAL.Add(entity);
                idal.IRecLossDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        public string RecLossEdit(RecLoss entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IRecLossDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "RecLossDescription", "Status", "Price", "UpdateUser", "UpdateDate", "RecSort" });
            idal.IRecLossDAL.SaveChanges();
            return "Y";
        }

        //收货耗材科目下拉菜单列表
        public IEnumerable<RecLossType> RecLossTypeListSelect(string whCode)
        {
            var sql = from a in idal.IRecLossTypeDAL.SelectAll()
                      where a.WhCode == whCode && a.Status == "Active"
                      select a;
            sql = sql.OrderBy(u => u.Id);
            return sql.AsEnumerable();
        }

        #endregion


        //获取照片FileId
        public string getFileId(string userId)
        {
            //type  1 超规  0 标准
            string res = "";
            //Dictionary<string, object> dicHeader = new Dictionary<string, object>();
            //dicHeader.Add("userId", userId);

            //string json = JsonConvert.SerializeObject(dicHeader);
            string strUrl = APIReqUrl + "APP/getUploadfileIdAPI";
            //string strUrl = "http://localhost:3103/NoticeInfo/NotificationInterface";
            string response = CreatePostHttpResponse(strUrl, "POST", userId);
            Dictionary<string, object> resultList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
            int Status = int.Parse(resultList["Status"].ToString());
            if (Status == 100)
            {
                string Data = resultList["Data"].ToString();

                res = Data;
            }
            else
            {
                res = "";
            }
            return res;
        }


        #region 调取外部接口
        public string CreatePostHttpResponse(string url, string method, string json = "")
        {
            string content = string.Empty;
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                // 设置安全协议
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.UserAgent = "EIP";

            // 设置安全协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpWebResponse response;
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(json);
                request.ContentLength = postData.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                response = (HttpWebResponse)request.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    content = "error$接口不通";
                }
                response.Close();
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }
            return content;
        }

        #endregion
    }
}
