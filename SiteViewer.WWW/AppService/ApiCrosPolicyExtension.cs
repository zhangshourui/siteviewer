namespace SiteViewer.WWW.AppService
{
    public static class ApiCrosPolicyExtension
    {
        public static void UseApiCrosPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("ApiCorsPolicy", builder =>
                {
                    if (AppSettings.AccessControlAllowOrigin?.Contains("*") ?? false)
                    {
                        builder.AllowAnyOrigin();
                    }
                    else if (AppSettings.AccessControlAllowOrigin != null && AppSettings.AccessControlAllowOrigin.Length > 0)
                    {
                        builder.WithOrigins(AppSettings.AccessControlAllowOrigin);
                    }

                    builder.WithMethods("GET", "POST")
                           .WithHeaders("Content-Type", "Authorization")
                           .WithHeaders("X-Requested-With", "Accept")
                           .WithMethods("GET", "POST", "OPTIONS")
                           .AllowCredentials();  // 如果需要凭证
                });
            });
        }
    }
}
