using Microsoft.AspNetCore.Identity;
using WebApi1.Models.Authentication.SignUp;

namespace WebApi1.Repositories
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        //public Task<IdentityResult> SignInAsync(Sign)
    }
}
