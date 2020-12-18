using System;

namespace AllinOne.Shared.Data
{
    public class CounterService
    {
        public int Counter { get; private set; } = 0;

        public event EventHandler CounterChanged;

        public void IncrementCounter()
        {
            this.Counter++;
            this.CounterChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
