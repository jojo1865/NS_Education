using System.Collections.Generic;

namespace NS_Education.Models.APIItems.CustomerGift.GetInfoById
{
    public class CustomerGift_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
    {
        public int CGID { get; set; }
        
        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }

        public ICollection<BaseResponseRowForSelectable> C_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public int Year { get; set; }
        public string SendDate { get; set; }
        
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        
        public ICollection<BaseResponseRowForSelectable> BSC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public string Title { get; set; }
        public int Ct { get; set; }
        public string Note { get; set; }       
    }
}