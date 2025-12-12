using cmd.tcplistener;

class Program {
    static async Task Main() {
        await TcpListenerService.StartListener();
    }
}
