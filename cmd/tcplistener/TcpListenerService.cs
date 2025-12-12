using System.Text;
using System.Threading.Channels;
using System.Net;
using System.Net.Sockets;

namespace cmd.tcplistener;

public class TcpListenerService {
    public static IAsyncEnumerable<string> GetLinesFromChannel(Stream stream) {
        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100) {
            SingleWriter = true,
            SingleReader = true
        });

        Task.Run(async () => {
            try {
                byte[] buffer = new byte[8];
                List<byte> lineBytes = new List<byte>();

                while (true) {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) break;
                    for (int i = 0; i < read; i++) {
                        if (buffer[i] == 10) {
                            string line = Encoding.UTF8.GetString(lineBytes.ToArray());
                            lineBytes.Clear();
                            await channel.Writer.WriteAsync(line);
                        } else {
                            lineBytes.Add(buffer[i]);
                        }
                    }
                }
            } catch (Exception ex) {
                channel.Writer.Complete(ex);
            } finally {
                channel.Writer.Complete();
            }
        });

        return channel.Reader.ReadAllAsync();
    }

    public static async Task StartListener() {
        var listener = new TcpListener(IPAddress.Any, 69420);
        listener.Start(); 

        Console.WriteLine("Listening on port 8080...");

        while (true) {
            using TcpClient client = await listener.AcceptTcpClientAsync();
            GetLinesFromChannel(client.GetStream());
        }
    }
}