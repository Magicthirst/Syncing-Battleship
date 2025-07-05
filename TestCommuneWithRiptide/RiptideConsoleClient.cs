using Riptide;
using Riptide.Utils;
using Syncing_Battleship_Common_Typing;

namespace TestCommuneWithRiptide;

/// <summary>
/// A console-based Riptide client for testing and debugging Riptide services.
/// Allows composing and sending messages, and viewing received messages in binary format.
/// </summary>
public class RiptideConsoleClient
{
    private Client _client;
    private bool _isRunning = true;
    
    // Stores the parts of the message currently being composed by the user.
    // Each tuple contains the type of data (e.g., "int", "string", "bool") and its object value.
    private List<(string type, object value)> _messageParts = new List<(string, object)>();
    
    // The default message ID to use for new outgoing messages. Can be changed by the user.
    private ushort _defaultMessageId = 0; 

    /// <summary>
    /// Initializes a new instance of the RiptideConsoleClient.
    /// Sets up Riptide logging and subscribes to client events.
    /// </summary>
    public RiptideConsoleClient()
    {
        // Initialize Riptide's logger to output to the console.
        RiptideLogger.Initialize(Console.WriteLine, true); 
        _client = new Client();

        // Subscribe to Riptide client lifecycle events.
        _client.Connected += Client_Connected;
        _client.Disconnected += Client_Disconnected;
        _client.MessageReceived += Client_MessageReceived;
    }

    /// <summary>
    /// Starts the Riptide client application, including its console interaction loop
    /// and a background task for Riptide's network updates.
    /// </summary>
    public void Run()
    {
        Console.WriteLine("Riptide Console Client Started.");
        Console.WriteLine("Type 'help' for commands.");

        // Start a separate background task to continuously run the Riptide client's update loop.
        // This ensures the client can process incoming/outgoing network traffic independently
        // of the main thread which handles console input.
        Task.Run(ClientUpdateLoop);

        // The main thread handles reading commands from the console.
        while (_isRunning)
        {
            Console.Write("> ");
            // Read a line of input from the user. `Trim()` removes leading/trailing whitespace.
            // Null-coalescing operator `?? string.Empty` handles cases where `ReadLine()` returns null.
            string input = Console.ReadLine()?.Trim() ?? string.Empty; 

            ProcessConsoleInput(input);
        }

        // After the main loop exits (when _isRunning becomes false), ensure the client is disconnected.
        if (_client.IsConnected)
        {
            _client.Disconnect();
        }
        Console.WriteLine("Riptide Console Client Shut Down.");
    }

    /// <summary>
    /// Attempts to connect the Riptide client to the specified server.
    /// </summary>
    /// <param name="ip">The IP address of the server to connect to.</param>
    /// <param name="port">The port number of the server.</param>
    private void Connect(string ip, ushort port)
    {
        if (_client.IsConnected)
        {
            Console.WriteLine("Already connected. Disconnect first if you want to connect to a different server.");
            return;
        }

        Console.WriteLine($"Attempting to connect to {ip}:{port}...");
        _client.Connect($"{ip}:{port}");
    }

    /// <summary>
    /// Disconnects the Riptide client from the currently connected server.
    /// </summary>
    private void Disconnect()
    {
        if (!_client.IsConnected)
        {
            Console.WriteLine("Not connected.");
            return;
        }
        Console.WriteLine("Disconnecting...");
        _client.Disconnect();
    }

    /// <summary>
    /// The continuous update loop for the Riptide client.
    /// This method calls `_client.Update()` periodically to process network events.
    /// </summary>
    private void ClientUpdateLoop()
    {
        while (_isRunning)
        {
            _client.Update(); // Process Riptide's internal networking logic
            Thread.Sleep(10); // Pause briefly to prevent high CPU usage (busy-waiting)
        }
    }

