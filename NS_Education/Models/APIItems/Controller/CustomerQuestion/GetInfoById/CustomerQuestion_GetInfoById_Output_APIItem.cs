using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.CustomerQuestion.GetInfoById
{
    public class CustomerQuestion_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int CQID { get; set; }

        public int CID { get; set; }
        public string C_TitleC { get; set; }
        public string C_TitleE { get; set; }

        public ICollection<CommonResponseRowForSelectable> C_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string AskDate { get; set; }
        public string AskTitle { get; set; }
        public string AskArea { get; set; }
        public string AskDescription { get; set; }

        public bool ResponseFlag { get; set; }
        public string ResponseUser { get; set; }
        public string ResponseDescription { get; set; }
        public string ResponseDate { get; set; }
    }
}