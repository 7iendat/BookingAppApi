using Microsoft.AspNetCore.Identity;
using System.Security.Policy;
using WebApi1.Data;
using WebApi1.Models.Authentication.SignUp;

namespace WebApi1.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManage;
        private readonly IConfiguration _configuration;

        public AccountRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration) {
            _userManage = userManager;
            _configuration = configuration;
        }
        public async Task<IdentityResult> SignUpAsync(SignUpModel model)
        {
            var user = new ApplicationUser
            {
                FirstName= model.FirstName,
                LastName= model.LastName,
                UserName = model.Email,
                Email = model.Email,
            };

            //await _userManage.AddToRoleAsync(user, role);
            //var token = await _userManage.GenerateEmailConfirmationTokenAsync(user);
            //var confirmationLink = Url.Action(nameof(ConfirmEmail))



            return await _userManage.CreateAsync(user, model.Password);
        }
    }
}
