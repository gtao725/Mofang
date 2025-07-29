using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ItemColorCodeController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };
            return View();
        }

        //款号列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhItemSearch entity = new WCF.RootService.WhItemSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.ClientCode = Request["WhClientId"];
            entity.ColorCode = Request["altItemNumber"].Trim();

            int total = 0;
            List<WCF.RootService.ItemMasterColorCode> list = cf.ItemMasterColorCodeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ColorCode", "款号Code");
            fieldsName.Add("ColorDescription", "款号描述");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,ColorDescription:250,default:140"));
        }

        //批量导入款号
        public ActionResult imports()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] altItemNumber = Request.Form.GetValues("款号Code");
            string[] colorDescription = Request.Form.GetValues("款号描述");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            if (clientCode.Count() != altItemNumber.Count() || altItemNumber.Count() != colorDescription.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + colorDescription[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + colorDescription[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + colorDescription[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            string result = cf.ItemMasterColorCodeImports(clientCode, altItemNumber, colorDescription, Session["whCode"].ToString(), Session["userName"].ToString());
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
        public ActionResult EditItemColorCode()
        {
            WCF.RootService.ItemMasterColorCode entity = new WCF.RootService.ItemMasterColorCode();
            entity.Id = Convert.ToInt32(Request["Id"]);

            entity.ColorDescription = Request["edit_description"].Trim();

            string result = cf.ItemColorCodeEdit(entity);
            if (result == "Y")
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
