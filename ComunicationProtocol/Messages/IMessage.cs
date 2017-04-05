using System;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// ������������ ������ ���������
	/// </summary>
	public interface IMessage: IEquatable<IMessage>
	{
		/// <summary>
		/// ID ���������
		/// </summary>
		Guid Id { get; }

		/// <summary>
		/// ID ��������. ����� �������� ����������� ����� ���������
		/// </summary>
		Guid ActionId { get; }

		/// <summary>
		/// ������������ ��������
		/// </summary>
		string ActionName { get; }

		/// <summary>
		/// ��������� ��������
		/// </summary>
		object[] Args { get; }

	}

	/// <summary>
	/// ������������ ������ ������ �� ���������
	/// </summary>
	public interface IMessageResult: IMessage, IEquatable<IMessageResult>
	{
		/// <summary>
		/// ��������� ������ �� ���������
		/// </summary>
		object Result { get; }
	}
}