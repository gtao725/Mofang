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
    public class RecWinceManager : IRecWinceManager
    {
        IDAL.IDALSession idal = BLLHelper.GetDal();
        RecHelper recHelper = new RecHelper();
        public ReceiptInsert GetReceipt(string ReceiptId, string WhCode)
        {
            var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                      select new ReceiptInsert
                      {
                          ReceiptId = a.ReceiptId,
                          WhCode = a.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          TransportType = a.TransportType,
                          TransportTypeExtend = a.TransportTypeExtend,
                          Status = a.Status,
                          ProcessId = a.ProcessId,
                          Remark = a.Remark
                      };

            if (sql.Count() > 0)
                return sql.First();
            else
                return null;
        }



        public bool ReceiptDSCheck(string ReceiptId, string WhCode, string RecType)
        {
            int? DSFLag = Convert.ToInt32(RecType);
            return (from a in idal.IReceiptRegisterDAL.SelectAll()
                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId && (a.DSFLag == null ? 0 : a.DSFLag) == DSFLag
                    select a.Id).Count() > 0;
        }
        public List<UserPriceT> UserPriceT()
        {

            return idal.IUserPriceTDAL.SelectAll().ToList();
        }
        public List<UserShenT> UserShenT()
        {
            return idal.IUserShenTDAL.SelectAll().ToList();
        }
        public string GetRecDes(string ReceiptId, string WhCode)
        {

            string res = "";
            var aa = from a in idal.IReceiptRegisterDAL.SelectAll()
                     where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                     select new { SumQty = a.SumQty, SumCBM = a.SumCBM };

            var bb = from a in idal.IReceiptDAL.SelectAll()
                     where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                     group a by new { a.ReceiptId, a.WhCode } into b
                     select new { SumQty = b.Sum(x => x.Qty), SumCBM = b.Sum(x => x.Length * x.Width * x.Height * x.Qty) };

            if (aa.Count() > 0)
                res = aa.First().SumQty + "$" + System.Decimal.Round(aa.First().SumCBM ?? 0, 3);
            else
                res = "0$0";

            if (bb.Count() > 0)
                res += "$" + bb.First().SumQty + "$" + System.Decimal.Round(bb.First().SumCBM ?? 0, 3);
            else
                res += "$0$0";

            return res;

        }
        public string RecConsumerCreate(string WhCode, string ReceiptId, int RecLossId, int Qty, string CreateUser)
        {

            int addValueServiceCount = idal.IAddValueServiceDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode && u.RecLossId == RecLossId).Count();
            if (addValueServiceCount > 0)
                return "耗材已经存在,请删除后再添加!";

            List<RecLoss> recLossList = idal.IRecLossDAL.SelectBy(u => u.Id == RecLossId && u.WhCode == WhCode);
            if (recLossList.Count == 1)
            {
                RecLoss recLoss = recLossList.First();
                AddValueService addValueService = new AddValueService();
                addValueService.Price = recLoss.Price;
                addValueService.Qty = Qty;
                addValueService.ReceiptId = ReceiptId;
                addValueService.WhCode = WhCode;
                addValueService.RecLossTypeId = recLoss.RecLossTypeId;
                addValueService.RecLossId = RecLossId;
                addValueService.CreateDate = DateTime.Now;
                addValueService.CreateUser = CreateUser;
                idal.IAddValueServiceDAL.Add(addValueService);
                ReceiptRegister receiptRegister = new ReceiptRegister();
                receiptRegister.AddServiceStatus = "U";
                receiptRegister.UpdateUser = CreateUser;
                receiptRegister.UpdateDate = DateTime.Now;
                idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.WhCode == WhCode && u.ReceiptId == ReceiptId, new string[] { "AddServiceStatus", "UpdateUser", "UpdateDate" });

                idal.SaveChanges();
                return "Y";

            }
            else
                return "耗材错误!";

        }

        public string RecCreateSkuBatch(ReceiptInsert rec, string eFlag)
        {

            List<Pallate> lpList = new List<Pallate>();
            List<ReceiptInsert> receiptInsertList = new List<ReceiptInsert>();
            string sku = rec.RecModeldetail.First().AltItemNumber;

            //简易收货
            if (eFlag == "1")
            {
                //插入SO当托盘
                lpList.Add(new Pallate { HuId = rec.SoNumber, WhCode = rec.WhCode });
                foreach (RecModeldetail item0 in rec.RecModeldetail)
                {
                    //批量托盘收货
                    item0.UnitName = "LP";
                }
                rec.HuId = rec.SoNumber;
                receiptInsertList.Add(rec);
            }
            else
            {

                List<SerialNumberInOutExtend> serialNumberInOutExtendList = idal.ISerialNumberInOutExtendDAL.SelectBy(u => u.ClientCode == rec.ClientCode && u.SoNumber == rec.SoNumber && u.WhCode == rec.WhCode && u.AltItemNumber == sku).ToList();
                int recQty = rec.RecModeldetail.First().Qty;
                if (serialNumberInOutExtendList.Count > 0)
                {

                    int ediQty = serialNumberInOutExtendList.Count;
                    if (recQty != ediQty)
                    {

                        return "实收:" + recQty + " 登记:" + ediQty + " 数量不同!";
                    }


                    foreach (SerialNumberInOutExtend item in serialNumberInOutExtendList)
                    {
                        //插入托盘
                        lpList.Add(new Pallate { HuId = item.CartonId, WhCode = rec.WhCode });

                        ReceiptInsert RInsert = new ReceiptInsert();
                        RInsert.ClientCode = rec.ClientCode;
                        RInsert.ClientId = rec.ClientId;
                        RInsert.WhCode = rec.WhCode;
                        RInsert.SoNumber = rec.SoNumber;
                        RInsert.CustomerPoNumber = rec.CustomerPoNumber;
                        RInsert.CreateUser = rec.CreateUser;
                        RInsert.ReceiptId = rec.ReceiptId;
                        RInsert.Location = rec.Location;
                        RInsert.HuId = item.CartonId;

                        foreach (RecModeldetail item0 in rec.RecModeldetail)
                        {
                            //批量托盘收货
                            item0.UnitName = "LP";
                            item0.Qty = 1;
                        }

                        RInsert.RecModeldetail = rec.RecModeldetail;
                        receiptInsertList.Add(RInsert);
                    }


                }
                else
                {
                    return "实收:" + recQty + " 登记:0 数量不同!";
                }

            }



            RootManager rM = new RootManager();
            //插入托盘
            rM.WhPallateListAddS(lpList);
            RecManager aa = new RecManager();
            string res = aa.ReceiptInsert(receiptInsertList);

            return res;
        }

        public string RecReMarkIn(string ReceiptId, string WhCode, string userName, string recRemark)
        {


            ReceiptRegister receiptRegister = new ReceiptRegister();
            receiptRegister.Remark = recRemark;
            receiptRegister.UpdateUser = userName;
            receiptRegister.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.WhCode == WhCode && u.ReceiptId == ReceiptId, new string[] { "Remark", "UpdateUser", "UpdateDate" });


            TranLog tranLog = new TranLog();
            tranLog.TranType = "117";
            tranLog.Description = "收货备注添加";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = userName;
            tranLog.WhCode = WhCode;
            tranLog.ReceiptId = ReceiptId;
            tranLog.Remark = recRemark;
            idal.ITranLogDAL.Add(tranLog);

            idal.SaveChanges();
            return "Y";

        }
        public string RecEIAssign(string ReceiptId, string WhCode, string userName)
        {

            if ((from a in idal.IReceiptRegisterDAL.SelectAll()
                 where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.Status == "U"
                 select a.Id).Count() == 1)
            {

                ReceiptRegister receiptRegister = new ReceiptRegister();
                receiptRegister.BeginReceiptDate = DateTime.Now;
                receiptRegister.UpdateUser = userName;
                receiptRegister.UpdateDate = DateTime.Now;
                idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.WhCode == WhCode && u.ReceiptId == ReceiptId, new string[] { "BeginReceiptDate", "UpdateUser", "UpdateDate" });
                TranLog tranLog = new TranLog();
                tranLog.TranType = "101";
                tranLog.Description = "开始卸货";
                tranLog.TranDate = DateTime.Now;
                tranLog.TranUser = userName;
                tranLog.WhCode = WhCode;
                tranLog.ReceiptId = ReceiptId;
                idal.ITranLogDAL.Add(tranLog);
                idal.SaveChanges();
                return "Y";
            }
            else
                return "收货操作单状态已经开始收货,开始失败!";
        }
        public string ReceiptFastCheck(string ReceiptId, string WhCode)
        {
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
                      join c in idal.IInBoundSODAL.SelectAll() on b.SoId equals c.Id
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                      select c.SoNumber;
            if (sql.Count() > 0)
            {
                return sql.First();
            }
            else
                return "N";
        }

        //sku转itemId
        public string RecItemNumberToId(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber)
        {

            return recHelper.RecItemNumberToId(ItemNumber, WhCode, ReceiptId, CustomerPoNumber, SoNumber);
        }

        //sku转itemIds
        public List<int> RecItemNumberToIds(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber)
        {

            return recHelper.RecItemNumberToIds(ItemNumber, WhCode, ReceiptId, CustomerPoNumber, SoNumber);
        }


        public bool CheckSoPo(string ReceiptId, string WhCode, string SoNumber, string CustomerPoNumber)
        {
            return recHelper.CheckSoPo(ReceiptId, WhCode, SoNumber, CustomerPoNumber);
        }



        public bool CheckRecLocation(string WhCode, string Location)
        {
            //验证收货门区
            return recHelper.CheckRecLocation(WhCode, Location);
        }

        public bool CheckReturnLocation(string WhCode, string Location)
        {
            //验证退货门区
            return recHelper.CheckReturnLocation(WhCode, Location);
        }

        //验证收货门区
        public bool CheckRecLocation(string WhCode, string Location, string ReceiptId)
        {
            int? DSFLag = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode).First().DSFLag;
            //直装收货,验证备货门区
            if (DSFLag == 1)
                return recHelper.CheckOutLocation(WhCode, Location);
            else
                //验证收货门区
                return recHelper.CheckRecLocation(WhCode, Location);
        }

        public bool CheckPlt(string WhCode, string HuId)
        {
            return recHelper.CheckPlt(WhCode, HuId);
        }



        public bool CheckSku(string ReceiptId, string WhCode, int ItemId, string CustomerPoNumber)
        {
            return recHelper.CheckSku(ReceiptId, WhCode, ItemId, CustomerPoNumber);
        }
        public string GetSkuAltItemNumber(int ItemId)
        {
            return recHelper.GetSkuAltItemNumber(ItemId);
        }

        public List<RecLotFlagDescription> GetRecLotFlag()
        {

            var sql = (from a in idal.ILookUpDAL.SelectAll()
                       where a.TableName == "Receipt" && a.ColumnName == "LotFlag" && a.ColumnKey != "0"
                       select new
                       {
                           a.ColumnKey,
                           a.Description
                       }).ToList();

            List<RecLotFlagDescription> list = new List<RecLotFlagDescription>();
            foreach (var item in sql)
            {
                RecLotFlagDescription aa = new RecLotFlagDescription();
                aa.LotFlag = Convert.ToInt32(item.ColumnKey);
                aa.Description = item.Description;
                list.Add(aa);
            }

            return list;


            //return  (from a in idal.ILookUpDAL.SelectAll()
            //          where a.TableName== "Receipt"&&a.ColumnName== "LotFlag"
            //          select new RecLotFlagDescription
            //          {
            //              LotFlag= Convert.ToInt32(a.ColumnKey),
            //              Description=a.Description
            //          }).ToList();

        }

        public List<HuDetailRemained> RecScanRemainedPlt(string WhCode, string HuId)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();
            //  List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            var huDetailList = from a in idal.IHuDetailDAL.SelectAll()
                               join b in idal.IItemMasterDAL.SelectAll() on new { A = a.ItemId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode }
                               where a.HuId == HuId && a.WhCode == WhCode
                               select new
                               {
                                   a.Id,
                                   a.WhCode,
                                   a.HuId,
                                   a.ItemId,
                                   a.AltItemNumber,
                                   a.UnitName,
                                   a.Qty,
                                   a.Length,
                                   a.Width,
                                   a.Height,
                                   a.Weight,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.ClientCode,
                                   a.ClientId,
                                   b.Style1,
                                   b.Style2,
                                   b.Style3
                               };


            List<HuDetailRemained> huDetailRemainedList = new List<HuDetailRemained>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (var item in huDetailList)
                {
                    HuDetailRemained huDetailRemained = new HuDetailRemained();
                    huDetailRemained.Id = item.Id;
                    huDetailRemained.HuId = item.HuId;
                    huDetailRemained.AltItemNumber = item.AltItemNumber;
                    huDetailRemained.UnitName = item.UnitName;
                    huDetailRemained.Qty = item.Qty;
                    huDetailRemained.Length = item.Length;
                    huDetailRemained.Width = item.Width;
                    huDetailRemained.Height = item.Height;
                    huDetailRemained.Weight = item.Weight;
                    huDetailRemained.LotNumber1 = item.LotNumber1;
                    huDetailRemained.LotNumber2 = item.LotNumber2;
                    huDetailRemained.LotDate = item.LotDate;
                    huDetailRemained.SoNumber = item.SoNumber;
                    huDetailRemained.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailRemained.ClientCode = item.ClientCode;
                    huDetailRemained.Style1 = item.Style1;
                    huDetailRemained.Style2 = item.Style2;
                    huDetailRemained.Style3 = item.Style3;
                    huDetailRemained.SerialNumberModel = (from u in idal.ISerialNumberInDAL.SelectAll()
                                                          where u.ClientId == item.ClientId && u.WhCode == item.WhCode && u.HuId == item.HuId
                                                                  && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber
                                                                  && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId
                                                                  && (u.LotNumber1 == item.LotNumber1)
                                                                  && (u.LotNumber2 == item.LotNumber2)
                                                                  && u.LotDate == item.LotDate
                                                                   && u.Length == item.Length && u.Height == item.Height && u.Width == item.Width && u.Weight == u.Weight
                                                          select new SerialNumberModel { CartonId = u.CartonId }).ToList();

                    huDetailRemainedList.Add(huDetailRemained);
                }
                return huDetailRemainedList;
            }
        }

        public List<HuDetailRemained> RecScanUPCPlt(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            //  List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            var huDetailList = from a in idal.IHuDetailDAL.SelectAll()
                               join b in idal.IItemMasterDAL.SelectAll() on new { A = a.ItemId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode }
                               where a.HuId == HuId && a.WhCode == WhCode
                               select new
                               {
                                   a.Id,
                                   a.WhCode,
                                   a.HuId,
                                   a.ItemId,
                                   a.AltItemNumber,
                                   a.UnitName,
                                   a.Qty,
                                   a.Length,
                                   a.Width,
                                   a.Height,
                                   a.Weight,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.ClientCode,
                                   a.ClientId,
                                   b.Style1,
                                   b.Style2,
                                   b.Style3
                               };


            List<HuDetailRemained> huDetailRemainedList = new List<HuDetailRemained>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (var item in huDetailList)
                {
                    HuDetailRemained huDetailRemained = new HuDetailRemained();
                    huDetailRemained.Id = item.Id;
                    huDetailRemained.HuId = item.HuId;
                    huDetailRemained.AltItemNumber = item.AltItemNumber;
                    huDetailRemained.UnitName = item.UnitName;
                    huDetailRemained.Qty = item.Qty;
                    huDetailRemained.Length = item.Length;
                    huDetailRemained.Width = item.Width;
                    huDetailRemained.Height = item.Height;
                    huDetailRemained.Weight = item.Weight;
                    huDetailRemained.LotNumber1 = item.LotNumber1;
                    huDetailRemained.LotNumber2 = item.LotNumber2;
                    huDetailRemained.LotDate = item.LotDate;
                    huDetailRemained.SoNumber = item.SoNumber;
                    huDetailRemained.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailRemained.ClientCode = item.ClientCode;
                    huDetailRemained.Style1 = item.Style1;
                    huDetailRemained.Style2 = item.Style2;
                    huDetailRemained.Style3 = item.Style3;
                    huDetailRemained.SerialNumberModel = (from u in idal.IHeportSerialNumberInDAL.SelectAll()
                                                          where u.ClientId == item.ClientId && u.WhCode == item.WhCode && u.HuId == item.HuId
                                                                  && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber
                                                                  && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId
                                                          select new SerialNumberModel { CartonId = u.CartonId }).ToList();

                    huDetailRemainedList.Add(huDetailRemained);
                }
                return huDetailRemainedList;
            }
        }

        public string RecScanRemainedComplete(HuDetailRemained huDetailRemained)
        {

            if (huDetailRemained.SerialNumberModel != null)
            {
                // List<string> a = huDetailRemained.SerialNumberModel.Select(u => u.CartonId).ToList();

                if (huDetailRemained.SerialNumberModel.Count != huDetailRemained.SerialNumberModel.Select(u => u.CartonId).ToList().Distinct().Count())
                    return "提交SN数据有重复!";
            }



            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailRemained.Id);
            HuDetail huDetail = new HuDetail();
            if (huDetailList.Count == 0)
            {
                return "托盘未收货,扫描失败!";
            }
            else
                huDetail = huDetailList.First();


            //   List<SerialNumberIn> serialNumberInList = idal.ISerialNumberInDAL.SelectBy(u=>u.ReceiptId== huDetail.ReceiptId&& u.WhCode== huDetail.WhCode);


            var serialNumberInList = (from a in idal.ISerialNumberInDAL.SelectAll()
                                      join b in idal.IHuMasterDAL.SelectAll() on new { a.ReceiptId, a.WhCode, a.HuId } equals new { b.ReceiptId, b.WhCode, b.HuId } into a_join
                                      from c in a_join.DefaultIfEmpty()
                                      where a.ReceiptId == huDetail.ReceiptId && a.WhCode == huDetail.WhCode
                                      select new { a.CartonId, c.Location, c.HuId }).ToList();


            var serialNumberList = (from a in serialNumberInList
                                    join b in huDetailRemained.SerialNumberModel on a.CartonId equals b.CartonId
                                    select new { a.CartonId, a.HuId, a.Location }).ToList();
            string res = "";
            if (serialNumberList.Count > 0)
            {

                foreach (var item in serialNumberList.Distinct())
                {
                    res += item.CartonId + "在 " + item.Location + " " + item.HuId + " ;";
                }
                return res + " 重复,保存失败!";
            }


            int i = 0;
            foreach (SerialNumberModel item in huDetailRemained.SerialNumberModel)
            {
                //插入采集箱号表
                SerialNumberIn serial = new SerialNumberIn();
                serial.WhCode = huDetail.WhCode;
                serial.ReceiptId = huDetail.ReceiptId;
                serial.ClientId = huDetail.ClientId;
                serial.ClientCode = huDetail.ClientCode;
                serial.SoNumber = huDetail.SoNumber;
                serial.CustomerPoNumber = huDetail.CustomerPoNumber;
                serial.AltItemNumber = huDetail.AltItemNumber;
                serial.ItemId = huDetail.ItemId;
                serial.HuId = huDetail.HuId;
                serial.Length = huDetail.Length;
                serial.Width = huDetail.Width;
                serial.Height = huDetail.Height;
                serial.Weight = huDetail.Weight;
                serial.LotNumber1 = huDetail.LotNumber1;
                serial.LotNumber2 = huDetail.LotNumber2;
                serial.LotDate = huDetail.LotDate;
                serial.ToOutStatus = 1;
                serial.CreateUser = huDetailRemained.UserName;
                serial.CreateDate = DateTime.Now;
                serial.CartonId = item.CartonId;
                serial.Id = i;
                idal.ISerialNumberInDAL.Add(serial);

                if (item.SerialNumberDetail != null)
                {
                    //添加UPC明细
                    foreach (SerialNumberDetailModel item0 in item.SerialNumberDetail)
                    {
                        SerialNumberDetail snDetail = new SerialNumberDetail();
                        snDetail.PCS = item0.PCS;
                        snDetail.UPC = item0.UPC;
                        snDetail.SNType = "I";
                        snDetail.HeadId = serial.Id;
                        snDetail.CreateDate = DateTime.Now;
                        snDetail.CreateUser = huDetailRemained.UserName;
                        idal.ISerialNumberDetailDAL.Add(snDetail);

                    }
                }
                i++;
            }
            idal.SaveChanges();
            return "Y";
        }

        public string RecUPCComplete(HuDetailRemained huDetailRemained)
        {


            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailRemained.Id);
            HuDetail huDetail = new HuDetail();
            if (huDetailList.Count == 0)
            {
                return "托盘未收货,扫描失败!";
            }
            else
                huDetail = huDetailList.First();





            int i = 0;
            foreach (SerialNumberModel item in huDetailRemained.SerialNumberModel)
            {
                //插入采集箱号表
                HeportSerialNumberIn serial = new HeportSerialNumberIn();
                serial.WhCode = huDetail.WhCode;
                serial.ReceiptId = huDetail.ReceiptId;
                serial.ClientId = huDetail.ClientId;
                serial.ClientCode = huDetail.ClientCode;
                serial.SoNumber = huDetail.SoNumber;
                serial.CustomerPoNumber = huDetail.CustomerPoNumber;
                serial.AltItemNumber = huDetail.AltItemNumber;
                serial.ItemId = huDetail.ItemId;
                serial.HuId = huDetail.HuId;
                serial.Length = huDetail.Length;
                serial.Width = huDetail.Width;
                serial.Height = huDetail.Height;
                serial.Weight = huDetail.Weight;
                serial.CreateUser = huDetailRemained.UserName;
                serial.CreateDate = DateTime.Now;
                serial.CartonId = item.CartonId;
                serial.Id = i;
                idal.IHeportSerialNumberInDAL.Add(serial);
                i++;
            }
            idal.SaveChanges();
            return "Y";
        }



        public string RecScanCheck(HuDetailRemained huDetailRemained)
        {

            if (huDetailRemained.SerialNumberModel != null)
            {
                if (huDetailRemained.SerialNumberModel.Count != huDetailRemained.SerialNumberModel.Distinct().Count())
                    return "提交SN数据有重复!";
            }



            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailRemained.Id);
            HuDetail huDetail = new HuDetail();
            if (huDetailList.Count == 0)
            {
                return "托盘未收货,扫描失败!";
            }
            else
                huDetail = huDetailList.First();

            int checkCount = (from a in huDetailRemained.SerialNumberModel
                              join b in idal.IR_SerialNumberInOutDAL.SelectAll() on a.CartonId equals b.CartonId
                              where b.SoNumber == huDetail.SoNumber
                               && b.CustomerPoNumber == huDetail.CustomerPoNumber
                               && b.AltItemNumber == huDetail.AltItemNumber
                              select a.CartonId).Count();
            int oldCount = huDetailRemained.SerialNumberModel.Count;
            if (checkCount != oldCount)
            {
                return "共:" + oldCount + " ,有效数为:" + checkCount;
            }
            return "Y";
        }

        public string RecUPCCheck(HuDetailRemained huDetailRemained)
        {

            List<string> CartonIdList = huDetailRemained.SerialNumberModel.Select(u => u.CartonId).ToList();

            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == huDetailRemained.Id);
            HuDetail huDetail = new HuDetail();
            if (huDetailList.Count == 0)
            {
                return "托盘未收货,扫描失败!";
            }
            else
                huDetail = huDetailList.First();

            //int checkCount = (from a in idal.IHeport_R_SerialNumberInOutDAL.SelectAll()
            //                  where a.SoNumber == huDetail.SoNumber
            //                   && a.CustomerPoNumber == huDetail.CustomerPoNumber
            //                   && a.AltItemNumber == huDetail.AltItemNumber
            //                   && CartonIdList.Contains(a.CartonId)
            //                  select a.CartonId).Distinct().Count();


            var checkList = (from a in idal.IHeport_R_SerialNumberInOutDAL.SelectAll()
                             where a.SoNumber == huDetail.SoNumber
                              && a.CustomerPoNumber == huDetail.CustomerPoNumber
                              && a.AltItemNumber == huDetail.AltItemNumber
                              && CartonIdList.Contains(a.CartonId)
                             select new { a.CartonId }).ToList();
            int oldCount = CartonIdList.Distinct().Count();
            int checkCount = checkList.Distinct().Count();
            string res = "";
            if (checkCount != oldCount)
            {
                var errList = from a in huDetailRemained.SerialNumberModel
                              join b in checkList on a.CartonId equals b.CartonId
                              into b_join
                              from b in b_join.DefaultIfEmpty()
                              select new { CartonId = a.CartonId, CartonIdE = (b == null ? "" : (b.CartonId ?? " ")) };

                foreach (var item in errList)
                {
                    if (item.CartonIdE == "")
                        res += item.CartonId + ";";
                }


                return "共:" + oldCount + " ,有效数为:" + checkCount + " UPC:" + res + "无效";
            }
            return "Y";

        }


        public string RecScanCheck(List<SerialNumberInModel> serialNumberInModelList, int checkPartFlag, string ClientCode, string SoNumber, string CustomerPoNumber, string AltItemNumber, string WhCode)
        {

            if (serialNumberInModelList != null)
            {
                if (serialNumberInModelList.Count != serialNumberInModelList.Distinct().Count())
                    return "提交SN数据有重复!";
            }

            var serialNumberList = (from c in idal.ISerialNumberInDAL.SelectAll()
                                    where c.WhCode == WhCode && c.SoNumber == SoNumber && c.CustomerPoNumber == CustomerPoNumber
                                    && c.AltItemNumber == AltItemNumber && c.ClientCode == ClientCode
                                    select new { c.CartonId }).ToList();

            string res = "";
            if (serialNumberList.Count > 0)
            {
                var errList = from a in serialNumberList
                              join b in serialNumberInModelList on a.CartonId equals b.CartonId
                              select a;
                if (errList.Count() > 0)
                {
                    foreach (var item in errList)
                    {
                        res += item.CartonId + ";";
                    }
                    return res + " 重复 ";
                }
            }



            List<SerialNumberInModel> sysSN = (from c in idal.IR_SerialNumberInOutDAL.SelectAll()
                                               where c.WhCode == WhCode && c.SoNumber == SoNumber && c.CustomerPoNumber == CustomerPoNumber
                                               && c.AltItemNumber == AltItemNumber && c.ClientCode == ClientCode
                                               select new SerialNumberInModel { CartonId = c.CartonId }).ToList();
            List<SerialNumberInModel> errorSN = new List<SerialNumberInModel>();
            if (sysSN.Count() > 0)
            {

                errorSN = (from a in serialNumberInModelList
                           join b in sysSN on a.CartonId equals b.CartonId
                           into a_join
                           from b in a_join.DefaultIfEmpty()
                           where b == null
                           select a).ToList();

            }
            //没有任何扫描数据,不是部分验证的时候返回扫描不存在,就是把整个传进来的LIST传回去,如果checkPartFlag=1说明不需要验证,只检查有没有重复的就行,所以不需要返回任何数据
            else if (checkPartFlag != 1)
            {
                errorSN = serialNumberInModelList;
            }

            // var errorSNList = errorSN;

            foreach (var item in errorSN)
            {
                if (res == "")
                    res += item.CartonId;
                else
                    res += ";" + item.CartonId;
            }

            if (res != "")
            {
                return "扫描总数:" + serialNumberInModelList.Count + " ,错误SN为;" + res;
            }
            return "Y";
        }
        public RecSkuDataCe GetRecSkuDataCe(int ItemId)
        {

            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      where a.Id == ItemId
                      select new RecSkuDataCe { ItemId = ItemId, AltItemNumber = a.AltItemNumber, EAN = a.EAN, Style1 = a.Style1, Style2 = a.Style2, Style3 = a.Style3 };
            if (sql.Count() > 0)
                return sql.First();
            else
                return null;
        }

        public List<RecSkuDataCe> GetRecSkuDataCeList(string ReceiptId, string SoNumber, string CustomerPoNumber, string WhCode)
        {
            List<RecSkuDataCe> list = (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                                       join b in idal.IItemMasterDAL.SelectAll() on a.ItemId equals b.Id
                                       where a.ReceiptId == ReceiptId && a.WhCode == WhCode && a.CustomerPoNumber == CustomerPoNumber
                                       select new RecSkuDataCe { ItemId = a.ItemId, AltItemNumber = a.AltItemNumber, EAN = b.EAN, RegQty = a.RegQty }).ToList();
            if (list.Count() > 0)
            {
                foreach (RecSkuDataCe item in list)
                {
                    List<int> itemList = new List<int>();
                    itemList.Add(item.ItemId);

                    item.RecQty = GetSkuRecQty(ReceiptId, WhCode, itemList, SoNumber, CustomerPoNumber);
                }
                return list;
            }
            else
                return null;
        }




        public List<RecSoModel> GetRecSoList(string WhCode, string ReceiptId, string SoNumber)
        {


            List<RecSoModel> res = (from a in idal.IReceiptDAL.SelectAll()
                                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                                    && (a.SoNumber == SoNumber || SoNumber == null)
                                    group a by new { a.ReceiptId, a.WhCode, a.SoNumber } into b
                                    select new RecSoModel { RecQty = b.Sum(x => x.Qty), RecCBM = b.Sum(x => x.Length * x.Width * x.Height * x.Qty), SoNumber = b.FirstOrDefault().SoNumber, WhCode = WhCode, ReceiptId = ReceiptId }).ToList();
            if (res.Count() > 0)
                return res;
            else
                return null;
        }


        public List<string> GetRecReturnOrder(string RecReturnOrderNumber, string WhCode)
        {

            var sql = from a in idal.IInBoundOrderDAL.SelectAll()
                      join b in idal.IReceiptRegisterDetailDAL.SelectAll() on a.Id equals b.PoId
                      join c in idal.IReceiptRegisterDAL.SelectAll() on new { A = b.ReceiptId, B = b.WhCode } equals new { A = c.ReceiptId, B = c.WhCode }
                      where a.WhCode == WhCode && (c.Status == "U" || c.Status == "A") && a.AltCustomerPoNumber.Contains(RecReturnOrderNumber)
                      select c.ReceiptId + "$" + b.CustomerPoNumber;
            if (sql.Count() > 0)
                return sql.Distinct().ToList();
            else
                return null;
        }




        //实收数量
        public int GetSkuRecQty(string ReceiptId, string WhCode, List<int> ItemIdArry, string SoNumber, string CustomerPoNumber)
        {


            var sql = from a in idal.IReceiptDAL.SelectAll()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId && ItemIdArry.Contains(a.ItemId)
                      && (a.SoNumber == SoNumber || a.SoNumber == (SoNumber == null ? "" : SoNumber))
                      && a.CustomerPoNumber == CustomerPoNumber
                      select a.Qty;
            if (sql.Count() > 0)
                return sql.Sum();
            else
                return 0;
        }
        //预录入数量
        public int GetSkuRegQty(string ReceiptId, string WhCode, List<int> ItemIdArry, string SoNumber, string CustomerPoNumber)
        {
            //var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
            //          where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.ItemId == ItemId
            //          && a.CustomerPoNumber == CustomerPoNumber
            //          select a.RegQty;
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
                      join c in idal.IInBoundSODAL.SelectAll() on b.SoId equals c.Id into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId && ItemIdArry.Contains(a.ItemId)
                      && a.CustomerPoNumber == CustomerPoNumber && (c.SoNumber == SoNumber || c.SoNumber == (SoNumber == null ? "" : SoNumber))
                      select a.RegQty;

            if (sql.Count() > 0)
                return sql.Sum();
            else
                return 0;
        }


        public RecSkuDataCe GetSkuRegCBMWeight(string ReceiptId, string WhCode, int ItemId, string SoNumber, string CustomerPoNumber)
        {
            //var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
            //          where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.ItemId == ItemId
            //          && a.CustomerPoNumber == CustomerPoNumber
            //          select a.RegQty;
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      join b in idal.IInBoundOrderDAL.SelectAll() on a.PoId equals b.Id
                      join d in idal.IInBoundOrderDetailDAL.SelectAll() on a.InOrderDetailId equals d.Id
                      join c in idal.IInBoundSODAL.SelectAll() on b.SoId equals c.Id into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId && a.ItemId == ItemId
                      && a.CustomerPoNumber == CustomerPoNumber && (c.SoNumber == SoNumber || c.SoNumber == (SoNumber == null ? "" : SoNumber))
                      select new { d.CBM, d.Qty, d.Weight, d.CustomDate, d.CustomNumber1 };

            if (sql.Count() > 0)
            {
                var dataSelect = sql.ToList();
                RecSkuDataCe data = new RecSkuDataCe();
                if (dataSelect.First().Weight != null)
                    data.Weight = (decimal)dataSelect.First().Weight / dataSelect.First().Qty;
                if (dataSelect.First().CBM != null)
                    data.CBM = (decimal)(dataSelect.First().CBM / dataSelect.First().Qty);
                if (dataSelect.First().CustomDate != null)
                    data.LotDate = dataSelect.First().CustomDate;
                if (dataSelect.First().CustomNumber1 != null)
                    data.LotNumber1 = dataSelect.First().CustomNumber1;
                return data;
            }
            else
                return null;
        }

        #region
        /// <summary>
        /// 获取收货批次中sku的单位列表
        /// </summary>
        /// <param name="ReceiptId">收货批次</param>
        /// <param name="WhCode"></param>
        /// <param name="ItemId">SKU的ID</param>
        /// <returns>IQueryable<RecSkuUnit></returns>
        public IQueryable<RecSkuUnit> GetUnit(string ReceiptId, string WhCode, int ItemId)
        {
            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      join b in idal.IUnitDefaultDAL.SelectAll() on new { WhCode = a.WhCode, UnitName = a.UnitName } equals new { WhCode = b.WhCode, UnitName = b.UnitName }
                      into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == WhCode && a.Id == ItemId
                      select new RecSkuUnit
                      {
                          UnitFlag = a.UnitFlag,
                          UnitName = a.UnitName,
                          UnitNameCN = (c.UnitNameCN == null ? a.UnitName : c.UnitNameCN),
                          WhCode = a.WhCode,
                          ClientCode = a.ClientCode
                      };
            if (sql.Count() > 0)
            {
                RecSkuUnit recSkuUnit = sql.First();
                if (recSkuUnit.UnitFlag == 1)
                {
                    return (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                            join b in idal.IUnitDefaultDAL.SelectAll() on new { WhCode = a.WhCode, UnitName = a.UnitName } equals new { WhCode = b.WhCode, UnitName = b.UnitName }
                            into c_join
                            from c in c_join.DefaultIfEmpty()
                            where a.ReceiptId == ReceiptId
                                && a.ItemId == ItemId
                                && a.WhCode == WhCode
                            select new RecSkuUnit { UnitName = a.UnitName, UnitNameCN = (c.UnitNameCN == null ? a.UnitName : c.UnitNameCN) });
                }
                else
                {
                    if (recSkuUnit.UnitName == "none")
                    {
                        return (from a in idal.IUnitDefaultDAL.SelectAll()
                                where a.WhCode == WhCode
                                select new RecSkuUnit { UnitName = a.UnitName, UnitNameCN = a.UnitNameCN });
                    }
                    else
                    {
                        return sql;
                    }
                }
            }
            else
                return sql;
        }

        /// <summary>
        /// 获取收货批次中sku的单位列表,取实收的单位
        /// </summary>
        /// <param name="ReceiptId">收货批次</param>
        /// <param name="WhCode"></param>
        /// <param name="ItemId">SKU的ID</param>
        /// <returns>IQueryable<RecSkuUnit></returns>
        public IQueryable<RecSkuUnit> GetUnitChange(string ReceiptId, string WhCode, int ItemId)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join b in idal.IUnitDefaultDAL.SelectAll() on new { WhCode = a.WhCode, UnitName = a.UnitName } equals new { WhCode = b.WhCode, UnitName = b.UnitName }
                      into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == WhCode && a.ItemId == ItemId && a.ReceiptId == ReceiptId
                      select new RecSkuUnit
                      {
                          UnitName = a.UnitName,
                          UnitNameCN = (c.UnitNameCN == null ? a.UnitName : c.UnitNameCN),
                          WhCode = a.WhCode,
                          ClientCode = a.ClientCode,
                          UnitFlag = (Int32)c.OrderById
                      };
            //没有收货数据
            if (sql.Count() == 0)
            {
                //返回默认菜单
                return (from a in idal.IUnitDefaultDAL.SelectAll()
                        where a.WhCode == WhCode
                        select new RecSkuUnit { UnitFlag = (Int32)a.OrderById, UnitName = a.UnitName, UnitNameCN = a.UnitNameCN });


            }
            else
                return sql;
        }
        #endregion
        /// <summary>
        /// 应该废弃了
        /// </summary>
        /// <param name="ReceiptId"></param>
        /// <param name="WhCode"></param>
        /// <param name="Percent"></param>
        /// <returns></returns>
        public string CheckRecCBMPercent(string ReceiptId, string WhCode, int Percent)
        {

            decimal regCBM = (decimal)idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).Sum(g => g.SumCBM);

            decimal recCBM = (decimal)idal.IReceiptDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).Sum(x => x.Length * x.Width * x.Height * x.Qty);

            if (regCBM == 0)
            {
                return "收货登记没有录入立方,无法验证比例!";
            }

            else if (regCBM > 0)
            {

                decimal perCBM = (recCBM - regCBM) / regCBM;
                if (Math.Abs(perCBM) * 100 > Percent)
                    return "实收:" + Convert.ToDouble(recCBM) + " 登记:" + Convert.ToDouble(regCBM) + " 相差比例:" + Convert.ToInt32(perCBM * 100) + "%";
                else
                    return "Y";
            }
            else
                return "Y";
        }
        public string CheckRecCBMPercent(string ReceiptId, string WhCode)
        {

            decimal regCBM = (decimal)idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).Sum(g => g.SumCBM);

            decimal recCBM = (decimal)idal.IReceiptDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).Sum(x => x.Length * x.Width * x.Height * x.Qty);

            if (regCBM == 0)
            {
                return "收货登记没有录入立方,无法验证比例!";
            }

            else if (regCBM > 0)
            {

                decimal perCBM = (recCBM - regCBM) / regCBM;
                int? Percent = (from a in idal.IReceiptRegisterDAL.SelectAll()
                                join b in idal.IWhClientDAL.SelectAll() on new { WhCode = a.WhCode, ClientCode = a.ClientCode } equals new { WhCode = b.WhCode, ClientCode = b.ClientCode }
                                where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                                select b.RecCBMPercent).ToList().First();

                if (Math.Abs(perCBM) * 100 > Percent)
                    return "实收:" + Convert.ToDouble(recCBM) + " 登记:" + Convert.ToDouble(regCBM) + " 相差比例:" + Convert.ToInt32(perCBM * 100) + "%";
                else
                    return "Y";
            }
            else
                return "Y";
        }
        public List<RecConsumerGoodsModel> GetRecConsumerGoodsModelList(string WhCode)
        {


            return (from a in idal.IRecLossDAL.SelectAll().OrderBy(u => u.RecSort)
                    where a.WhCode == WhCode && a.Status == "Active"
                    select new RecConsumerGoodsModel { RecLossId = a.Id, RecLossName = a.RecLossName, RecLossDescription = a.RecLossDescription }).ToList();

        }


        public List<RecConsumerGoodsModel> GetRecConsumerGoodsModelList(string ReceiptId, string WhCode)
        {


            return (from a in idal.IAddValueServiceDAL.SelectAll()
                    join b in idal.IRecLossDAL.SelectAll() on a.RecLossId equals b.Id
                    where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                    select new RecConsumerGoodsModel { RecLossId = a.Id, RecLossName = b.RecLossName, RecLossDescription = b.RecLossDescription, Qty = a.Qty }).ToList();

        }

        public string RecConsumerDelete(string ReceiptId, string WhCode, string userName)
        {

            ReceiptRegister receiptRegister = new ReceiptRegister();
            receiptRegister.AddServiceStatus = "U";
            receiptRegister.UpdateUser = userName;
            receiptRegister.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.WhCode == WhCode && u.ReceiptId == ReceiptId, new string[] { "AddServiceStatus", "UpdateUser", "UpdateDate" });
            idal.IAddValueServiceDAL.DeleteBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);


            TranLog tranLog = new TranLog();
            tranLog.TranType = "116";
            tranLog.Description = "收货耗材明细删除";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = userName;
            tranLog.WhCode = WhCode;
            tranLog.ReceiptId = ReceiptId;
            idal.ITranLogDAL.Add(tranLog);

            idal.SaveChanges();
            return "Y";



        }
        public string DcReturnExceptionInsert(DcReturnExceptionIn dcReturnExceptionIn)
        {

            DcReturnException dcReturnException = new DcReturnException();
            dcReturnException.ClientCode = dcReturnExceptionIn.ClientCode;
            dcReturnException.ExpressNumber = dcReturnExceptionIn.ExpressNumber;
            dcReturnException.HuId = dcReturnExceptionIn.HuId;
            dcReturnException.InorderNumberAlt = dcReturnExceptionIn.InorderNumberAlt;
            dcReturnException.SKU = dcReturnExceptionIn.SKU;
            dcReturnException.CreateUser = dcReturnExceptionIn.CreateUser;
            dcReturnException.CreateDate = DateTime.Now;
            dcReturnException.Status = "1";
            idal.IDcReturnExceptionDAL.Add(dcReturnException);

            TranLog tranLog = new TranLog();
            tranLog.TranType = "118";
            tranLog.Description = "DC异常件收货";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = dcReturnException.CreateUser;
            tranLog.WhCode = "02";
            tranLog.AltItemNumber = dcReturnExceptionIn.SKU;
            tranLog.Remark = dcReturnException.ExpressNumber + " " + dcReturnException.InorderNumberAlt;
            tranLog.ClientCode = dcReturnException.ClientCode;
            idal.ITranLogDAL.Add(tranLog);

            idal.SaveChanges();
            return "Y";
        }
        public string EANGetItem(string WhCode, string EAN)
        {


            var sql = (from a in idal.IItemMasterDAL.SelectAll()
                       where a.WhCode == WhCode && (a.EAN == EAN || a.AltItemNumber == EAN)
                       select new { a.ClientCode, a.AltItemNumber }).ToList();

            sql = sql.Where(u => u.ClientCode != "TEST" && u.ClientCode != "dmTest").ToList();
            if (sql.Count() != 1)
                return "N$款号数据异常";
            else
                return "Y$" + sql.First().ClientCode + "$" + sql.First().AltItemNumber;

        }

        public List<WorkloadAccountModelCN> GetRecWorkloadAccountModelList(string ReceiptId, string WhCode)
        {
            var sql = (from a in idal.IWorkloadAccountDAL.SelectAll()
                       where a.WhCode == WhCode && a.ReceiptId == ReceiptId && (a.WorkType == "装卸工" || a.WorkType == "电车工")
                       select new WorkloadAccountModel
                       {
                           UserCode = a.UserCode,
                           WorkType = a.WorkType
                       }).Distinct().ToList();

            List<WorkloadAccountModelCN> list = new List<WorkloadAccountModelCN>();
            foreach (var item in sql)
            {
                WorkloadAccountModelCN aa = new WorkloadAccountModelCN();
                aa.UserCode = item.UserCode;
                aa.WorkType = item.WorkType;
                aa.UserNameCN = idal.IWhUserDAL.SelectBy(u => u.UserCode == item.UserCode && u.CompanyId == 1).First().UserNameCN;
                list.Add(aa);
            }

            return list;
        }

        public List<WorkloadAccountModelCN> GetOutWorkloadAccountModelList(string LoadId, string WhCode)
        {
            var sql = (from a in idal.IWorkloadAccountDAL.SelectAll()
                       where a.WhCode == WhCode && a.LoadId == LoadId && (a.WorkType == "装卸工" || a.WorkType == "电车工")
                       select new WorkloadAccountModel
                       {
                           UserCode = a.UserCode,
                           WorkType = a.WorkType
                       }).Distinct().ToList();

            List<WorkloadAccountModelCN> list = new List<WorkloadAccountModelCN>();
            foreach (var item in sql)
            {
                WorkloadAccountModelCN aa = new WorkloadAccountModelCN();
                aa.UserCode = item.UserCode;
                aa.WorkType = item.WorkType;
                aa.UserNameCN = idal.IWhUserDAL.SelectBy(u => u.UserCode == item.UserCode && u.CompanyId == 1).First().UserNameCN;
                list.Add(aa);
            }

            return list;
        }

    }
}
