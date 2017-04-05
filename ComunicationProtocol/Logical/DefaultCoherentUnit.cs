using System.Net.Sockets;

namespace ComunicationProtocol.Logical
{
	/// <summary>
	/// Базовый связывающий объект.
	/// Данный объект выступает в роли связи с общителем, храня при себе Поставщик общения и прочую инфорамацию, связаную с поставщиком общения
	/// </summary>
	public class DefaultCoherentUnit: ICoherentUnit
	{
		/// <summary>
		/// Используемый сокет
		/// </summary>
		public Socket Socket { get; internal set; }

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
		/// Пользовательский объект
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Данный Объект Освобождён (True = освобождён)
		/// </summary>
		public bool Disposed { get; private set; }

		/// <summary/>
		internal DefaultCoherentUnit(int lenghBytes)
		{
			LenghtBuffer = new byte[lenghBytes];
		}

		/// <summary/>
		public DefaultCoherentUnit(Socket socket, int lenghBytes): this(lenghBytes)
		{
			Socket = socket;
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

			if (Socket.Connected)
			{
				Socket.Disconnect(false);
				Socket.Shutdown(SocketShutdown.Both);
			}
			
			Socket.Dispose();
		}
	}
}