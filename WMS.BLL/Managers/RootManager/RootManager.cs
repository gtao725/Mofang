using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;
using System.Collections;
using System.Transactions;
using WMS.DI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Web;





namespace WMS.BLL
{
    public class RootManager : IRootManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();
        private static object o1 = new object();
        private static object o2 = new object();

        #region 1.代理管理

        //代理列表
        //对应AgentController中的 List 方法
        public List<WhAgent> WhAgentList(WhAgentSearch searchEntity, out int total)
        {
            var sql = idal.IWhAgentDAL.SelectAll();

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.AgentName))
                sql = sql.Where(u => u.AgentName.Contains(searchEntity.AgentName));
            if (!string.IsNullOrEmpty(searchEntity.AgentCode))
                sql = sql.Where(u => u.AgentCode.Contains(searchEntity.AgentCode));
            if (!string.IsNullOrEmpty(searchEntity.AgentType))
                sql = sql.Where(u => u.AgentType.Contains(searchEntity.AgentType));

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增代理
        //对应AgentController中的 AddAgent 方法
        public WhAgent WhAgentAdd(WhAgent entity)
        {
            if (WhAgentCheck(entity) > 0)
            {
                idal.IWhAgentDAL.Add(entity);
                idal.IWhAgentDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //验证代理是否存在
        public int WhAgentCheck(WhAgent entity)
        {
            if (idal.IWhAgentDAL.SelectBy(u => u.AgentCode == entity.AgentCode && u.WhCode == entity.WhCode && u.Id != entity.Id).Count() == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //代理信息修改
        //对应AgentController中的 WhAgentEdit 方法
        public int WhAgentEdit(WhAgent entity, params string[] modifiedProNames)
        {
            if (WhAgentCheck(entity) > 0)
            {
                idal.IWhAgentDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
                idal.IWhAgentDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region 2.客户管理



        //客户列表
        public List<WhClientResult> WhClientList(WhClientSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IZoneDAL.SelectAll()
                      on new { A = (int)a.ZoneId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode } into temp1
                      from ab in temp1.DefaultIfEmpty()
                      select new WhClientResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          ClientCode = a.ClientCode,
                          ClientName = a.ClientName,
                          WarnCBM = a.WarnCBM,
                          Status = a.Status,
                          ZoneId = a.ZoneId,
                          ZoneName = ab.ZoneName,
                          ClientType = a.ClientType,
                          NightTime = a.NightTime,
                          ContractName = a.ContractName,
                          ContractNameOut = a.ContractNameOut,
                          Passageway = a.Passageway
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientName))
                sql = sql.Where(u => u.ClientName.Contains(searchEntity.ClientName));
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode.Contains(searchEntity.ClientCode));


            total = sql.Count();

            if (!string.IsNullOrEmpty(searchEntity.ClientCodeOrderBy))
            {
                if (searchEntity.ClientCodeOrderBy.ToLower() == "asc" || searchEntity.ClientCodeOrderBy.ToUpper() == "ASC")
                {
                    sql = sql.OrderBy(u => u.ClientCode);
                }
                else if (searchEntity.ClientCodeOrderBy.ToLower() == "desc" || searchEntity.ClientCodeOrderBy.ToUpper() == "DESC")
                {
                    sql = sql.OrderByDescending(u => u.ClientCode);
                }
            }
            else
                sql = sql.OrderByDescending(u => u.Id);

            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //客户释放规则列表
        public List<WhClientResult> WhClientReleaseRuleList(WhClientSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      join b in idal.ILookUpDAL.SelectAll()
                      on a.ReleaseRule.ToString() equals b.ColumnKey
                      where a.WhCode == searchEntity.WhCode && b.TableName == "WhClient" && b.ColumnName == "ReleaseRule"
                      select new WhClientResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          ReleaseRule = a.ReleaseRule,
                          ReleaseRuleShow = b.Description
                      };

            if (searchEntity.ClientId > 0)
                sql = sql.Where(u => u.Id == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);

            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增客户
        public WhClient WhClientAdd(WhClient entity)
        {
            if (WhClientCheck(entity) > 0)
            {
                idal.IWhClientDAL.Add(entity);
                idal.IWhClientDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //验证客户是否存在
        public int WhClientCheck(WhClient entity)
        {
            if (idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode && u.Id != entity.Id).Count() == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //客户信息修改
        public int WhClientEdit(WhClient entity, params string[] modifiedProNames)
        {
            if (WhClientCheck(entity) > 0)
            {
                idal.IWhClientDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
                idal.IWhClientDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //根据当前客户查询出未选择的货代
        public List<WhAgentResult> WhAgentUnselected(WhAgentSearch searchEntity, out int total)
        {
            var sql1 = from b in idal.IR_WhClient_WhAgentDAL.SelectAll()
                       where b.ClientId == searchEntity.ClientId
                       select b.AgentId;

            var sql = from a in idal.IWhAgentDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.WhCode == searchEntity.WhCode
                      select new WhAgentResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          AgentName = a.AgentName,
                          AgentCode = a.AgentCode,
                          AgentType = a.AgentType
                      };

            if (!string.IsNullOrEmpty(searchEntity.AgentName) && searchEntity.AgentName != "null")
            {
                sql = sql.Where(u => u.AgentName.Contains(searchEntity.AgentName));
            }
            if (!string.IsNullOrEmpty(searchEntity.AgentCode) && searchEntity.AgentCode != "null")
            {
                sql = sql.Where(u => u.AgentCode.Contains(searchEntity.AgentCode));
            }
            if (!string.IsNullOrEmpty(searchEntity.AgentType) && searchEntity.AgentType != "null")
            {
                sql = sql.Where(u => u.AgentType.Contains(searchEntity.AgentType));
            }
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据当前客户查询出已选择的货代
        public List<WhAgentResult> WhAgentSelected(WhAgentSearch searchEntity, out int total)
        {
            var sql1 = from b in idal.IR_WhClient_WhAgentDAL.SelectAll()
                       where b.ClientId == searchEntity.ClientId
                       select b.AgentId;

            var sql = from a in idal.IWhAgentDAL.SelectAll()
                      where sql1.Contains(a.Id) && a.WhCode == searchEntity.WhCode
                      select new WhAgentResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          Action = "",
                          AgentName = a.AgentName,
                          AgentCode = a.AgentCode,
                          AgentType = a.AgentType
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量添加客户代理关系
        public int WhAgentWhClientListAdd(List<R_WhClient_WhAgent> entity)
        {
            foreach (var item in entity)
            {
                idal.IR_WhClient_WhAgentDAL.Add(item);
            }
            idal.IR_WhClient_WhAgentDAL.SaveChanges();
            return 1;
        }

        //删除客户的某个代理
        public int WhAgentWhClientDel(R_WhClient_WhAgent entity)
        {
            idal.IR_WhClient_WhAgentDAL.DeleteBy(u => u.ClientId == entity.ClientId && u.AgentId == entity.AgentId);
            idal.IR_WhClient_WhAgentDAL.SaveChanges();
            return 1;
        }


        //客户异常原因列表
        public List<HoldMaster> HoldMasterListByClient(HoldMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IHoldMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ClientId == searchEntity.ClientId
                      select a;

            total = sql.Count();
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增客户异常原因
        public HoldMaster HoldMasterAdd(HoldMaster entity)
        {
            if (entity.ClientId == 0)
            {
                entity.Sequence = 9999;
            }
            else
            {
                int count = 1;

                List<HoldMaster> checkSequence = idal.IHoldMasterDAL.SelectBy(u => u.ClientId == entity.ClientId && u.Sequence > 0);
                if (checkSequence.Count > 0)
                {
                    count = (Int32)checkSequence.Max(u => u.Sequence) + 1;
                    entity.Sequence = count;
                }
                else
                {
                    entity.Sequence = count;
                }
            }

            idal.IHoldMasterDAL.Add(entity);
            idal.IHoldMasterDAL.SaveChanges();
            return entity;
        }

        //客户异常原因修改
        public int HoldMasterEdit(HoldMaster entity, params string[] modifiedProNames)
        {
            idal.IHoldMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
            idal.IHoldMasterDAL.SaveChanges();
            return 1;
        }

        //根据当前客户查询出未选择的流程
        public List<FlowHeadResult> FlowNameUnselected(FlowHeadSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.IWhClientDAL.SelectAll()
                       where a.WhCode == searchEntity.WhCode
                       join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                        on new { a.WhCode, a.Id } equals new { b.WhCode, Id = b.ClientId } into b_join
                       from b in b_join.DefaultIfEmpty()
                       where a.Id == searchEntity.ClientId && b.Type == searchEntity.Type
                       select b.BusinessFlowGroupId;

            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.WhCode == searchEntity.WhCode && a.Type == searchEntity.Type
                      select new FlowHeadResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName,
                          Remark = a.Remark
                      };

            if (!string.IsNullOrEmpty(searchEntity.FlowName) && searchEntity.FlowName != "null")
            {
                sql = sql.Where(u => u.FlowName == searchEntity.FlowName);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据当前客户查询出已选择的流程
        public List<FlowHeadResult> FlowNameSelected(FlowHeadSearch searchEntity, out int total)
        {
            var sql = from a in idal.IR_Client_FlowRuleDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.Type == searchEntity.Type && a.ClientId == searchEntity.ClientId
                      join b in idal.IFlowHeadDAL.SelectAll()
                      on new { BusinessFlowGroupId = (Int32)a.BusinessFlowGroupId } equals new { BusinessFlowGroupId = b.Id } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new FlowHeadResult
                      {
                          Id = a.Id,
                          FlowName = b.FlowName,
                          Remark = b.Remark
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //新增客户流程关系
        public int AddFlowRule(List<R_Client_FlowRule> entity)
        {
            foreach (var item in entity)
            {
                idal.IR_Client_FlowRuleDAL.Add(item);
            }
            idal.IR_Client_FlowRuleDAL.SaveChanges();
            return 1;
        }

        public List<int> SelectNextZone(List<int> ZoneId, int ClientZoneId)
        {

            List<int> NextIdl = new List<int>();
            List<Zone> zL = new List<Zone>();

            if (ZoneId.Count > 0)
            {
                zL = idal.IZoneDAL.SelectBy(u => ZoneId.Contains(u.UpId));
            }
            else
            {
                zL = idal.IZoneDAL.SelectBy(u => u.UpId == ClientZoneId);
            }


            foreach (var item in zL)
            {
                NextIdl.Add(item.Id);
            }

            return NextIdl;
        }

        public int CheckClientZone(string HuId, string Loaction, string WhCode)
        {

            int? ClientZoneId;
            int? ClientZoneFlag = 0;
            int? LociontZoneId = 0;
            int ClientId = 0;

            List<int> ZoneL = new List<int>();
            List<HuDetail> hdL = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);
            if (hdL.Count > 0)
            {
                ClientId = (int)hdL.First().ClientId;
            }
            else
            {
                return 1;
            }

            List<WhClient> wcL = idal.IWhClientDAL.SelectBy(u => u.Id == ClientId);
            List<WhLocation> wlL = idal.IWhLocationDAL.SelectBy(u => u.LocationId == Loaction && u.WhCode == WhCode);
            if (wlL.Count > 0)
            {
                LociontZoneId = wlL.First().ZoneId;
            }



            //判断客户存在
            if (wcL.Count > 0)
            {
                ClientZoneId = wcL.First().ZoneId;
                if (ClientZoneId == null)
                {
                    return 1;
                }





                ClientZoneFlag = wcL.First().ZoneFlag;
                //判断该客户是否需要区域管理
                if (ClientZoneFlag != 1)
                {
                    return 1;

                }
                else
                {
                    //如果库位区域未维护直接返回失败
                    if (LociontZoneId == null)
                    {
                        return 0;
                    }
                    ZoneL.Add((int)ClientZoneId);
                }

                int ZoneEnd = 0;
                List<int> NextIdl = new List<int>();
                //循环获取下级区域ID插入区域集合中
                while (ZoneEnd == 0)
                {
                    NextIdl = SelectNextZone(NextIdl, (int)ClientZoneId);

                    if (NextIdl.Count > 0)
                    {
                        ZoneL.AddRange(NextIdl);
                    }
                    else
                    {
                        ZoneEnd = 1;
                    }
                }
                //验证库位区域ID是否包含在客户区域下
                if (ZoneL.Contains((int)LociontZoneId))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }

            else
            {
                return 0;
            }

            return 1;
        }


        #endregion


        #region 3.储位管理

        //客户列表
        public List<WhLocationResult> WhLocationList(WhLocationSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhLocationDAL.SelectAll()
                      join b in idal.ILocationTypeDAL.SelectAll()
                      on a.LocationTypeId equals b.Id
                      join c in idal.IZoneDAL.SelectAll()
                      on a.ZoneId equals c.Id into tempc
                      from ac in tempc.DefaultIfEmpty()
                      select new WhLocationResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          LocationId = a.LocationId,
                          Location = a.Location,
                          MaxPltQty = a.MaxPltQty,
                          Status = a.Status,
                          ZoneId = a.ZoneId,
                          ZoneName = ac.ZoneName,
                          LocationTypeId = a.LocationTypeId,
                          LocationDescription = b.Description,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId.Contains(searchEntity.LocationId));
            if (!string.IsNullOrEmpty(searchEntity.LocationTypeId))
            {
                int LocationTypeId = Convert.ToInt32(searchEntity.LocationTypeId);
                sql = sql.Where(u => u.LocationTypeId == LocationTypeId);
            }
            if (searchEntity.ZoneId > 0)
            {
                sql = sql.Where(u => u.ZoneId == searchEntity.ZoneId);
            }
            if (!string.IsNullOrEmpty(searchEntity.Location))
                sql = sql.Where(u => u.Location == searchEntity.Location);

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //批量导入储位
        public string LocationImports(List<LocationResult> entity)
        {
            string result = "";
            string[] locationArr = (from a in entity
                                    select a.LocationId).ToList().Distinct().ToArray();
            string whCode = entity.First().WhCode;
            idal.IWhLocationDAL.DeleteByExtended(u => u.WhCode == whCode && locationArr.Contains(u.LocationId));

            string[] zoneArr = (from a in entity
                                where (a.ZoneName ?? "") != ""
                                select a.ZoneName).ToList().Distinct().ToArray();

            List<Zone> getZoneList = idal.IZoneDAL.SelectBy(u => u.WhCode == whCode && zoneArr.Contains(u.ZoneName));

            if (zoneArr.Count() != getZoneList.Count)
            {
                foreach (var item in zoneArr)
                {
                    if (getZoneList.Where(u => u.ZoneName == item).Count() == 0)
                    {
                        result += "区域:" + item + "不存在！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            List<WhLocation> list = new List<WhLocation>();
            foreach (var item in entity)
            {
                WhLocation location = new WhLocation();
                location.Status = "A";
                location.WhCode = item.WhCode;
                location.LocationId = item.LocationId;
                if (string.IsNullOrEmpty(item.ZoneName))
                {
                    location.ZoneId = 0;
                }
                else
                {
                    location.ZoneId = getZoneList.Where(u => u.ZoneName == item.ZoneName).First().Id;
                }

                location.LocationTypeId = 1;
                location.Location = item.Location;
                location.LocationColumn = item.LocationColumn;
                location.LocationRow = item.LocationRow;
                location.LocationFloor = item.LocationFloor;
                location.LocationPcs = item.LocationPcs;
                location.MaxPltQty = item.MaxPltQty;
                location.CreateDate = DateTime.Now;
                location.CreateUser = item.CreateUser;
                list.Add(location);
            }

            idal.IWhLocationDAL.Add(list);
            idal.IWhLocationDAL.SaveChanges();
            return "Y";
        }


        //按照规则生成储位
        public int AddLocation(string beginLocationArray, string beginLocationColumn, string endLocationColumn, string beginLocationColumn2, string endLocationColumn2, int beginLocationRow, int endLocationRow, int LocationFloor, int LocationPcs, int CheckBegin, string whCode, string userName)
        {
            char[] beginLocationColumnArray = beginLocationColumn.ToCharArray();
            char[] endLocationColumnArray = endLocationColumn.ToCharArray();
            char[] beginLocationColumnArray2 = beginLocationColumn2.ToCharArray();
            char[] endLocationColumnArray2 = endLocationColumn2.ToCharArray();

            List<WhLocation> whLocationListAdd = new List<WhLocation>();
            if (CheckBegin != 0)
            {
                if (Convert.ToInt32(beginLocationColumnArray[0]) >= 65) //字母
                {
                    for (int i = Convert.ToInt32(beginLocationColumnArray[0]); i <= Convert.ToInt32(endLocationColumnArray[0]); i++)
                    {
                        for (int j = beginLocationRow; j <= endLocationRow; j++)
                        {
                            for (int k = 1; k <= LocationFloor; k++)
                            {
                                for (int l = 1; l <= LocationPcs; l++)
                                {
                                    WhLocation entity = new WhLocation();
                                    entity.WhCode = whCode;
                                    entity.Location = beginLocationArray;          //库区
                                    entity.LocationColumn = ((char)i).ToString() + beginLocationColumn2;   //通道

                                    string locRow = j.ToString(), locFloor = k.ToString(), locPcs = l.ToString();
                                    if (j < 10)
                                    {
                                        entity.LocationRow = j;
                                        locRow = "0" + j.ToString();
                                    }
                                    else
                                        entity.LocationRow = j;  //列数

                                    if (k < 10)
                                    {
                                        entity.LocationFloor = k;
                                        locFloor = "0" + k.ToString();
                                    }
                                    else
                                        entity.LocationFloor = k;  //层数

                                    if (l < 10)
                                    {
                                        entity.LocationPcs = l;
                                        locPcs = "0" + l.ToString();
                                    }
                                    else
                                        entity.LocationPcs = l;  //个数

                                    entity.LocationId = entity.Location + entity.LocationColumn + locRow + locFloor + locPcs;
                                    entity.Status = "A";
                                    entity.LocationTypeId = 1;
                                    entity.CreateUser = userName;
                                    entity.CreateDate = DateTime.Now;

                                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LocationId == entity.LocationId).Count == 0)
                                    {
                                        whLocationListAdd.Add(entity);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int begin = Convert.ToInt32(beginLocationColumn);
                    int end = Convert.ToInt32(endLocationColumn);

                    for (int i = begin; i <= end; i++)
                    {
                        for (int j = beginLocationRow; j <= endLocationRow; j++)
                        {
                            for (int k = 1; k <= LocationFloor; k++)
                            {
                                for (int l = 1; l <= LocationPcs; l++)
                                {
                                    WhLocation entity = new WhLocation();
                                    entity.WhCode = whCode;
                                    entity.Location = beginLocationArray;          //库区
                                    if (i < 10)
                                        entity.LocationColumn = "0" + i.ToString() + beginLocationColumn2;
                                    else
                                        entity.LocationColumn = i.ToString() + beginLocationColumn2;   //通道

                                    string locRow = j.ToString(), locFloor = k.ToString(), locPcs = l.ToString();
                                    if (j < 10)
                                    {
                                        entity.LocationRow = j;
                                        locRow = "0" + j.ToString();
                                    }
                                    else
                                        entity.LocationRow = j;  //列数

                                    if (k < 10)
                                    {
                                        entity.LocationFloor = k;
                                        locFloor = "0" + k.ToString();
                                    }
                                    else
                                        entity.LocationFloor = k;  //层数

                                    if (l < 10)
                                    {
                                        entity.LocationPcs = l;
                                        locPcs = "0" + l.ToString();
                                    }
                                    else
                                        entity.LocationPcs = l;  //个数

                                    entity.LocationId = entity.Location + entity.LocationColumn + locRow + locFloor + locPcs;
                                    entity.Status = "A";
                                    entity.LocationTypeId = 1;
                                    entity.CreateUser = userName;
                                    entity.CreateDate = DateTime.Now;

                                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LocationId == entity.LocationId).Count == 0)
                                    {
                                        whLocationListAdd.Add(entity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (Convert.ToInt32(beginLocationColumnArray2[0]) >= 65) //字母
                {
                    for (int i = Convert.ToInt32(beginLocationColumnArray2[0]); i <= Convert.ToInt32(endLocationColumnArray2[0]); i++)
                    {
                        for (int j = beginLocationRow; j <= endLocationRow; j++)
                        {
                            for (int k = 1; k <= LocationFloor; k++)
                            {
                                for (int l = 1; l <= LocationPcs; l++)
                                {
                                    WhLocation entity = new WhLocation();
                                    entity.WhCode = whCode;
                                    entity.Location = beginLocationArray;          //库区
                                    entity.LocationColumn = beginLocationColumn + ((char)i).ToString();   //通道
                                    string locRow = j.ToString(), locFloor = k.ToString(), locPcs = l.ToString();
                                    if (j < 10)
                                    {
                                        entity.LocationRow = j;
                                        locRow = "0" + j.ToString();
                                    }
                                    else
                                        entity.LocationRow = j;  //列数

                                    if (k < 10)
                                    {
                                        entity.LocationFloor = k;
                                        locFloor = "0" + k.ToString();
                                    }
                                    else
                                        entity.LocationFloor = k;  //层数

                                    if (l < 10)
                                    {
                                        entity.LocationPcs = l;
                                        locPcs = "0" + l.ToString();
                                    }
                                    else
                                        entity.LocationPcs = l;  //个数

                                    entity.LocationId = entity.Location + entity.LocationColumn + locRow + locFloor + locPcs;
                                    entity.Status = "A";
                                    entity.LocationTypeId = 1;
                                    entity.CreateUser = userName;
                                    entity.CreateDate = DateTime.Now;

                                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LocationId == entity.LocationId).Count == 0)
                                    {
                                        whLocationListAdd.Add(entity);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int begin = Convert.ToInt32(beginLocationColumn2);
                    int end = Convert.ToInt32(endLocationColumn2);

                    for (int i = begin; i <= end; i++)
                    {
                        for (int j = beginLocationRow; j <= endLocationRow; j++)
                        {
                            for (int k = 1; k <= LocationFloor; k++)
                            {
                                for (int l = 1; l <= LocationPcs; l++)
                                {
                                    WhLocation entity = new WhLocation();
                                    entity.WhCode = whCode;
                                    entity.Location = beginLocationArray;          //库区
                                    if (i < 10)
                                        entity.LocationColumn = beginLocationColumn + "0" + i.ToString();
                                    else
                                        entity.LocationColumn = beginLocationColumn + i.ToString();   //通道
                                    string locRow = j.ToString(), locFloor = k.ToString(), locPcs = l.ToString();
                                    if (j < 10)
                                    {
                                        entity.LocationRow = j;
                                        locRow = "0" + j.ToString();
                                    }
                                    else
                                        entity.LocationRow = j;  //列数

                                    if (k < 10)
                                    {
                                        entity.LocationFloor = k;
                                        locFloor = "0" + k.ToString();
                                    }
                                    else
                                        entity.LocationFloor = k;  //层数

                                    if (l < 10)
                                    {
                                        entity.LocationPcs = l;
                                        locPcs = "0" + l.ToString();
                                    }
                                    else
                                        entity.LocationPcs = l;  //个数

                                    entity.LocationId = entity.Location + entity.LocationColumn + locRow + locFloor + locPcs;
                                    entity.Status = "A";
                                    entity.LocationTypeId = 1;
                                    entity.CreateUser = userName;
                                    entity.CreateDate = DateTime.Now;

                                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LocationId == entity.LocationId).Count == 0)
                                    {
                                        whLocationListAdd.Add(entity);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            idal.IWhLocationDAL.Add(whLocationListAdd);
            idal.IWhLocationDAL.SaveChanges();
            return 1;
        }


        //储位类型下拉列表
        public IEnumerable<WhLocationTypeResult> LocationTypeSelect()
        {
            var sql = from a in idal.ILocationTypeDAL.SelectAll()
                      select new WhLocationTypeResult
                      {
                          Id = a.Id,
                          Description = a.Description
                      };
            return sql.AsEnumerable();
        }


        //区域类型下拉列表
        public IEnumerable<WhZoneResult> ZoneSelect(string whCode)
        {
            var sql = from a in idal.IZoneDAL.SelectAll()
                      where a.WhCode == whCode
                      select new WhZoneResult
                      {
                          Id = a.Id,
                          ZoneName = a.ZoneName
                      };
            return sql.AsEnumerable();
        }

        public IEnumerable<WhLocationResult> LocationSelect(string whCode)
        {
            var sql = (from a in idal.IWhLocationDAL.SelectAll()
                       where a.WhCode == whCode && (a.Location ?? "") != ""
                       select new WhLocationResult
                       {
                           Location = a.Location
                       }).Distinct();
            return sql.AsEnumerable();
        }

        //新增储位
        public WhLocation LocationAdd(WhLocation entity)
        {
            if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LocationId == entity.LocationId).Count == 0)
            {
                idal.IWhLocationDAL.Add(entity);
                idal.IWhLocationDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }


        //批量修改储位类型
        public string LocationEdit(List<WhLocation> list)
        {
            foreach (var item in list)
            {
                idal.IWhLocationDAL.UpdateByExtended(u => u.Id == item.Id, t => new WhLocation { LocationTypeId = item.LocationTypeId });
            }
            return "Y";
        }

        //批量删除库位
        public string WhLocationBatchDel(int?[] idarr)
        {
            List<WhLocation> list = idal.IWhLocationDAL.SelectBy(u => idarr.Contains(u.Id));

            if (list.Count() > 0)
            {
                string[] locationArr = (from a in list
                                        select a.LocationId).ToList().Distinct().ToArray();

                WhLocation first = list.First();

                int[] getIdArr = (from a in idal.IHuMasterDAL.SelectAll()
                                  join b in idal.IWhLocationDAL.SelectAll()
                                  on new { A = a.WhCode, B = a.Location } equals new { A = b.WhCode, B = b.LocationId }
                                  where idarr.Contains(b.Id)
                                  select b.Id).ToList().Distinct().ToArray();

                foreach (var item in idarr.Where(u => !getIdArr.Contains(u.Value)))
                {
                    idal.IWhLocationDAL.DeleteByExtended(u => u.Id == item);
                }

                List<WhLocation> list1 = idal.IWhLocationDAL.SelectBy(u => getIdArr.Contains(u.Id));
                if (list1.Count > 0)
                {
                    string result = "";
                    foreach (var item in list1)
                    {
                        result += item.Location + ",";
                    }
                    result = result.Substring(0, result.Length - 1);
                    return "以下库位放有货物删除失败：" + result;
                }
                else
                {
                    return "Y";
                }
            }
            else
            {
                return "Y";
            }
        }

        public int WhLocationEdit(WhLocation entity)
        {
            List<WhLocation> list = idal.IWhLocationDAL.SelectBy(u => u.Id != entity.Id && u.LocationId == entity.LocationId && u.WhCode == entity.WhCode);
            if (list.Count > 0)
            {
                idal.IWhLocationDAL.DeleteByExtended(u => u.Id != entity.Id && u.LocationId == entity.LocationId && u.WhCode == entity.WhCode);
            }

            idal.IWhLocationDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "LocationId", "ZoneId", "LocationTypeId", "Location" });
            idal.IWhLocationDAL.SaveChanges();
            return 1;
        }


        //取得默认异常库位
        public string GetWhLocationLookUp(string whCode)
        {
            string ablocation = "";
            List<LookUp> lookUpList = idal.ILookUpDAL.SelectBy(u => u.ColumnKey == whCode && u.TableName == "WhLocation");
            if (lookUpList.Count > 0)
            {
                ablocation = lookUpList.First().ColumnName;
            }

            return ablocation;
        }

        //异常库位默认设置
        public string SetWhLocationLookUp(string whCode, string abLocationId)
        {
            List<LookUp> lookUpList = idal.ILookUpDAL.SelectBy(u => u.ColumnKey == whCode && u.TableName == "WhLocation");

            if (!string.IsNullOrEmpty(abLocationId))
            {
                var sql7 = from a in idal.IWhLocationDAL.SelectAll()
                           join b in idal.ILocationTypeDAL.SelectAll() on new { LocationTypeId = a.LocationTypeId } equals new { LocationTypeId = b.Id }
                           where
                             b.TypeName == "AB" &&
                             a.WhCode == whCode && a.LocationId == abLocationId
                           select new
                           {
                               a.LocationId
                           };
                if (sql7.Count() == 0)
                {
                    return "没有该异常库位！";
                }
            }

            if (lookUpList.Count > 0)
            {
                LookUp look = lookUpList.First();
                look.ColumnName = abLocationId;
                idal.ILookUpDAL.UpdateBy(look, u => u.Id == look.Id, new string[] { "ColumnName" });
            }
            else
            {
                LookUp look = new LookUp();
                look.TableName = "WhLocation";
                look.ColumnName = abLocationId;
                look.ColumnKey = whCode;
                idal.ILookUpDAL.Add(look);
            }

            idal.IWhLocationDAL.SaveChanges();
            return "Y";
        }

        #endregion


        #region 4.托盘管理

        //托盘列表
        //对应PallateController中的 List 方法
        public List<WhPallateResult> WhPallateList(WhPallateSearch searchEntity, out int total)
        {
            var sql = from a in idal.IPallateDAL.SelectAll()
                      join b in idal.IPallateTypeDAL.SelectAll()
                      on a.TypeId equals b.Id into tempa
                      from ab in tempa.DefaultIfEmpty()
                      select new WhPallateResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          HuId = a.HuId,
                          TypeId = a.TypeId,
                          TypeName = ab.TypeName,
                          Status = a.Status,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId.StartsWith(searchEntity.HuId));
            if (!string.IsNullOrEmpty(searchEntity.TypeId))
            {
                int TypeId = Convert.ToInt32(searchEntity.TypeId);
                sql = sql.Where(u => u.TypeId == TypeId);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量导入托盘
        //对应PallateController中的 AddPallate 方法
        public int WhPallateListAdd(List<Pallate> entity)
        {
            List<string> PallateListAdd = (from a in entity
                                           join b in idal.IPallateDAL.SelectAll()
                                           on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                                           select a.HuId).ToList();

            entity = entity.Where(u => !PallateListAdd.Contains(u.HuId)).ToList();

            foreach (var item in entity)
            {
                item.Status = "U";
                item.CreateDate = DateTime.Now;
            }
            idal.IPallateDAL.Add(entity);
            idal.IPallateDAL.SaveChanges();
            return 1;

        }

        //批量导入托盘
        public int WhPallateListAddS(List<Pallate> entity)
        {
            //List<string> PallateListAdd = (from a in entity
            //                               join b in idal.IPallateDAL.SelectAll()
            //                               on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
            //                               select a.HuId).ToList();

            List<string> PallateListAdd = entity.Select(u => u.HuId).ToList();
            string WhCode = entity.First().WhCode;

            List<string> havePLTList = idal.IPallateDAL.SelectBy(u => PallateListAdd.Contains(u.HuId) && u.WhCode == WhCode).Select(u => u.HuId).ToList();


            entity = entity.Where(u => !havePLTList.Contains(u.HuId)).ToList();

            if (entity.Count() > 0)
            {

                foreach (var item in entity)
                {
                    item.Status = "U";
                    item.CreateDate = DateTime.Now;
                }
                idal.IPallateDAL.Add(entity);
                idal.IPallateDAL.SaveChanges();
            }

            return 1;

        }


        //托盘类型下拉列表
        public IEnumerable<WhPallateTypeResult> PallateTypeSelect()
        {
            var sql = from a in idal.IPallateTypeDAL.SelectAll()
                      select new WhPallateTypeResult
                      {
                          Id = a.Id,
                          Description = a.Description
                      };
            return sql.AsEnumerable();
        }


        public string PallateImports(List<WhPallateResult> entity)
        {
            WhPallateResult first = entity.First();
            string[] huArr = (from a in entity
                              select a.HuId).ToList().Distinct().ToArray();

            string[] getPallateArr = (from u in idal.IPallateDAL.SelectAll()
                                      where u.WhCode == first.WhCode && huArr.Contains(u.HuId)
                                      select u.HuId).ToArray();

            List<Pallate> list = new List<Pallate>();
            foreach (var item in entity.Where(u => !getPallateArr.Contains(u.HuId)))
            {
                Pallate pallate = new Pallate();
                pallate.WhCode = item.WhCode;
                pallate.HuId = item.HuId;
                pallate.TypeId = 1;
                pallate.Status = "U";
                pallate.CreateDate = DateTime.Now;
                pallate.CreateUser = item.CreateUser;
                list.Add(pallate);
            }

            idal.IPallateDAL.Add(list);
            idal.IPallateDAL.SaveChanges();
            return "Y";
        }

        public string PallateBatchDel(int?[] idarr)
        {
            List<Pallate> list = idal.IPallateDAL.SelectBy(u => idarr.Contains(u.Id));
            if (list.Count() > 0)
            {
                string[] huArr = (from a in list
                                  select a.HuId).ToList().Distinct().ToArray();

                Pallate first = list.First();

                int[] getIdArr = (from a in idal.IHuDetailDAL.SelectAll()
                                  join b in idal.IPallateDAL.SelectAll()
                                  on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                                  where idarr.Contains(b.Id)
                                  select b.Id).ToList().Distinct().ToArray();

                foreach (var item in idarr.Where(u => !getIdArr.Contains(u.Value)))
                {
                    idal.IPallateDAL.DeleteByExtended(u => u.Id == item);
                }

                List<Pallate> list1 = idal.IPallateDAL.SelectBy(u => getIdArr.Contains(u.Id));
                if (list1.Count > 0)
                {
                    string result = "";
                    foreach (var item in list1)
                    {
                        result += item.HuId + ",";
                    }
                    result = result.Substring(0, result.Length - 1);
                    return "以下托盘放有货物删除失败：" + result;
                }
                else
                {
                    return "Y";
                }
            }
            else
            {
                return "Y";
            }

        }


        #endregion


        #region 5.区域管理

        //区域列表
        public List<WhZoneResult> WhZoneList(WhZoneSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.IZoneDAL.SelectAll() where a.WhCode == searchEntity.WhCode select a;
            var sql = from a in sql1
                      join b in (
                          (from zones in sql1
                           select new
                           {
                               zones.Id,
                               zones.ZoneName
                           })) on new { UpId = a.UpId } equals new { UpId = b.Id } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new WhZoneResult
                      {
                          Id = a.Id,
                          ZoneName = a.ZoneName,
                          ZoneCBM = a.ZoneCBM,
                          Description = a.Description,
                          ParentId = a.UpId,
                          ParentZoneName = b.ZoneName,
                          RegShow = a.RegFlag == 0 ? "否" : "是",
                          RegFlag = a.RegFlag
                      };


            if (!string.IsNullOrEmpty(searchEntity.ZoneName))
                sql = sql.Where(u => u.ZoneName.Contains(searchEntity.ZoneName));
            if (searchEntity.UpId > 0)
                sql = sql.Where(u => u.ParentId == searchEntity.UpId);

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增区域
        public Zone WhZoneAdd(Zone entity)
        {
            if (idal.IZoneDAL.SelectBy(u => u.ZoneName == entity.ZoneName && u.WhCode == entity.WhCode && u.Id != entity.Id).Count() == 0)
            {
                idal.IZoneDAL.Add(entity);
                idal.IZoneDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }


        //根据当前区域查询出未选择的库位信息
        public List<ZoneLocationResult> LocationUnselected(ZoneLocationSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.IWhLocationDAL.SelectAll() where a.WhCode == searchEntity.WhCode select a;
            var sql2 = from a in idal.IZoneLocationDAL.SelectAll() where a.WhCode == searchEntity.WhCode select a;

            var sql = from locations in sql1
                      where
                        locations.LocationTypeId == 1 &&
                        !
                          (from zonelocation in sql2
                           where
       zonelocation.ZoneId == searchEntity.ZoneId
                           select new
                           {
                               zonelocation.Location
                           }).Contains(new { locations.Location })
                      group locations by new
                      {
                          locations.Location,
                          locations.WhCode
                      } into g
                      select new ZoneLocationResult
                      {
                          Location = g.Key.Location,
                          WhCode = g.Key.WhCode
                      };

            if (!string.IsNullOrEmpty(searchEntity.Location) && searchEntity.Location != "null")
            {
                sql = sql.Where(u => u.Location.Contains(searchEntity.Location));
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Location);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }

        //根据当前区域查询出已选择的库位信息
        public List<ZoneLocationResult> LocationSelected(ZoneLocationSearch searchEntity, out int total)
        {
            var sql = from a in idal.IZoneLocationDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ZoneId == searchEntity.ZoneId
                      select new ZoneLocationResult
                      {
                          Id = a.Id,
                          Location = a.Location,
                          WhCode = a.WhCode
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }


        //新增区域关系
        public int ZoneLocationAdd(List<ZoneLocation> entity)
        {
            List<ZoneLocation> ZoneLocationListAdd = new List<ZoneLocation>();
            foreach (var item in entity)
            {
                ZoneLocationListAdd.Add(item);
                idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.Location == item.Location, t => new WhLocation { ZoneId = item.ZoneId });
            }
            idal.IZoneLocationDAL.Add(ZoneLocationListAdd);
            idal.IZoneLocationDAL.SaveChanges();
            return 1;
        }

        //取消区域关系
        public int ZoneLocationDelById(int Id)
        {
            ZoneLocation first = idal.IZoneLocationDAL.SelectBy(u => u.Id == Id).First();
            idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == first.WhCode && u.Location == first.Location, t => new WhLocation { ZoneId = 0 });

            idal.IZoneLocationDAL.DeleteByExtended(u => u.Id == Id);
            return 1;
        }

        //区域父级菜单下拉列表
        public IEnumerable<WhZoneResult> WhZoneParentSelect(string whCode)
        {
            var sql = from a in idal.IZoneDAL.SelectAll()
                      where a.WhCode == whCode
                      select new WhZoneResult
                      {
                          Id = a.Id,
                          RegFlag = a.RegFlag,
                          ZoneName = a.ZoneName
                      };
            return sql.AsEnumerable();
        }

        //区域信息修改
        public int WhZoneParentEdit(Zone entity, params string[] modifiedProNames)
        {
            idal.IZoneDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
            idal.IZoneDAL.SaveChanges();
            return 1;
        }

        //批量删除区域
        public string WhZoneBatchDel(int?[] idarr)
        {
            List<Zone> list = idal.IZoneDAL.SelectBy(u => idarr.Contains(u.UpId));
            string result = "";
            if (list.Count() > 0)
            {
                int[] zoneId = (from a in list
                                select a.Id).ToList().Distinct().ToArray();

                List<Zone> list1 = idal.IZoneDAL.SelectBy(u => zoneId.Contains(u.Id));

                foreach (var item in list1)
                {
                    result += item.ZoneName + ",";
                }
                result = result.Substring(0, result.Length - 1);
                return "区域存在子区域，请先删除子区域：" + result;
            }


            List<WhLocation> localist = idal.IWhLocationDAL.SelectBy(u => idarr.Contains(u.ZoneId));
            if (localist.Count() > 0)
            {
                int?[] zoneId = (from a in localist
                                 select a.ZoneId).ToList().Distinct().ToArray();

                List<Zone> list1 = idal.IZoneDAL.SelectBy(u => zoneId.Contains(u.Id));

                foreach (var item in list1)
                {
                    result += item.ZoneName + ",";
                }
                result = result.Substring(0, result.Length - 1);
                return "区域存在储位信息，请先删除区域" + result + "下的所有储位！";
            }

            foreach (var item in idarr)
            {
                idal.IZoneDAL.DeleteByExtended(u => u.Id == item);
            }

            idal.SaveChanges();
            return "Y";
        }

        public string ZoneImports(List<ZoneResult> entity)
        {
            string result = "";
            ZoneResult first = entity.First();
            string[] upZoneNameArr = (from a in entity
                                      where (a.UpZoneName ?? "") != ""
                                      select a.UpZoneName).ToList().Distinct().ToArray();

            List<Zone> getZoneList = idal.IZoneDAL.SelectBy(u => u.WhCode == first.WhCode && upZoneNameArr.Contains(u.ZoneName) && u.UpId == 0);
            if (upZoneNameArr.Count() != getZoneList.Count)
            {
                foreach (var item in upZoneNameArr)
                {
                    if (getZoneList.Where(u => u.ZoneName == item).Count() == 0)
                    {
                        result += "父级区域:" + item + "不存在！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            List<Zone> list = new List<Zone>();
            foreach (var item in entity)
            {
                Zone zone = new Zone();
                zone.WhCode = item.WhCode;
                zone.ZoneName = item.ZoneName;
                zone.Description = item.Description;
                if (string.IsNullOrEmpty(item.UpZoneName))
                {
                    zone.UpId = 0;
                }
                else
                {
                    zone.UpId = getZoneList.Where(u => u.ZoneName == item.UpZoneName).First().Id;
                }

                zone.RegFlag = item.RegFlag == "是" ? 1 : 0;

                zone.ZoneCBM = item.ZoneCBM;
                zone.CreateUser = item.CreateUser;
                zone.CreateDate = DateTime.Now;
                list.Add(zone);
            }

            idal.IZoneDAL.Add(list);
            idal.IZoneDAL.SaveChanges();
            return "Y";
        }

        #endregion


        #region 6.款号管理

        //款号列表
        //对应ItemController中的 List 方法
        public List<WhItemResult> ItemMasterList(WhItemSearch searchEntity, out int total)
        {
            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      join b in idal.IItemMasterExtendOMDAL.SelectAll() on new { a.WhCode, ClientCode = a.ClientCode, AltItemNumber = a.AltItemNumber }
            equals new { b.WhCode, ClientCode = b.ClientCode, AltItemNumber = b.AltItemNumber }
                      into ItemInfo
                      from c in ItemInfo.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      select new WhItemResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          AltItemNumber = a.AltItemNumber,
                          Description = a.Description,
                          ItemName = a.ItemName,
                          Style1 = a.Style1,
                          Style2 = a.Style2,
                          Style3 = a.Style3,
                          EAN = a.EAN,
                          Category = c.Category,
                          UnitName = (c.UnitName ?? "") == "" ? a.UnitName : c.UnitName,
                          OriginCountry = c.OriginCountry,
                          Color = c.Color,
                          Material = c.Matieral,
                          Style = c.Style,
                          BoxCode = c.BoxCode,
                          CusItemNumber = c.CusItemNumber,
                          CusStyle = c.CusStyle,
                          PackageStyle = c.PackageStyle,
                          Pcs = c.Pcs,
                          Length = a.Length,
                          Weight = a.Weight,
                          Height = a.Height,
                          Width = a.Width,
                          HandFlag = a.HandFlag,
                          ScanFlag = a.ScanFlag,
                          HandFlagShow = a.HandFlag == 0 ? "否" :
                           a.HandFlag == 1 ? "是" : "否",
                          InstallService = a.InStallService,
                          ScanFlagShow = a.ScanFlag == 0 ? "否" :
                           a.ScanFlag == 1 ? "是" : "否",
                          ScanRule = a.ScanRule,
                          UnitFlag = a.UnitFlag,
                          LocFlag = a.LocFlag,
                          LocOnHandFlag = a.LocOnHandFlag,
                          OnHandFlag = a.OnHandFlag,
                          OneItemLPFlag = a.OneItemLPFlag,
                          OneItemSizeLPFlag = a.OneItemSizeLPFlag,
                          CreateDate = a.CreateDate,
                          CreateUser = a.CreateUser,
                          UpdateDate = a.UpdateDate,
                          UpdateUser = a.UpdateUser,
                          CartonName = a.CartonName
                      };

            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));
            if (searchEntity.ClientId != 0)
            {
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            }
            if (!string.IsNullOrEmpty(searchEntity.HandFlag))
            {
                sql = sql.Where(u => u.HandFlagShow == searchEntity.HandFlag);
            }
            if (!string.IsNullOrEmpty(searchEntity.ScanFlag))
            {
                sql = sql.Where(u => u.ScanFlagShow == searchEntity.ScanFlag);
            }

            if (!string.IsNullOrEmpty(searchEntity.UnitName))
            {
                sql = sql.Where(u => u.UnitName == searchEntity.UnitName);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量导入款号
        //对应ItemController中的 imports 方法
        public string ItemImports(string[] clientCode, string[] altItemNumber, string[] style1, string[] style2, string[] style3, string[] unitName, string whCode, string userName)
        {
            var s = (from a in clientCode
                     select a).Distinct();

            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.WhCode == whCode && s.Contains(a.ClientCode)
                      select a;

            string mess = "";
            if (sql.Count() != s.Count())
            {
                Hashtable sqlResult = new Hashtable();
                Hashtable listResult = new Hashtable();
                int count = 0;
                int count1 = 0;
                foreach (var item in s)
                {
                    listResult.Add(count, item);
                    count++;
                }
                foreach (var item in sql)
                {
                    sqlResult.Add(count1, item.ClientCode);
                    count1++;
                }

                for (int i = 0; i < listResult.Count; i++)
                {
                    if (mess == "")
                    {
                        if (sqlResult.ContainsValue(listResult[i]) == false)
                        {
                            mess = listResult[i].ToString();
                        }
                    }
                }

            }
            if (mess != "")
            {
                return "该客户不存在，请检查：" + mess;
            }

            List<ItemMaster> ItemMasterListAdd = new List<ItemMaster>();
            WhClient whClient = new WhClient();
            for (int j = 0; j < clientCode.Count(); j++)
            {
                string ClientCode = clientCode[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string AltItemNumber = altItemNumber[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string Style1 = style1[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string Style2 = style2[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string Style3 = style3[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string UnitName = (unitName[j] == "" ? "none" : unitName[j]);

                whClient = sql.Where(u => u.WhCode == whCode && u.ClientCode == ClientCode).First();

                if (idal.IItemMasterDAL.SelectBy(u => u.WhCode == whCode && u.AltItemNumber == AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (Style1 == null ? "" : Style1) && (u.Style2 == null ? "" : u.Style2) == (Style2 == null ? "" : Style2) && (u.Style3 == null ? "" : u.Style3) == (Style3 == null ? "" : Style3) && u.ClientId == whClient.Id).Count == 0)
                {
                    ItemMaster itemMaster = new ItemMaster();

                    itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                    itemMaster.WhCode = whCode;
                    itemMaster.AltItemNumber = AltItemNumber;
                    itemMaster.ClientId = whClient.Id;
                    itemMaster.ClientCode = whClient.ClientCode;
                    itemMaster.Style1 = Style1;
                    itemMaster.Style2 = Style2;
                    itemMaster.Style3 = Style3;
                    itemMaster.UnitFlag = 0;
                    itemMaster.UnitName = UnitName;
                    itemMaster.RgularId = 0;
                    itemMaster.CreateUser = userName;
                    itemMaster.CreateDate = DateTime.Now;
                    ItemMasterListAdd.Add(itemMaster);   //款号不存在就新增
                }
            }
            idal.IItemMasterDAL.Add(ItemMasterListAdd);
            idal.IItemMasterDAL.SaveChanges();
            return "";

        }

        //批量导入款号品名
        public string ItemImportsItemName(string[] clientCode, string[] altItemNumber, string[] itemName, string whCode, string userName)
        {
            var s = (from a in clientCode
                     select a).Distinct();

            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.WhCode == whCode && s.Contains(a.ClientCode)
                      select a;

            string mess = "";
            if (sql.Count() != s.Count())
            {
                Hashtable sqlResult = new Hashtable();
                Hashtable listResult = new Hashtable();
                int count = 0;
                int count1 = 0;
                foreach (var item in s)
                {
                    listResult.Add(count, item);
                    count++;
                }
                foreach (var item in sql)
                {
                    sqlResult.Add(count1, item.ClientCode);
                    count1++;
                }

                for (int i = 0; i < listResult.Count; i++)
                {
                    if (mess == "")
                    {
                        if (sqlResult.ContainsValue(listResult[i]) == false)
                        {
                            mess = listResult[i].ToString();
                        }
                    }
                }

            }
            if (mess != "")
            {
                return "该客户不存在，请检查：" + mess;
            }

            for (int j = 0; j < clientCode.Count(); j++)
            {
                string ClientCode = clientCode[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string AltItemNumber = altItemNumber[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                string ItemName = itemName[j].Replace(@"""", "").Replace(@"'", "");

                idal.IItemMasterDAL.UpdateByExtended(u => u.WhCode == whCode && u.ClientCode == ClientCode && u.AltItemNumber == AltItemNumber, u => new ItemMaster() { ItemName = ItemName, UpdateUser = userName, UpdateDate = DateTime.Now });
            }

            return "Y";
        }


        /// <summary>
        /// OMS新增或修改的款号信息验证
        /// </summary>
        /// <param name="entity">款号信息列表实体类</param>
        /// <returns></returns>
        public string ItemMasterExtendOMSAuthorize(List<WhItemExtendOMS> entity)
        {
            string mess = "";
            string res = "";

            WhItemExtendOMS firstEntity = entity.First();


            var s = (from a in entity
                     select a.ClientCode).Distinct();

            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.WhCode == firstEntity.WhCode && s.Contains(a.ClientCode)
                      select a;

            if (sql.Count() != s.Count())
            {
                Hashtable sqlResult = new Hashtable();
                Hashtable listResult = new Hashtable();
                int count = 0;
                int count1 = 0;
                foreach (var item in s)
                {
                    listResult.Add(count, item);
                    count++;
                }
                foreach (var item in sql)
                {
                    sqlResult.Add(count1, item.ClientCode);
                    count1++;
                }

                for (int i = 0; i < listResult.Count; i++)
                {
                    if (mess == "")
                    {
                        string BOOL = sqlResult.ContainsValue(listResult[i]).ToString();
                        if (sqlResult.ContainsValue(listResult[i]) == false)
                        {
                            mess = listResult[i].ToString();
                        }
                    }
                }

            }
            if (mess != "")
            {
                return "该客户不存在，请检查：" + mess;
            }


            //款号信息列表实体类
            WhItemExtendOMS WhItemExtendOMS = new WhItemExtendOMS();
            //循环验证款号信息
            for (int i = 0; i < entity.Count; i++)
            {
                WhItemExtendOMS = entity[i];
                res = WhItemExtendOMS.Validate();
                if (res != "")
                {
                    mess += "第" + (i + 1).ToString() + "行错误:" + res + Environment.NewLine;
                }
            }
            //数据合法返回"Y" 不合法返回错误的原因和行号
            if (mess == "") mess = "Y";

            return mess;
        }
        /// <summary>
        /// OMS新增或修改款号信息
        /// </summary>
        /// <param name="entity">款号信息列表实体类</param>
        /// <returns>成功或失败的错误信息</returns>
        public string ItemMasterExtendOMSAdd(List<WhItemExtendOMS> entity)
        {
            string mess = "";
            string res = "";
            //信息验证
            mess = ItemMasterExtendOMSAuthorize(entity);
            if (mess == "Y")
            {
                WhItemExtendOMS WhItemExtendOMS = new WhItemExtendOMS();
                List<ItemMaster> ItemMasterListAdd = new List<ItemMaster>();
                List<ItemMasterExtendOM> ItemMasterExtendOMSListAdd = new List<ItemMasterExtendOM>();
                List<ItemMaster> ItemMasterListUpdate = new List<ItemMaster>();
                List<ItemMasterExtendOM> ItemMasterExtendOMSListUpdate = new List<ItemMasterExtendOM>();
                WhClient whClient = new WhClient();
                for (int i = 0; i < entity.Count; i++)
                {
                    WhItemExtendOMS = entity[i];
                    whClient = idal.IWhClientDAL.SelectBy(u => u.WhCode == WhItemExtendOMS.WhCode && u.ClientCode == WhItemExtendOMS.ClientCode).First();
                    //款号不存在就新增


                    if (idal.IItemMasterDAL.SelectBy(u => u.WhCode == WhItemExtendOMS.WhCode && u.AltItemNumber == WhItemExtendOMS.AltItemNumber && u.ClientId == whClient.Id).Count == 0)
                    {
                        ItemMaster itemMaster = new ItemMaster();
                        ItemMasterExtendOM ItemMasterExtendOMS = new ItemMasterExtendOM();
                        //款号主表itemMaster
                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;//魔方系统款号
                        itemMaster.WhCode = WhItemExtendOMS.WhCode;//仓库编码
                        itemMaster.AltItemNumber = WhItemExtendOMS.AltItemNumber;//新增的款号信息
                        itemMaster.ClientId = whClient.Id;//客户对应的客户表ID
                        itemMaster.ClientCode = whClient.ClientCode;//客户
                        itemMaster.Style1 = WhItemExtendOMS.Style1;//属性1
                        itemMaster.Style2 = WhItemExtendOMS.Style2;//属性2
                        itemMaster.Style3 = WhItemExtendOMS.Style3;//属性3
                        itemMaster.UnitFlag = 0;//
                        itemMaster.UnitName = "EA";//单位
                        itemMaster.RgularId = 0;//
                        itemMaster.CreateUser = WhItemExtendOMS.CreateUser;//创建人
                        itemMaster.CreateDate = DateTime.Now;//创建时间
                        itemMaster.Length = Convert.ToDecimal(WhItemExtendOMS.Length) / 100;//长度
                        itemMaster.Width = Convert.ToDecimal(WhItemExtendOMS.Width) / 100;//宽度
                        itemMaster.Height = Convert.ToDecimal(WhItemExtendOMS.Height) / 100;//高度
                        itemMaster.Weight = Convert.ToDecimal(WhItemExtendOMS.Weight);//重量
                        itemMaster.Description = WhItemExtendOMS.Description;//描述
                        itemMaster.HandFlag = Convert.ToInt32(WhItemExtendOMS.HandFlag);//包装是否可以输入数量
                        itemMaster.InStallService = Convert.ToInt32(WhItemExtendOMS.InstallSevice);//是否送装
                        itemMaster.EAN = WhItemExtendOMS.EAN;//EAN
                        ItemMasterListAdd.Add(itemMaster);   //新增到数据列表
                        //扩展表ItemMasterExtendOMS
                        ItemMasterExtendOMS.WhCode = WhItemExtendOMS.WhCode; //仓库编码
                        ItemMasterExtendOMS.AltItemNumber = WhItemExtendOMS.AltItemNumber;//新增的款号信息
                        ItemMasterExtendOMS.ClientCode = WhItemExtendOMS.ClientCode;//客户
                        ItemMasterExtendOMS.ItemName = WhItemExtendOMS.ItemName;//品名
                        ItemMasterExtendOMS.Category = WhItemExtendOMS.Category;//Category类别
                        ItemMasterExtendOMS.Class = WhItemExtendOMS.ClassName;//类别1
                        ItemMasterExtendOMS.Pcs = Convert.ToDecimal(WhItemExtendOMS.Pcs);//箱规(件/箱)
                        ItemMasterExtendOMS.UnitName = WhItemExtendOMS.UnitName;//单位
                        ItemMasterExtendOMS.Size = WhItemExtendOMS.Size;//尺码
                        ItemMasterExtendOMS.Style = WhItemExtendOMS.Style;//款号
                        ItemMasterExtendOMS.PackageStyle = WhItemExtendOMS.PackageStyle;//包装类型
                        ItemMasterExtendOMS.Matieral = WhItemExtendOMS.Matieral;//材质
                        ItemMasterExtendOMS.Color = WhItemExtendOMS.Color;//颜色
                        ItemMasterExtendOMS.BoxCode = WhItemExtendOMS.BoxCode;//鞋盒编码（最小包装盒编码）
                        ItemMasterExtendOMS.CusStyle = WhItemExtendOMS.CusStyle;//客户自定义类型
                        ItemMasterExtendOMS.CreateDate = DateTime.Now;//创建日期
                        ItemMasterExtendOMS.CreateUser = WhItemExtendOMS.CreateUser;//创建人
                        ItemMasterExtendOMS.OriginCountry = WhItemExtendOMS.OriginCountry;//原产过
                        ItemMasterExtendOMSListAdd.Add(ItemMasterExtendOMS);//新增到数据列表
                    }
                    else
                    {
                        ItemMaster itemMasterUpdate = idal.IItemMasterDAL.SelectBy(u => u.WhCode == WhItemExtendOMS.WhCode && u.AltItemNumber == WhItemExtendOMS.AltItemNumber && u.ClientId == whClient.Id).OrderBy(u => u.Id).ToList().First();

                        ItemMasterExtendOM ItemMasterExtendOMSUpdate = idal.IItemMasterExtendOMDAL.SelectBy(u => u.WhCode == WhItemExtendOMS.WhCode && u.AltItemNumber == WhItemExtendOMS.AltItemNumber && u.ClientCode == whClient.ClientCode).First();
                        //款号主表itemMaster
                        itemMasterUpdate.Length = Convert.ToDecimal(WhItemExtendOMS.Length) / 100;//长度
                        itemMasterUpdate.Width = Convert.ToDecimal(WhItemExtendOMS.Width) / 100;//宽度
                        itemMasterUpdate.Height = Convert.ToDecimal(WhItemExtendOMS.Height) / 100;//高度
                        itemMasterUpdate.Weight = Convert.ToDecimal(WhItemExtendOMS.Weight);//重量
                        itemMasterUpdate.Description = WhItemExtendOMS.Description;//描述
                        itemMasterUpdate.HandFlag = Convert.ToInt32(WhItemExtendOMS.HandFlag);//包装是否可以输入数量
                        itemMasterUpdate.InStallService = Convert.ToInt32(WhItemExtendOMS.InstallSevice);//是否送装
                        itemMasterUpdate.EAN = WhItemExtendOMS.EAN;//EAN
                        itemMasterUpdate.UpdateUser = WhItemExtendOMS.CreateUser;//创建人
                        itemMasterUpdate.UpdateDate = DateTime.Now;//创建时间
                        ItemMasterListUpdate.Add(itemMasterUpdate);

                        //扩展表ItemMasterExtendOMS
                        ItemMasterExtendOMSUpdate.ItemName = WhItemExtendOMS.ItemName;//品名
                        ItemMasterExtendOMSUpdate.Category = WhItemExtendOMS.Category;//Category类别
                        ItemMasterExtendOMSUpdate.Class = WhItemExtendOMS.ClassName;//类别1
                        ItemMasterExtendOMSUpdate.Pcs = Convert.ToDecimal(WhItemExtendOMS.Pcs);//箱规(件/箱)
                        ItemMasterExtendOMSUpdate.UnitName = WhItemExtendOMS.UnitName;//单位
                        ItemMasterExtendOMSUpdate.Size = WhItemExtendOMS.Size;//尺码
                        ItemMasterExtendOMSUpdate.Style = WhItemExtendOMS.Style;//款号
                        ItemMasterExtendOMSUpdate.PackageStyle = WhItemExtendOMS.PackageStyle;//包装类型
                        ItemMasterExtendOMSUpdate.Matieral = WhItemExtendOMS.Matieral;//材质
                        ItemMasterExtendOMSUpdate.Color = WhItemExtendOMS.Color;//颜色
                        ItemMasterExtendOMSUpdate.BoxCode = WhItemExtendOMS.BoxCode;//鞋盒编码（最小包装盒编码）
                        ItemMasterExtendOMSUpdate.CusStyle = WhItemExtendOMS.CusStyle;//客户自定义类型
                        ItemMasterExtendOMSUpdate.UpdateDate = DateTime.Now;//创建日期
                        ItemMasterExtendOMSUpdate.UpdateUser = WhItemExtendOMS.CreateUser;//创建人
                        ItemMasterExtendOMSUpdate.OriginCountry = WhItemExtendOMS.OriginCountry;//原产过
                        ItemMasterExtendOMSListUpdate.Add(ItemMasterExtendOMSUpdate);

                    }
                }

                if (ItemMasterListAdd.Count > 0)
                {
                    idal.IItemMasterDAL.Add(ItemMasterListAdd);
                    idal.IItemMasterExtendOMDAL.Add(ItemMasterExtendOMSListAdd);
                }
                idal.SaveChanges();
                //idal.IItemMasterDAL.SaveChanges();
                //idal.IItemMasterExtendOMDAL.SaveChanges();

            }
            return mess;
        }
        //修改款号基础信息
        public string ItemMasterEdit(ItemMaster im)
        {
            if (im.HandFlag == null)
            {
                im.HandFlag = 0;
            }
            if (im.ScanFlag == null)
            {
                im.ScanFlag = 0;
            }

            im.UpdateDate = DateTime.Now;
            idal.IItemMasterDAL.UpdateBy(im, u => u.Id == im.Id, new string[] { "Description", "EAN", "HandFlag", "ScanFlag", "ScanRule", "UpdateDate", "UpdateUser", "UnitName" });

            List<SortTaskDetail> checkSort = idal.ISortTaskDetailDAL.SelectBy(u => u.ItemId == im.Id && u.PlanQty != u.PackQty).ToList();
            if (checkSort.Count > 0)
            {
                SortTaskDetail sort = new SortTaskDetail();
                sort.EAN = im.EAN;
                sort.HandFlag = Convert.ToInt32(im.HandFlag);
                sort.ScanFlag = Convert.ToInt32(im.ScanFlag);
                if (string.IsNullOrEmpty(im.ScanRule))
                {
                    sort.ScanRule = "0";
                }
                else
                {
                    sort.ScanRule = im.ScanRule;
                }

                idal.ISortTaskDetailDAL.UpdateBy(sort, u => u.ItemId == im.Id && u.PlanQty != u.PackQty, new string[] { "EAN", "HandFlag", "ScanFlag", "ScanRule" });
            }

            idal.SaveChanges();
            return "Y";
        }

        //新增款号
        public string ItemMaterAdd(ItemMaster im)
        {

            if (im.ClientCode == "" || im.ClientCode == null)
            {
                if (im.ClientId != 0)
                {
                    im.ClientCode = idal.IWhClientDAL.SelectBy(u => u.Id == im.ClientId).First().ClientCode;
                }
                else
                {
                    return "0";
                }
            }

            if (im.ClientId == 0)
            {
                im.ClientId = idal.IWhClientDAL.SelectBy(u => u.ClientCode == im.ClientCode && u.WhCode == im.WhCode).First().Id;
            }

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == im.WhCode && u.AltItemNumber == im.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (im.Style1 == null ? "" : im.Style1) && (u.Style2 == null ? "" : u.Style2) == (im.Style2 == null ? "" : im.Style2) && (u.Style3 == null ? "" : u.Style3) == (im.Style3 == null ? "" : im.Style3) && u.ClientCode == im.ClientCode).OrderBy(u => u.Id).ToList();
            ItemMaster itemMaster = new ItemMaster();
            if (listItemMaster.Count == 0)
            {
                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                itemMaster.WhCode = im.WhCode;
                itemMaster.AltItemNumber = im.AltItemNumber;
                itemMaster.Description = im.Description;
                itemMaster.ScanFlag = im.ScanFlag;
                itemMaster.HandFlag = im.HandFlag;
                itemMaster.ScanRule = im.ScanRule;
                itemMaster.Weight = im.Weight;
                itemMaster.CartonName = im.CartonName;
                itemMaster.ClientId = (int)im.ClientId;
                itemMaster.ClientCode = im.ClientCode;
                itemMaster.Style1 = im.Style1 ?? "";
                itemMaster.Style2 = im.Style2 ?? "";
                itemMaster.Style3 = im.Style3 ?? "";
                itemMaster.UnitFlag = 0;
                itemMaster.EAN = im.EAN;
                itemMaster.InStallService = 0;
                if (im.UnitName == "" || im.UnitName == null)
                {
                    itemMaster.UnitName = "none";
                }
                else
                {
                    itemMaster.UnitName = im.UnitName;
                }

                itemMaster.CreateUser = im.CreateUser;
                itemMaster.CreateDate = DateTime.Now;
                idal.IItemMasterDAL.Add(itemMaster);   //款号不存在就新增

                return "1";
            }
            else
            {
                return "0";
            }

        }

        public string ItemMaterUpdate(ItemMaster im)
        {

            if (im.ClientCode == "" || im.ClientCode == null)
            {
                if (im.ClientId != 0)
                {
                    im.ClientCode = idal.IWhClientDAL.SelectBy(u => u.Id == im.ClientId).First().ClientCode;
                }
                else
                {
                    return "0";
                }
            }

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == im.WhCode && u.AltItemNumber == im.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (im.Style1 == null ? "" : im.Style1) && (u.Style2 == null ? "" : u.Style2) == (im.Style2 == null ? "" : im.Style2) && (u.Style3 == null ? "" : u.Style3) == (im.Style3 == null ? "" : im.Style3) && u.ClientCode == im.ClientCode).OrderBy(u => u.Id).ToList();
            ItemMaster itemMaster = new ItemMaster();

            if (listItemMaster.Count == 1)
            {
                itemMaster = listItemMaster.First();
                im.UpdateUser = im.CreateUser;
                im.UpdateDate = DateTime.Now;

                idal.IItemMasterDAL.UpdateBy(im, u => u.Id == itemMaster.Id, new string[] { "Description", "HandFlag", "EAN", "ScanFlag", "ScanRule", "UpdateUser", "UpdateDate", "Weight", "Remark1", "InStallService" });

                List<SortTaskDetail> checkSort = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == itemMaster.WhCode && u.ItemId == itemMaster.Id && u.PlanQty != u.PackQty).ToList();
                if (checkSort.Count > 0)
                {
                    SortTaskDetail sort = new SortTaskDetail();
                    sort.EAN = im.EAN;
                    sort.HandFlag = Convert.ToInt32(im.HandFlag ?? 0);
                    sort.ScanFlag = Convert.ToInt32(im.ScanFlag ?? 0);
                    if (string.IsNullOrEmpty(im.ScanRule))
                    {
                        sort.ScanRule = "0";
                    }
                    else
                    {
                        sort.ScanRule = im.ScanRule;
                    }

                    idal.ISortTaskDetailDAL.UpdateBy(sort, u => u.ItemId == itemMaster.Id && u.PlanQty != u.PackQty, new string[] { "EAN", "HandFlag", "ScanFlag", "ScanRule" });
                }

                idal.SaveChanges();
                return "1";
            }
            else
            {
                return "0";
            }





        }
        #endregion


        #region 7.仓库管理(盘点等仓库操作)

        //仓库异常原因列表
        //对应 C_HoldReasonController 中的 List 方法
        public List<HoldMaster> WarehouseHoldMasterList(HoldMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IHoldMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ClientCode == "all"
                      select a;

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //箱号采集信息管理-------------------------------------

        //箱号采集列表
        public List<SerialNumberInOut> SerialNumberInList(SerialNumberInSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISerialNumberInDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new SerialNumberInOut
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          ReceiptId = a.ReceiptId,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = a.AltItemNumber,
                          PoId = a.PoId,
                          ItemId = a.ItemId,
                          CartonId = a.CartonId,
                          HuId = a.HuId,
                          Length = a.Length,
                          Width = a.Width,
                          Height = a.Height,
                          Weight = a.Weight,
                          LotNumber1 = a.LotNumber1,
                          LotNumber2 = a.LotNumber2,
                          LotDate = a.LotDate,
                          ToOutStatus = a.ToOutStatus,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.CartonId))
                sql = sql.Where(u => u.CartonId == searchEntity.CartonId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }
        //箱号采集列表
        public List<SerialNumberDetailOut> SerialNumberDetailList(SerialNumberDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISerialNumberDetailDAL.SelectAll()
                      where a.HeadId == searchEntity.HeadId
                      select new SerialNumberDetailOut
                      {
                          Id = a.Id,
                          HeadId = a.HeadId,
                          PCS = a.PCS,
                          UPC = a.UPC,
                          CreateDate = a.CreateDate,
                          SNType = a.SNType
                      };
            if (!string.IsNullOrEmpty(searchEntity.UPC))
                sql = sql.Where(u => u.UPC.Contains(searchEntity.UPC));
            if (searchEntity.HeadId != 0)
                sql = sql.Where(u => u.HeadId == searchEntity.HeadId);
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //箱号信息修改
        public int SerialNumberEdit(SerialNumberIn entity)
        {
            SerialNumberIn Ser = idal.ISerialNumberInDAL.SelectBy(u => u.Id == entity.Id).First();

            if (idal.ISerialNumberInDAL.SelectBy(u => u.CartonId == entity.CartonId && u.ReceiptId == Ser.ReceiptId && u.WhCode == Ser.WhCode).Count() == 0)
            {
                entity.UpdateDate = DateTime.Now;
                idal.ISerialNumberInDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "CartonId", "UpdateUser", "UpdateDate" });
                idal.ISerialNumberInDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //批量删除序列号
        public int SerialNumberDelByHuId(SerialNumberIn entity)
        {
            SerialNumberIn Ser = idal.ISerialNumberInDAL.SelectBy(u => u.Id == entity.Id).First();
            //检查有没有已经出货的扫描序列号
            int counts = idal.ISerialNumberInDAL.SelectBy(u => u.WhCode == Ser.WhCode && u.ReceiptId == Ser.ReceiptId && u.HuId == Ser.HuId && u.ToOutStatus == 0).Count();
            //如果没有可以删除
            if (counts == 0)
            {
                idal.ISerialNumberInDAL.DeleteBy(u => u.WhCode == Ser.WhCode && u.ReceiptId == Ser.ReceiptId && u.HuId == Ser.HuId);
                idal.ISerialNumberInDAL.SaveChanges();
                return 1;
            }
            else
                return 0;
        }

        //新增箱号信息
        public int SerialNumberAdd(SerialNumberIn entity)
        {
            if (idal.ISerialNumberInDAL.SelectBy(u => u.CartonId == entity.CartonId && u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).Count() > 0)
            {
                return 0;
            }
            else
            {
                idal.ISerialNumberInDAL.Add(entity);
                idal.ISerialNumberInDAL.SaveChanges();
                return 1;
            }
        }

        //出货箱号采集管理 查询列表
        public List<SerialNumberOut> SerialNumberOutList(SerialNumberOutSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISerialNumberOutDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.CartonId))
                sql = sql.Where(u => u.CartonId == searchEntity.CartonId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //出货采集序列号 修改
        public string SerialNumberOutEdit(SerialNumberOut entity)
        {
            SerialNumberOut Ser = idal.ISerialNumberOutDAL.SelectBy(u => u.Id == entity.Id).First();

            if (idal.ISerialNumberOutDAL.SelectBy(u => u.CartonId == entity.CartonId && u.LoadId == Ser.LoadId && u.WhCode == Ser.WhCode).Count() == 0)
            {
                entity.UpdateDate = DateTime.Now;
                idal.ISerialNumberOutDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "CartonId", "UpdateUser", "UpdateDate" });
                idal.ISerialNumberInDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "序列号已存在！";
            }
        }

        //出货序列号删除
        public string SerialNumberOutDel(SerialNumberOut entity)
        {
            SerialNumberOut enti = idal.ISerialNumberOutDAL.SelectBy(u => u.Id == entity.Id).First();
            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "81";
            tl.Description = "出货序列号删除";
            tl.TranDate = DateTime.Now;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = enti.WhCode;
            tl.ClientCode = enti.ClientCode;
            tl.LoadId = enti.LoadId;
            tl.SoNumber = enti.SoNumber;
            tl.CustomerPoNumber = enti.CustomerPoNumber;
            tl.AltItemNumber = enti.AltItemNumber;
            tl.HuId = enti.HuId;
            tl.Remark = enti.CartonId;
            idal.ITranLogDAL.Add(tl);

            idal.ISerialNumberOutDAL.DeleteBy(u => u.Id == enti.Id);
            idal.SaveChanges();

            return "Y";
        }


        //新增出货序列号信息
        public string SerialNumberOutAdd(SerialNumberOut entity)
        {
            if (idal.ISerialNumberOutDAL.SelectBy(u => u.CartonId == entity.CartonId && u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).Count() > 0)
            {
                return "序列号已存在！";
            }
            else
            {
                entity.CreateDate = DateTime.Now;
                idal.ISerialNumberOutDAL.Add(entity);
                idal.ISerialNumberOutDAL.SaveChanges();
                return "Y";
            }
        }


        //新增出货序列号信息
        public string SerialNumberAddOther(SerialNumberOut entity)
        {
            if (idal.ISerialNumberOutDAL.SelectBy(u => u.CartonId == entity.CartonId && u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).Count() > 0)
            {
                return "序列号已存在！";
            }
            else
            {
                List<SerialNumberIn> list = idal.ISerialNumberInDAL.SelectBy(u => u.WhCode == entity.WhCode && u.SoNumber == entity.SoNumber);
                if (list.Count == 0)
                {
                    return "收货采集中未找到该SO号信息，请检查！";
                }

                List<SerialNumberIn> getList = list.Where(u => u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.HuId == entity.HuId).ToList();
                if (getList.Count == 0)
                {
                    return "收货采集中未找到该客户名SOPO款号托盘信息，请检查！";
                }

                List<SerialNumberIn> getList1 = getList.Where(u => u.CartonId == entity.CartonId && u.HuId == entity.HuId).ToList();
                if (getList1.Count == 0)
                {
                    return "收货采集中此托盘没有采集过该序列号，请检查！";
                }

                SerialNumberIn first = getList1.First();

                entity.ItemId = first.ItemId;
                entity.Length = first.Length;
                entity.Width = first.Width;
                entity.Height = first.Height;
                entity.Weight = first.Weight;
                entity.LotNumber1 = first.LotNumber1;
                entity.LotNumber2 = first.LotNumber2;
                entity.LotDate = first.LotDate;
                entity.CreateDate = DateTime.Now;

                idal.ISerialNumberOutDAL.Add(entity);
                idal.ISerialNumberOutDAL.SaveChanges();
                return "Y";
            }
        }



        //收货箱号采集管理 查询列表
        public List<HeportSerialNumberIn> HeportSerialNumberInList(SerialNumberInSearch searchEntity, out int total)
        {
            var sql = from a in idal.IHeportSerialNumberInDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.CartonId))
                sql = sql.Where(u => u.CartonId == searchEntity.CartonId);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //箱号信息修改
        public int HeportSerialNumberEdit(HeportSerialNumberIn entity)
        {
            HeportSerialNumberIn Ser = idal.IHeportSerialNumberInDAL.SelectBy(u => u.Id == entity.Id).First();

            entity.UpdateDate = DateTime.Now;
            idal.IHeportSerialNumberInDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "CartonId", "UpdateUser", "UpdateDate" });
            idal.IHeportSerialNumberInDAL.SaveChanges();
            return 1;

        }

        //批量删除序列号
        public int HeportSerialNumberDelByHuId(HeportSerialNumberIn entity)
        {
            HeportSerialNumberIn Ser = idal.IHeportSerialNumberInDAL.SelectBy(u => u.Id == entity.Id).First();

            idal.IHeportSerialNumberInDAL.DeleteBy(u => u.WhCode == Ser.WhCode && u.ReceiptId == Ser.ReceiptId && u.HuId == Ser.HuId);
            idal.IHeportSerialNumberInDAL.SaveChanges();
            return 1;
        }


        //盘点信息管理-----------------------------------------------------------------------------------------------


        public List<CycleCountMasterResult> CycleCountMasterList(CycleCountMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.ICycleCountMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new CycleCountMasterResult
                      {
                          Id = a.Id,
                          TaskNumber = a.TaskNumber,
                          Type =
                           a.Type == "B" ? "锁定模式" :
                           a.Type == "L" ? "非锁定模式" : null,
                          Status =
                           a.Status == "U" ? "未盘点" :
                           a.Status == "A" ? "正在盘点" :
                           a.Status == "C" ? "完成盘点" : null,
                          Description = a.Description,
                          CreateType = a.CreateType,
                          TypeDescription = a.TypeDescription,
                          LocationNullShow = a.LocationNullFlag == 1 ? "是" : "否",
                          OneByOneScanShow = a.OneByOneScanFlag == 1 ? "是" : "否",
                          CompareStorageLocationHuShow = a.CompareStorageLocationHu == 1 ? "否" : "是",
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.TaskNumber))
                sql = sql.Where(u => u.TaskNumber == searchEntity.TaskNumber);
            if (searchEntity.CreateType != 0)
                sql = sql.Where(u => u.CreateType == searchEntity.CreateType);
            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.Type == searchEntity.Type);
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //新增盘点任务
        public string CycleCountMasterAdd(CycleCountMasterInsert entity)
        {
            if (entity.Type == "")
            {
                return "数据有误，请重新操作！";
            }
            if (entity.Location == "" && entity.LocationColumn == "" && entity.LocationRowBegin == 0 && entity.LocationRowEnd == 0)
            {
                return "数据有误，请填写盘点数据！";
            }

            lock (o)
            {
                entity.TaskNumber = "PD" + DI.IDGenerator.NewId;

                string result = CycleCountDetailAdd(entity);

                if (result == "Y")
                {
                    CycleCountMaster master = new CycleCountMaster();
                    master.TaskNumber = entity.TaskNumber;
                    master.WhCode = entity.WhCode;
                    master.Type = entity.Type;
                    master.Description = entity.Description;
                    master.CreateType = entity.CreateType;
                    master.LocationNullFlag = entity.LocationNullFlag;
                    master.OneByOneScanFlag = entity.OneByOneScanFlag;
                    master.TypeDescription = entity.TypeDescription;
                    master.CompareStorageLocationHu = entity.CompareStorageLocationHu;
                    master.Status = "U";
                    master.CreateUser = entity.CreateUser;
                    master.CreateDate = DateTime.Now;
                    idal.ICycleCountMasterDAL.Add(master);

                    if (entity.Type == "B")     //如果是锁定模式
                    {
                        CycleCountInventoryAdd(entity);
                    }
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return result;
                }
            }
        }

        //生成盘点任务库位明细
        //按照起始库位创建或按照 库位 通道列数等属性创建
        public string CycleCountDetailAdd(CycleCountMasterInsert entity)
        {
            var sql = (from a in idal.IWhLocationDAL.SelectAll()
                       where a.WhCode == entity.WhCode && (a.LocationTypeId == 1 || a.LocationTypeId == 5) && a.Status == "A" && a.LocationId != ""
                       select a).Distinct().ToList();

            if (!string.IsNullOrEmpty(entity.BeginLocationId))
            {
                sql = sql.Where(u => String.Compare(u.LocationId, entity.BeginLocationId, StringComparison.Ordinal) >= 0 &&
   String.Compare(u.LocationId, entity.EndLocationId, StringComparison.Ordinal) <= 0).ToList();
            }

            if (!string.IsNullOrEmpty(entity.Location))
                sql = sql.Where(u => u.Location == entity.Location).ToList();
            if (!string.IsNullOrEmpty(entity.LocationColumn))
                sql = sql.Where(u => u.LocationColumn == entity.LocationColumn).ToList();
            if (entity.LocationRowBegin > 0)
                sql = sql.Where(u => u.LocationRow >= entity.LocationRowBegin && u.LocationRow <= entity.LocationRowEnd).ToList();
            //是否去掉空库位
            if (entity.LocationNullFlag == 1)
            {
                sql = (from a in sql
                       join b in idal.IHuMasterDAL.SelectAll()
                       on new { a = a.WhCode, b = a.LocationId } equals new { a = b.WhCode, b = b.Location }
                       select a).ToList();
            }

            List<WhLocation> whLocationList = sql.ToList();
            if (whLocationList.Count > 0)
            {
                string[] loc = (from a in whLocationList
                                select a.LocationId).Distinct().ToArray();

                List<CycleCountDetail> CycleCountDetailList = new List<CycleCountDetail>();
                foreach (var item in loc)
                {
                    CycleCountDetail detail = new CycleCountDetail();
                    detail.WhCode = entity.WhCode;
                    detail.TaskNumber = entity.TaskNumber;
                    detail.LocationId = item;
                    detail.Status = "U";
                    CycleCountDetailList.Add(detail);

                    if (entity.Type == "B")     //如果是锁定模式
                    {
                        WhLocation whLocation = new WhLocation();
                        whLocation.Status = "H";
                        whLocation.UpdateUser = entity.CreateUser;
                        idal.IWhLocationDAL.UpdateBy(whLocation, u => u.LocationId == item && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser" });
                    }
                }
                idal.ICycleCountDetailDAL.Add(CycleCountDetailList);
                return "Y";
            }
            else
            {
                return "库位无效或已释放盘点任务，请确认！";
            }
        }


        //锁定模式 释放盘点任务时 增加盘点任务库存
        public void CycleCountInventoryAdd(CycleCountMasterInsert entity)
        {
            List<CycleCountInventoryResult> sql = new List<CycleCountInventoryResult>();
            if (entity.CreateType == 1)
            {
                sql = (from a in idal.IWhLocationDAL.SelectAll()
                       where a.WhCode == entity.WhCode && a.Status == "A" && (a.LocationTypeId == 1 || a.LocationTypeId == 5)
                       join b in idal.IHuMasterDAL.SelectAll()
                             on new { a.WhCode, a.LocationId }
                         equals new { b.WhCode, LocationId = b.Location } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { a, b, c } by new
                       {
                           a.WhCode,
                           a.LocationId,
                           c.ClientCode,
                           b.HuId,
                           c.SoNumber,
                           c.CustomerPoNumber,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           a.Location,
                           a.LocationColumn,
                           a.LocationRow,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.LocationId,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           SoNumber = g.Key.SoNumber,
                           CustomerPoNumber = g.Key.CustomerPoNumber,
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           Location = g.Key.Location,
                           LocationColumn = g.Key.LocationColumn,
                           LocationRow = g.Key.LocationRow,
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();

            }
            else if (entity.CreateType == 2)
            {
                sql = (from a in idal.IWhLocationDAL.SelectAll()
                       where a.WhCode == entity.WhCode && a.Status == "A" && (a.LocationTypeId == 1 || a.LocationTypeId == 5)
                       join b in idal.IHuMasterDAL.SelectAll()
                             on new { a.WhCode, a.LocationId }
                         equals new { b.WhCode, LocationId = b.Location } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { a, b, c } by new
                       {
                           a.WhCode,
                           a.LocationId,
                           c.ClientCode,
                           b.HuId,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           a.Location,
                           a.LocationColumn,
                           a.LocationRow,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.LocationId,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           SoNumber = "",
                           CustomerPoNumber = "",
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           Location = g.Key.Location,
                           LocationColumn = g.Key.LocationColumn,
                           LocationRow = g.Key.LocationRow,
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();

            }
            if (!string.IsNullOrEmpty(entity.BeginLocationId))
            {
                sql = sql.Where(u => String.Compare(u.LocationId, entity.BeginLocationId, StringComparison.Ordinal) >= 0 &&
   String.Compare(u.LocationId, entity.EndLocationId, StringComparison.Ordinal) <= 0).ToList();
            }

            if (!string.IsNullOrEmpty(entity.Location))
                sql = sql.Where(u => u.Location == entity.Location).ToList();
            if (!string.IsNullOrEmpty(entity.LocationColumn))
                sql = sql.Where(u => u.LocationColumn == entity.LocationColumn).ToList();
            if (entity.LocationRowBegin > 0)
                sql = sql.Where(u => u.LocationRow >= entity.LocationRowBegin && u.LocationRow <= entity.LocationRowEnd).ToList();

            List<CycleCountInventoryResult> list = sql.ToList();
            foreach (var item in list)
            {
                CycleCountInventory inventory = new CycleCountInventory();
                inventory.WhCode = entity.WhCode;
                inventory.TaskNumber = entity.TaskNumber;
                inventory.LocationId = item.LocationId;
                inventory.ClientCode = item.ClientCode;

                if (entity.CompareStorageLocationHu == 1)
                {
                    inventory.HuId = item.LocationId;
                }
                else
                {
                    inventory.HuId = item.HuId ?? "";
                }
                //inventory.HuId = item.HuId;

                inventory.SoNumber = item.SoNumber;
                inventory.CustomerPoNumber = item.CustomerPoNumber;
                inventory.AltItemNumber = item.AltItemNumber;
                inventory.LotNumber1 = item.LotNumber1;
                inventory.LotNumber2 = item.LotNumber2;
                inventory.Qty = item.Qty;
                inventory.HoldId = item.HoldId;
                inventory.HoldReason = item.HoldReason;
                inventory.Status = item.Status;
                idal.ICycleCountInventoryDAL.Add(inventory);
            }
        }

        //盘点任务明细列表 比较库存与实盘结果
        public List<CycleCountDetailResult> CycleCountDetailList(CycleCountDetailSearch searchEntity, out int total)
        {
            //得到盘点类型 ByPo 还是BySku
            CycleCountMaster cycMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.TaskNumber == searchEntity.TaskNumber).First();

            //得到系统库存盘点数据
            List<CycleCountDetailResult> inventoryList = new List<CycleCountDetailResult>();

            //得到实际盘点数据
            List<CycleCountDetailResult> checkList = new List<CycleCountDetailResult>();

            //如果是ByPoSku 结果比较需要POSKU对比 
            //1是ByPoSku 2是BySku

            if (cycMaster.CreateType == 2)
            {
                //BySku盘点时 库存需要聚合同一托盘不同PO的数量
                inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                 join b in idal.ICycleCountInventoryDAL.SelectAll()
                                 on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                 from ab in temp1.DefaultIfEmpty()
                                 where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                                 group new { a, ab } by new
                                 {
                                     ab.WhCode,
                                     a.TaskNumber,
                                     a.Status,
                                     a.LocationId,
                                     ab.HuId,
                                     ab.AltItemNumber,
                                     a.CheckUser,
                                     a.CheckDate
                                 } into g
                                 select new CycleCountDetailResult
                                 {
                                     WhCode = g.Key.WhCode,
                                     TaskNumber = g.Key.TaskNumber,
                                     Status = g.Key.Status,
                                     LocationId = g.Key.LocationId,
                                     HuId = g.Key.HuId,
                                     Qty = g.Sum(p => p.ab.Qty),
                                     CustomerPoNumber = "",
                                     AltItemNumber = g.Key.AltItemNumber,
                                     CheckUser = g.Key.CheckUser,
                                     CheckDate = g.Key.CheckDate
                                 }).ToList();

                checkList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                             join b in idal.ICycleCountCheckDAL.SelectAll()
                             on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                             from ab in temp1.DefaultIfEmpty()
                             where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                             group new { a, ab } by new
                             {
                                 ab.WhCode,
                                 a.TaskNumber,
                                 a.Status,
                                 a.LocationId,
                                 ab.HuId,
                                 ab.AltItemNumber,
                                 a.CheckUser,
                                 a.CheckDate
                             } into g
                             select new CycleCountDetailResult
                             {
                                 WhCode = g.Key.WhCode,
                                 TaskNumber = g.Key.TaskNumber,
                                 Status = g.Key.Status,
                                 LocationId = g.Key.LocationId,
                                 HuId = g.Key.HuId,
                                 Qty = g.Sum(p => p.ab.Qty),
                                 CustomerPoNumber = "",
                                 AltItemNumber = g.Key.AltItemNumber,
                                 CheckUser = g.Key.CheckUser,
                                 CheckDate = g.Key.CheckDate
                             }).ToList();
            }
            else
            {
                //得到库存盘点
                inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                 join b in idal.ICycleCountInventoryDAL.SelectAll()
                                 on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                 from ab in temp1.DefaultIfEmpty()
                                 where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                                 select new CycleCountDetailResult
                                 {
                                     WhCode = ab.WhCode,
                                     TaskNumber = a.TaskNumber,
                                     Status = a.Status,
                                     LocationId = a.LocationId,
                                     HuId = ab.HuId,
                                     Qty = ab.Qty,
                                     CustomerPoNumber = ab.CustomerPoNumber,
                                     AltItemNumber = ab.AltItemNumber,
                                     CheckUser = a.CheckUser,
                                     CheckDate = a.CheckDate
                                 }).ToList();

                checkList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                             join b in idal.ICycleCountCheckDAL.SelectAll()
                             on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                             from ab in temp1.DefaultIfEmpty()
                             where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                             select new CycleCountDetailResult
                             {
                                 WhCode = ab.WhCode,
                                 Id = a.Id,
                                 TaskNumber = a.TaskNumber,
                                 Status = a.Status,
                                 LocationId = a.LocationId,
                                 HuId = ab.HuId,
                                 Qty = ab.Qty,
                                 CustomerPoNumber = ab.CustomerPoNumber,
                                 AltItemNumber = ab.AltItemNumber,
                                 CheckUser = a.CheckUser,
                                 CheckDate = a.CheckDate
                             }).ToList();
            }

            //得到盘点明细
            List<CycleCountDetail> detailList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                                 where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                                                 select a).ToList();

            //输入库位条件时 缩小范围
            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                detailList = detailList.Where(u => u.LocationId.Contains(searchEntity.LocationId)).ToList();


            //最终显示结果
            List<CycleCountDetailResult> sql = new List<CycleCountDetailResult>();

            foreach (var item in detailList)
            {
                List<CycleCountDetailResult> inventoryListResult = inventoryList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                List<CycleCountDetailResult> checkListResult = checkList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                CycleCountDetailResult entity = new CycleCountDetailResult();
                if (inventoryListResult.Count == checkListResult.Count)
                {
                    int checkCount = 0;
                    foreach (var item1 in checkListResult)
                    {
                        //如果是ByPoSku 结果比较需要POSKU对比
                        if (cycMaster.CreateType == 1)
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.CustomerPoNumber == null ? "" : u.CustomerPoNumber) == (item1.CustomerPoNumber == null ? "" : item1.CustomerPoNumber) && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }
                        else if (cycMaster.CreateType == 2)  //如果是BySKU 结果比较需要SKU对比
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }

                    }

                    if (checkCount == checkListResult.Count)
                    {
                        entity.Id = item.Id;
                        entity.WhCode = item.WhCode;
                        entity.TaskNumber = item.TaskNumber;
                        entity.LocationId = item.LocationId;
                        entity.Status = item.Status == "U" ? "未盘点" : item.Status == "C" ? "已盘点" : null;
                        entity.CheckUser = item.CheckUser;
                        entity.CheckDate = item.CheckDate;
                        entity.Action1 = "正常";
                        sql.Add(entity);
                    }
                    else
                    {
                        entity.Id = item.Id;
                        entity.WhCode = item.WhCode;
                        entity.TaskNumber = item.TaskNumber;
                        entity.LocationId = item.LocationId;
                        entity.Status = item.Status == "U" ? "未盘点" : item.Status == "C" ? "已盘点" : null;
                        entity.CheckUser = item.CheckUser;
                        entity.CheckDate = item.CheckDate;
                        entity.Action1 = "有差异";
                        sql.Add(entity);
                    }
                }
                else
                {
                    entity.Id = item.Id;
                    entity.WhCode = item.WhCode;
                    entity.TaskNumber = item.TaskNumber;
                    entity.LocationId = item.LocationId;
                    entity.Status = item.Status == "U" ? "未盘点" : item.Status == "C" ? "已盘点" : null;
                    entity.CheckUser = item.CheckUser;
                    entity.CheckDate = item.CheckDate;
                    entity.Action1 = "有差异";
                    sql.Add(entity);
                }
            }

            //输入盘点结果条件时 缩小范围
            if (!string.IsNullOrEmpty(searchEntity.Action))
                sql = sql.Where(u => u.Action1 == searchEntity.Action).ToList();

            //输入盘点状态条件时 缩小范围
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status).ToList();

            total = sql.Count();
            sql = sql.OrderBy(u => u.LocationId).ThenBy(u => u.CheckDate).ToList();
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return sql;
        }

        //删除盘点任务
        public string CycleCountMasterDel(CycleCountMaster entity)
        {
            List<CycleCountMaster> cycleMasterList = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber);
            if (cycleMasterList.Count == 0)
            {
                return "没有该盘点任务，请确认！";
            }

            CycleCountMaster cycleMaster = cycleMasterList.First();

            if (cycleMaster.Status == "C")
            {
                return "当前任务已完成盘点！";
            }

            //点击删除时 删除盘点任务中 未盘点的任务明细
            List<CycleCountDetail> sql = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && a.TaskNumber == entity.TaskNumber && a.Status == "U"
                                          select a).ToList();
            foreach (var item in sql)
            {
                if (cycleMaster.Type == "B")
                {
                    if (!string.IsNullOrEmpty(item.LocationId))
                    {
                        idal.IWhLocationDAL.UpdateByExtended(u => u.LocationId == item.LocationId && u.WhCode == entity.WhCode, t => new WhLocation { Status = "A", UpdateUser = entity.CreateUser, UpdateDate = DateTime.Now });

                        idal.ICycleCountInventoryDAL.DeleteByExtended(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber && u.LocationId == item.LocationId);
                    }
                }

                idal.ICycleCountDetailDAL.DeleteByExtended(u => u.Id == item.Id);
            }

            //再次验证盘点库位明细是否还有数据
            //没有需要删除 盘点任务号
            List<CycleCountDetail> sqlCheck = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                               where a.WhCode == entity.WhCode && a.TaskNumber == entity.TaskNumber
                                               select a).ToList();
            if (sqlCheck.Count > 0)
            {
                cycleMaster.Status = "C";
                cycleMaster.UpdateDate = DateTime.Now;
                cycleMaster.UpdateUser = entity.CreateUser;

                idal.ICycleCountMasterDAL.UpdateBy(cycleMaster, u => u.Id == cycleMaster.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
            }
            else
            {
                idal.ICycleCountMasterDAL.DeleteByExtended(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber);
            }

            idal.SaveChanges();
            return "Y";
        }


        #region 1.按SKU盘点

        //盘点任务开始
        public string CycleCountInsertComplex(CycleCountInsertComplex entity)
        {
            List<CycleCountMaster> cycleMasterList = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber);
            if (cycleMasterList.Count == 0)
            {
                return "没有该盘点任务，请确认！";
            }

            CycleCountMaster cycleMaster = cycleMasterList.First();
            if (cycleMaster.Status == "C")
            {
                return "该盘点任务状态有误，请确认！";
            }

            if (idal.ICycleCountDetailDAL.SelectBy(u => u.TaskNumber == entity.TaskNumber && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId && u.Status == "U").Count == 0)
            {
                return "当前盘点任务没有该库位或状态有误！";
            }
            else
            {
                if (cycleMaster.Status == "U")
                {
                    cycleMaster.Status = "A";
                    cycleMaster.UpdateUser = entity.CreateUser;
                    cycleMaster.UpdateDate = DateTime.Now;
                    idal.ICycleCountMasterDAL.UpdateBy(cycleMaster, u => u.Id == cycleMaster.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }
                CycleCountDetail detail = new CycleCountDetail();
                detail.Status = "C";
                detail.CheckUser = entity.CreateUser;
                detail.CheckDate = DateTime.Now;
                idal.ICycleCountDetailDAL.UpdateBy(detail, u => u.TaskNumber == entity.TaskNumber && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId, new string[] { "Status", "CheckUser", "CheckDate" });

                if (cycleMaster.Type == "B")    //如果是锁定模式
                {
                    B_CycleCountInsertComplex(entity);
                }
                else
                {
                    L_CycleCountInsertComplex(entity);
                }

                idal.SaveChanges();
                return "Y";
            }
        }


        //锁定模式下 盘点动作
        public void B_CycleCountInsertComplex(CycleCountInsertComplex entity)
        {
            CycleCountCheckAdd(entity);
        }

        //增加盘点结果
        private void CycleCountCheckAdd(CycleCountInsertComplex entity)
        {
            foreach (var item in entity.HuIdModel)
            {
                CycleCountCheck cycleCheck = new CycleCountCheck();
                cycleCheck.WhCode = entity.WhCode;
                cycleCheck.TaskNumber = entity.TaskNumber;
                cycleCheck.LocationId = entity.LocationId;
                cycleCheck.HuId = item.HuId;
                cycleCheck.AltItemNumber = item.AltItemNumber;
                cycleCheck.Qty = item.Qty;
                idal.ICycleCountCheckDAL.Add(cycleCheck);
            }
        }

        //非锁定模式下 盘点动作
        public void L_CycleCountInsertComplex(CycleCountInsertComplex entity)
        {
            CycleCountCheckAdd(entity);

            CycleCountMaster cycMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber).First();

            List<CycleCountInventoryResult> sql = new List<CycleCountInventoryResult>();
            if (cycMaster.CreateType == 1)
            {
                sql = (from b in idal.IHuMasterDAL.SelectAll()
                       where b.WhCode == entity.WhCode && b.Location == entity.LocationId
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { b, c } by new
                       {
                           b.WhCode,
                           b.Location,
                           c.ClientCode,
                           b.HuId,
                           c.CustomerPoNumber,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.Location,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           CustomerPoNumber = g.Key.CustomerPoNumber,
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();
            }
            else if (cycMaster.CreateType == 2)
            {
                sql = (from b in idal.IHuMasterDAL.SelectAll()
                       where b.WhCode == entity.WhCode && b.Location == entity.LocationId
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { b, c } by new
                       {
                           b.WhCode,
                           b.Location,
                           c.ClientCode,
                           b.HuId,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.Location,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           SoNumber = "",
                           CustomerPoNumber = "",
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();
            }
            if (sql.Count > 0)
            {
                foreach (var item in sql)
                {
                    CycleCountInventory inventory = new CycleCountInventory();
                    inventory.WhCode = entity.WhCode;
                    inventory.TaskNumber = entity.TaskNumber;
                    inventory.LocationId = item.LocationId;
                    inventory.ClientCode = item.ClientCode;

                    if (cycMaster.CompareStorageLocationHu == 1)
                    {
                        inventory.HuId = item.LocationId;
                    }
                    else
                    {
                        inventory.HuId = item.HuId;
                    }
                    //inventory.HuId = item.HuId;

                    inventory.SoNumber = item.SoNumber;
                    inventory.CustomerPoNumber = item.CustomerPoNumber;
                    inventory.AltItemNumber = item.AltItemNumber;
                    inventory.LotNumber1 = item.LotNumber1;
                    inventory.LotNumber2 = item.LotNumber2;
                    inventory.Qty = item.Qty;
                    inventory.HoldId = item.HoldId;
                    inventory.HoldReason = item.HoldReason;
                    inventory.Status = item.Status;
                    idal.ICycleCountInventoryDAL.Add(inventory);
                }
            }

        }

        #endregion

        #region 2.按POSKU盘点

        //盘点任务开始
        public string CycleCountInsertComplex(CycleCountInsertComplexAddPo entity)
        {
            List<CycleCountMaster> cycleMasterList = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber);
            if (cycleMasterList.Count == 0)
            {
                return "没有该盘点任务，请确认！";
            }

            CycleCountMaster cycleMaster = cycleMasterList.First();
            if (cycleMaster.Status == "C")
            {
                return "该盘点任务状态有误，请确认！";
            }

            if (idal.ICycleCountDetailDAL.SelectBy(u => u.TaskNumber == entity.TaskNumber && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId && u.Status == "U").Count == 0)
            {
                return "当前盘点任务没有该库位或状态有误！";
            }
            else
            {
                if (cycleMaster.Status == "U")
                {
                    cycleMaster.Status = "A";
                    cycleMaster.UpdateUser = entity.CreateUser;
                    cycleMaster.UpdateDate = DateTime.Now;
                    idal.ICycleCountMasterDAL.UpdateBy(cycleMaster, u => u.Id == cycleMaster.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }
                CycleCountDetail detail = new CycleCountDetail();
                detail.Status = "C";
                detail.CheckUser = entity.CreateUser;
                detail.CheckDate = DateTime.Now;
                idal.ICycleCountDetailDAL.UpdateBy(detail, u => u.TaskNumber == entity.TaskNumber && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId, new string[] { "Status", "CheckUser", "CheckDate" });

                if (cycleMaster.Type == "B")    //如果是锁定模式
                {
                    B_CycleCountInsertComplex(entity);
                }
                else
                {
                    L_CycleCountInsertComplex(entity);
                }

                idal.SaveChanges();
                return "Y";
            }
        }


        //锁定模式下 盘点动作
        public void B_CycleCountInsertComplex(CycleCountInsertComplexAddPo entity)
        {
            CycleCountCheckAdd(entity);
        }

        //增加实际盘点结果
        private void CycleCountCheckAdd(CycleCountInsertComplexAddPo entity)
        {
            foreach (var item in entity.HuIdModelAddPo)
            {
                CycleCountCheck cycleCheck = new CycleCountCheck();
                cycleCheck.WhCode = entity.WhCode;
                cycleCheck.TaskNumber = entity.TaskNumber;
                cycleCheck.LocationId = entity.LocationId;
                cycleCheck.HuId = item.HuId;

                if (item.PoModel != null)
                {
                    foreach (var item1 in item.PoModel)
                    {
                        cycleCheck.CustomerPoNumber = item1.CustomerPoNumber;
                        if (item1.HuDetailModel != null)
                        {
                            foreach (var item2 in item1.HuDetailModel)
                            {
                                cycleCheck.AltItemNumber = item2.AltItemNumber;
                                cycleCheck.Qty = item2.Qty;
                                idal.ICycleCountCheckDAL.Add(cycleCheck);
                                idal.SaveChanges();
                            }
                        }
                        else
                        {
                            idal.ICycleCountCheckDAL.Add(cycleCheck);
                            idal.SaveChanges();
                        }
                    }
                }
                else
                {
                    idal.ICycleCountCheckDAL.Add(cycleCheck);
                    idal.SaveChanges();
                }
            }
        }

        //非锁定模式下 盘点动作
        public void L_CycleCountInsertComplex(CycleCountInsertComplexAddPo entity)
        {
            CycleCountCheckAdd(entity);

            CycleCountMaster cycMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TaskNumber == entity.TaskNumber).First();

            List<CycleCountInventoryResult> sql = new List<CycleCountInventoryResult>();
            if (cycMaster.CreateType == 1)
            {
                sql = (from b in idal.IHuMasterDAL.SelectAll()
                       where b.WhCode == entity.WhCode && b.Location == entity.LocationId
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { b, c } by new
                       {
                           b.WhCode,
                           b.Location,
                           c.ClientCode,
                           b.HuId,
                           c.CustomerPoNumber,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.Location,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           CustomerPoNumber = g.Key.CustomerPoNumber,
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();
            }
            else if (cycMaster.CreateType == 2)
            {
                sql = (from b in idal.IHuMasterDAL.SelectAll()
                       where b.WhCode == entity.WhCode && b.Location == entity.LocationId
                       join c in idal.IHuDetailDAL.SelectAll()
                             on new { b.WhCode, b.HuId }
                         equals new { c.WhCode, c.HuId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       group new { b, c } by new
                       {
                           b.WhCode,
                           b.Location,
                           c.ClientCode,
                           b.HuId,
                           c.AltItemNumber,
                           c.ItemId,
                           c.LotNumber1,
                           c.LotNumber2,
                           b.HoldId,
                           b.HoldReason,
                           b.Status
                       } into g
                       select new CycleCountInventoryResult
                       {
                           WhCode = g.Key.WhCode,
                           LocationId = g.Key.Location,
                           ClientCode = g.Key.ClientCode,
                           HuId = g.Key.HuId,
                           SoNumber = "",
                           CustomerPoNumber = "",
                           AltItemNumber = g.Key.AltItemNumber,
                           LotNumber1 = g.Key.LotNumber1,
                           LotNumber2 = g.Key.LotNumber2,
                           Qty = g.Sum(p => p.c.Qty),
                           HoldId = g.Key.HoldId,
                           HoldReason = g.Key.HoldReason,
                           Status = g.Key.Status
                       }).ToList();
            }
            if (sql.Count > 0)
            {
                foreach (var item in sql)
                {
                    CycleCountInventory inventory = new CycleCountInventory();
                    inventory.WhCode = entity.WhCode;
                    inventory.TaskNumber = entity.TaskNumber;
                    inventory.LocationId = item.LocationId;
                    inventory.ClientCode = item.ClientCode;

                    if (cycMaster.CompareStorageLocationHu == 1)
                    {
                        inventory.HuId = item.LocationId;
                    }
                    else
                    {
                        inventory.HuId = item.HuId;
                    }
                    //inventory.HuId = item.HuId;

                    inventory.SoNumber = item.SoNumber;
                    inventory.CustomerPoNumber = item.CustomerPoNumber;
                    inventory.AltItemNumber = item.AltItemNumber;
                    inventory.LotNumber1 = item.LotNumber1;
                    inventory.LotNumber2 = item.LotNumber2;
                    inventory.Qty = item.Qty;
                    inventory.HoldId = item.HoldId;
                    inventory.HoldReason = item.HoldReason;
                    inventory.Status = item.Status;
                    idal.ICycleCountInventoryDAL.Add(inventory);
                }
            }

        }

        #endregion


        //盘点任务结果列表
        public List<CycleCountCheckResult> CycleCountCheckList(CycleCountCheckSearch searchEntity, out int total)
        {
            //盘点库位数据
            List<CycleCountDetail> forResult = idal.ICycleCountDetailDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.TaskNumber == searchEntity.TaskNumber && u.LocationId == searchEntity.LocationId);

            //库存盘点数据
            List<CycleCountCheckResult> sql = (from a in idal.ICycleCountInventoryDAL.SelectAll()
                                               where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                                               select new CycleCountCheckResult
                                               {
                                                   WhCode = a.WhCode,
                                                   TaskNumber = a.TaskNumber,
                                                   LocationId = a.LocationId,
                                                   Inv_HuId = a.HuId,
                                                   Inv_CustomerPoNumber = a.CustomerPoNumber,
                                                   Inv_AltItemNumber = a.AltItemNumber,
                                                   Inv_LotNumber1 = a.LotNumber1,
                                                   Inv_LotNumber2 = a.LotNumber2,
                                                   Inv_Qty = ((Int32?)a.Qty ?? (Int32?)0)
                                               }).ToList();

            //实盘数据
            List<CycleCountCheckResult> sql1 = (from a in idal.ICycleCountCheckDAL.SelectAll()
                                                where a.WhCode == searchEntity.WhCode && a.TaskNumber == searchEntity.TaskNumber
                                                select new CycleCountCheckResult
                                                {
                                                    WhCode = a.WhCode,
                                                    TaskNumber = a.TaskNumber,
                                                    LocationId = a.LocationId,
                                                    Che_HuId = a.HuId,
                                                    Che_CustomerPoNumber = a.CustomerPoNumber,
                                                    Che_AltItemNumber = a.AltItemNumber,
                                                    Che_Qty = ((Int32?)a.Qty ?? (Int32?)0)
                                                }).ToList();

            List<CycleCountCheckResult> resultList = new List<CycleCountCheckResult>();

            //循环盘点库位明细
            foreach (var item in forResult)
            {
                List<CycleCountCheckResult> InvLocationResult = sql.Where(u => u.LocationId == item.LocationId).ToList();
                List<CycleCountCheckResult> CheckLocationResult = sql1.Where(u => u.LocationId == item.LocationId).ToList();
                //1.库存盘点与实盘 都为空 插入空行
                if (InvLocationResult.Count == 0 && CheckLocationResult.Count == 0)
                {
                    CycleCountCheckResult newentity = new CycleCountCheckResult();
                    newentity.TaskNumber = item.TaskNumber;
                    newentity.WhCode = item.WhCode;
                    newentity.LocationId = item.LocationId;
                    newentity.Inv_HuId = "";
                    newentity.Inv_CustomerPoNumber = "";
                    newentity.Inv_AltItemNumber = "";
                    newentity.Inv_LotNumber1 = "";
                    newentity.Inv_LotNumber2 = "";
                    newentity.Inv_Qty = 0;
                    newentity.Che_HuId = "";
                    newentity.Che_CustomerPoNumber = "";
                    newentity.Che_AltItemNumber = "";
                    newentity.Che_Qty = 0;
                    resultList.Add(newentity);
                }
                //2.库存盘点数大于等于实盘点数
                if (InvLocationResult.Count >= CheckLocationResult.Count)
                {
                    //2.1循环库存盘点
                    foreach (var item1 in InvLocationResult)
                    {
                        //2.2查询实盘的同一库位同一托盘
                        List<CycleCountCheckResult> countList = CheckLocationResult.Where(u => u.TaskNumber == item1.TaskNumber && u.WhCode == item1.WhCode && u.LocationId == item1.LocationId && u.Che_HuId == item1.Inv_HuId).ToList();
                        if (countList.Count > 0)
                        {
                            CycleCountCheckResult oldentity = countList.First();

                            CycleCountCheckResult newentity = new CycleCountCheckResult();
                            newentity.TaskNumber = item1.TaskNumber;
                            newentity.WhCode = item1.WhCode;
                            newentity.LocationId = item1.LocationId;
                            newentity.Inv_HuId = item1.Inv_HuId;
                            newentity.Inv_CustomerPoNumber = item1.Inv_CustomerPoNumber;
                            newentity.Inv_AltItemNumber = item1.Inv_AltItemNumber;
                            newentity.Inv_LotNumber1 = item1.Inv_LotNumber1;
                            newentity.Inv_LotNumber2 = item1.Inv_LotNumber2;
                            newentity.Inv_Qty = item1.Inv_Qty;

                            newentity.Che_HuId = oldentity.Che_HuId;
                            newentity.Che_CustomerPoNumber = oldentity.Che_CustomerPoNumber;
                            newentity.Che_AltItemNumber = oldentity.Che_AltItemNumber;
                            newentity.Che_Qty = oldentity.Che_Qty;

                            resultList.Add(newentity);
                            CheckLocationResult.Remove(oldentity);
                        }
                        else
                        {
                            CycleCountCheckResult newentity = new CycleCountCheckResult();
                            newentity.TaskNumber = item1.TaskNumber;
                            newentity.WhCode = item1.WhCode;
                            newentity.LocationId = item1.LocationId;
                            newentity.Inv_HuId = item1.Inv_HuId;
                            newentity.Inv_CustomerPoNumber = item1.Inv_CustomerPoNumber;
                            newentity.Inv_AltItemNumber = item1.Inv_AltItemNumber;
                            newentity.Inv_LotNumber1 = item1.Inv_LotNumber1;
                            newentity.Inv_LotNumber2 = item1.Inv_LotNumber2;
                            newentity.Inv_Qty = item1.Inv_Qty;

                            newentity.Che_HuId = "";
                            newentity.Che_CustomerPoNumber = "";
                            newentity.Che_AltItemNumber = "";
                            newentity.Che_Qty = 0;
                            resultList.Add(newentity);
                        }
                    }

                    foreach (var item2 in CheckLocationResult)
                    {
                        CycleCountCheckResult newentity = new CycleCountCheckResult();
                        newentity.TaskNumber = item2.TaskNumber;
                        newentity.WhCode = item2.WhCode;
                        newentity.LocationId = item2.LocationId;
                        newentity.Inv_HuId = "";
                        newentity.Inv_CustomerPoNumber = "";
                        newentity.Inv_AltItemNumber = "";
                        newentity.Inv_LotNumber1 = "";
                        newentity.Inv_LotNumber2 = "";
                        newentity.Inv_Qty = 0;
                        newentity.Che_HuId = item2.Che_HuId;
                        newentity.Che_CustomerPoNumber = item2.Che_CustomerPoNumber;
                        newentity.Che_AltItemNumber = item2.Che_AltItemNumber;
                        newentity.Che_Qty = item2.Che_Qty;
                        resultList.Add(newentity);
                    }
                }
                else
                {
                    //3.库存盘点数小于实盘点数
                    //3.1循环实盘
                    foreach (var item3 in CheckLocationResult)
                    {
                        //3.2查询库存盘点的同一库位同一托盘
                        List<CycleCountCheckResult> countList = InvLocationResult.Where(u => u.TaskNumber == item3.TaskNumber && u.WhCode == item3.WhCode && u.LocationId == item3.LocationId && u.Che_HuId == item3.Inv_HuId).ToList();
                        if (countList.Count > 0)
                        {
                            CycleCountCheckResult oldentity = countList.First();

                            CycleCountCheckResult newentity = new CycleCountCheckResult();
                            newentity.TaskNumber = item3.TaskNumber;
                            newentity.WhCode = item3.WhCode;
                            newentity.LocationId = item3.LocationId;
                            newentity.Inv_HuId = oldentity.Inv_HuId;
                            newentity.Inv_CustomerPoNumber = oldentity.Inv_CustomerPoNumber;
                            newentity.Inv_AltItemNumber = oldentity.Inv_AltItemNumber;
                            newentity.Inv_LotNumber1 = oldentity.Inv_LotNumber1;
                            newentity.Inv_LotNumber2 = oldentity.Inv_LotNumber2;
                            newentity.Inv_Qty = oldentity.Inv_Qty;

                            newentity.Che_HuId = item3.Che_HuId;
                            newentity.Che_CustomerPoNumber = item3.Che_CustomerPoNumber;
                            newentity.Che_AltItemNumber = item3.Che_AltItemNumber;
                            newentity.Che_Qty = item3.Che_Qty;

                            resultList.Add(newentity);
                            InvLocationResult.Remove(oldentity);
                        }
                        else
                        {
                            CycleCountCheckResult newentity = new CycleCountCheckResult();
                            newentity.TaskNumber = item3.TaskNumber;
                            newentity.WhCode = item3.WhCode;
                            newentity.LocationId = item3.LocationId;
                            newentity.Inv_HuId = "";
                            newentity.Inv_CustomerPoNumber = "";
                            newentity.Inv_AltItemNumber = "";
                            newentity.Inv_LotNumber1 = "";
                            newentity.Inv_LotNumber2 = "";
                            newentity.Inv_Qty = 0;

                            newentity.Che_HuId = item3.Che_HuId;
                            newentity.Che_CustomerPoNumber = item3.Che_CustomerPoNumber;
                            newentity.Che_AltItemNumber = item3.Che_AltItemNumber;
                            newentity.Che_Qty = item3.Che_Qty;
                            resultList.Add(newentity);
                        }
                    }

                    foreach (var item4 in InvLocationResult)
                    {
                        CycleCountCheckResult newentity = new CycleCountCheckResult();
                        newentity.TaskNumber = item4.TaskNumber;
                        newentity.WhCode = item4.WhCode;
                        newentity.LocationId = item4.LocationId;
                        newentity.Inv_HuId = item4.Inv_HuId;
                        newentity.Inv_CustomerPoNumber = item4.Inv_CustomerPoNumber;
                        newentity.Inv_AltItemNumber = item4.Inv_AltItemNumber;
                        newentity.Inv_LotNumber1 = item4.Inv_LotNumber1;
                        newentity.Inv_LotNumber2 = item4.Inv_LotNumber2;
                        newentity.Inv_Qty = item4.Inv_Qty;

                        newentity.Che_HuId = "";
                        newentity.Che_CustomerPoNumber = "";
                        newentity.Che_AltItemNumber = "";
                        newentity.Che_Qty = 0;
                        resultList.Add(newentity);
                    }
                }
            }

            total = resultList.Count();
            resultList = resultList.OrderBy(u => u.LocationId).ToList();
            resultList = resultList.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return resultList;
        }

        //完成盘点任务
        public string CycleCountComplete(string taskNumber, string whCode, string userName)
        {
            List<CycleCountMaster> cycleMasterList = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == whCode && u.TaskNumber == taskNumber);
            if (cycleMasterList.Count == 0)
            {
                return "没有该盘点任务，请确认！";
            }
            if (cycleMasterList.First().Status == "C")
            {
                return "该盘点任务状态有误，请确认！";
            }

            List<CycleCountDetail> cycleCountDetailList = idal.ICycleCountDetailDAL.SelectBy(u => u.TaskNumber == taskNumber && u.WhCode == whCode);
            if (cycleCountDetailList.Where(u => u.Status == "U").Count() > 0)
            {
                return "当前盘点任务未完成，请先盘点！";
            }

            //解冻盘点任务中的库位
            foreach (var item in cycleCountDetailList)
            {
                WhLocation whLocation = new WhLocation();
                whLocation.Status = "A";
                whLocation.UpdateUser = userName;
                idal.IWhLocationDAL.UpdateBy(whLocation, u => u.LocationId == item.LocationId && u.WhCode == item.WhCode, new string[] { "Status", "UpdateUser" });
            }

            CycleCountMaster cycleMaster = cycleMasterList.First();
            cycleMaster.Status = "C";
            cycleMaster.UpdateUser = userName;
            cycleMaster.UpdateDate = DateTime.Now;
            idal.ICycleCountMasterDAL.UpdateBy(cycleMaster, u => u.Id == cycleMaster.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

            idal.ICycleCountDetailDAL.SaveChanges();
            return "Y";
        }

        //提交盘点结果时 验证是否与库存盘点结果一致
        public string CheckCycleResult(CycleCountInsertComplexAddPo searchEntity)
        {
            CycleCountMaster cycMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.TaskNumber == searchEntity.TaskNumber).First();

            //组织实盘数据
            List<CycleCountDetailResult> checkListResult = new List<CycleCountDetailResult>();
            if (searchEntity.HuIdModelAddPo == null)
            {
                return "N";
            }
            foreach (var item in searchEntity.HuIdModelAddPo)
            {
                if (item.PoModel != null)
                {
                    foreach (var item1 in item.PoModel)
                    {
                        if (item1.HuDetailModel != null)
                        {
                            foreach (var item2 in item1.HuDetailModel)
                            {
                                CycleCountDetailResult cyc = new CycleCountDetailResult();
                                cyc.WhCode = searchEntity.WhCode;
                                cyc.TaskNumber = searchEntity.TaskNumber;
                                cyc.LocationId = searchEntity.LocationId;
                                cyc.HuId = item.HuId;

                                cyc.CustomerPoNumber = item1.CustomerPoNumber;
                                cyc.AltItemNumber = item2.AltItemNumber;
                                cyc.Qty = item2.Qty;
                                checkListResult.Add(cyc);
                            }
                        }
                    }
                }
            }

            return getCycleResult(cycMaster, checkListResult, searchEntity.LocationId);
        }

        //提交盘点结果时 验证是否与库存盘点结果一致
        public string CheckCycleResult(CycleCountInsertComplex searchEntity)
        {
            CycleCountMaster cycMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.TaskNumber == searchEntity.TaskNumber).First();

            //组织实盘数据
            List<CycleCountDetailResult> checkListResult = new List<CycleCountDetailResult>();

            if (searchEntity.HuIdModel == null)
            {
                return "N";
            }
            foreach (var item in searchEntity.HuIdModel)
            {
                CycleCountDetailResult cyc = new CycleCountDetailResult();
                cyc.WhCode = searchEntity.WhCode;
                cyc.TaskNumber = searchEntity.TaskNumber;
                cyc.LocationId = searchEntity.LocationId;
                cyc.HuId = item.HuId;
                cyc.AltItemNumber = item.AltItemNumber;
                cyc.Qty = item.Qty;
                checkListResult.Add(cyc);
            }

            return getCycleResult(cycMaster, checkListResult, searchEntity.LocationId);
        }


        //实盘与库存 比较是否一致 是返回Y 否返回N
        public string getCycleResult(CycleCountMaster cycMaster, List<CycleCountDetailResult> checkList, string locationId)
        {
            //得到盘点明细
            List<CycleCountDetail> detailList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                                 where a.WhCode == cycMaster.WhCode && a.TaskNumber == cycMaster.TaskNumber
                                                 && a.LocationId == locationId
                                                 select a).ToList();


            List<CycleCountDetailResult> inventoryList = new List<CycleCountDetailResult>();
            //如果当前是锁定模式
            if (cycMaster.Type == "B")
            {
                //如果是ByPoSku 结果比较需要POSKU对比 1是ByPoSku 2是BySku
                if (cycMaster.CreateType == 2)
                {
                    //得到库存盘点
                    inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                     join b in idal.ICycleCountInventoryDAL.SelectAll()
                                     on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                     from ab in temp1.DefaultIfEmpty()
                                     where a.WhCode == cycMaster.WhCode && a.TaskNumber == cycMaster.TaskNumber && a.LocationId == locationId
                                     group new { a, ab } by new
                                     {
                                         ab.WhCode,
                                         a.TaskNumber,
                                         a.Status,
                                         a.LocationId,
                                         ab.HuId,
                                         ab.AltItemNumber,
                                         a.CheckUser,
                                         a.CheckDate
                                     } into g
                                     select new CycleCountDetailResult
                                     {
                                         WhCode = g.Key.WhCode,
                                         TaskNumber = g.Key.TaskNumber,
                                         Status = g.Key.Status,
                                         LocationId = g.Key.LocationId,
                                         HuId = g.Key.HuId,
                                         Qty = g.Sum(p => p.ab.Qty),
                                         CustomerPoNumber = "",
                                         AltItemNumber = g.Key.AltItemNumber,
                                         CheckUser = g.Key.CheckUser,
                                         CheckDate = g.Key.CheckDate
                                     }).ToList();
                }
                else
                {
                    //得到库存盘点
                    inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                     join b in idal.ICycleCountInventoryDAL.SelectAll()
                                     on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                     from ab in temp1.DefaultIfEmpty()
                                     where a.WhCode == cycMaster.WhCode && a.TaskNumber == cycMaster.TaskNumber && a.LocationId == locationId
                                     select new CycleCountDetailResult
                                     {
                                         WhCode = ab.WhCode,
                                         Id = a.Id,
                                         TaskNumber = a.TaskNumber,
                                         Status = a.Status,
                                         LocationId = a.LocationId,
                                         HuId = ab.HuId ?? "",
                                         Qty = ab.Qty ?? 0,
                                         CustomerPoNumber = ab.CustomerPoNumber ?? "",
                                         AltItemNumber = ab.AltItemNumber ?? "",
                                         CheckUser = a.CheckUser,
                                         CheckDate = a.CheckDate
                                     }).ToList();
                }
            }
            else
            {
                //如果是非锁定模式
                //获取待新增的库存盘点

                //如果是ByPoSku 结果比较需要POSKU对比 1是ByPoSku 2是BySku
                if (cycMaster.CreateType == 2)
                {
                    inventoryList = (from b in idal.IHuMasterDAL.SelectAll()
                                     where b.WhCode == cycMaster.WhCode && b.Location == locationId
                                     join c in idal.IHuDetailDAL.SelectAll()
                                           on new { b.WhCode, b.HuId }
                                       equals new { c.WhCode, c.HuId } into c_join
                                     from c in c_join.DefaultIfEmpty()
                                     group new { b, c } by new
                                     {
                                         b.WhCode,
                                         b.Location,
                                         b.HuId,
                                         c.AltItemNumber
                                     } into g
                                     select new CycleCountDetailResult
                                     {
                                         WhCode = g.Key.WhCode,
                                         LocationId = g.Key.Location,
                                         HuId = g.Key.HuId,
                                         AltItemNumber = g.Key.AltItemNumber,
                                         Qty = g.Sum(p => p.c.Qty)
                                     }).ToList();
                }
                else
                {
                    inventoryList = (from b in idal.IHuMasterDAL.SelectAll()
                                     where b.WhCode == cycMaster.WhCode && b.Location == locationId
                                     join c in idal.IHuDetailDAL.SelectAll()
                                           on new { b.WhCode, b.HuId }
                                       equals new { c.WhCode, c.HuId } into c_join
                                     from c in c_join.DefaultIfEmpty()
                                     group new { b, c } by new
                                     {
                                         b.WhCode,
                                         b.Location,
                                         b.HuId,
                                         c.CustomerPoNumber,
                                         c.AltItemNumber
                                     } into g
                                     select new CycleCountDetailResult
                                     {
                                         WhCode = g.Key.WhCode,
                                         LocationId = g.Key.Location,
                                         HuId = g.Key.HuId,
                                         CustomerPoNumber = g.Key.CustomerPoNumber,
                                         AltItemNumber = g.Key.AltItemNumber,
                                         Qty = g.Sum(p => p.c.Qty)
                                     }).ToList();
                }
            }

            //最终显示结果
            List<CycleCountDetailResult> sql = new List<CycleCountDetailResult>();

            foreach (var item in detailList)
            {
                List<CycleCountDetailResult> inventoryListResult = inventoryList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                List<CycleCountDetailResult> checkListResult = checkList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                CycleCountDetailResult entity = new CycleCountDetailResult();
                if (inventoryListResult.Count == checkListResult.Count)
                {
                    int checkCount = 0;
                    foreach (var item1 in checkListResult)
                    {
                        //如果是ByPoSku 结果比较需要POSKU对比
                        if (cycMaster.CreateType == 1)
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.CustomerPoNumber == null ? "" : u.CustomerPoNumber) == (item1.CustomerPoNumber == null ? "" : item1.CustomerPoNumber) && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }
                        else if (cycMaster.CreateType == 2)  //如果是BySKU 结果比较需要SKU对比
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }

                    }

                    if (checkCount != checkListResult.Count)
                    {
                        entity.LocationId = item.LocationId;
                        entity.Action1 = "有差异";
                        sql.Add(entity);
                    }
                }
                else
                {
                    entity.LocationId = item.LocationId;
                    entity.Action1 = "有差异";
                    sql.Add(entity);
                }
            }

            if (sql.Count > 0)
            {
                return "N";
            }
            else
            {
                return "Y";
            }

        }


        //盘点任务差异EAN验证款号 
        public string CheckCycleResultSkuByEAN(string taskNumber, string whCode, string userName)
        {
            if (taskNumber == null || taskNumber == "")
            {
                return "盘点差异数据有误！";
            }

            //取得第一次生成任务的模式 及类型
            CycleCountMaster cycleMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == whCode && u.TaskNumber == taskNumber).First();

            List<CycleCountDetailResult> sql = CheckCycleResult(taskNumber, whCode, cycleMaster);

            if (sql.Count == 0)
            {
                return "当前盘点任务结果没有差异，无需EAN匹配款号！";
            }

            //得到盘点有差异的库位
            var s = (from a in sql select a.LocationId).Distinct();

            List<CycleCountInventory> inventoryList = idal.ICycleCountInventoryDAL.SelectBy(u => u.WhCode == whCode && u.TaskNumber == taskNumber && s.Contains(u.LocationId) && u.ClientCode != "").ToList();

            if (inventoryList.Count > 0)
            {
                string[] clientCode = (from a in inventoryList select a.ClientCode).Distinct().ToArray();

                //得到实际盘点有差异的明细
                List<CycleCountDetailResult> ss = (from a in idal.ICycleCountCheckDAL.SelectAll()
                                                   join b in idal.IItemMasterDAL.SelectAll()
                                                   on new { A = a.WhCode, B = a.AltItemNumber } equals new { A = b.WhCode, B = b.EAN }
                                                   where s.Contains(a.LocationId) && a.WhCode == whCode && a.TaskNumber == taskNumber && clientCode.Contains(b.ClientCode)
                                                   select new CycleCountDetailResult
                                                   {
                                                       Id = a.Id,
                                                       AltItemNumber = b.AltItemNumber,
                                                       CheckDate = b.CreateDate
                                                   }).OrderBy(u => u.CheckDate).ToList();
                foreach (var item in ss)
                {
                    idal.ICycleCountCheckDAL.UpdateByExtended(u => u.Id == item.Id, t => new CycleCountCheck { AltItemNumber = item.AltItemNumber });
                }
            }
            else
            {
                //得到实际盘点有差异的明细
                List<CycleCountDetailResult> ss = (from a in idal.ICycleCountCheckDAL.SelectAll()
                                                   join b in idal.IItemMasterDAL.SelectAll()
                                                   on new { A = a.WhCode, B = a.AltItemNumber } equals new { A = b.WhCode, B = b.EAN }
                                                   where s.Contains(a.LocationId) && a.WhCode == whCode && a.TaskNumber == taskNumber
                                                   select new CycleCountDetailResult
                                                   {
                                                       Id = a.Id,
                                                       AltItemNumber = b.AltItemNumber,
                                                       CheckDate = b.CreateDate
                                                   }).OrderBy(u => u.CheckDate).ToList();
                foreach (var item in ss)
                {
                    idal.ICycleCountCheckDAL.UpdateByExtended(u => u.Id == item.Id, t => new CycleCountCheck { AltItemNumber = item.AltItemNumber });
                }
            }

            return "Y";
        }

        //盘点任务再次生成
        public string CycleCountMasterAddAgain(string taskNumber, string whCode, string userName)
        {
            if (taskNumber == null || taskNumber == "")
            {
                return "盘点差异数据有误！";
            }

            //取得第一次生成任务的模式 及类型
            CycleCountMaster cycleMaster = idal.ICycleCountMasterDAL.SelectBy(u => u.WhCode == whCode && u.TaskNumber == taskNumber).First();

            if (cycleMaster.Status != "C")
            {
                return "盘点任务未完成，无法再次创建！";
            }

            List<CycleCountDetailResult> sql = CheckCycleResult(taskNumber, whCode, cycleMaster);

            if (sql.Count == 0)
            {
                return "当前盘点任务结果没有差异，无需再次创建任务！";
            }

            string getTaskNumber = "PD" + DI.IDGenerator.NewId;

            //新增盘点任务
            CycleCountMaster master = new CycleCountMaster();
            master.TaskNumber = getTaskNumber;
            master.WhCode = whCode;
            master.Type = cycleMaster.Type;
            master.Description = cycleMaster.TaskNumber + "差异再次生成任务";
            master.CreateType = cycleMaster.CreateType;
            master.TypeDescription = cycleMaster.TypeDescription;
            master.OneByOneScanFlag = cycleMaster.OneByOneScanFlag;
            master.CompareStorageLocationHu = cycleMaster.CompareStorageLocationHu;
            master.LocationNullFlag = cycleMaster.LocationNullFlag;
            master.Status = "U";
            master.CreateUser = userName;
            master.CreateDate = DateTime.Now;
            idal.ICycleCountMasterDAL.Add(master);

            //添加盘点任务明细
            List<CycleCountDetail> CycleCountDetailList = new List<CycleCountDetail>();

            List<CycleCountInventory> cycInvlist = new List<CycleCountInventory>();

            foreach (var item in sql.Distinct())
            {
                CycleCountDetail detail = new CycleCountDetail();
                detail.WhCode = whCode;
                detail.TaskNumber = master.TaskNumber;
                detail.LocationId = item.LocationId;
                detail.Status = "U";
                CycleCountDetailList.Add(detail);

                if (master.Type == "B")     //如果是锁定模式
                {
                    WhLocation whLocation = new WhLocation();
                    whLocation.Status = "H";
                    whLocation.UpdateUser = userName;
                    idal.IWhLocationDAL.UpdateBy(whLocation, u => u.LocationId == item.LocationId && u.WhCode == whCode, new string[] { "Status", "UpdateUser" });

                    List<CycleCountInventoryResult> cycIResult = (from b in idal.IHuMasterDAL.SelectAll()
                                                                  where b.Location == item.LocationId && b.WhCode == whCode
                                                                  join c in idal.IHuDetailDAL.SelectAll()
                                                                        on new { b.WhCode, b.HuId }
                                                                    equals new { c.WhCode, c.HuId } into c_join
                                                                  from c in c_join.DefaultIfEmpty()
                                                                  group new { b, c } by new
                                                                  {
                                                                      b.WhCode,
                                                                      b.HuId,
                                                                      c.ClientCode,
                                                                      c.SoNumber,
                                                                      c.CustomerPoNumber,
                                                                      c.AltItemNumber,
                                                                      b.HoldId,
                                                                      b.HoldReason,
                                                                      b.Status
                                                                  } into g
                                                                  select new CycleCountInventoryResult
                                                                  {
                                                                      HuId = g.Key.HuId,
                                                                      ClientCode = g.Key.ClientCode,
                                                                      SoNumber = g.Key.SoNumber,
                                                                      CustomerPoNumber = g.Key.CustomerPoNumber,
                                                                      AltItemNumber = g.Key.AltItemNumber,
                                                                      Qty = g.Sum(p => p.c.Qty),
                                                                      HoldId = g.Key.HoldId,
                                                                      HoldReason = g.Key.HoldReason,
                                                                      Status = g.Key.Status
                                                                  }).ToList();

                    foreach (var item1 in cycIResult)
                    {
                        CycleCountInventory inventory = new CycleCountInventory();
                        inventory.WhCode = whCode;
                        inventory.TaskNumber = master.TaskNumber;
                        inventory.LocationId = item.LocationId;

                        if (master.CompareStorageLocationHu == 1)
                        {
                            inventory.HuId = item.LocationId;
                        }
                        else
                        {
                            inventory.HuId = item1.HuId ?? "";
                        }

                        // inventory.HuId = item1.HuId;

                        inventory.ClientCode = item1.ClientCode;
                        inventory.SoNumber = item1.SoNumber;
                        inventory.CustomerPoNumber = item1.CustomerPoNumber;
                        inventory.AltItemNumber = item1.AltItemNumber;
                        inventory.Qty = item1.Qty;
                        inventory.HoldId = item1.HoldId;
                        inventory.HoldReason = item1.HoldReason;
                        inventory.Status = item1.Status;
                        cycInvlist.Add(inventory);
                    }
                }
            }

            idal.ICycleCountDetailDAL.Add(CycleCountDetailList);
            idal.ICycleCountInventoryDAL.Add(cycInvlist);
            idal.SaveChanges();

            return "Y";
        }


        //盘点验证结果
        private List<CycleCountDetailResult> CheckCycleResult(string taskNumber, string whCode, CycleCountMaster cycleMaster)
        {
            List<CycleCountDetailResult> inventoryList = new List<CycleCountDetailResult>();

            List<CycleCountDetailResult> checkList = new List<CycleCountDetailResult>();

            if (cycleMaster.CreateType == 2)
            {
                //BySku盘点时 库存需要聚合同一托盘不同PO的数量
                inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                 join b in idal.ICycleCountInventoryDAL.SelectAll()
                                 on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                 from ab in temp1.DefaultIfEmpty()
                                 where a.WhCode == whCode && a.TaskNumber == taskNumber
                                 group new { a, ab } by new
                                 {
                                     ab.WhCode,
                                     a.TaskNumber,
                                     a.Status,
                                     a.LocationId,
                                     ab.HuId,
                                     ab.AltItemNumber,
                                     a.CheckUser,
                                     a.CheckDate
                                 } into g
                                 select new CycleCountDetailResult
                                 {
                                     WhCode = g.Key.WhCode,
                                     TaskNumber = g.Key.TaskNumber,
                                     Status = g.Key.Status,
                                     LocationId = g.Key.LocationId,
                                     HuId = g.Key.HuId,
                                     Qty = g.Sum(p => p.ab.Qty),
                                     CustomerPoNumber = "",
                                     AltItemNumber = g.Key.AltItemNumber,
                                     CheckUser = g.Key.CheckUser,
                                     CheckDate = g.Key.CheckDate
                                 }).ToList();

                checkList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                             join b in idal.ICycleCountCheckDAL.SelectAll()
                             on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                             from ab in temp1.DefaultIfEmpty()
                             where a.WhCode == whCode && a.TaskNumber == taskNumber
                             group new { a, ab } by new
                             {
                                 ab.WhCode,
                                 a.TaskNumber,
                                 a.Status,
                                 a.LocationId,
                                 ab.HuId,
                                 ab.AltItemNumber,
                                 a.CheckUser,
                                 a.CheckDate
                             } into g
                             select new CycleCountDetailResult
                             {
                                 WhCode = g.Key.WhCode,
                                 TaskNumber = g.Key.TaskNumber,
                                 Status = g.Key.Status,
                                 LocationId = g.Key.LocationId,
                                 HuId = g.Key.HuId,
                                 Qty = g.Sum(p => p.ab.Qty),
                                 CustomerPoNumber = "",
                                 AltItemNumber = g.Key.AltItemNumber,
                                 CheckUser = g.Key.CheckUser,
                                 CheckDate = g.Key.CheckDate
                             }).ToList();

            }
            else
            {
                //得到库存盘点
                inventoryList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                 join b in idal.ICycleCountInventoryDAL.SelectAll()
                                 on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                                 from ab in temp1.DefaultIfEmpty()
                                 where a.WhCode == whCode && a.TaskNumber == taskNumber
                                 select new CycleCountDetailResult
                                 {
                                     WhCode = ab.WhCode,
                                     Id = a.Id,
                                     TaskNumber = a.TaskNumber,
                                     Status = a.Status,
                                     LocationId = a.LocationId,
                                     HuId = ab.HuId,
                                     Qty = ab.Qty,
                                     CustomerPoNumber = ab.CustomerPoNumber,
                                     AltItemNumber = ab.AltItemNumber,
                                     CheckUser = a.CheckUser,
                                     CheckDate = a.CheckDate
                                 }).ToList();

                //得到实际盘点
                checkList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                             join b in idal.ICycleCountCheckDAL.SelectAll()
                             on new { A = a.WhCode, B = a.TaskNumber, C = a.LocationId } equals new { A = b.WhCode, B = b.TaskNumber, C = b.LocationId } into temp1
                             from ab in temp1.DefaultIfEmpty()
                             where a.WhCode == whCode && a.TaskNumber == taskNumber
                             select new CycleCountDetailResult
                             {
                                 WhCode = ab.WhCode,
                                 Id = a.Id,
                                 TaskNumber = a.TaskNumber,
                                 Status = a.Status,
                                 LocationId = a.LocationId,
                                 HuId = ab.HuId,
                                 Qty = ab.Qty,
                                 CustomerPoNumber = ab.CustomerPoNumber,
                                 AltItemNumber = ab.AltItemNumber,
                                 CheckUser = a.CheckUser,
                                 CheckDate = a.CheckDate
                             }).ToList();

            }


            //得到盘点明细
            List<CycleCountDetail> detailList = (from a in idal.ICycleCountDetailDAL.SelectAll()
                                                 where a.WhCode == whCode && a.TaskNumber == taskNumber
                                                 select a).ToList();

            //最终结果
            List<CycleCountDetailResult> sql = new List<CycleCountDetailResult>();

            foreach (var item in detailList)
            {
                List<CycleCountDetailResult> inventoryListResult = inventoryList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                List<CycleCountDetailResult> checkListResult = checkList.Where(u => u.WhCode == item.WhCode && u.TaskNumber == item.TaskNumber && u.LocationId == item.LocationId).ToList();

                CycleCountDetailResult entity = new CycleCountDetailResult();
                if (inventoryListResult.Count == checkListResult.Count)
                {
                    int checkCount = 0;
                    foreach (var item1 in checkListResult)
                    {
                        //如果是ByPoSku 结果比较需要POSKU对比
                        if (cycleMaster.CreateType == 1)
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.CustomerPoNumber == null ? "" : u.CustomerPoNumber) == (item1.CustomerPoNumber == null ? "" : item1.CustomerPoNumber) && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }
                        else if (cycleMaster.CreateType == 2)  //如果是BySKU 结果比较需要SKU对比
                        {
                            if (inventoryListResult.Where(u => u.HuId == item1.HuId && u.Qty == item1.Qty && (u.AltItemNumber == null ? "" : u.AltItemNumber) == (item1.AltItemNumber == null ? "" : item1.AltItemNumber)).Count() > 0)
                            {
                                checkCount++;
                            }
                        }

                    }

                    if (checkCount != checkListResult.Count)
                    {
                        entity.LocationId = item.LocationId;
                        sql.Add(entity);
                    }
                }
                else
                {
                    entity.LocationId = item.LocationId;
                    sql.Add(entity);
                }
            }

            return sql;
        }


        //创建盘点任务 按照客户和款号
        public string CycleCountMasterAddByClientCodeSku(CycleCountMasterInsert entity, string[] itemNumberList, string clientCode)
        {
            if (entity.Type == "" || itemNumberList == null || clientCode == "")
            {
                return "数据有误，请重新操作！";
            }
            lock (o)
            {
                entity.TaskNumber = "PD" + DI.IDGenerator.NewId;

                CycleCountMaster master = new CycleCountMaster();
                master.TaskNumber = entity.TaskNumber;
                master.WhCode = entity.WhCode;
                master.Type = entity.Type;
                master.Description = entity.Description;
                master.CreateType = entity.CreateType;
                master.LocationNullFlag = entity.LocationNullFlag;
                master.TypeDescription = entity.TypeDescription;
                master.Status = "U";
                master.CreateUser = entity.CreateUser;
                master.CreateDate = DateTime.Now;
                idal.ICycleCountMasterDAL.Add(master);

                //得到盘点库存明细
                List<CycleCountInventoryResult> cycIResult = (from b in idal.IHuMasterDAL.SelectAll()
                                                              join c in idal.IHuDetailDAL.SelectAll()
                                                                    on new { b.WhCode, b.HuId }
                                                                equals new { c.WhCode, c.HuId } into c_join
                                                              from c in c_join.DefaultIfEmpty()
                                                              where c.WhCode == entity.WhCode && c.ClientCode == clientCode && b.HuId.Substring(0, 2) != "LD"
                                                              group new { b, c } by new
                                                              {
                                                                  b.WhCode,
                                                                  b.HuId,
                                                                  c.ClientCode,
                                                                  c.SoNumber,
                                                                  c.CustomerPoNumber,
                                                                  c.AltItemNumber,
                                                                  b.HoldId,
                                                                  b.HoldReason,
                                                                  b.Status,
                                                                  b.Location
                                                              } into g
                                                              select new CycleCountInventoryResult
                                                              {
                                                                  WhCode = g.Key.WhCode,
                                                                  HuId = g.Key.HuId,
                                                                  ClientCode = g.Key.ClientCode,
                                                                  SoNumber = g.Key.SoNumber,
                                                                  CustomerPoNumber = g.Key.CustomerPoNumber,
                                                                  AltItemNumber = g.Key.AltItemNumber,
                                                                  Qty = g.Sum(p => p.c.Qty),
                                                                  HoldId = g.Key.HoldId,
                                                                  HoldReason = g.Key.HoldReason,
                                                                  Status = g.Key.Status,
                                                                  LocationId = g.Key.Location
                                                              }).ToList();

                cycIResult = cycIResult.Where(u => itemNumberList.Contains(u.AltItemNumber)).OrderBy(u => u.LocationId).ToList();

                if (cycIResult.Count == 0)
                {
                    return "款号没有对应库存，创建盘点失败！";
                }

                //得到盘点库位
                var sql = (from a in cycIResult select a.LocationId).Distinct();

                //添加盘点任务明细
                List<CycleCountDetail> CycleCountDetailList = new List<CycleCountDetail>();

                List<CycleCountInventory> cycInvlist = new List<CycleCountInventory>();

                foreach (var item in sql)
                {
                    CycleCountDetail detail = new CycleCountDetail();
                    detail.WhCode = entity.WhCode;
                    detail.TaskNumber = master.TaskNumber;
                    detail.LocationId = item;
                    detail.Status = "U";
                    CycleCountDetailList.Add(detail);

                    if (master.Type == "B")     //如果是锁定模式
                    {
                        WhLocation whLocation = new WhLocation();
                        whLocation.Status = "H";
                        whLocation.UpdateUser = entity.CreateUser;
                        idal.IWhLocationDAL.UpdateBy(whLocation, u => u.LocationId == item && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser" });

                        List<CycleCountInventoryResult> cycleCountInventoryList = new List<CycleCountInventoryResult>();
                        cycleCountInventoryList = cycIResult.Where(u => u.LocationId == item).ToList();

                        foreach (var item1 in cycleCountInventoryList)
                        {
                            CycleCountInventory inventory = new CycleCountInventory();
                            inventory.WhCode = entity.WhCode;
                            inventory.TaskNumber = master.TaskNumber;
                            inventory.LocationId = item;
                            inventory.ClientCode = item1.ClientCode;

                            if (master.CompareStorageLocationHu == 1)
                            {
                                inventory.HuId = item;
                            }
                            else
                            {
                                inventory.HuId = item1.HuId ?? "";
                            }

                            //inventory.HuId = item1.HuId;

                            inventory.SoNumber = item1.SoNumber;
                            inventory.CustomerPoNumber = item1.CustomerPoNumber;
                            inventory.AltItemNumber = item1.AltItemNumber;
                            inventory.Qty = item1.Qty;
                            inventory.HoldId = item1.HoldId;
                            inventory.HoldReason = item1.HoldReason;
                            inventory.Status = item1.Status;
                            cycInvlist.Add(inventory);
                        }
                    }
                }

                idal.ICycleCountDetailDAL.Add(CycleCountDetailList);
                idal.ICycleCountInventoryDAL.Add(cycInvlist);
                idal.SaveChanges();

                return "Y";
            }
        }


        //创建盘点任务 按照库位变更时间
        public string CycleCountMasterAddByLocationChangeTime(CycleCountMasterInsert entity, CycleCountMasterSeacrh searchEntity)
        {
            if (entity.Type == "" || searchEntity.BeginCreateDate == null || searchEntity.EndCreateDate == null)
            {
                return "数据有误，请重新操作！";
            }
            lock (o)
            {
                entity.TaskNumber = "PD" + DI.IDGenerator.NewId;

                CycleCountMaster master = new CycleCountMaster();
                master.TaskNumber = entity.TaskNumber;
                master.WhCode = entity.WhCode;
                master.Type = entity.Type;
                master.Description = entity.Description;
                master.CreateType = entity.CreateType;
                master.LocationNullFlag = entity.LocationNullFlag;
                master.TypeDescription = entity.TypeDescription;
                master.Status = "U";
                master.CreateUser = entity.CreateUser;
                master.CreateDate = DateTime.Now;
                idal.ICycleCountMasterDAL.Add(master);

                //得到盘点库存明细
                var sql3 = (from a in idal.IWhLocationDAL.SelectAll()
                            join b in idal.IHuMasterDAL.SelectAll()
                                  on new { a.WhCode, a.LocationId }
                              equals new { b.WhCode, LocationId = b.Location } into b_join
                            from b in b_join.DefaultIfEmpty()
                            join c in idal.IHuDetailDAL.SelectAll()
                                  on new { b.WhCode, b.HuId }
                              equals new { c.WhCode, c.HuId } into c_join
                            from c in c_join.DefaultIfEmpty()
                            where
                              a.WhCode == entity.WhCode && a.ChangeTime >= searchEntity.BeginCreateDate && a.ChangeTime < searchEntity.EndCreateDate
                            group new { a, b, c } by new
                            {
                                a.WhCode,
                                b.HuId,
                                c.ClientCode,
                                c.SoNumber,
                                c.CustomerPoNumber,
                                c.AltItemNumber,
                                b.HoldId,
                                b.HoldReason,
                                b.Status,
                                b.Location
                            } into g
                            select new CycleCountInventoryResult
                            {
                                WhCode = g.Key.WhCode,
                                HuId = g.Key.HuId,
                                ClientCode = g.Key.ClientCode,
                                SoNumber = g.Key.SoNumber,
                                CustomerPoNumber = g.Key.CustomerPoNumber,
                                AltItemNumber = g.Key.AltItemNumber,
                                Qty = (Int32?)g.Sum(p => p.c.Qty),
                                HoldId = (Int32?)g.Key.HoldId,
                                HoldReason = g.Key.HoldReason,
                                Status = g.Key.Status,
                                Location = g.Key.Location
                            }).OrderBy(u => u.Location);

                List<CycleCountInventoryResult> cycIResult = sql3.ToList();
                if (cycIResult.Count == 0)
                {
                    return "未找到动态库位信息，创建盘点失败！";
                }

                //得到盘点库位
                List<string> sql = (from a in cycIResult select a.Location).Distinct().ToList();

                //添加盘点任务明细
                List<CycleCountDetail> CycleCountDetailList = new List<CycleCountDetail>();

                List<CycleCountInventory> cycInvlist = new List<CycleCountInventory>();

                foreach (var item in sql)
                {
                    CycleCountDetail detail = new CycleCountDetail();
                    detail.WhCode = entity.WhCode;
                    detail.TaskNumber = master.TaskNumber;
                    detail.LocationId = item;
                    detail.Status = "U";
                    CycleCountDetailList.Add(detail);

                    if (master.Type == "B")     //如果是锁定模式
                    {
                        WhLocation whLocation = new WhLocation();
                        whLocation.Status = "H";
                        whLocation.UpdateUser = entity.CreateUser;
                        idal.IWhLocationDAL.UpdateBy(whLocation, u => u.LocationId == item && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser" });

                        List<CycleCountInventoryResult> cycleCountInventoryList = new List<CycleCountInventoryResult>();
                        cycleCountInventoryList = cycIResult.Where(u => u.LocationId == item).ToList();

                        foreach (var item1 in cycleCountInventoryList)
                        {
                            CycleCountInventory inventory = new CycleCountInventory();
                            inventory.WhCode = entity.WhCode;
                            inventory.TaskNumber = master.TaskNumber;
                            inventory.LocationId = item;
                            inventory.ClientCode = item1.ClientCode ?? "";

                            if (master.CompareStorageLocationHu == 1)
                            {
                                inventory.HuId = item;
                            }
                            else
                            {
                                inventory.HuId = item1.HuId ?? "";
                            }
                            //inventory.HuId = item1.HuId ?? "";

                            inventory.SoNumber = item1.SoNumber ?? "";
                            inventory.CustomerPoNumber = item1.CustomerPoNumber ?? "";
                            inventory.AltItemNumber = item1.AltItemNumber ?? "";
                            inventory.Qty = item1.Qty ?? 0;
                            inventory.HoldId = item1.HoldId;
                            inventory.HoldReason = item1.HoldReason ?? "";
                            inventory.Status = item1.Status ?? "";
                            cycInvlist.Add(inventory);
                        }
                    }
                }

                idal.ICycleCountDetailDAL.Add(CycleCountDetailList);
                idal.ICycleCountInventoryDAL.Add(cycInvlist);
                idal.SaveChanges();

                return "Y";
            }
        }

        //修改实际盘点结果
        public string EditCycleCheckDetail(CycleCountCheck entity, CycleCountCheck oldEntity, string userName)
        {
            //插入记录
            TranLog tl = new TranLog();
            tl.TranType = "71";
            tl.Description = "修改盘点实盘";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.LoadId = oldEntity.TaskNumber;
            tl.Location = oldEntity.LocationId;
            tl.WhCode = entity.WhCode;
            tl.HuId = oldEntity.HuId;
            tl.CustomerPoNumber = oldEntity.CustomerPoNumber;
            tl.AltItemNumber = oldEntity.AltItemNumber;
            tl.TranQty = oldEntity.Qty;
            tl.Remark = "修改为：" + entity.HuId + "-" + entity.CustomerPoNumber + "-" + entity.AltItemNumber + "-" + entity.Qty;
            idal.ITranLogDAL.Add(tl);

            idal.ICycleCountCheckDAL.UpdateBy(entity, u => u.TaskNumber == oldEntity.TaskNumber && u.WhCode == oldEntity.WhCode && u.LocationId == oldEntity.LocationId && u.HuId == oldEntity.HuId && (u.CustomerPoNumber ?? "") == (oldEntity.CustomerPoNumber == "null" ? "" : oldEntity.CustomerPoNumber == "" ? "" : oldEntity.CustomerPoNumber ?? "") && (u.AltItemNumber ?? "") == (oldEntity.AltItemNumber == "null" ? "" : oldEntity.AltItemNumber == "" ? "" : oldEntity.AltItemNumber ?? "") && (u.Qty ?? 0) == (oldEntity.Qty ?? 0), new string[] { "HuId", "CustomerPoNumber", "AltItemNumber", "Qty" });
            idal.ICycleCountCheckDAL.SaveChanges();
            return "Y";
        }

        //添加实际盘点结果
        public string AddCycleCheckDetail(List<CycleCountCheck> entity, string userName)
        {
            foreach (var item in entity)
            {
                TranLog tl = new TranLog();
                tl.TranType = "72";
                tl.Description = "新增盘点实盘";
                tl.TranDate = DateTime.Now;
                tl.TranUser = userName;
                tl.WhCode = item.WhCode;
                tl.HuId = item.HuId;
                tl.CustomerPoNumber = item.CustomerPoNumber;
                tl.AltItemNumber = item.AltItemNumber;
                tl.TranQty = item.Qty;
                tl.Remark = "任务号码：" + item.TaskNumber + "库位：" + item.LocationId;
                idal.ITranLogDAL.Add(tl);
            }
            idal.ICycleCountCheckDAL.Add(entity);
            idal.SaveChanges();
            return "Y";
        }

        //删除实际盘点结果
        public string DelCycleCheckDetail(CycleCountCheck entity, string userName)
        {
            TranLog tl = new TranLog();
            tl.TranType = "74";
            tl.Description = "删除盘点实盘";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = entity.WhCode;
            tl.HuId = entity.HuId;
            tl.CustomerPoNumber = entity.CustomerPoNumber;
            tl.AltItemNumber = entity.AltItemNumber;
            tl.TranQty = entity.Qty;
            tl.Remark = "任务号码：" + entity.TaskNumber + "库位：" + entity.LocationId;
            idal.ITranLogDAL.Add(tl);

            idal.ICycleCountCheckDAL.DeleteBy(u => u.TaskNumber == entity.TaskNumber && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId && u.HuId == entity.HuId && (u.CustomerPoNumber ?? "") == (entity.CustomerPoNumber ?? "") && (u.AltItemNumber ?? "") == (entity.AltItemNumber ?? "") && (u.Qty ?? 0) == (entity.Qty ?? 0));
            idal.SaveChanges();
            return "Y";
        }

        #endregion


        #region 8.款号单位管理

        //托盘列表
        //对应UnitsController中的 List 方法
        public List<UnitsResult> UnitsList(UnitsSearch searchEntity, out int total)
        {
            var sql = from a in idal.IUnitDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll() on new { ItemId = a.ItemId } equals new { ItemId = b.Id } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new UnitsResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          UnitName = a.UnitName,
                          ItemNumber = a.ItemNumber,
                          AltItemNumber = b.AltItemNumber,
                          Proportion = a.Proportion,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            //if ((searchEntity.ClientId != 0))
            if (searchEntity.ClientId != null)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //款号单位查询(用在创建出库订单的时候选取使用)
        public List<ItemUnitResult> ItemUnitList(ItemUnitSearch searchEntity, out int total)
        {
            var sql = from a in idal.IItemMasterDAL.SelectAll()
                      join b in idal.IUnitDAL.SelectAll() on new { ItemId = a.Id } equals new { ItemId = b.ItemId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new ItemUnitResult
                      {
                          ItemId = a.Id,
                          WhCode = a.WhCode,
                          UnitName = b.UnitName,
                          ItemNumber = a.ItemNumber,
                          AltItemNumber = a.AltItemNumber,
                          Proportion = b.Proportion,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          Style1 = a.Style1,
                          Style2 = a.Style2,
                          Style3 = a.Style3,
                          UnitFlag = a.UnitFlag

                      };

            if (!string.IsNullOrEmpty(searchEntity.WhCode))
                sql = sql.Where(u => u.WhCode == searchEntity.WhCode);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (searchEntity.ClientId != null)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);

            total = sql.Count();
            sql = sql.OrderBy(u => u.ItemId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public int AddUnit(Unit unit)
        {
            int cfunit = idal.IUnitDAL.SelectBy(a => a.ItemId == unit.ItemId && (a.UnitName == unit.UnitName || a.Proportion == unit.Proportion)).Count();
            int Result = 0;
            if (cfunit == 0)
            {
                ItemMaster item = idal.IItemMasterDAL.SelectBy(a => a.Id == unit.ItemId).First();
                unit.ItemNumber = item.ItemNumber;
                unit.WhCode = item.WhCode;
                unit.ClientCode = item.ClientCode;
                unit.ClientId = item.ClientId;
                unit.CreateUser = item.CreateUser;
                unit.CreateDate = item.CreateDate;
                unit.UpdateUser = item.UpdateUser;
                unit.UpdateDate = item.UpdateDate;

                if (item.UnitFlag == 0)
                {
                    item.UnitFlag = 1;

                    idal.IItemMasterDAL.UpdateBy(item, a => a.Id == item.Id, new string[] { "UnitFlag" });
                    //idal.IItemMasterDAL.Update(item, new string[] { "UnitFlag" });

                }

                idal.IUnitDAL.Add(unit);
                Result = idal.IUnitDAL.SaveChanges();
                //Result = idal.SaveChanges();
            }

            return Result;
        }
        #endregion


        #region 9.RF流程管理

        //流程列表
        //对应ClientFlowController中的 List 方法
        public List<BusinessFlowGroupResult> ClientRFFlowList(BusinessFlowGroupSearch searchEntity, out int total)
        {
            var sql = from a in idal.IBusinessFlowGroupDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new BusinessFlowGroupResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName,
                          Remark = a.Remark,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加流程
        //对应ClientFlowController中的 AddClientFlow 方法
        public BusinessFlowGroup AddClientRFFlow(BusinessFlowGroup entity)
        {
            if (idal.IBusinessFlowGroupDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FlowName == entity.FlowName).ToList().Count == 0)
            {
                idal.IBusinessFlowGroupDAL.Add(entity);
                idal.IBusinessFlowGroupDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //流程修改
        //对应ClientFlowController中的 ClientFlowEdit 方法
        public int ClientRFFlowEdit(BusinessFlowGroup entity, params string[] modifiedProNames)
        {
            if (idal.IBusinessFlowGroupDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FlowName == entity.FlowName).ToList().Count == 0)
            {
                idal.IBusinessFlowGroupDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
                idal.IBusinessFlowGroupDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //流程明细列表
        //对应ClientFlowController中的 FlowRuleDetailList 方法
        public List<BusinessFlowHeadResult> RFFlowRuleDetailList(BusinessFlowHeadSearch searchEntity, out int total)
        {
            var sql = from a in idal.IRFFlowRuleDAL.SelectAll()
                      where a.FunctionFlag == 1
                      join b in (
                          (from a0 in idal.IBusinessFlowGroupDAL.SelectAll()
                           where a0.Id == searchEntity.BusinessFlowGroupId
                           join b1 in idal.IBusinessFlowHeadDAL.SelectAll() on new { Id = a0.Id } equals new { Id = b1.GroupId } into b1_join
                           from b1 in b1_join.DefaultIfEmpty()
                           select new
                           {
                               FlowRuleId = b1.FlowRuleId
                           })) on new { Id = a.Id } equals new { Id = (Int32)b.FlowRuleId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new BusinessFlowHeadResult
                      {
                          Id = a.Id,
                          FunctionId = a.FunctionId,
                          FunctionName = a.FunctionName,
                          Description = a.Description,
                          SelectRuleDescription = a.SelectRuleDescription,
                          BusinessFlowHeadId = a.BusinessObjectHeadId,
                          FlowRuleId = b.FlowRuleId,
                          GroupId = a.GroupId
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.FunctionId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //添加流程配置
        //对应ClientFlowController中的 AddClientFlowDetail 方法
        public string AddClientFlowDetail(BusinessFlowHeadInsert entity)
        {
            var sql = idal.IRFFlowRuleDAL.SelectAll();        //得到整套流程配置
            string result = "";     //验证结果


            //1.      首先验证 所选功能流程是否满足必填
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
            foreach (var item1 in entity.FlowRuleModel)
            {
                listResult.Add(listCount, item1.FunctionName);
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
                        result = "流程名：" + sqlResult[i].ToString() + "为必选！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //2.    验证有流程组号的 是否勾选了其中一个
            var sql_group = sql.Where(p => sql.Where(u => u.RequiredFlag == 1 && u.FunctionFlag == 0).Select(x => x.FunctionId).Contains((int)p.GroupId));
            sqlResult.Clear();
            listResult.Clear();
            sqlCount = 0; listCount = 0;

            Hashtable sqlGroup = new Hashtable();
            foreach (var item in sql_group)
            {
                if (sqlGroup.ContainsValue(item.GroupId) == false)
                {
                    sqlGroup.Add(sqlCount, item.GroupId);
                    string sResult = "";
                    foreach (var item1 in sql_group.Where(u => u.GroupId == item.GroupId))
                    {
                        sResult += item1.FunctionName + "，";
                    }
                    sqlResult.Add(item.GroupId, sResult.Substring(0, sResult.Length - 1));
                    sqlCount++;
                }
            }
            //把客户所选流程的组号添加
            foreach (var item1 in entity.FlowRuleModel)
            {
                if (item1.GroupId != 0)
                {
                    listResult.Add(listCount, item1.GroupId);
                    listCount++;
                }
            }
            //循环数据库流程
            for (int i = 0; i < sqlGroup.Count; i++)
            {
                if (result == "")
                {
                    if (listResult.ContainsValue(sqlGroup[i]) == false)
                    {
                        result = "流程名：" + sqlResult[sqlGroup[i]].ToString() + "必须选择一个！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //3.       验证有组号的流程 只能选择一个
            var sql_groupCheck = sql.Where(p => sql.Where(u => u.FunctionFlag == 0).Select(x => x.FunctionId).Contains((int)p.GroupId));

            sqlResult.Clear();
            listResult.Clear();
            sqlGroup.Clear();
            sqlCount = 0; listCount = 0;

            foreach (var item in sql_groupCheck)
            {
                if (sqlGroup.ContainsValue(item.GroupId) == false)
                {
                    sqlGroup.Add(sqlCount, item.GroupId);
                    string sResult = "";
                    foreach (var item1 in sql_groupCheck.Where(u => u.GroupId == item.GroupId))
                    {
                        sResult += item1.FunctionName + "，";
                    }
                    sqlResult.Add(item.GroupId, sResult.Substring(0, sResult.Length - 1));
                    sqlCount++;
                }
            }
            foreach (var item1 in entity.FlowRuleModel)
            {
                if (item1.GroupId != 0)
                {
                    if (result == "")
                    {
                        if (listResult.ContainsValue(item1.GroupId))
                        {
                            result = "流程名：" + sqlResult[item1.GroupId].ToString() + "只能选择一个！";
                        }
                        else
                        {
                            listResult.Add(listCount, item1.GroupId);
                            listCount++;
                        }
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //4.      验证 依赖的流程是否选择
            //得到有依赖ID的流程
            var sql_Rely = from a in sql
                           where (a.RelyId ?? 0) != 0
                           select new RFFlowRuleResult
                           {
                               FunctionId = a.FunctionId,
                               FunctionName = a.FunctionName,
                               RelyId = a.RelyId
                           };

            sqlResult.Clear();
            listResult.Clear();

            //把数据库流程添加
            foreach (var item in sql_Rely.OrderBy(u => u.FunctionId))
            {
                sqlResult.Add(item.FunctionId, item.FunctionName);
            }

            //把客户所选流程添加
            foreach (var item1 in entity.FlowRuleModel)
            {
                RFFlowRule rfFlow = sql.Where(u => u.FunctionName == item1.FunctionName).First();
                listResult.Add(rfFlow.FunctionId, item1.FunctionName);
            }

            //循环数据库流程
            foreach (var item in sqlResult.Keys)
            {
                if (result == "")
                {
                    string s = sqlResult[item].ToString();

                    //是否选择了 带依赖的流程
                    if (listResult.ContainsValue(s) == true)
                    {
                        int functionid = Convert.ToInt32(item);

                        RFFlowRule getrf = sql.Where(u => u.FunctionId == functionid).First();

                        RFFlowRule getrely = sql.Where(u => u.FunctionId == getrf.RelyId).First();
                        string s1 = getrely.FunctionName;

                        //如果选择了带依赖的流程，同时验证 依赖的流程是否被选择
                        if (listResult.ContainsValue(s1) == false)
                        {
                            result = "如果选择流程：" + s + "，那么它的依赖流程：" + s1 + "必须选择！";
                        }

                    }
                }
            }

            if (result != "")
            {
                return result;
            }


            for (int i = 0; i < sqlResult.Count; i++)
            {
                if (result == "")
                {
                    //如果客户流程 选择了带有依赖流程的项
                    if (listResult.ContainsValue(sqlResult[i]) == true)
                    {
                        for (int j = 0; j < sqlGroup.Count; j++)
                        {
                            //就比较 客户流程中 是否同时选择了依赖流程
                            if (listResult.ContainsValue(sqlGroup[i]) == false)
                            {
                                result = "如果选择流程：" + sqlResult[i] + "，那么它的依赖流程：" + sqlGroup[i].ToString() + "必须选择！";
                            }
                        }
                    }
                }
            }
            if (result != "")
            {
                return result;
            }



            //5.      插入流程明细配置表

            //首先清空原配置
            idal.IBusinessFlowHeadDAL.DeleteBy(u => u.GroupId == entity.busGroupId);
            idal.IBusinessFlowHeadDAL.SaveChanges();
            int Count = 1;
            foreach (var item in entity.FlowRuleModel)
            {
                BusinessFlowHead busHead = new BusinessFlowHead();
                busHead.GroupId = (int)entity.busGroupId;
                busHead.ObjectHeadId = (int)item.BusinessFlowHeadId;
                busHead.ObjectBusinessName = item.FunctionName;
                busHead.OrderId = Count * 1000;
                busHead.FlowRuleId = item.Id;
                idal.IBusinessFlowHeadDAL.Add(busHead);
                Count++;
            }
            idal.IBusinessFlowHeadDAL.SaveChanges();

            return result;
        }

        #endregion


        #region 10.收出货等流程管理

        //流程列表
        public List<FlowHeadResult> ClientFlowRuleList(FlowHeadSearch searchEntity, out int total)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IFieldOrderByDAL.SelectAll()
                      on a.FieldOrderById equals b.Id into temp1
                      from b in temp1.DefaultIfEmpty()
                      join c in idal.ICRTemplateDAL.SelectAll()
                      on new { A = a.InTemplate, B = a.WhCode } equals new { A = c.TemplateName, B = c.WhCode } into temp2
                      from c in temp2.DefaultIfEmpty()
                      join d in idal.ICRTemplateDAL.SelectAll()
                      on new { A = a.OutTemplate, B = a.WhCode } equals new { A = d.TemplateName, B = d.WhCode } into temp3
                      from d in temp3.DefaultIfEmpty()
                      join e in idal.ICRTemplateDAL.SelectAll()
                      on new { A = a.PZTemplate, B = a.WhCode } equals new { A = e.TemplateName, B = e.WhCode } into temp4
                      from e in temp4.DefaultIfEmpty()
                      join f in idal.IUrlEdiDAL.SelectAll()
                      on a.UrlEdiId equals f.Id into temp5
                      from f in temp5.DefaultIfEmpty()
                      join g in idal.IUrlEdiDAL.SelectAll()
                      on a.UrlEdiId2 equals g.Id into temp6
                      from g in temp6.DefaultIfEmpty()
                      join h in idal.IUrlEdiDAL.SelectAll()
                      on a.UrlEdiId3 equals h.Id into temp7
                      from h in temp7.DefaultIfEmpty()
                      select new FlowHeadResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName,
                          Type = a.Type,
                          TypeName = a.Type == "InBound" ? "收货" : a.Type == "OutBound" ? "出货" : null,
                          InterceptFlag = a.InterceptFlag == 1 ? "按订单拦截" : a.InterceptFlag == 0 ? "不可拦截" : a.InterceptFlag == null ? "不可拦截" : a.InterceptFlag == 2 ? "按Load拦截" : null,
                          FieldOrderById = a.FieldOrderById,
                          FlowOrderBy = a.FlowOrderBy,
                          InTemplateShow = c.Description,
                          PZTemplateShow = e.Description,
                          OutTemplateShow = d.Description,
                          OrderByDescription = b.Description,
                          Remark = a.Remark,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate,
                          InTemplate = a.InTemplate,
                          PZTemplate = a.PZTemplate,
                          OutTemplate = a.OutTemplate,
                          UrlEdiId = a.UrlEdiId,
                          UrlEdiId2 = a.UrlEdiId2,
                          UrlEdiId3 = a.UrlEdiId3,
                          UrlNameShow = f.UrlName,
                          UrlNameShow2 = g.UrlName,
                          UrlNameShow3 = h.UrlName,
                          CheckAllHuWeightFlag = a.CheckAllHuWeightFlag ?? 0,
                          CheckAllHuWeightShow = a.CheckAllHuWeightFlag == 1 ? "是" : "否",
                      };

            if (!string.IsNullOrEmpty(searchEntity.FlowName))
                sql = sql.Where(u => u.FlowName.Contains(searchEntity.FlowName));
            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.TypeName == searchEntity.Type);

            sql = sql.Distinct();

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加流程
        public FlowHead AddClientFlowRule(FlowHead entity)
        {
            if (idal.IFlowHeadDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FlowName == entity.FlowName && u.Type == entity.Type).ToList().Count == 0)
            {
                idal.IFlowHeadDAL.Add(entity);
                idal.IFlowHeadDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //流程修改
        public int ClientFlowRuleEdit(FlowHead entity, params string[] modifiedProNames)
        {
            if (idal.IFlowHeadDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FlowName == entity.FlowName).ToList().Count == 0)
            {
                idal.IFlowHeadDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
                idal.IFlowHeadDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //流程明细列表
        public List<FlowDetailResult> FlowRuleDetailList(FlowDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.IFlowRuleDAL.SelectAll()
                      where a.FunctionFlag == 1
                      join b in (
                          (from a0 in idal.IFlowHeadDAL.SelectAll()
                           where a0.Id == searchEntity.FlowHeadId
                           join b1 in idal.IFlowDetailDAL.SelectAll()
                           on new { Id = a0.Id } equals new { Id = b1.FlowHeadId }
                           select new
                           {
                               b1.FlowRuleId,
                               FlowHeadId = b1.FlowHeadId
                           })) on new { Id = a.Id } equals new { Id = (Int32)b.FlowRuleId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new FlowDetailResult
                      {
                          FlowHeadId = b.FlowHeadId,
                          FlowRuleId = a.Id,
                          FunctionId = a.FunctionId,
                          FunctionName = a.FunctionName,
                          StatusId = a.StatusId,
                          StatusName = a.StatusName,
                          Description = a.Description,
                          GroupId = a.GroupId,
                          SelectRuleDescription = a.SelectRuleDescription,
                          BusinessObjectGroupId = a.BusinessObjectGroupId,
                          RollbackFlag = a.RollbackFlag,
                          RollbackFlagShow = a.RollbackFlag == 1 ? "是" : "否",
                          Type = a.Type,
                          Mark = a.Mark
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.FunctionId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加出货流程配置
        public string AddClientOutFlowDetail(FlowHeadInsert entity)
        {
            List<FlowRule> sql = idal.IFlowRuleDAL.SelectAll().OrderBy(u => u.FunctionId).ToList();        //得到整套流程配置
            string result = "";     //验证结果


            //1.      首先验证 所选功能流程是否满足必填
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
            foreach (var item1 in entity.FlowDetailModel)
            {
                listResult.Add(listCount, item1.FunctionName);
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
                        result = "流程名：" + sqlResult[i].ToString() + "为必选！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //2.    验证有流程组号的 是否勾选了其中一个
            var sql_group = sql.Where(p => sql.Where(u => u.RequiredFlag == 1 && u.FunctionFlag == 0).Select(x => x.FunctionId).Contains((int)p.GroupId));
            sqlResult.Clear();
            listResult.Clear();
            sqlCount = 0; listCount = 0;

            Hashtable sqlGroup = new Hashtable();
            foreach (var item in sql_group)
            {
                if (sqlGroup.ContainsValue(item.GroupId) == false)
                {
                    sqlGroup.Add(sqlCount, item.GroupId);
                    string sResult = "";
                    foreach (var item1 in sql_group.Where(u => u.GroupId == item.GroupId))
                    {
                        sResult += item1.FunctionName + "，";
                    }
                    sqlResult.Add(item.GroupId, sResult.Substring(0, sResult.Length - 1));
                    sqlCount++;
                }
            }
            //把客户所选流程的组号添加
            foreach (var item1 in entity.FlowDetailModel)
            {
                if (item1.GroupId != 0)
                {
                    listResult.Add(listCount, item1.GroupId);
                    listCount++;
                }
            }
            //循环数据库流程
            for (int i = 0; i < sqlGroup.Count; i++)
            {
                if (result == "")
                {
                    if (listResult.ContainsValue(sqlGroup[i]) == false)
                    {
                        result = "流程名：" + sqlResult[sqlGroup[i]].ToString() + "必须选择一个！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //3.       验证有组号的流程 只能选择一个
            var sql_groupCheck = sql.Where(p => sql.Where(u => u.FunctionFlag == 0).Select(x => x.FunctionId).Contains((int)p.GroupId));

            sqlResult.Clear();
            listResult.Clear();
            sqlGroup.Clear();
            sqlCount = 0; listCount = 0;

            foreach (var item in sql_groupCheck)
            {
                if (sqlGroup.ContainsValue(item.GroupId) == false)
                {
                    sqlGroup.Add(sqlCount, item.GroupId);
                    string sResult = "";
                    foreach (var item1 in sql_groupCheck.Where(u => u.GroupId == item.GroupId))
                    {
                        sResult += item1.FunctionName + "，";
                    }
                    sqlResult.Add(item.GroupId, sResult.Substring(0, sResult.Length - 1));
                    sqlCount++;
                }
            }
            foreach (var item1 in entity.FlowDetailModel)
            {
                if (item1.GroupId != 0)
                {
                    if (result == "")
                    {
                        if (listResult.ContainsValue(item1.GroupId))
                        {
                            result = "流程名：" + sqlResult[item1.GroupId].ToString() + "只能选择一个！";
                        }
                        else
                        {
                            listResult.Add(listCount, item1.GroupId);
                            listCount++;
                        }
                    }
                }
            }
            if (result != "")
            {
                return result;
            }


            //4.      验证 依赖的流程是否选择
            //得到有依赖ID的流程
            var sql_Rely = from a in sql
                           where a.RelyId != 0
                           select new RFFlowRuleResult
                           {
                               FunctionId = a.FunctionId,
                               FunctionName = a.FunctionName,
                               RelyId = a.RelyId
                           };
            var sql_Rely1 = from a in sql_Rely select a.RelyId;
            var sql_Rely2 = from a in sql
                            where sql_Rely1.Contains(a.FunctionId)
                            select new RFFlowRuleResult
                            {
                                FunctionId = a.FunctionId,
                                FunctionName = a.FunctionName,
                                RelyId = a.RelyId
                            };
            sqlResult.Clear();
            listResult.Clear();
            sqlGroup.Clear();
            sqlCount = 0; listCount = 0;
            int relyCount = 0;
            //把数据库流程添加
            foreach (var item in sql_Rely)
            {
                sqlResult.Add(sqlCount, item.FunctionName);
                sqlCount++;
            }
            foreach (var item in sql_Rely2)
            {
                sqlGroup.Add(relyCount, item.FunctionName);
                relyCount++;
            }
            //把客户所选流程添加
            foreach (var item1 in entity.FlowDetailModel)
            {
                listResult.Add(listCount, item1.FunctionName);
                listCount++;
            }
            //循环数据库流程
            for (int i = 0; i < sqlResult.Count; i++)
            {
                if (result == "")
                {
                    //如果客户流程 选择了带有依赖流程的项
                    if (listResult.ContainsValue(sqlResult[i]) == true)
                    {
                        for (int j = 0; j < sqlGroup.Count; j++)
                        {
                            //就比较 客户流程中 是否同时选择了依赖流程
                            if (listResult.ContainsValue(sqlGroup[j]) == false)
                            {
                                result = "如果选择流程：" + sqlResult[i] + "，那么它的依赖流程：" + sqlGroup[j].ToString() + "必须选择！";
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


            //5.      插入流程明细配置表

            //首先清空原配置
            idal.IFlowDetailDAL.DeleteBy(u => u.FlowHeadId == entity.FlowHeadId);
            idal.IFlowDetailDAL.SaveChanges();
            int Count = 1;
            foreach (var item in entity.FlowDetailModel)
            {
                FlowDetail busHead = new FlowDetail();
                busHead.FlowHeadId = (int)entity.FlowHeadId;
                busHead.FlowRuleId = (int)item.FlowRuleId;
                busHead.FunctionName = item.FunctionName;
                busHead.BusinessObjectGroupId = item.BusinessObjectGroupId;
                busHead.OrderId = Count * 1000;
                busHead.RollbackFlag = item.RollbackFlag;
                busHead.StatusId = item.StatusId;
                busHead.StatusName = item.StatusName;
                busHead.Type = item.Type;
                busHead.Mark = item.Mark;
                idal.IFlowDetailDAL.Add(busHead);
                Count++;
            }
            idal.IBusinessFlowHeadDAL.SaveChanges();

            return result;
        }

        //根据当前客户查询出未选择的流程
        public List<BusinessFlowGroupResult> ClientFlowNameUnselected(BusinessFlowGroupSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.IFlowHeadDAL.SelectAll()
                       where a.WhCode == searchEntity.WhCode && a.Type == searchEntity.Type && a.Id == searchEntity.FlowHeadId
                       join b in idal.IFlowDetailDAL.SelectAll()
                        on a.Id equals b.FlowHeadId into b_join
                       from b in b_join.DefaultIfEmpty()
                       select b.BusinessObjectGroupId;

            var sql = from a in idal.IBusinessFlowGroupDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.WhCode == searchEntity.WhCode && a.Type == searchEntity.Type
                      select new BusinessFlowGroupResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName,
                          Remark = a.Remark
                      };

            if (!string.IsNullOrEmpty(searchEntity.FlowName) && searchEntity.FlowName != "null")
            {
                sql = sql.Where(u => u.FlowName == searchEntity.FlowName);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据当前客户查询出已选择的流程
        public List<BusinessFlowGroupResult> ClientFlowNameSelected(BusinessFlowGroupSearch searchEntity, out int total)
        {
            var sql = from a in idal.IFlowDetailDAL.SelectAll()
                      where a.FlowHeadId == searchEntity.FlowHeadId
                      join b in idal.IBusinessFlowGroupDAL.SelectAll()
                      on a.BusinessObjectGroupId equals b.Id into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.Type == searchEntity.Type
                      select new BusinessFlowGroupResult
                      {
                          Id = a.Id,
                          FlowName = b.FlowName,
                          Remark = b.Remark
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增收货流程对RF流程的关系
        public int AddClientFlowRuleDetail(List<FlowDetail> entity)
        {
            int flowHeadId = entity[0].FlowHeadId;
            idal.IFlowDetailDAL.DeleteBy(u => u.FlowHeadId == flowHeadId);
            foreach (var item in entity)
            {
                idal.IFlowDetailDAL.Add(item);
            }
            idal.IFlowDetailDAL.SaveChanges();
            return 1;
        }


        //报表字段排序下拉列表
        public IEnumerable<FieldOrderByResult> FieldOrderBySelect(string whCode)
        {
            var sql = from a in idal.IFieldOrderByDAL.SelectAll()
                      where a.WhCode == whCode
                      select new FieldOrderByResult
                      {
                          Id = a.Id,
                          Description = a.Description
                      };
            return sql.AsEnumerable();
        }

        //打印报表名称下拉列表
        public IEnumerable<CRTempResult> TempSelect(string type, string whCode)
        {
            var sql = from a in idal.ICRTemplateDAL.SelectAll()
                      where a.Type == type && a.WhCode == whCode
                      select new CRTempResult
                      {
                          TemplateName = a.TemplateName,
                          Description = a.Description
                      };
            return sql.AsEnumerable();
        }

        #endregion


        #region 11.Load生成规则管理


        public List<LoadCreateRuleResult> GetLoadCreateRuleList(LoadCreateRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadCreateRuleDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new LoadCreateRuleResult
                      {
                          Id = a.Id,
                          RuleName = a.RuleName,
                          Description = a.Description,
                          OrderQty = a.OrderQty,
                          Qty = a.Qty,
                          Status = a.Status == "Active" ? "启用" :
                          a.Status == "UnActive" ? "未启用" : null,
                          ShipMode = a.ShipMode,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };
            if (!string.IsNullOrEmpty(searchEntity.RuleName))
            {
                sql = sql.Where(u => u.RuleName == searchEntity.RuleName);
            }
            if (!string.IsNullOrEmpty(searchEntity.Status))
            {
                sql = sql.Where(u => u.Status == searchEntity.Status);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //新增生成规则
        public LoadCreateRule LoadCreateRuleAdd(LoadCreateRule entity)
        {
            if (idal.ILoadCreateRuleDAL.SelectBy(u => u.WhCode == entity.WhCode && u.RuleName == entity.RuleName).Count == 0)
            {
                entity.CreateDate = DateTime.Now;
                idal.ILoadCreateRuleDAL.Add(entity);
                idal.ILoadCreateRuleDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }


        //信息修改
        public int LoadCreateRuleEdit(LoadCreateRule entity)
        {
            if (idal.ILoadCreateRuleDAL.SelectBy(u => u.WhCode == entity.WhCode && u.RuleName == entity.RuleName).Count == 0)
            {
                entity.UpdateDate = DateTime.Now;
                idal.ILoadCreateRuleDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "RuleName", "Description", "Qty", "OrderQty", "Status", "UpdateUser", "UpdateDate" });
                idal.ILoadCreateRuleDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }


        public List<BusinessFlowGroupResult> LoadCreateFlowNameUnselected(LoadCreateRuleSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.ILoadCreateRuleDAL.SelectAll()
                       where a.Id == searchEntity.Id
                       join b in idal.IR_LoadRule_FlowHeadDAL.SelectAll()
                        on a.Id equals b.RuleId into b_join
                       from b in b_join.DefaultIfEmpty()
                       select b.FlowHeadId;

            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.WhCode == searchEntity.WhCode && a.Type == "OutBound"
                      select new BusinessFlowGroupResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName,
                          Remark = a.Remark
                      };

            if (!string.IsNullOrEmpty(searchEntity.FlowName) && searchEntity.FlowName != "null")
            {
                sql = sql.Where(u => u.FlowName.Contains(searchEntity.FlowName));
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public List<BusinessFlowGroupResult> LoadCreateFlowNameSelected(LoadCreateRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      join b in idal.IR_LoadRule_FlowHeadDAL.SelectAll()
                      on a.Id equals b.FlowHeadId into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.RuleId == searchEntity.Id
                      select new BusinessFlowGroupResult
                      {
                          Id = b.Id,
                          FlowName = a.FlowName,
                          Remark = a.Remark
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public int R_LoadRule_FlowHeadListAdd(List<R_LoadRule_FlowHead> entity)
        {
            foreach (var item in entity)
            {
                item.CreateDate = DateTime.Now;
                idal.IR_LoadRule_FlowHeadDAL.Add(item);
            }
            idal.IR_LoadRule_FlowHeadDAL.SaveChanges();
            return 1;
        }


        //自动生成Load规则名称下拉列表
        public IEnumerable<LoadCreateRule> LoadCreateRuleSelect(string whCode)
        {
            var sql = from a in idal.ILoadCreateRuleDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            return sql.AsEnumerable();
        }

        //自动批量生成Load
        public string BeginLoadCreate(LoadCreateRuleInsert entity)
        {
            lock (o)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        int createLoadCount = 0;     //系统自动生成出的Load总个数
                        int count = 0;      //单流程下 系统自动生成出的Load个数
                                            //int loadCount = 10;  //每次最多生成的Load限制个数

                        string whCode = entity.WhCode;
                        string clientCode = entity.ClientCode;
                        string userName = entity.UserName;
                        int loadCount = entity.LoadCount;   //每次最多生成的Load限制个数
                        int ruleId = entity.RuleId;         //当前所选的生成流程Id


                        //得到流程头
                        FlowHead flowHead = idal.IFlowHeadDAL.SelectBy(u => u.Id == entity.RuleId).First();
                        //得到流程明细
                        List<FlowDetail> flowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == flowHead.Id);

                        //得到自动生成规则的订单数上限
                        List<LoadCreateRule> loadRule = (from a in idal.ILoadCreateRuleDAL.SelectAll()
                                                         join b in idal.IR_LoadRule_FlowHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.RuleId }
                                                         join c in idal.IFlowHeadDAL.SelectAll() on new { FlowHeadId = (Int32)b.FlowHeadId } equals new { FlowHeadId = c.Id }
                                                         where c.Id == ruleId && a.Status == "Active"
                                                         select a).ToList();

                        if (loadRule.Count == 0)
                        {
                            return "未找到启用的Load生成规则，请检查！";
                        }
                        else
                        {
                            #region 优化自动创建Load

                            lock (o2)
                            {
                                foreach (var item in loadRule)
                                {
                                    if (count == loadCount)
                                    {
                                        break;
                                    }

                                    List<OutBoundOrderDetailResult> outOrderSumList = new List<OutBoundOrderDetailResult>();
                                    List<OutBoundOrderDetailResult> outOrderList = new List<OutBoundOrderDetailResult>();

                                    //----------------------单品订单生成-------------------
                                    if (flowHead.FlowName.Contains("单品"))
                                    {
                                        //----------------------单品订单生成-------------------

                                        //                        select a.Id,a.ClientCode,sum(b.Qty) Qty from OutBoundOrder a
                                        //                        left join OutBoundOrderDetail b on a.Id = b.OutBoundOrderId
                                        //                        inner join (
                                        //                        select b.AltItemNumber, b.WhCode, sum(Qty) Qty from OutBoundOrder a
                                        //                         inner join OutBoundOrderDetail b on a.Id = b.OutBoundOrderId
                                        //                        where a.StatusName = '已确认订单' and a.ProcessId = 14 and a.LoadFlag = 0 and a.WhCode = '02' and a.ClientCode = 'DM'
                                        //                        group by b.AltItemNumber, b.WhCode) 
                                        //c on b.AltItemNumber = c.AltItemNumber and b.WhCode = c.WhCode
                                        //                        where a.LoadFlag = 0 and a.StatusName = '已确认订单'  and a.ProcessId = 14 and a.WhCode = '02' and a.ClientCode = 'DM'
                                        //                         group by a.Id,a.ClientCode,c.Qty,c.AltItemNumber
                                        //                         order by a.ClientCode,c.Qty desc, c.AltItemNumber

                                        if (entity.BeginDate != null)
                                        {
                                            //有创建时间条件 有订单来源条件
                                            if (!string.IsNullOrEmpty(entity.OrderSource))
                                            {
                                                #region 
                                                var sql1 = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                            join b in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                            join c in (
                                                                (from a0 in idal.IOutBoundOrderDAL.SelectAll()
                                                                 join b1 in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = b1.OutBoundOrderId }
                                                                 where
                                                                      a0.WhCode == whCode &&
                                                                      a0.ClientCode == clientCode &&
                                                                      a0.StatusName == "已确认订单" &&
                                                                      a0.ProcessId == flowHead.Id &&
                                                                      a0.LoadFlag == 0
                                                                      && a0.OrderSource == entity.OrderSource
                                                                      && a0.CreateDate >= entity.BeginDate
                                                                      && a0.CreateDate < entity.EndDate
                                                                      && (a0.OrderType ?? "") == entity.OrderType
                                                                 group b1 by new
                                                                 {
                                                                     b1.AltItemNumber,
                                                                     b1.WhCode
                                                                 } into g
                                                                 select new
                                                                 {
                                                                     WhCode = g.Key.WhCode,
                                                                     AltItemNumber = g.Key.AltItemNumber,
                                                                     Qty = (Int32?)g.Sum(p => p.Qty)
                                                                 }))
                                                                 on new { b.AltItemNumber, b.WhCode } equals new { c.AltItemNumber, c.WhCode }
                                                            where
                                                                   a.WhCode == whCode &&
                                                                   a.ClientCode == clientCode &&
                                                                   a.LoadFlag == 0 &&
                                                                   a.StatusName == "已确认订单" &&
                                                                   a.ProcessId == flowHead.Id &&
                                                                   a.OrderSource == entity.OrderSource
                                                                   && a.CreateDate >= entity.BeginDate
                                                                   && a.CreateDate < entity.EndDate
                                                                   && (a.OrderType ?? "") == entity.OrderType
                                                            group new { a, c, b } by new
                                                            {
                                                                a.Id,
                                                                a.ClientCode,
                                                                c.Qty,
                                                                c.AltItemNumber,
                                                                a.CreateDate
                                                            } into g
                                                            orderby
                                                              g.Key.ClientCode,
                                                              g.Key.Qty descending,
                                                              g.Key.AltItemNumber,
                                                              g.Key.CreateDate
                                                            select new OutBoundOrderDetailResult
                                                            {
                                                                Id = g.Key.Id,
                                                                ClientCode = g.Key.ClientCode,
                                                                Qty = g.Sum(p => p.b.Qty)
                                                            });
                                                outOrderList = sql1.ToList();

                                                #endregion
                                            }
                                            else
                                            {
                                                //有创建日期条件 但没有订单来源条件
                                                #region
                                                var sql1 = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                            join b in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                            join c in (
                                                                (from a0 in idal.IOutBoundOrderDAL.SelectAll()
                                                                 join b1 in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = b1.OutBoundOrderId }
                                                                 where
                                                                      a0.WhCode == whCode &&
                                                                      a0.ClientCode == clientCode &&
                                                                      a0.StatusName == "已确认订单" &&
                                                                      a0.ProcessId == flowHead.Id &&
                                                                      a0.LoadFlag == 0
                                                                      && a0.CreateDate >= entity.BeginDate
                                                                      && a0.CreateDate < entity.EndDate
                                                                      && (a0.OrderType ?? "") == entity.OrderType
                                                                      && (a0.OrderSource ?? "") == ""
                                                                 group b1 by new
                                                                 {
                                                                     b1.AltItemNumber,
                                                                     b1.WhCode
                                                                 } into g
                                                                 select new
                                                                 {
                                                                     WhCode = g.Key.WhCode,
                                                                     AltItemNumber = g.Key.AltItemNumber,
                                                                     Qty = (Int32?)g.Sum(p => p.Qty)
                                                                 }))
                                                                 on new { b.AltItemNumber, b.WhCode } equals new { c.AltItemNumber, c.WhCode }
                                                            where
                                                                   a.WhCode == whCode &&
                                                                   a.ClientCode == clientCode &&
                                                                   a.LoadFlag == 0 &&
                                                                   a.StatusName == "已确认订单" &&
                                                                   a.ProcessId == flowHead.Id
                                                                   && a.CreateDate >= entity.BeginDate
                                                                   && a.CreateDate < entity.EndDate
                                                                   && (a.OrderType ?? "") == entity.OrderType
                                                                   && (a.OrderSource ?? "") == ""
                                                            group new { a, c, b } by new
                                                            {
                                                                a.Id,
                                                                a.ClientCode,
                                                                c.Qty,
                                                                c.AltItemNumber,
                                                                a.CreateDate
                                                            } into g
                                                            orderby
                                                              g.Key.ClientCode,
                                                              g.Key.Qty descending,
                                                              g.Key.AltItemNumber,
                                                              g.Key.CreateDate
                                                            select new OutBoundOrderDetailResult
                                                            {
                                                                Id = g.Key.Id,
                                                                ClientCode = g.Key.ClientCode,
                                                                Qty = g.Sum(p => p.b.Qty)
                                                            });
                                                outOrderList = sql1.ToList();
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            //没有创建时间条件 但有订单来源条件
                                            if (!string.IsNullOrEmpty(entity.OrderSource))
                                            {
                                                #region
                                                outOrderList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                                join b in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                                join c in (
                                                                    (from a0 in idal.IOutBoundOrderDAL.SelectAll()
                                                                     join b1 in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = b1.OutBoundOrderId }
                                                                     where
                                                                          a0.WhCode == whCode &&
                                                                          a0.ClientCode == clientCode &&
                                                                          a0.StatusName == "已确认订单" &&
                                                                          a0.ProcessId == flowHead.Id &&
                                                                          a0.LoadFlag == 0
                                                                          && a0.OrderSource == entity.OrderSource
                                                                          && (a0.OrderType ?? "") == entity.OrderType
                                                                     group b1 by new
                                                                     {
                                                                         b1.AltItemNumber,
                                                                         b1.WhCode
                                                                     } into g
                                                                     select new
                                                                     {
                                                                         WhCode = g.Key.WhCode,
                                                                         AltItemNumber = g.Key.AltItemNumber,
                                                                         Qty = (Int32?)g.Sum(p => p.Qty)
                                                                     }))
                                                                     on new { b.AltItemNumber, b.WhCode } equals new { c.AltItemNumber, c.WhCode }
                                                                where
                                                                       a.WhCode == whCode &&
                                                                       a.ClientCode == clientCode &&
                                                                       a.LoadFlag == 0 &&
                                                                       a.StatusName == "已确认订单" &&
                                                                       a.ProcessId == flowHead.Id
                                                                       && a.OrderSource == entity.OrderSource
                                                                       && (a.OrderType ?? "") == entity.OrderType
                                                                group new { a, c, b } by new
                                                                {
                                                                    a.Id,
                                                                    a.ClientCode,
                                                                    c.Qty,
                                                                    c.AltItemNumber,
                                                                    a.CreateDate
                                                                } into g
                                                                orderby
                                                                  g.Key.ClientCode,
                                                                  g.Key.Qty descending,
                                                                  g.Key.AltItemNumber,
                                                                  g.Key.CreateDate
                                                                select new OutBoundOrderDetailResult
                                                                {
                                                                    Id = g.Key.Id,
                                                                    ClientCode = g.Key.ClientCode,
                                                                    Qty = g.Sum(p => p.b.Qty)
                                                                }).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                //没有创建日期 没有订单来源
                                                #region                 

                                                var sql = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                           join b in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                           join c in (
                                                               (from a0 in idal.IOutBoundOrderDAL.SelectAll()
                                                                join b1 in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = b1.OutBoundOrderId }
                                                                where
                                                                     a0.WhCode == whCode &&
                                                                     a0.ClientCode == clientCode &&
                                                                     a0.StatusName == "已确认订单" &&
                                                                     a0.ProcessId == flowHead.Id &&
                                                                     a0.LoadFlag == 0
                                                                     && (a0.OrderType ?? "") == entity.OrderType
                                                                     && (a0.OrderSource ?? "") == ""
                                                                group b1 by new
                                                                {
                                                                    b1.AltItemNumber,
                                                                    b1.WhCode
                                                                } into g
                                                                select new
                                                                {
                                                                    WhCode = g.Key.WhCode,
                                                                    AltItemNumber = g.Key.AltItemNumber,
                                                                    Qty = (Int32?)g.Sum(p => p.Qty)
                                                                }))
                                                                on new { b.AltItemNumber, b.WhCode } equals new { c.AltItemNumber, c.WhCode }
                                                           where
                                                                  a.WhCode == whCode &&
                                                                  a.ClientCode == clientCode &&
                                                                  a.LoadFlag == 0 &&
                                                                  a.StatusName == "已确认订单" &&
                                                                  a.ProcessId == flowHead.Id
                                                                  && (a.OrderType ?? "") == entity.OrderType
                                                                  && (a.OrderSource ?? "") == ""
                                                           group new { a, c, b } by new
                                                           {
                                                               a.Id,
                                                               a.ClientCode,
                                                               c.Qty,
                                                               c.AltItemNumber,
                                                               a.CreateDate
                                                           } into g
                                                           orderby
                                                             g.Key.ClientCode,
                                                             g.Key.Qty descending,
                                                             g.Key.AltItemNumber,
                                                             g.Key.CreateDate
                                                           select new OutBoundOrderDetailResult
                                                           {
                                                               Id = g.Key.Id,
                                                               ClientCode = g.Key.ClientCode,
                                                               Qty = g.Sum(p => p.b.Qty)
                                                           });

                                                outOrderList = sql.ToList();
                                                #endregion
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //--------------------------多品订单生成------------------

                                        //                                select a.Id,a.ClientCode,sum(b.Qty) qty  from OutBoundOrder a
                                        //                                left join OutBoundOrderDetail b on a.Id = b.OutBoundOrderId
                                        //where a.ProcessId = '15' and a.LoadFlag = 0 and a.StatusName = '已确认订单'
                                        //group by a.Id,a.ClientCode

                                        //--------------------------多品订单生成------------------
                                        if (entity.BeginDate != null)
                                        {
                                            //有创建日期条件 有订单来源条件
                                            if (!string.IsNullOrEmpty(entity.OrderSource))
                                            {
                                                #region
                                                outOrderSumList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                                   join b in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                   on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                                   where
                                                                     a.WhCode == whCode &&
                                                                     a.ClientCode == clientCode &&
                                                                     a.ProcessId == flowHead.Id &&
                                                                     a.LoadFlag == 0 &&
                                                                     a.StatusName == "已确认订单" &&
                                                                     a.CreateDate >= entity.BeginDate &&
                                                                     a.CreateDate < entity.EndDate &&
                                                                     a.OrderSource == entity.OrderSource
                                                                     && (a.OrderType ?? "") == entity.OrderType
                                                                   group new { a, b } by new
                                                                   {
                                                                       a.Id,
                                                                       a.ClientCode
                                                                   } into g
                                                                   select new OutBoundOrderDetailResult
                                                                   {
                                                                       Id = g.Key.Id,
                                                                       ClientCode = g.Key.ClientCode,
                                                                       Qty = g.Sum(p => p.b.Qty),
                                                                       Sequence = 99999
                                                                   }).OrderBy(u => u.ClientCode).ThenBy(u => u.Qty).ThenBy(u => u.Id).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                //有创建日期条件 没有订单来源条件
                                                #region
                                                outOrderSumList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                                   join b in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                   on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                                   where
                                                                     a.WhCode == whCode &&
                                                                     a.ClientCode == clientCode &&
                                                                     a.ProcessId == flowHead.Id &&
                                                                     a.LoadFlag == 0 &&
                                                                     a.StatusName == "已确认订单" &&
                                                                     a.CreateDate >= entity.BeginDate &&
                                                                     a.CreateDate < entity.EndDate
                                                                     && (a.OrderType ?? "") == entity.OrderType
                                                                     && (a.OrderSource ?? "") == ""
                                                                   group new { a, b } by new
                                                                   {
                                                                       a.Id,
                                                                       a.ClientCode
                                                                   } into g
                                                                   select new OutBoundOrderDetailResult
                                                                   {
                                                                       Id = g.Key.Id,
                                                                       ClientCode = g.Key.ClientCode,
                                                                       Qty = g.Sum(p => p.b.Qty),
                                                                       Sequence = 99999
                                                                   }).OrderBy(u => u.ClientCode).ThenBy(u => u.Qty).ThenBy(u => u.Id).ToList();
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            //没有创建日期条件 有订单来源条件
                                            if (!string.IsNullOrEmpty(entity.OrderSource))
                                            {
                                                #region
                                                outOrderSumList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                                   join b in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                   on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                                   where
                                                                     a.WhCode == whCode &&
                                                                     a.ClientCode == clientCode &&
                                                                     a.ProcessId == flowHead.Id
                                                                     && a.LoadFlag == 0
                                                                     && a.StatusName == "已确认订单"
                                                                     && a.OrderSource == entity.OrderSource
                                                                     && (a.OrderType ?? "") == entity.OrderType
                                                                   group new { a, b } by new
                                                                   {
                                                                       a.Id,
                                                                       a.ClientCode
                                                                   } into g
                                                                   select new OutBoundOrderDetailResult
                                                                   {
                                                                       Id = g.Key.Id,
                                                                       ClientCode = g.Key.ClientCode,
                                                                       Qty = g.Sum(p => p.b.Qty),
                                                                       Sequence = 99999
                                                                   }).OrderBy(u => u.ClientCode).ThenBy(u => u.Qty).ThenBy(u => u.Id).ToList();
                                                #endregion
                                            }
                                            else
                                            {
                                                //没有创建日期条件 没有有订单来源条件
                                                #region
                                                outOrderSumList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                                   join b in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                   on new { Id = a.Id } equals new { Id = b.OutBoundOrderId }
                                                                   where
                                                                     a.WhCode == whCode &&
                                                                     a.ClientCode == clientCode &&
                                                                     a.ProcessId == flowHead.Id
                                                                     && a.LoadFlag == 0
                                                                     && a.StatusName == "已确认订单"
                                                                     && (a.OrderType ?? "") == entity.OrderType
                                                                     && (a.OrderSource ?? "") == ""
                                                                   group new { a, b } by new
                                                                   {
                                                                       a.Id,
                                                                       a.ClientCode
                                                                   } into g
                                                                   select new OutBoundOrderDetailResult
                                                                   {
                                                                       Id = g.Key.Id,
                                                                       ClientCode = g.Key.ClientCode,
                                                                       Qty = g.Sum(p => p.b.Qty),
                                                                       Sequence = 99999
                                                                   }).OrderBy(u => u.ClientCode).ThenBy(u => u.Qty).ThenBy(u => u.Id).ToList();
                                                #endregion
                                            }
                                        }

                                        if (outOrderSumList.Count == 0)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            if (flowHead.FlowName.Contains("多品"))
                                            {
                                                //多品订单排序 按照 订单2件-3件-4件及以上 排序，件数越多越往后
                                                #region 多品订单排序

                                                //1.如果后台字段有排序，先启用后台字段的排序
                                                int[] getAllOutBoundOrderIdArr = (from a in outOrderSumList
                                                                                  select a.Id).ToList().Distinct().ToArray();

                                                List<OutBoundOrder> getAllOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => getAllOutBoundOrderIdArr.Contains(u.Id));
                                                if (getAllOutBoundOrderList.Where(u => (u.OutOrderGroupId ?? 0) > 0).Count() > 0)
                                                {
                                                    foreach (var itemCheck in getAllOutBoundOrderList.Where(u => (u.OutOrderGroupId ?? 0) > 0))
                                                    {
                                                        List<OutBoundOrderDetailResult> getCheck = outOrderSumList.Where(u => u.Id == itemCheck.Id).ToList();
                                                        if (getCheck.Count > 0)
                                                        {
                                                            OutBoundOrderDetailResult oldfirstOutBoundOrder = getCheck.First();
                                                            outOrderSumList.Remove(oldfirstOutBoundOrder);

                                                            OutBoundOrderDetailResult firstOutBoundOrder = new OutBoundOrderDetailResult();
                                                            firstOutBoundOrder.Id = oldfirstOutBoundOrder.Id;
                                                            firstOutBoundOrder.ClientCode = oldfirstOutBoundOrder.ClientCode;
                                                            firstOutBoundOrder.Qty = oldfirstOutBoundOrder.Qty;
                                                            firstOutBoundOrder.Sequence = ((itemCheck.OutOrderGroupId ?? 0) == 0 ? 99999 : itemCheck.OutOrderGroupId);
                                                            outOrderSumList.Add(firstOutBoundOrder);
                                                        }
                                                    }

                                                    outOrderList = outOrderSumList.OrderBy(u => u.ClientCode).ThenBy(u => u.Sequence).ThenBy(u => u.Qty).ThenBy(u => u.Id).ToList();

                                                }
                                                else
                                                {
                                                    //2.启用系统设定的排序规则

                                                    //2.1 订单数量为2的开始排序
                                                    List<OutBoundOrderDetailResult> twoOrderQtyList = new List<OutBoundOrderDetailResult>();

                                                    List<OutBoundOrderDetailResult> outOrderQtyList1 = outOrderSumList.Where(u => u.Qty == 2).ToList();

                                                    List<OutBoundOrderDetailResult> twoOrderQtyList1 = new List<OutBoundOrderDetailResult>();

                                                    GetOrderGroup(twoOrderQtyList, outOrderQtyList1, twoOrderQtyList1);


                                                    //2.2 订单数量为3的开始排序
                                                    List<OutBoundOrderDetailResult> threeOrderQtyList = new List<OutBoundOrderDetailResult>();

                                                    List<OutBoundOrderDetailResult> outOrderQtyList11 = outOrderSumList.Where(u => u.Qty == 3).ToList();

                                                    List<OutBoundOrderDetailResult> threeOrderQtyList1 = new List<OutBoundOrderDetailResult>();

                                                    GetOrderGroup(threeOrderQtyList, outOrderQtyList11, threeOrderQtyList1);


                                                    //2.3 订单数量为4的开始排序
                                                    List<OutBoundOrderDetailResult> forOrderQtyList = new List<OutBoundOrderDetailResult>();

                                                    List<OutBoundOrderDetailResult> outOrderQtyList111 = outOrderSumList.Where(u => u.Qty == 4).ToList();

                                                    List<OutBoundOrderDetailResult> forOrderQtyList1 = new List<OutBoundOrderDetailResult>();

                                                    GetOrderGroup(forOrderQtyList, outOrderQtyList111, forOrderQtyList1);


                                                    //2.4 订单数量大于4的
                                                    List<OutBoundOrderDetailResult> fiveoutOrderQtyList = outOrderSumList.Where(u => u.Qty > 4).ToList();


                                                    //AB款号 出现频率在3次以上的 优先创建Load
                                                    outOrderList = twoOrderQtyList.Concat(threeOrderQtyList).Concat(forOrderQtyList).Concat(twoOrderQtyList1).Concat(threeOrderQtyList1).Concat(forOrderQtyList1).Concat(fiveoutOrderQtyList).ToList();

                                                }

                                                #endregion
                                            }
                                            else
                                            {
                                                //单品订单流程  默认按照客户、数量排序
                                                outOrderList = outOrderSumList;
                                            }
                                        }
                                    }
                                    //---------------单品 多品订单排序及累计结束-------------------------

                                    //string s = "";
                                    //foreach (var item111 in outOrderList)
                                    //{
                                    //    s += item111.Id + "GroupId:" + item111.Sequence + "$";
                                    //}

                                    //return "Y$生成成功";

                                    if (outOrderList.Count == 0)
                                    {
                                        continue;
                                    }

                                    int orderResult = 0;    //订单个数
                                    int result = 0;         //总件数
                                    int forResult = 0;      //循环次数

                                    List<OutBoundOrderDetailResult> resultList = new List<OutBoundOrderDetailResult>(); //Load明细结果

                                    List<OutBoundOrderDetailResult> checkresultList = new List<OutBoundOrderDetailResult>();


                                    //博士 边备边包流程 需要验证收件信息
                                    if (flowDetailList.Where(u => u.Type == "Picking" && u.Mark == "5").Count() > 0)
                                    {

                                        //订单上限数 不为0 表示 有订单限制
                                        if (item.OrderQty != 0)
                                        {
                                            if (item.Qty != 0)
                                            {
                                                #region  如果 个数达到上限 或 订单个数达到上限
                                                foreach (var outBoundOrder in outOrderList)
                                                {
                                                    if (count == loadCount)
                                                    {
                                                        break;
                                                    }

                                                    if (checkresultList.Where(u => u.Id == outBoundOrder.Id).Count() > 0)
                                                    {
                                                        continue;
                                                    }

                                                    orderResult++;  //订单个数累加
                                                    result = result + outBoundOrder.Qty;
                                                    resultList.Add(outBoundOrder);

                                                    //博士创建Load时 必须保证订单收件人、公司、地址 是否一致

                                                    //得到所有出库订单的ID
                                                    int[] outBoundIdArr = (from a in outOrderList
                                                                           select a.Id).Distinct().ToArray();

                                                    int[] resultOutBoundIdArr = (from a in resultList
                                                                                 select a.Id).Distinct().ToArray();

                                                    int[] checkresultOutBoundIdArr = (from a in checkresultList
                                                                                      select a.Id).Distinct().ToArray();

                                                    List<OutBoundOrder> checkAddressList = idal.IOutBoundOrderDAL.SelectBy(u => outBoundIdArr.Contains(u.Id) && !resultOutBoundIdArr.Contains(u.Id) && !checkresultOutBoundIdArr.Contains(u.Id) && u.LoadFlag == 0);

                                                    OutBoundOrderDetailResult getFirst = resultList.First();
                                                    OutBoundOrder getAddress = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == getFirst.Id).First();

                                                    if (checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).Count() == 0)
                                                    {
                                                        foreach (var result1 in resultList)
                                                        {
                                                            checkresultList.Add(result1);
                                                        }

                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数
                                                    }
                                                    else
                                                    {
                                                        List<OutBoundOrder> list1 = checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).ToList();

                                                        foreach (var item2 in list1)
                                                        {
                                                            OutBoundOrderDetailResult AddOutBoundOrderDetailResult = new OutBoundOrderDetailResult();
                                                            AddOutBoundOrderDetailResult.Id = item2.Id;

                                                            orderResult++;  //订单个数累加
                                                            result = result + outBoundOrder.Qty;
                                                            resultList.Add(AddOutBoundOrderDetailResult);

                                                            //如果 个数达到上限 或 订单个数达到上限
                                                            if (resultList.Count != 0 && (result + outBoundOrder.Qty > item.Qty || orderResult == item.OrderQty))
                                                            {
                                                                break;
                                                            }
                                                        }

                                                        foreach (var result1 in resultList)
                                                        {
                                                            checkresultList.Add(result1);
                                                        }

                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数
                                                    }

                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region  如果 订单个数达到上限
                                                foreach (var outBoundOrder in outOrderList)
                                                {
                                                    if (count == loadCount)
                                                    {
                                                        break;
                                                    }

                                                    if (checkresultList.Where(u => u.Id == outBoundOrder.Id).Count() > 0)
                                                    {
                                                        continue;
                                                    }

                                                    orderResult++;  //订单个数累加
                                                    resultList.Add(outBoundOrder);

                                                    //博士创建Load时 必须保证订单收件人、公司、地址 是否一致

                                                    //得到所有出库订单的ID
                                                    int[] outBoundIdArr = (from a in outOrderList
                                                                           select a.Id).Distinct().ToArray();

                                                    int[] resultOutBoundIdArr = (from a in resultList
                                                                                 select a.Id).Distinct().ToArray();

                                                    int[] checkresultOutBoundIdArr = (from a in checkresultList
                                                                                      select a.Id).Distinct().ToArray();

                                                    List<OutBoundOrder> checkAddressList = idal.IOutBoundOrderDAL.SelectBy(u => outBoundIdArr.Contains(u.Id) && !resultOutBoundIdArr.Contains(u.Id) && !checkresultOutBoundIdArr.Contains(u.Id) && u.LoadFlag == 0);

                                                    OutBoundOrderDetailResult getFirst = resultList.First();
                                                    OutBoundOrder getAddress = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == getFirst.Id).First();

                                                    if (checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).Count() == 0)
                                                    {
                                                        foreach (var result1 in resultList)
                                                        {
                                                            checkresultList.Add(result1);
                                                        }

                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数
                                                    }
                                                    else
                                                    {
                                                        List<OutBoundOrder> list1 = checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).ToList();

                                                        foreach (var item2 in list1)
                                                        {
                                                            OutBoundOrderDetailResult AddOutBoundOrderDetailResult = new OutBoundOrderDetailResult();
                                                            AddOutBoundOrderDetailResult.Id = item2.Id;

                                                            orderResult++;  //订单个数累加
                                                            resultList.Add(AddOutBoundOrderDetailResult);

                                                            //如果 个数达到上限 或 订单个数达到上限
                                                            if (orderResult == item.OrderQty)
                                                            {
                                                                break;
                                                            }
                                                        }

                                                        foreach (var result1 in resultList)
                                                        {
                                                            checkresultList.Add(result1);
                                                        }

                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数
                                                    }

                                                }
                                                #endregion
                                            }
                                        }
                                        else if (item.Qty != 0)
                                        {
                                            #region  如果 总个数达到上限 
                                            foreach (var outBoundOrder in outOrderList)
                                            {
                                                if (count == loadCount)
                                                {
                                                    break;
                                                }

                                                if (checkresultList.Where(u => u.Id == outBoundOrder.Id).Count() > 0)
                                                {
                                                    continue;
                                                }

                                                result = result + outBoundOrder.Qty;    //总个数累加
                                                resultList.Add(outBoundOrder);

                                                //博士创建Load时 必须保证订单收件人、公司、地址 是否一致

                                                //得到所有出库订单的ID
                                                int[] outBoundIdArr = (from a in outOrderList
                                                                       select a.Id).Distinct().ToArray();

                                                int[] resultOutBoundIdArr = (from a in resultList
                                                                             select a.Id).Distinct().ToArray();

                                                int[] checkresultOutBoundIdArr = (from a in checkresultList
                                                                                  select a.Id).Distinct().ToArray();

                                                List<OutBoundOrder> checkAddressList = idal.IOutBoundOrderDAL.SelectBy(u => outBoundIdArr.Contains(u.Id) && !resultOutBoundIdArr.Contains(u.Id) && !checkresultOutBoundIdArr.Contains(u.Id) && u.LoadFlag == 0);

                                                OutBoundOrderDetailResult getFirst = resultList.First();
                                                OutBoundOrder getAddress = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == getFirst.Id).First();

                                                if (checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).Count() == 0)
                                                {
                                                    foreach (var result1 in resultList)
                                                    {
                                                        checkresultList.Add(result1);
                                                    }

                                                    LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                    createLoadCount++;
                                                    count++;
                                                    resultList.Clear(); //清除List元素
                                                    orderResult = 0;
                                                    result = 0;         //重新计数
                                                }
                                                else
                                                {
                                                    List<OutBoundOrder> list1 = checkAddressList.Where(u => u.buy_name == getAddress.buy_name && u.buy_company == getAddress.buy_company && u.address == getAddress.address).ToList();

                                                    foreach (var item2 in list1)
                                                    {
                                                        OutBoundOrderDetailResult AddOutBoundOrderDetailResult = new OutBoundOrderDetailResult();
                                                        AddOutBoundOrderDetailResult.Id = item2.Id;

                                                        result = result + outBoundOrder.Qty;    //总个数累加
                                                        resultList.Add(AddOutBoundOrderDetailResult);

                                                        //如果 个数达到上限 或 订单个数达到上限
                                                        if (resultList.Count != 0 && result + outBoundOrder.Qty > item.Qty)
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    foreach (var result1 in resultList)
                                                    {
                                                        checkresultList.Add(result1);
                                                    }

                                                    LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                    createLoadCount++;
                                                    count++;
                                                    resultList.Clear(); //清除List元素
                                                    orderResult = 0;
                                                    result = 0;         //重新计数
                                                }

                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        //订单上限数 不为0 表示 有订单限制
                                        if (item.OrderQty != 0)
                                        {
                                            if (item.Qty != 0)
                                            {
                                                #region  如果 个数达到上限 或 订单个数达到上限
                                                foreach (var outBoundOrder in outOrderList)
                                                {
                                                    if (count == loadCount)
                                                    {
                                                        break;
                                                    }
                                                    if (resultList.Count > 0)
                                                    {
                                                        if (resultList.Where(u => u.ClientCode == outBoundOrder.ClientCode).Count() == 0)
                                                        {
                                                            LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                            createLoadCount++;
                                                            count++;
                                                            resultList.Clear(); //清除List元素
                                                            orderResult = 0;
                                                            result = 0;         //重新计数
                                                        }
                                                    }
                                                    //如果 个数达到上限 或 订单个数达到上限
                                                    if (resultList.Count != 0 && (result + outBoundOrder.Qty > item.Qty || orderResult == item.OrderQty))
                                                    {
                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数  
                                                    }

                                                    forResult++;
                                                    orderResult++;  //订单个数累加
                                                    result = result + outBoundOrder.Qty;
                                                    resultList.Add(outBoundOrder);

                                                    //如果当前订单为最后一个
                                                    if (forResult == outOrderList.Count)
                                                    {
                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                    }
                                                }
                                                #endregion
                                            }
                                            else
                                            {
                                                #region  如果 订单个数达到上限
                                                foreach (var outBoundOrder in outOrderList)
                                                {
                                                    if (count == loadCount)
                                                    {
                                                        break;
                                                    }
                                                    if (resultList.Count > 0)
                                                    {
                                                        if (resultList.Where(u => u.ClientCode == outBoundOrder.ClientCode).Count() == 0)
                                                        {
                                                            LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                            createLoadCount++;
                                                            count++;
                                                            resultList.Clear(); //清除List元素
                                                            orderResult = 0;
                                                            result = 0;         //重新计数
                                                        }
                                                    }
                                                    //如果 订单个数达到上限
                                                    if (orderResult == item.OrderQty)
                                                    {
                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                    }

                                                    forResult++;
                                                    orderResult++;  //订单个数累加
                                                    resultList.Add(outBoundOrder);

                                                    //如果当前订单为最后一个
                                                    if (forResult == outOrderList.Count)
                                                    {
                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                        else if (item.Qty != 0)
                                        {
                                            #region  如果 总个数达到上限 
                                            foreach (var outBoundOrder in outOrderList)
                                            {
                                                if (count == loadCount)
                                                {
                                                    break;
                                                }
                                                if (resultList.Count > 0)
                                                {
                                                    if (resultList.Where(u => u.ClientCode == outBoundOrder.ClientCode).Count() == 0)
                                                    {
                                                        LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                        createLoadCount++;
                                                        count++;
                                                        resultList.Clear(); //清除List元素
                                                        orderResult = 0;
                                                        result = 0;         //重新计数
                                                    }
                                                }
                                                //如果 总个数达到上限 
                                                if (resultList.Count != 0 && result + outBoundOrder.Qty > item.Qty)
                                                {
                                                    LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                    createLoadCount++;
                                                    count++;
                                                    resultList.Clear(); //清除List元素
                                                    result = 0;         //重新计数  
                                                }

                                                forResult++;
                                                result = result + outBoundOrder.Qty;
                                                resultList.Add(outBoundOrder);

                                                //如果当前订单为最后一个
                                                if (forResult == outOrderList.Count)
                                                {
                                                    LoadCreateAdd(item, flowHead, flowDetailList, resultList, userName, entity.OrderType, entity.OrderSource);
                                                    createLoadCount++;
                                                    count++;
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }

                            #endregion
                        }

                        if (createLoadCount == 0)
                        {
                            return "该客户没有可生成所选流程的订单！";
                        }
                        else
                        {
                            trans.Complete();
                            return "Y$生成成功！本次共生成出" + createLoadCount + "个Load！";
                        }
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "自动生成Load超时，请重新提交！";
                    }
                }
            }
        }

        private void GetOrderGroup(List<OutBoundOrderDetailResult> OrderGroupList, List<OutBoundOrderDetailResult> outOrderQtyList1, List<OutBoundOrderDetailResult> OrderGroupList1)
        {
            if (outOrderQtyList1.Count > 0)
            {
                //1.首先 根据订单ID得到其款号明细 并把款号累加成一列 
                //2.然后 根据款号得到排序 哪俩个款号出现次数最多
                //其中 逻辑多变，请仔细查看

                //得到订单ID
                int[] outBoundOrderIdArr = (from a in outOrderQtyList1
                                            select a.Id).ToList().Distinct().ToArray();
                //得到订单ID对应的款号明细
                List<OutBoundOrderDetail> getOutBoundOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => outBoundOrderIdArr.Contains(u.OutBoundOrderId));

                List<OutBoundOrderDetailResult> OrderQtyResultList = new List<OutBoundOrderDetailResult>();

                //得到款号的数量排序
                List<OutBoundOrderDetailResult> getCountList = new List<OutBoundOrderDetailResult>();

                //循环订单ID 会减少一些不必要的循环
                foreach (var itemOrderId in outBoundOrderIdArr)
                {
                    //得到多品订单的ID及款号信息
                    List<OutBoundOrderDetailResult> outOrderQtyList2 = outOrderQtyList1.Where(u => u.Id == itemOrderId).ToList();
                    List<OutBoundOrderDetail> getOutBoundOrderDetailList2 = getOutBoundOrderDetailList.Where(u => u.OutBoundOrderId == itemOrderId).ToList();

                    //万一 订单主表或订单明细表数据不一致，跳过异常订单
                    if (outOrderQtyList2.Count == 0 || getOutBoundOrderDetailList2.Count == 0)
                    {
                        continue;
                    }

                    OutBoundOrderDetailResult outOrder1 = new OutBoundOrderDetailResult();

                    //把订单和款号绑定起来，后面才好根据款号来排序
                    foreach (var itemOrder in getOutBoundOrderDetailList2.OrderBy(u => u.AltItemNumber))
                    {
                        outOrder1.Id = outOrderQtyList2.First().Id;
                        outOrder1.ClientCode = outOrderQtyList2.First().ClientCode;
                        outOrder1.Qty = outOrderQtyList2.First().Qty;
                        outOrder1.AltItemNumber += itemOrder.AltItemNumber + ",";
                    }

                    //把款号数量累加，看哪些款号出现次数最多
                    if (getCountList.Where(u => u.AltItemNumber == outOrder1.AltItemNumber).Count() == 0)
                    {
                        OutBoundOrderDetailResult getCount = new OutBoundOrderDetailResult();
                        getCount.Qty = 1;
                        getCount.AltItemNumber = outOrder1.AltItemNumber;
                        getCountList.Add(getCount);
                    }
                    else
                    {
                        OutBoundOrderDetailResult oldgetCount = getCountList.Where(u => u.AltItemNumber == outOrder1.AltItemNumber).First();

                        OutBoundOrderDetailResult getCount = new OutBoundOrderDetailResult();
                        getCount.Qty = oldgetCount.Qty + 1;
                        getCount.AltItemNumber = outOrder1.AltItemNumber;

                        getCountList.Remove(oldgetCount);
                        getCountList.Add(getCount);
                    }

                    OrderQtyResultList.Add(outOrder1);
                }

                int FCount = 1;
                //循环款号数量排序，把OrderQtyResultList 按照款号订单的数量多少 GroupBy
                foreach (var getCountItem in getCountList.Where(u => u.Qty >= 3).OrderByDescending(u => u.Qty).ThenBy(u => u.AltItemNumber))
                {
                    List<OutBoundOrderDetailResult> setResultList = OrderQtyResultList.Where(u => u.AltItemNumber == getCountItem.AltItemNumber).ToList();

                    foreach (var setResultitem in setResultList)
                    {
                        OutBoundOrderDetailResult setResultEntity = new OutBoundOrderDetailResult();
                        setResultEntity.Id = setResultitem.Id;
                        setResultEntity.ClientCode = setResultitem.ClientCode;
                        setResultEntity.Qty = setResultitem.Qty;
                        setResultEntity.Sequence = FCount;

                        OrderGroupList.Add(setResultEntity);
                    }

                    FCount++;
                }

                int FCount1 = 1;
                //循环款号数量排序，把OrderQtyResultList 按照款号订单的数量多少 GroupBy
                foreach (var getCountItem in getCountList.Where(u => u.Qty < 3).OrderByDescending(u => u.Qty).ThenBy(u => u.AltItemNumber))
                {
                    List<OutBoundOrderDetailResult> setResultList = OrderQtyResultList.Where(u => u.AltItemNumber == getCountItem.AltItemNumber).ToList();

                    foreach (var setResultitem in setResultList)
                    {
                        OutBoundOrderDetailResult setResultEntity = new OutBoundOrderDetailResult();
                        setResultEntity.Id = setResultitem.Id;
                        setResultEntity.ClientCode = setResultitem.ClientCode;
                        setResultEntity.Qty = setResultitem.Qty;
                        setResultEntity.Sequence = FCount1;

                        OrderGroupList1.Add(setResultEntity);
                    }

                    FCount1++;
                }

            }
        }

        private void LoadCreateAdd(LoadCreateRule item, FlowHead flowHead, List<FlowDetail> flowDetailList, List<OutBoundOrderDetailResult> resultList, string userName, string orderType, string orderSource)
        {
            //新增Load
            LoadMaster loadMaster = new LoadMaster();
            loadMaster.LoadId = "LD" + DI.IDGenerator.NewId;
            loadMaster.Status0 = "U";
            loadMaster.Status1 = "U";
            loadMaster.Status2 = "U";
            loadMaster.Status3 = "U";
            loadMaster.WhCode = item.WhCode;
            loadMaster.ShipMode = item.ShipMode;
            loadMaster.ProcessId = flowHead.Id;
            loadMaster.ProcessName = flowHead.FlowName;
            loadMaster.CreateUser = userName;
            loadMaster.CreateDate = DateTime.Now;
            loadMaster.Remark = orderSource + " 订单类型：" + orderType;
            idal.ILoadMasterDAL.Add(loadMaster);
            idal.ILoadMasterDAL.SaveChanges();

            FlowDetail flowRule = flowDetailList.Where(u => u.FlowHeadId == flowHead.Id && u.Type == "Create").First();

            //FlowDetail flowRule = (from a in idal.IFlowDetailDAL.SelectAll() where a.FlowHeadId == flowHead.Id && a.Type == "Create" select a).First();

            int[] outBoundOrderIdArr = (from a in resultList
                                        select a.Id).ToList().Distinct().ToArray();

            List<OutBoundOrder> getList1 = idal.IOutBoundOrderDAL.SelectBy(u => outBoundOrderIdArr.Contains(u.Id));

            List<OutBoundOrder> getList = getList1.Where(u => u.WhCode == item.WhCode && u.LoadFlag == 0 && u.StatusName == "已确认订单").ToList();

            List<LoadDetail> loadDetailList = new List<LoadDetail>();
            //新增Load明细
            foreach (var orderId in resultList)
            {
                List<OutBoundOrder> sql = getList.Where(u => u.Id == orderId.Id && u.StatusId != -10).ToList();

                if (sql.Count > 0)
                {
                    OutBoundOrder outBoundOrder = sql.First();
                    outBoundOrder.LoadFlag = 1;
                    outBoundOrder.NowProcessId = flowRule.FlowRuleId;
                    outBoundOrder.StatusId = flowRule.StatusId;
                    outBoundOrder.StatusName = flowRule.StatusName;

                    if (loadDetailList.Where(u => u.LoadMasterId == loadMaster.Id && u.OutBoundOrderId == orderId.Id).Count() == 0)
                    {
                        LoadDetail loadDetail = new LoadDetail();
                        loadDetail.LoadMasterId = loadMaster.Id;
                        loadDetail.OutBoundOrderId = orderId.Id;
                        loadDetail.CreateUser = userName;
                        loadDetail.CreateDate = DateTime.Now;
                        loadDetailList.Add(loadDetail);
                    }
                }
            }

            idal.ILoadDetailDAL.Add(loadDetailList);

            idal.ILoadDetailDAL.SaveChanges();
        }


        //根据客户名、订单来源、创建时间 查询出货流程对应得订单数
        public List<LoadCreateRuleResult> GetOrderQtyList(LoadCreateRuleSearch searchEntity, out int total)
        {
            var sql = (from a in idal.ILoadCreateRuleDAL.SelectAll()
                       join b in idal.IR_LoadRule_FlowHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.RuleId }
                       join c in idal.IFlowHeadDAL.SelectAll() on new { FlowHeadId = (Int32)b.FlowHeadId } equals new { FlowHeadId = c.Id }
                       join d in idal.IOutBoundOrderDAL.SelectAll() on new { Id = c.Id } equals new { Id = d.ProcessId }
                       join e in idal.IWhClientDAL.SelectAll() on d.ClientId equals e.Id
                       where
                         a.WhCode == searchEntity.WhCode && a.Status == "Active" && d.StatusId == 10 && e.Status == "Active"
                       select new LoadCreateRuleResult
                       {
                           Id = c.Id,
                           RuleName = c.FlowName,
                           ClientCode = d.ClientCode,
                           OutPoNumber = d.OutPoNumber,
                           OrderSource = d.OrderSource ?? "",
                           OrderType = d.OrderType ?? "",
                           CreateDate = d.CreateDate
                       });

            if (searchEntity.BeginDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
            }
            if (searchEntity.EndDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndDate);
            }

            if (!string.IsNullOrEmpty(searchEntity.OrderSource))
            {
                sql = sql.Where(u => u.OrderSource == searchEntity.OrderSource);
            }
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            }

            List<LoadCreateRuleResult> list1 = sql.ToList();
            List<LoadCreateRuleResult> list = new List<LoadCreateRuleResult>();

            foreach (var item in list1)
            {
                if (list.Where(u => u.ClientCode == item.ClientCode && u.RuleName == item.RuleName && u.OrderSource == item.OrderSource && u.OrderType == item.OrderType).Count() == 0)
                {
                    item.OrderQty = 1;
                    list.Add(item);
                }
                else
                {
                    LoadCreateRuleResult first = list.Where(u => u.ClientCode == item.ClientCode && u.RuleName == item.RuleName && u.OrderSource == item.OrderSource && u.OrderType == item.OrderType).First();
                    list.Remove(first);

                    LoadCreateRuleResult newResult = first;
                    newResult.OrderQty = first.OrderQty + 1;
                    list.Add(newResult);
                }
            }

            total = list.Count;
            list = list.OrderBy(u => u.ClientCode).ThenByDescending(u => u.OrderQty).ThenBy(u => u.Id).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //出货订单类型下拉列表
        public IEnumerable<LoadCreateRuleInsert> OutBoundOrderSourceSelect(string whCode)
        {
            var sql = (from a in idal.IOutBoundOrderDAL.SelectAll()
                       where a.WhCode == whCode && (a.OrderSource ?? "") != ""
                       select new LoadCreateRuleInsert
                       {
                           OrderSource = a.OrderSource ?? "",
                       }).Distinct();

            return sql.AsEnumerable();
        }

        #endregion


        #region 12.补货任务管理

        //补货信息列表
        public List<R_Location_ItemResult> R_Location_ItemList(R_Location_ItemSearch searchEntity, out int total)
        {
            var sql = from a in idal.IR_Location_ItemDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new R_Location_ItemResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          LocationId = a.LocationId,
                          AltItemNumber = a.AltItemNumber,
                          UnitName = a.UnitName,
                          LotNumber1 = a.LotNumber1,
                          LotNumber2 = a.LotNumber2,
                          LotDate = a.LotDate,
                          MinQty = a.MinQty,
                          MaxQty = a.MaxQty,
                          Status = a.Status,
                          StatusShow = a.Status == "UnActive" ? "未启用" :
                           a.Status == "Active" ? "启用" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId == searchEntity.LocationId);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增补货库位信息
        public string R_Location_ItemAdd(List<R_Location_Item> entity)
        {
            lock (o)
            {
                var s = (from a in entity
                         select a.ClientCode).Distinct();

                string whCode = entity.First().WhCode;

                var sql = from a in idal.IWhClientDAL.SelectAll()
                          where a.WhCode == whCode && s.Contains(a.ClientCode)
                          select a;

                string mess = "";
                if (sql.Count() != s.Count())
                {
                    Hashtable sqlResult = new Hashtable();
                    Hashtable listResult = new Hashtable();
                    int count = 0;
                    int count1 = 0;
                    foreach (var item in s)
                    {
                        listResult.Add(count, item);
                        count++;
                    }
                    foreach (var item in sql)
                    {
                        sqlResult.Add(count1, item);
                        count1++;
                    }

                    for (int i = 0; i < listResult.Count; i++)
                    {
                        if (mess == "")
                        {
                            if (sqlResult.ContainsValue(listResult[i]) == false)
                            {
                                mess = listResult[i].ToString();
                            }
                        }
                    }

                }
                if (mess != "")
                {
                    return "该客户不存在，请检查：" + mess;
                }

                string result = "";

                List<R_Location_Item> PallateListAdd = new List<R_Location_Item>();

                List<ItemMaster> ItemMasterListAdd = new List<ItemMaster>();
                WhClient whClient = new WhClient();
                foreach (var item in entity)
                {
                    string ClientCode = item.ClientCode;
                    string AltItemNumber = item.AltItemNumber;
                    string LocationId = item.LocationId;

                    whClient = sql.Where(u => u.WhCode == whCode && u.ClientCode == ClientCode).First();

                    List<WhLocation> whLocationList = idal.IWhLocationDAL.SelectBy(u => u.LocationId == LocationId && u.WhCode == whCode && u.LocationTypeDetailId == 1);
                    if (whLocationList.Count == 0)
                    {
                        result = "捡货库位不存在，请确认：" + LocationId;
                        break;
                    }

                    List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == whCode && u.AltItemNumber == AltItemNumber && u.ClientId == whClient.Id).OrderBy(u => u.Id).ToList();
                    if (itemMasterList.Count == 0)
                    {
                        result = "款号不存在，请确认：" + AltItemNumber + ",客户名：" + ClientCode;
                        break;
                    }

                    ItemMaster itemMaster = itemMasterList.First();

                    item.ItemId = itemMaster.Id;
                    item.EAN = itemMaster.EAN;
                    item.UnitName = itemMaster.UnitName;
                    item.WhLocationId = whLocationList.First().Id;
                    item.CreateDate = DateTime.Now;
                    if (item.Status == null || item.Status == "")
                    {
                        item.Status = "Active";
                    }

                    PallateListAdd.Add(item);
                }
                if (result != "")
                {
                    return result;
                }

                foreach (var item in PallateListAdd)
                {
                    idal.IR_Location_ItemDAL.DeleteByExtended(u => u.ClientCode == item.ClientCode && u.WhLocationId == item.WhLocationId && u.WhCode == item.WhCode);
                }

                idal.IR_Location_ItemDAL.Add(PallateListAdd);
                idal.IR_Location_ItemDAL.SaveChanges();
                return "Y";
            }

        }

        public int R_Location_ItemEdit(R_Location_Item entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IR_Location_ItemDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "MinQty", "MaxQty", "Status", "UpdateUser", "UpdateDate" });
            idal.IR_Location_ItemDAL.SaveChanges();
            return 1;
        }


        //释放补货任务
        public string ReleaseSupplementTask(string whCode, string userName, int count, string[] altItemNumber)
        {
            if (whCode == "" || userName == "" || whCode == null || userName == null)
            {
                return "数据有误，请重新操作！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化释放补货机制

                    //1.首先查询补货库位款号表的数据 关联库存 得到 需要补货的库位、款号、补货数量等信息
                    var sql = from a in idal.IR_Location_ItemDAL.SelectAll()
                              join b in (
                                  (from a0 in idal.IHuMasterDAL.SelectAll()
                                   join b1 in idal.IHuDetailDAL.SelectAll()
                             on new { a0.WhCode, a0.HuId }
                         equals new { b1.WhCode, b1.HuId } into b1_join
                                   from b1 in b1_join.DefaultIfEmpty()
                                   group new { a0, b1 } by new
                                   {
                                       a0.WhCode,
                                       b1.ClientCode,
                                       a0.Location,
                                       b1.ItemId,
                                       b1.LotNumber1,
                                       b1.LotNumber2,
                                       b1.UnitName
                                   } into g
                                   select new
                                   {
                                       g.Key.WhCode,
                                       ClientCode = g.Key.ClientCode,
                                       g.Key.Location,
                                       ItemId = (Int32?)g.Key.ItemId,
                                       UnitName = g.Key.UnitName,
                                       LotNumber1 = g.Key.LotNumber1,
                                       LotNumber2 = g.Key.LotNumber2,
                                       qty = (System.Int32?)g.Sum(p => p.b1.Qty - ((Int32?)p.b1.PlanQty ?? (Int32?)0))
                                   }))
                                    on new { a.LocationId, a.WhCode, a.ClientCode, a.ItemId, Column1 = (a.LotNumber1 ?? ""), Column2 = (a.LotNumber2 ?? ""), Column3 = (a.UnitName ?? "") }
                                equals new { LocationId = b.Location, b.WhCode, b.ClientCode, b.ItemId, Column1 = (b.LotNumber1 ?? ""), Column2 = (b.LotNumber2 ?? ""), Column3 = (b.UnitName ?? "") } into b_join
                              from b in b_join.DefaultIfEmpty()
                              where
                                a.Status == "Active" && a.ClientCode != "Bosch" &&
                                (Int32)((System.Int32?)b.qty ?? (System.Int32?)0) < a.MinQty
                              select new R_Location_ItemResult
                              {
                                  WhCode = a.WhCode,
                                  ClientCode = a.ClientCode,
                                  LocationId = a.LocationId,
                                  ItemId = (Int32?)a.ItemId,
                                  AltItemNumber = a.AltItemNumber,
                                  EAN = a.EAN,
                                  UnitName = a.UnitName,
                                  LotNumber1 = a.LotNumber1,
                                  LotNumber2 = a.LotNumber2,
                                  LotDate = a.LotDate,
                                  MaxQty = a.MaxQty,
                                  MinQty = a.MinQty,
                                  InvQty = ((int?)b.qty ?? (int?)0)
                              };

                    if (altItemNumber != null)
                    {
                        sql = sql.Where(u => altItemNumber.Contains(u.AltItemNumber));
                    }

                    List<R_Location_ItemResult> Supplementlist = sql.ToList();

                    int i = 0;
                    int j = 0;
                    string supplementNumber = "SP" + DI.IDGenerator.NewId;

                    List<TranLog> tranLogList = new List<TranLog>();

                    List<SupplementTaskDetail> spDetilList = new List<SupplementTaskDetail>();

                    List<HuDetail> editHuDetailList = new List<HuDetail>();

                    List<HuDetailResult> setHuDetailList = new List<HuDetailResult>();

                    //通过需要补货的信息 创建补货任务 及补货明细
                    foreach (var item in Supplementlist)
                    {
                        //如果循环次数等于 补货库位
                        if (i >= count)
                        {
                            break;
                        }
                        if (j == 0)
                        {
                            //第一次时 创建补货任务头表
                            SupplementTask task = new SupplementTask();
                            task.WhCode = whCode;
                            task.SupplementNumber = supplementNumber;
                            task.Status = "U";
                            task.CreateUser = userName;
                            task.CreateDate = DateTime.Now;
                            idal.ISupplementTaskDAL.Add(task);
                        }
                        j++;
                        //1.首先验证 补货任务明细表中 补货库位是否已创建明细
                        List<SupplementTaskDetail> supplementTaskDetailList = idal.ISupplementTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.PutLocationId == item.LocationId && u.Status != "C");
                        if (supplementTaskDetailList.Count != 0)
                        {
                            continue;
                        }

                        int supQty = Convert.ToInt32(item.MaxQty - item.InvQty);  //得到补货数量

                        //2.查询补货款号的 库存数据
                        var sql2 = (from a in idal.IHuDetailDAL.SelectAll()
                                    join b in idal.IHuMasterDAL.SelectAll()
                                    on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                                    join c in idal.IWhLocationDAL.SelectAll()
                                     on new { b.Location, b.WhCode }
                                     equals new { Location = c.LocationId, c.WhCode }
                                    where a.WhCode == whCode && b.Location != item.LocationId
                                    && a.ClientCode == item.ClientCode && a.ItemId == item.ItemId
                                    && (a.UnitName == null ? "" : a.UnitName) == (item.UnitName == null ? "" : item.UnitName)
                                    && (a.LotNumber1 == null ? "" : a.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)
                                    && (a.Qty - (a.PlanQty == null ? 0 : a.PlanQty)) > 0
                                    && b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1
                                    && c.LocationTypeDetailId != 1
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
                                        LotNumber1 = a.LotNumber1 ?? "",
                                        LotNumber2 = a.LotNumber2 ?? "",
                                        LotDate = a.LotDate,
                                        Location = b.Location
                                    });

                        //if (altItemNumber != null)
                        //{
                        //    sql2 = sql2.Where(u => altItemNumber.Contains(u.AltItemNumber));
                        //}

                        List<HuDetailResult> huDetailList = sql2.OrderBy(u => u.ReceiptDate).ToList();

                        if (huDetailList.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            //2.1 插入补货任务明细表
                            foreach (var hudetail in huDetailList)
                            {
                                if (supQty == 0)
                                {
                                    break;
                                }
                                //循环至该托盘时 验证是否已释放补货任务 如果有释放,则应该取临时集中的数量来继续
                                if (setHuDetailList.Where(u => u.Id == hudetail.Id).Count() > 0)
                                {
                                    hudetail.Qty = setHuDetailList.Where(u => u.Id == hudetail.Id).First().Qty;
                                }
                                if (hudetail.Qty < 1)
                                {
                                    continue;
                                }

                                //插入补货任务明细
                                SupplementTaskDetail spDetail = new SupplementTaskDetail();
                                spDetail.WhCode = whCode;
                                spDetail.SupplementNumber = supplementNumber;
                                spDetail.HuDetailId = hudetail.Id;
                                spDetail.HuId = hudetail.HuId;
                                spDetail.LocationId = hudetail.Location;
                                spDetail.PutLocationId = item.LocationId;
                                spDetail.AltItemNumber = hudetail.AltItemNumber;
                                spDetail.ItemId = hudetail.ItemId;
                                spDetail.UnitName = hudetail.UnitName;
                                spDetail.UnitId = hudetail.UnitId;
                                spDetail.EAN = item.EAN;

                                //补货数量与库存数量比较
                                if (supQty >= (hudetail.Qty - (hudetail.PlanQty == null ? 0 : hudetail.PlanQty)))
                                {
                                    spDetail.Qty = (Int32)(hudetail.Qty - (hudetail.PlanQty == null ? 0 : hudetail.PlanQty));
                                    supQty = supQty - spDetail.Qty;
                                }
                                else
                                {
                                    spDetail.Qty = supQty;
                                    supQty = 0;
                                }

                                spDetail.LotNumber1 = hudetail.LotNumber1;
                                spDetail.LotNumber2 = hudetail.LotNumber2;
                                spDetail.LotDate = hudetail.LotDate;
                                spDetail.Status = "U";
                                spDetail.CreateUser = userName;
                                spDetail.CreateDate = DateTime.Now;
                                spDetilList.Add(spDetail);
                                i++;

                                //库存List中 如果托盘被释放过，数量应当扣除
                                //同时插入临时集中保存 下次循环至该托盘时 取临时集中的数量
                                hudetail.Qty = hudetail.Qty - spDetail.Qty;
                                if (setHuDetailList.Where(u => u.Id == hudetail.Id).Count() == 0)
                                {
                                    HuDetailResult newHu = new HuDetailResult();
                                    newHu.Id = hudetail.Id;
                                    newHu.HuId = hudetail.HuId;
                                    newHu.WhCode = hudetail.WhCode;
                                    newHu.ClientId = hudetail.ClientId;
                                    newHu.ClientCode = hudetail.ClientCode;
                                    newHu.SoNumber = hudetail.SoNumber;
                                    newHu.CustomerPoNumber = hudetail.CustomerPoNumber;
                                    newHu.AltItemNumber = hudetail.AltItemNumber;
                                    newHu.ReceiptDate = hudetail.ReceiptDate;
                                    newHu.PlanQty = hudetail.PlanQty ?? 0;
                                    newHu.Qty = hudetail.Qty;
                                    newHu.ItemId = hudetail.ItemId;
                                    newHu.UnitId = hudetail.UnitId;
                                    newHu.UnitName = hudetail.UnitName;
                                    newHu.Height = hudetail.Height;
                                    newHu.Length = hudetail.Length;
                                    newHu.Weight = hudetail.Weight;
                                    newHu.Width = hudetail.Width;
                                    newHu.LotNumber1 = hudetail.LotNumber1 ?? "";
                                    newHu.LotNumber2 = hudetail.LotNumber2 ?? "";
                                    newHu.LotDate = hudetail.LotDate;
                                    newHu.Location = hudetail.Location;
                                    setHuDetailList.Add(newHu);
                                }
                                else
                                {
                                    HuDetailResult oldHu = setHuDetailList.Where(u => u.Id == hudetail.Id).First();
                                    setHuDetailList.Remove(oldHu);

                                    HuDetailResult newHu = new HuDetailResult();
                                    newHu.Id = hudetail.Id;
                                    newHu.HuId = hudetail.HuId;
                                    newHu.WhCode = hudetail.WhCode;
                                    newHu.ClientId = hudetail.ClientId;
                                    newHu.ClientCode = hudetail.ClientCode;
                                    newHu.SoNumber = hudetail.SoNumber;
                                    newHu.CustomerPoNumber = hudetail.CustomerPoNumber;
                                    newHu.AltItemNumber = hudetail.AltItemNumber;
                                    newHu.ReceiptDate = hudetail.ReceiptDate;
                                    newHu.PlanQty = hudetail.PlanQty ?? 0;
                                    newHu.Qty = hudetail.Qty;
                                    newHu.ItemId = hudetail.ItemId;
                                    newHu.UnitId = hudetail.UnitId;
                                    newHu.UnitName = hudetail.UnitName;
                                    newHu.Height = hudetail.Height;
                                    newHu.Length = hudetail.Length;
                                    newHu.Weight = hudetail.Weight;
                                    newHu.Width = hudetail.Width;
                                    newHu.LotNumber1 = hudetail.LotNumber1 ?? "";
                                    newHu.LotNumber2 = hudetail.LotNumber2 ?? "";
                                    newHu.LotDate = hudetail.LotDate;
                                    newHu.Location = hudetail.Location;
                                    setHuDetailList.Add(newHu);
                                }

                                if (editHuDetailList.Where(u => u.Id == hudetail.Id).Count() == 0)
                                {
                                    HuDetail huDetail = new HuDetail();
                                    huDetail.Id = hudetail.Id;
                                    huDetail.PlanQty = hudetail.PlanQty + spDetail.Qty;
                                    editHuDetailList.Add(huDetail);
                                }
                                else
                                {
                                    HuDetail oldhuDetail = editHuDetailList.Where(u => u.Id == hudetail.Id).First();
                                    editHuDetailList.Remove(oldhuDetail);

                                    HuDetail newhuDetail = new HuDetail();
                                    newhuDetail.Id = hudetail.Id;
                                    newhuDetail.PlanQty = oldhuDetail.PlanQty + spDetail.Qty;
                                    editHuDetailList.Add(newhuDetail);
                                }

                            }
                        }
                    }

                    if (spDetilList.Count > 0)
                    {
                        if (editHuDetailList != null)
                        {
                            foreach (var item in editHuDetailList)
                            {
                                HuDetail huDetail1 = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                                TranLog tl = new TranLog();
                                tl.TranType = "33";
                                tl.Description = "释放补货锁定数量";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.ClientCode = huDetail1.ClientCode;
                                tl.SoNumber = huDetail1.SoNumber;
                                tl.CustomerPoNumber = huDetail1.CustomerPoNumber;
                                tl.AltItemNumber = huDetail1.AltItemNumber;
                                tl.ItemId = huDetail1.ItemId;
                                tl.UnitID = huDetail1.UnitId;
                                tl.UnitName = huDetail1.UnitName;
                                tl.ReceiptId = huDetail1.ReceiptId;
                                tl.ReceiptDate = huDetail1.ReceiptDate;
                                tl.TranQty = huDetail1.PlanQty;
                                tl.HuId = huDetail1.HuId;
                                tl.Length = huDetail1.Length;
                                tl.Width = huDetail1.Width;
                                tl.Height = huDetail1.Height;
                                tl.Weight = huDetail1.Weight;
                                tl.LotNumber1 = huDetail1.LotNumber1;
                                tl.LotNumber2 = huDetail1.LotNumber2;
                                tl.LotDate = huDetail1.LotDate;
                                tl.LoadId = supplementNumber;
                                tl.Remark = "锁定数量+" + (item.PlanQty - (huDetail1.PlanQty ?? 0));

                                HuDetail huDetail = new HuDetail();
                                huDetail.PlanQty = item.PlanQty;
                                huDetail.UpdateUser = userName;
                                huDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                tl.TranQty2 = huDetail.PlanQty;
                                tranLogList.Add(tl);
                            }
                        }

                        idal.ISupplementTaskDetailDAL.Add(spDetilList);
                        idal.ITranLogDAL.Add(tranLogList);
                    }
                    else
                    {
                        return "当前没有库位需要补货或需要补货的款号库存不足！";
                    }


                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "释放补货任务超时，请重新提交！";
                }
            }
        }


        //释放补货任务 博士定制
        public string BoschReleaseSupplementTask(string whCode, string userName, int count)
        {
            if (whCode == "" || userName == "" || whCode == null || userName == null)
            {
                return "数据有误，请重新操作！";
            }
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //1.首先查询补货库位款号表的数据 关联库存 得到 需要补货的库位、款号、补货数量等信息
                    var sql = from a in idal.IR_Location_ItemDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                               on new { a.LocationId, a.WhCode }
                                equals new { LocationId = b.Location, b.WhCode } into b_join
                              from b in b_join.DefaultIfEmpty()
                              where
                                a.Status == "Active" && a.ClientCode == "Bosch" && (b.HuId ?? "") == ""
                              select new R_Location_ItemResult
                              {
                                  WhCode = a.WhCode,
                                  ClientCode = a.ClientCode,
                                  LocationId = a.LocationId,
                                  ItemId = (Int32?)a.ItemId,
                                  AltItemNumber = a.AltItemNumber,
                                  EAN = a.EAN,
                                  UnitName = a.UnitName,
                                  LotNumber1 = a.LotNumber1,
                                  LotNumber2 = a.LotNumber2,
                                  LotDate = a.LotDate,
                                  MaxQty = a.MaxQty,
                                  MinQty = a.MinQty
                              };
                    List<R_Location_ItemResult> Supplementlist = sql.ToList();

                    int i = 0;
                    int j = 0;
                    string supplementNumber = "SP" + DI.IDGenerator.NewId;

                    List<TranLog> tranLogList = new List<TranLog>();

                    List<SupplementTaskDetail> spDetilList = new List<SupplementTaskDetail>();

                    List<HuDetail> editHuDetailList = new List<HuDetail>();

                    List<HuDetailResult> setHuDetailList = new List<HuDetailResult>();

                    //通过需要补货的信息 创建补货任务 及补货明细
                    foreach (var item in Supplementlist)
                    {
                        //如果循环次数等于 补货库位
                        if (i >= count)
                        {
                            break;
                        }
                        if (j == 0)
                        {
                            //第一次时 创建补货任务头表
                            SupplementTask task = new SupplementTask();
                            task.WhCode = whCode;
                            task.SupplementNumber = supplementNumber;
                            task.Status = "U";
                            task.CreateUser = userName;
                            task.CreateDate = DateTime.Now;
                            idal.ISupplementTaskDAL.Add(task);
                        }
                        j++;
                        //1.首先验证 补货任务明细表中 补货库位是否已创建明细
                        List<SupplementTaskDetail> supplementTaskDetailList = idal.ISupplementTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.PutLocationId == item.LocationId && u.Status != "C");
                        if (supplementTaskDetailList.Count != 0)
                        {
                            continue;
                        }


                        //2.查询补货款号的 库存数据
                        List<HuDetailResult> huDetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                                             join b in idal.IHuMasterDAL.SelectAll()
                                                             on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId }
                                                             join c in idal.IWhLocationDAL.SelectAll()
                                                              on new { b.Location, b.WhCode }
                                                              equals new { Location = c.LocationId, c.WhCode }
                                                             where a.WhCode == whCode && b.Location != item.LocationId
                                                             && a.ClientCode == item.ClientCode && a.ItemId == item.ItemId
                                                             && (a.UnitName == null ? "" : a.UnitName) == (item.UnitName == null ? "" : item.UnitName)
                                                             && (a.LotNumber1 == null ? "" : a.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)
                                                             && (a.Qty - (a.PlanQty == null ? 0 : a.PlanQty)) > 0
                                                             && b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1
                                                             && c.LocationTypeDetailId != 1
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
                                                                 LotNumber1 = a.LotNumber1 ?? "",
                                                                 LotNumber2 = a.LotNumber2 ?? "",
                                                                 LotDate = a.LotDate,
                                                                 Location = b.Location
                                                             }).OrderBy(u => u.ReceiptDate).ToList();
                        if (huDetailList.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            //2.1 插入补货任务明细表
                            foreach (var hudetail in huDetailList)
                            {
                                if (spDetilList.Where(u => u.HuDetailId == hudetail.Id).Count() > 0)
                                {
                                    continue;
                                }

                                //插入补货任务明细
                                SupplementTaskDetail spDetail = new SupplementTaskDetail();
                                spDetail.WhCode = whCode;
                                spDetail.SupplementNumber = supplementNumber;
                                spDetail.HuDetailId = hudetail.Id;
                                spDetail.HuId = hudetail.HuId;
                                spDetail.LocationId = hudetail.Location;
                                spDetail.PutLocationId = item.LocationId;
                                spDetail.AltItemNumber = hudetail.AltItemNumber;
                                spDetail.ItemId = hudetail.ItemId;
                                spDetail.UnitName = hudetail.UnitName;
                                spDetail.UnitId = hudetail.UnitId;
                                spDetail.EAN = item.EAN;

                                spDetail.Qty = (Int32)(hudetail.Qty - (hudetail.PlanQty == null ? 0 : hudetail.PlanQty));

                                spDetail.LotNumber1 = hudetail.LotNumber1;
                                spDetail.LotNumber2 = hudetail.LotNumber2;
                                spDetail.LotDate = hudetail.LotDate;
                                spDetail.Status = "U";
                                spDetail.CreateUser = userName;
                                spDetail.CreateDate = DateTime.Now;
                                spDetilList.Add(spDetail);
                                i++;

                                if (editHuDetailList.Where(u => u.Id == hudetail.Id).Count() == 0)
                                {
                                    HuDetail huDetail = new HuDetail();
                                    huDetail.Id = hudetail.Id;
                                    huDetail.PlanQty = hudetail.PlanQty + spDetail.Qty;

                                    editHuDetailList.Add(huDetail);
                                }
                                else
                                {
                                    HuDetail oldhuDetail = editHuDetailList.Where(u => u.Id == hudetail.Id).First();
                                    editHuDetailList.Remove(oldhuDetail);

                                    HuDetail newhuDetail = new HuDetail();
                                    newhuDetail.Id = hudetail.Id;
                                    newhuDetail.PlanQty = oldhuDetail.PlanQty + hudetail.PlanQty + spDetail.Qty;
                                    editHuDetailList.Add(newhuDetail);
                                }

                                break;
                            }
                        }
                    }

                    if (spDetilList.Count > 0)
                    {
                        if (editHuDetailList != null)
                        {
                            foreach (var item in editHuDetailList)
                            {
                                HuDetail huDetail1 = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                                TranLog tl = new TranLog();
                                tl.TranType = "33";
                                tl.Description = "释放补货锁定数量";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.ClientCode = huDetail1.ClientCode;
                                tl.SoNumber = huDetail1.SoNumber;
                                tl.CustomerPoNumber = huDetail1.CustomerPoNumber;
                                tl.AltItemNumber = huDetail1.AltItemNumber;
                                tl.ItemId = huDetail1.ItemId;
                                tl.UnitID = huDetail1.UnitId;
                                tl.UnitName = huDetail1.UnitName;
                                tl.ReceiptId = huDetail1.ReceiptId;
                                tl.ReceiptDate = huDetail1.ReceiptDate;
                                tl.TranQty = huDetail1.PlanQty;
                                tl.HuId = huDetail1.HuId;
                                tl.Length = huDetail1.Length;
                                tl.Width = huDetail1.Width;
                                tl.Height = huDetail1.Height;
                                tl.Weight = huDetail1.Weight;
                                tl.LotNumber1 = huDetail1.LotNumber1;
                                tl.LotNumber2 = huDetail1.LotNumber2;
                                tl.LotDate = huDetail1.LotDate;
                                tl.LoadId = supplementNumber;
                                tl.Remark = "锁定数量+" + (item.PlanQty - (huDetail1.PlanQty ?? 0));

                                HuDetail huDetail = new HuDetail();
                                huDetail.PlanQty = item.PlanQty;
                                huDetail.UpdateUser = userName;
                                huDetail.UpdateDate = DateTime.Now;
                                idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                tl.TranQty2 = huDetail.PlanQty;
                                tranLogList.Add(tl);
                            }
                        }

                        idal.ISupplementTaskDetailDAL.Add(spDetilList);
                        idal.ITranLogDAL.Add(tranLogList);
                    }
                    else
                    {
                        return "当前没有库位需要补货或需要补货的款号库存不足！";
                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "释放补货任务超时，请重新提交！";
                }
            }
        }

        //补货任务表查询
        public List<SupplementTaskResult> SupplementTaskResultList(SupplementTaskSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISupplementTaskDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new SupplementTaskResult
                      {
                          Id = a.Id,
                          SupplementNumber = a.SupplementNumber,
                          Status = a.Status == "U" ? "未补货" :
                          a.Status == "A" ? "正在补货" :
                           a.Status == "C" ? "完成补货" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.SupplementNumber))
                sql = sql.Where(u => u.SupplementNumber == searchEntity.SupplementNumber);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //补货任务明细查询
        public List<SupplementTaskDetailResult> SupplementTaskDetailResultList(SupplementTaskDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISupplementTaskDetailDAL.SelectAll()
                      where a.SupplementNumber == searchEntity.SupplementNumber
                      select new SupplementTaskDetailResult
                      {
                          Id = a.Id,
                          SupplementNumber = a.SupplementNumber,
                          HuId = a.HuId,
                          LocationId = a.LocationId,
                          PutLocationId = a.PutLocationId,
                          GroupNumber = a.GroupNumber,
                          AltItemNumber = a.AltItemNumber,
                          UnitName = a.UnitName,
                          Qty = a.Qty,
                          Status = a.Status == "U" ? "未补货" :
                          a.Status == "D" ? "已下架" :
                           a.Status == "C" ? "完成补货" : null,
                          LotNumber1 = a.LotNumber1,
                          LotNumber2 = a.LotNumber2,
                          LotDate = a.LotDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate

                      };

            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId == searchEntity.LocationId);
            if (!string.IsNullOrEmpty(searchEntity.PutLocationId))
                sql = sql.Where(u => u.PutLocationId == searchEntity.PutLocationId);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //删除补货任务
        public string SupplementTaskDetailDel(string whCode, string supplementNumber, string userName)
        {
            List<SupplementTaskDetail> list = idal.ISupplementTaskDetailDAL.SelectBy(u => u.SupplementNumber == supplementNumber && u.WhCode == whCode);
            if (list.Count == 0)
            {
                return "当前没有补货任务明细需要删除！";
            }
            if (list.Where(u => u.Status == "U").Count() == 0)
            {
                return "当前补货任务已经完成，没有明细需要删除！";
            }

            List<HuDetail> editHuDetailList = new List<HuDetail>();

            List<TranLog> tranLogList = new List<TranLog>();

            if (list.Where(u => u.Status == "C" || u.Status == "D").Count() == 0)
            {
                foreach (var item in list)
                {
                    if (editHuDetailList.Where(u => u.Id == item.HuDetailId).Count() == 0)
                    {
                        HuDetail huDetail = new HuDetail();
                        huDetail.Id = (Int32)item.HuDetailId;
                        huDetail.PlanQty = item.Qty;
                        editHuDetailList.Add(huDetail);
                    }
                    else
                    {
                        HuDetail oldhuDetail = editHuDetailList.Where(u => u.Id == item.HuDetailId).First();
                        editHuDetailList.Remove(oldhuDetail);

                        HuDetail newhuDetail = new HuDetail();
                        newhuDetail.Id = (Int32)item.HuDetailId;
                        newhuDetail.PlanQty = oldhuDetail.PlanQty + item.Qty;
                        editHuDetailList.Add(newhuDetail);
                    }
                }

                if (editHuDetailList != null)
                {
                    foreach (var item in editHuDetailList)
                    {
                        HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                        TranLog tl = new TranLog();
                        tl.TranType = "34";
                        tl.Description = "撤销补货锁定数量";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = userName;
                        tl.WhCode = whCode;
                        tl.ClientCode = huDetail.ClientCode;
                        tl.SoNumber = huDetail.SoNumber;
                        tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                        tl.AltItemNumber = huDetail.AltItemNumber;
                        tl.ItemId = huDetail.ItemId;
                        tl.UnitID = huDetail.UnitId;
                        tl.UnitName = huDetail.UnitName;
                        tl.ReceiptId = huDetail.ReceiptId;
                        tl.ReceiptDate = huDetail.ReceiptDate;
                        tl.TranQty = huDetail.PlanQty;
                        tl.HuId = huDetail.HuId;
                        tl.Length = huDetail.Length;
                        tl.Width = huDetail.Width;
                        tl.Height = huDetail.Height;
                        tl.Weight = huDetail.Weight;
                        tl.LotNumber1 = huDetail.LotNumber1;
                        tl.LotNumber2 = huDetail.LotNumber2;
                        tl.LotDate = huDetail.LotDate;
                        tl.LoadId = supplementNumber;
                        tl.Remark = "锁定数量-" + item.PlanQty;


                        huDetail.PlanQty = huDetail.PlanQty - item.PlanQty;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        tl.TranQty2 = huDetail.PlanQty;
                        tranLogList.Add(tl);

                    }
                }
                idal.ITranLogDAL.Add(tranLogList);
                idal.ISupplementTaskDAL.DeleteBy(u => u.WhCode == whCode && u.SupplementNumber == supplementNumber);
                idal.ISupplementTaskDetailDAL.DeleteBy(u => u.WhCode == whCode && u.SupplementNumber == supplementNumber);
                idal.SaveChanges();
                return "Y";
            }
            else
            {
                foreach (var item in list.Where(u => u.Status == "U"))
                {
                    if (editHuDetailList.Where(u => u.Id == item.HuDetailId).Count() == 0)
                    {
                        HuDetail huDetail = new HuDetail();
                        huDetail.Id = (Int32)item.HuDetailId;
                        huDetail.PlanQty = item.Qty;
                        editHuDetailList.Add(huDetail);
                    }
                    else
                    {
                        HuDetail oldhuDetail = editHuDetailList.Where(u => u.Id == item.HuDetailId).First();
                        editHuDetailList.Remove(oldhuDetail);

                        HuDetail newhuDetail = new HuDetail();
                        newhuDetail.Id = (Int32)item.HuDetailId;
                        newhuDetail.PlanQty = oldhuDetail.PlanQty + item.Qty;
                        editHuDetailList.Add(newhuDetail);
                    }
                }
                if (editHuDetailList != null)
                {
                    foreach (var item in editHuDetailList)
                    {
                        HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                        TranLog tl = new TranLog();
                        tl.TranType = "34";
                        tl.Description = "撤销补货锁定数量";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = userName;
                        tl.WhCode = whCode;
                        tl.ClientCode = huDetail.ClientCode;
                        tl.SoNumber = huDetail.SoNumber;
                        tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                        tl.AltItemNumber = huDetail.AltItemNumber;
                        tl.ItemId = huDetail.ItemId;
                        tl.UnitID = huDetail.UnitId;
                        tl.UnitName = huDetail.UnitName;
                        tl.ReceiptId = huDetail.ReceiptId;
                        tl.ReceiptDate = huDetail.ReceiptDate;
                        tl.TranQty = huDetail.PlanQty;
                        tl.HuId = huDetail.HuId;
                        tl.Length = huDetail.Length;
                        tl.Width = huDetail.Width;
                        tl.Height = huDetail.Height;
                        tl.Weight = huDetail.Weight;
                        tl.LotNumber1 = huDetail.LotNumber1;
                        tl.LotNumber2 = huDetail.LotNumber2;
                        tl.LotDate = huDetail.LotDate;
                        tl.LoadId = supplementNumber;
                        tl.Remark = "锁定数量-" + item.PlanQty;

                        huDetail.PlanQty = huDetail.PlanQty - item.PlanQty;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        tl.TranQty2 = huDetail.PlanQty;
                        tranLogList.Add(tl);
                    }
                }

                //保存日志
                idal.ITranLogDAL.Add(tranLogList);

                idal.ISupplementTaskDetailDAL.DeleteBy(u => u.WhCode == whCode && u.SupplementNumber == supplementNumber && u.Status == "U");

                if (list.Where(u => u.Status == "D").Count() == 0)
                {
                    SupplementTask task = new SupplementTask();
                    task.Status = "C";
                    task.UpdateUser = userName;
                    task.UpdateDate = DateTime.Now;
                    idal.ISupplementTaskDAL.UpdateBy(task, u => u.WhCode == whCode && u.SupplementNumber == supplementNumber, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }

                idal.SaveChanges();
                return "Y1";
            }

        }

        public List<WhLocationResult> SupplementLocationList(WhLocationSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhLocationDAL.SelectAll()
                      join b in idal.ILocationTypesDetailDAL.SelectAll()
                      on a.LocationTypeDetailId equals b.Id
                      where a.WhCode == searchEntity.WhCode && a.LocationTypeDetailId == 1
                      select new WhLocationResult
                      {
                          Id = a.Id,
                          LocationId = a.LocationId,
                          Status = a.Status,
                          LocationDescription = b.Description,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId.StartsWith(searchEntity.LocationId));
            if (!string.IsNullOrEmpty(searchEntity.LocationTypeId))
            {
                int LocationTypeId = Convert.ToInt32(searchEntity.LocationTypeId);
                sql = sql.Where(u => u.LocationTypeId == LocationTypeId);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        public int ImportSupplementLocation(List<WhLocation> entity)
        {
            lock (o)
            {
                List<WhLocation> whLocationListAdd = new List<WhLocation>();
                List<Pallate> pallateListAdd = new List<Pallate>();
                foreach (var item in entity)
                {
                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == item.WhCode && u.LocationId == item.LocationId).Count == 0)
                    {
                        item.CreateDate = DateTime.Now;
                        whLocationListAdd.Add(item);
                    }
                    else
                    {
                        idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.LocationId == item.LocationId, t => new WhLocation { LocationTypeDetailId = 1 });
                    }
                    if (idal.IPallateDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == item.LocationId).Count == 0)
                    {
                        Pallate p = new Pallate(); ;
                        p.WhCode = item.WhCode;
                        p.HuId = item.LocationId;
                        p.TypeId = 1;
                        p.Status = "U";
                        p.CreateUser = item.CreateUser;
                        p.CreateDate = DateTime.Now;
                        pallateListAdd.Add(p);
                    }
                }
                idal.IWhLocationDAL.Add(whLocationListAdd);
                idal.IPallateDAL.Add(pallateListAdd);
                idal.IWhLocationDAL.SaveChanges();
                return 1;
            }
        }

        //批量删除捡货库位
        public string SupplementLocationDel(List<WhLocation> entity)
        {
            lock (o)
            {
                string result = "";

                //List<WhLocationResult> checkSql = (from a in entity
                //                                   join b in idal.IHuMasterDAL.SelectAll()
                //                                   on new { A = a.WhCode, B = a.LocationId } equals new { A = b.WhCode, B = b.Location }
                //                                   group a by new
                //                                   {
                //                                       a.LocationId
                //                                   } into g
                //                                   select new WhLocationResult
                //                                   {
                //                                       LocationId = g.Key.LocationId
                //                                   }).ToList();
                //if (checkSql.Count > 0)
                //{
                //    foreach (var item in checkSql.Distinct())
                //    {
                //        result += item.LocationId + ",";
                //    }
                //    return "捡货库位存在库存无法删除：" + result.Substring(0, result.Length - 1);
                //}

                List<WhLocationResult> checkSql1 = (from a in entity
                                                    join b in idal.ISupplementTaskDetailDAL.SelectAll()
                                                    on new { A = a.WhCode, B = a.LocationId } equals new { A = b.WhCode, B = b.LocationId }
                                                    where b.Status != "C"
                                                    group a by new
                                                    {
                                                        a.LocationId
                                                    } into g
                                                    select new WhLocationResult
                                                    {
                                                        LocationId = g.Key.LocationId
                                                    }).ToList();
                if (checkSql1.Count > 0)
                {
                    foreach (var item in checkSql1.Distinct())
                    {
                        result += item.LocationId + ",";
                    }
                    return "捡货库位存在补货任务无法删除：" + result.Substring(0, result.Length - 1);
                }


                foreach (var location in entity)
                {
                    idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == location.WhCode && u.LocationId == location.LocationId, t => new WhLocation { LocationTypeDetailId = 0 });
                }

                idal.SaveChanges();
                return "Y";
            }
        }

        //批量删除捡货库位对应款号信息
        public string R_Location_Item_Del(List<R_Location_Item> entity)
        {
            lock (o)
            {
                foreach (var item in entity)
                {
                    idal.IR_Location_ItemDAL.DeleteBy(u => u.Id == item.Id);
                }

                idal.SaveChanges();
                return "Y";
            }
        }


        #endregion


        #region 13.耗材管理

        //耗材列表entity.LossCode + entity.LossDescription
        public List<Loss> LossList(LossSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILossDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.LossCode))
                sql = sql.Where(u => u.LossCode == searchEntity.LossCode);

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增耗材
        public string LossAdd(Loss entity)
        {
            if (idal.ILossDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LossCode == entity.LossCode && u.ClientCode == entity.ClientCode).Count == 0)
            {
                //添加日志
                TranLog tl = new TranLog();
                tl.TranType = "500";
                tl.Description = "新增耗材";
                tl.TranDate = DateTime.Now;
                tl.TranUser = entity.CreateUser;
                tl.WhCode = entity.WhCode;
                tl.Length = entity.Length;
                tl.Width = entity.Width;
                tl.Height = entity.Height;
                tl.Weight = entity.Weight;
                tl.TranQty = entity.Qty;
                tl.AltItemNumber = entity.LossCode;
                idal.ITranLogDAL.Add(tl);

                entity.CreateDate = DateTime.Now;
                idal.ILossDAL.Add(entity);
                idal.ILossDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "材质Code已存在！";
            }
        }

        //修改耗材
        public int LossEdit(Loss entity)
        {
            Loss loss1 = idal.ILossDAL.SelectBy(u => u.Id == entity.Id).First();
            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "501";
            tl.Description = "修改耗材";
            tl.TranDate = DateTime.Now;
            tl.ClientCode = entity.ClientCode;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = loss1.WhCode;
            tl.Length = loss1.Length;
            tl.Width = loss1.Width;
            tl.Height = loss1.Height;
            tl.Weight = loss1.Weight;
            tl.TranQty = loss1.Qty;
            tl.TranQty2 = entity.Qty;
            tl.AltItemNumber = loss1.LossCode;
            idal.ITranLogDAL.Add(tl);

            Loss loss = new Loss();
            loss.ClientCode = entity.ClientCode;
            loss.LossDescription = entity.LossDescription;
            loss.Qty = (Int32)entity.Qty;
            loss.Length = entity.Length;
            loss.Width = entity.Width;
            loss.Height = entity.Height;
            loss.Weight = entity.Weight;
            loss.WorkLevel = entity.WorkLevel;
            loss.UpdateUser = entity.UpdateUser;
            loss.UpdateDate = DateTime.Now;
            idal.ILossDAL.UpdateBy(loss, u => u.Id == entity.Id, new string[] { "ClientCode", "LossDescription", "Qty", "Length", "Width", "Height", "Weight", "WorkLevel", "UpdateUser", "UpdateDate" });
            idal.ILossDAL.SaveChanges();
            return 1;
        }

        //删除耗材
        public int LossDel(int id)
        {
            Loss entity = idal.ILossDAL.SelectBy(u => u.Id == id).First();

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "502";
            tl.Description = "删除耗材";
            tl.TranDate = DateTime.Now;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = entity.WhCode;
            tl.Length = entity.Length;
            tl.Width = entity.Width;
            tl.Height = entity.Height;
            tl.Weight = entity.Weight;
            tl.TranQty = entity.Qty;
            tl.AltItemNumber = entity.LossCode;
            idal.ITranLogDAL.Add(tl);

            idal.ILossDAL.DeleteBy(u => u.Id == entity.Id);
            idal.ILossDAL.SaveChanges();
            return 1;
        }


        #endregion


        #region 14.照片管理

        //新增TCR处理方式
        public string TCRProcessModeAdd(TCRProcess entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.ITCRProcessDAL.Add(entity);
            idal.SaveChanges();
            return "Y";
        }

        //TCR处理方式列表
        public List<TCRProcessResult> TCRProcessModeList(PhotoMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.ITCRProcessDAL.SelectAll()
                      join b in idal.IWhUserDAL.SelectAll()
                      on a.CreateUser equals b.UserName into b
                      from ab in b.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      select new TCRProcessResult
                      {
                          Id = a.Id,
                          TCRProcessMode = a.TCRProcessMode,
                          CreateUser = ab.UserNameCN,
                          CreateDate = a.CreateDate
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //新增TCR信息
        public string PhotoMasterAdd(List<PhotoMaster> entity, string whCode)
        {
            List<PhotoMaster> list = new List<PhotoMaster>();
            string type = "in";

            foreach (var item in entity)
            {
                PhotoMaster photo = item;

                List<Receipt> ReceiptList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == item.Number);

                item.CreateDate = DateTime.Now;
                item.TCRStatus = "未处理";
                item.CheckStatus1 = "N";
                item.CheckStatus2 = "N";
                item.OrderSource = "新增";
                item.Type = type;
                list.Add(photo);
            }
            idal.IPhotoMasterDAL.Add(list);
            idal.SaveChanges();
            return "Y";
        }

        //修改TCR信息
        public string PhotoMasterEdit(PhotoMaster entity)
        {
            idal.IPhotoMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "TCRProcessMode", "SettlementMode", "SumPrice", "KRemark1" });
            idal.SaveChanges();
            return "Y";
        }


        //修改TCR信息
        public string PhotoMasterEdit1(PhotoMaster entity)
        {
            idal.IPhotoMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "CRemark1" });
            idal.SaveChanges();
            return "Y";
        }

        //处理TCR
        public string EditTCRStatus(PhotoMaster entity)
        {
            entity.TCRCheckUser = entity.UpdateUser;
            entity.TCRCheckDate = DateTime.Now;
            idal.IPhotoMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "TCRStatus", "TCRCheckUser", "TCRCheckDate" });
            idal.SaveChanges();
            return "Y";
        }


        //CFS收货照片查询
        public List<PhotoMasterResult> InCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total)
        {
            string type = "in";

            if (searchEntity.PhotoType + "" != "")
            {
                type = searchEntity.PhotoType;
            }

            var sql = (from a in idal.IPhotoMasterDAL.SelectAll()
                       join b in idal.IHuMasterDAL.SelectAll()
                       on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId } into temp1
                       from b in temp1.DefaultIfEmpty()
                       join f in idal.IUnitDefaultDAL.SelectAll()
                       on new { A = a.WhCode, B = a.UnitName } equals new { A = f.WhCode, B = f.UnitName } into temp2
                       from f in temp2.DefaultIfEmpty()
                       join c in idal.IItemMasterDAL.SelectAll()
                       on a.ItemId equals c.Id into temp3
                       from c in temp3.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && a.Type == type
                       select new PhotoMasterResult
                       {
                           Action = a.Id.ToString(),
                           PhotoId = a.PhotoId ?? 0,
                           ClientCode = a.ClientCode ?? "",
                           Number = a.Number,
                           Number2 = a.Number2 ?? "",
                           Number3 = a.Number3 ?? "",
                           Number4 = a.Number4 ?? "",
                           UnitName = ((f.UnitNameCN ?? "") == "" ? a.UnitName : f.UnitNameCN),
                           Qty = a.Qty ?? 0,
                           RegQty = a.RegQty ?? 0,
                           HuId = a.HuId,
                           LocationId = b.Location,
                           HoldReason = a.HoldReason ?? "",
                           TCRStatus = a.TCRStatus ?? "",
                           TCRCheckUser = a.TCRCheckUser ?? "",
                           TCRCheckDate = a.TCRCheckDate,
                           UpdateUser = a.UpdateUser,
                           TCRProcessMode = a.TCRProcessMode ?? "",
                           SettlementMode = a.SettlementMode ?? "",
                           SumPrice = a.SumPrice ?? 0,
                           DeliveryDate = a.DeliveryDate,
                           Status =
                              (a.Status ?? 0) != 0 ? "已上传" : "未上传",
                           UploadDate = a.UploadDate,
                           CheckStatus1 =
                              (a.CheckStatus1 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser1 = a.CheckUser1,
                           CheckDate1 = a.CheckDate1,
                           KRemark1 = a.KRemark1 ?? "",
                           CheckStatus2 =
                              (a.CheckStatus2 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser2 = a.CheckUser2,
                           CheckDate2 = a.CheckDate2,
                           CRemark1 = a.CRemark1 ?? "",
                           CreateUser = a.CreateUser,
                           CreateDate = a.CreateDate,
                           OrderSource = a.OrderSource,
                           UpdateDate = a.UpdateDate,
                           UserCode = a.CreateUser,
                           UserNameCN = "",
                           Style1 = c.Style1 ?? "",
                           Style2 = c.Style2 ?? "",
                           Style3 = c.Style3 ?? ""
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.Number))
                sql = sql.Where(u => u.Number == searchEntity.Number);
            if (!string.IsNullOrEmpty(searchEntity.Number2))
                sql = sql.Where(u => u.Number2 == searchEntity.Number2);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            if (!string.IsNullOrEmpty(searchEntity.HoldReason))
            {
                if (searchEntity.HoldReason == "否")
                {
                    sql = sql.Where(u => u.HoldReason != "部分收货");
                }
                else
                {
                    sql = sql.Where(u => u.HoldReason == searchEntity.HoldReason);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.HoldReason1))
                sql = sql.Where(u => u.HoldReason.Contains(searchEntity.HoldReason1));

            if (!string.IsNullOrEmpty(searchEntity.HoldReasonType))
                sql = sql.Where(u => u.HoldReason.Contains(searchEntity.HoldReasonType));

            if (!string.IsNullOrEmpty(searchEntity.HoldReasonTypeNot))
                sql = sql.Where(u => !u.HoldReason.Contains(searchEntity.HoldReasonTypeNot));

            if (!string.IsNullOrEmpty(searchEntity.TCRStatus))
                sql = sql.Where(u => u.TCRStatus == searchEntity.TCRStatus);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus1))
                sql = sql.Where(u => u.CheckStatus1 == searchEntity.CheckStatus1);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus2))
                sql = sql.Where(u => u.CheckStatus2 == searchEntity.CheckStatus2);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }
            if (searchEntity.KRemark1 == "有")
                sql = sql.Where(u => u.KRemark1 != "");
            if (searchEntity.KRemark1 == "无")
                sql = sql.Where(u => u.KRemark1 == "");

            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();
            List<PhotoMasterResult> list = new List<PhotoMasterResult>();
            foreach (var item in sql)
            {
                PhotoMasterResult work = item;
                List<WhUser> userCheck = userList.Where(u => u.UserCode == item.TCRCheckUser).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.TCRCheckUser = user.UserNameCN;
                }

                List<WhUser> userCheck1 = userList.Where(u => u.UserCode == item.CreateUser).ToList();
                if (userCheck1.Count > 0)
                {
                    WhUser user = userCheck1.First();
                    work.UserNameCN = user.UserNameCN;
                }
                list.Add(work);
            }

            total = list.Count;
            list = list.OrderBy(u => u.CreateDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //CFS出货照片查询
        public List<PhotoMasterResult> OutCFSPhotoMasterList(PhotoMasterSearch searchEntity, out int total)
        {
            string type = "out";

            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.IPhotoMasterDAL.SelectAll()
                             on new { a.LoadId, a.WhCode, Type = type }
                         equals new { LoadId = b.Number, b.WhCode, b.Type } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.ILoadContainerExtendDAL.SelectAll()
                             on new { a.LoadId, a.WhCode }
                         equals new { c.LoadId, c.WhCode } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in (
                           (from a0 in idal.ILoadDetailDAL.SelectAll()
                            join b1 in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)a0.OutBoundOrderId } equals new { OutBoundOrderId = b1.Id }
                            select new
                            {
                                a0.LoadMasterId,
                                b1.ClientCode
                            })) on new { Id = a.Id } equals new { Id = (Int32)d.LoadMasterId } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in (
                           (from workloadaccount in idal.IWorkloadAccountDAL.SelectAll()
                            where workloadaccount.WorkType == "理货员"
                            group workloadaccount by new
                            {
                                workloadaccount.LoadId,
                                workloadaccount.WhCode,
                                workloadaccount.UserCode
                            } into g
                            select new
                            {
                                g.Key.LoadId,
                                g.Key.WhCode,
                                g.Key.UserCode
                            }))
                             on new { a.LoadId, a.WhCode }
                         equals new { e.LoadId, e.WhCode } into e_join
                       from e in e_join.DefaultIfEmpty()
                       join f in idal.IWhUserDAL.SelectAll() on e.UserCode equals f.UserCode into f_join
                       from f in f_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && d.ClientCode != "TEST"
                       select new PhotoMasterResult
                       {
                           Action = b.Id.ToString(),
                           Number = a.LoadId,
                           ClientCode = d.ClientCode,
                           BeginPackDate = a.BeginPackDate,
                           ShipDate = a.ShipDate,
                           PhotoId = b.PhotoId ?? 0,
                           ContainerNumber = c.ContainerNumber ?? "",
                           ContainerType = c.ContainerType,
                           Status =
                                (b.Status ?? 0) != 0 ? "已上传" : "未上传",
                           UploadDate = b.UploadDate,
                           CreateDate = a.BeginPackDate,
                           CheckStatus1 =
                                (b.CheckStatus1 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser1 = b.CheckUser1,
                           CheckDate1 = b.CheckDate1,
                           KRemark1 = b.KRemark1 ?? "",
                           CheckStatus2 =
                                (b.CheckStatus2 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser2 = b.CheckUser2,
                           CheckDate2 = b.CheckDate2,
                           CRemark1 = b.CRemark1 ?? "",
                           UserCode = f.UserCode,
                           UserNameCN = f.UserNameCN,
                           Location = a.Location
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.Number))
                sql = sql.Where(u => u.Number == searchEntity.Number);
            if (!string.IsNullOrEmpty(searchEntity.UserCode))
                sql = sql.Where(u => u.UserCode == searchEntity.UserCode);
            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus1))
                sql = sql.Where(u => u.CheckStatus1 == searchEntity.CheckStatus1);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus2))
                sql = sql.Where(u => u.CheckStatus2 == searchEntity.CheckStatus2);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }
            if (searchEntity.KRemark1 == "有")
                sql = sql.Where(u => u.KRemark1 != "");
            if (searchEntity.KRemark1 == "无")
                sql = sql.Where(u => u.KRemark1 == "");

            total = sql.Count();
            sql = sql.OrderBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //收货照片上传
        public string InCFSPhotoCComplete(PhotoMaster entity)
        {
            if (entity.WhCode.Length == 1)
            {
                entity.WhCode = "0" + entity.WhCode;
            }
            if ((entity.PhotoId ?? 0) != 0)
            {
                PhotoMaster photo = new PhotoMaster();
                photo.Status = 1;
                photo.PhotoId = entity.PhotoId;
                photo.UploadDate = DateTime.Now;
                idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == entity.Id, new string[] { "Status", "PhotoId", "UploadDate" });
            }
            idal.SaveChanges();
            return "Y";
        }

        //审核照片
        public string CFSPhotoCShenheComplete(PhotoMaster entity)
        {
            if (entity.WhCode.Length == 1)
            {
                entity.WhCode = "0" + entity.WhCode;
            }
            if ((entity.PhotoId ?? 0) != 0)
            {
                if (!string.IsNullOrEmpty(entity.CheckUser1))
                {
                    PhotoMaster photo = new PhotoMaster();
                    photo.CheckUser1 = entity.CheckUser1;
                    photo.CheckStatus1 = "Y";
                    photo.CheckDate1 = DateTime.Now;
                    idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == entity.Id, new string[] { "CheckUser1", "CheckStatus1", "CheckDate1" });
                }
                else if (!string.IsNullOrEmpty(entity.CheckUser2))
                {
                    PhotoMaster photo = new PhotoMaster();
                    photo.CheckUser2 = entity.CheckUser2;
                    photo.CheckStatus2 = "Y";
                    photo.CheckDate2 = DateTime.Now;
                    idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == entity.Id, new string[] { "CheckUser2", "CheckStatus2", "CheckDate2" });
                }
            }
            idal.SaveChanges();
            return "Y";
        }

        //出货照片上传
        public string OutCFSPhotoCComplete(PhotoMaster entity)
        {
            if (entity.WhCode.Length == 1)
            {
                entity.WhCode = "0" + entity.WhCode;
            }
            if ((entity.PhotoId ?? 0) != 0)
            {
                string type = "out";

                List<PhotoMaster> photoList = idal.IPhotoMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.Number == entity.Number && u.Number2 == entity.Number2);
                if (photoList.Count > 0)
                {
                    PhotoMaster photo = photoList.First();
                    photo.CreateDate = DateTime.Now;
                    photo.CreateUser = entity.CreateUser;
                    photo.UploadDate = DateTime.Now;
                    idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == photo.Id, new string[] { "UploadDate", "CreateUser", "CreateDate" });
                }
                else
                {
                    PhotoMaster photo = new PhotoMaster();
                    photo.WhCode = entity.WhCode;
                    photo.PhotoId = entity.PhotoId;
                    photo.UploadDate = DateTime.Now;
                    photo.Number = entity.Number;
                    photo.Number2 = entity.Number2;
                    photo.Type = type;
                    photo.Status = 1;
                    photo.CreateDate = DateTime.Now;
                    photo.CreateUser = entity.CreateUser;
                    idal.IPhotoMasterDAL.Add(photo);
                }
            }
            idal.SaveChanges();
            return "Y";
        }

        //审核照片百分比
        public decimal CheckCountPercent(PhotoMasterSearch searchEntity)
        {
            string type = "out";

            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.IPhotoMasterDAL.SelectAll()
                             on new { a.WhCode, a.LoadId, Type = type }
                         equals new { b.WhCode, LoadId = b.Number, b.Type } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.ILoadContainerExtendDAL.SelectAll()
                             on new { a.WhCode, a.LoadId }
                         equals new { c.WhCode, c.LoadId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)d.LoadMasterId } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)d.OutBoundOrderId } equals new { OutBoundOrderId = e.Id } into e_join
                       from e in e_join.DefaultIfEmpty()
                       join f in (
                            ((from a0 in idal.IPhotoMasterDAL.SelectAll()
                              join b1 in idal.IWorkloadAccountDAL.SelectAll()
                                    on new { a0.Number, a0.WhCode }
                                equals new { Number = b1.LoadId, b1.WhCode } into b1_join
                              from b1 in b1_join.DefaultIfEmpty()
                              where
                                b1.WorkType == "理货员" && b1.WhCode == searchEntity.WhCode && a0.Type == type
                              select new
                              {
                                  UserCode = b1.UserCode,
                                  a0.Number,
                                  a0.WhCode
                              }).Distinct()))
                              on new { b.WhCode, b.Number }
                          equals new { f.WhCode, f.Number } into f_join
                       from f in f_join.DefaultIfEmpty()
                       join g in idal.IWhUserDAL.SelectAll()
                       on f.UserCode equals g.UserCode into g_join
                       from g in g_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       select new PhotoMasterResult
                       {
                           Action = b.Id.ToString(),
                           Number = a.LoadId,
                           ClientCode = e.ClientCode,
                           BeginPackDate = a.BeginPackDate,
                           ShipDate = a.ShipDate,
                           PhotoId = b.PhotoId ?? 0,
                           ContainerNumber = c.ContainerNumber ?? "",
                           ContainerType = c.ContainerType,
                           Status =
                                (b.Status ?? 0) != 0 ? "已上传" : "未上传",
                           CreateDate = a.BeginPackDate,
                           CheckStatus1 =
                                (b.CheckStatus1 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser1 = b.CheckUser1,
                           CheckDate1 = b.CheckDate1,
                           KRemark1 = b.KRemark1 ?? "",
                           CheckStatus2 =
                                (b.CheckStatus2 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser2 = b.CheckUser2,
                           CheckDate2 = b.CheckDate2,
                           CRemark1 = b.CRemark1 ?? "",
                           UserCode = f.UserCode,
                           UserNameCN = g.UserNameCN
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.Number))
                sql = sql.Where(u => u.Number == searchEntity.Number);
            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus1))
                sql = sql.Where(u => u.CheckStatus1 == searchEntity.CheckStatus1);
            if (!string.IsNullOrEmpty(searchEntity.CheckStatus2))
                sql = sql.Where(u => u.CheckStatus2 == searchEntity.CheckStatus2);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            var sql1 = sql;
            sql1 = sql1.Where(u => (u.CheckUser1 ?? "") != "");
            decimal sql1Count = sql1.Count();

            int total = sql.Count();

            decimal checkCount = 0;
            if (total == 0)
            {
                checkCount = Math.Round((sql1Count / 1), 4) * 100;
            }
            else
            {
                checkCount = Math.Round((sql1Count / total), 4) * 100;
            }

            return checkCount;
        }

        //TCR类型
        public List<HoldMaster> HoldMasterList(HoldMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IHoldMasterDAL.SelectAll()
                      where a.ReasonType == "TCR" && (a.WhCode == searchEntity.WhCode && a.ClientCode == searchEntity.ClientCode) || (a.ClientCode == "all" && a.WhCode == searchEntity.WhCode)
                      select a;

            total = sql.Count();
            sql = sql.OrderBy(u => u.Sequence).ThenBy(u => u.Id);
            if (searchEntity.pageSize != 0 && searchEntity.pageIndex != 0) sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        public string PhotoMasterDelPhotoId(int id, string userName)
        {
            PhotoMaster rec = idal.IPhotoMasterDAL.SelectBy(u => u.Id == id).First();

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "700";
            tranLog.Description = "清除照片Id";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = userName;
            tranLog.WhCode = rec.WhCode;
            tranLog.ClientCode = rec.ClientCode;
            tranLog.SoNumber = rec.Number2;
            tranLog.PoID = rec.PoId;
            tranLog.CustomerPoNumber = rec.Number3;
            tranLog.AltItemNumber = rec.Number4;
            tranLog.ItemId = rec.ItemId;
            tranLog.UnitName = rec.UnitName;
            tranLog.TranQty = rec.Qty;
            tranLog.HuId = rec.HuId;

            if (rec.Number.Substring(0, 2) == "EI")
            {
                tranLog.ReceiptId = rec.Number;
            }
            else
            {
                tranLog.LoadId = rec.Number;
            }

            tranLog.Remark = "原始照片Id:" + rec.PhotoId.ToString();

            PhotoMaster photo = new PhotoMaster();
            photo.PhotoId = 0;
            photo.UpdateUser = userName;
            photo.UpdateDate = DateTime.Now;
            idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == id, new string[] { "PhotoId", "UpdateUser", "UpdateDate" });

            idal.ITranLogDAL.Add(tranLog);
            idal.IPhotoMasterDAL.SaveChanges();

            return "Y";
        }

        //删除TCR照片
        public string PhotoMasterDelById(int id, string userName)
        {
            PhotoMaster entity = idal.IPhotoMasterDAL.SelectBy(u => u.Id == id).First();

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "880";
            tl.Description = "删除TCR照片";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = entity.WhCode;
            tl.LoadId = entity.Number;
            tl.Remark = entity.Number2;
            tl.ReceiptId = entity.PhotoId.ToString();
            idal.ITranLogDAL.Add(tl);

            idal.IPhotoMasterDAL.DeleteBy(u => u.Id == entity.Id);
            idal.IPhotoMasterDAL.SaveChanges();
            return "Y";
        }

        #endregion


        #region 15.工作量管理

        //收货工作量
        public List<WorkloadAccountResult> InWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWorkloadAccountDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ReceiptId != null
                      group new { a } by new
                      {
                          a.WhCode,
                          a.ReceiptId,
                          a.UserCode,
                          a.WorkType
                      } into g
                      select new WorkloadAccountResult
                      {
                          Action = "",
                          WhCode = g.Key.WhCode,
                          ReceiptId = g.Key.ReceiptId,
                          ReceiptDate = g.Min(p => p.a.ReceiptDate),
                          UserCode = g.Key.UserCode,
                          UserNameCN = "",
                          WorkType = g.Key.WorkType,
                          cbm = g.Sum(p => p.a.CBM)
                      };
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.UserCode))
                sql = sql.Where(u => u.UserCode == searchEntity.UserCode);
            if (!string.IsNullOrEmpty(searchEntity.WorkType))
                sql = sql.Where(u => u.WorkType == searchEntity.WorkType);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.EndCreateDate);
            }

            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();

            List<WorkloadAccountResult> list = new List<WorkloadAccountResult>();
            foreach (var item in sql)
            {
                WorkloadAccountResult work = item;
                List<WhUser> userCheck = userList.Where(u => u.UserCode == item.UserCode).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.UserNameCN = user.UserNameCN;
                }
                list.Add(work);
            }

            total = list.Count;
            list = list.OrderBy(u => u.ReceiptId).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //出货工作量
        public List<WorkloadAccountResult> OutWorkloadAccountList(WorkloadAccountSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWorkloadAccountDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.LoadId != null
                      group new { a } by new
                      {
                          a.WhCode,
                          a.LoadId,
                          a.UserCode,
                          a.WorkType
                      } into g
                      select new WorkloadAccountResult
                      {
                          WhCode = g.Key.WhCode,
                          LoadId = g.Key.LoadId,
                          ReceiptDate = (DateTime?)g.Min(p => p.a.ReceiptDate),
                          UserCode = g.Key.UserCode,
                          UserNameCN = "",
                          WorkType = g.Key.WorkType,
                          cbm = (Decimal?)g.Sum(p => p.a.CBM),
                          baQty = (Decimal?)g.Sum(p => (p.a.BaQty ?? 0))
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.UserCode))
                sql = sql.Where(u => u.UserCode == searchEntity.UserCode);
            if (!string.IsNullOrEmpty(searchEntity.WorkType))
                sql = sql.Where(u => u.WorkType == searchEntity.WorkType);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.EndCreateDate);
            }

            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();

            List<WorkloadAccountResult> list = new List<WorkloadAccountResult>();
            foreach (var item in sql)
            {
                WorkloadAccountResult work = item;
                List<WhUser> userCheck = userList.Where(u => u.UserCode == item.UserCode).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.UserNameCN = user.UserNameCN;
                }
                list.Add(work);
            }

            total = list.Count;
            list = list.OrderBy(u => u.LoadId).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //工人种类
        public List<WorkloadAccountResult> WorkTypeList(string whCode)
        {
            List<WorkloadAccountResult> list = new List<WorkloadAccountResult>();
            WorkloadAccountResult work1 = new WorkloadAccountResult();
            work1.WorkType = "理货员";
            list.Add(work1);

            WorkloadAccountResult work2 = new WorkloadAccountResult();
            work2.WorkType = "叉车工";
            list.Add(work2);

            WorkloadAccountResult work3 = new WorkloadAccountResult();
            work3.WorkType = "装卸工";
            list.Add(work3);

            WorkloadAccountResult work4 = new WorkloadAccountResult();
            work4.WorkType = "电车工";
            list.Add(work4);

            return list;
        }


        //--------------收货修改工作量---------------------------------------------------------------
        //批量修改工人工号
        public string EditWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                //如果立方不一致
                if (!string.IsNullOrEmpty(item.cbm.ToString()))
                {
                    List<WorkloadAccount> getlist = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                    decimal? getCbm = getlist.Sum(u => u.CBM);

                    decimal qty = getlist.Sum(u => u.Qty);    //总数量
                    decimal? weight = getlist.Sum(u => u.Weight); //总重量

                    if (item.cbm != getCbm)
                    {
                        WorkloadAccount lhy = getlist.First();

                        idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                        WorkloadAccount work = new WorkloadAccount();
                        work.WhCode = lhy.WhCode;
                        work.ClientId = lhy.ClientId;
                        work.ClientCode = lhy.ClientCode;
                        work.ReceiptId = lhy.ReceiptId;
                        work.HuId = "";
                        work.WorkType = item.WorkType;
                        work.UserCode = lhy.UserCode;
                        work.LotFlag = lhy.LotFlag;
                        work.ReceiptDate = lhy.ReceiptDate;
                        work.CBM = item.cbm;
                        work.Qty = qty;
                        work.Weight = weight;

                        idal.IWorkloadAccountDAL.Add(work);
                        idal.SaveChanges();

                        List<WorkloadAccountResult> distinctResult1 = (from a in idal.IWorkloadAccountDAL.SelectAll()
                                                                       where a.WhCode == item.WhCode && a.ReceiptId == item.ReceiptId && a.WorkType != item.WorkType
                                                                       select new WorkloadAccountResult
                                                                       {
                                                                           WhCode = a.WhCode,
                                                                           ReceiptId = a.ReceiptId,
                                                                           WorkType = a.WorkType
                                                                       }).Distinct().ToList();

                        InUpdateWorkAccountByWorkType(distinctResult1);

                        TranLog tl2 = new TranLog();
                        tl2.TranType = "65";
                        tl2.Description = "修改工作量";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = item.UpdateUser;
                        tl2.WhCode = item.WhCode;
                        tl2.ReceiptId = item.ReceiptId;
                        tl2.Remark = "立方修改为" + item.cbm;
                        tranLogList.Add(tl2);
                    }
                }

                idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.UserCode == item.UserCode && u.WorkType == item.WorkType, u => new WorkloadAccount() { UserCode = userCode });

                TranLog tl1 = new TranLog();
                tl1.TranType = "65";
                tl1.Description = "修改工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.ReceiptId = item.ReceiptId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType + "修改为工号" + userCode;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);

            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }

            InUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }


        //批量修改收货工人
        public string EditWorkloadAccountList(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.UserCode == item.UserCode && u.WorkType == item.WorkType, u => new WorkloadAccount() { UserCode = userCode });

                TranLog tl1 = new TranLog();
                tl1.TranType = "65";
                tl1.Description = "修改工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.ReceiptId = item.ReceiptId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType + "修改为工号" + userCode;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);
            idal.SaveChanges();
            return "Y";
        }

        //调整其它工种工作量方法
        private void InUpdateWorkAccountByWorkType(List<WorkloadAccountResult> distinctResult)
        {
            WorkloadAccountResult first = distinctResult.First();

            List<WorkloadAccount> getlist = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == first.WhCode && u.ReceiptId == first.ReceiptId);

            List<WorkloadAccount> listAdd = new List<WorkloadAccount>();
            foreach (var item in distinctResult)
            {
                if (item.WorkType == "理货员")
                {
                    continue;
                }

                List<WorkloadAccount> list = getlist.Where(u => u.WorkType == item.WorkType).ToList();
                if (list.Count > 0)
                {
                    //如果有理货员的信息 就可以调整立方数量
                    List<WorkloadAccount> lhyList = getlist.Where(u => u.WorkType == "理货员").ToList();
                    if (lhyList.Count > 0)
                    {
                        //得到理货员的立方信息
                        WorkloadAccount lhy = lhyList.First();

                        decimal? qty = lhyList.Sum(u => u.Qty);    //总数量
                        decimal? cbm = lhyList.Sum(u => u.CBM);    //总体积
                        decimal? weight = lhyList.Sum(u => u.Weight); //总重量

                        string[] s = (from workloadaccount in list
                                      select workloadaccount.UserCode).Distinct().ToArray();

                        int count = s.Length;
                        if (count == 0)
                        {
                            count = 1;
                        }
                        decimal? avgCbm = cbm / count;
                        decimal? avgQty = qty / count;
                        decimal? avgWeight = weight / count;

                        idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.WorkType == item.WorkType);

                        for (int i = 0; i < s.Length; i++)
                        {
                            WorkloadAccount work = new WorkloadAccount();
                            work.WhCode = lhy.WhCode;
                            work.ClientId = lhy.ClientId;
                            work.ClientCode = lhy.ClientCode;
                            work.ReceiptId = lhy.ReceiptId;
                            work.HuId = "";
                            work.WorkType = item.WorkType;
                            work.UserCode = s[i];
                            work.LotFlag = lhy.LotFlag;
                            work.ReceiptDate = lhy.ReceiptDate;
                            work.CBM = avgCbm;
                            work.Qty = Convert.ToDecimal(avgQty);
                            work.Weight = avgWeight;

                            listAdd.Add(work);
                        }
                    }
                }
            }

            idal.IWorkloadAccountDAL.Add(listAdd);
        }

        //批量删除工人工号
        public string DelWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                TranLog tl1 = new TranLog();
                tl1.TranType = "66";
                tl1.Description = "删除工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.ReceiptId = item.ReceiptId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);
            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }

            InUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }

        //新增工人工号
        public string AddWorkloadAccount(List<WorkloadAccountResult> entity, string userCode)
        {
            string result = "";

            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                List<WorkloadAccount> checkwork = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId && u.WorkType == "理货员");
                if (checkwork.Count == 0)
                {
                    result = "未找到工作量数据，无法新增！";
                    break;
                }
                else
                {
                    WorkloadAccount lhy = checkwork.First();

                    WorkloadAccount work = new WorkloadAccount();
                    work.WhCode = item.WhCode;
                    work.ClientId = lhy.ClientId;
                    work.ClientCode = lhy.ClientCode;
                    work.ReceiptId = item.ReceiptId;
                    work.HuId = "";
                    work.WorkType = item.WorkType;
                    work.UserCode = item.UserCode;
                    work.LotFlag = lhy.LotFlag;
                    work.ReceiptDate = lhy.ReceiptDate;
                    work.CBM = lhy.CBM;
                    work.Qty = lhy.Qty;
                    work.Weight = lhy.Weight;
                    idal.IWorkloadAccountDAL.Add(work);

                    TranLog tl1 = new TranLog();
                    tl1.TranType = "67";
                    tl1.Description = "新增工作量";
                    tl1.TranDate = DateTime.Now;
                    tl1.TranUser = item.UpdateUser;
                    tl1.WhCode = item.WhCode;
                    tl1.ReceiptId = item.ReceiptId;
                    tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType;
                    tranLogList.Add(tl1);
                }
            }
            if (result != "")
            {
                return result;
            }

            idal.ITranLogDAL.Add(tranLogList);
            idal.SaveChanges();

            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }

            InUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }


        //--------------出货修改工作量---------------------------------------------------------------
        public string EditWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                //如果立方不一致
                if (!string.IsNullOrEmpty(item.cbm.ToString()))
                {
                    List<WorkloadAccount> getlist = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                    decimal? getCbm = getlist.Sum(u => u.CBM);
                    decimal? getbaQty = getlist.Sum(u => (u.BaQty ?? 0));

                    if (item.cbm != getCbm)
                    {
                        WorkloadAccount lhy = getlist.First();

                        idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                        WorkloadAccount work = new WorkloadAccount();
                        work.WhCode = lhy.WhCode;
                        work.ClientId = lhy.ClientId;
                        work.ClientCode = lhy.ClientCode;
                        work.LoadId = lhy.LoadId;
                        work.HuId = "";
                        work.WorkType = item.WorkType;
                        work.UserCode = lhy.UserCode;
                        work.LotFlag = lhy.LotFlag;
                        work.ReceiptDate = lhy.ReceiptDate;
                        work.CBM = item.cbm;
                        work.Qty = 0;
                        work.Weight = 0;
                        work.BaQty = item.baQty ?? 0;

                        idal.IWorkloadAccountDAL.Add(work);
                        idal.SaveChanges();

                        List<WorkloadAccountResult> distinctResult1 = (from a in idal.IWorkloadAccountDAL.SelectAll()
                                                                       where a.WhCode == item.WhCode && a.LoadId == item.LoadId && a.WorkType != item.WorkType
                                                                       select new WorkloadAccountResult
                                                                       {
                                                                           WhCode = a.WhCode,
                                                                           LoadId = a.LoadId,
                                                                           WorkType = a.WorkType
                                                                       }).Distinct().ToList();

                        OutUpdateWorkAccountByWorkType(distinctResult1);

                        TranLog tl2 = new TranLog();
                        tl2.TranType = "65";
                        tl2.Description = "修改工作量";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = item.UpdateUser;
                        tl2.WhCode = item.WhCode;
                        tl2.LoadId = item.LoadId;
                        tl2.Remark = "立方修改为" + item.cbm;
                        tranLogList.Add(tl2);
                    }
                }
                //如果把数不一致
                if (!string.IsNullOrEmpty(item.baQty.ToString()))
                {
                    List<WorkloadAccount> getlist = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                    decimal? getCbm = getlist.Sum(u => u.CBM);
                    decimal? getbaQty = getlist.Sum(u => (u.BaQty ?? 0));

                    if (item.baQty != getbaQty)
                    {
                        WorkloadAccount lhy = getlist.First();

                        idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                        LoadMaster loadMaster = new LoadMaster();
                        loadMaster.BaQty = (Int32?)item.baQty ?? 0;
                        idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == item.LoadId && u.WhCode == item.WhCode, new string[] { "BaQty" });

                        WorkloadAccount work = new WorkloadAccount();
                        work.WhCode = lhy.WhCode;
                        work.ClientId = lhy.ClientId;
                        work.ClientCode = lhy.ClientCode;
                        work.LoadId = lhy.LoadId;
                        work.HuId = "";
                        work.WorkType = item.WorkType;
                        work.UserCode = lhy.UserCode;
                        work.LotFlag = lhy.LotFlag;
                        work.ReceiptDate = lhy.ReceiptDate;
                        work.CBM = item.cbm ?? 0;
                        work.Qty = 0;
                        work.Weight = 0;
                        work.BaQty = item.baQty ?? 0;

                        idal.IWorkloadAccountDAL.Add(work);
                        idal.SaveChanges();

                        List<WorkloadAccountResult> distinctResult1 = (from a in idal.IWorkloadAccountDAL.SelectAll()
                                                                       where a.WhCode == item.WhCode && a.LoadId == item.LoadId && a.WorkType != item.WorkType
                                                                       select new WorkloadAccountResult
                                                                       {
                                                                           WhCode = a.WhCode,
                                                                           LoadId = a.LoadId,
                                                                           WorkType = a.WorkType
                                                                       }).Distinct().ToList();

                        OutUpdateWorkAccountByWorkType(distinctResult1);

                        TranLog tl2 = new TranLog();
                        tl2.TranType = "65";
                        tl2.Description = "修改工作量";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = item.UpdateUser;
                        tl2.WhCode = item.WhCode;
                        tl2.LoadId = item.LoadId;
                        tl2.Remark = "把数修改为" + item.baQty;
                        tranLogList.Add(tl2);
                    }
                }

                idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType, u => new WorkloadAccount() { UserCode = userCode });

                TranLog tl1 = new TranLog();
                tl1.TranType = "65";
                tl1.Description = "修改工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.LoadId = item.LoadId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType + "修改为工号" + userCode;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);
            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.LoadId == item.LoadId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }

            OutUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }

        //批量修改出货工人
        public string EditWorkloadAccount1List(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType, u => new WorkloadAccount() { UserCode = userCode });

                TranLog tl1 = new TranLog();
                tl1.TranType = "65";
                tl1.Description = "修改工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.LoadId = item.LoadId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType + "修改为工号" + userCode;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);

            idal.SaveChanges();
            return "Y";
        }

        //调整其它工种工作量方法
        private void OutUpdateWorkAccountByWorkType(List<WorkloadAccountResult> distinctResult)
        {
            WorkloadAccountResult first = distinctResult.First();

            List<WorkloadAccount> getlist = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == first.WhCode && u.LoadId == first.LoadId);

            foreach (var item in distinctResult)
            {
                if (item.WorkType == "理货员")
                {
                    continue;
                }

                List<WorkloadAccount> list = getlist.Where(u => u.WorkType == item.WorkType).ToList();
                if (list.Count > 0)
                {
                    //如果有理货员的信息 就可以调整立方数量
                    List<WorkloadAccount> lhyList = getlist.Where(u => u.WorkType == "理货员").ToList();
                    if (lhyList.Count > 0)
                    {
                        //得到理货员的立方信息
                        WorkloadAccount lhy = lhyList.First();

                        decimal? qty = lhyList.Sum(u => u.Qty);    //总数量
                        decimal? cbm = lhyList.Sum(u => u.CBM);    //总体积
                        decimal? weight = lhyList.Sum(u => u.Weight); //总重量
                        decimal? baqty = lhyList.Sum(u => u.BaQty);    //总把数

                        string[] s = (from workloadaccount in list
                                      select workloadaccount.UserCode).Distinct().ToArray();

                        int count = s.Length;
                        if (count == 0)
                        {
                            count = 1;
                        }
                        decimal? avgCbm = cbm / count;
                        decimal? avgQty = qty / count;
                        decimal? avgWeight = weight / count;
                        decimal? avgBaQty = baqty / count;

                        idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.WorkType == item.WorkType);

                        for (int i = 0; i < s.Length; i++)
                        {
                            WorkloadAccount work = new WorkloadAccount();
                            work.WhCode = lhy.WhCode;
                            work.ClientId = lhy.ClientId;
                            work.ClientCode = lhy.ClientCode;
                            work.LoadId = lhy.LoadId;
                            work.HuId = "";
                            work.WorkType = item.WorkType;
                            work.UserCode = s[i];
                            work.LotFlag = lhy.LotFlag;
                            work.ReceiptDate = lhy.ReceiptDate;
                            work.CBM = avgCbm;
                            work.Qty = Convert.ToDecimal(avgQty);
                            work.Weight = avgWeight;
                            if (item.WorkType != "叉车工")
                            {
                                work.BaQty = avgBaQty;
                            }

                            idal.IWorkloadAccountDAL.Add(work);
                        }
                    }
                }
            }
        }

        //批量删除工人工号
        public string DelWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                idal.IWorkloadAccountDAL.DeleteByExtended(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.UserCode == item.UserCode && u.WorkType == item.WorkType);

                TranLog tl1 = new TranLog();
                tl1.TranType = "66";
                tl1.Description = "删除工作量";
                tl1.TranDate = DateTime.Now;
                tl1.TranUser = item.UpdateUser;
                tl1.WhCode = item.WhCode;
                tl1.LoadId = item.LoadId;
                tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType;
                tranLogList.Add(tl1);
            }
            idal.ITranLogDAL.Add(tranLogList);
            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.LoadId == item.LoadId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }

            OutUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }

        //新增工人工号
        public string AddWorkloadAccount1(List<WorkloadAccountResult> entity, string userCode)
        {
            string result = "";

            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in entity)
            {
                List<WorkloadAccount> checkwork = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.WorkType == "理货员");
                if (checkwork.Count == 0)
                {
                    result = "未找到工作量数据，无法新增！";
                    break;
                }
                else
                {
                    WorkloadAccount lhy = checkwork.First();

                    WorkloadAccount work = new WorkloadAccount();
                    work.WhCode = item.WhCode;
                    work.ClientId = lhy.ClientId;
                    work.ClientCode = lhy.ClientCode;
                    work.LoadId = item.LoadId;
                    work.HuId = "";
                    work.WorkType = item.WorkType;
                    work.UserCode = item.UserCode;
                    work.LotFlag = lhy.LotFlag;
                    work.ReceiptDate = lhy.ReceiptDate;
                    work.CBM = lhy.CBM;
                    work.Qty = lhy.Qty;
                    work.Weight = lhy.Weight;
                    idal.IWorkloadAccountDAL.Add(work);

                    TranLog tl1 = new TranLog();
                    tl1.TranType = "67";
                    tl1.Description = "新增工作量";
                    tl1.TranDate = DateTime.Now;
                    tl1.TranUser = item.UpdateUser;
                    tl1.WhCode = item.WhCode;
                    tl1.LoadId = item.LoadId;
                    tl1.Remark = "工人" + item.UserCode + " 类型" + item.WorkType;
                    tranLogList.Add(tl1);
                }
            }
            if (result != "")
            {
                return result;
            }
            idal.ITranLogDAL.Add(tranLogList);
            idal.SaveChanges();

            List<WorkloadAccountResult> distinctResult = new List<WorkloadAccountResult>();

            foreach (var item in entity)
            {
                if (distinctResult.Where(u => u.WorkType == item.WorkType && u.WhCode == item.WhCode && u.LoadId == item.LoadId).Count() == 0)
                {
                    distinctResult.Add(item);
                }
            }
            OutUpdateWorkAccountByWorkType(distinctResult);

            idal.SaveChanges();
            return "Y";
        }


        #endregion


        #region 16.收出货订单流程修改管理

        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string type)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                      on new { Id = a.Id } equals new { Id = (Int32)b.BusinessFlowGroupId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.ClientId == clientId && b.Type == type && b.WhCode == whCode
                      select a;
            return sql.AsEnumerable();
        }

        public List<ReceiptRegisterResult> GetFlowHeadListByRec(ReceiptRegisterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new ReceiptRegisterResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          Status =
                           a.Status == "N" ? "未释放" :
                           a.Status == "U" ? "未收货" :
                           a.Status == "A" ? "正在收货" :
                           a.Status == "P" ? "暂停收货" :
                           a.Status == "C" ? "完成收货" : null,
                          ProcessName = a.ProcessName,
                          SumQty = a.SumQty,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId.Contains(searchEntity.ReceiptId));
            if (searchEntity.ClientId > 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.TruckNumber))
                sql = sql.Where(u => u.TruckNumber.Contains(searchEntity.TruckNumber));
            if (!string.IsNullOrEmpty(searchEntity.PhoneNumber))
                sql = sql.Where(u => u.PhoneNumber.Contains(searchEntity.PhoneNumber));

            if (!string.IsNullOrEmpty(searchEntity.Status))
            {
                sql = sql.Where(u => u.Status == searchEntity.Status);
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public List<LoadMasterResult> GetFlowHeadListByLoad(LoadMasterSearch searchEntity, out int total)
        {
            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                       from c in c_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       select new LoadMasterResult
                       {
                           Id = a.Id,
                           LoadId = a.LoadId,
                           ClientCode = c.ClientCode,
                           ClientId = c.ClientId,
                           Status0 =
                            a.Status0 == "U" ? "未释放" :
                            a.Status0 == "C" ? "已释放" : null,
                           Status1 =
                            a.Status1 == "U" ? "未备货" :
                            a.Status1 == "A" ? "正在备货" :
                            a.Status1 == "C" ? "完成备货" : null,
                           ProcessId = a.ProcessId,
                           ProcessName = a.ProcessName,
                           SumQty = a.SumQty,
                           DSSumQty = a.DSSumQty,
                           CreateDate = a.CreateDate
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.Status0))
                sql = sql.Where(u => u.Status0 == searchEntity.Status0);
            if (!string.IsNullOrEmpty(searchEntity.Status1))
                sql = sql.Where(u => u.Status1 == searchEntity.Status1);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public string EditProcessName(string number, string whCode, int processId, string processName, string type, string userCode)
        {
            if (type == "InBound")
            {
                List<ReceiptRegister> checkList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == number && u.Status == "U");
                if (checkList.Count == 0)
                {
                    return "收货批次状态有误，请重新查询！";
                }
                else
                {
                    ReceiptRegister first = checkList.First();

                    idal.IReceiptRegisterDAL.UpdateByExtended(u => u.WhCode == whCode && u.ReceiptId == number, u => new ReceiptRegister() { ProcessName = processName, ProcessId = processId });

                    TranLog tl = new TranLog();
                    tl.TranType = "73";
                    tl.Description = "修改收货流程";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userCode;
                    tl.WhCode = whCode;
                    tl.Remark = "流程:" + first.ProcessId + first.ProcessName + "改为：" + processId + "," + processName;
                    tl.ReceiptId = number;
                    idal.ITranLogDAL.Add(tl);
                }
            }
            else
            {
                List<LoadMaster> checkList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == number && u.Status1 == "U");
                if (checkList.Count == 0)
                {
                    return "Load状态有误，请重新查询！";
                }
                else
                {
                    LoadMaster load = checkList.First();
                    idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == whCode && u.LoadId == number, u => new LoadMaster() { ProcessName = processName, ProcessId = processId });

                    TranLog tl = new TranLog();
                    tl.TranType = "74";
                    tl.Description = "修改出货流程";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userCode;
                    tl.WhCode = whCode;
                    tl.Remark = "流程:" + load.ProcessId + load.ProcessName + "改为：" + processId + "," + processName;
                    tl.LoadId = number;
                    idal.ITranLogDAL.Add(tl);
                }
            }

            return "Y";
        }


        #endregion


        #region 17.报表单据管理
        public List<CRTemplate> GetCRTemplate(CRReportSearch searchEntity, out int total)
        {
            var sql = from a in idal.ICRTemplateDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.TemplateName))
                sql = sql.Where(u => u.TemplateName.Contains(searchEntity.TemplateName));
            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.Type == searchEntity.Type);
            if (!string.IsNullOrEmpty(searchEntity.Description))
                sql = sql.Where(u => u.Description.Contains(searchEntity.Description));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            List<CRTemplate> list = sql.ToList();
            foreach (var item in list)
            {
                if (item.Type == "In")
                {
                    item.Type = "收货";
                }
                else if (item.Type == "Rec")
                {
                    item.Type = "收货凭证";
                }
                else if (item.Type == "InDS")
                {
                    item.Type = "收货直装";
                }
                else if (item.Type == "Out")
                {
                    item.Type = "出货";
                }
            }

            return list;
        }


        //流程修改
        public String CrystallReportEdit(CRTemplate entity)
        {
            if (idal.ICRTemplateDAL.SelectBy(u => u.Id == entity.Id).ToList().Count != 0)
            {
                entity.UpdateDate = DateTime.Now;

                idal.ICRTemplateDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[]
                    { "TemplateName","Description","UpdateUser","UpdateDate"});

                idal.ICRTemplateDAL.UpdateBy(entity, u => u.WhCode == entity.WhCode, new string[]
                       { "Url"});
                idal.ICRTemplateDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "N";
            }
        }

        //新增
        public string CrystallReportAdd(CRTemplate entity)
        {
            if (idal.ICRTemplateDAL.SelectBy(u => u.WhCode == entity.WhCode && u.TemplateName == entity.TemplateName).Count == 0)
            {

                entity.CreateDate = DateTime.Now;
                idal.ICRTemplateDAL.Add(entity);
                idal.ICRTemplateDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "单据已存在！";
            }
        }


        #endregion


        #region 18.Edi任务基础数据管理
        //得到仓库下的所有Edi基础名称
        public IEnumerable<UrlEdi> UrlEdiSelect(string whCode)
        {
            var sql = from a in idal.IUrlEdiDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            return sql.AsEnumerable();
        }

        //查询列表
        public List<UrlEdi> UrlEdiList(UrlEdiSearch searchEntity, out int total)
        {
            var sql = from a in idal.IUrlEdiDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.Url))
                sql = sql.Where(u => u.Url.Contains(searchEntity.Url));


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();

        }

        //流程修改
        public String UrlEdiEdit(UrlEdi entity)
        {
            if (idal.IUrlEdiDAL.SelectBy(u => u.Id == entity.Id).ToList().Count != 0)
            {
                idal.IUrlEdiDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[]
                    { "Url"});

                idal.IUrlEdiDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "N";
            }
        }

        //新增
        public string UrlEdiAdd(UrlEdi entity)
        {
            entity.HttpType = "Get";
            idal.IUrlEdiDAL.Add(entity);
            idal.IUrlEdiDAL.SaveChanges();
            return "Y";

        }


        #endregion


        #region 19.highcharts数据管理(分析柱状图)

        //库位使用率
        public List<Highcharts> LocRateList(String WhCode)
        {

            var sql1 = from a in idal.IWhLocationDAL.SelectAll()
                       where a.WhCode == WhCode && a.LocationTypeId == 1
                       join b in idal.IHuMasterDAL.SelectAll() on new { A = a.WhCode, B = a.LocationId } equals new { A = b.WhCode, B = b.Location }
                       into c_join
                       from c in c_join.DefaultIfEmpty()
                       select new { A = a.Location, AA = a.LocationId, B = c.Location };
            var sql2 = from a in sql1
                       group a by a.A into g
                       where g.Select(u => u.B).Distinct().Count() > 200
                       select new Highcharts { Code = g.Key, Value2 = g.Select(u => u.AA).Distinct().Count().ToString(), Value1 = g.Select(u => u.B).Distinct().Count().ToString() };
            sql2 = sql2.OrderBy(u => u.Code);

            return sql2.ToList();
        }

        public List<Highcharts> InvRateList(String WhCode)
        {

            var sql1 = from a in idal.IHuDetailDAL.SelectAll()
                       join b in idal.IHuMasterDAL.SelectAll() on new { a.WhCode, a.HuId } equals new { b.WhCode, b.HuId }
                       join c in idal.IZoneDAL.SelectAll() on new { A = b.Location.Substring(0, 1) + "库", B = a.WhCode } equals new { A = c.ZoneName, B = c.WhCode }
                       where a.WhCode == WhCode && !a.UnitName.Contains("ECH") && c.RegFlag == 1
                       select new { ZoneName = c.ZoneName, CBM = a.Qty * a.Length * a.Width * a.Height, ZoneCBM = c.ZoneCBM }
                       ;
            var sql2 = from a in sql1
                       group a by new { a.ZoneName, a.ZoneCBM } into g
                       select new Highcharts { Code = g.Key.ZoneName, Value2 = g.Key.ZoneCBM.ToString(), Value1 = Math.Round((Double)g.Sum(x => x.CBM), 0).ToString() };


            sql2 = sql2.OrderBy(u => u.Code);

            return sql2.ToList();
        }

        public List<HighchartClient> ClientInvRateList(String WhCode)
        {
            var sql = from a in (
                              (from a0 in idal.IWhClientDAL.SelectAll()
                               join c in idal.IZoneDAL.SelectAll() on new { ZoneId = (Int32)a0.ZoneId } equals new { ZoneId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IR_WhClient_WhAgentDAL.SelectAll() on new { Id = a0.Id } equals new { Id = d.ClientId } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IWhAgentDAL.SelectAll() on new { AgentId = d.AgentId } equals new { AgentId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join b in (
                                             (from a01 in idal.IHuDetailDAL.SelectAll()
                                              where a01.WhCode == WhCode
                                              select new
                                              {
                                                  a01.WhCode,
                                                  Actual_Stock_CBM =
                                               (a01.UnitName ?? "").ToString().Substring(0, 3) == "ECH" ? 0 : (a01.Qty * a01.Length * a01.Width * a01.Height),
                                                  a01.ClientId
                                              })
                                          )
                                     on new { a0.Id, a0.WhCode }
                               equals new { Id = (Int32)b.ClientId, b.WhCode } into b_join
                               from b in b_join.DefaultIfEmpty()
                               where a0.WhCode == WhCode
                               group new { a0, c, e, b } by new
                               {
                                   a0.ClientCode,
                                   a0.WarnCBM,
                                   c.ZoneName,
                                   e.AgentName
                               } into g
                               select new
                               {
                                   g.Key.ClientCode,
                                   Actual_Stock_CBM = g.Sum(p => (p.b.Actual_Stock_CBM)),
                                   Alert_level_CBM = (Decimal?)g.Key.WarnCBM,
                                   WHS_Chamber = g.Key.ZoneName,
                                   Forwarder = g.Key.AgentName
                               }))
                      where (Int64)a.Actual_Stock_CBM > 0
                      orderby
                        a.Actual_Stock_CBM descending
                      select new HighchartClient
                      {
                          ClientCode = a.ClientCode,
                          Value1 = Math.Round((double)a.Actual_Stock_CBM, 2).ToString(),
                          Value2 = Math.Round((double)a.Alert_level_CBM, 2).ToString(),
                          Value3 = a.WHS_Chamber,
                          Value4 = a.Forwarder
                      };

            return sql.ToList();
        }

        #endregion


        #region 20.客户类型管理


        //客户类型查询列表----------------------
        public List<WhClientType> WhClientTypeList(WhClientTypeSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhClientTypeDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ClientType))
                sql = sql.Where(u => u.ClientType.Contains(searchEntity.ClientType));


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();

        }

        //修改客户类型
        public string WhClientTypeEdit(WhClientType entity)
        {
            if (idal.IWhClientTypeDAL.SelectBy(u => u.Id == entity.Id).ToList().Count != 0)
            {
                WhClientType first = idal.IWhClientTypeDAL.SelectBy(u => u.Id == entity.Id).First();

                WhClient client = new WhClient();
                client.ClientType = "";
                idal.IWhClientDAL.UpdateBy(client, u => u.WhCode == first.WhCode && u.ClientType == first.ClientType, new string[] { "ClientType" });


                idal.IWhClientTypeDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[]
                    { "ClientType"});

                idal.IWhClientTypeDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "N";
            }
        }

        //新增客户类型
        public string WhClientTypeAdd(WhClientType entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.IWhClientTypeDAL.Add(entity);
            idal.IWhClientTypeDAL.SaveChanges();
            return "Y";

        }

        //删除客户类型
        public int WhClientTypeDel(int id)
        {
            WhClientType first = idal.IWhClientTypeDAL.SelectBy(u => u.Id == id).First();

            WhClient client = new WhClient();
            client.ClientType = "";
            idal.IWhClientDAL.UpdateBy(client, u => u.WhCode == first.WhCode && u.ClientType == first.ClientType, new string[] { "ClientType" });

            idal.IWhClientTypeDAL.DeleteBy(u => u.Id == id);
            idal.IWhClientTypeDAL.SaveChanges();
            return 1;
        }

        //客户类型下拉菜单列表
        public IEnumerable<WhClientType> WhClientTypeListSelect(string whCode)
        {
            var sql = from a in idal.IWhClientTypeDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.Id);
            return sql.AsEnumerable();
        }

        #endregion


        #region 21.夜班区间管理


        //客户类型查询列表
        public List<NightTime> NightTimeList(NightTimeSearch searchEntity, out int total)
        {
            var sql = from a in idal.INightTimeDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增夜班区间
        public string NightTimeAdd(NightTime entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.INightTimeDAL.Add(entity);
            idal.INightTimeDAL.SaveChanges();
            return "Y";
        }

        //删除夜班区间
        public int NightTimeDel(int id)
        {
            NightTime first = idal.INightTimeDAL.SelectBy(u => u.Id == id).First();
            string nightTime = first.NightBegin + "-" + first.NightEnd;

            WhClient client = new WhClient();
            client.NightTime = "";
            idal.IWhClientDAL.UpdateBy(client, u => u.WhCode == first.WhCode && u.NightTime == nightTime, new string[] { "NightTime" });

            idal.INightTimeDAL.DeleteBy(u => u.Id == id);
            idal.INightTimeDAL.SaveChanges();
            return 1;
        }

        //修改夜班区间
        public string NightTimeEdit(NightTime entity)
        {
            if (idal.INightTimeDAL.SelectBy(u => u.Id == entity.Id).ToList().Count != 0)
            {
                NightTime first = idal.INightTimeDAL.SelectBy(u => u.Id == entity.Id).First();
                string nightTime = first.NightBegin + "-" + first.NightEnd;

                WhClient client = new WhClient();
                client.NightTime = "";
                idal.IWhClientDAL.UpdateBy(client, u => u.WhCode == first.WhCode && u.NightTime == nightTime, new string[] { "NightTime" });


                idal.INightTimeDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[]
                    { "NightBegin","NightEnd"});

                idal.INightTimeDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "N";
            }
        }

        //夜班区间下拉菜单列表
        public IEnumerable<NightTime> NightTimeListSelect(string whCode)
        {
            var sql = from a in idal.INightTimeDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.Id);
            return sql.AsEnumerable();
        }

        #endregion


        #region 22.道口收费节假日管理

        //查询
        public List<Holiday> HolidayList(HolidaySearch searchEntity, out int total)
        {
            var sql = from a in idal.IHolidayDAL.SelectAll()
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.HolidayName))
                sql = sql.Where(u => u.HolidayName.Contains(searchEntity.HolidayName));


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //节假日导入
        public string HolidayImports(string[] holiday, string[] dayBegin, string whCode, string userName)
        {

            List<Holiday> HolidayListAdd = new List<Holiday>();

            for (int j = 0; j < holiday.Count(); j++)
            {
                Holiday entity = new Holiday();
                entity.HolidayName = holiday[j];
                entity.DayBegin = dayBegin[j];

                entity.CreateUser = userName;
                entity.CreateDate = DateTime.Now;
                HolidayListAdd.Add(entity);   //款号不存在就新增

            }
            idal.IHolidayDAL.Add(HolidayListAdd);
            idal.IHolidayDAL.SaveChanges();
            return "";

        }

        //修改信息
        public string HolidayEdit(Holiday entity)
        {
            idal.IHolidayDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "DayBegin" });
            idal.SaveChanges();
            return "Y";
        }


        #endregion


        #region 23.收货合同管理

        //查询
        public List<ContractForm> ContractFormList(ContractFormSearch searchEntity, out int total)
        {
            var sql = from a in idal.IContractFormDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ContractName))
                sql = sql.Where(u => u.ContractName.Contains(searchEntity.ContractName));
            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.Type.Contains(searchEntity.Type));
            if (!string.IsNullOrEmpty(searchEntity.ChargeName))
                sql = sql.Where(u => u.ChargeName.Contains(searchEntity.ChargeName));

            total = sql.Count();
            sql = sql.OrderBy(u => u.ContractName).ThenBy(u => u.Type).ThenBy(u => u.ChargeName);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //合同导入
        public string ContractFormImports(List<ContractForm> entityList)
        {
            ContractForm first = entityList.First();
            string[] contractName = (from a in entityList
                                     select a.ContractName).ToList().Distinct().ToArray();

            List<ContractForm> checkList = idal.IContractFormDAL.SelectBy(u => contractName.Contains(u.ContractName) && u.WhCode == first.WhCode && u.ActiveDateBegin == first.ActiveDateBegin && u.ActiveDateEnd == first.ActiveDateEnd);
            if (checkList.Count > 0)
            {
                string[] getcontractName = (from a in checkList
                                            select a.ContractName).ToList().Distinct().ToArray();

                string checkResult = "";
                foreach (var item in getcontractName)
                {
                    checkResult += "合同名：" + item + "已存在！";
                }

                return checkResult;
            }

            idal.IContractFormDAL.Add(entityList);
            idal.IContractFormDAL.SaveChanges();
            return "Y";

        }

        //删除
        public int ContractFormDeleteAll(string contractName, string whCode, string userName)
        {
            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "870";
            tranLog.Description = "删除收货合同";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = userName;
            tranLog.WhCode = whCode;
            tranLog.ClientCode = "";
            tranLog.SoNumber = "";
            tranLog.CustomerPoNumber = "";
            tranLog.AltItemNumber = "";
            tranLog.ItemId = 0;
            tranLog.UnitID = 0;
            tranLog.UnitName = "";
            tranLog.TranQty = 0;
            tranLog.TranQty2 = 0;
            tranLog.HuId = "";
            tranLog.Length = 0;
            tranLog.Width = 0;
            tranLog.Height = 0;
            tranLog.Weight = 0;
            tranLog.LotNumber1 = "";
            tranLog.LotNumber2 = "";
            tranLog.ReceiptId = "";
            tranLog.Location = "";
            tranLog.Location2 = "";
            tranLog.HoldId = 0;
            tranLog.HoldReason = "";
            tranLog.Remark = "删除收货合同：" + contractName;
            idal.ITranLogDAL.Add(tranLog);

            idal.IContractFormDAL.DeleteBy(u => u.WhCode == whCode && u.ContractName == contractName);
            idal.IContractFormDAL.SaveChanges();
            return 1;
        }



        //修改信息
        public string ContractFormEdit(ContractForm entity)
        {
            ContractForm getOld = idal.IContractFormDAL.SelectBy(u => u.Id == entity.Id).First();

            //得到原始数据 进行日志添加
            TranLog tranLog = new TranLog();
            tranLog.TranType = "875";
            tranLog.Description = "修改收货合同";
            tranLog.TranDate = DateTime.Now;
            tranLog.TranUser = entity.CreateUser;
            tranLog.WhCode = entity.WhCode;
            tranLog.ClientCode = "";
            tranLog.SoNumber = "";
            tranLog.CustomerPoNumber = "";
            tranLog.AltItemNumber = "";
            tranLog.ItemId = 0;
            tranLog.UnitID = 0;
            tranLog.UnitName = "";
            tranLog.TranQty = 0;
            tranLog.TranQty2 = 0;
            tranLog.HuId = "";
            tranLog.Length = 0;
            tranLog.Width = 0;
            tranLog.Height = 0;
            tranLog.Weight = 0;
            tranLog.LotNumber1 = "";
            tranLog.LotNumber2 = "";
            tranLog.ReceiptId = "";
            tranLog.Location = "";
            tranLog.Location2 = "";
            tranLog.HoldId = 0;
            tranLog.HoldReason = "";
            tranLog.Remark = "修改收货合同:" + entity.ContractName + "原单价为:" + getOld.Price + "修改为:" + entity.Price;
            idal.ITranLogDAL.Add(tranLog);

            idal.IContractFormDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "LadderNumberBegin", "LadderNumberEnd", "LadderNumberBeginCBM", "LadderNumberEndCBM", "Price", "MaxPriceTotal", "Ratio", "ActiveDateBegin", "ActiveDateEnd" });
            idal.SaveChanges();
            return "Y";
        }

        //合同下拉菜单列表
        public IEnumerable<string> ContractNameListSelect(string whCode)
        {
            var sql = (from a in idal.IContractFormDAL.SelectAll()
                       where a.WhCode == whCode
                       select a.ContractName).Distinct();

            return sql.AsEnumerable();
        }


        #endregion


        #region 24.TCR费用管理

        //查询
        public List<FeeMaster> FeeMaseterList(FeeMasterSearch searchEntity, string[] soNumberList, out int total, out string str)
        {
            var sql = from a in idal.IFeeMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (soNumberList != null)
                sql = sql.Where(u => soNumberList.Contains(u.SoNumber) || soNumberList.Contains(u.LoadId));

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.FeeNumber))
                sql = sql.Where(u => u.FeeNumber == searchEntity.FeeNumber);
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);
            if (!string.IsNullOrEmpty(searchEntity.Type))
                sql = sql.Where(u => u.Type == searchEntity.Type);

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            if (searchEntity.BeginFeeCreateDate != null)
            {
                sql = sql.Where(u => u.UpdateDate >= searchEntity.BeginFeeCreateDate);
            }
            if (searchEntity.EndFeeCreateDate != null)
            {
                sql = sql.Where(u => u.UpdateDate <= searchEntity.EndFeeCreateDate);
            }
            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
            {
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);
            }

            List<FeeMaster> list = sql.ToList();
            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == searchEntity.WhCode && u.Status == "Active");
            foreach (var item in list)
            {
                if (clientList.Where(u => u.ClientCode == item.ClientCode).Count() > 0)
                {
                    item.ClientCodeCN = clientList.Where(u => u.ClientCode == item.ClientCode).First().ClientName;
                }
            }

            total = list.Count;
            str = "";
            if (total > 0)
            {
                str = "{\"结算款\":\"" + sql.Sum(u => u.JieSuanFee).ToString() + "\"}";
            }

            if (!string.IsNullOrEmpty(searchEntity.InvoiceNumberOrderBy))
            {
                list = list.OrderBy(u => u.InvoiceNumber).ToList();
            }
            else
            {
                list = list.OrderByDescending(u => u.Id).ToList();
            }

            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //添加正常TCR费用
        public FeeMaster FeeMaseterAdd(FeeMaster entity)
        {
            if (entity.ClientCode == null || entity.ClientCode == "" || entity.WhCode == "" || entity.LocationId == "" || entity.LocationId == null)
            {
                return null;
            }

            entity.Type = "TCR";
            entity.FeeNumber = "FM" + DI.IDGenerator.NewId;
            entity.CreateDate = DateTime.Now;
            entity.Status = "草稿";
            entity.LuruFee = 0;
            entity.YuShouFee = 0;
            entity.JieSuanFee = 0;

            idal.IFeeMasterDAL.Add(entity);
            idal.IFeeMasterDAL.SaveChanges();
            return entity;
        }

        //添加收货特殊费用
        public string FeeMaseterSpecialAdd(FeeMaster entity, int type)
        {
            if (entity.ClientCode == null || entity.ClientCode == "" || entity.WhCode == "" || entity.LocationId == "" || entity.LocationId == null)
            {
                return "录入信息有误，请重新添加！";
            }

            //需要检测是否是节假日及周末、白班时间
            if (type == 1)
            {
                string date = Convert.ToDateTime(DateTime.Now).ToString("d");
                List<FeeHoliday> feeholidayList = idal.IFeeHolidayDAL.SelectAll().ToList();

                if (feeholidayList.Where(u => u.DayBegin == date && u.Type == 0).Count() > 0)
                {

                }
                else if (feeholidayList.Where(u => u.DayBegin == date && u.Type == 1).Count() > 0)
                {
                    if (DateTime.Now >= Convert.ToDateTime(date + " 09:00:00") && (DateTime.Now < Convert.ToDateTime(date + " 17:00:00")))
                    {
                        return "正常班时间请联系客服处理！";
                    }
                }
                else
                {
                    var checkWeekDate = Convert.ToDateTime(date).DayOfWeek;
                    if (checkWeekDate == DayOfWeek.Sunday || checkWeekDate == DayOfWeek.Saturday)
                    {
                        //周末

                    }
                    else
                    {
                        if (DateTime.Now >= Convert.ToDateTime(date + " 09:00:00") && (DateTime.Now < Convert.ToDateTime(date + " 17:00:00")))
                        {
                            return "正常班时间请联系客服处理！";
                        }
                    }
                }
            }

            List<ReceiptRegister> list = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
            if (list.Count == 0)
            {
                return "收货批次未找到登记信息，请检查是否输入有误！";
            }
            ReceiptRegister fir = list.First();
            if (entity.ClientCode != fir.ClientCode)
            {
                return "客户名匹配有误，请重新选择！";
            }


            entity.SoNumber = entity.ReceiptId;
            entity.Type = "现场操作";
            entity.FeeNumber = "FM" + DI.IDGenerator.NewId;
            entity.CreateDate = DateTime.Now;
            entity.Status = "未预收款";
            entity.YuShouFee = 0;
            entity.JieSuanFee = 0;

            idal.IFeeMasterDAL.Add(entity);

            ReceiptChargeDetail regDetail = new ReceiptChargeDetail();
            regDetail.WhCode = entity.WhCode;
            regDetail.ClientCode = entity.ClientCode;
            regDetail.ReceiptId = entity.ReceiptId;
            regDetail.ChargeType = "卸货附加费";
            regDetail.ChargeName = "现场操作费";
            regDetail.TransportType = "";
            regDetail.UnitName = "个";
            regDetail.NightTimeFlag = "否";

            regDetail.Qty = 1;
            regDetail.CBM = 0;
            regDetail.Weight = 0;
            regDetail.ChargeUnitName = "个";
            regDetail.LadderNumber = entity.Description;

            regDetail.Price = entity.LuruFee;
            regDetail.PriceTotal = entity.LuruFee;
            regDetail.HolidayFlag = "否";
            regDetail.MonthlyFlag = "否";
            regDetail.CreateUser = entity.CreateUser;
            regDetail.CreateDate = DateTime.Now;
            idal.IReceiptChargeDetailDAL.Add(regDetail);

            idal.IFeeMasterDAL.SaveChanges();
            return "Y";
        }


        //添加出货提货特殊费用
        public string FeeMasterOutLoadAdd(FeeMaster entity)
        {
            if (entity.ClientCode == null || entity.ClientCode == "" || entity.WhCode == "")
            {
                return "录入信息有误，请重新添加！";
            }

            if (entity.LocationId == null || entity.LocationId == "")
            {
                var sql = (from a in idal.IWhClientDAL.SelectAll()
                           join b in idal.IZoneDAL.SelectAll() on new { ZoneId = (Int32)a.ZoneId } equals new { ZoneId = b.Id } into b_join
                           from b in b_join.DefaultIfEmpty()
                           where a.WhCode == entity.WhCode && b.RegFlag == 1 && a.ClientCode == entity.ClientCode
                           select new WhZoneResult
                           {
                               Id = b.Id,
                               ZoneName = b.ZoneName
                           }).OrderByDescending(u => u.Id);
                entity.LocationId = sql.First().ZoneName;
            }

            List<LoadMaster> list = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId);
            if (list.Count == 0)
            {
                return "未找到Load信息，请检查是否输入有误！";
            }

            List<FeeMaster> checkList = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId);
            if (checkList.Count > 0)
            {
                return "出货提货费用已录入，无法重复录入！";
            }

            entity.SoNumber = entity.LoadId;
            entity.Type = "出货提货";
            entity.FeeNumber = "FM" + DI.IDGenerator.NewId;
            entity.CreateDate = DateTime.Now;
            entity.Status = "未预收款";
            entity.YuShouFee = 0;
            entity.JieSuanFee = 0;

            idal.IFeeMasterDAL.Add(entity);
            idal.IFeeMasterDAL.SaveChanges();
            return "Y";
        }

        //费用明细列表
        public List<FeeDetail> FeeDetailList(FeeDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IFeeDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId.Contains(searchEntity.HuId));
            if (!string.IsNullOrEmpty(searchEntity.FeeNumber))
                sql = sql.Where(u => u.FeeNumber == searchEntity.FeeNumber);
            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId.Contains(searchEntity.LocationId));

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }

            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加费用明细
        public string FeeDetailAdd(FeeDetail entity)
        {
            if (entity.FeeNumber == null)
            {
                return "N";
            }
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，无法添加明细！";
            }

            FeeMaster fir = list.First();
            if (fir.Status == "草稿")
            {
                entity.OperationHours = 0;
                entity.OperationQty = 0;
                entity.FactoryUserCount = 0;
                entity.CreateDate = DateTime.Now;

                List<FeeDetailResult> feeList = (from feedetail in
                                           (from feedetail in idal.IFeeDetailDAL.SelectAll()
                                            where feedetail.WhCode == fir.WhCode && feedetail.FeeNumber == fir.FeeNumber
                                            select new
                                            {
                                                Column1 = (System.Decimal?)(feedetail.Price * feedetail.Qty) + (feedetail.ChangDiFee + feedetail.PeopleFee + feedetail.TruckFee + feedetail.DaDanFee + feedetail.OtherFee),
                                                Dummy = "x"
                                            })
                                                 group feedetail by new { feedetail.Dummy } into g
                                                 select new FeeDetailResult
                                                 {
                                                     TotalPrice = (System.Decimal?)g.Sum(p => p.Column1)
                                                 }).ToList();

                decimal? totalprice = (entity.Price * entity.Qty) + entity.ChangDiFee + entity.PeopleFee + entity.TruckFee + entity.DaDanFee + entity.OtherFee;
                if (feeList.Count == 0)
                {
                    fir.LuruFee = totalprice;
                }
                else
                {
                    fir.LuruFee = feeList.First().TotalPrice + totalprice;
                }

                idal.IFeeDetailDAL.Add(entity);
                idal.IFeeMasterDAL.UpdateBy(fir, u => u.Id == fir.Id, new string[] { "LuruFee" });

                idal.IFeeDetailDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "当前状态异常，请重新查询！";
            }
        }

        //添加费用明细
        public string FeeDetailAddList(List<FeeDetail> entityList)
        {
            FeeDetail entity = entityList.First();
            if (entity.FeeNumber == null)
            {
                return "N";
            }
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，无法添加明细！";
            }

            FeeMaster fir = list.First();
            if (fir.Status == "草稿")
            {
                decimal? totalprice = 0;
                foreach (var item in entityList)
                {
                    item.CreateDate = DateTime.Now;
                    item.OperationHours = 0;
                    item.OperationQty = 0;
                    item.FactoryUserCount = 0;

                    decimal? s = (item.Price * item.Qty) + item.ChangDiFee + item.PeopleFee + item.TruckFee + item.DaDanFee + item.OtherFee;

                    totalprice = s + totalprice;
                }

                List<FeeDetailResult> feeList = (from feedetail in
                                          (from feedetail in idal.IFeeDetailDAL.SelectAll()
                                           where feedetail.WhCode == fir.WhCode && feedetail.FeeNumber == fir.FeeNumber
                                           select new
                                           {
                                               Column1 = (System.Decimal?)(feedetail.Price * feedetail.Qty) + (feedetail.ChangDiFee + feedetail.PeopleFee + feedetail.TruckFee + feedetail.DaDanFee + feedetail.OtherFee),
                                               Dummy = "x"
                                           })
                                                 group feedetail by new { feedetail.Dummy } into g
                                                 select new FeeDetailResult
                                                 {
                                                     TotalPrice = (System.Decimal?)g.Sum(p => p.Column1)
                                                 }).ToList();


                if (feeList.Count == 0)
                {
                    fir.LuruFee = totalprice;
                }
                else
                {
                    fir.LuruFee = feeList.First().TotalPrice + totalprice;
                }

                idal.IFeeDetailDAL.Add(entityList);
                idal.IFeeMasterDAL.UpdateBy(fir, u => u.Id == fir.Id, new string[] { "LuruFee" });

                idal.IFeeDetailDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "当前状态异常，请重新查询！";
            }
        }

        //删除全部费用信息
        public string FeeMaseterDel(string feeNumber, string whCode, string userName)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == whCode && u.FeeNumber == feeNumber);
            if (list.Count == 0)
            {
                return "Y";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status != "草稿")
                {
                    return "当前状态异常，请重新查询！";
                }
                else
                {
                    if (fir.Type == "现场操作")
                    {
                        idal.IReceiptChargeDetailDAL.DeleteBy(u => u.WhCode == whCode && u.ReceiptId == fir.ReceiptId && u.ChargeName == "现场操作费");
                    }

                    idal.IFeeMasterDAL.DeleteBy(u => u.WhCode == whCode && u.FeeNumber == feeNumber);
                    idal.IFeeDetailDAL.DeleteBy(u => u.WhCode == whCode && u.FeeNumber == feeNumber);


                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "640";
                    tl.Description = "TCR费用删除";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.SoNumber;
                    tl.AltItemNumber = fir.SoNumber;
                    tl.ClientCode = fir.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.Remark = fir.LuruFee.ToString();

                    idal.ITranLogDAL.Add(tl);

                    idal.IFeeMasterDAL.SaveChanges();
                    return "Y";
                }
            }
        }

        //删除费用明细
        public string FeeDetailDel(int id, string userName)
        {
            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.Id == id);
            if (list.Count == 0)
            {
                return "Y";
            }
            else
            {
                FeeDetail fir = list.First();
                FeeMaster master = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber).First();

                if (master.Status != "草稿")
                {
                    return "当前状态异常，请重新查询！";
                }
                else
                {
                    List<FeeDetail> checkList = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber && u.Id != id);
                    if (checkList.Count == 0)
                    {
                        master.LuruFee = 0;
                        idal.IFeeMasterDAL.UpdateBy(master, u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber, new string[] { "LuruFee" });
                    }
                    else
                    {
                        master.LuruFee = master.LuruFee - fir.TotalPrice - fir.ChangDiFee - fir.PeopleFee - fir.TruckFee - fir.DaDanFee - fir.OtherFee; ;
                        idal.IFeeMasterDAL.UpdateBy(master, u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber, new string[] { "LuruFee" });
                    }


                    idal.IFeeDetailDAL.DeleteBy(u => u.Id == id);

                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "641";
                    tl.Description = "TCR费用明细删除";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.CustomerPoNumber;
                    tl.AltItemNumber = fir.AltItemNumber;
                    tl.ClientCode = master.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.HuId = fir.HuId;
                    tl.Location = fir.LocationId;
                    tl.Remark = fir.Price.ToString() + "/" + fir.Qty.ToString() + "/" + fir.TotalPrice.ToString();

                    idal.ITranLogDAL.Add(tl);

                    idal.IFeeMasterDAL.SaveChanges();
                    return "Y";
                }
            }
        }

        //客服确认状态未预收款
        public string FeeMaseterEditStatus(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "草稿")
                {
                    entity.Status = "未预收款";
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Status" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //客服撤销确认状态未预收款
        public string FeeMaseterEditStatus1(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "未预收款")
                {
                    entity.Status = "草稿";
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Status" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //客服订单确认作废 
        public string FeeMaseterEditStatus2(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "已预收款" || fir.Status == "未结算")
                {
                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "646";
                    tl.Description = "TCR费用订单作废";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = entity.CreateUser;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.SoNumber;
                    tl.AltItemNumber = fir.SoNumber;
                    tl.ClientCode = fir.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.Remark = entity.Description;

                    idal.ITranLogDAL.Add(tl);

                    entity.Status = "已作废";



                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Status", "Description" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //修改信息
        public string FeeMaseterEdit(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "草稿")
                {
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "SoNumber", "Description" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //修改费用备注
        public string FeeMasterRemarkEdit(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status != "完成")
                {
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "LuruFee", "Description" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }


        //道口预收款更新状态
        public string FeeMaseterDKEdit(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "未预收款")
                {
                    FeeDetailResult fee = (from feedetail in
                                            (from feedetail in idal.IFeeDetailDAL.SelectAll()
                                             where feedetail.WhCode == fir.WhCode && feedetail.FeeNumber == fir.FeeNumber
                                             select new
                                             {
                                                 Column1 = (System.Decimal?)(feedetail.Price * feedetail.Qty) + (feedetail.ChangDiFee + feedetail.PeopleFee + feedetail.TruckFee + feedetail.DaDanFee + feedetail.OtherFee),
                                                 Dummy = "x"
                                             })
                                           group feedetail by new { feedetail.Dummy } into g
                                           select new FeeDetailResult
                                           {
                                               TotalPrice = (System.Decimal?)g.Sum(p => p.Column1)
                                           }).ToList().First();

                    if (entity.LuruFee != fir.LuruFee && entity.LuruFee != fee.TotalPrice)
                    {
                        return "当前录入总价已变更，请重新确认预收款！";
                    }

                    entity.Status = "已预收款";
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Status", "YuShouFee", "YuShouUser" });

                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "645";
                    tl.Description = "TCR预收款";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = entity.YuShouUser;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.SoNumber;
                    tl.AltItemNumber = fir.SoNumber;
                    tl.ClientCode = fir.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.Remark = entity.YuShouFee.ToString();

                    idal.ITranLogDAL.Add(tl);

                    try
                    {
                        idal.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        string s = e.InnerException.Message;
                    }

                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //道口费用结算
        public string FeeMaseterDKJiesuanEdit(FeeMaster entity, int type)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "未结算" || type == 1)
                {
                    entity.Status = "完成";
                    entity.UpdateDate = DateTime.Now;
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == fir.Id, new string[] { "Status", "JieSuanFee", "JieSuanUser", "BillingType", "InvoiceType", "NoNumber", "InvoiceNumber", "InvoiceTopContent", "UpdateDate" });

                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "645";
                    tl.Description = "TCR结算收款";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = entity.JieSuanUser;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.SoNumber;
                    tl.AltItemNumber = fir.SoNumber;
                    tl.ClientCode = fir.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.Remark = entity.JieSuanFee.ToString();

                    idal.ITranLogDAL.Add(tl);

                    List<FeeDetail> listDetail = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);

                    foreach (var item in listDetail)
                    {
                        PhotoMaster photo = new PhotoMaster();
                        photo.TCRStatus = "已处理";
                        photo.TCRCheckUser = item.CreateUser;
                        photo.TCRCheckDate = DateTime.Now;
                        photo.TCRProcessMode = item.TCRProcessMode;
                        photo.SettlementMode = "现金";
                        photo.CheckStatus1 = "Y";
                        photo.CheckUser1 = item.CreateUser;
                        photo.CheckDate1 = item.CreateDate;
                        photo.KRemark1 = item.Description;

                        photo.CheckStatus2 = "Y";
                        photo.CheckUser2 = item.OperationUser;
                        photo.CheckDate2 = item.UpdateDate;
                        photo.UpdateUser = "1008";
                        photo.UpdateDate = DateTime.Now;

                        idal.IPhotoMasterDAL.UpdateBy(photo, u => u.WhCode == item.WhCode && u.ClientCode == fir.ClientCode && u.Number2 == item.SoNumber && u.Number3 == item.CustomerPoNumber && u.Number4 == item.AltItemNumber && u.HuId == item.HuId);
                    }
                    try
                    {
                        idal.SaveChanges();
                        return "Y";
                    }
                    catch (Exception e)
                    {
                        string s = e.InnerException.Message;
                        return s;
                    }
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }


        //道口修改发票信息
        public string FeeMaseterInvoiceEdit(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster fir = list.First();
                if (fir.Status == "完成")
                {
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == fir.Id, new string[] { "BillingType", "InvoiceType", "NoNumber", "InvoiceNumber", "InvoiceTopContent" });

                    idal.IFeeMasterDAL.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //客服修改费用明细信息
        public string FeeDetailEdit(FeeDetail entity)
        {
            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeDetail fir = list.First();

                FeeMaster master = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber).First();

                if (master.Status == "草稿")
                {
                    master.LuruFee = master.LuruFee - (fir.Qty * fir.Price) - fir.ChangDiFee - fir.PeopleFee - fir.TruckFee - fir.DaDanFee - fir.OtherFee;
                    master.LuruFee = master.LuruFee + entity.TotalPrice + entity.ChangDiFee + entity.PeopleFee + entity.TruckFee + entity.DaDanFee + entity.OtherFee;

                    idal.IFeeMasterDAL.UpdateBy(master, u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber, new string[] { "LuruFee" });

                    entity.UpdateDate = DateTime.Now;

                    idal.IFeeDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "HuId", "LocationId", "CustomerPoNumber", "AltItemNumber", "Price", "Qty", "TotalPrice", "SoNumber", "Description", "UpdateUser", "UpdateDate", "ChangDiFee", "PeopleFee", "TruckFee", "DaDanFee", "OtherFee", "ChangDiHours" });
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }


        //仓库修改操作数量
        public string FeeDetailCKEdit(FeeDetail entity)
        {
            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.Id == entity.Id);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeDetail fir = list.First();

                FeeMaster master = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber).First();

                if (master.Status == "已预收款" || master.Status == "未结算")
                {
                    entity.UpdateUser = entity.OperationUser;
                    entity.UpdateDate = DateTime.Now;
                    idal.IFeeDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "OperationQty", "OperationUser", "FactoryUserCount", "UpdateUser", "UpdateDate" });

                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前已操作完成，无法再修改信息！";
                }
            }
        }

        //仓库修改操作时间
        public string FeeMasterCKEditBeginEndDate(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                FeeMaster master = list.First();

                if (master.Status == "未结算")
                {
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == master.Id, new string[] { "OperationBeginDate", "OperationEndDate" });

                    TCRFeeCalculate(entity);
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态有误，请重新查询！";
                }
            }
        }

        //仓库确认状态
        public string ConfirmFeeMasterCK(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，请重新查询！";
            }
            else
            {
                List<FeeDetail> detaillist = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
                if (detaillist.Where(u => (u.OperationUser ?? "") == "").Count() > 0)
                {
                    foreach (var item in detaillist.Where(u => (u.OperationUser ?? "") == ""))
                    {
                        item.OperationQty = item.Qty;
                        item.OperationUser = entity.Description;
                        item.UpdateUser = entity.Description;
                        item.UpdateDate = DateTime.Now;
                        idal.IFeeDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "OperationQty", "OperationUser", "UpdateUser", "UpdateDate" });
                    }

                }
                FeeMaster fir = list.First();
                if (fir.Status == "已预收款")
                {
                    entity.Status = "未结算";
                    idal.IFeeMasterDAL.UpdateBy(entity, u => u.Id == fir.Id, new string[] { "Status", "OperationBeginDate", "OperationEndDate" });

                    TCRFeeCalculate(entity);
                    idal.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //仓库删除耗材费用明细
        public string FeeDetailDelCKLoss(int id, string userName)
        {
            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.Id == id);
            if (list.Count == 0)
            {
                return "Y";
            }
            else
            {
                FeeDetail fir = list.First();
                FeeMaster master = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == fir.WhCode && u.FeeNumber == fir.FeeNumber).First();

                if (master.Status == "已预收款" || master.Status == "未结算")
                {
                    idal.IFeeDetailDAL.DeleteBy(u => u.Id == id);

                    //添加日志
                    TranLog tl = new TranLog();
                    tl.TranType = "641";
                    tl.Description = "仓库耗材费用明细删除";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;

                    tl.WhCode = fir.WhCode;
                    tl.SoNumber = fir.SoNumber;
                    tl.CustomerPoNumber = fir.CustomerPoNumber;
                    tl.AltItemNumber = fir.AltItemNumber;
                    tl.ClientCode = master.ClientCode;

                    tl.ReceiptId = fir.FeeNumber;
                    tl.OutPoNumber = fir.FeeNumber;
                    tl.CustomerOutPoNumber = fir.FeeNumber;
                    tl.LoadId = fir.FeeNumber;
                    tl.HuId = fir.HuId;
                    tl.Location = fir.LocationId;
                    tl.Remark = fir.Price.ToString() + "/" + fir.Qty.ToString() + "/" + fir.TotalPrice.ToString();

                    idal.ITranLogDAL.Add(tl);

                    idal.IFeeMasterDAL.SaveChanges();
                    return "Y";
                }
                else
                {
                    return "当前状态异常，请重新查询！";
                }
            }
        }

        //仓库添加耗材费用明细
        public string FeeDetailAddCKLoss(FeeDetail entity)
        {
            if (entity.FeeNumber == null)
            {
                return "N";
            }
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到信息，无法添加明细！";
            }

            FeeMaster fir = list.First();
            if (fir.Status == "已预收款" || fir.Status == "未结算")
            {
                entity.HuId = "";
                entity.LocationId = "";
                entity.SoNumber = "";
                entity.CustomerPoNumber = "";
                entity.AltItemNumber = "";
                entity.TCRProcessMode = "仓库耗材";

                entity.Price = 0;
                entity.Qty = 1;
                entity.TotalPrice = 0;

                entity.OtherFee = entity.OtherFee;
                entity.OperationHours = 0;
                entity.OperationQty = 0;
                entity.FactoryUserCount = 0;
                entity.CreateDate = DateTime.Now;
                entity.CreateUser = entity.UpdateUser;

                idal.IFeeDetailDAL.Add(entity);

                idal.IFeeDetailDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "当前状态异常，请重新查询！";
            }
        }

        //计算TCR费用
        public void TCRFeeCalculate(FeeMaster entity)
        {
            TimeSpan operHour1 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);
            int operHour = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(operHour1.TotalHours)));
            FeeDetail updateFeeDetail = new FeeDetail();
            updateFeeDetail.OperationHours = operHour;
            idal.IFeeDetailDAL.UpdateBy(updateFeeDetail, u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber, new string[] { "OperationHours" });

            decimal normalH, normalNightH, weekendH, statutoryHolidayH;
            GetHours(entity, out normalH, out normalNightH, out weekendH, out statutoryHolidayH);

            List<FeeDetail> peoplelist = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber && u.PeopleFee > 0).ToList();
            foreach (var item in peoplelist)
            {
                item.PeopleNormalFee = normalH * item.PeopleFee;

                //人员监管附加费
                item.PeopleFujiaFee = (statutoryHolidayH * 3 * item.PeopleFee) + (weekendH * 2 * item.PeopleFee) + (normalNightH * Convert.ToDecimal(1.5) * item.PeopleFee);

                item.PeopleNormalHours = normalH;
                item.PeopleNormalNightHours = normalNightH;
                item.PeopleWeekendHours = weekendH;
                item.PeopleStatutoryHolidayHours = statutoryHolidayH;

                idal.IFeeDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "PeopleNormalFee", "PeopleFujiaFee", "PeopleNormalHours", "PeopleNormalNightHours", "PeopleWeekendHours", "PeopleStatutoryHolidayHours" });
            }

            List<FeeDetail> pricelist = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber && u.Price > 0).ToList();

            foreach (var item in pricelist)
            {
                //设备使用附加费，周末是托盘费双倍
                if (statutoryHolidayH > 0)
                {
                    item.EquipmentUseFee = (2 * item.Price);
                }
                else if (weekendH > 0)
                {
                    item.EquipmentUseFee = (1 * item.Price);
                }
                else if (normalNightH > 0)
                {
                    item.EquipmentUseFee = (Convert.ToDecimal(0.5) * item.Price);
                }
                else
                {
                    item.EquipmentUseFee = 0;
                }

                idal.IFeeDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "EquipmentUseFee" });
            }
        }

        //重新计算TCR费用
        public string AgainTCRFeeCalculate(FeeMaster entity)
        {
            List<FeeMaster> list = idal.IFeeMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.FeeNumber == entity.FeeNumber);
            if (list.Count == 0)
            {
                return "未找到费用信息，请重新查询！";
            }

            FeeMaster first = list.First();
            TCRFeeCalculate(first);
            idal.SaveChanges();
            return "Y";
        }

        //得到操作小时数
        private void GetHours(FeeMaster entity, out decimal normalH, out decimal normalNightH, out decimal weekendH, out decimal statutoryHolidayH)
        {
            //验证结束时间
            //正常上班时间：9点-17点
            //1.大于17点的 超出部份人员监管费 周一至周五的 1.5倍
            //2.周六周日 2倍
            //3.国家法定节假日 3倍  FeeHoliday表

            List<FeeHoliday> feeholidayList = idal.IFeeHolidayDAL.SelectAll().Distinct().ToList();
            string begindatetimeNow = Convert.ToDateTime(entity.OperationBeginDate).ToString("d");    //2020/1/1 格式化
            string enddatetimeNow = Convert.ToDateTime(entity.OperationEndDate).ToString("d");    //2020/1/1 格式化

            normalH = 0;
            normalNightH = 0;
            weekendH = 0;
            statutoryHolidayH = 0;
            var checkWeekDate = Convert.ToDateTime(begindatetimeNow).DayOfWeek;

            string s = Convert.ToDateTime(entity.OperationBeginDate).ToString("mm");
            int minbegintime = Convert.ToInt32(Convert.ToDateTime(entity.OperationBeginDate).ToString("mm"));
            int minendtime = Convert.ToInt32(Convert.ToDateTime(entity.OperationEndDate).ToString("mm"));

            bool checkMin = false;
            if (minendtime <= minbegintime)
            {
                checkMin = true;
            }

            //如果是隔天才结束操作
            if (begindatetimeNow != enddatetimeNow)
            {
                //------------------------计算开始时间使用小时------------------------------
                //法定节假日
                if (feeholidayList.Where(u => u.DayBegin == begindatetimeNow && u.Type == 0).Count() > 0)
                {
                    TimeSpan t4 = Convert.ToDateTime(begindatetimeNow).AddDays(1) - Convert.ToDateTime(entity.OperationBeginDate);

                    statutoryHolidayH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                }
                else if (feeholidayList.Where(u => u.DayBegin == begindatetimeNow && u.Type == 1).Count() > 0)
                {
                    //调班
                    if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {

                        TimeSpan t4 = Convert.ToDateTime(entity.OperationBeginDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 17:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                        normalNightH += 7;
                    }
                    else
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours)) + 7;
                        normalH += 8;
                    }
                }
                else if (checkWeekDate == DayOfWeek.Sunday || checkWeekDate == DayOfWeek.Saturday)
                {
                    //周末
                    TimeSpan t4 = Convert.ToDateTime(entity.OperationBeginDate) - Convert.ToDateTime(begindatetimeNow + " 00:00:00");

                    weekendH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                }
                else
                {
                    //正常班
                    if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationBeginDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 17:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                        normalNightH += 7;
                    }
                    else
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours)) + 7;
                        normalH += 8;
                    }
                }

                //------------------------计算结束时间使用小时------------------------------
                //2020-10-07 12:10至2020-10-09 10:30 假设：10月7日是法定节假日，10月8日是周末但也是调班，10月9日周末
                for (int i = 1; i < 30; i++)
                {
                    string checkDate = Convert.ToDateTime(entity.OperationBeginDate).AddDays(i).ToString("d");

                    checkWeekDate = Convert.ToDateTime(checkDate).DayOfWeek;
                    if (checkDate == enddatetimeNow)
                    {
                        //法定节假日
                        if (feeholidayList.Where(u => u.DayBegin == checkDate && u.Type == 0).Count() > 0)
                        {
                            if (checkMin)
                            {
                                entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                            }

                            TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 00:00:00");

                            statutoryHolidayH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                        }
                        else if (feeholidayList.Where(u => u.DayBegin == checkDate && u.Type == 1).Count() > 0)
                        {
                            //调班
                            if (Convert.ToDateTime(entity.OperationEndDate) >= Convert.ToDateTime(checkDate + " 17:00:00"))
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 17:00:00");

                                normalH += 8;
                                normalNightH += 9 + Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                            }
                            else if (Convert.ToDateTime(entity.OperationEndDate) >= Convert.ToDateTime(checkDate + " 09:00:00"))
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 09:00:00");
                                normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                                normalNightH += 9;
                            }
                            else
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 00:00:00");
                                normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                            }
                        }
                        else if (checkWeekDate == DayOfWeek.Sunday || checkWeekDate == DayOfWeek.Saturday)
                        {
                            //周末
                            if (checkMin)
                            {
                                entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                            }

                            TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 00:00:00");

                            weekendH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                        }
                        else
                        {
                            //正常班
                            if (Convert.ToDateTime(entity.OperationEndDate) >= Convert.ToDateTime(checkDate + " 17:00:00"))
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 17:00:00");

                                normalH += 8;
                                normalNightH += 9 + Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                            }
                            else if (Convert.ToDateTime(entity.OperationEndDate) >= Convert.ToDateTime(checkDate + " 09:00:00"))
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 09:00:00");
                                normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                                normalNightH += 9;
                            }
                            else
                            {
                                if (checkMin)
                                {
                                    entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                                }

                                TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(checkDate + " 00:00:00");
                                normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                            }
                        }

                        break;
                    }
                    else
                    {
                        if (feeholidayList.Where(u => u.DayBegin == checkDate && u.Type == 0).Count() > 0)
                        {
                            //法定节假日
                            statutoryHolidayH += 24;
                        }
                        else if (feeholidayList.Where(u => u.DayBegin == checkDate && u.Type == 1).Count() > 0)
                        {
                            //调班
                            normalH += 8;
                            normalNightH += 16;
                        }
                        else if (checkWeekDate == DayOfWeek.Sunday || checkWeekDate == DayOfWeek.Saturday)
                        {
                            //周末
                            weekendH += 24;
                        }
                        else
                        {
                            //调班
                            normalH += 8;
                            normalNightH += 16;

                        }
                    }
                }

            }
            else
            {
                //如果操作是当天结束

                if (feeholidayList.Where(u => u.DayBegin == begindatetimeNow && u.Type == 0).Count() > 0)
                {
                    TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                    statutoryHolidayH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                }
                else if (feeholidayList.Where(u => u.DayBegin == begindatetimeNow && u.Type == 1).Count() > 0)
                {
                    //调班
                    if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationEndDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) > Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 17:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        TimeSpan t44 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) <= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) > Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t44 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        normalH += 8;

                        TimeSpan t45 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t45.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) <= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t44 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 09:00:00");
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));

                    }

                }
                else if (checkWeekDate == DayOfWeek.Sunday || checkWeekDate == DayOfWeek.Saturday)
                {
                    //周末
                    TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                    weekendH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                }
                else
                {
                    //正常班
                    if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationEndDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);

                        normalNightH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) > Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(begindatetimeNow + " 17:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        TimeSpan t44 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) >= Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) <= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(entity.OperationBeginDate);
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));
                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) > Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t44 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        normalH += 8;

                        TimeSpan t45 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 17:00:00");
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t45.TotalHours));

                    }
                    else if (Convert.ToDateTime(entity.OperationBeginDate) < Convert.ToDateTime(begindatetimeNow + " 09:00:00") && Convert.ToDateTime(entity.OperationEndDate) <= Convert.ToDateTime(begindatetimeNow + " 17:00:00"))
                    {
                        TimeSpan t44 = Convert.ToDateTime(begindatetimeNow + " 09:00:00") - Convert.ToDateTime(entity.OperationBeginDate);
                        normalNightH += Math.Ceiling(Convert.ToDecimal(t44.TotalHours));

                        if (checkMin)
                        {
                            entity.OperationEndDate = Convert.ToDateTime(Convert.ToDateTime(entity.OperationEndDate).ToString("d") + " " + Convert.ToDateTime(entity.OperationEndDate).ToString("HH") + ":00:00");
                        }

                        TimeSpan t4 = Convert.ToDateTime(entity.OperationEndDate) - Convert.ToDateTime(begindatetimeNow + " 09:00:00");
                        normalH += Math.Ceiling(Convert.ToDecimal(t4.TotalHours));


                    }
                }
            }
        }

        //得到实际操作费用
        public string getOperationFee(string feeNumber, string whCode)
        {
            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == whCode && u.FeeNumber == feeNumber);
            decimal? TotalPrice = 0;
            foreach (var item in list)
            {
                decimal? Price = item.Price * item.OperationQty;
                decimal? Price1 = item.TruckFee ?? 0;
                decimal? Price2 = item.DaDanFee ?? 0;
                decimal? Price3 = item.OtherFee ?? 0;
                decimal? Price4 = (item.PeopleNormalFee ?? 0) + (item.PeopleFujiaFee ?? 0);
                decimal? Price6 = item.EquipmentUseFee ?? 0;

                TotalPrice = Price + Price1 + Price2 + Price3 + Price4 + TotalPrice + Price6;

                if (item.ChangDiHours == 4)
                {
                    decimal Price5 = Math.Ceiling(Convert.ToDecimal((item.OperationHours ?? 0) / item.ChangDiHours));

                    TotalPrice += Price5 * (item.ChangDiFee ?? 0);
                }
                else
                {
                    TotalPrice += ((item.OperationHours ?? 0) * (item.ChangDiFee ?? 0));
                }
            }

            TotalPrice = Math.Ceiling(Convert.ToDecimal(TotalPrice * Convert.ToDecimal(1.06)));

            int sr = Convert.ToInt32(TotalPrice.ToString().Substring(TotalPrice.ToString().Length - 1, 1));
            if (sr == 0)
            {

            }
            else if (sr < 5)
            {
                TotalPrice = TotalPrice - sr + 5;
            }
            else if (sr == 5)
            {

            }
            else if (sr > 5 & sr < 10)
            {
                TotalPrice = TotalPrice - sr + 10;
            }

            return TotalPrice.ToString();
        }

        //得到实际操作费用列表 
        public List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total)
        {
            List<FeeDetailResult1> resultList = new List<FeeDetailResult1>();

            List<FeeDetail> list = idal.IFeeDetailDAL.SelectBy(u => u.WhCode == whCode && u.FeeNumber == feeNumber);

            decimal? TotalPrice = 0;
            foreach (var item in list)
            {
                FeeDetailResult1 entity = new FeeDetailResult1();
                entity.TCRProcessMode = item.TCRProcessMode;
                entity.Price = item.Price;
                entity.Qty = item.Qty;
                entity.OperationQty = item.OperationQty;
                entity.OperationHours = item.OperationHours;

                entity.OperationQtyFee = item.Price * item.OperationQty;
                if (item.ChangDiHours == 4)
                {
                    decimal Price5 = Math.Ceiling(Convert.ToDecimal((item.OperationHours ?? 0) / item.ChangDiHours));

                    entity.ChangDiFee = Price5 * (item.ChangDiFee ?? 0);
                }
                else
                {
                    entity.ChangDiFee = ((item.OperationHours ?? 0) * (item.ChangDiFee ?? 0));
                }

                entity.TruckFee = item.TruckFee ?? 0;
                entity.DaDanFee = item.DaDanFee ?? 0;
                entity.OtherFee = item.OtherFee ?? 0;
                entity.PeopleNormalFee = item.PeopleNormalFee ?? 0;
                entity.PeopleFujiaFee = item.PeopleFujiaFee ?? 0;
                entity.EquipmentUseFee = item.EquipmentUseFee ?? 0;

                entity.PeopleNormalHours = item.PeopleNormalHours;
                entity.PeopleNormalNightHours = item.PeopleNormalNightHours;
                entity.PeopleWeekendHours = item.PeopleWeekendHours;
                entity.PeopleStatutoryHolidayHours = item.PeopleStatutoryHolidayHours;

                entity.TotalPrice = entity.OperationQtyFee + entity.ChangDiFee + entity.TruckFee + entity.DaDanFee + entity.OtherFee + entity.PeopleNormalFee + entity.PeopleFujiaFee + entity.EquipmentUseFee;

                TotalPrice += entity.TotalPrice;
                resultList.Add(entity);
            }

            FeeDetailResult1 entity1 = new FeeDetailResult1();
            entity1.HSTotalPrice = Math.Ceiling(Convert.ToDecimal(TotalPrice * Convert.ToDecimal(1.06)));
            entity1.TotalPrice = TotalPrice;
            resultList.Add(entity1);

            total = resultList.Count;
            return resultList;
        }

        //查询库存TCR且不在费用明细内
        public List<FeeDetailHuDetailListResult> HuDetailListByPOSKU(FeeDetailSearch searchEntity, string[] soList, string[] poList, string[] skuList, string[] huList, out int total, out string str)
        {
            var sql = from a in (
                                (from a0 in idal.IHuDetailDAL.SelectAll()
                                 join b in idal.IHuMasterDAL.SelectAll()
                                       on new { a0.WhCode, a0.HuId }
                                   equals new { b.WhCode, b.HuId }
                                 where a0.WhCode == searchEntity.WhCode && (b.Type == "M" || b.Type == "R")
                                 select new
                                 {
                                     a0.ClientCode,
                                     a0.ReceiptId,
                                     WhCode = b.WhCode,
                                     HuId = b.HuId,
                                     Location = b.Location,
                                     a0.SoNumber,
                                     a0.CustomerPoNumber,
                                     a0.AltItemNumber,
                                     Qty = a0.UnitName == "ECH-THIN" ? a0.Qty : a0.UnitName == "ECH-THICK" ? a0.Qty : 1,
                                     HoldReason = b.HoldReason
                                 }).Distinct())
                      join c in (
                        from a in idal.IFeeDetailDAL.SelectAll()
                        join b in idal.IFeeMasterDAL.SelectAll()
                              on new { a.WhCode, a.FeeNumber }
                          equals new { b.WhCode, b.FeeNumber }
                        where
                             b.Status != "完成" && b.Status != "已作废"
                        select new
                        {
                            a.WhCode,
                            a.HuId,
                            a.SoNumber,
                            a.CustomerPoNumber,
                            a.AltItemNumber
                        }
                      )
                        on new { a.WhCode, a.HuId, a.SoNumber, a.CustomerPoNumber, a.AltItemNumber }
                    equals new { c.WhCode, c.HuId, c.SoNumber, c.CustomerPoNumber, c.AltItemNumber } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where
                        a.WhCode == searchEntity.WhCode &&
                        a.ClientCode == searchEntity.ClientCode &&
                        c.HuId == null
                      select new FeeDetailHuDetailListResult
                      {
                          Action = "",
                          ClientCode = a.ClientCode,
                          ReceiptId = a.ReceiptId,
                          HuId = a.HuId,
                          LocationId = a.Location,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = a.AltItemNumber,
                          Qty = a.Qty,
                          HoldReason = a.HoldReason
                      };

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);

            if (soList != null)
                sql = sql.Where(u => soList.Contains(u.SoNumber));
            if (poList != null)
                sql = sql.Where(u => poList.Contains(u.CustomerPoNumber));
            if (skuList != null)
                sql = sql.Where(u => skuList.Contains(u.AltItemNumber));
            if (huList != null)
                sql = sql.Where(u => huList.Contains(u.HuId));

            List<FeeDetailHuDetailListResult> list = sql.ToList();

            total = list.Count;

            str = "";
            if (total > 0)
            {
                str = "{\"库存数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //查询库存TCR根据托盘条件检索
        public FeeDetailHuDetailListResult GetHuDetailByHuId(string huId, string whCode, string clientCode)
        {
            var sql = from a in (
                               (from a0 in idal.IHuDetailDAL.SelectAll()
                                join b in idal.IHuMasterDAL.SelectAll()
                                      on new { a0.WhCode, a0.HuId }
                                  equals new { b.WhCode, b.HuId }
                                where a0.WhCode == whCode && (b.Type == "M" || b.Type == "R")
                                select new
                                {
                                    a0.ClientCode,
                                    a0.ReceiptId,
                                    WhCode = b.WhCode,
                                    HuId = b.HuId,
                                    Location = b.Location,
                                    a0.SoNumber,
                                    a0.CustomerPoNumber,
                                    a0.AltItemNumber,
                                    a0.UnitName,
                                    a0.Qty,
                                    HoldReason = b.HoldReason
                                }))
                      join c in (
                        from a in idal.IFeeDetailDAL.SelectAll()
                        join b in idal.IFeeMasterDAL.SelectAll()
                              on new { a.WhCode, a.FeeNumber }
                          equals new { b.WhCode, b.FeeNumber }
                        where
                             b.Status != "完成" && b.Status != "已作废"
                        select new
                        {
                            a.WhCode,
                            a.HuId,
                            a.SoNumber,
                            a.CustomerPoNumber,
                            a.AltItemNumber
                        }
                      )
                        on new { a.WhCode, a.HuId, a.SoNumber, a.CustomerPoNumber, a.AltItemNumber }
                    equals new { c.WhCode, c.HuId, c.SoNumber, c.CustomerPoNumber, c.AltItemNumber } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where
                        a.WhCode == whCode &&
                        a.ClientCode == clientCode &&
                        a.HuId == huId &&
                        c.HuId == null
                      select new FeeDetailHuDetailListResult
                      {
                          HuId = a.HuId,
                          LocationId = a.Location,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = a.AltItemNumber,
                          Qty = a.UnitName == "ECH-THIN" ? a.Qty :
                                a.UnitName == "ECH-THICK" ? a.Qty : 1
                      };
            List<FeeDetailHuDetailListResult> list = sql.ToList();
            if (list.Count == 1)
            {
                return list.First();
            }
            else
            {
                return null;
            }
        }

        #endregion


        #region 25.TCR收费节假日管理

        //查询
        public List<FeeHoliday> FeeHolidayList(HolidaySearch searchEntity, out int total)
        {
            var sql = from a in idal.IFeeHolidayDAL.SelectAll()
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.HolidayName))
                sql = sql.Where(u => u.HolidayName.Contains(searchEntity.HolidayName));


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //节假日导入
        public string FeeHolidayImports(string[] holiday, string[] dayBegin, string[] type, string whCode, string userName)
        {

            List<FeeHoliday> HolidayListAdd = new List<FeeHoliday>();

            for (int j = 0; j < holiday.Count(); j++)
            {
                FeeHoliday entity = new FeeHoliday();
                entity.HolidayName = holiday[j];
                entity.DayBegin = dayBegin[j];
                entity.Type = (type[j] == "" ? 0 : 1);

                entity.CreateUser = userName;
                entity.CreateDate = DateTime.Now;
                HolidayListAdd.Add(entity);   //不存在就新增

            }
            idal.IFeeHolidayDAL.Add(HolidayListAdd);
            idal.IFeeHolidayDAL.SaveChanges();
            return "";

        }

        //修改信息
        public string FeeHolidayEdit(FeeHoliday entity)
        {
            idal.IFeeHolidayDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "DayBegin" });
            idal.SaveChanges();
            return "Y";
        }


        #endregion



        #region 26.DamcoGrnRule管理


        //DamcoGrnRule查询列表
        public List<DamcoGrnRule> DamcoGrnRuleList(DamcoGrnRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.IDamcoGrnRuleDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.MailTo))
                sql = sql.Where(u => u.MailTo.Contains(searchEntity.MailTo));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增
        public string DamcoGrnRuleAdd(DamcoGrnRule entity)
        {
            if (idal.IDamcoGrnRuleDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).Count > 0)
            {
                return "每个客户只能添加一个Rule，该客户已存在Rule，请前去修改！";
            }
            entity.CreateDate = DateTime.Now;
            idal.IDamcoGrnRuleDAL.Add(entity);
            idal.IDamcoGrnRuleDAL.SaveChanges();
            return "Y";
        }

        //删除
        public int DamcoGrnRuleDel(int id)
        {
            idal.IDamcoGrnRuleDAL.DeleteBy(u => u.Id == id);
            idal.IDamcoGrnRuleDAL.SaveChanges();
            return 1;
        }

        //修改
        public string DamcoGrnRuleEdit(DamcoGrnRule entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IDamcoGrnRuleDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[]
                { "AutoSend","DifferenceRate","DifferentialInterceptionFlag","DifferentialMailFlag","TotalCheck","CBMSource","KgsSource","MailTo","UpdateUser","UpdateDate"});

            idal.IDamcoGrnRuleDAL.SaveChanges();
            return "Y";

        }

        #endregion


        #region 27.出货合同管理

        //查询
        public List<ContractFormOut> ContractFormOutList(ContractFormOutSearch searchEntity, out int total)
        {
            var sql = from a in idal.IContractFormOutDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ContractName))
                sql = sql.Where(u => u.ContractName.Contains(searchEntity.ContractName));
            if (!string.IsNullOrEmpty(searchEntity.ChargeName))
                sql = sql.Where(u => u.ChargeName.Contains(searchEntity.ChargeName));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //合同导入
        public string ContractFormOutImports(List<ContractFormOut> entityList)
        {
            string[] contractName = (from a in entityList
                                     select a.ContractName).ToList().Distinct().ToArray();

            ContractFormOut first = entityList.First();
            List<ContractFormOut> checkList = idal.IContractFormOutDAL.SelectBy(u => contractName.Contains(u.ContractName) && u.WhCode == first.WhCode);
            if (checkList.Count > 0)
            {
                string[] getcontractName = (from a in checkList
                                            select a.ContractName).ToList().Distinct().ToArray();

                string checkResult = "";
                foreach (var item in getcontractName)
                {
                    checkResult += "合同名：" + item + "已存在！";
                }

                return checkResult;
            }

            idal.IContractFormOutDAL.Add(entityList);
            idal.IContractFormOutDAL.SaveChanges();
            return "Y";

        }

        //删除
        public int ContractFormOutDeleteAll(string contractName, string whCode)
        {
            idal.IContractFormOutDAL.DeleteBy(u => u.WhCode == whCode && u.ContractName == contractName);
            idal.IContractFormOutDAL.SaveChanges();
            return 1;
        }


        //修改信息
        public string ContractFormOutEdit(ContractFormOut entity)
        {
            idal.IContractFormOutDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "LadderNumberBegin", "LadderNumberEnd", "Price" });
            idal.SaveChanges();
            return "Y";
        }

        //合同下拉菜单列表
        public IEnumerable<string> ContractFormOutListSelect(string whCode)
        {
            var sql = (from a in idal.IContractFormOutDAL.SelectAll()
                       where a.WhCode == whCode
                       select a.ContractName).Distinct();

            return sql.AsEnumerable();
        }


        #endregion


        #region 28.账单管理

        //账单查询
        public List<BillMaster> BillMasterList(BillMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IBillMasterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
            {
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //添加账单
        public BillMaster BillMasterAdd(BillMaster entity)
        {
            entity.BLNumber = "BL" + DI.IDGenerator.NewId;
            entity.Status = "U";
            entity.CreateDate = DateTime.Now;
            idal.IBillMasterDAL.Add(entity);
            idal.IBillMasterDAL.SaveChanges();

            return entity;
        }

        //查询Load列表 显示是否已确认费用状态
        public List<BillDetailResult> GetLoadMasterList(BillDetailSearch searchEntity, out int total, string[] loadIdList, string[] containerNumberList)
        {
            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.ILoadContainerExtendDAL.SelectAll()
                       on new { a.WhCode, a.LoadId } equals new { b.WhCode, b.LoadId } into temp1
                       from b in temp1.DefaultIfEmpty()
                       join c in idal.ILoadDetailDAL.SelectAll()
                       on a.Id equals c.LoadMasterId
                       join d in idal.IOutBoundOrderDAL.SelectAll()
                       on c.OutBoundOrderId equals d.Id
                       join e in idal.IR_WhClient_WhAgentDAL.SelectAll()
                       on d.ClientId equals e.ClientId
                       join f in idal.IWhAgentDAL.SelectAll()
                       on e.AgentId equals f.Id
                       join g in idal.ILoadChargeDAL.SelectAll()
                       on new { a.WhCode, a.LoadId } equals new { g.WhCode, g.LoadId } into temp2
                       from g in temp2.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && a.Status0 == "C" && g.Status2 != "C" && (b.PortSuitcase ?? "") != ""
                       select new BillDetailResult
                       {
                           Action = "",
                           LoadId = a.LoadId,
                           ContainerNumber = b.ContainerNumber,
                           ContainerType = b.ContainerType,
                           AgentCode = f.AgentCode,
                           ClientCode = d.ClientCode,
                           SumCBM = b.TotalCBM,
                           SumQty = b.TotalQty,
                           Status = a.Status3 == "C" ? "已装箱" : "未完成装箱",
                           // FeeStatus = g.Status == "C" ? "已确认" : "已确认",
                           FeeStatus = g.Status == "C" ? "已确认" : "费用未确认",
                           CreateDate = b.ETD
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.AgentCode))
                sql = sql.Where(u => u.AgentCode == searchEntity.AgentCode);
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            if (!string.IsNullOrEmpty(searchEntity.FeeStatus))
            {
                if (searchEntity.FeeStatus == "已确认")
                {
                    sql = sql.Where(u => u.FeeStatus == "已确认");
                }
                else
                {
                    sql = sql.Where(u => u.FeeStatus != "已确认");
                }
            }

            if (loadIdList != null)
            {
                sql = sql.Where(u => loadIdList.Contains(u.LoadId));
            }
            if (containerNumberList != null)
            {
                sql = sql.Where(u => containerNumberList.Contains(u.ContainerNumber));
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.CreateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据Load号查询出货箱单费用并添加至账单
        public string BillDetailAdd(string whCode, string billNumber, string userName, string[] loadId)
        {
            List<LoadCharge> list = idal.ILoadChargeDAL.SelectBy(u => u.WhCode == whCode && loadId.Contains(u.LoadId));
            if (list.Where(u => u.Status == "U").Count() > 0)
            {
                return "该Load未确认费用：" + list.Where(u => u.Status2 == "U").First().LoadId;
            }
            if (list.Where(u => u.Status2 == "C").Count() > 0)
            {
                return "该Load已添加账单：" + list.Where(u => u.Status2 == "C").First().LoadId;
            }

            if (list.Where(u => (u.Description ?? "") != "").Count() > 0)
            {
                return "该Load费用异常：" + list.Where(u => (u.Description ?? "") != "").First().Description;
            }

            if (idal.IBillMasterDAL.SelectBy(u => u.WhCode == whCode && u.BLNumber == billNumber && u.Status == "C").Count > 0)
            {
                return "该账单已确认，无法继续添加费用：" + billNumber;
            }


            List<LoadChargeDetail> getDetailList = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && loadId.Contains(u.LoadId) && u.NotLoadFlag != 1);
            List<BillDetail> entityList = new List<BillDetail>();
            foreach (var item in getDetailList)
            {
                BillDetail bill = new BillDetail();
                bill.WhCode = item.WhCode;
                bill.BLNumber = billNumber;
                bill.ClientCode = item.ClientCode;
                bill.LoadId = item.LoadId;
                bill.NotLoadFlag = item.NotLoadFlag;
                bill.SoId = item.Id;
                bill.SoNumber = item.SoNumber;
                bill.ChargeCode = item.ChargeCode;
                bill.ChargeItem = item.ChargeItem;
                bill.TaxRate = item.TaxRate;
                bill.DaiDianId = item.DaiDianId;
                bill.ETD = item.ETD;

                bill.ContainerNumber = item.ContainerNumber;
                bill.ChargeType = item.ChargeType;
                bill.ChargeName = item.ChargeName;
                bill.ContainerType = item.ContainerType;
                bill.UnitName = item.UnitName;
                bill.Qty = item.Qty;
                bill.CBM = item.CBM;
                bill.Weight = item.Weight;

                bill.ChargeUnitName = item.ChargeUnitName;
                bill.LadderNumber = item.LadderNumber;
                bill.Price = item.Price;
                bill.PriceTotal = item.PriceTotal;
                bill.CustomerName = item.CreateUser;
                bill.TaxInclusiveFlag = item.TaxInclusiveFlag;

                bill.CreateUser = userName;
                bill.CreateDate = DateTime.Now;
                entityList.Add(bill);
            }

            lock (o1)
            {
                idal.IBillDetailDAL.Add(entityList);
                idal.SaveChanges();
                foreach (var item in list)
                {
                    item.Status2 = "C";
                    idal.ILoadChargeDAL.UpdateByExtended(u => u.Id == item.Id, t => new LoadCharge { Status2 = "C" });
                }
            }

            return "Y";
        }

        //根据SO号查询出货SO特费并添加至账单
        public string BillDetailAddSO(string whCode, string billNumber, string userName, int[] idList)
        {
            List<LoadChargeDetail> list = idal.ILoadChargeDetailDAL.SelectBy(u => u.WhCode == whCode && idList.Contains(u.Id));
            if (list.Where(u => u.SoStatus == "C").Count() > 0)
            {
                return "该SO已做账单：" + list.Where(u => u.SoStatus == "C").First().SoNumber;
            }

            if (idal.IBillMasterDAL.SelectBy(u => u.WhCode == whCode && u.BLNumber == billNumber && u.Status == "C").Count > 0)
            {
                return "该账单已确认，无法继续添加费用：" + billNumber;
            }

            List<BillDetail> entityList = new List<BillDetail>();
            foreach (var item in list)
            {
                BillDetail bill = new BillDetail();
                bill.WhCode = item.WhCode;
                bill.BLNumber = billNumber;
                bill.ClientCode = item.ClientCode;
                bill.LoadId = item.LoadId;
                bill.NotLoadFlag = item.NotLoadFlag;
                bill.SoId = item.Id;
                bill.SoNumber = item.SoNumber;
                bill.ChargeCode = item.ChargeCode;
                bill.ChargeItem = item.ChargeItem;
                bill.TaxRate = item.TaxRate;
                bill.DaiDianId = item.DaiDianId;
                bill.ETD = item.ETD;

                bill.ContainerNumber = item.ContainerNumber;
                bill.ChargeType = item.ChargeType;
                bill.ChargeName = item.ChargeName;
                bill.ContainerType = item.ContainerType;
                bill.UnitName = item.UnitName;
                bill.Qty = item.Qty;
                bill.CBM = item.CBM;
                bill.Weight = item.Weight;

                bill.ChargeUnitName = item.ChargeUnitName;
                bill.LadderNumber = item.LadderNumber;
                bill.Price = item.Price;
                bill.PriceTotal = item.PriceTotal;
                bill.CustomerName = item.CreateUser;
                bill.TaxInclusiveFlag = item.TaxInclusiveFlag;

                bill.CreateUser = userName;
                bill.CreateDate = DateTime.Now;
                entityList.Add(bill);
            }

            lock (o1)
            {
                idal.IBillDetailDAL.Add(entityList);
                idal.SaveChanges();
                foreach (var item in list)
                {
                    item.SoStatus = "C";
                    idal.ILoadChargeDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new LoadChargeDetail { SoStatus = "C" });
                }
            }

            return "Y";
        }

        //丹马士账单显示
        public List<BillDetailRepostResult> DamcoBillDetailList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total)
        {
            var sql = from a in idal.IBillDetailDAL.SelectAll()
                      join b in idal.ILoadContainerExtendDAL.SelectAll()
                      on new { a.LoadId, a.WhCode }
                        equals new { b.LoadId, b.WhCode } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.ILoadContainerTypeDAL.SelectAll()
                      on b.ContainerType equals c.ContainerType into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.ILoadMasterDAL.SelectAll()
                       on new { b.LoadId, b.WhCode }
                        equals new { d.LoadId, d.WhCode } into d_join
                      from d in d_join.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode && a.BLNumber == searchEntity.BLNumber && (a.NotLoadFlag ?? 0) == searchEntity.NotLoadFlag && (b.PortSuitcase ?? "") != ""
                      select new BillDetailRepostResult
                      {
                          LoadId = a.LoadId,
                          ClientCode = a.ClientCode,
                          ChargeName = a.ChargeName,
                          etd = b.ETD,
                          booking_no = a.SoNumber,
                          cbl = b.BillNumber,
                          container_no = a.ContainerNumber,
                          container_size = c.ContainerCodeType,
                          tixiangdian = b.PortSuitcase,
                          jingangdian = b.PortSuitcase,
                          portsurtcase = b.PortSuitcase,
                          quantity = b.TotalCBM,
                          charge_code = a.ChargeCode,
                          charge_item = a.ChargeItem,
                          unit_price = (double)(a.Price),
                          invoice_amount = (double)(a.PriceTotal),
                          no_vat_amount = (double)(a.PriceTotal),
                          booking_damco_pic = b.CaoZuoUser,
                          fcr = a.SoNumber,
                          createUser = b.CreateUser,
                          Qty = a.Qty == 0 ? a.CBM : a.Qty,
                          DaiDianId = a.DaiDianId,
                          TaxRate = a.TaxRate
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.container_no == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.booking_no == searchEntity.SoNumber);

            if (searchEntity.BeginETD != null)
            {
                sql = sql.Where(u => u.etd >= searchEntity.BeginETD);
            }
            if (searchEntity.EndETD != null)
            {
                sql = sql.Where(u => u.etd < searchEntity.EndETD);
            }

            List<WhUser> userList = idal.IWhUserDAL.SelectAll().ToList();
            List<WhClientResult> clientList = (from a in idal.IWhClientDAL.SelectAll()
                                               where a.WhCode == searchEntity.WhCode
                                               join b in idal.IZoneDAL.SelectAll()
                                               on new { A = (int)a.ZoneId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode } into temp1
                                               from ab in temp1.DefaultIfEmpty()
                                               where a.WhCode == searchEntity.WhCode
                                               select new WhClientResult
                                               {
                                                   ClientCode = a.ClientCode,
                                                   ClientName = a.ClientName,
                                                   ZoneName = ab.ZoneName
                                               }).ToList();

            List<BillDetailRepostResult> list = new List<BillDetailRepostResult>();
            if (searchEntity.ReportAgentType == "Damco" && searchEntity.ReportType == "装箱费")
            {
                if (clientCode != null)
                {
                    sql = sql.Where(u => clientCode.Contains(u.ClientCode));
                }
                if (clientCodeNotIn != null)
                {
                    sql = sql.Where(u => !clientCodeNotIn.Contains(u.ClientCode));
                }
                if (chargeName != null)
                {
                    sql = sql.Where(u => chargeName.Contains(u.ChargeName));
                }
                List<BillDetailRepostResult> list1 = sql.ToList();

                List<BillDetailRepostResult> list2 = new List<BillDetailRepostResult>();

                string[] loadIdList = (from a in list1
                                       select a.LoadId).Distinct().ToArray();

                foreach (var item in loadIdList)
                {
                    BillDetailRepostResult getResult1 = list1.Where(u => u.LoadId == item && u.ChargeName.Contains("进港")).ToList().First();

                    BillDetailRepostResult getResult2 = list1.Where(u => u.LoadId == item && u.ChargeName.Contains("装箱")).ToList().First();

                    BillDetailRepostResult entity = new BillDetailRepostResult();
                    entity.LoadId = item;
                    entity.unit_price = getResult1.unit_price + getResult2.unit_price;
                    list2.Add(entity);
                }

                #region
                foreach (var item in list1)
                {
                    if (item.ChargeName.Contains("进港"))
                    {
                        BillDetailRepostResult newResult = item;
                        newResult.origin = "SGH";
                        newResult.invoice_name = "";
                        newResult.invoice_no = "马士基非保仓库";
                        newResult.due_date = "";
                        newResult.consignee_name = item.ClientCode.ToUpper();
                        newResult.sending_date = null;
                        newResult.charge_item = item.charge_item;
                        newResult.charge_code = item.charge_code;
                        newResult.unit_price = 15.16;

                        newResult.invoice_amount = Math.Round(Convert.ToDouble(newResult.unit_price * 1.09 * Convert.ToDouble(item.quantity)), 3);
                        newResult.no_vat_amount = Math.Round(Convert.ToDouble(newResult.unit_price * Convert.ToDouble(item.quantity)), 3);

                        newResult.currency = "CNY";
                        newResult.vat_rate = "9%";
                        newResult.fapiao_type = "增值税专用发票";
                        newResult.remark = "散货操作费";
                        newResult.oce = searchEntity.OceScmType;

                        newResult.truck_pack_fee = Math.Round(Convert.ToDouble(Convert.ToDouble(item.quantity) * newResult.unit_price), 2);
                        newResult.warhouse_pack_fee = 0;

                        newResult.warhouse_daidian_fee = 0;
                        newResult.truck_daidian_fee = 0;
                        newResult.warhouse_unload_fee = 0;
                        newResult.other_fee = 0;
                        newResult.yard_fee = 0;
                        newResult.difference = "";

                        newResult.etd_show = Convert.ToDateTime(item.etd).ToString("yyyy-MM-dd");
                        newResult.tixiangdian = item.portsurtcase.Substring(0, item.portsurtcase.IndexOf("-")).Substring(0, 1);
                        newResult.jingangdian = item.portsurtcase.Substring(item.portsurtcase.IndexOf("-") + 1, item.portsurtcase.Length - (item.portsurtcase.IndexOf("-") + 1)).Substring(0, 1);

                        if (userList.Where(u => u.UserCode == item.createUser).Count() > 0)
                        {
                            newResult.customerName = userList.Where(u => u.UserCode == item.createUser).First().UserNameCN;
                        }

                        if (clientList.Where(u => u.ClientCode == item.ClientCode).Count() > 0)
                        {
                            string location = clientList.Where(u => u.ClientCode == item.ClientCode).First().ZoneName;
                            if (searchEntity.WhCode == "10")
                            {
                                if (location == "A库" || location == "B库" || location == "E库")
                                {
                                    newResult.locationId = "LC-CFS1";
                                }
                                else
                                {
                                    newResult.locationId = "LC-CFS2";
                                }
                            }
                            else
                            {
                                newResult.locationId = location;
                            }
                        }

                        list.Add(newResult);
                    }
                    else if (item.ChargeName.Contains("装箱"))
                    {
                        BillDetailRepostResult getResult = list2.Where(u => u.LoadId == item.LoadId).ToList().First();

                        BillDetailRepostResult newResult1 = item;
                        newResult1.origin = "SGH";
                        newResult1.invoice_name = "";
                        newResult1.invoice_no = "马士基非保仓库";
                        newResult1.due_date = "";
                        newResult1.consignee_name = item.ClientCode.ToUpper();
                        newResult1.sending_date = null;
                        newResult1.charge_item = item.charge_item;
                        newResult1.charge_code = item.charge_code;
                        newResult1.currency = "CNY";
                        newResult1.vat_rate = "6%";
                        newResult1.fapiao_type = "增值税专用发票";
                        newResult1.remark = "散货操作费";
                        newResult1.oce = searchEntity.OceScmType;

                        newResult1.unit_price = getResult.unit_price - 15.16;
                        newResult1.invoice_amount = Math.Round(Convert.ToDouble(newResult1.unit_price * 1.06 * Convert.ToDouble(item.quantity)), 3);
                        newResult1.no_vat_amount = Math.Round(Convert.ToDouble(newResult1.unit_price * Convert.ToDouble(item.quantity)), 3);

                        if (item.container_size.Contains("20"))
                        {
                            newResult1.yardZhongKong_fee = Math.Round(75 / 1.06, 2);
                        }
                        else
                        {
                            newResult1.yardZhongKong_fee = Math.Round(150 / 1.06, 2);
                        }

                        newResult1.truck_pack_fee = 0;
                        newResult1.warhouse_pack_fee = Math.Round(Convert.ToDouble(newResult1.no_vat_amount - newResult1.yardZhongKong_fee), 2);

                        newResult1.warhouse_daidian_fee = 0;
                        newResult1.truck_daidian_fee = 0;
                        newResult1.warhouse_unload_fee = 0;
                        newResult1.other_fee = 0;
                        newResult1.yard_fee = 0;
                        newResult1.difference = "";

                        newResult1.etd_show = Convert.ToDateTime(item.etd).ToString("yyyy-MM-dd");
                        newResult1.tixiangdian = item.portsurtcase.Substring(0, item.portsurtcase.IndexOf("-")).Substring(0, 1);
                        newResult1.jingangdian = item.portsurtcase.Substring(item.portsurtcase.IndexOf("-") + 1, item.portsurtcase.Length - (item.portsurtcase.IndexOf("-") + 1)).Substring(0, 1);

                        if (userList.Where(u => u.UserCode == item.createUser).Count() > 0)
                        {
                            newResult1.customerName = userList.Where(u => u.UserCode == item.createUser).First().UserNameCN;
                        }

                        if (clientList.Where(u => u.ClientCode == item.ClientCode).Count() > 0)
                        {
                            string location = clientList.Where(u => u.ClientCode == item.ClientCode).First().ZoneName;
                            if (searchEntity.WhCode == "10")
                            {
                                if (location == "A库" || location == "B库" || location == "E库")
                                {
                                    newResult1.locationId = "LC-CFS1";
                                }
                                else
                                {
                                    newResult1.locationId = "LC-CFS2";
                                }
                            }
                            else
                            {
                                newResult1.locationId = location;
                            }
                        }

                        list.Add(newResult1);

                        //BillDetailRepostResult first = list.Where(u => u.LoadId == item.LoadId).First();
                        //list.Remove(first);

                        //BillDetailRepostResult newResult = first;

                        //newResult.unit_price = Math.Round(Convert.ToDouble(newResult.unit_price + item.unit_price), 2);

                        //newResult.invoice_amount = Math.Round(Convert.ToDouble(newResult.invoice_amount + item.invoice_amount), 3);//保留3位
                        //newResult.no_vat_amount = Math.Round(Convert.ToDouble(newResult.no_vat_amount + item.no_vat_amount), 3);//保留3位

                        //newResult.truck_pack_fee = Math.Round(Convert.ToDouble(Convert.ToDouble(item.quantity) * 15.16), 2);
                        //newResult.warhouse_pack_fee = Math.Round(Convert.ToDouble(newResult.no_vat_amount - newResult.truck_pack_fee), 2);

                        //list.Add(newResult);
                    }
                }
                #endregion
            }
            else if (searchEntity.ReportAgentType == "Damco" && searchEntity.ReportType == "特费")
            {
                if (clientCode != null)
                {
                    sql = sql.Where(u => clientCode.Contains(u.ClientCode));
                }
                if (clientCodeNotIn != null)
                {
                    sql = sql.Where(u => !clientCodeNotIn.Contains(u.ClientCode));
                }

                //特费不包含装箱费及进港费
                if (chargeName != null)
                {
                    sql = sql.Where(u => !chargeName.Contains(u.ChargeName));
                }
                List<BillDetailRepostResult> list1 = sql.Where(u => u.unit_price > 0).ToList();

                List<LoadChargeDaiDian> daidianList = idal.ILoadChargeDaiDianDAL.SelectAll().ToList();

                #region
                foreach (var item in list1)
                {
                    BillDetailRepostResult newResult = item;
                    newResult.origin = "SGH";
                    newResult.invoice_name = "";
                    newResult.invoice_no = "马士基非保仓库";
                    newResult.due_date = "";
                    newResult.consignee_name = item.ClientCode.ToUpper();
                    newResult.sending_date = null;
                    newResult.charge_item = item.charge_item;
                    newResult.charge_code = item.charge_code;
                    newResult.quantity = item.Qty;

                    newResult.tixiangdian = "";
                    newResult.jingangdian = "";
                    newResult.portsurtcase = "";

                    newResult.currency = "CNY";
                    newResult.vat_rate = item.TaxRate + "%";
                    newResult.fapiao_type = "增值税专用发票";
                    newResult.remark = "OELG" + item.ChargeName + item.no_vat_amount + "元";
                    newResult.oce = searchEntity.OceScmType;

                    newResult.unit_price = Math.Round(Convert.ToDouble(item.unit_price), 2);//保留2位
                    newResult.invoice_amount = Math.Round(Convert.ToDouble(item.invoice_amount * (1 + Convert.ToDouble(item.TaxRate) / 100)), 3);//保留3位
                    newResult.no_vat_amount = Math.Round(Convert.ToDouble(item.no_vat_amount), 3);//保留3位

                    if (item.DaiDianId == 1)
                    {
                        newResult.warhouse_daidian_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }
                    if (item.DaiDianId == 2)
                    {
                        newResult.truck_daidian_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }
                    if (item.DaiDianId == 3)
                    {

                    }
                    if (item.DaiDianId == 4)
                    {
                        newResult.warhouse_pack_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }
                    else if (item.DaiDianId == 5)
                    {
                        newResult.truck_pack_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }
                    else if (item.DaiDianId == 6)
                    {

                    }
                    else if (item.DaiDianId == 7)
                    {
                        newResult.other_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }
                    else if (item.DaiDianId == 8)
                    {
                        newResult.yard_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                    }

                    newResult.demurrageCharge_fee = 0;
                    newResult.yardZhongKong_fee = 0;
                    newResult.truck_pack_outSourcing_fee = 0;
                    newResult.agent_fee = 0;
                    newResult.difference = "";

                    newResult.etd_show = Convert.ToDateTime(item.etd).ToString("yyyy-MM-dd");

                    if (userList.Where(u => u.UserCode == item.createUser).Count() > 0)
                    {
                        newResult.customerName = userList.Where(u => u.UserCode == item.createUser).First().UserNameCN;
                    }

                    if (clientList.Where(u => u.ClientCode == item.ClientCode).Count() > 0)
                    {
                        string location = clientList.Where(u => u.ClientCode == item.ClientCode).First().ZoneName;
                        if (searchEntity.WhCode == "10")
                        {
                            if (location == "A库" || location == "B库" || location == "E库")
                            {
                                newResult.locationId = "LC-CFS1";
                            }
                            else
                            {
                                newResult.locationId = "LC-CFS2";
                            }
                        }
                        else
                        {
                            newResult.locationId = location;
                        }
                    }

                    list.Add(newResult);
                }
                #endregion

            }

            total = list.Count;
            list = list.OrderBy(u => u.LoadId).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //丹马士SO账单显示
        public List<BillDetailRepostResult> DamcoBillDetailSOList(BillDetailSearch searchEntity, string[] chargeName, string[] clientCode, string[] clientCodeNotIn, out int total)
        {
            var sql = from a in idal.IBillDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.BLNumber == searchEntity.BLNumber && (a.NotLoadFlag ?? 0) == searchEntity.NotLoadFlag
                      select new BillDetailRepostResult
                      {
                          Id = a.Id,
                          LoadId = "",
                          ClientCode = a.ClientCode,
                          ChargeName = a.ChargeName,
                          etd = a.ETD,
                          booking_no = a.SoNumber,
                          cbl = a.UnitName,
                          container_no = a.ContainerNumber,
                          container_size = a.ContainerType,
                          tixiangdian = "",
                          jingangdian = "",
                          portsurtcase = "",
                          quantity = a.CBM,
                          charge_code = a.ChargeCode,
                          charge_item = a.ChargeItem,
                          TaxRate = a.TaxRate,
                          unit_price = a.TaxInclusiveFlag == 1 ? (double)(a.Price) : (double)(a.Price) * (1 + Convert.ToDouble(a.TaxRate) / 100),
                          invoice_amount = a.TaxInclusiveFlag == 1 ? (double)(a.PriceTotal) : (double)(a.PriceTotal) * (1 + Convert.ToDouble(a.TaxRate) / 100),
                          no_vat_amount = (double)(a.PriceTotal),
                          booking_damco_pic = a.ChargeUnitName,
                          fcr = a.LadderNumber,
                          createUser = a.CustomerName,
                          DaiDianId = a.DaiDianId
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.container_no == searchEntity.ContainerNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.booking_no == searchEntity.SoNumber);

            if (searchEntity.BeginETD != null)
            {
                sql = sql.Where(u => u.etd >= searchEntity.BeginETD);
            }
            if (searchEntity.EndETD != null)
            {
                sql = sql.Where(u => u.etd < searchEntity.EndETD);
            }

            List<WhUser> userList = idal.IWhUserDAL.SelectAll().ToList();
            List<WhClientResult> clientList = (from a in idal.IWhClientDAL.SelectAll()
                                               where a.WhCode == searchEntity.WhCode
                                               join b in idal.IZoneDAL.SelectAll()
                                               on new { A = (int)a.ZoneId, B = a.WhCode } equals new { A = b.Id, B = b.WhCode } into temp1
                                               from ab in temp1.DefaultIfEmpty()
                                               where a.WhCode == searchEntity.WhCode
                                               select new WhClientResult
                                               {
                                                   ClientCode = a.ClientCode,
                                                   ClientName = a.ClientName,
                                                   ZoneName = ab.ZoneName
                                               }).ToList();

            List<BillDetailRepostResult> list = new List<BillDetailRepostResult>();

            if (clientCode != null)
            {
                sql = sql.Where(u => clientCode.Contains(u.ClientCode));
            }
            if (clientCodeNotIn != null)
            {
                sql = sql.Where(u => !clientCodeNotIn.Contains(u.ClientCode));
            }
            if (chargeName != null)
            {
                sql = sql.Where(u => chargeName.Contains(u.ChargeName));
            }
            List<BillDetailRepostResult> list1 = sql.ToList();

            List<LoadChargeDaiDian> daidianList = idal.ILoadChargeDaiDianDAL.SelectAll().ToList();

            #region
            foreach (var item in list1)
            {
                BillDetailRepostResult newResult = item;
                newResult.origin = "SGH";
                newResult.invoice_name = "";
                newResult.invoice_no = "马士基非保仓库";
                newResult.due_date = "";
                newResult.consignee_name = item.ClientCode.ToUpper();
                newResult.sending_date = null;
                newResult.currency = "CNY";
                newResult.vat_rate = item.TaxRate + "%";
                newResult.fapiao_type = "增值税专用发票";
                newResult.remark = "OELG" + item.ChargeName + item.no_vat_amount + "元";
                newResult.oce = searchEntity.OceScmType;

                newResult.unit_price = Math.Round(Convert.ToDouble(item.unit_price), 2);//保留2位
                newResult.invoice_amount = Math.Round(Convert.ToDouble(item.invoice_amount), 3);//保留3位
                newResult.no_vat_amount = Math.Round(Convert.ToDouble(item.no_vat_amount), 3);//保留3位

                if (item.DaiDianId == 1)
                {
                    newResult.warhouse_daidian_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }
                if (item.DaiDianId == 2)
                {
                    newResult.truck_daidian_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }
                if (item.DaiDianId == 3)
                {

                }
                if (item.DaiDianId == 4)
                {
                    newResult.warhouse_pack_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }
                else if (item.DaiDianId == 5)
                {
                    newResult.truck_pack_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }
                else if (item.DaiDianId == 6)
                {

                }
                else if (item.DaiDianId == 7)
                {
                    newResult.other_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }
                else if (item.DaiDianId == 8)
                {
                    newResult.yard_fee = Math.Round(Convert.ToDouble(item.no_vat_amount), 2);
                }

                newResult.difference = "";

                newResult.etd_show = Convert.ToDateTime(item.etd).ToString("yyyy-MM-dd");

                if (userList.Where(u => u.UserCode == item.createUser).Count() > 0)
                {
                    newResult.customerName = userList.Where(u => u.UserCode == item.createUser).First().UserNameCN;
                }

                if (clientList.Where(u => u.ClientCode == item.ClientCode).Count() > 0)
                {
                    string location = clientList.Where(u => u.ClientCode == item.ClientCode).First().ZoneName;
                    if (searchEntity.WhCode == "10")
                    {
                        if (location == "A库" || location == "B库" || location == "E库")
                        {
                            newResult.locationId = "CFS1";
                        }
                        else
                        {
                            newResult.locationId = "CFS2";
                        }
                    }
                    else
                    {
                        newResult.locationId = location;
                    }
                }

                list.Add(newResult);
            }

            #endregion

            total = list.Count;
            list = list.OrderBy(u => u.LoadId).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }



        //根据Id删除账单明细，并撤销费用状态
        public string BillDetailDelBySO(string whCode, string billNumber, string userName, int id)
        {
            if (idal.IBillMasterDAL.SelectBy(u => u.WhCode == whCode && u.BLNumber == billNumber && u.Status == "C").Count > 0)
            {
                return "该账单已确认，无法删除费用：" + billNumber;
            }

            lock (o1)
            {
                BillDetail first = idal.IBillDetailDAL.SelectBy(u => u.Id == id).First();
                idal.IBillDetailDAL.DeleteBy(u => u.Id == id);

                LoadChargeDetail loadCharge = new LoadChargeDetail();
                loadCharge.SoStatus = "";
                idal.ILoadChargeDetailDAL.UpdateBy(loadCharge, u => u.Id == first.SoId, new string[] { "SoStatus" });
                idal.SaveChanges();
            }

            return "Y";
        }

        //删除账单
        public string BillFeeDetailDelAll(string whCode, string billNumber)
        {
            List<BillDetail> billDetail = idal.IBillDetailDAL.SelectBy(u => u.WhCode == whCode && u.BLNumber == billNumber);

            string[] loadIdList = (from a in billDetail
                                   select a.LoadId).Distinct().ToArray();

            idal.IBillDetailDAL.DeleteByExtended(u => u.WhCode == whCode && u.BLNumber == billNumber);

            idal.ILoadChargeDAL.UpdateByExtended(u => loadIdList.Contains(u.LoadId) && u.WhCode == whCode, t => new LoadCharge { Status2 = "" });

            idal.IBillMasterDAL.DeleteByExtended(u => u.WhCode == whCode && u.BLNumber == billNumber);
            idal.SaveChanges();

            return "Y";
        }

        //根据Load号删除账单明细，并撤销费用状态
        public string BillDetailDelByLoad(string whCode, string billNumber, string userName, string loadId)
        {
            if (idal.IBillMasterDAL.SelectBy(u => u.WhCode == whCode && u.BLNumber == billNumber && u.Status == "C").Count > 0)
            {
                return "该账单已确认，无法删除费用：" + billNumber;
            }

            lock (o1)
            {
                idal.IBillDetailDAL.DeleteBy(u => u.WhCode == whCode && u.LoadId == loadId && u.BLNumber == billNumber);

                LoadCharge loadCharge = new LoadCharge();
                loadCharge.Status2 = "";
                idal.ILoadChargeDAL.UpdateBy(loadCharge, u => u.WhCode == whCode && u.LoadId == loadId, new string[] { "Status2" });
                idal.SaveChanges();
            }

            return "Y";
        }

        //修改账单状态
        public string BillMasterEdit(string whCode, string billNumber, string status)
        {
            BillMaster edit = new BillMaster();
            edit.Status = status;
            idal.IBillMasterDAL.UpdateBy(edit, u => u.WhCode == whCode && u.BLNumber == billNumber, new string[] { "Status" });
            idal.SaveChanges();

            return "Y";
        }


        #endregion


        #region 29.区域与库位拓展管理

        //区域与库位扩展列表
        public List<ZoneExtendResult> ZonesExtendList(WhZoneSearch searchEntity, out int total)
        {

            var sql = from a in idal.IZonesExtendDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IZoneDAL.SelectAll()
                      on a.ZoneId equals b.Id into b_join
                      from b in b_join.DefaultIfEmpty()
                      select new ZoneExtendResult
                      {
                          Id = a.Id,
                          ZoneId = a.ZoneId,
                          ZoneName = b.ZoneName,
                          ZoneOrderBy = a.ZoneOrderBy,
                          OnlySkuFlag = a.OnlySkuFlag,
                          OnlySkuShow = (a.OnlySkuFlag ?? 0) == 0 ? "否" : "是",
                          MaxLocationIdQty = a.MaxLocationIdQty,
                          MaxPallateQty = a.MaxPallateQty
                      };

            if (searchEntity.ZoneId > 0)
                sql = sql.Where(u => u.ZoneId == searchEntity.ZoneId);

            if (!string.IsNullOrEmpty(searchEntity.ZoneName))
                sql = sql.Where(u => u.ZoneName.Contains(searchEntity.ZoneName));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增区域扩展
        public ZonesExtend ZonesExtendAdd(ZonesExtend entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.IZonesExtendDAL.Add(entity);
            idal.IZonesExtendDAL.SaveChanges();
            return entity;
        }

        //区域扩展信息修改
        public int ZonesExtendEdit(ZonesExtend entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IZonesExtendDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "ZoneOrderBy", "OnlySkuFlag", "MaxLocationIdQty", "MaxPallateQty", "UpdateDate" });
            idal.IZonesExtendDAL.SaveChanges();
            return 1;
        }

        public string ZonesExtendBatchDel(int?[] idarr)
        {
            foreach (var item in idarr)
            {
                idal.IZonesExtendDAL.DeleteByExtended(u => u.Id == item);
            }

            return "Y";
        }

        public string ZoneExtendImports(List<ZoneExtendResult> entity)
        {
            string result = "";
            ZoneExtendResult first = entity.First();
            string[] zoneNameArr = (from a in entity
                                    select a.ZoneName).ToList().Distinct().ToArray();

            List<Zone> getZoneList = idal.IZoneDAL.SelectBy(u => u.WhCode == first.WhCode && zoneNameArr.Contains(u.ZoneName));
            if (zoneNameArr.Count() != getZoneList.Count)
            {
                foreach (var item in zoneNameArr)
                {
                    if (getZoneList.Where(u => u.ZoneName == item).Count() == 0)
                    {
                        result += "区域:" + item + "不存在！";
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            foreach (var item in getZoneList)
            {
                idal.IZonesExtendDAL.DeleteByExtended(u => u.ZoneId == item.Id);
            }

            List<ZonesExtend> list = new List<ZonesExtend>();
            foreach (var item in entity)
            {
                ZonesExtend zone = new ZonesExtend();
                zone.WhCode = item.WhCode;
                zone.ZoneId = getZoneList.Where(u => u.ZoneName == item.ZoneName).First().Id;
                zone.ZoneOrderBy = item.ZoneOrderBy;
                zone.OnlySkuFlag = item.OnlySkuShow == "Y" ? 1 : 0;

                zone.MaxLocationIdQty = item.MaxLocationIdQty;
                zone.MaxPallateQty = item.MaxPallateQty;
                zone.CreateUser = item.CreateUser;
                zone.CreateDate = DateTime.Now;
                list.Add(zone);
            }

            idal.IZonesExtendDAL.Add(list);
            idal.IZonesExtendDAL.SaveChanges();
            return "Y";
        }

        #endregion


        #region 30.客户扩展管理

        //列表
        public List<WhClientExtendResult> WhClientExtendList(WhClientSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhClientExtendDAL.SelectAll()
                      join b in idal.IWhClientDAL.SelectAll()
                      on a.ClientId equals b.Id into b_join
                      from b in b_join.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      select new WhClientExtendResult
                      {
                          Id = a.Id,
                          ClientId = a.ClientId,
                          ClientCode = b.ClientCode,
                          InvClearUpSkuMaxQty = a.InvClearUpSkuMaxQty,
                          NotOnlySkuPutawayQty = a.NotOnlySkuPutawayQty,
                          RegularExpression = a.RegularExpression
                      };

            if (searchEntity.ClientId > 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增
        public WhClientExtend WhClientExtendAdd(WhClientExtend entity)
        {
            entity.CreateDate = DateTime.Now;
            idal.IWhClientExtendDAL.Add(entity);
            idal.IWhClientExtendDAL.SaveChanges();
            return entity;
        }

        //修改
        public int WhClientExtendEdit(WhClientExtend entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IWhClientExtendDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "InvClearUpSkuMaxQty", "NotOnlySkuPutawayQty", "RegularExpression", "UpdateUser", "UpdateDate" });
            idal.IWhClientExtendDAL.SaveChanges();
            return 1;
        }

        //批量删除
        public string WhClientExtendBatchDel(int?[] idarr)
        {
            foreach (var item in idarr)
            {
                idal.IWhClientExtendDAL.DeleteByExtended(u => u.Id == item);
            }

            return "Y";
        }

        #endregion



        #region 31.部分收货原因登记管理

        //列表
        public List<ReceiptPartialRegisterResult> ReceiptPartialRegisterList(ReceiptPartialRegisterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptPartialRegisterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new ReceiptPartialRegisterResult
                      {
                          Id = a.Id,
                          PhotoId = a.PhotoId ?? 0,
                          UploadDate = a.UploadDate,
                          ClientCode = a.ClientCode,
                          ReceiptId = a.ReceiptId,
                          Status =
                          a.Status == "U" ? "未登记"
                          : a.Status == "A" ? "部分登记"
                          : a.Status == "C" ? "已登记"
                          : "",
                          Qty = a.Qty,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public List<ReceiptPartialUnReceiptResult> ReceiptPartialUnReceiptList(ReceiptPartialUnPreceiptSearch searchEntity, out int total)
        {
            var sql = from a in idal.IPhotoMasterDAL.SelectAll()
                      join b in (from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                                 group a by
                                 new
                                 {
                                     a.WhCode,
                                     a.ReceiptId,
                                     a.SoNumber,
                                     a.PoNumber,
                                     a.ItemNumber,
                                     a.ItemId

                                 }
                             into g
                                 select new
                                 {
                                     WhCode = g.Key.WhCode,
                                     ReceiptId = g.Key.ReceiptId,
                                     SoNumber = g.Key.SoNumber,
                                     PoNumber = g.Key.PoNumber,
                                     ItemNumber = g.Key.ItemNumber,
                                     ItemId = g.Key.ItemId,
                                     qty = g.Sum(p => p.Qty)

                                 })
                         on new { A = a.WhCode, B = a.Number, C = a.Number2, D = a.Number3, E = a.Number4, F = a.ItemId }
                         equals new { A = b.WhCode, B = b.ReceiptId, C = b.SoNumber, D = b.PoNumber, E = b.ItemNumber, F = b.ItemId } into b_temp
                      from b in b_temp.DefaultIfEmpty()
                      where a.HoldReason == "部分收货"
                      select new ReceiptPartialUnReceiptResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          ReceiptId = a.Number,
                          SoNumber = a.Number2,
                          PoNumber = a.Number3,
                          itemNumber = a.Number4,
                          ItemId = a.ItemId,
                          UnQty = a.RegQty - a.Qty,
                          RegisteredQty = b.qty ?? 0,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };



            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public List<ReceiptPartialRegisteredDetailResult> RegisteredList(ReceiptPartialUnPreceiptSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                      where a.ReceiptId == searchEntity.ReceiptId
                      select new ReceiptPartialRegisteredDetailResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          SoNumber = a.SoNumber,
                          PoNumber = a.PoNumber,
                          itemNumber = a.ItemNumber,
                          ItemId = a.ItemId,
                          RegisteredQty = a.Qty ?? 0,
                          Reason = a.Reason,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);


            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }



        //未收货登记
        public string UnReceiptRegister(ReceiptPartialRegisterDetail entity, int UnQty)
        {
            //检查数据是否
            var sql = from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                      where a.WhCode == entity.WhCode && a.ReceiptId == entity.ReceiptId && a.SoNumber == entity.SoNumber && a.PoNumber == entity.PoNumber && a.ItemNumber == entity.ItemNumber && a.ItemId == entity.ItemId
                      group a by
                      new
                      {
                          a.WhCode,
                          a.ReceiptId,
                          a.SoNumber,
                          a.PoNumber,
                          a.ItemNumber,
                          a.ItemId
                      }
                             into g
                      select new
                      {
                          qty = g.Sum(p => (p.Qty ?? 0))
                      };

            if (sql.Count() > 0)
            {

                int s = Convert.ToInt32(sql.ToList().First().qty);
                s += Convert.ToInt32(entity.Qty);
                if (s <= UnQty)
                {
                    idal.IReceiptPartialRegisterDetailDAL.Add(entity);
                    idal.SaveChanges();

                    return "Y";
                }

                else
                {
                    return "已超最大未收货数量，不允许登记！";
                }
            }
            else
            {
                int s = 0;
                s += Convert.ToInt32(entity.Qty);
                if (s <= UnQty)
                {
                    idal.IReceiptPartialRegisterDetailDAL.Add(entity);
                    idal.SaveChanges();


                    ReceiptPartialRegister reg = (from a in idal.IReceiptPartialRegisterDAL.SelectAll()
                                                  where a.WhCode == entity.WhCode && a.ReceiptId == entity.ReceiptId
                                                  select a).ToList().First();
                    if (reg.Status == "U")
                    {
                        reg.Status = "A";
                        idal.IReceiptPartialRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "Status" });
                        idal.SaveChanges();
                    }

                    return "Y";
                }
                else
                {
                    return "已超最大未收货数量，不允许登记！";
                }
            }


        }

        //删除明细
        public string ReceiptPartialDeleteDetail(int Id, string rediptId, string WhCode)
        {

            idal.IReceiptPartialRegisterDetailDAL.DeleteBy(u => u.Id == Id);
            idal.IReceiptPartialRegisterDetailDAL.SaveChanges();

            var sql = from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                      where a.WhCode == WhCode && a.ReceiptId == rediptId
                      select a;

            if (sql.Count() == 0)
            {
                ReceiptPartialRegister reg = (from a in idal.IReceiptPartialRegisterDAL.SelectAll()
                                              where a.WhCode == WhCode && a.ReceiptId == rediptId
                                              select a).ToList().First();
                if (reg.Status == "A")
                {
                    reg.Status = "U";
                    idal.IReceiptPartialRegisterDAL.UpdateBy(reg, u => u.ReceiptId == reg.ReceiptId, new string[] { "Status" });
                    idal.SaveChanges();
                }
            }
            return "Y";
        }

        //确认完成
        public string ReceiptPartialComplete(string ReceiptId, string WhCode)
        {
            var sql = from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                      group a by
                      new
                      {
                          a.WhCode,
                          a.ReceiptId,
                      }
                           into g
                      select new
                      {
                          qty = g.Sum(p => (p.Qty ?? 0))
                      };

            if (sql.Count() > 0)
            {

                ReceiptPartialRegister reg = (from a in idal.IReceiptPartialRegisterDAL.SelectAll()
                                              where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                                              select a).ToList().First();
                int s = Convert.ToInt32(sql.ToList().First().qty);
                if (s < reg.Qty)
                {
                    return "数量未完全登记，不允许确认";
                }
                else
                {

                    reg.Status = "C";
                    idal.IReceiptPartialRegisterDAL.UpdateBy(reg, u => u.ReceiptId == reg.ReceiptId, new string[] { "Status" });
                    idal.SaveChanges();
                    return "Y";
                }
            }
            else
            {
                return "未登记，无法完成";
            }


        }


        //撤销完成
        public string ReceiptPartialReBack(string ReceiptId, string WhCode)
        {
            var sql = from a in idal.IReceiptPartialRegisterDetailDAL.SelectAll()
                      where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                      select a;
            ReceiptPartialRegister reg = (from a in idal.IReceiptPartialRegisterDAL.SelectAll()
                                          where a.WhCode == WhCode && a.ReceiptId == ReceiptId
                                          select a).ToList().First();
            if (sql.Count() > 0)
            {
                reg.Status = "A";
                idal.IReceiptPartialRegisterDAL.UpdateBy(reg, u => u.ReceiptId == reg.ReceiptId, new string[] { "Status" });
                idal.SaveChanges();
                return "Y";
            }
            else
            {
                reg.Status = "U";
                idal.IReceiptPartialRegisterDAL.UpdateBy(reg, u => u.ReceiptId == reg.ReceiptId, new string[] { "Status" });
                idal.SaveChanges();
                return "Y";
            }
        }


        //拒收登记异常原因下拉列表
        public List<HoldMaster> HoldMasterListByReceiptPart(HoldMasterSearch searchEntity)
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
                      where a.ReasonType == "Return" && a.WhCode == searchEntity.WhCode &&
                      (a.ClientId == searchEntity.ClientId || a.ClientCode == "all")
                      select a;

            sql = sql.OrderBy(u => u.Id);

            return sql.ToList();
        }


        //拒收登记照片上传
        public string ReceiptPartPhotoUpload(ReceiptPartialRegister entity)
        {
            if ((entity.PhotoId ?? 0) != 0)
            {
                ReceiptPartialRegister photo = new ReceiptPartialRegister();
                photo.PhotoId = entity.PhotoId;
                photo.UploadDate = DateTime.Now;
                idal.IReceiptPartialRegisterDAL.UpdateBy(photo, u => u.Id == entity.Id, new string[] { "PhotoId", "UploadDate" });
            }
            idal.SaveChanges();
            return "Y";
        }

        #endregion



        #region 32.收货收费特殊项目收费管理

        //查询
        public List<ContractFormExtend> ContractFormExtendList(ContractFormExtendSearch searchEntity, out int total)
        {
            var sql = from a in idal.IContractFormExtendDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //修改信息
        public string ContractFormExtendEdit(ContractFormExtend entity)
        {
            idal.IContractFormExtendDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Price" });
            idal.SaveChanges();
            return "Y";
        }


        #endregion



        #region 33.托盘称重管理

        //列表查询
        public List<HuMasterResult1> HuMasterHeavyPalletList(HuMasterSearch1 searchEntity, string[] soNumber, out int total)
        {
            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      join b in idal.IHuDetailDAL.SelectAll()
                       on new { a.WhCode, a.HuId } equals new { b.WhCode, b.HuId }
                      join e in (
                          (from a0 in idal.IHuDetailDAL.SelectAll()
                           join b1 in idal.IHuMasterDAL.SelectAll()
                           on new { a0.WhCode, a0.HuId } equals new { b1.WhCode, b1.HuId }
                           group new { a0, b1 } by new
                           {
                               a0.SoNumber,
                               a0.CustomerPoNumber,
                               a0.AltItemNumber,
                               a0.WhCode
                           } into g
                           select new
                           {
                               g.Key.SoNumber,
                               g.Key.CustomerPoNumber,
                               g.Key.AltItemNumber,
                               g.Key.WhCode,
                               recCBM = (System.Decimal?)g.Sum(p => p.a0.Qty * p.a0.Height * p.a0.Length * p.a0.Width)
                           }))
                            on new { b.WhCode, b.SoNumber, b.CustomerPoNumber, b.AltItemNumber }
                        equals new { e.WhCode, e.SoNumber, e.CustomerPoNumber, e.AltItemNumber } into e_join
                      from e in e_join.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      select new HuMasterResult1
                      {
                          ClientCode = b.ClientCode,
                          SoNumber = b.SoNumber,
                          PoNumber = b.CustomerPoNumber,
                          ReceiptDate = b.ReceiptDate,
                          ReceiptId = b.ReceiptId,
                          HuId = b.HuId,
                          LocationId = a.Location,
                          ItemNumber = b.AltItemNumber,
                          Qty = b.Qty,
                          recCBM = e.recCBM,
                          Height = a.HuHeight,
                          Length = a.HuLength,
                          Width = a.HuWidth,
                          HuWeight = a.HuWeight,
                          Show = ""
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.PoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.ItemNumber.Contains(searchEntity.AltItemNumber));

            if (soNumber != null)
                sql = sql.Where(u => soNumber.Contains(u.SoNumber));

            if (searchEntity.BeginReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.BeginReceiptDate);
            }
            if (searchEntity.EndReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.EndReceiptDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.PoNumber).ThenBy(u => u.ItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //托盘修改长宽高
        public string HuMasterHeavyPalletEdit(HuMasterResult1 entity)
        {
            List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);

            if (huMasterList.Count == 0)
            {
                return "托盘不存在请重新查询！";
            }
            else
            {
                HuMaster huMaster = huMasterList.First();
                huMaster.HuLength = entity.Length;
                huMaster.HuWidth = entity.Width;
                huMaster.HuHeight = entity.Height;
                huMaster.HuWeight = entity.HuWeight;
                huMaster.UpdateDate = DateTime.Now;

                idal.IHuMasterDAL.UpdateBy(huMaster, u => u.WhCode == entity.WhCode && u.HuId == entity.HuId, new string[] { "HuLength", "HuWidth", "HuHeight", "HuWeight", "UpdateUser", "UpdateDate" });
                idal.IHuMasterDAL.SaveChanges();
                return "Y";
            }
        }

        #endregion


        #region 34.款号Code管理
        //款号Code列表
        public List<ItemMasterColorCode> ItemMasterColorCodeList(WhItemSearch searchEntity, out int total)
        {
            var sql = from a in idal.IItemMasterColorCodeDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ColorCode))
                sql = sql.Where(u => u.ColorCode.Contains(searchEntity.ColorCode));

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量导入款号Code
        public string ItemMasterColorCodeImports(string[] clientCode, string[] colorCode, string[] colorDescription, string whCode, string userName)
        {
            if (colorCode.Count() == 0)
            {
                return "请导入数据！";
            }

            idal.IItemMasterColorCodeDAL.DeleteByExtended(u => clientCode.Contains(u.ClientCode) && colorCode.Contains(u.ColorCode) && u.WhCode == whCode);

            List<ItemMasterColorCode> ListAdd = new List<ItemMasterColorCode>();

            for (int j = 0; j < clientCode.Count(); j++)
            {
                if (string.IsNullOrEmpty(clientCode[j]) || string.IsNullOrEmpty(colorCode[j]))
                {
                    break;
                }
                ItemMasterColorCode itemMaster = new ItemMasterColorCode();
                itemMaster.WhCode = whCode;
                itemMaster.ClientCode = clientCode[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                itemMaster.ColorCode = colorCode[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                itemMaster.ColorDescription = colorDescription[j].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                ListAdd.Add(itemMaster);
            }
            idal.IItemMasterColorCodeDAL.Add(ListAdd);
            idal.IItemMasterColorCodeDAL.SaveChanges();
            return "";

        }

        //修改
        public string ItemColorCodeEdit(ItemMasterColorCode im)
        {
            idal.IItemMasterColorCodeDAL.UpdateBy(im, u => u.Id == im.Id, new string[] { "ColorDescription" });
            idal.SaveChanges();
            return "Y";
        }

        #endregion


        #region 35.退货上架库位管理

        public List<WhLocationResult> ReturnGoodLocationList(WhLocationSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhLocationDAL.SelectAll()
                      join b in idal.ILocationTypesDetailDAL.SelectAll()
                      on a.LocationTypeDetailId equals b.Id
                      where a.WhCode == searchEntity.WhCode && a.LocationTypeDetailId == 2
                      select new WhLocationResult
                      {
                          Id = a.Id,
                          LocationId = a.LocationId,
                          Status = a.Status,
                          LocationDescription = b.Description,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId.StartsWith(searchEntity.LocationId));

            if (!string.IsNullOrEmpty(searchEntity.LocationTypeId))
            {
                int LocationTypeId = Convert.ToInt32(searchEntity.LocationTypeId);
                sql = sql.Where(u => u.LocationTypeId == LocationTypeId);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public int ImportReturnGoodLocation(List<WhLocation> entity)
        {
            lock (o)
            {
                List<WhLocation> whLocationListAdd = new List<WhLocation>();
                List<Pallate> pallateListAdd = new List<Pallate>();
                foreach (var item in entity)
                {
                    if (idal.IWhLocationDAL.SelectBy(u => u.WhCode == item.WhCode && u.LocationId == item.LocationId).Count == 0)
                    {
                        item.CreateDate = DateTime.Now;
                        whLocationListAdd.Add(item);
                    }
                    else
                    {
                        idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.LocationId == item.LocationId, t => new WhLocation { LocationTypeDetailId = 2 });
                    }
                    if (idal.IPallateDAL.SelectBy(u => u.WhCode == item.WhCode && u.HuId == item.LocationId).Count == 0)
                    {
                        Pallate p = new Pallate(); ;
                        p.WhCode = item.WhCode;
                        p.HuId = item.LocationId;
                        p.TypeId = 2;
                        p.Status = "U";
                        p.CreateUser = item.CreateUser;
                        p.CreateDate = DateTime.Now;
                        pallateListAdd.Add(p);
                    }
                }
                idal.IWhLocationDAL.Add(whLocationListAdd);
                idal.IPallateDAL.Add(pallateListAdd);
                idal.IWhLocationDAL.SaveChanges();
                return 1;
            }
        }


        //批量删除捡货库位
        public string ReturnGoodLocationDel(List<WhLocation> entity)
        {
            lock (o)
            {
                foreach (var location in entity)
                {
                    idal.IWhLocationDAL.UpdateByExtended(u => u.WhCode == location.WhCode && u.LocationId == location.LocationId, t => new WhLocation { LocationTypeDetailId = 0 });
                }

                idal.SaveChanges();
                return "Y";
            }
        }


        //退货上架库位款号信息列表
        public List<R_Location_ItemResult> R_Location_ItemRGList(R_Location_ItemSearch searchEntity, out int total)
        {
            var sql = from a in idal.IR_Location_Item_RGDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new R_Location_ItemResult
                      {
                          Id = a.Id,
                          ClientCode = a.ClientCode,
                          LocationId = a.LocationId,
                          AltItemNumber = a.AltItemNumber,
                          LotNumber1 = a.LotNumber1,
                          LotNumber2 = a.LotNumber2,
                          LotDate = a.LotDate,
                          MinQty = a.MinQty,
                          MaxQty = a.MaxQty,
                          Status = a.Status,
                          StatusShow = a.Status == "UnActive" ? "未启用" :
                           a.Status == "Active" ? "启用" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                sql = sql.Where(u => u.LocationId == searchEntity.LocationId);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.Status))
                sql = sql.Where(u => u.Status == searchEntity.Status);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增退货上架库位信息
        public string R_Location_ItemRGAdd(List<R_Location_Item_RG> entity)
        {
            lock (o)
            {
                var s = (from a in entity
                         select a.ClientCode).Distinct();

                string whCode = entity.First().WhCode;

                var sql = from a in idal.IWhClientDAL.SelectAll()
                          where a.WhCode == whCode && s.Contains(a.ClientCode)
                          select a;

                string mess = "";
                if (sql.Count() != s.Count())
                {
                    Hashtable sqlResult = new Hashtable();
                    Hashtable listResult = new Hashtable();
                    int count = 0;
                    int count1 = 0;
                    foreach (var item in s)
                    {
                        listResult.Add(count, item);
                        count++;
                    }
                    foreach (var item in sql)
                    {
                        sqlResult.Add(count1, item);
                        count1++;
                    }

                    for (int i = 0; i < listResult.Count; i++)
                    {
                        if (mess == "")
                        {
                            if (sqlResult.ContainsValue(listResult[i]) == false)
                            {
                                mess = listResult[i].ToString();
                            }
                        }
                    }

                }
                if (mess != "")
                {
                    return "该客户不存在，请检查：" + mess;
                }

                string result = "";

                List<R_Location_Item_RG> ListAdd = new List<R_Location_Item_RG>();

                List<ItemMaster> ItemMasterListAdd = new List<ItemMaster>();
                WhClient whClient = new WhClient();
                foreach (var item in entity)
                {
                    string ClientCode = item.ClientCode;
                    string AltItemNumber = item.AltItemNumber;
                    string LocationId = item.LocationId;

                    whClient = sql.Where(u => u.WhCode == whCode && u.ClientCode == ClientCode).First();

                    List<WhLocation> whLocationList = idal.IWhLocationDAL.SelectBy(u => u.LocationId == LocationId && u.WhCode == whCode && u.LocationTypeDetailId == 2);
                    if (whLocationList.Count == 0)
                    {
                        result = "退货上架库位不存在，请确认：" + LocationId;
                        break;
                    }

                    List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == whCode && u.AltItemNumber == AltItemNumber && u.ClientId == whClient.Id).OrderBy(u => u.Id).ToList();
                    if (itemMasterList.Count == 0)
                    {
                        result = "款号不存在，请确认：" + AltItemNumber + ",客户名：" + ClientCode;
                        break;
                    }

                    ItemMaster itemMaster = itemMasterList.First();

                    item.ItemId = itemMaster.Id;
                    item.EAN = itemMaster.EAN;
                    item.WhLocationId = whLocationList.First().Id;
                    item.CreateDate = DateTime.Now;
                    if (item.Status == null || item.Status == "")
                    {
                        item.Status = "Active";
                    }

                    ListAdd.Add(item);
                }
                if (result != "")
                {
                    return result;
                }

                foreach (var item in ListAdd)
                {
                    idal.IR_Location_Item_RGDAL.DeleteByExtended(u => u.ClientCode == item.ClientCode && u.WhLocationId == item.WhLocationId && u.WhCode == item.WhCode);
                }

                idal.IR_Location_Item_RGDAL.Add(ListAdd);
                idal.IR_Location_Item_RGDAL.SaveChanges();
                return "Y";
            }

        }

        public int R_Location_ItemRGEdit(R_Location_Item_RG entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IR_Location_Item_RGDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "MinQty", "MaxQty", "Status", "UpdateUser", "UpdateDate" });
            idal.IR_Location_Item_RGDAL.SaveChanges();
            return 1;
        }



        //批量删除信息
        public string R_Location_Item_RG_Del(List<R_Location_Item_RG> entity)
        {
            lock (o)
            {
                foreach (var item in entity)
                {
                    idal.IR_Location_Item_RGDAL.DeleteBy(u => u.Id == item.Id);
                }

                idal.SaveChanges();
                return "Y";
            }
        }


        #endregion

        #region 37.照片上传
        //CFS收货照片查询
        public List<PhotoMasterResult> TCRRecPhotoMasterList(PhotoMasterApiSearch searchEntity, out int total)
        {
            string type = "in";

            if (searchEntity.PhotoType + "" != "")
            {
                type = searchEntity.PhotoType;
            }

            var sql = (from a in idal.IPhotoMasterDAL.SelectAll()
                       join b in idal.IHuMasterDAL.SelectAll()
                       on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId } into temp1
                       from b in temp1.DefaultIfEmpty()
                       join f in idal.IUnitDefaultDAL.SelectAll()
                       on new { A = a.WhCode, B = a.UnitName } equals new { A = f.WhCode, B = f.UnitName } into temp2
                       from f in temp2.DefaultIfEmpty()
                       join c in idal.IItemMasterDAL.SelectAll()
                       on a.ItemId equals c.Id into temp3
                       from c in temp3.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && a.Type == type && (a.CheckStatus2 ?? "N") != "Y"
                       select new PhotoMasterResult
                       {
                           Action = a.Id.ToString(),
                           PhotoId = a.PhotoId ?? 0,
                           ClientCode = a.ClientCode ?? "",
                           Number = a.Number,
                           Number2 = a.Number2 ?? "",
                           Number3 = a.Number3 ?? "",
                           Number4 = a.Number4 ?? "",
                           UnitName = ((f.UnitNameCN ?? "") == "" ? a.UnitName : f.UnitNameCN),
                           Qty = a.Qty ?? 0,
                           RegQty = a.RegQty ?? 0,
                           HuId = a.HuId,
                           LocationId = b.Location,
                           HoldReason = a.HoldReason ?? "",
                           TCRStatus = a.TCRStatus ?? "",
                           TCRCheckUser = a.TCRCheckUser ?? "",
                           TCRCheckDate = a.TCRCheckDate,
                           UpdateUser = a.UpdateUser,
                           TCRProcessMode = a.TCRProcessMode ?? "",
                           SettlementMode = a.SettlementMode ?? "",
                           SumPrice = a.SumPrice ?? 0,
                           DeliveryDate = a.DeliveryDate,
                           Status =
                              (a.Status ?? 0) != 0 ? "已上传" : "未上传",
                           UploadDate = a.UploadDate,
                           CheckStatus1 =
                              (a.CheckStatus1 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser1 = a.CheckUser1,
                           CheckDate1 = a.CheckDate1,
                           KRemark1 = a.KRemark1 ?? "",
                           CheckStatus2 =
                              (a.CheckStatus2 ?? "N") != "N" ? "已审核" : "未审核",
                           CheckUser2 = a.CheckUser2,
                           CheckDate2 = a.CheckDate2,
                           CRemark1 = a.CRemark1 ?? "",
                           CreateUser = a.CreateUser,
                           CreateDate = a.CreateDate,
                           OrderSource = a.OrderSource,
                           UpdateDate = a.UpdateDate,
                           UserCode = a.CreateUser,
                           UserNameCN = "",
                           Style1 = c.Style1 ?? "",
                           Style2 = c.Style2 ?? "",
                           Style3 = c.Style3 ?? ""
                       }).Distinct();


            if (!string.IsNullOrEmpty(searchEntity.Number2))
                sql = sql.Where(u => u.Number2 == searchEntity.Number2);
            if (searchEntity.status == "N")
                sql = sql.Where(u => u.PhotoId == null || u.PhotoId == 0);
            if (searchEntity.status == "Y")
                sql = sql.Where(u => u.PhotoId != null && u.PhotoId != 0);
            if (searchEntity.CreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.CreateDate);
            }
            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();
            List<PhotoMasterResult> list = new List<PhotoMasterResult>();
            foreach (var item in sql)
            {
                PhotoMasterResult work = item;
                List<WhUser> userCheck = userList.Where(u => u.UserCode == item.TCRCheckUser).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.TCRCheckUser = user.UserNameCN;
                }

                List<WhUser> userCheck1 = userList.Where(u => u.UserCode == item.CreateUser).ToList();
                if (userCheck1.Count > 0)
                {
                    WhUser user = userCheck1.First();
                    work.UserNameCN = user.UserNameCN;
                }
                list.Add(work);
            }


            list = list.OrderBy(u => u.CreateDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            total = list.Count;
            return list;
        }


        //CFS出货照片查询
        public List<PhotoMasterResult> TCROutPhotoMasterList(PhotoMasterApiSearch searchEntity, out int total)
        {
            string type = "out";

            if (searchEntity.PhotoType + "" != "")
            {
                type = searchEntity.PhotoType;
            }

            //a.LoadId,e.ClientCode,c.PhotoId,b.ContainerNumber,b.ContainerType,a.BeginPickDate
            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.ILoadContainerExtendDAL.SelectAll()
                       on new { A = a.WhCode, B = a.LoadId } equals new { A = b.WhCode, B = b.LoadId } into temp1
                       from b in temp1.DefaultIfEmpty()
                       join f in idal.IPhotoMasterDAL.SelectAll()
                       on new { A = a.WhCode, B = a.LoadId } equals new { A = f.WhCode, B = f.Number } into temp2
                       from e in temp2.DefaultIfEmpty()
                           //join ld in idal.ILoadDetailDAL.SelectAll()
                           //on new { A = a.Id } equals new { A = (Int32)ld.LoadMasterId } into temp3
                           //from c in temp3.DefaultIfEmpty()
                           //join d in idal.IOutBoundOrderDAL.SelectAll()
                           // on new { A = (Int32)c.OutBoundOrderId } equals new { A = d.Id } into temp4
                           //from h in temp4.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode && (e.Type == type || (e.Type ?? "") == "") && (e.CheckStatus2 ?? "N") != "Y"
                       select new PhotoMasterResult
                       {
                           Action = e.Id.ToString(),
                           PhotoId = e.PhotoId ?? 0,
                           ClientCode = a.ClientCode ?? "",
                           Number = a.LoadId,
                           Number2 = b.ContainerNumber ?? "",
                           Status =
                              (e.PhotoId ?? 0) != 0 ? "已上传" : "未上传",
                           ContainerType = b.ContainerType,
                           CreateDate = a.BeginPickDate,
                           HoldReason = e.HoldReason ?? "",

                       }).Distinct();


            if (!string.IsNullOrEmpty(searchEntity.Number2))
                sql = sql.Where(u => u.Number2 == searchEntity.Number2);
            if (searchEntity.status == "N")
                sql = sql.Where(u => u.PhotoId == null || u.PhotoId == 0);
            if (searchEntity.status == "Y")
                sql = sql.Where(u => u.PhotoId != null && u.PhotoId != 0);
            if (searchEntity.CreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.CreateDate);
            }
            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();
            List<PhotoMasterResult> list = new List<PhotoMasterResult>();
            foreach (var item in sql)
            {
                PhotoMasterResult work = item;
                List<WhUser> userCheck = userList.Where(u => u.UserCode == item.TCRCheckUser).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.TCRCheckUser = user.UserNameCN;
                }

                List<WhUser> userCheck1 = userList.Where(u => u.UserCode == item.CreateUser).ToList();
                if (userCheck1.Count > 0)
                {
                    WhUser user = userCheck1.First();
                    work.UserNameCN = user.UserNameCN;
                }
                list.Add(work);
            }


            list = list.OrderBy(u => u.CreateDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            total = list.Count;
            return list;
        }



        //CFS收货照片查询
        public string uploadPhotoFile(HttpFileCollection fileList, UploadPhotoApi UploadPhotoEntity)
        {
            HttpFileCollection filelist = HttpContext.Current.Request.Files;
            string res = "";
            //if (string.IsNullOrEmpty(UploadPhotoEntity.fileId.ToString()) || UploadPhotoEntity.fileId.ToString() == "null" || UploadPhotoEntity.fileId.ToString() == "0")
            //{
            UploadPhotoEntity.fileId = upload(UploadPhotoEntity).ToString();

            //}
            if (fileList == null)
                return "没图片数据";

            HttpPostedFile file = null;
            if (filelist != null && filelist.Count > 0)
            {
                for (int i = 0; i < filelist.Count; i++)
                {
                    file = filelist[i];
                    res += SaveFilesTo(file, UploadPhotoEntity);
                }
            }


            if (res == "")
            {
                return "Y";
            }
            else
                throw new Exception("照片服务器异常");

        }

        //获取PhotoId
        public string upload(UploadPhotoApi UploadPhotoEntity)
        {
            string fileId = "0";
            var sql = from a in idal.IPhotoMasterDAL.SelectAll()
                      where a.WhCode == UploadPhotoEntity.whCode && a.Id == UploadPhotoEntity.id
                      select a;
            if (!string.IsNullOrEmpty(UploadPhotoEntity.id.ToString()))
            {
                List<PhotoMaster> photoList = idal.IPhotoMasterDAL.SelectBy(u => u.Id == UploadPhotoEntity.id && u.WhCode == UploadPhotoEntity.whCode);
                if (photoList.Count > 0)
                {
                    //调取API访问98
                    RecManager Rec = new RecManager();
                    fileId = Rec.getFileId(UploadPhotoEntity.userId).ToString();
                    PhotoMaster photo = new PhotoMaster();
                    photo.PhotoId = int.Parse(fileId);
                    photo.UpdateUser = UploadPhotoEntity.userId;
                    photo.Status = 1;
                    photo.UpdateDate = DateTime.Now;
                    idal.IPhotoMasterDAL.UpdateBy(photo, u => u.Id == UploadPhotoEntity.id, new string[] { "PhotoId", "UpdateUser", "UpdateDate", "Status" });
                    idal.SaveChanges();
                }
            }



            return fileId;
        }

        public HostPW getHostPW(string host)
        {

            HostPW hp = new HostPW();
            hp.host = host;
            switch (host)
            {

                case "10.88.88.98":
                    hp.ud = "administrator";
                    hp.pw = "1qaz2wsx,";

                    break;

                case "10.88.88.161":
                    hp.ud = "edipicuse01@oe.com";
                    hp.pw = "Mar20th2024!@#";
                    break;
            }
            return hp;
        }


        public string Connect(string remoteHost, string userName, string passWord)
        {
            string res = "Y";
            if (!Ping(remoteHost))
            {
                return "Y";
            }

            Process proc = new Process();
            try
            {

                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                //  net use \\10.88.88.15 1qaz2wsx, / user:10.88.88.15\eip > NUL
                // string dosLine = @"net use \\" + remoteHost + " " + passWord + " " + " /user:"+ remoteHost + "\\" + userName + ">NUL";
                string dosLine = @"net use \\" + remoteHost + " " + passWord + " " + " /user:" + userName + ">NUL";
                // return dosLine;
                //  proc.StandardInput.WriteLine("net use * / del / y");
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(1000);
                }

                string errormsg = proc.StandardError.ReadToEnd();
                if (errormsg != "")
                {

                    res = errormsg;
                }
                proc.StandardError.Close();

                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                // return ex.ToString();
            }
            finally
            {
                try
                {
                    proc.Close();
                    proc.Dispose();
                }
                catch
                {
                }
            }
            //return "Y";
        }


        //上传
        public string SaveFilesTo(HttpPostedFile file, UploadPhotoApi UploadPhotoEntity)
        {

            string fileId = UploadPhotoEntity.fileId.ToString();
            string destHost = "10.88.88.161";
            string destinationFile = "ElectronicDocument";
            if (Ping(destHost))
            {

                HostPW hp = getHostPW(destHost);
                string res = Connect(destHost, hp.ud, hp.pw);
                if (res == "Y")
                {
                    // string sourceFile = @"d:\\aa.zip";
                    // string destinationFile = @"\\10.88.88.90\eip_old\aa.zip";
                    //FileInfo file = new FileInfo(sourceFile);
                    //if (file.Exists)
                    //{
                    // true is overwrite


                    string urlPath = "\\\\" + destHost + "\\" + destinationFile + "\\" + fileId;

                    string filePathName = string.Empty;
                    string localPath = Path.Combine("", urlPath);
                    //string ex = Path.GetExtension(file.FileName);


                    // filePathName = Guid.NewGuid().ToString("N") + ex;
                    try
                    {
                        if (!Directory.Exists(urlPath))
                        {
                            Directory.CreateDirectory(urlPath);
                        }


                        // file.SaveAs(Path.Combine(localPath, file.FileName));

                        file.SaveAs(Path.Combine(localPath, file.FileName));
                        return "";
                        // sourceFile.CopyTo(destinationFile, true);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                        // return ex.ToString();
                    }



                }
                else
                    return res;


            }
            else
                return "N";
        }

        public bool Ping(string remoteHost)
        {
            bool Flag = false;
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                string dosLine = @"ping -n 1 " + remoteHost;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(500);
                }
                string pingResult = proc.StandardOutput.ReadToEnd();
                if (pingResult.IndexOf("(0% 丢失)") != -1)
                {
                    Flag = true;
                }
                proc.StandardOutput.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                try
                {
                    proc.Close();
                    proc.Dispose();
                }
                catch
                {
                }
            }
            return Flag;
        }

        #endregion
    }
}
