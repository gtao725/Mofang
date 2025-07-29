using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class LoadManager : ILoadManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        ShipHelper shipHelper = new ShipHelper();

        //Load查询
        public List<LoadMasterResult> LoadMasterList(LoadMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.ILoadContainerExtendDAL.SelectAll()
                            on new { a.LoadId, a.WhCode }
                        equals new { d.LoadId, d.WhCode } into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in idal.ILoadContainerTypeDAL.SelectAll()
                            on d.ContainerType equals e.ContainerType into temp4
                      from e in temp4.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      group new { a, c, d } by new
                      {
                          a.Id,
                          a.LoadId,
                          a.ShipMode,
                          a.Status0,
                          a.Status1,
                          a.Status2,
                          a.Status3,
                          a.ProcessName,
                          a.Remark,
                          a.ReleaseDate,
                          a.BeginPickDate,
                          a.EndPickDate,
                          a.BeginPackDate,
                          a.EndPackDate,
                          a.BeginSortDate,
                          a.EndSortDate,
                          a.ShipDate,
                          a.ProcessId,
                          c.ClientId,
                          c.ClientCode,
                          c.CustomerOutPoNumber,
                          a.CreateUser,
                          a.CreateDate,
                          d.ETD,
                          d.ContainerNumber,
                          d.SealNumber,
                          d.ContainerType,
                          d.VesselName,
                          d.VesselNumber,
                          d.CarriageName,
                          d.Port,
                          d.DeliveryPlace,
                          d.BillNumber,
                          d.PortSuitcase,
                          e.ContainerName,
                          a.SumQty,
                          a.DSSumQty,
                          a.SumCBM,
                          a.SumWeight,
                          a.EchQty
                      } into g
                      select new LoadMasterResult
                      {
                          Id = g.Key.Id,
                          ClientCode = g.Key.ClientCode,
                          LoadId = g.Key.LoadId,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          ShipMode = g.Key.ShipMode,
                          Status0 =
                           g.Key.Status0 == "U" ? "未释放" :
                           g.Key.Status0 == "C" ? "已释放" : null,
                          Status1 =
                           g.Key.Status1 == "U" ? "未备货" :
                           g.Key.Status1 == "A" ? "正在备货" :
                           g.Key.Status1 == "C" ? "完成备货" : null,
                          Status2 =
                           g.Key.Status2 == "U" ? "未分拣" :
                           g.Key.Status2 == "A" ? "正在分拣" :
                           g.Key.Status2 == "C" ? "完成分拣" : null,
                          Status3 =
                           g.Key.Status3 == "U" ? "未装箱" :
                           g.Key.Status3 == "A" ? "正在装箱" :
                           g.Key.Status3 == "C" ? "完成装箱" : null,
                          ShipStatus = g.Key.ShipDate == null ? "未封箱" : "已封箱",
                          ProcessId = g.Key.ProcessId,
                          ProcessName = g.Key.ProcessName,
                          ContainerNumber = g.Key.ContainerNumber ?? "",
                          SealNumber = g.Key.SealNumber ?? "",
                          ContainerType = g.Key.ContainerType ?? "",
                          ContainerName = g.Key.ContainerName ?? "",
                          ETD = g.Key.ETD,
                          VesselName = g.Key.VesselName ?? "",
                          VesselNumber = g.Key.VesselNumber ?? "",
                          CarriageName = g.Key.CarriageName ?? "",
                          Port = g.Key.Port ?? "",
                          DeliveryPlace = g.Key.DeliveryPlace ?? "",
                          BillNumber = g.Key.BillNumber ?? "",
                          PortSuitcase = g.Key.PortSuitcase,
                          ReleaseDate = g.Key.ReleaseDate,
                          BeginPickDate = g.Key.BeginPickDate,
                          EndPickDate = g.Key.EndPickDate,
                          BeginPackDate = g.Key.BeginPackDate,
                          EndPackDate = g.Key.EndPackDate,
                          BeginSortDate = g.Key.BeginSortDate,
                          EndSortDate = g.Key.EndSortDate,
                          ShipDate = g.Key.ShipDate,
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Key.CreateDate,
                          Remark = g.Key.Remark ?? "",
                          SumQty = g.Key.SumQty,
                          DSSumQty = g.Key.DSSumQty,
                          EchQty = g.Key.EchQty,
                          SumCBM = g.Key.SumCBM,
                          SumWeight = g.Key.SumWeight
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ShipMode))
                sql = sql.Where(u => u.ShipMode == searchEntity.ShipMode);
            if (!string.IsNullOrEmpty(searchEntity.Status0))
                sql = sql.Where(u => u.Status0 == searchEntity.Status0);
            if (!string.IsNullOrEmpty(searchEntity.Status1))
                sql = sql.Where(u => u.Status1 == searchEntity.Status1);
            if (!string.IsNullOrEmpty(searchEntity.Status3))
                sql = sql.Where(u => u.Status3 == searchEntity.Status3);
            if (!string.IsNullOrEmpty(searchEntity.ShipStatus))
                sql = sql.Where(u => u.ShipStatus == searchEntity.ShipStatus);
            if (searchEntity.BeginShipDate != null)
            {
                sql = sql.Where(u => u.ShipDate >= searchEntity.BeginShipDate);
            }
            if (searchEntity.EndShipDate != null)
            {
                sql = sql.Where(u => u.ShipDate <= searchEntity.EndShipDate);
            }
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);

            List<LoadMasterResult> list = new List<LoadMasterResult>();

            foreach (var item in sql)
            {
                if (list.Where(u => u.Id == item.Id).Count() == 0)
                {
                    LoadMasterResult loadMaster = new LoadMasterResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Action3 = "";
                    loadMaster.Id = item.Id;
                    loadMaster.ClientCode = item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId;
                    loadMaster.CustomerOutPoNumber = item.CustomerOutPoNumber ?? "";
                    loadMaster.ShipMode = item.ShipMode;
                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;
                    loadMaster.Status2 = item.Status2;
                    loadMaster.Status3 = item.Status3;
                    loadMaster.ShipStatus = item.ShipStatus;
                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;
                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";
                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.ContainerName = item.ContainerName ?? "";

                    if (item.ETD != null)
                    {
                        loadMaster.ETDShow = Convert.ToDateTime(item.ETD).ToString("d");
                    }

                    loadMaster.VesselName = item.VesselName ?? "";
                    loadMaster.VesselNumber = item.VesselNumber ?? "";
                    loadMaster.CarriageName = item.CarriageName ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.DeliveryPlace = item.DeliveryPlace ?? "";
                    loadMaster.BillNumber = item.BillNumber ?? "";
                    loadMaster.PortSuitcase = item.PortSuitcase;
                    loadMaster.ReleaseDate = item.ReleaseDate;
                    loadMaster.BeginPickDate = item.BeginPickDate;
                    loadMaster.EndPickDate = item.EndPickDate;
                    loadMaster.BeginPackDate = item.BeginPackDate;
                    loadMaster.EndPackDate = item.EndPackDate;
                    loadMaster.BeginSortDate = item.BeginSortDate;
                    loadMaster.EndSortDate = item.EndSortDate;
                    loadMaster.ShipDate = item.ShipDate;
                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    list.Add(loadMaster);
                }
                else
                {
                    LoadMasterResult getModel = list.Where(u => u.Id == item.Id).First();
                    list.Remove(getModel);

                    LoadMasterResult loadMaster = new LoadMasterResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Action3 = "";
                    loadMaster.Id = item.Id;
                    loadMaster.ClientCode = getModel.ClientCode ?? "" + "," + item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId;

                    loadMaster.CustomerOutPoNumber = getModel.CustomerOutPoNumber + "," + item.CustomerOutPoNumber;

                    loadMaster.ShipMode = item.ShipMode;
                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;
                    loadMaster.Status2 = item.Status2;
                    loadMaster.Status3 = item.Status3;
                    loadMaster.ShipStatus = item.ShipStatus;
                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;
                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";
                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.ContainerName = item.ContainerName ?? "";

                    if (item.ETD != null)
                    {
                        loadMaster.ETDShow = Convert.ToDateTime(item.ETD).ToString("d");
                    }

                    loadMaster.VesselName = item.VesselName ?? "";
                    loadMaster.VesselNumber = item.VesselNumber ?? "";
                    loadMaster.CarriageName = item.CarriageName ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.DeliveryPlace = item.DeliveryPlace ?? "";
                    loadMaster.BillNumber = item.BillNumber ?? "";
                    loadMaster.PortSuitcase = item.PortSuitcase;
                    loadMaster.ReleaseDate = item.ReleaseDate;
                    loadMaster.BeginPickDate = item.BeginPickDate;
                    loadMaster.EndPickDate = item.EndPickDate;
                    loadMaster.BeginPackDate = item.BeginPackDate;
                    loadMaster.EndPackDate = item.EndPackDate;
                    loadMaster.BeginSortDate = item.BeginSortDate;
                    loadMaster.EndSortDate = item.EndSortDate;
                    loadMaster.ShipDate = item.ShipDate;
                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    list.Add(loadMaster);

                }

            }
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                list = list.Where(u => u.ClientCode.Contains(searchEntity.ClientCode)).ToList();
            }
            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
            {
                list = list.Where(u => u.CustomerOutPoNumber.Contains(searchEntity.CustomerOutPoNumber)).ToList();
            }

            List<LoadMasterResult> list1 = new List<LoadMasterResult>();
            foreach (var item in list)
            {
                LoadMasterResult loadmas = item;

                if (loadmas.CustomerOutPoNumber.Length > 30)
                {
                    loadmas.CustomerOutPoNumber = loadmas.CustomerOutPoNumber.Substring(0, 30) + "...";
                }
                list1.Add(loadmas);
            }

            total = list1.Count;
            list1 = list1.OrderByDescending(u => u.Id).ToList();
            list1 = list1.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list1;

        }

        //Load明细查询 直接显示订单明细
        public List<OutBoundOrderDetailResult> LoadToOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.ILoadDetailDAL.SelectAll()
                      where a.LoadMasterId == searchEntity.LoadMasterId
                      join b in idal.IOutBoundOrderDAL.SelectAll()
                      on a.OutBoundOrderId equals b.Id
                      join d in idal.IOutBoundOrderDetailDAL.SelectAll()
                     on new { Id = b.Id } equals new { Id = d.OutBoundOrderId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on d.ItemId equals c.Id
                      join f in idal.IUnitDefaultDAL.SelectAll()
                     on new { A = d.WhCode, B = d.UnitName } equals new { A = f.WhCode, B = f.UnitName }
                      select new OutBoundOrderDetailResult
                      {
                          ClientCode = b.ClientCode,
                          SoNumber = d.SoNumber,
                          CustomerPoNumber = d.CustomerPoNumber,
                          AltItemNumber = d.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Sequence = d.Sequence ?? 0,
                          UnitName = f.UnitNameCN,
                          Qty = d.Qty,
                          TotalCbm = d.TotalCbm,
                          LotNumber1 = d.LotNumber1,
                          LotNumber2 = d.LotNumber2
                      };

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"实装立方\":\"" + sql.Sum(u => u.TotalCbm).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Sequence);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //Load创建
        public LoadMaster LoadMasterAdd(LoadMaster entity)
        {
            if (entity.WhCode == null || entity.ShipMode == null || entity.WhCode == "" || entity.ShipMode == "" || entity.ProcessId == 0 || entity.ProcessName == "")
            {
                return null;
            }
            entity.LoadId = "LD" + DI.IDGenerator.NewId;
            entity.Status0 = "U";
            entity.Status1 = "U";
            entity.Status2 = "U";
            entity.Status3 = "U";
            entity.CreateDate = DateTime.Now;
            idal.ILoadMasterDAL.Add(entity);
            idal.ILoadMasterDAL.SaveChanges();
            return entity;
        }

        //Load创建
        public string LoadContainerExtendAdd(LoadContainerExtend entity)
        {
            if (entity.WhCode == "" || entity.LoadId == "")
            {
                return "数据有误！";
            }

            List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId);
            if (loadMasterList.Count == 0)
            {
                return "Load信息有误！";
            }
            if (loadMasterList.First().Status0 != "U")
            {
                return "Load状态有误，请重新查询！";
            }

            List<LoadContainerType> loadContainerTypeList = idal.ILoadContainerTypeDAL.SelectBy(u => u.ContainerType == entity.ContainerType);
            if (loadContainerTypeList.Count == 0)
            {
                return "箱型不存在或有误！";
            }

            List<LoadContainerExtend> LoadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId);
            if (LoadContainerExtendList.Count == 0)
            {
                LoadContainerExtend loadContainerExtend = new LoadContainerExtend();
                loadContainerExtend.WhCode = entity.WhCode;
                loadContainerExtend.LoadId = entity.LoadId;
                loadContainerExtend.VesselName = entity.VesselName;
                loadContainerExtend.VesselNumber = entity.VesselNumber;
                loadContainerExtend.CarriageName = entity.CarriageName;
                loadContainerExtend.ETD = entity.ETD;
                loadContainerExtend.ContainerType = entity.ContainerType;
                loadContainerExtend.Port = entity.Port;
                loadContainerExtend.PortSuitcase = entity.PortSuitcase;
                loadContainerExtend.BaQty = entity.BaQty;
                loadContainerExtend.DeliveryPlace = entity.DeliveryPlace;
                loadContainerExtend.BillNumber = entity.BillNumber;
                loadContainerExtend.ContainerNumber = entity.ContainerNumber;
                loadContainerExtend.SealNumber = entity.SealNumber;
                loadContainerExtend.CreateUser = entity.CreateUser;
                loadContainerExtend.CreateDate = DateTime.Now;
                loadContainerExtend.ContainerSource = "WMS";
                idal.ILoadContainerExtendDAL.Add(loadContainerExtend);
            }
            else
            {
                entity.UpdateUser = entity.CreateUser;
                entity.UpdateDate = DateTime.Now;
                idal.ILoadContainerExtendDAL.UpdateBy(entity, u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId, new string[] { "VesselName", "VesselNumber", "CarriageName", "ETD", "ContainerType", "Port", "DeliveryPlace", "BillNumber", "ContainerNumber", "SealNumber", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendDAL.SaveChanges();
            return "Y";
        }


        //箱型下拉列表
        public IEnumerable<LoadContainerType> LoadContainerTypeSelect()
        {
            var sql = from a in idal.ILoadContainerTypeDAL.SelectAll()
                      select a;
            return sql.AsEnumerable();
        }

        //出库订单查询
        public List<OutBoundOrderResult> Load_OutBoundOrderList(OutBoundOrderSearch searchEntity, string[] customerOutPo, out int total)
        {
            var sql1 = (from loaddetail in idal.ILoadDetailDAL.SelectAll()
                        where loaddetail.LoadMasterId == searchEntity.LoadMasterId
                        select loaddetail.OutBoundOrderId).Distinct();

            //---------------------------------------------------- 表示状态为已确认订单 才能被Load选择
            var sql = from a in idal.IOutBoundOrderDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadFlag == 0 && a.StatusName == "已确认订单" && a.DSFLag != 1
                      join b in idal.IFlowHeadDAL.SelectAll()
                      on new { ProcessId = a.ProcessId } equals new { ProcessId = b.Id }
                      where !sql1.Contains(a.Id)
                      select new OutBoundOrderResult
                      {
                          Id = a.Id,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          OutPoNumber = a.OutPoNumber,
                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          FlowName = b.FlowName,
                          OrderSource = a.OrderSource,
                          PlanOutTime = a.PlanOutTime,
                          ProcessId = a.ProcessId
                      };

            if (searchEntity.ClientId != 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (customerOutPo != null)
                sql = sql.Where(u => customerOutPo.Contains(u.CustomerOutPoNumber));
            if (searchEntity.ProcessId != 0)
                sql = sql.Where(u => u.ProcessId == searchEntity.ProcessId);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加Load明细
        public int LoadDetailAdd(List<LoadDetail> entity)
        {
            if (entity == null)
            {
                return 0;
            }

            int loadMasterId = Convert.ToInt32(entity.First().LoadMasterId);
            int processId = idal.ILoadMasterDAL.SelectBy(u => u.Id == loadMasterId).First().ProcessId;

            FlowDetail flowRule = (from a in idal.IFlowDetailDAL.SelectAll() where a.FlowHeadId == processId && a.Type == "Create" select a).First();
            int count = 0;
            foreach (var item in entity)
            {
                var sql = from a in idal.IOutBoundOrderDAL.SelectAll()
                          where a.Id == item.OutBoundOrderId && a.LoadFlag == 0 && a.StatusName == "已确认订单"
                          select a;
                if (sql.Count() > 0)
                {
                    idal.ILoadDetailDAL.Add(item);
                    OutBoundOrder outBoundOrder = new OutBoundOrder();
                    outBoundOrder.LoadFlag = 1;
                    outBoundOrder.NowProcessId = flowRule.FlowRuleId;
                    outBoundOrder.StatusId = flowRule.StatusId;
                    outBoundOrder.StatusName = flowRule.StatusName;
                    outBoundOrder.UpdateUser = item.CreateUser;
                    outBoundOrder.UpdateDate = item.CreateDate;
                    idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.OutBoundOrderId, new string[] { "LoadFlag", "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });
                    if (count == 0)
                    {
                        LoadMaster loadMaster = new LoadMaster();
                        loadMaster.ProcessId = sql.First().ProcessId;
                        loadMaster.UpdateUser = item.CreateUser;
                        loadMaster.UpdateDate = item.CreateDate;
                        idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.Id == item.LoadMasterId, new string[] { "ProcessId", "UpdateUser", "UpdateDate" });
                    }
                    count++;
                }
            }
            idal.ILoadDetailDAL.SaveChanges();
            return 1;
        }


        //Load删除 同时删除明细
        public string LoadMasterDel(int id)
        {
            if (id == 0)
            {
                return "数据有误，请重新操作！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == id).First();
                    if (loadMaster.Status0 != "U")
                    {
                        return "Load状态有误，请重新查询！";
                    }

                    List<LoadDetail> loadDetailList = (from a in idal.ILoadDetailDAL.SelectAll()
                                                       where a.LoadMasterId == id
                                                       select a).ToList();

                    //一次性查询出Load包含的订单
                    int?[] outBoundOrderIdArr = (from a in loadDetailList
                                                 select a.OutBoundOrderId).ToList().Distinct().ToArray();

                    List<OutBoundOrder> getAllOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => outBoundOrderIdArr.Contains(u.Id));

                    List<TranLog> tranLogList = new List<TranLog>();

                    string result = "";
                    foreach (var item in loadDetailList)
                    {
                        List<OutBoundOrder> OutBoundOrderList = getAllOutBoundOrderList.Where(u => u.Id == item.OutBoundOrderId).ToList();
                        if (OutBoundOrderList.Count > 0)
                        {
                            OutBoundOrder eneity = OutBoundOrderList.First();
                            if (eneity.StatusId > 15)
                            {
                                result = "错误！订单状态有误！";
                                break;
                            }
                            if (eneity.StatusId == -10)
                            {
                                continue;
                            }

                            if (eneity.DSFLag == 1)
                            {
                                //1.删除直装订单
                                idal.IOutBoundOrderDAL.DeleteByExtended(u => u.Id == eneity.Id);
                                idal.IOutBoundOrderDetailDAL.DeleteByExtended(u => u.OutBoundOrderId == eneity.Id);
                            }
                            else
                            {
                                string remark1 = "原状态：" + eneity.StatusId + eneity.StatusName;

                                FlowHelper flowHelper = new FlowHelper(eneity, "OutBound");
                                FlowDetail flowDetail = flowHelper.GetPreviousFlowDetail();

                                if (flowDetail != null && flowDetail.StatusId != 0)
                                {
                                    eneity.LoadFlag = 0;
                                    eneity.NowProcessId = flowDetail.FlowRuleId;
                                    eneity.StatusId = flowDetail.StatusId;
                                    eneity.StatusName = flowDetail.StatusName;

                                    //idal.IOutBoundOrderDAL.UpdateBy(eneity, u => u.Id == eneity.Id, new string[] { "LoadFlag", "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                    idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == eneity.Id, t => new OutBoundOrder { LoadFlag = eneity.LoadFlag, NowProcessId = eneity.NowProcessId, StatusId = eneity.StatusId, StatusName = eneity.StatusName });

                                    //更新订单状态，插入日志
                                    TranLog tlorder = new TranLog();
                                    tlorder.TranType = "32";
                                    tlorder.Description = "更新订单状态";
                                    tlorder.TranDate = DateTime.Now;
                                    tlorder.TranUser = eneity.CreateUser;
                                    tlorder.WhCode = eneity.WhCode;
                                    tlorder.CustomerOutPoNumber = eneity.CustomerOutPoNumber;
                                    tlorder.OutPoNumber = eneity.OutPoNumber;
                                    tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                                    tranLogList.Add(tlorder);
                                }
                                else
                                {
                                    result = "错误！获取订单状态有误！";
                                    break;
                                }
                            }
                        }
                        else
                        {
                            result = "错误！订单出现异常信息，无法关联Load！";
                            break;
                        }
                    }

                    if (result + "" != "")
                    {
                        return result;
                    }

                    idal.ITranLogDAL.Add(tranLogList);

                    List<LoadContainerExtend> getloadExend = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId);
                    if (getloadExend.Count > 0)
                    {
                        LoadContainerExtend firstLoadExtend = getloadExend.First();
                        idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.LoadContainerId == firstLoadExtend.Id);

                        LoadContainerExtend loadExend = new LoadContainerExtend();
                        loadExend.LoadId = "";
                        idal.ILoadContainerExtendDAL.UpdateBy(loadExend, u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId, new string[] { "LoadId" });
                    }

                    idal.ILoadDetailDAL.DeleteBy(u => u.LoadMasterId == id);
                    idal.ILoadMasterDAL.DeleteBy(u => u.Id == id);

                    idal.ILoadMasterDAL.SaveChanges();
                    trans.Complete();
                    return "Y";

                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "删除Load超时，请重新提交！";
                }
            }
        }


        //Load订单 明细查询
        public List<OutBoundOrderResult> LoadSecond_OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadDetailDAL.SelectAll()
                      where a.LoadMasterId == searchEntity.LoadMasterId
                      join b in idal.IOutBoundOrderDAL.SelectAll()
                      on a.OutBoundOrderId equals b.Id
                      join c in idal.IFlowHeadDAL.SelectAll()
                      on b.ProcessId equals c.Id
                      join d in idal.IOutBoundOrderDetailDAL.SelectAll()
                     on new { Id = b.Id } equals new { Id = d.OutBoundOrderId }
                      group new { b, c, d } by new
                      {
                          a.Id,
                          Column1 = b.Id,
                          b.ClientCode,
                          b.OutPoNumber,
                          b.CustomerOutPoNumber,
                          c.FlowName,
                          b.StatusName,
                          b.AltCustomerOutPoNumber
                      } into g
                      select new OutBoundOrderResult
                      {
                          Id = g.Key.Id,
                          OutBoundOrderId = g.Key.Column1,
                          ClientCode = g.Key.ClientCode,
                          OutPoNumber = g.Key.OutPoNumber,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          AltCustomerOutPoNumber = g.Key.AltCustomerOutPoNumber,
                          FlowName = g.Key.FlowName,
                          StatusName = g.Key.StatusName,
                          SumQty = g.Sum(p => p.d.Qty)
                      };

            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
                sql = sql.Where(u => u.CustomerOutPoNumber == searchEntity.CustomerOutPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltCustomerOutPoNumber))
                sql = sql.Where(u => u.AltCustomerOutPoNumber == searchEntity.AltCustomerOutPoNumber);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //Load删除明细 验证Load下是否还有明细 如果没有 删除Load任务
        public string LoadDetailDel(int id)
        {
            if (id == 0)
            {
                return "数据有误，请重新操作！";
            }

            LoadDetail loadDetail = idal.ILoadDetailDAL.SelectBy(u => u.Id == id).First();
            LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == loadDetail.LoadMasterId).First();
            if (loadMaster.Status0 != "U")
            {
                return "Load状态有误，请重新查询！";
            }

            OutBoundOrder eneity = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == loadDetail.OutBoundOrderId).First();
            //未释放前删除订单  回滚订单状态至上一步 ，当前状态为已生成Load 15
            if (eneity.StatusId != 15)
            {
                return "订单状态有误，请重新查询！";
            }

            string remark1 = "原状态：" + eneity.StatusId + eneity.StatusName;

            FlowHelper flowHelper = new FlowHelper(eneity, "OutBound");
            FlowDetail flowDetail = flowHelper.GetPreviousFlowDetail();
            if (flowDetail != null && flowDetail.StatusId != 0)
            {
                eneity.LoadFlag = 0;
                eneity.NowProcessId = flowDetail.FlowRuleId;
                eneity.StatusId = flowDetail.StatusId;
                eneity.StatusName = flowDetail.StatusName;
                eneity.UpdateUser = eneity.CreateUser ?? "1008";
                eneity.UpdateDate = DateTime.Now;
                idal.IOutBoundOrderDAL.UpdateBy(eneity, u => u.Id == eneity.Id, new string[] { "LoadFlag", "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                //更新订单状态，插入日志
                TranLog tlorder = new TranLog();
                tlorder.TranType = "32";
                tlorder.Description = "更新订单状态";
                tlorder.TranDate = DateTime.Now;
                tlorder.TranUser = eneity.CreateUser;
                tlorder.WhCode = eneity.WhCode;
                tlorder.CustomerOutPoNumber = eneity.CustomerOutPoNumber;
                tlorder.OutPoNumber = eneity.OutPoNumber;
                tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                idal.ITranLogDAL.Add(tlorder);

            }
            else
            {
                return "错误！获取订单状态有误！";
            }

            idal.ILoadDetailDAL.DeleteBy(u => u.Id == id);
            idal.ILoadDetailDAL.SaveChanges();

            List<LoadDetail> loadDetailList = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadMaster.Id);
            if (loadDetailList.Count == 0)
            {
                idal.ILoadMasterDAL.DeleteBy(u => u.Id == loadMaster.Id);

                LoadContainerExtend loadExend = new LoadContainerExtend();
                loadExend.LoadId = "";
                idal.ILoadContainerExtendDAL.UpdateBy(loadExend, u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId, new string[] { "LoadId" });
            }

            idal.ILoadMasterDAL.SaveChanges();
            return "Y";
        }

        //查询Load批量导入出货托盘列表
        public List<LoadHuIdExtend> LoadHuIdExtendSearch(LoadHuIdExtendSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadHuIdExtendDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId == searchEntity.LoadId
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //Load 批量导入出货托盘
        public string LoadHuIdExtendImport(List<LoadHuIdExtend> entity)
        {
            lock (o)
            {
                LoadHuIdExtend loadHuId = entity.First();
                LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadHuId.LoadId && u.WhCode == loadHuId.WhCode).First();
                if (load.Status0 != "U")
                {
                    return "Load状态有误，请重新查询！";
                }

                List<LoadHuIdExtend> loadHuIdExtendList = new List<LoadHuIdExtend>();
                foreach (var item in entity)
                {
                    item.CreateDate = DateTime.Now;
                    loadHuIdExtendList.Add(item);
                }

                idal.ILoadHuIdExtendDAL.Add(loadHuIdExtendList);
                idal.ILoadHuIdExtendDAL.SaveChanges();
                return "Y";
            }

        }

        //直装订单列表
        public List<OutBoundOrderResult> DSOutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadDetailDAL.SelectAll()
                      join b in idal.IOutBoundOrderDAL.SelectAll()
                      on a.OutBoundOrderId equals b.Id
                      join c in idal.IFlowHeadDAL.SelectAll()
                      on b.ProcessId equals c.Id
                      join d in idal.IOutBoundOrderDetailDAL.SelectAll()
                     on new { Id = b.Id } equals new { Id = d.OutBoundOrderId }
                      where a.LoadMasterId == searchEntity.LoadMasterId && b.DSFLag == 1
                      group new { b, c, d } by new
                      {
                          a.Id,
                          Column1 = b.Id,
                          b.ClientCode,
                          b.OutPoNumber,
                          b.CustomerOutPoNumber,
                          c.FlowName,
                          b.StatusName,
                          b.PlanOutTime
                      } into g
                      select new OutBoundOrderResult
                      {
                          Id = g.Key.Id,
                          OutBoundOrderId = g.Key.Column1,
                          ClientCode = g.Key.ClientCode,
                          OutPoNumber = g.Key.OutPoNumber,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          FlowName = g.Key.FlowName,
                          StatusName = g.Key.StatusName,
                          PlanOutTime = g.Key.PlanOutTime,
                          SumQty = g.Sum(p => p.d.Qty)
                      };

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //直装订单 验证客户收货流程是否一致
        public List<FlowHeadResult> CheckClientCodeRule(string whCode, string[] clientCode)
        {
            List<FlowHeadResult> result = new List<FlowHeadResult>();

            List<FlowHeadResult> sql = (from a in idal.IWhClientDAL.SelectAll()
                                        join b in idal.IR_Client_FlowRuleDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                        from b in b_join.DefaultIfEmpty()
                                        join c in idal.IFlowHeadDAL.SelectAll() on new { BusinessFlowGroupId = (Int32)b.BusinessFlowGroupId } equals new { BusinessFlowGroupId = c.Id } into c_join
                                        from c in c_join.DefaultIfEmpty()
                                        where
                                          a.WhCode == whCode && c.Type == "InBound"
                                        select new FlowHeadResult
                                        {
                                            Remark = a.ClientCode,
                                            FlowName = c.FlowName,
                                            Id = c.Id
                                        }).ToList();

            sql = sql.Where(u => clientCode.Contains(u.Remark)).ToList();
            if (sql.Count == 0)
            {
                return result;
            }
            else
            {
                //得到第一个客户的流程
                List<FlowHeadResult> first = sql.Where(u => u.Remark == clientCode[0]).ToList();
                if (clientCode.Length == 1)
                {
                    result = first;
                    return result;
                }

                //循环第二个客户以后的流程
                for (int i = 1; i < clientCode.Length; i++)
                {
                    List<FlowHeadResult> check = sql.Where(u => u.Remark == clientCode[i]).ToList();
                    if (i == 1)
                    {
                        //如果第一个客户的流程多余第二个客户的流程
                        if (first.Count >= check.Count)
                        {
                            foreach (var item in first)
                            {
                                if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in check)
                            {
                                if (first.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        //如果第一个客户和第二个客户 没有共同的流程 直接断开循环 返回Null
                        if (result.Count == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        List<FlowHeadResult> resulttemp = new List<FlowHeadResult>();

                        foreach (var item in result)
                        {
                            if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                            {
                                FlowHeadResult f = new FlowHeadResult();
                                f.FlowName = item.FlowName;
                                f.Id = item.Id;
                                resulttemp.Add(f);
                            }
                        }

                        if (resulttemp.Count == 0)
                        {
                            result.Clear();
                        }
                        else
                        {
                            if (resulttemp.Count < result.Count)
                            {
                                result = resulttemp;
                            }
                        }
                    }
                }

                return result;
            }
        }

        //直装出库订单导入
        public string DSOutBoundImport(List<ImportOutBoundOrder> entityList, string loadId)
        {
            string result = "";     //执行总结果

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            string whCode = entityList.First().WhCode;
            string userName = entityList.First().OutBoundOrderDetailModel.First().CreateUser;

            List<InBoundOrderInsert> getClientFlowRule1 = (from a in idal.IWhClientDAL.SelectAll()
                                                           join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                                                           on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                                           from b in b_join.DefaultIfEmpty()
                                                           join c in idal.IFlowHeadDAL.SelectAll()
                                                           on new { BusinessFlowGroupId = (Int32)b.BusinessFlowGroupId } equals new { BusinessFlowGroupId = c.Id } into c_join
                                                           from c in c_join.DefaultIfEmpty()
                                                           where
                                                             a.WhCode == whCode &&
                                                             b.BusinessFlowGroupId != null
                                                             && c.Type == "InBound"
                                                           orderby
                                                             b.Id
                                                           select new InBoundOrderInsert
                                                           {
                                                               ClientId = a.Id,
                                                               ClientCode = a.ClientCode,
                                                               ProcessId = b.BusinessFlowGroupId,
                                                               ProcessName = c.FlowName
                                                           }).ToList();

            List<string> checkClientCode1 = (from a in entityList
                                             select a.ClientCode).Distinct().ToList();

            //首先验证  批量导入的客户名是否设置了流程
            foreach (var item in checkClientCode1)
            {
                if (getClientFlowRule1.Where(u => u.ClientCode == item).Count() == 0)
                {
                    result = "当前客户未配置收货流程：" + item;
                    break;
                }
            }
            if (result != "")
            {
                return result;
            }

            //验证直装订单中的客户收货流程是否一致
            List<FlowHeadResult> flowHeadRuesultList = CheckClientCodeRule(whCode, checkClientCode1.ToArray());

            FlowHeadResult inboundProcess = new FlowHeadResult();
            if (flowHeadRuesultList.Count > 0)
            {
                inboundProcess = flowHeadRuesultList.First();
            }
            else
            {
                return "客户收货流程不一致，无法添加！";
            }

            //验证Load出货流程 与 直装所选流程 是否一致
            LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode).First();
            if (load.ProcessId != entityList[0].ProcessId)
            {
                return "Load出货流程与当前所选流程不一致，无法添加！";
            }

            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode);

            //验证 出库订单号是否已存在 和状态
            foreach (var entity in entityList)
            {
                if (result != "")
                {
                    break;
                }

                if (clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).Count() == 0)
                {
                    result = "客户不存在或有误：" + entity.ClientCode;
                    break;
                }

                List<OutBoundOrder> sqlCheck = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                where a.ClientCode == entity.ClientCode && a.WhCode == whCode && a.CustomerOutPoNumber == entity.CustomerOutPoNumber
                                                select a).ToList();
                if (sqlCheck.Count > 0)
                {
                    if ((sqlCheck.First().DSFLag ?? 0) == 0)
                    {
                        result = "非直装订单已存在：" + entity.ClientCode + "-" + entity.CustomerOutPoNumber;
                        break;
                    }
                    if (sqlCheck.First().StatusId != 5)
                    {
                        result = "直装订单状态有误：" + entity.ClientCode + "-" + entity.CustomerOutPoNumber;
                        break;
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            //批量导入
            foreach (var entity in entityList)
            {
                List<OutBoundOrder> sqlCheck = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                where a.ClientCode == entity.ClientCode && a.WhCode == whCode && a.CustomerOutPoNumber == entity.CustomerOutPoNumber
                                                select a).ToList();

                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                OutBoundOrder outBoundOrder = new OutBoundOrder();
                //1.首先生成 出库订单头 并且状态为已生成Load
                if (sqlCheck.Count == 0)
                {
                    outBoundOrder.WhCode = whCode;
                    outBoundOrder.ClientId = client.Id;
                    outBoundOrder.ClientCode = entity.ClientCode;

                    string sa = DI.IDGenerator.NewId;
                    outBoundOrder.OutPoNumber = "SA" + sa;
                    outBoundOrder.CustomerOutPoNumber = sa;
                    outBoundOrder.ProcessId = load.ProcessId;

                    var sql = from a in idal.IFlowDetailDAL.SelectAll()
                              where a.FlowHeadId == outBoundOrder.ProcessId
                              select a;
                    FlowDetail flowDetail = sql.Where(u => u.StatusId == 15).OrderBy(u => u.OrderId).First();
                    outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                    outBoundOrder.StatusId = flowDetail.StatusId;
                    outBoundOrder.StatusName = flowDetail.StatusName;
                    outBoundOrder.ReceiptId = "";
                    outBoundOrder.OrderSource = "WMS直装导入";
                    outBoundOrder.PlanOutTime = DateTime.Now;
                    outBoundOrder.LoadFlag = 1;
                    outBoundOrder.DSFLag = 1;
                    outBoundOrder.CreateUser = userName;
                    outBoundOrder.CreateDate = DateTime.Now;

                    idal.IOutBoundOrderDAL.Add(outBoundOrder);
                    idal.SaveChanges();
                }
                else
                {
                    outBoundOrder = sqlCheck.First();
                }

                //2.添加Load与订单关系
                LoadDetail loadDetail = new LoadDetail();
                loadDetail.LoadMasterId = load.Id;
                loadDetail.OutBoundOrderId = outBoundOrder.Id;
                idal.ILoadDetailDAL.Add(loadDetail);

                //3.添加预录入 及 出库单明细
                foreach (var item in entity.OutBoundOrderDetailModel)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();

                    #region //3.1 添加预录入明细
                    if (!string.IsNullOrEmpty(item.SoNumber))
                    {
                        InBoundSO inBoundSO = new InBoundSO();
                        //验证InBoundSO
                        List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == item.SoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);

                        if (listInBoundSO.Count == 0)
                        {
                            inBoundSO.WhCode = entity.WhCode;
                            inBoundSO.SoNumber = item.SoNumber;
                            inBoundSO.ClientCode = entity.ClientCode;
                            inBoundSO.ClientId = client.Id;              //添加新数据 必须赋予客户ID
                            inBoundSO.CreateUser = item.CreateUser;
                            inBoundSO.CreateDate = DateTime.Now;
                            idal.IInBoundSODAL.Add(inBoundSO);
                            idal.IInBoundSODAL.SaveChanges();
                        }
                        else
                        {
                            //存在，就获取
                            inBoundSO = listInBoundSO.First();
                        }

                        //添加InBoundOrder  
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.SoId == inBoundSO.Id);

                        if (listInBoundOrder.Count == 0)
                        {
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.SoId = inBoundSO.Id;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.ClientId = client.Id;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = "CFS";
                            inBoundOrder.ProcessId = inboundProcess.Id;
                            inBoundOrder.ProcessName = inboundProcess.FlowName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            idal.IInBoundOrderDAL.Add(inBoundOrder);   //不存在就新增
                        }
                        else
                        {
                            inBoundOrder = listInBoundOrder.First();
                        }
                    }
                    else
                    {
                        //添加InBoundOrder  
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientId == client.Id && u.SoId == null);

                        if (listInBoundOrder.Count == 0)
                        {
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.ClientId = client.Id;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = "DC";
                            inBoundOrder.ProcessId = inboundProcess.Id;
                            inBoundOrder.ProcessName = inboundProcess.FlowName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            idal.IInBoundOrderDAL.Add(inBoundOrder);   //不存在就新增
                        }
                        else
                        {
                            inBoundOrder = listInBoundOrder.First();
                        }
                    }

                    List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == client.Id).OrderBy(u => u.Id).ToList();
                    ItemMaster itemMaster = new ItemMaster();
                    if (listItemMaster.Count == 0)
                    {
                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                        itemMaster.WhCode = entity.WhCode;
                        itemMaster.AltItemNumber = item.AltItemNumber;
                        itemMaster.ClientId = client.Id;
                        itemMaster.ClientCode = entity.ClientCode;
                        itemMaster.Style1 = item.Style1 ?? "";
                        itemMaster.Style2 = item.Style2 ?? "";
                        itemMaster.Style3 = item.Style3 ?? "";
                        itemMaster.UnitFlag = 0;
                        if (item.UnitName == "" || item.UnitName == null)
                        {
                            itemMaster.UnitName = "none";
                        }
                        else
                        {
                            itemMaster.UnitName = item.UnitName;
                        }

                        itemMaster.CreateUser = item.CreateUser;
                        itemMaster.CreateDate = DateTime.Now;
                        idal.IItemMasterDAL.Add(itemMaster);   //款号不存在就新增
                    }
                    else
                    {
                        itemMaster = listItemMaster.First();
                    }

                    idal.IItemMasterDAL.SaveChanges();

                    List<InBoundOrderDetail> listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item.UnitId).ToList();

                    InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                    if (listInBoundOrderDetail.Count == 0)
                    {
                        inBoundOrderDetail.WhCode = entity.WhCode;
                        inBoundOrderDetail.PoId = inBoundOrder.Id;
                        inBoundOrderDetail.ItemId = itemMaster.Id;
                        inBoundOrderDetail.UnitId = item.UnitId;
                        if (itemMaster.UnitName == "" || itemMaster.UnitName == null)
                        {
                            inBoundOrderDetail.UnitName = "none";
                        }
                        else
                        {
                            inBoundOrderDetail.UnitName = itemMaster.UnitName;
                        }

                        inBoundOrderDetail.Qty = item.Qty;
                        inBoundOrderDetail.Weight = 0;
                        inBoundOrderDetail.CBM = 0;
                        inBoundOrderDetail.CreateUser = item.CreateUser;
                        inBoundOrderDetail.CreateDate = DateTime.Now;
                        idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                    }
                    else
                    {
                        inBoundOrderDetail = listInBoundOrderDetail.First();
                        int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;

                        List<OutBoundOrderDetail> checkdsqty = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                join b in idal.IOutBoundOrderDAL.SelectAll()
                                                                on a.OutBoundOrderId equals b.Id
                                                                where a.WhCode == inBoundOrderDetail.WhCode && b.StatusId != 40 && b.StatusId >= 15 && a.InBoundOrderDetailId == inBoundOrderDetail.Id && b.DSFLag == 1
                                                                select a).ToList();
                        if (checkdsqty.Count > 0)
                        {
                            int dsqty = checkdsqty.Sum(u => u.Qty);
                            int checkq = item.Qty + dsqty;
                            if (yuluruQty < checkq)
                            {
                                inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (checkq - yuluruQty);
                                inBoundOrderDetail.UpdateUser = item.CreateUser;
                                inBoundOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                            }
                        }
                        else
                        {
                            if (yuluruQty < item.Qty)
                            {
                                inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item.Qty - yuluruQty);
                                inBoundOrderDetail.UpdateUser = item.CreateUser;
                                inBoundOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                            }
                        }
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();
                    #endregion  预录入添加结束


                    //3.2 添加出库订单明细
                    List<OutBoundOrderDetail> listOutBoundOrderDetail = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.OutBoundOrderId == outBoundOrder.Id).ToList();

                    if (listOutBoundOrderDetail.Count == 0)
                    {
                        OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                        orderDetail.OutBoundOrderId = outBoundOrder.Id;
                        orderDetail.WhCode = entity.WhCode;
                        orderDetail.SoNumber = item.SoNumber;
                        orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                        orderDetail.AltItemNumber = item.AltItemNumber;
                        orderDetail.ItemId = itemMaster.Id;
                        orderDetail.UnitId = 0;
                        orderDetail.UnitName = itemMaster.UnitName;

                        orderDetail.Qty = item.Qty;
                        orderDetail.DSFLag = 1;         //直装Flag
                        orderDetail.InBoundOrderDetailId = inBoundOrderDetail.Id;   //预录入ID
                        orderDetail.LotNumber1 = item.LotNumber1;
                        orderDetail.LotNumber2 = item.LotNumber2;
                        orderDetail.CreateUser = item.CreateUser;
                        orderDetail.CreateDate = DateTime.Now;
                        idal.IOutBoundOrderDetailDAL.Add(orderDetail);
                    }
                    else
                    {
                        OutBoundOrderDetail orderDetail = listOutBoundOrderDetail.First();
                        orderDetail.Qty += item.Qty;
                        orderDetail.UpdateUser = item.CreateUser;
                        orderDetail.UpdateDate = DateTime.Now;
                        idal.IOutBoundOrderDetailDAL.UpdateBy(orderDetail, u => u.Id == orderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    }

                    idal.IOutBoundOrderDAL.SaveChanges();
                }
            }
            return "Y";
        }

        //直装订单删除
        public string DSLoadDetailDel(int id)
        {
            if (id == 0)
            {
                return "数据有误，请重新操作！";
            }
            //验证Load状态 
            LoadDetail loadDetail = idal.ILoadDetailDAL.SelectBy(u => u.Id == id).First();
            LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == loadDetail.LoadMasterId).First();
            if (loadMaster.Status0 != "U")
            {
                return "Load状态有误，请重新查询！";
            }

            OutBoundOrder eneity = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == loadDetail.OutBoundOrderId).First();
            if (eneity.DSFLag != 1)
            {
                return "订单非直装，请重新查询！";
            }

            //1.删除直装订单
            idal.IOutBoundOrderDAL.DeleteBy(u => u.Id == eneity.Id);
            idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.OutBoundOrderId == eneity.Id);

            //2.删除订单与Load关系
            idal.ILoadDetailDAL.DeleteBy(u => u.Id == id);
            idal.ILoadDetailDAL.SaveChanges();

            //3.如果Load只有一个订单 要删除Load
            List<LoadDetail> loadDetailList = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadMaster.Id);
            if (loadDetailList.Count == 0)
            {
                idal.ILoadMasterDAL.DeleteBy(u => u.Id == loadMaster.Id);
                idal.ILoadContainerExtendDAL.DeleteBy(u => u.WhCode == loadMaster.WhCode && u.LoadId == loadMaster.LoadId);
            }

            idal.ILoadDetailDAL.SaveChanges();
            return "Y";
        }

        //修改Load备注
        public string LoadMasterEditRemark(LoadMaster entity)
        {
            idal.ILoadMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Remark" });
            idal.SaveChanges();
            return "Y";
        }

        //出货操作单模版
        public string PrintOutTempalte(string whCode, string loadId)
        {
            List<CRTemplate> list = (from a in idal.ILoadMasterDAL.SelectAll()
                                     join b in idal.IFlowHeadDAL.SelectAll()
                                     on a.ProcessId equals b.Id
                                     where a.WhCode == whCode && a.LoadId == loadId
                                     join c in idal.ICRTemplateDAL.SelectAll()
                                     on new { A = b.WhCode, B = b.OutTemplate } equals new { A = c.WhCode, B = c.TemplateName } into temp1
                                     from c in temp1.DefaultIfEmpty()
                                     select c).ToList();
            if (list.Count == 0)
            {
                return "";
            }

            CRTemplate cr = list.First();

            return cr.Url + cr.TemplateName + ".aspx";
        }


        //查询释放异常列表
        public List<ReleaseLoadDetail> ReleaseLoadDetailList(LoadHuIdExtendSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReleaseLoadDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId == searchEntity.LoadId
                      select a;

            total = sql.Count();
            sql = sql.OrderBy(u => u.OutSoNumber).ThenBy(u => u.OutCustomerPoNumber).ThenBy(u => u.OutAltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        #region 装箱单功能管理
        //装箱单列表查询
        public List<LoadContainerResult> LoadContainerList(LoadContainerSearch searchEntity, string[] containerNumber, out int total, out string str)
        {
            var sql = (from a in idal.ILoadContainerExtendDAL.SelectAll()
                       join b in idal.IWhUserDAL.SelectAll()
                       on a.CreateUser equals b.UserName
                       join e in idal.ILoadContainerTypeDAL.SelectAll()
                            on a.ContainerType equals e.ContainerType into temp4
                       from e in temp4.DefaultIfEmpty()
                       join f in idal.ILoadMasterDAL.SelectAll()
                       on new { a.LoadId, a.WhCode } equals new { f.LoadId, f.WhCode } into temp6
                       from f in temp6.DefaultIfEmpty()
                       join g in idal.ILoadDetailDAL.SelectAll() on new { Id = f.Id } equals new { Id = (Int32)g.LoadMasterId } into g_join
                       from g in g_join.DefaultIfEmpty()
                       join h in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)g.OutBoundOrderId } equals new { OutBoundOrderId = h.Id } into h_join
                       from h in h_join.DefaultIfEmpty()

                       join c in idal.ILoadChargeDAL.SelectAll()
                       on new { a.LoadId, a.WhCode } equals new { c.LoadId, c.WhCode } into temp5
                       from c in temp5.DefaultIfEmpty()
                       join i in idal.IWhClientDAL.SelectAll()
                       on new { h.WhCode, h.ClientCode } equals new { i.WhCode, i.ClientCode } into temp7
                       from i in temp7.DefaultIfEmpty()
                       join j in idal.IFeeMasterDAL.SelectAll()
                         on new { a.LoadId, a.WhCode } equals new { j.LoadId, j.WhCode } into temp8
                       from j in temp8.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       select new LoadContainerResult
                       {
                           Id = a.Id,
                           Action1 = "",
                           Action2 = "",
                           Action3 = "",
                           Action4 = (j.LoadId ?? "") == "" ? "" : "已创建",
                           LoadMasterId = f.Id,
                           LoadId = a.LoadId ?? "",
                           ClientCode = (h.ClientCode ?? "") == "" ? a.ClientCode : h.ClientCode ?? "",
                           ChuCangFS = a.ChuCangFS,
                           Status0 =
                           f.Status0 == "U" ? "未释放" :
                           f.Status0 == "C" ? "已释放" : "",
                           Status1 =
                           f.Status1 == "U" ? "未备货" :
                           f.Status1 == "A" ? "正在备货" :
                           f.Status1 == "C" ? "完成备货" : null,
                           Status3 =
                           f.Status3 == "U" ? "未装箱" :
                           f.Status3 == "A" ? "正在装箱" :
                          f.Status3 == "C" ? "完成装箱" : null,
                           ShipStatus = f.ShipDate == null ? "未封箱" : "已封箱",
                           ProcessId = f.ProcessId,
                           ProcessName = f.ProcessName,
                           VesselName = a.VesselName,
                           VesselNumber = a.VesselNumber,
                           CarriageName = a.CarriageName,
                           ETD = a.ETD,
                           //ETDShow = a.ETD.ToString(),
                           ContainerType = a.ContainerType,
                           ContainerName = e.ContainerName,
                           Port = a.Port,
                           PortSuticase = a.PortSuitcase,
                           DeliveryPlace = a.DeliveryPlace,
                           BillNumber = a.BillNumber,
                           ContainerNumber = a.ContainerNumber,
                           SealNumber = a.SealNumber,
                           ContainerSource = a.ContainerSource,
                           CreateUserName = b.UserNameCN,
                           CreateUser = a.CreateUser,
                           CreateDate = a.CreateDate,
                           LoadContainerHuDetailId = a.Id,
                           SumQty = f.SumQty,
                           DSSumQty = f.DSSumQty,
                           EchQty = f.EchQty,
                           SumCBM = f.SumCBM,
                           SumWeight = f.SumWeight,
                           WeightFlag = a.WeightFlag == 0 ? "否" :
                           a.WeightFlag == 1 ? "是" : null,
                           Remark = f.Remark,
                           ReleaseDate = f.ReleaseDate,
                           BeginPickDate = f.BeginPickDate,
                           EndPickDate = f.EndPickDate,
                           BeginPackDate = f.BeginPackDate,
                           EndPackDate = f.EndPackDate,
                           BeginSortDate = f.BeginSortDate,
                           EndSortDate = f.EndSortDate,
                           ShipDate = f.ShipDate,
                           LoadChargeStatus = c.Status ?? "",
                           ClientContractNameOut = i.ContractNameOut,
                           PlanQty = a.PlanQty,
                           ShippingOrigin = a.ShippingOrigin
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ChuCangFS))
                sql = sql.Where(u => u.ChuCangFS == searchEntity.ChuCangFS);
            if (!string.IsNullOrEmpty(searchEntity.BillNumber))
                sql = sql.Where(u => u.BillNumber == searchEntity.BillNumber);
            if (containerNumber != null)
                sql = sql.Where(u => containerNumber.Contains(u.ContainerNumber));
            if (!string.IsNullOrEmpty(searchEntity.SealNumber))
                sql = sql.Where(u => u.SealNumber == searchEntity.SealNumber);

            if (!string.IsNullOrEmpty(searchEntity.VesselName))
                sql = sql.Where(u => u.VesselName == searchEntity.VesselName);

            if (searchEntity.BeginETD != null)
            {
                sql = sql.Where(u => u.ETD >= searchEntity.BeginETD);
            }
            if (searchEntity.EndETD != null)
            {
                sql = sql.Where(u => u.ETD < searchEntity.EndETD);
            }
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndCreateDate);
            }
            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
            {
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);
            }

            if (!string.IsNullOrEmpty(searchEntity.Status0))
                sql = sql.Where(u => u.Status0 == searchEntity.Status0);
            if (!string.IsNullOrEmpty(searchEntity.Status1))
                sql = sql.Where(u => u.Status1 == searchEntity.Status1);
            if (!string.IsNullOrEmpty(searchEntity.Status3))
                sql = sql.Where(u => u.Status3 == searchEntity.Status3);
            if (!string.IsNullOrEmpty(searchEntity.ShipStatus))
                sql = sql.Where(u => u.ShipStatus == searchEntity.ShipStatus);

            if (!string.IsNullOrEmpty(searchEntity.LoadChargeStatus))
            {
                if (searchEntity.LoadChargeStatus == "C")
                {
                    sql = sql.Where(u => u.LoadChargeStatus == searchEntity.LoadChargeStatus);
                }
                else
                {
                    sql = sql.Where(u => (u.ClientContractNameOut ?? "") != "" && u.LoadChargeStatus != "C");
                }
            }

            List<LoadContainerResult> list1 = sql.ToList();

            List<LoadContainerResult> list = new List<LoadContainerResult>();
            foreach (var item in list1)
            {
                if (list.Where(u => u.Id == item.Id).Count() == 0)
                {
                    LoadContainerResult loadMaster = new LoadContainerResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Action3 = "";
                    loadMaster.Action4 = item.Action4;
                    loadMaster.Id = item.Id;
                    loadMaster.LoadMasterId = item.LoadMasterId;
                    loadMaster.ClientCode = item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId ?? "";
                    loadMaster.ChuCangFS = item.ChuCangFS;

                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;

                    loadMaster.Status3 = item.Status3;
                    loadMaster.ShipStatus = item.ShipStatus;

                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;

                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";

                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.ContainerName = item.ContainerName ?? "";

                    if (item.ETD != null)
                    {
                        loadMaster.ETDShow = Convert.ToDateTime(item.ETD).ToString("d");
                    }
                    loadMaster.VesselName = item.VesselName ?? "";
                    loadMaster.VesselNumber = item.VesselNumber ?? "";
                    loadMaster.CarriageName = item.CarriageName ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.PortSuticase = item.PortSuticase ?? "";

                    loadMaster.DeliveryPlace = item.DeliveryPlace ?? "";
                    loadMaster.BillNumber = item.BillNumber ?? "";

                    loadMaster.ContainerSource = item.ContainerSource;
                    loadMaster.CreateUserName = item.CreateUserName;
                    loadMaster.LoadContainerHuDetailId = item.LoadContainerHuDetailId;

                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;

                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    loadMaster.WeightFlag = item.WeightFlag;

                    loadMaster.ReleaseDate = item.ReleaseDate;
                    loadMaster.BeginPickDate = item.BeginPickDate;
                    loadMaster.EndPickDate = item.EndPickDate;
                    loadMaster.BeginPackDate = item.BeginPackDate;
                    loadMaster.EndPackDate = item.EndPackDate;
                    loadMaster.BeginSortDate = item.BeginSortDate;
                    loadMaster.EndSortDate = item.EndSortDate;
                    loadMaster.ShipDate = item.ShipDate;
                    loadMaster.LoadChargeStatus = item.LoadChargeStatus;
                    loadMaster.ClientContractNameOut = item.ClientContractNameOut ?? "";
                    loadMaster.PlanQty = item.PlanQty;
                    loadMaster.ShippingOrigin = item.ShippingOrigin;

                    list.Add(loadMaster);
                }
                else
                {
                    LoadContainerResult getModel = list.Where(u => u.Id == item.Id).First();
                    list.Remove(getModel);

                    LoadContainerResult loadMaster = new LoadContainerResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Action3 = "";
                    loadMaster.Action4 = item.Action4;
                    loadMaster.Id = item.Id;
                    loadMaster.LoadMasterId = item.LoadMasterId;
                    loadMaster.ClientCode = getModel.ClientCode + "," + item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId ?? "";
                    loadMaster.ChuCangFS = item.ChuCangFS;

                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;
                    loadMaster.Status3 = item.Status3;
                    loadMaster.ShipStatus = item.ShipStatus;

                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;

                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";

                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.ContainerName = item.ContainerName ?? "";

                    if (item.ETD != null)
                    {
                        loadMaster.ETDShow = Convert.ToDateTime(item.ETD).ToString("d");
                    }

                    loadMaster.VesselName = item.VesselName ?? "";
                    loadMaster.VesselNumber = item.VesselNumber ?? "";
                    loadMaster.CarriageName = item.CarriageName ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.PortSuticase = item.PortSuticase ?? "";
                    loadMaster.DeliveryPlace = item.DeliveryPlace ?? "";
                    loadMaster.BillNumber = item.BillNumber ?? "";

                    loadMaster.ContainerSource = item.ContainerSource;
                    loadMaster.CreateUserName = item.CreateUserName;
                    loadMaster.LoadContainerHuDetailId = item.LoadContainerHuDetailId;

                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;

                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    loadMaster.WeightFlag = item.WeightFlag;

                    loadMaster.ReleaseDate = item.ReleaseDate;
                    loadMaster.BeginPickDate = item.BeginPickDate;
                    loadMaster.EndPickDate = item.EndPickDate;
                    loadMaster.BeginPackDate = item.BeginPackDate;
                    loadMaster.EndPackDate = item.EndPackDate;
                    loadMaster.BeginSortDate = item.BeginSortDate;
                    loadMaster.EndSortDate = item.EndSortDate;
                    loadMaster.ShipDate = item.ShipDate;
                    loadMaster.LoadChargeStatus = item.LoadChargeStatus;
                    loadMaster.ClientContractNameOut = item.ClientContractNameOut;
                    loadMaster.PlanQty = item.PlanQty;
                    loadMaster.ShippingOrigin = item.ShippingOrigin;

                    list.Add(loadMaster);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                list = list.Where(u => u.ClientCode.Contains(searchEntity.ClientCode)).ToList();
            }

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                list = list.Where(u => u.ClientCode != null).Where(u => u.ClientCode.Contains(searchEntity.ClientCode)).ToList();
            }

            total = list.Count;

            str = "";
            if (total > 0)
            {
                str = "{\"总立方\":\"" + list.Sum(u => u.SumCBM).ToString() + "\"}";
            }

            list = list.OrderBy(u => u.Id).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //装箱单创建人下拉列表
        public List<CreateUserResult> CreateUserSelect(string whCode)
        {
            DateTime date = DateTime.Now.AddDays(-90);

            var sql = (from a in idal.ILoadContainerExtendDAL.SelectAll()
                       join b in idal.IWhUserDAL.SelectAll()
                       on a.CreateUser equals b.UserName into temp1
                       from ab in temp1.DefaultIfEmpty()
                       where a.WhCode == whCode && a.CreateDate >= date
                       select new CreateUserResult
                       {
                           CreateUser = a.CreateUser,
                           CreateUserName = ab.UserNameCN
                       }).Distinct().ToList();

            return sql;
        }

        //添加装箱单
        public LoadContainerExtend LoadContainerAdd(LoadContainerExtend entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.ILoadContainerExtendDAL.Add(entity);
            idal.SaveChanges();
            return entity;
        }

        //修改箱单信息
        public string LoadContainerEdit(LoadContainerExtend entity)
        {
            //修改箱单时 验证是否已生成了Load且状态为未释放
            List<LoadContainerExtend> list = idal.ILoadContainerExtendDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Where(u => (u.LoadId ?? "") != "").Count() > 0)
            {
                LoadContainerExtend first = list.First();
                List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == first.WhCode && u.LoadId == first.LoadId);
                if (loadMasterList.Count > 0)
                {
                    if (loadMasterList.Where(u => u.Status0 != "U").Count() > 0)
                    {
                        return "Load已释放，无法再修改信息！";
                    }
                }
            }

            entity.UpdateDate = DateTime.Now;
            idal.ILoadContainerExtendDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "VesselName", "VesselNumber", "CarriageName", "ETD", "ContainerType", "Port", "DeliveryPlace", "BillNumber", "ContainerNumber", "SealNumber", "UpdateUser", "UpdateDate" });
            idal.SaveChanges();
            return "Y";
        }

        //装箱单查询库存  按SO查询
        public List<LoadContainerHuDetailResult> LoadContainerHuDetailList(LoadContainerHuDetailSearch searchEntity, string[] soList, out int total, out string str)
        {
            var sql = (from a in (
                      (from a0 in idal.IHuDetailDAL.SelectAll()
                       where
                        (a0.SoNumber ?? "") != ""
                       group a0 by new
                       {
                           a0.WhCode,
                           a0.ClientCode,
                           a0.SoNumber
                       } into g
                       select new
                       {
                           g.Key.WhCode,
                           g.Key.ClientCode,
                           g.Key.SoNumber,
                           Qty = g.Sum(p => p.Qty),
                           PlanQty = g.Sum(p => (p.PlanQty ?? 0)),
                           acQty = g.Sum(p => p.Qty),

                           CBM = g.Sum(p => (p.Qty * p.Length * p.Width * p.Height)),
                           InvDate = g.Min(p => p.ReceiptDate)
                       }))
                       join b in (
                           (from loadcontainerextendhudetail in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                            join loadcontainerextend in idal.ILoadContainerExtendDAL.SelectAll()
                            on loadcontainerextendhudetail.LoadContainerId equals loadcontainerextend.Id into temp1
                            from loadcontainerextend in temp1.DefaultIfEmpty()
                            group loadcontainerextendhudetail by new
                            {
                                loadcontainerextendhudetail.WhCode,
                                loadcontainerextendhudetail.ClientCode,
                                loadcontainerextendhudetail.SoNumber,
                                loadcontainerextend.ContainerNumber
                            } into g
                            select new
                            {
                                g.Key.WhCode,
                                g.Key.ClientCode,
                                g.Key.SoNumber,
                                g.Key.ContainerNumber,
                                Qty1 = g.Sum(p => p.Qty),
                                CBM1 = g.Sum(p => p.Qty * (p.Length ?? 0) * (p.Width ?? 0) * (p.Height ?? 0))
                            }))
                             on new { a.WhCode, a.ClientCode, a.SoNumber }
                         equals new { b.WhCode, b.ClientCode, b.SoNumber } into b_join
                       from b in b_join.DefaultIfEmpty()
                       where
                         a.Qty != 0 && a.WhCode == searchEntity.WhCode
                       orderby
                         a.ClientCode,
                         a.SoNumber
                       select new LoadContainerHuDetailResult
                       {
                           Action = "",
                           ClientCode = a.ClientCode,
                           SoNumber = a.SoNumber,
                           Qty = a.Qty,
                           PlanQty = a.PlanQty,
                           AcQty = a.acQty,
                           SelectQty = b.Qty1,
                           CBM = a.CBM,
                           SelectCBM = b.CBM1,
                           InvDate = a.InvDate,
                           ContainerNumber = b.ContainerNumber,
                           sequence = 9999999
                       });

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));

            if (soList.Count() > 0)
                sql = sql.Where(u => soList.Contains(u.SoNumber));

            //获得库存天数
            List<LoadContainerHuDetailResult> list1 = new List<LoadContainerHuDetailResult>();
            foreach (var item in sql)
            {
                TimeSpan d3 = DateTime.Now.Subtract(Convert.ToDateTime(item.InvDate == null ? DateTime.Now : item.InvDate));
                item.Date = d3.Days;
                list1.Add(item);
            }

            //根据客服输入的SO顺序 查询库存后排序
            if (soList.Count() > 0)
            {
                int count = 1;
                foreach (var item in soList)
                {
                    if (list1.Where(u => u.SoNumber == item).Count() > 0)
                    {
                        LoadContainerHuDetailResult oldload = list1.Where(u => u.SoNumber == item).First();
                        list1.Remove(oldload);

                        LoadContainerHuDetailResult newload = oldload;
                        newload.sequence = count;
                        list1.Add(newload);
                        count++;
                    }
                }
            }

            List<LoadContainerHuDetailResult> list = new List<LoadContainerHuDetailResult>();
            foreach (var item in list1.OrderBy(u => u.sequence))
            {
                if (list.Where(u => u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber).Count() == 0)
                {
                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Action = item.Action;
                    entity.ClientCode = item.ClientCode;
                    entity.SoNumber = item.SoNumber;
                    entity.Qty = item.Qty;
                    entity.PlanQty = item.PlanQty;
                    entity.AcQty = item.AcQty - (item.SelectQty ?? 0);
                    entity.SelectQty = item.SelectQty;
                    entity.CBM = item.CBM - (item.SelectCBM ?? 0);
                    entity.SelectCBM = item.SelectCBM;
                    entity.Date = item.Date;
                    entity.ContainerNumber = item.ContainerNumber;
                    list.Add(entity);
                }
                else
                {
                    LoadContainerHuDetailResult getModel = list.Where(u => u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber).First();
                    list.Remove(getModel);

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Action = item.Action;
                    entity.ClientCode = item.ClientCode;
                    entity.SoNumber = item.SoNumber;
                    entity.Qty = item.Qty;
                    entity.PlanQty = item.PlanQty;
                    entity.AcQty = item.AcQty - (item.SelectQty ?? 0) - (getModel.SelectQty ?? 0);
                    entity.SelectQty = getModel.SelectQty + item.SelectQty;
                    entity.CBM = item.CBM - (item.SelectCBM ?? 0) - (getModel.SelectCBM ?? 0);
                    entity.SelectCBM = getModel.SelectCBM + item.SelectCBM;
                    entity.Date = item.Date;
                    entity.ContainerNumber = getModel.ContainerNumber + "," + item.ContainerNumber;
                    list.Add(entity);
                }
            }

            total = list.Count;
            str = "";
            if (total > 0)
            {
                str = "{\"库存数量\":\"" + list.Sum(u => u.Qty).ToString() + "\",\"可用库存\":\"" + list.Sum(u => u.AcQty).ToString() + "\"}";
            }
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //装箱单查询库存  按SOPOSKU查询
        public List<LoadContainerHuDetailResult> LoadContainerHuDetailListByPOSKU(LoadContainerHuDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] style1List, string[] style2List, out int total, out string str)
        {
            if (soList.Count() > 0)
            {
                soList = soList.Where(u => u != "").ToArray();
            }

            if (poList.Count() > 0)
            {
                poList = poList.Where(u => u != "").ToArray();
            }


            if (skuList.Count() > 0)
            {
                skuList = skuList.Where(u => u != "").ToArray();
            }


            if (style1List.Count() > 0)
            {
                style1List = style1List.Where(u => u != "").ToArray();
            }


            if (style2List.Count() > 0)
            {
                style2List = style2List.Where(u => u != "").ToArray();
            }


            var sql = (from a in (
                      (from a0 in idal.IHuDetailDAL.SelectAll()
                       join b0 in idal.IItemMasterDAL.SelectAll()
                       on a0.ItemId equals b0.Id
                       where (a0.SoNumber ?? "") != ""
                       group a0 by new
                       {
                           a0.WhCode,
                           a0.ClientCode,
                           a0.SoNumber,
                           a0.CustomerPoNumber,
                           a0.AltItemNumber,
                           a0.ItemId,
                           b0.Style1,
                           b0.Style2
                       } into g
                       select new
                       {
                           g.Key.WhCode,
                           g.Key.ClientCode,
                           g.Key.SoNumber,
                           g.Key.CustomerPoNumber,
                           g.Key.AltItemNumber,
                           g.Key.ItemId,
                           g.Key.Style1,
                           g.Key.Style2,
                           Qty = g.Sum(p => p.Qty),
                           PlanQty = g.Sum(p => (p.PlanQty ?? 0)),
                           acQty = g.Sum(p => p.Qty),
                           InvDate = g.Min(p => p.ReceiptDate)
                       }))
                       join b in (
                           (from loadcontainerextendhudetail in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                            join loadcontainerextend in idal.ILoadContainerExtendDAL.SelectAll()
                            on loadcontainerextendhudetail.LoadContainerId equals loadcontainerextend.Id into temp1
                            from loadcontainerextend in temp1.DefaultIfEmpty()
                            group loadcontainerextendhudetail by new
                            {
                                loadcontainerextendhudetail.WhCode,
                                loadcontainerextendhudetail.ClientCode,
                                loadcontainerextendhudetail.SoNumber,
                                loadcontainerextendhudetail.CustomerPoNumber,
                                loadcontainerextendhudetail.AltItemNumber,
                                loadcontainerextendhudetail.ItemId,
                                loadcontainerextend.ContainerNumber
                            } into g
                            select new
                            {
                                g.Key.WhCode,
                                g.Key.ClientCode,
                                g.Key.SoNumber,
                                g.Key.CustomerPoNumber,
                                g.Key.AltItemNumber,
                                g.Key.ItemId,
                                g.Key.ContainerNumber,
                                Qty1 = g.Sum(p => p.Qty)
                            }))
                             on new { a.WhCode, a.ClientCode, a.SoNumber, a.CustomerPoNumber, a.AltItemNumber, a.ItemId }
                         equals new { b.WhCode, b.ClientCode, b.SoNumber, b.CustomerPoNumber, b.AltItemNumber, b.ItemId } into b_join
                       from b in b_join.DefaultIfEmpty()
                       where
                         a.Qty != 0 && a.WhCode == searchEntity.WhCode
                       orderby
                         a.ClientCode,
                         a.SoNumber,
                         a.CustomerPoNumber,
                         a.AltItemNumber
                       select new LoadContainerHuDetailResult
                       {
                           Action = "",
                           ClientCode = a.ClientCode,
                           SoNumber = a.SoNumber,
                           CustomerPoNumber = a.CustomerPoNumber,
                           AltItemNumber = a.AltItemNumber,
                           Style1 = a.Style1 ?? "",
                           Style2 = a.Style2 ?? "",
                           Qty = a.Qty,
                           PlanQty = a.PlanQty,
                           AcQty = a.acQty,
                           SelectQty = b.Qty1,
                           InvDate = a.InvDate,
                           ContainerNumber = b.ContainerNumber,
                           ItemId = a.ItemId,
                           sequence = 99999,
                           Stragg = a.SoNumber + "-" + a.CustomerPoNumber + "-" + a.AltItemNumber + "-" + a.Style1 ?? "" + "-" + a.Style2 ?? ""
                       });

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            if (soList.Count() > 0)
                sql = sql.Where(u => soList.Contains(u.SoNumber));
            if (poList.Count() > 0)
                sql = sql.Where(u => poList.Contains(u.CustomerPoNumber));
            if (skuList.Count() > 0)
                sql = sql.Where(u => skuList.Contains(u.AltItemNumber));
            if (style1List.Count() > 0)
                sql = sql.Where(u => style1List.Contains(u.Style1));
            if (style2List.Count() > 0)
                sql = sql.Where(u => style2List.Contains(u.Style2));

            //获得库存天数
            List<LoadContainerHuDetailResult> list1 = new List<LoadContainerHuDetailResult>();

            List<int?> itemIdList = new List<int?>();
            foreach (var item in sql)
            {
                TimeSpan d3 = DateTime.Now.Subtract(Convert.ToDateTime(item.InvDate == null ? DateTime.Now : item.InvDate));
                item.Date = d3.Days;
                itemIdList.Add(item.ItemId);
                list1.Add(item);
            }

            #region 按查询的SO PO SKU STYLE排序

            List<LoadContainerHuDetailResult> list3 = new List<LoadContainerHuDetailResult>();

            List<LoadContainerHuDetailResult> list2 = new List<LoadContainerHuDetailResult>();

            if (soList.Count() > 0 && poList.Count() > 0 && skuList.Count() > 0 && style1List.Count() > 0 && style2List.Count() > 0)
            {
                if (soList.Count() == poList.Count() && poList.Count() == skuList.Count() && skuList.Count() == style1List.Count() && style1List.Count() == style2List.Count())
                {
                    for (int i = 0; i < soList.Count(); i++)
                    {
                        LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                        entity.Stragg = soList[i] + "-" + poList[i] + "-" + skuList[i] + "-" + style1List[i] + "-" + style2List[i];
                        list2.Add(entity);
                    }
                }
                else
                {
                    foreach (var item in soList)
                    {
                        string so = item;
                        foreach (var item1 in poList)
                        {
                            string po = item1;
                            foreach (var item2 in skuList)
                            {
                                string sku = item2;
                                foreach (var item3 in style1List)
                                {
                                    string style = item3;
                                    foreach (var item4 in style2List)
                                    {
                                        string style2 = item4;
                                        string stragg = so + "-" + po + "-" + sku + "-" + style + "-" + style2;

                                        LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                                        entity.Stragg = stragg;
                                        list2.Add(entity);
                                    }

                                }
                            }
                        }
                    }

                    list3 = list1;
                }
            }
            else if (soList.Count() > 0 && poList.Count() > 0 && skuList.Count() > 0 && style1List.Count() > 0)
            {
                if (soList.Count() == poList.Count() && poList.Count() == skuList.Count() && skuList.Count() == style1List.Count())
                {
                    for (int i = 0; i < soList.Count(); i++)
                    {
                        LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                        entity.Stragg = soList[i] + "-" + poList[i] + "-" + skuList[i] + "-" + style1List[i];
                        list2.Add(entity);
                    }
                }
                else
                {
                    foreach (var item in soList)
                    {
                        string so = item;
                        foreach (var item1 in poList)
                        {
                            string po = item1;
                            foreach (var item2 in skuList)
                            {
                                string sku = item2;
                                foreach (var item3 in style1List)
                                {
                                    string style = item3;
                                    string stragg = so + "-" + po + "-" + sku + "-" + style;

                                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                                    entity.Stragg = stragg;
                                    list2.Add(entity);
                                }
                            }
                        }
                    }
                    list3 = list1;
                }
            }
            else if (soList.Count() > 0 && poList.Count() > 0 && skuList.Count() > 0)
            {
                if (soList.Count() == poList.Count() && poList.Count() == skuList.Count())
                {
                    for (int i = 0; i < soList.Count(); i++)
                    {
                        LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                        entity.Stragg = soList[i] + "-" + poList[i] + "-" + skuList[i];
                        list2.Add(entity);
                    }
                }
                else
                {
                    foreach (var item in soList)
                    {
                        string so = item;
                        foreach (var item1 in poList)
                        {
                            string po = item1;
                            foreach (var item2 in skuList)
                            {
                                string sku = item2;

                                string stragg = so + "-" + po + "-" + sku;

                                LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                                entity.Stragg = stragg;
                                list2.Add(entity);

                            }
                        }
                    }
                }

                list3 = list1;
            }
            else if (soList.Count() > 0 && poList.Count() > 0)
            {
                if (soList.Count() == poList.Count())
                {
                    for (int i = 0; i < soList.Count(); i++)
                    {
                        LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                        entity.Stragg = soList[i] + "-" + poList[i];
                        list2.Add(entity);
                    }
                }
                else
                {
                    foreach (var item in soList)
                    {
                        string so = item;
                        foreach (var item1 in poList)
                        {
                            string po = item1;

                            string stragg = so + "-" + po;

                            LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                            entity.Stragg = stragg;
                            list2.Add(entity);
                        }
                    }
                }

                list3 = list1;
            }
            else if (soList.Count() > 0)
            {
                string stragg = "";
                foreach (var item in soList)
                {
                    stragg = item;

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Stragg = stragg;
                    list2.Add(entity);
                }
                list3 = list1;
            }
            else if (poList.Count() > 0)
            {
                string stragg = "";
                foreach (var item in poList)
                {
                    stragg = item;

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Stragg = stragg;
                    list2.Add(entity);
                }
                list3 = list1;
            }
            else if (skuList.Count() > 0)
            {
                string stragg = "";
                foreach (var item in skuList)
                {
                    stragg = item;

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Stragg = stragg;
                    list2.Add(entity);
                }
                list3 = list1;
            }
            else if (style1List.Count() > 0)
            {
                string stragg = "";
                foreach (var item in style1List)
                {
                    stragg = item;

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Stragg = stragg;
                    list2.Add(entity);
                }
                list3 = list1;
            }
            else if (style2List.Count() > 0)
            {
                string stragg = "";
                foreach (var item in style2List)
                {
                    stragg = item;

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Stragg = stragg;
                    list2.Add(entity);
                }
                list3 = list1;
            }
            else
            {
                list3 = list1;
            }

            if (list2.Count > 0)
            {
                int count = 1;
                foreach (var item in list2)
                {
                    if (list1.Where(u => u.Stragg.Contains(item.Stragg)).Count() > 0)
                    {
                        LoadContainerHuDetailResult oldEntity = list1.Where(u => u.Stragg.Contains(item.Stragg)).First();
                        list3.Remove(oldEntity);

                        LoadContainerHuDetailResult newEntity = oldEntity;
                        newEntity.sequence = count;
                        count++;

                        list3.Add(newEntity);
                    }
                }
            }

            #endregion

            List<LoadContainerHuDetailResult> list = new List<LoadContainerHuDetailResult>();
            foreach (var item in list3.OrderBy(u => u.sequence))
            {
                if (list.Where(u => u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId).Count() == 0)
                {
                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Action = item.Action;
                    entity.ClientCode = item.ClientCode;
                    entity.SoNumber = item.SoNumber;
                    entity.CustomerPoNumber = item.CustomerPoNumber;
                    entity.AltItemNumber = item.AltItemNumber;
                    entity.ItemId = item.ItemId;
                    entity.Style1 = item.Style1;
                    entity.Style2 = item.Style2;
                    entity.Qty = item.Qty;
                    entity.PlanQty = item.PlanQty;
                    entity.AcQty = item.AcQty - (item.SelectQty ?? 0);
                    entity.SelectQty = item.SelectQty;
                    entity.Date = item.Date;
                    entity.ContainerNumber = item.ContainerNumber;
                    list.Add(entity);
                }
                else
                {
                    LoadContainerHuDetailResult getModel = list.Where(u => u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId).First();
                    list.Remove(getModel);

                    LoadContainerHuDetailResult entity = new LoadContainerHuDetailResult();
                    entity.Action = item.Action;
                    entity.ClientCode = item.ClientCode;
                    entity.SoNumber = item.SoNumber;
                    entity.CustomerPoNumber = item.CustomerPoNumber;
                    entity.AltItemNumber = item.AltItemNumber;
                    entity.ItemId = item.ItemId;
                    entity.Style1 = item.Style1;
                    entity.Style2 = item.Style2;
                    entity.Qty = item.Qty;
                    entity.PlanQty = item.PlanQty;
                    entity.AcQty = item.AcQty - (item.SelectQty ?? 0) - (getModel.SelectQty ?? 0);
                    entity.SelectQty = getModel.SelectQty + item.SelectQty;
                    entity.Date = item.Date;
                    entity.ContainerNumber = getModel.ContainerNumber + "," + item.ContainerNumber;
                    list.Add(entity);
                }
            }

            total = list.Count;

            str = "";
            if (total > 0)
            {
                str = "{\"库存数量\":\"" + list.Sum(u => u.Qty).ToString() + "\",\"可用库存\":\"" + list.Sum(u => u.AcQty).ToString() + "\"}";
            }

            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //验证所选库存的客户出货流程是否一致
        public List<FlowHeadResult> CheckLoadContainerClientCodeRule(string whCode, List<WhClient> client)
        {
            List<FlowHeadResult> result = new List<FlowHeadResult>();

            List<FlowHeadResult> sql = (from a in idal.IWhClientDAL.SelectAll()
                                        join b in idal.IR_Client_FlowRuleDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                        from b in b_join.DefaultIfEmpty()
                                        join c in idal.IFlowHeadDAL.SelectAll() on new { BusinessFlowGroupId = (Int32)b.BusinessFlowGroupId } equals new { BusinessFlowGroupId = c.Id } into c_join
                                        from c in c_join.DefaultIfEmpty()
                                        where
                                          a.WhCode == whCode && c.Type == "OutBound"
                                        select new FlowHeadResult
                                        {
                                            Remark = a.ClientCode,
                                            FlowName = c.FlowName,
                                            Id = c.Id
                                        }).ToList();

            string[] clientCode = (from a in client
                                   select a.ClientCode).Distinct().ToArray();

            sql = sql.Where(u => clientCode.Contains(u.Remark)).ToList();
            if (sql.Count == 0)
            {
                return result;
            }
            else
            {
                //得到第一个客户的流程
                List<FlowHeadResult> first = sql.Where(u => u.Remark == clientCode[0]).ToList();
                if (clientCode.Length == 1)
                {
                    result = first;
                    return result;
                }

                //循环第二个客户以后的流程
                for (int i = 1; i < clientCode.Length; i++)
                {
                    List<FlowHeadResult> check = sql.Where(u => u.Remark == clientCode[i]).ToList();
                    if (i == 1)
                    {
                        //如果第一个客户的流程多余第二个客户的流程
                        if (first.Count >= check.Count)
                        {
                            foreach (var item in first)
                            {
                                if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in check)
                            {
                                if (first.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        //如果第一个客户和第二个客户 没有共同的流程 直接断开循环 返回Null
                        if (result.Count == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        List<FlowHeadResult> resulttemp = new List<FlowHeadResult>();

                        foreach (var item in result)
                        {
                            if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                            {
                                FlowHeadResult f = new FlowHeadResult();
                                f.FlowName = item.FlowName;
                                f.Id = item.Id;
                                resulttemp.Add(f);
                            }
                        }

                        if (resulttemp.Count == 0)
                        {
                            result.Clear();
                        }
                        else
                        {
                            if (resulttemp.Count < result.Count)
                            {
                                result = resulttemp;
                            }
                        }
                    }
                }

                return result;
            }
        }

        //装箱单选择库存生成明细
        public string LoadContainerHuDetailAdd(string whCode, int loadContainerId, string[] soList, string[] clientCodeList, int processId, string processName, string userName)
        {
            lock (o)
            {
                //验证所选客户的 货代是否一致
                var checkAgent = (from a in idal.IWhClientDAL.SelectAll()
                                  join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                  from b in b_join.DefaultIfEmpty()
                                  join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                  from c in c_join.DefaultIfEmpty()
                                  where a.WhCode == whCode && clientCodeList.Contains(a.ClientCode)
                                  select new
                                  {
                                      AgentCode = c.AgentCode
                                  }).Distinct();
                if (checkAgent.Count() > 1)
                {
                    return "所选库存货代不一致，无法添加！";
                }

                //验证是否已选择库存，所选库存的客户 与 现选择的客户 比较货代
                List<LoadContainerExtendHuDetail> checkCount = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);
                if (checkCount.Count > 0)
                {
                    string[] checkClientCodeList = (from a in checkCount select a.ClientCode).Distinct().ToArray();

                    var checkAgent1 = (from a in idal.IWhClientDAL.SelectAll()
                                       join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                       from b in b_join.DefaultIfEmpty()
                                       join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                       from c in c_join.DefaultIfEmpty()
                                       where a.WhCode == whCode && checkClientCodeList.Contains(a.ClientCode)
                                       select new
                                       {
                                           AgentCode = c.AgentCode
                                       }).Distinct();
                    if (checkAgent1.Count() > 0)
                    {
                        if (checkAgent1.First().AgentCode != checkAgent.First().AgentCode)
                        {
                            return "所选库存与已选择的库存货代不一致，无法添加！";
                        }
                    }
                }

                List<LoadContainerHuDetailResult1> sql =
                             (from a in (
                                    (from a in (
                                      (from hudetail in idal.IHuDetailDAL.SelectAll()
                                       where
                                        hudetail.SoNumber != "" && hudetail.WhCode == whCode
                                       group hudetail by new
                                       {
                                           hudetail.WhCode,
                                           hudetail.ClientCode,
                                           hudetail.ClientId,
                                           hudetail.SoNumber,
                                           hudetail.CustomerPoNumber,
                                           hudetail.AltItemNumber,
                                           hudetail.ItemId,
                                           hudetail.UnitId,
                                           hudetail.UnitName,
                                           hudetail.LotNumber1,
                                           hudetail.LotNumber2,
                                           hudetail.LotDate
                                       } into g
                                       select new
                                       {
                                           g.Key.WhCode,
                                           g.Key.ClientCode,
                                           ClientId = g.Key.ClientId,
                                           g.Key.SoNumber,
                                           g.Key.CustomerPoNumber,
                                           g.Key.AltItemNumber,
                                           ItemId = g.Key.ItemId,
                                           UnitId = g.Key.UnitId,
                                           g.Key.UnitName,
                                           Qty = g.Sum(p => p.Qty),
                                           Length = g.Average(p => p.Length),
                                           Width = g.Average(p => p.Width),
                                           Height = g.Average(p => p.Height),
                                           Weight = g.Average(p => p.Weight),
                                           g.Key.LotNumber1,
                                           g.Key.LotNumber2,
                                           LotDate = (DateTime?)g.Key.LotDate
                                       }))
                                     join b in (from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                                                where a.WhCode == whCode
                                                group a by new
                                                {
                                                    a.ClientCode,
                                                    a.WhCode,
                                                    a.SoNumber,
                                                    a.CustomerPoNumber,
                                                    a.ItemId,
                                                    a.UnitName,
                                                    a.LotNumber1,
                                                    a.LotNumber2
                                                } into h
                                                select new
                                                {
                                                    h.Key.ClientCode,
                                                    h.Key.WhCode,
                                                    h.Key.SoNumber,
                                                    h.Key.CustomerPoNumber,
                                                    h.Key.ItemId,
                                                    h.Key.UnitName,
                                                    h.Key.LotNumber1,
                                                    h.Key.LotNumber2,
                                                    Qty = h.Sum(u => u.Qty)
                                                }
                                                 )
                                           on new { a.ClientCode, a.WhCode, a.SoNumber, a.CustomerPoNumber, ItemId = a.ItemId, a.UnitName, a.LotNumber1, a.LotNumber2 }
                                       equals new { b.ClientCode, b.WhCode, b.SoNumber, b.CustomerPoNumber, ItemId = b.ItemId, b.UnitName, b.LotNumber1, b.LotNumber2 } into b_join
                                     from b in b_join.DefaultIfEmpty()
                                     select new
                                     {
                                         a.WhCode,
                                         a.ClientId,
                                         a.ClientCode,
                                         a.SoNumber,
                                         a.CustomerPoNumber,
                                         a.AltItemNumber,
                                         Qty = (System.Int32?)(a.Qty - ((Int32?)b.Qty ?? (Int32?)0)),
                                         a.Length,
                                         a.Width,
                                         a.Height,
                                         a.Weight,
                                         ItemId = (Int32?)a.ItemId,
                                         a.UnitName,
                                         a.UnitId,
                                         a.LotNumber1,
                                         a.LotNumber2,
                                         a.LotDate
                                     }))
                              where a.Qty > 0
                              select new LoadContainerHuDetailResult1
                              {
                                  WhCode = a.WhCode,
                                  ClientId = a.ClientId,
                                  ClientCode = a.ClientCode,
                                  SoNumber = a.SoNumber,
                                  CustomerPoNumber = a.CustomerPoNumber,
                                  AltItemNumber = a.AltItemNumber,
                                  Qty = a.Qty,
                                  Length = a.Length,
                                  Width = a.Width,
                                  Height = a.Height,
                                  Weight = a.Weight,
                                  ItemId = a.ItemId,
                                  UnitName = a.UnitName,
                                  UnitId = a.UnitId,
                                  LotNumber1 = a.LotNumber1,
                                  LotNumber2 = a.LotNumber2,
                                  LotDate = a.LotDate,
                                  sequence = 99999
                              }).ToList();

                sql = sql.Where(u => clientCodeList.Contains(u.ClientCode)).ToList();
                sql = sql.Where(u => soList.Contains(u.SoNumber)).ToList();

                if (sql.Count == 0)
                {
                    return "所选库存为空，请重新查询库存！";
                }

                //验证是否已选择库存

                if (checkCount.Count > 0)
                {
                    //如果已选择库存，比较已选择的出货流程与预保存的出货流程是否一致
                    LoadContainerExtendHuDetail firstCheck = checkCount.First();
                    if (firstCheck.ProcessId != processId)
                    {
                        return "所选出货流程与已保存的存在差异，无法添加！";
                    }
                }

                List<LoadContainerHuDetailResult1> list1 = sql.ToList();
                Hashtable hs = new Hashtable();
                if (soList != null)
                {
                    int count = 1;
                    foreach (var item in soList.Distinct())
                    {
                        if (list1.Where(u => u.SoNumber == item).Count() > 0)
                        {
                            LoadContainerHuDetailResult1 oldload = list1.Where(u => u.SoNumber == item).First();
                            list1.Remove(oldload);

                            LoadContainerHuDetailResult1 newload = oldload;
                            newload.sequence = count;
                            list1.Add(newload);
                            hs.Add(item, count);

                            count++;
                        }
                    }
                }

                int setcount = 0;
                List<LoadContainerExtendHuDetail> checkSequence = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);
                if (checkSequence.Count != 0)
                {
                    setcount = (Int32)checkSequence.Max(u => u.Sequence);
                }
                List<LoadContainerExtendHuDetail> list = new List<LoadContainerExtendHuDetail>();

                List<LoadContainerExtendHuDetail> checkSeq = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);

                foreach (var item in list1)
                {


                    if (checkSeq.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ClientCode == item.ClientCode && u.UnitName == item.UnitName && u.ItemId == item.ItemId && u.ClientId == item.ClientId).Count() > 0)
                    {
                        LoadContainerExtendHuDetail setseq = checkSeq.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ClientCode == item.ClientCode).First();
                        setseq.Qty = (Int32)setseq.Qty + (Int32)item.Qty;
                        idal.ILoadContainerExtendHuDetailDAL.UpdateBy(setseq, u => u.Id == setseq.Id, new string[] { "Qty" });
                    }
                    else
                    {
                        LoadContainerExtendHuDetail loadContainerExtendHuDetail = new LoadContainerExtendHuDetail();
                        loadContainerExtendHuDetail.LoadContainerId = loadContainerId;
                        loadContainerExtendHuDetail.WhCode = whCode;
                        loadContainerExtendHuDetail.ProcessId = processId;
                        loadContainerExtendHuDetail.ProcessName = processName;
                        loadContainerExtendHuDetail.ClientId = item.ClientId;
                        loadContainerExtendHuDetail.ClientCode = item.ClientCode;
                        loadContainerExtendHuDetail.SoNumber = item.SoNumber;
                        loadContainerExtendHuDetail.CustomerPoNumber = item.CustomerPoNumber;
                        loadContainerExtendHuDetail.AltItemNumber = item.AltItemNumber;
                        loadContainerExtendHuDetail.ItemId = (Int32)item.ItemId;
                        loadContainerExtendHuDetail.UnitName = item.UnitName;
                        loadContainerExtendHuDetail.UnitId = item.UnitId;
                        loadContainerExtendHuDetail.Qty = (Int32)item.Qty;
                        loadContainerExtendHuDetail.Length = item.Length;
                        loadContainerExtendHuDetail.Width = item.Width;
                        loadContainerExtendHuDetail.Height = item.Height;
                        loadContainerExtendHuDetail.Weight = item.Weight;
                        loadContainerExtendHuDetail.LotNumber1 = item.LotNumber1;
                        loadContainerExtendHuDetail.LotNumber2 = item.LotNumber2;
                        loadContainerExtendHuDetail.LotDate = item.LotDate;

                        if (hs.ContainsKey(item.SoNumber))
                        {
                            loadContainerExtendHuDetail.Sequence = Convert.ToInt32(hs[item.SoNumber].ToString()) + setcount;
                        }
                        else
                        {
                            loadContainerExtendHuDetail.Sequence = setcount;
                        }

                        loadContainerExtendHuDetail.CreateUser = userName;
                        loadContainerExtendHuDetail.CreateDate = DateTime.Now;
                        list.Add(loadContainerExtendHuDetail);
                    }
                }

                idal.ILoadContainerExtendHuDetailDAL.Add(list);
                idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
                return "Y";
            }
        }


        //装箱单  选择库存生成明细  ByPOSKU
        public string LoadContainerHuDetailAddByPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int processId, string processName, string userName)
        {
            lock (o)
            {
                //验证所选客户的 货代是否一致
                var checkAgent = (from a in idal.IWhClientDAL.SelectAll()
                                  join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                  from b in b_join.DefaultIfEmpty()
                                  join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                  from c in c_join.DefaultIfEmpty()
                                  where a.WhCode == whCode && clientCodeList.Contains(a.ClientCode)
                                  select new
                                  {
                                      AgentCode = c.AgentCode
                                  }).Distinct();
                if (checkAgent.Count() > 1)
                {
                    return "所选库存货代不一致，无法添加！";
                }

                //验证是否已选择库存，所选库存的客户 与 现选择的客户 比较货代
                List<LoadContainerExtendHuDetail> checkCount = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);
                if (checkCount.Count > 0)
                {
                    string[] checkClientCodeList = (from a in checkCount select a.ClientCode).Distinct().ToArray();

                    var checkAgent1 = (from a in idal.IWhClientDAL.SelectAll()
                                       join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                       from b in b_join.DefaultIfEmpty()
                                       join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                       from c in c_join.DefaultIfEmpty()
                                       where a.WhCode == whCode && checkClientCodeList.Contains(a.ClientCode)
                                       select new
                                       {
                                           AgentCode = c.AgentCode
                                       }).Distinct();
                    if (checkAgent1.Count() > 0)
                    {
                        if (checkAgent1.First().AgentCode != checkAgent.First().AgentCode)
                        {
                            return "所选库存与已选择的库存货代不一致，无法添加！";
                        }
                    }
                }

                var s =
                              (from a in (
                                     (from a in (
                                       (from hudetail in idal.IHuDetailDAL.SelectAll()
                                        where
                                         hudetail.SoNumber != "" && hudetail.WhCode == whCode
                                        group hudetail by new
                                        {
                                            hudetail.WhCode,
                                            hudetail.ClientCode,
                                            hudetail.ClientId,
                                            hudetail.SoNumber,
                                            hudetail.CustomerPoNumber,
                                            hudetail.AltItemNumber,
                                            hudetail.ItemId,
                                            hudetail.UnitId,
                                            hudetail.UnitName,
                                            hudetail.LotNumber1,
                                            hudetail.LotNumber2,
                                            hudetail.LotDate
                                        } into g
                                        select new
                                        {
                                            g.Key.WhCode,
                                            g.Key.ClientCode,
                                            ClientId = g.Key.ClientId,
                                            g.Key.SoNumber,
                                            g.Key.CustomerPoNumber,
                                            g.Key.AltItemNumber,
                                            ItemId = g.Key.ItemId,
                                            UnitId = g.Key.UnitId,
                                            g.Key.UnitName,
                                            Qty = g.Sum(p => p.Qty),
                                            Length = g.Average(p => p.Length),
                                            Width = g.Average(p => p.Width),
                                            Height = g.Average(p => p.Height),
                                            Weight = g.Average(p => p.Weight),
                                            g.Key.LotNumber1,
                                            g.Key.LotNumber2,
                                            LotDate = (DateTime?)g.Key.LotDate
                                        }))
                                      join b in (from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                                                 group a by new
                                                 {
                                                     a.ClientCode,
                                                     a.WhCode,
                                                     a.SoNumber,
                                                     a.CustomerPoNumber,
                                                     a.ItemId,
                                                     a.UnitName,
                                                     a.LotNumber1,
                                                     a.LotNumber2
                                                 } into h
                                                 select new
                                                 {
                                                     h.Key.ClientCode,
                                                     h.Key.WhCode,
                                                     h.Key.SoNumber,
                                                     h.Key.CustomerPoNumber,
                                                     h.Key.ItemId,
                                                     h.Key.UnitName,
                                                     h.Key.LotNumber1,
                                                     h.Key.LotNumber2,
                                                     Qty = h.Sum(u => u.Qty)
                                                 }
                                                 )
                                            on new { a.ClientCode, a.WhCode, a.SoNumber, a.CustomerPoNumber, ItemId = a.ItemId, a.UnitName, a.LotNumber1, a.LotNumber2 }
                                        equals new { b.ClientCode, b.WhCode, b.SoNumber, b.CustomerPoNumber, ItemId = b.ItemId, b.UnitName, b.LotNumber1, b.LotNumber2 } into b_join
                                      from b in b_join.DefaultIfEmpty()
                                      select new
                                      {
                                          a.WhCode,
                                          a.ClientId,
                                          a.ClientCode,
                                          a.SoNumber,
                                          a.CustomerPoNumber,
                                          a.AltItemNumber,
                                          Qty = (System.Int32?)(a.Qty - ((Int32?)b.Qty ?? (Int32?)0)),
                                          a.Length,
                                          a.Width,
                                          a.Height,
                                          a.Weight,
                                          ItemId = (Int32?)a.ItemId,
                                          a.UnitName,
                                          a.UnitId,
                                          a.LotNumber1,
                                          a.LotNumber2,
                                          a.LotDate
                                      }))
                               where a.Qty > 0
                               select new LoadContainerHuDetailResult1
                               {
                                   WhCode = a.WhCode,
                                   ClientId = a.ClientId,
                                   ClientCode = a.ClientCode,
                                   SoNumber = a.SoNumber,
                                   CustomerPoNumber = a.CustomerPoNumber,
                                   AltItemNumber = a.AltItemNumber,
                                   Qty = a.Qty,
                                   Length = a.Length,
                                   Width = a.Width,
                                   Height = a.Height,
                                   Weight = a.Weight,
                                   ItemId = a.ItemId,
                                   UnitName = a.UnitName,
                                   UnitId = a.UnitId,
                                   LotNumber1 = a.LotNumber1,
                                   LotNumber2 = a.LotNumber2,
                                   LotDate = a.LotDate,
                                   sequence = 99999
                               });

                List<LoadContainerHuDetailResult1> sql = s.Distinct().ToList();
                sql = sql.Where(u => clientCodeList.Contains(u.ClientCode)).ToList();
                sql = sql.Where(u => soList.Contains(u.SoNumber)).ToList();
                sql = sql.Where(u => poList.Contains(u.CustomerPoNumber)).ToList();
                sql = sql.Where(u => altList.Contains(u.ItemId.ToString())).ToList();

                if (sql.Count == 0)
                {
                    return "所选库存为空，请重新查询库存！";
                }

                //验证是否已选择库存

                if (checkCount.Count > 0)
                {
                    //如果已选择库存，比较已选择的出货流程与预保存的出货流程是否一致
                    LoadContainerExtendHuDetail firstCheck = checkCount.First();
                    if (firstCheck.ProcessId != processId)
                    {
                        return "所选出货流程与已保存的存在差异，无法添加！";
                    }
                }

                List<LoadContainerHuDetailResult1> list1 = sql.ToList();

                int count = 1;
                List<LoadContainerExtendHuDetail> checkSequence = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);
                if (checkSequence.Count > 0)
                {
                    if (checkSequence.Where(u => u.Sequence != 99999).Count() > 0)
                    {
                        count = (Int32)checkSequence.Where(u => u.Sequence != 99999).Max(u => u.Sequence) + 1;
                    }
                }

                for (int i = 0; i < soList.Length; i++)
                {
                    string so = soList[i];
                    string po = poList[i];
                    int? itemId = Convert.ToInt32(altList[i]);
                    string clientcode = clientCodeList[i];

                    if (list1.Where(u => u.SoNumber == so && u.CustomerPoNumber == po && u.ItemId == itemId && u.ClientCode == clientcode).Count() > 0)
                    {
                        LoadContainerHuDetailResult1 oldentity = list1.Where(u => u.SoNumber == so && u.CustomerPoNumber == po && u.ItemId == itemId && u.ClientCode == clientcode).First();
                        list1.Remove(oldentity);

                        LoadContainerHuDetailResult1 newentity = oldentity;
                        newentity.sequence = count;
                        list1.Add(newentity);
                        count++;
                    }
                }

                List<LoadContainerExtendHuDetail> list = new List<LoadContainerExtendHuDetail>();
                foreach (var item in list1.OrderBy(u => u.sequence))
                {
                    if (checkSequence.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ClientCode == item.ClientCode && u.UnitName == item.UnitName && u.ItemId == item.ItemId && u.ClientId == item.ClientId).Count() > 0)
                    {
                        LoadContainerExtendHuDetail setseq = checkSequence.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ClientCode == item.ClientCode).First();
                        setseq.Qty = (Int32)setseq.Qty + (Int32)item.Qty;
                        idal.ILoadContainerExtendHuDetailDAL.UpdateBy(setseq, u => u.Id == setseq.Id, new string[] { "Qty" });
                    }
                    else
                    {
                        LoadContainerExtendHuDetail loadContainerExtendHuDetail = new LoadContainerExtendHuDetail();
                        loadContainerExtendHuDetail.LoadContainerId = loadContainerId;
                        loadContainerExtendHuDetail.WhCode = whCode;
                        loadContainerExtendHuDetail.ProcessId = processId;
                        loadContainerExtendHuDetail.ProcessName = processName;
                        loadContainerExtendHuDetail.ClientId = item.ClientId;
                        loadContainerExtendHuDetail.ClientCode = item.ClientCode;
                        loadContainerExtendHuDetail.SoNumber = item.SoNumber;
                        loadContainerExtendHuDetail.CustomerPoNumber = item.CustomerPoNumber;
                        loadContainerExtendHuDetail.AltItemNumber = item.AltItemNumber;
                        loadContainerExtendHuDetail.ItemId = (Int32)item.ItemId;
                        loadContainerExtendHuDetail.UnitName = item.UnitName;
                        loadContainerExtendHuDetail.UnitId = item.UnitId;
                        loadContainerExtendHuDetail.Qty = (Int32)item.Qty;
                        loadContainerExtendHuDetail.Length = item.Length;
                        loadContainerExtendHuDetail.Width = item.Width;
                        loadContainerExtendHuDetail.Height = item.Height;
                        loadContainerExtendHuDetail.Weight = item.Weight;
                        loadContainerExtendHuDetail.LotNumber1 = item.LotNumber1;
                        loadContainerExtendHuDetail.LotNumber2 = item.LotNumber2;
                        loadContainerExtendHuDetail.LotDate = item.LotDate;
                        loadContainerExtendHuDetail.Sequence = item.sequence;

                        loadContainerExtendHuDetail.CreateUser = userName;
                        loadContainerExtendHuDetail.CreateDate = DateTime.Now;
                        list.Add(loadContainerExtendHuDetail);
                    }
                }

                idal.ILoadContainerExtendHuDetailDAL.Add(list);
                idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
                return "Y";
            }
        }


        //装箱单  批量导入出货明细
        public string LoadContainerHuDetailAddByImportPOSKU(string whCode, int loadContainerId, string[] soList, string[] poList, string[] altList, string[] clientCodeList, int[] qtyList, int[] sequenceList, string userName)
        {
            lock (o)
            {
                //验证所选客户的 货代是否一致
                var checkAgent = (from a in idal.IWhClientDAL.SelectAll()
                                  join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                  from b in b_join.DefaultIfEmpty()
                                  join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                  from c in c_join.DefaultIfEmpty()
                                  where a.WhCode == whCode && clientCodeList.Contains(a.ClientCode)
                                  select new
                                  {
                                      AgentCode = c.AgentCode
                                  }).Distinct();
                if (checkAgent.Count() > 1)
                {
                    return "导入明细中客户所属货代不一致，无法导入！";
                }

                //验证是否已选择库存，所选库存的客户 与 现选择的客户 比较货代
                List<LoadContainerExtendHuDetail> checkCount = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);
                if (checkCount.Count > 0)
                {
                    string[] checkClientCodeList = (from a in checkCount select a.ClientCode).Distinct().ToArray();

                    var checkAgent1 = (from a in idal.IWhClientDAL.SelectAll()
                                       join b in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                       from b in b_join.DefaultIfEmpty()
                                       join c in idal.IWhAgentDAL.SelectAll() on new { AgentId = b.AgentId } equals new { AgentId = c.Id } into c_join
                                       from c in c_join.DefaultIfEmpty()
                                       where a.WhCode == whCode && checkClientCodeList.Contains(a.ClientCode)
                                       select new
                                       {
                                           AgentCode = c.AgentCode
                                       }).Distinct();
                    if (checkAgent1.Count() > 0)
                    {
                        if (checkAgent1.First().AgentCode != checkAgent.First().AgentCode)
                        {
                            return "已导入明细与现导入明细货代不一致，无法导入！";
                        }
                    }
                }


                List<WhClient> list3 = new List<WhClient>();
                for (int i = 0; i < clientCodeList.Length; i++)
                {
                    if (list3.Where(u => u.ClientCode == clientCodeList[i]).Count() == 0)
                    {
                        string clientCodeString = clientCodeList[i];
                        WhClient client = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == clientCodeString).First();
                        list3.Add(client);
                    }
                }

                int processId = 0;
                string processName = "";
                List<FlowHeadResult> list2 = CheckLoadContainerClientCodeRule(whCode, list3).ToList();
                if (list2.Count == 0)
                {
                    return "已导入客户无出货流程，无法导入！";
                }
                else if (list2.Count == 1)
                {
                    FlowHeadResult first = list2.First();
                    processId = Convert.ToInt32(first.Id);
                    processName = first.FlowName;
                }
                else
                {
                    return "已导入客户存在多个出货流程，无法导入！";
                }

                List<HuDetailResult> huDetailList = (from a in idal.IHuMasterDAL.SelectAll()
                                                     join b in idal.IHuDetailDAL.SelectAll()
                                                           on new { a.WhCode, a.HuId }
                                                       equals new { b.WhCode, b.HuId }
                                                     join c in idal.IWhLocationDAL.SelectAll()
                                                           on new { a.WhCode, a.Location }
                                                       equals new { c.WhCode, Location = c.LocationId }
                                                     join d in (
                                                         (from hudetail in idal.IHuDetailDAL.SelectAll()
                                                          group hudetail by new
                                                          {
                                                              hudetail.AltItemNumber,
                                                              hudetail.ItemId
                                                          } into g
                                                          select new
                                                          {
                                                              g.Key.AltItemNumber,
                                                              ItemId = (Int32?)g.Key.ItemId,
                                                              Qty = (Int32?)g.Sum(p => p.Qty)
                                                          }))
                                                           on new { b.AltItemNumber, b.ItemId }
                                                       equals new { d.AltItemNumber, ItemId = (Int32)d.ItemId } into d_join
                                                     from d in d_join.DefaultIfEmpty()
                                                     where
                                                     a.Type == "M" &&
                                                    a.Status == "A" &&
                                                    c.LocationTypeId == 1
                                                    && (b.Qty - (b.PlanQty ?? 0)) > 0
                                                    && a.WhCode == whCode && altList.Contains(b.AltItemNumber) && clientCodeList.Contains(b.ClientCode)
                                                     orderby d.Qty descending
                                                     select new HuDetailResult
                                                     {
                                                         WhCode = b.WhCode,
                                                         ClientCode = b.ClientCode,
                                                         SoNumber = b.SoNumber,
                                                         CustomerPoNumber = b.CustomerPoNumber,
                                                         AltItemNumber = b.AltItemNumber,
                                                         ItemId = b.ItemId,
                                                         UnitName = b.UnitName,
                                                         Qty = d.Qty
                                                     }).Distinct().ToList();

                string result = "";

                List<LoadContainerExtendHuDetail> list = new List<LoadContainerExtendHuDetail>();
                for (int i = 0; i < qtyList.Length; i++)
                {
                    string so = soList[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                    string po = poList[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                    string item = altList[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                    string clientcode = clientCodeList[i].Replace(" ", "").Replace(@"\r\n", "").Replace(@"\r", "").Replace(@"\n", "");
                    int qty = qtyList[i];
                    int sequence = 0;
                    if (sequenceList != null)
                    {
                        sequence = sequenceList[i];
                    }

                    WhClient client = list3.Where(u => u.ClientCode == clientcode && u.WhCode == whCode).First();
                    HuDetailResult itemMaster = new HuDetailResult();
                    if (!string.IsNullOrEmpty(so) && !string.IsNullOrEmpty(po))
                    {
                        List<HuDetailResult> checkList = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item && u.SoNumber == so && u.CustomerPoNumber == po).OrderByDescending(u => u.Qty).ToList();
                        if (checkList.Count == 0)
                        {
                            result = "该款号无有效库存：" + item;
                            break;
                        }
                        else
                        {
                            itemMaster = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item && u.SoNumber == so && u.CustomerPoNumber == po).OrderByDescending(u => u.Qty).ToList().First();
                        }

                    }
                    else if (!string.IsNullOrEmpty(po))
                    {
                        List<HuDetailResult> checkList = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item && u.CustomerPoNumber == po).OrderByDescending(u => u.Qty).ToList();

                        if (checkList.Count == 0)
                        {
                            result = "该款号无有效库存：" + item;
                            break;
                        }
                        else
                        {
                            itemMaster = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item && u.CustomerPoNumber == po).OrderByDescending(u => u.Qty).ToList().First();
                        }
                    }
                    else
                    {
                        List<HuDetailResult> checkList = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item).OrderByDescending(u => u.Qty).ToList();

                        if (checkList.Count == 0)
                        {
                            result = "该款号无有效库存：" + item;
                            break;
                        }
                        else
                        {
                            itemMaster = huDetailList.Where(u => u.WhCode == whCode && u.ClientCode == clientcode && u.AltItemNumber == item).OrderByDescending(u => u.Qty).ToList().First();
                        }
                    }

                    LoadContainerExtendHuDetail loadContainerExtendHuDetail = new LoadContainerExtendHuDetail();
                    loadContainerExtendHuDetail.LoadContainerId = loadContainerId;
                    loadContainerExtendHuDetail.WhCode = whCode;
                    loadContainerExtendHuDetail.ProcessId = processId;
                    loadContainerExtendHuDetail.ProcessName = processName;
                    loadContainerExtendHuDetail.ClientId = client.Id;
                    loadContainerExtendHuDetail.ClientCode = client.ClientCode;
                    loadContainerExtendHuDetail.SoNumber = so;
                    loadContainerExtendHuDetail.CustomerPoNumber = po;
                    loadContainerExtendHuDetail.AltItemNumber = itemMaster.AltItemNumber;
                    loadContainerExtendHuDetail.ItemId = Convert.ToInt32(itemMaster.ItemId);
                    loadContainerExtendHuDetail.UnitName = itemMaster.UnitName;
                    loadContainerExtendHuDetail.UnitId = 0;
                    loadContainerExtendHuDetail.Qty = qty;
                    loadContainerExtendHuDetail.Length = 0;
                    loadContainerExtendHuDetail.Width = 0;
                    loadContainerExtendHuDetail.Height = 0;
                    loadContainerExtendHuDetail.Weight = 0;
                    loadContainerExtendHuDetail.LotNumber1 = "";
                    loadContainerExtendHuDetail.LotNumber2 = "";
                    loadContainerExtendHuDetail.Sequence = sequence;
                    loadContainerExtendHuDetail.CreateUser = userName;
                    loadContainerExtendHuDetail.CreateDate = DateTime.Now;
                    list.Add(loadContainerExtendHuDetail);
                }

                if (result != "")
                {
                    return result;
                }

                idal.ILoadContainerExtendHuDetailDAL.Add(list);
                idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
                return "Y";
            }
        }



        //装箱单所选库存 按SO查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySo(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                      where a.LoadContainerId == searchEntity.LoadContainerHuDetailId
                      group new { a } by new
                      {
                          a.ProcessName,
                          a.ClientCode,
                          a.SoNumber,
                          a.Sequence,
                          a.CreateUser
                      } into g
                      select new LoadContainerHuDetailResult2
                      {
                          Action = "",
                          ProcessName = g.Key.ProcessName,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          Sequence = g.Key.Sequence,
                          Qty = g.Sum(p => p.a.Qty),
                          CBM = g.Sum(p => p.a.Qty * p.a.Length * p.a.Width * p.a.Height),
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Min(p => p.a.CreateDate)
                      };
            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql.Sum(u => u.CBM).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //装箱单所选库存 按PO查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPo(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                      where a.LoadContainerId == searchEntity.LoadContainerHuDetailId
                      group new { a } by new
                      {
                          a.ProcessName,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.Sequence,
                          a.CreateUser
                      } into g
                      select new LoadContainerHuDetailResult2
                      {
                          Action = "",
                          ProcessName = g.Key.ProcessName,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          Sequence = g.Key.Sequence,
                          Qty = g.Sum(p => p.a.Qty),
                          CBM = g.Sum(p => p.a.Qty * p.a.Length * p.a.Width * p.a.Height),
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Min(p => p.a.CreateDate)
                      };

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql.Sum(u => u.CBM).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //装箱单所选库存 按款号查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSku(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                      where a.LoadContainerId == searchEntity.LoadContainerHuDetailId
                      group new { a } by new
                      {
                          a.ProcessName,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.Sequence,
                          a.CreateUser
                      } into g
                      select new LoadContainerHuDetailResult2
                      {
                          Action = "",
                          ProcessName = g.Key.ProcessName,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          Sequence = g.Key.Sequence,
                          Qty = g.Sum(p => p.a.Qty),
                          CBM = g.Sum(p => p.a.Qty * p.a.Length * p.a.Width * p.a.Height),
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Min(p => p.a.CreateDate)
                      };

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql.Sum(u => u.CBM).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //装箱单所选库存 按款号属性查询
        public List<LoadContainerHuDetailResult2> SelectLoadContainerHuDetailBySoPoSkuStyle(LoadContainerHuDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.ILoadContainerExtendHuDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll()
                      on a.ItemId equals b.Id
                      where a.LoadContainerId == searchEntity.LoadContainerHuDetailId
                      group new { a } by new
                      {
                          a.Id,
                          a.ProcessName,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.ItemId,
                          b.Style1,
                          b.Style2,
                          b.Style3,
                          a.Sequence,
                          a.Length,
                          a.Width,
                          a.Height,
                          a.Weight,
                          a.UnitName,
                          a.LotNumber1,
                          a.LotNumber2,
                          a.LotDate,
                          a.CreateUser
                      } into g
                      select new LoadContainerHuDetailResult2
                      {
                          Action = "",
                          Id = g.Key.Id,
                          ProcessName = g.Key.ProcessName,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          ItemId = g.Key.ItemId,
                          Style1 = g.Key.Style1,
                          Style2 = g.Key.Style2,
                          Style3 = g.Key.Style3,
                          Sequence = g.Key.Sequence,
                          Qty = g.Sum(p => p.a.Qty),
                          CBM = g.Sum(p => p.a.Qty * p.a.Length * p.a.Width * p.a.Height),
                          Length = g.Key.Length,
                          Width = g.Key.Width,
                          Height = g.Key.Height,
                          Weight = g.Key.Weight,
                          UnitName = g.Key.UnitName,
                          LotNumber1 = g.Key.LotNumber1,
                          LotNumber2 = g.Key.LotNumber2,
                          LotDate = g.Key.LotDate,
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Min(p => p.a.CreateDate)
                      };
            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"立方\":\"" + sql.Sum(u => u.CBM).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //修改装箱单选择的库存明细的顺序
        public string LoadContainerHuDetailEdit(LoadContainerExtendHuDetail entity)
        {
            if (entity.Id != 0)
            {
                List<LoadContainerExtendHuDetail> checklist = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.Id == entity.Id);
                if (checklist.Count > 0)
                {
                    LoadContainerExtendHuDetail check = checklist.First();
                    if (entity.Qty > check.Qty)
                    {
                        return "修改数量不能大于原始数量！";
                    }
                    entity.UpdateDate = DateTime.Now;
                    idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Qty", "Sequence", "UpdateUser", "UpdateDate" });
                }
            }
            else if (entity.AltItemNumber != "")
            {
                entity.UpdateDate = DateTime.Now;
                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.WhCode == entity.WhCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }
            else if (entity.CustomerPoNumber != "")
            {
                entity.UpdateDate = DateTime.Now;
                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.WhCode == entity.WhCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }
            else
            {
                entity.UpdateDate = DateTime.Now;
                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //删除装箱单选择的库存明细
        public string LoadContainerHuDetailDel(LoadContainerExtendHuDetail entity)
        {
            if (entity.Id != 0)
            {
                idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.Id == entity.Id);
            }
            else if (entity.AltItemNumber != "")
            {
                idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.WhCode == entity.WhCode);
            }
            else if (entity.CustomerPoNumber != "")
            {
                idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.WhCode == entity.WhCode);
            }
            else
            {
                idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.LoadContainerId == entity.LoadContainerId && u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode);
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySo(string whCode, int loadContainerId, string[] soList, string[] sequenceList, string userName)
        {
            if (soList.Length != sequenceList.Length)
            {
                return "数据有误，无法导入！";
            }
            for (int i = 0; i < soList.Length; i++)
            {
                LoadContainerExtendHuDetail entity = new LoadContainerExtendHuDetail();
                entity.Sequence = Convert.ToInt32(sequenceList[i].ToString());
                entity.UpdateDate = DateTime.Now;

                string soNumber = soList[i].ToString();
                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == loadContainerId
                && u.SoNumber == soNumber && u.WhCode == whCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPo(string whCode, int loadContainerId, string[] soList, string[] poList, string[] sequenceList, string userName)
        {
            if (soList.Length != sequenceList.Length || sequenceList.Length != poList.Length)
            {
                return "数据有误，无法导入！";
            }
            for (int i = 0; i < soList.Length; i++)
            {
                LoadContainerExtendHuDetail entity = new LoadContainerExtendHuDetail();
                entity.Sequence = Convert.ToInt32(sequenceList[i].ToString());
                entity.UpdateDate = DateTime.Now;

                string soNumber = soList[i].ToString();
                string poNumber = poList[i].ToString();

                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == loadContainerId
                && u.SoNumber == soNumber && u.CustomerPoNumber == poNumber && u.WhCode == whCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPoSku(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] sequenceList, string userName)
        {
            if (soList.Length != sequenceList.Length || sequenceList.Length != poList.Length || poList.Length != skuList.Length)
            {
                return "数据有误，无法导入！";
            }
            for (int i = 0; i < soList.Length; i++)
            {
                LoadContainerExtendHuDetail entity = new LoadContainerExtendHuDetail();
                entity.Sequence = Convert.ToInt32(sequenceList[i].ToString());
                entity.UpdateDate = DateTime.Now;

                string soNumber = soList[i].ToString();
                string poNumber = poList[i].ToString();
                string skuNumber = skuList[i].ToString();

                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == loadContainerId
                && u.SoNumber == soNumber && u.CustomerPoNumber == poNumber && u.AltItemNumber == skuNumber && u.WhCode == whCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //导入装箱单选择库存的顺序
        public string ImportsSequenceBySoPoSkuStyle(string whCode, int loadContainerId, string[] soList, string[] poList, string[] skuList, string[] itemIdList, string[] sequenceList, string userName)
        {
            if (soList.Length != sequenceList.Length || sequenceList.Length != poList.Length || poList.Length != skuList.Length || skuList.Length != itemIdList.Length)
            {
                return "数据有误，无法导入！";
            }
            for (int i = 0; i < soList.Length; i++)
            {
                LoadContainerExtendHuDetail entity = new LoadContainerExtendHuDetail();
                entity.Sequence = Convert.ToInt32(sequenceList[i].ToString());
                entity.UpdateDate = DateTime.Now;

                string soNumber = soList[i].ToString();
                string poNumber = poList[i].ToString();
                string skuNumber = skuList[i].ToString();
                int itemId = Convert.ToInt32(itemIdList[i].ToString());

                idal.ILoadContainerExtendHuDetailDAL.UpdateBy(entity, u => u.LoadContainerId == loadContainerId
                && u.SoNumber == soNumber && u.CustomerPoNumber == poNumber && u.AltItemNumber == skuNumber && u.ItemId == itemId && u.WhCode == whCode, new string[] { "Sequence", "UpdateUser", "UpdateDate" });
            }

            idal.ILoadContainerExtendHuDetailDAL.SaveChanges();
            return "Y";
        }

        //装箱单生成CLP
        public string ConfirmLoadMasterAdd(string whCode, string userName, int loadContainerId)
        {
            lock (o)
            {
                LoadContainerExtend loadContainer = idal.ILoadContainerExtendDAL.SelectBy(u => u.Id == loadContainerId).First();
                if ((loadContainer.LoadId ?? "") != "")
                {
                    //如果已生成Load了
                    //修改Load对应的订单明细即可

                    LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadContainer.LoadId && u.WhCode == loadContainer.WhCode).First();
                    if (loadMaster.Status0 != "U")
                    {
                        return "Load状态有误，请重新查询！";
                    }
                    else
                    {
                        //先删除Load的出库订单及关系
                        List<LoadDetail> loadDetailList = (from a in idal.ILoadDetailDAL.SelectAll()
                                                           where a.LoadMasterId == loadMaster.Id
                                                           select a).ToList();

                        foreach (var item in loadDetailList)
                        {
                            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.OutBoundOrderId);
                            if (OutBoundOrderList.Count > 0)
                            {
                                OutBoundOrder eneity = OutBoundOrderList.First();

                                idal.IOutBoundOrderDetailDAL.DeleteByExtended(u => u.OutBoundOrderId == eneity.Id);
                            }
                        }

                        //再重新添加出库订单及关系

                        List<LoadContainerExtendHuDetail> list = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);

                        string[] clientCode = (from a in list select a.ClientCode).Distinct().ToArray();
                        foreach (var item in clientCode)
                        {
                            //取得客户的明细
                            List<LoadContainerExtendHuDetail> listByClientCode = list.Where(u => u.ClientCode == item).ToList();

                            LoadContainerExtendHuDetail first = listByClientCode.First();

                            List<OutBoundOrder> checkOutList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == first.WhCode && u.ClientCode == first.ClientCode && u.CustomerOutPoNumber == loadContainer.BillNumber && u.AltCustomerOutPoNumber == loadContainerId.ToString());
                            //创建出库订单
                            OutBoundOrder outOrder = new OutBoundOrder();
                            if (checkOutList.Count == 0)
                            {
                                outOrder.WhCode = whCode;
                                outOrder.ClientId = Convert.ToInt32(first.ClientId);
                                outOrder.ClientCode = first.ClientCode;
                                outOrder.OutPoNumber = "SA" + DI.IDGenerator.NewId;
                                outOrder.CustomerOutPoNumber = loadContainer.BillNumber;    //客户出库单号等于 装箱单的提单号
                                outOrder.AltCustomerOutPoNumber = loadContainerId.ToString();
                                outOrder.ReceiptId = "";

                                outOrder.ProcessId = first.ProcessId;
                                List<FlowDetail> sql = (from a in idal.IFlowDetailDAL.SelectAll()
                                                        where a.FlowHeadId == first.ProcessId
                                                        select a).ToList();
                                FlowDetail flowDetail = sql.Where(u => u.StatusId == 15).OrderBy(u => u.OrderId).First();
                                outOrder.NowProcessId = flowDetail.FlowRuleId;
                                outOrder.StatusId = flowDetail.StatusId;
                                outOrder.StatusName = flowDetail.StatusName;
                                outOrder.OrderSource = "WMS装箱单创建";
                                outOrder.PlanOutTime = loadContainer.ETD;           //计划出库时间等于ETD时间
                                outOrder.LoadFlag = 1;
                                outOrder.CreateUser = userName;
                                outOrder.CreateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.Add(outOrder);
                                idal.IOutBoundOrderDAL.SaveChanges();   //保存一次 取得出库订单主键
                            }
                            else
                            {
                                outOrder = checkOutList.First();
                            }

                            List<OutBoundOrderDetail> outBoundOrderDetailList = new List<OutBoundOrderDetail>();
                            foreach (var item1 in listByClientCode)
                            {
                                OutBoundOrderDetail outOrderDetail = new OutBoundOrderDetail();
                                outOrderDetail.WhCode = whCode;
                                outOrderDetail.OutBoundOrderId = outOrder.Id;
                                outOrderDetail.SoNumber = item1.SoNumber;
                                outOrderDetail.CustomerPoNumber = item1.CustomerPoNumber;
                                outOrderDetail.AltItemNumber = item1.AltItemNumber;
                                outOrderDetail.ItemId = item1.ItemId;
                                outOrderDetail.UnitId = item1.UnitId;
                                outOrderDetail.UnitName = item1.UnitName;
                                outOrderDetail.Qty = item1.Qty;
                                outOrderDetail.DSFLag = 0;
                                outOrderDetail.LotNumber1 = item1.LotNumber1 ?? "";
                                outOrderDetail.LotNumber2 = item1.LotNumber2 ?? "";
                                outOrderDetail.LotDate = item1.LotDate;
                                outOrderDetail.Sequence = item1.Sequence;
                                outOrderDetail.CreateUser = userName;
                                outOrderDetail.CreateDate = DateTime.Now;
                                outBoundOrderDetailList.Add(outOrderDetail);
                            }
                            idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetailList);

                            List<LoadDetail> checkLoadDetailList = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadMaster.Id && u.OutBoundOrderId == outOrder.Id);
                            if (checkLoadDetailList.Count == 0)
                            {
                                LoadDetail loadDetail = new LoadDetail();
                                loadDetail.LoadMasterId = loadMaster.Id;
                                loadDetail.OutBoundOrderId = outOrder.Id;
                                loadDetail.CreateUser = userName;
                                loadDetail.CreateDate = DateTime.Now;
                                idal.ILoadDetailDAL.Add(loadDetail);
                            }

                            idal.ILoadDetailDAL.SaveChanges();
                        }
                    }
                }
                else
                {
                    List<LoadContainerExtendHuDetail> list = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerId);

                    string[] clientCode = (from a in list select a.ClientCode).Distinct().ToArray();

                    LoadMaster loadMaster = new LoadMaster();
                    loadMaster.WhCode = whCode;
                    loadMaster.LoadId = "LD" + DI.IDGenerator.NewId;
                    loadMaster.ShipMode = "集装箱";
                    loadMaster.Status0 = "U";
                    loadMaster.Status1 = "U";
                    loadMaster.Status2 = "U";
                    loadMaster.Status3 = "U";
                    loadMaster.ProcessId = list.First().ProcessId;
                    loadMaster.ProcessName = list.First().ProcessName;
                    loadMaster.CreateUser = userName;
                    loadMaster.CreateDate = DateTime.Now;
                    idal.ILoadMasterDAL.Add(loadMaster);

                    foreach (var item in clientCode)
                    {
                        //取得客户的明细
                        List<LoadContainerExtendHuDetail> listByClientCode = list.Where(u => u.ClientCode == item).ToList();

                        LoadContainerExtendHuDetail first = listByClientCode.First();

                        //创建出库订单
                        OutBoundOrder outOrder = new OutBoundOrder();
                        outOrder.WhCode = whCode;
                        outOrder.ClientId = Convert.ToInt32(first.ClientId);
                        outOrder.ClientCode = first.ClientCode;
                        outOrder.OutPoNumber = "SA" + DI.IDGenerator.NewId;
                        outOrder.CustomerOutPoNumber = loadContainer.BillNumber;    //客户出库单号等于 装箱单的提单号
                        outOrder.AltCustomerOutPoNumber = loadContainerId.ToString();
                        outOrder.ReceiptId = "";

                        outOrder.ProcessId = first.ProcessId;
                        List<FlowDetail> sql = (from a in idal.IFlowDetailDAL.SelectAll()
                                                where a.FlowHeadId == first.ProcessId
                                                select a).ToList();
                        FlowDetail flowDetail = sql.Where(u => u.StatusId == 15).OrderBy(u => u.OrderId).First();
                        outOrder.NowProcessId = flowDetail.FlowRuleId;
                        outOrder.StatusId = flowDetail.StatusId;
                        outOrder.StatusName = flowDetail.StatusName;
                        outOrder.OrderSource = "WMS装箱单创建";
                        outOrder.PlanOutTime = loadContainer.ETD;           //计划出库时间等于ETD时间
                        outOrder.LoadFlag = 1;
                        outOrder.CreateUser = userName;
                        outOrder.CreateDate = DateTime.Now;
                        idal.IOutBoundOrderDAL.Add(outOrder);
                        idal.IOutBoundOrderDAL.SaveChanges();   //保存一次 取得出库订单主键

                        List<OutBoundOrderDetail> outBoundOrderDetailList = new List<OutBoundOrderDetail>();
                        foreach (var item1 in listByClientCode)
                        {
                            OutBoundOrderDetail outOrderDetail = new OutBoundOrderDetail();
                            outOrderDetail.WhCode = whCode;
                            outOrderDetail.OutBoundOrderId = outOrder.Id;
                            outOrderDetail.SoNumber = item1.SoNumber;
                            outOrderDetail.CustomerPoNumber = item1.CustomerPoNumber;
                            outOrderDetail.AltItemNumber = item1.AltItemNumber;
                            outOrderDetail.ItemId = item1.ItemId;
                            outOrderDetail.UnitId = item1.UnitId;
                            outOrderDetail.UnitName = item1.UnitName;
                            outOrderDetail.Qty = item1.Qty;
                            outOrderDetail.DSFLag = 0;
                            outOrderDetail.LotNumber1 = item1.LotNumber1 ?? "";
                            outOrderDetail.LotNumber2 = item1.LotNumber2 ?? "";
                            outOrderDetail.LotDate = item1.LotDate;
                            outOrderDetail.Sequence = item1.Sequence;
                            outOrderDetail.CreateUser = userName;
                            outOrderDetail.CreateDate = DateTime.Now;
                            outBoundOrderDetailList.Add(outOrderDetail);
                        }
                        idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetailList);

                        LoadDetail loadDetail = new LoadDetail();
                        loadDetail.LoadMasterId = loadMaster.Id;
                        loadDetail.OutBoundOrderId = outOrder.Id;
                        loadDetail.CreateUser = userName;
                        loadDetail.CreateDate = DateTime.Now;
                        idal.ILoadDetailDAL.Add(loadDetail);
                        idal.ILoadDetailDAL.SaveChanges();
                    }

                    //最后更新Load
                    LoadContainerExtend ex = new LoadContainerExtend();
                    ex.LoadId = loadMaster.LoadId;
                    ex.UpdateUser = userName;
                    ex.UpdateDate = DateTime.Now;

                    idal.ILoadContainerExtendDAL.UpdateBy(ex, u => u.Id == loadContainerId, new string[] { "LoadId", "UpdateUser", "UpdateDate" });

                }
                idal.ILoadContainerExtendDAL.SaveChanges();
                return "Y";
            }
        }


        //保存前 验证装箱单数量与立方 和实际释放的做比对
        public string CheckLoadContainerDetailToRealease(int loadContainerId)
        {
            List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.Id == loadContainerId);

            if (loadContainerExtendList.Count > 0)
            {
                LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();
                if (loadContainerExtend.ChuCangFS == "海运")
                {
                    var loadContainerExtendHuDetailList = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerExtend.Id);

                    int qty = loadContainerExtendHuDetailList.Sum(u => u.Qty);
                    double cbm = Math.Round((double)loadContainerExtendHuDetailList.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Length * u.Width * u.Height * u.Qty), 4);

                    double diff = Math.Round(cbm - (double)(loadContainerExtend.TotalCBM ?? 0), 4);
                    if (diff < 0)
                    {
                        diff = -diff;
                    }
                    if (qty != loadContainerExtend.TotalQty || cbm != (double)(loadContainerExtend.TotalCBM ?? 0))
                    {
                        return "当前选择数量或立方与装箱单数量或立方不一致！<br />选择数量为：" + qty + "，装箱单数量为：" + (loadContainerExtend.TotalQty ?? 0) + "<br />选择立方为：" + cbm + "，装箱单立方为：" + (loadContainerExtend.TotalCBM ?? 0) + "，立方相差：" + diff + "。确认保存吗？";
                    }
                }
            }

            return "Y";
        }


        //验证箱型 和 所选立方
        public string CheckLoadContainerCBMToRealease(int loadContainerId)
        {
            List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.Id == loadContainerId);

            if (loadContainerExtendList.Count > 0)
            {
                LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();
                if (loadContainerExtend.ChuCangFS == "海运")
                {
                    var loadContainerExtendHuDetailList = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerExtend.Id);

                    double cbm = Math.Round((double)loadContainerExtendHuDetailList.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Length * u.Width * u.Height * u.Qty), 4);

                    if (loadContainerExtend.ContainerType == "22G1")
                    {
                        if (cbm > 28)
                        {
                            return "箱型20'GP立方:" + cbm + "已超过警戒立方28！<br />确认保存吗？";
                        }
                    }
                    if (loadContainerExtend.ContainerType == "25G1")
                    {
                        if (cbm > 31)
                        {
                            return "箱型20'HC立方:" + cbm + "已超过警戒立方31！<br />确认保存吗？";
                        }
                    }
                    if (loadContainerExtend.ContainerType == "42G1")
                    {
                        if (cbm > 58)
                        {
                            return "箱型40'GP立方:" + cbm + "已超过警戒立方58！<br />确认保存吗？";
                        }
                    }
                    if (loadContainerExtend.ContainerType == "45G1")
                    {
                        if (cbm > 68)
                        {
                            return "箱型40'HC立方:" + cbm + "已超过警戒立方68！<br />确认保存吗？";
                        }
                    }
                    if (loadContainerExtend.ContainerType == "L5G1")
                    {
                        if (cbm > 78)
                        {
                            return "箱型45'HC立方:" + cbm + "已超过警戒立方78！<br />确认保存吗？";
                        }
                    }
                }
            }

            return "Y";
        }

        //验证 装箱单所选数量 和 已保存的Load数量做比对
        public string CheckLoadContainerQtyToOutDetail(string loadId, string whCode)
        {
            List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

            if (loadContainerExtendList.Count > 0)
            {
                LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();
                var loadContainerExtendHuDetailList = idal.ILoadContainerExtendHuDetailDAL.SelectBy(u => u.LoadContainerId == loadContainerExtend.Id);

                if (loadContainerExtendHuDetailList.Count > 0)
                {
                    var outBoundOrderDetailList = from a in idal.ILoadMasterDAL.SelectAll()
                                                  join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                                                  from b in b_join.DefaultIfEmpty()
                                                  join c in idal.IOutBoundOrderDetailDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.OutBoundOrderId } into c_join
                                                  from c in c_join.DefaultIfEmpty()
                                                  where a.WhCode == whCode && a.LoadId == loadId && c.DSFLag != 1
                                                  select c;

                    if (loadContainerExtendHuDetailList.Sum(u => u.Qty) != outBoundOrderDetailList.Sum(u => u.Qty))
                    {
                        return "装箱单选择库存信息变更过，请先查看已选库存-确认保存后再释放！";
                    }
                    else
                    {
                        if (loadContainerExtendHuDetailList.Max(u => u.CreateDate) > outBoundOrderDetailList.Max(u => u.CreateDate) || loadContainerExtendHuDetailList.Max(u => u.UpdateDate) > outBoundOrderDetailList.Max(u => u.CreateDate))
                        {
                            return "装箱单选择库存信息变更过，请先查看已选库存-确认保存后再释放！";
                        }
                    }
                }
            }

            return "Y";
        }


        //删除装箱单
        public string LoadContainerDelete(int loadContainerId)
        {
            string result = "";
            LoadContainerExtend loadContainer = idal.ILoadContainerExtendDAL.SelectBy(u => u.Id == loadContainerId).First();
            if (!string.IsNullOrEmpty(loadContainer.LoadId))
            {
                LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadContainer.LoadId && u.WhCode == loadContainer.WhCode).First();
                if (loadMaster.Status0 != "U")
                {
                    return "Load状态有误，请重新查询！";
                }
                else
                {
                    List<LoadDetail> loadDetailList = (from a in idal.ILoadDetailDAL.SelectAll()
                                                       where a.LoadMasterId == loadMaster.Id
                                                       select a).ToList();

                    foreach (var item in loadDetailList)
                    {
                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.OutBoundOrderId);
                        if (OutBoundOrderList.Count > 0)
                        {
                            OutBoundOrder eneity = OutBoundOrderList.First();

                            if (eneity.StatusId >= 20)
                            {
                                result = "订单状态有误，无法删除装箱单！";
                                break;
                            }
                            else
                            {
                                if (eneity.StatusId != -10)
                                {
                                    idal.IOutBoundOrderDAL.DeleteBy(u => u.Id == eneity.Id);
                                    idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.OutBoundOrderId == eneity.Id);
                                }
                            }
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    idal.ILoadDetailDAL.DeleteBy(u => u.LoadMasterId == loadMaster.Id);
                    idal.ILoadMasterDAL.DeleteBy(u => u.Id == loadMaster.Id);
                }
            }

            idal.ILoadContainerExtendDAL.DeleteBy(u => u.Id == loadContainerId);
            idal.ILoadContainerExtendHuDetailDAL.DeleteBy(u => u.LoadContainerId == loadContainerId);

            idal.ILoadContainerExtendDAL.SaveChanges();
            return "Y";
        }


        //验证批量导入装箱单
        public string CheckImportLoadContainer(List<ImportLoadContainerResult> entityList)
        {
            string mess = "";
            string mess_bill = "";   //验证船公司和主单号的关系
            string mess_company = "";//验证船公司和主单号的关系

            List<LoadContainerType> containerTypeList = idal.ILoadContainerTypeDAL.SelectAll().ToList();
            List<string> sealNumberList = new List<string>();
            List<string> containerNumberList = new List<string>();
            foreach (var item in entityList)
            {
                if (containerTypeList.Where(u => u.ContainerType == item.ContainerType).Count() == 0)
                {
                    mess += item.ContainerType;
                }
                sealNumberList.Add(item.SealNumber);
                containerNumberList.Add(item.ContainerNumber);

                if (CheckShipBill(item.CarriageName, item.BillNumber, item.DischageCode) == "S")
                {
                    mess_company += item.CarriageName + ",";
                }
                else if (CheckShipBill(item.CarriageName, item.BillNumber, item.DischageCode) == "N")
                {
                    mess_bill += item.BillNumber + ",";
                }
            }
            if (mess != "")
            {
                return "存在错误箱型：" + mess;
            }

            if (mess_bill != "")
            {
                return "船公司代码异常：" + mess_bill;
            }
            if (mess_company != "")
            {
                return "提单号与船公司不匹配，请查看提单号规则：" + mess_company;
            }

            List<string> check1 = (from a in idal.ILoadContainerExtendDAL.SelectAll()
                                   where sealNumberList.Contains(a.SealNumber)
                                   select a.SealNumber).ToList();
            if (check1.Count > 0)
            {
                foreach (var item in check1)
                {
                    mess += item;
                }
            }
            if (mess != "")
            {
                return "封号已存在：" + mess;
            }

            DateTime beginDate = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"));
            DateTime endDate = Convert.ToDateTime(DateTime.Now.AddDays(+1).ToString("yyyy-MM-dd"));

            List<string> check2 = (from a in idal.ILoadContainerExtendDAL.SelectAll()
                                   where containerNumberList.Contains(a.ContainerNumber) && a.CreateDate >= beginDate && a.CreateDate < endDate
                                   select a.ContainerNumber).ToList();
            if (check2.Count > 0)
            {
                foreach (var item in check2)
                {
                    mess += item;
                }
            }
            if (mess != "")
            {
                return "箱号在一个月内已存在：" + mess;
            }



            return "Y";
        }

        //批量导入装箱单
        public string ImportLoadContainer(List<ImportLoadContainerResult> entityList)
        {


            return "Y";
        }

        //验证关单号和船公司是否匹配
        public string CheckShipBill(string CarriageName, string BillNumber, string DischageCode)
        {
            string res = "", head = "", result = "";

            //得出最右边8位
            // BillNumber.Substring(BillNumber.Length - 8, 8)
            //SH98R87654321 得到 87654321

            //得出去除最后8位的前面数据 
            // BillNumber.Substring(0, BillNumber.Length - 8);
            //SH98R87654321 得到 SH98R

            //得出去除最后8位的前面数据的 最右边一位 R
            // BillNumber.Substring(BillNumber.Substring(0, BillNumber.Length - 8).Length - 1);
            //SH98R87654321 得到 SH98R 再得到R

            //纯数字 @"^[0-9]*$"
            //纯字母 @"^[A-Za-z]+$"

            try
            {
                #region 验证提单号与船公司
                if (res == "")
                {
                    if (BillNumber.Length > 2)
                    {
                        //        	 ---'TSC' WT
                        head = BillNumber.Substring(0, 2);
                        if (head == "WT")
                        {
                            res = "31";
                        }
                    }
                }

                if (res == "")
                {
                    if (BillNumber.Length > 3)
                    {
                        //        	--'SGH' or 'HMSGH' + 4位数字        
                        if ((BillNumber.Substring(0, 3) == "SGH" || BillNumber.Substring(0, 5) == "HMSGH") && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 4, 4), @"^[0-9]*$") == true)
                        {
                            res = "33";
                        }
                    }
                }

                if (BillNumber.Length > 4)
                {
                    if (res == "")
                    {
                        //--'APL'APLU+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "APLU" && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                        {
                            res = "01";
                        }
                    }

                    if (res == "")
                    {
                        #region 长度等于10
                        if (BillNumber.Length == 10)
                        {
                            if (res == "")
                            {
                                // --'ANL'ZSZZ+数字 --Z开头的4个字母+6个数字
                                head = BillNumber.Substring(0, 1);
                                if (head == "Z" && Regex.IsMatch(BillNumber.Substring(1, 3), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                                {
                                    res = "02";
                                }
                            }

                            if (res == "")
                            {
                                // --'CNC'ACML/ACPA+6数字
                                head = BillNumber.Substring(0, 4);
                                if ((head == "ACML" || head == "ACPA") && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                                {
                                    res = "06";
                                }
                            }

                            if (res == "")
                            {
                                // --'DMS'DSS+1字母+6数字
                                head = BillNumber.Substring(0, 3);
                                if (head == "DSS" && Regex.IsMatch(BillNumber.Substring(3, 1), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                                {
                                    res = "07";
                                }
                            }

                            if (res == "")
                            {
                                // --'YML'一个字母+9个数字
                                if (Regex.IsMatch(BillNumber.Substring(0, 1), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(1), @"^[0-9]*$") == true)
                                {
                                    res = "24";
                                }
                            }

                        }
                        #endregion
                    }

                    if (res == "")
                    {
                        // --'CMA'C打头4个字母+数字
                        head = BillNumber.Substring(0, 1);
                        if ((head == "C" || head == "W") && Regex.IsMatch(BillNumber.Substring(1, 3), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                        {
                            res = "03";
                        }
                    }

                    if (res == "")
                    {
                        // --'COSCO'COSU+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "COSU" && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                        {
                            res = "05";
                        }
                    }

                    if (res == "")
                    {
                        //  --'EVG'EGLV+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "EGLV" && Regex.IsMatch(BillNumber.Substring(4, BillNumber.Length - 4), @"^[0-9]*$") == true)
                        {
                            res = "08";
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 7)
                        {
                            //  --'HLC'HLCUSHA+数字
                            head = BillNumber.Substring(0, 7);
                            if (head == "HLCUSHA")
                            {
                                res = "09";
                            }
                        }
                    }

                    if (res == "")
                    {
                        //   --'HSD'数字+SHA开头 
                        if (Regex.IsMatch(BillNumber.Substring(0, 1), @"^[0-9]*$") == true && BillNumber.Substring(1, 3) == "SHA")
                        {
                            res = "10";
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 10)
                        {
                            //   --'HJS'SH+字母+后8位数字
                            head = BillNumber.Substring(0, 2);
                            if (head == "SH" && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 8, 8), @"^[0-9]*$") == true && Regex.IsMatch(BillNumber.Substring(BillNumber.Substring(0, BillNumber.Length - 8).Length - 1), @"^[A-Za-z]+$") == true)
                            {
                                res = "11";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 5)
                        {
                            //   --'HMM'HDMU+字母数字
                            head = BillNumber.Substring(0, 4);
                            if (head == "HDMU" && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 1), @"^[0-9]*$") == true)
                            {
                                res = "12";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 6)
                        {
                            //    --'KL'KKLUSH+数字
                            head = BillNumber.Substring(0, 6);
                            if (head == "KKLUSH" && Regex.IsMatch(BillNumber.Substring(6), @"^[0-9]*$") == true)
                            {
                                res = "13";
                            }
                        }
                    }

                    if (res == "")
                    {
                        //     --'MOL'MOLU+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "MOLU" && Regex.IsMatch(BillNumber.Substring(4), @"^[0-9]*$") == true)
                        {
                            res = "14";
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 5)
                        {
                            //    --'MSC'177+字母数字
                            head = BillNumber.Substring(0, 3);
                            if (head == "177" && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 1), @"^[0-9]*$") == true && Regex.IsMatch(BillNumber.Substring(0, 4).Substring(3), @"^[A-Za-z]+$") == true)
                            {
                                res = "15";
                            }
                        }
                    }

                    if (res == "")
                    {
                        //      --'MAT'MATS+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "MATS" && Regex.IsMatch(BillNumber.Substring(4), @"^[0-9]*$") == true)
                        {
                            res = "16";
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length == 9)
                        {
                            //       --'MSK'9位数字
                            if (Regex.IsMatch(BillNumber, @"^[0-9]*$") == true)
                            {
                                res = "17";
                            }
                        }
                    }

                    if (res == "")
                    {
                        //      --'NYK'NYKS+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "NYKS")
                        {
                            res = "18";
                        }
                    }

                    if (res == "")
                    {
                        //       --'OOCL'OOLU+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "OOLU" && Regex.IsMatch(BillNumber.Substring(4), @"^[0-9]*$") == true)
                        {
                            res = "19";
                        }
                    }

                    if (res == "")
                    {
                        //        --'SAF'SGHZ+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "SGHZ" && Regex.IsMatch(BillNumber.Substring(4), @"^[0-9]*$") == true)
                        {
                            res = "21";
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 8)
                        {
                            //         --'SITC'SITGSHTY+数字
                            head = BillNumber.Substring(0, 8);
                            if (head == "SITGSHTY")
                            {
                                res = "22";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length == 11)
                        {
                            //         --'UASC'CNSHA+6数字
                            head = BillNumber.Substring(0, 5);
                            if (head == "CNSHA" && Regex.IsMatch(BillNumber.Substring(5), @"^[0-9]*$") == true)
                            {
                                res = "23";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 7)
                        {
                            //  --'ZIM'ZIMUSNH+数字
                            head = BillNumber.Substring(0, 7);
                            if (head == "ZIMUSNH" && Regex.IsMatch(BillNumber.Substring(7), @"^[0-9]*$") == true)
                            {
                                res = "25";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length == 15)
                        {
                            //        	 --'ZIM' 3位字母+4位数字+5位字母+1位数字
                            if (Regex.IsMatch(BillNumber.Substring(0, 3), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(3, 4), @"^[0-9]*$") == true && Regex.IsMatch(BillNumber.Substring(7, 5), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 1), @"^[0-9]*$") == true)
                            {
                                res = "26";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 5)
                        {
                            //        	---'TSC' SX+3位字母+数字或字母
                            head = BillNumber.Substring(0, 2);
                            if (head == "SX" && Regex.IsMatch(BillNumber.Substring(2, 3), @"^[A-Za-z]+$") == true)
                            {
                                res = "27";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length == 9)
                        {
                            //        	---'TSC' SGH+后4位数字
                            head = BillNumber.Substring(0, 3);
                            if (head == "SGH" && Regex.IsMatch(BillNumber.Substring(5), @"^[0-9]*$") == true)
                            {
                                res = "28";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length == 9)
                        {
                            //        	--'MSK'   MCT +6位数字
                            head = BillNumber.Substring(0, 3);
                            if (head == "MCT" && Regex.IsMatch(BillNumber.Substring(3), @"^[0-9]*$") == true)
                            {
                                res = "29";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 5)
                        {
                            //        	 ---'TSC' SX+字母+数字+字母+数字或字母
                            head = BillNumber.Substring(0, 2);
                            if (head == "SX" && Regex.IsMatch(BillNumber.Substring(2, 1), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(3, 1), @"^[0-9]*$") == true && Regex.IsMatch(BillNumber.Substring(4, 1), @"^[A-Za-z]+$") == true)
                            {
                                res = "30";
                            }
                        }
                    }

                    if (res == "")
                    {
                        if (BillNumber.Length > 2)
                        {
                            //        	 ---'TSC' WT
                            head = BillNumber.Substring(0, 2);
                            if (head == "WT")
                            {
                                res = "31";
                            }
                        }
                    }

                    if (res == "")
                    {
                        //        	 ---'TSC' ONEY +1个字母+最后一位数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "ONEY" && Regex.IsMatch(BillNumber.Substring(4, 1), @"^[A-Za-z]+$") == true && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 1), @"^[0-9]*$") == true)
                        {
                            res = "32";
                        }
                    }

                    if (res == "")
                    {
                        //        --'VF' MCB+6位数字
                        head = BillNumber.Substring(0, 3);
                        if (head == "MCB" && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 6, 6), @"^[0-9]*$") == true)
                        {
                            res = "34";
                        }
                    }

                    if (res == "")
                    {
                        //        --COAU+10个数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "COAU" && Regex.IsMatch(BillNumber.Substring(BillNumber.Length - 10, 10), @"^[0-9]*$") == true)
                        {
                            res = "35";
                        }
                    }

                    if (res == "")
                    {
                        //        	--SHML+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "SHML" && Regex.IsMatch(BillNumber.Substring(4), @"^[0-9]*$") == true)
                        {
                            res = "36";
                        }
                    }

                    if (res == "")
                    {
                        //        		 --KMTC+数字
                        head = BillNumber.Substring(0, 4);
                        if (head == "KMTC")
                        {
                            res = "37";
                        }
                    }
                }

                if (DischageCode.Length >= 3)
                {
                    if (BillNumber.Length == 12)
                    {
                        if (res == "")
                        {
                            //   --'CSC'SHA+中转港+数字
                            head = BillNumber.Substring(0, 6);
                            if ((head == "SHA" + DischageCode.Substring(DischageCode.Length - 2, 2)) && Regex.IsMatch(BillNumber.Substring(6, BillNumber.Length - 6), @"^[0-9]*$") == true)
                            {
                                res = "04";
                            }
                        }
                    }

                    if (BillNumber.Length > 7)
                    {
                        if (res == "")
                        {
                            //   --'PIL'SHAU+中转港+数字
                            head = BillNumber.Substring(0, 7);
                            if ((head == "SHAU" + DischageCode.Substring(DischageCode.Length - 3, 3)) && Regex.IsMatch(BillNumber.Substring(7), @"^[0-9]*$") == true)
                            {
                                res = "20";
                            }
                        }
                    }

                }
                #endregion
            }
            catch
            {
                result = "N";
            }

            if (res == "01")
            {
                if (CarriageName != "APL")
                {
                    result = "N";
                }
            }
            else if (res == "02")
            {
                if (CarriageName != "ANL")
                {
                    result = "N";
                }
            }
            else if (res == "03")
            {
                if (CarriageName != "CMA")
                {
                    result = "N";
                }
            }
            else if (res == "04")
            {
                if (CarriageName != "CSC")
                {
                    result = "N";
                }
            }
            else if (res == "05")
            {
                if (CarriageName != "COSCO")
                {
                    result = "N";
                }
            }
            else if (res == "06")
            {
                if (CarriageName != "CNC")
                {
                    result = "N";
                }
            }
            else if (res == "07")
            {
                if (CarriageName != "DMS")
                {
                    result = "N";
                }
            }
            else if (res == "08")
            {
                if (CarriageName != "EVG")
                {
                    result = "N";
                }
            }
            else if (res == "09")
            {
                if (CarriageName != "HLC")
                {
                    result = "N";
                }
            }
            else if (res == "10")
            {
                if (CarriageName != "HSD")
                {
                    result = "N";
                }
            }
            else if (res == "11")
            {
                if (CarriageName != "HJS" && CarriageName != "SML")
                {
                    result = "N";
                }
            }
            else if (res == "12")
            {
                if (CarriageName != "HMM")
                {
                    result = "N";
                }
            }
            else if (res == "13")
            {
                if (CarriageName != "KL")
                {
                    result = "N";
                }
            }
            else if (res == "14")
            {
                if (CarriageName != "MOL")
                {
                    result = "N";
                }
            }
            else if (res == "15")
            {
                if (CarriageName != "MSC")
                {
                    result = "N";
                }
            }
            else if (res == "16")
            {
                if (CarriageName != "MAT")
                {
                    result = "N";
                }
            }
            else if (res == "17")
            {
                if (CarriageName != "MSK")
                {
                    result = "N";
                }
            }
            else if (res == "18")
            {
                if (CarriageName != "NYK")
                {
                    result = "N";
                }
            }
            else if (res == "19")
            {
                if (CarriageName != "OOCL")
                {
                    result = "N";
                }
            }
            else if (res == "20")
            {
                if (CarriageName != "PIL")
                {
                    result = "N";
                }
            }
            else if (res == "21")
            {
                if (CarriageName != "SAF")
                {
                    result = "N";
                }
            }
            else if (res == "22")
            {
                if (CarriageName != "SITC")
                {
                    result = "N";
                }
            }
            else if (res == "23")
            {
                if (CarriageName != "UASC")
                {
                    result = "N";
                }
            }
            else if (res == "24")
            {
                if (CarriageName != "YML")
                {
                    result = "N";
                }
            }
            else if (res == "25")
            {
                if (CarriageName != "ZIM")
                {
                    result = "N";
                }
            }
            else if (res == "26")
            {
                if (CarriageName != "DMCQ")
                {
                    result = "N";
                }
            }
            else if (res == "27")
            {
                if (CarriageName != "TSC")
                {
                    result = "N";
                }
            }
            else if (res == "28")
            {
                if (CarriageName != "MSK")
                {
                    result = "N";
                }
            }
            else if (res == "29")
            {
                if (CarriageName != "MSK")
                {
                    result = "N";
                }
            }
            else if (res == "30")
            {
                if (CarriageName != "TSC")
                {
                    result = "N";
                }
            }
            else if (res == "31")
            {
                if (CarriageName != "WHL")
                {
                    result = "N";
                }
            }
            else if (res == "32")
            {
                if (CarriageName != "ONE")
                {
                    result = "N";
                }
            }
            else if (res == "33")
            {
                if (CarriageName != "MSK")
                {
                    result = "N";
                }
            }
            else if (res == "34")
            {
                if (CarriageName != "MSK")
                {
                    result = "N";
                }
            }
            else if (res == "35")
            {
                if (CarriageName != "CSE")
                {
                    result = "N";
                }
            }
            else if (res == "36")
            {
                if (CarriageName != "APL")
                {
                    result = "N";
                }
            }
            else if (res == "37")
            {
                if (CarriageName != "KMT")
                {
                    result = "N";
                }
            }
            else if (res == "")
            {
                result = "S";
            }


            return result;
        }


        #endregion


        #region 装箱出货费用计算

        //显示未选择科目列表
        public List<LoadChargeRuleResult> LoadChargeRuleUnselected(LoadChargeRuleSearch searchEntity, out int total)
        {
            //成功选择后 先不要隐藏已选的科目
            //var sql1 = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.LoadId == searchEntity.LoadId);

            var sql = from a in idal.ILoadChargeRuleDAL.SelectAll()
                      where a.FunctionFlag == 1 && (a.SOFeeFlag ?? 0) == 0
                      select new LoadChargeRuleResult
                      {
                          Id = a.Id,
                          FunctionId = a.FunctionId,
                          FunctionName = a.FunctionName,
                          FunctionUnitName = a.FunctionUnitName,
                          TypeName = a.TypeName ?? "",
                          SOFeeFlag = a.SOFeeFlag ?? 0,
                          CustomerFlag = a.CustomerFlag,
                          WarehouseFlag = a.WarehouseFlag,
                          BargainingFlag = a.BargainingFlag,
                          Description = a.WarehouseFlag == 1 ? "仓库录入数量" : "",
                          BargainingDescription = a.BargainingFlag == 1 ? "议价" : "",
                          DaiDianId = a.DaiDianId ?? 0,
                          ClientAutoCharge = a.ClientAutoCharge ?? ""
                      };

            if (!string.IsNullOrEmpty(searchEntity.TypeName))
                sql = sql.Where(u => u.TypeName == searchEntity.TypeName);

            total = sql.Count();
            sql = sql.OrderBy(u => u.FunctionId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //显示已选择科目列表
        public List<LoadChargeDetailResult> LoadChargeRuleSelected(LoadChargeRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadChargeDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId == searchEntity.LoadId
                      && a.LoadChargeRuleId > 0
                      select new LoadChargeDetailResult
                      {
                          Id = a.Id,
                          ChargeUnitName = a.ChargeUnitName,
                          ChargeName = a.ChargeName,
                          QtyCbm = (a.Qty == 0 ? null : a.Qty) + "" + (a.CBM == 0 ? null : a.CBM)
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据客户自动计算某些科目
        public string LoadChargeClientAutoCharge(LoadChargeDetailResult entity)
        {
            List<LoadCharge> loadChargeList = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.Status == "C");
            if (loadChargeList.Count > 0)
            {
                return "该Load已被确认，无法再进行录入操作！";
            }

            LoadChargeRule loadChargeRule = idal.ILoadChargeRuleDAL.SelectBy(u => u.FunctionName == entity.ChargeName).First();

            List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.LoadChargeRuleId > 0);

            //1.检测该ruleId是否已添加至出货费用明细中LoadChargeDetail
            if (list.Where(u => u.LoadChargeRuleId == loadChargeRule.Id).Count() > 0)
            {
                return "收费科目：" + loadChargeRule.FunctionName + "无法重复添加！";
            }

            WhClient client = idal.IWhClientDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode).First();

            //取得客户的出货合同
            List<ContractFormOut> contractList = new List<ContractFormOut>();

            if (!string.IsNullOrEmpty(client.ContractNameOut))
            {
                contractList = idal.IContractFormOutDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ContractName == client.ContractNameOut);
            }
            else
            {
                return "N";
            }
            string[] chargeNameArr = (from a in contractList
                                      select a.ChargeName).ToList().Distinct().ToArray();

            List<LoadChargeDetail> LoadChargeDetailList = new List<LoadChargeDetail>();

            //如果合同中包含该科目，自动计算费用
            if (chargeNameArr.Contains(entity.ChargeName))
            {
                ContractFormOut contract = contractList.Where(u => u.ChargeName == entity.ChargeName).First();

                List<LoadContainerType> loadContainerTypeList = (from a in idal.ILoadContainerTypeDAL.SelectAll()
                                                                 join b in idal.ILoadContainerExtendDAL.SelectAll()
                                                                 on a.ContainerType equals b.ContainerType
                                                                 where b.WhCode == entity.WhCode && b.LoadId == entity.LoadId
                                                                 select a).ToList();

                List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId);
                if (loadContainerExtendList.Count == 0)
                {
                    return "N";
                }

                LoadContainerType containerType = loadContainerTypeList.First();
                LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();

                if (entity.ChargeName.Contains("蛇形装箱费"))
                {
                    LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                    recChargeDetail.WhCode = entity.WhCode;
                    recChargeDetail.LoadId = entity.LoadId;
                    recChargeDetail.NotLoadFlag = 0;
                    recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                    recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                    recChargeDetail.ClientCode = entity.ClientCode;
                    recChargeDetail.ChargeType = entity.ChargeName;
                    recChargeDetail.ChargeName = contract.ChargeName;
                    recChargeDetail.UnitName = contract.ChargeUnitName;
                    recChargeDetail.Qty = 0;
                    recChargeDetail.CBM = loadContainerExtend.TotalCBM;
                    recChargeDetail.Weight = 0;
                    recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                    recChargeDetail.LadderNumber = "";
                    recChargeDetail.Price = contract.Price;
                    recChargeDetail.PriceTotal = recChargeDetail.CBM * recChargeDetail.Price;

                    recChargeDetail.ContainerType = containerType.ClassType;
                    recChargeDetail.CreateDate = DateTime.Now;
                    recChargeDetail.CreateUser = entity.CreateUser;

                    recChargeDetail.ChargeCode = loadChargeRule.ChargeCode;
                    recChargeDetail.ChargeItem = loadChargeRule.ChargeItem;
                    recChargeDetail.TaxRate = loadChargeRule.TaxRate;
                    recChargeDetail.LoadChargeRuleId = loadChargeRule.Id;

                    LoadChargeDetailList.Add(recChargeDetail);
                }
                else if (entity.ChargeName.Contains("扫描费"))
                {
                    LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                    recChargeDetail.WhCode = entity.WhCode;
                    recChargeDetail.LoadId = entity.LoadId;
                    recChargeDetail.NotLoadFlag = 0;
                    recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                    recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                    recChargeDetail.ClientCode = entity.ClientCode;
                    recChargeDetail.ChargeType = entity.ChargeName;
                    recChargeDetail.ChargeName = contract.ChargeName;
                    recChargeDetail.UnitName = contract.ChargeUnitName;

                    recChargeDetail.Qty = loadContainerExtend.TotalQty;
                    recChargeDetail.CBM = 0;
                    recChargeDetail.Weight = 0;
                    recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                    recChargeDetail.LadderNumber = "";
                    recChargeDetail.Price = contract.Price;
                    recChargeDetail.PriceTotal = recChargeDetail.Qty * recChargeDetail.Price;

                    recChargeDetail.ContainerType = containerType.ClassType;
                    recChargeDetail.CreateDate = DateTime.Now;
                    recChargeDetail.CreateUser = entity.CreateUser;

                    recChargeDetail.ChargeCode = loadChargeRule.ChargeCode;
                    recChargeDetail.ChargeItem = loadChargeRule.ChargeItem;
                    recChargeDetail.TaxRate = loadChargeRule.TaxRate;
                    recChargeDetail.LoadChargeRuleId = loadChargeRule.Id;

                    LoadChargeDetailList.Add(recChargeDetail);
                }

            }

            idal.ILoadChargeDetailDAL.Add(LoadChargeDetailList);
            idal.SaveChanges();

            return "Y";
        }

        //出货装箱费用等计算
        public string LoadChargeAdd(string loadId, string whCode, string userName)
        {
            //从备货任务表中取得数据，因库存收货时间等在备货表中存在且不变
            List<PickTaskDetail> getPickTaskDetailList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

            //取得Load的发货信息：箱型等
            List<LoadContainerType> loadContainerTypeList = (from a in idal.ILoadContainerTypeDAL.SelectAll()
                                                             join b in idal.ILoadContainerExtendDAL.SelectAll()
                                                             on a.ContainerType equals b.ContainerType
                                                             where b.WhCode == whCode && b.LoadId == loadId
                                                             select a).ToList();
            if (loadContainerTypeList.Count == 0)
            {
                return "该Load未找到出货箱型等信息！";
            }

            List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
            if (loadContainerExtendList.Count == 0)
            {
                return "该Load未找到出货装箱单信息！";
            }

            LoadContainerType containerType = loadContainerTypeList.First();
            LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();

            //取得Load的客户
            List<string> outBoundList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                         join b in idal.ILoadDetailDAL.SelectAll()
                                         on a.Id equals b.OutBoundOrderId
                                         join c in idal.ILoadMasterDAL.SelectAll()
                                         on b.LoadMasterId equals c.Id
                                         where c.WhCode == whCode && c.LoadId == loadId
                                         select a.ClientCode).ToList();

            string clientCode = outBoundList.First();

            WhClient client = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == clientCode).First();

            //取得客户的出货合同
            List<ContractFormOut> contractList = new List<ContractFormOut>();

            if (!string.IsNullOrEmpty(client.ContractNameOut))
            {
                contractList = idal.IContractFormOutDAL.SelectBy(u => u.WhCode == whCode && u.ContractName == client.ContractNameOut);
            }
            else
            {
                return "N";
            }

            DateTime getShipdate = DateTime.Now;
            LoadMaster lm = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).First();
            if (lm.ShipDate != null)
            {
                getShipdate = Convert.ToDateTime(lm.ShipDate);
            }

            List<LoadChargeRule> ruleList = idal.ILoadChargeRuleDAL.SelectAll().Where(u => u.FunctionFlag == 0).ToList();

            List<ContractFormOut> contractList1 = contractList.Where(u => u.Type == "超期存储").ToList();

            List<ContractFormOut> getContractList = new List<ContractFormOut>();

            List<LoadChargeDetail> LoadChargeDetailList = new List<LoadChargeDetail>();

            string result = "";

            #region 超期存储费计算

            //如果是提货 存储费与正常海运的区分
            if (loadContainerExtend.ChuCangFS == "提货")
            {
                getContractList = contractList.Where(u => u.Type == "超期存储" && u.ChargeName.Contains("提货存储")).ToList();

                #region 1.提货存储费

                if (getContractList.Count > 0)
                {
                    List<string> unitList = new List<string>();
                    unitList.Add("LP");
                    unitList.Add("CTN");
                    unitList.Add("EA-BIG");
                    unitList.Add("EA");
                    unitList.Add("CLOTH-ROLL");

                    List<PickTaskDetail> listNotBig = getPickTaskDetailList.Where(u => unitList.Contains(u.UnitName)).ToList();

                    if (listNotBig.Count > 0)
                    {
                        List<PickTaskDetail> listNotBig1 = new List<PickTaskDetail>();

                        //得到备货数据汇总： 天数、单位、立方的合计
                        foreach (var item in listNotBig)
                        {
                            TimeSpan d3 = getShipdate.Subtract(Convert.ToDateTime(item.ReceiptDate == null ? getShipdate : item.ReceiptDate));
                            item.InvDays = d3.Days;
                            listNotBig1.Add(item);
                        }

                        //select sum(Length*Width*Height*Qty) CBM,UnitName,PickQty  from PickTaskDetail where WhCode='10' and LoadId='LD210303090406208' group by UnitName,PickQty

                        List<PickTaskDetailResult1> listGroupBy = (from picktaskdetail in listNotBig1
                                                                   where
                                                                     picktaskdetail.WhCode == whCode &&
                                                                     picktaskdetail.LoadId == loadId
                                                                   group picktaskdetail by new
                                                                   {
                                                                       picktaskdetail.UnitName,
                                                                       picktaskdetail.InvDays
                                                                   } into g
                                                                   select new PickTaskDetailResult1
                                                                   {
                                                                       CBM = g.Sum(p => p.Length * p.Width * p.Height * p.Qty),
                                                                       UnitName = g.Key.UnitName,
                                                                       Days = (Int32?)g.Key.InvDays
                                                                   }).ToList();

                        List<LoadChargeDetail> checkLoadChargeDetail = new List<LoadChargeDetail>();
                        List<ContractFormOut> checkLadderNumber = new List<ContractFormOut>();

                        foreach (var item in listGroupBy)
                        {
                            List<ContractFormOut> checkContractCount = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.Days && (u.LadderNumberEnd ?? 0) > item.Days).ToList();

                            foreach (var contract in checkContractCount)
                            {
                                LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                                recChargeDetail.WhCode = whCode;
                                recChargeDetail.LoadId = loadId;
                                recChargeDetail.NotLoadFlag = 0;
                                recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                                recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "提货存储费";
                                recChargeDetail.ChargeName = contract.ChargeName;
                                recChargeDetail.UnitName = item.UnitName;
                                recChargeDetail.Qty = 0;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = 0;
                                recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.CBM;
                                recChargeDetail.ContainerType = "";
                                recChargeDetail.CreateDate = DateTime.Now;
                                recChargeDetail.CreateUser = userName;

                                if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                                {
                                    recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                    recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;

                                    recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                                }
                                else
                                {
                                    recChargeDetail.ChargeCode = "";
                                    recChargeDetail.ChargeItem = "";
                                    recChargeDetail.TaxRate = 0;
                                }

                                if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.ChargeName == contract.ChargeName).Count() == 0)
                                {
                                    checkLadderNumber.Add(contract);
                                    checkLoadChargeDetail.Add(recChargeDetail);
                                }
                                else
                                {
                                    LoadChargeDetail oldrecChargeDetail = checkLoadChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd) && u.ChargeName == contract.ChargeName).First();

                                    checkLoadChargeDetail.Remove(oldrecChargeDetail);

                                    LoadChargeDetail newrecChargeDetail = recChargeDetail;
                                    newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                    newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                    newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                    newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                    checkLoadChargeDetail.Add(newrecChargeDetail);
                                }
                            }
                        }

                        foreach (var item in checkLoadChargeDetail)
                        {
                            LoadChargeDetailList.Add(item);
                        }

                        listNotBig.Clear();
                    }
                }
                else
                {
                    result = "合同名:" + client.ContractNameOut + ",提货存储费 没有找到合同明细,无法计算费用";
                }

                if (result != "")
                {
                    LoadCharge recCharge = new LoadCharge();
                    recCharge.WhCode = whCode;
                    recCharge.ClientCode = clientCode;
                    recCharge.LoadId = loadId;
                    recCharge.WhCode = whCode;
                    recCharge.Status = "N";
                    recCharge.Description = "类型:超期存储-" + result;
                    recCharge.CreateUser = userName;
                    recCharge.CreateDate = DateTime.Now;
                    idal.ILoadChargeDAL.Add(recCharge);
                    idal.ILoadChargeDAL.SaveChanges();
                    return "N";
                }

                #endregion


                getContractList = contractList.Where(u => u.Type == "基础" && u.ChargeName.Contains("提货车辆")).ToList();

                #region 2.提货车辆管理费

                if (getContractList.Count > 0)
                {
                    foreach (var contract in getContractList)
                    {
                        LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                        recChargeDetail.WhCode = whCode;
                        recChargeDetail.LoadId = loadId;
                        recChargeDetail.NotLoadFlag = 0;
                        recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                        recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "提货车辆管理费";
                        recChargeDetail.ChargeName = contract.ChargeName;
                        recChargeDetail.UnitName = contract.ChargeUnitName;
                        recChargeDetail.Qty = 0;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                        recChargeDetail.LadderNumber = "";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price;
                        recChargeDetail.ContainerType = containerType.ClassType;
                        recChargeDetail.CreateDate = DateTime.Now;
                        recChargeDetail.CreateUser = userName;

                        if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                        {
                            recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                            recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                            recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                        }
                        else
                        {
                            recChargeDetail.ChargeCode = "";
                            recChargeDetail.ChargeItem = "";
                            recChargeDetail.TaxRate = 0;
                        }

                        LoadChargeDetailList.Add(recChargeDetail);
                    }
                }
                else
                {
                    result = "合同名:" + client.ContractNameOut + ",提货车辆管理费 没有找到合同明细,无法计算费用";
                }

                if (result != "")
                {
                    LoadCharge recCharge = new LoadCharge();
                    recCharge.WhCode = whCode;
                    recCharge.ClientCode = clientCode;
                    recCharge.LoadId = loadId;
                    recCharge.WhCode = whCode;
                    recCharge.Status = "N";
                    recCharge.Description = "类型:基础-" + result;
                    recCharge.CreateUser = userName;
                    recCharge.CreateDate = DateTime.Now;
                    idal.ILoadChargeDAL.Add(recCharge);
                    idal.ILoadChargeDAL.SaveChanges();
                    return "N";
                }

                #endregion


                getContractList = contractList.Where(u => u.Type == "基础" && u.ChargeName.Contains("提货打单")).ToList();

                #region 3.提货打单费

                if (getContractList.Count > 0)
                {
                    //得到出货SO数量，按SO收费
                    List<string> soCountList = (from a in getPickTaskDetailList
                                                select a.SoNumber).ToList().Distinct().ToList();

                    foreach (var contract in getContractList)
                    {
                        LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                        recChargeDetail.WhCode = whCode;
                        recChargeDetail.LoadId = loadId;
                        recChargeDetail.NotLoadFlag = 0;
                        recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                        recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "提货打单费";
                        recChargeDetail.ChargeName = contract.ChargeName;
                        recChargeDetail.UnitName = contract.ChargeUnitName;
                        recChargeDetail.Qty = soCountList.Count;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                        recChargeDetail.LadderNumber = "";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price * soCountList.Count;
                        recChargeDetail.ContainerType = containerType.ClassType;
                        recChargeDetail.CreateDate = DateTime.Now;
                        recChargeDetail.CreateUser = userName;

                        if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                        {
                            recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                            recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                            recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                        }
                        else
                        {
                            recChargeDetail.ChargeCode = "";
                            recChargeDetail.ChargeItem = "";
                            recChargeDetail.TaxRate = 0;
                        }

                        LoadChargeDetailList.Add(recChargeDetail);
                    }
                }
                else
                {
                    result = "合同名:" + client.ContractNameOut + ",提货打单费 没有找到合同明细,无法计算费用";
                }

                if (result != "")
                {
                    LoadCharge recCharge = new LoadCharge();
                    recCharge.WhCode = whCode;
                    recCharge.ClientCode = clientCode;
                    recCharge.LoadId = loadId;
                    recCharge.WhCode = whCode;
                    recCharge.Status = "N";
                    recCharge.Description = "类型:基础-" + result;
                    recCharge.CreateUser = userName;
                    recCharge.CreateDate = DateTime.Now;
                    idal.ILoadChargeDAL.Add(recCharge);
                    idal.ILoadChargeDAL.SaveChanges();
                    return "N";
                }

                #endregion

            }
            else
            {
                getContractList = contractList1.Where(u => u.ChargeName.Contains("非大件")).ToList();

                #region 1.超期存储费（非大件）

                if (getContractList.Count > 0)
                {
                    List<string> unitList = new List<string>();
                    unitList.Add("LP");
                    unitList.Add("CTN");
                    unitList.Add("EA");
                    unitList.Add("CLOTH-ROLL");

                    List<PickTaskDetail> listNotBig = getPickTaskDetailList.Where(u => unitList.Contains(u.UnitName)).ToList();

                    if (listNotBig.Count > 0)
                    {
                        List<PickTaskDetail> listNotBig1 = new List<PickTaskDetail>();

                        //得到备货数据汇总： 天数、单位、立方的合计
                        foreach (var item in listNotBig)
                        {
                            TimeSpan d3 = getShipdate.Subtract(Convert.ToDateTime(item.ReceiptDate == null ? getShipdate : item.ReceiptDate));
                            item.InvDays = d3.Days;
                            listNotBig1.Add(item);
                        }

                        //select sum(Length*Width*Height*Qty) CBM,UnitName,PickQty  from PickTaskDetail where WhCode='10' and LoadId='LD210303090406208' group by UnitName,PickQty

                        List<PickTaskDetailResult1> listGroupBy = (from picktaskdetail in listNotBig1
                                                                   where
                                                                     picktaskdetail.WhCode == whCode &&
                                                                     picktaskdetail.LoadId == loadId
                                                                   group picktaskdetail by new
                                                                   {
                                                                       picktaskdetail.UnitName,
                                                                       picktaskdetail.InvDays
                                                                   } into g
                                                                   select new PickTaskDetailResult1
                                                                   {
                                                                       CBM = g.Sum(p => p.Length * p.Width * p.Height * p.Qty),
                                                                       UnitName = g.Key.UnitName,
                                                                       Days = (Int32?)g.Key.InvDays
                                                                   }).ToList();

                        List<LoadChargeDetail> checkLoadChargeDetail = new List<LoadChargeDetail>();
                        List<ContractFormOut> checkLadderNumber = new List<ContractFormOut>();

                        foreach (var item in listGroupBy)
                        {
                            List<ContractFormOut> checkContractCount = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.Days && (u.LadderNumberEnd ?? 0) > item.Days).ToList();

                            foreach (var contract in checkContractCount)
                            {
                                LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                                recChargeDetail.WhCode = whCode;
                                recChargeDetail.LoadId = loadId;
                                recChargeDetail.NotLoadFlag = 0;
                                recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                                recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "超期存储费";
                                recChargeDetail.ChargeName = contract.ChargeName;
                                recChargeDetail.UnitName = item.UnitName;
                                recChargeDetail.Qty = 0;
                                recChargeDetail.CBM = item.CBM;
                                recChargeDetail.Weight = 0;
                                recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.CBM;
                                recChargeDetail.ContainerType = "";
                                recChargeDetail.CreateDate = DateTime.Now;
                                recChargeDetail.CreateUser = userName;

                                if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                                {
                                    recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                    recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                    recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                                }
                                else
                                {
                                    recChargeDetail.ChargeCode = "";
                                    recChargeDetail.ChargeItem = "";
                                    recChargeDetail.TaxRate = 0;
                                }

                                if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.ChargeName == contract.ChargeName).Count() == 0)
                                {
                                    checkLadderNumber.Add(contract);
                                    checkLoadChargeDetail.Add(recChargeDetail);
                                }
                                else
                                {
                                    LoadChargeDetail oldrecChargeDetail = checkLoadChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd) && u.ChargeName == contract.ChargeName).First();

                                    checkLoadChargeDetail.Remove(oldrecChargeDetail);

                                    LoadChargeDetail newrecChargeDetail = recChargeDetail;
                                    newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                    newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                    newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                    newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                    checkLoadChargeDetail.Add(newrecChargeDetail);
                                }
                            }
                        }

                        foreach (var item in checkLoadChargeDetail)
                        {
                            LoadChargeDetailList.Add(item);
                        }

                        listNotBig1.Clear();
                    }
                }

                #endregion


                getContractList = contractList1.Where(u => u.ChargeName.Contains("厚挂衣")).ToList();

                #region 2.超期存储费（厚挂衣）

                if (getContractList.Count > 0)
                {
                    List<string> unitList = new List<string>();
                    unitList.Add("ECH-THICK");

                    List<PickTaskDetail> listNotBig = getPickTaskDetailList.Where(u => unitList.Contains(u.UnitName)).ToList();

                    if (listNotBig.Count > 0)
                    {
                        List<PickTaskDetail> listNotBig1 = new List<PickTaskDetail>();

                        //得到备货数据汇总： 天数、单位、立方的合计
                        foreach (var item in listNotBig)
                        {
                            TimeSpan d3 = getShipdate.Subtract(Convert.ToDateTime(item.ReceiptDate == null ? getShipdate : item.ReceiptDate));
                            item.InvDays = d3.Days;
                            listNotBig1.Add(item);
                        }

                        //select sum(Qty) Qty,UnitName,PickQty  from PickTaskDetail where WhCode='10' and LoadId='LD210303090406208' group by UnitName,PickQty

                        List<PickTaskDetailResult1> listGroupBy = (from picktaskdetail in listNotBig1
                                                                   where
                                                                     picktaskdetail.WhCode == whCode &&
                                                                     picktaskdetail.LoadId == loadId
                                                                   group picktaskdetail by new
                                                                   {
                                                                       picktaskdetail.UnitName,
                                                                       picktaskdetail.InvDays
                                                                   } into g
                                                                   select new PickTaskDetailResult1
                                                                   {
                                                                       Qty = g.Sum(p => p.Qty),
                                                                       UnitName = g.Key.UnitName,
                                                                       Days = (Int32?)g.Key.InvDays
                                                                   }).ToList();
                        List<LoadChargeDetail> checkLoadChargeDetail = new List<LoadChargeDetail>();
                        List<ContractFormOut> checkLadderNumber = new List<ContractFormOut>();

                        foreach (var item in listGroupBy)
                        {
                            List<ContractFormOut> checkContractCount = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.Days && (u.LadderNumberEnd ?? 0) > item.Days).ToList();

                            foreach (var contract in checkContractCount)
                            {
                                LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                                recChargeDetail.WhCode = whCode;
                                recChargeDetail.LoadId = loadId;
                                recChargeDetail.NotLoadFlag = 0;
                                recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                                recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "超期存储费";
                                recChargeDetail.ChargeName = contract.ChargeName;
                                recChargeDetail.UnitName = item.UnitName;
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = 0;
                                recChargeDetail.Weight = 0;
                                recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.Qty;
                                recChargeDetail.ContainerType = "";
                                recChargeDetail.CreateDate = DateTime.Now;
                                recChargeDetail.CreateUser = userName;

                                if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                                {
                                    recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                    recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                    recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                                }
                                else
                                {
                                    recChargeDetail.ChargeCode = "";
                                    recChargeDetail.ChargeItem = "";
                                    recChargeDetail.TaxRate = 0;
                                }

                                if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.ChargeName == contract.ChargeName).Count() == 0)
                                {
                                    checkLadderNumber.Add(contract);
                                    checkLoadChargeDetail.Add(recChargeDetail);
                                }
                                else
                                {
                                    LoadChargeDetail oldrecChargeDetail = checkLoadChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd) && u.ChargeName == contract.ChargeName).First();

                                    checkLoadChargeDetail.Remove(oldrecChargeDetail);

                                    LoadChargeDetail newrecChargeDetail = recChargeDetail;
                                    newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                    newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                    newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                    newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                    checkLoadChargeDetail.Add(newrecChargeDetail);
                                }
                            }
                        }

                        foreach (var item in checkLoadChargeDetail)
                        {
                            LoadChargeDetailList.Add(item);
                        }

                        listNotBig1.Clear();
                    }
                }
                #endregion


                getContractList = contractList1.Where(u => u.ChargeName.Contains("薄挂衣")).ToList();

                #region 3.超期存储费（薄挂衣）

                if (getContractList.Count > 0)
                {
                    List<string> unitList = new List<string>();
                    unitList.Add("ECH-THIN");

                    List<PickTaskDetail> listNotBig = getPickTaskDetailList.Where(u => unitList.Contains(u.UnitName)).ToList();

                    if (listNotBig.Count > 0)
                    {
                        List<PickTaskDetail> listNotBig1 = new List<PickTaskDetail>();

                        //得到备货数据汇总： 天数、单位、立方的合计
                        foreach (var item in listNotBig)
                        {
                            TimeSpan d3 = getShipdate.Subtract(Convert.ToDateTime(item.ReceiptDate == null ? getShipdate : item.ReceiptDate));
                            item.InvDays = d3.Days;
                            listNotBig1.Add(item);
                        }

                        //select sum(Qty) Qty,UnitName,PickQty  from PickTaskDetail where WhCode='10' and LoadId='LD210303090406208' group by UnitName,PickQty

                        List<PickTaskDetailResult1> listGroupBy = (from picktaskdetail in listNotBig1
                                                                   where
                                                                     picktaskdetail.WhCode == whCode &&
                                                                     picktaskdetail.LoadId == loadId
                                                                   group picktaskdetail by new
                                                                   {
                                                                       picktaskdetail.UnitName,
                                                                       picktaskdetail.InvDays
                                                                   } into g
                                                                   select new PickTaskDetailResult1
                                                                   {
                                                                       Qty = g.Sum(p => p.Qty),
                                                                       UnitName = g.Key.UnitName,
                                                                       Days = (Int32?)g.Key.InvDays
                                                                   }).ToList();
                        List<LoadChargeDetail> checkLoadChargeDetail = new List<LoadChargeDetail>();
                        List<ContractFormOut> checkLadderNumber = new List<ContractFormOut>();

                        foreach (var item in listGroupBy)
                        {
                            List<ContractFormOut> checkContractCount = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= item.Days && (u.LadderNumberEnd ?? 0) > item.Days).ToList();

                            foreach (var contract in checkContractCount)
                            {
                                LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                                recChargeDetail.WhCode = whCode;
                                recChargeDetail.LoadId = loadId;
                                recChargeDetail.NotLoadFlag = 0;
                                recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                                recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                                recChargeDetail.ClientCode = clientCode;
                                recChargeDetail.ChargeType = "超期存储费";
                                recChargeDetail.ChargeName = contract.ChargeName;
                                recChargeDetail.UnitName = item.UnitName;
                                recChargeDetail.Qty = item.Qty;
                                recChargeDetail.CBM = 0;
                                recChargeDetail.Weight = 0;
                                recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                                recChargeDetail.LadderNumber = contract.LadderNumberBegin + "-" + contract.LadderNumberEnd;
                                recChargeDetail.Price = contract.Price;
                                recChargeDetail.PriceTotal = contract.Price * item.Qty;
                                recChargeDetail.ContainerType = "";
                                recChargeDetail.CreateDate = DateTime.Now;
                                recChargeDetail.CreateUser = userName;

                                if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                                {
                                    recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                    recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                    recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                                }
                                else
                                {
                                    recChargeDetail.ChargeCode = "";
                                    recChargeDetail.ChargeItem = "";
                                    recChargeDetail.TaxRate = 0;
                                }

                                if (checkLadderNumber.Where(u => u.LadderNumberBegin == contract.LadderNumberBegin && u.LadderNumberEnd == contract.LadderNumberEnd && u.ChargeName == contract.ChargeName).Count() == 0)
                                {
                                    checkLadderNumber.Add(contract);
                                    checkLoadChargeDetail.Add(recChargeDetail);
                                }
                                else
                                {
                                    LoadChargeDetail oldrecChargeDetail = checkLoadChargeDetail.Where(u => u.LadderNumber.Contains(contract.LadderNumberBegin + "-" + contract.LadderNumberEnd) && u.ChargeName == contract.ChargeName).First();

                                    checkLoadChargeDetail.Remove(oldrecChargeDetail);

                                    LoadChargeDetail newrecChargeDetail = recChargeDetail;
                                    newrecChargeDetail.PriceTotal += oldrecChargeDetail.PriceTotal;
                                    newrecChargeDetail.Qty += oldrecChargeDetail.Qty;
                                    newrecChargeDetail.CBM += oldrecChargeDetail.CBM;
                                    newrecChargeDetail.Weight += oldrecChargeDetail.Weight;

                                    checkLoadChargeDetail.Add(newrecChargeDetail);
                                }
                            }
                        }

                        foreach (var item in checkLoadChargeDetail)
                        {
                            LoadChargeDetailList.Add(item);
                        }

                        listNotBig1.Clear();
                    }
                }
                #endregion

            }

            #endregion


            contractList1 = contractList.Where(u => u.Type == "基础").ToList();
            getContractList = contractList1.Where(u => u.ChargeName.Contains("装箱费")).ToList();

            #region 4.装箱费

            if (getContractList.Count > 0)
            {
                decimal? sumCbm = loadContainerExtend.TotalCBM ?? 0;

                //得到箱型 20 40 45的合同明细行 getContractList.Where(u => u.ContainerType == containerType.ClassType)
                foreach (var contract in getContractList)
                {
                    LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                    recChargeDetail.WhCode = whCode;
                    recChargeDetail.LoadId = loadId;
                    recChargeDetail.NotLoadFlag = 0;
                    recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                    recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                    recChargeDetail.ClientCode = clientCode;
                    recChargeDetail.ChargeType = "装箱费";
                    recChargeDetail.ChargeName = contract.ChargeName;
                    recChargeDetail.UnitName = contract.ChargeUnitName;
                    recChargeDetail.Qty = 0;
                    recChargeDetail.CBM = sumCbm;
                    recChargeDetail.Weight = 0;
                    recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                    recChargeDetail.LadderNumber = "";
                    recChargeDetail.Price = contract.Price;
                    recChargeDetail.PriceTotal = contract.Price * sumCbm;
                    recChargeDetail.ContainerType = containerType.ClassType;
                    recChargeDetail.CreateDate = DateTime.Now;
                    recChargeDetail.CreateUser = userName;

                    if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                    {
                        recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                        recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                        recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                    }
                    else
                    {
                        recChargeDetail.ChargeCode = "";
                        recChargeDetail.ChargeItem = "";
                        recChargeDetail.TaxRate = 0;
                    }

                    LoadChargeDetailList.Add(recChargeDetail);
                }
            }
            else
            {
                result = "合同名:" + client.ContractNameOut + ",装箱费 没有找到合同明细,无法计算费用";
            }

            if (result != "")
            {
                LoadCharge recCharge = new LoadCharge();
                recCharge.WhCode = whCode;
                recCharge.ClientCode = clientCode;
                recCharge.LoadId = loadId;
                recCharge.WhCode = whCode;
                recCharge.Status = "N";
                recCharge.Description = "类型:基础-" + result;
                recCharge.CreateUser = userName;
                recCharge.CreateDate = DateTime.Now;
                idal.ILoadChargeDAL.Add(recCharge);
                idal.ILoadChargeDAL.SaveChanges();
                return "N";
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("挂衣操作费")).ToList();

            #region 5.挂衣操作费

            if (getContractList.Count > 0)
            {
                if (!string.IsNullOrEmpty(loadContainerExtend.BaQty))
                {
                    string s = loadContainerExtend.BaQty.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
                    if (!string.IsNullOrEmpty(s))
                    {
                        foreach (var contract in getContractList)
                        {
                            LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                            recChargeDetail.WhCode = whCode;
                            recChargeDetail.LoadId = loadId;
                            recChargeDetail.NotLoadFlag = 0;
                            recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                            recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "挂衣操作费";
                            recChargeDetail.ChargeName = contract.ChargeName;
                            recChargeDetail.UnitName = contract.ChargeUnitName;
                            recChargeDetail.Qty = 0;
                            recChargeDetail.CBM = 0;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                            recChargeDetail.LadderNumber = "挂衣把数：" + loadContainerExtend.BaQty;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * Convert.ToDecimal(loadContainerExtend.BaQty.Replace("\r\n", "").Replace("\n", "").Replace("\r", ""));
                            recChargeDetail.ContainerType = containerType.ClassType;
                            recChargeDetail.CreateDate = DateTime.Now;
                            recChargeDetail.CreateUser = userName;

                            if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                            {
                                recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                            }
                            else
                            {
                                recChargeDetail.ChargeCode = "";
                                recChargeDetail.ChargeItem = "";
                                recChargeDetail.TaxRate = 0;
                            }

                            LoadChargeDetailList.Add(recChargeDetail);
                        }
                    }
                }
            }

            if (result != "")
            {
                LoadCharge recCharge = new LoadCharge();
                recCharge.WhCode = whCode;
                recCharge.ClientCode = clientCode;
                recCharge.LoadId = loadId;
                recCharge.WhCode = whCode;
                recCharge.Status = "N";
                recCharge.Description = "类型:基础-" + result;
                recCharge.CreateUser = userName;
                recCharge.CreateDate = DateTime.Now;
                idal.ILoadChargeDAL.Add(recCharge);
                idal.ILoadChargeDAL.SaveChanges();
                return "N";
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("出货SN扫描")).ToList();

            #region 6.出货SN扫描费

            if (getContractList.Count > 0)
            {
                List<SerialNumberOut> seriList = idal.ISerialNumberOutDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

                foreach (var contract in getContractList)
                {
                    LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                    recChargeDetail.WhCode = whCode;
                    recChargeDetail.LoadId = loadId;
                    recChargeDetail.NotLoadFlag = 0;
                    recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                    recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                    recChargeDetail.ClientCode = clientCode;
                    recChargeDetail.ChargeType = "出货SN扫描费";
                    recChargeDetail.ChargeName = contract.ChargeName;
                    recChargeDetail.UnitName = "件";
                    recChargeDetail.Qty = seriList.Count;
                    recChargeDetail.CBM = 0;
                    recChargeDetail.Weight = 0;
                    recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                    recChargeDetail.LadderNumber = "";
                    recChargeDetail.Price = contract.Price;
                    recChargeDetail.PriceTotal = contract.Price * seriList.Count;
                    recChargeDetail.ContainerType = containerType.ClassType;
                    recChargeDetail.CreateDate = DateTime.Now;
                    recChargeDetail.CreateUser = userName;

                    if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                    {
                        recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                        recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                        recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                    }
                    else
                    {
                        recChargeDetail.ChargeCode = "";
                        recChargeDetail.ChargeItem = "";
                        recChargeDetail.TaxRate = 0;
                    }

                    LoadChargeDetailList.Add(recChargeDetail);
                }
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("VGM称重")).ToList();

            #region 7.VGM称重

            if (getContractList.Count > 0)
            {
                if (loadContainerExtend.WeightFlag == 1)
                {
                    foreach (var contract in getContractList)
                    {
                        LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                        recChargeDetail.WhCode = whCode;
                        recChargeDetail.LoadId = loadId;
                        recChargeDetail.NotLoadFlag = 0;
                        recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                        recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "VGM称重费";
                        recChargeDetail.ChargeName = contract.ChargeName;
                        recChargeDetail.UnitName = contract.ChargeUnitName;
                        recChargeDetail.Qty = 1;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                        recChargeDetail.LadderNumber = "";
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price;
                        recChargeDetail.ContainerType = containerType.ClassType;
                        recChargeDetail.CreateDate = DateTime.Now;
                        recChargeDetail.CreateUser = userName;

                        if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                        {
                            recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                            recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                            recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                        }
                        else
                        {
                            recChargeDetail.ChargeCode = "";
                            recChargeDetail.ChargeItem = "";
                            recChargeDetail.TaxRate = 0;
                        }

                        LoadChargeDetailList.Add(recChargeDetail);
                    }
                }
            }
            else
            {
                result = "合同名:" + client.ContractNameOut + ",VGM称重费 没有找到合同明细,无法计算费用";
            }

            if (result != "")
            {
                LoadCharge recCharge = new LoadCharge();
                recCharge.WhCode = whCode;
                recCharge.ClientCode = clientCode;
                recCharge.LoadId = loadId;
                recCharge.WhCode = whCode;
                recCharge.Status = "N";
                recCharge.Description = "类型:基础-" + result;
                recCharge.CreateUser = userName;
                recCharge.CreateDate = DateTime.Now;
                idal.ILoadChargeDAL.Add(recCharge);
                idal.ILoadChargeDAL.SaveChanges();
                return "N";
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("进港")).ToList();

            #region 8.进港费

            if (getContractList.Count > 0)
            {
                if (!string.IsNullOrEmpty(loadContainerExtend.PortSuitcase) && loadContainerExtend.PortSuitcase.IndexOf("-") > 0)
                {
                    string PortSuitcase = loadContainerExtend.PortSuitcase; //提箱点-进港点

                    //Y1-Y3
                    //W-W3
                    //Y-W
                    string suitcase = PortSuitcase.Substring(0, PortSuitcase.IndexOf("-"));
                    string port = PortSuitcase.Substring(PortSuitcase.IndexOf("-") + 1, PortSuitcase.Length - (PortSuitcase.IndexOf("-") + 1));

                    string tixiang = "", jingang = "";

                    if (suitcase == "Y")
                    {
                        tixiang = "YS";
                    }
                    else if (suitcase == "W" || suitcase == "W1" || suitcase == "W2" || suitcase == "W3" || suitcase == "W4" || suitcase == "W5" || suitcase == "WB")
                    {
                        tixiang = "WGQ";
                    }
                    else if (suitcase == "Y1" || suitcase == "Y2" || suitcase == "Y3" || suitcase == "Y4" || suitcase == "Y5")
                    {
                        tixiang = "YSD";
                    }

                    if (port == "Y")
                    {
                        jingang = "YS";
                    }
                    else if (port == "W" || port == "W1" || port == "W2" || port == "W3" || port == "W4" || port == "W5" || suitcase == "WB")
                    {
                        jingang = "WGQ";
                    }
                    else if (port == "Y1" || port == "Y2" || port == "Y3" || port == "Y4" || port == "Y5")
                    {
                        jingang = "YSD";
                    }

                    if (tixiang == "" || jingang == "")
                    {
                        result = "合同名:" + client.ContractNameOut + ",进港费 提箱-进港点:" + PortSuitcase + "值异常,无法计算费用";
                    }
                    else
                    {
                        if (getContractList.Where(u => u.Port == jingang && u.SuitCase == tixiang).Count() == 0)
                        {
                            result = "合同名:" + client.ContractNameOut + ",进港费 提箱-进港点:" + PortSuitcase + "未找到合同明细,无法计算费用";
                        }
                        else
                        {
                            getContractList = getContractList.Where(u => u.Port == jingang && u.SuitCase == tixiang).ToList();
                        }


                        if (result != "")
                        {
                            LoadCharge recCharge = new LoadCharge();
                            recCharge.WhCode = whCode;
                            recCharge.ClientCode = clientCode;
                            recCharge.LoadId = loadId;
                            recCharge.WhCode = whCode;
                            recCharge.Status = "N";
                            recCharge.Description = "类型:基础-" + result;
                            recCharge.CreateUser = userName;
                            recCharge.CreateDate = DateTime.Now;
                            idal.ILoadChargeDAL.Add(recCharge);
                            idal.ILoadChargeDAL.SaveChanges();
                            return "N";
                        }

                        decimal? sumCbm = loadContainerExtend.TotalCBM;

                        foreach (var contract in getContractList)
                        {
                            LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                            recChargeDetail.WhCode = whCode;
                            recChargeDetail.LoadId = loadId;
                            recChargeDetail.NotLoadFlag = 0;
                            recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                            recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "进港费";
                            recChargeDetail.ChargeName = contract.ChargeName;
                            recChargeDetail.UnitName = contract.ChargeUnitName;
                            recChargeDetail.Qty = 0;
                            recChargeDetail.CBM = sumCbm;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                            recChargeDetail.LadderNumber = "提箱-进港点：" + PortSuitcase + " 合同点：" + contract.SuitCase + "-" + contract.Port;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * sumCbm;
                            recChargeDetail.ContainerType = containerType.ClassType;
                            recChargeDetail.CreateDate = DateTime.Now;
                            recChargeDetail.CreateUser = userName;

                            if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                            {
                                recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                            }
                            else
                            {
                                recChargeDetail.ChargeCode = "";
                                recChargeDetail.ChargeItem = "";
                                recChargeDetail.TaxRate = 0;
                            }

                            LoadChargeDetailList.Add(recChargeDetail);
                        }
                    }
                }
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("提箱加收")).ToList();

            #region 9.提箱加收费

            if (getContractList.Count > 0)
            {
                if (!string.IsNullOrEmpty(loadContainerExtend.PortSuitcase) && loadContainerExtend.PortSuitcase.IndexOf("-") > 0)
                {
                    string PortSuitcase = loadContainerExtend.PortSuitcase; //提箱点-进港点

                    //Y1-Y3
                    //W-W3
                    //Y-W
                    string suitcase = PortSuitcase.Substring(0, PortSuitcase.IndexOf("-"));
                    string port = PortSuitcase.Substring(PortSuitcase.IndexOf("-") + 1, PortSuitcase.Length - (PortSuitcase.IndexOf("-") + 1));

                    string tixiang = "", jingang = "";

                    if (suitcase == "Y")
                    {
                        tixiang = "YS";
                    }
                    else if (suitcase == "W" || suitcase == "W1" || suitcase == "W2" || suitcase == "W3" || suitcase == "W4" || suitcase == "W5")
                    {
                        tixiang = "WGQ";
                    }
                    else if (suitcase == "Y1" || suitcase == "Y2" || suitcase == "Y3" || suitcase == "Y4" || suitcase == "Y5")
                    {
                        tixiang = "YSD";
                    }
                    else if (suitcase == "WB")
                    {
                        tixiang = "WB";
                    }

                    if (port == "Y")
                    {
                        jingang = "YS";
                    }
                    else if (port == "W" || port == "W1" || port == "W2" || port == "W3" || port == "W4" || port == "W5")
                    {
                        jingang = "WGQ";
                    }
                    else if (port == "Y1" || port == "Y2" || port == "Y3" || port == "Y4" || port == "Y5")
                    {
                        jingang = "YSD";
                    }
                    else if (port == "WB")
                    {
                        jingang = "WB";
                    }

                    if (tixiang == "YSD")
                    {
                        foreach (var contract in getContractList.Where(u => u.SuitCase == tixiang && u.ContainerType == containerType.ClassType))
                        {
                            LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                            recChargeDetail.WhCode = whCode;
                            recChargeDetail.LoadId = loadId;
                            recChargeDetail.NotLoadFlag = 0;
                            recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                            recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "洋岛提箱加收费";
                            recChargeDetail.ChargeName = contract.ChargeName;
                            recChargeDetail.UnitName = contract.ChargeUnitName;
                            recChargeDetail.Qty = 1;
                            recChargeDetail.CBM = 0;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                            recChargeDetail.LadderNumber = "提箱-进港点：" + PortSuitcase;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price;
                            recChargeDetail.ContainerType = containerType.ClassType;
                            recChargeDetail.CreateDate = DateTime.Now;
                            recChargeDetail.CreateUser = userName;

                            if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                            {
                                recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                            }
                            else
                            {
                                recChargeDetail.ChargeCode = "";
                                recChargeDetail.ChargeItem = "";
                                recChargeDetail.TaxRate = 0;
                            }

                            LoadChargeDetailList.Add(recChargeDetail);
                        }
                    }
                    else if (tixiang == "WB")
                    {
                        foreach (var contract in getContractList.Where(u => u.SuitCase == tixiang && u.ContainerType == containerType.ClassType))
                        {
                            LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                            recChargeDetail.WhCode = whCode;
                            recChargeDetail.LoadId = loadId;
                            recChargeDetail.NotLoadFlag = 0;
                            recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                            recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "宝山提箱加收费";
                            recChargeDetail.ChargeName = contract.ChargeName;
                            recChargeDetail.UnitName = contract.ChargeUnitName;
                            recChargeDetail.Qty = 1;
                            recChargeDetail.CBM = 0;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                            recChargeDetail.LadderNumber = "提箱-进港点：" + PortSuitcase;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price;
                            recChargeDetail.ContainerType = containerType.ClassType;
                            recChargeDetail.CreateDate = DateTime.Now;
                            recChargeDetail.CreateUser = userName;

                            if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                            {
                                recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                            }
                            else
                            {
                                recChargeDetail.ChargeCode = "";
                                recChargeDetail.ChargeItem = "";
                                recChargeDetail.TaxRate = 0;
                            }

                            LoadChargeDetailList.Add(recChargeDetail);
                        }
                    }
                }

            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("包干")).ToList();

            #region 10.包干费

            if (getContractList.Count > 0)
            {
                if (!string.IsNullOrEmpty(loadContainerExtend.PortSuitcase) && loadContainerExtend.PortSuitcase.IndexOf("-") > 0)
                {
                    string PortSuitcase = loadContainerExtend.PortSuitcase; //提箱点-进港点

                    //Y1-Y3
                    //W-W3
                    //Y-W
                    string suitcase = PortSuitcase.Substring(0, PortSuitcase.IndexOf("-"));
                    string port = PortSuitcase.Substring(PortSuitcase.IndexOf("-") + 1, PortSuitcase.Length - (PortSuitcase.IndexOf("-") + 1));

                    string tixiang = "", jingang = "";

                    if (suitcase == "Y")
                    {
                        tixiang = "YS";
                    }
                    else if (suitcase == "W" || suitcase == "W1" || suitcase == "W2" || suitcase == "W3" || suitcase == "W4" || suitcase == "W5")
                    {
                        tixiang = "WGQ";
                    }
                    else if (suitcase == "Y1" || suitcase == "Y2" || suitcase == "Y3" || suitcase == "Y4" || suitcase == "Y5")
                    {
                        tixiang = "YSD";
                    }

                    if (port == "Y")
                    {
                        jingang = "YS";
                    }
                    else if (port == "W" || port == "W1" || port == "W2" || port == "W3" || port == "W4" || port == "W5")
                    {
                        jingang = "WGQ";
                    }
                    else if (port == "Y1" || port == "Y2" || port == "Y3" || port == "Y4" || port == "Y5")
                    {
                        jingang = "YSD";
                    }

                    if (tixiang == "" || jingang == "")
                    {
                        result = "合同名:" + client.ContractNameOut + ",进港费 提箱-进港点:" + PortSuitcase + "值异常,无法计算费用";
                    }
                    else
                    {
                        if (getContractList.Where(u => u.Port == jingang && u.SuitCase == tixiang && u.ContainerType == containerType.ClassType).Count() == 0)
                        {
                            result = "合同名:" + client.ContractNameOut + ",进港费 提箱-进港点:" + PortSuitcase + ",箱型:" + containerType.ClassType + " 未找到合同明细,无法计算费用";
                        }
                        else
                        {
                            getContractList = getContractList.Where(u => u.Port == jingang && u.SuitCase == tixiang && u.ContainerType == containerType.ClassType).ToList();
                        }


                        if (result != "")
                        {
                            LoadCharge recCharge = new LoadCharge();
                            recCharge.WhCode = whCode;
                            recCharge.ClientCode = clientCode;
                            recCharge.LoadId = loadId;
                            recCharge.WhCode = whCode;
                            recCharge.Status = "N";
                            recCharge.Description = "类型:基础-" + result;
                            recCharge.CreateUser = userName;
                            recCharge.CreateDate = DateTime.Now;
                            idal.ILoadChargeDAL.Add(recCharge);
                            idal.ILoadChargeDAL.SaveChanges();
                            return "N";
                        }

                        decimal? sumCbm = loadContainerExtend.TotalCBM;

                        foreach (var contract in getContractList)
                        {
                            LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                            recChargeDetail.WhCode = whCode;
                            recChargeDetail.LoadId = loadId;
                            recChargeDetail.NotLoadFlag = 0;
                            recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                            recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                            recChargeDetail.ClientCode = clientCode;
                            recChargeDetail.ChargeType = "包干费";
                            recChargeDetail.ChargeName = contract.ChargeName;
                            recChargeDetail.UnitName = contract.ChargeUnitName;
                            recChargeDetail.Qty = 1;
                            recChargeDetail.CBM = 0;
                            recChargeDetail.Weight = 0;
                            recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                            recChargeDetail.LadderNumber = "提箱-进港点：" + PortSuitcase + " 合同点：" + contract.SuitCase + "-" + contract.Port;
                            recChargeDetail.Price = contract.Price;
                            recChargeDetail.PriceTotal = contract.Price * 1;
                            recChargeDetail.ContainerType = containerType.ClassType;
                            recChargeDetail.CreateDate = DateTime.Now;
                            recChargeDetail.CreateUser = userName;

                            if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                            {
                                recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                                recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                                recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                            }
                            else
                            {
                                recChargeDetail.ChargeCode = "";
                                recChargeDetail.ChargeItem = "";
                                recChargeDetail.TaxRate = 0;
                            }

                            LoadChargeDetailList.Add(recChargeDetail);
                        }
                    }
                }
            }

            #endregion


            getContractList = contractList1.Where(u => u.ChargeName.Contains("箱重")).ToList();

            #region 11.箱重费

            if (getContractList.Count > 0)
            {
                if (loadContainerExtend.TotalWeight > 0)
                {
                    List<ContractFormOut> checkContractList = getContractList.Where(u => (u.LadderNumberBegin ?? 0) <= loadContainerExtend.TotalWeight && (u.LadderNumberEnd ?? 0) > loadContainerExtend.TotalWeight).ToList();

                    foreach (var contract in checkContractList)
                    {
                        LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                        recChargeDetail.WhCode = whCode;
                        recChargeDetail.LoadId = loadId;
                        recChargeDetail.NotLoadFlag = 0;
                        recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                        recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                        recChargeDetail.ClientCode = clientCode;
                        recChargeDetail.ChargeType = "箱重费";
                        recChargeDetail.ChargeName = contract.ChargeName;
                        recChargeDetail.UnitName = contract.ChargeUnitName;
                        recChargeDetail.Qty = 1;
                        recChargeDetail.CBM = 0;
                        recChargeDetail.Weight = 0;
                        recChargeDetail.ChargeUnitName = contract.ChargeUnitName;
                        recChargeDetail.LadderNumber = "箱重：" + loadContainerExtend.TotalWeight;
                        recChargeDetail.Price = contract.Price;
                        recChargeDetail.PriceTotal = contract.Price;
                        recChargeDetail.ContainerType = containerType.ClassType;
                        recChargeDetail.CreateDate = DateTime.Now;
                        recChargeDetail.CreateUser = userName;

                        if (ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).Count() > 0)
                        {
                            recChargeDetail.ChargeCode = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeCode;
                            recChargeDetail.ChargeItem = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().ChargeItem;
                            recChargeDetail.TaxRate = ruleList.Where(u => u.FunctionName == recChargeDetail.ChargeType).First().TaxRate;
                        }
                        else
                        {
                            recChargeDetail.ChargeCode = "";
                            recChargeDetail.ChargeItem = "";
                            recChargeDetail.TaxRate = 0;
                        }

                        LoadChargeDetailList.Add(recChargeDetail);
                    }

                }
            }

            #endregion


            if (idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).Count == 0)
            {
                LoadCharge recCharge = new LoadCharge();
                recCharge.WhCode = whCode;
                recCharge.ClientCode = clientCode;
                recCharge.LoadId = loadId;
                recCharge.WhCode = whCode;
                recCharge.Status = "U";
                recCharge.Description = "";
                recCharge.CreateUser = userName;
                recCharge.CreateDate = DateTime.Now;
                idal.ILoadChargeDAL.Add(recCharge);
            }

            idal.ILoadChargeDetailDAL.Add(LoadChargeDetailList);
            idal.SaveChanges();

            return "Y";
        }


        //Load出货规则验证
        public string LoadChargeRuleCheck(LoadChargeDetailInsert entity)
        {
            int? ruleId = entity.loadChargeRuleId;
            string qtycbm = entity.qtycbm;
            string unitName = entity.unitName;
            decimal price = entity.price;
            string loadId = entity.loadId;
            string whCode = entity.whCode;
            string userName = entity.userName;

            List<LoadCharge> loadChargeList = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.Status == "C");
            if (loadChargeList.Count > 0)
            {
                return "该Load已被确认，无法再进行录入操作！";
            }

            List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
            if (loadContainerExtendList.Count == 0)
            {
                return "该Load未找到出货装箱单信息！";
            }

            LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();

            //取得Load的客户
            List<string> outBoundList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                         join b in idal.ILoadDetailDAL.SelectAll()
                                         on a.Id equals b.OutBoundOrderId
                                         join c in idal.ILoadMasterDAL.SelectAll()
                                         on b.LoadMasterId equals c.Id
                                         where c.WhCode == whCode && c.LoadId == loadId
                                         select a.ClientCode).ToList();

            string clientCode = outBoundList.First();

            WhClient client = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == clientCode).First();

            //取得客户的出货合同
            List<ContractFormOut> contractList = new List<ContractFormOut>();

            if (!string.IsNullOrEmpty(client.ContractNameOut))
            {
                contractList = idal.IContractFormOutDAL.SelectBy(u => u.WhCode == whCode && u.ContractName == client.ContractNameOut);
            }
            else
            {
                return "该客户：" + client.ClientCode + "未维护出货合同，无法添加费用！";
            }

            List<LoadChargeRule> ruleList = idal.ILoadChargeRuleDAL.SelectAll().ToList();
            LoadChargeRule getRule = ruleList.Where(u => u.Id == ruleId).First();

            List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.LoadChargeRuleId > 0);

            //1.检测该ruleId是否已添加至出货费用明细中LoadChargeDetail
            if (list.Where(u => u.LoadChargeRuleId == ruleId).Count() > 0)
            {
                return "收费科目：" + getRule.FunctionName + "无法重复添加！";
            }
            else
            {
                if (idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).Count == 0)
                {
                    LoadCharge recCharge = new LoadCharge();
                    recCharge.WhCode = whCode;
                    recCharge.ClientCode = clientCode;
                    recCharge.LoadId = loadId;
                    recCharge.WhCode = whCode;
                    recCharge.Status = "U";
                    recCharge.Description = "";
                    recCharge.CreateUser = userName;
                    recCharge.CreateDate = DateTime.Now;
                    idal.ILoadChargeDAL.Add(recCharge);
                }

                LoadChargeDetail recChargeDetail = new LoadChargeDetail();
                recChargeDetail.WhCode = whCode;
                recChargeDetail.LoadId = loadId;
                recChargeDetail.ContainerNumber = loadContainerExtend.ContainerNumber;
                recChargeDetail.ClientCode = clientCode;
                recChargeDetail.ChargeType = getRule.FunctionName;
                recChargeDetail.ChargeName = getRule.FunctionName;
                recChargeDetail.UnitName = unitName;

                if (unitName == "立方")
                {
                    recChargeDetail.CBM = Convert.ToDecimal(qtycbm);
                }
                else
                {
                    recChargeDetail.Qty = Convert.ToInt32(qtycbm);
                }
                recChargeDetail.ChargeUnitName = unitName;
                recChargeDetail.LoadChargeRuleId = getRule.Id;
                recChargeDetail.CustomerFlag = getRule.CustomerFlag;
                recChargeDetail.WarehouseFlag = getRule.WarehouseFlag;
                recChargeDetail.BargainingFlag = getRule.BargainingFlag;
                if (getRule.BargainingFlag == 1)
                {
                    recChargeDetail.Price = price;
                    recChargeDetail.PriceTotal = recChargeDetail.Price * Convert.ToDecimal(qtycbm);
                }
                if (string.IsNullOrEmpty(entity.soNumber))
                {
                    recChargeDetail.SoNumber = loadContainerExtend.SoNumber;
                }
                else
                {
                    recChargeDetail.SoNumber = entity.soNumber;
                }

                recChargeDetail.DaiDianId = entity.daiDianId;
                recChargeDetail.NotLoadFlag = 0;
                recChargeDetail.ChargeCode = getRule.ChargeCode;
                recChargeDetail.ChargeItem = getRule.ChargeItem;
                recChargeDetail.TaxRate = getRule.TaxRate;

                recChargeDetail.CreateUser = userName;
                recChargeDetail.CreateDate = DateTime.Now;

                idal.ILoadChargeDetailDAL.Add(recChargeDetail);
            }

            idal.ILoadChargeDetailDAL.SaveChanges();

            return "Y";
        }

        //查询需仓库录入数量的箱单
        public List<LoadContainerResult> LoadChargeDetailWarehouseList(LoadContainerSearch searchEntity, string[] containerNumber, out int total)
        {
            var sql = (from a in idal.ILoadChargeDetailDAL.SelectAll()
                       join f in idal.ILoadMasterDAL.SelectAll()
                       on new { a.LoadId, a.WhCode } equals new { f.LoadId, f.WhCode } into temp6
                       from f in temp6.DefaultIfEmpty()
                       join g in idal.ILoadDetailDAL.SelectAll()
                       on new { Id = f.Id } equals new { Id = (Int32)g.LoadMasterId } into g_join
                       from g in g_join.DefaultIfEmpty()
                       join h in idal.IOutBoundOrderDAL.SelectAll()
                       on new { OutBoundOrderId = (Int32)g.OutBoundOrderId } equals new { OutBoundOrderId = h.Id } into h_join
                       from h in h_join.DefaultIfEmpty()
                       join b in idal.ILoadContainerExtendDAL.SelectAll()
                       on new { a.LoadId, a.WhCode } equals new { b.LoadId, b.WhCode } into temp7
                       from b in temp7.DefaultIfEmpty()
                       join c in idal.ILoadChargeDAL.SelectAll()
                       on new { a.LoadId, a.WhCode } equals new { c.LoadId, c.WhCode } into temp5
                       from c in temp5.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && a.LoadChargeRuleId > 0
                       select new LoadContainerResult
                       {
                           Action1 = "",
                           Status0 = c.Status,
                           LoadMasterId = f.Id,
                           LoadId = a.LoadId,
                           ClientCode = h.ClientCode,
                           ChuCangFS = b.ChuCangFS,
                           ETD = b.ETD,
                           ContainerType = b.ContainerType,
                           Port = b.Port,
                           ContainerNumber = b.ContainerNumber,
                           SealNumber = b.SealNumber,
                           CreateDate = f.CreateDate,
                           SumQty = f.SumQty,
                           SumCBM = f.SumCBM
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);

            if (containerNumber != null)
                sql = sql.Where(u => containerNumber.Contains(u.ContainerNumber));

            if (!string.IsNullOrEmpty(searchEntity.SealNumber))
                sql = sql.Where(u => u.SealNumber == searchEntity.SealNumber);

            if (!string.IsNullOrEmpty(searchEntity.ContainerNumberSix))
                sql = sql.Where(u => u.ContainerNumber.Contains(searchEntity.ContainerNumberSix));

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            List<LoadContainerResult> list = new List<LoadContainerResult>();
            foreach (var item in sql)
            {
                if (list.Where(u => u.Id == item.LoadMasterId).Count() == 0)
                {
                    LoadContainerResult loadMaster = new LoadContainerResult();
                    loadMaster.Action1 = "";
                    loadMaster.Status0 = (item.Status0 ?? "U") == "U" ? "未确认" : "已确认";
                    loadMaster.LoadMasterId = item.LoadMasterId;
                    loadMaster.ClientCode = item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId ?? "";
                    loadMaster.ChuCangFS = item.ChuCangFS;
                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";
                    loadMaster.ETD = item.ETD;
                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.SumCBM = item.SumCBM;

                    list.Add(loadMaster);
                }
                else
                {
                    LoadContainerResult getModel = list.Where(u => u.Id == item.LoadMasterId).First();
                    list.Remove(getModel);

                    LoadContainerResult loadMaster = new LoadContainerResult();
                    loadMaster.Action1 = "";
                    loadMaster.Status0 = (item.Status0 ?? "U") == "U" ? "未确认" : "已确认";
                    loadMaster.LoadMasterId = item.LoadMasterId;
                    loadMaster.ClientCode = item.ClientCode + getModel.ClientCode;
                    loadMaster.LoadId = item.LoadId ?? "";
                    loadMaster.ChuCangFS = item.ChuCangFS;
                    loadMaster.ContainerNumber = item.ContainerNumber ?? "";
                    loadMaster.SealNumber = item.SealNumber ?? "";
                    loadMaster.ETD = item.ETD;
                    loadMaster.ContainerType = item.ContainerType ?? "";
                    loadMaster.Port = item.Port ?? "";
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.SumCBM = item.SumCBM;

                    list.Add(loadMaster);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                list = list.Where(u => u.ClientCode.Contains(searchEntity.ClientCode)).ToList();
            }

            total = list.Count;
            list = list.OrderBy(u => u.CreateDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //删除出货箱单明细
        public string LoadChargeDetailDelById(int id)
        {
            LoadChargeDetail first = idal.ILoadChargeDetailDAL.SelectBy(u => u.Id == id).First();

            List<LoadCharge> loadChargeList = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == first.WhCode && u.LoadId == first.LoadId && u.Status == "C");
            if (loadChargeList.Count > 0)
            {
                return "该Load已被确认，无法进行删除操作！";
            }

            idal.ILoadChargeDetailDAL.DeleteBy(u => u.Id == id);
            idal.ILoadChargeDetailDAL.SaveChanges();
            return "Y";
        }

        //仓库收费科目列表显示
        public List<LoadChargeDetailResult> LoadChargeRuleWarehouseSelected(LoadChargeRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadChargeDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId == searchEntity.LoadId
                      && a.WarehouseFlag > 0
                      select new LoadChargeDetailResult
                      {
                          Id = a.Id,
                          ChargeName = a.ChargeName,
                          ChargeUnitName = a.ChargeUnitName,
                          QtyCbm = (a.Qty == 0 ? null : a.Qty) + "" + (a.CBM == 0 ? null : a.CBM)
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //仓库录入数量操作
        public string LoadContainerWarehouseEdit(string whCode, string loadId, string[] id, string[] qtycbm)
        {
            List<LoadCharge> loadChargeList = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.Status == "C");
            if (loadChargeList.Count > 0)
            {
                return "该Load已被确认，无法再进行录入操作！";
            }

            List<LoadChargeDetail> loadChargeDetailList = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.WarehouseFlag > 0);

            for (int i = 0; i < id.Length; i++)
            {
                int intId = Convert.ToInt32(id[i]);
                if (loadChargeDetailList.Where(u => u.Id == intId).Count() > 0)
                {
                    LoadChargeDetail getFirst = loadChargeDetailList.Where(u => u.Id == intId).First();
                    if (getFirst.ChargeUnitName == "立方")
                    {
                        if (qtycbm[i].ToString() == "")
                        {
                            getFirst.CBM = 0;
                        }
                        else
                        {
                            getFirst.CBM = Convert.ToDecimal(qtycbm[i].ToString());
                        }

                    }
                    else
                    {
                        if (qtycbm[i].ToString() == "")
                        {
                            getFirst.Qty = 0;
                        }
                        else
                        {
                            getFirst.Qty = Convert.ToInt32(qtycbm[i].ToString());
                        }
                    }
                    idal.ILoadChargeDetailDAL.UpdateBy(getFirst, u => u.Id == getFirst.Id, new string[] { "CBM", "Qty" });
                }
            }

            idal.SaveChanges();
            return "Y";
        }

        //检测出货项目是否全部录入完成，必选项是否遗漏
        public string LoadChargeDetailCheck(string whCode, string loadId)
        {
            //得到整套系统流程配置
            List<LoadChargeRule> sql = idal.ILoadChargeRuleDAL.SelectAll().OrderBy(u => u.FunctionId).ToList();

            //得到Load已选择流程配置
            List<LoadChargeDetail> cus = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.LoadChargeRuleId > 0);

            string result = "";     //验证结果

            //1.首先验证 必选项是否已选择
            var sql_req = sql.Where(u => u.RequiredFlag == 1 && u.FunctionFlag == 1);

            Hashtable sqlResult = new Hashtable();
            Hashtable listResult = new Hashtable();
            int sqlCount = 0, listCount = 0;

            //把数据库流程添加
            foreach (var item in sql_req)
            {
                sqlResult.Add(sqlCount, item.FunctionName);
                sqlCount++;
            }

            //把客户所选流程添加
            foreach (var item1 in cus)
            {
                listResult.Add(listCount, item1.ChargeName);
                listCount++;
            }
            //循环数据库流程
            for (int i = 0; i < sqlResult.Count; i++)
            {
                if (result == "")
                {
                    //如果客户流程 不包含数据库流程 表示必选流程不够
                    if (listResult.ContainsValue(sqlResult[i]) == false)
                    {
                        result = "N$收费科目：" + sqlResult[i].ToString() + " 为必选！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            //2.验证仓库输入数量的科目是否已填写
            if (cus.Where(u => u.WarehouseFlag == 1 && ((u.Qty ?? 0) == 0 && (u.CBM ?? 0) == 0)).Count() > 0)
            {
                List<LoadChargeDetail> getlist = cus.Where(u => u.WarehouseFlag == 1 && ((u.Qty ?? 0) == 0 && (u.CBM ?? 0) == 0)).ToList();
                result = "N$收费科目：";
                foreach (var item in getlist)
                {
                    result += item.ChargeName + ",";
                }
            }
            if (result != "")
            {
                return result.Substring(0, result.Length - 1) + " 仓库未录入数量，请联系仓库录入或删除此收费科目！";
            }


            //3.验证有流程组号的 如果勾选了其中一个，则其余组号的科目要提示出来
            int?[] ruleIdArr = (from a in cus
                                select a.LoadChargeRuleId).ToList().Distinct().ToArray();
            List<LoadChargeRule> getCheckList = sql.Where(u => ruleIdArr.Contains(u.Id) && u.GroupId > 0).ToList();

            //得到选择的流程列表
            List<LoadChargeRule> getCheckList2 = sql.Where(u => ruleIdArr.Contains(u.Id)).ToList();

            if (getCheckList.Count > 0)
            {
                Hashtable groupTable = new Hashtable();
                int groupTableCount = 0;
                foreach (var item in cus)
                {
                    List<LoadChargeRule> getCheckGroupList = sql.Where(u => u.Id == item.LoadChargeRuleId && u.GroupId > 0).ToList();
                    if (getCheckGroupList.Count > 0)
                    {
                        LoadChargeRule firstGroup = getCheckGroupList.First();
                        //得到组好的流程列表
                        List<LoadChargeRule> getCheckGroupList1 = sql.Where(u => u.GroupId == firstGroup.GroupId).OrderBy(u => u.FunctionId).ToList();

                        string s = " ";
                        int count = 0;
                        //得到组，如果有组 提示其它科目
                        foreach (var item1 in getCheckGroupList1)
                        {
                            //如果选择的科目 在组列表中存在，累加1
                            if (getCheckList2.Where(u => u.FunctionName == item1.FunctionName).Count() > 0)
                            {
                                count++;
                            }
                            s += item1.FunctionName + ",";
                        }

                        //总数如果和 组数 不一致，即：组3个 而所选不满3个，应予以提示
                        if (!groupTable.ContainsKey(firstGroup.GroupId) && count != getCheckGroupList1.Count)
                        {
                            groupTable.Add(groupTableCount, s.Substring(0, s.Length - 1));
                            groupTableCount++;
                        }

                    }
                }

                if (groupTable.Count > 0)
                {
                    for (int i = 0; i < groupTable.Count; i++)
                    {
                        result += "科目:" + groupTable[i].ToString() + " 为一组而有科目未选择,确定其余科目本次未遗漏吗?\r\n";
                    }

                    return "Y1$" + result;
                }
                else
                {
                    return "Y$Y";
                }
            }
            else
            {
                return "Y$Y";
            }
        }

        //显示load下的所有费用科目及数量 
        public List<LoadChargeDetailResult> LoadChargeDetailList(LoadChargeRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadChargeDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId == searchEntity.LoadId
                      && a.PriceTotal > 0
                      select new LoadChargeDetailResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          LoadId = a.LoadId,
                          ChargeType = a.ChargeType,
                          ChargeName = a.ChargeName,
                          ChargeUnitName = a.ChargeUnitName,
                          LadderNumber = a.LadderNumber,
                          QtyCbm = ((a.Qty ?? 0) == 0 ? "" : a.Qty.ToString()) + "" + ((a.CBM ?? 0) == 0 ? "" : a.CBM.ToString()),
                          Remark = (a.LoadChargeRuleId ?? 0) == 0 ? "系统自动" : "",
                          Description = a.WarehouseFlag == 1 ? "仓库输入数量" : "",
                          Price = (a.LoadChargeRuleId ?? 0) > 0 ? a.Price : 0,
                          PriceTotal = (a.LoadChargeRuleId ?? 0) > 0 ? a.PriceTotal : 0
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //确认费用及撤销确认
        public string LoadChargeEdit(string whCode, string loadId, string status)
        {
            //确认费用时，计算非基础费用是否有合同明细，有 要补充费用明细单价及总价
            if (status == "C")
            {
                List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.LoadChargeRuleId > 0 && (u.BargainingFlag ?? 0) == 0);

                //取得Load的客户
                List<string> outBoundList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                             join b in idal.ILoadDetailDAL.SelectAll()
                                             on a.Id equals b.OutBoundOrderId
                                             join c in idal.ILoadMasterDAL.SelectAll()
                                             on b.LoadMasterId equals c.Id
                                             where c.WhCode == whCode && c.LoadId == loadId
                                             select a.ClientCode).ToList();

                string clientCode = outBoundList.First();

                WhClient client = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == clientCode).First();

                //取得客户的出货合同
                List<ContractFormOut> contractList = new List<ContractFormOut>();

                if (!string.IsNullOrEmpty(client.ContractNameOut))
                {
                    contractList = idal.IContractFormOutDAL.SelectBy(u => u.WhCode == whCode && u.ContractName == client.ContractNameOut);
                }
                else
                {
                    return "该客户未配置出货合同，无法计算费用！";
                }

                List<LoadContainerExtend> loadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadContainerExtendList.Count == 0)
                {
                    return "该Load未找到出货装箱单信息！";
                }
                LoadContainerExtend loadContainerExtend = loadContainerExtendList.First();

                string result = "";
                foreach (var item in list)
                {
                    List<ContractFormOut> contractList1 = contractList.Where(u => u.ChargeName == item.ChargeName && u.ChargeUnitName == item.ChargeUnitName).ToList();
                    if (contractList1.Count == 0)
                    {
                        result = "合同名:" + client.ContractNameOut + ",收费科目：" + item.ChargeName + " 没有找到合同明细,无法计算费用";
                        break;
                    }
                    else
                    {
                        ContractFormOut contractFirst = new ContractFormOut();

                        //如果非基础项目-合同中 存在箱型的，要根据箱型取得合同
                        if (contractList1.Where(u => u.ContainerType != "").Count() > 0)
                        {
                            //取得Load的发货信息：箱型等
                            List<LoadContainerType> loadContainerTypeList = (from a in idal.ILoadContainerTypeDAL.SelectAll()
                                                                             join b in idal.ILoadContainerExtendDAL.SelectAll()
                                                                             on a.ContainerType equals b.ContainerType
                                                                             where b.WhCode == whCode && b.LoadId == loadId
                                                                             select a).ToList();
                            if (loadContainerTypeList.Count == 0)
                            {
                                result += "合同名:" + client.ContractNameOut + ",收费科目：" + item.ChargeName + " 该Load未找到出货箱型等信息！";
                                break;
                            }
                            LoadContainerType containerType = loadContainerTypeList.First();
                            contractFirst = contractList1.Where(u => u.ContainerType == containerType.ClassType).First();
                        }
                        else
                        {
                            contractFirst = contractList1.First();
                        }


                        if (item.ChargeUnitName == "立方")
                        {
                            item.Price = contractFirst.Price;
                            item.PriceTotal = item.CBM * contractFirst.Price;
                        }
                        else
                        {
                            item.Price = contractFirst.Price;
                            item.PriceTotal = item.Qty * contractFirst.Price;
                        }

                        idal.ILoadChargeDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Price", "PriceTotal" });

                    }
                }
                if (result != "")
                {
                    return result;
                }
            }

            LoadCharge edit = new LoadCharge();
            edit.Status = status;
            idal.ILoadChargeDAL.UpdateBy(edit, u => u.WhCode == whCode && u.LoadId == loadId, new string[] { "Status" });
            idal.SaveChanges();
            return "Y";
        }

        //重新计算基础费用
        public string AgainLoadCharge(string whCode, string loadId, string userName)
        {
            LoadMaster lm = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).First();
            if (lm.ShipDate == null)
            {
                return "该Load未封箱，无法计算费用！";
            }

            List<LoadCharge> checkList = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
            if (checkList.Count > 0)
            {
                LoadCharge first = checkList.First();
                idal.ILoadChargeDAL.UpdateByExtended(u => u.WhCode == whCode && u.LoadId == loadId, u => new LoadCharge { Status2 = first.Status2 });
            }

            idal.ILoadChargeDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId);
            idal.ILoadChargeDetailDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId && (u.LoadChargeRuleId ?? 0) == 0);

            string result = LoadChargeAdd(loadId, whCode, userName);

            if (result == "Y")
            {
                return "Y";
            }
            else
            {
                return result;
            }
        }

        //代垫列表
        public List<LoadChargeDaiDian> DaiDianSelectList()
        {
            List<LoadChargeDaiDian> list = idal.ILoadChargeDaiDianDAL.SelectAll().ToList();
            return list;
        }


        //特费科目列表
        public List<LoadChargeRule> LoadChargeRuleSelectList()
        {
            List<LoadChargeRule> list = idal.ILoadChargeRuleDAL.SelectBy(u => u.FunctionFlag == 1 && u.SOFeeFlag == 1).ToList();
            return list;
        }

        //代垫列表
        public List<LoadChargeDaiDian> LoadChargeDaiDianSelectList()
        {
            List<LoadChargeDaiDian> list = idal.ILoadChargeDaiDianDAL.SelectAll().ToList();
            return list;
        }

        //SO特费科目添加
        public string LoadChargeRuleAdd(LoadChargeRule entity)
        {
            int? getMaxFunctionId = idal.ILoadChargeRuleDAL.SelectAll().Max(u => u.FunctionId);
            int? setFunctionId = getMaxFunctionId + 10;
            entity.FunctionId = setFunctionId;

            entity.FunctionUnitName = "个";
            entity.SOFeeFlag = 1;
            entity.FunctionFlag = 1;
            entity.CustomerFlag = 1;
            entity.WarehouseFlag = 0;
            entity.BargainingFlag = 1;


            idal.ILoadChargeRuleDAL.Add(entity);
            idal.ILoadChargeRuleDAL.SaveChanges();
            return "Y";
        }


        //编辑SO特费科目
        public string LoadChargeRuleEdit(LoadChargeRule entity)
        {
            List<LoadChargeRule> list = idal.ILoadChargeRuleDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "数据异常，请重新查询！";
            }

            LoadChargeRule getOldLoadChargeRule = list.First();

            idal.ILoadChargeDetailDAL.UpdateByExtended(u => u.ChargeName == getOldLoadChargeRule.FunctionName && u.NotLoadFlag == 1, t => new LoadChargeDetail { ChargeCode = entity.ChargeCode, ChargeItem = entity.ChargeItem, ChargeType = entity.FunctionName, ChargeName = entity.FunctionName, DaiDianId = entity.DaiDianId });

            idal.ILoadChargeRuleDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "DaiDianId", "ChargeCode", "ChargeItem", "FunctionName" });

            idal.ILoadChargeDetailDAL.SaveChanges();
            return "Y";
        }


        //添加SO特费
        public string LoadChargeDetailAddBySO(LoadChargeDetail entity)
        {
            List<LoadChargeRule> ruleList = idal.ILoadChargeRuleDAL.SelectBy(u => u.Id == entity.LoadChargeRuleId);
            if (ruleList.Count > 0)
            {
                LoadChargeRule first = ruleList.First();
                entity.ChargeCode = first.ChargeCode;
                entity.ChargeItem = first.ChargeItem;
                entity.TaxRate = first.TaxRate;
                entity.DaiDianId = first.DaiDianId;
                entity.ChargeType = first.FunctionName;
                entity.ChargeName = first.FunctionName;
            }

            entity.CreateDate = DateTime.Now;
            idal.ILoadChargeDetailDAL.Add(entity);
            idal.ILoadChargeDetailDAL.SaveChanges();
            return "Y";
        }


        //编辑SO特费
        public string LoadChargeDetailEditBySO(LoadChargeDetail entity)
        {
            List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "数据异常，请重新查询！";
            }

            if (list.Where(u => u.SoStatus == "C").Count() > 0)
            {
                return "SO特费已做账单，无法编辑！";
            }

            List<LoadChargeRule> ruleList = idal.ILoadChargeRuleDAL.SelectBy(u => u.Id == entity.LoadChargeRuleId);
            if (ruleList.Count > 0)
            {
                LoadChargeRule first = ruleList.First();
                entity.ChargeCode = first.ChargeCode;
                entity.ChargeItem = first.ChargeItem;
                entity.TaxRate = first.TaxRate;
                entity.DaiDianId = first.DaiDianId;
                entity.ChargeType = first.FunctionName;
                entity.ChargeName = first.FunctionName;
            }

            idal.ILoadChargeDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "ETD", "SoNumber", "UnitName", "ContainerNumber", "ContainerType", "CBM", "ChargeCode", "Price", "PriceTotal", "ChargeUnitName", "LadderNumber", "ChargeItem", "TaxRate", "DaiDianId", "ChargeType", "ChargeName", "CreateUser", "Remark", "TaxInclusiveFlag" });

            idal.ILoadChargeDetailDAL.SaveChanges();
            return "Y";
        }

        //查询SO特费列表
        public List<LoadChargeDetailResult> LoadChargeDetailSOList(LoadChargeDetailSearch searchEntity, string[] soList, out int total)
        {
            var sql = from a in idal.ILoadChargeDetailDAL.SelectAll()
                      join b in idal.ILoadChargeRuleDAL.SelectAll()
                      on a.ChargeName equals b.FunctionName
                      where a.WhCode == searchEntity.WhCode && a.NotLoadFlag == 1 && b.SOFeeFlag == 1 && b.FunctionFlag == 1
                      select new LoadChargeDetailResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          ETD = a.ETD,
                          SoNumber = a.SoNumber,
                          UnitName = a.UnitName,
                          ContainerNumber = a.ContainerNumber,
                          ContainerType = a.ContainerType,
                          ChargeName = a.ChargeName,
                          CBM = a.CBM,
                          Price = a.Price,
                          PriceTotal = a.PriceTotal,
                          ChargeUnitName = a.ChargeUnitName,
                          LadderNumber = a.LadderNumber,
                          SoStatus = a.SoStatus == "C" ? "已做账单" : "正常",
                          CreateUser = a.CreateUser,
                          LoadChargeRuleId = b.Id,
                          Remark = a.Remark ?? "",
                          TaxInclusiveFlag = a.TaxInclusiveFlag == 1 ? "是" : "否"
                      };
            if (soList != null)
            {
                sql = sql.Where(u => soList.Contains(u.SoNumber));
            }
            if (!string.IsNullOrEmpty(searchEntity.SoStatus))
                sql = sql.Where(u => u.SoStatus == searchEntity.SoStatus);
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.BillNumber))
                sql = sql.Where(u => u.UnitName == searchEntity.BillNumber);
            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);

            if (searchEntity.BeginETD != null)
            {
                sql = sql.Where(u => u.ETD >= searchEntity.BeginETD);
            }
            if (searchEntity.EndETD != null)
            {
                sql = sql.Where(u => u.ETD < searchEntity.EndETD);
            }

            List<LoadChargeDetailResult> list = new List<LoadChargeDetailResult>();

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            foreach (var item in sql.ToList())
            {
                item.ETDShow = Convert.ToDateTime(item.ETD).ToString("yyyy-MM-dd");
                list.Add(item);
            }

            return list;
        }

        //删除SO特费
        public string LoadChargeDetailSoDel(int id, string userName)
        {
            List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.Id == id);
            if (list.Count == 0)
            {
                return "数据异常，请重新查询！";
            }

            if (list.Where(u => u.SoStatus == "C").Count() > 0)
            {
                return "SO特费已做账单，无法删除！";
            }

            LoadChargeDetail first = list.First();

            TranLog tl = new TranLog();
            tl.TranType = "750";
            tl.Description = "SO特费删除";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = first.WhCode;
            tl.ClientCode = first.ClientCode;
            tl.LoadId = first.LoadId;
            tl.SoNumber = first.SoNumber;
            tl.CustomerPoNumber = first.SoNumber;
            tl.AltItemNumber = first.SoNumber;
            tl.HuId = "";
            tl.Remark = first.ChargeName + "/" + first.CBM + "/" + first.Price;
            idal.ITranLogDAL.Add(tl);

            idal.ILoadChargeDetailDAL.DeleteBy(u => u.Id == id);
            idal.ILoadChargeDetailDAL.SaveChanges();

            return "Y";
        }


        //科目类型下拉列表
        public IEnumerable<LoadChargeRuleResult> GetLoadChangeRuleTypeName()
        {
            var sql = (from a in idal.ILoadChargeRuleDAL.SelectAll()
                       where (a.TypeName ?? "") != ""
                       select new LoadChargeRuleResult
                       {
                           FunctionName = a.TypeName
                       }).Distinct();

            return sql.AsEnumerable();
        }


        //得到确认箱单费用异常数量
        public List<LoadChargeDetailResult> CheckLoadChangeErrorCount(LoadContainerSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadChargeDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && (a.Description ?? "") != "" && a.Status == "N"
                      select new LoadChargeDetailResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          LoadId = a.LoadId,
                          Description = a.Description
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            total = sql.Count();
            sql = sql.OrderBy(u => u.LoadId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }





        #endregion


        #region 特殊功能：SN批量导入一键创建Load并释放 更换、删除、新增其它托盘

        //SN批量导入一键创建Load并释放
        public string ImportsOutBoundOrderExtendBySN(List<OutBoundOrderExtendInsert> entityList)
        {
            string result = "";     //执行总结果

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            OutBoundOrderExtendInsert frist = entityList.First();
            frist.PlanQty = entityList.Sum(u => u.Qty).ToString();

            //得到所有仓库
            int companyId = (from a in idal.IWhInfoDAL.SelectAll()
                             where a.WhCode == frist.WhCode
                             select a.CompanyId).First();
            List<WhInfo> getAllWhCodeList = idal.IWhInfoDAL.SelectBy(u => u.CompanyId == companyId);
            string[] allWhCodeArr = (from a in getAllWhCodeList
                                     select a.WhCode).ToList().Distinct().ToArray();


            List<OutBoundOrder> outBoundOrderListAdd = new List<OutBoundOrder>();
            List<OutBoundOrderDetail> outBoundOrderDetailListAdd = new List<OutBoundOrderDetail>();

            List<LoadHuIdExtend> loadHuIdExtendList = new List<LoadHuIdExtend>();

            foreach (var entity in entityList)
            {
                if (result != "")
                {
                    break;
                }

                //首先验证 出库订单号是否已存在
                List<OutBoundOrder> sqlCheck = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                where a.ClientCode == entity.ClientCode && a.CustomerOutPoNumber == entity.SoNumber && allWhCodeArr.Contains(a.WhCode)
                                                select a).ToList();
                if (sqlCheck.Count > 0)
                {
                    result = "出库订单号已存在:" + entity.SoNumber + "，无法重复导入！";
                    break;
                }

                List<OutBoundOrderDetailExtendModel> snList = entity.OutBoundOrderDetailExtendModel;
                string[] snarr = (from a in snList
                                  select a.SNNumber).ToArray();

                List<HuDetailResult> List = new List<HuDetailResult>();     //库存

                if (snarr.Count() > 0)
                {
                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                                              on new { a.HuId, a.WhCode }
                                          equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                                    on new { b.Location, b.WhCode }
                                equals new { Location = c.LocationId, c.WhCode }
                              where
                                b.Type == "M" &&
                                b.Status == "A" &&
                                c.LocationTypeId == 1 &&
                                (a.Qty - (a.PlanQty ?? 0)) > 0 && snarr.Contains(a.HuId)
                                && allWhCodeArr.Contains(a.WhCode)
                              group a by new
                              {
                                  a.HuId,
                                  a.WhCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitName,
                                  a.UnitId
                              } into g
                              select new HuDetailResult
                              {
                                  HuId = g.Key.HuId,
                                  WhCode = g.Key.WhCode,
                                  SoNumber = g.Key.SoNumber,
                                  CustomerPoNumber = g.Key.CustomerPoNumber,
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitName = g.Key.UnitName,
                                  UnitId = g.Key.UnitId
                              };
                    List = sql.Distinct().ToList();

                    foreach (var item in snList)
                    {
                        if (List.Where(u => u.HuId == item.SNNumber).Count() == 0)
                        {
                            result += "SN对应库存不存在：" + item.SNNumber + "！";
                        }
                        else
                        {
                            HuDetailResult getHudetailFirst = List.Where(u => u.HuId == item.SNNumber).First();

                            LoadHuIdExtend loadHuIdExtend = new LoadHuIdExtend();
                            loadHuIdExtend.WhCode = getHudetailFirst.WhCode;
                            loadHuIdExtend.HuId = getHudetailFirst.HuId;
                            loadHuIdExtend.CreateUser = frist.CreateUser;
                            loadHuIdExtend.CreateDate = DateTime.Now;
                            loadHuIdExtendList.Add(loadHuIdExtend);
                        }
                    }
                }
                else
                {
                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                                              on new { a.HuId, a.WhCode }
                                          equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                                    on new { b.Location, b.WhCode }
                                equals new { Location = c.LocationId, c.WhCode }
                              where
                                b.Type == "M" &&
                                b.Status == "A" &&
                                c.LocationTypeId == 1 &&
                                (a.Qty - (a.PlanQty ?? 0)) > 0 && a.SoNumber == entity.SoNumber
                                 && allWhCodeArr.Contains(a.WhCode)
                              group a by new
                              {
                                  a.WhCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitName,
                                  a.UnitId
                              } into g
                              select new HuDetailResult
                              {
                                  WhCode = g.Key.WhCode,
                                  SoNumber = g.Key.SoNumber,
                                  CustomerPoNumber = g.Key.CustomerPoNumber,
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitName = g.Key.UnitName,
                                  UnitId = g.Key.UnitId
                              };
                    List = sql.Distinct().ToList();
                }

                if (List.Count == 0)
                {
                    result += "SO号库存不足：" + entity.SoNumber + "！请确认库存状态是否正常！";
                }
                else
                {
                    int? sumQty = List.Sum(u => u.Qty);
                    if (entity.Qty > sumQty)
                    {
                        result += "SO号库存数量少于出货数量：" + entity.SoNumber + "！";
                        break;
                    }
                    else
                    {
                        string outponumber = "SA" + DI.IDGenerator.NewId;

                        int getPlanQty = entity.Qty;
                        foreach (var item in List)
                        {
                            if (getPlanQty <= 0)
                            {
                                break;
                            }

                            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == item.WhCode);
                            if (clientList.Count == 0)
                            {
                                result = "未匹配到客户名：" + entity.ClientCode + "！";
                                break;
                            }

                            WhClient client = clientList.First();

                            var sql = from a in idal.IWhClientDAL.SelectAll()
                                      join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                                      on a.Id equals b.ClientId
                                      join c in idal.IFlowHeadDAL.SelectAll()
                                      on b.BusinessFlowGroupId equals c.Id
                                      where a.Id == client.Id && c.Type == "OutBound" && c.FlowOrderBy == 1
                                      select c;
                            if (sql.Count() == 0)
                            {
                                result = "未找到客户名：" + client.ClientCode + "的出货流程！";
                                break;
                            }

                            FlowHead flowHeadfirst = sql.First();

                            if (outBoundOrderListAdd.Where(u => u.WhCode == item.WhCode && u.CustomerOutPoNumber == entity.SoNumber).Count() == 0)
                            {
                                OutBoundOrder outBoundOrder = new OutBoundOrder();
                                outBoundOrder.WhCode = item.WhCode;
                                outBoundOrder.ClientId = client.Id;
                                outBoundOrder.ClientCode = client.ClientCode;
                                outBoundOrder.OutPoNumber = outponumber;
                                outBoundOrder.CustomerOutPoNumber = entity.SoNumber;
                                outBoundOrder.ProcessId = flowHeadfirst.Id;

                                var sql1 = from a in idal.IFlowDetailDAL.SelectAll()
                                           where a.FlowHeadId == outBoundOrder.ProcessId && a.StatusId == 15
                                           select a;
                                if (sql1.Count() == 0)
                                {
                                    WhInfo getinfo = idal.IWhInfoDAL.SelectBy(u => u.WhCode == item.WhCode).First();
                                    result = getinfo.WhName + "的客户：" + client.ClientCode + "出货流程配置有误！";
                                    break;
                                }

                                FlowDetail flowDetail = sql1.OrderBy(u => u.OrderId).First();

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.ReceiptId = "";
                                outBoundOrder.OrderSource = "SN导入";
                                outBoundOrder.PlanOutTime = DateTime.Now;
                                outBoundOrder.LoadFlag = 0;
                                outBoundOrder.DSFLag = 0;
                                outBoundOrder.CreateUser = entity.CreateUser;
                                outBoundOrder.CreateDate = DateTime.Now;
                                outBoundOrderListAdd.Add(outBoundOrder);
                            }

                            if (item.Qty < getPlanQty)
                            {
                                OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                                orderDetail.OutPoNumber = outponumber;
                                orderDetail.OutBoundOrderId = 0;
                                orderDetail.WhCode = item.WhCode;
                                orderDetail.SoNumber = item.SoNumber;
                                orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                                orderDetail.AltItemNumber = item.AltItemNumber;
                                orderDetail.ItemId = Convert.ToInt32(item.ItemId);
                                orderDetail.UnitId = 0;
                                orderDetail.UnitName = item.UnitName;

                                orderDetail.Qty = Convert.ToInt32(item.Qty);
                                orderDetail.DSFLag = 0;
                                orderDetail.LotNumber1 = item.LotNumber1;
                                orderDetail.LotNumber2 = item.LotNumber2;
                                orderDetail.CreateUser = entity.CreateUser;
                                orderDetail.CreateDate = DateTime.Now;
                                outBoundOrderDetailListAdd.Add(orderDetail);

                                getPlanQty = getPlanQty - Convert.ToInt32(item.Qty);
                            }
                            else
                            {
                                OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                                orderDetail.OutPoNumber = outponumber;
                                orderDetail.OutBoundOrderId = 0;
                                orderDetail.WhCode = item.WhCode;
                                orderDetail.SoNumber = item.SoNumber;
                                orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                                orderDetail.AltItemNumber = item.AltItemNumber;
                                orderDetail.ItemId = Convert.ToInt32(item.ItemId);
                                orderDetail.UnitId = 0;
                                orderDetail.UnitName = item.UnitName;

                                orderDetail.Qty = getPlanQty;
                                orderDetail.DSFLag = 0;
                                orderDetail.LotNumber1 = item.LotNumber1;
                                orderDetail.LotNumber2 = item.LotNumber2;
                                orderDetail.CreateUser = entity.CreateUser;
                                orderDetail.CreateDate = DateTime.Now;
                                outBoundOrderDetailListAdd.Add(orderDetail);

                                getPlanQty = 0;

                                break;
                            }
                        }

                    }
                }

            }
            if (result != "")
            {
                return result;
            }

            idal.IOutBoundOrderDAL.Add(outBoundOrderListAdd);
            idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetailListAdd);
            idal.IOutBoundOrderDAL.SaveChanges();

            string[] OutPoNumberList = (from a in outBoundOrderListAdd
                                        select a.OutPoNumber).ToList().Distinct().ToArray();

            string[] whcodeArr = (from a in outBoundOrderListAdd
                                  select a.WhCode).ToList().Distinct().ToArray();

            int maxQty = 0;
            string remark = "", remark1 = "", shipOrigion = "";
            foreach (var item in whcodeArr)
            {
                WhInfo info = idal.IWhInfoDAL.SelectBy(u => u.WhCode == item).First();
                int getwhCodeQty = outBoundOrderDetailListAdd.Where(u => u.WhCode == item).Sum(u => u.Qty);
                remark1 += info.WhName + "的发货数量：" + getwhCodeQty;

                if (maxQty == 0)
                {
                    maxQty = getwhCodeQty;
                    shipOrigion = info.WhName;
                }
                else
                {
                    if (maxQty < getwhCodeQty)
                    {
                        shipOrigion = info.WhName;
                        maxQty = getwhCodeQty;
                    }
                }
            }
            remark = "计划总数量:" + frist.PlanQty + remark1;

            int loadMasterId = 0;
            List<LoadDetail> loadDetailList = new List<LoadDetail>();
            List<LoadMaster> loadMasterList = new List<LoadMaster>();

            foreach (var item in whcodeArr)
            {
                //根据OutPoNumber 修改订单明细Id
                List<OutBoundOrder> getOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutPoNumberList.Contains(u.OutPoNumber) && u.WhCode == item).ToList();
                foreach (var item1 in outBoundOrderListAdd.Where(u => u.WhCode == item))
                {
                    OutBoundOrder getOutBoundOrder = getOutBoundOrderList.Where(u => u.WhCode == item && u.OutPoNumber == item1.OutPoNumber).First();
                    idal.IOutBoundOrderDetailDAL.UpdateByExtended(u => u.WhCode == item && u.OutPoNumber == item1.OutPoNumber, t => new OutBoundOrderDetail { OutBoundOrderId = getOutBoundOrder.Id });
                }

                OutBoundOrder fristOut = outBoundOrderListAdd.Where(u => u.WhCode == item).First();
                FlowHead flowheadFir = idal.IFlowHeadDAL.SelectBy(u => u.WhCode == item && u.Id == fristOut.ProcessId).First();

                //新增Load
                LoadMaster loadMaster = new LoadMaster();
                loadMaster.LoadId = "LD" + DI.IDGenerator.NewId;
                loadMaster.Status0 = "U";
                loadMaster.Status1 = "U";
                loadMaster.Status2 = "U";
                loadMaster.Status3 = "U";
                loadMaster.WhCode = item;
                loadMaster.ShipMode = "";
                loadMaster.Remark = remark;
                loadMaster.ProcessId = flowheadFir.Id;
                loadMaster.PlanQty = Convert.ToInt32(frist.PlanQty);
                loadMaster.ProcessName = flowheadFir.FlowName;
                loadMaster.CreateUser = fristOut.CreateUser;
                loadMaster.CreateDate = DateTime.Now;
                idal.ILoadMasterDAL.Add(loadMaster);

                LoadContainerExtend loadContainerExtend = new LoadContainerExtend();
                loadContainerExtend.WhCode = item;
                loadContainerExtend.LoadId = loadMaster.LoadId;
                loadContainerExtend.VesselName = "";
                loadContainerExtend.VesselNumber = "";
                loadContainerExtend.CarriageName = "";
                loadContainerExtend.ContainerType = "";
                loadContainerExtend.Port = "";
                loadContainerExtend.ChuCangFS = "海运";
                loadContainerExtend.PortSuitcase = "";
                loadContainerExtend.BaQty = "";
                loadContainerExtend.DeliveryPlace = "";
                loadContainerExtend.BillNumber = "";
                loadContainerExtend.ContainerNumber = fristOut.CustomerOutPoNumber;
                loadContainerExtend.SealNumber = fristOut.CustomerOutPoNumber;
                loadContainerExtend.CreateUser = fristOut.CreateUser;
                loadContainerExtend.CreateDate = DateTime.Now;
                loadContainerExtend.ContainerSource = "WMS";

                loadContainerExtend.PlanQty = Convert.ToInt32(frist.PlanQty);
                loadContainerExtend.ShippingOrigin = shipOrigion;
                idal.ILoadContainerExtendDAL.Add(loadContainerExtend);

                foreach (var loadHuid in loadHuIdExtendList.Where(u => u.WhCode == item))
                {
                    loadHuid.LoadId = loadMaster.LoadId;
                }
                idal.ILoadHuIdExtendDAL.Add(loadHuIdExtendList);

                idal.ILoadMasterDAL.SaveChanges();

                loadMasterId = loadMaster.Id;
                loadMasterList.Add(loadMaster);

                //新增Load明细
                foreach (var orderId in outBoundOrderListAdd.Where(u => u.WhCode == item))
                {
                    LoadDetail loadDetail = new LoadDetail();
                    loadDetail.LoadMasterId = loadMasterId;
                    loadDetail.OutBoundOrderId = orderId.Id;
                    loadDetail.CreateUser = fristOut.CreateUser;
                    loadDetail.CreateDate = DateTime.Now;
                    loadDetailList.Add(loadDetail);
                }
            }

            idal.ILoadDetailDAL.Add(loadDetailList);
            idal.IOutBoundOrderDAL.SaveChanges();

            //释放Load
            ReleaseLoadManager rea = new ReleaseLoadManager();
            foreach (var item in loadMasterList)
            {
                rea.CheckReleaseLoad(item.LoadId, item.WhCode, item.CreateUser, "1");
            }

            return "Y";
        }


        //备货单信息显示
        public List<PickTaskDetailResult2> PickTaskDetailList(PickTaskDetailSearch searchEntity, out int total)
        {
            var sql = from picktaskdetail in idal.IPickTaskDetailDAL.SelectAll()
                      where picktaskdetail.WhCode == searchEntity.WhCode
                      group picktaskdetail by new
                      {
                          picktaskdetail.LoadId,
                          picktaskdetail.ClientCode,
                          picktaskdetail.HuId,
                          picktaskdetail.SoNumber,
                          picktaskdetail.CustomerPoNumber,
                          picktaskdetail.AltItemNumber,
                          picktaskdetail.UnitName,
                          picktaskdetail.ItemId,
                          picktaskdetail.LotNumber1,
                          picktaskdetail.LotNumber2,
                          picktaskdetail.Status,
                          picktaskdetail.Status1
                      } into g
                      select new PickTaskDetailResult2
                      {
                          Action1 = "",
                          Action2 = "",
                          Action3 = "",
                          LoadId = g.Key.LoadId,
                          HuId = g.Key.HuId,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          UnitName = g.Key.UnitName,
                          ItemId = g.Key.ItemId,
                          LotNumber1 = g.Key.LotNumber1,
                          LotNumber2 = g.Key.LotNumber2,
                          Status = g.Key.Status == "U" ? "未备货" : "已备货",
                          Status1 = g.Key.Status1 == "U" ? "未装箱" : "已装箱",
                          Qty = g.Sum(p => p.Qty)
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            total = sql.Count();
            sql = sql.OrderBy(u => u.HuId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //更换托盘
        public string PickTaskDetailHuIdEdit(PickTaskDetailResult2 entity, string newHuId)
        {
            List<PickTaskDetail> pickTaskDetailList = idal.IPickTaskDetailDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && (u.SoNumber ?? "") == (entity.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (entity.CustomerPoNumber ?? "") && u.ItemId == entity.ItemId && u.UnitName == entity.UnitName && u.ClientCode == entity.ClientCode && (u.LotNumber1 == entity.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (entity.LotNumber1 == null ? "" : entity.LotNumber1))
                    && (u.LotNumber2 == entity.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (entity.LotNumber2 == null ? "" : entity.LotNumber2)));

            if (pickTaskDetailList.Count == 0)
            {
                return "更换SN异常，请重新查询后提交！";
            }
            PickTaskDetail pickTaskDetail = pickTaskDetailList.First();
            if (pickTaskDetail.Status1 != "U")
            {
                return "已装箱无法再更换！";
            }

            var sql = from picktaskdetail in pickTaskDetailList
                      group picktaskdetail by new
                      {
                          picktaskdetail.OutBoundOrderId,
                          picktaskdetail.WhCode,
                          picktaskdetail.ClientCode,
                          picktaskdetail.SoNumber,
                          picktaskdetail.CustomerPoNumber,
                          picktaskdetail.AltItemNumber,
                          picktaskdetail.UnitName,
                          picktaskdetail.UnitId,
                          picktaskdetail.ItemId,
                          picktaskdetail.LotNumber1,
                          picktaskdetail.LotNumber2
                      } into g
                      select new PickTaskDetailResult2
                      {
                          OutBoundOrderId = g.Key.OutBoundOrderId,
                          WhCode = g.Key.WhCode,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber ?? "",
                          CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                          AltItemNumber = g.Key.AltItemNumber,
                          UnitName = g.Key.UnitName,
                          UnitId = g.Key.UnitId,
                          ItemId = g.Key.ItemId,
                          LotNumber1 = g.Key.LotNumber1 ?? "",
                          LotNumber2 = g.Key.LotNumber2 ?? "",
                          Qty = g.Sum(p => p.Qty)
                      };

            var sql1 = from a in idal.IHuDetailDAL.SelectAll()
                       join b in idal.IHuMasterDAL.SelectAll()
                       on new { a.HuId, a.WhCode }
                       equals new { b.HuId, b.WhCode }
                       join c in idal.IWhLocationDAL.SelectAll()
                       on new { b.Location, b.WhCode }
                       equals new { Location = c.LocationId, c.WhCode }
                       where a.WhCode == pickTaskDetail.WhCode && a.HuId == newHuId &&
                       b.Type == "M" &&
                       b.Status == "A" &&
                       c.LocationTypeId == 1 &&
                       (a.Qty - (a.PlanQty ?? 0)) > 0
                       group a by new
                       {
                           a.WhCode,
                           a.ClientCode,
                           a.SoNumber,
                           a.CustomerPoNumber,
                           a.AltItemNumber,
                           a.UnitName,
                           a.UnitId,
                           a.ItemId,
                           a.LotNumber1,
                           a.LotNumber2
                       } into g
                       select new PickTaskDetailResult2
                       {
                           WhCode = g.Key.WhCode,
                           ClientCode = g.Key.ClientCode,
                           SoNumber = g.Key.SoNumber ?? "",
                           CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                           AltItemNumber = g.Key.AltItemNumber,
                           UnitName = g.Key.UnitName,
                           UnitId = g.Key.UnitId,
                           ItemId = g.Key.ItemId,
                           LotNumber1 = g.Key.LotNumber1 ?? "",
                           LotNumber2 = g.Key.LotNumber2 ?? "",
                           Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                       };

            List<PickTaskDetailResult2> oldHuIdList = sql.ToList();
            List<PickTaskDetailResult2> newHuIdList = sql1.ToList();

            LoadMaster loadmaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == pickTaskDetail.LoadId && u.WhCode == pickTaskDetail.WhCode).First();
            List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                               where a.FlowHeadId == loadmaster.ProcessId
                                               select a).ToList();
            string mark = flowDetailList.Where(u => u.Type == "Release").First().Mark;
            string ReleaseType = flowDetailList.Where(u => u.Type == "ReleaseType").First().Mark;

            string result = "";
            if (sql1.Count() == 0)
            {
                return "新SN库存数量少于原SN出货数量，无法更换SN！";
            }
            else
            {
                foreach (var item in oldHuIdList)
                {
                    List<PickTaskDetailResult2> huDetailResultList = new List<PickTaskDetailResult2>();

                    //等于1,9 为先进先出  
                    //等于2 为后进先出
                    //等于3 为先进先出无视SOPO
                    //等于4 为后进先出无视SOPO
                    //等于5 为后进先出无视SO
                    //等于6 为后进先出无视SO
                    //等于7 为先进先出无视SOPO优先捡货区
                    //等于10 为先进先出无视SOPOLotData优先LotData

                    if (mark == "1" || mark == "2" || mark == "9")
                    {
                        huDetailResultList = newHuIdList.Where(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                    && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();
                    }
                    else if (mark == "3" || mark == "4" || mark == "7" || mark == "10")
                    {
                        huDetailResultList = newHuIdList.Where(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                   && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();
                    }
                    else if (mark == "5" || mark == "6")
                    {
                        huDetailResultList = newHuIdList.Where(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                  && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();
                    }

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查SO、PO、款号、属性123及Lot是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }
            }

            if (result != "")
            {
                result = "库存不足！" + result;
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    if (pickTaskDetail.Status == "U")
                    {

                        #region 如果托盘未备货

                        HuMaster humaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == pickTaskDetail.HuId && u.WhCode == pickTaskDetail.WhCode).First();
                        humaster.LoadId = "";
                        idal.IHuMasterDAL.UpdateBy(humaster, u => u.Id == humaster.Id, new string[] { "LoadId" });

                        HuMaster newhumaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == newHuId && u.WhCode == pickTaskDetail.WhCode).First();
                        newhumaster.LoadId = pickTaskDetail.LoadId;
                        idal.IHuMasterDAL.UpdateBy(newhumaster, u => u.Id == newhumaster.Id, new string[] { "LoadId" });

                        List<HuDetail> checkHuDetailList = new List<HuDetail>();
                        foreach (var item in pickTaskDetailList)
                        {
                            //原托盘减少计划数量
                            if (checkHuDetailList.Where(u => u.Id == item.HuDetailId).Count() == 0)
                            {
                                HuDetail hudetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.HuDetailId).First();
                                hudetail.PlanQty = hudetail.PlanQty - pickTaskDetail.Qty;
                                checkHuDetailList.Add(hudetail);
                            }
                            else
                            {
                                HuDetail oldhudetail = checkHuDetailList.Where(u => u.Id == item.HuDetailId).First();
                                checkHuDetailList.Remove(oldhudetail);

                                HuDetail newhudetail = oldhudetail;
                                newhudetail.PlanQty = newhudetail.PlanQty - pickTaskDetail.Qty;
                                checkHuDetailList.Add(newhudetail);
                            }
                        }

                        List<TranLog> tranLogList = new List<TranLog>();
                        foreach (var item in checkHuDetailList)
                        {
                            idal.IHuDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "PlanQty" });

                            TranLog tl = new TranLog();
                            tl.TranType = "812";
                            tl.Description = "更换托盘中更换托盘";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = entity.CreateUser;
                            tl.WhCode = item.WhCode;
                            tl.ClientCode = item.ClientCode;
                            tl.LoadId = pickTaskDetail.LoadId;
                            tl.SoNumber = item.SoNumber;
                            tl.CustomerPoNumber = item.CustomerPoNumber;
                            tl.AltItemNumber = item.AltItemNumber;
                            tl.HuId = item.HuId;
                            tl.TranQty = item.Qty;
                            tl.Remark = "托盘未备货，库存计划数量-" + item.Qty;
                            tranLogList.Add(tl);
                        }
                        idal.ITranLogDAL.Add(tranLogList);

                        foreach (var item in pickTaskDetailList)
                        {
                            idal.IPickTaskDetailDAL.DeleteBy(u => u.Id == item.Id);
                        }

                        //by订单List 
                        List<HuDetailResult> orderList = new List<HuDetailResult>();

                        //by订单验证同一托盘是否被重复释放
                        List<PickTaskDetail> checkHuDetailListByOrder = new List<PickTaskDetail>();

                        List<HuDetailResult> List = (from a in idal.IHuDetailDAL.SelectAll()
                                                     join b in idal.IHuMasterDAL.SelectAll()
                                                     on new { a.HuId, a.WhCode }
                                                     equals new { b.HuId, b.WhCode }
                                                     join c in idal.IWhLocationDAL.SelectAll()
                                                     on new { b.Location, b.WhCode }
                                                     equals new { Location = c.LocationId, c.WhCode }
                                                     where a.WhCode == pickTaskDetail.WhCode && a.HuId == newHuId &&
                                                     b.Type == "M" &&
                                                     b.Status == "A" &&
                                                     c.LocationTypeId == 1 &&
                                                     (a.Qty - (a.PlanQty ?? 0)) > 0
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
                                                         PlanQty = a.PlanQty,
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
                                                         Location = b.Location,
                                                         LocationTypeId = c.LocationTypeId,
                                                         LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                                                     }).ToList();

                        //3. 开始插入 备货任务表
                        foreach (var item in oldHuIdList)
                        {
                            //-----------------------先进先出、后进先出排序规则----------------

                            #region 库存排序
                            List<HuDetailResult> ListWhere = new List<HuDetailResult>();
                            //等于1,9 为先进先出  
                            //等于2 为后进先出
                            //等于3 为先进先出无视SOPO
                            //等于4 为后进先出无视SOPO
                            //等于5 为后进先出无视SO
                            //等于6 为后进先出无视SO
                            //等于7 为先进先出无视SOPO优先捡货区
                            //等于10 为先进先出无视SOPOLotData优先LotData

                            if (mark == "1" || mark == "9")
                            {
                                ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "2")
                            {
                                ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "3")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "4")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "5")
                            {
                                ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "6")
                            {
                                ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "7")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "10")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "11")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId).OrderBy(u => u.LotNumber1).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }


                            #endregion  结束排序

                            ///--------开始插入备货任务表--------------------
                            int qtyResult = (Int32)item.Qty;  //得到出库订单的数量
                            foreach (var sqlDetail in ListWhere)    //得到经过筛选后的库存
                            {
                                if (qtyResult <= 0)
                                {
                                    break;
                                }
                                else
                                {
                                    int? sqlDetailQty = sqlDetail.Qty;
                                    if (checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                                    {
                                        int getQty = checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                                        sqlDetailQty = sqlDetail.Qty - getQty;
                                    }
                                    if (sqlDetailQty <= 0)
                                    {
                                        continue;
                                    }

                                    PickTaskDetail detail = new PickTaskDetail();
                                    if (ReleaseType == "2")  //by 订单释放
                                        detail.OutBoundOrderId = item.OutBoundOrderId;

                                    detail.WhCode = sqlDetail.WhCode;
                                    detail.LoadId = pickTaskDetail.LoadId;
                                    detail.HuDetailId = sqlDetail.Id;
                                    detail.ClientCode = sqlDetail.ClientCode;
                                    detail.HuId = sqlDetail.HuId;
                                    detail.Location = sqlDetail.Location;
                                    detail.SoNumber = sqlDetail.SoNumber;
                                    detail.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                    detail.AltItemNumber = sqlDetail.AltItemNumber;
                                    detail.ItemId = (Int32)sqlDetail.ItemId;
                                    detail.UnitId = sqlDetail.UnitId;
                                    detail.UnitName = sqlDetail.UnitName;
                                    if (qtyResult >= sqlDetailQty - (sqlDetail.PlanQty ?? 0))
                                    {
                                        detail.Qty = (Int32)sqlDetailQty - (sqlDetail.PlanQty ?? 0);  //释放数量
                                        qtyResult = qtyResult - detail.Qty;    //剩余的未释放数量
                                    }
                                    else
                                    {
                                        detail.Qty = qtyResult;
                                        qtyResult = 0;
                                    }
                                    detail.PickQty = 0;
                                    detail.Length = (sqlDetail.Length ?? 0);
                                    detail.Width = (sqlDetail.Width ?? 0);
                                    detail.Height = (sqlDetail.Height ?? 0);
                                    detail.Weight = (sqlDetail.Weight ?? 0);
                                    detail.LotNumber1 = sqlDetail.LotNumber1;
                                    detail.LotNumber2 = sqlDetail.LotNumber2;
                                    detail.LotDate = sqlDetail.LotDate;
                                    detail.Sequence = 0;
                                    detail.Status = "U";
                                    detail.Status1 = "U";
                                    detail.CreateUser = entity.CreateUser;
                                    detail.CreateDate = DateTime.Now;
                                    detail.ReceiptDate = sqlDetail.ReceiptDate;
                                    checkHuDetailListByOrder.Add(detail);

                                    HuDetail huDetail = new HuDetail();

                                    //2016年修改  --张雨佳
                                    //by 订单释放修改
                                    //如果多个订单包含同一个款号 因这里没有实时更新 导致只会更新最后一次出库订单的数量
                                    //现增加List累加后 最后执行一次保存就没有问题
                                    if (ReleaseType == "2")
                                    {
                                        if (orderList.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                                        {
                                            HuDetailResult en = new HuDetailResult();
                                            en.Id = sqlDetail.Id;
                                            en.PlanQty = detail.Qty;
                                            orderList.Add(en);
                                        }
                                        else
                                        {
                                            HuDetailResult en = orderList.Where(u => u.Id == sqlDetail.Id).First();
                                            HuDetailResult en1 = new HuDetailResult();
                                            en1.Id = sqlDetail.Id;
                                            en1.PlanQty = en.PlanQty + detail.Qty;
                                            orderList.Add(en1);
                                            orderList.Remove(en);
                                        }
                                    }
                                    else
                                    {
                                        //更新by Load 情况的 库存数量

                                        TranLog tl = new TranLog();
                                        tl.TranType = "30";
                                        tl.Description = "释放Load";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = entity.CreateUser;
                                        tl.WhCode = entity.WhCode;
                                        tl.ClientCode = sqlDetail.ClientCode;
                                        tl.SoNumber = sqlDetail.SoNumber;
                                        tl.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                        tl.AltItemNumber = sqlDetail.AltItemNumber;
                                        tl.Location = sqlDetail.Location;
                                        tl.ItemId = sqlDetail.ItemId;
                                        tl.UnitID = sqlDetail.UnitId;
                                        tl.UnitName = sqlDetail.UnitName;
                                        tl.ReceiptDate = sqlDetail.ReceiptDate;
                                        tl.TranQty = (sqlDetail.PlanQty ?? 0);
                                        tl.HuId = sqlDetail.HuId;
                                        tl.Length = sqlDetail.Length;
                                        tl.Width = sqlDetail.Width;
                                        tl.Height = sqlDetail.Height;
                                        tl.Weight = sqlDetail.Weight;
                                        tl.LotNumber1 = sqlDetail.LotNumber1;
                                        tl.LotNumber2 = sqlDetail.LotNumber2;
                                        tl.LotDate = sqlDetail.LotDate;
                                        tl.LoadId = pickTaskDetail.LoadId;
                                        tl.Remark = "锁定数量+" + detail.Qty;

                                        //更新by Load 情况的 库存数量
                                        huDetail.PlanQty = (sqlDetail.PlanQty ?? 0) + detail.Qty;

                                        huDetail.UpdateUser = entity.CreateUser;
                                        huDetail.UpdateDate = DateTime.Now;
                                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == sqlDetail.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                        tl.TranQty2 = huDetail.PlanQty;
                                        tranLogList.Add(tl);
                                    }
                                }
                            }
                        }

                        List<PickTaskDetail> PickTaskDetailList = new List<PickTaskDetail>();
                        if (ReleaseType == "2")  //by 订单释放
                        {
                            foreach (var item in checkHuDetailListByOrder.Where(u => u.Qty > 0))
                            {
                                if (PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).Count() == 0)
                                {
                                    PickTaskDetail detail = new PickTaskDetail();
                                    detail.OutBoundOrderId = item.OutBoundOrderId;
                                    detail.WhCode = item.WhCode;
                                    detail.LoadId = item.LoadId;
                                    detail.HuDetailId = item.HuDetailId;
                                    detail.ClientCode = item.ClientCode;
                                    detail.HuId = item.HuId;
                                    detail.Location = item.Location;
                                    detail.SoNumber = item.SoNumber;
                                    detail.CustomerPoNumber = item.CustomerPoNumber;
                                    detail.AltItemNumber = item.AltItemNumber;
                                    detail.ItemId = item.ItemId;
                                    detail.UnitId = item.UnitId;
                                    detail.UnitName = item.UnitName;
                                    detail.Qty = item.Qty;
                                    detail.PickQty = 0;
                                    detail.Length = (item.Length ?? 0);
                                    detail.Width = (item.Width ?? 0);
                                    detail.Height = (item.Height ?? 0);
                                    detail.Weight = (item.Weight ?? 0);
                                    detail.LotNumber1 = item.LotNumber1;
                                    detail.LotNumber2 = item.LotNumber2;
                                    detail.LotDate = item.LotDate;
                                    detail.Sequence = item.Sequence;
                                    detail.Status = "U";
                                    detail.Status1 = "U";
                                    detail.CreateUser = item.CreateUser;
                                    detail.CreateDate = DateTime.Now;
                                    detail.ReceiptDate = item.ReceiptDate;
                                    PickTaskDetailList.Add(detail);
                                }
                                else
                                {
                                    PickTaskDetail en = PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).First();
                                    PickTaskDetail en1 = en;
                                    en1.Qty = en.Qty + item.Qty;

                                    PickTaskDetailList.Remove(en);
                                    PickTaskDetailList.Add(en1);
                                }
                            }
                            //添加备货任务信息
                            idal.IPickTaskDetailDAL.Add(PickTaskDetailList.Where(u => u.Qty > 0));
                        }
                        else
                        {
                            //添加备货任务信息
                            idal.IPickTaskDetailDAL.Add(checkHuDetailListByOrder.Where(u => u.Qty > 0));
                        }

                        //更新 by订单情况的 库存数量
                        if (orderList != null)
                        {
                            foreach (var item in orderList)
                            {
                                HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                                TranLog tl = new TranLog();
                                tl.TranType = "30";
                                tl.Description = "释放Load";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = entity.CreateUser;
                                tl.WhCode = entity.WhCode;
                                tl.ClientCode = huDetail.ClientCode;
                                tl.SoNumber = huDetail.SoNumber;
                                tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                                tl.AltItemNumber = huDetail.AltItemNumber;
                                tl.ItemId = huDetail.ItemId;
                                tl.UnitID = huDetail.UnitId;
                                tl.UnitName = huDetail.UnitName;
                                tl.ReceiptId = huDetail.ReceiptId;
                                tl.ReceiptDate = huDetail.ReceiptDate;
                                tl.TranQty = (huDetail.PlanQty ?? 0);
                                tl.HuId = huDetail.HuId;
                                tl.Length = huDetail.Length;
                                tl.Width = huDetail.Width;
                                tl.Height = huDetail.Height;
                                tl.Weight = huDetail.Weight;
                                tl.LotNumber1 = huDetail.LotNumber1;
                                tl.LotNumber2 = huDetail.LotNumber2;
                                tl.LotDate = huDetail.LotDate;
                                tl.LoadId = pickTaskDetail.LoadId;
                                tl.Remark = "锁定数量+" + item.PlanQty;

                                huDetail.PlanQty = item.PlanQty + (huDetail.PlanQty ?? 0);
                                huDetail.UpdateUser = entity.CreateUser;
                                huDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                tl.TranQty2 = huDetail.PlanQty;
                                tranLogList.Add(tl);
                            }
                        }
                        //保存释放日志
                        idal.ITranLogDAL.Add(tranLogList);

                        #endregion

                    }
                    else
                    {
                        #region 如果托盘已备货

                        HuMaster humaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == pickTaskDetail.HuId && u.WhCode == pickTaskDetail.WhCode).First();

                        string locationId = humaster.Location;
                        humaster.LoadId = "";
                        humaster.Location = pickTaskDetail.Location;
                        idal.IHuMasterDAL.UpdateBy(humaster, u => u.Id == humaster.Id, new string[] { "Location", "LoadId" });

                        HuMaster newhumaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == newHuId && u.WhCode == pickTaskDetail.WhCode).First();
                        newhumaster.LoadId = pickTaskDetail.LoadId;
                        newhumaster.Location = locationId;
                        idal.IHuMasterDAL.UpdateBy(newhumaster, u => u.Id == newhumaster.Id, new string[] { "Location", "LoadId" });

                        List<TranLog> tranLogList = new List<TranLog>();

                        if (1 == 1)
                        {
                            TranLog tl = new TranLog();
                            tl.TranType = "812";
                            tl.Description = "更换托盘中更换托盘";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = entity.CreateUser;
                            tl.WhCode = entity.WhCode;
                            tl.ClientCode = entity.ClientCode;
                            tl.LoadId = pickTaskDetail.LoadId;
                            tl.SoNumber = entity.SoNumber;
                            tl.CustomerPoNumber = entity.CustomerPoNumber;
                            tl.AltItemNumber = entity.AltItemNumber;
                            tl.HuId = entity.HuId;
                            tl.TranQty = entity.Qty;
                            tl.Remark = "托盘未备货，库存计划数量-" + entity.Qty;
                            tranLogList.Add(tl);
                        }

                        idal.ITranLogDAL.Add(tranLogList);

                        foreach (var item in pickTaskDetailList)
                        {
                            idal.IPickTaskDetailDAL.DeleteBy(u => u.Id == item.Id);
                        }

                        //by订单List 
                        List<HuDetailResult> orderList = new List<HuDetailResult>();

                        //by订单验证同一托盘是否被重复释放
                        List<PickTaskDetail> checkHuDetailListByOrder = new List<PickTaskDetail>();

                        List<HuDetailResult> List = (from a in idal.IHuDetailDAL.SelectAll()
                                                     join b in idal.IHuMasterDAL.SelectAll()
                                                     on new { a.HuId, a.WhCode }
                                                     equals new { b.HuId, b.WhCode }
                                                     join c in idal.IWhLocationDAL.SelectAll()
                                                     on new { b.Location, b.WhCode }
                                                     equals new { Location = c.LocationId, c.WhCode }
                                                     where a.WhCode == pickTaskDetail.WhCode && a.HuId == newHuId &&
                                                     b.Type == "M" &&
                                                     b.Status == "A" &&
                                                     c.LocationTypeId == 1 &&
                                                     (a.Qty - (a.PlanQty ?? 0)) > 0
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
                                                         PlanQty = a.PlanQty,
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
                                                         Location = b.Location,
                                                         LocationTypeId = c.LocationTypeId,
                                                         LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                                                     }).ToList();

                        //3. 开始插入 备货任务表
                        foreach (var item in oldHuIdList)
                        {
                            //-----------------------先进先出、后进先出排序规则----------------

                            #region 库存排序
                            List<HuDetailResult> ListWhere = new List<HuDetailResult>();
                            //等于1,9 为先进先出  
                            //等于2 为后进先出
                            //等于3 为先进先出无视SOPO
                            //等于4 为后进先出无视SOPO
                            //等于5 为后进先出无视SO
                            //等于6 为后进先出无视SO
                            //等于7 为先进先出无视SOPO优先捡货区
                            //等于10 为先进先出无视SOPOLotData优先LotData

                            if (mark == "1" || mark == "9")
                            {
                                ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "2")
                            {
                                ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "3")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "4")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "5")
                            {
                                ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "6")
                            {
                                ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                            }
                            else if (mark == "7")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "10")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }
                            else if (mark == "11")
                            {
                                ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId).OrderBy(u => u.LotNumber1).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                            }


                            #endregion  结束排序

                            ///--------开始插入备货任务表--------------------
                            int qtyResult = (Int32)item.Qty;  //得到出库订单的数量
                            foreach (var sqlDetail in ListWhere)    //得到经过筛选后的库存
                            {
                                if (qtyResult <= 0)
                                {
                                    break;
                                }
                                else
                                {
                                    int? sqlDetailQty = sqlDetail.Qty;
                                    if (checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                                    {
                                        int getQty = checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                                        sqlDetailQty = sqlDetail.Qty - getQty;
                                    }
                                    if (sqlDetailQty <= 0)
                                    {
                                        continue;
                                    }

                                    PickTaskDetail detail = new PickTaskDetail();
                                    if (ReleaseType == "2")  //by 订单释放
                                        detail.OutBoundOrderId = item.OutBoundOrderId;

                                    detail.WhCode = sqlDetail.WhCode;
                                    detail.LoadId = pickTaskDetail.LoadId;
                                    detail.HuDetailId = sqlDetail.Id;
                                    detail.ClientCode = sqlDetail.ClientCode;
                                    detail.HuId = sqlDetail.HuId;
                                    detail.Location = sqlDetail.Location;
                                    detail.SoNumber = sqlDetail.SoNumber;
                                    detail.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                    detail.AltItemNumber = sqlDetail.AltItemNumber;
                                    detail.ItemId = (Int32)sqlDetail.ItemId;
                                    detail.UnitId = sqlDetail.UnitId;
                                    detail.UnitName = sqlDetail.UnitName;
                                    if (qtyResult >= sqlDetailQty - (sqlDetail.PlanQty ?? 0))
                                    {
                                        detail.Qty = (Int32)sqlDetailQty - (sqlDetail.PlanQty ?? 0);  //释放数量
                                        qtyResult = qtyResult - detail.Qty;    //剩余的未释放数量
                                    }
                                    else
                                    {
                                        detail.Qty = qtyResult;
                                        qtyResult = 0;
                                    }
                                    detail.PickQty = 0;
                                    detail.Length = (sqlDetail.Length ?? 0);
                                    detail.Width = (sqlDetail.Width ?? 0);
                                    detail.Height = (sqlDetail.Height ?? 0);
                                    detail.Weight = (sqlDetail.Weight ?? 0);
                                    detail.LotNumber1 = sqlDetail.LotNumber1;
                                    detail.LotNumber2 = sqlDetail.LotNumber2;
                                    detail.LotDate = sqlDetail.LotDate;
                                    detail.Sequence = 0;
                                    detail.Status = "U";
                                    detail.Status1 = "U";
                                    detail.CreateUser = entity.CreateUser;
                                    detail.CreateDate = DateTime.Now;
                                    detail.ReceiptDate = sqlDetail.ReceiptDate;
                                    checkHuDetailListByOrder.Add(detail);

                                    HuDetail huDetail = new HuDetail();

                                    //2016年修改  --张雨佳
                                    //by 订单释放修改
                                    //如果多个订单包含同一个款号 因这里没有实时更新 导致只会更新最后一次出库订单的数量
                                    //现增加List累加后 最后执行一次保存就没有问题
                                    if (ReleaseType == "2")
                                    {
                                        if (orderList.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                                        {
                                            HuDetailResult en = new HuDetailResult();
                                            en.Id = sqlDetail.Id;
                                            en.PlanQty = detail.Qty;
                                            orderList.Add(en);
                                        }
                                        else
                                        {
                                            HuDetailResult en = orderList.Where(u => u.Id == sqlDetail.Id).First();
                                            HuDetailResult en1 = new HuDetailResult();
                                            en1.Id = sqlDetail.Id;
                                            en1.PlanQty = en.PlanQty + detail.Qty;
                                            orderList.Add(en1);
                                            orderList.Remove(en);
                                        }
                                    }
                                    else
                                    {
                                        //更新by Load 情况的 库存数量

                                        TranLog tl = new TranLog();
                                        tl.TranType = "30";
                                        tl.Description = "释放Load";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = entity.CreateUser;
                                        tl.WhCode = entity.WhCode;
                                        tl.ClientCode = sqlDetail.ClientCode;
                                        tl.SoNumber = sqlDetail.SoNumber;
                                        tl.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                        tl.AltItemNumber = sqlDetail.AltItemNumber;
                                        tl.Location = sqlDetail.Location;
                                        tl.ItemId = sqlDetail.ItemId;
                                        tl.UnitID = sqlDetail.UnitId;
                                        tl.UnitName = sqlDetail.UnitName;
                                        tl.ReceiptDate = sqlDetail.ReceiptDate;
                                        tl.TranQty = (sqlDetail.PlanQty ?? 0);
                                        tl.HuId = sqlDetail.HuId;
                                        tl.Length = sqlDetail.Length;
                                        tl.Width = sqlDetail.Width;
                                        tl.Height = sqlDetail.Height;
                                        tl.Weight = sqlDetail.Weight;
                                        tl.LotNumber1 = sqlDetail.LotNumber1;
                                        tl.LotNumber2 = sqlDetail.LotNumber2;
                                        tl.LotDate = sqlDetail.LotDate;
                                        tl.LoadId = pickTaskDetail.LoadId;
                                        tl.Remark = "锁定数量+" + detail.Qty;

                                        //更新by Load 情况的 库存数量
                                        huDetail.PlanQty = (sqlDetail.PlanQty ?? 0) + detail.Qty;

                                        huDetail.UpdateUser = entity.CreateUser;
                                        huDetail.UpdateDate = DateTime.Now;
                                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == sqlDetail.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                        tl.TranQty2 = huDetail.PlanQty;
                                        tranLogList.Add(tl);
                                    }
                                }
                            }
                        }

                        List<PickTaskDetail> PickTaskDetailList = new List<PickTaskDetail>();
                        if (ReleaseType == "2")  //by 订单释放
                        {
                            foreach (var item in checkHuDetailListByOrder.Where(u => u.Qty > 0))
                            {
                                if (PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).Count() == 0)
                                {
                                    PickTaskDetail detail = new PickTaskDetail();
                                    detail.OutBoundOrderId = item.OutBoundOrderId;
                                    detail.WhCode = item.WhCode;
                                    detail.LoadId = item.LoadId;
                                    detail.HuDetailId = item.HuDetailId;
                                    detail.ClientCode = item.ClientCode;
                                    detail.HuId = item.HuId;
                                    detail.Location = item.Location;
                                    detail.SoNumber = item.SoNumber;
                                    detail.CustomerPoNumber = item.CustomerPoNumber;
                                    detail.AltItemNumber = item.AltItemNumber;
                                    detail.ItemId = item.ItemId;
                                    detail.UnitId = item.UnitId;
                                    detail.UnitName = item.UnitName;
                                    detail.Qty = item.Qty;
                                    detail.PickQty = 0;
                                    detail.Length = (item.Length ?? 0);
                                    detail.Width = (item.Width ?? 0);
                                    detail.Height = (item.Height ?? 0);
                                    detail.Weight = (item.Weight ?? 0);
                                    detail.LotNumber1 = item.LotNumber1;
                                    detail.LotNumber2 = item.LotNumber2;
                                    detail.LotDate = item.LotDate;
                                    detail.Sequence = item.Sequence;
                                    detail.Status = "U";
                                    detail.Status1 = "U";
                                    detail.CreateUser = item.CreateUser;
                                    detail.CreateDate = DateTime.Now;
                                    detail.ReceiptDate = item.ReceiptDate;
                                    PickTaskDetailList.Add(detail);
                                }
                                else
                                {
                                    PickTaskDetail en = PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).First();
                                    PickTaskDetail en1 = en;
                                    en1.Qty = en.Qty + item.Qty;

                                    PickTaskDetailList.Remove(en);
                                    PickTaskDetailList.Add(en1);
                                }
                            }
                            //添加备货任务信息
                            idal.IPickTaskDetailDAL.Add(PickTaskDetailList.Where(u => u.Qty > 0));
                        }
                        else
                        {
                            //添加备货任务信息
                            idal.IPickTaskDetailDAL.Add(checkHuDetailListByOrder.Where(u => u.Qty > 0));
                        }

                        //更新 by订单情况的 库存数量
                        if (orderList != null)
                        {
                            foreach (var item in orderList)
                            {
                                HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                                TranLog tl = new TranLog();
                                tl.TranType = "30";
                                tl.Description = "释放Load";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = entity.CreateUser;
                                tl.WhCode = entity.WhCode;
                                tl.ClientCode = huDetail.ClientCode;
                                tl.SoNumber = huDetail.SoNumber;
                                tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                                tl.AltItemNumber = huDetail.AltItemNumber;
                                tl.ItemId = huDetail.ItemId;
                                tl.UnitID = huDetail.UnitId;
                                tl.UnitName = huDetail.UnitName;
                                tl.ReceiptId = huDetail.ReceiptId;
                                tl.ReceiptDate = huDetail.ReceiptDate;
                                tl.TranQty = (huDetail.PlanQty ?? 0);
                                tl.HuId = huDetail.HuId;
                                tl.Length = huDetail.Length;
                                tl.Width = huDetail.Width;
                                tl.Height = huDetail.Height;
                                tl.Weight = huDetail.Weight;
                                tl.LotNumber1 = huDetail.LotNumber1;
                                tl.LotNumber2 = huDetail.LotNumber2;
                                tl.LotDate = huDetail.LotDate;
                                tl.LoadId = pickTaskDetail.LoadId;
                                tl.Remark = "锁定数量+" + item.PlanQty;

                                huDetail.PlanQty = item.PlanQty + (huDetail.PlanQty ?? 0);
                                huDetail.UpdateUser = entity.CreateUser;
                                huDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                tl.TranQty2 = huDetail.PlanQty;
                                tranLogList.Add(tl);
                            }
                        }
                        //保存释放日志
                        idal.ITranLogDAL.Add(tranLogList);

                        #endregion

                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                    return "更换SN异常，请重试！";
                }
            }
        }


        //删除托盘
        public string PickTaskDetailHuIdDel(PickTaskDetailResult2 entity)
        {
            List<PickTaskDetail> pickTaskDetailList = idal.IPickTaskDetailDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && (u.SoNumber ?? "") == (entity.SoNumber ?? "") && (u.CustomerPoNumber ?? "") == (entity.CustomerPoNumber ?? "") && u.ItemId == entity.ItemId && u.UnitName == entity.UnitName && u.ClientCode == entity.ClientCode && (u.LotNumber1 ?? "") == (entity.LotNumber1 ?? "")
                    && (u.LotNumber2 ?? "") == (entity.LotNumber2 ?? ""));

            if (pickTaskDetailList.Count == 0)
            {
                return "删除SN异常，请重新查询后提交！";
            }

            PickTaskDetail pickTaskDetail = pickTaskDetailList.First();
            if (pickTaskDetail.Status1 != "U")
            {
                return "已装箱无法再删除！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //如果没有备货，直接还原库存的计划数量，Load的出货数量与计划数量
                    if (pickTaskDetail.Status == "U")
                    {
                        #region 如果托盘未备货

                        HuMaster humaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == pickTaskDetail.HuId && u.WhCode == pickTaskDetail.WhCode).First();
                        humaster.LoadId = "";
                        idal.IHuMasterDAL.UpdateBy(humaster, u => u.Id == humaster.Id, new string[] { "LoadId" });

                        List<HuDetail> checkHuDetailList = new List<HuDetail>();
                        foreach (var item in pickTaskDetailList)
                        {
                            if (checkHuDetailList.Where(u => u.Id == item.HuDetailId).Count() == 0)
                            {
                                HuDetail hudetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.HuDetailId).First();
                                hudetail.PlanQty = hudetail.PlanQty - pickTaskDetail.Qty;
                                checkHuDetailList.Add(hudetail);
                            }
                            else
                            {
                                HuDetail oldhudetail = checkHuDetailList.Where(u => u.Id == item.HuDetailId).First();
                                checkHuDetailList.Remove(oldhudetail);

                                HuDetail newhudetail = oldhudetail;
                                newhudetail.PlanQty = newhudetail.PlanQty - pickTaskDetail.Qty;
                                checkHuDetailList.Add(newhudetail);
                            }
                        }

                        List<TranLog> tranLogList = new List<TranLog>();
                        foreach (var item in checkHuDetailList)
                        {
                            idal.IHuDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "PlanQty" });

                            TranLog tl = new TranLog();
                            tl.TranType = "810";
                            tl.Description = "更换托盘中删除托盘";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = entity.CreateUser;
                            tl.WhCode = item.WhCode;
                            tl.ClientCode = item.ClientCode;
                            tl.LoadId = pickTaskDetail.LoadId;
                            tl.SoNumber = item.SoNumber;
                            tl.CustomerPoNumber = item.CustomerPoNumber;
                            tl.AltItemNumber = item.AltItemNumber;
                            tl.HuId = item.HuId;
                            tl.TranQty = item.Qty;
                            tl.Remark = "托盘未备货，库存计划数量-" + item.Qty;
                            tranLogList.Add(tl);
                        }

                        idal.ITranLogDAL.Add(tranLogList);

                        int sumQty = pickTaskDetailList.Sum(u => u.Qty);
                        LoadMaster loadmaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == pickTaskDetail.LoadId && u.WhCode == pickTaskDetail.WhCode).First();
                        loadmaster.SumQty = loadmaster.SumQty - sumQty;
                        idal.ILoadMasterDAL.UpdateBy(loadmaster, u => u.Id == loadmaster.Id, new string[] { "SumQty" });

                        LoadDetail loaddetail = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadmaster.Id).First();

                        //等于1,9 为先进先出  
                        //等于2 为后进先出
                        //等于3 为先进先出无视SOPO
                        //等于4 为后进先出无视SOPO
                        //等于5 为后进先出无视SO
                        //等于6 为后进先出无视SO
                        //等于7 为先进先出无视SOPO优先捡货区
                        //等于10 为先进先出无视SOPOLotData优先LotData

                        List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                           where a.FlowHeadId == loadmaster.ProcessId
                                                           select a).ToList();
                        string mark = flowDetailList.Where(u => u.Type == "Release").First().Mark;

                        List<OutBoundOrderDetail> OutBoundOrderDetailList = new List<OutBoundOrderDetail>();
                        if (mark == "1" || mark == "9" || mark == "2")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }

                        }
                        else if (mark == "3" || mark == "4" || mark == "7" || mark == "10")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }
                        }
                        else if (mark == "5" || mark == "6")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region 如果托盘已备货

                        HuMaster humaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == pickTaskDetail.HuId && u.WhCode == pickTaskDetail.WhCode).First();
                        humaster.Location = pickTaskDetail.Location;
                        humaster.LoadId = "";
                        idal.IHuMasterDAL.UpdateBy(humaster, u => u.Id == humaster.Id, new string[] { "Location", "LoadId" });

                        int sumQty = pickTaskDetailList.Sum(u => u.Qty);
                        LoadMaster loadmaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == pickTaskDetail.LoadId && u.WhCode == pickTaskDetail.WhCode).First();
                        loadmaster.SumQty = loadmaster.SumQty - sumQty;
                        idal.ILoadMasterDAL.UpdateBy(loadmaster, u => u.Id == loadmaster.Id, new string[] { "SumQty" });

                        LoadDetail loaddetail = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadmaster.Id).First();

                        //等于1,9 为先进先出  
                        //等于2 为后进先出
                        //等于3 为先进先出无视SOPO
                        //等于4 为后进先出无视SOPO
                        //等于5 为后进先出无视SO
                        //等于6 为后进先出无视SO
                        //等于7 为先进先出无视SOPO优先捡货区
                        //等于10 为先进先出无视SOPOLotData优先LotData

                        List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                           where a.FlowHeadId == loadmaster.ProcessId
                                                           select a).ToList();
                        string mark = flowDetailList.Where(u => u.Type == "Release").First().Mark;

                        List<OutBoundOrderDetail> OutBoundOrderDetailList = new List<OutBoundOrderDetail>();
                        if (mark == "1" || mark == "2" || mark == "9")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }

                        }
                        else if (mark == "3" || mark == "4" || mark == "7" || mark == "10")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }
                        }
                        else if (mark == "5" || mark == "6")
                        {
                            foreach (var item in pickTaskDetailList)
                            {
                                int qty = item.Qty;

                                List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == loaddetail.OutBoundOrderId && u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? ""));

                                foreach (var orderDetail in orderDetailList)
                                {
                                    if (qty == 0)
                                    {
                                        break;
                                    }
                                    if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                    {
                                        orderDetail.Qty = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First().Qty;
                                        if (orderDetail.Qty == 0)
                                        {
                                            continue;
                                        }
                                    }

                                    if (qty >= orderDetail.Qty)
                                    {
                                        qty = qty - orderDetail.Qty;

                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = 0;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }
                                    }
                                    else
                                    {
                                        if (OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).Count() > 0)
                                        {
                                            OutBoundOrderDetail oldoutbound = OutBoundOrderDetailList.Where(u => u.Id == orderDetail.Id).First();
                                            OutBoundOrderDetailList.Remove(oldoutbound);

                                            OutBoundOrderDetail newoutbound = oldoutbound;
                                            newoutbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(newoutbound);
                                        }
                                        else
                                        {
                                            OutBoundOrderDetail outbound = orderDetail;
                                            outbound.Qty = orderDetail.Qty - qty;
                                            OutBoundOrderDetailList.Add(outbound);
                                        }

                                        qty = 0;
                                    }
                                }
                            }

                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty > 0))
                            {
                                idal.IOutBoundOrderDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "Qty" });
                            }
                            foreach (var item in OutBoundOrderDetailList.Where(u => u.Qty == 0))
                            {
                                idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.Id == item.Id);
                            }
                        }

                        TranLog tl = new TranLog();
                        tl.TranType = "811";
                        tl.Description = "更换托盘中删除托盘";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.CreateUser;
                        tl.WhCode = pickTaskDetail.WhCode;
                        tl.ClientCode = pickTaskDetail.ClientCode;
                        tl.LoadId = pickTaskDetail.LoadId;
                        tl.SoNumber = pickTaskDetail.SoNumber;
                        tl.CustomerPoNumber = pickTaskDetail.CustomerPoNumber;
                        tl.AltItemNumber = pickTaskDetail.AltItemNumber;
                        tl.HuId = pickTaskDetail.HuId;
                        tl.Remark = "托盘已备货，库存数量未变更,还原至库位" + pickTaskDetail.Location;
                        idal.ITranLogDAL.Add(tl);

                        #endregion
                    }

                    foreach (var item in pickTaskDetailList)
                    {
                        idal.IPickTaskDetailDAL.DeleteBy(u => u.Id == item.Id);
                    }

                    idal.ILoadHuIdExtendDAL.DeleteBy(u => u.LoadId == pickTaskDetail.LoadId && u.WhCode == pickTaskDetail.WhCode && u.HuId == entity.HuId);

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                    return "删除异常，请重试！";
                }
            }
        }


        //新增托盘
        public string PickTaskDetailHuIdAdd(PickTaskDetailResult2 entity)
        {
            LoadMaster loadmaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).First();
            if (loadmaster.ShipDate != null)
            {
                return "Load已封箱，无法再做任何操作！";
            }

            List<HuDetailResult> List = (from a in idal.IHuDetailDAL.SelectAll()
                                         join b in idal.IHuMasterDAL.SelectAll()
                                         on new { a.HuId, a.WhCode }
                                         equals new { b.HuId, b.WhCode }
                                         join c in idal.IWhLocationDAL.SelectAll()
                                         on new { b.Location, b.WhCode }
                                         equals new { Location = c.LocationId, c.WhCode }
                                         where a.WhCode == entity.WhCode && a.HuId == entity.HuId &&
                                         b.Type == "M" &&
                                         b.Status == "A" &&
                                         c.LocationTypeId == 1 &&
                                         (a.Qty - (a.PlanQty ?? 0)) > 0
                                         select new HuDetailResult
                                         {
                                             Id = a.Id,
                                             HuId = a.HuId,
                                             WhCode = a.WhCode,
                                             ClientId = a.ClientId,
                                             ClientCode = a.ClientCode,
                                             SoNumber = a.SoNumber ?? "",
                                             CustomerPoNumber = a.CustomerPoNumber ?? "",
                                             AltItemNumber = a.AltItemNumber,
                                             ReceiptDate = a.ReceiptDate,
                                             PlanQty = a.PlanQty,
                                             Qty = a.Qty,
                                             ItemId = a.ItemId,
                                             UnitId = a.UnitId,
                                             UnitName = a.UnitName ?? "",
                                             Height = a.Height,
                                             Length = a.Length,
                                             Weight = a.Weight,
                                             Width = a.Width,
                                             LotNumber1 = a.LotNumber1 ?? "",
                                             LotNumber2 = a.LotNumber2 ?? "",
                                             LotDate = a.LotDate,
                                             Location = b.Location,
                                             LocationTypeId = c.LocationTypeId,
                                             LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                                         }).ToList();


            List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                               where a.FlowHeadId == loadmaster.ProcessId
                                               select a).ToList();
            string mark = flowDetailList.Where(u => u.Type == "Release").First().Mark;
            string ReleaseType = flowDetailList.Where(u => u.Type == "ReleaseType").First().Mark;

            LoadDetail loaddetail = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadmaster.Id).First();

            if (List.Count == 0)
            {
                return "可用库存数量不足，请查询后再操作！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    List<PickTaskDetail> PickTaskDetailList = new List<PickTaskDetail>();
                    List<OutBoundOrderDetail> outBoundOrderDetailListAdd = new List<OutBoundOrderDetail>();

                    foreach (var item in List)
                    {
                        int chayiQty = Convert.ToInt32(item.Qty - (item.PlanQty ?? 0));

                        PickTaskDetail detail = new PickTaskDetail();
                        if (ReleaseType == "2")  //by 订单释放
                            detail.OutBoundOrderId = loaddetail.OutBoundOrderId;

                        detail.WhCode = item.WhCode;
                        detail.LoadId = entity.LoadId;
                        detail.HuDetailId = item.Id;
                        detail.ClientCode = item.ClientCode;
                        detail.HuId = item.HuId;
                        detail.Location = item.Location;
                        detail.SoNumber = item.SoNumber;
                        detail.CustomerPoNumber = item.CustomerPoNumber;
                        detail.AltItemNumber = item.AltItemNumber;
                        detail.ItemId = Convert.ToInt32(item.ItemId);
                        detail.UnitId = item.UnitId;
                        detail.UnitName = item.UnitName;
                        detail.Qty = chayiQty;
                        detail.PickQty = 0;
                        detail.Length = (item.Length ?? 0);
                        detail.Width = (item.Width ?? 0);
                        detail.Height = (item.Height ?? 0);
                        detail.Weight = (item.Weight ?? 0);
                        detail.LotNumber1 = item.LotNumber1;
                        detail.LotNumber2 = item.LotNumber2;
                        detail.LotDate = item.LotDate;
                        detail.Sequence = 0;
                        detail.Status = "U";
                        detail.Status1 = "U";
                        detail.CreateUser = entity.CreateUser;
                        detail.CreateDate = DateTime.Now;
                        detail.ReceiptDate = item.ReceiptDate;
                        PickTaskDetailList.Add(detail);

                        if (outBoundOrderDetailListAdd.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.SoNumber == item.SoNumber && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.ItemId == item.ItemId && u.UnitName == item.UnitName).Count() == 0)
                        {
                            OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                            orderDetail.OutPoNumber = "";
                            orderDetail.OutBoundOrderId = Convert.ToInt32(loaddetail.OutBoundOrderId);
                            orderDetail.WhCode = item.WhCode;
                            orderDetail.SoNumber = item.SoNumber;
                            orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                            orderDetail.AltItemNumber = item.AltItemNumber;
                            orderDetail.ItemId = Convert.ToInt32(item.ItemId);
                            orderDetail.UnitId = 0;
                            orderDetail.UnitName = item.UnitName;
                            orderDetail.Qty = chayiQty;
                            orderDetail.DSFLag = 0;
                            orderDetail.LotNumber1 = item.LotNumber1;
                            orderDetail.LotNumber2 = item.LotNumber2;
                            orderDetail.CreateUser = entity.CreateUser;
                            orderDetail.CreateDate = DateTime.Now;
                            outBoundOrderDetailListAdd.Add(orderDetail);
                        }
                        else
                        {
                            OutBoundOrderDetail oldorderDetail = outBoundOrderDetailListAdd.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.SoNumber == item.SoNumber && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.ItemId == item.ItemId && u.UnitName == item.UnitName).First();
                            outBoundOrderDetailListAdd.Remove(oldorderDetail);

                            OutBoundOrderDetail neworderDetail = oldorderDetail;
                            neworderDetail.Qty += chayiQty;
                            outBoundOrderDetailListAdd.Add(neworderDetail);
                        }

                        //修改库存计划数量
                        HuDetail hudetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();
                        hudetail.PlanQty = (hudetail.PlanQty ?? 0) + chayiQty;
                        idal.IHuDetailDAL.UpdateBy(hudetail, u => u.Id == item.Id, new string[] { "PlanQty" });
                    }
                    idal.IPickTaskDetailDAL.Add(PickTaskDetailList);
                    idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetailListAdd);

                    LoadHuIdExtend loadHuIdExtend = new LoadHuIdExtend();
                    loadHuIdExtend.WhCode = entity.WhCode;
                    loadHuIdExtend.HuId = entity.HuId;
                    loadHuIdExtend.CreateUser = entity.CreateUser;
                    loadHuIdExtend.CreateDate = DateTime.Now;
                    idal.ILoadHuIdExtendDAL.Add(loadHuIdExtend);

                    if (loadmaster.Status1 == "C")
                    {
                        loadmaster.Status1 = "U";
                    }

                    int sumQty = PickTaskDetailList.Sum(u => u.Qty);
                    loadmaster.SumQty = loadmaster.SumQty + sumQty;
                    idal.ILoadMasterDAL.UpdateBy(loadmaster, u => u.Id == loadmaster.Id, new string[] { "SumQty", "Status1" });

                    idal.SaveChanges();

                    int[] OutPoNumberIdList = (from a in outBoundOrderDetailListAdd
                                               select a.OutBoundOrderId).ToList().Distinct().ToArray();

                    List<OutBoundOrder> getOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutPoNumberIdList.Contains(u.Id)).ToList();

                    foreach (var item1 in OutPoNumberIdList)
                    {
                        OutBoundOrder getOutBoundOrder = getOutBoundOrderList.Where(u => u.Id == item1).First();
                        idal.IOutBoundOrderDetailDAL.UpdateByExtended(u => u.OutBoundOrderId == item1, t => new OutBoundOrderDetail { OutPoNumber = getOutBoundOrder.OutPoNumber });
                    }

                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                    return "新增异常，请重试！";
                }
            }
        }

        #endregion


        #region 客户装箱单数据处理

        public List<ExcelImportOutBound> CottonExcelImportOutBoundList(ExcelImportOutBoundSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IExcelImportOutBoundDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.ItemNumber))
                sql = sql.Where(u => u.ItemNumber == searchEntity.ItemNumber);

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"Qty\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public string ExcelImportOutBoundCotton(List<ExcelImportOutBound> entity)
        {
            ExcelImportOutBound first = entity.First();
            idal.IExcelImportOutBoundDAL.DeleteByExtended(u => u.WhCode == first.WhCode && u.ClientCode == first.ClientCode);
            idal.IExcelImportOutBoundDAL.Add(entity);
            idal.IExcelImportOutBoundDAL.SaveChanges();

            return "Y";
        }

        #endregion

    }
}
