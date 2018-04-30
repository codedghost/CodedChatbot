using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class Song
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SongId { get; set; }
        public string SongName { get; set; }
        public string SongArtist { get; set; }

        //public virtual List<SongRequest> SongRequests { get; set; }
    }
}
