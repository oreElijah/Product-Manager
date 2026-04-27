using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Account
{
    public class LoginResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; 
        public string FirstName { get; set; } = string.Empty;    
        public string LastName { get; set; } = string.Empty;    
        public string Token {get; set;} = string.Empty;
    }
}