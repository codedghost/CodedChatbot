using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class GiveawayEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GiveawayEntryId { get; set; }
        public string Username { get; set; }
        public DateTime EntryTime { get; set; }
    }
}
