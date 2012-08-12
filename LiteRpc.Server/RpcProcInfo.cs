namespace LiteRpc.Server
{
	using System.Collections.Generic;
	using System.Reflection;

	public class RpcProcInfo
	{
		public string DomainName { get; set; }
		public IEnumerable<MethodInfo> Methods { get; set; }
	}
}
