using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol.Protocols
{
	/// <summary>
	/// ������ �������� ��������� �������� ������
	/// </summary>
	/// <typeparam name="TCurrentObject">�������� ������, ������� � ����� ������������ ���� � ���� ��������</typeparam>
	public abstract class ComunicationProtocol<TCurrentObject>: Singleton<TCurrentObject>, IDisposable where TCurrentObject : class
	{
		private const int MaxSocketBufferSize = 32784;

		/// <summary>
		/// ������ ��������� ������
		/// </summary>
		public const int BytesLenght = sizeof(long);

		private static readonly BinaryFormatter Formatter = new BinaryFormatter();

		/// <summary>
		/// ������� ��� ���������� ������������� ���������� � ��������.
		/// �������� ������, ������� �������� ������� ����� ������.
		/// </summary>
		protected static ManualResetEvent ManualResetEvent { get; } = new ManualResetEvent(false);

		/// <summary>
		/// ���� ����������
		/// </summary>
		protected virtual int Port { get; } = 8948;

		/// <summary>
		/// ���� callback ����������
		/// </summary>
		protected virtual int ReceiverPort { get; } = 8949;

		/// <summary>
		/// ������
		/// </summary>
		protected virtual string Host { get; } = "127.0.0.1";
		
		/// <summary>
		/// ������� ���������
		/// </summary>
		public bool Disposed { get; protected set; }
		
		/// <summary>
		/// ��������� ����������
		/// </summary>
		protected Socket Socket { get; private set; }

		/// <summary>
		/// ��������� �� ������� ������ ���������
		/// </summary>
		protected event ListenEventHandler AsyncMessageReceived;

		/// <summary>
		/// ��������� �� �������� ������ ���������
		/// </summary>
		protected event ListenEventHandler AsyncMessageSended = delegate { };

		/// <summary>
		/// ���������� ����� ���������� ��������. �������� ������ ���������� ��� �������� ������� ����������
		/// </summary>
		protected event ListenEventHandler AsyncErrorMessageReceive = delegate { };

		/// <summary/>
		protected ComunicationProtocol()
		{
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
				SendBufferSize = MaxSocketBufferSize,
				ReceiveBufferSize = MaxSocketBufferSize,
				LingerState = new LingerOption(false, 0)
			};
		}

		#region Async protocol send/receive message
		private void AcceptSendMessage(IAsyncResult result)
		{
			SendingStateObject state = (SendingStateObject)result.AsyncState;

			if (Disposed || state.Client.Disposed)
				return;

			int bytesSend = state.Client.Socket.EndSend(result, out SocketError errorCode);

			if (bytesSend < 0 || errorCode != SocketError.Success)
			{
				AsyncErrorMessageReceive(state.Client, state.Message);
				return;
			}

			AsyncMessageSended(state.Client, state.Message);
		}

		private void LenghtCallbackReceived(IAsyncResult result)
		{
			ICoherentUnit state = (ICoherentUnit)result.AsyncState;

			if (Disposed || state.Disposed)
				return;


			Socket handler = state.Socket;

			int bytesRead = handler.EndReceive(result, out SocketError errorCode);

			if (bytesRead < 1 || errorCode != SocketError.Success)
			{
				AsyncErrorMessageReceive(state, default(IMessage));
				return;
			}

			state.Lenght = BitConverter.ToInt32(state.LenghtBuffer, 0);
			state.ContentBuffer = new byte[state.Lenght];
			state.CurrentPosition = 0;

			if (state.Lenght > 0)
			{
				handler.BeginReceive(state.ContentBuffer, 0, state.Lenght, SocketFlags.None,
					out errorCode, ReadCallbackContent, state);
			}
		}

		private void ReadCallbackContent(IAsyncResult ar)
		{
			ICoherentUnit state = (ICoherentUnit)ar.AsyncState;

			if (Disposed || state.Disposed)
				return;

			Socket handler = state.Socket;

			int bytesReaded = handler.EndReceive(ar, out SocketError errorCode);

			if (bytesReaded < 1 || errorCode != SocketError.Success)
			{
				AsyncErrorMessageReceive(state, default(IMessage));
				return;
			}

			state.CurrentPosition += bytesReaded;


			if (state.CurrentPosition >= state.Lenght)
			{
				IMessage result = (IMessage)ToObjectPacket(state.ContentBuffer);

				AsyncMessageReceived?.Invoke(state, result);

				// go new receive
				StartReceiveMessages(state);
			}
			else
			{
				handler.BeginReceive(state.ContentBuffer, state.CurrentPosition,
					state.Lenght - state.CurrentPosition, SocketFlags.None, out errorCode,
					ReadCallbackContent, state);
			}
		}
		#endregion

		#region Send Message
		/// <summary>
		/// ��������� ���������
		/// </summary>
		/// <param name="message">������������ ���������</param>
		/// <param name="client">������, �������� ������������ ���������</param>
		/// <returns></returns>
		protected bool SendMessage(IMessage message, ICoherentUnit client)
		{
			if (Disposed || client.Disposed)
			{
				throw new ObjectDisposedException("Current Object is Disposed");
			}

			byte[] b = ToPacketByte(message);

			SendingStateObject stateObj = new SendingStateObject {
				Client = client,
				Message = message
			};

			client.Socket.BeginSend(b, 0, b.Length, SocketFlags.None, out SocketError error, AcceptSendMessage, stateObj);

			return error == SocketError.Success;
		}

		/// <summary>
		/// ��������� ��������� ������ ���������
		/// </summary>
		/// <param name="message">������������ ���������</param>
		/// <param name="client">�����������</param>
		/// <returns></returns>
		protected IMessageResult SendMessageSync(IMessage message, ICoherentUnit client)
		{
			if (Disposed || client.Disposed)
			{
				throw new ObjectDisposedException("Current Object is Disposed");
			}

			byte[] b = ToPacketByte(message);


			client.Socket.Send(b, 0, b.Length, SocketFlags.None, out SocketError error);

			if (error != SocketError.Success)
				return null;
			
			AsyncMessageSended(client, message);
			
			byte[] buffer = new byte[0];
			long length = -1;

			IMessageResult resultMessage = default(IMessageResult);

			while (true)
			{
				if (client.Socket.Available == 0)
					continue;
				
				byte[] currentMessage = new byte[client.Socket.Available];

				int readedBytes = client.Socket.Receive(currentMessage, 0, currentMessage.Length, SocketFlags.None, out SocketError errorSocket);

				if (errorSocket != SocketError.Success)
					break;
				
				if (readedBytes < 1)
					continue;
				
				if (length == -1)
				{
					length = BitConverter.ToInt64(currentMessage.Take(BytesLenght).ToArray(), 0);
					
					buffer = new byte[length];
					length = 0;
					
					currentMessage = currentMessage.Skip(BytesLenght).ToArray();
				}
				
				for (int i = 0; i < readedBytes; i++)
				{
					if (length + i >= buffer.Length)
						break;
					
					buffer[length + i] = currentMessage[i];
				}
				
				length += currentMessage.Length;
				
				if (length < buffer.Length)
					continue;
				
				resultMessage = (IMessageResult)ToObjectPacket(buffer);
				AsyncMessageReceived?.Invoke(client, resultMessage);
				break;
			}

			return resultMessage;
		}
		#endregion

		/// <summary>
		/// ������� ��������� ��������� �� ������� ������
		/// </summary>
		/// <param name="client">�������������� ������</param>
		protected void StartReceiveMessages(ICoherentUnit client)
		{
			if (Disposed || client.Disposed)
				return;

			client.Socket.BeginReceive(client.LenghtBuffer, 0, client.LenghtBuffer.Length,
				SocketFlags.None, out SocketError errorCode, LenghtCallbackReceived, client);
		}

		#region Generic Messages
		/// <summary>
		/// ������ ��������� ����������
		/// </summary>
		protected Message ConnectMessage()
		{
			return new Message {
				ActionId = Guid.Empty,
				ActionName = nameof(Connect),
				Args = new object[] { },
				Id = Guid.NewGuid()
			};
		}

		/// <summary>
		/// ������ ��������� ����������
		/// </summary>
		protected Message DisconectMesage()
		{
			return new Message {
				ActionId = Guid.Empty,
				ActionName = nameof(Disconect),
				Args = new object[] { },
				Id = Guid.NewGuid()
			};
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// ������������
		/// </summary>
		/// <returns>��������� �����������</returns>
		public abstract bool Connect();

		/// <summary>
		/// �����������
		/// </summary>
		/// <returns>��������� ����������</returns>
		public abstract bool Disconect();

		/// <summary>
		/// ���������� ��������
		/// </summary>
		public virtual void Dispose()
		{
			if (Socket.Connected)
			{
				Socket.Disconnect(false);
				Socket.Shutdown(SocketShutdown.Both);
			}
			
			Socket.Dispose();
			
			Socket = null;
			Disposed = true;
		}
		#endregion

		private static byte[] ToPacketByte(object obj)
		{
			byte[] bytes;
			
			using (MemoryStream stream = new MemoryStream())
			{
				Formatter.Serialize(stream, obj);
				
				bytes = stream.ToArray();
			}
			
			byte[] lenght = BitConverter.GetBytes(bytes.LongLength);
			
			byte[] b = new byte[bytes.Length + lenght.Length];
			
			for (int i = 0; i < lenght.Length; i++)
				b[i] = lenght[i];
			
			for (int i = lenght.Length; i < b.Length; i++)
				b[i] = bytes[i - lenght.Length];
			
			return b;
		}

		private static object ToObjectPacket(byte[] bytes)
		{
			using (Stream stream = new MemoryStream(bytes))
			{
				return Formatter.Deserialize(stream);
			}
		}

		private struct SendingStateObject
		{
			public ICoherentUnit Client;
			public IMessage Message;
		}
	}
}