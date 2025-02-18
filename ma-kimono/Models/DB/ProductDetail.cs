using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ma_kimono.Models.DB
{

    //29/05 Ayako Multi
    public class ProductDetail
    {

        public int Id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public decimal price { get; set; }

        public int qty { get; set; }

        public string material { get; set; }

        public string size { get; set; }

        public string colour { get; set; }

        [Display(Name = "Image URL")]

        public string img { get; set; }


        public string brand { get; set; }

        public string cate { get; set; }




    }
}
