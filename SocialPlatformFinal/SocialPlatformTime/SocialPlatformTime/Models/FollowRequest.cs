using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialPlatformTime.Models
{
    public class FollowRequest
    {
        public int Id { get; set; }
        public string Status { get; set; }

        public string FollowerId { get; set; } // User who sent the follow request //FK

        public string FollowingId { get; set; } // User who received the follow request //FK

        [ForeignKey("FollowerId")]
        [InverseProperty("FollowRequestsSent")]
        public virtual ApplicationUser Follower { get; set; }

        [ForeignKey("FollowingId")]
        [InverseProperty("FollowRequestsReceived")]
        public virtual ApplicationUser Following { get; set; }


    }
}
