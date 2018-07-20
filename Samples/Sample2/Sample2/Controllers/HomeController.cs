using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sample2.Configuration;
using Sample2.DataAccess.Invoices;
using Sample2.DataAccess.InvoiceItems;
using Sample2.Models;

namespace Sample2.Controllers
{
    public class HomeController : Controller
    {
        private InvoiceDao invoiceDao;
        private InvoiceItemDao invoiceItemDao;

        public HomeController(IOptions<ConnectionConfiguration> config)
        {
            invoiceDao = new InvoiceDao(config.Value.Sqlite);
            invoiceItemDao = new InvoiceItemDao(config.Value.Sqlite);
        }

        public IActionResult Index()
        {
            var invoices = invoiceDao.GetInvoices();

            return View(invoices);
        }

        public IActionResult Detail(long id)
        {
            var invoiceItems = invoiceItemDao.GetInvoiceItemsByInvoiceId(id);

            return View(invoiceItems);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
