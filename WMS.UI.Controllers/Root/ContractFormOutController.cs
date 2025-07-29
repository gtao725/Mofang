using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ContractFormOutController : Controller
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
            WCF.RootService.ContractFormOutSearch entity = new WCF.RootService.ContractFormOutSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.ContractName = Request["txt_contractName"].Trim();
            entity.ChargeName = Request["txt_chargeName"].Trim();

            int total = 0;
            List<WCF.RootService.ContractFormOut> list = cf.ContractFormOutList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ContractName", "合同名称");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("ChargeName", "科目");
            fieldsName.Add("LadderNumberBegin", "天数阶梯数值起");
            fieldsName.Add("LadderNumberEnd", "天数阶梯数值止");

            fieldsName.Add("ChargeUnitName", "计费单位");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("SuitCase", "提箱点");
            fieldsName.Add("Port", "进港点");

            fieldsName.Add("Price", "单价");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,ContractName:110,ChargeName:130,default:90"));
        }

        //批量导入
        public ActionResult imports()
        {
            string[] contractName = Request.Form.GetValues("合同名称");
            string[] type = Request.Form.GetValues("类型");
            string[] chargeName = Request.Form.GetValues("科目");

            string[] ladderNumberBegin = Request.Form.GetValues("天数阶梯数值起");
            string[] ladderNumberEnd = Request.Form.GetValues("天数阶梯数值止");

            string[] chargeUnitName = Request.Form.GetValues("计费单位");
            string[] containerType = Request.Form.GetValues("箱型");

            string[] suitCase = Request.Form.GetValues("提箱点");
            string[] port = Request.Form.GetValues("进港点");

            string[] price = Request.Form.GetValues("单价");

            if (contractName == null || type == null || chargeName == null || ladderNumberBegin == null || ladderNumberEnd == null || chargeUnitName == null || containerType == null  || port == null || price == null)
            {
                return Helper.RedirectAjax("err", "导入数据列少于系统所需，请检查列名与列数！", null, "");
            }
            if (contractName.Count() > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            if (contractName.Count() != type.Count() || contractName.Count() != chargeName.Count() || contractName.Count() != price.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            List<WCF.RootService.ContractFormOut> list = new List<WCF.RootService.ContractFormOut>();

            //清除excel表中的数据
            string errorItemNumber = "", errorResult1 = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < contractName.Length; i++)
            {
                if (!data.ContainsValue(contractName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + chargeUnitName[i].ToString().Trim() + "-" + containerType[i].ToString().Trim() + "-" + suitCase[i].ToString().Trim() + "-" + port[i].ToString().Trim()))     //Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, contractName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + chargeUnitName[i].ToString().Trim() + "-" + containerType[i].ToString().Trim() + "-" + suitCase[i].ToString().Trim() + "-" + port[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + contractName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + chargeUnitName[i].ToString().Trim() + "-" + containerType[i].ToString().Trim() + "-" + suitCase[i].ToString().Trim() + "-" + port[i].ToString().Trim();
                }

                try
                {

                    WCF.RootService.ContractFormOut entity = new WCF.RootService.ContractFormOut();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ContractName = contractName[i].ToString().Trim();
                    entity.Type = type[i].ToString().Trim();
                    entity.ChargeName = chargeName[i].ToString().Trim();

                    entity.ChargeUnitName = chargeUnitName[i].ToString().Trim();
                    entity.ContainerType = containerType[i].ToString().Trim();

                    entity.SuitCase = suitCase[i].ToString().Trim();
                    entity.Port = port[i].ToString().Trim();

                    entity.CreateUser = Session["userName"].ToString();
                    entity.CreateDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(ladderNumberBegin[i].ToString().Trim()))
                    {
                        int s = Convert.ToInt32(ladderNumberBegin[i].ToString().Trim());
                        entity.LadderNumberBegin = s;
                    }
                    else
                    {
                        entity.LadderNumberBegin = 0;
                    }

                    if (!string.IsNullOrEmpty(ladderNumberEnd[i].ToString().Trim()))
                    {
                        int s = Convert.ToInt32(ladderNumberEnd[i].ToString().Trim());
                        entity.LadderNumberEnd = s;
                    }
                    else
                    {
                        entity.LadderNumberEnd = 99999;
                    }


                    if (!string.IsNullOrEmpty(price[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(price[i].ToString().Trim());
                        entity.Price = s;
                    }

                    list.Add(entity);

                }
                catch
                {
                    errorResult1 = "数据:" + contractName[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim();
                    break;
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }
            if (errorResult1 != "")
            {
                return Helper.RedirectAjax("err", "数字格式不正确！" + errorResult1, null, "");
            }

            string result = cf.ContractFormOutImports(list.ToArray());
            if (result == "Y")
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
        public ActionResult EditContractForm()
        {
            WCF.RootService.ContractFormOut entity = new WCF.RootService.ContractFormOut();
            entity.Id = Convert.ToInt32(Request["Id"]);

            string errorResult1 = "";
            try
            {
                if (!string.IsNullOrEmpty(Request["edit_ladderNumberBegin"].Trim()))
                {
                    int s = Convert.ToInt32(Request["edit_ladderNumberBegin"].Trim());
                    entity.LadderNumberBegin = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_ladderNumberEnd"].Trim()))
                {
                    int s = Convert.ToInt32(Request["edit_ladderNumberEnd"].Trim());
                    entity.LadderNumberEnd = s;
                }
 

                if (!string.IsNullOrEmpty(Request["edit_price"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_price"].Trim());
                    entity.Price = s;
                }
            }
            catch
            {
                errorResult1 = "数据:" + Request["edit_ladderNumberBegin"].Trim() + Request["edit_ladderNumberEnd"].Trim();
            }
            if (errorResult1 != "")
            {
                return Helper.RedirectAjax("err", "数字格式不正确！" + errorResult1, null, "");
            }

            string result = cf.ContractFormOutEdit(entity);
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
        public ActionResult ContractFormDeleteAll()
        {
            int result = cf.ContractFormOutDeleteAll(Request["ContractName"], Session["whCode"].ToString());
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
