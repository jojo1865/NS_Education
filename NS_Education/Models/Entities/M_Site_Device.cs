//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace NS_Education.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class M_Site_Device
    {
        public int MID { get; set; }
        public int BSID { get; set; }
        public int BDID { get; set; }
        public int Ct { get; set; }
        public System.DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public System.DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
    
        public virtual B_Device B_Device { get; set; }
        public virtual B_SiteData B_SiteData { get; set; }
    }
}
