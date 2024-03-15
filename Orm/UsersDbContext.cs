using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Orm.Models;

namespace Orm;

public partial class UsersDbContext : DbContext
{
    public UsersDbContext()
    {
    }

    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ItRole> ItRoles { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<RequestRight> RequestRights { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserITRole> UserITRoles { get; set; }

    public virtual DbSet<UserRequestRight> UserRequestRights { get; set; }

    public virtual DbSet<_MigrationHistory> _MigrationHistories { get; set; }

}
