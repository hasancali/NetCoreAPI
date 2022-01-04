using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum Permissions : short
    {
        [Display(
            GroupName = "Roles",
            Name = "ManageRoles",
            Description = "Can add, update and remove roles")]
        RoleManager = 1,

        [Display(
            GroupName = "Users",
            Name = "Read Users",
            Description = "Can read company users")]
        ReadUsers = 10,

        [Display(
            GroupName = "Users",
            Name = "Edit Users",
            Description = "Can edit company users")]
        EditUsers = 11,

        [Display(
            GroupName = "Super User",
            Name = "SuperUser",
            Description = "Has access to everything")]
        SuperUser = Int16.MaxValue,
    }
}