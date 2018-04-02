namespace TestProject.Core
{
    public class WordLocation
    {
        public long Location { get; }
        public string FileName { get; }

        public WordLocation(long location, string fileName)
        {
            Location = location;
            FileName = fileName;
        }

        public override string ToString()
        {
            return $"{FileName} ({Location})";
        }
    }
}