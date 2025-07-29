using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using WMS.BLLClass;
using MODEL_MSSQL;
using WMS.IBLL;
using System.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WMS.BLL
{
    public class AdminManager : IAdminManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();

        MD5 md5Hasher = MD5.Create();

        //验证当前登录用户
        public WhUser LoginIn(WhUser whUser)
        {
            if (!string.IsNullOrEmpty(whUser.UserName) && !string.IsNullOrEmpty(whUser.PassWord))
            {
                //byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(whUser.PassWord));
                //whUser.PassWord = BitConverter.ToString(data);

                var sql = from a in idal.IWhUserDAL.SelectAll()
                          where a.Status == "Active"
                          select a;

                if (!string.IsNullOrEmpty(whUser.UserName))
                {
                    sql = sql.Where(u => u.UserName == whUser.UserName);
                }
                if (!string.IsNullOrEmpty(whUser.PassWord))
                {
                    sql = sql.Where(u => u.PassWord == whUser.PassWord);
                }

                List<WhUser> list = sql.ToList();
                if (list.Count > 0)
                {
                    if (whUser.CreateUser != null)
                        idal.ILoginLogDAL.Add(new LoginLog { UserName = whUser.UserName, DeviceName = whUser.CreateUser, CreateDate = DateTime.Now });
                    idal.IWhUserDAL.SaveChanges();
                    return list.First();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //登录后 通过公司ID 获取该公司的仓库数组
        public List<WhInfoResult> WhInfoList(int companyId, string userName)
        {
            if (companyId == 0)
            {
                var sql = from a in idal.IWhInfoDAL.SelectAll()
                          select new WhInfoResult
                          {
                              Id = a.Id,
                              WhCode = a.WhCode,
                              WhName = a.WhName,
                              CompanyId = a.CompanyId

                          };
                return sql.ToList();
            }
            else
            {
                int compid = idal.IWhUserDAL.SelectBy(u => u.UserName == userName).First().CompanyId;
                if (compid == 0)
                {
                    var sql = from a in idal.IWhInfoDAL.SelectAll()
                              select new WhInfoResult
                              {
                                  Id = a.Id,
                                  WhCode = a.WhCode,
                                  WhName = a.WhName,
                                  CompanyId = a.CompanyId
                              };
                    return sql.ToList();
                }
                else
                {
                    var sql = from a in idal.IWhUserDAL.SelectAll()
                              join b in idal.IR_WhInfo_WhUserDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.UserId } into b_join
                              from b in b_join.DefaultIfEmpty()
                              join c in idal.IWhInfoDAL.SelectAll() on new { WhCodeId = b.WhCodeId } equals new { WhCodeId = c.Id }
                              where
                                a.CompanyId == companyId && a.UserName == userName
                              group c by new
                              {
                                  c.Id,
                                  c.WhCode,
                                  c.WhName,
                                  c.CompanyId,
                                  c.UpdateDate
                              } into g
                              select new WhInfoResult
                              {
                                  Id = g.Key.Id,
                                  WhCode = g.Key.WhCode,
                                  WhName = g.Key.WhName,
                                  CompanyId = g.Key.CompanyId
                              };

                    return sql.ToList();
                }

            }
        }

        //修改密码
        public int UserUpdatePwd(WhUser entity)
        {
            //var sql = from a in idal.IWhUserDAL.SelectAll()
            //          where a.Status == "Active"
            //          select a;

            //if (!string.IsNullOrEmpty(entity.UserName))
            //{
            //    sql = sql.Where(u => u.UserName == entity.UserName);
            //}
            //if (!string.IsNullOrEmpty(entity.PassWord))
            //{
            //    sql = sql.Where(u => u.PassWord == entity.PassWord);
            //}

            //List<WhUser> list = sql.ToList();
            //if (list.Count > 0)
            //{

            //    byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(entity.PassWord));

            //    var regex = new Regex(@"
            //            (?=.*[0-9])                     #必须包含数字
            //            (?=.*[a-zA-Z])                  #必须包含小写或大写字母
            //            (?=([\x21-\x7e]+)[^a-zA-Z0-9])  #必须包含特殊符号
            //            .{8,16}                         #至少8个字符，最多16个字符
            //            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            //    //校验密码是否符合
            //    bool pwdIsMatch = regex.IsMatch(entity.PassWord);
            //    if (pwdIsMatch)
            //    {

            //        entity.PassWord = BitConverter.ToString(data);
            //        entity.UpdateDate = DateTime.Now;
            //        idal.IWhUserDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "PassWord", "UpdateDate" });

            //        idal.IWhUserDAL.SaveChanges();
            //        return 1;
            //    }
            //    else
            //        return 0;
            //}else
            //    return 0;


            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(entity.PassWord));
            entity.PassWord = BitConverter.ToString(data);
            entity.UpdateDate = DateTime.Now;
            idal.IWhUserDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "PassWord", "UpdateDate" });

            idal.IWhUserDAL.SaveChanges();
            return 1;

        }

        //超级管理员公司下拉菜单列表
        public IEnumerable<WhCompany> WhCompanyList()
        {
            var sql = from a in idal.IWhCompanyDAL.SelectAll()
                      select a;
            return sql;
        }

        //WMS工作台特殊权限
        public List<WhPosition> GetWorkPowerByUser(string userCode)
        {
            List<WhPosition> list = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhUserPositionDAL.SelectAll()
                                     on a.Id equals b.UserId
                                     join c in idal.IWhPositionDAL.SelectAll()
                                     on b.PositionId equals c.Id
                                     where a.UserCode == userCode
                                     select c).Distinct().ToList();

            return list;
        }


        #region 1.用户管理

        //新增用户
        //对应UserInfoController中的 AddUser 方法
        public WhUser WhUserAdd(WhUser entity)
        {
            if (WhUserCheck(entity) > 0)
            {
                byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(entity.PassWord));
                entity.PassWord = BitConverter.ToString(data);
                idal.IWhUserDAL.Add(entity);
                idal.IWhUserDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //批量新增用户与仓库关系
        public int R_WhInfo_WhUserAdd(List<R_WhInfo_WhUser> entity)
        {
            foreach (var item in entity)
            {
                if (idal.IR_WhInfo_WhUserDAL.SelectBy(u => u.UserId == item.UserId && u.WhCodeId == item.WhCodeId).Count() == 0)
                {
                    idal.IR_WhInfo_WhUserDAL.Add(item);
                }
            }
            idal.IWhUserPositionDAL.SaveChanges();
            return 1;
        }

        //验证用户是否存在
        public int WhUserCheck(WhUser entity)
        {
            if (idal.IWhUserDAL.SelectBy(u => u.UserName == entity.UserName && u.CompanyId == entity.CompanyId && u.Id != entity.Id).Count() == 0)
            {
                return 1;
            }
            else
                return 0;
        }

        //验证用户是否存在
        public WhUser WhUserInfoCheck(WhUser entity)
        {
            if (idal.IWhUserDAL.SelectBy(u => u.UserName == entity.UserName && u.CompanyId == entity.CompanyId && u.Id != entity.Id).Count() == 0)
            {
                return null;
            }
            else
                return idal.IWhUserDAL.SelectBy(u => u.UserName == entity.UserName && u.CompanyId == entity.CompanyId && u.Id != entity.Id).First();
        }

        //用户列表
        //对应UserInfoController中的 List 方法
        public List<WhUserResult> WhUserList(WhUserSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhUserDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId
                      join b in idal.IWhUserPositionDAL.SelectAll()
                      on a.Id equals b.UserId into b_temp
                      from b in b_temp.DefaultIfEmpty()
                      join c in idal.IWhPositionDAL.SelectAll()
                      on b.PositionId equals c.Id into c_temp
                      from c in c_temp.DefaultIfEmpty()
                      select new WhUserResult
                      {
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          UserName = a.UserName,
                          UserNameCN = a.UserNameCN,
                          PassWord = a.PassWord,
                          Status = a.Status,
                          UserCode = a.UserCode,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          PositionNameCN = c.PositionNameCN,
                          CheckFlag = a.CheckFlag
                      };

            if (!string.IsNullOrEmpty(searchEntity.UserCode))
                sql = sql.Where(u => u.UserCode.Contains(searchEntity.UserCode));
            if (!string.IsNullOrEmpty(searchEntity.UserNameCN))
                sql = sql.Where(u => u.UserNameCN.Contains(searchEntity.UserNameCN));
            if (!string.IsNullOrEmpty(searchEntity.UserName))
                sql = sql.Where(u => u.UserName.Contains(searchEntity.UserName));


            List<WhUserResult> list = new List<WhUserResult>();
            foreach (var item in sql)
            {
                if (list.Where(u => u.Id == item.Id).Count() == 0)
                {
                    WhUserResult loadMaster = new WhUserResult();
                    loadMaster.Id = item.Id;
                    loadMaster.CompanyId = item.CompanyId;
                    loadMaster.UserName = item.UserName;
                    loadMaster.UserNameCN = item.UserNameCN;
                    loadMaster.PassWord = item.PassWord;
                    loadMaster.Status = item.Status;
                    loadMaster.UserCode = item.UserCode;
                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.PositionNameCN = item.PositionNameCN ?? "";
                    loadMaster.WhName = "";
                    loadMaster.CheckFlag= item.CheckFlag;
                    list.Add(loadMaster);
                }
                else
                {
                    WhUserResult getModel = list.Where(u => u.Id == item.Id).First();
                    list.Remove(getModel);

                    WhUserResult loadMaster = new WhUserResult();
                    loadMaster.Id = item.Id;
                    loadMaster.CompanyId = item.CompanyId;
                    loadMaster.UserName = item.UserName;
                    loadMaster.UserNameCN = item.UserNameCN;
                    loadMaster.PassWord = item.PassWord;
                    loadMaster.Status = item.Status;
                    loadMaster.UserCode = item.UserCode;
                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.PositionNameCN = getModel.PositionNameCN + "," + item.PositionNameCN;
                    loadMaster.WhName = "";
                    loadMaster.CheckFlag = item.CheckFlag;
                    list.Add(loadMaster);
                }
            }

            int?[] idArr = (from a in list
                            select a.Id).ToList().Distinct().ToArray();

            List<WhInfoResult> getWhUserWhInfoList = (from a in idal.IR_WhInfo_WhUserDAL.SelectAll()
                                                      join b in idal.IWhInfoDAL.SelectAll()
                                                      on a.WhCodeId equals b.Id into b_temp
                                                      from b in b_temp.DefaultIfEmpty()
                                                      where idArr.Contains(a.UserId)
                                                      select new WhInfoResult
                                                      {
                                                          UserId = a.UserId,
                                                          WhName = b.WhName ?? ""
                                                      }).ToList();

            List<WhUserResult> list1 = new List<WhUserResult>();
            foreach (var item in list)
            {
                if (getWhUserWhInfoList.Where(u => u.UserId == item.Id).Count() == 0)
                {
                    list1.Add(item);
                }
                else
                {
                    string whName = "";
                    List<WhInfoResult> getList = getWhUserWhInfoList.Where(u => u.UserId == item.Id).ToList();
                    foreach (var item1 in getList)
                    {
                        whName += item1.WhName + ",";
                    }
                    item.WhName = whName.Substring(0, whName.Length - 1);
                    list1.Add(item);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.WhCodeName))
                list1 = list1.Where(u => u.WhName.Contains(searchEntity.WhCodeName)).ToList();
            if (!string.IsNullOrEmpty(searchEntity.PositionNameCN))
                list1 = list1.Where(u => u.PositionNameCN.Contains(searchEntity.PositionNameCN)).ToList();

            total = list1.Count();
            list1 = list1.OrderByDescending(u => u.Id).ToList();
            list1 = list1.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list1;
        }

        //根据当前用户查询出未选择职位
        //对应UserInfoController中的 WhPositionUnselected 方法
        public List<WhPositionResult> WhPositionUnselected(WhPositionSearch searchEntity, out int total)
        {
            var sql1 = from b in idal.IWhUserPositionDAL.SelectAll()
                       where b.UserName == searchEntity.UserName && b.CompanyId == searchEntity.CompanyId
                       select b.PositionId;

            var sql = from a in idal.IWhPositionDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.CompanyId == searchEntity.CompanyId && a.Status == "Active"
                      select new WhPositionResult
                      {
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          PositionName = a.PositionName,
                          PositionNameCN = a.PositionNameCN
                      };
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据当前用户查询出已选择职位
        //对应UserInfoController中的 WhPositionSelected 方法
        public List<WhUserWhPositionResult> WhPositionSelected(WhUserWhPositionSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhUserDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId && a.UserName == searchEntity.UserName
                      join b in idal.IWhUserPositionDAL.SelectAll()
                      on new { A = a.Id, B = a.UserName, C = a.CompanyId } equals new { A = b.UserId, B = b.UserName, C = b.CompanyId }
                      join c in idal.IWhPositionDAL.SelectAll()
                      on new { A = b.PositionId, B = b.PositionName, C = b.CompanyId } equals new { A = c.Id, B = c.PositionName, C = c.CompanyId }
                      where c.Status == "Active"
                      select new WhUserWhPositionResult
                      {
                          Id = b.Id,
                          Action = "",
                          CompanyId = a.CompanyId,
                          PositionName = c.PositionName,
                          PositionNameCN = c.PositionNameCN
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //根据当前用户查询出未选择仓库
        //对应UserInfoController中的 WhInfoUnselected 方法
        public List<WhInfoResult> WhInfoUnselected(WhInfoSearch searchEntity, out int total)
        {
            var sql1 = from a in idal.IWhUserDAL.SelectAll()
                       join b in idal.IR_WhInfo_WhUserDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.UserId } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IWhInfoDAL.SelectAll() on new { WhCodeId = b.WhCodeId } equals new { WhCodeId = c.Id }
                       where
                         a.CompanyId == searchEntity.CompanyId && a.UserName == searchEntity.UserName
                       group c by new
                       {
                           c.Id,
                           c.WhCode,
                           c.WhName,
                           c.CompanyId
                       } into g
                       select g.Key.Id;

            var sql = from a in idal.IWhInfoDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.CompanyId == searchEntity.CompanyId
                      select new WhInfoResult
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          WhName = a.WhName,
                          CompanyId = a.CompanyId
                      };
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据当前用户查询出已选择仓库
        //对应UserInfoController中的 WhInfoSelected 方法
        public List<WhInfoWhUserResult> WhInfoSelected(WhInfoSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhUserDAL.SelectAll()
                      join b in idal.IR_WhInfo_WhUserDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.UserId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IWhInfoDAL.SelectAll() on new { WhCodeId = b.WhCodeId } equals new { WhCodeId = c.Id }
                      where
                        a.CompanyId == searchEntity.CompanyId && a.UserName == searchEntity.UserName
                      group c by new
                      {
                          b.Id,
                          c.WhCode,
                          c.WhName,
                          c.CompanyId
                      } into g
                      select new WhInfoWhUserResult
                      {
                          Id = g.Key.Id,
                          Action = "",
                          WhCode = g.Key.WhCode,
                          WhName = g.Key.WhName,
                          CompanyId = g.Key.CompanyId
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量添加用户对应职位
        //对应UserInfoController中的 WhUserPositionListAdd 方法
        public int WhUserPositionListAdd(List<WhUserPosition> entity)
        {
            int result = 0;
            foreach (var item in entity)
            {
                if (idal.IWhUserPositionDAL.SelectBy(u => u.CompanyId == item.CompanyId && u.UserId == item.UserId && u.UserName == item.UserName && u.PositionId == item.PositionId && u.PositionName == item.PositionName).Count() == 0)
                    idal.IWhUserPositionDAL.Add(item);
                else
                {
                    result++;
                }
            }
            if (result == 0)
            {
                return idal.IWhUserPositionDAL.SaveChanges();
            }
            else
            {
                return 0;
            }
        }

        //用户密码初始化
        //对应UserInfoController中的 WhUserPwdInit 方法
        public int WhUserPwdInit(WhUser entity)
        {
            return UserUpdatePwd(entity);
        }

        //用户信息修改
        //对应UserInfoController中的 WhUserEdit 方法
        public int WhUserEdit(WhUser entity, params string[] modifiedProNames)
        {
            if (WhUserCheck(entity) > 0)
            {
                idal.IWhUserDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
                idal.IWhUserDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        //修改用户对应的仓库
        public int WhInfoWhUserEdit(R_WhInfo_WhUser entity, params string[] modifiedProNames)
        {
            idal.IR_WhInfo_WhUserDAL.UpdateBy(entity, u => u.UserId == entity.UserId, modifiedProNames);
            idal.IR_WhInfo_WhUserDAL.SaveChanges();
            return 1;
        }

        //职位列表
        public List<WhPosition> WhPositionSelect(string whCode)
        {
            int companyId = idal.IWhInfoDAL.SelectBy(u => u.WhCode == whCode).First().CompanyId;
            var sql = from a in idal.IWhPositionDAL.SelectAll()
                      where a.CompanyId == companyId
                      select a;
            return sql.ToList();
        }

        //复制权限及仓库
        public string CopyWhUserPosition(int userId, int copyUserId, int companyId)
        {
            idal.IWhUserPositionDAL.DeleteBy(u => u.UserId == userId);
            idal.IR_WhInfo_WhUserDAL.DeleteBy(u => u.UserId == userId);

            idal.SaveChanges();

            List<WhUserPosition> positionlist = idal.IWhUserPositionDAL.SelectBy(u => u.UserId == copyUserId);
            List<R_WhInfo_WhUser> infolist = idal.IR_WhInfo_WhUserDAL.SelectBy(u => u.UserId == copyUserId);
            WhUser user = idal.IWhUserDAL.SelectBy(u => u.Id == userId).First();

            List<WhUserPosition> addpositionlist = new List<WhUserPosition>();
            List<R_WhInfo_WhUser> addinfolist = new List<R_WhInfo_WhUser>();
            foreach (var item in positionlist)
            {
                WhUserPosition position = new WhUserPosition();
                position.CompanyId = item.CompanyId;
                position.UserId = userId;
                position.UserName = user.UserName;
                position.PositionId = item.PositionId;
                position.PositionName = item.PositionName;
                position.CreateUser = "";
                position.CreateDate = DateTime.Now;
                addpositionlist.Add(position);
            }

            foreach (var item in infolist)
            {
                R_WhInfo_WhUser info = new R_WhInfo_WhUser();
                info.UserId = userId;
                info.WhCodeId = item.WhCodeId;
                info.CreateUser = "";
                info.CreateDate = DateTime.Now;
                addinfolist.Add(info);
            }

            idal.IWhUserPositionDAL.Add(addpositionlist);
            idal.IR_WhInfo_WhUserDAL.Add(addinfolist);
            idal.SaveChanges();

            return "Y";
        }

        //修改密码检测开关
        public int WhUserCheckFlagEdit(int checkFlag)
        {
            idal.IWhUserDAL.UpdateByExtended(u => u.Id > 0, t => new WhUser { CheckFlag = checkFlag });
            idal.IWhUserDAL.SaveChanges();
            return 1;
        }


        #endregion


        #region 2.职位管理

        //职位列表
        //对应PositionController中的 List 方法
        public List<WhPosition> WhPositionList(WhPositionSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhPositionDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.PositionName))
                sql = sql.Where(u => u.PositionName.Contains(searchEntity.PositionName));
            if (!string.IsNullOrEmpty(searchEntity.PositionNameCN))
                sql = sql.Where(u => u.PositionNameCN.Contains(searchEntity.PositionNameCN));

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增职位
        //对应PositionController中的 WhPositionAdd 方法
        public WhPosition WhPositionAdd(WhPosition entity)
        {
            if (idal.IWhPositionDAL.SelectBy(u => u.CompanyId == entity.CompanyId && u.PositionName == entity.PositionName).Count() == 0)
            {
                idal.IWhPositionDAL.Add(entity);
                idal.IWhPositionDAL.SaveChanges();
                return entity;
            }
            else
                return null;
        }

        //职位信息修改
        //对应PositionController中的 WhPositionEdit 方法
        public int WhPositionEdit(WhPosition entity, params string[] modifiedProNames)
        {
            idal.IWhPositionDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
            idal.IWhPositionDAL.SaveChanges();
            return 1;
        }

        //根据当前职位查询出未选择的权限信息
        //对应PositionController中的 WhPowerUnselected 方法
        public List<WhPowerResult> WhPowerUnselected(WhPowerSearch searchEntity, out int total)
        {
            var sql1 = from b in idal.IWhPositionPowerDAL.SelectAll()
                       where b.PositionName == searchEntity.PositionName && b.CompanyId == searchEntity.CompanyId
                       select b.PowerId;

            var sql = from a in idal.IWhPowerDAL.SelectAll()
                      where !sql1.Contains(a.Id) && a.CompanyId == searchEntity.CompanyId
                      select new WhPowerResult
                      {
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          PowerName = a.PowerName,
                          PowerType = a.PowerType,
                          PowerDescription = a.PowerDescription
                      };
            if (!string.IsNullOrEmpty(searchEntity.PowerName) && searchEntity.PowerName != "null")
            {
                sql = sql.Where(u => u.PowerName.Contains(searchEntity.PowerName));
            }
            if (!string.IsNullOrEmpty(searchEntity.PowerType) && searchEntity.PowerType != "null")
            {
                sql = sql.Where(u => u.PowerType.Contains(searchEntity.PowerType));
            }
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }

        //根据当前职位查询出已选择的权限信息
        //对应PositionController中的 WhPowerSelected 方法
        public List<WhPositionWhPowerResult> WhPowerSelected(WhPositionWhPowerSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhPositionDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId && a.PositionName == searchEntity.PositionName
                      join b in idal.IWhPositionPowerDAL.SelectAll()
                      on new { A = a.Id, B = a.PositionName, C = a.CompanyId } equals new { A = b.PositionId, B = b.PositionName, C = b.CompanyId }
                      join c in idal.IWhPowerDAL.SelectAll()
                      on new { A = b.PowerId, B = b.PowerName, C = b.CompanyId } equals new { A = c.Id, B = c.PowerName, C = c.CompanyId }
                      select new WhPositionWhPowerResult
                      {
                          Action = "",
                          Id = b.Id,
                          CompanyId = a.CompanyId,
                          PositionName = a.PositionName,
                          PositionNameCN = a.PositionNameCN,
                          PowerName = c.PowerName,
                          PowerDescription = c.PowerDescription,
                          PowerType = c.PowerType
                      };

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //批量添加职位权限关系
        //对应PositionController中的 WhPositionPowerListAdd 方法
        public int WhPositionPowerListAdd(List<WhPositionPower> entity)
        {
            int result = 0;
            foreach (var item in entity)
            {
                if (idal.IWhPositionPowerDAL.SelectBy(u => u.CompanyId == item.CompanyId && u.PositionId == item.PositionId && u.PositionName == item.PositionName && u.PowerId == item.PowerId && u.PowerName == item.PowerName).Count() == 0)
                    idal.IWhPositionPowerDAL.Add(item);
                else
                {
                    result++;
                }
            }
            if (result == 0)
            {
                return idal.IWhPositionPowerDAL.SaveChanges();
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region 3.权限管理

        //权限列表
        //对应PowerController中的 List 方法
        public List<WhPower> WhPowerList(WhPowerSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhPowerDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.PowerName))
                sql = sql.Where(u => u.PowerName.Contains(searchEntity.PowerName));
            if (!string.IsNullOrEmpty(searchEntity.PowerType))
                sql = sql.Where(u => u.PowerType.Contains(searchEntity.PowerType));

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增权限
        //对应PowerController中的 AddPower 方法
        public WhPower WhPowerAdd(WhPower entity)
        {
            if (WhPowerCheck(entity) > 0)
            {
                idal.IWhPowerDAL.Add(entity);
                idal.IWhPowerDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //验证权限名及权限类型是否存在
        public int WhPowerCheck(WhPower entity)
        {
            if (idal.IWhPowerDAL.SelectBy(u => u.CompanyId == entity.CompanyId && u.PowerName == entity.PowerName && u.Id != entity.Id).Count() == 0)
            {
                return 1;
            }
            else
                return 0;
        }

        //删除权限后 更新权限控制MVC 表
        //对应PowerController中的 WhPowerDelById 方法
        public int PowerMVCUpdateByPowerId(WhPositionPowerMVC entity, int powerId, params string[] modifiedProNames)
        {
            idal.IWhPositionPowerMVCDAL.UpdateBy(entity, u => u.PowerId == powerId && u.CompanyId == entity.CompanyId, modifiedProNames);
            idal.IWhPositionPowerMVCDAL.SaveChanges();
            return 1;
        }

        //根据权限查询出未选择的控制
        //对应PowerController中的 WhPowerMVCUnselected 方法
        public List<WhPositionPowerMVCResult> WhPowerMVCUnselected(WhPositionPowerMVCSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhPositionPowerMVCDAL.SelectAll()
                      where a.PowerId != searchEntity.PowerId && a.CompanyId == searchEntity.CompanyId
                      select new WhPositionPowerMVCResult
                      {
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          ParentId = a.ParentId,
                          AreaName = a.AreaName,
                          ControllerName = a.ControllerName,
                          ActionName = a.ActionName,
                          HttpMethod = a.HttpMethod,
                          PowerId = a.PowerId,
                          PowerName = a.PowerName,
                          Description = a.Description
                      };
            if (!string.IsNullOrEmpty(searchEntity.AreaName) && searchEntity.AreaName != "null")
            {
                sql = sql.Where(u => u.AreaName.Contains(searchEntity.AreaName));
            }
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据权限查询出已选择的控制
        //对应PowerController中的 WhPoweMVCSelected 方法
        public List<WhPositionPowerMVCResult> WhPoweMVCSelected(WhPositionPowerMVCSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhPositionPowerMVCDAL.SelectAll()
                      where a.PowerId == searchEntity.PowerId && a.CompanyId == searchEntity.CompanyId
                      select new WhPositionPowerMVCResult
                      {
                          Action = "",
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          ParentId = a.ParentId,
                          AreaName = a.AreaName,
                          ControllerName = a.ControllerName,
                          ActionName = a.ActionName,
                          HttpMethod = a.HttpMethod,
                          PowerId = a.PowerId,
                          PowerName = a.PowerName,
                          Description = a.Description
                      };
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //取消权限的某个控制 即更新控制器
        //对应PowerController中的 WhPowerMVCDel 方法-------------以下方法同时被俩个方法调用
        //添加权限对应的MVC关系
        //对应PowerController中的 WhPowerMVCListAdd 方法
        public int PowerMVCUpdateById(WhPositionPowerMVC entity, int Id, params string[] modifiedProNames)
        {
            idal.IWhPositionPowerMVCDAL.UpdateBy(entity, u => u.Id == Id, modifiedProNames);
            idal.IWhPositionPowerMVCDAL.SaveChanges();
            return 1;
        }

        //修改权限信息
        //对应PowerController中的 WhPowerEdit 方法
        public int WhPowerEdit(WhPower enity, params string[] modifiedProNames)
        {
            if (WhPowerCheck(enity) > 0)
            {
                idal.IWhPowerDAL.UpdateBy(enity, u => u.Id == enity.Id, modifiedProNames);
                idal.IWhPowerDAL.SaveChanges();
                return 1;
            }
            else
                return 0;
        }

        //MVC 域、control及方法同步至数据表WhPositionPowerMVC
        //对应PowerController中的 Sync 方法
        public int Sync(List<WhPositionPowerMVC> entity)
        {
            Hashtable strEnt = new Hashtable();
            Hashtable strDb = new Hashtable();

            int strEntCount = 0, strDbCount = 0;
            foreach (var item in entity)
            {
                strEnt.Add(strEntCount, item.CompanyId + "," + item.AreaName + "," + item.ControllerName + "," + item.ActionName);
                strEntCount++;

                if (idal.IWhPositionPowerMVCDAL.SelectBy(u => u.CompanyId == item.CompanyId && u.AreaName == item.AreaName && u.ControllerName == item.ControllerName && u.ActionName == item.ActionName).Count() == 0)
                    idal.IWhPositionPowerMVCDAL.Add(item);
            }
            if (strEntCount == 0)
            {
                return 1;
            }
            idal.IWhPositionPowerMVCDAL.SaveChanges();

            var sql = from a in idal.IWhPositionPowerMVCDAL.SelectAll() select a;
            foreach (var item in sql)
            {
                strDb.Add(strDbCount, item.CompanyId + "," + item.AreaName + "," + item.ControllerName + "," + item.ActionName);
                strDbCount++;
            }
            for (int i = 0; i < strDb.Count; i++)
            {
                if (strEnt.ContainsValue(strDb[i]) == false)
                {
                    string getDbValue = strDb[i].ToString();
                    int CompanyId = Convert.ToInt32(getDbValue.Split(',')[0]);
                    string areaName = getDbValue.Split(',')[1];
                    string controllerName = getDbValue.Split(',')[2];
                    string actionName = getDbValue.Split(',')[3];

                    string getEntValue = strEnt[0].ToString();      //得到前台同步设定的域
                    int EntCompanyId = Convert.ToInt32(getEntValue.Split(',')[0]);
                    string EntAreaName = getEntValue.Split(',')[1];
                    if (EntAreaName == areaName && EntCompanyId == CompanyId)    //验证是否是同一个域
                    {
                        idal.IWhPositionPowerMVCDAL.DeleteBy(u => u.CompanyId == CompanyId && u.AreaName == areaName && u.ControllerName == controllerName && u.ActionName == actionName);
                    }
                }
            }
            idal.IWhPositionPowerMVCDAL.SaveChanges();
            return 1;
        }

        #endregion


        #region 4.菜单管理

        //菜单列表
        //对应MenuController中的 List方法
        public List<WhMenuResult> WhMenuList(WhMenuSearch searchEntity, out int total)
        {
            var sql = from a in idal.IWhMenuDAL.SelectAll()
                      join b in idal.IWhMenuDAL.SelectAll()
                      on a.ParentMenuId equals b.Id into tempa
                      from ab in tempa.DefaultIfEmpty()
                      select new WhMenuResult
                      {
                          Id = a.Id,
                          CompanyId = a.CompanyId,
                          MenuName = a.MenuName,
                          MenuNameCN = a.MenuNameCN,
                          MenuUrl = a.MenuUrl,
                          MenuIcon = a.MenuIcon,
                          MenuSort = a.MenuSort,
                          PowerId = a.PowerId,
                          PowerName = a.PowerName,
                          ParentMenuId = a.ParentMenuId,
                          ParentMenuName = ab.MenuNameCN,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };

            if (searchEntity.CompanyId > 0)
                sql = sql.Where(u => u.CompanyId == searchEntity.CompanyId);
            if (!string.IsNullOrEmpty(searchEntity.MenuNameCN))
                sql = sql.Where(u => u.MenuNameCN.Contains(searchEntity.MenuNameCN));
            if (!string.IsNullOrEmpty(searchEntity.ParentId))
            {
                int pid = Convert.ToInt32(searchEntity.ParentId);
                sql = sql.Where(u => u.ParentMenuId == pid);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.ParentMenuId).ThenBy(u => u.MenuSort).ThenBy(u => u.Id);

            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增菜单，验证菜单中文名是否存在
        //对应MenuController中的 AddMenu方法
        public WhMenu WhMenuAdd(WhMenu entity)
        {
            if (WhMenuCheck(entity) > 0)
            {
                idal.IWhMenuDAL.Add(entity);
                idal.IWhMenuDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //验证菜单是否存在
        public int WhMenuCheck(WhMenu entity)
        {
            if (idal.IWhMenuDAL.SelectBy(u => u.MenuNameCN == entity.MenuNameCN && u.CompanyId == entity.CompanyId && u.Id != entity.Id).Count() == 0)
            {
                return 1;
            }
            else
                return 0;
        }

        //查询当前菜单未选择的权限
        //对应MenuController中的 WhMenuUnselected 方法
        public List<WhPower> WhMenuUnselected(WhMenuSearch searchEntity, out int total)
        {
            WhMenu whMenu = (from a in idal.IWhMenuDAL.SelectBy(u => u.CompanyId == searchEntity.CompanyId && u.Id == searchEntity.MenuId) select a).ToList().First();
            int PowerId = 0;
            if (!string.IsNullOrEmpty(whMenu.PowerId.ToString()) && whMenu.PowerId.ToString() != "null")
            {
                PowerId = (int)whMenu.PowerId;
            }
            var sql = from a in idal.IWhPowerDAL.SelectAll()
                      where a.Id != PowerId
                      select a;
            if (searchEntity.CompanyId > 0)
                sql = sql.Where(u => u.CompanyId == searchEntity.CompanyId);
            if (!string.IsNullOrEmpty(searchEntity.PowerName) && searchEntity.PowerName != "null")
                sql = sql.Where(u => u.PowerName.Contains(searchEntity.PowerName));
            if (!string.IsNullOrEmpty(searchEntity.PowerType) && searchEntity.PowerType != "null")
                sql = sql.Where(u => u.PowerType.Contains(searchEntity.PowerType));
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //查询当前菜单已选择的权限
        //对应MenuController中的 WhMenuSelected 方法
        public List<WhPower> WhMenuSelected(WhMenuSearch searchEntity, out int total)
        {
            WhMenu whMenu = (from a in idal.IWhMenuDAL.SelectBy(u => u.CompanyId == searchEntity.CompanyId && u.Id == searchEntity.MenuId) select a).ToList().First();

            int PowerId = 0;
            if (!string.IsNullOrEmpty(whMenu.PowerId.ToString()) && whMenu.PowerId.ToString() != "null")
            {
                PowerId = (int)whMenu.PowerId;
            }
            var sql = from a in idal.IWhPowerDAL.SelectAll()
                      where a.Id == PowerId
                      select a;
            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //根据菜单ID 修改权限ID和权限名
        //对应MenuController中的 WhMenuUpdateById 方法        ------------注意以下方法同时被俩个方法调用
        //菜单批量添加权限
        //对应MenuController中的 WhMenuAddPower 方法
        public int WhMenuUpdateById(WhMenu entity, int Id, params string[] modifiedProNames)
        {
            idal.IWhMenuDAL.UpdateBy(entity, u => u.Id == Id, modifiedProNames);
            idal.IWhMenuDAL.SaveChanges();
            return 1;
        }

        //菜单名称下拉列表
        public IEnumerable<WhMenuResult> MenuNameSelect(int CompanyId)
        {
            List<WhMenuResult> sql = (from a in idal.IWhMenuDAL.SelectAll()
                                      where a.CompanyId == CompanyId
                                      select new WhMenuResult
                                      {
                                          Id = a.Id,
                                          MenuNameCN = a.MenuNameCN,
                                          ParentMenuId = a.ParentMenuId,
                                          MenuSort = a.MenuSort
                                      }).OrderBy(u => u.ParentMenuId).ThenBy(u => u.MenuSort).ToList();
            return sql.AsEnumerable();
        }

        //菜单信息修改
        //对应MenuController中的 WhMenuEdit 方法
        public int WhMenuEdit(WhMenu whMenu, params string[] modifiedProNames)
        {
            if (WhMenuCheck(whMenu) > 0)
            {
                idal.IWhMenuDAL.UpdateBy(whMenu, u => u.Id == whMenu.Id, modifiedProNames);
                idal.IWhMenuDAL.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region 5.登录权限

        public List<WhPositionPowerMVCResult> WhPowerMVCList(WhPositionPowerMVCSearch entity)
        {
            int CompanyId = idal.IWhUserDAL.SelectBy(u => u.UserName == entity.UserName).ToList().First().CompanyId;

            if (CompanyId == 0)
            {
                var sql = from a in idal.IWhUserDAL.SelectAll()
                          where a.UserName == entity.UserName
                          join b in idal.IWhUserPositionDAL.SelectAll()
                          on a.Id equals b.UserId
                          join c in idal.IWhPositionDAL.SelectAll()
                          on b.PositionId equals c.Id
                          join d in idal.IWhPositionPowerDAL.SelectAll()
                          on c.Id equals d.PositionId
                          join e in idal.IWhPowerDAL.SelectAll()
                          on d.PowerId equals e.Id
                          join f in idal.IWhPositionPowerMVCDAL.SelectAll()
                          on 1 equals 1
                          select new WhPositionPowerMVCResult
                          {
                              Id = f.Id,
                              CompanyId = f.CompanyId,
                              ParentId = f.ParentId,
                              AreaName = f.AreaName,
                              ControllerName = f.ControllerName,
                              ActionName = f.ActionName,
                              PowerId = f.PowerId,
                              PowerName = f.PowerName,
                              Description = f.Description,
                              HttpMethod = f.HttpMethod
                          };
                //string sql1 = sql.ToString();
                return sql.ToList();
            }
            else
            {
                var sql = from a in idal.IWhUserDAL.SelectAll()
                          where a.UserName == entity.UserName
                          join b in idal.IWhUserPositionDAL.SelectAll()
                          on a.Id equals b.UserId
                          join c in idal.IWhPositionDAL.SelectAll()
                          on b.PositionId equals c.Id
                          join d in idal.IWhPositionPowerDAL.SelectAll()
                          on c.Id equals d.PositionId
                          join e in idal.IWhPowerDAL.SelectAll()
                          on d.PowerId equals e.Id
                          join f in idal.IWhPositionPowerMVCDAL.SelectAll()
                          on e.Id equals f.PowerId
                          select new WhPositionPowerMVCResult
                          {
                              Id = f.Id,
                              CompanyId = f.CompanyId,
                              ParentId = f.ParentId,
                              AreaName = f.AreaName,
                              ControllerName = f.ControllerName,
                              ActionName = f.ActionName,
                              PowerId = f.PowerId,
                              PowerName = f.PowerName,
                              Description = f.Description,
                              HttpMethod = f.HttpMethod
                          };
                string sql1 = sql.ToString();
                return sql.ToList();
            }
        }

        public List<WhMenuResult> WhUserMenuGet(WhUser whUser)
        {
            var sql = from a in idal.IWhUserDAL.SelectAll()
                      where a.UserName == whUser.UserName
                      join b in idal.IWhUserPositionDAL.SelectAll()
                      on a.Id equals b.UserId
                      join c in idal.IWhPositionPowerDAL.SelectAll()
                      on b.PositionId equals c.PositionId
                      join d in idal.IWhMenuDAL.SelectAll()
                      on new { A = whUser.CompanyId } equals new { A = d.CompanyId }
                      where c.PowerName == d.PowerName || d.PowerName == null
                      || b.CompanyId == 0
                      group new { d } by new { d.Id, d.CompanyId, d.MenuName, d.MenuNameCN, d.MenuUrl, d.MenuIcon, d.MenuSort, d.PowerName, d.ParentMenuId } into tempa
                      select new WhMenuResult
                      {
                          Id = tempa.Key.Id,
                          CompanyId = tempa.Key.CompanyId,
                          MenuName = tempa.Key.MenuName,
                          MenuNameCN = tempa.Key.MenuNameCN,
                          MenuUrl = tempa.Key.MenuUrl,
                          MenuIcon = tempa.Key.MenuIcon,
                          MenuSort = tempa.Key.MenuSort,
                          PowerName = tempa.Key.PowerName,
                          ParentMenuId = tempa.Key.ParentMenuId
                      };
            string sql1 = sql.ToString();
            sql = sql.OrderBy(u => u.Id);
            return sql.ToList();
        }

        #endregion


        #region 6.WinCE管理

        //WinCE 基础数据管理
        //对应 WinCEController 的 List  方法
        public List<BusinessObject> BusinessObjectList(BusinessObjectSearch searchEntity, out int total)
        {
            var sql = from a in idal.IBusinessObjectDAL.SelectAll()
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ObjectDes))
                sql = sql.Where(u => u.ObjectDes.Contains(searchEntity.ObjectDes));
            if (!string.IsNullOrEmpty(searchEntity.ObjectName))
                sql = sql.Where(u => u.ObjectName.Contains(searchEntity.ObjectName));
            if (!string.IsNullOrEmpty(searchEntity.ObjectType))
                sql = sql.Where(u => u.ObjectType == searchEntity.ObjectType);

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //业务中文名下拉列表
        public IEnumerable<BusinessObject> BusObjectDesSelect()
        {
            var sql = from a in idal.IBusinessObjectDAL.SelectAll()
                      select a;
            sql = sql.Distinct();
            return sql.AsEnumerable();
        }

        //业务类型下拉列表
        public IEnumerable<BusinessObjectResult> BusObjectTypeSelect()
        {
            var sql = from a in idal.IBusinessObjectDAL.SelectAll()
                      select new BusinessObjectResult
                      {
                          ObjectType = a.ObjectType
                      };
            sql = sql.Distinct();
            return sql.AsEnumerable();
        }

        //新增WinCE业务对象
        //对应 WinCEController中的 AddBusObject 方法
        public BusinessObject AddBusObject(BusinessObject entity)
        {
            if (idal.IBusinessObjectDAL.SelectBy(u => u.ObjectDes == entity.ObjectDes).Count == 0)
            {
                idal.IBusinessObjectDAL.Add(entity);
                idal.IBusinessObjectDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //新增WinCE业务对象
        //对应 WinCEController中的 AddBusObject 方法
        public BusinessObjectItem AddBusObjectItem(BusinessObjectItem entity)
        {
            idal.IBusinessObjectItemDAL.Add(entity);
            idal.IBusinessObjectItemDAL.SaveChanges();
            return entity;
        }


        //WinCE 业务对象明细查询
        //对应 WinCEController 的 ObjectItemList  方法
        public List<BusinessObjectItem> BusinessObjectItemList(BusinessObjectItemSearch searchEntity, out int total)
        {
            var sql = from a in idal.IBusinessObjectItemDAL.SelectAll()
                      select a;

            if (searchEntity.ObjectId != 0)
                sql = sql.Where(u => u.ObjectId == searchEntity.ObjectId);

            total = sql.Count();
            sql = sql.OrderBy(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //删除业务对象
        //对应 WinCEController 的 BusObjectDel  方法
        public int BusObjectDel(int id)
        {
            idal.IBusinessObjectDAL.DeleteBy(u => u.Id == id);
            idal.IBusinessObjectItemDAL.DeleteBy(u => u.ObjectId == id);
            return idal.IBusinessObjectDAL.SaveChanges();
        }

        #endregion


        #region 7.RF流程管理

        //流程规则查询
        //对应 FlowRuleController 的 List  方法
        public List<RFFlowRuleResult> RFFlowRuleList(RFFlowRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.IRFFlowRuleDAL.SelectAll()
                      select new RFFlowRuleResult
                      {
                          Id = a.Id,
                          FunctionId = a.FunctionId,
                          FunctionName = a.FunctionName,
                          Description = a.Description,
                          GroupId = a.GroupId,
                          FunctionFlag = a.FunctionFlag,
                          RequiredFlag = a.RequiredFlag,
                          RelyId = a.RelyId,
                          BusinessObjectHeadId = a.BusinessObjectHeadId,
                          SelectRuleDescription = a.SelectRuleDescription
                      };

            if (!string.IsNullOrEmpty(searchEntity.FunctionName))
                sql = sql.Where(u => u.FunctionName.Contains(searchEntity.FunctionName));

            total = sql.Count();
            sql = sql.OrderBy(u => u.FunctionId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增流程规则对象
        //对应 FlowRuleController中的 AddFlorRule 方法
        public RFFlowRule AddRFFlorRule(RFFlowRule entity)
        {
            if (idal.IRFFlowRuleDAL.SelectBy(u => u.FunctionName == entity.FunctionName).Count == 0)
            {
                idal.IRFFlowRuleDAL.Add(entity);
                idal.IRFFlowRuleDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //修改流程规则对象
        //对应 FlowRuleController中的 EditFlorRule 方法
        public int EditRFFlowRule(RFFlowRule entity)
        {
            idal.IRFFlowRuleDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "FunctionId", "FunctionName", "Description", "GroupId", "FunctionFlag", "RequiredFlag", "RelyId", "BusinessObjectHeadId", "SelectRuleDescription" });
            idal.IRFFlowRuleDAL.SaveChanges();
            return 1;
        }

        #endregion


        #region 8.出货流程管理

        //流程规则查询
        //对应 FlowRuleController 的 List  方法
        public List<FlowRuleResult> FlowRuleList(FlowRuleSearch searchEntity, out int total)
        {
            var sql = from a in idal.IFlowRuleDAL.SelectAll()
                      join b in idal.IBusinessFlowGroupDAL.SelectAll()
                      on a.BusinessObjectGroupId equals b.Id into temp1
                      from ab in temp1.DefaultIfEmpty()
                      select new FlowRuleResult
                      {
                          Id = a.Id,
                          FunctionId = a.FunctionId,
                          FunctionName = a.FunctionName,
                          StatusName = a.StatusName,
                          Description = a.Description,
                          GroupId = a.GroupId,
                          FunctionFlag = a.FunctionFlag,
                          RequiredFlag = a.RequiredFlag,
                          RelyId = a.RelyId,
                          BusinessObjectGroupId = a.BusinessObjectGroupId,
                          SelectRuleDescription = a.SelectRuleDescription,
                          RollbackFlag = a.RollbackFlag,
                          RollbackFlagShow = a.RollbackFlag == 0 ? "否" : "是",
                          FlowName = ab.FlowName
                      };

            if (!string.IsNullOrEmpty(searchEntity.FunctionName))
                sql = sql.Where(u => u.FunctionName.Contains(searchEntity.FunctionName));

            total = sql.Count();
            sql = sql.OrderBy(u => u.FunctionId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增流程规则对象
        //对应 FlowRuleController中的 AddFlorRule 方法
        public FlowRule AddFlowRule(FlowRule entity)
        {
            if (idal.IFlowRuleDAL.SelectBy(u => u.FunctionName == entity.FunctionName).Count == 0)
            {
                idal.IFlowRuleDAL.Add(entity);
                idal.IFlowRuleDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //修改流程规则对象
        //对应 FlowRuleController中的 EditFlorRule 方法
        public int EditFlowRule(FlowRule entity)
        {
            idal.IFlowRuleDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "FunctionId", "FunctionName", "StatusName", "Description", "GroupId", "FunctionFlag", "RequiredFlag", "RelyId", "BusinessObjectGroupId", "SelectRuleDescription", "RollbackFlag" });
            idal.IFlowRuleDAL.SaveChanges();
            return 1;
        }

        //RF枪的出货流程下拉列表
        public IEnumerable<BusinessFlowGroupResult> FlowNameSelect(string whCode)
        {
            var sql = from a in idal.IBusinessFlowGroupDAL.SelectAll()
                      where a.WhCode == whCode && a.Type == "OutBound"
                      select new BusinessFlowGroupResult
                      {
                          Id = a.Id,
                          FlowName = a.FlowName
                      };
            return sql.AsEnumerable();
        }

        public string ApiVersion()
        {
            return idal.ILookUpDAL.SelectBy(u => u.TableName == "CeApiVersion" && u.ColumnName == "Version").Select(u => u.ColumnKey).First();
        }

        #endregion


        #region 9.仓库管理

        //列表
        public List<WhInfoResult1> WhInfoNameList(WhInfoSearch1 searchEntity, out int total)
        {
            var sql = from a in idal.IWhInfoDAL.SelectAll()
                      where a.CompanyId == searchEntity.CompanyId
                      select new WhInfoResult1
                      {
                          Id = a.Id,
                          WhCode = a.WhCode,
                          WhName = a.WhName,
                          NoHuIdFlag = a.NoHuIdFlag,
                          NoHuIdFlagShow = a.NoHuIdFlag == 1 ? "是" : "否"
                      };

            if (!string.IsNullOrEmpty(searchEntity.WhName))
                sql = sql.Where(u => u.WhName == searchEntity.WhName);

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //新增
        public WhInfo WhInfoAdd(WhInfo entity)
        {
            List<WhInfo> checkList = idal.IWhInfoDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CompanyId == entity.CompanyId);
            if (checkList.Count == 0)
            {
                entity.CreateDate = DateTime.Now;
                idal.IWhInfoDAL.Add(entity);
                idal.IWhInfoDAL.SaveChanges();
                return entity;
            }
            else
            {
                return null;
            }
        }

        //修改
        public int WhInfoEdit(WhInfo entity)
        {
            entity.UpdateDate = DateTime.Now;
            idal.IWhInfoDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "WhName", "NoHuIdFlag", "UpdateUser", "UpdateDate" });
            idal.IWhInfoDAL.SaveChanges();
            return 1;
        }

        #endregion

    }

}
