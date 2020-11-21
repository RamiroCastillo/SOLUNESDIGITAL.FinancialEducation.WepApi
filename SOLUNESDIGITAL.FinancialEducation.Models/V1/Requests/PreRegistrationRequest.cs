using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class PreRegistrationRequest : Token.TokenPublic
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Ci { get; set; }
        public string CiExpedition { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public bool AcceptTerms { get; set; }

    }
}
