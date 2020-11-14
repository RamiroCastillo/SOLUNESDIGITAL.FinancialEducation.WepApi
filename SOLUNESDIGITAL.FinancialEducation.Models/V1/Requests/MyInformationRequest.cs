using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class MyInformationRequest : Token.TokenPublic
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserAplication { get; set; }
        [Required]
        public string PasswordAplication { get; set; }
    }
}
