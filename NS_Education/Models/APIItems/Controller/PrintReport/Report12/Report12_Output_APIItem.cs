using System.Collections.Generic;

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

        /// <summary>
        /// （可選）年月區間（起）。YYYY/MM
        /// </summary>
        public string StartYearMonth { get; set; }

        /// <summary>
        /// （可選）年月區間（迄）。YYYY/MM
        /// </summary>
        public string EndYearMonth { get; set; }

        /// <summary>
        /// 每個月的時段數。
        /// </summary>
        public int[] MonthHours { get; set; }

        /// <summary>
        /// （可選）場地類別
        /// </summary>
        public string BC_Title { get; set; }

        /// <summary>
        /// （可選）場地名稱（支援模糊搜尋）
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// （可選）允許顯示內部單位。未給值時，視為 true。
        /// </summary>
        public bool? ShowInternal { get; set; }

        /// <summary>
        /// （可選）允許顯示外部單位。未給值時，視為 true。
        /// </summary>
        public bool? ShowExternal { get; set; }

        /// <summary>
        /// （可選）允許顯示通訊處。未給值時，視為 true。
        /// </summary>
        public bool? ShowCommDept { get; set; }

        /// <summary>
        /// 資料行的集合
        /// </summary>
        public ICollection<Report12_Output_Row_APIItem> Rows => Items;

        public override void SetByInput(BaseRequestForPagedList input)
        {
            if (input is Report12_Input_APIItem r12)
            {
                StartYearMonth = r12.StartYearMonth;
                EndYearMonth = r12.EndYearMonth;
                MonthHours = r12.MonthHours;
                BC_Title = r12.BC_Title;
                SiteName = r12.SiteName;
                ShowInternal = r12.ShowInternal;
                ShowExternal = r12.ShowExternal;
                ShowCommDept = r12.ShowCommDept;
            }

            base.SetByInput(input);
        }
    }
}