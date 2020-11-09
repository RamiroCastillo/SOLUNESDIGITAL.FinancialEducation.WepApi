using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class VerifyEmailRequest : Token.TokenPublic
    {
        [Required]
        public string TokenEmailVerify { get; set; }
    }
}
