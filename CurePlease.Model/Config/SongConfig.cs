
namespace CurePlease.Model.Config
{
    public class SongConfig
    {
        public bool SingingEnabled { get; set; }

        public int Song1 { get; set; }
        public int Song2 { get; set; }
        public int Song3 { get; set; }
        public int Song4 { get; set; }

        public int Dummy1 { get; set; }
        public int Dummy2 { get; set; }

        public decimal SongRecastMinutes { get; set; }
        public bool SingOnlyWhenNear { get; set; }
    }
}
