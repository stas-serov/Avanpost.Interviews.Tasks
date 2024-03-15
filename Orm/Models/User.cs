using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Orm.Models;

[Table("User", Schema = "AvanpostIntegrationTestTaskSchema")]
public partial class User
{
    [Key]
    [StringLength(22)]
    public string login { get; set; } = null!;

    [StringLength(20)]
    public string lastName { get; set; } = null!;

    [StringLength(20)]
    public string firstName { get; set; } = null!;

    [StringLength(20)]
    public string middleName { get; set; } = null!;

    [StringLength(20)]
    public string telephoneNumber { get; set; } = null!;

    public bool isLead { get; set; }
}
