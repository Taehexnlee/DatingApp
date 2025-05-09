using System;
using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>>GetUsersAsync();
    Task<AppUser?>GetUserByIdAsync(int id);
    Task<AppUser?>GetUserbyUsernameAsync(string username);
    Task<PagedList<MemberDto>>GetMembersAsync(UserParams userParams);
    Task<MemberDto?>GetMemberAsync(string username);
}
