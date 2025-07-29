
using MODEL_MSSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class RecController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet, HttpPost]
        public object RecCreate(ReceiptInsert rec)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);

            foreach (RecModeldetail item in rec.RecModeldetail)
            {
                if (item.LotDate != null)
                {
                    DateTime aaa = Convert.ToDateTime(item.LotDate);
                    if (aaa <= Convert.ToDateTime("1900-01-01"))
                    {
                        item.LotDate = null;
                    }
                }
            }

            IRecManager aa = new RecManager();
            string res = aa.ReceiptInsert(rec);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });
        }

        [HttpGet, HttpPost]
        public object ReturnRecCreate(ReceiptInsert rec)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);

            foreach (RecModeldetail item in rec.RecModeldetail)
            {
                if (item.LotDate != null)
                {
                    DateTime aaa = Convert.ToDateTime(item.LotDate);
                    if (aaa <= Convert.ToDateTime("1900-01-01"))
                    {
                        item.LotDate = null;
                    }
                }
            }

            IInterceptManager aa = new InterceptManager();
            string res = aa.ReceiptInsertByOther(rec);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet, HttpPost]
        public object RecCreateSkuBatch(ReceiptInsert rec, [FromUri] string eFlag)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);

            foreach (RecModeldetail item in rec.RecModeldetail)
            {

                if (item.LotDate != null)
                {
                    DateTime aaa = Convert.ToDateTime(item.LotDate);
                    if (aaa <= Convert.ToDateTime("1900-01-01"))
                    {
                        item.LotDate = null;
                    }
                }
            }

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecCreateSkuBatch(rec, eFlag);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet, HttpPost]
        public object ReceiptInsert(ReceiptInsert rec)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);
            foreach (RecModeldetail item in rec.RecModeldetail)
            {
                if (item.LotDate != null)
                {
                    DateTime aaa = Convert.ToDateTime(item.LotDate);
                    if (aaa <= Convert.ToDateTime("1900-01-01"))
                    {
                        item.LotDate = null;
                    }
                }
            }
            IRecManager aa = new RecManager();
            string res = aa.ReceiptByOutOrderIntercept(rec, 0);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });
        }


        [HttpGet, HttpPost]
        public object RecCreateFast(List<ReceiptInsert> rec)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);

            foreach (ReceiptInsert item0 in rec)
            {
                foreach (RecModeldetail item in item0.RecModeldetail)
                {


                    if (item.LotDate != null)
                    {
                        DateTime aaa = Convert.ToDateTime(item.LotDate);
                        if (aaa <= Convert.ToDateTime("1900-01-01"))
                        {
                            item.LotDate = null;
                        }
                    }
                }
            }
            IRecManager aa = new RecManager();
            string res = aa.ReceiptInsert(rec);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });
        }

        [HttpGet]
        public object RecConsumerCreate([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int RecLossId, [FromUri]int Qty, [FromUri] string userName)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecConsumerCreate(WhCode, ReceiptId, RecLossId, Qty, userName);

            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });


        }
        [HttpGet]
        public object RecConsumerDelete([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri] string userName)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecConsumerDelete(ReceiptId, WhCode, userName);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", "无已选择耗材", new { });

        }
        [HttpGet]
        public object RecReMarkIn([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri] string userName, [FromUri] string recRemark)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecReMarkIn(ReceiptId, WhCode, userName, recRemark);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });

            }
            else
                return Helper.ResultData("N", "无已选择耗材", new { });

        }
        [HttpGet]
        public object RecEIAssign([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri] string userName)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecEIAssign(ReceiptId, WhCode, userName);
            if (res == "Y")
            {
                return Helper.ResultData("Y", ReceiptId + "开始卸货成功", new { });

            }
            else
                return Helper.ResultData("N", res, new { });

        }



        [HttpGet]
        public object GetRecConsumerGoodsModelList([FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            List<RecConsumerGoodsModel> res = aa.GetRecConsumerGoodsModelList(WhCode);

            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "未维护耗材", new { });
        }
        [HttpGet]
        public object GetRecConsumerGoodsModelList([FromUri]string ReceiptId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            List<RecConsumerGoodsModel> res = aa.GetRecConsumerGoodsModelList(ReceiptId, WhCode);

            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "无已选择耗材", new { });
        }


        [HttpGet]
        public object GetRecDes([FromUri]string ReceiptId, [FromUri]string WhCode)
        {
            //string jsonstr = "{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161117153510003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"SO1611171\",\"CustomerPoNumber\":\"PO1\",\"PoId\":0,\"HuId\":\"L03\",\"Location\":null,\"LotFlag\":0,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":[{\"AltItemNumber\":\"SKU1\",\"ItemId\":1,\"UnitId\":0,\"UnitName\":\"LP\",\"Qty\":1,\"Length\":1,\"Width\":2,\"Height\":3,\"Weight\":0,\"LotNumber1\":null,\"LotNumber2\":null,\"LotDate\":null,\"SerialNumberInModel\":[{\"CartonId\":\"1\"}]}],\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]}";
            //ReceiptInsert rec = JsonConvert.DeserializeObject<ReceiptInsert>(jsonstr);
            IRecWinceManager aa = new RecWinceManager();
            string res = aa.GetRecDes(ReceiptId, WhCode);

            return Helper.ResultData("Y", res, new { });

        }



        [HttpGet]
        public object GetReceiptRec([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string RecType)
        {

            IRecWinceManager aa = new RecWinceManager();
            ReceiptInsert res = aa.GetReceipt(ReceiptId, WhCode);

            if (res != null)
            {
                if (res.Status == "N" || res.Status == "C")
                    return Helper.ResultData("N", "已完成收货", new { });

                bool dsFlag = aa.ReceiptDSCheck(ReceiptId, WhCode, RecType);
                string err = "";
                //直装收货判断
                if (RecType == "1")
                    err = "非直装收货批次!";
                else
                    err = "直装收货批次!";
                if (!dsFlag)
                    return Helper.ResultData("N", err, new { });

                return Helper.ResultData("Y", "", res);
            }
            else
                return Helper.ResultData("N", "收货批次错误", new { });
        }
        [HttpGet]
        public object GetReceiptRec([FromUri]string ReceiptId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            ReceiptInsert res = aa.GetReceipt(ReceiptId, WhCode);

            if (res != null)
            {
                return Helper.ResultData("Y", res.Remark, new { });
            }
            else
                return Helper.ResultData("N", "收货批次错误", new { });
        }
        [HttpGet]
        public object GetReceiptRecFast([FromUri]string ReceiptId, [FromUri]string WhCode)
        {
            IRecWinceManager aa = new RecWinceManager();
            ReceiptInsert res = aa.GetReceipt(ReceiptId, WhCode);
            if (res != null)
            {
                if (res.Status == "N" || res.Status == "C")
                    return Helper.ResultData("N", "已完成收货", new { });
                string so = aa.ReceiptFastCheck(ReceiptId, WhCode);

                if (so == "N")
                    return Helper.ResultData("N", "非快捷收货批次!", new { });
                else
                {

                    //定制快捷收货流程
                    //res.ProcessId = 31;
                    res.SoNumber = so;
                    return Helper.ResultData("Y", "", res);
                }
            }
            else
                return Helper.ResultData("N", "收货批次错误", new { });
        }



        [HttpGet]
        public object GetRecPoFast([FromUri]string ReceiptId, [FromUri]string SoNumber, [FromUri]string CustomerPoNumber, [FromUri]string WhCode)
        {
            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckSoPo(ReceiptId, WhCode, SoNumber, CustomerPoNumber))
            {
                List<RecSkuDataCe> res = aa.GetRecSkuDataCeList(ReceiptId, SoNumber, CustomerPoNumber, WhCode);
                return Helper.ResultData("Y", "", res);
            }
            else
                return Helper.ResultData("N", "箱号不存在!", new { });
        }

        [HttpGet]
        public object GetRecSo([FromUri]string ReceiptId, [FromUri]string SoNumber, [FromUri]string WhCode)
        {
            IRecWinceManager aa = new RecWinceManager();

            List<RecSoModel> res = aa.GetRecSoList(WhCode, ReceiptId, SoNumber);

            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "SO数据错误", new { });



        }

        [HttpGet]
        public object GetRecReturnOrder([FromUri]string RecReturnOrderNumber, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            List<string> res = aa.GetRecReturnOrder(RecReturnOrderNumber, WhCode);

            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "退单无对应的收货批次", new { });
        }
        [HttpGet]
        public object GetHoldMasterListByRec([FromUri]int ClientId, [FromUri]string WhCode)
        {

            IRecManager aa = new RecManager();
            int total = 0;
            HoldMasterSearch recSearch = new HoldMasterSearch();
            recSearch.ClientId = ClientId;
            recSearch.WhCode = WhCode;
            List<HoldMasterModel> resList = new List<HoldMasterModel>();
            List<HoldMaster> res = aa.HoldMasterListByRec(recSearch, out total);
            int count = res.Where(u => u.ClientCode != "all").Count();
            //有自己的异常原因时,只显示自己的
            if (count > 0)
                res = res.Where(u => u.ClientId != 0).ToList();
            foreach (var item in res)
            {
                HoldMasterModel hm = new HoldMasterModel();
                hm.HoldId = item.Id;
                hm.HoldReason = item.HoldReason;
                resList.Add(hm);
            }
            if (res != null)
                return Helper.ResultData("Y", "", resList);
            else
                return Helper.ResultData("N", "原因异常", new { });
        }




        [HttpPost]
        public object CheckSoPo(ApiRequestDataModel res)
        {

            //string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI160906154110004\",\"ClientId\":2,\"ClientCode\":\"test\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"so2\",\"CustomerPoNumber\":\"so2\",\"PoId\":0,\"HuId\":null,\"Location\":null,\"LotFlag\":0,\"CreateUser\":null,\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]},\"pallatesModel\":null}";
            //ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);


            IRecWinceManager aa = new RecWinceManager();


            if (aa.CheckSoPo(res.recModel.ReceiptId, res.recModel.WhCode, res.recModel.SoNumber, res.recModel.CustomerPoNumber))
                return Helper.ResultData("Y", "", new { });
            //return Helper.ResultData("Y", "", new OpenFormModel { formName = "RecEIn" });
            else
                return Helper.ResultData("N", "SO或PO不存在!", new { });
        }


        [HttpGet]
        public object GetRecLotFlag()
        {

            IRecWinceManager aa = new RecWinceManager();

            List<RecLotFlagDescription> lotFlagList = aa.GetRecLotFlag();
            if (lotFlagList.Count > 0)
                return Helper.ResultData("Y", "", lotFlagList);
            else
                return Helper.ResultData("N", "数据异常", new { });
        }

        [HttpPost, HttpGet]
        public object CheckLocation(ApiRequestDataModel res)
        {

            // string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI161118152910003\",\"ClientId\":1,\"ClientCode\":\"TEST\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":null,\"CustomerPoNumber\":null,\"PoId\":0,\"HuId\":null,\"Location\":\"A01\",\"LotFlag\":1,\"CreateUser\":\"1012\",\"ProcessId\":1,\"TransportType\":null,\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":null},\"pallatesModel\":null}";
            // ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);
            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckRecLocation(res.recModel.WhCode, res.recModel.Location, res.recModel.ReceiptId))
                return Helper.ResultData("Y", "", new { });
            //return Helper.ResultData("Y", "", new OpenFormModel { formName = "RecEIn" });
            else
                return Helper.ResultData("N", "收货区错误!", new { });
        }
        [HttpPost, HttpGet]
        public object CheckReturnLocation(string Location, string WhCode)
        {


            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckReturnLocation(WhCode, Location))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "退货区错误!", new { });
        }
        [HttpPost]
        public object CheckPlt(ApiRequestDataModel res)
        {

            //string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI160906154110004\",\"ClientId\":2,\"ClientCode\":\"test\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"so2\",\"CustomerPoNumber\":\"so2\",\"PoId\":0,\"HuId\":null,\"Location\":null,\"LotFlag\":0,\"CreateUser\":null,\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]},\"pallatesModel\":null}";
            //ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);


            IRecWinceManager aa = new RecWinceManager();


            if (aa.CheckPlt(res.recModel.WhCode, res.recModel.HuId))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "托盘不可用或不存在!", new { });
        }
        [HttpGet]
        public object CheckPlt([FromUri]string HuId, [FromUri]string WhCode)
        {

            //string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI160906154110004\",\"ClientId\":2,\"ClientCode\":\"test\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"so2\",\"CustomerPoNumber\":\"so2\",\"PoId\":0,\"HuId\":null,\"Location\":null,\"LotFlag\":0,\"CreateUser\":null,\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]},\"pallatesModel\":null}";
            //ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);


            IRecWinceManager aa = new RecWinceManager();


            if (aa.CheckPlt(WhCode, HuId))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "托盘不可用或不存在!", new { });
        }
        [HttpGet]
        public object RecScanRemainedPlt([FromUri]string HuId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            List<HuDetailRemained> res = aa.RecScanRemainedPlt(WhCode, HuId).Where(u => u.Qty > u.SerialNumberModel.Count()).ToList();

            if (res != null)
            {
                if (res.Count() > 0)
                    return Helper.ResultData("Y", "", res);
                else
                    return Helper.ResultData("N", "托盘已扫描完成!", new { });
            }
            else
                return Helper.ResultData("N", "托盘不存在", new { });
        }

        [HttpGet]
        public object RecScanUPC([FromUri]string HuId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            List<HuDetailRemained> res = aa.RecScanUPCPlt(WhCode, HuId).Where(u => u.Qty > u.SerialNumberModel.Count()).ToList();

            if (res != null)
            {
                if (res.Count() > 0)
                    return Helper.ResultData("Y", "", res);
                else
                    return Helper.ResultData("N", "托盘已扫描完成!", new { });
            }
            else
                return Helper.ResultData("N", "托盘不存在", new { });
        }

        [HttpPost]
        public object RecScanCheck(HuDetailRemained huDetailRemained)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecScanCheck(huDetailRemained);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "验证通过", new { });
            }
            else
            {
                return Helper.ResultData("N", res, new { });
            }
        }

        [HttpPost]
        public object RecUPCCheck(HuDetailRemained huDetailRemained)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecUPCCheck(huDetailRemained);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "验证通过", new { });
            }
            else
            {
                return Helper.ResultData("N", res, new { });
            }
        }

        [HttpPost]
        public object RecScanCheck(List<SerialNumberInModel> serialNumberInModelList, [FromUri]int checkPartFlag, [FromUri]string ClientCode, [FromUri]string SoNumber, [FromUri]string CustomerPoNumber, [FromUri]string AltItemNumber, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecScanCheck(serialNumberInModelList, checkPartFlag, ClientCode, SoNumber, CustomerPoNumber, AltItemNumber, WhCode);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "验证通过", new { });
            }
            else
            {

                List<string> recList = new List<string>();

                foreach (var item in res.Split(';'))
                {
                    recList.Add(item);
                }


                return Helper.ResultData("N", res, recList);
            }
        }


        [HttpPost]
        public object RecScanRemainedComplete(HuDetailRemained huDetailRemained)
        {
            //string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI160906154110004\",\"ClientId\":2,\"ClientCode\":\"test\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"so2\",\"CustomerPoNumber\":\"so2\",\"PoId\":0,\"HuId\":null,\"Location\":null,\"LotFlag\":0,\"CreateUser\":null,\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]},\"pallatesModel\":null}";
            //ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecScanRemainedComplete(huDetailRemained);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });
            }
            else
            {
                List<string> recList = new List<string>();

                foreach (var item in res.Split(';'))
                {
                    recList.Add(item.Split('在')[0]);
                }

                return Helper.ResultData("N", res, recList);
            }
        }
        [HttpPost]
        public object RecUPCComplete(HuDetailRemained huDetailRemained)
        {
            //string jsonstr = "{\"recModel\":{\"WhCode\":\"01\",\"RegId\":0,\"ReceiptId\":\"EI160906154110004\",\"ClientId\":2,\"ClientCode\":\"test\",\"ReceiptDate\":\"01/01/0001 00:00:00\",\"Status\":\"U\",\"SoNumber\":\"so2\",\"CustomerPoNumber\":\"so2\",\"PoId\":0,\"HuId\":null,\"Location\":null,\"LotFlag\":0,\"CreateUser\":null,\"ProcessId\":1,\"TransportType\":\"箱式车\",\"CreateDate\":\"01/01/0001 00:00:00\",\"RecModeldetail\":null,\"HoldMasterModel\":null,\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"123\"}]},\"pallatesModel\":null}";
            //ApiRequestDataModel res = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonstr);

            IRecWinceManager aa = new RecWinceManager();
            string res = aa.RecUPCComplete(huDetailRemained);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "保存成功", new { });
            }
            else
            {
                List<string> recList = new List<string>();

                foreach (var item in res.Split(';'))
                {
                    recList.Add(item.Split('在')[0]);
                }

                return Helper.ResultData("N", res, recList);
            }
        }


        [HttpGet]
        public object CheckSku([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string ItemNumber, [FromUri]string CustomerPoNumber)
        {

            IRecWinceManager aa = new RecWinceManager();
            string ItemIdStr = aa.RecItemNumberToId(ItemNumber, WhCode, ReceiptId, CustomerPoNumber, null);
            if (ItemIdStr.Split('$')[0] == "Y")
            {
                int ItemId = Convert.ToInt32(ItemIdStr.Split('$')[1]);
                if (ItemId != 0)
                {
                    if (aa.CheckSku(ReceiptId, WhCode, ItemId, CustomerPoNumber))
                    {
                        RecSkuDataCe recSkuDataCe = new RecSkuDataCe();
                        recSkuDataCe.ItemId = ItemId;
                        recSkuDataCe.AltItemNumber = aa.GetSkuAltItemNumber(ItemId);

                        RecSkuDataCe recSkuOut = aa.GetSkuRegCBMWeight(ReceiptId, WhCode, ItemId, null, CustomerPoNumber);
                        recSkuDataCe.LotDate = recSkuOut.LotDate;
                        recSkuDataCe.Weight = recSkuOut.Weight;

                        return Helper.ResultData("Y", aa.GetSkuAltItemNumber(ItemId), recSkuDataCe);
                    }
                    else
                        return Helper.ResultData("N", "SKU不在收货批次的SO中!", new { });
                }
                else
                    return Helper.ResultData("N", "SKU不存在!", new { });
            }
            return Helper.ResultData("N", ItemIdStr.Split('$')[1], new { });

        }

        [HttpGet]
        public object CheckSku([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string ItemNumber, [FromUri]string CustomerPoNumber, [FromUri]string SoNumber)
        {

            IRecWinceManager aa = new RecWinceManager();
            string ItemIdStr = aa.RecItemNumberToId(ItemNumber, WhCode, ReceiptId, CustomerPoNumber, SoNumber);


            if (ItemIdStr.Split('$')[0] == "Y")
            {
                int ItemId = Convert.ToInt32(ItemIdStr.Split('$')[1]);
                if (ItemId != 0)
                {
                    if (aa.CheckSku(ReceiptId, WhCode, ItemId, CustomerPoNumber))
                    {
                        RecSkuDataCe recSkuDataCe = aa.GetRecSkuDataCe(ItemId);
                        recSkuDataCe.ItemId = ItemId;
                        recSkuDataCe.AltItemNumber = aa.GetSkuAltItemNumber(ItemId);

                        RecSkuDataCe recSkuDataCeDefault = aa.GetSkuRegCBMWeight(ReceiptId, WhCode, ItemId, SoNumber, CustomerPoNumber);
                        recSkuDataCe.LotDate = recSkuDataCeDefault.LotDate;
                        recSkuDataCe.LotNumber1 = recSkuDataCeDefault.LotNumber1;

                        return Helper.ResultData("Y", aa.GetSkuAltItemNumber(ItemId), recSkuDataCe);
                    }
                    else
                        return Helper.ResultData("N", "SKU不在收货批次的SO中!", new { });
                }
                else
                    return Helper.ResultData("N", "SKU不存在!", new { });
            }
            return Helper.ResultData("N", ItemIdStr.Split('$')[1], new { });
        }

        [HttpGet]
        public object GetUnit([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int ItemId)
        {

            IRecWinceManager aa = new RecWinceManager();

            List<RecSkuUnit> cc = aa.GetUnit(ReceiptId, WhCode, ItemId).Distinct().ToList();
            if (cc.ToList().Count() > 0)
                return Helper.ResultData("Y", "", cc);
            else
                return Helper.ResultData("N", "没有单位！数据异常！", cc);

        }
        [HttpGet]
        public object GetUnitChange([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int ItemId)
        {

            IRecWinceManager aa = new RecWinceManager();

            List<RecSkuUnit> cc = aa.GetUnitChange(ReceiptId, WhCode, ItemId).Distinct().OrderBy(u => u.UnitFlag).ToList();
            if (cc.ToList().Count() > 0)
                return Helper.ResultData("Y", "", cc);
            else
                return Helper.ResultData("N", "没有单位！数据异常！", cc);

        }
        [HttpGet]
        public object GetRecSkuData([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int ItemId, [FromUri]string SoNumber, [FromUri]string CustomerPoNumber)
        {

            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckSku(ReceiptId, WhCode, ItemId, CustomerPoNumber))
            {
                RecSkuDataCe cc = aa.GetRecSkuDataCe(ItemId);
                List<int> ItemIdArry = new List<int>();
                ItemIdArry.Add(ItemId);
                cc.RecQty = aa.GetSkuRecQty(ReceiptId, WhCode, ItemIdArry, SoNumber, CustomerPoNumber);
                cc.RegQty = aa.GetSkuRegQty(ReceiptId, WhCode, ItemIdArry, SoNumber, CustomerPoNumber);
                return Helper.ResultData("Y", "", cc);
            }
            else
                return Helper.ResultData("N", "SKU不存在!", new { });


        }

        [HttpGet]
        public object GetRecSkuData([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string ItemNumber, [FromUri]string SoNumber, [FromUri]string CustomerPoNumber)
        {

            IRecWinceManager aa = new RecWinceManager();

            List<int> ItemIdList = aa.RecItemNumberToIds(ItemNumber, WhCode, ReceiptId, CustomerPoNumber, SoNumber);
            RecSkuDataCe cc = new RecSkuDataCe();
            cc.AltItemNumber = ItemNumber;
            cc.WhCode = WhCode;
            cc.RecQty = aa.GetSkuRecQty(ReceiptId, WhCode, ItemIdList, SoNumber, CustomerPoNumber);
            cc.RegQty = aa.GetSkuRegQty(ReceiptId, WhCode, ItemIdList, SoNumber, CustomerPoNumber);
            return Helper.ResultData("Y", "", cc);



        }
        [HttpGet]
        public object GetSkuRegCBMWeight([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int ItemId, [FromUri]string SoNumber, [FromUri]string CustomerPoNumber)
        {

            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckSku(ReceiptId, WhCode, ItemId, CustomerPoNumber))
            {
                RecSkuDataCe skuRegCBMWeight = aa.GetSkuRegCBMWeight(ReceiptId, WhCode, ItemId, SoNumber, CustomerPoNumber);


                return Helper.ResultData("Y", "", skuRegCBMWeight);
            }
            else
                return Helper.ResultData("N", "SKU不存在!", new { });


        }
        [HttpGet]
        public object CheckRecCBMPercent([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]int Percent)
        {
            IRecWinceManager aa = new RecWinceManager();
            string res = aa.CheckRecCBMPercent(ReceiptId, WhCode, Percent);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object CheckRecCBMPercent([FromUri]string ReceiptId, [FromUri]string WhCode)
        {
            IRecWinceManager aa = new RecWinceManager();
            string res = aa.CheckRecCBMPercent(ReceiptId, WhCode);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }

        [HttpGet]
        public object CheckRecComplete([FromUri]string ReceiptId, [FromUri]string WhCode)
        {
            IRecManager aa = new RecManager();
            string res = aa.CheckRecComplete(ReceiptId, WhCode);
            //if (res == "Y")
            return Helper.ResultData(res, res, new { });
            //else
            //    return Helper.ResultData("N", res, new { });
        }

        [HttpGet]
        public object PartRecComplete([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string CreateUser)
        {
            IRecManager aa = new RecManager();
            string res = aa.PartRecComplete(ReceiptId, WhCode, CreateUser);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object RecComplete([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string CreateUser)
        {
            IRecManager aa = new RecManager();
            string res = aa.RecComplete(ReceiptId, WhCode, CreateUser);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet, HttpPost]
        public object RecCheckOverWeight(List<ReceiptInsert> recList)
        {


            IRecManager aa = new RecManager();
            string res = aa.RecCheckOverWeightList(recList);
            if (res.Split('$')[0] == "Y")
            {
                return Helper.ResultData("Y", "验证成功", new { });

            }
            else
                return Helper.ResultData("N", res.Split('$')[1], new { });
        }



        [HttpGet]
        public object PauseRec([FromUri]string ReceiptId, [FromUri]string WhCode, [FromUri]string CreateUser)
        {
            IRecManager aa = new RecManager();
            string res = aa.PauseRec(ReceiptId, WhCode, CreateUser);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpPost]
        public object DcReturnExceptionIn(DcReturnExceptionIn dcReturnExceptionIn)
        {
            IRecWinceManager aa = new RecWinceManager();
            string res = aa.DcReturnExceptionInsert(dcReturnExceptionIn);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object EANGetItem([FromUri]string WhCode, [FromUri]string EAN)
        {
            IRecWinceManager aa = new RecWinceManager();
            string res = aa.EANGetItem(WhCode, EAN);
            if (res.Split('$')[0] == "Y")
                return Helper.ResultData("Y", res.Split('$')[1] + "$" + res.Split('$')[2], new { });
            else
                return Helper.ResultData("N", res.Split('$')[1], new { });
        }

        [HttpPost]
        public object TCRRecPhoto(PhotoMasterApiSearch PhotoMasterApiIn)
        {
            RootManager aa = new RootManager();
            int total = 0;
            List<PhotoMasterResult> resList = aa.TCRRecPhotoMasterList(PhotoMasterApiIn, out total);
            int count = resList.Count();
            if (count > 0)
                return Helper.ResultData("Y", "", resList);
            else
                return Helper.ResultData("N", "", new { });
        }

        [HttpPost]
        public object TCROutPhoto(PhotoMasterApiSearch PhotoMasterApiIn)
        {
            RootManager aa = new RootManager();
            int total = 0;
            List<PhotoMasterResult> resList = aa.TCROutPhotoMasterList(PhotoMasterApiIn, out total);
            int count = resList.Count();
            if (count > 0)
                return Helper.ResultData("Y", "", resList);
            else
                return Helper.ResultData("N", "", new { });
        }

        [HttpPost]
        //照片上传
        public object UploadPhoto()
        {

            HttpFileCollection filelist = HttpContext.Current.Request.Files;

            UploadPhotoApi picInfo = new UploadPhotoApi();

            picInfo.userId = HttpContext.Current.Request["userId"];
            picInfo.id = int.Parse(HttpContext.Current.Request["id"]);
            picInfo.whCode = HttpContext.Current.Request["whCode"];
            picInfo.fileId = HttpContext.Current.Request["fileId"];
            picInfo.number = HttpContext.Current.Request["number"];
            picInfo.number2 = HttpContext.Current.Request["number2"];
            picInfo.numberType = HttpContext.Current.Request["numberType"];
            RootManager aa = new RootManager();
            string res = aa.uploadPhotoFile(filelist, picInfo);

            if (res == "Y")
                return Helper.ResultData("Y", "", "");
            else
                return Helper.ResultData("N", "", res);
        }


        [HttpGet]
        public object GetRecWorkloadAccountModelList([FromUri]string ReceiptId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();

            List<WorkloadAccountModelCN> workList = aa.GetRecWorkloadAccountModelList(ReceiptId, WhCode);
            if (workList.Count > 0)
                return Helper.ResultData("Y", "", workList);
            else
                return Helper.ResultData("N", "无工人信息！", new { });
        }

        [HttpGet]
        public object GetOutWorkloadAccountModelList([FromUri]string LoadId, [FromUri]string WhCode)
        {

            IRecWinceManager aa = new RecWinceManager();

            List<WorkloadAccountModelCN> workList = aa.GetOutWorkloadAccountModelList(LoadId, WhCode);
            if (workList.Count > 0)
                return Helper.ResultData("Y", "", workList);
            else
                return Helper.ResultData("N", "无工人信息！", new { });
        }

    }
}
