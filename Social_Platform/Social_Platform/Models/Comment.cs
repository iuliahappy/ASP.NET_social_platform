using System.ComponentModel.DataAnnotations;

namespace Social_Platform.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string CommentBody {  get; set; }

        public DateTime Date { get; set; }

        public int? PostId { get; set; }

        public virtual Post? Post { get; set; }
    }
}
