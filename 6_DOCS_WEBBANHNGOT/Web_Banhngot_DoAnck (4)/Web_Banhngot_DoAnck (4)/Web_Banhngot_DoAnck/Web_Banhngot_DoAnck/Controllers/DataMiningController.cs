using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web_Banhngot_DoAnck.Models;
using Web_Banhngot_DoAnck.Utils;

namespace Web_Banhngot_DoAnck.Controllers
{
    // Cần đăng nhập với quyền Admin (bỏ comment [Authorize(Roles = "Admin")] sau khi test xong)
    // [Authorize(Roles = "Admin")]
    public class DataMiningController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var totalOrders = db.Orders.Count();
            var totalProducts = db.Products.Count();
            var totalRules = db.ProductRecommendations.Count();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalRules = totalRules;

            // Lấy Top 20 luật có Confidence cao nhất để hiển thị
            var topRules = db.ProductRecommendations
                .OrderByDescending(r => r.Confidence)
                .Take(20)
                .ToList();
            ViewBag.TopRules = topRules;

            return View();
        }

        public ActionResult Report()
        {
            // Tổng quan
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalRules = db.ProductRecommendations.Count();

            // Thống kê chi tiết
            var allRules = db.ProductRecommendations.ToList();
            ViewBag.AllRules = allRules;
            
            if (allRules.Any())
            {
                ViewBag.AvgConfidence = allRules.Average(r => r.Confidence);
                ViewBag.MaxConfidence = allRules.Max(r => r.Confidence);
                ViewBag.MinConfidence = allRules.Min(r => r.Confidence);
                ViewBag.AvgSupport = allRules.Average(r => r.Support);
                ViewBag.MaxSupport = allRules.Max(r => r.Support);
            }
            else
            {
                ViewBag.AvgConfidence = 0.0;
                ViewBag.MaxConfidence = 0.0;
                ViewBag.MinConfidence = 0.0;
                ViewBag.AvgSupport = 0.0;
                ViewBag.MaxSupport = 0.0;
            }

            // Phân bổ Confidence theo khoảng
            ViewBag.Conf30_50 = allRules.Count(r => r.Confidence >= 0.3 && r.Confidence < 0.5);
            ViewBag.Conf50_70 = allRules.Count(r => r.Confidence >= 0.5 && r.Confidence < 0.7);
            ViewBag.Conf70_90 = allRules.Count(r => r.Confidence >= 0.7 && r.Confidence < 0.9);
            ViewBag.Conf90_100 = allRules.Count(r => r.Confidence >= 0.9);

            // Top 10 sản phẩm xuất hiện nhiều nhất trong luật (bên A - sản phẩm gốc)
            var topSourceProducts = allRules
                .GroupBy(r => r.ProductId_A)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { 
                    ProductId = g.Key, 
                    Count = g.Count(), 
                    ProductName = db.Products.Where(p => p.Id == g.Key).Select(p => p.Name).FirstOrDefault() 
                })
                .ToList();
            ViewBag.TopSourceProducts = topSourceProducts;

            return View();
        }

        [HttpPost]
        public ActionResult SeedData()
        {
            Random rnd = new Random();

            // 0. Xóa dữ liệu giả cũ để làm sạch Database
            // Cần xóa các luật gợi ý trước để tránh lỗi ForeignKey (do tắt Cascade Delete)
            var oldRules = db.ProductRecommendations.ToList();
            db.ProductRecommendations.RemoveRange(oldRules);

            // Xóa các đơn hàng giả
            var fakeOrders = db.Orders.Where(o => o.CustomerName.StartsWith("Khách Hàng ")).ToList();
            db.Orders.RemoveRange(fakeOrders);
            
            // Xóa các sản phẩm giả đã tạo từ trước (có chữ [Mock] hoặc có mô tả mẫu)
            var fakeProducts = db.Products.Where(p => 
                p.Name.Contains("[Mock]") || 
                p.Description == "Sản phẩm được làm từ nguyên liệu hảo hạng, thơm ngon và an toàn cho sức khỏe."
            ).ToList();
            db.Products.RemoveRange(fakeProducts);
            db.SaveChanges();

            // 1. Lấy danh sách danh mục hiện có
            var categoryIds = db.Categories.Select(c => c.Id).ToList();
            if (!categoryIds.Any())
            {
                var cat = new Category { Name = "Bánh Ngọt Chung" };
                db.Categories.Add(cat);
                db.SaveChanges();
                categoryIds.Add(cat.Id);
            }

            // 2. Lấy danh sách ảnh có sẵn từ sản phẩm thật của bạn
            var existingImages = db.Products
                .Where(p => p.Description != "Sản phẩm được làm từ nguyên liệu hảo hạng, thơm ngon và an toàn cho sức khỏe.")
                .Select(p => p.ImageUrl)
                .Where(img => img != null && img != "")
                .Distinct()
                .ToList();
                
            if (existingImages.Count == 0)
            {
                existingImages.Add("placeholder.jpg");
            }

            var cakeNames = new string[] {
                "Tiramisu", "Macaron", "Mousse Trà Xanh", "Bánh Mì Hoa Cúc", "Croissant", 
                "Red Velvet", "Cheesecake", "Tart Trứng", "Su Kem", "Cupcake", 
                "Donut Socola", "Matcha Crepe", "Apple Pie", "Cookie", "Bánh Sinh Nhật", 
                "Muffin", "Bánh Kem Bắp", "Mochi", "Brownie", "Pancake"
            };

            // Sinh 300 sản phẩm mới
            var productsToAdd = new List<Product>();
            for (int i = 1; i <= 300; i++)
            {
                string baseName = cakeNames[rnd.Next(cakeNames.Length)];
                string imageUrl = existingImages[rnd.Next(existingImages.Count)];
                
                productsToAdd.Add(new Product
                {
                    // Tên giờ hoàn toàn tự nhiên, không còn [Mock]
                    Name = baseName + (rnd.Next(2) == 0 ? " Đặc Biệt" : " Thơm Ngon"),
                    Description = "Sản phẩm được làm từ nguyên liệu hảo hạng, thơm ngon và an toàn cho sức khỏe.",
                    Price = rnd.Next(2, 50) * 10000, 
                    ImageUrl = imageUrl,
                    CategoryId = categoryIds[rnd.Next(categoryIds.Count)]
                });
            }
            db.Products.AddRange(productsToAdd);
            db.SaveChanges();

            // 3. Lấy lại ID sản phẩm
            var allProductIds = db.Products.Select(p => p.Id).ToList();
            
            // Lấy riêng ID các sản phẩm GỐC (30 sp ban đầu - không phải Mock)
            var originalProductIds = db.Products
                .Where(p => p.Description != "Sản phẩm được làm từ nguyên liệu hảo hạng, thơm ngon và an toàn cho sức khỏe.")
                .Select(p => p.Id)
                .ToList();
            
            // 4. Sinh 5000 đơn hàng giả có logic mua kèm
            // TẠO 15 NHÓM MUA KÈM, ưu tiên sản phẩm gốc vào nhóm
            List<List<int>> ruleGroups = new List<List<int>>();
            
            // Nhóm 1-10: Mỗi nhóm gồm 2-3 sản phẩm GỐC (để trang đầu luôn có gợi ý)
            if (originalProductIds.Count >= 2)
            {
                for (int i = 0; i < originalProductIds.Count - 1; i += 2)
                {
                    var group = new List<int>();
                    group.Add(originalProductIds[i]);
                    group.Add(originalProductIds[i + 1]);
                    // Thêm 1 sp mock ngẫu nhiên cho đa dạng
                    group.Add(allProductIds[rnd.Next(allProductIds.Count)]);
                    ruleGroups.Add(group);
                    if (ruleGroups.Count >= 10) break;
                }
            }
            
            // Nhóm 11-15: Nhóm ngẫu nhiên hỗn hợp
            for (int i = 0; i < 5; i++)
            {
                var group = new List<int>();
                for (int j = 0; j < 3; j++)
                    group.Add(allProductIds[rnd.Next(allProductIds.Count)]);
                ruleGroups.Add(group);
            }

            var firstUser = db.Users.FirstOrDefault();
            var userId = firstUser != null ? firstUser.Id : null;

            for (int batch = 0; batch < 5; batch++)
            {
                var ordersBatch = new List<Order>();
                for (int i = 0; i < 1000; i++)
                {
                    var order = new Order
                    {
                        OrderDate = DateTime.Now.AddDays(-rnd.Next(1, 365)),
                        CustomerName = "Khách Hàng " + rnd.Next(1, 10000),
                        Address = "TP.HCM",
                        Phone = "090" + rnd.Next(1000000, 9999999),
                        Status = "Hoàn thành",
                        UserId = userId,
                        OrderDetails = new List<OrderDetail>()
                    };

                    decimal totalAmount = 0;
                    int numItems = rnd.Next(2, 5); 
                    List<int> itemsToBuy = new List<int>();

                    // 60% số đơn hàng sẽ rơi vào quy luật mua kèm (tăng để sinh nhiều luật hơn)
                    if (rnd.NextDouble() < 0.6)
                    {
                        var selectedGroup = ruleGroups[rnd.Next(ruleGroups.Count)];
                        itemsToBuy.AddRange(selectedGroup);
                    }

                    while (itemsToBuy.Count < numItems)
                    {
                        itemsToBuy.Add(allProductIds[rnd.Next(allProductIds.Count)]);
                    }
                    
                    itemsToBuy = itemsToBuy.Distinct().ToList();

                    foreach (var pId in itemsToBuy)
                    {
                        var qty = rnd.Next(1, 3);
                        var price = 50000; // Tạm tính trung bình 50k
                        totalAmount += qty * price;

                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductId = pId,
                            Quantity = qty,
                            Price = price
                        });
                    }

                    order.TotalAmount = totalAmount;
                    ordersBatch.Add(order);
                }
                
                db.Orders.AddRange(ordersBatch);
                db.SaveChanges();
            }

            TempData["Message"] = "Đã làm sạch dữ liệu cũ, sinh thành công 300 sản phẩm mới với ảnh đẹp và 5000 đơn hàng mô phỏng!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RunApriori(double minSupport = 0.01, double minConfidence = 0.5)
        {
            // 1. Chuẩn bị dữ liệu: Lấy danh sách giao dịch
            // Một giao dịch là 1 đơn hàng, trong đó chứa list các ProductId
            var transactions = db.Orders
                .Select(o => o.OrderDetails.Select(od => od.ProductId).ToList())
                .ToList();

            // 2. Chạy thuật toán Apriori
            var apriori = new AprioriAlgorithm(minSupport, minConfidence);
            var rules = apriori.GenerateRules(transactions);

            // 3. Xóa các luật cũ
            db.Database.ExecuteSqlCommand("TRUNCATE TABLE ProductRecommendations"); // Hoặc dùng RemoveRange nếu muốn an toàn EF

            // 4. Lưu kết quả mới vào Database
            var recommendations = rules.Select(r => new ProductRecommendation
            {
                ProductId_A = r.ProductId_A,
                ProductId_B = r.ProductId_B,
                Support = r.Support,
                Confidence = r.Confidence,
                CreatedAt = DateTime.Now
            }).ToList();

            db.ProductRecommendations.AddRange(recommendations);
            db.SaveChanges();

            TempData["Message"] = string.Format("Huấn luyện mô hình thành công! Sinh ra được {0} luật kết hợp.", recommendations.Count);
            return RedirectToAction("Index");
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
