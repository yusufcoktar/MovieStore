using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.DTOs
{
    public class ReviewCreateDto
    {
        [Required(ErrorMessage = "Yorum alanı boş bırakılamaz.")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Puan 1 ile 10 arasında olmalıdır.")]
        public int Rating { get; set; }
    }
}
