using StadiumBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Managers
{
    public class FansCreator
    {
        public FansCreator() { }

        public async Task<Fan> GenerateFan()
        {
            bool isHomeTeam = Random.Shared.Next(2) == 0;
            Fan newFan = new Fan(isHomeTeam);

            return newFan;
        }
    }
}
