namespace NS_Education.Variables
{
    public enum StaticCodeSafetyControlCode
    {
        PasswordMinLength = 1,
        PasswordChangeDailyLimit = 2,
        PasswordNoReuseCount = 3,
        PasswordExpireDays = 4,
        SuspendIfLoginFailTooMuch = 5,
        WarnChangePasswordInDays = 6,
        IdleSecondsBeforeScreenSaver = 7,
        EnforceOneSessionPerUser = 8,
        NewSessionTerminatesOld = 9,
        PersistSecurityControlErrors = 10,
        UserLogKeepDays = 11,
        LoginFailLimit = 12
    }
}