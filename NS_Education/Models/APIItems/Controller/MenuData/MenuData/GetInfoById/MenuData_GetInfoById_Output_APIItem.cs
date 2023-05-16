namespace NS_Education.Models.APIItems.Controller.MenuData.MenuData.GetInfoById
{
    public class MenuData_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int MDID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int SortNo { get; set; }
    }
}