using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class TokenRefreshResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
