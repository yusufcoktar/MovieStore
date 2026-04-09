using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // Filmin o anki satın alınma fiyatı

        // İlişkiler (Foreign Keys)
        public int UserId { get; set; }//bu ve alttakini yazdık istedeiğimiz zaman kullanıcının bütün bilgileriniz getirebilsin yoksa sürekli veri tabanınından sorgulamamız gerekirdi 
        // Bu kısım SQL'de bir kolon DEĞİLDİR. Bu tamamen bizim C# kodunda rahat etmemiz içindir.
        // Entity Framework, yukarıdaki 'UserId' (Örn: 5) sayısına bakar, gider User tablosundan 5 numaralı adamı bulur 
        // ve o adamın tüm bilgilerini (Adı, Emaili vs.) bu nesnenin içine otomatik doldurur.
        public User User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
