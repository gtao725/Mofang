using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhClientSearch : BaseSearch
    {
        public int? Id { get; set; }
        public string ClientName { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public int ClientId { get; set; }
        public string ClientCodeOrderBy { get; set; }
    }

    public class WhClientResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public int? ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string Description { get; set; }
        public decimal? WarnCBM { get; set; }
        public string ClientType { get; set; }
        public string NightTime { get; set; }
        public string ContractName { get; set; }
        public string ContractNameOut { get; set; }
        public string Passageway { get; set; }
        public int? ReleaseRule { get; set; }
        public string ReleaseRuleShow { get; set; }
    }

    public class WhClientTypeSearch : BaseSearch
    {
        public string ClientType { get; set; }
        public string WhCode { get; set; }
    }

    public class WhClientExtendResult
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public int? InvClearUpSkuMaxQty { get; set; }
        public int? NotOnlySkuPutawayQty { get; set; }
        public string RegularExpression { get; set; }

    }

}
