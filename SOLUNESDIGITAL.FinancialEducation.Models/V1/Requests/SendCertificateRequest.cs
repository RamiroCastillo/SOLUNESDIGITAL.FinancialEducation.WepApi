using Microsoft.AspNetCore.Http;
using System;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class SendCertificateRequest : Token.TokenPublic
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public List<CertificateParameter> CertificateParameters { get; set; }
        public class CertificateParameter 
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public float FontSizeLabel { get; set; }
            public float FontSize { get; set; }
            public float VerticalTextDirection { get; set; }
            public float HorizontalTextDirection { get; set; }

        }

    }
}
