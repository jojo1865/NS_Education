namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表的輸出物件。
    /// </summary>
    public class Report12_Output_APIItem : CommonResponseForPagedList<Report12_Output_Row_APIItem>
    {
        /// <summary>
        /// 製表者 ID
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// 製表者名稱
        /// </summary>
        public string Username { get; set; }

        public int Year { get; set; }
        public int? JanHours { get; set; }
        public int? FebHours { get; set; }
        public int? MarHours { get; set; }
        public int? AprHours { get; set; }
        public int? MayHours { get; set; }
        public int? JunHours { get; set; }
        public int? JulHours { get; set; }
        public int? AugHours { get; set; }
        public int? SepHours { get; set; }
        public int? OctHours { get; set; }
        public int? NovHours { get; set; }
        public int? DecHours { get; set; }

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report12_Input_APIItem r12)
            {
                Year = r12.Year;
                JanHours = r12.JanHours;
                FebHours = r12.FebHours;
                MarHours = r12.MarHours;
                AprHours = r12.AprHours;
                MayHours = r12.MayHours;
                JunHours = r12.JunHours;
                JulHours = r12.JulHours;
                AugHours = r12.AugHours;
                SepHours = r12.SepHours;
                OctHours = r12.OctHours;
                NovHours = r12.NovHours;
                DecHours = r12.DecHours;
            }

            base.SetByInput(input);
        }
    }
}