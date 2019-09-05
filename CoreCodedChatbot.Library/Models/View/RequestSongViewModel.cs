using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreCodedChatbot.Library.Models.View
{
    public class RequestSongViewModel
    {
        public int SongRequestId { get; set; }

        public string ModalTitle { get; set; }

        public bool IsNewRequest { get; set; }

        [Display(Name="Song Name")]
        public string Title { get; set; }

        [Display(Name="Song Artist")]
        public string Artist { get; set; }

        public string SelectedInstrument { get; set; }

        [Display(Name="Instrument")]
        public SelectListItem[] Instruments { get; set; }

        [Display(Name="Use a VIP token?")]
        public bool IsVip { get; set; }

        [Display(Name="Use a Super VIP token?")]
        public bool IsSuperVip { get; set; }

        public bool ShouldShowVip { get; set; }

        public bool ShouldShowSuperVip { get; set; }
    }
}
