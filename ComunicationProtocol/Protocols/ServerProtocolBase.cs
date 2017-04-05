using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol.Protocols
{
	/// <summary>
	/// Базовый объект серверного протокола передачи данных
	/// </summary>
	public abstract class ServerProtocolBase<TCurrentObject>: ComunicationProtocol<TCurrentObject> where TCurrentObject : class
	{
		/// <summary>
		/// Максимальное число одновременных запросов на подключение подключений к данному серверу
		/// </summary>
		private const int MaxListeningForConnection = 256;

		private Thread _listeningThread;

		/// <summary>
		/// Оповещает об отправке сообщения
		/// </summary>
		protected event ListenForwardEventHandler MessageSended = delegate { return ListenerResult.None; };

		/// <summary>
		/// Оповещает об приходе сообщения
		/// </summary>
		protected event ListenForwardEventHandler MessageReceived;

		/// <summary>
		/// Оповещает когда пользователь произвёл подключение к серверу
		/// </summary>
		protected event ListenEventHandler ClientConnected = delegate { }; 
		/// <summary>
		/// Оповещает когда пользователь отключился от сервера
		/// </summary>
		protected event ListenEventHandler ClientDisconnected = delegate { };

		/// <summary>
		/// весь список тем, от кого данное подключение могло получать уведомления
		/// </summary>
		protected List<ICoherentReceiverUnit> Clients { get; } = new List<ICoherentReceiverUnit>();

		/// <summary/>
		protected ServerProtocolBase(): base()
		{
			AsyncMessageReceived += MessageReceivedEventHandler;
			AsyncErrorMessageReceive += MessageReceiverErrorEventHandler;
		}

		#region Private Socket Logic
		private void MessageReceiverErrorEventHandler(ICoherentUnit client, IMessage message)
		{
			if (!Clients.Contains(client))
				return;
			
			DisconnectUnit(client);
		}

		private void MessageReceivedEventHandler(ICoherentUnit client, IMessage message)
		{
			ICoherentReceiverUnit user = client as ICoherentReceiverUnit;

			ListenerResult r;

			if (MessageReceived != null)
			{
				foreach (var item in MessageReceived.GetInvocationList())
				{
					ListenForwardEventHandler handler = item as ListenForwardEventHandler;

					if (handler == null)
						continue;

					r = handler(client, message);

					if (r == ListenerResult.Override)
						break;
				}
			}

			if (user == null)
				return;
			
			// Зарезервированые сообщения, их обработка.
			switch (message.ActionName)
			{
				case nameof(Disconect):
				{
					ReturnMessage(new MessageResult(message) { Result = true }, client);
					DisconnectUnit(client);
					ManualResetEvent.WaitOne();
					break;
				}
				case nameof(Connect):
				{
					
					ReturnMessage(new MessageResult(message) { Result = true }, client);
					ClientConnected(client, message);
					break;
				}
				default:
				{
					break;
				}
			}
		}

		/// <summary>
		///  Проверяет зарезервированно ли данное сообщение как `Протокольное`
		/// </summary>
		/// <param name="message">Проверяемое сообщение</param>
		/// <returns>true - протокольное сообщение || false - Не протокольное сообщение</returns>
		protected bool HasMessageRegistered(IMessage message)
		{
			//todo: Добавить проверку Guid иначе могут быть другие команды, которые могут так-же именоваться. Да и проверка по строке очень плохо, но как hotfix..
			bool result;
			switch (message.ActionName)
			{
				case nameof(Disconect):
				case nameof(Connect):
				{
					result = true;

					break;
				}
				default:
				{
					result = false;
					break;
				}
			}

			return result;
		}

		private void ListeningBegin()
		{
			while (true)
			{
				ManualResetEvent.Reset();
				Socket.BeginAccept(null, 0, ClientAcepted, Socket);
				ManualResetEvent.WaitOne();
			}
		}

		private void ClientAcepted(IAsyncResult result)
		{
			ManualResetEvent.Set();
			Socket currentSocket = (Socket)result.AsyncState;
			
			if (Disposed)
				return;
			
			Socket handler = currentSocket.EndAccept(result);
			
			// one ip = one app.
			if (Clients.Any(item => item.Socket.RemoteEndPoint == handler.RemoteEndPoint)) {
				handler.Disconnect(false);
				handler.Shutdown(SocketShutdown.Both);
				handler.Dispose();
				return;
			}
			
			Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.Tcp) {
					SendBufferSize = currentSocket.SendBufferSize,
					ReceiveBufferSize = currentSocket.ReceiveBufferSize
				};
			
			ICoherentReceiverUnit state = CreateReceiverUnit(handler, receiver, BytesLenght);
			Clients.Add(state);
			
			StartReceiveMessages(state);
			
			receiver.BeginConnect(((IPEndPoint)handler.RemoteEndPoint).Address, ReceiverPort,
				CallbackConnection, state);
			
			ManualResetEvent.WaitOne();
		}

		private void CallbackConnection(IAsyncResult result)
		{
			ICoherentReceiverUnit state = (ICoherentReceiverUnit)result.AsyncState;

			// todo: with return message
		}
		#endregion

		#region Protected logic adventure methods
		/// <summary>
		/// Создать объект, который может хранить информацию об пользователе, который умеет общаться в обе стороны
		/// </summary>
		/// <param name="socket">поставщик - для основного общения</param>
		/// <param name="receiver">поставщик - для обратного общения (callback)</param>
		/// <param name="lenghtBytes">размер заголовка - для связного, обратного общения</param>
		/// <returns>Созданный объект</returns>
		protected abstract ICoherentReceiverUnit CreateReceiverUnit(Socket socket, Socket receiver, int lenghtBytes);

		/// <summary>
		/// Принудительно отключить пользователя от сервера
		/// </summary>
		/// <param name="client">пользователя, которого нужно отключить</param>
		protected void DisconnectUnit(ICoherentUnit client)
		{
			ClientDisconnected(client, default(IMessage));

			if (client is ICoherentReceiverUnit ul)
			{
				Clients.Remove(ul);
			}

			client.Dispose();
		}

		/// <summary>
		/// Вернуть результат клиенту от указаной операции
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <param name="client"></param>
		/// <returns></returns>
		protected bool ReturnMessage(IMessageResult message, ICoherentUnit client)
		{
			MessageSended(client, message);
			return base.SendMessage(message, client);
		}

		/// <summary>
		/// Отправить Сообщение клиенту
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <param name="client"></param>
		/// <returns></returns>
		protected bool SendMessage(Message message, ICoherentReceiverUnit client)
		{
			MessageSended(client, message);

			return base.SendMessage(message, client.Receiver);
		}

		/// <summary>
		/// Отправить всем клиентам сообщение, которые могут получать неожиданые сообщения от клиента
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ignoring"></param>
		protected void SendMessageAll(Message msg, params ICoherentUnit[] ignoring)
		{
			foreach (var usr in Clients)
			{
				if (ignoring.Contains(usr))
					continue;

				SendMessage(msg, usr);
			}
		}
		#endregion

		/// <summary>
		/// Начать прослушивать и получать сообщения от клиентов
		/// </summary>
		/// <returns></returns>
		public override bool Connect()
		{
			if (Disposed)
				return false;

			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(Host), Port);

			Socket.Bind(localEndPoint);
			Socket.Listen(MaxListeningForConnection);

			_listeningThread = new Thread(ListeningBegin) {
				IsBackground = true,
				Name = $"{nameof(ServerProtocolBase<TCurrentObject>)}-Listener",
				Priority = ThreadPriority.Normal
			};

			_listeningThread.Start();

			return true;
		}

		/// <summary>
		/// Закончить прослушивания и обрубить сокеты
		/// </summary>
		public override bool Disconect()
		{
			Dispose();
			return true;
		}

		/// <summary>
		/// Освободить рессурсы
		/// </summary>
		public override void Dispose()
		{
			if (Disposed)
				return;

			Disposed = true;

			_listeningThread.Abort();
			_listeningThread = null;

			base.Dispose();
		}
	}
}