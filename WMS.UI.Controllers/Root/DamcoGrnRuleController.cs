using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class DamcoGrnRuleController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        WCF.InBoundService.InBoundServiceClient cf1= new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();

        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };
            ViewData["WhClientCodeList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };

            return View();
        }

        //查询列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.DamcoGrnRuleSearch entity = new WCF.RootService.DamcoGrnRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["WhClientCode"];
            entity.MailTo = Request["MailTo"];

            int total = 0;
            List<WCF.RootService.DamcoGrnRule> list = cf.DamcoGrnRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("AutoSend", "是否自动发送");
            fieldsName.Add("DifferenceRate", "差异率");
            fieldsName.Add("TotalCheck", "是否验证总数");
            fieldsName.Add("DifferentialInterceptionFlag", "是否拦截差异发送");
            fieldsName.Add("DifferentialMailFlag", "是否发送差异Mail");
            fieldsName.Add("ReceiptDateSource", "收货时间");
            fieldsName.Add("CBMSource", "Cbm来源");
            fieldsName.Add("KgsSource", "Kgs来源");
            fieldsName.Add("MailTo", "收件人");           

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,CreateUser:70,MailTo:180,UpdateUser:70,default:130"));
        }

        //新增
        [HttpGet]
        public ActionResult AddDamcoGrnRule()
        {
            WCF.RootService.DamcoGrnRule entity = new WCF.RootService.DamcoGrnRule();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_WhClientCode"].Trim();
            entity.AutoSend = Convert.ToInt32(Request["txt_AutoSend"].Trim());
            entity.DifferenceRate = Convert.ToInt32(Request["txt_DifferenceRate"].Trim());
            entity.TotalCheck = Convert.ToInt32(Request["txt_TotalCheck"].Trim());
            entity.DifferentialInterceptionFlag = Convert.ToInt32(Request["txt_DifferentialInterceptionFlag"].Trim());
            entity.DifferentialMailFlag = Convert.ToInt32(Request["txt_DifferentialMailFlag"].Trim());
            entity.MailTo = Request["txt_MailTo"].Trim();
            entity.CBMSource = Request["txt_CbmSource"].Trim();
            entity.KgsSource = Request["txt_KgsSource"].Trim();

            entity.CreateUser= Session["userName"].ToString();

            string result = cf.DamcoGrnRuleAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改
        [HttpGet]
         public ActionResult DamcoGrnRuleEdit()
        {
      
            WCF.RootService.DamcoGrnRule entity = new WCF.RootService.DamcoGrnRule();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.AutoSend = Convert.ToInt32(Request["edit_AutoSend"].Trim());
            entity.DifferenceRate = Convert.ToInt32(Request["edit_DifferenceRate"].Trim());
            entity.TotalCheck = Convert.ToInt32(Request["edit_TotalCheck"].Trim());
            entity.MailTo = Request["edit_MailTo"].Trim();

            entity.DifferentialInterceptionFlag = Convert.ToInt32(Request["edit_DifferentialInterceptionFlag"].Trim());
            entity.DifferentialMailFlag = Convert.ToInt32(Request["edit_DifferentialMailFlag"].Trim());

            entity.CBMSource = Request["edit_CBMSource"].Trim();
            entity.KgsSource = Request["edit_KgsSource"].Trim();

            entity.UpdateUser= Session["userName"].ToString();

            string result = cf.DamcoGrnRuleEdit(entity);
            if (result =="Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

   
        //删除
        [HttpGet]
        public ActionResult DamcoGrnRuledel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.DamcoGrnRuleDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，删除失败！", null, "");
            }
        }

    }
}
