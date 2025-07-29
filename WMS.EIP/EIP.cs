using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Reflection;
using System.Data;
using System.Collections;
using Newtonsoft.Json.Converters;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Mvc;

using System.Configuration;
 
namespace WMS.EIP    
{
    public  partial class EIP
    {
        //public const string ReportServer = "http://10.88.88.103:88";
        // public const 
        public static string WMSDB = ConfigurationManager.AppSettings["WMSDBAddress"].ToString();
        public static string ReportServer = ConfigurationManager.AppSettings["ReportServerAddress"].ToString();
        //public  string ReportServer = ReportServer1;
      //  public const string ReportServer = "http://localhost:63055";
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">传入的数据Model类型</typeparam>
        /// <param name="detail">数据List</param>
        /// <param name="rowsCount">查询的总行数</param>
        /// <param name="fieldsName">客制化字段名称</param>
        /// <param name="fieldsWidth">客制化字段宽度</param>
        /// <param name="cds">数据datatable</param>
        /// <param name="sumFields">求和字段,暂时未实现</param>
        /// <param name="pageSize">每页行数</param>
        ///  <param name="sumResult">每页行数</param>
        /// 
        /// <returns></returns>
        public string EipListJson<T>(List<T> detail, int rowsCount, Dictionary<string, string> fieldsName = null, string fieldsWidth = "default:80", System.Data.DataSet cds = null, string sumFields = "", int pageSize = 50,string sumResult="")
        {

            if (HttpContext.Current.Request["ExportExecl"] != "Y")
            {

                //输出json对象到页面
                HttpContext.Current.Response.AddHeader("Content-Type", "application/json");

                ////时间格式的处理
                //IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
                //timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";


                if (detail.Count() > 0)
                {
                    pageSize = HttpContext.Current.Request["eip_page_size"] == null ? pageSize : Convert.ToInt32(HttpContext.Current.Request["eip_page_size"]);
                    int page = HttpContext.Current.Request["eip_page"] == null ? 1 : Convert.ToInt32(HttpContext.Current.Request["eip_page"]);
                    //int page = (int)Math.Ceiling((double)rowsCount / (double)pageSize);
                    EipDataGridModel eipDataGridModel = new EipDataGridModel { rows = EipDataGridModelData<T>(detail, fieldsName), total = rowsCount, page = page, sum = sumResult, columns = eipDataGridColumnsList(detail, fieldsName, fieldsWidth) };
                    return JsonConvert.SerializeObject(eipDataGridModel, Newtonsoft.Json.Formatting.Indented, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
                }
                else
                    return EipListJsonEmpty();
            }
            else {
                DataTable dt = EipDataGridModelData<T>(detail, fieldsName,false);
                
               string eip_page_title = string.IsNullOrEmpty( HttpContext.Current.Request["eip_page_title"])? "魔方汇出" :  HttpContext.Current.Request["eip_page_title"];

                int i = 0;
                foreach (var item in fieldsName)
                {

                    dt.Columns[item.Value].SetOrdinal(i);
                    i++;
                }
                ExportExecl(dt, eip_page_title+ ".xlsx");
                return null;
            }

        }

        private  string EipListJsonEmpty()
        {
            return JsonConvert.SerializeObject(new EipDataGridModel { rows = new ArrayList { new { 查询结果 = "抱歉!未查询到数据!" } }, total = 1, page = 1, columns = new ArrayList { new EipDataGridModelColumns { column = "查询结果", width = "120", type = "string", osort = 1, sort = 1 } } });
        }


        /// <summary>
        /// 返货客制化好的list数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="detail">传入的List</param>
        /// <param name="fieldsName">客制化的字段</param>
        /// <returns></returns>
        private DataTable EipDataGridModelData<T>(List<T> detail, Dictionary<string, string> fieldsName = null,bool showOtherColumns = true )
        {
            //如果没有需要客制化的字段直接返回 
            if (fieldsName == null) return ToDataTable<T>(detail);
            //创建返回 DataTable
            DataTable result = new DataTable();
            //生成fieldsNameList
          //  List<string> fieldsNameList = fieldsName.Split(',').ToList();

            //反射List出列名
            PropertyInfo[] propertys = detail[0].GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                Type columnType = pi.PropertyType;

                if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    columnType = pi.PropertyType.GetGenericArguments()[0];
                }

                //fieldsNameList如果包含该字段的别名,取出 ":" 后面的值作为别名
                
                if (fieldsName.ContainsKey(pi.Name))
                    result.Columns.Add(fieldsName[pi.Name].ToString(), columnType);
                else if(showOtherColumns)
                    //否则直接取detail里面的名称,如果要去掉的话要与下面的数据一起去掉
                    result.Columns.Add(pi.Name, columnType);
            }


