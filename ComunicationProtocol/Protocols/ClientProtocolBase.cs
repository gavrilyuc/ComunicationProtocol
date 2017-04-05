using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol.Protocols
{
	/// <summary>
	/// ������� ������ ����������� ��������� �������� ������ �� ������.
	/// </summary>
	public abstract class ClientProtocolBase<TCurrentObject>: ComunicationProtocol<TCurrentObject>, ICoherentUnit where TCurrentObject : class
	{
		private readonly DefaultCoherentUnit _server = new DefaultCoherentUnit(BytesLenght);

		/// <summary>
		/// �� �������� ���������� �������
		/// </summary>
		protected event ListenEventHandler MessageServerReceived = delegate { };

		#region ICoherentUnit
		
		/// <summary />
		public new bool Disposed => base.Disposed;

		/// <summary>
		/// ������������ �����
		/// </summary>
		public new Socket Socket => base.Socket;

		/// <summary>
		/// ������ ��� ��������� ��������� ������
		/// </summary>
		public byte[] LenghtBuffer { get; } = new byte[4];

		/// <summary>
		/// ������ ����� ������
		/// </summary>
		public int Lenght { get; set; }

		/// <summary>
		/// ������� ������� ������ ���������
		/// </summary>
		public int CurrentPosition { get; set; }

		/// <summary>
		/// ������� ������
		/// </summary>
		public byte[] ContentBuffer { get; set; }

		/// <summary>
		/// ���������������� ������
		/// </summary>
		public object Tag { get; set; }
		#endregion

		/// <summary/>
		protected ClientProtocolBase(): base()
		{
			AsyncMessageReceived += MessageServerReceivedCallback;
		}

		private void MessageServerReceivedCallback(ICoherentUnit client, IMessage message)
		{
			if (client != _server)
				return;
			
			MessageServerReceived(client, message);
		}

		/// <summary>
		/// ��������� ��������� �������
		/// </summary>
		/// <param name="message">������������ ���������</param>
		/// <param name="sync">���������� �� ������ ������ ���������. �� ���������: ���������</param>
		/// <returns>��������� �������. ��������, ���� ��� ������������ �����, �� �������� ��������� � ������� MessageReceived</returns>
		protected IMessageResult SendMessage(IMessage message, bool sync = true)
		{
			if (sync)
			{
				return SendMessageSync(message, this);
			}

			SendMessage(message, this);

			return default(IMessageResult);
		}
		
		/// <summary>
		/// ������� �����������
		/// </summary>
		/// <returns></returns>
		public override bool Connect()
		{
			Socket.Connect(new IPEndPoint(IPAddress.Parse(Host), Port));

			if (!Socket.Connected)
			{
				return false;
			}

			
			Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
				SendBufferSize = Socket.SendBufferSize,
				ReceiveBufferSize = Socket.ReceiveBufferSize
			};

			//todo: �����, �� �������� ��������� ��� �� ��������� ����� �� ip:port ���������� ������� ������.
			try
			{
				//todo:  set the local ip
				IPAddress hostIp = Dns.GetHostEntry(Dns.GetHostName())
								.AddressList.Where(e => e.AddressFamily == AddressFamily.InterNetwork)
								.ToArray().First();
				string localHost = hostIp.ToString();
				receiver.Bind(new IPEndPoint(IPAddress.Parse(localHost), ReceiverPort));
				receiver.Listen(1);

				_server.Socket = receiver.Accept();
			}
			catch(SocketException ex)// ip address is already in use
			{
				var clr = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
				Console.ForegroundColor = clr;

				return false;
			}

			StartReceiveMessages(_server);

			IMessageResult resultMessage = SendMessageSync(ConnectMessage(), this);
			
			return (bool)resultMessage.Result;
		}

		/// <summary>
		/// ������� �����������
		/// </summary>
		/// <returns></returns>
		public override bool Disconect()
		{
			IMessageResult resultMessage = SendMessageSync(DisconectMesage(), this);

			return (bool)resultMessage.Result;
		}

		/// <summary>
		/// ���������� ��������
		/// </summary>
		public override void Dispose()
		{
			if (Disposed || !Disconect())
				return;
			
			_server.Socket.Disconnect(false);
			_server.Socket.Shutdown(SocketShutdown.Both);
			_server.Socket.Dispose();

			_server.ClearParameters();
			ClearParameters();

			base.Dispose();
		}

		/// <summary>
		/// �������� ���������
		/// </summary>
		public void ClearParameters()
		{
			for (int i = 0; i < LenghtBuffer.Length; i++)
				LenghtBuffer[i] = 0;
			ContentBuffer = null;
			CurrentPosition = 0;
			Lenght = 0;
		}
	}
}