    /// <summary>
    /// Event handler for when the Riptide client receives a message from the server.
    /// Displays the message ID and its raw binary (hexadecimal) content.
    /// </summary>
    private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        // Use a lock to prevent console output from being interleaved with the input prompt,
        // ensuring cleaner display of incoming messages.
        lock (Console.Out)
        {
            Console.WriteLine($"[RECEIVED] Message From: {e.FromConnection.Id}");
            Console.WriteLine($"[RECEIVED] Message Id: {MessageMarkSupply.DescriptionOf(e.MessageId)}");
            Console.WriteLine($"[RECEIVED] Message Payload: {BitConverter.ToString(e.Message.GetBytes(e.Message.BytesInUse)).Replace("-", " ")}");
            Console.Write("> "); // Re-draw the input prompt after displaying the message.
        }
    }

    /// <summary>
    /// Event handler for when the Riptide client successfully connects to a server.
    /// </summary>
    private void Client_Connected(object sender, EventArgs e)
    {
        lock (Console.Out)
        {
            Console.WriteLine("\n[STATUS] Connected to server!");
            Console.Write("> ");
        }
    }

    /// <summary>
    /// Event handler for when the Riptide client disconnects from a server.
    /// </summary>
    private void Client_Disconnected(object sender, EventArgs e)
    {
        lock (Console.Out)
        {
            Console.WriteLine("\n[STATUS] Disconnected from server.");
            Console.Write("> ");
        }
    }

    /// <summary>
    /// Parses and processes user commands entered into the console.
    /// </summary>
    /// <param name="input">The raw string input from the console.</param>
    private void ProcessConsoleInput(string input)
    {
        // Split the input into command and arguments. Limit to 2 parts: command and the rest as args.
        string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        // The command itself is case-insensitive for easier user input.
        string command = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
        // Arguments might be case-sensitive (e.g., string values), so keep original casing.
        string args = parts.Length > 1 ? parts[1] : string.Empty;

        switch (command)
        {
            case "connect":
                HandleConnectCommand(args);
                break;
            case "disconnect":
                Disconnect();
                break;
            case "int":
                AddIntToMessage(args);
                break;
            case "string":
                AddStringToMessage(args);
                break;
            case "bool":
                AddBoolToMessage(args);
                break;
            case "send":
                SendMessage(args);
                break;
            case "clear":
                ClearCurrentMessage();
                break;
            case "setid":
                SetDefaultMessageId(args);
                break;
            case "exit":
                _isRunning = false; // Set flag to stop main loop and exit application.
                break;
            case "help":
                DisplayHelp();
                break;
            default:
                Console.WriteLine("Unknown command. Type 'help' for available commands.");
                break;
        }
    }

    /// <summary>
    /// Handles the 'connect' command. Parses the IP address and port from the arguments.
    /// </summary>
    /// <param name="args">The arguments string (e.g., "127.0.0.1 8766").</param>
    private void HandleConnectCommand(string args)
    {
        string[] connectArgs = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Expects exactly two arguments: IP and Port.
        if (connectArgs.Length == 2 && ushort.TryParse(connectArgs[1], out ushort port))
        {
            Connect(connectArgs[0], port);
        }
        else
        {
            Console.WriteLine("Usage: connect <ip> <port> (e.g., connect 127.0.0.1 8766)");
        }
    }

    /// <summary>
    /// Adds an integer value to the current message composition list.
    /// </summary>
    /// <param name="arg">The string representation of the integer (e.g., "123").</param>
    private void AddIntToMessage(string arg)
    {
        if (int.TryParse(arg, out int value))
        {
            _messageParts.Add(("int", value));
            Console.WriteLine($"Queued int: {value}");
        }
        else
        {
            Console.WriteLine("Invalid integer value. Usage: int <value>");
        }
    }

    /// <summary>
    /// Adds a string value to the current message composition list.
    /// </summary>
    /// <param name="arg">The string value to add (e.g., "hello world").</param>
    private void AddStringToMessage(string arg)
    {
        _messageParts.Add(("string", arg));
        Console.WriteLine($"Queued string: \"{arg}\"");
    }

    /// <summary>
    /// Adds a boolean value to the current message composition list.
    /// </summary>
    /// <param name="arg">The string representation of the boolean ("0" for false, "1" for true).</param>
    private void AddBoolToMessage(string arg)
    {
        if (arg == "1")
        {
            _messageParts.Add(("bool", true));
            Console.WriteLine("Queued bool: true");
        }
        else if (arg == "0")
        {
            _messageParts.Add(("bool", false));
            Console.WriteLine("Queued bool: false");
        }
        else
        {
            Console.WriteLine("Invalid boolean value. Use '0' for false or '1' for true. Usage: bool <0|1>");
        }
    }

    /// <summary>
    /// Sends the currently composed message to the server.
    /// The message is constructed from the queued parts.
    /// Allows overriding the message ID for the *current* send operation.
    /// After sending, the message composition is cleared.
    /// </summary>
    /// <param name="arg">Optional string representing a `ushort` message ID to use for this send.
    /// If not provided or invalid, the default message ID is used.</param>
    private void SendMessage(string arg)
    {
        if (!_client.IsConnected)
        {
            Console.WriteLine("Not connected to a server. Cannot send message.");
            return;
        }

        if (_messageParts.Count == 0)
        {
            Console.WriteLine("No data added to the message. Add data (int, string, bool) first.");
            return;
        }

        // Determine the message ID to use for this specific send operation.
        ushort messageIdToSend = _defaultMessageId;
        if (!string.IsNullOrEmpty(arg) && ushort.TryParse(arg, out ushort id))
        {
            messageIdToSend = id; // Override with user-provided ID
        }
        else if (!string.IsNullOrEmpty(arg))
        {
            // User provided an argument, but it wasn't a valid ushort ID.
            Console.WriteLine($"Invalid message ID '{arg}'. Using default ID: {_defaultMessageId}");
        }

        // Create a new Riptide Message object for this send.
        // Message.Create requires the MessageSendMode (e.g., Reliable) and the message ID upfront.
        Message message = Message.Create(MessageSendMode.Reliable, messageIdToSend);

        // Iterate through all the queued message parts and add them to the Riptide Message.
        foreach (var part in _messageParts)
        {
            switch (part.type)
            {
                case "int":
                    message.AddInt((int)part.value);
                    break;
                case "string":
                    message.AddString((string)part.value);
                    break;
                case "bool":
                    message.AddBool((bool)part.value);
                    break;
            }
        }

        Console.WriteLine($"Sending message ID {messageIdToSend} with {message.WrittenLength - 2} bytes of payload (total {message.WrittenLength} bytes)..."); // -2 for the ushort ID
        _client.Send(message); // Send the constructed message to the server.

        // Clear the message parts list to prepare for composing a new message.
        ClearCurrentMessage(false); 
        Console.WriteLine("Message sent. Ready to compose a new message.");
    }

    /// <summary>
    /// Clears all currently queued message parts, effectively starting a new message composition.
    /// </summary>
    /// <param name="displayMessage">If true, a confirmation message is displayed to the console.</param>
    private void ClearCurrentMessage(bool displayMessage = true)
    {
        _messageParts.Clear(); // Clear the list of data parts.
        if (displayMessage)
        {
            Console.WriteLine($"Current message composition cleared. Ready for a new message with ID: {_defaultMessageId}");
        }
    }

    /// <summary>
    /// Sets the default message ID that will be used for subsequent message compositions.
    /// This does not affect messages currently being composed unless `clear` or `send` is used afterward.
    /// </summary>
    /// <param name="arg">The string representation of the new `ushort` default message ID.</param>
    private void SetDefaultMessageId(string arg)
    {
        if (ushort.TryParse(arg, out ushort newId))
        {
            _defaultMessageId = newId;
            Console.WriteLine($"Default message ID for future compositions set to: {_defaultMessageId}");
            Console.WriteLine($"Default message ID described as: {MessageMarkSupply.DescriptionOf(_defaultMessageId)}");
        }
        else
        {
            Console.WriteLine("Invalid message ID. Usage: setid <ushort_id>");
        }
    }

    /// <summary>
    /// Displays a list of all available commands and their usage to the console.
    /// </summary>
    private void DisplayHelp()
    {
        Console.WriteLine("\n--- Riptide Console Client Commands ---");
        Console.WriteLine("  connect <ip> <port>   - Connects to the Riptide server (e.g., connect 127.0.0.1 8766)");
        Console.WriteLine("  disconnect            - Disconnects from the server");
        Console.WriteLine("  setid <id>            - Sets the default message ID (ushort) for messages you compose. (Current: " + _defaultMessageId + ")");
        Console.WriteLine("  int <value>           - Adds an integer to the current message composition (e.g., int 123)");
        Console.WriteLine("  string <value>        - Adds a string to the current message composition (e.g., string hello world)");
        Console.WriteLine("  bool <0|1>            - Adds a boolean to the current message composition (0 for false, 1 for true)");
        Console.WriteLine("  send [id]             - Sends the currently composed message. Optional [id] overrides default for this send.");
        Console.WriteLine("  clear                 - Clears the current message composition");
        Console.WriteLine("  exit                  - Shuts down the client");
        Console.WriteLine("  help                  - Displays this help message");
        Console.WriteLine("-------------------------------------");
        Console.WriteLine($"Current default message ID for new compositions: {_defaultMessageId}");
        Console.WriteLine($"Number of parts in current message composition: {_messageParts.Count}");
    }
}
