using JobApplication.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommand : IRequest<Job?>
    {
        public int Id { get; set; }
        public int RecruiterId { get; set; }
    }
}
