# Web Bán Bánh Ngọt (Project Cá Nhân)

Đây là mã nguồn dự án Website Bán Bánh Ngọt (xây dựng bằng ASP.NET MVC). Dự án được tạo ra phục vụ cho mục đích thực hành các kỹ năng Kiểm thử phần mềm (Manual & Automation Test) cũng như tìm hiểu quy trình xây dựng web E-commerce cơ bản.

---

## 🚀 Hướng dẫn chạy dự án trên Visual Studio (2019 - 2022)

Dự án này sử dụng .NET Framework và được thiết kế để chạy mượt mà nhất trên **Visual Studio**.

### Các bước thực hiện:

1. **Clone mã nguồn:**
   Mở Terminal hoặc Git Bash và chạy lệnh:
   ```bash
   git clone https://github.com/Huong36/Web_Banhngot_DoAnck.git
   ```

2. **Mở Solution:**
   - Mở phần mềm **Visual Studio 2019** hoặc **Visual Studio 2022**.
   - Chọn **Open a project or solution** (Mở một dự án).
   - Duyệt đến thư mục bạn vừa clone về, tìm và mở file **`.sln`** (Ví dụ: `Web_Banhngot_DoAnck.sln`).

3. **Khôi phục thư viện (Restore NuGet Packages):**
   - Khi mở dự án lên, nhìn sang cột **Solution Explorer** bên phải.
   - Click chuột phải vào Solution (dòng trên cùng) và chọn **Restore NuGet Packages** để hệ thống tự động tải về các thư viện cần thiết.

4. **Cơ sở dữ liệu (Database):**
   - File Database (dạng `.mdf` và `.ldf`) đã được đính kèm sẵn bên trong thư mục `App_Data`.
   - Mặc định, Visual Studio (LocalDB) sẽ tự động nhận diện và kết nối với file dữ liệu này mà bạn không cần phải config thủ công trên SQL Server Management Studio.

5. **Chạy ứng dụng:**
   - Nhấn nút **Start** (Mũi tên Play màu xanh chữ IIS Express) trên thanh công cụ trên cùng.
   - Hoặc đơn giản là bấm phím **F5**.
   - Trình duyệt web sẽ tự động mở ra trang chủ của cửa hàng bán Bánh Ngọt. Chúc bạn thành công!
