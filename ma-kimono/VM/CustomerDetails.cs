using System.ComponentModel.DataAnnotations;

namespace ma_kimono.VM
{
    public class CustomerDetails
    {
        //from ASP.net User
        [Display(Name = "User ID")]
        public string Id { get; set; } = null!;
        public int customerID { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        //From Customer 
        [Display(Name = "Mobile Number")]
        public string CustomerMobileNumber { get; set; } = null!;

        [Display(Name = "Shipping Address")]
        public string CustomerAddress { get; set; } = null!;

        [Display(Name = "Payment method")]
        public string? CustomerPaymentMethod { get; set; }

        [Display(Name = "Subscription")]
        public bool? IsSubscribed { get; set; }
    }
}
