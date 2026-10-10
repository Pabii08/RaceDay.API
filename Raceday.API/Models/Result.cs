namespace RaceDay.API.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan Time { get; set; }
        public int Position { get; set; }
        public string Status { get; set; } = "Completed";
        public DateTime ResultDate { get; set; } = DateTime.Now;

        // Navigation property
        public Enrolment? Enrolment { get; set; }
    }
}