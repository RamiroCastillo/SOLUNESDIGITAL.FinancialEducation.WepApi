using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class User
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Public { get; set; }
        public string Status { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreadtionDate { get; set; }
        public string ModificationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool State { get; set; }
    }
}
