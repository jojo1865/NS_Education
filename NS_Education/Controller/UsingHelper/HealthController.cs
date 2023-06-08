using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Controller.UsingHelper
{
    public class HealthController : PublicClass
    {
        [HttpGet]
        public async Task<string> Ping()
        {
            // 檢查 DB
            try
            {
                await DC.UserData.AnyAsync();
            }
            catch (Exception e)
            {
                AddError("DB 連線失敗！");
                AddError(e.Message);
            }

            return GetResponseJson();
        }
    }
}