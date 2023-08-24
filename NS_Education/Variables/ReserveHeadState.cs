namespace NS_Education.Variables
{
    public static class ReserveHeadState
    {
        public const string Draft = "1";
        public const string DepositPaid = "2";
        public const string FullyPaid = "3";
        public const string Terminated = "4";
    }

    public enum ReserveHeadGetListState
    {
        Draft = 1,
        Checked,
        CheckedIn,
        FullyPaid,
        Terminated,
        Deleted
    }
}