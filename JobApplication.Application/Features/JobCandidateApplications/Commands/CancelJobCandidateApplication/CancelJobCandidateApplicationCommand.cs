using JobApplication.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelJobCandidateApplication
{
    public class CancelJobCandidateApplicationCommand : IRequest<JobCandidateApplication?>
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
    }
}
