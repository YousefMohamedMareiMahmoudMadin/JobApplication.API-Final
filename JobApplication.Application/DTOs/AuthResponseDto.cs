using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public int? CandidateId { get; set; }
    }
}
