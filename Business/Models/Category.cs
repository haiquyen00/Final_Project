using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models
{
    public class Category
    {
        public Category()
        {
            Books = new HashSet<Books>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public virtual ICollection<Books> Books { get; set; }
    }
}