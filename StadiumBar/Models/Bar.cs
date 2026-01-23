using StadiumBar.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Bar
    {
        public event EventHandler<BarEventOccurredArgs> BarEventOccurred;

        // Per sincronizzazione:
        private readonly SemaphoreSlim _capacity;   // gestione dell'entrata
        private readonly object _stateLock; // gestione variabili comuni

        private BarStatus _status;
        private bool? _areHomeFansInside;
        private int _fansInside, _maxCapacity;

        public Bar(Bartender bartender, int maxCapacity)
        {
            bartender.ClosingBarOrdered = async () => { await CloseForCleaning(3000); };

            if (maxCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCapacity),
                    "Capacity must be greater than zero.");

            _maxCapacity = maxCapacity;
            _capacity = new SemaphoreSlim(_maxCapacity, _maxCapacity);
            _stateLock = new object();
            SetupState();
        }

        private void SetupState()
        {
            _status = BarStatus.Open;
            _areHomeFansInside = null;
            _fansInside = 0;
        }

        public async Task Enter(Fan fan)
        {
            bool isEntryDenied = false;

            await _capacity.WaitAsync().ConfigureAwait(false);

            try
            {
                lock (_stateLock)
                {
                    // Entrata rifiutata se:
                    // -> Bar chiuso
                    // -> Tifosi nel bar tifano una squadra diversa

                    if (IsClosedForCleaningLocked())
                    {
                        isEntryDenied = true;
                        OnBarEventOccurred(BarEvent.EntryDenied, "Bar is closed for cleaning");
                        return;
                    }

                    if (!SupportsSameTeamLocked(fan))
                    {
                        isEntryDenied = true;
                        OnBarEventOccurred(BarEvent.EntryDenied, "Opposing team fans already inside");
                        return;
                    }

                    // Altrimenti l'entrata effettiva è confermata
                    _fansInside++;
                    OnBarEventOccurred(BarEvent.FanEntered, $"Fan entered ({_fansInside}/{_maxCapacity})");
                }

                // Simulazione del tempo speso nel bar:
                await Task.Delay(fan.TimeToSpendInside)
                    .ConfigureAwait(false);
            }
            finally
            {
                // Se si è entrati nel bar effettivamente:
                // -> Si decrementa il numero di tifosi nel bar
                // Altrimenti si incrementa solamente il contatore del semaforo
                
                lock (_stateLock)
                {
                    if (!isEntryDenied)
                    {
                        _fansInside--;
                        OnBarEventOccurred(BarEvent.FanLeft, $"Fan left ({_fansInside}/{_maxCapacity})");
                    }

                    if (_fansInside <= 0)
                        _areHomeFansInside = null;
                }

                _capacity.Release();
            }
        }

        public async Task CloseForCleaning(int cleaningTime)
        {
            // Se viene richiesta la chiusura del bar:
            // -> Aspetta che il numero di fan nel bar è zero per chiudere
            // -> Riapre il bar dopo il tempo di pulizia

            lock (_stateLock)
            {
                _status = BarStatus.Closed;
                OnBarEventOccurred(BarEvent.BarClosing, "Bar is closing for cleaning");
            }

            while (true)
            {
                lock (_stateLock)
                {
                    if (_fansInside == 0)
                        break;

                }

                await Task.Delay(100);
            }

            OnBarEventOccurred(BarEvent.BarClosed, "Bar closed, cleaning started");
            await Task.Delay(cleaningTime).ConfigureAwait(false);

            lock (_stateLock)
            {
                _status = BarStatus.Open;
                OnBarEventOccurred(BarEvent.BarOpened, "Bar reopened after cleaning");
            }
        }

        private bool IsClosedForCleaningLocked()
        {
            if (_status != BarStatus.Closed)
                return false;

            return true;
        }

        private bool SupportsSameTeamLocked(Fan fan)
        {
            if ((_areHomeFansInside is bool fansType)
                && (fansType != fan.SupportsHomeTeam))
                return false;

            _areHomeFansInside ??= fan.SupportsHomeTeam;

            return true;
        }

        protected virtual void OnBarEventOccurred(BarEvent eventType, string message)
        {
            message ??= "Unknown";

            BarEventOccurred?.Invoke(this, new BarEventOccurredArgs(eventType, message));
        }
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
}
