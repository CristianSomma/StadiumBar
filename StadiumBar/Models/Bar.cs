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
        public event EventHandler<BarEventOccurredArgs> BarEventOccurred;
        
        // status del bar (aperto, in chiusura, chiuso)
        private BarStatus _status;
        // tifosi che sono all'interno del bar
        private int _fansInside;

        // oggetto per la gestione della lettura/scrittura di variabili condivise
        private readonly object _stateLock;

        private bool? _areHomeFansInside;
        private int _maxCapacity;
        private readonly SemaphoreSlim _barCapacity;

        public Bar(Bartender bartender, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            _barCapacity = new SemaphoreSlim(MaxCapacity, MaxCapacity);
            _fansInside = 0; // contatore dei fan nel bar, separato per evitare race conditions
            _stateLock = new object();
        }

        // Property per la capacità massima del bar
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

        // Metodo principale:
        public async Task Enter(Fan fan)
        {
            bool hasEntered = true;
            
            // se il bar è in chiusura, allora...
            if (CannotEnterBecauseClosing(fan))
            {
                // se non c'é nessuno nel bar allora chiude per pulizie
                await ManageClosing().ConfigureAwait(false);
                // il tifoso non può entrare
                return; 
            }

            // se c'é spazio nel bar, allora il tifoso entra
            await _barCapacity.WaitAsync().ConfigureAwait(false);

            try
            {
                lock (_stateLock)
                {
                    // se il tifoso supporta una squadra diversa dai tifosi dentro il bar, esce
                    if (!IsSameTeamInside(fan))
                    {
                        hasEntered = false;
                        return;
                    }

                    // incrementa il numero dei fan nel bar (entrato)
                    _fansInside++;
                    OnBarEventOccurred(BarEvent.FanEntered, "The fan has entered the bar.");
                }

                await Task.Delay(fan.TimeToSpendInside)
                    .ConfigureAwait(false);
            }
            finally
            {
                // se è entrato nel bar, allora lo toglie dal contatore dei presenti nel bar
                // se l'accesso non gli è stato consentito non è entrato e quindi non viene decrementato
                lock (_stateLock)
                {
                    if (hasEntered)
                    {
                        // decrementa il numero dei fan nel bar (uscito)
                        _fansInside--;
                        OnBarEventOccurred(BarEvent.FanLeft, "A fan has left the bar.");
                    }

                    // se non ci sono più fan dentro, reset a null
                    if (_fansInside == 0)
                        _areHomeFansInside = null;
                }

                // rilascio indipendente dall'entrata
                _barCapacity.Release();
            }
        }

        public void ChangeStatus()
        {
            lock (_stateLock)
            {
                _status = _status switch
                {
                    BarStatus.Open => BarStatus.Closed,
                    BarStatus.Closed => BarStatus.Open,
                    _ => throw new ArgumentOutOfRangeException(nameof(_status),
                    "Bar current status is not implemented.")
                };
            }
        }

        private async Task ManageClosing()
        {
            bool isClose = false;
            
            // se non ci sono più tifosi dentro al bar, allora va in chiusura
            lock (_stateLock)
            {
                if (_fansInside == 0)
                {
                    _status = BarStatus.Closed;
                    isClose = true;
                }
            }

            if (isClose)
            {
                OnBarEventOccurred(BarEvent.BarClosed, "The bar has closed");

                await Task.Delay(Random.Shared.Next(3000, 7000))
                    .ConfigureAwait(false);

                // il bar diventa aperto
                lock (_stateLock)
                    _status = BarStatus.Open;
            }
        }

        private bool CannotEnterBecauseClosing(Fan fan)
        {
            lock (_stateLock)
            {
                // se il bar è aperto, ritorna false (non in chiusura)
                if (_status == BarStatus.Open)
                    return false;

                // altrimenti il bar viene considerato come chiuso o in chiusura
                OnBarEventOccurred(BarEvent.BarClosing, "The bar is closing, the fan cannot enter.");
                return true;
            }
        }

        private bool IsSameTeamInside(Fan fan)
        {
            // se nessuno è dentro allora fan dentro supportano la squadra
            // di quello in entrata
            if (_areHomeFansInside is null || _fansInside == 0)
                _areHomeFansInside = fan.SupportsHomeTeam;

            // se invece i fan dentro supportano una squadra diversa da quello che cerca
            // di entrare viene cacciato
            if ((_areHomeFansInside is bool areHomeInside) &&
                (areHomeInside != fan.SupportsHomeTeam))
            {
                OnBarEventOccurred(BarEvent.EntryDenied, "The fan is trying to enter while opponent fans are inside.");
                return false;
            }

                return true;
        }

        protected virtual void OnBarEventOccurred(BarEvent eventType, string messageToSend)
        {
            if (messageToSend is null) 
                messageToSend = "Unknown";

            BarEventOccurred?.Invoke(this, new BarEventOccurredArgs(eventType, messageToSend));
        }
    }

    public enum BarStatus
    {
        Open = 0,
        Closed = 1,
    }

    public class BarEventOccurredArgs : EventArgs
    {
        private BarEvent _eventType;
        private string _message;

        public BarEventOccurredArgs(BarEvent eventType, string message)
        {
            Message = message;
            _eventType = eventType;
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

        public BarEvent EventType => _eventType;
    }

    public enum BarEvent
    {
        FanEntered,
        FanLeft,
        EntryDenied,
        BarClosing,
        BarClosed,
        BarOpened
    }
}
