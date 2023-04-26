namespace NS_Education.Models.APIItems.CustomerVisit.GetList
{
    public class CustomerVisit_GetList_Output_Row_APIItem : BaseResponseWithCreUpd<Entities.CustomerVisit>
    {
        public int CVID { get; set; }
        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        
        public int BUID { get; set; }
        public string BU_Name { get; set; }
            
        public string TargetTitle { get; set; }
        public string Title { get; set; }
        
        public string VisitDate { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }
    }
}