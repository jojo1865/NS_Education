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
    
    public partial class D_Zip
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public D_Zip()
        {
            this.M_Address = new HashSet<M_Address>();
        }
    
        public int DZID { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public System.DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public System.DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<M_Address> M_Address { get; set; }
    }
}
