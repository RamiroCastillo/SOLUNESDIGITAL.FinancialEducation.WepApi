using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class AuthenticateResponse
    {
        public string Email { get; set; }
        public int CurrentModule { get; set; }
        public string Role { get; set; }
        public bool Verify { get; set; }
        public bool RegistredCompleted { get; set; }
        public string Token { get; set; }
        [JsonIgnore] 
        public string RefreshToken { get; set; }
    }
}
