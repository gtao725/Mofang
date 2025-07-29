using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;
using WMS.BLLClass;

namespace WMS.WCFServices.CommonService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“CommonService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 CommonService.svc 或 CommonService.svc.cs，然后开始调试。
    public class CommonService : ICommonService
    {

        public string PageFunc()
        {
            string aaa = "";

            Type t = typeof(InBoundOrderDetailSearch);
            Type m = typeof(InBoundOrderDetail);

            System.Reflection.PropertyInfo[] searchmodel = t.GetProperties();
            System.Reflection.PropertyInfo[] model = m.GetProperties();
            foreach (System.Reflection.PropertyInfo property in searchmodel)
            {
                if (property.Name != "OrderByColumn" && property.Name != "Sort" && property.Name != "pageSize" && property.Name != "pageIndex" && property.Name != "total")
                {
                    aaa += " if ( !string.IsNullOrEmpty(searchEntity." + property.Name + "))" + "<br>";
                    aaa += " sql = sql.Where(u => u." + property.Name + " == searchEntity." + property.Name + ");" + "<br>";
                }
            }

            aaa += "<br>" + "total = sql.Count();" + "<br><br>";

            aaa += "if (searchEntity.OrderByColumn != null)" + "<br>";
            aaa += "{" + "<br>";
            aaa += "for (int i = searchEntity.OrderByColumn.Count(); i > 0; i--)" + "<br>";
            aaa += "{" + "<br>";
            foreach (System.Reflection.PropertyInfo property in model)
            {
                aaa += "if (searchEntity.OrderByColumn[i - 1] == \"" + property.Name + "\" && searchEntity.Sort[i - 1] == true)" + "<br>";
                aaa += "sql = sql.OrderBy(u => u." + property.Name + ");" + "<br>";
                aaa += "if (searchEntity.OrderByColumn[i - 1] == \"" + property.Name + "\" && searchEntity.Sort[i - 1] == false)" + "<br>";
                aaa += "sql = sql.OrderByDescending(u => u." + property.Name + ");" + "<br>";
            }
            aaa += "}" + "<br>";
            aaa += "}" + "<br>";
            aaa += "else{" + "<br>";
            aaa += "sql = sql.OrderBy(u => u.Id);" + "<br>";
            aaa += "}" + "<br>";

            aaa += "<br>" + "sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);";
            aaa += "<br>" + "return sql.ToList();" + "<br>";

            return aaa;

        }

        public string ServiceFunc()
        {
            string aaa = "";

            string model = "InBoundOrder";     //传入实体表 实现普通增删改

            aaa += "#region" + "<br><br>";

            aaa += "[OperationContract]" + "<br>";
            aaa += model + " " + model + "Add(" + model + " entity);" + "<br><br>";

            aaa += "[OperationContract]" + "<br>";
            aaa += "int " + model + "Update(" + model + " entity,params string[] proNames);" + "<br><br>";

            aaa += "[OperationContract]" + "<br>";
            aaa += "int " + model + "DeleteById(int id);" + "<br><br>";

            aaa += "[OperationContract]" + "<br>";
            aaa += "int " + model + "DeleteByListId(List&lt;int&gt; delId);" + "<br><br>";

            aaa += "[OperationContract]" + "<br>";
            aaa += model + " " + model + "Select(int delId);" + "<br><br>";

            aaa += "#endregion" + "<br>";
            return aaa;
        }

        public string ServiceAutoFunc()
        {
            string aaa = "";

            string[] model = new string[] { "InBoundOrder", "InBoundOrderDetail", "InBoundSO" };     //传入实体表 实现普通增删改

            for (int i = 0; i < model.Length; i++)
            {
                aaa += " IBLL.I" + model[i] + "Service i" + model[i] + "Service = new BLL." + model[i] + "Service();" + "<br><br>";

                aaa += "public " + model[i] + " " + model[i] + "Add(" + model[i] + " entity){" + "<br>";
                aaa += "  return i" + model[i] + "Service.Add(entity);" + "<br>";
                aaa += "}" + "<br>";

                aaa += "public " + "int " + model[i] + "Update(" + model[i] + " entity,params string[] proNames){" + "<br>";
                aaa += "  return i" + model[i] + "Service.Update(entity, proNames);" + "<br>";
                aaa += "}" + "<br>";

                aaa += "public " + "int " + model[i] + "DeleteById(int id){" + "<br>";
                aaa += " return i" + model[i] + "Service.DeleteById(id);" + "<br>";
                aaa += "}" + "<br>";

                aaa += "public " + "int " + model[i] + "DeleteByListId(List&lt;int&gt; delId){" + "<br>";
                aaa += "  return i" + model[i] + "Service.DeleteByListId(delId);" + "<br>";
                aaa += "}" + "<br>";

                aaa += "public " + model[i] + " " + model[i] + "Select(int id){" + "<br>";
                aaa += " return i" + model[i] + "Service.Select(id);" + "<br>";
                aaa += "}" + "<br>";
                aaa += "<br>";
            }

            return aaa;
        }
    }
}
