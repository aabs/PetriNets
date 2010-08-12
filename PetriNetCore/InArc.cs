namespace PetriNetCore
{
    public class InArc : Arc
    {
        public InArc(int source, int weight = 1, bool inhibitor = false)
            : base(weight)
        {
            Source = source;
            IsInhibitor = inhibitor;
        }
        public int Source { get; set; }
        public bool IsInhibitor { get; set; }
    }
}