using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ContractFormController : Controller
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
            WCF.RootService.ContractFormSearch entity = new WCF.RootService.ContractFormSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.ContractName = Request["txt_contractName"].Trim();
            entity.Type = Request["sel_type"].Trim();
            entity.ChargeName = Request["txt_chargeName"].Trim();

            int total = 0;
            List<WCF.RootService.ContractForm> list = cf.ContractFormList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ContractName", "合同名称");
            fieldsName.Add("WhCodeName", "仓库");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("ChargeName", "名称");
            fieldsName.Add("TransportType", "车型");
            fieldsName.Add("Shift", "白晚班");
            fieldsName.Add("LadderNumberBegin", "重量阶梯数值起");
            fieldsName.Add("LadderNumberEnd", "重量阶梯数值止");
            fieldsName.Add("LadderNumberBeginCBM", "立方阶梯数值起");
            fieldsName.Add("LadderNumberEndCBM", "立方阶梯数值止");
            fieldsName.Add("MonthlyFlag", "是否月结");
            fieldsName.Add("Description", "描述");
            fieldsName.Add("ChargeUnitName", "计费单位");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("Ratio", "系数");
            fieldsName.Add("MaxPriceTotal", "最大金额");
            fieldsName.Add("GroupId", "择大组号");
            fieldsName.Add("ActiveDateBegin", "有效时间起");
            fieldsName.Add("ActiveDateEnd", "有效时间止");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,Shift:70,TransportType:80,Description:180,CreateDate:130,default:90"));
        }

        //批量导入
        public ActionResult imports()
        {
            string[] contractName = Request.Form.GetValues("合同名称");
            string[] whCodeName = Request.Form.GetValues("仓库");
            string[] type = Request.Form.GetValues("类型");
            string[] chargeName = Request.Form.GetValues("名称");
            string[] transportType = Request.Form.GetValues("车型");
            string[] shift = Request.Form.GetValues("白晚班");
            string[] ladderNumberBegin = Request.Form.GetValues("重量阶梯数值起");
            string[] ladderNumberEnd = Request.Form.GetValues("重量阶梯数值止");

            string[] ladderNumberBeginCBM = Request.Form.GetValues("立方阶梯数值起");
            string[] ladderNumberEndCBM = Request.Form.GetValues("立方阶梯数值止");

            string[] monthlyFlag = Request.Form.GetValues("是否月结");
            string[] description = Request.Form.GetValues("描述");
            string[] chargeUnitName = Request.Form.GetValues("计费单位");
            string[] price = Request.Form.GetValues("单价");
            string[] ratio = Request.Form.GetValues("系数");

            string[] maxPriceTotal = Request.Form.GetValues("最大金额");
            string[] groupId = Request.Form.GetValues("择大组号");

            string[] activeDateBegin = Request.Form.GetValues("有效时间起");
            string[] activeDateEnd = Request.Form.GetValues("有效时间止");


            if (contractName == null || whCodeName == null || type == null || chargeName == null || transportType == null || shift == null || ladderNumberBegin == null || ladderNumberEnd == null || monthlyFlag == null || description == null || chargeUnitName == null || price == null || ratio == null || maxPriceTotal == null || groupId == null || activeDateBegin == null || activeDateEnd == null || ladderNumberBeginCBM == null || ladderNumberEndCBM == null)
            {
                return Helper.RedirectAjax("err", "导入数据列少于系统所需，请检查列名与列数！", null, "");
            }
            if (contractName.Count() > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            if (contractName.Count() != whCodeName.Count() || contractName.Count() != type.Count() || contractName.Count() != chargeName.Count() || contractName.Count() != transportType.Count() || contractName.Count() != shift.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            List<WCF.RootService.ContractForm> list = new List<WCF.RootService.ContractForm>();

            //清除excel表中的数据
            string errorItemNumber = "", errorResult1 = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < contractName.Length; i++)
            {
                if (!data.ContainsValue(contractName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + transportType[i].ToString().Trim() + "-" + shift[i].ToString().Trim() + "-" + whCodeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + ladderNumberBeginCBM[i].ToString().Trim() + "-" + ladderNumberEndCBM[i].ToString().Trim() + "-" + activeDateBegin[i].ToString().Trim() + "-" + activeDateEnd[i].ToString().Trim()))     //Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, contractName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + transportType[i].ToString().Trim() + "-" + shift[i].ToString().Trim() + "-" + whCodeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + ladderNumberBeginCBM[i].ToString().Trim() + "-" + ladderNumberEndCBM[i].ToString().Trim() + "-" + activeDateBegin[i].ToString().Trim() + "-" + activeDateEnd[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + contractName[i].ToString().Trim() + "-" + whCodeName[i].ToString().Trim() + "-" + type[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + transportType[i].ToString().Trim() + "-" + shift[i].ToString().Trim() + "-" + whCodeName[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + ladderNumberBeginCBM[i].ToString().Trim() + "-" + ladderNumberEndCBM[i].ToString().Trim() + "-" + activeDateBegin[i].ToString().Trim() + "-" + activeDateEnd[i].ToString().Trim();
                }

                try
                {

                    WCF.RootService.ContractForm entity = new WCF.RootService.ContractForm();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ContractName = contractName[i].ToString().Trim();
                    entity.WhCodeName = whCodeName[i].ToString().Trim();
                    entity.Type = type[i].ToString().Trim();
                    entity.ChargeName = chargeName[i].ToString().Trim();
                    entity.TransportType = transportType[i].ToString().Trim();
                    entity.Shift = shift[i].ToString().Trim();
                    entity.MonthlyFlag = monthlyFlag[i].ToString().Trim();
                    entity.Description = description[i].ToString().Trim();
                    entity.ChargeUnitName = chargeUnitName[i].ToString().Trim();

                    entity.GroupId = groupId[i].ToString().Trim();
                    entity.CreateUser = Session["userName"].ToString();
                    entity.CreateDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(ladderNumberBegin[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(ladderNumberBegin[i].ToString().Trim());
                        entity.LadderNumberBegin = s;
                    }
                    else
                    {
                        entity.LadderNumberBegin = 0;
                    }

                    if (!string.IsNullOrEmpty(ladderNumberEnd[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(ladderNumberEnd[i].ToString().Trim());
                        entity.LadderNumberEnd = s;
                    }
                    else
                    {
                        entity.LadderNumberEnd = 99999;
                    }

                    if (!string.IsNullOrEmpty(ladderNumberBeginCBM[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(ladderNumberBeginCBM[i].ToString().Trim());
                        entity.LadderNumberBeginCBM = s;
                    }
                    else
                    {
                        entity.LadderNumberBeginCBM = 0;
                    }

                    if (!string.IsNullOrEmpty(ladderNumberEndCBM[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(ladderNumberEndCBM[i].ToString().Trim());
                        entity.LadderNumberEndCBM = s;
                    }
                    else
                    {
                        entity.LadderNumberEndCBM = 99999;
                    }

                    if (!string.IsNullOrEmpty(activeDateBegin[i].ToString().Trim()))
                    {
                        try
                        {
                            entity.ActiveDateBegin = Convert.ToDateTime(activeDateBegin[i].ToString().Trim());
                        }
                        catch
                        {
                            return Helper.RedirectAjax("err", "有效时间起格式不正确！", null, "");
                        }
                    }
                    else
                    {
                        entity.ActiveDateBegin = DateTime.Now;
                    }

                    if (!string.IsNullOrEmpty(activeDateEnd[i].ToString().Trim()))
                    {
                        try
                        {
                            entity.ActiveDateEnd = Convert.ToDateTime(activeDateEnd[i].ToString().Trim());
                        }
                        catch
                        {
                            return Helper.RedirectAjax("err", "有效时间止格式不正确！", null, "");
                        }

                    }
                    else
                    {
                        entity.ActiveDateEnd = Convert.ToDateTime("2099-01-01");
                    }

                    if (!string.IsNullOrEmpty(price[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(price[i].ToString().Trim());
                        entity.Price = s;
                    }

                    if (!string.IsNullOrEmpty(ratio[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(ratio[i].ToString().Trim());
                        entity.Ratio = s;
                    }
                    else
                    {
                        entity.Ratio = 1;
                    }

                    if (!string.IsNullOrEmpty(maxPriceTotal[i].ToString().Trim()))
                    {
                        decimal s = Convert.ToDecimal(maxPriceTotal[i].ToString().Trim());
                        entity.MaxPriceTotal = s;
                    }
                    else
                    {
                        entity.MaxPriceTotal = 0;
                    }

                    list.Add(entity);

                }
                catch
                {
                    errorResult1 = "数据:" + contractName[i].ToString().Trim() + "-" + chargeName[i].ToString().Trim() + "-" + shift[i].ToString().Trim() + "-" + ladderNumberBegin[i].ToString().Trim() + "-" + ladderNumberEnd[i].ToString().Trim() + "-" + ladderNumberBeginCBM[i].ToString().Trim() + "-" + ladderNumberEndCBM[i].ToString().Trim();
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

            string result = cf.ContractFormImports(list.ToArray());
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
            WCF.RootService.ContractForm entity = new WCF.RootService.ContractForm();
            entity.Id = Convert.ToInt32(Request["Id"]);

            string errorResult1 = "";
            try
            {
                if (!string.IsNullOrEmpty(Request["edit_ladderNumberBegin"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_ladderNumberBegin"].Trim());
                    entity.LadderNumberBegin = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_ladderNumberEnd"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_ladderNumberEnd"].Trim());
                    entity.LadderNumberEnd = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_ladderNumberBeginCBM"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_ladderNumberBeginCBM"].Trim());
                    entity.LadderNumberBeginCBM = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_ladderNumberEndCBM"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_ladderNumberEndCBM"].Trim());
                    entity.LadderNumberEndCBM = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_price"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_price"].Trim());
                    entity.Price = s;
                }

                if (!string.IsNullOrEmpty(Request["edit_radio"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_radio"].Trim());
                    entity.Ratio = s;
                }
                else
                {
                    entity.Ratio = 1;
                }

                if (!string.IsNullOrEmpty(Request["edit_maxPrice"].Trim()))
                {
                    decimal s = Convert.ToDecimal(Request["edit_maxPrice"].Trim());
                    entity.MaxPriceTotal = s;
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

            try
            {
                if (!string.IsNullOrEmpty(Request["edit_ActiveDateBegin"].Trim()))
                {
                    entity.ActiveDateBegin = Convert.ToDateTime(Request["edit_ActiveDateBegin"].Trim()); ;
                }

                if (!string.IsNullOrEmpty(Request["edit_ActiveDateEnd"].Trim()))
                {
                    entity.ActiveDateEnd = Convert.ToDateTime(Request["edit_ActiveDateEnd"].Trim()); ;
                }

            }
            catch
            {
                return Helper.RedirectAjax("err", "有效日期格式不正确！", null, "");
            }

            entity.CreateUser = Session["userName"].ToString();

            string result = cf.ContractFormEdit(entity);
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
        public ActionResult ContractFormDel()
        {
            int result = cf.ContractFormDel(Convert.ToInt32(Request["Id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult ContractFormDeleteAll()
        {
            int result = cf.ContractFormDeleteAll(Request["ContractName"], Session["whCode"].ToString(), Session["userName"].ToString());
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
