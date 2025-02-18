using System;
using System.Collections.Generic;

namespace ma_kimono.Models.DB;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeMobileNumber { get; set; } = null!;

    public string EmployeeAddress { get; set; } = null!;

    public string? UserId { get; set; }

    public virtual AspNetUser? User { get; set; }
}
