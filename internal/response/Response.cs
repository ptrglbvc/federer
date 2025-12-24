using System.Text;

namespace response;

public class Response
{
    private static Dictionary<string, string> methodLineTemplates = new()
    {
        ["200"] = "HTTP/1.1 200 OK",
        ["206"] = "HTTP/1.1 206 Partial Content",
        ["404"] = "HTTP/1.1 404 Not Found",
        ["405"] = "HTTP/1.1 405 Method Not Allowed"
    };
    private static byte[] RN = "\r\n"u8.ToArray();

    private string _method;
    private Dictionary<string, List<string>> _headers = new();
    private byte[]? _body;

    public Response(string method)
    {
        _method = method;
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
        SetHeader("Content-Length", _body.Length.ToString());
    }

    public byte[] GetBytes()
    {
        var responseBytes = new List<byte>();

        responseBytes.AddRange(Encoding.UTF8.GetBytes(methodLineTemplates[_method]));
        responseBytes.AddRange(RN);

        foreach (var header in _headers)
        {
            foreach (var value in header.Value)
            {
                responseBytes.AddRange(Encoding.UTF8.GetBytes($"{header.Key}: {value}"));
                responseBytes.AddRange(RN);
            }
        }
        responseBytes.AddRange(RN);

        if (_body != null)
        {
            responseBytes.AddRange(_body);
        }

        return responseBytes.ToArray();
    }
}
