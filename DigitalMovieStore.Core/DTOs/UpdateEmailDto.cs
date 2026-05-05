using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.DTOs
{
    public class UpdateEmailDto
    {
        public string NewEmail { get; set; }
        public string CurrentPassword { get; set; }
    }
}
