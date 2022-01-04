namespace Application.Features.Roles
{
    public class RoleDto
    {
        protected RoleDto() { }

        public RoleDto(
            int id,
            string name,
            string permissions)
        {
            Id = id;
            Name = name;
            Permissions = permissions;
        }

        public int Id { get; }
        public string Name { get; }
        public string Permissions { get; }
    }
}