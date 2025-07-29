using System.Web.Http;
using WMS.WebApi.Models;
using WMS.WebApi.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using WMS.IBLL;
using WMS.BLL;
using WMS.BLLClass;
using System.Linq;
using MODEL_MSSQL;

namespace WMS.WebApi.Controllers
{
    public class TestController : ApiController
    {
        Helper Helper = new Helper();


        [HttpGet]
        public object RecTest()
        {

            IRecWinceManager aa = new RecWinceManager();

            List<UserPriceT> userPriceT = aa.UserPriceT();
            List<UserPrice> userPrice = new List<UserPrice>();
            foreach (UserPriceT item in userPriceT)
            {
                userPrice.Add(new UserPrice {Name=item.Name,Qty=item.Qty,Price=item.Price,Rank=item.Rank,itemType=item.itemType });
            }

            List<UserShenT> userShenT = aa.UserShenT();
            List<UserShen> userShen = new List<UserShen>();
            foreach (UserShenT item in userShenT)
            {
                userShen.Add(new UserShen { User = item.User, Amt = item.Amt });

            }
            //List<UserPrice> userPrice = new List<UserPrice>();
            //userPrice.Add(new UserPrice { Name = "A8", Qty = 31, Price = 849, Rank = 1 });
            //userPrice.Add(new UserPrice { Name = "Reno2 (128G)", Qty = 7, Price = 2049, Rank = 1, itemType = 1 });
            //userPrice.Add(new UserPrice { Name = "A11（256G）", Qty = 47, Price = 1099, Rank = 1, itemType = 1 });
            //userPrice.Add(new UserPrice { Name = "Reno3 8+128G", Qty = 2, Price = 2459, Rank = 1, itemType = 1 });
            //userPrice.Add(new UserPrice { Name = "Reno3 12+128G", Qty = 1, Price = 2619, Rank = 1, itemType = 1 });
            //userPrice.Add(new UserPrice { Name = "Reno3 Pro 8+128G", Qty = 1, Price = 2929, Rank = 1, itemType = 1 });
            //userPrice.Add(new UserPrice { Name = "已包装真无线耳机T103", Qty = 29, Price = 370, Rank = 9999, itemType = 0 });
            //userPrice.Add(new UserPrice { Name = "电源适配器VC54", Qty = 9, Price = 43, Rank = 9999, itemType = 0 });
            //userPrice.Add(new UserPrice { Name = "闪充数据线DL109", Qty = 29, Price = 6, Rank = 9999, itemType = 0 });
            //userPrice.Add(new UserPrice { Name = "已包装耳机135", Qty = 6, Price = 22, Rank = 9999, itemType = 0 });
            //userPrice.Add(new UserPrice { Name = "伞", Qty = 5, Price = 14, Rank = 9999, itemType = 0 });


            //List<UserShen> userShen = new List<UserShen>();
            //userShen.Add(new UserShen { User = "杜可立", Amt = 11770 });
            //userShen.Add(new UserShen { User = "方玉屏", Amt = 4347 });
            //userShen.Add(new UserShen { User = "冯桂芳", Amt = 12997 });
            //userShen.Add(new UserShen { User = "冯俊", Amt = 1140 });
            //userShen.Add(new UserShen { User = "洪道根", Amt = 3007 });
            //userShen.Add(new UserShen { User = "凌新华", Amt = 7355 });
            //userShen.Add(new UserShen { User = "潘小峰", Amt = 6000 });
            //userShen.Add(new UserShen { User = "汪银河", Amt = 9941 });
            //userShen.Add(new UserShen { User = "吴凤娟", Amt = 7494 });
            //userShen.Add(new UserShen { User = "吴建武", Amt = 16987 });
            //userShen.Add(new UserShen { User = "吴景平", Amt = 2000 });
            //userShen.Add(new UserShen { User = "张少梅", Amt = 5000 });
            //userShen.Add(new UserShen { User = "张有辉", Amt = 11637 });
            //userShen.Add(new UserShen { User = "郑美娟", Amt = 6656 });
            //userShen.Add(new UserShen { User = "周秀芳", Amt = 7943 });


