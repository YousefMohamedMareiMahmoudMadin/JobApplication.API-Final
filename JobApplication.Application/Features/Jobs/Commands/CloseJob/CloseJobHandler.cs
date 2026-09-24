using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobHandler : IRequestHandler<CloseJobCommand, Job?>
    {
        private readonly IRepository<Job> _jobRepository;

        public CloseJobHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Job?> Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.Get().FirstOrDefaultAsync(j => j.Id == request.Id);
            if (job == null)
            {
                return null;
            }
            if (job.RecruiterId != request.RecruiterId)
            {
                throw new UnauthorizedAccessException("You can only close your own job.");
            }
            if (!job.IsActive)
            {
                throw new InvalidOperationException("Job is already closed.");
            }

            job.IsActive = false;
            job.ClosedAt = DateTime.UtcNow;
            job.ClosedBy = request.RecruiterId;

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            return job;
        }
    }
}
