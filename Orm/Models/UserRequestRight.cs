using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Orm.Models;

[PrimaryKey("rightId", "userId")]
[Table("UserRequestRight", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class UserRequestRight
{
    [Key]
    [StringLength(22)]
    public string userId { get; set; } = null!;

    [Key]
    public int rightId { get; set; }
}
