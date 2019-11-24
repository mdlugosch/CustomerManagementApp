using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    /*
     * Für den Admin-Controller wird hier die MVC-Model-Validation genutzt.
     * Für jede Aktion die der Admincontroller unterstützt werden, an dieser
     * Stelle ViewModels abgelegt die so eine einfache Nutzung der MVC-Model-Validation
     * zu ermöglichen.
     */
    public class CreateModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
