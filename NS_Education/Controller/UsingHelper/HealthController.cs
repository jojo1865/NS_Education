using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Controller.Health.Ping;
using NS_Education.Tools.ControllerTools.BaseClass;

namespace NS_Education.Controller.UsingHelper
{
    public class HealthController : PublicClass
    {
        private const string DbConnectionFail = "DB 連線失敗！";

        [HttpGet]
        public async Task<string> Ping()
        {
            // 檢查 DB
            try
            {
                int test = await DC.Database.SqlQuery<int>("SELECT 1").FirstAsync();
                if (test != 1)
                {
                    AddError(DbConnectionFail);
                }
            }
            catch (Exception e)
            {
                AddError(DbConnectionFail);
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