using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Orm.Models;

[Table("RequestRight", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class RequestRight
{
    [Key]
    public int id { get; set; }

    public string name { get; set; } = null!;
}
