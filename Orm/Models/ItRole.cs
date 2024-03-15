using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Orm.Models;

[Table("ItRole", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class ItRole
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    public string name { get; set; } = null!;

    [StringLength(4)]
    public string corporatePhoneNumber { get; set; } = null!;
}
