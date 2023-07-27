namespace NS_Education.Variables
{
    /// <summary>
    /// 預約修改歷史的類型
    /// </summary>
    public enum ResverHistoryType
    {
        /// <summary>
        /// 建立草稿
        /// </summary>
        DraftCreated = 1,

        /// <summary>
        /// 確認預約
        /// </summary>
        Checked,

        /// <summary>
        /// 報到作業
        /// </summary>
        CheckedIn,

        /// <summary>
        /// 結帳作業
        /// </summary>
        FullyPaid,

        /// <summary>
        /// 預約中止
        /// </summary>
        Terminated
    }
}