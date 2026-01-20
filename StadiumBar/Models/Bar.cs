using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Bar
    {
        public event EventHandler<EnteringBarEventArgs> EnteringBar;

        private bool _areHomeFansInside, _hasToCloseForCleaning;
        private readonly SemaphoreSlim _barCapacity;

        public Bar(int maxCapacity)
        {
            if (maxCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity),
                    "Capacity must be greater than zero.");

            _barCapacity = new SemaphoreSlim(maxCapacity, maxCapacity);
        }

        public async Task Enter(Fan fan)
        {
            if (!CanEnter(fan)) 
                return;

            await _barCapacity.WaitAsync();
            try
            {
                OnEnteringBar("The fan has entered the bar.");
                await Task.Delay()
            }
            finally
            {
                _barCapacity.Release();
                OnEnteringBar("A fan has leaved the bar.");
            }
        }

        private bool CanEnter(Fan fan)
        {
            if (fan.SupportsHomeTeam && !_areHomeFansInside
                || !fan.SupportsHomeTeam && _areHomeFansInside)
            {
                OnEnteringBar("The fan is trying to enter while opponent fans are inside.");
                return false;
            }

            if (_hasToCloseForCleaning)
            {
                OnEnteringBar("The bar is closing, the fan cannot enter.");
                return false;
            }

            return true;
        }

        protected virtual void OnEnteringBar(string messageToSend)
        {
            if (messageToSend is null) 
                messageToSend = "Unknown";

            EnteringBar?.Invoke(this, new EnteringBarEventArgs(messageToSend));
        }
    }

    public class EnteringBarEventArgs : EventArgs
    {
        private string _message;

        public EnteringBarEventArgs(string message)
        {
            Message = message;
        }

        public string Message
        {
            get => _message;
            private set
            {
                ArgumentNullException.ThrowIfNullOrEmpty(value);

                _message = value;
            }
        }
    }
}
