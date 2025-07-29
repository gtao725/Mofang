using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class HolidayController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }

        //查询列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.HolidaySearch entity = new WCF.RootService.HolidaySearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.HolidayName = Request["txt_holiday"].Trim();


            int total = 0;
            List<WCF.RootService.Holiday> list = cf.HolidayList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("HolidayName", "节假日");
            fieldsName.Add("DayBegin", "日期");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:130"));
        }

        //批量导入款号
        public ActionResult imports()
        {
            string[] holiday = Request.Form.GetValues("节假日");
            string[] dayBegin = Request.Form.GetValues("日期");

            if (holiday == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (holiday.Count() > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            if (holiday.Count() != dayBegin.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = "", errorResult1 = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < holiday.Length; i++)
            {
                if (!data.ContainsValue(holiday[i].ToString().Trim() + "-" + dayBegin[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, holiday[i].ToString().Trim() + "-" + dayBegin[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + holiday[i].ToString().Trim() + "-" + dayBegin[i].ToString().Trim();
                }

                try
                {
                    DateTime s = Convert.ToDateTime(dayBegin[i].ToString().Trim());
                }
                catch
                {
                    errorResult1 = "数据:" + holiday[i].ToString().Trim() + "-" + dayBegin[i].ToString().Trim();
                    break;
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }
            if (errorResult1 != "")
            {
                return Helper.RedirectAjax("err", "日期格式不正确！" + errorResult1, null, "");
            }



            string result = cf.HolidayImports(holiday, dayBegin, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //修改信息
        [HttpGet]
        public ActionResult EditHoliday()
        {
            WCF.RootService.Holiday entity = new WCF.RootService.Holiday();
            entity.Id = Convert.ToInt32(Request["Id"]);

            entity.DayBegin = Request["edit_dayBegin"].Trim();

            string errorResult1 = "";
            try
            {
                DateTime s = Convert.ToDateTime(entity.DayBegin);
            }
            catch
            {
                errorResult1 = "数据:" + entity.DayBegin;
            }
            if (errorResult1 != "")
            {
                return Helper.RedirectAjax("err", "日期格式不正确！" + errorResult1, null, "");
            }

            string result = cf.HolidayEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult HolidayDel()
        {
            int result = cf.HolidayDel(Convert.ToInt32(Request["Id"]));
            if (result >0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


    }
}
