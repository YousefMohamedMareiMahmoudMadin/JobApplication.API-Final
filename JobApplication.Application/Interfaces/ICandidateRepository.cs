using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Interfaces
{
    public interface ICandidateRepository
    {
        Task InsertAsync(Candidate candidate);
        IQueryable<Candidate> Get();
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }
}
