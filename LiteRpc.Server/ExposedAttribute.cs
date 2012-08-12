namespace LiteRpc.Server
{
	using System;

	/// <summary>
	/// This attribute facilitates making an instance controller expose Json-Rpc/Xml-Rpc service.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ExposedAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the type of rpc serialization protocol: JsonRpc or XmlRpc.
		/// </summary>
		public ExposurType Kind { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public ExposedAttribute()
		{
			this.Kind = ExposurType.Json;
		}

		/// <summary>
		/// Constructor with one parameter as ExposureType.
		/// </summary>
		/// <param name="kind"></param>
		public ExposedAttribute(ExposurType kind)
		{
			this.Kind = kind;
		}
	}

	/// <summary>
	/// Enum type specifying serilization type.
	/// </summary>
	[Flags]
	public enum ExposurType
	{
		/// <summary>
		/// Json-Rpc
		/// </summary>
		Json,
		/// <summary>
		/// Xml-Rpc
		/// </summary>
		XML
	}
}