namespace PetriNetCore
{
    public class InArc : Arc
    {
        public InArc(int source)
            : base(1)
        {
            Source = source;
        }
        public InArc(int source, bool inhibitor)
            : base(1)
        {
            Source = source;
            IsInhibitor = inhibitor;
        }
        public int Source { get; set; }
        public bool IsInhibitor { get; set; }
    }
}