using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Management.Api.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("name")]

        public IEnumerable<string> Get() {
            return new List<string>
            {
                "Hunter", "Dev", "Sunny"
            };
        }
    }
}
