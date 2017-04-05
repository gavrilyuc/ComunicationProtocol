using System;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// Представляет объект сообщения
	/// </summary>
	public interface IMessage: IEquatable<IMessage>
	{
		/// <summary>
		/// ID Сообщения
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// ID Операции. Какую операцию контрактник хочет совершить
		/// </summary>
		Guid ActionId { get; }

		/// <summary>
		/// Наименование операции
		/// </summary>
		string ActionName { get; }

		/// <summary>
		/// Параметры операции
		/// </summary>
		object[] Args { get; }

	}

	/// <summary>
	/// Представляет объект ответа на сообщение
	/// </summary>
	public interface IMessageResult: IMessage, IEquatable<IMessageResult>
	{
		/// <summary>
		/// Результат ответа на сообщение
		/// </summary>
		object Result { get; }
	}
}