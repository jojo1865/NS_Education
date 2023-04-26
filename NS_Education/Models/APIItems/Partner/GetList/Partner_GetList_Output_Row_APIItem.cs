using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.Partner.GetList
{
    public class Partner_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<B_Partner>
    {
        public int BPID { get; set; }
        
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }
        
        public string Code { get; set; }
        public string Title { get; set; }
        public string Compilation { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }

        public string Email { get; set; }
        public string Note { get; set; }
        public bool CleanFlag { get; set; }
        public int CleanPrice { get; set; }
        public string CleanSDate { get; set; }
        public string CleanEDate { get; set; }
    }
}