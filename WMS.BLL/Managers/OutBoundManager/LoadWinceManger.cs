
using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class LoadWinceManger : ILoadWinceManger
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();

        #region CE装箱方法 

        #region  1.0验证Load是否存在,并且是否在备货中
        public bool CheckLoad(string WhCode, string LoadId)
        {
            return (from a in idal.ILoadMasterDAL.SelectAll()
                    where a.LoadId == LoadId && a.WhCode == WhCode && a.ShipDate == null && (a.Status1 == "A" || a.Status1 == "C")
                    select a.Id).Count() > 0;
        }

        //针对亚马逊业务,托盘TransactionFlag=1,需要验证托盘,其他客户都需要扫
        public bool CheckPltScan(string WhCode, string LoadId, string HuId)
        {
            var ListPltScan = from a in idal.IHuMasterDAL.SelectAll()
                              join b in idal.IHuDetailDAL.SelectAll() on new { A = a.HuId, B = a.WhCode } equals new { A = b.HuId, B = b.WhCode }
                              where a.LoadId == LoadId && a.WhCode == WhCode && a.HuId == HuId
                              // && a.TransactionFlag == 1 // && (b.ClientCode == "Amazon" || b.ClientCode == "AMZ")
                              select new { a.TransactionFlag, b.ClientCode };
            foreach (var item in ListPltScan.Distinct())
            {
                if (item.ClientCode == "Amazon" || item.ClientCode == "AMZ")
                {

                    if (item.TransactionFlag == 1)
                        return true;
                    else
                    {
                        //抛转入库扫描资料
                        if (AmazonToSerialNumberOut(WhCode, LoadId, HuId) == "Y")
                            return false;
                        else
                            return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            //没进循环的都是 true
            return true;

            //return (from a in idal.IHuMasterDAL.SelectAll()
            //        where a.LoadId == LoadId && a.WhCode == WhCode && a.HuId == HuId && a.TransactionFlag == 1 
            //        select a.Id).Count() > 0;
            //针对亚马逊业务,托盘货输入Attribute1,不扫描的验证
            //return (from a in idal.IPickTaskDetailDAL.SelectAll()
            //        join b in idal.IHuDetailDAL.SelectAll() on new {a.HuId,a.WhCode } equals  new { b.HuId,b.WhCode  }
            //        where a.LoadId == LoadId && a.WhCode == WhCode && a.HuId == HuId && (b.Attribute1 ==null || b.Attribute1=="")
            //        select a.Id).Count() > 0;
        }
        public string AmazonToSerialNumberOut(string WhCode, string LoadId, string HuId)
        {
            ////需要抛转扫描表数据
            //var sql = from a in idal.ILoadMasterDAL.SelectAll()
            //          join c in idal.IFlowHeadDAL.SelectAll() on a.ProcessId equals c.Id
            //          where a.LoadId == LoadId && a.WhCode == WhCode && c.OutSerialNumberCheckIn == "Y"
            //          select c.OutSerialNumberCheckIn;
            string Message = "";
            //if (sql.Count() > 0)
            //{
            SqlParameter[] prams = new SqlParameter[3];
            prams[0] = new SqlParameter("@WhCode", WhCode);
            prams[1] = new SqlParameter("@LoadId", LoadId);
            prams[2] = new SqlParameter("@HuId", HuId);
            string sqlCheck = @"-- declare @LoadId varchar(40),@WhCode varchar(30),@HuId varchar(48)
                                insert into SerialNumberOut (WhCode,LoadId,ClientId,ClientCode,SoNumber,CustomerPoNumber,AltItemNumber,ItemId,CartonId,HuId,Length,Width,Height,Weight,CreateUser,CreateDate)
                                select a.WhCode,b.LoadId,a.ClientId,a.ClientCode,a.SoNumber,a.CustomerPoNumber,a.AltItemNumber,a.ItemId,a.CartonId,a.HuId,a.Length,a.Width,a.Height,a.Weight,'SystemWms',getdate()
                                from SerialNumberIn a
                                INNER JOIN  PickTaskDetail b on a.WhCode = b.WhCode AND a.SoNumber = b.SoNumber  AND a.CustomerPoNumber = b.CustomerPoNumber AND a.ItemId = b.ItemId and a.HuId=b.HuId
                                where  a.WhCode = @WhCode and b.LoadId=@LoadId and b.HuId=@HuId
                                AND NOT EXISTS(SELECT 1 from SerialNumberOut b where a.WhCode = b.WhCode
                                and a.ClientCode = b.ClientCode and a.SoNumber = b.SoNumber AND a.CustomerPoNumber = b.CustomerPoNumber
                                AND a.ItemId = b.ItemId  and a.HuId=b.HuId)
                                SELECT 'Y'";
            try
            {

                return Message = idal.ExecSqlToString(sqlCheck, prams);
            }

            catch (Exception ex)
            {

                return ex.ToString();
            }

            //}
            //return Message;


        }



        //抛转Out扫描数据到R_SerialNumberInOut
        public string ToSerialNumberOut(string WhCode, string LoadId)
        {

            //需要抛转扫描表数据
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join c in idal.IFlowHeadDAL.SelectAll() on a.ProcessId equals c.Id
                      where a.LoadId == LoadId && a.WhCode == WhCode && c.OutSerialNumberCheckIn == "Y"
                      select c.OutSerialNumberCheckIn;
            string Message = "";
            if (sql.Count() > 0)
            {
                SqlParameter[] prams = new SqlParameter[2];
                prams[0] = new SqlParameter("@WhCode", WhCode);
                prams[1] = new SqlParameter("@LoadId", LoadId);

                string sqlCheck = @"-- declare @LoadId varchar(30),@WhCode varchar(30)
                INSERT INTO R_SerialNumberInOut(WhCode, ClientCode, SoNumber, CustomerPoNumber, AltItemNumber, ItemId, CartonId,HuId,CreateDate)
                SELECT DISTINCT a.WhCode,a.ClientCode,a.SoNumber,a.CustomerPoNumber,a.AltItemNumber,a.ItemId,a.CartonId,a.HuId,getdate() FROM SerialNumberIn a
                INNER JOIN  PickTaskDetail b on a.WhCode = b.WhCode AND a.SoNumber = b.SoNumber  AND a.CustomerPoNumber = b.CustomerPoNumber AND a.ItemId = b.ItemId
                 where a.ToOutStatus = 1
                 AND b.LoadId = @LoadId AND b.WhCode = @WhCode
                 AND NOT EXISTS(SELECT 1 from R_SerialNumberInOut b where a.WhCode = b.WhCode
                 and a.ClientCode = b.ClientCode and a.SoNumber = b.SoNumber AND a.CustomerPoNumber = b.CustomerPoNumber
                  AND a.ItemId = b.ItemId AND a.CartonId=b.CartonId )

                UPDATE a SET a.ToOutStatus = 0
                FROM SerialNumberIn a
                INNER  JOIN PickTaskDetail b on a.WhCode = b.WhCode AND a.SoNumber = b.SoNumber  AND a.CustomerPoNumber = b.CustomerPoNumber AND a.ItemId = b.ItemId
                 where a.ToOutStatus = 1
                 AND b.LoadId = @LoadId AND b.WhCode = @WhCode
                 SELECT 'Y'";
                try
                {

                    Message = idal.ExecSqlToString(sqlCheck, prams);
                }

                catch (Exception ex)
                {

                    return ex.ToString();
                }

            }
            //不check入库采集时候直接返回Y
            return "Y";
        }

        public List<string> GetSerialNumber(int HuDetailId)
        {

            return (from a in idal.IHuDetailDAL.SelectAll()
                    join b in idal.IR_SerialNumberInOutDAL.SelectAll() on new { A = a.WhCode, B = a.ClientCode, C = a.ItemId, D = a.CustomerPoNumber, E = a.SoNumber, F = a.HuId } equals
                    new { A = b.WhCode, B = b.ClientCode, C = (int)b.ItemId, D = b.CustomerPoNumber, E = b.SoNumber, F = b.HuId }
                    where a.Id == HuDetailId
                    select b.CartonId).ToList();
        }

        #endregion

        #region  2.0获取装箱的基本信息  ShipDate等于null(未封箱的)
        public ShipLoadDesModel GetShipLoadDesModel(string WhCode, string LoadId)
        {
            ShipLoadDesModel shipLoadDes = GetShipLoadDesHead(WhCode, LoadId);
            if (shipLoadDes != null)
            {
                //获取load号
                string loadId = shipLoadDes.LoadId;
                List<PickTaskDetail> sugPlt = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == WhCode && u.LoadId == loadId);
                if (sugPlt.Count() > 0)
                {
                    ////备货完成的明细(状态为C),装箱状态为U 未装箱 建议托盘号 按照HuId名字排序
                    //if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "U").OrderBy(u => u.HuId).Count() > 0)
                    //    shipLoadDes.SugPlt = sugPlt.Where(u => u.Status == "C" && u.Status1 == "U").OrderBy(u => u.HuId).First().HuId;
                    //else
                    //    shipLoadDes.SugPlt = "暂无";
                    //装箱总数量
                    shipLoadDes.LoadTotalQty = sugPlt.Sum(u => u.Qty);
                    shipLoadDes.LoadCBM = (int)sugPlt.Sum(u => u.Length * u.Height * u.Width * u.Qty);
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Count() > 0)
                        shipLoadDes.LoadQty = sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Sum(u => u.Qty);
                    else
                        shipLoadDes.LoadQty = 0;
                }
                ////装箱完成后 HuDetail中的HuId变成LoadId
                //List<HuDetail> huDetail = idal.IHuDetailDAL.SelectBy(u => u.HuId == loadId && u.WhCode == WhCode);
                //if (huDetail.Count() > 0)
                //    shipLoadDes.LoadQty = huDetail.Sum(u => u.Qty);
                //else
                //    shipLoadDes.LoadQty = 0;


            }
            return shipLoadDes;
        }

        public ShipLoadDesModel GetShipLoadDesHead(string WhCode, string LoadId)
        {
            ShipLoadDesModel shipLoadDes = new ShipLoadDesModel();
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join b in idal.ILoadContainerExtendDAL.SelectAll()
                      on new { A = a.LoadId, B = a.WhCode } equals new { A = b.LoadId, B = b.WhCode }
                      into temp1
                      from ab in temp1.DefaultIfEmpty()
                      join c in idal.ILoadContainerTypeDAL.SelectAll() on ab.ContainerType equals c.ContainerType
                      into temp2
                      from bc in temp2.DefaultIfEmpty()
                      where a.LoadId == LoadId && a.WhCode == WhCode && a.ShipDate == null
                      select new ShipLoadDesModel
                      {
                          LoadId = a.LoadId,
                          WhCode = a.WhCode,
                          ProcessId = a.ProcessId,
                          ContainerNumber = ab.ContainerNumber,
                          ContainerType = ab.ContainerType,
                          SealNumber = ab.SealNumber,
                          ContainerName = bc.ContainerName
                      };
            if (sql.Count() == 1)
            {
                return sql.First();
            }
            else
                return null;
        }

        #endregion

        public string LoadSugPlt(string WhCode, string LoadId)
        {
            string SugPltStr = "暂无";
            //备货完成的明细(状态为C),装箱状态为U 未装箱 建议托盘号 按照备货顺序排序
            List<PickTaskDetail> sugPlt = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == WhCode && u.LoadId == LoadId).ToList();
            if (sugPlt.Count() > 0)
            {
                List<PickTaskDetail> checksugPlt = sugPlt.Where(u => u.Status == "C" && u.Status1 == "U").OrderBy(u => u.Sequence).ToList();
                if (checksugPlt.Count() == 0)
                {
                    //装箱总数量
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Count() > 0)
                        SugPltStr += "$" + sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Sum(u => u.Qty);
                    else
                        SugPltStr += "$0";
                    //添加AGV装箱位置PickLocationDes
                    SugPltStr += "$";
                    //已装箱立方
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Count() > 0)
                        SugPltStr += "$" + ((int)sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Sum(u => u.Length * u.Height * u.Width * u.Qty)).ToString();
                    else
                        SugPltStr += "$0";
                }
                else
                {
                    PickTaskDetail sugPltDo = sugPlt.Where(u => u.Status == "C" && u.Status1 == "U").OrderBy(u => u.Sequence).First();
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "U").OrderBy(u => u.Sequence).Count() > 0)
                        SugPltStr = sugPltDo.HuId;
                    //装箱总数量
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Count() > 0)
                        SugPltStr += "$" + sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Sum(u => u.Qty);
                    else
                        SugPltStr += "$0";
                    //添加AGV装箱位置PickLocationDes
                    SugPltStr += "$" + (sugPltDo.PickLocationDes == null ? "" : sugPltDo.PickLocationDes);
                    //已装箱立方
                    if (sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Count() > 0)
                        SugPltStr += "$" + ((int)sugPlt.Where(u => u.Status == "C" && u.Status1 == "C").Sum(u => u.Length * u.Height * u.Width * u.Qty)).ToString();
                    else
                        SugPltStr += "$0";
                }       
            }
            return SugPltStr;
        }



        #region  3.0验证装箱托盘信息
        public string CheckPltLoad(string WhCode, string LoadId, string HuId)
        {
            string res = "";
            List<PickTaskDetail> pList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == WhCode && u.LoadId == LoadId && u.HuId == HuId);
            if (pList.Count() <= 0)
                res = "托盘不在备货任务中";
            else
            {
                //备货状态不是已备货
                if (pList.Where(u => u.Status == "C").Count() <= 0)
                    res = "托盘未备货";
                else
                    res = "Y";
            }
            if (res == "Y")
            {
                //库存已经不存在说明已经装箱了
                if (idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).Count() == 0)
                    res = "托盘已经装箱";
            }
            return res;
        }
        #endregion

        #endregion

        #region 4.0 装箱完成方法
        public string LoadComplete(LoadPlt loadPlt)
        {
            //string s = loadPlt.LoadId;
            //return "Y";
            ShipLoadManager shipLoadManager = new ShipLoadManager();
            return shipLoadManager.PackingLoad(loadPlt.LoadId, loadPlt.WhCode, loadPlt.HuId, loadPlt.Position, loadPlt.UserName, loadPlt.WorkloadAccountModel, loadPlt.HuDetailRemained);
        }
        #endregion

        #region 5.0 装箱完成
        public bool CheckLoadIfComplete(string WhCode, string LoadId)
        {
            return idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == WhCode && u.LoadId == LoadId && u.Status1 == "U").Count() == 0;
        }
        #endregion


    }
}
