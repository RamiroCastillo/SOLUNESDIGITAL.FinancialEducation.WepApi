using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class RegistrationCompleteRequest : Token.TokenPublic
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string NameComplete { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateTime Birthdate { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string CellPhone { get; set; }
        public string Phone { get; set; }
        [Required]
        public string EducationLevel { get; set; }
        [Required]
        public bool Disability { get; set; }
        public string TypeDisability { get; set; }
        [Required]
        public string ReferenceName { get; set; }
        [Required]
        public string ReferencePhone { get; set; }
        public string UserAplication { get; set; }
        public string PasswordAplication { get; set; }

    }
}