            for (int i = 0; i < detail.Count; i++)
            {
                // 创建Object类型的数组
                ArrayList tempList = new ArrayList();
                foreach (PropertyInfo pi in propertys) 
                {
                    //创建一个行对象
                    object obj = pi.GetValue(detail[i], null);

                    if (DictionaryOrdinal(fieldsName,pi.Name,null) !=999||showOtherColumns)// 判断有没有这1列 暂时不启用,这个要与上面的反射一起启用
                        tempList.Add(obj);
                }
                object[] array = tempList.ToArray();
                //填充到DataTable
                result.LoadDataRow(array, true);
            }

 

            return result;
        }


        /// <summary>  
        /// 转化一个DataTable  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="list"></param>  
        /// <returns></returns>  
        public  DataTable ToDataTable<T>(List<T> list)
        {

            //创建属性的集合  
            List<PropertyInfo> pList = new List<PropertyInfo>();
            //获得反射的入口  
            Type type = typeof(T);
            DataTable dt = new DataTable();
            //把所有的public属性加入到集合 并添加DataTable的列  
            Array.ForEach<PropertyInfo>(type.GetProperties(), p => { 

                pList.Add(p);
                Type columnType = p.PropertyType;

                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    columnType = p.PropertyType.GetGenericArguments()[0];
                }

                dt.Columns.Add(p.Name, columnType); 
            
            });
            foreach (var item in list)
            {
                //创建一个DataRow实例  
                DataRow row = dt.NewRow();
                //给row 赋值  
                pList.ForEach(p => row[p.Name] = p.GetValue(item, null));
                //加入到DataTable  
                dt.Rows.Add(row);
            }
            return dt;
        }

      
        /// <summary>
        /// 获取eipDataGridColumnsList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="detail">数据表detail 的list</param>
        /// <param name="fieldsName">客制化的字段名称</param>
        /// <param name="fieldsWidth">客制化的字段宽度</param>
        /// <param name="ifWcfObject">detail是否为wcf对象</param>
        /// <returns> List<EipDataGridModelColumns></returns>
        private   List<EipDataGridModelColumns> eipDataGridColumnsList<T>(List<T> detail, Dictionary<string, string> fieldsName, string fieldsWidth, bool ifWcfObject = true)
        {

            //detail明细或者 fieldsWidth 为空返回null
            if (detail.Count == 0 || fieldsWidth.Length == 0) return null;
            //字段宽度
            Dictionary<string, string> fieldsWidthDic = new Dictionary<string, string>();
            string[] fieldsWidthArray = fieldsWidth.Split(',');
            foreach (var item in fieldsWidthArray)
            {
                fieldsWidthDic.Add(item.Split(':')[0], item.Split(':')[1]);
            }
          

            //客制话字段名称
            //List<string> fieldsNameList = fieldsName.Split(',').ToList();

            //返回字段名称List
            Dictionary<string, string> ColumnsNameDic = new Dictionary<string, string>();
            //字段类型List
            Dictionary<string, string> ColumnsTypeDic = new Dictionary<string, string>();
            //字段宽度List
            Dictionary<string, string> ColumnsWidthDic = new Dictionary<string, string>();

            //字段宽度默认值
            string defaultFieldsWidth = fieldsWidthDic["default"];

            //返回columnsList
            //目标 "columns": [{"column":"批号",  "width": 50,"type": "string","osort": 0, "sort": 0},{"column":"批号",  "width": 50,"type": "string","osort": 0, "sort": 0}]"
            List<EipDataGridModelColumns> columnsList = new List<EipDataGridModelColumns>();

            int i = 0;
            //反射detail,获取detail对象所有的属性,取得Name与PropertyType 即List里面的column与 type
            foreach (PropertyInfo pi in detail[0].GetType().GetProperties())
            {
                //wcf的detail对象多余一个ExtensionData列, 去除ExtensionData,
                if (i > 0 || !ifWcfObject)
                {

                    Type columnType = pi.PropertyType;

                    if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        columnType = pi.PropertyType.GetGenericArguments()[0];
                    }
                    string newFieldsName=null;
                    bool flag = fieldsName.ContainsKey(pi.Name);
                    //fieldsName如果包含该字段的别名,取出值作为别名
                    if (flag) {
                        newFieldsName = fieldsName[pi.Name].ToString();
                        ColumnsNameDic.Add(pi.Name, newFieldsName);
                        //获取detail字段的类型
                        ColumnsTypeDic.Add(newFieldsName, columnType.Name);
                    }
                    //else
                    //    //否则直接取detail里面的名称
                    //    ColumnsNameDic.Add(pi.Name, pi.Name);

                    //取得fieldsWidthList里面客制化的宽度,取出 ":" 后面的值作为别名
                    if (fieldsWidthDic.ContainsKey(pi.Name)&& flag)
                        fieldsWidthDic.Add(newFieldsName, fieldsWidthDic[pi.Name].ToString());
                    else if(flag)
                        //否则取得默认值defaultFieldsWidth
                        fieldsWidthDic.Add(newFieldsName, defaultFieldsWidth);
                }
                i++;
            }

            var sql = from a in fieldsName
                      join b in fieldsWidthDic
                      on a.Value equals b.Key
                      join c in ColumnsTypeDic
                      on a.Value equals c.Key
                      select new EipDataGridModelColumns
                      {
                          column = a.Value,
                          width = b.Value,
                          type = c.Value,
                          //osort = index,
                          //sort = Guid.NewGuid()
                      };

            List<EipDataGridModelColumns> EipDataGridModelColumnsList = sql.ToList();

            int j = 0;
            foreach (var item in EipDataGridModelColumnsList)
            {
                item.osort = j;
                item.sort = j;
                j++;
            }
            return EipDataGridModelColumnsList;
        }


        private   int DictionaryOrdinal(Dictionary<string, string> di,string key, string value)
        {

            if (value!=null|| key!=null) { 
                int j = 0;
                foreach (var item in di)
                { 
                    if (item.Value == value)
                        return j;
                    else if(item.Key == key)
                        return j;
                    j++;
                }
            }

            return 999;
        }


        /// <summary>
        /// 报表的List
        /// </summary>
        /// <param name="SqlName">sql名称,如果SQL名称和SQL内容都有,优先SQL内容</param>
        /// <param name="Sql">sql内容,如果SQL名称和SQL内容都有,优先SQL内容</param>
        /// <param name="SqlParameters">sql的查询参数, 例如 Hashtable SqlParameters = new Hashtable(); SqlParameters.Add("@WhCode", "01"); SqlParameters.Add("@HuId", "");</param>
        /// <param name="fieldsWidth">报表字段宽度,默认default:80</param>
        /// <param name="OrderByPara">排序字段,这个只用别名字段,不用数据库字段,这个要是与前台排序字段重复的话优先后台排序
        /// ,Dictionary<string, string> OrderByPara = new Dictionary<string, string>(); OrderByPara.Add("托盘号", " asc");OrderByPara.Add("ReceiptDate", " desc");</param>
        /// <param name="ExecDbName"> 执行数据的DB,默认是"10#88#88#103.WMS";</param>
        /// <param name="pageSize">每页的行数,默认是50</param>
        ///  <param name="SumPara">求和字段</param>
        ///  <param name="ExecType">复杂SQL执行方式##SqlQueryBegin开始 ##SqlQueryEnd 结尾</param> 
        /// <returns></returns>
        public string ReportList(string SqlName,string Sql, Hashtable SqlParameters, string fieldsWidth = "default:80", Dictionary<string, string> OrderByPara =null , string ExecDbName= null, int pageSize = 50,bool IfReturnSql=false, string SumPara = null,bool ExecType=false) {

            HttpContext aa = HttpContext.Current;

            //输出json对象到页面
            HttpContext.Current.Response.AddHeader("Content-Type", "application/json");
            pageSize = HttpContext.Current.Request["eip_page_size"] == null ? pageSize : Convert.ToInt32(HttpContext.Current.Request["eip_page_size"]);
            int page = HttpContext.Current.Request["eip_page"] == null ? 1 : Convert.ToInt32(HttpContext.Current.Request["eip_page"]);
            if (string.IsNullOrEmpty(ExecDbName))
                ExecDbName = WMSDB;
            else
                ExecDbName= ConfigurationManager.AppSettings[ExecDbName].ToString();

            //处理前台的排序对象
            string eip_sorts = HttpUtility.UrlDecode(HttpContext.Current.Request["eip_sorts"]);
 
            if (!string.IsNullOrEmpty(eip_sorts))
            {
                List<eip_sorts> listSorts = JsonConvert.DeserializeObject<List<eip_sorts>>(eip_sorts);
                foreach (eip_sorts item in listSorts)
                {
                    if (OrderByPara == null) OrderByPara = new Dictionary<string, string>();
 
                    if (!OrderByPara.ContainsKey(item.name))
                        OrderByPara.Add(item.name, item.value);
                }
            }
            ReportRequestDataModel requestDataModel = new ReportRequestDataModel();
            requestDataModel.SqlName = SqlName;
            requestDataModel.ExecDbName = ExecDbName;
            requestDataModel.Sql = Sql;
            requestDataModel.Page = page;
            requestDataModel.Size = pageSize;
            requestDataModel.SqlParameter = SqlParameters;
            requestDataModel.OrderByPara = OrderByPara;
            requestDataModel.IfReturnSql = IfReturnSql;
            requestDataModel.SumPara = SumPara;
            requestDataModel.ExecType = ExecType;
            //如果是汇出的话返回SQL参数对象
            if (HttpContext.Current.Request["ExportExecl"] != null) return JsonConvert.SerializeObject(requestDataModel);
            string jsonStr = ReportPost(requestDataModel, ReportServer+ "/api/SqlToJson/SqlToJson");
            //返回sql
            if (IfReturnSql) return jsonStr;
            DataSet ds = JsonToDataSet(jsonStr);
           // DataSet  ds= JsonToDataSet("{\"Result\":[{\"WhCode\":\"01\",\"托盘号\":\"L10\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L11\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L12\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L13\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L14\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L15\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L16\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:41\"},{\"WhCode\":\"01\",\"托盘号\":\"L17\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L18\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L19\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L20\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L21\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L22\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L23\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L24\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:42\"},{\"WhCode\":\"01\",\"托盘号\":\"L25\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L26\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L27\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L28\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L29\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L30\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L31\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:43\"},{\"WhCode\":\"01\",\"托盘号\":\"L32\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L33\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L34\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L35\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L36\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L37\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L38\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"},{\"WhCode\":\"01\",\"托盘号\":\"L39\",\"库位\":\"A1001\",\"ReceiptDate\":\"2016-11-21 11:16:44\"}],\"Count\":[{\"Counts\":15339}]}");
            return EipListJson(ds, fieldsWidth, page, SumPara);

        }

        /// <summary>
        /// 报表的List
        /// </summary>
        /// <param name="Sql">sql内容,如果SQL名称和SQL内容都有,优先SQL内容</param>
        /// <param name="SqlName">sql名称,如果SQL名称和SQL内容都有,优先SQL内容</param>
        /// <param name="SqlParameters">sql的查询参数, 例如 Hashtable SqlParameters = new Hashtable(); SqlParameters.Add("@WhCode", "01"); SqlParameters.Add("@HuId", "");</param>
        /// <param name="ExecDbName"> 执行数据的DB,默认是"10#88#88#103.WMS";</param>
        /// <returns></returns>
        public List<SelectListItem> ReportSelect(string Sql,string SqlName , Hashtable SqlParameters, string ExecDbName = null)
        {
            
            List<SelectListItem> selectListItem = new List<SelectListItem>();
            ReportRequestDataModel requestDataModel = new ReportRequestDataModel();
            requestDataModel.SqlName = SqlName;
            requestDataModel.ExecDbName = WMSDB;
            requestDataModel.Sql = Sql;
            requestDataModel.SqlParameter = SqlParameters;
            //requestDataModel.SumPara = SumPara;
            //这个是所有数据都拉
            requestDataModel.Size = 0;

            DataTable dt = ReportDataTable(requestDataModel);
            if (dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    SelectListItem item = new SelectListItem();
                    item.Text = dr[0].ToString();
                    if (dt.Columns.Count > 1) 
                        item.Value = dr[1].ToString();
                    else
                        item.Value = dr[0].ToString();
                    selectListItem.Add(item);

                }
            }
            return selectListItem;

        }

 
        public List<string> ReportList(string Sql, string SqlName)
        {

            List<string> selectListItem = new List<string>();
            ReportRequestDataModel requestDataModel = new ReportRequestDataModel();
            requestDataModel.SqlName = SqlName;
            requestDataModel.ExecDbName = WMSDB;
            requestDataModel.Sql = Sql;
            //requestDataModel.SumPara = SumPara;
            //这个是所有数据都拉
            requestDataModel.Size = 0;

            DataTable dt = ReportDataTable(requestDataModel);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    //SelectListItem item = new SelectListItem();
                    //item.Text = dr[0].ToString();
                    //if (dt.Columns.Count > 1)
                    //    item.Value = dr[1].ToString();
                    //else
                    //    item.Value = dr[0].ToString();
                    selectListItem.Add(dr[0].ToString());

                }
            }
            return selectListItem;

        }

        /// <summary>
        /// 请求报表服务器返回DataTable
        /// </summary>
        /// <param name="requestDataModel"> 请求对象</param>
        /// <returns>请求报表服务器返回DataTable</returns>
        public DataTable ReportDataTable(ReportRequestDataModel requestDataModel) {
            return JsonToDataTable(ReportPost(requestDataModel, ReportServer + "/api/SqlToJson/SqlToJson"));
        }
         

 
        public DataTable JsonToDataTable(string Json)
        {
            try
            {
                return (DataTable)JsonConvert.DeserializeObject(Json, typeof(DataTable));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将JSON解析成DataSet只限标准的JSON数据
        /// 例如：Json＝{t1:[{name:'数据name',type:'数据type'}]} 
        /// 或 Json＝{t1:[{name:'数据name',type:'数据type'}],t2:[{id:'数据id',gx:'数据gx',val:'数据val'}]}
        /// </summary>
        /// <param name="Json">Json字符串</param>
        /// <returns>DataSet</returns>
        public   DataSet JsonToDataSet(string Json)
        {
            try
            {
               return (DataSet)JsonConvert.DeserializeObject(Json, typeof(DataSet));
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 返回EipListJson
        /// </summary>
        /// <param name="details">明细包含总数的DataSet</param>
        /// <param name="fieldsWidth">字段宽度</param>
        /// <param name="page">当前页</param>
        /// <returns></returns>
        private   string EipListJson(DataSet details, string fieldsWidth, int page,string sumPara=null)
        {

            DataTable dt = details.Tables["Result"];
            List<EipDataGridModelColumns> columns = eipDataGridColumnsList(dt, fieldsWidth);
        
            if (dt.Rows.Count > 0)
            {
                EipDataGridModel eipDataGridModel = new EipDataGridModel { rows = dt, total = Convert.ToInt32(details.Tables["Count"].Rows[0]["Counts"].ToString()),sum= SumStr(details, sumPara), page = page, columns = columns };
                return JsonConvert.SerializeObject(eipDataGridModel, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            }
            else
                return EipListJsonEmpty();
        }
        /// <summary>
        /// 返回求和字段
        /// </summary>
        /// <param name="details">明细包含总数的DataSet</param>
        /// <param name="sumPara">求和字段 aaa:中文</param>
        /// <returns></returns>
        private string SumStr(DataSet details, string sumPara)
        {

            DataRow dr = details.Tables["Count"].Rows[0];
             string sumStr = "";
            if (!string.IsNullOrEmpty(sumPara))
            {
                string[] fieldsWidthArray = sumPara.Split(',');
                sumStr += " {";
                int i = 0;
                foreach (var item in fieldsWidthArray)
                {
                    // fieldsWidthDic.Add(item.Split(':')[0], item.Split(':')[1]);


                   if(i==0)
                        sumStr +="\""+ item + "\":\"" + dr[item]+"\"";
                   else
                        sumStr += ",\"" + item + "\":\"" + dr[item] + "\"";
                    i++;
                }
                sumStr += " }";
            }
            return sumStr;
            //if (dt.Rows.Count > 0)
            //{
            //    EipDataGridModel eipDataGridModel = new EipDataGridModel { rows = dt, total = Convert.ToInt32(details.Tables["Count"].Rows[0]["Counts"].ToString()), page = page, columns = columns };
            //    return JsonConvert.SerializeObject(eipDataGridModel, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            //}
            //else
            //    return EipListJsonEmpty();
        }




        /// <summary>
        /// 获取eipDataGridColumnsList
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="fieldsWidth"></param>
        /// <returns></returns>
        private   List<EipDataGridModelColumns> eipDataGridColumnsList(DataTable detail, string fieldsWidth)
        {
            //detail明细或者 fieldsWidth 为空返回null
            if (detail.Rows.Count == 0 || fieldsWidth.Length == 0) return null;
            //字段宽度
            Dictionary<string, string> fieldsWidthDic = new Dictionary<string, string>();
            string[] fieldsWidthArray = fieldsWidth.Split(',');
            foreach (var item in fieldsWidthArray)
            {
                fieldsWidthDic.Add(item.Split(':')[0], item.Split(':')[1]);
            }

            int i = 0;
            List<EipDataGridModelColumns> EipDataGridModelColumnsList = new List<EipDataGridModelColumns>();
            foreach (DataColumn dc in detail.Columns)
            {
                EipDataGridModelColumns eipDataGridModelColumns = new EipDataGridModelColumns();
                eipDataGridModelColumns.column = dc.ColumnName;
                eipDataGridModelColumns.type = dc.DataType.ToString();
                if (fieldsWidthDic.ContainsKey(dc.ColumnName))
                    eipDataGridModelColumns.width = fieldsWidthDic[dc.ColumnName].ToString();
                else
                    eipDataGridModelColumns.width = fieldsWidthDic["default"];
                eipDataGridModelColumns.sort = i;
                eipDataGridModelColumns.osort = i;
                EipDataGridModelColumnsList.Add(eipDataGridModelColumns);
                i++;
            }

        
            return EipDataGridModelColumnsList;
        }


        public   string ReportPost(ReportRequestDataModel requestDataModel,string postUrl)
        {
          
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(postUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Proxy = null;
            SetWebRequest(request);
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestDataModel));
            WriteRequestData(request, data);
            WebResponse cc = request.GetResponse();
            StreamReader sr = new StreamReader(cc.GetResponseStream(), Encoding.UTF8);
            string retStr = sr.ReadToEnd();
            sr.Close();
 
            return retStr;

            //return ReadResponse(request.GetResponse());
        }

        /// <summary>
        /// 设置超时时间和进程同步或异步
        /// </summary>
        /// <param name="request"></param>
        private   void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 60000;
        }
        /// <summary>
        /// POST请求参数
        /// </summary>
        /// <param name="request"></param>
        /// <param name="data"></param>
        private   void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
        }

 


        public void ExportExecl(DataTable dt,string fileName)
        {
            //ReportRequestDataModel requestDataModel = SetModel();
            // DataTable dt = OpenDataHelper.SqlToDataTable(requestDataModel.SqlName, requestDataModel.Sql, requestDataModel.SqlParameter, requestDataModel.ExecDbName, requestDataModel.OrderByPara);

            NPOIExcelHelper NPOIExcelHelper = new NPOIExcelHelper();
            using (MemoryStream cc = NPOIExcelHelper.DataTableToExcel(dt))
            {
                HttpContext.Current.Response.Expires = 0;
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.Charset = "utf-8";
                HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName));
                HttpContext.Current.Response.BinaryWrite(cc.ToArray());
                HttpContext.Current.Response.End();
            }
            dt.Dispose();
        }
    }
}
