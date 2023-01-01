using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Unesi lozinku: ");
            string lozinka = Console.ReadLine();

            Hashing hash = new Hashing();
            GenerateHash(lozinka, hash);

            Console.WriteLine($"tvoja lozinka je: {lozinka}\nkoja hashirano glasi: {hash.Hash}\n");

            if(VerifyPassword(lozinka, hash))
                Console.WriteLine("isti su");
            else
                Console.WriteLine("bome nisu");

        }

        private static string HashingPassword(string password, byte[] salt = null)
        {
            if (salt == null || salt.Length != 16)
            {
                salt = new byte[128 / 8];
                using (var rngCrypto = RandomNumberGenerator.Create())
                {
                    rngCrypto.GetBytes(salt);
                }
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

            return $"{hashedPassword}:{Convert.ToBase64String(salt)}";
        }

        private static bool VerifyPassword(string hashedPassword, string password)
        {
            var passwordAndHash = hashedPassword.Split(':');

            var salt = Convert.FromBase64String(passwordAndHash[1]);

            var hashOfpasswordToCheck = HashingPassword(password, salt);

            if (String.Compare(passwordAndHash[0], hashOfpasswordToCheck) == 0)
            {
                return true;
            }
            return false;
        }

        public static void GenerateHash(string lozinka, Hashing hashing)
        {
            var saltBytes = new byte[64];
            var provider = new RNGCryptoServiceProvider();
            provider.GetNonZeroBytes(saltBytes);
            hashing.Salt = Convert.ToBase64String(saltBytes);

            var rfc2898 = new Rfc2898DeriveBytes(lozinka, saltBytes, 10000);
            hashing.Hash = Convert.ToBase64String(rfc2898.GetBytes(256));
        }

        public static bool VerifyPassword(string lozinka, Hashing hashing)
        {
            var saltBytes = Convert.FromBase64String(hashing.Salt);
            var rfc2898 = new Rfc2898DeriveBytes(lozinka, saltBytes, 10000);
            return Convert.ToBase64String(rfc2898.GetBytes(256)) == hashing.Hash;
        }
    }
}
