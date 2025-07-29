using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.DAL;
using WMS.IBLL;

namespace WMS.BLL
{
    public class InventoryWinceManager : IInventoryWinceManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        #region 1.收货操作

        //验证收货门区
        public bool CheckWhLocation(string WhCode, string Location)
        {
            RecHelper recHelper = new RecHelper();
            return recHelper.CheckRecLocation(WhCode, Location);
        }

        public string RecStockMove(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            //是否采用无托盘管理
            if (inventoryHelper.CheckWhCodeIsNoHuIdFlag(WhCode))
            {
                string result = RecStockMove(WhCode, Location, DestLoc, HuId, User, 1);
                return result;
            }
            else
            {
                RecHelper recHelper = new RecHelper();
                RootManager rm = new RootManager();

                //判断托盘上架目标库位是否合法
                if (rm.CheckClientZone(HuId, DestLoc, WhCode) == 0)
                    return "非保货物不允许上架到保税库位";
                if (Location == DestLoc)
                    return "目标库位与当前库位相同!";
                //检测是否有该托盘
                if (!recHelper.IfPlt(WhCode, HuId))
                    return "无此托盘!";
                //检测托盘当前库位是否与移库的当前库位相同
                string pltLocation = inventoryHelper.GetPltLocation(WhCode, HuId);
                if (pltLocation != Location)
                    return HuId + "已经在" + pltLocation;
                //验证收货门区是否正确
                if (!recHelper.CheckRecLocation(WhCode, Location))
                    return "收货门区错误!";
                //当前门区有无库存
                if (!recHelper.IfPltHaveStock(WhCode, HuId))
                    return "收货门区无此托盘!";
                if (StockMaxPltCheck(DestLoc, WhCode) != "Y")
                    return "库位:" + DestLoc + "已有超过最大托盘数!";

                //验证目标门区是否正确
                string CheckDestLocRes = inventoryHelper.CheckLocationId(DestLoc, WhCode);
                if (CheckDestLocRes != "Y") return DestLoc + CheckDestLocRes;
                //验证是不是冻结托盘上架TCR库位,正常托盘上正常库位
                // if (!CheckStockLocation(WhCode, HuId, DestLoc))
                //    return "收货门区与托盘状态不符!";
                InVentoryManager inVentoryManager = new InVentoryManager();
                HuDetailResult huMaster = new HuDetailResult();
                huMaster.Location = DestLoc;
                huMaster.WhCode = WhCode;
                huMaster.UserName = User;
                huMaster.HuId = HuId;
                if (inVentoryManager.HuMasterEdit(huMaster, new string[] { "Location", "UpdateDate", "UpdateUser" }) == 1)
                    return "Y";
                else
                    return "数据更新错误!";
            }
        }

        //Location为当前库位,DestLoc为上架库位
        //如果仓库采用无托盘上架，托盘号改为库位号
        public string RecStockMove(string WhCode, string Location, string DestLoc, string HuId, string User, int type)
        {
            RecHelper recHelper = new RecHelper();
            InventoryHelper inventoryHelper = new InventoryHelper();
            RootManager rm = new RootManager();

            //判断托盘上架目标库位是否合法
            if (rm.CheckClientZone(HuId, DestLoc, WhCode) == 0)
                return "非保货物不允许上架到保税库位";
            if (Location == DestLoc)
                return "目标库位与当前库位相同!";
            //检测是否有该托盘
            if (!recHelper.IfPlt(WhCode, HuId))
                return "无此托盘!";
            //检测托盘当前库位是否与移库的当前库位相同
            string pltLocation = inventoryHelper.GetPltLocation(WhCode, HuId);
            if (pltLocation != Location)
                return HuId + "已经在" + pltLocation;
            //验证收货门区是否正确
            if (!recHelper.CheckRecLocation(WhCode, Location))
                return "收货门区错误!";
            //当前门区有无库存
            if (!recHelper.IfPltHaveStock(WhCode, HuId))
                return "收货门区无此托盘!";
            if (StockMaxPltCheck(DestLoc, WhCode) != "Y")
                return "库位:" + DestLoc + "已有超过最大托盘数!";

            //验证目标门区是否正确
            string CheckDestLocRes = inventoryHelper.CheckLocationId(DestLoc, WhCode);
            if (CheckDestLocRes != "Y") return DestLoc + CheckDestLocRes;

            //变更无托盘库存
            string result = EditNoHuIdByHuDetail(WhCode, Location, DestLoc, HuId, User);
            if (result != "Y")
            {
                return result;
            }

            return "Y";
        }

