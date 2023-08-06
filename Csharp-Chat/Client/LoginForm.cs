using Core.LoginPacket;

namespace Client;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
        Singleton.Instance.LoginResponsed += LoginResponsed;

        //창을 닫을때 제거
        FormClosing += (s, e) =>
        {
            Singleton.Instance.LoginResponsed -= LoginResponsed;
        };
    }

    private async void btn_login_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(tbx_id.Text) || string.IsNullOrEmpty(tbx_nickname.Text))
        {
            MessageBox.Show("입력하세요");
            return;
        }

        await Singleton.Instance.ConnectAsync();

        //로그인 리퀘스트 패킷생성 
        LoginRequestPacket packet = new LoginRequestPacket(tbx_id.Text, tbx_nickname.Text);
        await Singleton.Instance.Socket.SendAsync(packet.Serialize(), System.Net.Sockets.SocketFlags.None);

    }

    private void LoginResponsed(object? sender, EventArgs e)
    {
        LoginResponsePacket packet = (LoginResponsePacket)sender!;
        if (packet.Code == 1)
        {
            Singleton.Instance.Id = tbx_id.Text;
            Singleton.Instance.Nickname = tbx_nickname.Text;

            IAsyncResult ar = null;
            ar = BeginInvoke(() => //ShowDialog 창을 끄기전까지 종료 X , 비동기식처리
            {
                RoomList roomList = new RoomList();
                roomList.ShowDialog();
                EndInvoke(ar);
            });
        }
        else //자원해제, shutdown을 송신제한으로 설정
        {
            Singleton.Instance.Socket.Shutdown(System.Net.Sockets.SocketShutdown.Send);
        }
    }
}