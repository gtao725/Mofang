using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class UnitController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
       
            return View();
        }

        //托盘列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.UnitsSearch entity = new WCF.RootService.UnitsSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.AltItemNumber = Request["AltItemNumber"];
            

            int total = 0;
            List<WCF.RootService.UnitsResult> list = cf.UnitsList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("Proportion", "单位比例");
            fieldsName.Add("AltItemNumber", "客户款号");
            fieldsName.Add("ClientCode", "客户代码");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,UnitName:110,AltItemNumber:100,CreateDate:130,default:80"));
        }

        public ActionResult AddUnit()
        {
            WCF.RootService.Unit unit = new WCF.RootService.Unit();
            unit.ItemId = Convert.ToInt32( Request["txt_ItemId"]);
            unit.UnitName = Request["txt_unitname"];
            unit.Proportion = Convert.ToInt32(Request["txt_proportion"]);
            unit.WhCode = Session["whCode"].ToString();
            unit.CreateUser = Session["userName"].ToString();
            unit.CreateDate = DateTime.Now;

            int result = cf.AddUnit(unit);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else {
                return Helper.RedirectAjax("err", "添加失败,请检查数据或已存在该单位,！", null, "");
            }
        }



        //新增单位


    }
}
