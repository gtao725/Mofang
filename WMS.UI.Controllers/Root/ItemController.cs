using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ItemController : Controller
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
                                           Value = r.Id.ToString()
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
            if (Request["WhClientId"] == "")
            {
                entity.ClientId = 0;
            }
            else
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
            entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.HandFlag = Request["handFlag"];
            entity.ScanFlag = Request["scanFlag"];
            entity.UnitName = Request["sel_unitName"];

            int total = 0;
            List<WCF.RootService.WhItemResult> list = cf.ItemMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("CusItemNumber", "客户款号");
            fieldsName.Add("Description", "描述");
            fieldsName.Add("EAN", "EAN");
            fieldsName.Add("ItemName", "品名");
            fieldsName.Add("Category", "Category");
            fieldsName.Add("Material", "材质");
            fieldsName.Add("Color", "颜色");
            fieldsName.Add("PackageStyle", "包装类型");
            fieldsName.Add("Class", "类别");
            fieldsName.Add("CusStyle", "客户自定义类型");
            fieldsName.Add("OriginCountry", "原产国");
            fieldsName.Add("UnitName", "客户单位");
            fieldsName.Add("Pcs", "箱规");
            fieldsName.Add("HandFlag", "HandFlag");
            fieldsName.Add("ScanFlag", "ScanFlag");
            fieldsName.Add("HandFlagShow", "包装时输入数量");
            fieldsName.Add("ScanFlagShow", "包装时采集SN");
            fieldsName.Add("InstallService", "是否送装");
            fieldsName.Add("ScanRule", "采集SN长度");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CartonName", "包装耗材名称");
            fieldsName.Add("UnitFlag", "多种包装");
            fieldsName.Add("LocFlag", "固定库位");
            fieldsName.Add("LocOnHandFlag", "固定库位其他库位");
            fieldsName.Add("OnHandFlag", "合并托盘");
            fieldsName.Add("OneItemLPFlag", "款号共用托盘");
            fieldsName.Add("OneItemSizeLPFlag", "尺码共用托盘");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,Style1:70,Style2:70,Style3:70,Length:50,Width:50,Height:50,Weight:50,ItemName:120,default:100"));
        }

        //批量导入款号
        public ActionResult imports()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] style1 = Request.Form.GetValues("属性1");
            string[] style2 = Request.Form.GetValues("属性2");
            string[] style3 = Request.Form.GetValues("属性3");
            string[] unitNmae = Request.Form.GetValues("单位");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            if (clientCode.Count() != altItemNumber.Count() || altItemNumber.Count() != style1.Count() || style1.Count() != style2.Count() || style2.Count() != style3.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim() + "-" + unitNmae[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim() + "-" + unitNmae[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + altItemNumber[i].ToString().Trim() + "-" + style1[i].ToString().Trim() + "-" + style2[i].ToString().Trim() + "-" + style3[i].ToString().Trim() + "-" + unitNmae[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }



            string result = cf.ItemImports(clientCode, altItemNumber, style1, style2, style3, unitNmae, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //批量导入款号
        public ActionResult importsItemName()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] itemNmae = Request.Form.GetValues("品名");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 10000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过10000条！", null, "");
            }

            if (clientCode.Count() != altItemNumber.Count() || altItemNumber.Count() != itemNmae.Count() )
            {
                return Helper.RedirectAjax("err", "数据出现异常，请更换浏览器或减少导入量！", null, "");
            }


            string result = cf.ItemImportsItemName(clientCode, altItemNumber, itemNmae, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //批量导入款号
        public ActionResult importsextendoms()
        {

            string[] ClientCode = Request.Form.GetValues("客户");//客户CODE
            string[] AltItemNumber = Request.Form.GetValues("款号");//款号
            string[] Description = Request.Form.GetValues("描述"); //描述
            string[] ItemName = Request.Form.GetValues("品名");//品名
            string[] EAN = Request.Form.GetValues("EAN");//EAN序列号
            string[] Length = Request.Form.GetValues("长(cm)");//长度
            string[] Width = Request.Form.GetValues("宽(cm)");//宽度
            string[] Height = Request.Form.GetValues("高(cm)");//高度
            string[] Weight = Request.Form.GetValues("重量(kg)");//重量
            string[] Pcs = Request.Form.GetValues("箱规(件/箱)");//箱规(件/箱)
            string[] HandFlag = Request.Form.GetValues("包装时输入数量(1/0)");//是否可以输入数量批量出货(1:是；0:否)
            string[] InstallSevice = Request.Form.GetValues("是否送装(1/0)");//送装服务(1:是；0:否)
            string[] Category = Request.Form.GetValues("Category");//Category类别
            string[] Style1 = Request.Form.GetValues("属性1");//属性1
            string[] Style2 = Request.Form.GetValues("属性2");//属性2
            string[] Style3 = Request.Form.GetValues("属性3");//属性3
            string[] ClassName = Request.Form.GetValues("类别");//类别1
            string[] Style = Request.Form.GetValues("款式");//款式
            string[] PackageStyle = Request.Form.GetValues("包装类型");//包装类型
            string[] Size = Request.Form.GetValues("尺码");//尺码
            string[] BoxCode = Request.Form.GetValues("鞋盒编码");//鞋盒编码
            string[] OriginCountry = Request.Form.GetValues("原产国");//原产国
            string[] UnitName = Request.Form.GetValues("单位");//单位
            string[] Matieral = Request.Form.GetValues("材质");//材质
            string[] Color = Request.Form.GetValues("颜色");//颜色
            string[] CusItemNumber = Request.Form.GetValues("客户款号");//客户款号
            string[] CusStyle = Request.Form.GetValues("客户自定义类型");//客户自定义类型

            Hashtable hash = new Hashtable();
            string mess = "";

            if (ClientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (ClientCode.Length > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的款号
            string error = "";

            if (error != "")
            {
                return Helper.RedirectAjax("err", error, null, "");
            }

            List<WCF.RootService.WhItemExtendOMS> list = new List<WCF.RootService.WhItemExtendOMS>();
            for (int i = 0; i < ClientCode.Length; i++)
            {
                if (!hash.ContainsValue("客户:" + ClientCode[i].ToString() + "款号:" + AltItemNumber[i].ToString()))
                {
                    hash.Add(i, ClientCode[i].ToString() + AltItemNumber[i].ToString());
                    WCF.RootService.WhItemExtendOMS entity = new WCF.RootService.WhItemExtendOMS();
                    //仓库编码
                    entity.WhCode = Session["whCode"].ToString();
                    //客户
                    entity.ClientCode = ClientCode[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    //款号
                    entity.AltItemNumber = AltItemNumber[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    //描述
                    entity.Description = Description[i].Replace(@"""", "").Replace(@"'", "");
                    //品名
                    entity.ItemName = ItemName[i].Replace(@"""", "").Replace(@"'", "");
                    //EAN
                    if (!string.IsNullOrEmpty(EAN[i]))
                    {
                        entity.EAN = EAN[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.EAN = "";
                    }
                    //长度(cm)
                    if (!string.IsNullOrEmpty(Length[i]))
                    {
                        entity.Length = Length[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Length = "0";
                    }
                    //宽度(cm)
                    if (!string.IsNullOrEmpty(Width[i]))
                    {
                        entity.Width = Width[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Width = "0";
                    }
                    //高度(cm)
                    if (!string.IsNullOrEmpty(Height[i]))
                    {
                        entity.Height = Height[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Height = "0";
                    }
                    //重量(cm)
                    if (!string.IsNullOrEmpty(Weight[i]))
                    {
                        entity.Weight = Weight[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Weight = "0";
                    }
                    //箱规(件/箱)
                    if (!string.IsNullOrEmpty(Pcs[i]))
                    {
                        entity.Pcs = Pcs[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Pcs = "0";
                    }
                    //包装是否可以输入数量
                    if (!string.IsNullOrEmpty(HandFlag[i]))
                    {
                        entity.HandFlag = HandFlag[i].Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.HandFlag = "0";
                    }
                    //是否送装
                    if (!string.IsNullOrEmpty(InstallSevice[i]))
                    {
                        entity.InstallSevice = InstallSevice[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.InstallSevice = "0";
                    }
                    //Category
                    if (!string.IsNullOrEmpty(Category[i]))
                    {
                        entity.Category = Category[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Category = "";
                    }
                    //属性1
                    if (!string.IsNullOrEmpty(Style1[i]))
                    {
                        entity.Style1 = Style1[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Style1 = "";
                    }
                    //属性2
                    if (!string.IsNullOrEmpty(Style2[i]))
                    {
                        entity.Style2 = Style2[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Style2 = "";
                    }
                    //属性3
                    if (!string.IsNullOrEmpty(Style3[i]))
                    {
                        entity.Style3 = Style3[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Style3 = "";
                    }
                    //类别
                    if (!string.IsNullOrEmpty(ClassName[i]))
                    {
                        entity.ClassName = ClassName[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.ClassName = "";
                    }
                    //款式
                    if (!string.IsNullOrEmpty(Style[i]))
                    {
                        entity.Style = Style[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Style = "";
                    }
                    //包装类型
                    if (!string.IsNullOrEmpty(PackageStyle[i]))
                    {
                        entity.PackageStyle = PackageStyle[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.PackageStyle = "";
                    }
                    //尺码
                    if (!string.IsNullOrEmpty(Size[i]))
                    {
                        entity.Size = Size[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Size = "";
                    }
                    //鞋盒编码
                    if (!string.IsNullOrEmpty(BoxCode[i]))
                    {
                        entity.BoxCode = BoxCode[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.BoxCode = "";
                    }
                    //原产国
                    if (!string.IsNullOrEmpty(OriginCountry[i]))
                    {
                        entity.OriginCountry = OriginCountry[i].Replace("\r\n", "").Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.OriginCountry = "";
                    }
                    //单位
                    if (!string.IsNullOrEmpty(UnitName[i]))
                    {
                        entity.UnitName = UnitName[i].Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.UnitName = "";
                    }
                    //材质
                    if (!string.IsNullOrEmpty(Matieral[i]))
                    {
                        entity.Matieral = Matieral[i].Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Matieral = "";
                    }
                    //颜色
                    if (!string.IsNullOrEmpty(Color[i]))
                    {
                        entity.Color = Color[i].Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.Color = "";
                    }
                    //客户款号
                    if (!string.IsNullOrEmpty(CusItemNumber[i]))
                    {
                        entity.CusItemNumber = CusItemNumber[i].Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.CusItemNumber = "";
                    }
                    //客户自定义类型
                    if (!string.IsNullOrEmpty(CusStyle[i]))
                    {
                        entity.CusStyle = CusStyle[i].Replace(@"""", "").Replace(@"'", "");
                    }
                    else
                    {
                        entity.CusStyle = "";
                    }
                    entity.CreateUser = Session["userName"].ToString();
                    entity.Remark1 = ""; //备注1
                    entity.Remark2 = ""; //备注2
                    entity.Remark3 = ""; //备注3
                    entity.Remark4 = ""; //备注4
                    entity.Remark5 = ""; //备注5
                    list.Add(entity);
                }
                else
                {
                    mess += "区域重复：" + "客户:" + ClientCode[i].ToString() + "款号:" + AltItemNumber[i].ToString() + "<br/>";
                }
            }

            if (mess != "")
            {
                return Helper.RedirectAjax("err", "导入失败！<br/>" + mess, null, "");
            }

            string result = cf.ItemMasterExtendOMSAdd(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //修改代理信息
        [HttpGet]
        public ActionResult EditItemMaster()
        {
            WCF.RootService.ItemMaster entity = new WCF.RootService.ItemMaster();
            entity.Id = Convert.ToInt32(Request["Id"]);

            entity.Description = Request["edit_description"].Trim();
            entity.EAN = Request["edit_ean"].Trim();

            if (!string.IsNullOrEmpty(Request["edit_handFlag"]))
            {
                if (Request["edit_handFlag"] == "null")
                {
                    entity.HandFlag = 0;
                }
                else
                {
                    entity.HandFlag = Convert.ToInt32(Request["edit_handFlag"]);
                }
            }
            else
            {
                entity.HandFlag = 0;
            }

            if (!string.IsNullOrEmpty(Request["edit_scanFlag"]))
            {
                if (Request["edit_scanFlag"] == "null")
                {
                    entity.ScanFlag = 0;
                }
                else
                {
                    entity.ScanFlag = Convert.ToInt32(Request["edit_scanFlag"]);
                }
            }
            else
            {
                entity.ScanFlag = 0;
            }     

            entity.ScanRule = Request["edit_scanRule"].Trim();
            entity.UnitName = Request["edit_unitName"].Trim();

            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.ItemMasterEdit(entity);
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
