using System;
using System.Collections.Generic;
using NsEduCore_DAL.Models;

namespace NsEduCore_DAL.Domains
{
    public class Company
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public string Code { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
        
        internal D_Company Data { get; set; }
        internal ICollection<D_Department> Departments { get; set; }
    }
}