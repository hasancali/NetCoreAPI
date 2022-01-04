using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Helpers;

namespace Application.Common.Extensions
{
    public static class ValidatePermissionsExtension
    {
        public static bool ThesePermissionsAreAllowed(
            this string packedPermissions,
            IEnumerable<string> permissions)
        {
            var usersPermissions = packedPermissions.UnpackPermissions().ToArray();
            return usersPermissions.UserHasThesePermission(
                permissions.Select(
                    x => (Permissions) Enum.Parse(
                        typeof(Permissions),
                        x)));
        }


        public static bool UserHasThesePermission(
            this Permissions[] usersPermissions,
            IEnumerable<Permissions> permissionsToCheck)
        {
            return usersPermissions.Select(x => x)
                .Intersect(permissionsToCheck)
                .Any() || usersPermissions.Contains(Permissions.SuperUser);
        }

        public static void Authorize(
            this ICurrentUserService currentUserService,
            params Permissions[] permissionsToCheck)
        {
            var currentUserPermissions = currentUserService.Permissions.UnpackPermissions();
            var hasPermission = currentUserPermissions.Select(x => x)
                .Intersect(permissionsToCheck)
                .Any() || currentUserPermissions.Contains(Permissions.SuperUser);

            if(!hasPermission)
                throw new AccessDeniedException(HttpStatusCode.Forbidden);
        }
        
        public static bool IsAllowed(
            this ICurrentUserService currentUserService,
            params Permissions[] permissionsToCheck)
        {
            var currentUserPermissions = currentUserService.Permissions.UnpackPermissions();
            return currentUserPermissions.Select(x => x)
                .Intersect(permissionsToCheck)
                .Any() || currentUserPermissions.Contains(Permissions.SuperUser);
        }
    }
}