using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Orm.Models;

[Table("Passwords", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class Password
{
    [Key]
    public int id { get; set; }

    [StringLength(22)]
    public string userId { get; set; } = null!;

    [Column("password")]
    [StringLength(20)]
    public string password { get; set; } = null!;
}
