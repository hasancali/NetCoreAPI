using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Domain.Common;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Domain.Entities.Identity
{
    public class User : Entity, IHaveCustomFields
    {
        private readonly List<RefreshToken> _refreshTokens = new List<RefreshToken>();

        public User()
        {
            
        }

        public User(
            string firstName,
            string lastName,
            string email,
            string userName,
            string phoneNumber,
            byte[] hash,
            byte[] salt,
            Address address)
        {
            Update(firstName, lastName, phoneNumber);
            Email = email;
            UserName = userName;
            Hash = hash;
            Salt = salt;
            Address = address;
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }
        public string PhoneNumber { get; private set; }
        public string TwoFactorEnabled { get; private set; }
        [JsonIgnore]
        [DoNotAudit]
        public byte[] Hash { get; private set; }

        [JsonIgnore]
        [DoNotAudit]
        public byte[] Salt { get; private set; }

        public Address Address { get; set; }
        public CustomField[] CustomFields { get; set; } = new CustomField[0];

        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        public ICollection<UserRole> Roles { get; } = new HashSet<UserRole>();

        public void Update(
            string firstName,
            string lastName,
            string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            FullName = $"{FirstName} {LastName}";
            PhoneNumber = phoneNumber;
        }

        public void AddRefreshToken(
            string token,
            double daysToExpire = 2)
        {
            _refreshTokens.Add(
                new RefreshToken(
                    token,
                    DateTime.UtcNow.AddDays(daysToExpire),
                    this));
        }

        public void RemoveRefreshToken(
            string refreshToken)
        {
            _refreshTokens.Remove(_refreshTokens.First(t => t.Token == refreshToken));
        }

        public bool IsValidRefreshToken(
            string refreshToken)
        {
            return _refreshTokens.Any(rt => rt.Token == refreshToken && rt.Active);
        }
    }
}