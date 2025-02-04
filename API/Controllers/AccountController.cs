using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Controllers;

public class AccountController(DataContext context,  ITokenService tokenService, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
    {
        if(await UserExits(registerDto.Username)) return BadRequest("Username is taken");
        
        using var hmac = new HMACSHA512();
        var user = mapper.Map<AppUser>(registerDto);
        user.UserName= registerDto.Username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt =hmac.Key;
       

        context.Users.Add(user);
        
        await context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) 
    {
        var user = await context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => 
            x.UserName == loginDto.Username.ToLower());
        
        if(user == null) return Unauthorized("Invaild username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < ComputeHash.Length; i++)
        {
            if(ComputeHash[i] != user.PasswordHash[i]) return Unauthorized("Invaild password");
    
        }

        return new UserDto
        {
            Username = user.UserName,
            KnownAs = user.KnownAs,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url
        };
    } 
    private async Task<bool> UserExits(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());

    }
}
