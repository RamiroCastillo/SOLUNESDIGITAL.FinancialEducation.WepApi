using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class Client
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Ci { get; set; }
        public string CiExpedition { get; set; }
        public string NameComplete { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public int Age { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string CellPhone { get; set; }
        public string Phone { get; set; }
        public string EducationLevel { get; set; }
        public bool Disability { get; set; }
        public string TypeDisability { get; set; }
        public string ReferenceName { get; set; }
        public string ReferencePhone { get; set; }
        public bool AcceptTerms { get; set; }
        public Role Role { get; set; }
        public string VerificationTokenEmail { get; set; } 
        public DateTime? Verified { get; set; }
        public bool IsVerified { get; set; }
        public string ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public DateTime? PasswordReset { get; set; }
        public int CurrentModule { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public bool CompleteRegister { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreadtionDate { get; set; }
        public string ModificationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool State { get; set; }
        public bool VerifyExists { get; set; }

        public class RefreshToken
        {
            public int Id { get; set; }
            public long IdCliente { get; set; }
            public string Token { get; set; }
            public DateTime Expires { get; set; }
            public bool IsExpired => DateTime.UtcNow >= Expires;
            public string CreatedByIp { get; set; }
            public DateTime? Revoked { get; set; }
            public string RevokedByIp { get; set; }
            public string ReplacedByToken { get; set; }
            public bool IsActive => Revoked == null && !IsExpired;
            public string CreationUser { get; set; }
            public DateTime CreadtionDate { get; set; }
            public string ModificationUser { get; set; }
            public DateTime ModificationDate { get; set; }
            public bool State { get; set; }
        }
    }
}
