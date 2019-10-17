using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class StreamStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StreamStatusId { get; set; }
        public string BroadcasterUsername { get; set; }
        public bool IsOnline { get; set; }
    }
}
