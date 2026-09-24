using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Infrastructure.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly ApplicationDbContext _context;

        public CandidateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertAsync(Candidate candidate)
        {
            await _context.Candidates.AddAsync(candidate);
        }
        public IQueryable<Candidate> Get()
        {
            var candidates = _context.Candidates.AsQueryable();
            return candidates;
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Candidates.AnyAsync(c => c.Id == id);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
