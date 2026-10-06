
using AutoMapper;
using BusBookingSystem.Application.Common;
using BusBookingSystem.Application.DTOs;
using BusBookingSystem.Application.Interfaces.Services;
using BusBookingSystem.Core.Entities;
using BusBookingSystem.Core.Enums;
using BusBookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusBookingSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration.GetSection("JwtSettings")["Secret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException("JwtSettings:Secret is missing or empty.");
            }
            byte[] key = Encoding.ASCII.GetBytes(jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                    new Claim(ClaimTypes.Role, user.UserType.ToString()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> IsEmailExistAsync(String email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<Result<UserDto>> RegisterAsync(UserRegisterRequestDto userRegisterRequestDto)
        {
            try
            {
                if (userRegisterRequestDto is null)
                {
                    return Result<UserDto>.BadRequest($"User registration data is required.");
                }
                //check email exist
                var emailExists = await IsEmailExistAsync(userRegisterRequestDto.Email);
                if (emailExists)
                {
                    return Result<UserDto>.Conflict($"The Email '{userRegisterRequestDto.Email}' already belongs to an existing acount.");
                }

                // map request dto to entity
                User user = _mapper.Map<User>(userRegisterRequestDto);
                user.UserType = UserType.Customer;
                user.PasswordHash = new PasswordHasher<User>().HashPassword(user, userRegisterRequestDto.Password);

                // add entity
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // map entity to response dto
                var responseOk = _mapper.Map<UserDto>(user);

                // return response dto
                return Result<UserDto>.Created(responseOk, "User registered successfully.");
            }
            catch (Exception e)
            {
                return Result<UserDto>.Unexpected("An unexpected error occurred during user registration.", e.Message);
            }

        }

        public async Task<Result<AuthResponseDto>> LoginAsync(UserLoginRequestDto userLoginRequestDto)
        {
            try
            {
                // retrieve user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginRequestDto.Email);

                // same message for unknown email and wrong password, so login does not reveal which emails are registered
                if (user is null)
                {
                    return Result<AuthResponseDto>.UnAuthorized("Invalid email or password.");
                }

                // check password
                if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, userLoginRequestDto.Password) == PasswordVerificationResult.Failed)
                {
                    return Result<AuthResponseDto>.UnAuthorized("Invalid email or password.");
                }

                // map entity to response dto
                var UserDto = _mapper.Map<UserDto>(user);

                // generate jwt token
                var token = GenerateJwtToken(user);

                // return response dto

                var result = new AuthResponseDto
                {
                    UserDto = UserDto,
                    Token = token
                };

                return Result<AuthResponseDto>.Ok(result, "User login successfull");


            }
            catch (Exception e)
            {
                //throw new InvalidOperationException("An unxpected error occured during user log in.", e);
                return Result<AuthResponseDto>.Unexpected("Error occured during user login", e.Message);


            }
        }

    }
}
