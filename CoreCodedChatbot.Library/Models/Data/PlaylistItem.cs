﻿namespace CoreCodedChatbot.Library.Models.Data
{
    public class PlaylistItem
    {
        public int songRequestId { get; set; }
        public string songRequestText { get; set; }
        public string songRequester { get; set; }
        public bool isInChat { get; set; }
        public bool isVip { get; set; }
        public bool isSuperVip { get; set; }
        public bool isEvenIndex { get; set; }
        public bool isInDrive { get; set; }

        public FormattedRequest FormattedRequest => FormattedRequest.GetFormattedRequest(songRequestText);
    }
}