        public string EditNoHuIdByHuDetail(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    if (idal.IPallateDAL.SelectBy(u => u.WhCode == WhCode && u.HuId == DestLoc).Count == 0)
                    {
                        Pallate pallate = new Pallate();
                        pallate.WhCode = WhCode;
                        pallate.HuId = DestLoc;
                        pallate.TypeId = 1;
                        pallate.Status = "U";
                        idal.IPallateDAL.Add(pallate);
                    }

                    //验证HuMaster 
                    HuMaster huMaster = new HuMaster();
                    List<HuMaster> GethuMasterList = idal.IHuMasterDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                    if (GethuMasterList.Count == 0)
                    {
                        //如果库位托盘不存在，直接修改原托盘为库位托盘
                        huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).First();
                        huMaster.HuId = DestLoc;
                        huMaster.Location = DestLoc;
                        huMaster.UpdateUser = User;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "HuId", "Location", "UpdateUser", "UpdateDate" });
                    }
                    else
                    {
                        //如果库位托盘存在，删除原托盘
                        idal.IHuMasterDAL.DeleteBy(u => u.WhCode == WhCode && u.HuId == HuId);
                        huMaster = GethuMasterList.First();
                    }

                    //得到托盘库存
                    List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);

                    #region 添加上架工作量
                    List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
                    foreach (var huDetail in huDetailList)
                    {
                        //得到原始数据 进行日志添加
                        TranLog tranLog = new TranLog();
                        tranLog.TranType = "105";
                        tranLog.Description = "摆货操作";
                        tranLog.TranDate = DateTime.Now;
                        tranLog.TranUser = User;
                        tranLog.WhCode = WhCode;
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
                        tranLog.Location = Location;
                        tranLog.Location2 = DestLoc;
                        tranLog.HoldId = huMaster.HoldId;
                        tranLog.HoldReason = huMaster.HoldReason;
                        idal.ITranLogDAL.Add(tranLog);

                        //插入工人工作量
                        if (addWorkList.Where(u => u.WhCode == WhCode && u.HuId == huDetail.HuId).Count() == 0)
                        {
                            WorkloadAccount work = new WorkloadAccount();
                            work.WhCode = WhCode;
                            work.ReceiptId = huDetail.ReceiptId;
                            work.ClientId = huDetail.ClientId;
                            work.ClientCode = huDetail.ClientCode;
                            work.HuId = huDetail.HuId;
                            work.WorkType = "叉车工";
                            work.UserCode = User;
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
                            WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == WhCode && u.HuId == huDetail.HuId).First();
                            addWorkList.Remove(getModel);

                            WorkloadAccount work = new WorkloadAccount();
                            work.WhCode = WhCode;
                            work.ReceiptId = huDetail.ReceiptId;
                            work.ClientId = huDetail.ClientId;
                            work.ClientCode = huDetail.ClientCode;
                            work.HuId = huDetail.HuId;
                            work.WorkType = "叉车工";
                            work.UserCode = User;
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
                    #endregion

                    //库位变更时间
                    InVentoryManager inVentoryManager = new InVentoryManager();
                    inVentoryManager.WhLocationEditChangeTime(WhCode, DestLoc);

                    //需修改托盘的List
                    List<HuDetail> EditList = new List<HuDetail>();
                    //需删除托盘的List
                    List<HuDetail> DelList = new List<HuDetail>();
                    //得到库位托盘库存
                    List<HuDetail> GetHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                    if (GetHuDetailList.Count == 0)
                    {
                        //如果以库位为托盘的库存不存在
                        //直接把原托盘变更为库位托盘
                        foreach (var huDetail in huDetailList)
                        {
                            huDetail.HuId = DestLoc;
                            huDetail.UpdateUser = User;
                            huDetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                        }
                    }
                    else
                    {
                        //以库位为托盘的库存存在
                        //证明 库位托盘有货 需要合并库存
                        foreach (var item in huDetailList)
                        {
                            if (GetHuDetailList.Where(u => u.ClientId == item.ClientId && (u.SoNumber ?? "") == (item.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (item.CustomerPoNumber ?? "") && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.ReceiptId ?? "") == (item.ReceiptId ?? "") && (u.Length ?? 0) == (item.Length ?? 0) && (u.Width ?? 0) == (item.Width ?? 0) && (u.Height ?? 0) == (item.Height ?? 0) && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).Count() == 0)
                            {
                                //如果库位托盘不存在该款号，直接修改原托盘库存信息为库位托盘
                                //原托盘明细修改为库位托盘明细： HuId变更为Location
                                EditList.Add(item);
                            }
                            else
                            {
                                //如果库位托盘存在该款号，需增加库位托盘数量后删除原托盘明细
                                //库位托盘数量++
                                //原托盘明细删除
                                DelList.Add(item);
                            }
                        }

                        foreach (var huDetail in EditList)
                        {
                            huDetail.HuId = DestLoc;
                            huDetail.UpdateUser = User;
                            huDetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                        }

                        List<HuDetail> checkList = new List<HuDetail>();
                        foreach (var item in DelList)
                        {
                            HuDetail getHuDetail = GetHuDetailList.Where(u => u.ClientId == item.ClientId && (u.SoNumber ?? "") == (item.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (item.CustomerPoNumber ?? "") && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.ReceiptId ?? "") == (item.ReceiptId ?? "") && (u.Length ?? 0) == (item.Length ?? 0) && (u.Width ?? 0) == (item.Width ?? 0) && (u.Height ?? 0) == (item.Height ?? 0) && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).First();

                            if (checkList.Where(u => u.Id == getHuDetail.Id).Count() == 0)
                            {
                                getHuDetail.Qty += item.Qty;
                                getHuDetail.UpdateUser = User;
                                getHuDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(getHuDetail, u => u.Id == getHuDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                checkList.Add(getHuDetail);
                            }
                            else
                            {
                                HuDetail oldHudetail = checkList.Where(u => u.Id == getHuDetail.Id).First();
                                checkList.Remove(oldHudetail);

                                HuDetail newHudetail = oldHudetail;
                                newHudetail.Qty = newHudetail.Qty + item.Qty;
                                newHudetail.UpdateUser = User;
                                newHudetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(newHudetail, u => u.Id == newHudetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                checkList.Add(newHudetail);
                            }

                            idal.IHuDetailDAL.DeleteBy(u => u.Id == item.Id);
                        }
                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "操作异常请重试！";
                }
            }
        }

        public bool CheckStockLocation(string WhCode, string HuId, string DestLoc)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            //库位状态
            List<WhLocation> listLoc = idal.IWhLocationDAL.SelectBy(u => u.LocationId == DestLoc && u.WhCode == WhCode);
            if (listLoc.Count == 1)
                //库位未锁定
                if (listLoc.First().Status == "A")
                {
                    //托盘状态
                    List<HuMaster> listPltStatus = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);

                    if (listPltStatus.Count() > 0)
                        //托盘未锁定,并且目标库位不是TCR库位
                        if (listPltStatus.First().Status == "A" && listLoc.First().LocationTypeId != 5)
                            return true;
                        else
                            //库位是TCR库位,托盘锁定
                            return listLoc.First().LocationTypeId == 5 && listPltStatus.First().Status == "H";
                    else
                        return false;
                }
                else
                    return false;
            else
                return false;
        }

        public bool CheckSupplementLocation(string WhCode, string Location, string DestLoc)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            return idal.IWhLocationDAL.SelectBy(u => (u.LocationId == DestLoc || u.Location == Location) && u.LocationTypeDetailId == 1 && u.WhCode == WhCode).Count == 0;

        }



        public string IfPltStock(string WhCode, string Location, string HuId)
        {
            RecHelper recHelper = new RecHelper();
            //检测是否有该托盘
            if (recHelper.IfPlt(WhCode, HuId))
            {
                if (IfStockInLoc(WhCode, Location, HuId))
                {

                    return IfPlt(WhCode, HuId, Location);
                }
                else
                    return "该托盘不在此库位中!";
            }
            else
                return "无此托盘!";

        }
        public string IfPlt(string WhCode, string HuId, string Location)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();

            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      join b in idal.IReceiptRegisterDAL.SelectAll() on new { A = a.WhCode, B = a.ReceiptId } equals new { A = b.WhCode, B = b.ReceiptId }
                      into b_join
                      from dd in b_join.DefaultIfEmpty()
                      join c in idal.IFlowHeadDAL.SelectAll() on dd.ProcessId equals c.Id
                      into c_join
                      from ee in c_join.DefaultIfEmpty()
                      where a.HuId == HuId && a.WhCode == WhCode
                      select ee.RecStockCheckApi;

            if (sql.Count() == 1)
            {
                string res = sql.First();
                if (!string.IsNullOrEmpty(sql.First()))
                {
                    //获取类型信息
                    Type t = Type.GetType("WMS.BLL.InventoryWinceManager");
                    //构造器的参数
                    // object[] constuctParms = new object[] { "timmy" };
                    //根据类型创建对象
                    // object dObj = Activator.CreateInstance(t, constuctParms);

                    object dObj = Activator.CreateInstance(t);
                    //获取方法的信息
                    MethodInfo method = t.GetMethod(res);
                    //调用方法的一些标志位，这里的含义是Public并且是实例方法，这也是默认的值
                    BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
                    //GetValue方法的参数
                    object[] parameters = new object[] { Location, HuId, WhCode };
                    //调用方法，用一个object接收返回值
                    object returnValue = method.Invoke(dObj, flag, Type.DefaultBinder, parameters, null);
                    return returnValue.ToString();


                }
                else
                    return "Y";

            }
            else
                return "上架验证失败!";
        }
        /// <summary>
        /// 亚马逊业务上架规则,验证扫描,Attribute1 为空,或者Style1为DTM1和DTM2的不需要扫描
        /// </summary>
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string AmazonRecPltStockCheck(string Location, string HuId, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();


            var sql = from a in idal.IHuDetailDAL.SelectAll()
                      join b in idal.IHuMasterDAL.SelectAll() on new { a.WhCode, a.HuId } equals new { b.WhCode, b.HuId }
                      //  join b in idal.ISerialNumberInDAL.SelectAll() on new { A = a.WhCode, B = a.HuId,C=a.ReceiptId,D=(int?)a.ItemId } equals new { A = b.WhCode, B = b.HuId,C=b.ReceiptId,D=b.ItemId }
                      join c in idal.IItemMasterDAL.SelectAll() on a.ItemId equals c.Id
                      where a.HuId == HuId && a.WhCode == WhCode && (c.Style1 != "DTM1" && c.Style1 != "DTM2" && c.Style1 != "") && (b.HoldId == 0 || b.HoldId == null)
                      select new { a.ReceiptId, a.ItemId, a.Qty };

            if (sql.Count() > 0)
            {
                string ReceiptId = sql.First().ReceiptId;
                //库存数量
                int recQty = sql.Sum(u => u.Qty);
                //扫描数量,可能会大于
                List<int?> itemStr = sql.Select(u => (int?)u.ItemId).ToList();

                int scanQty = idal.ISerialNumberInDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.HuId == HuId && u.WhCode == WhCode
                                                                ).Where(u => itemStr.Contains(u.ItemId)).Count();

                if (recQty > scanQty)
                    return "需要:" + recQty + " 实际扫描:" + scanQty + " 不允许上架!";
                else
                    return "Y";

            }
            else
                return "Y";
        }


        /// <summary>
        /// TarGet业务上架规则,验证质检货物即Style2 为Y时候 只能GRB，GRC，GRD这三根通道
        /// </summary>
        /// 
        /// <param name="Location"></param> 
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string TarGetRecPltStockCheck(string Location, string HuId, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();


            var sql = from a in idal.IHuDetailDAL.SelectAll()
                      join c in idal.IItemMasterDAL.SelectAll() on a.ItemId equals c.Id
                      where a.HuId == HuId && a.WhCode == WhCode && c.Style2 == "Y"
                      select a.ItemId;

            if (sql.Count() > 0)
            {


                if (Location.StartsWith("GRB") || Location.StartsWith("GRC") || Location.StartsWith("GRD"))
                    return "Y";
                else
                    return "库位:" + Location + " 错误,质检货物只能上架到GRB,GRC,GRD的库位!";

            }
            else
                return "Y";
        }


        /// <summary>
        /// 库位最大上架托盘数
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string StockMaxPltCheck(string Location, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            var sql = from a in (from a in idal.IHuMasterDAL.SelectAll()
                                 join b in idal.IWhLocationDAL.SelectAll() on new { A = a.WhCode, B = a.Location } equals new { A = b.WhCode, B = b.LocationId }
                                 where a.WhCode == WhCode && a.Location == Location && b.LocationTypeId == 1 && b.MaxPltQty > 0
                                 select new { a.Location, b.MaxPltQty })
                      group a by new { a.Location, a.MaxPltQty } into g
                      where g.Count() >= g.Min(u => u.MaxPltQty)
                      select g.Key.Location;

            if (sql.Count() > 0)
                return "库位:" + Location + "已有超过最大托盘数!";

            else
                return "Y";
        }


        #endregion

        #region 2.0库存管理


        public string StockMove(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            RecHelper recHelper = new RecHelper();
            InventoryHelper inventoryHelper = new InventoryHelper();

            //是否采用无托盘管理
            if (inventoryHelper.CheckWhCodeIsNoHuIdFlag(WhCode))
            {
                string result = StockMove(WhCode, Location, DestLoc, HuId, User, 1);
                return result;
            }
            else
            {
                if (Location == DestLoc)
                    return "目标库位与当前库位相同!";
                //检测是否有该托盘  
                if (!recHelper.IfPlt(WhCode, HuId))
                    return "无此托盘!";
                //检测托盘当前库位是否与移库的当前库位相同
                string pltLocation = inventoryHelper.GetPltLocation(WhCode, HuId);
                if (pltLocation != Location)
                    return HuId + "已经在" + pltLocation;
                //验证当前门区是否正确
                if (!inventoryHelper.IfLocation(WhCode, Location)) return Location + "不存在或已经冻结";
                //当前门区有无库存
                if (!recHelper.IfPltHaveStock(WhCode, HuId))
                    return "当前门区无此托盘!";
                //验证目标门区是否正确
                if (!inventoryHelper.IfLocation(WhCode, DestLoc)) return DestLoc + "不存在或已经冻结";
                //验证托盘是否有锁定数量
                if (IfHuDetailLocked(WhCode, HuId))
                    return HuId + "有备货锁定数量!";
                //验证是不是冻结托盘上架TCR库位,正常托盘上正常库位
                if (!CheckStockLocation(WhCode, HuId, DestLoc))
                    return "当前门区与托盘状态不符!";
                //验证是不是捡货区库位
                if (!CheckSupplementLocation(WhCode, Location, DestLoc))
                    return "捡货区库位不允许移库!";

                //if (!recHelper.CheckLocationTypes(WhCode, Location, DestLoc))
                //    return "起始库位类型不同不允许移库!";

                if (StockMaxPltCheck(DestLoc, WhCode) != "Y")
                    return "库位:" + DestLoc + "已有超过最大托盘数!";

                InVentoryManager inVentoryManager = new InVentoryManager();
                HuDetailResult huMaster = new HuDetailResult();
                huMaster.Location = DestLoc;
                huMaster.WhCode = WhCode;
                huMaster.UserName = User;
                huMaster.HuId = HuId;
                if (inVentoryManager.HuMasterEdit(huMaster))
                    return "Y";
                else
                    return "数据更新错误!";
            }
        }

        public string StockMove(string WhCode, string Location, string DestLoc, string HuId, string User, int type)
        {
            RecHelper recHelper = new RecHelper();
            InventoryHelper inventoryHelper = new InventoryHelper();
            if (Location == DestLoc)
                return "目标库位与当前库位相同!";
            //检测是否有该托盘  
            if (!recHelper.IfPlt(WhCode, HuId))
                return "无此托盘!";
            //检测托盘当前库位是否与移库的当前库位相同
            string pltLocation = inventoryHelper.GetPltLocation(WhCode, HuId);
            if (pltLocation != Location)
                return HuId + "已经在" + pltLocation;
            //验证当前门区是否正确
            if (!inventoryHelper.IfLocation(WhCode, Location)) return Location + "不存在或已经冻结";
            //当前门区有无库存
            if (!recHelper.IfPltHaveStock(WhCode, HuId))
                return "当前门区无此托盘!";
            //验证目标门区是否正确
            if (!inventoryHelper.IfLocation(WhCode, DestLoc)) return DestLoc + "不存在或已经冻结";
            //验证托盘是否有锁定数量
            if (IfHuDetailLocked(WhCode, HuId))
                return HuId + "有备货锁定数量!";
            //验证是不是冻结托盘上架TCR库位,正常托盘上正常库位
            if (!CheckStockLocation(WhCode, HuId, DestLoc))
                return "当前门区与托盘状态不符!";
            //验证是不是捡货区库位
            if (!CheckSupplementLocation(WhCode, Location, DestLoc))
                return "捡货区库位不允许移库!";
            //if (!recHelper.CheckLocationTypes(WhCode, Location, DestLoc))
            //    return "起始库位类型不同不允许移库!";
            if (StockMaxPltCheck(DestLoc, WhCode) != "Y")
                return "库位:" + DestLoc + "已有超过最大托盘数!";

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    if (idal.IPallateDAL.SelectBy(u => u.WhCode == WhCode && u.HuId == DestLoc).Count == 0)
                    {
                        Pallate pallate = new Pallate();
                        pallate.WhCode = WhCode;
                        pallate.HuId = DestLoc;
                        pallate.TypeId = 1;
                        pallate.Status = "U";
                        idal.IPallateDAL.Add(pallate);
                    }

                    //验证HuMaster 
                    HuMaster huMaster = new HuMaster();
                    List<HuMaster> GethuMasterList = idal.IHuMasterDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                    if (GethuMasterList.Count == 0)
                    {
                        //如果库位托盘不存在，直接修改原托盘为库位托盘
                        huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).First();
                        huMaster.HuId = DestLoc;
                        huMaster.Location = DestLoc;
                        huMaster.UpdateUser = User;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "HuId", "Location", "UpdateUser", "UpdateDate" });
                    }
                    else
                    {
                        //如果库位托盘存在，删除原托盘
                        idal.IHuMasterDAL.DeleteBy(u => u.WhCode == WhCode && u.HuId == HuId);
                        huMaster = GethuMasterList.First();
                    }

                    //得到托盘库存
                    List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);

                    foreach (var huDetail in huDetailList)
                    {
                        //得到原始数据 进行日志添加
                        TranLog tranLog = new TranLog();
                        tranLog.TranType = "109";
                        tranLog.Description = "移库操作";
                        tranLog.TranDate = DateTime.Now;
                        tranLog.TranUser = User;
                        tranLog.WhCode = WhCode;
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
                        tranLog.Location = Location;
                        tranLog.Location2 = DestLoc;
                        tranLog.HoldId = huMaster.HoldId;
                        tranLog.HoldReason = huMaster.HoldReason;
                        idal.ITranLogDAL.Add(tranLog);
                    }

                    //库位变更时间
                    InVentoryManager inVentoryManager = new InVentoryManager();
                    inVentoryManager.WhLocationEditChangeTime(WhCode, DestLoc);

                    //需修改托盘的List
                    List<HuDetail> EditList = new List<HuDetail>();
                    //需删除托盘的List
                    List<HuDetail> DelList = new List<HuDetail>();
                    //得到库位托盘库存
                    List<HuDetail> GetHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                    if (GetHuDetailList.Count == 0)
                    {
                        //如果以库位为托盘的库存不存在
                        //直接把原托盘变更为库位托盘
                        foreach (var huDetail in huDetailList)
                        {
                            huDetail.HuId = DestLoc;
                            huDetail.UpdateUser = User;
                            huDetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                        }
                    }
                    else
                    {
                        //以库位为托盘的库存存在
                        //证明 库位托盘有货 需要合并库存
                        foreach (var item in huDetailList)
                        {
                            if (GetHuDetailList.Where(u => u.ClientId == item.ClientId && (u.SoNumber ?? "") == (item.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (item.CustomerPoNumber ?? "") && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.ReceiptId ?? "") == (item.ReceiptId ?? "") && (u.Length ?? 0) == (item.Length ?? 0) && (u.Width ?? 0) == (item.Width ?? 0) && (u.Height ?? 0) == (item.Height ?? 0) && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).Count() == 0)
                            {
                                //如果库位托盘不存在该款号，直接修改原托盘库存信息为库位托盘
                                //原托盘明细修改为库位托盘明细： HuId变更为Location
                                EditList.Add(item);
                            }
                            else
                            {
                                //如果库位托盘存在该款号，需增加库位托盘数量后删除原托盘明细
                                //库位托盘数量++
                                //原托盘明细删除
                                DelList.Add(item);
                            }
                        }

                        foreach (var huDetail in EditList)
                        {
                            huDetail.HuId = DestLoc;
                            huDetail.UpdateUser = User;
                            huDetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                        }

                        List<HuDetail> checkList = new List<HuDetail>();
                        foreach (var item in DelList)
                        {
                            HuDetail getHuDetail = GetHuDetailList.Where(u => u.ClientId == item.ClientId && (u.SoNumber ?? "") == (item.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (item.CustomerPoNumber ?? "") && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.ReceiptId ?? "") == (item.ReceiptId ?? "") && (u.Length ?? 0) == (item.Length ?? 0) && (u.Width ?? 0) == (item.Width ?? 0) && (u.Height ?? 0) == (item.Height ?? 0) && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).First();

                            if (checkList.Where(u => u.Id == getHuDetail.Id).Count() == 0)
                            {
                                getHuDetail.Qty += item.Qty;
                                getHuDetail.UpdateUser = User;
                                getHuDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(getHuDetail, u => u.Id == getHuDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                checkList.Add(getHuDetail);
                            }
                            else
                            {
                                HuDetail oldHudetail = checkList.Where(u => u.Id == getHuDetail.Id).First();
                                checkList.Remove(oldHudetail);

                                HuDetail newHudetail = oldHudetail;
                                newHudetail.Qty = newHudetail.Qty + item.Qty;
                                newHudetail.UpdateUser = User;
                                newHudetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(newHudetail, u => u.Id == newHudetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                checkList.Add(newHudetail);
                            }

                            idal.IHuDetailDAL.DeleteBy(u => u.Id == item.Id);
                        }
                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "操作异常请重试！";
                }
            }

        }

        /// <summary>
        /// 检测这个托盘是否在这个库位中有库存
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public bool IfStockInLoc(string WhCode, string Location, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            return idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode && u.Location == Location).Select(u => u.Id).Count() > 0;

        }


        /// <summary>
        /// 获取托盘信息-用于锁定
        /// </summary>
        public HuInfo GetPlt(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            HuInfo hi = new HuInfo();

            List<HuMaster> Lh = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            List<HuDetail> Ld = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);

            if (Lh.Count == 1 && Ld.Count>0)
            {
                hi.Id = Lh.First().Id;
                hi.HuId= Lh.First().HuId;
                hi.WhCode= Lh.First().WhCode;
                hi.Status= Lh.First().Status;
                hi.Location= Lh.First().Location;
                hi.HoldReason= Lh.First().HoldReason;
                hi.ClientId = Ld.First().ClientId;
                hi.ClientCode = Ld.First().ClientCode;
                return hi;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 获取托盘的客户ID
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public int? GetPltClient(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            IEnumerable<int?> listHuDetail = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).Select(u => u.ClientId);
            return listHuDetail.Count() > 0 ? listHuDetail.First() : 0;

        }
        /// <summary>
        /// 托盘是否锁定
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public string IfHuIdLocked(string WhCode, string HuId)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            return inventoryHelper.IfHuIdLocked(WhCode, HuId);
        }

        /// <summary>
        /// 托盘明细是否有释放
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public bool IfHuDetailLocked(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            IEnumerable<HuDetail> listHuDetail = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            if (listHuDetail.Count() > 0)
                return listHuDetail.Sum(u => u.PlanQty ?? 0) > 0;
            else
                return false;

        }

        /// <summary>
        /// 检测托盘是否有货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public bool IfPltHaveStock(string WhCode, string HuId)
        {

            InventoryHelper inventoryHelper = new InventoryHelper();
            return inventoryHelper.IfPltHaveStock(WhCode, HuId);
        }

        public string GetPltLocation(string WhCode, string HuId)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            return inventoryHelper.GetPltLocation(WhCode, HuId);
        }

        public HuMasterResult GetPltLWH(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      where a.HuId == HuId && a.WhCode == WhCode
                      select new HuMasterResult { HuId=a.HuId,WhCode=a.WhCode, HuLength = a.HuLength*100, HuWidth=a.HuWidth*100, HuHeight=a.HuHeight*100, HuWeight=a.HuWeight };

            if (sql.Count() == 1)
            {
              
                    return sql.First();
            }
            else
                return null;
        }
        public string SetPltLWH(HuMasterResult  huMasterResult, string User)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();

            IEnumerable<HuMaster> listHuMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == huMasterResult.HuId && u.WhCode == huMasterResult.WhCode);

            if (listHuMaster.Count() != 1)
                return "托盘库存异常!";
   
            HuMaster huMaster = new HuMaster();
            huMaster.HuLength = huMasterResult.HuLength;
            huMaster.HuWidth = huMasterResult.HuWidth;
            huMaster.HuHeight = huMasterResult.HuHeight;
            huMaster.UpdateDate = DateTime.Now;
            huMaster.UpdateUser = User;
            idal.IHuMasterDAL.UpdateBy(huMaster, u => u.WhCode == huMasterResult.WhCode&& u.HuId== huMasterResult.HuId, new string[] { "HuLength", "HuWidth", "HuHeight", "UpdateUser", "UpdateDate" });
            //得到托盘库存
            List<HuMaster> huDetailList = idal.IHuMasterDAL.SelectBy(u => u.HuId == huMasterResult.HuId && u.WhCode == huMasterResult.WhCode);

            foreach (var huDetail in huDetailList)
            {
                //得到原始数据 进行日志添加
                TranLog tranLog = new TranLog();
                tranLog.TranType = "91";
                tranLog.Description = "托盘长款高修改";
                tranLog.TranDate = DateTime.Now;
                tranLog.TranUser = User;
                tranLog.WhCode = huDetail.WhCode;
                tranLog.HuId = huDetail.HuId;
                tranLog.Length = huDetail.HuLength;
                tranLog.Width = huDetail.HuWidth;
                tranLog.Height = huDetail.HuHeight;
                tranLog.Weight = huDetail.HuWeight;
                tranLog.ReceiptId = huDetail.ReceiptId;
                tranLog.ReceiptDate = huDetail.ReceiptDate;
                tranLog.Remark = "长宽高改为:"+ huMasterResult.HuLength+"*"+ huMasterResult.HuWidth + "*"+ huMasterResult.HuHeight;
                idal.ITranLogDAL.Add(tranLog);
            }
            
            idal.SaveChanges();

    
            return "Y";
        }

        public string GetRecStockSug(string WhCode, string HuId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            //   IEnumerable<int?> listHuDetail = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).Select(u => u.ClientId);
            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      join b in idal.IReceiptRegisterDAL.SelectAll() on new { A = a.WhCode, B = a.ReceiptId } equals new { A = b.WhCode, B = b.ReceiptId }
                      join c in idal.IFlowHeadDAL.SelectAll() on b.ProcessId equals c.Id
                      where a.HuId == HuId && a.WhCode == WhCode
                      select new { c.RecStockSugApi, a.Location };

            if (sql.Count() == 1)
            {
                string res = sql.First().RecStockSugApi;
                if (!string.IsNullOrEmpty(sql.First().RecStockSugApi))
                {
                    //获取类型信息
                    Type t = Type.GetType("WMS.BLL.InventoryWinceManager");
                    //构造器的参数
                    // object[] constuctParms = new object[] { "timmy" };
                    //根据类型创建对象
                    // object dObj = Activator.CreateInstance(t, constuctParms);

                    object dObj = Activator.CreateInstance(t);
                    //获取方法的信息
                    MethodInfo method = t.GetMethod(res);
                    //调用方法的一些标志位，这里的含义是Public并且是实例方法，这也是默认的值
                    BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
                    //GetValue方法的参数
                    object[] parameters = new object[] { sql.First().Location, HuId, WhCode };
                    //调用方法，用一个object接收返回值
                    object returnValue = method.Invoke(dObj, flag, Type.DefaultBinder, parameters, null);
                    return returnValue.ToString();

                }
                else
                    return "";

            }
            else
                return "";
        }
        /// <summary>
        /// 亚马逊业务上架规则,提示 Sort
        /// </summary>
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string AmazonRecStock(string Location, string HuId, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            var sql = from a in idal.IHuDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll() on a.ItemId equals b.Id
                      where a.HuId == HuId && a.WhCode == WhCode && b.Style1 != null && b.Style1 != ""
                      select b.Style1 + "-" + b.Style2 + "-" + b.Style3;

            if (sql.Distinct().Count() > 0)
            {
                if (sql.Distinct().Count() > 1)
                    return "多个Sort!请确认货物是否真确!";
                else
                    return sql.Distinct().First();

            }
            else
                return "";
        }

        /// <summary>
        /// 外高桥电商,上架库位建议
        /// </summary>
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string WGQRecStock(string Location, string HuId, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            List<int> sqlList = (from a in idal.IHuDetailDAL.SelectAll()
                                 where a.HuId == HuId && a.WhCode == WhCode
                                 select a.ItemId).ToList();

            if (sqlList.Distinct().Count() > 0)
            {

                var sqlLoc = from a in idal.IHuDetailDAL.SelectAll()
                             join c in idal.IHuMasterDAL.SelectAll() on new { A = a.WhCode, B = a.HuId } equals new { A = c.WhCode, B = c.HuId }
                             join d in idal.IWhLocationDAL.SelectAll() on new { A = c.WhCode, B = c.Location } equals new { A = d.WhCode, B = d.LocationId }
                             join e in idal.ILocationTypeDAL.SelectAll() on d.LocationTypeId equals e.Id
                             where sqlList.Contains(a.ItemId) && a.WhCode == WhCode && e.TypeName == "M"
                             select c.Location;
                int sqlLocCount = sqlLoc.Count();
                //一个库存库位
                if (sqlLocCount == 1)
                    return "建议库位:" + sqlLoc.First();
                //多个库存库位
                else if (sqlLocCount > 1)
                    return "建议库位:" + string.Join(",", sqlLoc.Distinct().ToArray());
                //没有库存,建议空库位
                else
                {
                    var sqlSug = from a in idal.IWhLocationDAL.SelectAll()
                                 join b in idal.IHuMasterDAL.SelectAll() on new { A = a.WhCode, B = a.LocationId } equals new { A = b.WhCode, B = b.Location } into b_temp
                                 from tt in b_temp.DefaultIfEmpty()
                                 where a.WhCode == WhCode && a.LocationId.StartsWith("DS") && tt.HuId == null
                                 select a;

                    return "空库位:" + sqlSug.OrderBy(u => u.LocationColumn).ThenBy(u => u.LocationRow).Take(1).First().LocationId;
                }

            }
            else
                return "";
        }
        /// <summary>
        ///TarGet,上架库位建议
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public string TarGetRecStock(string Location, string HuId, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            List<int> sqlList = (from a in idal.IHuDetailDAL.SelectAll()
                                 join b in idal.IItemMasterDAL.SelectAll() on new { A = a.ItemId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode }
                                 where a.HuId == HuId && a.WhCode == WhCode && b.Style2 == "Y"
                                 select a.ItemId).ToList();

            if (sqlList.Distinct().Count() > 0)
            {
                return "注意:质检货物只能上架到GRB,GRC,GRD的库位!";
            }
            else
                return "";
        }

        public string EasternChinaRecStock(string Location, string HuId, string WhCode)
        {

            return EasternChinaRecStockNotRec(Location, HuId, WhCode, new List<int> { 0 }).res;
        }

        public ZoneNameSearch EasternChinaRecStockNotRec(string Location, string HuId, string WhCode, List<int> zoneArry)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            int ZoneId = 0;
            //收货库位判断
            List<WhLocation> locZoneList = idal.IWhLocationDAL.SelectBy(u => u.WhCode == WhCode && u.LocationId == Location).ToList();
            if (locZoneList.Count != 1)
                return new ZoneNameSearch { res = "收货门区" + Location + "有误", ZoneId = 0 };
            else
                ZoneId = (Int32)locZoneList.First().ZoneId;

            return EasternChinaRecStockNotDoZone(ZoneId, HuId, WhCode, zoneArry);
        }


        /// <summary>
        ///华东太阳能项目,上架库位建议
        /// </summary>
        /// <param name="Location"></param>
        /// <param name="HuId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        public ZoneNameSearch EasternChinaRecStockNotDoZone(int ZoneId, string HuId, string WhCode, List<int> zoneArry)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            //收货数据判断
            List<HuDetail> sqlSku = idal.IHuDetailDAL.SelectBy(u => u.WhCode == WhCode && u.HuId == HuId).ToList();
            if (sqlSku.Count == 0)
                return new ZoneNameSearch { res = "收货数据有误!", ZoneId = 0, OnHandQty = 0, MaxPallateQty = 0 };
            int ItemId = sqlSku.First().ItemId;
            var sqlHuDetail = (from a in idal.IHuDetailDAL.SelectAll()
                               join aa in idal.IHuMasterDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = aa.HuId, B = aa.WhCode }
                               join bb in idal.IWhLocationDAL.SelectAll() on new { A = aa.Location, B = aa.WhCode } equals new { A = bb.LocationId, B = bb.WhCode }
                               join b in idal.IWhClientExtendDAL.SelectAll() on new { A = (int)a.ClientId, B = a.WhCode } equals new { A = b.ClientId, B = b.WhCode }
                               where a.ItemId == ItemId && a.WhCode == WhCode
                               select new { bb.ZoneId, bb.Location, bb.LocationTypeId, a.Qty, a.ClientCode, b.NotOnlySkuPutawayQty });

            int OnHandQtyALL = sqlHuDetail.ToList().Sum(u => u.Qty);
            int NotOnlySkuPutawayQty = 0;
            if (sqlHuDetail.Count() > 0)
            {
                if (!string.IsNullOrEmpty(sqlHuDetail.First().NotOnlySkuPutawayQty.ToString()))
                    NotOnlySkuPutawayQty = Convert.ToInt32(sqlHuDetail.First().NotOnlySkuPutawayQty.ToString());
                else
                    return new ZoneNameSearch { res = "客户" + sqlHuDetail.First().ClientCode + "未设置警戒库存值", ZoneId = 0, OnHandQty = 0, MaxPallateQty = 0 };
            }
            else
                return new ZoneNameSearch { res = "收货数据异常!", ZoneId = 0, OnHandQty = 0, MaxPallateQty = 0 };



            //int? ZoneId = locZoneList.First().ZoneId;
            //获取建议库位的基础信息
            List<ZoneNameSearch> zoneSql = (from a in idal.IZoneDAL.SelectAll()
                                            join b in idal.IZonesExtendDAL.SelectAll() on a.Id equals b.ZoneId
                                            where a.UpId == ZoneId && !zoneArry.Contains(a.Id)
                                            select new ZoneNameSearch { ZoneName = a.ZoneName, ZoneId = a.Id, ZoneOrderBy = b.ZoneOrderBy, OnlySkuFlag = b.OnlySkuFlag, MaxPallateQty = b.MaxPallateQty }).ToList();

            //此次收货+库存高于警戒值
            if (OnHandQtyALL > NotOnlySkuPutawayQty)
            {
                //进入非混合区
                zoneSql = zoneSql.Where(u => u.OnlySkuFlag == 0).ToList();
                ////有效库存
                var sqlOnHand = (from a in sqlHuDetail
                                 where a.LocationTypeId == 1
                                 group a by new { a.ZoneId } into g
                                 select new { g.Key.ZoneId, SumQty = g.Sum(u => u.Qty) }).ToList();

                List<ZoneNameSearch> sugSql = (from a in zoneSql
                                               join b in sqlOnHand on a.ZoneId equals b.ZoneId
                                               where a.MaxPallateQty > b.SumQty
                                               select new ZoneNameSearch { ZoneId = a.ZoneId, OnHandQty = b.SumQty, MaxPallateQty = a.MaxPallateQty, ZoneName = a.ZoneName, ZoneOrderBy = a.ZoneOrderBy, OnlySkuFlag = a.OnlySkuFlag }).OrderBy(u => u.OnlySkuFlag).ThenByDescending(u => u.SumQty).ThenBy(u => u.ZoneOrderBy).ToList();

                //没有库存
                if (sugSql.Count() == 0)
                {
                    //全库存,包括收货区的
                    var sqlOnHandAll = (from a in (
                                                   (from a in idal.IHuMasterDAL.SelectAll()
                                                    join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                                                    join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                                                    where a.WhCode == WhCode && c.ZoneId != null
                                                    select new { c.ZoneId, b.Qty })
                                         )
                                        group a by new { a.ZoneId } into g
                                        select new ZoneNameSearch { ZoneId = g.Key.ZoneId, SumQty = (g.Sum(u => u.Qty)) }).ToList();

                    var sqlOnHandDo = (from a in
                                               (from a in zoneSql
                                                join b in sqlOnHandAll on a.ZoneId equals b.ZoneId into b_join
                                                from b in b_join.DefaultIfEmpty()
                                                select new ZoneNameSearch { ZoneId = a.ZoneId, ZoneName = a.ZoneName, ZoneOrderBy = a.ZoneOrderBy, MaxPallateQty = a.MaxPallateQty, SumQty = b?.SumQty }
                                               )
                                       group a by new { a.ZoneId, a.ZoneName, a.ZoneOrderBy, a.MaxPallateQty } into g
                                       // where g.Key.MaxPallateQty > g.Sum(u => u.SumQty) || g.Sum(u => u.SumQty) == 0
                                       //非混合区的只能进新库位
                                       where g.Sum(u => u.SumQty) == 0
                                       select new ZoneNameSearch { ZoneId = g.Key.ZoneId, ZoneName = g.Key.ZoneName, ZoneOrderBy = g.Key.ZoneOrderBy, MaxPallateQty = g.Key.MaxPallateQty, SumQty = g.Sum(u => u.SumQty) })
                                       .OrderBy(u => u.ZoneOrderBy).ToList();
                    if (sqlOnHandDo.Count() == 0)
                        return new ZoneNameSearch { res = "无空余库区", ZoneId = 0, OnHandQty = 0, MaxPallateQty = 0 };
                    else
                        return new ZoneNameSearch { res = "建议上架:" + sqlOnHandDo.First().ZoneName, ZoneName = sqlOnHandDo.First().ZoneName, ZoneId = sqlOnHandDo.First().ZoneId, OnHandQty = sqlOnHandDo.First().SumQty, MaxPallateQty = sqlOnHandDo.First().MaxPallateQty };
                }
                else
                    return new ZoneNameSearch { res = "建议上架:" + sugSql.First().ZoneName, ZoneName = sugSql.First().ZoneName, ZoneId = sugSql.First().ZoneId, OnHandQty = sugSql.First().OnHandQty, MaxPallateQty = sugSql.First().MaxPallateQty };

            }
            else
            {

                //进入混合区OnlySkuFlag=1
                zoneSql = zoneSql.Where(u => u.OnlySkuFlag == 1).ToList();
                ////该款号有效库存所在的库区List
                List<int?> sqlOnHandZoneId = (from a in sqlHuDetail
                                              where a.LocationTypeId == 1
                                              group a by new { a.ZoneId } into g
                                              select g.Key.ZoneId).ToList();
                //查询所在库区的所有库存,包含到其他款号的,单一款号区域不换了
                var sqlOnHand = (from a in (
                                          from a in idal.IHuDetailDAL.SelectAll()
                                          join aa in idal.IHuMasterDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = aa.HuId, B = aa.WhCode }
                                          join bb in idal.IWhLocationDAL.SelectAll() on new { A = aa.Location, B = aa.WhCode } equals new { A = bb.LocationId, B = bb.WhCode }
                                          where a.WhCode == WhCode && sqlOnHandZoneId.Contains(bb.ZoneId)
                                          select new { bb.ZoneId, a.Qty })
                                 group a by new { a.ZoneId } into g
                                 select new ZoneNameSearch { ZoneId = g.Key.ZoneId, SumQty = (g.Sum(u => u.Qty)) }).ToList();




                List<ZoneNameSearch> sugSql = (from a in zoneSql
                                               join b in sqlOnHand on a.ZoneId equals b.ZoneId
                                               where a.MaxPallateQty > b.SumQty
                                               select new ZoneNameSearch { ZoneName = a.ZoneName, ZoneOrderBy = a.ZoneOrderBy, OnlySkuFlag = a.OnlySkuFlag, OnHandQty = b.SumQty, MaxPallateQty = a.MaxPallateQty }).OrderBy(u => u.OnlySkuFlag).ThenByDescending(u => u.SumQty).ThenBy(u => u.ZoneOrderBy).ToList();

                //没有库存
                if (sugSql.Count() == 0)
                {

                    var sqlOnHandAll = (from a in (
                                                   (from a in idal.IHuMasterDAL.SelectAll()
                                                    join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                                                    join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                                                    where a.WhCode == WhCode && c.ZoneId != null
                                                    select new { c.ZoneId, b.Qty })
                                         )
                                        group a by new { a.ZoneId } into g
                                        select new ZoneNameSearch { ZoneId = g.Key.ZoneId, SumQty = (g.Sum(u => u.Qty)) }).ToList();

                    var sqlOnHandDo = (from a in
                                               (from a in zoneSql
                                                join b in sqlOnHandAll on a.ZoneId equals b.ZoneId into b_join
                                                from b in b_join.DefaultIfEmpty()
                                                    //b?.SumQty 为空的时候不能直接求和
                                                select new ZoneNameSearch { ZoneId = a.ZoneId, ZoneName = a.ZoneName, ZoneOrderBy = a.ZoneOrderBy, MaxPallateQty = a.MaxPallateQty, SumQty = b?.SumQty }
                                               )
                                       group a by new { a.ZoneId, a.ZoneName, a.ZoneOrderBy, a.MaxPallateQty } into g
                                       // 无库存的情况下,混合区的优先新库位(排序优先SumQty),没有的话随便进老库位MaxPallateQty>SumQty
                                       where g.Key.MaxPallateQty > g.Sum(u => u.SumQty) || g.Sum(u => u.SumQty) == 0
                                       select new ZoneNameSearch { ZoneId = g.Key.ZoneId, MaxPallateQty = g.Key.MaxPallateQty, ZoneName = g.Key.ZoneName, ZoneOrderBy = g.Key.ZoneOrderBy, SumQty = g.Sum(u => u.SumQty) });
                    var aa = sqlOnHandDo.OrderBy(u => u.SumQty).ThenBy(u => u.ZoneOrderBy).ToList();

                    if (sqlOnHandDo.ToList().Count() == 0)
                        return new ZoneNameSearch { res = "无空余库区", ZoneId = 0, OnHandQty = 0, MaxPallateQty = 0 };
                    else
                        return new ZoneNameSearch { res = "建议上架:" + aa.First().ZoneName, ZoneName = aa.First().ZoneName, ZoneId = aa.First().ZoneId, OnHandQty = 0, MaxPallateQty = aa.First().MaxPallateQty };
                }
                else
                    return new ZoneNameSearch { res = "建议上架:" + sugSql.First().ZoneName, ZoneName = sugSql.First().ZoneName, ZoneId = sugSql.First().ZoneId, OnHandQty = sugSql.First().OnHandQty, MaxPallateQty = sugSql.First().MaxPallateQty };

            }

        }

        /// <summary>
        /// 验证当前托盘是否可以移货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        public string PltItemEnableMove(string WhCode, string HuId)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            //检测是否有该托盘  
            if (!inventoryHelper.IfPlt(WhCode, HuId))
                return "无此托盘!";
            //检测该托盘是否有货
            if (!inventoryHelper.IfPltHaveStock(WhCode, HuId))
                return HuId + "无库存!";
            //验证托盘是否有锁定数量
            if (IfHuDetailLocked(WhCode, HuId))
                return HuId + "有备货锁定数量!";
            //验证托盘库位是否存在
            string pltLocation = inventoryHelper.GetPltLocation(WhCode, HuId);
            if (pltLocation == null)
                return HuId + "库位不存在!";
            //验证托盘库位是否被冻结
            string CheckLocationIdRes = inventoryHelper.CheckLocationId(pltLocation, WhCode);
            if (CheckLocationIdRes != "Y")
                return pltLocation + CheckLocationIdRes;
            return "Y";
        }
        /// <summary>
        /// 验证目标托盘是否可以接受移货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="DestHuId">目标托盘</param>
        /// <returns></returns>
        public string DestPltItemEnableMove(string WhCode, string DestHuId, string pltLocation)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            //检测是否有该托盘  
            if (!inventoryHelper.IfPlt(WhCode, DestHuId))
                return DestHuId + "无此托盘!";
            //验证目标托盘是否有锁定数量
            if (IfHuDetailLocked(WhCode, DestHuId))
                return DestHuId + "有备货锁定数量!";
            //验证目标托盘库位是否存在,如果不存在则取当前托盘的库位
            if (string.IsNullOrEmpty(pltLocation))
                pltLocation = inventoryHelper.GetPltLocation(WhCode, DestHuId);
            if (string.IsNullOrEmpty(pltLocation))
                return "N$" + DestHuId + "不在库位上!";
            //if (pltLocation == null)
            //    pltLocation= inventoryHelper.GetPltLocation(WhCode, HuId);
            //验证托盘库位是否被冻结
            string CheckLocationIdRes = inventoryHelper.CheckLocationId(pltLocation, WhCode);
            if (CheckLocationIdRes != "Y")
                return pltLocation + CheckLocationIdRes;
            return "Y";
        }

        public List<HuDetailResult> PltItemMoveList(string WhCode, string HuId)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            List<HuDetailResult> huDetailResultList = new List<HuDetailResult>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (HuDetail item in huDetailList)
                {
                    HuDetailResult huDetailResult = new HuDetailResult();
                    huDetailResult.Id = item.Id;
                    huDetailResult.HuId = item.HuId;
                    huDetailResult.AltItemNumber = item.AltItemNumber;
                    huDetailResult.UnitName = item.UnitName;
                    huDetailResult.Qty = item.Qty;
                    huDetailResult.Length = item.Length;
                    huDetailResult.Width = item.Width;
                    huDetailResult.Height = item.Height;
                    huDetailResult.Weight = item.Weight;
                    huDetailResult.LotNumber1 = item.LotNumber1;
                    huDetailResult.LotNumber2 = item.LotNumber2;
                    huDetailResult.LotDate = item.LotDate;
                    huDetailResult.SoNumber = item.SoNumber;
                    huDetailResult.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailResult.ClientCode = item.ClientCode;
                    huDetailResultList.Add(huDetailResult);
                }
                return huDetailResultList;
            }
        }

        public List<HuDetailResult> PltItemMoveList(string WhCode, string HuId, string SKU)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            if (!string.IsNullOrEmpty(SKU))
                huDetailList = huDetailList.Where(u => u.AltItemNumber.Contains(SKU)).ToList();
            List<HuDetailResult> huDetailResultList = new List<HuDetailResult>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (HuDetail item in huDetailList)
                {
                    HuDetailResult huDetailResult = new HuDetailResult();
                    huDetailResult.Id = item.Id;
                    huDetailResult.HuId = item.HuId;
                    huDetailResult.AltItemNumber = item.AltItemNumber;
                    huDetailResult.UnitName = item.UnitName;
                    huDetailResult.Qty = item.Qty;
                    huDetailResult.Length = item.Length;
                    huDetailResult.Width = item.Width;
                    huDetailResult.Height = item.Height;
                    huDetailResult.Weight = item.Weight;
                    huDetailResult.LotNumber1 = item.LotNumber1;
                    huDetailResult.LotNumber2 = item.LotNumber2;
                    huDetailResult.LotDate = item.LotDate;
                    huDetailResult.SoNumber = item.SoNumber;
                    huDetailResult.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailResult.ClientCode = item.ClientCode;
                    huDetailResultList.Add(huDetailResult);
                }
                return huDetailResultList;
            }
        }
        private string PltItemMoveActionCheck(string WhCode, string HuId, string DestHuId, string DestLoc, int HuDetailId)
        {
            InventoryHelper inventoryHelper = new InventoryHelper();
            //检测当前托盘是否可以移货
            string res = PltItemEnableMove(WhCode, HuId);
            if (res != "Y")
                return res;
            //验证目标托盘是否可以接受移货
            res = DestPltItemEnableMove(WhCode, DestHuId, DestLoc);
            if (res != "Y")
                return res;
            if (DestHuId == HuId)
                return "目标托盘不能与当先托盘相同!";
            return res;
        }



        public string PltItemMoveAction(string WhCode, string HuId, string DestHuId, string DestLoc, int HuDetailId, int MoveQty, string User)
        {
            string res = PltItemMoveActionCheck(WhCode, HuId, DestHuId, DestLoc, HuDetailId);
            if (res != "Y")
                return res;
            else
                return PltItemMoveActionHu(WhCode, HuId, DestHuId, DestLoc, HuDetailId, MoveQty, User);

        }
        public string PltItemMoveActionHu(string WhCode, string HuId, string DestHuId, string DestLoc, int HuDetailId, int MoveQty, string User)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            //当前托盘表头情况,出现移货TransactionFlag 货物异动FLAG启用 标记为1
            HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).First();
            huMaster.UpdateDate = DateTime.Now;
            huMaster.UpdateUser = User;
            huMaster.TransactionFlag = 1;

            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            List<HuMaster> destHuMasterList = idal.IHuMasterDAL.SelectBy(u => u.HuId == DestHuId && u.WhCode == WhCode);
            List<HuDetail> destHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == DestHuId && u.WhCode == WhCode);

            if (huDetailList.Where(u => u.Id == HuDetailId).Count() == 0)
                return HuId + "移货托盘明细不存在";


            //当前托盘明细情况
            HuDetail huDetail = huDetailList.Where(u => u.Id == HuDetailId).First();
            huDetail.UpdateUser = User;
            huDetail.UpdateDate = DateTime.Now;

            string oldHuId = huDetail.HuId;
            //当前托盘数量
            int huDetailQty = huDetailList.Where(u => u.Id == HuDetailId).Sum(u => u.Qty);

            //目标托盘存在
            if (destHuDetailList.Count() != 0)
            {
                //目标托盘情况
                HuMaster destHuMaster = destHuMasterList.First();
                destHuMaster.UpdateUser = User;
                destHuMaster.UpdateDate = DateTime.Now;

                if (huDetail.ClientId != destHuDetailList.First().ClientId)
                    return "两托盘客户不同,移货失败!";

                //判断目标托盘与当前托盘是否有相同规则的ITEM
                HuDetail destHuDetail = destHuDetailList.Find(delegate (HuDetail HuDetail)
                {
                    return HuDetail.WhCode == huDetail.WhCode && HuDetail.ClientId == huDetail.ClientId && HuDetail.SoNumber == huDetail.SoNumber && HuDetail.ReceiptId == huDetail.ReceiptId
                            && HuDetail.CustomerPoNumber == huDetail.CustomerPoNumber && HuDetail.ItemId == huDetail.ItemId && HuDetail.UnitId == huDetail.UnitId && HuDetail.UnitName == huDetail.UnitName
                            && HuDetail.Length == huDetail.Length && HuDetail.Width == huDetail.Width && HuDetail.Height == huDetail.Height
                            && HuDetail.Weight == huDetail.Weight && HuDetail.LotNumber1 == huDetail.LotNumber1 && HuDetail.LotNumber2 == huDetail.LotNumber2
                            && HuDetail.LotDate == huDetail.LotDate;
                });

                //检测目标huDetail是否要合并明细
                if (destHuDetail != null)
                {

                    #region 1.0当前托盘huDetail处理
                    //如果数量全部移走,当前托盘删除
                    if (huDetailQty == MoveQty)
                        idal.IHuDetailDAL.Delete(huDetail);
                    else if (huDetailQty > MoveQty)
                    {
                        int oldQty = huDetail.Qty;
                        huDetail.Qty = oldQty - MoveQty;
                        //部分移走,修改数量
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    }
                    else
                    {
                        return "移动数量大于库存数量";
                    }
                    #endregion
                    #region 1.1目标托盘huDetail处理
                    int destOldQty = destHuDetail.Qty;
                    destHuDetail.Qty = destOldQty + MoveQty;
                    destHuDetail.UpdateDate = DateTime.Now;
                    destHuDetail.UpdateUser = User;
                    //目标托盘数量更新
                    idal.IHuDetailDAL.UpdateBy(destHuDetail, u => u.Id == destHuDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    #endregion
                    #region  1.2添加目标托盘的TranLog
                    idal.ITranLogDAL.Add(tranLogAdd(destHuDetail, destHuMaster, MoveQty, oldHuId, DestHuId, DestLoc, User));
                    #endregion

                }

                //不需要合并的时候
                else
                {
                    #region 2.0托盘huDetail处理
                    //如果数量全部移走,当前托盘变更为新托盘明细
                    if (huDetailQty == MoveQty)
                    {
                        huDetail.HuId = DestHuId;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                    }
                    //更新数量
                    else if (huDetailQty > MoveQty)
                    {
                        //当前托盘减少数量
                        int oldQty = huDetail.Qty;
                        huDetail.Qty = oldQty - MoveQty;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                        //目标托盘添加明细

                        HuDetail huDetailAdd = new HuDetail();
                        huDetailAdd = SetHuDetailValue(huDetail, huDetailAdd);
                        huDetailAdd.Qty = MoveQty;
                        huDetailAdd.HuId = DestHuId;
                        huDetailAdd.UpdateDate = DateTime.Now;
                        huDetailAdd.UpdateUser = User;
                        idal.IHuDetailDAL.Add(huDetailAdd);
                    }
                    else
                    {
                        return "移动数量大于库存数量";
                    }
                    #endregion
                }


                #region 3.0当前托盘HuMaster处理情况
                //如果没有其他明细,并且数量全部移走了
                if (huDetailList.Where(u => u.Id != HuDetailId).Count() == 0 && huDetailQty == MoveQty)
                    //删除当前托盘HuMaster
                    idal.IHuMasterDAL.DeleteBy(u => u.HuId == HuId && u.WhCode == WhCode);
                else
                    //还有明细的话,只更新当前托盘HuMaster时间和更新人
                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "UpdateUser", "UpdateDate" });
                #endregion

                #region 4.0目标托盘HuMaster处理情况 
                //更新目标托盘HuMaster时间和更新人
                idal.IHuMasterDAL.UpdateBy(destHuMaster, u => u.Id == destHuMaster.Id, new string[] { "UpdateUser", "UpdateDate" });
                #endregion


            }
            else
            {

                #region 6.0托盘huDetail处理
                //如果数量全部移走,当前托盘变更为新托盘明细
                if (huDetailQty == MoveQty)
                {
                    huDetail.HuId = DestHuId;
                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                }
                //更新数量
                else if (huDetailQty > MoveQty)
                {
                    //当前托盘减少数量
                    int oldQty = huDetail.Qty;
                    huDetail.Qty = oldQty - MoveQty;
                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    //目标托盘添加明细
                    HuDetail huDetailAdd = new HuDetail();
                    huDetailAdd = SetHuDetailValue(huDetail, huDetailAdd);
                    huDetailAdd.Qty = MoveQty;
                    huDetailAdd.HuId = DestHuId;
                    huDetailAdd.UpdateDate = DateTime.Now;
                    huDetailAdd.UpdateUser = User;
                    idal.IHuDetailDAL.Add(huDetailAdd);
                }
                else
                {
                    return "移动数量大于库存数量";
                }
                #endregion


                #region 7.0当前托盘HuMaster处理情况
                //如果没有其他明细的话
                if (huDetailList.Where(u => u.Id != HuDetailId).Count() == 0 && huDetailQty == MoveQty)
                    //删除当前托盘HuMaster
                    idal.IHuMasterDAL.DeleteBy(u => u.HuId == HuId && u.WhCode == WhCode);
                else
                    //还有明细的话,只更新当前托盘HuMaster时间和更新人
                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "UpdateUser", "UpdateDate" });
                #endregion

                #region 8.0目标托盘HuMaster处理情况  TransactionFlag 货物异动FLAG启用 标记为1
                HuMaster huMasterAdd = new HuMaster();
                SetHuMasterValue(huMaster, huMasterAdd);
                huMasterAdd.UpdateDate = DateTime.Now;
                huMasterAdd.UpdateUser = User;
                huMasterAdd.HuId = DestHuId;
                huMasterAdd.Location = DestLoc;
                huMasterAdd.TransactionFlag = 1;
                idal.IHuMasterDAL.Add(huMasterAdd);
                #endregion


            }

            #region 9.0当前托盘TranLog
            idal.ITranLogDAL.Add(tranLogAdd(huDetail, huMaster, MoveQty, oldHuId, DestHuId, DestLoc, User));
            #endregion
            idal.SaveChanges();
            return "Y";
        }

        public string PltItemMoveActionDoPlanQty(string WhCode, int HuDetailId, int MoveQty, string User)
        {
            string res = "Y";
            IDAL.IDALSession idal = BLLHelper.GetDal();
            //当前托盘
            int oldPlanQty;
            List<HuDetail> HuDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == HuDetailId);
            if (HuDetail.Count == 1)
            {
                oldPlanQty = Convert.ToInt32(HuDetail.First().PlanQty);
                HuDetail huDetail = new HuDetail();
                huDetail.PlanQty = oldPlanQty - MoveQty;
                //备货锁定数量减掉移库数量,新增目标托盘应该是没有备货锁定数量,老的备货数量不需要改
                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == HuDetailId, new string[] { "PlanQty" });
                idal.IHuDetailDAL.SaveChanges();
            }
            else
                //说明旧的明细数据已经删除合并到新的明细里面了,所以不需要处理PlanQty
                res = "Y";
            return res;

        }

        public TranLog tranLogAdd(HuDetail huDetail, HuMaster huMaster, int MoveQty, string HuId, string DestHuId, string DestLoc, string User)
        {


            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "115";
            tranLog.Description = "移货操作";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = User;
            tranLog.WhCode = huMaster.WhCode;
            tranLog.ClientCode = huDetail.ClientCode;
            tranLog.SoNumber = huDetail.SoNumber;
            tranLog.CustomerPoNumber = huDetail.CustomerPoNumber;
            tranLog.AltItemNumber = huDetail.AltItemNumber;
            tranLog.ItemId = huDetail.ItemId;
            tranLog.UnitID = huDetail.UnitId;
            tranLog.UnitName = huDetail.UnitName;
            tranLog.TranQty = huDetail.Qty;
            tranLog.TranQty2 = MoveQty;
            tranLog.HuId = HuId;
            tranLog.HuId2 = DestHuId;
            tranLog.Length = huDetail.Length;
            tranLog.Width = huDetail.Width;
            tranLog.Height = huDetail.Height;
            tranLog.Weight = huDetail.Weight;
            tranLog.LotNumber1 = huDetail.LotNumber1;
            tranLog.LotNumber2 = huDetail.LotNumber2;
            tranLog.LotDate = huDetail.LotDate;
            tranLog.ReceiptId = huMaster.ReceiptId;
            tranLog.Location = huMaster.Location;
            if (string.IsNullOrEmpty(DestLoc))
            {
                HuMaster getHuMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == huMaster.WhCode && u.HuId == DestHuId).First();
                tranLog.Location2 = getHuMaster.Location;
            }
            else
            {
                tranLog.Location2 = DestLoc;
            }

            tranLog.HoldId = huMaster.HoldId;
            tranLog.HoldReason = huMaster.HoldReason;
            tranLog.Remark = "托盘:" + HuId + "库位:" + huMaster.Location + "数量:" + MoveQty + "=>托盘:" + DestHuId + "库位:" + tranLog.Location2 + "数量:" + MoveQty;

            return tranLog;
        }


        public HuDetail SetHuDetailValue(HuDetail OldObj, HuDetail NewObj)
        {
            NewObj.WhCode = OldObj.WhCode;
            NewObj.HuId = OldObj.HuId;
            NewObj.ClientId = OldObj.ClientId;
            NewObj.ClientCode = OldObj.ClientCode;
            NewObj.SoNumber = OldObj.SoNumber;
            NewObj.CustomerPoNumber = OldObj.CustomerPoNumber;
            NewObj.AltItemNumber = OldObj.AltItemNumber;
            NewObj.ItemId = OldObj.ItemId;
            NewObj.UnitName = OldObj.UnitName;
            NewObj.UnitId = OldObj.UnitId;
            NewObj.ReceiptId = OldObj.ReceiptId;
            NewObj.Qty = OldObj.Qty;
            // NewObj.PlanQty = OldObj.PlanQty;
            //新增的HuDetail不应该有锁定数量,补货下架新增的detail就是这情况
            NewObj.PlanQty = 0;
            NewObj.ReceiptDate = OldObj.ReceiptDate;
            NewObj.Length = OldObj.Length;
            NewObj.Width = OldObj.Width;
            NewObj.Height = OldObj.Height;
            NewObj.Weight = OldObj.Weight;
            NewObj.LotNumber1 = OldObj.LotNumber1;
            NewObj.LotNumber2 = OldObj.LotNumber2;
            NewObj.LotDate = OldObj.LotDate;
            NewObj.CreateUser = OldObj.CreateUser;
            NewObj.CreateDate = OldObj.CreateDate;
            NewObj.UpdateDate = OldObj.UpdateDate;
            NewObj.UpdateUser = OldObj.UpdateUser;
            return NewObj;
        }
        public HuMaster SetHuMasterValue(HuMaster OldObj, HuMaster NewObj)
        {
            NewObj.WhCode = OldObj.WhCode;
            NewObj.HuId = OldObj.HuId;
            NewObj.Type = OldObj.Type;
            NewObj.Status = OldObj.Status;
            NewObj.Location = OldObj.Location;
            NewObj.ReceiptId = OldObj.ReceiptId;
            NewObj.ReceiptDate = OldObj.ReceiptDate;
            NewObj.HoldId = OldObj.HoldId;
            NewObj.HoldReason = OldObj.HoldReason;
            NewObj.LoadId = OldObj.LoadId;
            NewObj.CreateUser = OldObj.CreateUser;
            NewObj.CreateDate = OldObj.CreateDate;
            NewObj.UpdateDate = OldObj.UpdateDate;
            NewObj.UpdateUser = OldObj.UpdateUser;
            return NewObj;
        }

        public string SkuToItemId(string WhCode, string AltItemNumber)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            var ItemIds = from a in idal.IItemMasterDAL.SelectAll()
                          where a.WhCode == WhCode && (a.AltItemNumber == AltItemNumber || a.EAN == AltItemNumber)
                          select a.Id;
            if (ItemIds.Count() == 0)
                return "N$" + AltItemNumber + "款号不存";
            else
                return "Y$" + ItemIds.First().ToString();

        }

        public List<HuDetailResult> LocationHuList(string WhCode, string LocationId, string AltItemNumber)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();

            var ItemIds = (from a in idal.IItemMasterDAL.SelectAll()
                           where a.WhCode == WhCode && (a.AltItemNumber == AltItemNumber || a.EAN == AltItemNumber)
                           select a.Id).ToList();

            List<HuDetail> huDetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                           join b in idal.IHuMasterDAL.SelectAll() on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                                           where b.Location == LocationId && a.WhCode == WhCode && ItemIds.Contains(a.ItemId)
                                           select a).ToList();
            List<HuDetailResult> huDetailResultList = new List<HuDetailResult>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (HuDetail item in huDetailList)
                {
                    HuDetailResult huDetailResult = new HuDetailResult();
                    huDetailResult.Id = item.Id;
                    huDetailResult.HuId = item.HuId;
                    huDetailResult.AltItemNumber = item.AltItemNumber;
                    huDetailResult.UnitName = item.UnitName;
                    huDetailResult.Qty = item.Qty;
                    huDetailResult.Length = item.Length;
                    huDetailResult.Width = item.Width;
                    huDetailResult.Height = item.Height;
                    huDetailResult.Weight = item.Weight;
                    huDetailResult.LotNumber1 = item.LotNumber1;
                    huDetailResult.LotNumber2 = item.LotNumber2;
                    huDetailResult.LotDate = item.LotDate;
                    huDetailResult.SoNumber = item.SoNumber;
                    huDetailResult.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailResult.ClientCode = item.ClientCode;
                    huDetailResultList.Add(huDetailResult);
                }
                return huDetailResultList;
            }

        }

        public List<HuDetailResult> LocationHuList(string WhCode, string LocationId)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<HuDetail> huDetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                           join b in idal.IHuMasterDAL.SelectAll() on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                                           where b.Location == LocationId && a.WhCode == WhCode
                                           select a).ToList();
            List<HuDetailResult> huDetailResultList = new List<HuDetailResult>();
            if (huDetailList.Count() == 0)
                return null;
            else
            {
                foreach (HuDetail item in huDetailList)
                {
                    HuDetailResult huDetailResult = new HuDetailResult();
                    huDetailResult.Id = item.Id;
                    huDetailResult.HuId = item.HuId;
                    huDetailResult.AltItemNumber = item.AltItemNumber;
                    huDetailResult.UnitName = item.UnitName;
                    huDetailResult.Qty = item.Qty;
                    huDetailResult.Length = item.Length;
                    huDetailResult.Width = item.Width;
                    huDetailResult.Height = item.Height;
                    huDetailResult.Weight = item.Weight;
                    huDetailResult.LotNumber1 = item.LotNumber1;
                    huDetailResult.LotNumber2 = item.LotNumber2;
                    huDetailResult.LotDate = item.LotDate;
                    huDetailResult.SoNumber = item.SoNumber;
                    huDetailResult.CustomerPoNumber = item.CustomerPoNumber;
                    huDetailResult.ClientCode = item.ClientCode;
                    huDetailResultList.Add(huDetailResult);
                }
                return huDetailResultList;
            }
        }
        public string SNSearch(string WhCode, string SN)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<string> huList = idal.ISerialNumberInDAL.SelectBy(u => u.WhCode == WhCode && u.CartonId == SN).Select(g => g.HuId).ToList();
            if (huList.Count() > 0)
            {
                string huId = huList.First();
                string ss = (from a in idal.IHuMasterDAL.SelectAll()
                             join b in idal.IHuDetailDAL.SelectAll() on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                             where a.HuId == huId && a.WhCode == WhCode
                             select a.Location + "$" + b.SoNumber + "$" + b.CustomerPoNumber + "$" + b.AltItemNumber + "$" + b.HuId).ToList().First();

                return ss;
            }
            else
                return null;
        }
        public List<ItemZone> GetItemZone(string WhCode, string AltItemNumber)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();
            var itemZone = from a in idal.IHuMasterDAL.SelectAll()
                           join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                           join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }
                           join d in idal.IZonesExtendDAL.SelectAll() on new { A = c.ZoneId, B = c.WhCode } equals new { A = d.ZoneId, B = d.WhCode }
                           where a.WhCode == WhCode && b.AltItemNumber == AltItemNumber
                           select new { c.Location, c.ZoneId, b.Qty, d.MaxPallateQty };

            List<ItemZone> itemZoneList = (from a in itemZone
                                           group a by new { a.ZoneId, a.Location, a.MaxPallateQty } into g
                                           select new ItemZone { ZoneId = g.Key.ZoneId, ZoneName = g.Key.Location, OnhandQty = g.Sum(u => u.Qty), RemainingQty = g.Key.MaxPallateQty - g.Sum(u => u.Qty) }).ToList();

            if (itemZoneList.Count() > 0)
            {
                return itemZoneList;
            }
            else
                return null;
        }
        public List<ItemZone> GetZoneLoc(string WhCode, string ZoneName)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();
            var ZoneLoc = from a in idal.IWhLocationDAL.SelectAll()
                              // join b in idal.IZoneDAL.SelectAll() on  a.ZoneId equals b.Id 
                              // join c in idal.IZonesExtendDAL.SelectAll() on new { A = b.Id, B = b.WhCode } equals new { A = (Int32)c.ZoneId, B = c.WhCode }
                          join d in idal.IHuMasterDAL.SelectAll() on new { A = a.LocationId, B = a.WhCode } equals new { A = d.Location, B = d.WhCode }
                          into d_join
                          from dd in d_join.DefaultIfEmpty()
                          where a.WhCode == WhCode && a.Location.StartsWith(ZoneName)
                          select new { a.LocationId, a.Location, a.ZoneId, OnhandQty = (dd.HuId == null ? 0 : 1) };

            List<ItemZone> zoneLocList = (from a in ZoneLoc
                                          group a by new { a.ZoneId, a.Location, a.LocationId } into g
                                          select new ItemZone { ZoneId = g.Key.ZoneId, ZoneName = g.Key.Location, LocationId = g.Key.LocationId, OnhandQty = g.Sum(u => u.OnhandQty) }).ToList();

            if (zoneLocList.Count() > 0)
            {
                return zoneLocList;
            }
            else
                return null;
        }
        public List<ItemZone> GetItemLoc(string WhCode, string AltItemNumber)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();

            var itemLoc = from a in idal.IHuMasterDAL.SelectAll()
                          join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll() on new { A = a.Location, B = a.WhCode } equals new { A = c.LocationId, B = c.WhCode }

                          where a.WhCode == WhCode && b.AltItemNumber == AltItemNumber
                          select new { c.LocationId, b.Qty, b.PlanQty };

            List<ItemZone> itemLocList = (from a in itemLoc
                                          group a by new { a.LocationId } into g
                                          select new ItemZone { LocationId = g.Key.LocationId, OnhandQty = g.Sum(u => u.Qty), RemainingQty = g.Sum(u => u.Qty - u.PlanQty) }).ToList();

            if (itemLocList.Count() > 0)
            {
                return itemLocList;
            }
            else
                return null;
        }

        #region 补货
        public List<SupplementTaskCe> SupplementTaskCe(string WhCode, string[] NotStatusArry)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            return (from a in idal.ISupplementTaskDAL.SelectAll()
                    join b in idal.ISupplementTaskDetailDAL.SelectAll() on new { A = a.SupplementNumber, B = a.WhCode } equals new { A = b.SupplementNumber, B = b.WhCode }
                    where a.WhCode == WhCode && (a.Status == "U" || a.Status == "A") && !NotStatusArry.Contains(b.Status)//  b.Status!="C" && b.Status != "D"
                    select new SupplementTaskCe { SupplementNumber = a.SupplementNumber, UpdateUser = a.UpdateUser }).Distinct().ToList();
        }
        public List<SupplementTaskDetailCe> SupplementTaskDetailCe(string WhCode, string SupplementNumber, string SupplementGroupNumber)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();
            return (from a in idal.ISupplementTaskDetailDAL.SelectBy(u => u.WhCode == WhCode && u.SupplementNumber == SupplementNumber && (u.GroupNumber == SupplementGroupNumber || string.IsNullOrEmpty(SupplementGroupNumber))).OrderBy(u => u.LocationId)
                    select new SupplementTaskDetailCe
                    {
                        SupplementNumber = a.SupplementNumber,
                        WhCode = a.WhCode,
                        HuDetailId = a.HuDetailId,
                        HuId = a.HuId,
                        LocationId = a.LocationId,
                        PutLocationId = a.PutLocationId,
                        GroupNumber = a.GroupNumber,
                        ItemId = a.ItemId,
                        AltItemNumber = a.AltItemNumber,
                        EAN = a.EAN,
                        Qty = a.Qty,
                        LotNumber1 = a.LotNumber1,
                        LotNumber2 = a.LotNumber2,
                        LotDate = a.LotDate,
                        Status = a.Status

                    }).ToList();
        }

        public string SupplementTaskDown(string SupplementNumber, string SupplementGroupNumber, string HuId, string User, string WhCode)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();

            string res = "N";
            //验证补货任务,更新状态及最近修改人
            var supplementTaskStatusList = idal.ISupplementTaskDAL.SelectBy(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode);
            if (supplementTaskStatusList.Count == 1)
            {
                SupplementTask supplementTask = new SupplementTask();
                string supplementTaskStatus = supplementTaskStatusList.First().Status;
                if (supplementTaskStatus == "C")
                    return "补货任务号已经完成";
                else if (supplementTaskStatus == "U")
                {
                    supplementTask.Status = "A";
                    supplementTask.UpdateDate = DateTime.Now;
                    supplementTask.UpdateUser = User;
                }
                else if (supplementTaskStatus == "A")
                {
                    supplementTask.Status = "A";
                    supplementTask.UpdateDate = DateTime.Now;
                    supplementTask.UpdateUser = User;
                }
                idal.ISupplementTaskDAL.UpdateBy(supplementTask, u => u.SupplementNumber == SupplementNumber, new string[] { "Status", "UpdateUser", "UpdateDate" });
            }
            else
                return "补货任务号不存在";

            List<SupplementTaskDetail> supplementTaskDetailList = idal.ISupplementTaskDetailDAL.SelectBy(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode).ToList();
            if (supplementTaskDetailList.Count > 0)
            {
                if (supplementTaskDetailList.Where(u => u.HuId == HuId).Count() == 0)
                    return "托盘:" + HuId + "的补货任务不存在";

                List<SupplementTaskDetail> sTaskDetailList = supplementTaskDetailList.Where(u => u.HuId == HuId && u.Status == "U").ToList();
                if (sTaskDetailList.Count() == 0)
                    return "托盘:" + HuId + "的补货任务已经完成";
                else
                {

                    List<SupplementTaskDetail> sTaskDetailListLotCheck = supplementTaskDetailList.Where(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode && u.GroupNumber == SupplementGroupNumber).ToList();
                    //有相同的组号,要验证SKU的LOT不能多个
                    if (sTaskDetailListLotCheck.Count() > 0)
                    {
                        List<SupplementTaskDetail> sTaskDetailListLotNew = supplementTaskDetailList.Where(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode && u.HuId == HuId).ToList();

                        List<SupplementTaskDetail> sTDetail = new List<SupplementTaskDetail>();
                        sTDetail.AddRange(sTaskDetailListLotCheck);
                        sTDetail.AddRange(sTaskDetailListLotNew);
                        //list.GroupBy(p => p.Name).Where(g => g.Count() > 1)
                        int queyLot = sTDetail.GroupBy(u => new { u.ItemId, u.UnitName, u.LotNumber1, u.LotNumber2, u.LotDate }).Count();
                        int queyItem = sTDetail.GroupBy(u => u.ItemId).Count();
                        if (queyLot != queyItem)
                            return "SKU:" + sTaskDetailListLotNew.First().AltItemNumber + " 在补货单号:" + SupplementGroupNumber + "已存在其它LOT的货物";
                    }

                    using (TransactionScope trans = new TransactionScope())
                    {
                        try
                        {
                            foreach (SupplementTaskDetail item in supplementTaskDetailList.Where(u => u.HuId == HuId))
                            {
                                //执行补货的WMS数据处理
                                res = SupplementTaskDownHuId(item, User);
                                if (res != "Y")
                                {
                                    trans.Dispose();//出现异常，事务手动释放
                                    return res;
                                }
                            }
                            //处理补货任务状态
                            SupplementTaskDetail supplementTaskDetail = new SupplementTaskDetail();
                            supplementTaskDetail.Status = "D";
                            supplementTaskDetail.UpdateDate = DateTime.Now;
                            supplementTaskDetail.UpdateUser = User;
                            supplementTaskDetail.GroupNumber = SupplementGroupNumber;
                            idal.ISupplementTaskDetailDAL.UpdateBy(supplementTaskDetail, u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode && u.HuId == HuId, new string[] { "GroupNumber", "", "Status", "UpdateUser", "UpdateDate" });
                            idal.SaveChanges();
                            //提交事务
                            trans.Complete();

                        }
                        catch
                        {
                            trans.Dispose();//出现异常，事务手动释放
                            return "系统执行错误!";
                        }
                    }
                }
            }
            else
                return "无可补货的明细";
            return res;
        }
        public string SupplementTaskDownHuId(SupplementTaskDetail sTD, string User)
        {

            InventoryHelper inventoryHelper = new InventoryHelper();
            //检测是否有当前托盘  
            if (!inventoryHelper.IfPlt(sTD.WhCode, sTD.HuId))
                return sTD.HuId + "无此托盘!";
            ////检测是否有目标托盘  
            //if (!inventoryHelper.IfPlt(sTD.WhCode, sTD.PutLocationId))
            //    return sTD.PutLocationId + "无此托盘!";
            //检测当前托盘是否有货
            if (!inventoryHelper.IfPltHaveStock(sTD.WhCode, sTD.HuId))
                return sTD.HuId + "无库存!";
            //验证当前托盘库位是否被冻结
            string CheckLocationIdRes = inventoryHelper.CheckLocationId(sTD.LocationId, sTD.WhCode);
            if (CheckLocationIdRes != "Y")
                return sTD.LocationId + CheckLocationIdRes;
            ////验证目标托盘库位是否被冻结
            //CheckLocationIdRes = inventoryHelper.CheckLocationId(sTD.PutLocationId, sTD.WhCode);
            //if (CheckLocationIdRes != "Y")
            //    return sTD.PutLocationId + CheckLocationIdRes;

            //执行移库操作,目标库位和托盘都是SupplementNumber
            string res = PltItemMoveActionHu(sTD.WhCode, sTD.HuId, sTD.SupplementNumber, sTD.SupplementNumber, Convert.ToInt32(sTD.HuDetailId), sTD.Qty, User);
            //解除原托盘的锁定数量
            if (res == "Y")
                res = PltItemMoveActionDoPlanQty(sTD.WhCode, Convert.ToInt32(sTD.HuDetailId), sTD.Qty, User);

            return "Y";
        }

        public string SupplementTaskUp(string SupplementNumber, string SupplementGroupNumber, string PutLocationId, string User, string WhCode)
        {

            IDAL.IDALSession idal = BLLHelper.GetDal();

            string res = "N";
            //验证补货任务,更新状态及最近修改人
            var supplementTaskStatusList = idal.ISupplementTaskDAL.SelectBy(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode);
            if (supplementTaskStatusList.Count == 1)
            {
                SupplementTask supplementTask = new SupplementTask();
                string supplementTaskStatus = supplementTaskStatusList.First().Status;
                if (supplementTaskStatus == "C")
                    return "补货任务号已经完成";
                else if (supplementTaskStatus == "A")
                {
                    supplementTask.Status = "A";
                    supplementTask.UpdateDate = DateTime.Now;
                    supplementTask.UpdateUser = User;
                }
                idal.ISupplementTaskDAL.UpdateBy(supplementTask, u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
            }
            else
                return "补货任务号不存在";

            List<SupplementTaskDetail> supplementTaskDetailList = idal.ISupplementTaskDetailDAL.SelectBy(u => u.SupplementNumber == SupplementNumber && u.GroupNumber == SupplementGroupNumber && u.PutLocationId == PutLocationId && u.WhCode == WhCode).ToList();
            if (supplementTaskDetailList.Count > 0)
            {

                if (supplementTaskDetailList.First().Status == "C")
                    return "目标库位:" + PutLocationId + "的补货任务已经完成";
                else if (supplementTaskDetailList.First().Status == "U")
                    return "目标库位:" + PutLocationId + "的补货任务未下架";

                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        //目标补货库位有多个托盘的时候,会分开补货
                        foreach (SupplementTaskDetail item in supplementTaskDetailList.Where(u => u.PutLocationId == PutLocationId && u.Status == "D"))
                        {
                            //执行补货的WMS数据处理
                            res = SupplementTaskUpHuId(item, User);
                            if (res != "Y")
                            {
                                trans.Dispose();//出现异常，事务手动释放
                                return res;
                            }
                        }

                        //处理补货任务状态
                        SupplementTaskDetail supplementTaskDetail = new SupplementTaskDetail();
                        supplementTaskDetail.Status = "C";
                        supplementTaskDetail.UpdateDate = DateTime.Now;
                        supplementTaskDetail.UpdateUser = User;
                        idal.ISupplementTaskDetailDAL.UpdateBy(supplementTaskDetail, u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode && u.PutLocationId == PutLocationId && u.Status == "D", new string[] { "Status", "UpdateUser", "UpdateDate" });
                        idal.SaveChanges();

                        //提交事务
                        trans.Complete();
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "系统执行错误!";
                    }
                }
                //更新补货任务头表状态
                if (idal.ISupplementTaskDetailDAL.SelectBy(u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode && u.Status != "C").Count() == 0)
                {
                    SupplementTask supplementTask = new SupplementTask();
                    supplementTask.Status = "C";
                    supplementTask.UpdateDate = DateTime.Now;
                    supplementTask.UpdateUser = User;
                    idal.ISupplementTaskDAL.UpdateBy(supplementTask, u => u.SupplementNumber == SupplementNumber && u.WhCode == WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    idal.SaveChanges();
                }


            }
            else
                return "目标库位:" + PutLocationId + "的补货任务不存在";
            return res;
        }
        public string SupplementTaskUpHuId(SupplementTaskDetail sTD, string User)
        {


            InventoryHelper inventoryHelper = new InventoryHelper();
            //检测当前补货任务所在托盘是否有货
            if (!inventoryHelper.IfPltHaveStock(sTD.WhCode, sTD.SupplementNumber))
                return sTD.SupplementNumber + "无 SKU:" + sTD.AltItemNumber + " 的库存!";
            //检测目标托盘  
            if (!inventoryHelper.IfPlt(sTD.WhCode, sTD.PutLocationId))
                return "库位:" + sTD.PutLocationId + "不存在托盘!";
            //验证当目标托盘库位是否被冻结
            string CheckLocationIdRes = inventoryHelper.CheckLocationId(sTD.PutLocationId, sTD.WhCode);
            if (CheckLocationIdRes != "Y")
                return sTD.PutLocationId + CheckLocationIdRes;


            IDAL.IDALSession idal = BLLHelper.GetDal();



            string res = "";

            List<HuDetail> huDetailList = idal.IHuDetailDAL.
                                    SelectBy(u => u.HuId == sTD.SupplementNumber && u.WhCode == sTD.WhCode
                                            && u.AltItemNumber == sTD.AltItemNumber && u.ItemId == sTD.ItemId && u.UnitName == sTD.UnitName && u.UnitId == sTD.UnitId
                                            && (u.LotNumber1 == sTD.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (sTD.LotNumber1 == null ? "" : sTD.LotNumber1))
                                            && (u.LotNumber2 == sTD.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (sTD.LotNumber2 == null ? "" : sTD.LotNumber2))
                                            && u.LotDate == sTD.LotDate).OrderByDescending(u => u.Qty).ToList();

            if (huDetailList.Count == 0) return "库存异常,无库存可以移库";
            //移库数量
            int MoveQty = 0;
            int unMoveQty = 0;
            foreach (HuDetail item in huDetailList)
            {

                if (unMoveQty == 0)
                    unMoveQty = sTD.Qty;
                //库存数量大于等于未移库数量
                if (item.Qty >= unMoveQty)
                {
                    MoveQty = unMoveQty;
                    //最后一次就一完了,直接返回结果
                    return PltItemMoveActionHu(sTD.WhCode, sTD.SupplementNumber, sTD.PutLocationId, sTD.PutLocationId, item.Id, unMoveQty, User);
                }
                else
                    MoveQty = item.Qty;
                //剩余数量等于未移数量-要移数量
                unMoveQty = unMoveQty - MoveQty;
                //执行移库操作,起始库位是SupplementNumber所在的库位,目标库位和托盘都是PutLocationId ,HuDetailId
                res = PltItemMoveActionHu(sTD.WhCode, sTD.SupplementNumber, sTD.PutLocationId, sTD.PutLocationId, item.Id, unMoveQty, User);
                //移库中结果又错,也返回
                if (res != "Y") return res;

            }
            if (unMoveQty > 0) return "数据异常,还剩余数量:" + unMoveQty + "未分配";


            //执行移库操作,起始库位是SupplementNumber所在的库位,目标库位和托盘都是PutLocationId 
            //string res = PltItemMoveActionHu(sTD.WhCode, sTD.SupplementNumber, sTD.PutLocationId, sTD.PutLocationId, HuDetailId, sTD.Qty, User);



            return "Y";
        }

        #endregion
        #endregion

        #region 3.0 盘点
        /// <summary>
        /// 检测盘点任务是否存在
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>Y或错误结果</returns>
        public string IfCycleTaskNumber(string WhCode, string TaskNumber)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<string> list = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == WhCode && u.TaskNumber == TaskNumber).Select(u => u.Status).ToList();
            if (list.Count > 0)
                if (list.First() == "U" || list.First() == "A")
                    return "Y";
                else
                    return TaskNumber + "已经完成盘点";
            else
                return "无此盘点任务!";

        }
        public int CycleTaskNumberOneByOneScanFlag(string WhCode, string TaskNumber)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<int?> list = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == WhCode && u.TaskNumber == TaskNumber).Select(u => u.OneByOneScanFlag).ToList();
            if (list.Count > 0)
                if (list.First() == 1)
                    return 1;
                else
                    return 0;
            else
                return 0;

        }

        /// <summary>
        /// 获取盘点任务建议库位,按照库位名顺序排序
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回null</returns>
        public string GetCTSugLoc(string WhCode, string TaskNumber)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<string> list = idal.ICycleCountDetailDAL.SelectBy(u => u.WhCode == WhCode && u.TaskNumber == TaskNumber && (u.Status == "U" || u.Status == "A")).OrderBy(u => u.LocationId).Take(1).Select(u => u.LocationId).ToList();
            if (list.Count > 0)
                return list.First();
            else
                return null;

        }
        /// <summary>
        /// 获取盘点任务剩余库位数
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回0</returns>
        public int GetLocRemainingQty(string WhCode, string TaskNumber)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            return idal.ICycleCountDetailDAL.SelectBy(u => u.WhCode == WhCode && u.TaskNumber == TaskNumber && (u.Status == "U" || u.Status == "A")).Count();
        }

        /// <summary>
        /// 检测托盘是否在盘点任务中
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <param name="LocationId"></param>
        /// <returns>Y或异常信息</returns>
        public string IfLocInCycleTaskNumber(string WhCode, string TaskNumber, string LocationId)
        {
            IDAL.IDALSession idal = BLLHelper.GetDal();
            if (idal.IWhLocationDAL.SelectBy(u => u.LocationId == LocationId && u.WhCode == WhCode).Count() == 0)
                return LocationId + "不存在!";
            List<string> list = idal.ICycleCountDetailDAL.SelectBy(u => u.WhCode == WhCode && u.TaskNumber == TaskNumber && u.LocationId == LocationId).Select(u => u.Status).ToList();
            if (list.Count > 0)
                if (list.First() == "U" || list.First() == "A")
                    return "Y";
                else
                    return LocationId + "已经盘点";
            else
                return LocationId + "不在" + TaskNumber + "任务中";

        }
        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string CycleCountInsert(CycleCountInsertComplex entity)
        {

            IRootManager rootManager = new RootManager();
            string res = rootManager.CycleCountInsertComplex(entity);
            if (res == "")
                return "Y";
            else
                return res;
        }
        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string CycleCountInsert(CycleCountInsertComplexAddPo entity)
        {

            IRootManager rootManager = new RootManager();
            string res = rootManager.CycleCountInsertComplex(entity);
            if (res == "")
                return "Y";
            else
                return res;
        }
        /// <summary>
        /// 盘点检查
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string CheckCycleResult(CycleCountInsertComplexAddPo entity)
        {
            IRootManager rootManager = new RootManager();
            string res = rootManager.CheckCycleResult(entity);
            if (res == "")
                return "Y";
            else
                return res;
        }

        public CycleCountInsertComplexAddPo CycleEANChangeToSKU(CycleCountInsertComplexAddPo entity)
        {


            IDAL.IDALSession idal = BLLHelper.GetDal();
            List<string> skuList = new List<string>();
            foreach (var item in entity.HuIdModelAddPo)
            {
                if (item.PoModel != null)
                {
                    foreach (var item1 in item.PoModel)
                    {
                        if (item1.HuDetailModel != null)
                        {
                            foreach (var item2 in item1.HuDetailModel)
                            {

                                skuList.Add(item2.AltItemNumber);
                            }
                        }
                    }
                }
            }

            var sql = (from a in idal.IItemMasterDAL.SelectAll()
                       where skuList.Contains(a.EAN) && a.WhCode == entity.WhCode
                       select new { a.AltItemNumber, a.EAN }).ToList();
            if (sql.Count > 0)
            {
                foreach (var item in entity.HuIdModelAddPo)
                {
                    if (item.PoModel != null)
                    {
                        foreach (var item1 in item.PoModel)
                        {
                            if (item1.HuDetailModel != null)
                            {
                                foreach (var item2 in item1.HuDetailModel)
                                {

                                    var sq0 = sql.Where(u => u.EAN == item2.AltItemNumber);
                                    //转换成功后 ,即行数为1
                                    if (sq0.Count() == 1)
                                    {
                                        string AltItemNumber = sq0.First().AltItemNumber;
                                        item2.AltItemNumber = AltItemNumber;
                                    }


                                }
                            }
                        }
                    }
                }

            }


            return entity;
        }

        /// <summary>
        /// 盘点完成
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string CycleCountComplete(string WhCode, string TaskNumber, string User)
        {
            IRootManager rootManager = new RootManager();
            return rootManager.CycleCountComplete(TaskNumber, WhCode, User);
        }

        #endregion
    }
}
