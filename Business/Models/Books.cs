using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{
    [Index(nameof(ISBN), IsUnique = true)]
    public class Books
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tiêu đề sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [MaxLength(100, ErrorMessage = "Tên tác giả không được vượt quá 100 ký tự")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "ISBN là bắt buộc")]
        [RegularExpression(@"^(?:\d{10}|\d{13})$",
            ErrorMessage = "ISBN không hợp lệ. Vui lòng nhập 10 hoặc 13 chữ số liên tiếp")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }


        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.Now;

        [Display(Name = "Mô tả")]
        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; }

        [Display(Name = "Hình ảnh")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "Nhà xuất bản")]
        public string Publisher { get; set; }

        [Display(Name = "Ngày xuất bản")]
        [DataType(DataType.Date)]
        public DateTime? PublishedDate { get; set; }

        public string GoogleBooksId { get; set; }

        // Navigation Properties
        public virtual Category Category { get; set; }
        public virtual ICollection<BorrowRecords> BorrowRecords { get; set; }
    }
}
