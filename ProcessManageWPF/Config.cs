using System.Collections.Generic;

namespace ProcessManageWPF
{
    public class Config
    {
        public int cpuCount { get; set; }
        public int memorySize { get; set; }
        public string cfgPath { get; set; }
        public Dictionary<string, string> stringCache { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> intCache { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 获取整型缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public int GetIntCache(string key, int @default = 0)
        {
            if (intCache.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return @default;
            }
        }

        /// <summary>
        /// 设置整型缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetIntCache(string key, int value)
        {
            if (intCache.ContainsKey(key))
            {
                intCache[key] = value;
            }
            else
            {
                intCache.Add(key, value);
            }
            App.SaveConfig();
        }

        /// <summary>
        /// 获取字符串缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetStringCache(string key, string @default = "")
        {
            if(stringCache.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return @default;
            }
        }

        /// <summary>
        /// 设置字符串缓存的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetStringCache(string key, string value)
        {
            if (stringCache.ContainsKey(key))
            {
                stringCache[key] = value;
            }
            else
            {
                stringCache.Add(key, value);
            }
            App.SaveConfig();
        }

        public static Config defaultConfig = new Config { cpuCount = 4, memorySize = 1024, cfgPath = "config.json"};
    }
}
