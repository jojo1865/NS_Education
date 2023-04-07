using System.Collections.Generic;
using NS_Education.Tools;

namespace NS_Education.Models
{
    public class UserData_Login_Output_APIItem : cReturnMessageInfusableAbstract
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public List<User_Privilege_Output_APIItem> Privileges { get; set; } = new List<User_Privilege_Output_APIItem>();
    }
}