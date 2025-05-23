using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VeragWebApp.Repos.Models
{
    [Table("tbl_refreshtoken")]
    public partial class TblRefreshtoken
    {

        [Key]
        public int Id { get; set; } // Primärschlüssel
 
        [Column("deviceid")]
        [StringLength(50)]
        [Unicode(false)]
        public string DeviceId { get; set; } = Guid.NewGuid().ToString("N");

        [Column("userid")]
        [StringLength(50)]
        [Unicode(false)]
        public string UserId { get; set; } = null!;

        [Column("refreshtoken")]
        [Unicode(false)]
        public string? RefreshToken { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("expires")]
        public DateTime Expires { get; set; }

        [Column("revoked")]
        public DateTime? Revoked { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= Expires;

        [NotMapped]
        public bool IsActive => !IsExpired && Revoked == null;

 
    }
}
