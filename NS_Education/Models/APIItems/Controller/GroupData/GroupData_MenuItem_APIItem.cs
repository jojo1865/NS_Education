namespace NS_Education.Models.APIItems.Controller.GroupData
{
    public class GroupData_MenuItem_APIItem
    {
        public int MDID { get; set; }
        public string Title { get; set; }
        public bool AddFlag { get; set; }
        public bool ShowFlag { get; set; }
        public bool EditFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public bool PrintFlag { get; set; }
        public bool AddFlagReadOnly { get; set; }
        public bool ShowFlagReadOnly { get; set; }
        public bool EditFlagReadOnly { get; set; }
        public bool DeleteFlagReadOnly { get; set; }
        public bool PrintFlagReadOnly { get; set; }
    }
}