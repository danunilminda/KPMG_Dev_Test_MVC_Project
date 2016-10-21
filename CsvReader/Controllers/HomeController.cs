using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CsvReader.Models;
using CsvReader.ViewModel;
using System.Data.Entity;

namespace CsvReader.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public int IgnoredRecordCountMissingFields = 0;
        public int IgnoredRecordCountInvalidCurrCode = 0;
        public int ImportedRecordCount = 0;

        public HomeController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult Upload()
        {
            return View();
        }

        public ViewResult Index(int? page)
        {
            var transactions = _context.Transactions.ToList();
            return View(transactions);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.FileName.ToLower().EndsWith(".csv"))
                    {
                        var path = string.Empty;
                        if (upload.ContentLength > 0)
                        {
                            path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), Path.GetFileName(upload.FileName));
                            upload.SaveAs(path);
                        }

                        using (var fs = new FileStream(path, FileMode.Open))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                while (sr.Peek() > -1)
                                {
                                    var readLine = sr.ReadLine();
                                    if (readLine == null) continue;
                                    var line = readLine.Split(',');
                                    var hasEmptyElement = line.Any(string.IsNullOrEmpty);
                                        
                                    IEnumerable<string> currencyCodes = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                        .Select(x => (new RegionInfo(x.LCID)).ISOCurrencySymbol)
                                        .Distinct()
                                        .OrderBy(x => x);

                                    if (!hasEmptyElement)
                                    {
                                        if (currencyCodes.Contains(line[2]))
                                        {
                                            var transaction = new Transaction()
                                            {
                                                Account = line[0],
                                                Description = line[1],
                                                CurrencyCode = line[2],
                                                Amount = Convert.ToDecimal(line[3]),
                                                DateUploaded = DateTime.Now
                                            };

                                            _context.Transactions.Add(transaction);
                                            _context.SaveChanges();
                                            ImportedRecordCount++;
                                        }
                                        else
                                        {
                                            IgnoredRecordCountInvalidCurrCode++;
                                        }
                                    }
                                    else
                                    {
                                        IgnoredRecordCountMissingFields++;
                                    }
                                }
                            }
                        }
                        ViewBag.IgnoredRecordCountInvalidCurrCode = IgnoredRecordCountInvalidCurrCode;
                        ViewBag.IgnoredRecordCountMissingFields = IgnoredRecordCountMissingFields;
                        ViewBag.ImportedRecordCount = ImportedRecordCount;
                        
                        var transactions = _context.Transactions.ToList();
                        return View(transactions);
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }
            return View();
        }

        public ActionResult Save(Transaction transaction)
        {
            var transactionInDb = _context.Transactions.Single(t => t.Id == transaction.Id);

            transactionInDb.Account = transaction.Account;
            transactionInDb.Description = transaction.Description;
            transactionInDb.CurrencyCode = transaction.CurrencyCode;
            transactionInDb.Amount = transaction.Amount;

            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Edit(int id)
        {
            var transaction = _context.Transactions.Single(t => t.Id == id);

            if (transaction == null)
            {
                HttpNotFound();
            }
            
            var viewModel = new TransactionFormViewModel()
            {
                Transaction = transaction,
            };

            return View("TransactionForm", viewModel);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var transaction = _context.Transactions.SingleOrDefault(v => v.Id == id);
            if (transaction == null)
                return HttpNotFound();
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DeleteConfirmation(int id)
        {
            var transaction = _context.Transactions.SingleOrDefault(v => v.Id == id);

            if (transaction == null)
                return HttpNotFound();

            var viewModel = new DeleteConfirmationViewModel()
            {
                Transaction = transaction,
            };

            return View("Delete", viewModel);
        }
    }
}