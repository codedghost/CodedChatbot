using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class SongRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SongRequestId { get; set; }
        public string RequestUsername { get; set; }
        public int SongId { get; set; }
        public DateTime RequestTime { get; set; }
        
        public bool Played { get; set; }
        public bool InDrive { get; set; }
        public DateTime? VipRequestTime { get; set; }
        public DateTime? SuperVipRequestTime { get; set; }
        public string RequestText { get; set; } // This is temporary functionality until we are hooked up to CustomsForge

        //public virtual Song Song { get; set; }
    }
}
