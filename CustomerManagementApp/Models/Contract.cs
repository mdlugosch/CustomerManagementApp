using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models
{
    public class Contract
    {
        public long ContractId { get; set; }
        public long CustomerId { get; set; }
        public DateTime? Startdate { get; set; } = null;
        public DateTime? Enddate { get; set; } = null;

        public decimal PurchasePrice { get; set; }
        public decimal RetailPrice { get; set; }

        public IEnumerable<Service> Services { get; set; }

        public bool SoftDeleted { get; set; } = false;

        /*
         * Der Gewinn berechnet sich auf der Grundlage des EK und VK Preises, daher kann hier
         * ein dynamischer Ansatz gewählt werden anstatt den Gewinn in der Datenbank zu speichern.
         * Die Property Profit ist aus diesem Grund mit "NotMapped" dekoriert.
         */
        [NotMapped]
        public decimal Profit
        {
            get
            {
                if (Enddate != null && Enddate.Value != DateTime.MinValue)
                {
                    return RetailPrice - PurchasePrice;
                }
                return 0m;
            }
        }
        /*
         * Das Programm gibt dem Kunden die Möglichkeit gelöschte Daten im Papierkorb (DeleteView.cshtml) wiederherzustellen.
         * Bei Verträgen ist es für den Anwender hilfreich zu wissen zu welcher Firma ein Vertrag gehört. Beim Laden der
         * Daten für die DeleteView werden durch die Methode CreateDeleteList() im DataRepository zur CustomerId auch
         * temporär der Firmenname(CompanyName) abgelegt. So kann dem Anwender ein Firmenname anstatt einer CustomerId
         * in der Löschübersicht(DeleteView.cshtml) angezeigt werden.
         */
        [NotMapped]
        public string CompanyName { get; set; }

        /*
         * Wenn die Editiermaske direkt mit einer Dezimaleigenschaft wie RetailPrice verbunden ist
         * sorgt die Client-Validierung dafür das zB. Fehleingaben wie Buchstaben erkannt und eine
         * 0 an den Controller geschickt wird. Serverseitig ist ein 0-Wert kein Fehler und eigene
         * Fehlermeldungen werden nicht angezeigt. Das bedeutet das die Client-Validierung immer
         * ihre Standartfehlermeldung nutzt. Um dies zu Umgehen werden die Eingabefelder mit String
         * Properties verbunden die den Wert an die Decimalproperties weitergeben.
         */
        [NotMapped]
        public string StrPurchasePrice { get; set; }

        [NotMapped]
        public string StrRetailPrice { get; set; }

        /*
         * Schlägt die Validierung der Benutzereingaben fehl muss der Anwender wieder auf die Edittiermaske
         * zurückgeführt werden. Beim bearbeiten von alten Verträgen wollte ich allerdings bei falschen
         * Eingaben die Maske zurück auf die alten Daten stellen. Um dies zu ermöglichen muss ich auch
         * hier über ein Stringfeld auf die DateTime Properties zugreifen. Für weiter Informationen siehe
         * EditContract.cshtml.
         */
        [NotMapped]
        public String strStartdate;
        [NotMapped]
        public String strEnddate;

        [NotMapped]
        public String StrStartdate
        { get { return Startdate.HasValue ? Startdate.Value.ToShortDateString() : strStartdate; } set { strStartdate = value; } }
        [NotMapped]
        public String StrEnddate
        { get { return Enddate.HasValue ? Enddate.Value.ToShortDateString() : strEnddate; } set { strEnddate = value; } }
    }
}
