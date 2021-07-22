namespace CurePlease
{
    public class ComboBoxItemWithDetails
    {
        public string Text { get; set; }
        public int Id { get; set; }
        public string WrapperMode { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
