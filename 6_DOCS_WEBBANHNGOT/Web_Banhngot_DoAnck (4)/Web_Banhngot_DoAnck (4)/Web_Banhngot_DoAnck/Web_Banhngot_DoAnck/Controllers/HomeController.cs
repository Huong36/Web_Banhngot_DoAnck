using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Web_Banhngot_DoAnck.Models;

namespace Web_Banhngot_DoAnck.Controllers
{
    public class HomeController : Controller
    {
            private ApplicationDbContext db = new ApplicationDbContext();

            public ActionResult Index()
            {
                var featuredProducts = db.Products
                                         .Include(p => p.Category)
                                         .OrderByDescending(p => p.Id)
                                         .Take(8)
                                         .ToList();

                return View(featuredProducts);
            }

            public ActionResult About()
            {
                ViewBag.Message = "Your application description page.";
                return View();
            }

            public ActionResult Contact()
            {
                ViewBag.Message = "Your contact page.";
                return View();
            }

            [HttpPost]
            public ActionResult Contact(string name, string email, string message)
            {
                ViewBag.SuccessMessage = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
                return View();
            }
        protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
    }
}