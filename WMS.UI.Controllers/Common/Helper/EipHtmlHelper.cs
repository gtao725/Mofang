
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


namespace System.Web.Mvc.Html
{
    public static class EipHtmlHelper
    {
        /// <summary>
        /// 使用IEnumerable<SelectListItem>创建下拉菜单
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="selectList">下拉菜单IEnumerable对象</param>
        /// <param name="selectName">下拉菜单名称</param>
        /// <param name="hint">下拉菜单前面的SPAN描述</param>
        /// <param name="selectValue">默认值</param>
        /// <param name="otherStr">扩展HTML</param>
        /// <param name="IsAddEmpty">是否添加空选项<option></option></param>
        /// <returns></returns>
        public static MvcHtmlString EipGetSelect(this HtmlHelper helper, IEnumerable<SelectListItem> selectList, string selectName, string hint = "", string selectValue = "", string otherStr = "", bool IsAddEmpty = true)
        {
            StringBuilder sb = new StringBuilder();            
            sb.AppendFormat("<select name='{0}' {1}", selectName, otherStr);
            if (hint+"" != "") sb.AppendFormat(" hint='{0}'", hint);
            sb.Append(">");
            if (IsAddEmpty) sb.Append("<option></option>");
            foreach (SelectListItem item in selectList)
            {
                if (item.Value == selectValue)
                    sb.AppendFormat("<option value='{0}' selected='selected'>{1}</option>", item.Value, item.Text);
                else
                    sb.AppendFormat("<option value='{0}' >{1}</option>", item.Value, item.Text);
            }
            sb.Append("</select>");

            return new MvcHtmlString(sb.ToString());
            //return  sb.ToString();
        }
        //创建下拉菜单中Option
        public static MvcHtmlString EipSelectOption(this HtmlHelper helper, IEnumerable<SelectListItem> selectList, string selectValue = "", bool IsAddEmpty = true)
        {
            StringBuilder sb = new StringBuilder();
            if (IsAddEmpty) sb.Append("<option></option>");
            foreach (SelectListItem item in selectList)
            {
                if (item.Value == selectValue)
                    sb.AppendFormat("<option value='{0}' selected='selected'>{1}</option>", item.Value, item.Text);
                else
                    sb.AppendFormat("<option value='{0}' >{1}</option>", item.Value, item.Text);
            }
            return new MvcHtmlString(sb.ToString());
            //return  sb.ToString();
        }

