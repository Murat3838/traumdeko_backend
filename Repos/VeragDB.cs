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

    // Neue Tabellen
    public virtual DbSet<Expense> Expenses { get; set; }
    public virtual DbSet<Income> Incomes { get; set; }
    public virtual DbSet<Event> Events { get; set; }
    public virtual DbSet<EventType> EventTypes { get; set; }
    public virtual DbSet<Todo> Todos { get; set; }
     
 

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
