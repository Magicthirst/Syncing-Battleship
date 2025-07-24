using System.Diagnostics;
using Humanizer;
using Riptide.Utils;
using Syncing_Battleship;

const int targetTicksPerSecond = 30;
var tickInterval = TimeSpan.FromSeconds(1.0 / targetTicksPerSecond);

Console.Title = "Sync Service";

var riptideLogsFilePath = Environment.GetEnvironmentVariable("RIPTIDE_LOG");
if (riptideLogsFilePath == null)
{
    throw new ArgumentException(nameof(riptideLogsFilePath));
}

var behaviourLoader = new BehaviourLoader(args);
var sessions = new SessionsRouter
(
    behaviourLoader.Configure,
    disconnectionTimeout: 30.Seconds(),
    allowNotSotUpdates: args.Length >= 3 && args.Contains("--allowNotSotUpdates")
);

var isRunning = true;

var control = new ControlOutlet(port: 8765, sessions: sessions, stop: () => isRunning = false);
var riptide = new RiptideServer(port: 8766, maxClients: 100, sessions: sessions, logsPath: riptideLogsFilePath);

riptide.Start();
control.Start();

RiptideLogger.Log(LogType.Info, $"Main loop started, targeting {targetTicksPerSecond} ticks per second.");
RiptideLogger.Log(LogType.Info, "Press Ctrl+C to shut down the server gracefully.");

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    isRunning = false;
};

var tickStopwatch = new Stopwatch();
tickStopwatch.Start();

while (isRunning)
{
    var sleepTime = tickInterval - tickStopwatch.Elapsed;
    if (sleepTime > TimeSpan.Zero)
    {
        Thread.Sleep(sleepTime);
    }

    riptide.Update();
    tickStopwatch.Restart();
}

RiptideLogger.Log(LogType.Info, "\nShutting down...");
control.WaitForShutdown();
riptide.Stop();

Console.WriteLine("Server shut down successfully.");
return 0;
