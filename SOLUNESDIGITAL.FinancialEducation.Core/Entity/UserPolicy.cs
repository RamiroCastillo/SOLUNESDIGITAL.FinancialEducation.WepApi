using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class UserPolicy
    {
        public long Id { get; set; }
        public long IdUser { get; set; }
        public long IdPolicy { get; set; }
        public string AppUserId { get; set; }
        public string Status { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreadtionDate { get; set; }
        public string ModificationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool State { get; set; }

    }
}
