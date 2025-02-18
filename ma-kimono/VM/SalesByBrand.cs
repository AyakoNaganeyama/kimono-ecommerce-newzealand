using System.ComponentModel.DataAnnotations;

namespace ma_kimono.VM
{
    public class SalesByBrand
    {
        //From Brand table
        [Key]
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        //from OrderItemTable
        public decimal Total { get; set; }
    }
}
