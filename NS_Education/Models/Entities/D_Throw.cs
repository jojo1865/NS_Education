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
    
    public partial class D_Throw
    {
        public int DTID { get; set; }
        public int BOCID { get; set; }
        public int BSCID { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int FixedPrice { get; set; }
        public string Remark { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public System.DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public System.DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
    
        public virtual B_OrderCode B_OrderCode { get; set; }
        public virtual B_StaticCode B_StaticCode { get; set; }
    }
}