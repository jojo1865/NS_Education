namespace NS_Education.Models.APIItems.Controller.Resver.GetHistory
{
    public class Resver_GetHistory_Input_APIItem : BaseRequestForList
    {
        /// <summary>
        /// 預約單號
        /// </summary>
        public int? RHID { get; set; }
    }
}