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
        private static Random _random = new Random(Environment.TickCount);

        public FansCreator() { }

        public async Task<Fan> GenerateFan()
        {
            await Task.Delay(_random.Next(350, 3000));

            Fan newFan = new Fan(_random.NextDouble() > 0.5f);

            return newFan;
        }
    }
}
