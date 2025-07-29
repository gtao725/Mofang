using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class AdminController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();

        [DefaultRequestAttribute]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoginCheck(WCF.AdminService.WhUser whUser, string WhCode)
        {

            //string url = "http://localhost:52375/api/test?ReceiptId=123";
            //string JSONData = "{Id:123,ReceiptId:'EI123',ItemNumber:'ITEM123',RecModeldetail:[{title:'这是一个标题',body:'what'},{title:'这是一个标题1',body:'what1'}]}";
            ////string JSONData = "{ReceiptId:'EI23121'}";
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            //request.Method = "Post";
            //request.ContentType = "application/json";
            //SetWebRequest(request);
            //byte[] data = Encoding.UTF8.GetBytes(JSONData);
            //WriteRequestData(request, data);
            //return ReadResponse(request.GetResponse());


            bool result = Helper.LoginUser(whUser, 0, WhCode);
            if (result)
            {
                string aaa = Session["whCompany"].ToString();
                var sqlWhUser = cf.LoginIn(whUser);

                var sql = from r in cf.WhInfoList(Convert.ToInt32(Session["whCompany"].ToString()), Session["userName"].ToString())
                          select new Model.EipSelectItems()
                          {
                              key = r.WhName,     //text
                              val = r.WhCode
                          };



                if (!string.IsNullOrEmpty(WhCode))
                {
                    sql = sql.Where(u => u.val == WhCode);
                }

                int counts = sql.Count();
                if (counts > 1)
                {
                    //多个的时候不跳转等设置好以后再跳转
                    return Helper.RedirectAjax("ok", null, sql.ToList(), null);
                }
                else
                {
                    //1个的时候直接设置whCode
                    Helper.setWhCode(sql.ToList()[0].val, 1);
                    DateTime? updateDate = sqlWhUser.UpdateDate;

                    //CheckFlag:0关闭密码检测，1开启密码检测
                    if ((sqlWhUser.CheckFlag ?? 0) == 0)
                    {
                        return Helper.RedirectAjax("ok", null, null, "/Admin/Common/Index");
                    }
                    else
                    {
                        if (updateDate == null || updateDate <= DateTime.Now.AddDays(-30))
                            return Helper.RedirectAjax("ok", null, null, "/Admin/Common/Index?ifPWUnActive=Y");
                        else
                            return Helper.RedirectAjax("ok", null, null, "/Admin/Common/Index");
                    }
                }

            }
            else
            {
                return Helper.RedirectAjax("err", "用户名或密码错误！", null, "");
            }

        }

        public ActionResult UserUpdatePwd()
        {
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();
            whUser.UserName = Session["userName"].ToString();
            whUser.PassWord = Request["txt_pwd"];

            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(whUser.PassWord));
            whUser.PassWord = BitConverter.ToString(data);

            WCF.AdminService.WhUser result = cf.LoginIn(whUser);

            if (result != null)
            {
                WCF.AdminService.WhUser whUserUpdate = new WCF.AdminService.WhUser();
                whUserUpdate.Id = Convert.ToInt32(Session["userId"]);
                whUserUpdate.PassWord = Request["txt_new_pwd"];

                int count = cf.UserUpdatePwd(whUserUpdate);
                if (count > 0)
                {
                    return Helper.RedirectAjax("ok", "密码修改成功！", null, "");
                }
                else
                {
                    return Helper.RedirectAjax("err", "对不起，密码修改失败！", null, "");
                }
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，旧密码错误，修改失败！", null, "");
            }
        }

        public ActionResult ExitSys()
        {
            Session["userId"] = null;
            Session["userName"] = null;
            Session["whCode"] = null;
            Session["userNameCN"] = null;
            Session["userPermission"] = null;
            Session["userMenu"] = null;
            Session["passWord"] = null;
            return Helper.RedirectAjax("ok", "", null, "/Admin/Admin/Index");
        }

    }
}
