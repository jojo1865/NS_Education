namespace NS_Education.Models.APIItems
{
    public class CommonRequestForUniqueNames
    {
        public string Keyword { get; set; }
        public int DeleteFlag { get; set; } = 0;
        public int MaxRow { get; set; } = 10;
    }
}