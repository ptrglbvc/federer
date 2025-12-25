using cmd.httpserver;

class Program
{
    private static int _port = 6969;
    private static Dictionary<string, string> _routes = new();

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    public static void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    private static void PrintUsage()
    {
        Console.WriteLine(@"
Usage: httpfromtcp [options] [routes...]

Options:
  -p, --port <port>     Port to listen on (default: 6969)
  -h, --help            Show this help message

Routes:
  Specify routes as: /url-path=/file/path
  
Examples:
  httpfromtcp -p 8080 /video=/home/user/movie.mp4
  httpfromtcp /doc=/path/to/file.pdf /music=/path/to/song.mp3
  httpfromtcp --port 3000 /index=/var/www/index.html
");
    }

    private static bool ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == "-h" || arg == "--help")
            {
                PrintUsage();
                return false;
            }
            else if (arg == "-p" || arg == "--port")
            {
                if (i + 1 >= args.Length)
                {
                    LogError("Port number required after -p/--port");
                    return false;
                }

                if (!int.TryParse(args[++i], out _port) || _port < 1 || _port > 65535)
                {
                    LogError($"Invalid port number: {args[i]}");
                    return false;
                }
            }
            else if (arg.Contains('='))
            {
                var parts = arg.Split('=', 2);
                string urlPath = parts[0];
                string filePath = parts[1];

                if (!urlPath.StartsWith('/'))
                {
                    LogError($"URL path must start with '/': {urlPath}");
                    return false;
                }

                if (!File.Exists(filePath))
                {
                    LogError($"File not found: {filePath}");
                    return false;
                }

                _routes[urlPath] = filePath;
                LogInfo($"Route added: {urlPath} -> {filePath}");
            }
            else
            {
                LogError($"Unknown argument: {arg}");
                PrintUsage();
                return false;
            }
        }

        if (_routes.Count == 0)
        {
            LogError("No routes specified. Use -h for help.");
            return false;
        }

        return true;
    }

    async static Task Main(string[] args)
    {
        if (!ParseArgs(args))
        {
            Environment.Exit(1);
            return;
        }

        try
        {
            var server = new Server(_routes);
            await server.Serve(_port);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Server started at http://localhost:{_port}");
            Console.ResetColor();
            Console.WriteLine("Press Ctrl+C to stop");
            Console.WriteLine();

            Console.WriteLine("Available routes:");
            foreach (var route in _routes)
            {
                Console.WriteLine($"  http://localhost:{_port}{route.Key}");
            }
            Console.WriteLine();

            using var exitEvent = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            exitEvent.Wait();

            Console.WriteLine("Server gracefully stopped");
        }
        catch (Exception ex)
        {
            LogError($"Error starting server: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
