﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NS_Education.Models;
using NS_Education.Models.Entities;
using NS_Education.Tools.Filters;
using NS_Education.Tools.Filters.FinalizeResponseFilter;

namespace NS_Education.Tools.ControllerTools.BaseClass
{
    [FinalizeResponseFilter(Order = Int32.MaxValue)]
    public class PublicClass : System.Web.Mvc.Controller
    {
        private readonly ICollection<string> _errors = new List<string>();
        protected readonly string[] CategoryTypes = { "通用", "公司", "部門", "場地", "備忘", "服務", "設備", "客戶", "付款類", "合作廠商" };

        /// <summary>
        /// 暫存 UID 處。ASP.NET 的 Controller 在每個 Action 都是獨特的 Instance，
        /// 因此會基於每次 Action 為單位做暫存。
        /// </summary>
        private int? _uid;

        public PublicClass()
        {
            DC = new NsDbContext();
        }

        protected internal NsDbContext DC { get; }

        public BaseApiResponse GetMsgClass(IEnumerable<string> errors)
        {
            BaseApiResponse response = new BaseApiResponse();
            if (errors == null || !errors.Any()) return response;

            response.SuccessFlag = false;
            response.Messages = _errors;
            return response;
        }

        public string GetResponseJson()
        {
            return ChangeJson(GetMsgClass(_errors));
        }

        public string GetResponseJson<T>(T infusable) where T : IReturnMessageInfusable
        {
            infusable.Infuse(GetMsgClass(_errors));
            return ChangeJson(infusable);
        }

        /// <summary>
        /// 清空 Error。
        /// </summary>
        protected void InitializeResponse()
        {
            _errors.Clear();
        }

        /// <summary>
        /// 加入錯誤訊息。
        /// </summary>
        /// <param name="errorMessage">錯誤訊息</param>
        protected internal void AddError(string errorMessage)
        {
            _errors.Add(errorMessage);
        }

        /// <summary>
        /// 回傳一串缺少權限時可用的預設錯誤字串。
        /// </summary>
        /// <returns>預設錯誤訊息字串</returns>
        protected internal string NoPrivilege()
        {
            return "權限不足以進行此操作！";
        }

        /// <summary>
        /// 回傳一串更新 DB 失敗時可用的預設錯誤字串。
        /// </summary>
        /// <param name="e">錯誤物件</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected static string UpdateDbFailed(Exception e = null)
        {
            return e is null ? "更新 DB 時失敗！" : $"更新 DB 時失敗：{e.Message}；Inner:{e.InnerException?.Message}！";
        }

        /// <summary>
        /// 回傳一串查無資料時可用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">（可選）欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string NotFound(string fieldName = null)
        {
            return fieldName is null ? "查無資料！" : $"「{fieldName}」查無對應資料！";
        }

        /// <summary>
        /// 回傳一串已存在同樣資料時的預設錯誤訊息字串。
        /// </summary>
        /// <param name="keyFieldName">（可選）用於判定重覆的欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string AlreadyExists(string keyFieldName = null)
        {
            return keyFieldName is null ? "已存在相同資料！" : $"已存在「{keyFieldName}」包含相同值的資料！";
        }

        /// <summary>
        /// 回傳一串發現重複資料時可用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="keyFieldName">（可選）用於判定重覆的子欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string CopyNotAllowed(string fieldName, string keyFieldName = null)
        {
            return keyFieldName is null ? $"「{fieldName}」不允許包含重複的資料！" : $"「{fieldName}」中不允許「{keyFieldName}」有任何重複資料！";
        }

        /// <summary>
        /// 回傳一串不支援輸入值時可用的預設錯誤字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string UnsupportedValue(string fieldName)
        {
            return $"{fieldName} 不支援這個輸入值，請重新確認。";
        }

        /// <summary>
        /// 回傳一串最小值大於最大值時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="minFieldName">最小值的欄位名稱</param>
        /// <param name="maxFieldName">最大值的欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected internal string MinLargerThanMax(string minFieldName, string maxFieldName)
        {
            return $"「{minFieldName}」必須小於等於「{maxFieldName}」！";
        }

        /// <summary>
        /// 回傳一串缺少欄位時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected internal string EmptyNotAllowed(string fieldName)
        {
            return $"「{fieldName}」未輸入或格式不正確！";
        }

        /// <summary>
        /// 回傳一串欄位格式錯誤時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string WrongFormat(string fieldName)
        {
            return $"「{fieldName}」格式不正確！";
        }

        /// <summary>
        /// 回傳一串超出範圍時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string OutOfRange(string fieldName, object min = null, object max = null)
        {
            string minMessage = min is null ? "" : $"，最小值為 {min}";
            string maxMessage = max is null ? "" : $"，最大值為 {max}";
            return $"「{fieldName}」超出範圍{minMessage}{maxMessage}！";
        }

