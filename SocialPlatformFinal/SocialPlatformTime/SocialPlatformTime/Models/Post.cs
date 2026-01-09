using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string? PostDescription { get; set; }

        public string? TextContent { get; set; }

        public string? Image { get; set; } //@ The path for the photo in wwwroot

        public string? Video { get; set; } //@ The path for the video in wwwroot

        public DateTime Date { get; set; }

        public string ApplicationUserId { get; set; } //FK

        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Reaction>? Reactions { get; set; } = new List<Reaction>();

        public virtual ICollection<SavedPost>? SavedPosts { get; set; } = new List<SavedPost>();


        // proprietate doar pentru upload imagini, nu se mapează în DB
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [NotMapped]
        public IFormFile? VideoFile { get; set; }


        //// CAMPURI NOI PENTRU ANALIZA DE SENTIMENT
        // Eticheta sentimentului: "positive", "neutral", "negative"
        public string? SentimentLabel { get; set; }
        // Scorul de incredere: valoare intre 0.0 si 1.0
        public double? SentimentConfidence { get; set; }
        // Data si ora la care s-a efectuat analiza
        public DateTime? SentimentAnalyzedAt { get; set; }
    }
}
