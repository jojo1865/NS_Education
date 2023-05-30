namespace NS_Education.Models.APIItems.Controller.SafetyControl.Submit
{
    public class SafetyControl_Submit_Input_APIItem
    {
        public int PasswordMinLength { get; set; }
        public int PasswordChangeDailyLimit { get; set; }
        public int PasswordNoReuseCount { get; set; }
        public int PasswordExpireDays { get; set; }
        public bool SuspendIfLoginFailTooMuch { get; set; }
        public int WarnChangePasswordInDays { get; set; }
        public int IdleSecondsBeforeScreenSaver { get; set; }
        public bool EnforceOneSessionPerUser { get; set; }
        public bool NewSessionTerminatesOld { get; set; }
        public bool PersistSecurityControlErrors { get; set; }
        public int LoginFailLimit { get; set; }
    }
}