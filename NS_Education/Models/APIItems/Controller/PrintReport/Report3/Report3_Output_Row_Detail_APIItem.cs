namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    public class Report3_Output_Row_Detail_APIItem
    {
        public string TypeName { get; set; }
        public string Date { get; set; }
        public string[] TimeSpans { get; set; }
        public string Title { get; set; }
        public string SubTypeName { get; set; }
        public string SubType { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
    }
}