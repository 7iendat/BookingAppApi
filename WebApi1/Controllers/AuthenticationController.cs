
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using User.Management.Service.Models;
using User.Management.Service.Service;
using WebApi1.Data;
using WebApi1.Models;
using WebApi1.Models.Authentication.SignIn;
using WebApi1.Models.Authentication.SignUp;
using WebApi1.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApi1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAccountRepository accountRepo;
        private readonly UserManager<ApplicationUser> _userManage;
        private readonly RoleManager<IdentityRole> _roleManage;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthenticationController(IAccountRepository repo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> role, IConfiguration configuration, IEmailService emailService) {
            accountRepo = repo;
            _userManage = userManager;
            _roleManage = role;
            _configuration = configuration;
            _emailService = emailService;
        }

       
        [HttpPost("SignUp")]

        public async Task<IActionResult> SignUp(SignUpModel signUpModel, string role)
        {
            try
            {
                //Check User exist
                var userExist = await _userManage.FindByEmailAsync(signUpModel.Email);
                if (userExist != null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "User already exist!" });
                }
                // Add user in the database
                var user = new ApplicationUser
                {
                    FirstName = signUpModel.FirstName,
                    LastName = signUpModel.LastName,
                    UserName = signUpModel.Email,
                    Email = signUpModel.Email,
                };


                if (await _roleManage.RoleExistsAsync(role))
                {
                    IdentityResult result =  await _userManage.CreateAsync(user, signUpModel.Password); ;
                    if (!result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response
                        {
                            Status = "Error",
                            Message = "User failed to create"
                        });
                        //List<IdentityError> errorList = result.Errors.ToList();
                        //string errors = "";

                        //foreach (var error in errorList)
                        //{
                        //    errors = errors + error.Description.ToString();
                        //}

                        //return Content(errors);
                    }

                    await _userManage.AddToRoleAsync(user, role);
                    var token = await _userManage.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email }, Request.Scheme);
                    var message = new User.Management.Service.Models.Message(new string[] { user.Email }, 
                        "Confirmation email link",
                        confirmationLink!);
                    _emailService.SendEmail(message);


                    return StatusCode(StatusCodes.Status200OK, new Response
                    {
                        Status = "Success",
                        Message = $"User created successfully and Send email to {user.Email}"
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role don't exist" });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status416RangeNotSatisfiable, ex);
            }
                           
        }

        [HttpGet("ConfirmEmail")]

        public async Task<IActionResult> ConfirmEmail (string token, string email)
        {
            var user = await _userManage.FindByEmailAsync(email);
            if(user != null)
            {
                var result = await _userManage.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Email verified successfully" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User don't exist" });

        }

        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var user = await _userManage.FindByEmailAsync(signInModel.Email);

            if (user != null && await _userManage.CheckPasswordAsync(user, signInModel.Password))
            {
                var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                var userRoles = await _userManage.GetRolesAsync(user);
                foreach(var role in userRoles)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role, role));
                }
                var jwtToken = GetToken(authClaim);
                return Ok(
                        new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                            expiration = jwtToken.ValidTo,
                        }
                    );
            }
            return Unauthorized();
        }

        private JwtSecurityToken GetToken(List<Claim> authClaim)
        {
            var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires:DateTime.Now.AddHours(3),
                claims: authClaim,
                signingCredentials:new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256));
            return token;
        }
        
    }
}
