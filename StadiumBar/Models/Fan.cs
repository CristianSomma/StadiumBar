using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Models
{
    public class Fan
    {
        private int _timeToSpendInside;
        private bool _supportsHomeTeam;

        public Fan(bool supportsHomeTeam)
        {
            SupportsHomeTeam = supportsHomeTeam;
            _timeToSpendInside = Random.Shared.Next(450, 1250);
        }

        public bool SupportsHomeTeam
        {
            get => _supportsHomeTeam;
            private set
            {
                _supportsHomeTeam = value;
            }
        }

        public int TimeToSpendInside => _timeToSpendInside;
    }
}
