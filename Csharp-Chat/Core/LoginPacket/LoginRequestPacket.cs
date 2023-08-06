using System.Net;
using System.Text;

namespace Core.LoginPacket;

public class LoginRequestPacket : IPacket
{
    public string Id { get; private set; }
    public string Nickname { get; private set; }

    public LoginRequestPacket(string id, string nickname)
    {
        Id = id;
        Nickname = nickname;
    }

    public LoginRequestPacket(byte[] buffer)
    {
        //패킷타입을 가져올떄 0번째 인덱스부터 가져왔으니 2부터 시작함
        int offset = 2;

        short idSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, offset));
        offset += sizeof(short);

        //아이디 가져오기
        Id = Encoding.UTF8.GetString(buffer, offset, idSize);
        offset += idSize;

        //닉네임 가져오기
        short nicknameSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, offset));
        offset += sizeof(short);

        Nickname = Encoding.UTF8.GetString(buffer, offset, nicknameSize);
    }

    public byte[] Serialize()
    {
        //패킷타입 변환
        byte[] packetType = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)PacketType.LoginRequest));
        byte[] id = Encoding.UTF8.GetBytes(Id);
        byte[] idSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)id.Length));
        byte[] nickname = Encoding.UTF8.GetBytes(Nickname);
        byte[] nicknameSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)nickname.Length));

        //전체 데이터 길이 더하기 
        short dataSize = (short)(packetType.Length + id.Length + idSize.Length + nickname.Length + nicknameSize.Length);

        //헤더버퍼 생성 및 빅엔디언으로 변환
        byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataSize));
        byte[] buffer = new byte[2 + dataSize];

        int offset = 0;

        //버퍼에 복사
        Array.Copy(header, 0, buffer, offset, header.Length);
        offset += header.Length;

        Array.Copy(packetType, 0, buffer, offset, packetType.Length);
        offset += packetType.Length;

        Array.Copy(idSize, 0, buffer, offset, idSize.Length);
        offset += idSize.Length;

        Array.Copy(id, 0, buffer, offset, id.Length);
        offset += id.Length;

        Array.Copy(nicknameSize, 0, buffer, offset, nicknameSize.Length);
        offset += nicknameSize.Length;

        Array.Copy(nickname, 0, buffer, offset, nickname.Length);

        return buffer;
    }
}
