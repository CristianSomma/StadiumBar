using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Bar
    {
        public event EventHandler<EnteringBarEventArgs> EnteringBar;

        private bool? _areHomeFansInside;
        private int _maxCapacity;
        private BarStatus _status;
        private readonly SemaphoreSlim _barCapacity;

        public Bar(Bartender bartender, int maxCapacity)
        {
            bartender.ClosingBarOrdered += ChangeStatus;
            MaxCapacity = maxCapacity;
            _barCapacity = new SemaphoreSlim(MaxCapacity, MaxCapacity);
        }

        public int MaxCapacity
        {
            get => _maxCapacity;
            private set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Capacity must be greater than zero.");

                _maxCapacity = value;
            }
        }

        public async Task Enter(Fan fan)
        {
            if (CannotEnterBecauseClosing(fan))
            {
                await ManageClosing();
                return;
            }

            await _barCapacity.WaitAsync();

            try
            {
                if (!IsSameTeamInside(fan))
                    return;

                OnEnteringBar("The fan has entered the bar.");
                await Task.Delay(fan.TimeToSpendInside);
            }
            finally
            {
                _barCapacity.Release();
                OnEnteringBar("A fan has left the bar.");
            }
        }

        public void ChangeStatus(object? sender, EventArgs e)
        {
            _status = _status switch
            {
                BarStatus.Open => BarStatus.Closed,
                BarStatus.Closed => BarStatus.Open,
                _ => throw new ArgumentOutOfRangeException(nameof(_status), 
                "Bar current status is not implemented.")
            };
        }

        public void ChangeStatus()
        {
            _status = _status switch
            {
                BarStatus.Open => BarStatus.Closed,
                BarStatus.Closed => BarStatus.Open,
                _ => throw new ArgumentOutOfRangeException(nameof(_status), 
                "Bar current status is not implemented.")
            };
        }

        private async Task ManageClosing()
        {
            if (_barCapacity.CurrentCount != MaxCapacity)
                return;

            OnEnteringBar("The bar has closed");
            await Task.Delay(3000);
            ChangeStatus();
        }

        private bool CannotEnterBecauseClosing(Fan fan)
        {
            if (_status == BarStatus.Open)
                return false;

            OnEnteringBar("The bar is closing, the fan cannot enter.");
            return true;
        }

        private bool IsSameTeamInside(Fan fan)
        {
            if (_areHomeFansInside is null
                || _barCapacity.CurrentCount == MaxCapacity)
            {
                _areHomeFansInside = fan.SupportsHomeTeam;
            }

            if((_areHomeFansInside is bool areHomeInside) && 
                (areHomeInside != fan.SupportsHomeTeam))
            {
                OnEnteringBar("The fan is trying to enter while opponent fans are inside.");
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

    public enum BarStatus
    {
        Open = 0,
        Closed = 1,
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
