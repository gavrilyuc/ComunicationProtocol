using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol.Protocols
{
	/// <summary>
	/// Базовый объект клиентского протокола передачи данных на сервер.
	/// </summary>
	public abstract class ClientProtocolBase<TCurrentObject>: ComunicationProtocol<TCurrentObject>, ICoherentUnit where TCurrentObject : class
	{
		private readonly DefaultCoherentUnit _server = new DefaultCoherentUnit(BytesLenght);

		/// <summary>
		/// Не ожиданые оповещения сервера
		/// </summary>
		protected event ListenEventHandler MessageServerReceived = delegate { };

		#region ICoherentUnit
		
		/// <summary />
		public new bool Disposed => base.Disposed;

		/// <summary>
		/// Используемый сокет
		/// </summary>
		public new Socket Socket => base.Socket;

		/// <summary>
		/// Буффер для получение заголовка пакета
		/// </summary>
		public byte[] LenghtBuffer { get; } = new byte[4];

		/// <summary>
		/// Размер всего пакета
		/// </summary>
		public int Lenght { get; set; }

		/// <summary>
		/// текущая позиция чтения сообщения
		/// </summary>
		public int CurrentPosition { get; set; }

		/// <summary>
		/// Контент пакета
		/// </summary>
		public byte[] ContentBuffer { get; set; }

		/// <summary>
		/// Пользовательский объект
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
		/// Отправить Сообщение серверу
		/// </summary>
		/// <param name="message">Отправляемое сообщение</param>
		/// <param name="sync">Отправлять ли данный запрос синхронно. По умолчанию: Синхронно</param>
		/// <returns>Результат запроса. Внимание, если это ассинхронный вызов, то ожидайте результат в событии MessageReceived</returns>
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
		/// Создать подключение
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

			//todo: знать, ли запущена программа или же проверить занят ли ip:port прослушкой другого сокета.
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
		/// Закрыть поделючение
		/// </summary>
		/// <returns></returns>
		public override bool Disconect()
		{
			IMessageResult resultMessage = SendMessageSync(DisconectMesage(), this);

			return (bool)resultMessage.Result;
		}

		/// <summary>
		/// Освободить рессурсы
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
		/// Очистить параметры
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