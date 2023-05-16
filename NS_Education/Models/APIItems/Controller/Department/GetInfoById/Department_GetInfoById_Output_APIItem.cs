namespace NS_Education.Models.APIItems.Controller.Department.GetInfoById
{
    public class Department_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DDID { get; set; }
        public int DCID { get; set; }
        public string DC_TitleC { get; set; }
        public string DC_TitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int HallCt { get; set; }
        public int PeopleCt { get; set; }
    }
}