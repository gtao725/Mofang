using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WMS.UI.Controllers.Model
{
    public class RootModel
    {
        //1. 代理管理新增代理验证
        [Display(Name = "代理中文名")]
        [Required(ErrorMessage = "代理中文名不能为空!")]
        public string txt_agentName { get; set; }

        [Display(Name = "代理Code")]
        [Required(ErrorMessage = "代理Code不能为空!")]
        public string txt_agentCode { get; set; }

        [Display(Name = "代理类型")]
        [Required(ErrorMessage = "代理类型不能为空!")]
        public string txt_agentType { get; set; }



        //2. 客户管理新增客户验证
        [Display(Name = "客户名")]
        [Required(ErrorMessage = "客户名不能为空!")]
        public string txt_clientName { get; set; }

        [Display(Name = "客户Code")]
        [Required(ErrorMessage = "客户Code不能为空!")]
        public string txt_clientCode { get; set; }


        //3.储位管理 新增储位验证
        [Display(Name = "收货门区")]
        [Required(ErrorMessage = "收货门区不能为空!")]
        public string txt_reclocation { get; set; }

        [Display(Name = "出货门区")]
        [Required(ErrorMessage = "出货门区不能为空!")]
        public string txt_outlocation { get; set; }


        [Display(Name = "储位所属库区名:")]
        [Required(ErrorMessage = "库区名不能为空!")]
        [RegularExpression(@"^[0-9A-Z]+$", ErrorMessage = "必须为数字或大写字母!")]

        public string txt_beginLocation { get; set; }

        [Display(Name = "储位通道名:")]
        [Required(ErrorMessage = "通道名不能为空!")]
        [RegularExpression(@"^[0-9A-Z]+$", ErrorMessage = "必须为数字或大写字母!")]

        public string txt_beginLocationColumn1 { get; set; }


        [Display(Name = "")]
        [Required(ErrorMessage = "通道名不能为空!")]
        [RegularExpression(@"^[0-9A-Z]+$", ErrorMessage = "必须为数字或大写字母!")]

        public string txt_endLocationColumn1 { get; set; }

        [Display(Name = "起始列(竖列):")]
        [Required(ErrorMessage = "起始列不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_beginLocationRow { get; set; }

        [Display(Name = "结束列(竖列):")]
        [Required(ErrorMessage = "结束列不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_endLocationRow { get; set; }

        [Display(Name = "层数:")]
        [Required(ErrorMessage = "层数不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_LocationFloor { get; set; }

        [Display(Name = "个数:")]
        [Required(ErrorMessage = "个数不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_LocationPcs { get; set; }




        //4.托盘管理 新增托盘验证
        [Display(Name = "托盘字母")]
        [Required(ErrorMessage = "托盘字母不能为空!")]
        public string txt_pallate { get; set; }

        [Display(Name = "起始数字")]
        [Required(ErrorMessage = "起始数字不能为空!")]

        public int txt_beginpallate { get; set; }

        [Display(Name = "结束数字")]
        [Required(ErrorMessage = "结束数字不能为空!")]
        public int txt_endpallate { get; set; }


        [Display(Name = "托盘数字位数")]
        [Required(ErrorMessage = "托盘数字位数不能为空!")]
        public int txt_pallateqtylength { get; set; }


        //5.区域管理 新增区域验证
        [Display(Name = "区域名")]
        [Required(ErrorMessage = "区域名不能为空!")]
        public string txt_zone { get; set; }
    }


    public class ClientRFFlowModel
    {
        //1. RF流程管理
        [Display(Name = "流程名")]
        [Required(ErrorMessage = "流程名不能为空!")]
        public string txt_FlowName { get; set; }


    }


    public class ClientFlowRuleModel
    {
        //1. 客户流程管理
        [Display(Name = "流程名")]
        [Required(ErrorMessage = "流程名不能为空!")]
        public string txt_FlowName { get; set; }


    }


    public class CycleCountMasterModel
    {
       
        [Display(Name = "起始库位")]
        [Required(ErrorMessage = "起始库位不能为空!")]
        public string BeginLocationId { get; set; }

        [Display(Name = "结束库位")]
        [Required(ErrorMessage = "结束库位不能为空!")]
        public string EndLocationId { get; set; }

        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string WhClientList { get; set; }
    }


    public class LoadCreateRuleModel
    {
        //1. 客户流程管理
        [Display(Name = "规则名")]
        [Required(ErrorMessage = "规则名不能为空!")]
        public string txt_RuleName { get; set; }


    }

    public class R_Location_ItemModel
    {
        [Display(Name = "客户名")]
        [Required(ErrorMessage = "客户名不能为空!")]
        public string txt_clientCode { get; set; }

        [Display(Name = "捡货库位")]
        [Required(ErrorMessage = "收捡货库位不能为空!")]
        public string txt_LocationId { get; set; }


        [Display(Name = "款号")]
        [Required(ErrorMessage = "款号不能为空!")]
        public string txt_AltItemNumber { get; set; }

        [Display(Name = "警戒数量")]
        [Required(ErrorMessage = "警戒数量不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_MinQty { get; set; }

        [Display(Name = "上限数量")]
        [Required(ErrorMessage = "上限数量不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_MaxQty { get; set; }
    }


    public class SupplementTaskModel
    {

        [Display(Name = "补货任务个数")]
        [Required(ErrorMessage = "补货任务个数不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_Qty { get; set; }


        [Display(Name = "补货任务个数")]
        [Required(ErrorMessage = "补货任务个数不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int bosch_txt_Qty { get; set; }

    }

    public class LossModel
    {

        [Display(Name = "耗材Code")]
        [Required(ErrorMessage = "耗材Code不能为空!")]
        public string txt_LossCode { get; set; }


        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "必须为数字!")]

        public int txt_Qty { get; set; }


    }

}
