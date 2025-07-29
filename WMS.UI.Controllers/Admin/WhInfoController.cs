using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class WhInfoController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        //查询
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.WhInfoSearch1 search = new WCF.AdminService.WhInfoSearch1();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.WhName = Request["whName"];

            int total = 0;
            List<WCF.AdminService.WhInfoResult1> list = cf.WhInfoNameList(search, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库Code");
            fieldsName.Add("WhName", "仓库名称");
            fieldsName.Add("NoHuIdFlag", "NoHuIdFlag");
            fieldsName.Add("NoHuIdFlagShow", "是否无托盘");

            return Content(EIP.EipListJson(list, total, fieldsName, "WhName:140,default:80"));
        }

        //新增
        [HttpGet]
        public ActionResult AddWhInfo()
        {
            WCF.AdminService.WhInfo entity = new WCF.AdminService.WhInfo();
            entity.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            entity.WhName = Request["txt_whName"];
            entity.WhCode = Request["txt_whCode"];
            entity.WhDescription = entity.WhName;
            entity.WhLevel = 0;
            entity.NoHuIdFlag = Convert.ToInt32(Request["txt_noHuIdFlag"].ToString());

            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            WCF.AdminService.WhInfo result = cf.WhInfoAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，仓库Code已存在！", null, "");
            }
        }

        //修改
        [HttpGet]
        public ActionResult WhInfoEdit()
        {
            string whName = Request["edit_whName"];
            string noHuIdFlag = Request["edit_noHuIdFlag"];

            WCF.AdminService.WhInfo whInfo = new WCF.AdminService.WhInfo();
            whInfo.Id = Convert.ToInt32(Request["Id"]);
            whInfo.WhName = whName;
            whInfo.NoHuIdFlag = Convert.ToInt32(noHuIdFlag);
            whInfo.UpdateUser = Session["userName"].ToString();

            int result = cf.WhInfoEdit(whInfo);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

    }
}
