using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Web_Banhngot_DoAnck.Models;
using PagedList;
using System.Net;

namespace Web_Banhngot_DoAnck.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index(int? page, string categoryName, string sortOrder, string[] priceRange)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCategory = categoryName;
            ViewBag.CurrentPriceRanges = priceRange ?? new string[0];

            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!String.IsNullOrEmpty(categoryName))
            {
                products = products.Where(p => p.Category.Name == categoryName);
                ViewBag.Title = "Danh mục: " + categoryName;
            }
            else
            {
                ViewBag.Title = "Tất cả sản phẩm";
            }

            if (priceRange != null && priceRange.Length > 0)
            {
                bool under100 = priceRange.Contains("0-100");
                bool r100_300 = priceRange.Contains("100-300");
                bool r300_500 = priceRange.Contains("300-500");
                bool r500_1000 = priceRange.Contains("500-1000");
                bool above1000 = priceRange.Contains("1000+");

                products = products.Where(p =>
                    (under100 && p.Price < 100000) ||
                    (r100_300 && p.Price >= 100000 && p.Price <= 300000) ||
                    (r300_500 && p.Price >= 300000 && p.Price <= 500000) ||
                    (r500_1000 && p.Price >= 500000 && p.Price <= 1000000) ||
                    (above1000 && p.Price > 1000000)
                );
            }

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "latest":
                default:
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            int pageSize = 9;
            int pageNumber = (page ?? 1);

            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            if (pagedProducts.PageNumber != 1 && page.HasValue && page > pagedProducts.PageCount && pagedProducts.PageCount > 0)
            {
                return RedirectToAction("Index", new
                {
                    categoryName,
                    sortOrder,
                    priceRange,
                    page = 1
                });
            }

            return View(pagedProducts);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            // Gợi ý sản phẩm mua kèm từ thuật toán Apriori
            var recommendedProducts = db.ProductRecommendations
                .Where(r => r.ProductId_A == id)
                .OrderByDescending(r => r.Confidence)
                .Take(4)
                .Select(r => r.ProductB)
                .ToList();

            // Fallback: Nếu không có luật Apriori cho SP này, gợi ý SP cùng danh mục
            if (recommendedProducts.Count < 4)
            {
                var existingIds = recommendedProducts.Select(p => p.Id).ToList();
                existingIds.Add(product.Id); // Loại chính nó

                var fallbackProducts = db.Products
                    .Where(p => p.CategoryId == product.CategoryId && !existingIds.Contains(p.Id))
                    .OrderBy(p => Guid.NewGuid()) // Random
                    .Take(4 - recommendedProducts.Count)
                    .ToList();

                recommendedProducts.AddRange(fallbackProducts);
            }

            // Fallback 2: Nếu vẫn chưa đủ, lấy SP bất kỳ
            if (recommendedProducts.Count < 4)
            {
                var existingIds = recommendedProducts.Select(p => p.Id).ToList();
                existingIds.Add(product.Id);

                var moreProducts = db.Products
                    .Where(p => !existingIds.Contains(p.Id))
                    .OrderBy(p => Guid.NewGuid())
                    .Take(4 - recommendedProducts.Count)
                    .ToList();

                recommendedProducts.AddRange(moreProducts);
            }

            ViewBag.RecommendedProducts = recommendedProducts;

            return View(product);
        }
        public JsonResult SearchSuggest(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                // SỬA Ở ĐÂY: Thêm <object> vào
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var rawSuggestions = db.Products
                .Where(p => p.Name.Contains(term))
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    imageUrl = p.ImageUrl
                })
                .ToList(); 

  
            var formattedSuggestions = rawSuggestions.Select(s => new
            {
                s.name,
                priceString = s.price.ToString("N0") + "đ",
                s.imageUrl,
                url = Url.Action("Details", "Products", new { id = s.id })
            });

            return Json(formattedSuggestions, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // dung "/Products/GetProductListApi" de xem list san pham 
        [AllowAnonymous]
        public JsonResult GetProductListApi()
        {
            try
            {
                var productList = db.Products
                    .Include(p => p.Category)
                    .Select(p => new {
                        Id = p.Id,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl ?? "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTxdAOY_-vITFVI-ej84s2U_ErxhOly-z3y_Q&s",
                        Price = p.Price,
                        CategoryName = p.Category != null ? p.Category.Name : "Chưa phân loại"
                    })
                    .ToList();

                return Json(new { status = true, data = productList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // dung "/Products/GetProductListApi" de xem list san pham 
        public ActionResult ShowProductAjax()
        {
            return View();
        }
    }
}