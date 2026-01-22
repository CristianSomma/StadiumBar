using StadiumBar.Managers;
using StadiumBar.Models;

namespace StadiumBar
{
    internal class Program
    {
        private static MainManager _manager;

        static async Task Main(string[] args)
        {
            _manager = new MainManager();
            _manager.Bar.EnteringBar += PrintMessage;
            await _manager.Simulate().ConfigureAwait(false);
        }

        static void PrintMessage(object? sender, EnteringBarEventArgs args)
        {
            Console.WriteLine(args.Message);
        }
    }
}