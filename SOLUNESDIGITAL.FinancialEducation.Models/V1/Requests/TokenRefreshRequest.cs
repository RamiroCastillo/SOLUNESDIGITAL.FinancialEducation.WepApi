﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class TokenRefreshRequest : Token.TokenPublic
    {
        public string Email { get; set; }
    }
}
