using System;
using System.Net;

namespace Domain.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(
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