            userShen= userShen.OrderBy(t => t.Amt).ToList();

          
            //List<UserShen> queList = new List<UserShen>();
            //queList = this.RandomSortList<UserShen>(userShen);
            //userShen = queList;

            for (int i = 0; i < userShen.Count(); i++)
            {
                int Amt = userShen[i].Amt;
                List<UserPrice> userPriceDo = new List<UserPrice>();


                userPriceDo = userPrice.Where(u => u.Price <= Amt && u.Qty > 0).ToList();

                //// var sql= from  a in userPriceDo
                //foreach (UserPrice item in userPriceDo.Where(u=>u.itemType==1))
                //{
                //    item.Rank = Amt % item.Price;
                //}
                //foreach (UserPrice item in userPriceDo.Where(u => u.itemType == 0))
                //{
                //    item.Rank = item.Rank % item.Price;
                //}


                userPriceDo = userPriceDo.OrderByDescending(t => t.itemType).ThenBy(t => t.Rank).ThenByDescending(t => t.Price).ToList();


                for (int j = 0; j < userPriceDo.Count(); j++)
                {
                    if (Amt == 0)
                        break;


                    // userPriceDo = userPriceDo.Where(u => u.Price <= Amt ).OrderByDescending(t => t.Price).ToList();

                    int doQty = 0;
                    for (int k = 0; k < userPriceDo[j].Qty; k++)
                    {
                        if (Amt >= userPriceDo[j].Price)
                        {
                            Amt = Amt - userPriceDo[j].Price;
                            doQty++;
                            // userShen[i].Name = userShen[i].Name + userPriceDo[j].Name+",";
                            //userShen[i].UserPrice.Add(new UserPrice { Name = userShen[i].Name, Price= userPriceDo[j].Price, Qty = 1 });
                            if (userShen[i].UserPrice == null)
                                userShen[i].UserPrice = new List<UserPrice>();
                            userShen[i].UserPrice.Add(new UserPrice { Name = userPriceDo[j].Name, Price = userPriceDo[j].Price, Qty = 1 });

                            userShen[i].Qty++;
                        }
                        else
                            break;
                    }
                    userPriceDo[j].Qty = userPriceDo[j].Qty - doQty;
                }
                userShen[i].Amt = Amt;

            }

            

            userPrice = userPrice.Where(u => u.Qty > 0).ToList();

           var  userShen1 = userShen.Where(u => u.Amt <=0).OrderByDescending(u => u.Amt).ToList();
            userShen = userShen.Where(u => u.Amt > 0).OrderByDescending(u=>u.Amt).ToList();


            int AmtList = userShen.Sum(u => u.Amt);
            for (int i = 0; i < userPrice.Count(); i++)
            {
                for (int j = 0; j < userPrice[i].Qty; j++)
                {
   
                      userShen[j].UserPrice.Add(new UserPrice { Name = userPrice[i].Name, Price = userPrice[i].Price, Qty = 1 });
                      userShen[j].Amt = userShen[j].Amt - userPrice[i].Price;
 

                }
            }



           // AmtList = userShen.Sum(u => u.Amt);
            return AmtList;
 
        }

        public List<T> RandomSortList<T>(List<T> ListT)
        {
            Random random = new Random();
            List<T> newList = new List<T>();
            foreach (T item in ListT)
            {
                newList.Insert(random.Next(newList.Count + 1), item);
            }
            return newList;
        }

        //List<UserPrice> DoUserPrice(List<UserPrice> list,int Amt ) {
        //     for (int i = 0; i < list.Count; i++)
        //     {
        //         if((float)Amt / (float)list[i].Price> list[i].Qty)
        //             list[i].QtyDo = list[i].Qty;
        //         else
        //             list[i].QtyDo = (float)Amt / (float)list[i].Price;

