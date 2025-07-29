using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class UnitsResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string UnitName { get; set; }
        public string ItemNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int Proportion { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class UnitsSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string AltItemNumber { get; set; }
        public int? ClientId { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }
}
