using ComunicationProtocol.Logical;
using ComunicationProtocol.Messages;

namespace ComunicationProtocol
{
	/// <summary>
	/// Делегат об оповещении действий пользователя
	/// </summary>
	/// <param name="client">Клиент испольнитель</param>
	/// <param name="message">Инициированое Сообщение</param>
	public delegate void ListenEventHandler(ICoherentUnit client, IMessage message);

	/// <summary>
	/// Результат выполнение форварда
	/// </summary>
	public enum ListenerResult
	{
		/// <summary>
		/// None
		/// </summary>
		None,
		
		/// <summary>
		/// Продолжить обработку (логируется как true)
		/// </summary>
		Continue,

		/// <summary>
		/// Данный вызов был уже обработан этим вызовом, значит последующие вызовы данного форварда не будут происходить
		/// </summary>
		Override
	}

	/// <summary>
	/// Форвард-Делегат оповещений действий пользователя
	/// </summary>
	/// <param name="client">Клиент испольнитель</param>
	/// <param name="message">Инициированое Сообщение</param>
	/// <returns>Если возвращаемый результат будет равен = overide то данный делегат произвёл заменение операции</returns>
	public delegate ListenerResult ListenForwardEventHandler(ICoherentUnit client, IMessage message);
}