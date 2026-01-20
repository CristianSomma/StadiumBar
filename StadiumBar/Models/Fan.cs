using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Fan
    {
        private static Random _random = new Random(Environment.TickCount);
        private int _timeToSpendInside;
        private bool _supportsHomeTeam;

        public Fan(bool supportsHomeTeam)
        {
            SupportsHomeTeam = supportsHomeTeam;
            _timeToSpendInside = _random.Next()
        }

        public bool SupportsHomeTeam
        {
            get => _supportsHomeTeam;
            private set
            {
                _supportsHomeTeam = value;
            }
        }

        public int TimeToSpendInside
        {
            get => _time
        }
    }
}
