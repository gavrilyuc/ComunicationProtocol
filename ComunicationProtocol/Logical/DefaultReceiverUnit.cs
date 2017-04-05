using System.Net.Sockets;

namespace ComunicationProtocol.Logical
{
	/// <summary>
	/// Базовое конкретное представление пользователя/сокета с возможностью обратного запроса/ответа
	/// </summary>
	public class DefaultReceiverUnit: ICoherentReceiverUnit
	{
		/// <summary>
		/// Используемый сокет
		/// </summary>
		public Socket Socket { get; }

		/// <summary>
		/// Обратное общение
		/// </summary>
		public ICoherentUnit Receiver { get; }

		/// <summary>
		/// Пользовательское свойство
		/// </summary>
		public object Tag { get; set; }

		/// <summary/>
		public DefaultReceiverUnit(Socket socket, Socket receiver, int lenghtBytes)
		{
			Socket = socket;
			Receiver = new DefaultCoherentUnit(receiver, lenghtBytes);
			LenghtBuffer = new byte[lenghtBytes];
		}

		/// <summary>
		/// Буффер для получение заголовка пакета
		/// </summary>
		public byte[] LenghtBuffer { get; }

		/// <summary>
		/// Размер всего пакета
		/// </summary>
		public int Lenght { get; set; }

		/// <summary/>
		public int CurrentPosition { get; set; }

		/// <summary>
		/// Контент пакета
		/// </summary>
		public byte[] ContentBuffer { get; set; }

		/// <summary>
		/// Данный Объект Освобождён (True = освобождён)
		/// </summary>
		public bool Disposed { get; private set; }

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
			Receiver.ClearParameters();
		}

		/// <summary>
		/// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением
		/// или сбросом неуправляемых ресурсов.
		/// </summary>
		public void Dispose()
		{
			if (Disposed)
				return;
			
			Disposed = true;
			ClearParameters();
			Socket.Disconnect(false);
			Socket.Shutdown(SocketShutdown.Both);
			Socket.Dispose();

			Receiver.Dispose();
		}
	}
}