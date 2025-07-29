using System.Collections.Generic;
using System.Web.Mvc;
using System.Text;
namespace WMS.EIP
{
    public static class EIPHtml
    {

        //MvcHtmlString
        public static  MvcHtmlString GetSelect<T>(List<T> selectList,string selectName,string selectValue="",string otherStr="",bool selectNullFlag=true)
        {
             
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<select name={0} {1} >", selectName, otherStr);
            if(selectNullFlag) sb.Append("<option></option>");

 
            //int i = 0;
            //foreach (PropertyInfo pi in selectList[0].GetType().GetProperties()) {
            //    if (selectNameCol == null) selectNameCol = pi.Name;
            //    if (i==1) selectValueCol= pi.Name;
            //    i++;
            //}

            //if (selectValueCol == null&& selectNameCol!=null) selectValueCol = selectNameCol;


            foreach (var item in selectList)
            {
                var optionValue = item.GetType().GetProperty("Name").GetValue(item, null).ToString();
                var optionText = item.GetType().GetProperty("Text").GetValue(item, null).ToString();

                if (optionValue == selectValue)
                    sb.AppendFormat("<option value='{0}' selected='selected'>{1}</option>", optionValue, optionText);
                else
                    sb.AppendFormat("<option value='{0}' >{1}</option>", optionValue, optionText);
            }
            sb.Append("</select>");

           return new MvcHtmlString(sb.ToString());
            //return  sb.ToString();
        }
 

    }
}
