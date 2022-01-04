using System;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Application.Common.Auth.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(
            params Permissions[] permissions)
        {
            Policy = string.Join("|", permissions);
        }
    }
}