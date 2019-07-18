using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace System
{
    public static class ObjectExtentions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null || obj == DBNull.Value;
        }

        public static string HtmlEncode(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return HttpUtility.HtmlEncode(str).Replace("'", "&dot");
        }

        public static string HtmlDecode(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return HttpUtility.HtmlDecode(str).Replace("&dot", "'");
        }

        public static string UrlEncode(this string str, string charset = "utf-8")
        {
            return str.UrlEncode(Encoding.GetEncoding(charset));
        }
        public static string UrlEncode(this string str, Encoding encoding)
        {
            return HttpUtility.UrlEncode(str, encoding);
        }

        public static string UrlDecode(this string str, string charset = "utf-8")
        {
            return str.UrlDecode(Encoding.GetEncoding(charset));
        }
        public static string UrlDecode(this string str, Encoding encoding)
        {
            return HttpUtility.UrlDecode(str, encoding);
        }


        public static string Safe(this string str, string safeValue)
        {
            return string.IsNullOrWhiteSpace(str) ? safeValue : str;
        }

        public static string BeforeContact(this string str, string prefix)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : string.Concat(prefix, str);
        }

        public static string AfterContact(this string str, string suffix)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : string.Concat(str, suffix);
        }


        public static int TrueLength(this string str, string charset = "utf-8")
        {
            return str.TrueLength(Encoding.GetEncoding(charset));
        }
        public static int TrueLength(this string str, Encoding encoding)
        {
            return encoding.GetByteCount(str);
        }

        public static string Cutting(this string str, int len, string tail)
        {
            string result = string.Empty;  
            int byteLen = str.TrueLength();  

            if (byteLen > len) {
                int charLen = str.Length;  
                int byteCount = 0;  
                int pos = 0;  

                for (int i = 0; i < charLen; i++) {
                    if (Convert.ToInt32(str.ToCharArray()[i]) > 255) { 
                        byteCount += 2;
                    }
                    else { 
                        byteCount += 1;
                    }

                    if (byteCount > len) { 
                        pos = i;
                        break;
                    }
                    else if (byteCount == len) { 
                        pos = i + 1;
                        break;
                    }
                }

                if (pos >= 0) {
                    result = string.Concat(str.Substring(0, pos), tail);
                }
            }
            else {
                result = str;
            }

            return result;
        }

        public static string[] Split(this string str, string split)
        {
            return (from piece in Regex.Split(str, Regex.Escape(split), RegexOptions.IgnoreCase)
                    let trimmed = piece.Trim()
                    where !string.IsNullOrEmpty(trimmed)
                    select trimmed).ToArray();
        }

        public static bool InArray(this string str, string[] array, bool caseInsensetive = true)
        {
            return Array.Exists<string>(array, delegate(string element) {
                return string.Equals(element, str,
                        caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        public static int InArrayIndexOf(this string str, string[] array, bool caseInsensetive = true)
        {
            return Array.FindIndex<string>(array, delegate(string element) {
                return string.Equals(element, str,
                        caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        public static bool InArray(this string str, string strarray, string strsplit = ",", bool caseInsensetive = false)
        {
            return Array.Exists(Split(strarray, strsplit), delegate(string element) {
                return string.Equals(element, str,
                        caseInsensetive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
            });
        }

        public static bool IsNumeric(this string str)
        {
            return Regex.IsMatch(str, @"^[-]?[0-9]*$");
        }

        public static bool IsDate(this string str)
        {
            return Regex.IsMatch(str, @"(\d{4})-(\d{1,2})-(\d{1,2})");
        }

        public static bool IsTime(this string str)
        {
            return Regex.IsMatch(str, @"^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        public static bool IsDateTime(this string str)
        {
            return Regex.IsMatch(str, @"(\d{4})-(\d{1,2})-(\d{1,2}) ^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }

        public static bool IsDecimal(this string str)
        {
            return Regex.IsMatch(str, @"^[-]?[0-9]*[.]?[0-9]*$");
        }

        public static bool IsEmail(this string str)
        {
            return Regex.IsMatch(str, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static bool IsIPV4(this string ip)
        {
            string num = "(25[0-5]|2[0-4]//d|[0-1]//d{2}|[1-9]?//d)";
            return Regex.IsMatch(ip, string.Concat("^", num, "//.", num, "//.", num, "//.", num, "$"));
        }

        public static bool ToBoolean(this string str)
        {
            return ToBoolean(str, false);
        }

        public static bool ToBoolean(this string str, bool value)
        {
            bool val;
            if (!string.IsNullOrEmpty(str) && bool.TryParse(str, out val)) {
                return val;
            }
            return value;
        }

        public static int ToInt(this string str)
        {
            return ToInt(str, 0);
        }

        public static int ToInt(this string str, int value)
        {
            int val;
            if (!string.IsNullOrEmpty(str) && int.TryParse(str, out val)) {
                return val;
            }
            return value;
        }


        public static decimal ToDecimal(this string str)
        {
            return ToDecimal(str, 0);
        }

        public static decimal ToDecimal(this string str, decimal value)
        {
            decimal val;
            if (!string.IsNullOrEmpty(str) && decimal.TryParse(str, out val)) {
                return val;
            }
            return value;
        }


        public static DateTime ToDate(this string str)
        {
            return ToDate(str, DateTime.Today);
        }

        public static DateTime ToDate(this string str, DateTime value)
        {
            DateTime date;
            if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out date)) {
                return date;
            }
            return value;
        }

        public static bool InIPArray(this string ip, string[] iparray)
        {
            string[] userip = Split(ip, @".");
            for (int ipIndex = 0; ipIndex < iparray.Length; ipIndex++) {
                string[] tmpip = Split(iparray[ipIndex], @".");
                int r = 0;
                for (int i = 0; i < tmpip.Length; i++) {
                    if (tmpip[i] == "*") {
                        return true;
                    }

                    if (userip.Length > i) {
                        if (tmpip[i] == userip[i]) {
                            r++;
                        }
                        else {
                            break;
                        }
                    }
                    else {
                        break;
                    }

                }
                if (r == 4) {
                    return true;
                }
            }
            return false;
        }

    }
}
