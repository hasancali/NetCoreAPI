using System;
using System.Net;

namespace Application.Common.ErrorHandling
{
    public class HttpException : Exception
    {
        public HttpException(
            HttpStatusCode code,
            object errors = null)
        {
            Code = code;
            Errors = errors;
        }

        public object Errors { get; set; }
        public HttpStatusCode Code { get; }
    }
}