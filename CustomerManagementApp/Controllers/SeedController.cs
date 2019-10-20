using CustomerManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Controllers
{
    public class SeedController : Controller
    {
        private IDataRepository repository;

        public SeedController(IDataRepository repo)
        {
            repository = repo;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllData());
        }

        [HttpPost]
        public IActionResult CreateSeedData()
        {
            ClearData();
            repository.AddSeedData();
            repository.UpdateData();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ClearData()
        {
            repository.ClearData();
            return RedirectToAction(nameof(Index));
        }
    }
}
