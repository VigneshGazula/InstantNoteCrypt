using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;

namespace ShareItems_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserContext _userContext;
        private readonly IEncryptionService _encryptionService;

        public HomeController(UserContext userContext, IEncryptionService encryptionService)
        {
            _userContext = userContext;
            _encryptionService = encryptionService;
        }
        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Index(string Code)
        {
            if (string.IsNullOrEmpty(Code))
            {
                return View();
            }
            else
            {
                return RedirectToAction("VerifySecondaryPassword", new { Code });
            }
        }
        public IActionResult AccessToItems(string Code)
        {
            if (string.IsNullOrEmpty(Code))
            {
                return RedirectToAction("Index");
            }
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            if (entry == null)
            {
                UserCredential model = new UserCredential()
                {
                    code = Code,
                    matter = "",
                    secondaryPassword = null,
                    dateTimes = new List<DateTime> { DateTime.Now },
                };
                _userContext.UserCredentials.Add(model);
                _userContext.SaveChanges();
                return View(model);
            }
            entry.dateTimes.Add(DateTime.Now);
            _userContext.UserCredentials.Update(entry);
            _userContext.SaveChanges();
            entry.matter = _encryptionService.DecryptData(entry.matter);
            return View(entry);
        }
        [HttpPost]
        public IActionResult Save(UserCredential model)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(model.code));
            if (entry != null)
            {
                entry.matter = _encryptionService.EncryptData(model.matter);
                _userContext.UserCredentials.Update(entry);
                _userContext.SaveChanges();
            }
            return RedirectToAction("AccessToItems", new { Code = model.code });
        }
        [HttpPost]
        public IActionResult CreateSecondaryLock(string Code, string Pin)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            entry.secondaryPassword = _encryptionService.EncryptData(Pin);
            _userContext.Update(entry);
            _userContext.SaveChanges();
            return RedirectToAction("AccessToItems", new { Code });
        }

        public IActionResult VerifySecondaryPassword(string Code)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            if (entry == null || entry.secondaryPassword == null)
            {
                return RedirectToAction("AccessToItems", new { Code });
            }
            return View("CheckSecondaryPassword", entry);
        }
        [HttpPost]
        public IActionResult CheckSecondaryPassword(string Code, string Pin)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            string decryptedPin = _encryptionService.DecryptData(entry.secondaryPassword);
            if (decryptedPin.Equals(Pin))
            {
                return RedirectToAction("AccessToItems", new { Code });
            }
            ViewBag.Message = "Invalid Pin";
            return View("CheckSecondaryPassword", entry);
        }
        public IActionResult DeleteSecondaryPassword(string Code)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            entry.secondaryPassword = null;
            _userContext.Update(entry);
            _userContext.SaveChanges();
            ViewBag.Message = "Secondary Password removed.";
            return RedirectToAction("AccessToItems", new { Code });
        }
        public IActionResult DestroyNote(string Code)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            _userContext.UserCredentials.Remove(entry);
            _userContext.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult OpenActivityLog(string Code)
        {
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code.Equals(Code));
            entry.dateTimes.Reverse();
            return View(entry);
        }
        public IActionResult ExitToAccessItems(string Code)
        {
            return RedirectToAction("AccessToItems", new { Code });
        }
        public IActionResult ExitToIndex()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DownloadPdf(string Code)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var entry = _userContext.UserCredentials.FirstOrDefault(x => x.code == Code);
            string matter = _encryptionService.DecryptData(entry.matter);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14));

                    page.Header()
                        .Text(entry.code)
                        .SemiBold().FontSize(22).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text(matter);
                        });

                    //page.Footer()
                    //    .AlignCenter()
                    //    .Text("Confidential - Internal Use Only").Italic().FontSize(10);
                });
            });

            var pdfBytes = document.GeneratePdf();
            string pdfName = entry.code + "_"+".pdf";
            return File(pdfBytes, "application/pdf", pdfName);
        }


    }
}
