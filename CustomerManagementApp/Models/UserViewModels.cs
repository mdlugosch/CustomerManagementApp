using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    /*
     * Für den Admin-Controller wird hier die MVC-Model-Validation genutzt.
     * Für jede Aktion die im Admincontroller unterstützt werden an dieser
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

    public class LoginModel
    {
        [Required]
        [UIHint("email")]
        public string Email { get; set; }

        [Required]
        [UIHint("password")]
        public string Password { get; set; }
    }

    /*
     * Im folgenden Abschnitt werden Anwender und Rollen
     * miteinander in beziehung gesetzt.
     */
    public class RoleEditModel
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
}
