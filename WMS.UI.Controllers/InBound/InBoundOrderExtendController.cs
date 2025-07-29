using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class InBoundOrderExtendController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            return View();
        }

        //查询列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.InBoundService.InBoundOrderExntedSearch entity = new WCF.InBoundService.InBoundOrderExntedSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.ClientCode = Request["clientCode"].Trim();
            entity.SoNumber = Request["soNumber"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.CartonId = Request["cartonId"].Trim();

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }
            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.InBoundService.SerialNumberInOutExtend> list = cf.InBoundOrderExtendList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("AltItemNumber", "SKU");
            fieldsName.Add("CartonId", "SN");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CartonId:160,default:130"));
        }

        //验证客户流程
        [HttpPost]
        public ActionResult CheckImportsInBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");

            List<WCF.InBoundService.WhClient> list = new List<WCF.InBoundService.WhClient>();
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (list.Where(u => u.ClientCode == clientCode[i]).Count() == 0)
                {
                    WCF.InBoundService.WhClient client = new WCF.InBoundService.WhClient();
                    client.ClientCode = clientCode[i];
                    list.Add(client);
                }
            }

            List<WCF.InBoundService.FlowHeadResult> list1 = cf.CheckClientCodeRule(Session["whCode"].ToString(), list.ToArray()).ToList();

            if (list1.Count > 0)
            {
                var sql = from r in list1
                          select new
                          {
                              Value = r.Id.ToString(),
                              Text = r.FlowName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }

        }

        [HttpPost]
        public ActionResult ImportsInBoundOrder()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] soNumber = Request.Form.GetValues("SO");
            string[] altItemNumber = Request.Form.GetValues("SKU");
            string[] sn = Request.Form.GetValues("SN");
            string[] qty = Request.Form.GetValues("数量");
            string[] checksku = Request.Form.GetValues("是否验证款号");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != soNumber.Count() || soNumber.Count() != altItemNumber.Count() || altItemNumber.Count() != qty.Count() || checksku.Count() != qty.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位
            Hashtable data1 = new Hashtable();   //去excel重复的库位

            List<string> checkSNList = new List<string>();
            for (int i = 0; i < sn.Length; i++)
            {
                if (!string.IsNullOrEmpty(sn[i].ToString()))
                {
                    checkSNList.Add(sn[i].ToString());
                }
            }
            if (checkSNList.Count > 0 && checkSNList.Count != soNumber.Count())
            {
                return Helper.RedirectAjax("err", "如果导入SN，SN请全部填写或全部为空！", null, "");
            }

            //清除excel表中的数据
            string errorItemNumber = "", errorItemNumber1 = "", errorItemNumber2 = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + sn[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add 
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + sn[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + sn[i].ToString().Trim();
                }

                if (checkSNList.Count == 0)
                {
                    if (!data1.ContainsValue(clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add 
                    {
                        data1.Add(k, clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim());
                        k++;
                    }
                    else
                    {
                        errorItemNumber2 = "数据:" + clientCode[i].ToString().Trim() + "-" + soNumber[i].ToString().Trim();
                    }
                }


                if (checksku.Length > 0)
                {
                    if (checksku[i] != "Y" && checksku[i] != "N")
                    {
                        errorItemNumber1 = "是否验证款号应填写Y/N";
                    }
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }
            if (errorItemNumber1 != "")
            {
                return Helper.RedirectAjax("err", errorItemNumber1, null, "");
            }
            if (errorItemNumber2 != "")
            {
                return Helper.RedirectAjax("err", "导入失败，未导入SN判断为简单业务，SO存在多种SKU！" + errorItemNumber2, null, "");
            }

            List<WCF.InBoundService.InBoundOrderInsert> entityList = new List<WCF.InBoundService.InBoundOrderInsert>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                if (entityList.Where(u => u.SoNumber == soNumber[i].Trim() && u.ClientCode == clientCode[i].Trim()).Count() == 0)
                {
                    WCF.InBoundService.InBoundOrderInsert entity = new WCF.InBoundService.InBoundOrderInsert();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ClientCode = clientCode[i].Trim();
                    entity.SoNumber = soNumber[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    entity.OrderType = "CFS";
                    entity.ProcessId = Convert.ToInt32(Request["processId"]);
                    entity.ProcessName = Request["processName"];

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

                    WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();

                    orderDetail.CustomerPoNumber = entity.SoNumber;
                    orderDetail.CheckAltItemNumberFlag = checksku[i].ToString();

                    if (string.IsNullOrEmpty(altItemNumber[i].ToString().Trim()))
                    {
                        orderDetail.AltItemNumber = orderDetail.CustomerPoNumber;
                    }
                    else
                    {
                        orderDetail.AltItemNumber = altItemNumber[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }

                    List<WCF.InBoundService.InBoundOrderDetailSNInsert> snList = new List<WCF.InBoundService.InBoundOrderDetailSNInsert>();

                    if (!string.IsNullOrEmpty(sn[i].ToString().Trim()))
                    {
                        WCF.InBoundService.InBoundOrderDetailSNInsert snEntity = new WCF.InBoundService.InBoundOrderDetailSNInsert();

                        snEntity.SN = sn[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        snList.Add(snEntity);
                    }

                    orderDetail.Qty = Convert.ToInt32(qty[i].ToString().Trim());
                    orderDetail.CreateUser = Session["userName"].ToString();

                    orderDetail.InBoundOrderDetailSNInsert = snList.ToArray();
                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                    entityList.Add(entity);
                }
                else
                {
                    WCF.InBoundService.InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == soNumber[i].Trim() && u.ClientCode == clientCode[i].Trim()).First();
                    entityList.Remove(oldentity);

                    WCF.InBoundService.InBoundOrderInsert newentity = oldentity;

                    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();

                    if (orderDetailList.Where(u => u.AltItemNumber == altItemNumber[i].Trim()).Count() == 0)
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();

                        orderDetail.CustomerPoNumber = oldentity.SoNumber;
                        orderDetail.CheckAltItemNumberFlag = checksku[i].ToString();

                        if (string.IsNullOrEmpty(altItemNumber[i].ToString().Trim()))
                        {
                            orderDetail.AltItemNumber = orderDetail.CustomerPoNumber;
                        }
                        else
                        {
                            orderDetail.AltItemNumber = altItemNumber[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                        }

                        List<WCF.InBoundService.InBoundOrderDetailSNInsert> snList = new List<WCF.InBoundService.InBoundOrderDetailSNInsert>();

                        if (!string.IsNullOrEmpty(sn[i].ToString().Trim()))
                        {
                            WCF.InBoundService.InBoundOrderDetailSNInsert snEntity = new WCF.InBoundService.InBoundOrderDetailSNInsert();
                            snEntity.SN = sn[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                            snList.Add(snEntity);
                        }

                        orderDetail.Qty = Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetail.CreateUser = Session["userName"].ToString();
                        orderDetail.InBoundOrderDetailSNInsert = snList.ToArray();

                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        WCF.InBoundService.InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.AltItemNumber == altItemNumber[i].Trim()).First();

                        orderDetailList.Remove(oldorderDetail);

                        WCF.InBoundService.InBoundOrderDetailInsert neworderDetail = oldorderDetail;

                        List<WCF.InBoundService.InBoundOrderDetailSNInsert> snList = oldorderDetail.InBoundOrderDetailSNInsert.ToList();

                        if (!string.IsNullOrEmpty(sn[i].ToString().Trim()))
                        {
                            WCF.InBoundService.InBoundOrderDetailSNInsert snEntity = new WCF.InBoundService.InBoundOrderDetailSNInsert();

                            snEntity.SN = sn[i].Trim().ToUpper().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                            snList.Add(snEntity);
                        }
                        neworderDetail.InBoundOrderDetailSNInsert = snList.ToArray();

                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(qty[i].Trim().ToString());
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.InBoundOrderDetailInsert = orderDetailList.ToArray();

                    entityList.Add(newentity);
                }
            }

            string result = cf.ImportsInBoundOrderExtend(entityList.ToArray());
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
