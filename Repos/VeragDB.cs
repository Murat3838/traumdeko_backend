using System;
using System.Collections.Generic;
using VeragWebApp.Modal;
using VeragWebApp.Repos.Models;
using Microsoft.EntityFrameworkCore;

namespace VeragWebApp.Repos;

public partial class VeragDB : DbContext
{
    public VeragDB()
    {
    }

    public VeragDB(DbContextOptions<VeragDB> options)
        : base(options)
    {
    }

 
    public virtual DbSet<TblRefreshtoken> TblRefreshtokens { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

 
    public virtual DbSet<TblUser> TblUsers { get; set; }
     
 

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
