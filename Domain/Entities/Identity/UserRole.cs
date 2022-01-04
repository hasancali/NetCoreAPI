namespace Domain.Entities.Identity
{
    public class UserRole 
    {
        private UserRole() { }

        public UserRole(
            User user,
            Role role)
        {
            User = user;
            Role = role;
        }

        public int UserId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}