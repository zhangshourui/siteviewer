namespace SiteViewer.WWW.AppService
{

    public class AppConfigService
    {
        //通过这个方法IConfiguration与配置文件链接起来
        //public static void Init(IConfiguration configuration)
        //{
        //    AppSettings appConfig = new AppSettings();
        //    configuration.Bind("AppSettings", appConfig);
        //}
        //public static AppSettings GetAppSettings(IConfiguration configuration)
        //{

        //    var appConfig = configuration.GetRequiredSection("AppSettings").Get<AppSettings>()
        //        ?? new AppSettings()
        //        {
        //            ResourceCache = "",
        //            ResourceRoot = ""
        //        };

        //    return appConfig;
        //}


    }
    public class AppSettings
    {
        //通过这个方法IConfiguration与配置文件链接起来
        //public static void Init(IConfiguration configuration)
        //{
        //    AppSettings appConfig = new AppSettings();
        //    configuration.Bind("AppSettings", appConfig);
        //}
        static AppSettings()
        {
            ResourceRoot = "";
            ResourceCache = "";
            Host = "";
        }

        public static string ResourceRoot { get; set; }

        public static string ResourceCache { get; set; }

        public static string Host { get; set; }

        public static string[]? AccessControlAllowOrigin { get; set; }

        public static string? Ffmpeg { get; set; }

    }

    public static class AppConfigExtension
    {
        public static IConfiguration UseAppConfig(this IConfiguration configuration)
        {
            var appConfig = new AppSettings();
            configuration.Bind("AppSettings", appConfig);
            // configuration.GetRequiredSection("AppSettings").Get<AppSettings>();
            return configuration;
        }

    }
}
