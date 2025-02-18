using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ma_kimono.Models.DB;

public partial class Product
{
    public int ProductId { get; set; }

    [Display(Name ="Product Name")]

    public string ProductName { get; set; } = null!;


    [Display(Name = "Desription")]
    public string? ProductDescription { get; set; }

    [Display(Name = "Price")]
    public decimal ProductPrice { get; set; }

    [Display(Name = "Quantity")]
    public int ProductQty { get; set; }


    [Display(Name = "Category")]
    [Required(ErrorMessage = "Category is required.")]
    public int? CategoryId { get; set; }


    [Display(Name = "Material")]
    [Required(ErrorMessage = "Material is required.")]
    public string ProductMaterial { get; set; } = null!;


    [Display(Name = "Brand")]

    [Required(ErrorMessage = "Brand is required.")]

    public int? BrandId { get; set; }

    public string Size { get; set; } = null!;

    public string Colour { get; set; } = null!;

    [Display(Name = "Image URL")]
    public string? ProductImg { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
