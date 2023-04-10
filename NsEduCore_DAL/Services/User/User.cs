using System;
using NsEduCore_DAL.Models;

namespace NsEduCore_DAL.Services.User
{
    public class User
    {
        public int UID { get; set; }
        public string UserName { get; set; }
        public string LoginAccount { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
        public DateTime LoginDate { get; set; }
        
        internal UserData UserData { get; set; }
    }
}