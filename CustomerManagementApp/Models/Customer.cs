using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    public class Customer
    {
        public long CustomerId { get; set; }
        public string CompanyName { get; set; }
        public IEnumerable<Contract> Contracts { get; set; }

        public bool SoftDeleted { get; set; } = false;
    }
}
