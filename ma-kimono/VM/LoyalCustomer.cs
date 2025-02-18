using System.ComponentModel.DataAnnotations;

namespace ma_kimono.VM
{
    public class LoyalCustomer
    {
        //from Asp.netUsers
        [Key]
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //from orders 
        public decimal TotalSpent { get; set; }
    }
}
