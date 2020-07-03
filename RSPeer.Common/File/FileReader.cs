using System.IO;
using System.Reflection;

namespace RSPeer.Common.File
{
	public class FileReader
	{
		public static string GetFullPath(string path)
		{
			return GetFullPath(path, Assembly.GetEntryAssembly());
		}
		
		public static string GetFullPath(string path, Assembly assembly)
		{
			var location = Path.GetDirectoryName(assembly.Location);
			return Path.Combine(location, path);
		}
	}
}