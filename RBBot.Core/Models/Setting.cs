namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Setting")]
    public partial class Setting
    {
        public int Id { get; set; }

        public int? ExchangeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(1024)]
        public string Value { get; set; }

        [Required]
        public bool IsEncrypted { get; set; }

        [Required]
        public bool ShouldBeEncrypted { get; set; }

        public virtual Exchange Exchange { get; set; }

    }
}
