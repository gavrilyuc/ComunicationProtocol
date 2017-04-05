using System;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// представляет объект сообщения.
	/// </summary>
	[Serializable]
	public abstract class MessageBase: EventArgs, IMessage
	{
		/// <summary>
		/// ID Сообщения
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// ID Операции. Какую операцию контрактник хочет совершить
		/// </summary>
		public Guid ActionId { get; set; }
		
		/// <summary>
		/// Наименование операции
		/// </summary>
		public string ActionName { get; set; }

		/// <summary>
		/// Параметры операции
		/// </summary>
		public object[] Args { get; set; }

		/// <summary>
		/// Указывает, равен ли текущий объект другому объекту того же типа.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
		/// <returns>true, если текущий объект равен параметру other, в противном случае — false.</returns>
		public bool Equals(IMessage other)
		{
			if (other == null)
				return false;

			return Id == other.Id && ActionId == other.ActionId && ActionName == other.ActionName;
		}

		/// <summary>
		/// Указывает, равен ли текущий объект другому объекту того же типа.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с данным объектом.</param>
		/// <returns>true, если текущий объект равен параметру other, в противном случае — false.</returns>
		public override bool Equals(object obj) => Equals(obj as IMessage);

		/// <summary>
		/// Играет роль хэш-функции для определенного типа.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта <see cref="MessageBase" /></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Id.GetHashCode();
				hashCode = (hashCode * 397) ^ ActionId.GetHashCode();
				hashCode = (hashCode * 397) ^ ActionName.GetHashCode();
				hashCode = (hashCode * 397) ^ Args.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Возвращает объект System.String, который представляет текущий объект  <see cref="MessageBase" />
		/// </summary>
		/// <returns>Объект System.String, представляющий текущий объект  <see cref="MessageBase" /></returns>
		public override string ToString()
		{
			string[] s = { nameof(Id), nameof(ActionId), nameof(ActionName), nameof(Args) };
			object[] p = { Id, ActionId, ActionName, Args.ToString() };

			for (int i = 0; i < 4; i++)
			{
				s[i] = $"{s[i]} = {p[i]}";
			}

			s[0] = $"\n{{\n\t{s[0]}";
			return string.Join(",\n\t", s) + "\n}";
		}
	}
}