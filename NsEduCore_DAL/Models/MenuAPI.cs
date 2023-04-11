using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class MenuAPI
    {
        public int SeqNo { get; set; }
        public int MDID { get; set; }
        public string APIURL { get; set; }
        public int APIType { get; set; }
        public string Note { get; set; }
        public DateTime CreDate { get; set; }

        public virtual MenuData MD { get; set; }
    }
}
