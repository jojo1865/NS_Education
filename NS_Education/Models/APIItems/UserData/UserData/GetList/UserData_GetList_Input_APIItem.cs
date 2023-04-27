namespace NS_Education.Models.APIItems.UserData.UserData.GetList
{
    public class UserData_GetList_Input_APIItem : BaseRequestForList
    {
        /// <summary>
        /// 查詢關鍵字。可以用使用者名稱查詢。忽略或空白時不以此做篩選。
        /// </summary>
        public string Keyword { get; set; }
        /// <summary>
        /// 公司 ID。忽略或 0 時不以此做篩選。
        /// </summary>
        public int DCID { get; set; }
        /// <summary>
        /// 部門 ID。忽略或 0 時不以此做篩選。
        /// </summary>
        public int DDID { get; set; }
    }
}