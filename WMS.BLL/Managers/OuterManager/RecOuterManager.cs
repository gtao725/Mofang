using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.IDAL;

namespace WMS.BLL
{
    public class RecOuterManager : IRecOuterManager
    {
        IDAL.IDALSession idal = BLLHelper.GetDal();
        RecHelper recHelper = new RecHelper();
        
        public bool CheckRecLocation(string WhCode, string Location)
        {
            //验证收货门区
            return recHelper.CheckRecLocation(WhCode, Location);
        }
        //获取收货数据
        public ReceiptData GetReceiptData(ReceiptParam receiptParam)
        {
            try
            {
                ReceiptData receiptData = new ReceiptData();
                ReceiptHeadInfo receiptHeadInfo = new ReceiptHeadInfo();
                //查询收获登记表数据作为表头
                ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == receiptParam.ReceiptId).First();
                receiptHeadInfo.WhCode = receiptRegister.WhCode;
                receiptHeadInfo.ReceiptId = receiptRegister.ReceiptId;
                receiptHeadInfo.ClientCode = receiptRegister.ClientCode;
                receiptHeadInfo.RegisterDate = receiptRegister.RegisterDate.ToString();
                receiptHeadInfo.ReceiptType = receiptRegister.ReceiptType;
                receiptHeadInfo.TransportType = receiptRegister.TransportType;
                receiptHeadInfo.TruckNumber = receiptRegister.TruckNumber;
                receiptHeadInfo.TruckLength = receiptRegister.TruckLength;
                receiptHeadInfo.PhoneNumber = receiptRegister.PhoneNumber;
                //是否需要传值？
                //receiptHeadInfo.TruckStatus=receiptRegister.TruckStatus;
                //receiptHeadInfo.GreenPassFLag= receiptRegister.GreenPassFLag;
                //需按照receipt汇总还是直接取register？
                receiptHeadInfo.SumQty = receiptRegister.SumQty;
                receiptHeadInfo.SumCBM = receiptRegister.SumCBM;
                receiptHeadInfo.RecSumQty = receiptRegister.RecSumQty;
                receiptHeadInfo.RecSumCBM = receiptRegister.RecSumCBM;
                receiptHeadInfo.RecSumWeight = receiptRegister.RecSumWeight;
                receiptHeadInfo.BkDate = receiptRegister.BkDate;
                receiptHeadInfo.BkDateBegin = receiptRegister.BkDateBegin.ToString();
                receiptHeadInfo.BkDateEnd = receiptRegister.BkDateEnd.ToString();
                receiptHeadInfo.ArriveDate = receiptRegister.ArriveDate.ToString();
                receiptHeadInfo.ParkingUser = receiptRegister.ParkingUser;
                receiptHeadInfo.ParkingDate = receiptRegister.ParkingDate.ToString();
                receiptHeadInfo.BeginReceiptDate = receiptRegister.BeginReceiptDate.ToString();
                receiptHeadInfo.EndReceiptDate = receiptRegister.EndReceiptDate.ToString();
                //receiptHeadInfo.ResetCount=receiptRegister.ResetCount;
                receiptHeadInfo.TruckCount = receiptRegister.TruckCount;
                receiptHeadInfo.BillCount = receiptRegister.BillCount;
                receiptHeadInfo.DNCount = receiptRegister.DNCount;
                receiptHeadInfo.GoodType = receiptRegister.GoodType;
                receiptHeadInfo.Remark = receiptRegister.Remark;
                receiptHeadInfo.CreateUser = receiptRegister.CreateUser;
                receiptHeadInfo.CreateDate = receiptRegister.CreateDate.ToString();
                receiptHeadInfo.UpdateDate = receiptRegister.UpdateDate.ToString();
                receiptHeadInfo.UpdateUser = receiptRegister.UpdateUser;
                receiptHeadInfo.BookOrigin = receiptRegister.BookOrigin;
                //将表头放入receiptData
                receiptData.receiptHeadInfo = receiptHeadInfo;
                //查询收货表数据作为表体
                List<Receipt> ReceiptList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == receiptParam.ReceiptId).ToList();
                foreach (Receipt receipt in ReceiptList)
                {
                    ReceiptDetailInfo receiptDetailInfo = new ReceiptDetailInfo();
                    ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == receipt.ItemId && u.AltItemNumber == receipt.AltItemNumber && u.ClientCode == receipt.ClientCode).First();
                    receiptDetailInfo.WhCode = receipt.WhCode;
                    receiptDetailInfo.ReceiptId = receipt.ReceiptId;
                    receiptDetailInfo.ClientCode = receipt.ClientCode;
                    receiptDetailInfo.ReceiptDate = receipt.ReceiptDate.ToString();
                    receiptDetailInfo.HoldReason = receipt.HoldReason;
                    receiptDetailInfo.SoNumber = receipt.SoNumber;
                    receiptDetailInfo.CustomerPoNumber = receipt.CustomerPoNumber;
                    receiptDetailInfo.AltItemNumber = receipt.AltItemNumber;
                    receiptDetailInfo.ItemNumber = receipt.ItemId.ToString();
                    receiptDetailInfo.Style1 = itemMaster.Style1;
                    receiptDetailInfo.Style2 = itemMaster.Style2;
                    receiptDetailInfo.Style3 = itemMaster.Style3;
                    receiptDetailInfo.HuId = receipt.HuId;
                    receiptDetailInfo.UnitName = receipt.UnitName;
                    receiptDetailInfo.Qty = receipt.Qty;
                    receiptDetailInfo.Length = receipt.Length;
                    receiptDetailInfo.Width = receipt.Width;
                    receiptDetailInfo.Height = receipt.Height;
                    receiptDetailInfo.Weight = receipt.Weight;
                    receiptDetailInfo.LotNumber1 = receipt.LotNumber1;
                    receiptDetailInfo.LotNumber2 = receipt.LotNumber2;
                    receiptDetailInfo.LotDate = receipt.LotDate.ToString();
                    receiptDetailInfo.Attribute1 = receipt.Attribute1;
                    receiptDetailInfo.CreateUser = receipt.CreateUser;
                    receiptDetailInfo.CreateDate = receipt.CreateDate.ToString();
                    receiptDetailInfo.UpdateDate = receipt.UpdateDate.ToString();
                    receiptDetailInfo.UpdateDate = receipt.UpdateDate.ToString();
                    receiptDetailInfo.CBM = receipt.Length * receipt.Width * receipt.Height*receipt.Qty*1000000;
                    if(receiptData.receiptDetailInfoList==null)
                        receiptData.receiptDetailInfoList=new List<ReceiptDetailInfo>();
                    receiptData.receiptDetailInfoList.Add(receiptDetailInfo);
                }
                List<ReceiptWorkInfo> receiptWorkInfoList = (from a in idal.IWorkloadAccountDAL.SelectAll()
                        where   a.ReceiptId == receiptParam.ReceiptId
                        select new ReceiptWorkInfo
                        {
                            ReceiptId = a.ReceiptId,
                            WhCode = a.WhCode,
                            WorkType = a.WorkType,
                            WorkId = a.UserCode
                         }).Distinct().ToList();
                receiptData.receiptWorkInfoList = receiptWorkInfoList;
                return receiptData;
            }catch (Exception ex)
            {
                return null;
            }
        }
    }
}
