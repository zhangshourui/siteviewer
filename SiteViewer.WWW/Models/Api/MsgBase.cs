using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
// using Newtonsoft.Json;

namespace SiteViewer.WWW.Models.Api
{
    public class JsonActionResult : IActionResult
    {
        private readonly JsonSerializerOptions _options;

        public JsonActionResult()
        {
            _options = new JsonSerializerOptions
            {
                //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }


        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json; charset=utf-8";
            response.StatusCode = StatusCodes.Status200OK;

            await JsonSerializer.SerializeAsync(response.Body, this, this.GetType());

        }

     
    }


    public abstract class MsgBase : JsonActionResult
    {
        public MsgBase()
        {
            // = StatusCodes.Status200OK;
        }

        public ResultCode Result { get; set; }
        public string ErrorMsg { get; set; } = string.Empty;

        public static ErrorMsg BuildErrorMsg(string errorMsg, ResultCode code = ResultCode.Error)
        {
            return new ErrorMsg(errorMsg, code);
        }
    }

    public enum ResultCode
    {
        Success = 0,
        Error = 1,
        NotFound = 2,
        Unauthorized = 3,
        Forbidden = 4,
        BadRequest = 5,
        InternalServerError = 500
    }

    public class DefaultMsg<T> : MsgBase
    {
        public T? Data { get; set; } = default;
    }

    public class ErrorMsg : MsgBase
    {
        public ErrorMsg()
        { }

        public ErrorMsg(string errorMsg, ResultCode code = ResultCode.Error)
        {
            Result = code;
            ErrorMsg = errorMsg;
        }
    }
}