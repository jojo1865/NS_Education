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
    
    public partial class Resver_Throw_Food
    {
        public int RTFID { get; set; }
        public int RTID { get; set; }
        public int DFCID { get; set; }
        public int BSCID { get; set; }
        public int BPID { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
    
        public virtual B_Partner B_Partner { get; set; }
        public virtual B_StaticCode B_StaticCode { get; set; }
        public virtual D_FoodCategory D_FoodCategory { get; set; }
        public virtual Resver_Throw Resver_Throw { get; set; }
    }
}
