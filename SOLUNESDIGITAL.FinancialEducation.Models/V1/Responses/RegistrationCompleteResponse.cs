using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class RegistrationCompleteResponse
    {
        public string Email { get; set; }
        public string NameComplete { get; set; }
        public bool RegistrationComplete { get; set; }
    }
}
