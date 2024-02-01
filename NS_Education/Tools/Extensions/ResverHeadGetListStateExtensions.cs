using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class ResverHeadGetListStateExtensions
    {
        public static string GetChineseName(this ReserveHeadGetListState? state)
        {
            switch (state)
            {
                case ReserveHeadGetListState.Draft:
                    return "草稿";
                case ReserveHeadGetListState.Checked:
                    return "已預約";
                case ReserveHeadGetListState.CheckedIn:
                    return "已報到";
                case ReserveHeadGetListState.FullyPaid:
                    return "已結帳";
                case ReserveHeadGetListState.Terminated:
                    return "已中止";
                case ReserveHeadGetListState.Deleted:
                    return "刪除";
                default:
                    return "未知";
            }
        }
    }
}