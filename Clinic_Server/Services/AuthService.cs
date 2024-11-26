using Clinic_Server.Data;
using Clinic_Server.Helper;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Clinic_Server.Services
{
    public class AuthService
    {
        private USER_PKG user_pkg;
        public AuthService(USER_PKG user_pkg) {
            this.user_pkg = user_pkg;
        }
        async public Task<bool> RegisterDoctor(Doctor request)
        {
            if (string.IsNullOrEmpty(request.email))
            {
                throw new ArgumentException("მეილი არავალიდურია");
            }
            var finduser = user_pkg.FindUser(request.email);
            if (finduser != null)
            {
                 throw new ArgumentException("ამ მეილით ექაუნთი უკვე შექმნილია");
            }

            byte[] photoBytes;
            byte[] resumeBytes;

            using (var ms = new MemoryStream())
            {
                await request.photo.CopyToAsync(ms);
                photoBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                await request.resume.CopyToAsync(ms);
                resumeBytes = ms.ToArray();
            }
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

            var user = new Users
            {
                name = request.name,
                surname = request.surname,
                email = request.email,
                private_number = request.private_number,
                password = passwordHash,
                category_id = int.Parse(request.category_id),
                photo = photoBytes,
                resume = resumeBytes
            };

            var result = this.user_pkg.DoctorAuth(user);
            return result;
        }

        async public Task<Users> UserLogin(Signin request)
        {
            Users finduser = user_pkg.FindUser(request.email);
            if (finduser == null)
            {
                throw new ArgumentException("მოცემული მეილი არ არის რეგისტრირებული");
            }

            bool verified = BCrypt.Net.BCrypt.Verify(request.password, finduser.password);

            if (verified != true)
            {
                throw new ArgumentException("პაროლი არასწორია");
            }
            finduser.password = null;
            return finduser;
        }
    }
}
