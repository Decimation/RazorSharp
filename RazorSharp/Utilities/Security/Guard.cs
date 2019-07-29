using System;
using System.Reflection;
using System.Text;
using RazorSharp.Reflection;
using RazorSharp.Utilities.Security.Exceptions;

// ReSharper disable InconsistentNaming

namespace RazorSharp.Utilities.Security
{
	internal static class Guard
	{
		/// <summary>
		/// Raised when there is an error with retrieving the fail message template.
		/// </summary>
		/// <returns></returns>
		private static SentryException DeepFail()
		{
			const string FAILMSG_ERR = "Fail message was not found, not const/static, or not a string";
			return Fail<SentryException>(FAILMSG_ERR);
		}

		private static string GetFailMessage<TException>() where TException : CoreException, new()
		{
			var (memberInfo, failMessageAttribute) =
				typeof(TException).GetFirstAnnotated<FailMessageTemplateAttribute>();

			// This should never happen

			if (memberInfo == null || failMessageAttribute == null) {
				throw DeepFail();
			}

			var field = (FieldInfo) memberInfo;

//			const string REQ_NAME = "ERR_MSG";

			if (!field.IsStatic || !field.IsLiteral || field.FieldType != typeof(string)) {
				throw DeepFail();
			}

			var value = field.GetValue(null);

			if (value is string failMsg) {
				return failMsg;
			}

			throw DeepFail();
		}

		private static TException Fail<TException>(string msg = null, string extra = null)
			where TException : CoreException, new()
		{
			var template = GetFailMessage<TException>();
			return CreateFailStub<TException>(template, msg, extra);
		}

		private static TException CreateFailStub<TException>(string template, string msg = null, string extra = null)
			where TException : CoreException, new()
		{
			var sb = new StringBuilder();
			sb.Append(template);

			if (msg != null) {
				const string APPEND = ": {0}";

				sb.AppendFormat(APPEND, msg);

				if (extra != null) {
					sb.AppendFormat(APPEND, extra);
				}
			}

			return (TException) Activator.CreateInstance(typeof(TException), sb.ToString());
		}


		#region CoreException

		/// <summary>
		/// Shortcut to <see cref="Fail{TException}"/> with <see cref="ImportException"/>
		/// </summary>
		internal static ImportException ImportFail(string msg = null, string extra = null)
		{
			return Fail<ImportException>(msg, extra);
		}

		/// <summary>
		/// Shortcut to <see cref="Fail{TException}"/> with <see cref="ImageException"/>
		/// </summary>
		internal static ImageException ImageFail(string msg = null, string extra = null)
		{
			return Fail<ImageException>(msg, extra);
		}

		/// <summary>
		/// Shortcut to <see cref="Fail{TException}"/> with <see cref="CorILException"/>
		/// </summary>
		internal static CorILException CorILFail(string msg = null, string extra = null)
		{
			return Fail<CorILException>(msg, extra);
		}

		/// <summary>
		/// Shortcut to <see cref="Fail{TException}"/> with <see cref="ClrException"/>
		/// </summary>
		internal static ClrException ClrFail(string msg = null, string extra = null)
		{
			return Fail<ClrException>(msg, extra);
		}

		/// <summary>
		/// Shortcut to <see cref="Fail{TException}"/> with <see cref="AmbiguousStateException"/>
		/// </summary>
		internal static AmbiguousStateException AmbiguousFail(string msg = null, string extra = null)
		{
			return Fail<AmbiguousStateException>(msg, extra);
		}

		#endregion

		internal static NotImplementedException NotImplementedFail(string func)
		{
			return new NotImplementedException($"{func} is not implemented.");
		}

		internal static NotSupportedException NotSupportedMemberFail(MemberInfo member)
		{
			return new NotSupportedException($"Member type {member.MemberType} not supported.");
		}

		internal static InvalidOperationException Require64BitFail(string func)
		{
			return new InvalidOperationException($"64 bit is required for the requested operation: {func} ");
		}
	}
}