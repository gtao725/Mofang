using System;
using System.Collections.Generic;
using System.Linq;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class ApiBusiness : IBusiness
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        //public IEnumerable<BusinessObjectHeadModel> BusinessDetailGet(int objectHeadId)
        //{

        //    var sql = from a in idal.IBusinessObjectHeadDAL.SelectAll()
        //              join b in idal.IBusinessObjectDetailDAL.SelectAll()
        //              on a.Id equals b.HeadId
        //              where a.Id== objectHeadId
        //              select new BusinessObjectHeadModel
        //              {
        //                  Id = a.Id,
        //                  BusinessName = a.BusinessName,
        //                  ListBusinessObjectDetail= BusinessObjectDetailModelGet(a.Id)
        //              };

        //    var aaa = sql.ToList();


        // // var result = from c in findallchildren(objectHeadId) select c;
        //    return sql;
        //}


        //public List<BusinessObjectDetailModel> BusinessObjectDetailModelGet(int headId) {
        //    var sql = from a in idal.IBusinessObjectDetailDAL.SelectAll()
        //              where a.HeadId== headId
        //              select new BusinessObjectDetailModel
        //              {
        //Id=a.Id,
        //HeadId=a.HeadId,
        //DetailSeq=a.DetailSeq,
        //InParaObjectId = BusinessObjectsEModelGet(a.InParaObjectId),
        //InExecObjectId = BusinessObjectsEModelGet(a.InExecObjectId),
        //FormName=a.FormName,
        //BeforeOpenExecObjectId= BusinessObjectsEModelGet(a.BeforeOpenExecObjectId),
        //NextOpenFormName=a.NextOpenFormName
        //              };

        //    return sql.ToList();
        //}


        //public BusinessFlowDetailList GetFlowDetail(int FlowId)
        //{
        //    return BusinessFlowDetailModelGet(FlowId);
        //}

        public BusinessFlowDetailList GetFlowDetail(int FlowHeadId, string FlowDetailType)
        {

            if (FlowHeadId != 0)
            {
                //IEnumerable<BusinessFlowDetailList> list = from a in idal.IBusinessFlowGroupDAL.SelectAll()
                //                                            where a.Id == groupId
                //                                            select new BusinessFlowDetailList
                //                                            {
                //                                                Id = a.Id,
                //                                                FlowName=a.FlowName
                //                                            };
                IEnumerable<BusinessFlowDetailList> list = from a in idal.IFlowDetailDAL.SelectAll()
                                                           where a.FlowHeadId == FlowHeadId &&a.Type== FlowDetailType&&a.BusinessObjectGroupId!=0
                                                           select new BusinessFlowDetailList
                                                           {
                                                               Id = a.BusinessObjectGroupId
                                                           };
                if (list.Count() > 0)
                {
                    BusinessFlowDetailList BusinessFlowDetail = list.First();
                    BusinessFlowDetail.ListBusinessObjectDetail = BusinessGroupDetailGet(BusinessFlowDetail.Id);
                    return BusinessFlowDetail;
                }
                else
                    return null;
            }
            else
                return null;


        }
        public BusinessFlowDetailList GetFlowDetailAPP(int FlowHeadId, string FlowDetailType)
        {

            if (FlowHeadId != 0)
            {
                //IEnumerable<BusinessFlowDetailList> list = from a in idal.IBusinessFlowGroupDAL.SelectAll()
                //                                            where a.Id == groupId
                //                                            select new BusinessFlowDetailList
                //                                            {
                //                                                Id = a.Id,
                //                                                FlowName=a.FlowName
                //                                            };
                IEnumerable<BusinessFlowDetailList> list = from a in idal.IFlowDetailDAL.SelectAll()
                                                           where a.FlowHeadId == FlowHeadId && a.Type == FlowDetailType && a.BusinessObjectGroupId != 0
                                                           select new BusinessFlowDetailList
                                                           {
                                                               Id = a.BusinessObjectGroupId
                                                           };
                if (list.Count() > 0)
                {
                    BusinessFlowDetailList BusinessFlowDetail = list.First();
                    BusinessFlowDetail.ListBusinessObjectDetail = BusinessGroupDetailGetAPP(BusinessFlowDetail.Id);
                    return BusinessFlowDetail;
                }
                else
                    return null;
            }
            else
                return null;


        }
        public List<BusinessObjectDetailModel> BusinessGroupDetailGet(int? groupId)
        {
            IEnumerable<BusinessObjectDetailModel> sql = from a in idal.IBusinessObjectDetailDAL.SelectAll()
                                                         join b in idal.IBusinessFlowHeadDAL.SelectAll()
                                                         on a.HeadId equals b.ObjectHeadId
                                                         where b.GroupId== groupId
                                                         select new BusinessObjectDetailModel
                                                         {
                                                             Id = a.Id,
                                                             HeadId = a.HeadId,
                                                             DetailSeq =b.OrderId+a.DetailSeq,
                                                             InParaObjectId = a.InParaObjectId,
                                                             InExecObjectId = a.InExecObjectId,
                                                             FormName = a.FormName,
                                                             BeforeOpenExecObjectId = a.BeforeOpenExecObjectId,
                                                             NextOpenFormName = a.NextOpenFormName,
                                                             ExecFlag="Y"
                                                         };
            List<BusinessObjectDetailModel> list = new List<BusinessObjectDetailModel>();
            foreach (BusinessObjectDetailModel obj in sql)
            {
                BusinessObjectDetailModel ccc = obj;
                ccc.InParaObject = BusinessObjectsModelGet(ccc.InParaObjectId);
                ccc.InExecObject = BusinessObjectsModelGet(ccc.InExecObjectId);
                ccc.BeforeOpenExecObject = BusinessObjectsModelGet(ccc.BeforeOpenExecObjectId);
                list.Add(ccc);
            }
            return list;

        }

        public List<BusinessObjectDetailModel> BusinessGroupDetailGetAPP(int? groupId)
        {
            IEnumerable<BusinessObjectDetailModel> sql = from a in idal.IBusinessObjectDetailDAL.SelectAll()
                                                         join b in idal.IBusinessFlowHeadDAL.SelectAll()
                                                         on a.HeadId equals b.ObjectHeadId
                                                         where b.GroupId == groupId
                                                         select new BusinessObjectDetailModel
                                                         {
                                                             Id = a.Id,
                                                             HeadId = a.HeadId,
                                                             DetailSeq = b.OrderId + a.DetailSeq,
                                                             InParaObjectId = a.InParaObjectId,
                                                             InExecObjectId = a.InExecObjectId,
                                                             FormName = a.FormNameApp,
                                                             BeforeOpenExecObjectId = a.BeforeOpenExecObjectId,
                                                             NextOpenFormName = a.NextOpenFormName,
                                                             ExecFlag = "Y"
                                                         };
 
            return sql.ToList();

        }


        public BusinessObjectHeadModel BusinessObjectHeadModelGet(int? businessHeadId) {

            if (businessHeadId != null)
            {
                IEnumerable<BusinessObjectHeadModel> list = from a in idal.IBusinessObjectHeadDAL.SelectAll()
                                                         where a.Id == businessHeadId
                                                         select new BusinessObjectHeadModel
                                                         {
                                                             Id = a.Id,
                                                             BusinessName = a.BusinessName
                                                            
                                                         };
                if (list.Count() > 0)
                {
                    BusinessObjectHeadModel businessObjectHead = list.First();
                    businessObjectHead.ListBusinessObjectDetail = BusinessObjectDetailModelGet(businessHeadId);
                    return businessObjectHead;
                }
                else
                    return null;
            }
            else
                return null;


        }


        public List<BusinessObjectDetailModel> BusinessObjectDetailModelGet(int? businessHeadId)
        {
            IEnumerable<BusinessObjectDetailModel> sql = from a in idal.IBusinessObjectDetailDAL.SelectAll()
                                                        where a.HeadId== businessHeadId
                                                        select new BusinessObjectDetailModel
                                                        {
                                                            Id = a.Id,
                                                            HeadId = a.HeadId,
                                                            DetailSeq = a.DetailSeq,
                                                            InParaObjectId= a.InParaObjectId,
                                                            InExecObjectId= a.InExecObjectId,
                                                            FormName = a.FormName,
                                                            BeforeOpenExecObjectId =a.BeforeOpenExecObjectId,
                                                            NextOpenFormName = a.NextOpenFormName,
                                                            ExecFlag = "Y"
                                                        };
            List<BusinessObjectDetailModel> list = new List<BusinessObjectDetailModel>();
            foreach (BusinessObjectDetailModel obj in sql)
            {
                BusinessObjectDetailModel ccc = obj;
                ccc.InParaObject = BusinessObjectsModelGet(ccc.InParaObjectId);
                ccc.InExecObject = BusinessObjectsModelGet(ccc.InExecObjectId);
                ccc.BeforeOpenExecObject = BusinessObjectsModelGet(ccc.BeforeOpenExecObjectId);
                list.Add(ccc);
            }
            return list;

        }


        public BusinessObjectsModel BusinessObjectsModelGet(int? objectId)
        {
            if (objectId != null)
            {
                IEnumerable <BusinessObjectsModel> list = from a in idal.IBusinessObjectDAL.SelectAll()
                                                          where a.Id == objectId
                                                          select new BusinessObjectsModel
                                                          {
                                                              Id = a.Id,
                                                              ObjectName = a.ObjectName,
                                                              ObjectType = a.ObjectType,
                                                              ObjectValue=a.ObjectValue,
                                                              ObjectDes = a.ObjectDes
                                                          };
                if (list.Count() > 0)
                {
                    BusinessObjectsModel businessObjects = list.First();
                    businessObjects.ListBusinessObjectItems = BusinessObjectItemsModelGet(objectId);
                    return businessObjects;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public List<BusinessObjectItemsModel> BusinessObjectItemsModelGet(int? objectId)
        {
            IEnumerable<BusinessObjectItemsModel> sql= from a in idal.IBusinessObjectItemDAL.SelectAll()
                                                          where a.ObjectId == objectId
                                                          select new BusinessObjectItemsModel
                                                          {
                                                              Id = a.Id,
                                                              ObjectId = a.ObjectId,
                                                              MustAttributeName = a.MustAttributeName,
                                                              MustAttributeNameCN = a.MustAttributeNameCN,
                                                              ParaObjectId = a.ParaObjectId
                                                          };
            List<BusinessObjectItemsModel> list = new List<BusinessObjectItemsModel>();
            
            foreach (BusinessObjectItemsModel obj in sql)
            {
                BusinessObjectItemsModel ccc =obj;
                ccc.ParaObject = BusinessObjectsModelGet(ccc.ParaObjectId);
                list.Add(ccc);
            }
            return list;

        }


        //public List<BusinessObjectItem> findallchildren(int objectId)
        //{

        //    var list = (from c in idal.IBusinessObjectItemDAL.SelectAll()
        //                where c.ObjectId == objectId
        //                select c).ToList();
        //    List<BusinessObjectItem> tmpList = new List<BusinessObjectItem>(list);
        //    foreach (BusinessObjectItem single in tmpList)
        //    {
        //        List<BusinessObjectItem> tmpChildren = findallchildren(single.ObjectId);
        //        if (tmpChildren.Count != 0)
        //        {
        //            list.AddRange(tmpChildren);
        //        }
        //    }
        //    return list;
        //}

    }
}
