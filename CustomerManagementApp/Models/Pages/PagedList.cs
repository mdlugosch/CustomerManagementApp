using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerManagementApp.Models.Pages
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PagedList(IQueryable<T> query, QueryOptions options = null)
        {
            CurrentPage = options.CurrentPage;
            PageSize = options.PageSize;

            /*
             * Bei einer krummen Anzahl an Einträgen muss der TotalPages Wert um
             * eine Seite erhöht werden damit alle Daten angezeigt werden.
             */
            TotalPages = query.Count() / PageSize;
            if ((query.Count() % PageSize) > 0) TotalPages++;

            AddRange(query.Skip((CurrentPage - 1) * PageSize).Take(PageSize));
        }
    }
}

