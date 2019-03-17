using System;
using NUnit.Framework;
using RazorCommon;

namespace Test.Testing.Tests
{
	[TestFixture]
	public class ConsoleTableTests
	{
		[Test]
		public void Test()
		{
			var table = new ConsoleTable("A", "B");
			table.AddRow(1, 2);
			table.AddRow(10, 9);
			table.Attach("C", 3, 8);
			Console.WriteLine(table.ToMarkDownString());
			
			var table2 = new ConsoleTable("A", "B");
			table2.AddRow(1, 2);
			table2.AddRow(10, 9);
			table2.Attach(0,"C", 3, 8);
			Console.WriteLine(table2.ToMarkDownString());
		}
	}
}