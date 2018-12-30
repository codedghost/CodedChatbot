using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreCodedChatbot.Library.Models.View
{
    public class RequestSongViewModel
    {
        [Display(Name="Song Name")]
        public string Title { get; set; }

        [Display(Name="Song Artist")]
        public string Artist { get; set; }

        public string SelectedInstrument { get; set; }

        [Display(Name="Instrument")]
        public SelectListItem[] Instruments { get; set; }

        [Display(Name="Use a VIP token?")]
        public bool IsVip { get; set; }

        public bool ShouldShowVip { get; set; }
    }
}
