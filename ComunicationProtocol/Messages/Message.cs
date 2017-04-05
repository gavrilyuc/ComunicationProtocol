using System;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// представляет объект сообщения.
	/// </summary>
	[Serializable]
	public sealed class Message: MessageBase
	{
		/// <summary/>
		public Message()
		{
			
		}

		/// <summary>
		/// Конструктор копирования
		/// </summary>
		/// <param name="msg"></param>
		public Message(IMessage msg)
		{
			ActionName = msg.ActionName;
			ActionId = msg.ActionId;
			Args = (object[])msg.Args.Clone();
			Id = Guid.NewGuid();
		}
	}
}