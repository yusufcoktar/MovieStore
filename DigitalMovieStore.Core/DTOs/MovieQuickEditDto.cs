using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.DTOs
{
    public class MovieQuickEditDto
    {
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
