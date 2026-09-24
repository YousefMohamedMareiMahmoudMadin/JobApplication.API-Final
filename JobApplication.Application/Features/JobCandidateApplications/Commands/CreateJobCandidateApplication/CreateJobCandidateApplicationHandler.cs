using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CreateJobCandidateApplication
{
    public class CreateJobCandidateApplicationHandler : IRequestHandler<CreateJobCandidateApplicationCommand, int>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IBackgroundJobScheduler _backgroundJobScheduler;

        public CreateJobCandidateApplicationHandler(IRepository<JobCandidateApplication> jobApplicationRepository, IRepository<Job> jobRepository, IRepository<Candidate> candidateRepository, IBackgroundJobScheduler backgroundJobScheduler)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobRepository = jobRepository;
            _candidateRepository = candidateRepository;
            _backgroundJobScheduler = backgroundJobScheduler;
        }

        public async Task<int> Handle(CreateJobCandidateApplicationCommand request, CancellationToken cancellationToken)
        {
            var job = await _jobRepository.Get().FirstOrDefaultAsync(j => j.Id == request.JobId);
            if (job == null)
            {
                throw new InvalidOperationException("Job not found.");
            }
            if (!job.IsActive)
            {
                throw new InvalidOperationException("Job is not active.");
            }
            var candidateExists = await _candidateRepository.Get().AnyAsync(c => c.Id == request.CandidateId);
            if (!candidateExists)
            {
                throw new InvalidOperationException("Candidate not found.");
            }
            var duplicateExists = await _jobApplicationRepository.Get().AnyAsync(a => a.JobId == request.JobId && a.CandidateId == request.CandidateId);
            if (duplicateExists)
            {
                throw new InvalidOperationException("Candidate already applied to this job.");
            }

            var jobApplication = new JobCandidateApplication()
            {
                JobId = request.JobId,
                CandidateId = request.CandidateId,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = DateTime.UtcNow,
                StatusUpdatedAt = DateTime.UtcNow
            };
            await _jobApplicationRepository.AddAsync(jobApplication);
            await _jobApplicationRepository.SaveChangesAsync();

            _backgroundJobScheduler.Enqueue<INotificationService>(s => s.NotifyRecruiter(jobApplication.Id));

            return jobApplication.Id;
        }
    }
}
