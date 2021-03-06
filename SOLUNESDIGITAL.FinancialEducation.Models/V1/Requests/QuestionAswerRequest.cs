﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class QuestionAswerRequest : Token.TokenPublic
    {
        [Required]
        public int ModuleNumber { get; set; }
        [Required]
        public string UserAplication { get; set; }
        [Required]
        public string PasswordAplication { get; set; }
    }
}
