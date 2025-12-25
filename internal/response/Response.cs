using System.Text;

namespace response;

public class Response
{
    private static Dictionary<string, string> statusLines = new()
    {
        ["200"] = "HTTP/1.1 200 OK",
        ["206"] = "HTTP/1.1 206 Partial Content",
        ["404"] = "HTTP/1.1 404 Not Found",
        ["405"] = "HTTP/1.1 405 Method Not Allowed"
    };
    private static byte[] CRLF = "\r\n"u8.ToArray();

    private string _status;
    private Dictionary<string, List<string>> _headers = new();
    private byte[]? _body;

    public Response(string status)
    {
        _status = status;
    }

    public void SetHeader(string key, string value)
    {
        key = key.ToLower();
        if (!_headers.ContainsKey(key))
        {
            _headers[key] = new List<string>();
        }
        _headers[key].Add(value);
    }

    public void SetBody(byte[] body)
    {
        _body = body;
        SetHeader("Content-Length", body.Length.ToString());
    }

    public byte[] GetBytes()
    {
        var responseBytes = new List<byte>();
        responseBytes.AddRange(Encoding.UTF8.GetBytes(statusLines[_status]));
        responseBytes.AddRange(CRLF);

        foreach (var header in _headers)
        {
            foreach (var value in header.Value)
            {
                responseBytes.AddRange(Encoding.UTF8.GetBytes($"{header.Key}: {value}"));
                responseBytes.AddRange(CRLF);
            }
        }
        responseBytes.AddRange(CRLF);

        if (_body != null)
        {
            responseBytes.AddRange(_body);
        }

        return responseBytes.ToArray();
    }

    public async Task WriteHeadersAsync(Stream networkStream)
    {
        await networkStream.WriteAsync(Encoding.UTF8.GetBytes(statusLines[_status]));
        await networkStream.WriteAsync(CRLF);

        foreach (var header in _headers)
        {
            foreach (var value in header.Value)
            {
                await networkStream.WriteAsync(Encoding.UTF8.GetBytes($"{header.Key}: {value}"));
                await networkStream.WriteAsync(CRLF);
            }
        }
        await networkStream.WriteAsync(CRLF);
    }

    public async Task WriteWithFileStreamAsync(Stream networkStream, string filePath, long start = 0, long? length = null)
    {
        await WriteHeadersAsync(networkStream);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            useAsync: true
        );

        if (start > 0)
        {
            fileStream.Seek(start, SeekOrigin.Begin);
        }

        if (length.HasValue)
        {
            await CopyBytesAsync(fileStream, networkStream, length.Value);
        }
        else
        {
            await fileStream.CopyToAsync(networkStream);
        }

        await networkStream.FlushAsync();
    }

    private static async Task CopyBytesAsync(Stream source, Stream destination, long bytesToCopy)
    {
        byte[] buffer = new byte[64 * 1024];
        long remaining = bytesToCopy;

        while (remaining > 0)
        {
            int toRead = (int)Math.Min(buffer.Length, remaining);
            int read = await source.ReadAsync(buffer.AsMemory(0, toRead));

            if (read == 0) break;

            await destination.WriteAsync(buffer.AsMemory(0, read));
            remaining -= read;
        }
    }
}
