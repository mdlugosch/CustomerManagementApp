using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    public class Service
    {
        public long ServiceId { get; set; }
        public long ContractId { get; set; }
        public string ServiceDescription { get; set; }

        public bool SoftDeleted { get; set; } = false;

        /*
         * Zusätzliche Informationen die für die Löschübersicht(DeleteView) genutzt werden
         */
        [NotMapped]
        public string CompanyName { get; set; }
        [NotMapped]
        public DateTime? Startdate { get; set; }
    }
}
