namespace Server;

internal class Program
{
    const string ip = "175.118.212.178";

    static async Task Main(string[] args)
    {
        Server server = new Server(ip, 20000, 10);
        await server.StartAsync();
    }
}