using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ma_kimono.Models.DB;

public partial class Customer
{
    public int CustomerId { get; set; }

    [Display(Name = "Mobile Number")]
    public string CustomerMobileNumber { get; set; } = null!;

    [Display(Name = "Address")]
    public string CustomerAddress { get; set; } = null!;

    [Display(Name = "Payment method")]
    public string? CustomerPaymentMethod { get; set; }


    [Display(Name = "ID")]
    public string? UserId { get; set; }



    [Display(Name = "Subscription")]
    public bool? IsSubscribed { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual AspNetUser? User { get; set; }
}
