using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_HoldReasonController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        //仓库异常原因列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.HoldMasterSearch entity = new WCF.RootService.HoldMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            int total = 0;
            List<WCF.RootService.HoldMaster> list = cf.WarehouseHoldMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReasonType", "异常类型");
            fieldsName.Add("HoldReason", "仓库异常原因");
            fieldsName.Add("Sequence", "显示顺序");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,HoldReason:150,Sequence:120,default:80"));
        }


        //新增仓库异常原因
        [HttpPost]
        public ActionResult HoldMasterAdd()
        {
            WCF.RootService.HoldMaster entity = new WCF.RootService.HoldMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = 0;
            entity.ClientCode = "all";
            entity.HoldReason = Request["txt_holdReason"].Trim();
            entity.ReasonType = Request["txt_holdReasonType"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            WCF.RootService.HoldMaster result = cf.HoldMasterAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }


        //仓库异常原因修改
        [HttpGet]
        public ActionResult HoldMasterEdit()
        {
            WCF.RootService.HoldMaster entity = new WCF.RootService.HoldMaster();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.HoldReason = Request["holdReason"].Trim();
            if (Request["sequence"]=="")
            {
                entity.Sequence = 0;
            }
            else
            {
                entity.Sequence = Convert.ToInt32(Request["sequence"]);
            }
            
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.HoldMasterEdit(entity, new string[] { "HoldReason", "Sequence", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //删除仓库异常原因
        [HttpGet]
        public ActionResult HoldMasterDelById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.HoldMasterDelById(id);
            if (result > 0)
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
