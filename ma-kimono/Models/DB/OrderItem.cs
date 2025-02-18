using System;
using System.Collections.Generic;

namespace ma_kimono.Models.DB;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? Qty { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
