using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web_Banhngot_DoAnck.Models;

namespace Web_Banhngot_DoAnck.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class QuanLyDonHangController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate);
            return View(orders.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders
                            .Include(o => o.User)
                            .Include(o => o.OrderDetails.Select(d => d.Product))
                            .SingleOrDefault(o => o.Id == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            Order order = db.Orders.Find(id);
            if (order != null)
            {
                order.Status = status;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = id });
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