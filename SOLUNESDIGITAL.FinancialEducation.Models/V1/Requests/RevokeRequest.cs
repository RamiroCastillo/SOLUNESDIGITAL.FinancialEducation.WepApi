using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class RevokeRequest: Token.TokenPublic
    {
        [Required]
        public string Email { get; set; }
        public string Token { get; set; }
        [Required]
        public string UserAplication { get; set; }
        [Required]
        public string PasswordAplication { get; set; }
    }
}
