namespace NsEduCore.Responses.Company
{
    /// <summary>
    /// 取得單筆公司資料的傳入物件。
    /// </summary>
    public class CompanyGetRequest
    {
        /// <summary>
        /// 欲取得的公司 ID。
        /// </summary>
        public int Id { get; set; }
    }
}