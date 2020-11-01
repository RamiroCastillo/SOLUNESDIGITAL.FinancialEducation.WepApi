using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string TokenFrefresh { get; set; }
    }
}
