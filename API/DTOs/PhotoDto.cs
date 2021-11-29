namespace API.DTOs
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        /***** Photo Management Challenge 3. ******/
        public bool IsApproved { get; set; }
    }
}