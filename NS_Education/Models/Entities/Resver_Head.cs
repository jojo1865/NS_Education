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
    
    public partial class Resver_Head
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Resver_Head()
        {
            this.M_Resver_TimeSpan = new HashSet<M_Resver_TimeSpan>();
            this.Resver_Bill = new HashSet<Resver_Bill>();
            this.Resver_GiveBack = new HashSet<Resver_GiveBack>();
            this.Resver_Head_Log = new HashSet<Resver_Head_Log>();
            this.Resver_Other = new HashSet<Resver_Other>();
            this.Resver_Questionnaire = new HashSet<Resver_Questionnaire>();
            this.Resver_Site = new HashSet<Resver_Site>();
        }
    
        public int RHID { get; set; }
        public string Code { get; set; }
        public System.DateTime SDate { get; set; }
        public System.DateTime EDate { get; set; }
        public int BSCID11 { get; set; }
        public string Title { get; set; }
        public int CID { get; set; }
        public string CustomerTitle { get; set; }
        public string ContactName { get; set; }
        public int PeopleCt { get; set; }
        public string Note { get; set; }
        public int MK_BUID { get; set; }
        public string MK_Phone { get; set; }
        public int OP_BUID { get; set; }
        public string OP_Phone { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public bool CheckFlag { get; set; }
        public bool CheckInFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public System.DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public System.DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
        public int State { get; set; }
        public string MKT { get; set; }
        public string Owner { get; set; }
        public string ParkingNote { get; set; }
    
        public virtual B_StaticCode B_StaticCode { get; set; }
        public virtual BusinessUser BusinessUser { get; set; }
        public virtual BusinessUser BusinessUser1 { get; set; }
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<M_Resver_TimeSpan> M_Resver_TimeSpan { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_Bill> Resver_Bill { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_GiveBack> Resver_GiveBack { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_Head_Log> Resver_Head_Log { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_Other> Resver_Other { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_Questionnaire> Resver_Questionnaire { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
    }
}
