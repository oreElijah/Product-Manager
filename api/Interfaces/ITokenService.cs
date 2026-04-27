using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.models;
namespace api.Interfaces
{
    public interface ITokenService
    {
        public Task<string> CreateToken(AppUser user);
        public Task<string> CreateRefreshToken(AppUser user);
        public Task Logout(string jti);
    }
}