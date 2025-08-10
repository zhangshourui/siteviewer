namespace SiteViewer.WWW.AppService
{
    public static class RoutingExtension
    {
        public static void UseRouting(this WebApplication app)
        {
            EndpointRoutingApplicationBuilderExtensions.UseRouting(app);



            // app.MapGet("/", (ctx) => "Hello World!");

            app.MapControllerRoute(
            name: "api",
            pattern: "api/{controller=Error}/{action=Index}/{id?}",
            defaults: new { controller = "Error", action = "Index" },
            constraints: null,
            dataTokens: new { Namespace = "SiteViewer.WWW.Controllers.Api" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}",
                defaults: new { controller = "Home", action = "Index" },
                constraints: null,
                dataTokens: new { Namespace = "SiteViewer.WWW.Controllers" });

            //// 添加跨域处理
            //app.UseCors(builder =>
            //{
            //    builder.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader();
            //    //  .AllowCredentials()
            //    // .SetIsOriginAllowed(origin => true); // 允许所有来源
            //    builder.WithOrigins("http://localhost:5173/") // 允许所有来源
            //     .AllowCredentials();
            //});
        }
    }
}
