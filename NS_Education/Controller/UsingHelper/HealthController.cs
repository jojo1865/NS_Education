using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.Health.Ping;
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

            Health_Ping_Output_APIItem response = new Health_Ping_Output_APIItem
            {
                Pong = "Pong!"
            };

            return GetResponseJson(response);
        }
    }
}