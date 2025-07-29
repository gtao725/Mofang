using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WMS.UI.Controllers.Model
{
    public class AdminModel
    {
        //1. 用户信息用户新增验证

        [Display(Name = "登录名")]
        [Required(ErrorMessage = "登录名不能为空!")]
        public string txt_username { get; set; }

        [Display(Name = "姓名")]
        [Required(ErrorMessage = "姓名不能为空!")]
        public string txt_usernameCN { get; set; }

        [Display(Name = "密码")]
        [Required(ErrorMessage = "密码不能为空!")]
        public string txt_password { get; set; }

        [Display(Name = "工号")]
        [Required(ErrorMessage = "工号不能为空!")]
        public string txt_userCode { get; set; }


        //2.职位信息新增职位验证
        [Display(Name = "职位英文简称")]
        [Required(ErrorMessage = "职位英文简称不能为空!")]
        public string txt_posname { get; set; }

        [Display(Name = "职位中文名")]
        [Required(ErrorMessage = "职位中文名不能为空!")]
        public string txt_posnameCN { get; set; }



        //3.权限信息新增权限验证
        [Display(Name = "权限名")]
        [Required(ErrorMessage = "权限名不能为空!")]
        public string txt_powername { get; set; }

        [Display(Name = "权限类型")]
        [Required(ErrorMessage = "权限类型不能为空!")]
        public string txt_powertype { get; set; }



        //4.菜单信息新增菜单验证
        [Display(Name = "菜单英文简称")]
        [Required(ErrorMessage = "菜单英文简称不能为空!")]
        public string txt_menu_name { get; set; }

        [Display(Name = "菜单中文名")]
        [Required(ErrorMessage = "菜单中文名不能为空!")]
        public string txt_menu_nameCN { get; set; }

        [Display(Name = "菜单图标")]
        [Required(ErrorMessage = "菜单图标不能为空!")]
        public string txt_icon { get; set; }


        //4.WinCE管理新增业务对象验证
        [Display(Name = "对象Name")]
        [Required(ErrorMessage = "对象Name不能为空!")]
        public string txt_objectName { get; set; }

        [Display(Name = "对象Value")]
        [Required(ErrorMessage = "对象Value不能为空!")]
        public string txt_objectValue { get; set; }

        [Display(Name = "业务中文名")]
        [Required(ErrorMessage = "业务中文名不能为空!")]
        public string txt_objectDes { get; set; }

        [Display(Name = "业务类型")]
        [Required(ErrorMessage = "业务类型不能为空!")]
        public string txt_objectType { get; set; }

        [Display(Name = "所属业务对象")]
        [Required(ErrorMessage = "所属业务对象不能为空!")]
        public string ObjDes1 { get; set; }

    }

    public class RFFlowRuleModel
    {
        //1. 流程规则对象新增验证

        [Display(Name = "流程ID")]
        [Required(ErrorMessage = "流程ID不能为空!")]
        public string txt_id { get; set; }

        [Display(Name = "流程名")]
        [Required(ErrorMessage = "流程名不能为空!")]
        public string txt_functionName { get; set; }


    }

    public class FlowRuleModel
    {
        //1. 流程规则对象新增验证

        [Display(Name = "流程ID")]
        [Required(ErrorMessage = "流程ID不能为空!")]
        public string txt_id { get; set; }

        [Display(Name = "流程名")]
        [Required(ErrorMessage = "流程名不能为空!")]
        public string txt_functionName { get; set; }

        [Display(Name = "状态名")]
        [Required(ErrorMessage = "状态名不能为空!")]
        public string txt_StatusName { get; set; }


        [Display(Name = "对应RF流程")]
        [Required(ErrorMessage = "RF流程不能为空!")]
        public string txt_businessObjectGroupId { get; set; }

    }

}
