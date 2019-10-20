using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    /*
     * Die Hilfsklasse Deletelist nimmt für die DeleteView alle Daten auf
     * die mit Softdeleted zum löschen markiert wurden. Da eine Actionmethode
     * nur ein Datenmodel an die View zurückgeben kann müssen hier mehrere
     * Modeldaten "zusammengelegt" werden um der View einen einfachen Datenzugriff
     * zu ermöglichen.
     */
    [NotMapped]
    public class DeleteList
    {
        public List<Customer> Customers { get; set; }
        public List<Contract> Contracts { get; set; }
        public List<Service> Services { get; set; }
    }
}
