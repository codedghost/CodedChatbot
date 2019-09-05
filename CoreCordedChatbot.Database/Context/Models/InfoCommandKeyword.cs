using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class InfoCommandKeyword
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string InfoCommandKeywordText { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InfoCommandId { get; set; }

        public virtual InfoCommand InfoCommand {get; set; }
    }
}
