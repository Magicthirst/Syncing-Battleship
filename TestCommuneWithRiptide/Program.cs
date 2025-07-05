using TestCommuneWithRiptide;

Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("\nCtrl+C pressed. Initiating graceful shutdown...");
    e.Cancel = true; // Prevent the process from terminating immediately.
    // The `_isRunning` flag in `RiptideConsoleClient` will be set to false,
    // allowing the main loop to exit cleanly.
};

var clientApp = new RiptideConsoleClient();
clientApp.Run();
