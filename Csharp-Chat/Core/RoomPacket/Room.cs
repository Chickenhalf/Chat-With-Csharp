using System.Collections.Concurrent;

namespace Core.RoomPacket;

public class Room
{
    // concurrentdic : 일반 dic과는 다르게 여러 쓰레드에서 접근가능함! 
    public ConcurrentDictionary<string, string> Users { get; } = new ConcurrentDictionary<string, string>();
}
