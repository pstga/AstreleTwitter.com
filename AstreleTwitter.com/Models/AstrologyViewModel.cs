namespace AstreleTwitter.com.Models
{
    public class AstrologyViewModel
    {
        public DateTime BirthDate { get; set; } = DateTime.Today;

        public string? ZodiacSign { get; set; }
        public string? HoroscopeText { get; set; }
        public string? DateRange { get; set; }
        public bool HasData { get; set; } = false;
    }
}