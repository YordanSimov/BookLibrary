namespace BookLibrary.ViewModels
{
    public class AddAndEditBookViewModel
    {
        public int? Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Pages { get; set; }

        public string Authors { get; set; }

        public string Publisher { get; set; }

        public string Genres { get; set; }
    }
}
