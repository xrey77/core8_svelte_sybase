using core8_svelte_sybase.Entities;
using core8_svelte_sybase.Helpers;
using core8_svelte_sybase.Models.dto;
using core8_svelte_sybase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace core8_svelte_sybase.Controllers.Users
{
    [ApiExplorerSettings(GroupName = "Update User")]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UpdateController : ControllerBase {
        
    private IUserService _userService;

    private IMapper _mapper;
    private readonly IConfiguration _configuration;  

    private readonly IWebHostEnvironment _env;

    private readonly ILogger<UpdateController> _logger;

    public UpdateController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        IUserService userService,
        IMapper mapper,
        ILogger<UpdateController> logger
        )
    {
        _configuration = configuration;  
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        _env = env;        
    }  

        [HttpPatch("/api/updateprofile/{id}")]        
        public async Task<IActionResult> updateUser(int id, [FromBody]UserUpdate model) {
            var user = _mapper.Map<User>(model);
            user.Id = id;
            user.FirstName = model.Firstname;
            user.LastName = model.Lastname;
            user.Mobile = model.Mobile;
            try
            {
                await _userService.UpdateProfile(user);
                return Ok(new {message="Your profile has been updated.",user = model});
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}