        public static MvcHtmlString EipGetSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, string selectValue = "",string selectName="", bool IsAddEmpty = true)
        {
            return htmlHelper.EipSelect(expression, selectList, selectValue, selectName, IsAddEmpty, htmlAttributes: HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        public static MvcHtmlString EipSelect<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string selectValue, string selectName, bool IsAddEmpty, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata _metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string _name = ExpressionHelper.GetExpressionText(expression);
            string _fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(_name);
            //如果传递了selectName则不使用Model的名称
            if (!String.IsNullOrEmpty(selectName)) _fullName = selectName;
            string _htmlCtrlstr = string.Empty;//控件Html字符
            if (String.IsNullOrEmpty(_fullName))
            {
                throw new ArgumentException(_name + " 字段不存在！", "name");
            }
            TagBuilder tagBuilder = new TagBuilder("select");

            tagBuilder.MergeAttribute("name", _fullName, true);

            //暂时不添加ID
            //tagBuilder.MergeAttribute("id", _fullName, true);
            //hint添加DisplayName为input前面的名称
            tagBuilder.MergeAttribute("hint", _metadata.DisplayName, true);

            //添加扩展属性例如left=90 onclik等,会覆盖前面所有的同名属性
            tagBuilder.MergeAttributes(htmlAttributes,true);

            //默认值暂时启用前台传递过来的,不使用Model的默认值
 
             tagBuilder.InnerHtml = EipSelectOption(htmlHelper, selectList, selectValue, IsAddEmpty).ToHtmlString();

 
            ///验证部分代码开始
            Dictionary<string, object> _results = new Dictionary<string, object>();
            string _validType = string.Empty;
            string _ErrorMessage = string.Empty;
            if (htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                IEnumerable<ModelClientValidationRule> _clientRules = ModelValidatorProviders.Providers.GetValidators(_metadata ?? ModelMetadata.FromStringExpression(_name, htmlHelper.ViewData), htmlHelper.ViewContext).SelectMany(v => v.GetClientValidationRules());
                if (_clientRules.Count() > 0)
                {
                    //如果有验证资料,添加验证属性
                    tagBuilder.MergeAttribute("class", "easyui-validatebox", true);
                    _validType = string.Empty;
                    foreach (ModelClientValidationRule rule in _clientRules)
                    {
                        switch (rule.ValidationType)
                        {
                            case "required":
                                if (!String.IsNullOrEmpty(rule.ErrorMessage))
                                    tagBuilder.MergeAttribute("missingMessage", rule.ErrorMessage, true);
                                break;

                            //下拉菜单暂时不验证格式
                            //default:
                            //    if (!string.IsNullOrEmpty(_validType)) _validType += ",";
                            //    //整数时候返回integer验证样式,否则都返回前台样式
                            //    if (expression.ReturnType.Name == "Int32")
                            //        _validType += "'integer'";
                            //    else
                            //        _validType += "'" + rule.ValidationType + "'";
                            //    break;
                        }
                    }
                    //添加验证错误时候的提示信息
                    //if (!String.IsNullOrEmpty(_ErrorMessage))
                    //    tagBuilder.MergeAttribute("invalidMessage", _ErrorMessage, true);

                    //if (!string.IsNullOrEmpty(_validType)) _validType = "validType:[" + _validType + "]";
                    if (_metadata.IsRequired)
                    {
                        if (string.IsNullOrEmpty(_validType))
                            _validType = "required:true ";
                        else
                            _validType = "required:true," + _validType;
                    }
                    if (!string.IsNullOrEmpty(_validType)) tagBuilder.MergeAttribute("data-options", _validType);
                }
            }
 
                
            return new MvcHtmlString(tagBuilder.ToString());
        }



        public static MvcHtmlString EipInputFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string type = "input",string inputname="")
        {
            return htmlHelper.EipInputFor(expression, null, type: type, name: inputname);
        }
        public static MvcHtmlString EipInputFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, string type = "input", string inputname = "")
        {
            return htmlHelper.EipInputFor(expression, htmlAttributes: HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes), type: type, name: inputname);
        }
        public static MvcHtmlString EipInputFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes, string type = "input", string name = "")
        {
            ModelMetadata _metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string _name = ExpressionHelper.GetExpressionText(expression);
            string _dataType = expression.ReturnType.Name;
            
            string _fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(_name);
            if (!String.IsNullOrEmpty(name)) _fullName = name;
            string _htmlCtrlstr = string.Empty;//控件Html字符
            if (String.IsNullOrEmpty(_fullName))
            {
                throw new ArgumentException(_name + " 字段不存在！", "name");
            }

            TagBuilder tagBuilder = new TagBuilder(type);

            tagBuilder.MergeAttribute("name", _fullName, true);
 
            //暂时不添加ID
            // tagBuilder.MergeAttribute("id", _fullName, true);
            //hint添加DisplayName为input前面的名称
            tagBuilder.MergeAttribute("hint", _metadata.DisplayName, true);
            //DateTime类型添加date属性,前天可以弹出js控件
            if (_dataType == "DateTime")
            {
                if (_metadata.DisplayFormatString == null)
                    tagBuilder.MergeAttribute("date", "Y", true);
                //能选择时分的控件
                else if (_metadata.DisplayFormatString == "{0:yyyy-MM-dd hh:mm:00}")
                {
                    tagBuilder.MergeAttribute("datetime", "Y", true);
                }
            }
             
            //添加扩展属性例如left=90 onclik等,会覆盖前面所有的同名属性
            tagBuilder.MergeAttributes(htmlAttributes,true);

            //默认值暂时启用前台传递过来的,不使用Model的默认值
            //if (_metadata.Model != null&& _dataType=="String")
            //{
            //    if (type.ToLower() == "input") tagBuilder.MergeAttribute("value", Convert.ToString(_metadata.Model));
            //    else if (type.ToLower() == "textarea") tagBuilder.InnerHtml = Convert.ToString(_metadata.Model);

            //}
            ///验证部分代码开始
            Dictionary<string, object> _results = new Dictionary<string, object>();
            string _validType = string.Empty;
            string _ErrorMessage = string.Empty;

            if (htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                IEnumerable<ModelClientValidationRule> _clientRules = ModelValidatorProviders.Providers.GetValidators(_metadata ?? ModelMetadata.FromStringExpression(_name, htmlHelper.ViewData), htmlHelper.ViewContext).SelectMany(v => v.GetClientValidationRules());
                if (_clientRules.Count() > 0)
                {
                    //如果有验证资料,添加验证属性
                    tagBuilder.MergeAttribute("class", "easyui-validatebox", true);

                    _validType = string.Empty;
                    foreach (ModelClientValidationRule rule in _clientRules)
                    {
                    
                        switch (rule.ValidationType)
                        {
                            case "required":
                                if(!String.IsNullOrEmpty(rule.ErrorMessage))
                                    tagBuilder.MergeAttribute("missingMessage", rule.ErrorMessage, true);
                                break;
                            case "length":
                                if (!string.IsNullOrEmpty(_validType))
                                    _validType += ",";

                                if (!string.IsNullOrEmpty(rule.ErrorMessage)) _ErrorMessage += rule.ErrorMessage;
                                if (rule.ValidationParameters.ContainsKey("min")) _validType += "'" + rule.ValidationType + "[" + rule.ValidationParameters["min"].ToString() + "," + rule.ValidationParameters["max"].ToString() + "]'";
                                else _validType += "'" + rule.ValidationType + "[0," + rule.ValidationParameters["max"].ToString() + "]'";

                                break;
                            //正则表达式中不能包含×,这里将\\转义成×了,然后js解析
                            case "regex":
                                if (!string.IsNullOrEmpty(_validType))
                                    _validType += ",";
                                if (!string.IsNullOrEmpty(rule.ErrorMessage)) _ErrorMessage += rule.ErrorMessage;
                                _validType += "'" + rule.ValidationType + "[\"" + rule.ValidationParameters["pattern"].ToString().Replace("\\", "×")  + "\"]'";
                                break;
                            default:
                                if (!string.IsNullOrEmpty(_validType)) _validType += ",";
                                //整数时候返回integer验证样式,否则都返回前台样式
                                if (_dataType == "Int32")
                                    _validType += "'integer'";
                                else {
                                    if (rule.ValidationType=="date"&& _metadata.DisplayFormatString == "{0:yyyy-MM-dd hh:mm:00}")
                                        _validType += "'datetime'";
                                    else
                                        _validType += "'" + rule.ValidationType + "'";

                                }
                                    
                                break;
                        }
                    }
                    //添加验证错误时候的提示信息
                    if (!String.IsNullOrEmpty(_ErrorMessage))
                        tagBuilder.MergeAttribute("invalidMessage", _ErrorMessage, true);

                    if (!string.IsNullOrEmpty(_validType)) _validType = "validType:[" + _validType + "]";
                    if (_metadata.IsRequired)
                    {
                        if (string.IsNullOrEmpty(_validType))
                            _validType = "required:true ";
                        else
                            _validType = "required:true," + _validType;
                    }
                    if (!string.IsNullOrEmpty(_validType)) tagBuilder.MergeAttribute("data-options", _validType);
                }
            }
            ///验证部分代码结束
            if (type.ToLower() == "input") _htmlCtrlstr = tagBuilder.ToString((TagRenderMode.SelfClosing));
            else if (type.ToLower() == "textarea") _htmlCtrlstr = tagBuilder.ToString();
            _htmlCtrlstr = _htmlCtrlstr.Replace("\"", "'");
            return new MvcHtmlString(_htmlCtrlstr);
        }


        //public static MvcHtmlString EipInput<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes, string type = "input")
        //{
        //    ModelMetadata _metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        //    string _name = ExpressionHelper.GetExpressionText(expression);
        //    string _fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(_name);
        //    string _htmlCtrlstr = string.Empty;//控件Html字符
        //    if (String.IsNullOrEmpty(_fullName))
        //    {
        //        throw new ArgumentException(_name + " 字段不存在！", "name");
        //    }
        //    TagBuilder tagBuilder = new TagBuilder(type);
        //    tagBuilder.MergeAttributes(htmlAttributes);
        //    tagBuilder.MergeAttribute("name", _fullName, true);
        //    tagBuilder.MergeAttribute("id", _fullName, true);
        //    //值
        //    if (_metadata.Model != null)
        //    {
        //        if (type.ToLower() == "input") tagBuilder.MergeAttribute("value", (string)_metadata.Model);
        //        else if (type.ToLower() == "textarea") tagBuilder.InnerHtml = (string)_metadata.Model;

        //    }
        //    ///验证部分代码开始
        //    Dictionary<string, object> _results = new Dictionary<string, object>();
        //    string _validType = string.Empty;
        //    if (htmlHelper.ViewContext.UnobtrusiveJavaScriptEnabled)
        //    {
        //        IEnumerable<ModelClientValidationRule> _clientRules = ModelValidatorProviders.Providers.GetValidators(_metadata ?? ModelMetadata.FromStringExpression(_name, htmlHelper.ViewData), htmlHelper.ViewContext).SelectMany(v => v.GetClientValidationRules());
        //        if (_clientRules.Count() > 0)
        //        {
        //            _validType = string.Empty;
        //            foreach (ModelClientValidationRule rule in _clientRules)
        //            {
        //                switch (rule.ValidationType)
        //                {
        //                    case "required":
        //                        break;
        //                    case "length":
        //                        if (!string.IsNullOrEmpty(_validType)) _validType += ",";
        //                        if (rule.ValidationParameters.ContainsKey("min")) _validType += "'" + rule.ValidationType + "[" + rule.ValidationParameters["min"].ToString() + "," + rule.ValidationParameters["max"].ToString() + "]'";
        //                        else _validType += "'" + rule.ValidationType + "[0," + rule.ValidationParameters["max"].ToString() + "]'";
        //                        break;
        //                    default:
        //                        if (!string.IsNullOrEmpty(_validType)) _validType += ",";
        //                        _validType += "'" + rule.ValidationType + "'";
        //                        break;
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(_validType)) _validType = "validType:[" + _validType + "]";
        //            if (_metadata.IsRequired)
        //            {
        //                if (string.IsNullOrEmpty(_validType))
        //                    _validType = "required:true ";
        //                else
        //                    _validType = "required:true," + _validType;
        //            }
        //            if (!string.IsNullOrEmpty(_validType)) tagBuilder.MergeAttribute("data-options", _validType);
        //        }
        //    }
        //    ///验证部分代码结束
        //    if (type.ToLower() == "input") _htmlCtrlstr = tagBuilder.ToString((TagRenderMode.SelfClosing));
        //    else if (type.ToLower() == "textarea") _htmlCtrlstr = tagBuilder.ToString();
        //    return new MvcHtmlString(_htmlCtrlstr);
        //}


 

    }
}
