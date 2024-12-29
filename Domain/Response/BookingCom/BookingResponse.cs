namespace AutomatedWebscraper.Domain.Response.BookingCom
{
    public class BookingResponse
    {
        public InputData Input { get; set; }
        public string Url { get; set; }
        public string Location { get; set; }
        public string Check_in { get; set; }
        public string Check_out { get; set; }
        public int Adults { get; set; }
        public int? Children { get; set; }
        public int Rooms { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public double Review_score { get; set; }
        public string Review_count { get; set; }
        public string Image { get; set; }
        public decimal Final_price { get; set; }
        public decimal Original_price { get; set; }
        public string Currency { get; set; }
        public string Tax_description { get; set; }
        public int Nb_livingrooms { get; set; }
        public int Nb_kitchens { get; set; }
        public int Nb_bedrooms { get; set; }
        public int Nb_all_beds { get; set; }
        public FullLocation Full_location { get; set; }
        public bool No_prepayment { get; set; }
        public bool Free_cancellation { get; set; }
        public PropertySustainability Property_sustainability { get; set; }
        public string Timestamp { get; set; }
    }

    public class InputData
    {
        public string Url { get; set; }
        public string Location { get; set; }
        public string Check_in { get; set; }
        public string Check_out { get; set; }
        public int Adults { get; set; }
        public int Rooms { get; set; }
    }

    public class FullLocation
    {
        public string Description { get; set; }
        public string Main_distance { get; set; }
        public string Display_location { get; set; }
        public string Beach_distance { get; set; }
        public List<string> Nearby_beach_names { get; set; }
    }

    public class PropertySustainability
    {
        public bool Is_sustainable { get; set; }
        public string Level_id { get; set; }
        public List<string> Facilities { get; set; }
    }
}
