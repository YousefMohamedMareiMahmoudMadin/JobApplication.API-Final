using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface IUserRepository
    {
        Task InsertAsync(User user);
        IQueryable<User> Get();
        Task<User?> GetByEmailAsync(string email);
        Task SaveChangesAsync();
    }
}
