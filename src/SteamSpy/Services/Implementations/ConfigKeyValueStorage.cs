using Framework;
using System;
using System.Configuration;
using ThunderHawk.Core;

namespace ThunderHawk
{
    public class ConfigKeyValueStorage : IKeyValueStorage
    {
        readonly Configuration _config;

        public ConfigKeyValueStorage()
        {
            try
            {
                _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool ClearValue(string key)
        {
            if (_config == null || !ContainsKey(key))
                return false;

            _config.AppSettings.Settings.Remove(key);
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            return true;
        }

        public bool ContainsKey(string key)
        {
            if (_config == null)
                return false;

            return _config.AppSettings.Settings[key] != null;
        }

        public string GetValue(string key)
        {
            if (!ContainsKey(key))
                return default;

            return _config.AppSettings.Settings[key].Value;
        }

        public void SetValue(string key, string value)
        {
            if (_config == null)
                return;

            if (value == default)
                _config.AppSettings.Settings.Remove(key);
            else
            {
                if (ContainsKey(key))
                    _config.AppSettings.Settings.Remove(key);
                _config.AppSettings.Settings.Add(key, value);
            }

            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
