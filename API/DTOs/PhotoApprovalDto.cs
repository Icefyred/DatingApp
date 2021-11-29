/**** Photo Management Challenge 8. ****/
namespace API.DTOs{
    public class PhotoApprovalDto{
        public int PhotoId { get; set; }
        public string PhotoUrl { get; set; }
        public string Username { get; set; }
        public bool IsApproved { get; set; }
    }
}
