using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Hall
{
    public class D_Hall_List
    {
        public D_Hall_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<D_Hall_APIItem> Items { get; set; }
    }
    public class D_Hall_APIItem
    {
        public int DDID { get; set; }
        public int DHID { get; set; }
        public string DD_TitleC { get; set; }
        public string DD_TitleE { get; set; }
        public List<cSelectItem> DepartmentList { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public bool DiscountFlag { get; set; }
        public bool CheckoutNowFlag { get; set; }
        public bool PrintCheckFlag { get; set; }
        public bool Invoice3Flag { get; set; }
        public int CheckType { get; set; }
        public double BusinessTaxRate { get; set; }
        public int DeviceCt { get; set; }
        public int SiteCt { get; set; }
        public int PartnerItemCt { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}