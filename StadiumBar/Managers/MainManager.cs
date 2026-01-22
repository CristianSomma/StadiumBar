using StadiumBar.Models;
using System;
using System.Collections.Generic;
using System.Threading;
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
            _bar = new Bar(_bartender, 3);
            _fansCreator = new FansCreator();
            _tokenSource = new CancellationTokenSource();
        }

        public Bar Bar => _bar;

        public async Task Simulate()
        {
            _ = _bartender.CloseBar(_tokenSource.Token);

            while (!_tokenSource.Token.IsCancellationRequested)
            {
                Fan fan = await _fansCreator.GenerateFan();

                _ = Task.Run(async () => await _bar.Enter(fan));

                await Task.Delay(Random.Shared.Next(200, 700));
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}