using AutoMapper;
using core8_svelte_sybase.Helpers;
using core8_svelte_sybase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace core8_svelte_sybase.Controllers.Users
{
    [ApiExplorerSettings(GroupName = "Delete User")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DeleteController : ControllerBase {

    private IUserService _userService;

    private IMapper _mapper;
    private readonly IConfiguration _configuration;  

    private readonly IWebHostEnvironment _env;

    private readonly ILogger<DeleteController> _logger;

    public DeleteController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        IUserService userService,
        IMapper mapper,
        ILogger<DeleteController> logger
        )
    {
        _configuration = configuration;  
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        _env = env;        
    }  

        [HttpDelete]
        public async Task<IActionResult> deleteUser(int id) {
            try
            {
              await _userService.Delete(id);
            return Ok(new {statuscode = 200, message = "Successfully Deleted."});
           }
            catch (AppException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }

        }
    }
}