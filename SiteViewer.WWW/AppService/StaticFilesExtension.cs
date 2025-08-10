using Microsoft.AspNetCore.StaticFiles;

namespace SiteViewer.WWW.AppService
{
    public static class StaticFilesExtension
    {
        public static void UseStaticFiles(this WebApplication app)
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";

            StaticFileExtensions.UseStaticFiles(app, new StaticFileOptions
            {
                ContentTypeProvider = provider,
                //OnPrepareResponse = (c) => {
                //    c.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                //}
            });
        }
    }
}
