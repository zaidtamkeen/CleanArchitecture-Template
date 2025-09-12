using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CleanTemplate.ApiFramework.Tools
{
    /// <summary>
    /// Convenience wrapper for creating an <see cref="ApiResult{T}"/> without
    /// specifying the generic type. Used by controllers to return data in a
    /// consistent envelope.
    /// </summary>
    public class ApiResult : ApiResult<object>
    {
        /// <summary>
        /// Creates an API result for non-generic data payloads.
        /// </summary>
        public ApiResult(object data, int statusCode = StatusCodes.Status200OK, string[] errors = null) : base(data, statusCode, errors)
        {

        }
    }

    /// <summary>
    /// Standard response envelope used by controllers to return data and
    /// errors with a status code.
    /// </summary>
    public class ApiResult<T> : IActionResult, IDisposable, IStatusCodeActionResult
    {
        /// <summary>List of validation or processing errors.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] Errors { get; set; }

        /// <summary>Payload being returned to the client.</summary>
        public T Data { get; set; }

        /// <summary>HTTP status code for the response.</summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ApiResult{T}"/>.
        /// </summary>
        public ApiResult(T data, int statusCode = StatusCodes.Status200OK, string[] errors = null)
        {
            Data = data;
            StatusCode = statusCode;
            Errors = errors != null && errors.Length > 0 ? errors : null;
        }

        /// <summary>
        /// Writes the result to the HTTP response via <see cref="OkObjectResult"/>.
        /// </summary>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            await new OkObjectResult(this).ExecuteResultAsync(context);
        }

        /// <summary>
        /// Disposes the payload if it implements <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            if (Data != null && typeof(T).GetInterfaces().Contains(typeof(IDisposable)))
            {
                ((IDisposable)Data).Dispose();
            }
        }
    }
}