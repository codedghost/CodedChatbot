﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class PlaylistItem
    {
        private Regex songRequestRegex = new Regex(
            "((?<index>[0-9]*)?(?<seperator> ?- ?)?(?<artistname>[\\d\\w ,.!\"£$%^&*=+\\\\\\/|<>?`¬[\\]{};\'#:@~]*)(?<seperator2> ?- ?)(?<songname>[\\d\\w ,.!\"£$%^&*\\(\\)=+\\\\\\/|<>?`¬[\\]{};\'#:@~]*)(?<instrument>(( ?- ?)?\\(?(bass|guitar)?\\)?)?))");

        private Match regMatch => songRequestRegex.Match(songRequestText);
        private string possibleInstrument => songRequestText.IndexOf("bass",  StringComparison.OrdinalIgnoreCase) >= 0 ? "bass" : "guitar";

        public int songRequestId { get; set; }
        public string songRequestText { get; set; }
        public string songRequester { get; set; }
        public bool isInChat { get; set; }
        public bool isVip { get; set; }
        public bool isEvenIndex { get; set; }

        public FormattedRequest FormattedRequest =>
            songRequestRegex.IsMatch(songRequestText)
                ? new FormattedRequest
                {
                    SongName = regMatch.Groups["songname"].Captures.FirstOrDefault()?.Value,
                    SongArtist = regMatch.Groups["artistname"].Captures.FirstOrDefault()?.Value,
                    InstrumentName = possibleInstrument
                }
                : null;
    }
}
