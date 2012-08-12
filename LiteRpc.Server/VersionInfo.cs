namespace LiteRpc.Server
{
	/// <summary>
	/// Class containing the version information of the Json-Rpc service
	/// </summary>
	public class VersionInfo
	{
		/// <summary>
		/// The full title of the service
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The provider company name
		/// </summary>
		public string Provider { get; set; }

		/// <summary>
		/// Full version information
		/// </summary>
		public string Version { get; set; }
	}
}
