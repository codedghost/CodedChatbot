using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class InfoCommand
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InfoCommandId { get; set; }
        public string InfoText { get; set; }
        public string InfoHelpText { get; set; }

        public virtual ICollection<InfoCommandKeyword> InfoCommandKeywords { get; set; }
    }
}
