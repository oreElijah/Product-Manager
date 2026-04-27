using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.models;

namespace api.Mappers
{
    public static class AccountMapper
    {
     public static RegisterResponseDto ToRegisterResponseDto(this RegisterRequestDto registerRequestDto, string message)
        {
            return new RegisterResponseDto
            {
                Username = registerRequestDto.Username ?? string.Empty,
                Email = registerRequestDto.Email ?? string.Empty,
                Message = message
            };
        }

     public static LoginResponseDto ToLoginResponseDto(this LoginRequestDto loginRequestDto, AppUser user, string token)
        {
            return new LoginResponseDto
            {
                Email = loginRequestDto.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Token = token
            };
        }
    }
}