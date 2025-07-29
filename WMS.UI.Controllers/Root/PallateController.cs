using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class PallateController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["PallateType"] = from r in cf.PallateTypeSelect()
                                      select new SelectListItem()
                                      {
                                          Text = r.Description,     //text
                                          Value = r.Id.ToString()
                                      };
            return View();
        }

        //托盘列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhPallateSearch entity = new WCF.RootService.WhPallateSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.HuId = Request["pallate_name"].Trim();
            entity.TypeId = Request["SelPallateType"];
            int total = 0;
            List<WCF.RootService.WhPallateResult> list = cf.WhPallateList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("HuId", "托盘号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("TypeId", "托盘类型ID");
            fieldsName.Add("TypeName", "托盘类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,UserName:110,UserNameCN:110,PassWord:100,CreateDate:130,default:80"));
        }

        //新增托盘
        [HttpGet]
        public ActionResult AddPallate()
        {
            List<WCF.RootService.Pallate> list = new List<WCF.RootService.Pallate>();
            //string hu_gs = "PLTA000001";
            //int hu_length = 6;
            int hu_length = Convert.ToInt32(Request["txt_pallateqtylength"].Trim());
            string hu = Request["txt_pallate"].Trim();
            int huStar = Convert.ToInt32(Request["txt_beginpallate"].Trim());
            int huEnd = Convert.ToInt32(Request["txt_endpallate"].Trim());

            if (huEnd - huStar > 1000)
            {
                return Helper.RedirectAjax("err", "新增托盘数不能超过1000条！", null, "");
            }

            for (int i = huStar; i <= huEnd; i++)
            {
                WCF.RootService.Pallate entity = new WCF.RootService.Pallate();
                entity.WhCode = Session["whCode"].ToString();
                entity.HuId = hu + i.ToString().PadLeft(hu_length, '0');
                if (Request["txt_type"] == "")
                {
                    entity.TypeId = 0;
                }
                else
                {
                    entity.TypeId = Convert.ToInt32(Request["txt_type"]);
                }
                entity.CreateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            int result = cf.WhPallateListAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        [HttpPost]
        public ActionResult BatchDelPallate()
        {
            string[] idarr = Request.Form.GetValues("idarr");

            List<int?> list = new List<int?>();
            foreach (var item in idarr)
            {
                list.Add(Convert.ToInt32(item));
            }
            string result = cf.PallateBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        public ActionResult imports()
        {
            string[] pallate = Request.Form.GetValues("托盘");

            Hashtable hash = new Hashtable();
            string mess = "";

            if (pallate == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (pallate.Length > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            List<WCF.RootService.WhPallateResult> list = new List<WCF.RootService.WhPallateResult>();
            for (int i = 0; i < pallate.Length; i++)
            {
                if (!hash.ContainsValue(pallate[i].ToString()))
                {
                    hash.Add(i, pallate[i].ToString());
                    WCF.RootService.WhPallateResult entity = new WCF.RootService.WhPallateResult();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.HuId = pallate[i].ToString().Trim();

                    entity.CreateUser = Session["userName"].ToString();

                    list.Add(entity);
                }
                else
                {
                    mess += "托盘重复：" + pallate[i].ToString() + "<br/>";
                }
            }

            if (mess != "")
            {
                return Helper.RedirectAjax("err", "导入失败！<br/>" + mess, null, "");
            }

            string result = cf.PallateImports(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

    }
}
