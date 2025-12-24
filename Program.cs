using cmd.httpserver;

class Program
{
    static Int32 port = 6969;

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    async static Task Main()
    {
        try
        {
            var routes = new Dictionary<string, string>{
                {"/dictator", "/Users/petar/Movies/The Onion/Newsroom/First Female Dictator Hailed As Step Forward For Women.mp4"},
                { "/butt", "/Users/petar/Downloads/AQOBGN6hPgKKlUFUxjzAj0qat28X1AgjVLrY6E8xzonEjqfWTafbwUiwuEHPjvRMZUcJSUWtJkIzCv8GApQaGumPPgoWSsfHOKk0VZE.mp4"},
                { "/car", "/Users/petar/Projects/CSS/move-things-with-css-main/src/css-animations/racecar/3.stop-the-race/index.html"}
        };

            var server = new Server(routes);
            await server.Serve(port);
            Console.WriteLine($"Server started at port {port}");

            // This acts like the channel block '<-sigChan':w
            using var exitEvent = new ManualResetEventSlim(false);

            // Handle Ctrl+C (SIGINT)
            Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true; // Prevents immediate termination
                        exitEvent.Set();
                    };

            // Wait here indefinitely until the event is triggered
            exitEvent.Wait();

            Console.WriteLine("Server gracefully stopped");
        }
        catch (Exception ex)
        {
            LogError($"Error starting server: {ex.Message}");
        }
    }
}
