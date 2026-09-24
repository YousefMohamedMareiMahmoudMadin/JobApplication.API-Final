using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelJobCandidateApplication
{
    public class CancelJobCandidateApplicationHandler : IRequestHandler<CancelJobCandidateApplicationCommand, JobCandidateApplication?>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;

        public CancelJobCandidateApplicationHandler(IRepository<JobCandidateApplication> jobApplicationRepository)
        {
            _jobApplicationRepository = jobApplicationRepository;
        }

        public async Task<JobCandidateApplication?> Handle(CancelJobCandidateApplicationCommand request, CancellationToken cancellationToken)
        {
            var jobApplication = await _jobApplicationRepository.Get().FirstOrDefaultAsync(a => a.Id == request.Id);
            if (jobApplication == null)
            {
                return null;
            }
            if (jobApplication.CandidateId != request.RequesterId)
            {
                throw new UnauthorizedAccessException("You can only cancel your own application.");
            }
            if (jobApplication.JobApplicationStatus != JobApplicationStatus.Applied && jobApplication.JobApplicationStatus != JobApplicationStatus.UnderReview)
            {
                throw new InvalidOperationException("Application can only be cancelled while status is Applied or UnderReview.");
            }

            jobApplication.JobApplicationStatus = JobApplicationStatus.Cancelled;
            jobApplication.StatusUpdatedAt = DateTime.UtcNow;
            jobApplication.CancelledAt = DateTime.UtcNow;

            _jobApplicationRepository.Update(jobApplication);
            await _jobApplicationRepository.SaveChangesAsync();

            return jobApplication;
        }
    }
}
