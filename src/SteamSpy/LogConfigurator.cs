using NLog.Config;
using NLog.Targets;

namespace ThunderHawk
{
    public static class LogConfigurator
    {
        public static LoggingConfiguration GetConfiguration(string logsFolder)
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = logsFolder + "/${shortdate}.log",
                Layout = "${longdate} | ${uppercase:${level}} | ${logger} | ${message}",
                MaxArchiveFiles = 7,
                ArchiveEvery = FileArchivePeriod.Day
            };
            config.AddTarget(fileTarget);

            config.AddRuleForAllLevels(fileTarget);

            return config;
        }
    }
}