        //         list[i].QtyMod=  Amt %list[i].Price;
        //     }
        //     return list.Where(u=>u.QtyDo>=1).ToList();
        // }

        //[HttpGet, HttpPost]
        //public object List1(RecModel test1)
        //{

        //    // string JSONData = "{Id:123,ReceiptId:'EI123',ItemNumber:'ITEM123',RecModeldetail:[{title:'这是一个标题',body:'what'},{title:'这是一个标题1',body:'what1'}]}";

        //// string aa = @"{""Status"":""200"",""Data"":{""Statu"":""11"",""Msg"":""测试111"",""ResultData"":{""id"":123,""ReceiptId"":null,""RecModeldetail"":[{""title"":""这是一个标题"",""body"":""what""},{""title"":""这是一个标题1"",""body"":""what1""}]},""ResultObjectName"":""RecModel""},""ErrorMessage"":null}";

        ////string JSONData = "{Id:123,ReceiptId:'EI123'}";


        //// RecModel apiResultModel = Converter.Deserialize<RecModel>(JSONData, SerializeOption.Property);
        //    RecDetailModel recDeatail1 = new RecDetailModel();
        //    recDeatail1.body = "body1";
        //    recDeatail1.title = "title中文啊啊啊";
        //    // recDeatail1.user = new User("name1", "password1");

        //    RecDetailModel recDeatail2 = new RecDetailModel();
        //    recDeatail2.body = "body2";
        //    recDeatail2.title = "title2";

        //    List<RecDetailModel> recDeatailList = new List<RecDetailModel>();
        //    recDeatailList.Add(recDeatail1);
        //    recDeatailList.Add(recDeatail2);

        //    // string aa = "";

        //    //RecModel persons = Newtonsoft.Json.JsonConvert.DeserializeObject<RecModel>(JSONData);

        //    return Helper.ResultData("Y", "aaa", new { });
        //}

        [HttpGet, HttpPost]
        public object List([FromUri]int aa)
        {
            //IBLL.IWhMenuService menu = new BLL.WhMenuService();
            // menu.Select(1);
            ApiBusiness iaa = new ApiBusiness();

            BusinessObjectHeadModel aaa = iaa.BusinessObjectHeadModelGet(aa);


            // foreach (Object obj in aaa)
            //{
            //    if (obj is BLLClass.BusinessObjectHeadModel)//这个是类型的判断，这里Student是一个类或结构
            //    {
            //        BLLClass.BusinessObjectHeadModel s = (BLLClass.BusinessObjectHeadModel)obj;
            //        Console.WriteLine(s.BusinessName);
            //    }
            //    if (obj is int)
            //    {
            //        Console.WriteLine("INT:{0}", obj);
            //    }
            //}

            // IWhMenuService menu = new BLL.WhMenuService();

            return Helper.ResultData("Y", "aaa", aaa);
        }


        [HttpGet, HttpPost]
        public object RecIfComplete(ApiRequestDataModel aa)
        {
            //ApiRequestDataModel aa = new ApiRequestDataModel();
            //aa.recModel.ReceiptId = "aaa";
            string formName = "RecPltIn";
            if (aa.recModel != null)
            {
                if (aa.recModel.ReceiptId == "EI002311")
                    formName = "RecSkuIn";
                else
                    formName = "RecPltIn";
            }

            // return "AAA";
            return Helper.ResultData("Y", null, new OpenFormModel { });
        }




    }
    public class UserPrice
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
        public float Rank { get; set; }
        public int itemType { get; set; }
        //public float Priority { get; set; }
        //public int QtyMod { get; set; }
        //public int QtyRes{ get; set; }


    }
    public class UserShen
    {
        public string User { get; set; }
        public int Amt { get; set; }
        // public string Name { get; set; }
        public int Qty { get; set; }

        public List<UserPrice> UserPrice { get; set; }


    }


}
