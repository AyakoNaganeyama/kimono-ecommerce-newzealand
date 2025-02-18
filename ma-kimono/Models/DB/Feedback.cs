using System;
using System.Collections.Generic;

namespace ma_kimono.Models.DB;

public partial class Feedback
{
    public int FeedBackId { get; set; }

    public DateTime? FeedBackDate { get; set; }

    public int? CustomerId { get; set; }

    public string? Comment { get; set; }

    public virtual Customer? Customer { get; set; }
}
