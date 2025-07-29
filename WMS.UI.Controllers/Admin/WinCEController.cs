using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class WinCEController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["ObjType"] = from r in cf.BusObjectTypeSelect()
                                  select new SelectListItem()
                                  {
                                      Text = r.ObjectType,     //text
                                      Value = r.ObjectType
                                  };
            ViewData["ObjDes1"] = from r in cf.BusObjectDesSelect()
                                  select new SelectListItem()
                                  {
                                      Text = r.ObjectDes,     //text
                                      Value = r.Id.ToString()
                                  };
            ViewData["ObjDes2"] = from r in cf.BusObjectDesSelect()
                                  where r.ObjectType == "obj"
                                  select new SelectListItem()
                                  {
                                      Text = r.ObjectDes,     //text
                                      Value = r.Id.ToString()
                                  };
            return View();
        }

        /// <summary>
        /// 新增业务对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddBusObject()
        {
            WCF.AdminService.BusinessObject entity = new WCF.AdminService.BusinessObject();
            entity.ObjectName = Request["txt_objectName"];
            entity.ObjectValue = Request["txt_objectValue"];
            entity.ObjectDes = Request["txt_objectDes"];
            entity.ObjectType = Request["txt_objectType"];

            WCF.AdminService.BusinessObject result = cf.AddBusObject(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，业务中文名已存在！", null, "");
            }
        }

        /// <summary>
        /// 新增业务对象明细
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddBusObjectItem()
        {
            WCF.AdminService.BusinessObjectItem entity = new WCF.AdminService.BusinessObjectItem();
            entity.ObjectId = Convert.ToInt32(Request["ObjDes1"]);
            entity.MustAttributeName = Request["buteName"];
            entity.MustAttributeNameCN = Request["buteNameCN"];
            if (Request["ObjDes2"] != "")
            {
                entity.ParaObjectId = Convert.ToInt32(Request["ObjDes2"]);
            }

            WCF.AdminService.BusinessObjectItem result = cf.AddBusObjectItem(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        /// <summary>
        /// 查询业务对象信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.BusinessObjectSearch entity = new WCF.AdminService.BusinessObjectSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.ObjectName = Request["objectName"];
            entity.ObjectDes = Request["objectDes"];
            entity.ObjectType = Request["ObjType"];

            int total = 0;
            List<WCF.AdminService.BusinessObject> list = cf.BusinessObjectList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ObjectName", "ObjectName");
            fieldsName.Add("ObjectValue", "ObjectValue");
            fieldsName.Add("ObjectDes", "ObjectDes");
            fieldsName.Add("ObjectType", "ObjectType");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:150"));
        }

        /// <summary>
        /// 查询WinCE 业务对象明细信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ObjectItemList()
        {
            WCF.AdminService.BusinessObjectItemSearch entity = new WCF.AdminService.BusinessObjectItemSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.ObjectId = Convert.ToInt32(Request["objId"]);

            int total = 0;
            List<WCF.AdminService.BusinessObjectItem> list = cf.BusinessObjectItemList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ObjectId", "ObjectId");
            fieldsName.Add("MustAttributeName", "字段Name");
            fieldsName.Add("MustAttributeNameCN", "字段中文名");
            fieldsName.Add("ParaObjectId", "下层业务对象");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:50,default:150"));
        }

        //删除业务对象
        [HttpGet]
        public ActionResult BusObjectDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.BusObjectDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，删除失败！", null, "");
            }
        }

        //删除业务对象明细
        [HttpGet]
        public ActionResult BusObjectItemDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.BusObjectItemDelById(id);
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
