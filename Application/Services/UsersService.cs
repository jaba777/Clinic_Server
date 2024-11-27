using Infrastructure.Data;
using Infrastructure.Models;
using System.Text.RegularExpressions;
using System.Text.Json;
using Infrastructure.Redis;

namespace Application.Services
{
    public class UsersService
    {
        USER_PKG user_pkg;
        private IRedisService redisService;
        private EmailService emailService;
        public UsersService(USER_PKG user_pkg,IRedisService redisService, EmailService emailService)
        {
            this.user_pkg = user_pkg;
            this.redisService = redisService;
            this.emailService = emailService;
        }

        async public Task<Users> MyProfile(int userId)
        {
            return this.user_pkg.Myprofile(userId);
        }

        async public Task<DoctorResult> GetDoctors(int categoryId, int page)
        {
            return this.user_pkg.FindDoctors(categoryId, page);
        }
        async public Task<Users> GetDoctor(int userId)
        {
            return this.user_pkg.getUser(userId);
        }
        async public Task<bool> EditUser(Doctor request, int userId)
        {
            if (!string.IsNullOrEmpty(request?.password))
            {
                var regexPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?""':;{}|<>]).{8,16}$";

                if (!Regex.IsMatch(request.password, regexPattern))
                {
                    throw new ArgumentException("პაროლი არავალიდურია");

                }
                else
                {
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);
                    request.password = passwordHash;
                }
            }
            else
            {
                request.password = "";
            }


            byte[] photoBytes = null;
            byte[] resumeBytes = null;

            if (request?.photo != null)
            {
                var ms = new MemoryStream();
                await request.photo.CopyToAsync(ms);
                photoBytes = ms.ToArray();
            }
            if (request?.resume != null)
            {
                var ms = new MemoryStream();
                await request.resume.CopyToAsync(ms);
                resumeBytes = ms.ToArray();
            }
            var user = new Users
            {
                name = request.name,
                surname = request.surname,
                email = request.email,
                private_number = request.private_number,
                password = request.password,
                category_id = int.Parse(request.category_id),
                photo = photoBytes,
                resume = resumeBytes,
            };
            var update_user = this.user_pkg.UserUpdate(user, userId);
            return update_user;
        }
        async public Task<bool> DeleteUser(int userId)
        {
            return this.user_pkg.UserDelete(userId);
        }
        async public Task<bool> VerifyEmail(VerifyEmailRequest request)
        {
            var redisDb = redisService.GetDatabase();
            var finduser = user_pkg.FindUser(request.email);
            if (finduser == null)
            {
                throw new ArgumentException("ექაუნთი ვერ მოიძებნა");

            }


            var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
            if (userCache.HasValue)
            {
                await redisDb.KeyDeleteAsync($"user_email:{request.email}");
            }

            Random random = new Random();
            string otp = random.Next(1000, 10000).ToString();
            var verify = new OtpData { otp = otp, verify = false };
            await redisDb.StringSetAsync($"user_email:{request.email}", JsonSerializer.Serialize(verify), TimeSpan.FromMinutes(2));
            await this.emailService.SendEmailOtp(request.email, otp, finduser.name);
            return true;
        }
        async public Task<bool> VerifyOtp(VerifyRequest request)
        {
            var redisDb = redisService.GetDatabase();
            var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
            if (userCache.IsNull)
            {
                throw new ArgumentException("დრო ამოიწურა, სცადეთ თავიდან");
            }

            var getOtp = JsonSerializer.Deserialize<OtpData>(userCache.ToString());
            if (getOtp.otp != request.otp)
            {
                throw new ArgumentException("კოდი არასწორია");
            }

            await redisDb.KeyDeleteAsync($"user_email:{request.email}");

            var verify = new OtpData { otp = request.otp, verify = true };
            await redisDb.StringSetAsync($"user_email:{request.email}", JsonSerializer.Serialize(verify), TimeSpan.FromMinutes(5));
            return true;
        }
        async public Task<bool> ChangePassword(ChangePass request)
        {
            var redisDb = redisService.GetDatabase();
            var userCache = await redisDb.StringGetAsync($"user_email:{request.email}");
            if (userCache.IsNull)
            {
                throw new ArgumentException("დრო ამოიწურა, სცადეთ თავიდან");

            }

            var getOtp = JsonSerializer.Deserialize<OtpData>(userCache.ToString());
            if (getOtp.verify != true)
            {
                throw new ArgumentException("კოდი არ არის ვერიფიცირებული");
            }
            var regexPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?""':;{}|<>]).{8,16}$";
            if (!Regex.IsMatch(request.password, regexPattern))
            {
                throw new ArgumentException("პაროლი არავალიდურია");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

            return user_pkg.ChangePassword(request.email, passwordHash);
        }
    }
}
