﻿using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class BusinessUser
    {
        public BusinessUser()
        {
            CustomerVisit = new HashSet<CustomerVisit>();
        }

        public int BUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool MKsalesFlag { get; set; }
        public bool OPsalesFlag { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<CustomerVisit> CustomerVisit { get; set; }
    }
}
