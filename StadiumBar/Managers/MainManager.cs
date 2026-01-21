using StadiumBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumBar.Managers
{
    public class MainManager
    {
        private Bar _bar;
        private Bartender _bartender;
        private FansCreator _fansCreator;
        private CancellationTokenSource _tokenSource;

        public MainManager()
        {
            _bartender = new Bartender(20);
            _bar = new Bar(_bartender, 10);
            _fansCreator = new FansCreator();
            _tokenSource = new CancellationTokenSource();
        }

        public Bar Bar => _bar;

        public async Task Simulate()
        {
            _ = Task.Run(async () =>
            {
                await _bartender.TryCloseBar();
            });
            
            while(true)
            {
                Fan fan = await _fansCreator.GenerateFan();
                
                _ = Task.Run(async () =>
                {
                    await _bar.Enter(fan);
                });

                await Task.Delay(200);
            }
        }
    }
}
