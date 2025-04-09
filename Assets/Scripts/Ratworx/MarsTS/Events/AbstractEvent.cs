namespace Ratworx.MarsTS.Events
{
    public abstract class AbstractEvent
    {
        public string Name { get; private set; }
        public Phase Phase { get; set; }
        public bool Canceled { get; set; }

        protected AbstractEvent(string name) {
            Name = name;
            Phase = Phase.Pre;
            Canceled = false;
        }
    }
}