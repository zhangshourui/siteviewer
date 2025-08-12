using Microsoft.AspNetCore.StaticFiles;
using SiteViewer.WWW.Service;

namespace SiteViewer.WWW.AppService
{
    public class FileRepRewriteMiddleware
    {
        private readonly RequestDelegate _next;

        public FileRepRewriteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {

            var path = context.Request.Path.Value ?? "";// format: /a/b/c

            var relativeFile = path.Substring(1).Replace('/', Path.DirectorySeparatorChar);

            // Convert /a/b/c to a\b\c, and resolve it into full path
            var fullPath = FileRep.ResolveCachePath(relativeFile);

            // serve as static file if exists at cache first, then at resource root
            if (File.Exists(fullPath))
            {
                await ServeFileAsync(context, fullPath);
                return;
            }
            else
            {
                fullPath = FileRep.ResolveRepPath(relativeFile);
                if (File.Exists(fullPath))
                {
                    await ServeFileAsync(context, fullPath);
                    return;
                }
            }

            // serve as mvc controller if not found
            await _next(context);
        }

        private async Task ServeFileAsync(HttpContext context, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var contentType = FileRep.GetContentType(filePath);

            context.Response.ContentType = contentType;
            context.Response.ContentLength = fileInfo.Length;
            // cros
            context.Response.Headers.AccessControlAllowOrigin = "*";
            //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
            //context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");

            using (var fileStream = fileInfo.OpenRead())
            {
                await fileStream.CopyToAsync(context.Response.Body);
            }
        }
    }

    public static class FileRepRewriteMiddlewareExtensions
    {
        public static IApplicationBuilder UseFileRepRewrite(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FileRepRewriteMiddleware>();
        }
    }
}