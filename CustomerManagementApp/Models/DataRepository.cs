using CustomerManagementApp.Models.Pages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    public interface IDataRepository
    {
        PagedList<Customer> GetAllData(QueryOptions options);
        PagedList<Customer> GetAllCustomers(QueryOptions options);
        Customer GetCustomerById(long customerId, bool ignoreQueryFilter = false);
        Contract GetContractById(long contractId, bool ignoreQueryFilter = false);
        Service GetServiceById(long serviceId, bool ignoreQueryFilter = false);
        IEnumerable<Contract> GetContractsByCustomerId(long customerId, bool ignoreQueryFilter = false);
        void DeleteCustomer(Customer customer);
        void DeleteContract(Contract contract);
        void DeleteService(Service service);
        void UpdateData();
        void UpdateCustomer(Customer changedCustomer, Customer originalCustomer = null);
        void UpdateContract(Contract changedContract, Contract originalContract = null);
        void UpdateService(Service changedService, Service originalService = null);
        void AddCustomer(Customer customer);
        void AddContract(Contract contract);
        void AddService(Service service);
        IEnumerable<Service> GetAllServicesFromContract(long contractId);
        List<KeyValuePair<long, string>> CustomerList();
        List<KeyValuePair<long, string>> ContractList();
        DeleteList CreateDeleteList();
        void AddSeedData();
        void ClearData();
    }

    public class DataRepository : IDataRepository
    {
        private DataContext context;
        private TempCompanyData TempCompanyData;

        public DataRepository(DataContext ctx)
        {
            context = ctx;
        }

        public PagedList<Customer> GetAllCustomers(QueryOptions options)
        {
            return new PagedList<Customer>(context.Customers.OrderBy(c => c.CustomerId).Include(v => v.Contracts), options);
        }

        public PagedList<Customer> GetAllData(QueryOptions options)
        {
            return new PagedList<Customer>(context.Customers.IgnoreQueryFilters().OrderBy(c => c.CustomerId).Include(v => v.Contracts).ThenInclude(s => s.Services).IgnoreQueryFilters(), options);
        }

        #region GetItemById
        /*
         * Für die Softdeletefunktion muss mit IgnoreQueryFilters() der im DataContext
         * eingestellte Filter umgangen werden damit Datensätze die als gelöscht makiert
         * sind in die Abfrage einbezogen werden. Wenn customerId = 0 dann wird ein
         * leeres Objekt zurückgeliefert.
         */
        public Customer GetCustomerById(long customerId, bool ignoreQueryFilter = false)
        {
            if (ignoreQueryFilter)
                return context.Customers.IgnoreQueryFilters().First(c => c.CustomerId == customerId);
            else
                return context.Customers.First(c => c.CustomerId == customerId);
        }

        public Contract GetContractById(long contractId, bool ignoreQueryFilter = false)
        {
            if (ignoreQueryFilter)
                return context.Contracts.IgnoreQueryFilters().Where(v => v.ContractId == contractId).Include(s => s.Services).IgnoreQueryFilters().FirstOrDefault();
            else
                return context.Contracts.Where(v => v.ContractId == contractId).Include(s => s.Services).FirstOrDefault();
        }

        public Service GetServiceById(long serviceId, bool ignoreQueryFilter = false)
        {
            if (ignoreQueryFilter)
                return context.Services.IgnoreQueryFilters().First(s => s.ServiceId == serviceId);
            else
                return context.Services.First(s => s.ServiceId == serviceId);
        }
        #endregion

        public IEnumerable<Contract> GetContractsByCustomerId(long customerId, bool ignoreQueryFilter = false)
        {
            if (ignoreQueryFilter)
                return context.Contracts.IgnoreQueryFilters().Where(v => v.CustomerId == customerId).Include(v => v.Services).IgnoreQueryFilters();
            else
                return context.Contracts.Where(v => v.CustomerId == customerId).Include(v => v.Services);
        }

        public IEnumerable<Service> GetAllServicesFromContract(long contractId)
        {
            return context.Services.Where(s => s.ContractId == contractId);
        }

        #region Neue Objekte hinzufügen
        /*
         * Einfach Methoden die einen Datensatz über die Id
         * der Datenbank hinzufügen
         */
        public void AddCustomer(Customer customer)
        {
            context.Customers.Add(customer);
            context.SaveChanges();
        }

        public void AddContract(Contract contract)
        {
            context.Contracts.Add(contract);
            context.SaveChanges();
        }

        public void AddService(Service service)
        {
            context.Services.Add(service);
            context.SaveChanges();
        }
        #endregion

        #region DeleteItem
        /*
         * Einfach Löschmethoden die einen Datensatz über die Id
         * aus der Datenbank entfernen
         */
        public void DeleteCustomer(Customer customer)
        {
            context.Customers.Remove(customer);
            context.SaveChanges();
        }

        public void DeleteContract(Contract contract)
        {
            context.Contracts.Remove(contract);
            context.SaveChanges();
        }

        public void DeleteService(Service service)
        {
            context.Services.Remove(service);
            context.SaveChanges();
        }
        #endregion

        #region Updatemethoden
        /*
         * Eine einfache Updatemethode die für Änderungen beim Wiederherstellen oder
         * löschen von Daten genutzt wird.
         */
        public void UpdateData()
        {
            context.SaveChanges();
        }

        /*
         * Updatemethoden die Entitäten vor dem Speichern mit dem Original abgleichen
         * damit Entity Framework Core nur Änderungen übernimmt.
         */
        public void UpdateCustomer(Customer changedCustomer, Customer originalCustomer = null)
        {
                if (originalCustomer == null)
                {
                    originalCustomer = context.Customers.Find(changedCustomer.CustomerId);
                }
                else
                {
                    context.Customers.Attach(originalCustomer);
                }
                originalCustomer.CompanyName = changedCustomer.CompanyName;
                originalCustomer.Contracts = changedCustomer.Contracts;
                originalCustomer.SoftDeleted = changedCustomer.SoftDeleted;

            context.SaveChanges();
        }

        public void UpdateContract(Contract changedContract, Contract originalContract = null)
        {
            if (originalContract == null)
            {
                originalContract = context.Contracts.Find(changedContract.CustomerId);
            }
            else
            {
                context.Contracts.Attach(originalContract);
            }
            originalContract.CustomerId = changedContract.CustomerId;
            originalContract.Startdate = changedContract.Startdate;
            originalContract.Enddate = changedContract.Enddate;
            originalContract.PurchasePrice = changedContract.PurchasePrice;
            originalContract.RetailPrice = changedContract.RetailPrice;
            originalContract.Services = changedContract.Services;
            originalContract.SoftDeleted = changedContract.SoftDeleted;

            context.SaveChanges();
        }

        public void UpdateService(Service changedService, Service originalService = null)
        {
            if (originalService == null)
            {
                originalService = context.Services.Find(changedService.ContractId);
            }
            else
            {
                context.Services.Attach(originalService);
            }
            originalService.ContractId = changedService.ContractId;
            originalService.ServiceDescription = changedService.ServiceDescription;
            originalService.SoftDeleted = changedService.SoftDeleted;

            context.SaveChanges();
        }
        #endregion

        /*
         * In EditContract kann über ein Dropdownfeld eine firma für den jeweiligen Vertrag festgelegt werden.
         * Da Id's nicht aussagekräftig sind habe ich ein KeyValuePair als Lösung gewählt. Die Id ist der Key
         * der in der Datenbank abgelegt wird während das Value den Firmennamen enthält der in dem Dropdownfeld
         * dem Anwender angezeigt wird.
         */
        public List<KeyValuePair<long, string>> CustomerList()
        {
            var CustomerItems = new List<KeyValuePair<long, string>>();
            foreach (Customer c in context.Customers)
            {
                CustomerItems.Add(new KeyValuePair<long, string>(c.CustomerId, c.CompanyName));
            }
            return CustomerItems;
        }

        /*
         * In der Maske EditService kann über ein Dropdownfeld eine Dienstleistung an einen Vertrag
         * angehanden werden. Statt wenig hilfreiche Id's im Feld einzutragen habe ich hier ein
         * KeyValuePair als Lösung genommen. Der Key ist die Id  und der Wert der in der Datenbank
         * abgelegt wird während der Anwender im Dropdownfeld als Value einen zusammengesetzten String
         * zu sehen bekommt der den Vertrag identifiziert(Firmenname und Datum).
         */
        public List<KeyValuePair<long, string>> ContractList()
        {
            var ContractItems = new List<KeyValuePair<long, string>>();

            foreach (Contract v in context.Contracts)
            {
                string contractIdentCode = (context.Customers.First(c => c.CustomerId == v.CustomerId)).CompanyName + " vom: " + v.Startdate.Value.ToString("dd. MM. yyyy");
                ContractItems.Add(new KeyValuePair<long, string>(v.ContractId, contractIdentCode));
            }
            return ContractItems;
        }

        /*
         * Hier wird die Liste der zu löschenden Einträge erstellt. Diese Liste wird temporär im DeleteList Objekt gehalten. 
         */
        public DeleteList CreateDeleteList()
        {
            DeleteList deleteList = new DeleteList();

            deleteList.Customers = context.Customers.Where(c => c.SoftDeleted).IgnoreQueryFilters().ToList();
            List<long> CustomerIds = (from c in deleteList.Customers select c.CustomerId).ToList();

            deleteList.Contracts = context.Contracts.Where(v => v.SoftDeleted && !CustomerIds.Contains(v.CustomerId)).IgnoreQueryFilters().ToList();
            long[] ContractIds = (from v in context.Contracts.Where(v => v.SoftDeleted).IgnoreQueryFilters() select v.ContractId).ToArray();

            foreach (Contract v in deleteList.Contracts)
            {
                v.CompanyName = (from c in context.Customers.Where(c => c.CustomerId == v.CustomerId).IgnoreQueryFilters() select c.CompanyName).FirstOrDefault();
            }

            deleteList.Services = context.Services.Where(s => s.SoftDeleted && !ContractIds.Contains(s.ContractId)).IgnoreQueryFilters().ToList();

            /*
             * Zusätzlich zu den eigentlichen Einträgen werden noch zusätzliche Informationen abgelegt die
             * mit den Einträgen zusammen auf der DeleteView angezeigt werden.
             */
            foreach (Service s in deleteList.Services)
            {
                long customerId = (from v in context.Contracts.Where(v => v.ContractId == s.ContractId).IgnoreQueryFilters() select v.CustomerId).FirstOrDefault();
                s.Startdate = (from v in context.Contracts.Where(v => v.ContractId == s.ContractId).IgnoreQueryFilters() select v.Startdate).FirstOrDefault();
                s.CompanyName = (from c in context.Customers.Where(c => c.CustomerId == customerId).IgnoreQueryFilters() select c.CompanyName).FirstOrDefault();
            }

            return deleteList;
        }

        /*
         * Für Testzweke können Testdaten für die Datenbank generiert werden. 
         * Zum Erzeugen der Testdaten habe ich einen eigenen Controller (SeedController) eingebaut.
         */
        #region SeedData generieren
        public void AddSeedData()
        {
            context.Customers.AddRange(new Customer[] {
            new Customer
            {
                CompanyName = "Dahl OHG",
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        Startdate = new DateTime(2018, 1, 2),
                        Enddate = new DateTime(2018, 3, 15),
                        PurchasePrice = 37707.45m,
                        RetailPrice = 57807.45m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 1: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 2: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                                                        new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 3: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2017, 5, 5),
                        Enddate = new DateTime(2017, 9, 16),
                        PurchasePrice = 30946.52m,
                        RetailPrice = 50849.52m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 4: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                                                        new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 5: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 12, 2),
                        Enddate = new DateTime(),
                        PurchasePrice = 27020.42m,
                        RetailPrice = 48025.42m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 6: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                                                        new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 7: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 8: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 9: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    }
                }
            },
            new Customer
            {
                CompanyName = "Valdof GmbH & Co KG",
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 1, 8),
                        Enddate = new DateTime(2019, 7, 10),
                        PurchasePrice = 10576.25m,
                        RetailPrice = 30676.24m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 10: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 11: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    }
                }
            },
            new Customer
            {
                CompanyName = "Drägerwerke KG",
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        Startdate = new DateTime(2015, 1, 7),
                        Enddate = new DateTime(2015, 9, 14),
                        PurchasePrice = 36131.18m,
                        RetailPrice = 56331.20m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 12: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 13: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 14: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2016, 2, 2),
                        Enddate = new DateTime(2016, 11, 15),
                        PurchasePrice = 24559.69m,
                        RetailPrice = 49559.68m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 15: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 16: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 12, 9),
                        Enddate = new DateTime(),
                        PurchasePrice = 23653.02m,
                        RetailPrice = 45652.02m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 17: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    }
                }
            },
            new Customer
            {
                CompanyName = "Hundertwasser Design AG",
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 5, 3),
                        Enddate = new DateTime(2019, 8, 12),
                        PurchasePrice = 34649.06m,
                        RetailPrice = 57649.10m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 18: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 19: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                 new Contract()
                 {
                        Startdate = new DateTime(2018, 3, 12),
                        Enddate = new DateTime(2018, 12, 3),
                        PurchasePrice = 18971.74m,
                        RetailPrice = 38571.14m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 20: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 21: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 22: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 2, 1),
                        Enddate = new DateTime(2019, 12, 15),
                        PurchasePrice = 24928.48m,
                        RetailPrice = 44958.68m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 23: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 24: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 25: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 26: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 27: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 4, 15),
                        Enddate = new DateTime(2019, 11, 11),
                        PurchasePrice = 37677.02m,
                        RetailPrice = 57647.03m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 28: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    }
                }
            },
            new Customer
            {
                CompanyName = "NeoNET GmbH",
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 8, 5),
                        Enddate = new DateTime(2019, 9, 19),
                        PurchasePrice = 34509.39m,
                        RetailPrice = 59509.29m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 29: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 30: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2017, 2, 16),
                        Enddate = new DateTime(2017, 6, 19),
                        PurchasePrice = 12486.52m,
                        RetailPrice = 32886.11m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 31: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 32: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 33: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 34: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2018, 3, 12),
                        Enddate = new DateTime(2018, 8, 10),
                        PurchasePrice = 17086.94m,
                        RetailPrice = 37083.74m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 35: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 36: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    },
                    new Contract()
                    {
                        Startdate = new DateTime(2019, 1, 2),
                        Enddate = new DateTime(2019, 9, 16),
                        PurchasePrice = 10731.80m,
                        RetailPrice = 30931.81m,
                        Services = new List<Service>()
                        {
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 37: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 38: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            },
                            new Service()
                            {
                                ServiceDescription = "Leistungbeschreibung 39: Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua."
                            }
                        }
                    }
                }
            }
            });
        }
        #endregion

        /*
         * Der Anwender hat die Möglichkeit die erzeugten Testdaten
         * komplett aus der Datenbank per Knopfdurch zu entfernen.
         */
        public void ClearData()
        {
            context.Database.SetCommandTimeout(System.TimeSpan.FromMinutes(10));
            context.Database.BeginTransaction();
            context.Database.ExecuteSqlCommand("DELETE FROM Services");
            context.Database.ExecuteSqlCommand("DELETE FROM Contracts");
            context.Database.ExecuteSqlCommand("DELETE FROM Customers");
            context.Database.CommitTransaction();
        }
    }

    /*
     * Bei jeder neuen Anfrage wird eine neue Instanz des Controllers
     * erzeugt. Zwischen den einzelnen ActionMethoden gehen daher per
     * Post gesendete Daten verloren. Daher müssen Daten die für den 
     * Seitenaufbau notwendig sind persisten abgelegt werden damit 
     * zum Beispiel nach dem Bearbeiten mit EditService eine Rückkehr 
     * zu ServiceDetails möglich ist.
     */
    public sealed class TempCompanyData
    {
        private static TempCompanyData instance = null;
        private TempCompanyData() { }

        public static TempCompanyData getInstance()
        {
            if (instance == null)
            {
                instance = new TempCompanyData();
            }
            return instance;
        }

        public DateTime? Startdate { get; set; }
        public string CompanyName { get; set; }
        public long ContractId { get; set; }
    }
}

