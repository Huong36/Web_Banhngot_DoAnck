using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Banhngot_DoAnck.Models
{
    public class ProductRecommendation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId_A { get; set; } // Sản phẩm khách hàng đang xem

        [Required]
        public int ProductId_B { get; set; } // Sản phẩm được gợi ý mua kèm

        [ForeignKey("ProductId_A")]
        public virtual Product ProductA { get; set; }

        [ForeignKey("ProductId_B")]
        public virtual Product ProductB { get; set; }

        // Độ tự tin (Confidence) của luật: Ví dụ 0.8 (80%)
        public double Confidence { get; set; }
        
        // Độ hỗ trợ (Support): Tần suất xuất hiện cùng nhau
        public double Support { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
