using System;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class ResverHistoryTypeExtension
    {
        public static string GetChineseName(this ResverHistoryType? type)
        {
            switch (type)
            {
                case ResverHistoryType.DraftCreated:
                    return "建立草稿";
                case ResverHistoryType.Checked:
                    return "確認預約";
                case ResverHistoryType.CheckedIn:
                    return "報到作業";
                case ResverHistoryType.FullyPaid:
                    return "結帳作業";
                case ResverHistoryType.Terminated:
                    return "預約中止";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}