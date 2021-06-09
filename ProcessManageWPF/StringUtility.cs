using System.Collections.Generic;

namespace ProcessManageWPF
{
    /// <summary>
    /// 字符串工具库
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// 将以"{Macro}"形式的宏替换为字典中的对象, 如果没有则保留原样
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="macroDictionary"></param>
        /// <returns></returns>
        public static string ReplaceMacro(this string raw, Dictionary<string, object> macroDictionary)
        {
            var temp = raw;
            foreach (var keyValuePair in macroDictionary)
            {
                temp = temp.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value.ToString());
            }
            return temp;
        }
    }
}