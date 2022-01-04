using System;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Auth.Authentication;
using Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly HMACSHA512 _hmacsha512;

        public PasswordHasher(
            IOptions<PasswordHasherSettings> passwordHasherSettings)
        {
            _hmacsha512 = new HMACSHA512(Encoding.UTF8.GetBytes(passwordHasherSettings.Value.Key));
        }

        public byte[] Hash(
            string password,
            byte[] salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var passwordBytesWithSaltBytes = new byte[passwordBytes.Length + salt.Length];

            //copy bytes from both password and salt
            Buffer.BlockCopy(
                passwordBytes,
                0,
                passwordBytesWithSaltBytes,
                0,
                passwordBytes.Length);

            Buffer.BlockCopy(
                salt,
                0,
                passwordBytesWithSaltBytes,
                passwordBytes.Length,
                salt.Length);

            return _hmacsha512.ComputeHash(passwordBytesWithSaltBytes);
        }
    }
}