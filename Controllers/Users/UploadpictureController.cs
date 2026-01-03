using Microsoft.AspNetCore.Mvc;
using core8_svelte_sybase.Models;
using AutoMapper;
using core8_svelte_sybase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System;

namespace core8_svelte_sybase.Controllers.Users
{
    [ApiExplorerSettings(GroupName = "Upload User Image")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class UploadpictureController : ControllerBase {

    private IUserService _userService;

    private IMapper _mapper;
    private readonly IConfiguration _configuration;  

    private readonly IWebHostEnvironment _env;

    private readonly ILogger<UploadpictureController> _logger;

    public UploadpictureController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        IUserService userService,
        IMapper mapper,
        ILogger<UploadpictureController> logger
        )
    {
        _configuration = configuration;  
        _userService = userService;
        _mapper = mapper;
        _logger = logger;
        _env = env;        
    }  
        [HttpPost]
        public async Task<IActionResult> uploadPicture([FromForm]UploadfileModel model) {
                if (model.Profilepic.FileName != null)
                {
                    try
                    {
                        string ext= Path.GetExtension(model.Profilepic.FileName);

                        var folderName = Path.Combine("wwwroot", "users/");
                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                        var newFilename =pathToSave + "00" + model.Id + ".jpg";

                        using var image = SixLabors.ImageSharp.Image.Load(model.Profilepic.OpenReadStream());
                        image.Mutate(x => x.Resize(100, 100));
                        image.Save(newFilename);

                        if (model.Profilepic != null)
                        {
                            string file = "https://localhost:7286/users/00"+model.Id.ToString()+".jpg";
                            await _userService.UpdatePicture(model.Id, file);                            
                        }
                        return Ok(new { message = "Profile Picture has been updated."});
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message =ex.Message});
                    }
                }
                return NotFound(new { message = "Profile Picture not found."});
        }
    }
    
}