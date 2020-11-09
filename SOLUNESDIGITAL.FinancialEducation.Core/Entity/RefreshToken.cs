using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public long IdClient { get; set; }
        public string EmailClient { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string RevokedByIp { get; set; }
        public string ReplacedByToken { get; set; }
        public bool IsActive { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreadtionDate { get; set; }
        public string ModificationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool State { get; set; }
    }
}
