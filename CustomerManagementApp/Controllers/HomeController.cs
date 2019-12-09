using CustomerManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using CustomerManagementApp.Models.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CustomerManagementApp.Controllers
{
    public class HomeController : Controller
    {
        private IDataRepository repository;
        TempCompanyData TempCompanyData = TempCompanyData.getInstance();

        private UserManager<AppUser> userManager;
        private Task<AppUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);

        public HomeController(IDataRepository repo)
        {
            repository = repo;
        }

        #region Controllermethoden Datenübersicht
        public IActionResult Index(QueryOptions options) => View(repository.GetAllCustomers(options));

        /*
         * ServiceDetails fällt aus dem Rahmen der Namensgebung. Wie bei Index handelt es sich um eine
         * Übersicht. Da es zu unübersichtlich ist im Index Firmen-, Vertrags- und Dienstleistungsdaten
         * anzuzeigen habe ich mich entschieden im Index nur die Firmen mit den dazugehörenden Verträgen
         * anzuzeigen. Die Dienstleistungen werden in einer separaten Übersicht aufgeführt welche über einen
         * Button des jeweiligen Vertrages erreichbar sind.
         */
        [HttpPost]
        public IActionResult ServiceDetails(DateTime Startdate, string CompanyName, long ContractId, QueryOptions options)
        {
            /*
             * Sollten die Parameter Startdate, CompanyName oder ContractId leer übertragen werden wird versucht
             * die Daten aus der temporären TempCompanyData Instanz zu holen. Die Daten werden in den ServiceDetails
             * zur Identifikationsanzeige benötigt.
             */
            long cId = ContractId == 0 ? TempCompanyData.ContractId : ContractId;
            string cName = String.IsNullOrEmpty(CompanyName) ? TempCompanyData.CompanyName : CompanyName;
            DateTime? sDate = (Startdate == DateTime.MinValue) ? TempCompanyData.Startdate : Startdate;

            ViewBag.Startdate = sDate;
            ViewBag.CompanyName = cName;
            ViewBag.ContractId = cId;

            TempCompanyData.Startdate = sDate;
            TempCompanyData.CompanyName = cName;
            TempCompanyData.ContractId = cId;

            return View(repository.GetAllServicesFromContract(cId, options));
        }
        #endregion

        #region Controllermethoden der Bearbeitungsmasken
        /*
         * Bei neuen Kunden ist CustomerId gleich Null und es wird ein Leeres Customer-Objekt
         * übertragen. Wird eine CustomerId mitübertragen wird der entsprechende Datensatz
         * aus Datenbank an die View übertragen.
         */
        [HttpPost]
        public IActionResult EditCustomer(long CustomerId)
        {
            return View(CustomerId == 0 ? new Customer() : repository.GetCustomerById(CustomerId));
        }

        /*
         * In der View EditContract wird ein Optionsfeld genutzt das dem Anwender
         * ermöglicht Verträge über den Firmennamen zuzuordnen anstatt über Ids.
         * Dafür wird die Repository Methode CustomerList() und die Liste CustomerItems benötigt.
         * Im Selektionsfeld der Vertragsmaske wird die Firma voreingestellt unter der, der neue
         * Vertrag erstellt werden soll.
         */
        [HttpPost]
        public IActionResult EditContract(long ContractId, long customerId)
        {
            var CustomerItems = repository.CustomerList();
            ViewData["CustomerItems"] = CustomerItems;

            Contract newItem = new Contract() { CustomerId = customerId };

            return View(ContractId == 0 ? newItem : repository.GetContractById(ContractId));
        }

        /*
         * In der View EditService wird ein Optionsfeld genutzt das dem Anwender
         * ermöglicht Dienstleistungen über Vertragdaten zuzuordnen anstatt über Ids.
         * Dafür wird die Repository Methode ContractList() und die Liste ContractItems benötigt.
         * Aus den temporär abgelegten Daten wird die Vertrags-ID bei neuen Objekten mitgegeben
         * damit das Selektionsfeld der Service-Editiermaske den Vertrag voreingestellt bekommt
         * unter dem die neue Dienstleistung hinzugefügt werden soll.
         */
        [HttpPost]
        public IActionResult EditService(long ServiceId)
        {
            var ContractItems = repository.ContractList();
            ViewData["ContractItems"] = ContractItems;

            Service newItem = new Service() { ContractId = TempCompanyData.ContractId };

            return View(ServiceId == 0 ? newItem : repository.GetServiceById(ServiceId));
        }
        #endregion

        #region Updatefunktionen der Editiermasken
        /*
         * Ein Objekt mit 0 Id ist ein neues Objekt und wird über das Repository an den Context angehangen.
         * Ist die Id nicht 0 handelt es sich um ein Update eines vorhandenen Objekts und wird durch die
         * entsprechende Upatefunktion im Repository behandelt.
         */
        public IActionResult UpdateCustomer(Customer customer, Customer original)
        {
            if (string.IsNullOrEmpty(customer.CompanyName)) ModelState.AddModelError(nameof(customer.CompanyName), "Bitte geben Sie einen Firmennamen an!");

            if (ModelState.GetValidationState(nameof(customer.CompanyName)) == ModelValidationState.Valid)
            {
                if (customer.CustomerId == 0)
                {
                    repository.AddCustomer(customer);
                }
                else
                {
                    repository.UpdateCustomer(customer, original);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("EditCustomer", original);
            }
               
        }

        public IActionResult UpdateContract(Contract contract, Contract original)
        {
            var CustomerItems = repository.CustomerList();
            ViewData["CustomerItems"] = CustomerItems;

            /*
             * Für den Vertrag habe ich festgelegt das Sonderfälle berücksichtigt werden bei denen EK oder VK 0 Euro betragen.
             * Negative Werte für EK und VK sollen nicht erlaubt sein. In allen Fällen darf der EK nicht größer als VK sein.
             */
            decimal PurchasePriceValue;
            decimal RetailPriceValue;

            DateTime startdateValue = new DateTime();
            DateTime enddateValue = new DateTime();

            if (ParseStringToDecimal(contract.StrPurchasePrice, out PurchasePriceValue))
            {
                contract.PurchasePrice = PurchasePriceValue;
            }
            else ModelState.AddModelError(nameof(contract.StrPurchasePrice), "Hier ist nur ein Zahlenwert erlaubt!");

            if (ParseStringToDecimal(contract.StrRetailPrice, out RetailPriceValue))
            {
                contract.RetailPrice = RetailPriceValue;
            }
            else ModelState.AddModelError(nameof(contract.StrRetailPrice), "Hier ist nur ein Zahlenwert erlaubt!");

            if (string.IsNullOrEmpty(contract.StrStartdate)) ModelState.AddModelError(nameof(contract.StrStartdate), "Der Vertrag benötigt ein Startdatum!");
            else if (ParseStringToDate(contract.StrStartdate, out startdateValue))
            {
                contract.Startdate = startdateValue;
            }
            else ModelState.AddModelError(nameof(contract.StrStartdate), "Fehler: Datumsfeld (dd.mm.yyyy)");

            if (ParseStringToDate(contract.StrEnddate, out enddateValue))
            {
                contract.Enddate = enddateValue;
            }
            else ModelState.AddModelError(nameof(contract.StrEnddate), "Fehler: Datumsfeld (dd.mm.yyyy)");

            if (PurchasePriceValue < 0M) ModelState.AddModelError(nameof(contract.StrPurchasePrice), "Einkaufspreis muss Null oder größer Null sein!");
            if (RetailPriceValue < 0M) ModelState.AddModelError(nameof(contract.StrRetailPrice), "Verkaufspreis muss Null oder größer Null sein!");
            if (PurchasePriceValue > 0M && RetailPriceValue > 0M && PurchasePriceValue > RetailPriceValue) ModelState.AddModelError(nameof(contract.StrPurchasePrice), "Einkaufspreis muss höher oder gleich dem Verkaufspreis sein!");

            if (startdateValue > enddateValue) ModelState.AddModelError(nameof(contract.StrEnddate), "Endedatum muss nach dem Startdatum liegen!");

            if (ModelState.GetValidationState(nameof(contract.StrStartdate)) == ModelValidationState.Valid &&
            ModelState.GetValidationState(nameof(contract.StrEnddate)) == ModelValidationState.Valid &&
            ModelState.GetValidationState(nameof(contract.StrPurchasePrice)) == ModelValidationState.Valid &&
            ModelState.GetValidationState(nameof(contract.StrRetailPrice)) == ModelValidationState.Valid)
            {
                if (contract.ContractId == 0)
                {
                    repository.AddContract(contract);
                }
                else
                {
                    repository.UpdateContract(contract, original);
                }
                return RedirectToAction(nameof(Index));
            }
            else return View("EditContract", original);
        }

        [HttpPost]
        public IActionResult UpdateService(Service service, Service original, QueryOptions options)
        {
            ViewBag.Startdate = TempCompanyData.Startdate;
            ViewBag.CompanyName = TempCompanyData.CompanyName;

            var ContractItems = repository.ContractList();
            ViewData["ContractItems"] = ContractItems;

            if (string.IsNullOrEmpty(service.ServiceDescription)) ModelState.AddModelError(nameof(service.ServiceDescription), "Bitte geben Sie eine Dienstleistungsbeschreibung ein!");

            if (ModelState.GetValidationState(nameof(service.ServiceDescription)) == ModelValidationState.Valid)
            { 
                if (service.ServiceId == 0)
                {
                    repository.AddService(service);
                }
                else
                {
                    repository.UpdateService(service, original);
                }
                return View("ServiceDetails", repository.GetAllServicesFromContract(TempCompanyData.ContractId, options));
            }
            return View("EditService", original);
        }
        #endregion

        #region Funktionen für permanentes Löschen
        /*
         * Die DeleteList besteht aus den zum Löschen markierten Einträgen.
         * Die aufgeführten Löschmethoden übergeben ein Objekt an das Repository
         * wo diese dann permanent aus der Datenbank entfernt werden.
         */
        [Authorize(Roles = "Users")]
        public IActionResult DeleteView()
        {
            DeleteList deleteList = repository.CreateDeleteList();
            return View("DeleteView", deleteList);
        }

        public IActionResult RemoveCustomer(Customer customer)
        {
            repository.DeleteCustomer(customer);
            return RedirectToAction("DeleteView");
        }

        public IActionResult RemoveContract(Contract contract)
        {
            repository.DeleteContract(contract);
            return RedirectToAction("DeleteView");
        }

        public IActionResult RemoveService(Service service)
        {
            repository.DeleteService(service);
            return RedirectToAction("DeleteView");
        }
        #endregion

        #region  Reversible Löschfunktionen
        /*
         * Wird ein Kunde(Customer) gelöscht müssen alle zugehörigen Verträge und deren Dienstleistungen
         * ebenfalls zum löschen markiert werden.
         */
        [HttpPost]
        public IActionResult SoftDeleteCustomer(long CustomerId)
        {
            repository.GetCustomerById(CustomerId).SoftDeleted = true;

            IEnumerable<Contract> contracts = repository.GetContractsByCustomerId(CustomerId, true);

            foreach (Contract contract in contracts)
            {
                contract.SoftDeleted = true;
                foreach (Service service in contract.Services)
                {
                    service.SoftDeleted = true;
                }
            }

            repository.UpdateData();
            return RedirectToAction(nameof(Index));
        }

        /*
         * Beim löschen von Verträgen verhält es sich ähnlich wie beim löschen eines Kunden.
         * Wird ein Vertrag als gelöscht markiert müssen alle Dienstleistungen(Services) die
         * diesem zugeordnet sind ebenfalls zum Löschen markiert werden.
         */
        [HttpPost]
        public IActionResult SoftDeleteContract(long ContractId)
        {
            Contract contract = repository.GetContractById(ContractId);
            contract.SoftDeleted = true;
            foreach(Service service in contract.Services)
            {
                service.SoftDeleted = true;
            }

            repository.UpdateData();
            return RedirectToAction(nameof(Index));
        }

        /*
         * Da Dienstleistungen keine Unterelemente haben müssen die Dienstleistungen nur als gelöscht markiert werden.
         * Hinweis:
         * Sollten die Parameter Startdate, CompanyName oder ContractId leer übertragen werden wird versucht
         * die Daten aus der temporären TempCompanyData Instanz zu holen. Die Daten werden in den ServiceDetails
         * zur Identifikationsanzeige benötigt.
         */
        [HttpPost]
        public IActionResult SoftDeleteService(long ContractId,DateTime Startdate,String CompanyName, long ServiceId, QueryOptions options)
        {
            long cId = ContractId == 0 ? TempCompanyData.ContractId : ContractId;
            string cName = String.IsNullOrEmpty(CompanyName) ? TempCompanyData.CompanyName : CompanyName;
            DateTime? sDate = (Startdate == DateTime.MinValue) ? TempCompanyData.Startdate : Startdate;

            ViewBag.ContractId = cId;
            ViewBag.Startdate = sDate;
            ViewBag.CompanyName = cName;

            repository.GetServiceById(ServiceId).SoftDeleted = true;
            repository.UpdateData();
                                       
            return View("ServiceDetails", repository.GetAllServicesFromContract(ContractId, options));
        }
        #endregion

        #region Wiederherstellen von Daten
        /*
         * Das Feature des nicht permantenten Löschens wird in den Models über die boolsche Property
         * SoftDeleted gelöst. Daten die als gelöscht markiert sind werden durch einen Filter aus den
         * Übersichten ausgeschlossen und nur in der dafür vorgesehen DeleteView aufgeführt. Dort kann
         * dann entschieden werden ob Daten permanent gelöscht oder wiederhergestellt werden.
         */
        [HttpPost]
        public IActionResult RestoreCustomer(long CustomerId)
        {
            repository.GetCustomerById(CustomerId,true).SoftDeleted = false;

            IEnumerable<Contract> contracts = repository.GetContractsByCustomerId(CustomerId,true);
            foreach (Contract v in contracts)
            {
                v.SoftDeleted = false;
                foreach(Service s in v.Services)
                {
                    s.SoftDeleted = false;
                }
            }

            repository.UpdateData();
            return RedirectToAction("DeleteView");
        }

        [HttpPost]
        public IActionResult RestoreContract(long ContractId)
        {
            Contract contract = repository.GetContractById(ContractId, true);
            contract.SoftDeleted = false;

            foreach (Service s in contract.Services) 
            {
                s.SoftDeleted = false;
            }

            repository.UpdateData();

            return RedirectToAction("DeleteView");
        }

        [HttpPost]
        public IActionResult RestoreService(long ServiceId)
        {
            repository.GetServiceById(ServiceId,true).SoftDeleted = false;
            repository.UpdateData();
            return RedirectToAction("DeleteView");
        }
        #endregion

        #region Hilfsmethoden
        /*
         * Die Editiermaske für Services stellt eine Ausnahme dar. Wenn Abbrechen gewählt wird,
         * soll nicht zum Index zurückgekehrt werden sondern zu View ServiceDetails, da der
         * Aufruf der Editiermaske von hier kam und nicht von der Index-View. Um die Inhalte
         * der ServiceDetails darstellen zu können wird auch hier TempCompanyData genutzt.
         */
        [HttpPost]
        public IActionResult BackToServiceDetails(QueryOptions options)
        {
            ViewBag.Startdate = TempCompanyData.Startdate;
            ViewBag.CompanyName = TempCompanyData.CompanyName;

            return View("ServiceDetails", repository.GetAllServicesFromContract(TempCompanyData.ContractId, options));
        }

        public bool ParseStringToDecimal(string strVal,out decimal dval)
        {
            return decimal.TryParse(strVal, out dval);
        }

        public bool ParseStringToDate(string strVal, out DateTime dateValue)
        {
            return DateTime.TryParseExact(strVal, "dd.MM.yyyy", null, DateTimeStyles.None, out dateValue);
        }
        #endregion
    }
}
