using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VeragWebApp.Repos.Models;

[Table("tblRole")]
public partial class TblRole
{
    [Key]
    [Column("username")]
    [StringLength(200)]
    public string username { get; set; } = null!;

    [Column("role")]
    [StringLength(50)]
    public string Role { get; set; } = null!;
 
}
