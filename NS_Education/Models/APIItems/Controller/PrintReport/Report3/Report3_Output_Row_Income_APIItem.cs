namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    public class Report3_Output_Row_Income_APIItem
    {
        public string Title { get; set; }

        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public int UnitPrice { get; set; }
        public int Difference => QuotedPrice - UnitPrice;
    }
}