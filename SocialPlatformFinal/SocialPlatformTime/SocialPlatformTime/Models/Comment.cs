using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace SocialPlatformTime.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment content is required!")]
        public string CommentBody { get; set; }

        public DateTime Date { get; set; }

        public DateTime? EditedDate { get; set; } // data editarii (nullable)

        public int PostId { get; set; } //FK

        public string ApplicationUserId { get; set; } //FK

        public virtual Post? Post { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }

        //// CAMPURI NOI PENTRU ANALIZA DE SENTIMENT
        // Eticheta sentimentului: "positive", "neutral", "negative"
        public string? SentimentLabel { get; set; }
        // Scorul de incredere: valoare intre 0.0 si 1.0
        public double? SentimentConfidence { get; set; }
        // Data si ora la care s-a efectuat analiza
        public DateTime? SentimentAnalyzedAt { get; set; }

    }
}