        /// <summary>
        /// 回傳一串不符合預期值時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="targetValue">預期值</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string NotEqual(string fieldName, object targetValue)
        {
            return $"「{fieldName}」應等於 {targetValue}！";
        }

        /// <summary>
        /// 回傳一串輸入值過大時可使用的預設錯誤訊息駔懺。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="max">最大值</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string TooLarge(string fieldName, object max = null)
        {
            return max is null ? $"「{fieldName}」值太大了！" : $"「{fieldName}」不得超過 {max}！";
        }

        /// <summary>
        /// 回傳一串長度超出範圍時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string LengthOutOfRange(string fieldName, int? min = null, int? max = null)
        {
            string minMessage = min is null ? "" : $"，最小長度為 {min}";
            string maxMessage = max is null ? "" : $"，最大長度為 {max}";
            return $"「{fieldName}」長度不符合規定範圍{minMessage}{maxMessage}！";
        }

        /// <summary>
        /// 回傳一串欄位內容過短時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="length">（可選）長度</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string TooShort(string fieldName, int? length = null)
        {
            return length == null
                ? $"「{fieldName}」低於最小長度！"
                : $"「{fieldName}」不得低於 {length} 個字！";
        }

        /// <summary>
        /// 回傳一串欄位內容過長時可使用的預設錯誤訊息字串。
        /// </summary>
        /// <param name="fieldName">欄位名稱</param>
        /// <param name="length">（可選）長度</param>
        /// <returns>預設錯誤訊息字串</returns>
        protected string TooLong(string fieldName, int? length = null)
        {
            return length == null
                ? $"「{fieldName}」超過最大長度！"
                : $"「{fieldName}」不得超過 {length} 個字！";
        }

        /// <summary>
        /// 判定目前是否有錯誤訊息。
        /// </summary>
        /// <returns>true：有<br/>
        /// false：沒有
        /// </returns>
        public bool HasError()
        {
            return _errors?.Any() ?? false;
        }

        /// <summary>
        /// 取得目前 Request header JWT Token 中的 UID（送來請求的使用者 UID）。<br/>
        /// 找不到值時拋錯。
        /// </summary>
        /// <returns>UID</returns>
        protected internal int GetUid()
        {
            _uid = _uid ?? FilterStaticTools.GetUidInRequestInt(Request);
            return _uid.Value;
        }

        /// <summary>
        /// 利用ID取得使用者資料
        /// </summary>
        /// <param name="UID">使用者ID</param>
        /// <returns>使用者名稱</returns>
        public async Task<string> GetUserNameByID(int UID)
        {
            string Name = "";
            if (UID > 0)
            {
                var U = await DC.UserData.FirstOrDefaultAsync(q => q.UID == UID);
                if (U != null)
                    Name = U.UserName;
            }

            return Name;
        }

        public string ChangeJson(object O)
        {
            return JsonConvert.SerializeObject(O);
        }

        #region 加解密工具

        public static class HSM
        {
            private static string sKey1 = "0A1B2C3D";
            private static string sKey2 = "4E5F6E7D";
            private static string sKey3 = "8C9B0A1B";
            private static string sIv = "2C3D4E5F";
            private static string sAnswer;

            /// <summary>
            /// 單層加密
            /// </summary>
            /// <param name="sInput">輸入字串(限英數)</param>
            /// <returns></returns>
            public static string Enc_1(string sInput)
            {
                return EncryptDES(sInput, sKey1, sIv);
            }

            /// <summary>
            /// 單層解密
            /// </summary>
            /// <param name="sInput">輸入字串(限英數)</param>
            /// <returns>輸出</returns>
            public static string Des_1(string sInput)
            {
                return DecryptDES(sInput, sKey1, sIv);
            }

            /// <summary>
            /// 3層加密
            /// </summary>
            /// <param name="sInput">輸入字串(限英數)</param>
            /// <returns>輸出</returns>
            public static string Enc_3(string sInput)
            {
                sAnswer = EncryptDES(sInput, sKey3, sIv);
                sAnswer = EncryptDES(sAnswer, sKey2, sIv);
                sAnswer = EncryptDES(sAnswer, sKey1, sIv);
                return sAnswer;
            }

            /// <summary>
            /// 3層解密
            /// </summary>
            /// <param name="sInput">輸入字串(限英數)</param>
            /// <returns>輸出</returns>
            public static string Des_3(string sInput)
            {
                sAnswer = DecryptDES(sInput, sKey1, sIv);
                sAnswer = DecryptDES(sAnswer, sKey2, sIv);
                sAnswer = DecryptDES(sAnswer, sKey3, sIv);
                return sAnswer;
            }

            /// <summary>
            /// 雜湊加密(無法解密)
            /// </summary>
            /// <param name="sInput">輸入字串(限英數)</param>
            /// <returns>輸出</returns>
            public static string Hash(string sInput)
            {
                sAnswer = GetMD5(sInput);
                return sAnswer;
            }

