using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class AccountController : ApiController
    {
        Helper Helper = new Helper();
        [HttpGet]
        public object LoginCe([FromUri] string userId, [FromUri] string passWord)
        {
            IAdminManager ia = new AdminManager();
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(passWord));
            WhUser whUser = ia.LoginIn(new WhUser { UserName = userId, PassWord = BitConverter.ToString(data) });
            if (whUser != null)
            {
                List<WhInfoResult> whInFoList = ia.WhInfoList(whUser.CompanyId, userId);
                return Helper.ResultData("Y", "登陆成功", new UserModel { WhInfo = whInFoList, CompanyId = whUser.CompanyId });
            }
            else
                return Helper.ResultData("N", "账号或密码错误", null, null);
        }
        [HttpGet]
        public object LoginCe([FromUri] string userId, [FromUri] string passWord, [FromUri] string deviceName)
        {
            IAdminManager ia = new AdminManager();
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(passWord));
            WhUser whUser = ia.LoginIn(new WhUser { UserName = userId, PassWord = BitConverter.ToString(data), CreateUser = deviceName });
            if (whUser != null)
            {
                List<WhInfoResult> whInFoList = ia.WhInfoList(whUser.CompanyId, userId);
                return Helper.ResultData("Y", "登陆成功", new UserModel { WhInfo = whInFoList, CompanyId = whUser.CompanyId });
            }
            else
                return Helper.ResultData("N", "账号或密码错误", null, null);
        }

        [HttpGet]
        public object WhUserCheck([FromUri] string userName, [FromUri] int companyId)
        {

            IAdminManager ia = new AdminManager();
            int resInt = ia.WhUserCheck(new WhUser { UserName = userName, CompanyId = companyId });
            if (resInt == 0)
            {
                return Helper.ResultData("Y", "登陆成功", new { });
            }
            else
                return Helper.ResultData("N", "无此工号!", null, null);
        }


        [HttpGet]
        public object WhUserInfoCheck([FromUri] string userName, [FromUri] int companyId)
        {

            IAdminManager ia = new AdminManager();
            WhUser user = ia.WhUserInfoCheck(new WhUser { UserName = userName, CompanyId = companyId });
            if (user != null)
            {
                return Helper.ResultData("Y", "登陆成功", new UserModel { UserName = userName, UserNameCN = user.UserNameCN });
            }
            else
                return Helper.ResultData("N", "无此工号!", null, null);
        }
    }
}