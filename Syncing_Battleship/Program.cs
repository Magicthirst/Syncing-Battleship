using System.Diagnostics;
using Humanizer;
using Syncing_Battleship;

const int targetTicksPerSecond = 30;
const float millisecondsPerTick = 1000f / targetTicksPerSecond;

Console.Title = "Sync Service";

var behaviourLoader = new BehaviourLoader(args);
var sessions = new SessionsRouter
(
    behaviourLoader.Configure,
    disconnectionTimeout: 30.Seconds(),
    allowNotSotUpdates: args.Length >= 3 && args.Contains("--allowNotSotUpdates")
);

var isRunning = true;

var control = new ControlOutlet(port: 8765, sessions: sessions, stop: () => isRunning = false);
var websockets = new WebsocketsHost(port: 8766, maxClients: 100, sessions: sessions);

websockets.Start();
control.Start();

Console.WriteLine($"Main loop started, targeting {targetTicksPerSecond} ticks per second.");
Console.WriteLine("Press Ctrl+C to shut down the server gracefully.");

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    isRunning = false;
};

var stopwatch = new Stopwatch();
stopwatch.Start();
var lastTickTime = 0.0;

while (isRunning)
{
    double currentTime = stopwatch.Elapsed.TotalMilliseconds;
    int sleepTimeMs = (int) (millisecondsPerTick - (currentTime - lastTickTime));
    if (sleepTimeMs > 0)
    {
        Thread.Sleep(sleepTimeMs);
    }

    lastTickTime = currentTime;
    websockets.Update();
}

Console.WriteLine("\nShutting down...");
control.WaitForShutdown();
websockets.Stop();

Console.WriteLine("Server shut down successfully.");
return 0;
