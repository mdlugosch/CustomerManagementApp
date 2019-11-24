using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    /*
     * Die AppUser Klasse ist abgeleitet von der ASP.NET Core 2 IdentityUser Klasse.
     * In IdentityUser sind die meist verwendeten Eigenschaften wie Username, Claims, Roles
     * usw. schon vordefiniert.
     */
    public class AppUser : IdentityUser { }
}
