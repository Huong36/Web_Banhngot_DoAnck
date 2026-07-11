using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Web_Banhngot_DoAnck.Models;

namespace Web_Banhngot_DoAnck.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HieuNangController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetServerMetrics()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                // Thông tin hệ điều hành
                string osVersion = Environment.OSVersion.ToString();
                int cpuCores = Environment.ProcessorCount;

                // Bộ nhớ tiến trình hiện tại (MB)
                double memoryMB = Math.Round(process.PrivateMemorySize64 / (1024.0 * 1024.0), 2);
                double workingSetMB = Math.Round(process.WorkingSet64 / (1024.0 * 1024.0), 2);

                // Thời gian hoạt động tiến trình
                TimeSpan uptime = DateTime.Now - process.StartTime;
                string uptimeStr = string.Format("{0}d {1}h {2}m {3}s",
                    (int)uptime.TotalDays, uptime.Hours, uptime.Minutes, uptime.Seconds);

                // Số luồng đang hoạt động
                int threadCount = process.Threads.Count;

                // Đo độ trễ kết nối Database
                double dbLatencyMs = 0;
                string dbStatus = "OK";
                try
                {
                    var sw = Stopwatch.StartNew();
                    // Thực hiện truy vấn đơn giản để đo latency
                    var count = db.Products.Count();
                    sw.Stop();
                    dbLatencyMs = Math.Round(sw.Elapsed.TotalMilliseconds, 2);
                }
                catch (Exception ex)
                {
                    dbStatus = "ERROR: " + ex.Message;
                    dbLatencyMs = -1;
                }

                // Tổng quan dữ liệu
                int totalProducts = 0;
                int totalOrders = 0;
                int totalUsers = 0;
                try
                {
                    totalProducts = db.Products.Count();
                    totalOrders = db.Orders.Count();
                    totalUsers = db.Users.Count();
                }
                catch { }

                return Json(new
                {
                    success = true,
                    os = osVersion,
                    cpuCores = cpuCores,
                    memoryMB = memoryMB,
                    workingSetMB = workingSetMB,
                    uptime = uptimeStr,
                    threadCount = threadCount,
                    dbLatencyMs = dbLatencyMs,
                    dbStatus = dbStatus,
                    totalProducts = totalProducts,
                    totalOrders = totalOrders,
                    totalUsers = totalUsers,
                    timestamp = DateTime.Now.ToString("HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult SimulateError()
        {
            // Trả về HTTP 500 Internal Server Error
            return new HttpStatusCodeResult(500, "Mô phỏng lỗi máy chủ");
        }

        [HttpGet]
        public ActionResult SimulateTimeout()
        {
            // Tạm dừng luồng trong 5 giây để mô phỏng phản hồi chậm / timeout
            System.Threading.Thread.Sleep(5000);
            return Content("Phản hồi sau 5 giây");
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

