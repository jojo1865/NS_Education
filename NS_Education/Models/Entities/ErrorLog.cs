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
    
    public partial class ErrorLog
    {
        public int MID { get; set; }
        public string JWT { get; set; }
        public string RequestUrl { get; set; }
        public string Payload { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionTrace { get; set; }
        public System.DateTime CreDate { get; set; }
    }
}
