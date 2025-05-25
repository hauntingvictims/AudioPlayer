namespace AudioPlayer.ViewModels
{
    public class TrackViewModel
    {
        public int Number { get; set; }
        public string FilePath { get; set; }

        public TrackViewModel(int number, string filePath)
        {
            Number = number;
            FilePath = filePath;
        }
    }
}