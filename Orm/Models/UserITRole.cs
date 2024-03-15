using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Orm.Models;

[PrimaryKey("roleId", "userId")]
[Table("UserITRole", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class UserITRole
{
    [Key]
    [StringLength(22)]
    public string userId { get; set; } = null!;

    [Key]
    public int roleId { get; set; }
}
