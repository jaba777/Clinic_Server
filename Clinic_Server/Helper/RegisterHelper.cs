
using Clinic_Server.Models;
using Clinic_Server.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace Clinic_Server.Helper
{
    public class RegisterHelper
    {
        private IRedisService redisService;
        private EmailService emailService;
        public RegisterHelper(IRedisService redisService, EmailService emailService)
        {
            this.redisService = redisService;
            this.emailService = emailService;
        }

        async public Task<bool> Register(Users request)
        {
            var redisDb = redisService.GetDatabase();
            var regexPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?""':;{}|<>]).{8,16}$";
            if (!Regex.IsMatch(request.password, regexPattern))
            {
                throw new ArgumentException("Password not valid");
            }

            if (request.name.Length <= 4)
            {
                throw new ArgumentException("Name not valid");
            }
            var userCache = await redisDb.StringGetAsync($"users:{request.email}");
            if (userCache.HasValue)
            {
                await redisDb.KeyDeleteAsync($"users:{request.email}");
            }
            Random random = new Random();
            int otp = random.Next(1000, 10000);
            request.otp=otp.ToString();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);
            request.password = passwordHash;



            var userJson=JsonSerializer.Serialize(request);
            await redisDb.StringSetAsync($"users:{request.email}",userJson,TimeSpan.FromMinutes(2));

            await this.emailService.SendEmailOtp(request.email, otp.ToString(),request.name);


            return true;
        }

       

        async public Task<Users> verifyOtp(string email,string otp)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("დაფიქსირდა შეცდომა");
            }
            var redisDb = redisService.GetDatabase();

            var user=await redisDb.StringGetAsync($"users:{email}");
            if (user.IsNull)
            {
                throw new ArgumentException("დრო ამოიწურა, სცადეთ ხელახლა");
            }
            var userObject = JsonSerializer.Deserialize<Users>(user.ToString());
            if (userObject.otp != otp)
            {
                throw new ArgumentException("კოდი არასწორია");
            }

            return userObject;
        }
    }
}
