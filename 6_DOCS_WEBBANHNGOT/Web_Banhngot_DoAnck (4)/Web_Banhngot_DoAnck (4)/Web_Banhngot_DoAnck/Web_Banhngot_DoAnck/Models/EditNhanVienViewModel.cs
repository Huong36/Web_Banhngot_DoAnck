using System.ComponentModel.DataAnnotations;

namespace Web_Banhngot_DoAnck.Models
{
    public class EditNhanVienViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Tên Đăng Nhập")]
        public string TenDangNhap { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}