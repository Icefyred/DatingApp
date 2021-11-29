using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities {
    [Table("Photos")]
    public class Photo {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        /***** Photo Management Challenge 1. ******/
        public bool IsApproved { get; set; }
        public string PublicId { get; set; }
        //this two next properties allow that the photos cannot exist without an assigned user
        //even if there's no functionality of deleting users, if such were to exist,
        //and without this two properties a user would be deleted from the database, however the 
        //photos would still be around the database 
        public AppUser AppUser { get; set; }
        public int AppUserId { get; set; }
    }
}