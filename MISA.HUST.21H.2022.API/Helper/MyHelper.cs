using Dapper;
using MISA.HUST._21H._2022.API.Entities;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace MISA.HUST._21H._2022.API.Helper
{
    public class MyHelper
    {
        public static bool isGreaterThanNow(DateTime time)
        {
            int result = DateTime.Compare(time, DateTime.Now);
            if(result > 0)
            {
                return true;
            }
            return false;
        }

        public static Object buildError(string devMsg = "", string userMsg = "Có lỗi trong quá trình thực hiện, liên hệ admin", string type = "e001")
        {
            return new {
                errorCode = type,
                devMsg = devMsg,
                userMsg = userMsg,
                moreInfo = "",
                traceId = "",
            };
        }

        public static bool isValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
