using Raceday.API.Models;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; } // Participant
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Confirmed";

        // Navigation properties
        public User? Participant { get; set; }
        public Event? Event { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}