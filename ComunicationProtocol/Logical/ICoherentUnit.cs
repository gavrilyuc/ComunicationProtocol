using System.Net.Sockets;

namespace ComunicationProtocol.Logical
{
	/// <summary>
	/// Объект Пользователя/Сокета. Подключённого к провайдеру
	/// </summary>
	public interface ICoherentUnit: IDisposable
	{
		/// <summary>
		/// Используемый сокет
		/// </summary>
		Socket Socket { get; }

		/// <summary>
		/// Буффер для получение заголовка пакета
		/// </summary>
		byte[] LenghtBuffer { get; }

		/// <summary>
		/// Размер всего пакета
		/// </summary>
		int Lenght { get; set; }

		/// <summary/>
		int CurrentPosition { get; set; }

		/// <summary>
		/// Контент пакета
		/// </summary>
		byte[] ContentBuffer { get; set; }

		/// <summary>
		/// Очистить параметры предыдущего использования
		/// </summary>
		void ClearParameters();
	}

	/// <summary>
	/// Объект общения Пользователя/Сокета. Подключённого к провайдеру
	/// </summary>
	public interface ICoherentReceiverUnit: ICoherentUnit
	{
		/// <summary>
		/// Обратное общение
		/// </summary>
		ICoherentUnit Receiver { get; }
	}
}