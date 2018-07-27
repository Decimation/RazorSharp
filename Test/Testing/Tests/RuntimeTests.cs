using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;

namespace Test.Testing.Tests
{

	[TestFixture]
	internal unsafe class RuntimeTests
	{
		[Test]
		public void Test()
		{
			string s  = "foo";
			var    mt = Runtime.ReadMethodTable(ref s);

			// Name:        System.String
			// MethodTable: 00007fff1c1a6830
			// EEClass:     00007fff1ba86cb8
			// Size:        32(0x20) bytes
			// File:        C:\WINDOWS\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll
			// String:      foo
			// Fields:
			//               MT    Field   Offset                 Type VT     Attr            Value Name
			// 00007fff1c1a9180  4000273        8         System.Int32  1 instance                3 m_stringLength
			// 00007fff1c1a79e8  4000274        c          System.Char  1 instance               66 m_firstChar
			// 00007fff1c1a6830  4000278       a0        System.String  0   shared           static Empty
			//                                  >> Domain:Value  0000027796286260:NotInit  <<

			Debug.Assert(mt == (MethodTable*) 0x00007fff1c1a6830);
			Debug.Assert(mt->EEClass == (void*) 0x00007fff1ba86cb8);
			Debug.Assert(Unsafe.HeapSize(ref s) == 32);

			// Note: SOS's BaseSize is wrong here

			// EEClass:         00007fff1ba86cb8
			// Module:          00007fff1ba81000
			// Name:            System.String
			// mdToken:         0000000002000073
			// File:            C:\WINDOWS\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll
			// BaseSize:        0x18
			// ComponentSize:   0x2
			// Slots in VTable: 195
			// Number of IFaces in IFaceMap: 7

			//Debug.Assert(mt->BaseSize == 0x18);
			Debug.Assert(mt->ComponentSize == 0x2);
			Debug.Assert(mt->NumInterfaces == 7);

			// Class Name:      System.String
			// mdToken:         0000000002000073
			// File:            C:\WINDOWS\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll
			// Parent Class:    00007fff1ba86d60
			// Module:          00007fff1ba81000
			// Method Table:    00007fff1c1a6830
			// Vtable Slots:    1b
			// Total Method Slots:  1d
			// Class Attributes:    102101
			// Transparency:        Transparent
			// NumInstanceFields:   2
			// NumStaticFields:     1
			//               MT    Field   Offset                 Type VT     Attr            Value Name
			// 00007fff1c1a9180  4000273        8         System.Int32  1 instance           m_stringLength
			// 00007fff1c1a79e8  4000274        c          System.Char  1 instance           m_firstChar
			// 00007fff1c1a6830  4000278       a0        System.String  0   shared           static Empty
			//                                  >> Domain:Value  0000020aecb433a0:NotInit  <<

			Debug.Assert(mt->Module == (void*) 0x00007fff1ba81000);
			Debug.Assert(mt->EEClass->Attributes == 0x102101);

			// 26 = string's base size
			// 6 = (sizeof(char) + sizeof(int))
			// 26 - 6 = 20
			Debug.Assert(mt->EEClass->BaseSizePadding == 20);
			Debug.Assert(Unsafe.BaseFieldsSize<string>() == 6);
			Debug.Assert(Unsafe.BaseInstanceSize<string>() == 26);


			// Name:       C:\WINDOWS\Microsoft.Net\assembly\GAC_64\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll
			// Attributes: PEFile
			// Assembly:   0000020aecb938e0
			// LoaderHeap:              0000000000000000
			// TypeDefToMethodTableMap: 00007fff1cdf6ca4
			// TypeRefToMethodTableMap: 00007fff1bde9f18
			// MethodDefToDescMap:      00007fff1cdf925c
			// FieldDefToDescMap:       00007fff1bde9f78
			// MemberRefToDescMap:      0000000000000000
			// FileReferencesMap:       00007fff1ba89720
			// AssemblyReferencesMap:   00007fff1ba89750
			// MetaData start address:  00007fff1c32986c (2834728 bytes)
		}

		private static void AssertProperties<T>(ref T t) { }
	}

}