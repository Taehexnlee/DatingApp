using System;
using API.Entities;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository{get;}
    IMessageRepository MessageRepository{get;}
    ILikesRepository LikesRepository{get;}
    Task<bool> Complete();
    bool HasChanges();
}
