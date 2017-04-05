using System;
using System.Linq;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// Сообщение-Ответ на отправленое сообщение
	/// </summary>
	[Serializable]
	public sealed class MessageResult : MessageBase, IMessageResult
	{
		/// <summary>
		/// Основной объект результата
		/// </summary>
		public object Result { get; set; }
		
		/// <summary>
		/// Конструктор копирования
		/// </summary>
		/// <param name="msg">сообщение</param>
		public MessageResult(IMessage msg)
		{
			Id = msg.Id;
			ActionId = msg.ActionId;
			Args = msg.Args.ToArray();// copy
			ActionName = msg.ActionName;
		}

		/// <summary>
		/// Указывает, равен ли текущий объект другому объекту того же типа.
		/// </summary>
		/// <returns>
		/// true, если текущий объект равен параметру <paramref name="obj"/>, в противном случае — false.
		/// </returns>
		/// <param name="obj">Объект, который требуется сравнить с данным объектом.</param>
		public override bool Equals(object obj)
		{
			IMessageResult r = obj as IMessageResult;
			return r != null && Equals(r);
		}

		/// <summary>
		/// Служит хэш-функцией по умолчанию. 
		/// </summary>
		/// <returns>
		/// Хэш-код для текущего объекта.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ (Result?.GetHashCode() ?? 0);
			}
		}

		/// <summary>
		/// Указывает, равен ли текущий объект другому объекту того же типа.
		/// </summary>
		/// <returns>
		/// true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.
		/// </returns>
		/// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
		public bool Equals(IMessageResult other)
		{
			if (other == null)
				return false;
			
			return other.Id == Id && other.ActionId == ActionId && other.ActionName == ActionName;
		}
	}
}