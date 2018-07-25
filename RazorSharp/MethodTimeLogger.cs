using System.Reflection;
using RazorCommon;

namespace RazorSharp
{

	public class MethodTimeLogger
	{
		public static void Log(MethodBase methodBase, long milliseconds, string message)
		{
#if DEBUG
			string className = methodBase.ReflectedType.Name;
			string fullName  = className + "::" + methodBase.Name;

			if (string.IsNullOrEmpty(message)) {
				Logger.Log(Flags.Timer, "Method {0} executed in {1} ms", fullName, milliseconds);
			}
			else {
				Logger.Log(Flags.Timer, "Method {0} executed in {1} ms with message {2}", fullName, milliseconds,
					message);
			}
#endif

		}
	}

}