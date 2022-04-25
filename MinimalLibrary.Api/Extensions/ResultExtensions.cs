using System.Net.Mime;
using System.Text;

namespace MinimalLibrary.Api.Extensions;

public static class ResultExtensions
{
    public static IResult Html(this IResultExtensions resultExtensions, string htmlContent)
    {
        return new HtmlResult(htmlContent);
    }
    
    private class HtmlResult : IResult
    {
        private readonly string _htmlContent;

        public HtmlResult(string htmlContent)
        {
            _htmlContent = htmlContent;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            httpContext.Request.ContentLength = Encoding.UTF8.GetByteCount(_htmlContent);

            return httpContext.Response.WriteAsync(_htmlContent);
        }
    }
}