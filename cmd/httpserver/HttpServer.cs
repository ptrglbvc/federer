using System.Net.Sockets;
using cmd.tcplistener;
using request;
using response;

namespace cmd.httpserver;

public class Server
{
    public Dictionary<string, string> routes;

    public Server(Dictionary<string, string> Routes)
    {
        routes = Routes;
    }

    public string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

        var mappings = new Dictionary<string, string> {
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".svg", "image/svg+xml" },
            { ".pdf", "application/pdf" },
            { ".txt", "text/plain" },
            { ".zip", "application/zip" },
            { ".mp3", "audio/mpeg" },
            { ".mp4", "video/mp4" },
            { ".wav", "audio/wav" },
            { ".epub", "application/epub+zip" }
        };

        if (extension != null && mappings.TryGetValue(extension, out var contentType))
        {
            return contentType;
        }

        return "application/octet-stream";
    }

    private async Task HandleRequest(Request req, NetworkStream stream)
    {
        await HandleAsync(req, stream);
    }

    private async Task HandleAsync(Request req, NetworkStream stream)
    {
        if (req.requestLine.Method != "GET")
        {
            var res = new Response("405");
            await stream.WriteAsync(res.GetBytes());
            await stream.FlushAsync();
        }
        else
        {
            await GETHandlerAsync(req, stream);
        }
    }

    private async Task GETHandlerAsync(Request req, NetworkStream stream)
    {
        foreach (var route in routes)
        {
            if (req.requestLine.Target == route.Key)
            {
                string filePath = route.Value;

                if (!File.Exists(filePath))
                {
                    await Send404Async(stream);
                    return;
                }

                var fileInfo = new FileInfo(filePath);
                long totalSize = fileInfo.Length;
                string mimeType = GetMimeType(filePath);

                Response res;
                long start = 0;
                long length = totalSize;

                if (req.GetHeader("range") is string range)
                {
                    try
                    {
                        range = range.Replace("bytes=", "").Trim();
                        var rangeArr = range.Split("-");
                        start = long.Parse(rangeArr[0]);
                        long end = string.IsNullOrEmpty(rangeArr[1])
                            ? totalSize - 1
                            : long.Parse(rangeArr[1]);

                        length = end - start + 1;

                        res = new Response("206");
                        res.SetHeader("Content-Type", mimeType);
                        res.SetHeader("Accept-Ranges", "bytes");
                        res.SetHeader("Content-Range", $"bytes {start}-{end}/{totalSize}");
                        res.SetHeader("Content-Length", length.ToString());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error parsing the range header value");
                        res = new Response("200");
                        res.SetHeader("Content-Type", mimeType);
                        res.SetHeader("Accept-Ranges", "bytes");
                        res.SetHeader("Content-Length", totalSize.ToString());
                        start = 0;
                        length = totalSize;
                    }
                }
                else
                {
                    res = new Response("200");
                    res.SetHeader("Content-Type", mimeType);
                    res.SetHeader("Accept-Ranges", "bytes");
                    res.SetHeader("Content-Length", totalSize.ToString());
                }

                await res.WriteWithFileStreamAsync(stream, filePath, start, length);
                return;
            }
        }

        await Send404Async(stream);
    }

    private async Task Send404Async(NetworkStream stream)
    {
        var res = new Response("404");
        res.SetHeader("Content-Type", "text/plain");
        res.SetBody("404 - Not Found"u8.ToArray());
        await stream.WriteAsync(res.GetBytes());
        await stream.FlushAsync();
    }

    public async Task Serve(Int32 port)
    {
        await TcpListenerService.StartListener(port, HandleRequest);
    }

    public void Stop()
    {
    }
}