            /// <summary>
            /// MD5加密,不可解密,已能破解不建議使用
            /// </summary>
            /// <param name="sInput">輸入字串</param>
            /// <returns>輸出字串</returns>
            public static string DoMD5(string sInput)
            {
                MD5 md5Hash = MD5.Create();
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(sInput));

                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }

            /// <summary>
            /// SHA-1加密,不可解密,已能破解不建議使用
            /// </summary>
            /// <param name="sInput">輸入字串</param>
            /// <returns>輸出字串</returns>
            public static string DoSHA1(string sInput)
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider(); //建立一個SHA1
                byte[] source = Encoding.Default.GetBytes(sInput); //將字串轉為Byte[]
                byte[] crypto = sha1.ComputeHash(source); //進行SHA1加密
                return Convert.ToBase64String(crypto); //把加密後的字串從Byte[]轉為字串
            }

            /// <summary>
            /// SHA-256加密
            /// </summary>
            /// <param name="sInput">輸入字串</param>
            /// <returns>輸出字串</returns>
            public static string DoSHA256(string sInput)
            {
                SHA256 sha256 = new SHA256CryptoServiceProvider(); //建立一個SHA256
                byte[] source = Encoding.Default.GetBytes(sInput); //將字串轉為Byte[]
                byte[] crypto = sha256.ComputeHash(source); //進行SHA256加密
                return Convert.ToBase64String(crypto); //把加密後的字串從Byte[]轉為字串
            }

            /// <summary>
            /// SHA-384加密
            /// </summary>
            /// <param name="sInput">輸入字串</param>
            /// <returns>輸出字串</returns>
            public static string DoSHA384(string sInput)
            {
                SHA384 sha384 = new SHA384CryptoServiceProvider(); //建立一個SHA384
                byte[] source = Encoding.Default.GetBytes(sInput); //將字串轉為Byte[]
                byte[] crypto = sha384.ComputeHash(source); //進行SHA384加密
                return Convert.ToBase64String(crypto); //把加密後的字串從Byte[]轉為字串
            }

            /// <summary>
            /// SHA-512加密
            /// </summary>
            /// <param name="sInput">輸入字串</param>
            /// <returns>輸出字串</returns>
            public static string DoSHA512(string sInput)
            {
                SHA512 sha512 = new SHA512CryptoServiceProvider(); //建立一個SHA512
                byte[] source = Encoding.Default.GetBytes(sInput); //將字串轉為Byte[]
                byte[] crypto = sha512.ComputeHash(source); //進行SHA512加密
                return Convert.ToBase64String(crypto); //把加密後的字串從Byte[]轉為字串
            }

            #region 加解密底層處理

            /// <summary>   
            /// DES 加密字串   
            /// </summary>   
            /// <param name="original">原始字串</param>   
            /// <param name="key">Key，長度必須為 8 個 ASCII 字元</param>   
            /// <param name="iv">IV，長度必須為 8 個 ASCII 字元</param>   
            /// <returns></returns>   
            private static string EncryptDES(string original, string key, string iv)
            {
                try
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    des.Key = Encoding.ASCII.GetBytes(key);
                    des.IV = Encoding.ASCII.GetBytes(iv);
                    byte[] s = Encoding.ASCII.GetBytes(original);
                    ICryptoTransform desencrypt = des.CreateEncryptor();
                    return BitConverter.ToString(desencrypt.TransformFinalBlock(s, 0, s.Length))
                        .Replace("-", string.Empty);
                }
                catch
                {
                    return original;
                }
            }

            /// <summary>   
            /// DES 解密字串   
            /// </summary>   
            /// <param name="hexString">加密後 Hex String</param>   
            /// <param name="key">Key，長度必須為 8 個 ASCII 字元</param>   
            /// <param name="iv">IV，長度必須為 8 個 ASCII 字元</param>   
            /// <returns></returns>   
            private static string DecryptDES(string hexString, string key, string iv)
            {
                try
                {
                    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                    des.Key = Encoding.ASCII.GetBytes(key);
                    des.IV = Encoding.ASCII.GetBytes(iv);

                    byte[] s = new byte[hexString.Length / 2];
                    int j = 0;
                    for (int i = 0; i < hexString.Length / 2; i++)
                    {
                        s[i] = Byte.Parse(hexString[j].ToString() + hexString[j + 1].ToString(),
                            NumberStyles.HexNumber);
                        j += 2;
                    }

                    ICryptoTransform desencrypt = des.CreateDecryptor();
                    return Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(s, 0, s.Length));
                }
                catch
                {
                    return hexString;
                }
            }

            /// <summary>   
            /// 取得 MD5 編碼後的 Hex 字串   
            /// 加密後為 32 Bytes Hex String (16 Byte)   
            /// </summary>   
            /// <param name="original">原始字串</param>   
            /// <returns></returns>   
            private static string GetMD5(string original)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(original));
                return BitConverter.ToString(b).Replace("-", string.Empty);
            }

            #endregion
        }

        #endregion
    }
}