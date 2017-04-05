using System;
using System.Linq;

namespace ComunicationProtocol.Messages
{
	/// <summary>
	/// ���������-����� �� ����������� ���������
	/// </summary>
	[Serializable]
	public sealed class MessageResult : MessageBase, IMessageResult
	{
		/// <summary>
		/// �������� ������ ����������
		/// </summary>
		public object Result { get; set; }
		
		/// <summary>
		/// ����������� �����������
		/// </summary>
		/// <param name="msg">���������</param>
		public MessageResult(IMessage msg)
		{
			Id = msg.Id;
			ActionId = msg.ActionId;
			Args = msg.Args.ToArray();// copy
			ActionName = msg.ActionName;
		}

		/// <summary>
		/// ���������, ����� �� ������� ������ ������� ������� ���� �� ����.
		/// </summary>
		/// <returns>
		/// true, ���� ������� ������ ����� ��������� <paramref name="obj"/>, � ��������� �����堗 false.
		/// </returns>
		/// <param name="obj">������, ������� ��������� �������� � ������ ��������.</param>
		public override bool Equals(object obj)
		{
			IMessageResult r = obj as IMessageResult;
			return r != null && Equals(r);
		}

		/// <summary>
		/// ������ ���-�������� �� ���������. 
		/// </summary>
		/// <returns>
		/// ���-��� ��� �������� �������.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (base.GetHashCode() * 397) ^ (Result?.GetHashCode() ?? 0);
			}
		}

		/// <summary>
		/// ���������, ����� �� ������� ������ ������� ������� ���� �� ����.
		/// </summary>
		/// <returns>
		/// true, ���� ������� ������ ����� ��������� <paramref name="other"/>, � ��������� �����堗 false.
		/// </returns>
		/// <param name="other">������, ������� ��������� �������� � ������ ��������.</param>
		public bool Equals(IMessageResult other)
		{
			if (other == null)
				return false;
			
			return other.Id == Id && other.ActionId == ActionId && other.ActionName == ActionName;
		}
	}
}