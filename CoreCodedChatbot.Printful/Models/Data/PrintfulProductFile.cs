using System;

namespace CoreCodedChatbot.Printful.Models.Data
{
    public class PrintfulProductFile
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Hash { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Mime_Type { get; set; }
        public double Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Dpi { get; set; }
        public string Status { get; set; }
        public int Created { get; set; }
        public string Thumbnail_Url { get; set; }
        public string Preview_Url { get; set; }
        public bool Visible { get; set; }
    }
}