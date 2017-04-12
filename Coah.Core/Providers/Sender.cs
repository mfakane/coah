using System.Runtime.Serialization;

namespace Linearstar.Coah
{
	[DataContract]
	public struct Sender
	{
		[DataMember]
		public string Name
		{
			get;
		}

		[DataMember]
		public string Mail
		{
			get;
		}

		[DataMember]
		public string Identifier
		{
			get;
			set;
		}

		public Sender(string name, string mail)
		{
			Name = name;
			Mail = mail;
			Identifier = null;
		}

		public Sender(string name, string mail, string identifier)
			: this(name, mail)
		{
			Identifier = identifier;
		}
	}
}
