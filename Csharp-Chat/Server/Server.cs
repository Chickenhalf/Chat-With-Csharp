using Core;
using Core.LoginPacket;
using Core.ManagePakcet;
using Core.RoomPacket;
using Core.UserPacket;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Server;

internal class Server
{
    private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public ConcurrentDictionary<string, Room> Rooms { get; } = new ConcurrentDictionary<string, Room>();
    public ConcurrentDictionary<string, Socket> Clients { get; } = new ConcurrentDictionary<string, Socket>();

    public Server(string ip, int port, int backlog)
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        serverSocket.Bind(endPoint);
        serverSocket.Listen(backlog);
    }

    public async Task StartAsync()
    {
        while (true)
        {
            try
            {
                Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine(clientSocket.RemoteEndPoint);
                ThreadPool.QueueUserWorkItem(RunAsync, clientSocket);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async void RunAsync(object? sender)
    {
        Socket clientSocket = (Socket)sender!;
        byte[] headerBuffer = new byte[2];

        string id = "";
        string nickname = "";
        string roomName = "";

        try
        {
            while (true)
            {
                #region 헤더버퍼 가져오기
                var t1 = clientSocket.ReceiveAsync(headerBuffer, SocketFlags.None);
                var t2 = Task.Delay(1000 * 30);
                var result = await Task.WhenAny(t1, t2); //먼저끝난 task 반환

                if (result == t2) //30초간 응답이없으면 종료
                {
                    Console.WriteLine("client disconnect");
                    await Remove(id, nickname, roomName, clientSocket);
                    return;
                }

                int WaitSec = await t1;
                if (WaitSec < 1)
                {
                    Console.WriteLine("client disconnect");
                    await Remove(id, nickname, roomName, clientSocket);
                    return;
                }
                else if (WaitSec == 1)
                {
                    await clientSocket.ReceiveAsync(new ArraySegment<byte>(headerBuffer, 1, 1), SocketFlags.None);
                }
                #endregion

                #region 데이터버퍼 가져오기
                short dataSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(headerBuffer));
                byte[] dataBuffer = new byte[dataSize];

                int totalRecv = 0;
                while (totalRecv < dataSize)
                {
                    int n2 = await clientSocket.ReceiveAsync(new ArraySegment<byte>(dataBuffer, totalRecv, dataSize - totalRecv), SocketFlags.None);
                    totalRecv += n2;
                }
                #endregion

                //패킷타입 가져오기
                PacketType packetType = (PacketType)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(dataBuffer));

                //패킷타입 분류
                if (packetType == PacketType.LoginRequest)
                {
                    LoginRequestPacket LoginPacket = new LoginRequestPacket(dataBuffer);
                    Clients.AddOrUpdate(LoginPacket.Id, clientSocket, (k, v) =>
                    {
                        DuplicatePacket packet = new DuplicatePacket();
                        v.Send(packet.Serialize());
                        return clientSocket;
                    });
                    Console.WriteLine($"id:{LoginPacket.Id} nickname:{LoginPacket.Nickname} 생성");

                    id = LoginPacket.Id;
                    nickname = LoginPacket.Nickname;
                    
                    LoginResponsePacket _LoginResponsePacket = new LoginResponsePacket(1);
                    await clientSocket.SendAsync(_LoginResponsePacket.Serialize(), SocketFlags.None);
                }

                else if (packetType == PacketType.CreateRoomRequest)
                {
                    CreateRoomRequestPacket CrrPacket = new CreateRoomRequestPacket(dataBuffer);
                    Room room = new Room();

                    if (Rooms.TryAdd(CrrPacket.RoomName, room))
                    {
                        roomName = CrrPacket.RoomName;
                        room.Users.TryAdd(id, nickname);
                        Console.WriteLine("created room : " + roomName);
                        CreateRoomResponsePacket CrrPacket2 = new CreateRoomResponsePacket(1);
                        await clientSocket.SendAsync(CrrPacket2.Serialize(), SocketFlags.None);
                    }
                    else
                    {
                        Console.WriteLine("created failed");
                        CreateRoomResponsePacket CrrPacket2 = new CreateRoomResponsePacket(500);
                        await clientSocket.SendAsync(CrrPacket2.Serialize(), SocketFlags.None);
                    }
                }

                else if (packetType == PacketType.RoomListRequest)
                {
                    RoomListResponsePacket packet = new RoomListResponsePacket(Rooms.Keys);
                    await clientSocket.SendAsync(packet.Serialize(), SocketFlags.None);
                }

                else if (packetType == PacketType.EnterRoomRequset)
                {
                    EnterRoomRequestPacket ErrPacket = new EnterRoomRequestPacket(dataBuffer);
                    if (Rooms.TryGetValue(ErrPacket.RoomName, out var room))
                    {
                        roomName = ErrPacket.RoomName;
                        room.Users.TryAdd(id, nickname);
                        Console.WriteLine($"{ErrPacket.RoomName} : {nickname}  님 입장!");
                        EnterRoomResponsePacket ErrPacket2 = new EnterRoomResponsePacket(1);
                        await clientSocket.SendAsync(ErrPacket2.Serialize(), SocketFlags.None);

                        await Task.Delay(100);

                        foreach (var user in room.Users)
                        {
                            // 자기자신이면 추가하지않음
                            if (user.Value == nickname)
                                continue;

                            // 상대방한테 나를 추가
                            if (Clients.TryGetValue(user.Key, out var otherClient))
                            {
                                UserEnterPacket AddMePacket = new UserEnterPacket(nickname);
                                await otherClient.SendAsync(AddMePacket.Serialize(), SocketFlags.None);
                            }

                            // 나한테 상대방 추가
                            UserEnterPacket AddUsersPacket = new UserEnterPacket(user.Value);
                            await clientSocket.SendAsync(AddUsersPacket.Serialize(), SocketFlags.None);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{ErrPacket.RoomName} : {nickname} 입장실패");
                        EnterRoomResponsePacket FailedPacket = new EnterRoomResponsePacket(500);
                        await clientSocket.SendAsync(FailedPacket.Serialize(), SocketFlags.None);
                    }
                }

                else if (packetType == PacketType.UserLeave)
                {
                    UserLeavePacket UserLeavePacket = new UserLeavePacket(dataBuffer);
                    if (Rooms.TryGetValue(roomName, out var room))
                    {
                        room.Users.TryRemove(id, out _);

                        if (room.Users.IsEmpty)
                        {
                            Rooms.TryRemove(roomName, out _);
                        }

                        roomName = "";

                        foreach (var user in room.Users)
                        {
                            if (Clients.TryGetValue(user.Key, out var otherClient))
                            {
                                await otherClient.SendAsync(UserLeavePacket.Serialize(), SocketFlags.None);
                            }
                        }
                    }
                }

                else if (packetType == PacketType.Chat)
                {
                    ChatPacket packet = new ChatPacket(dataBuffer);
                    if (Rooms.TryGetValue(roomName, out var room))
                    {
                        foreach (var user in room.Users)
                        {
                            if (Clients.TryGetValue(user.Key, out var client))
                            {
                                await client.SendAsync(packet.Serialize(), SocketFlags.None);
                            }
                        }
                    }
                }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
            await Remove(id, nickname, roomName, clientSocket);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Remove(id, nickname, roomName, clientSocket);
        }
    }

    private async Task Remove(string id, string nickname, string roomName, Socket clientSocket)
    {
        if (Clients.TryGetValue(id, out var client) && client == clientSocket)
        {
            Clients.TryRemove(id, out _);
        }
        if (Rooms.TryGetValue(roomName, out var room))
        {
            UserLeavePacket packet = new UserLeavePacket(nickname);
            room.Users.TryRemove(id, out _);

            if (room.Users.IsEmpty)
            {
                Rooms.TryRemove(roomName, out _);
            }

            foreach (var user in room.Users)
            {
                if (Clients.TryGetValue(user.Key, out var otherClient))
                {
                    await otherClient.SendAsync(packet.Serialize(), SocketFlags.None);
                }
            }
        }
        clientSocket.Dispose();
    }
}
