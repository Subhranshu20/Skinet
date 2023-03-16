using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using core.Entities.Identity;
using core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        public UserManager<AppUser> _userManager { get; }
        public SignInManager<AppUser> _signInManager { get; }
        public ITokenService _tokenService { get; }
           public IMapper _mapper { get; }
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
     
       
        ,ITokenService tokenService , IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;

        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // var email = HttpContext.User?.Claims?.FirstOrDefault(x=> x.Type==ClaimTypes.Email)?.Value;
            // var user = await _userManager.FindByEmailAsync(email);
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddress(User);
            return new UserDto{
                Email=user.Email,
                Token = _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }
        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManager.FindByEmailAsync(email)!= null;
        }
        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            //var email = HttpContext.User?.Claims?.FirstOrDefault(x=> x.Type==ClaimTypes.Email)?.Value;
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddress(User);
            //return user.Address;
            return _mapper.Map<Address,AddressDto>(user.Address);
        }
         [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto addressDto)
        {
            var user = await _userManager.FindByUserByClaimsPrincipleWithAddress(HttpContext.User);
            user.Address = _mapper.Map<AddressDto,Address>(addressDto);
            var result = await _userManager.UpdateAsync(user);
            if(!result.Succeeded) return Ok(_mapper.Map<Address,AddressDto>(user.Address));
            return BadRequest("Problem Updating the User");
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user == null) return Unauthorized(new ApiResponse(401));
            var result = await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password,false);
            if(!result.Succeeded) return Unauthorized(new ApiResponse(401));
            return new UserDto{
                Email=user.Email,
                Token = _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(CheckEmailExistsAsync(registerDto.Email).Result.Value) 
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse 
                    { Errors = new[] { "Email address is in use" } });
            }
            var user = new AppUser{
                DisplayName=registerDto.DisplayName,
                Email= registerDto.Email,
                UserName=registerDto.Email                
            };

            var result = await _userManager.CreateAsync(user,registerDto.Password);
            if(!result.Succeeded) return BadRequest(new ApiResponse(400));
            
            return new UserDto{
                Email=user.Email,
                Token = _tokenService.CreateToken(user),
                DisplayName = user.DisplayName
            };
        }
    }
}