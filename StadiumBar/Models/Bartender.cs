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
        public Func<Task> ClosingBarOrdered;
        private int _closingProbability;
        
        public Bartender(int closingProbability)
        {
            if (closingProbability < 0 || closingProbability > 100)
                throw new ArgumentOutOfRangeException(nameof(closingProbability), 
                    "The closing probability has to be between 0 and 100.");

            _closingProbability = closingProbability;
        }

        public async Task CloseBar(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1500).ConfigureAwait(false);

                if (Random.Shared.Next(0, 100) < _closingProbability)
                    await OnClosingBarOrdered();
            }
        }

        protected virtual async Task OnClosingBarOrdered()
        {
            if(ClosingBarOrdered != null)
                await ClosingBarOrdered.Invoke();
        }
    }
}
