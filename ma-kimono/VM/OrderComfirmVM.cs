using ma_kimono.Models.DB;

namespace ma_kimono.VM
{
    public class OrderComfirmVM
    {
        public string FirstName { get; set; }
        public string Address { get; set; }
        public decimal? total { get; set; }
    }
}
