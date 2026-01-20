using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Bartender
    {
        public event EventHandler ClosingBarOrdered;
        private static Random _random = new Random(Environment.TickCount);
        private int _closingProbability;
        
        public Bartender(int closingProbability)
        {
            if (closingProbability < 0 || closingProbability > 100)
                throw new ArgumentOutOfRangeException(nameof(closingProbability), 
                    "The closing probability has to be between 0 and 100.");

            _closingProbability = closingProbability;
        }

        public async Task TryCloseBar()
        {
            await Task.Delay(1500);

            if (_random.Next(0, 100) < _closingProbability)
                OnClosingBarOrdered();
        }

        protected virtual void OnClosingBarOrdered()
        {
            ClosingBarOrdered?.Invoke(this, EventArgs.Empty);
        }
    }
}
