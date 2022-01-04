using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;

namespace Domain.Helpers
{
    public static class PermissionHelper
    {
        public static string PackPermissions(
            this IEnumerable<Permissions> permissions)
        {
            return permissions.Aggregate("", (s, permission) => s + (char) permission);
        }

        public static IEnumerable<Permissions> UnpackPermissions(
            this string packedPermissions)
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            foreach (var character in packedPermissions)
            {
                yield return ((Permissions) character);
            }
        }
    }
}