using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web_Banhngot_DoAnck.Models;

namespace Web_Banhngot_DoAnck.Controllers
{
    public class CartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private List<CartItem> GetCart()
        {
            List<CartItem> cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        public ActionResult Index()
        {
            List<CartItem> cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(item => item.Total);

            // Gợi ý sản phẩm mua kèm dựa trên thuật toán Apriori
            var cartProductIds = cart.Select(c => c.ProductId).ToList();
            if (cartProductIds.Any())
            {
                var recommendedProducts = db.ProductRecommendations
                    .Where(r => cartProductIds.Contains(r.ProductId_A) && !cartProductIds.Contains(r.ProductId_B))
                    .OrderByDescending(r => r.Confidence)
                    .Select(r => r.ProductB)
                    .Distinct()
                    .Take(4)
                    .ToList();

                ViewBag.CartRecommendations = recommendedProducts;
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int id, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, redirectUrl = Url.Action("Index", "DangNhap") });
                }
                return RedirectToAction("Index", "DangNhap");
            }

            var cart = GetCart();
            CartItem item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                Product product = db.Products.Find(id);
                if (product != null)
                {
                    item = new CartItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price,
                        Quantity = quantity
                    };
                    cart.Add(item);
                }
            }
            Session["Cart"] = cart;

            if (Request.IsAjaxRequest())
            {
                int newCount = cart.Sum(i => i.Quantity);
                return Json(new { success = true, cartCount = newCount, message = "Đã thêm sản phẩm vào giỏ hàng!" });
            }

            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            CartItem item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
            }
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public ActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCart();
            CartItem item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                if (quantity > 0 && quantity <= 99)
                {
                    item.Quantity = quantity;
                }
                else if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                Session["Cart"] = cart;
            }
            if (Request.UrlReferrer != null)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
            return RedirectToAction("Index", "Cart");
        }

        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            int count = 0;
            List<CartItem> cart = Session["Cart"] as List<CartItem>;
            if (cart != null)
            {
                count = cart.Sum(i => i.Quantity);
            }
            ViewBag.CartCount = count;
            return PartialView("_CartSummary");
        }
    }
}