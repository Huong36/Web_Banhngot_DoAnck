using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Web_Banhngot_DoAnck.Models;

namespace Web_Banhngot_DoAnck.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = cart.Sum(item => item.Total);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "CustomerName,Address,Phone")] Order model)
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn trống.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.OrderDate = DateTime.Now;
                model.UserId = User.Identity.GetUserId();
                model.TotalAmount = cart.Sum(item => item.Total);
                model.Status = "Đang xử lý";

                db.Orders.Add(model);
                db.SaveChanges();

                int orderId = model.Id;
                List<OrderDetail> orderDetails = new List<OrderDetail>();
                foreach (var item in cart)
                {
                    orderDetails.Add(new OrderDetail
                    {
                        OrderId = orderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }
                db.OrderDetails.AddRange(orderDetails);
                db.SaveChanges();

                Session["Cart"] = null;

                return RedirectToAction("Complete");
            }

            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = cart.Sum(item => item.Total);
            return View(model);
        }

        public ActionResult Complete()
        {
            return View();
        }
    }
}