﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class VerifyEmailResponse
    {
        public string TokenEmailVerify { get; set; }
        public bool Verify { get; set; }
    }
}
