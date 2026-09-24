using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ICandidateRepository candidateRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _candidateRepository = candidateRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<int> RegisterAsync(RegisterDto registerDto)
        {
            var email = registerDto.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }
            if (registerDto.Password.Length < 6)
            {
                throw new InvalidOperationException("Password must be at least 6 characters.");
            }

            var user = new User
            {
                Name = registerDto.Name,
                Email = email,
                PasswordHash = _passwordHasher.Hash(registerDto.Password),
                Role = registerDto.Role
            };

            if (user.Role == UserRole.Candidate)
            {
                var candidate = new Candidate
                {
                    Name = registerDto.Name,
                    CvUrl = registerDto.CvUrl ?? string.Empty
                };
                await _candidateRepository.InsertAsync(candidate);
                await _candidateRepository.SaveChangesAsync();
                user.CandidateId = candidate.Id;
            }

            await _userRepository.InsertAsync(user);
            await _userRepository.SaveChangesAsync();

            return user.Id;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var email = loginDto.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !_passwordHasher.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            var response = new AuthResponseDto
            {
                Token = _tokenService.GenerateToken(user),
                UserId = user.Id,
                Name = user.Name,
                Role = user.Role,
                CandidateId = user.CandidateId
            };

            return response;
        }
    }
}
