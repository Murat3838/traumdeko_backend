using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VeragWebApp.Repos.Models;

[Table("tblMitarbeiter")]
public partial class TblUser
{
    [Key]
    [Column("mit_username")]
    [StringLength(50)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("mit_vname")]
    [StringLength(250)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("mit_email")]
    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [Column("mit_telefonnr")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [Column("mit_pwd")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Password { get; set; }

    [Column("mit_gekuendigt")]
    public bool? isGekuendigt { get; set; }

    [Column("mit_position")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Role { get; set; }

    [Column("mit_timasId")]
    [Unicode(false)]
    public int? TimasId { get; set; }

     
}
