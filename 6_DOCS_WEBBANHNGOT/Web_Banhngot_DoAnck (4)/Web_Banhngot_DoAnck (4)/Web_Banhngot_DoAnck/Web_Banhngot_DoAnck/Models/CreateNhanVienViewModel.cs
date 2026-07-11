using System.ComponentModel.DataAnnotations;

namespace Web_Banhngot_DoAnck.Models
{
    public class CreateNhanVienViewModel
    {
        [Required]
        [Display(Name = "Tên Đăng Nhập")]
        public string TenDangNhap { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }
    }
}