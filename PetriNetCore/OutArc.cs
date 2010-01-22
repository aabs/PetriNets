namespace PetriNetCore
{
    public class OutArc : Arc
    {
        public OutArc(int target)
            : base(1)
        {
            Target = target;
        }
        public int Target { get; set; }
    }
}