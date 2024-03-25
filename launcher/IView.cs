namespace launcher
{
    public interface IView
    {
        string speed { get; set; }
        float progress { get; set; }
        void speedCalculate(long bytes);
    }
}
