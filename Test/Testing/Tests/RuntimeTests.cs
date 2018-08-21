#region

using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	internal unsafe class RuntimeTests
	{


		[Test]
		public void TestString()
		{
			string       s  = "foo";
			MethodTable* mt = Runtime.ReadMethodTable(ref s);

			Debug.Assert(mt->IsStringOrArray);

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

//			Debug.Assert(mt == (MethodTable*) 0x00007fff1c1a6830);
//			Debug.Assert(mt->EEClass == (void*) 0x00007fff1ba86cb8);
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

//			Debug.Assert(mt->Module == (void*) 0x00007fff1ba81000);
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

		[Test]
		public void TestList()
		{
			List<int>    list = new List<int>();
			MethodTable* mt   = Runtime.ReadMethodTable(ref list);

			// Name:        System.Collections.Generic.List`1[[System.Int32, mscorlib]]
			// MethodTable: 00007ff819d105d8
			// EEClass:     00007ff8196d88e0
			// Size:        40(0x28) bytes
			// File:
			// Fields:
			// MT Field   Offset Type VT Attr            Value Name
			// 00007ff819d39118  4001886        8       System.Int32[]  0 instance 00000175000192e0 _items
			// 00007ff819d39180  4001887       18         System.Int32  1 instance                0 _size
			// 00007ff819d39180  4001888       1c         System.Int32  1 instance                0 _version
			// 00007ff819d36e10  4001889       10        System.Object  0 instance 0000000000000000 _syncRoot
			// 00007ff819d39118  400188a        0       System.Int32[]  0   shared           static _emptyArray

			Debug.Assert(!mt->IsArray);
			Debug.Assert(!mt->HasComponentSize);
			Debug.Assert(!mt->IsStringOrArray);

			// 0:007> !DumpMT /d 00007ff819d105d8
			// EEClass:         00007ff8196d88e0
			// Module:          00007ff819611000
			// Name:            System.Collections.Generic.List`1[[System.Int32, mscorlib]]
			// mdToken:         00000000020004af
			// File:
			// BaseSize:        0x28
			// ComponentSize:   0x0
			// Slots in VTable: 77
			// Number of IFaces in IFaceMap: 8

//			Debug.Assert(mt->Module == (void*) 0x00007ff819611000);
			Debug.Assert(mt->BaseSize == 40);
			Debug.Assert(Unsafe.HeapSize(ref list) == 40);

//			Debug.Assert(mt == (MethodTable*) 0x00007ff819d105d8);
			Debug.Assert(mt->ComponentSize == 0);
			Debug.Assert(mt->NumInterfaces == 8);

			// 0:007> !DumpClass /d 00007ff8196d88e0
			// Class Name:      System.Collections.Generic.List`1[[System.Int32, mscorlib]]
			// mdToken:         00000000020004af
			// File:
			// Parent Class:    00007ff819616d60
			// Module:          00007ff819611000
			// Method Table:    00007ff819d105d8
			// Vtable Slots:    1e
			// Total Method Slots:  4d
			// Class Attributes:    102001
			// Transparency:        Transparent
			// NumInstanceFields:   4
			// NumStaticFields:     1


//			Debug.Assert(mt->Parent == (MethodTable*) 0x00007ff819616d60);
//			Debug.Assert(mt->EEClass->MethodTable == (MethodTable*) 0x00007ff819d105d8);
			Debug.Assert(mt->NumInstanceFields == 4);
			Debug.Assert(mt->NumStaticFields == 1);

			Debug.Assert(mt->EEClass->Attributes == 0x102001);

//			Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff8196d88e0);
			Debug.Assert(mt->FieldDescListLength == 5);
		}

		[Test]
		public void TestArray()
		{
			int[]        arr = new int[5];
			MethodTable* mt  = Runtime.ReadMethodTable(ref arr);


			// 0:007> !do 0x17500083FB0
			// Name:        System.Int32[]
			// MethodTable: 00007ff819d39118
			// EEClass:     00007ff8196e4668
			// Size:        44(0x2c) bytes
			// Array:       Rank 1, Number of elements 5, Type Int32 (Print Array)
			// Fields:
			// None

//			Debug.Assert(mt == (MethodTable*) 0x00007ff819d39118);
//			Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff8196e4668);
			Debug.Assert(Unsafe.HeapSize(ref arr) == 44);

			// 0:007> !DumpMT /d 00007ff819d39118
			// EEClass:         00007ff8196e4668
			// Module:          00007ff819611000
			// Name:            System.Int32[]
			// mdToken:         0000000002000000
			// File:
			// BaseSize:        0x18
			// ComponentSize:   0x4
			// Slots in VTable: 28
			// Number of IFaces in IFaceMap: 6

//			Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff8196e4668);
//			Debug.Assert(mt->Module == (void*) 0x00007ff819611000);
			Debug.Assert(mt->BaseSize == 0x18);
			Debug.Assert(mt->ComponentSize == 4);
			Debug.Assert(mt->NumInterfaces == 6);


			// !DumpClass /d 00007ff8196e4668
			// Class Name:      System.Int32[]
			// mdToken:         0000000002000000
			// File:
			// Parent Class:    00007ff8196e4838
			// Module:          00007ff819611000
			// Method Table:    00007ff819d39118
			// Vtable Slots:    18
			// Total Method Slots:  1c
			// Class Attributes:    2101
			// Transparency:        Transparent
			// NumInstanceFields:   0
			// NumStaticFields:     0

//			Debug.Assert(mt->Parent == (MethodTable*) 0x00007ff8196e4838);
//			Debug.Assert(mt->EEClass->MethodTable == (MethodTable*) 0x00007ff819d39118);
			Debug.Assert(mt->EEClass->Attributes == 0x2101);
			Debug.Assert(mt->NumInstanceFields == 0);
			Debug.Assert(mt->NumStaticFields == 0);
		}

		[Test]
		public void TestPtrArray()
		{
			string[]     arr = new string[5];
			MethodTable* mt  = Runtime.ReadMethodTable(ref arr);


			Debug.Assert(mt->ElementTypeHandle == Runtime.MethodTableOf<string>());

			// !do 0x175000746A8
			// Name:        System.String[]
			// MethodTable: 00007ff819d1eb08
			// EEClass:     00007ff8196e3ca0
			// Size:        64(0x40) bytes
			// Array:       Rank 1, Number of elements 5, Type CLASS (Print Array)
			// Fields:
			// None

//			Debug.Assert(mt == (MethodTable*)0x00007ff819d1eb08);
//			Assert.That((long) mt, Is.EqualTo(0x00007ff819d1eb08));
//			Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff8196e3ca0);
			Debug.Assert(Unsafe.HeapSize(ref arr) == 64);

			// !DumpMT /d 00007ff819d1eb08
			// EEClass:         00007ff8196e3ca0
			// Module:          00007ff819611000
			// Name:            System.String[]
			// mdToken:         0000000002000000
			// File:
			// BaseSize:        0x18
			// ComponentSize:   0x8
			// Slots in VTable: 28
			// Number of IFaces in IFaceMap: 6

//			Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff8196e3ca0);
//			Debug.Assert(mt->Module == (void*) 0x00007ff819611000);
			Debug.Assert(mt->BaseSize == 0x18);
			Debug.Assert(mt->ComponentSize == 8);
			Debug.Assert(mt->NumInterfaces == 6);

			// !DumpClass /d 00007ff8196e3ca0
			// Class Name:      System.Object[]
			// mdToken:         0000000002000000
			// File:
			// Parent Class:    00007ff8196e4838
			// Module:          00007ff819611000
			// Method Table:    00007ff819d36ea8
			// Vtable Slots:    18
			// Total Method Slots:  1c
			// Class Attributes:    2101
			// Transparency:        Transparent
			// NumInstanceFields:   0
			// NumStaticFields:     0

			// fails
//			Debug.Assert(mt->Parent == (void*) 0x00007ff8196e4838);
//			Assert.That((long)mt->Parent, Is.EqualTo(0x00007ff8196e4838));
//			Debug.Assert(mt->Module == (void*) 0x00007ff819611000);

//			Debug.Assert(mt->EEClass->MethodTable == (MethodTable*)0x00007ff819d36ea8);
			Debug.Assert(mt->EEClass->Attributes == 0x2101);
			Debug.Assert(mt->NumInstanceFields == 0);
			Debug.Assert(mt->NumStaticFields == 0);
		}

		[Test]
		public void TestPoint()
		{
			FieldDesc* xfd = Runtime.GetFieldDesc<Point>("_x");
			Debug.Assert(xfd->MemberDef == xfd->FieldInfo.MetadataToken);
		}

		[Test]
		public void TestDummy()
		{
			Dummy        d  = new Dummy();
			MethodTable* mt = Runtime.ReadMethodTable(ref d);


			// 0:007> !do 0x1750004EEC8
			// Name:        Test.Testing.Dummy
			// MethodTable: 00007ff7bdce6c18
			// EEClass:     00007ff7bdcd8ab0
			// Size:        112(0x70) bytes
			// File:        C:\Users\Viper\RiderProjects\RazorSharp\Test\bin\x64\Debug\Test.exe
			// Fields:

			//Debug.Assert(mt == (MethodTable*) 0x00007ff7bdce6c18);
			//Debug.Assert(mt->EEClass == (EEClass*) 0x00007ff7bdcd8ab0);
			Debug.Assert(Unsafe.HeapSize(ref d) == 112);

			// 0:007> !DumpMT /d 00007ff7bdce6c18
			// EEClass:         00007ff7bdcd8ab0
			// Module:          00007ff7bdb34118
			// Name:            Test.Testing.Dummy
			// mdToken:         0000000002000005
			// File:            C:\Users\Viper\RiderProjects\RazorSharp\Test\bin\x64\Debug\Test.exe
			// BaseSize:        0x70
			// ComponentSize:   0x0
			// Slots in VTable: 16
			// Number of IFaces in IFaceMap: 0

			//Debug.Assert(mt->Module==(void*) 0x00007ff7bdb34118);
			Debug.Assert(mt->BaseSize == 0x70);
			Debug.Assert(mt->ComponentSize == 0);
			Debug.Assert(mt->NumInterfaces == 0);


			// 0:007> !DumpClass /d 00007ff7bdcd8ab0
			// Class Name:      Test.Testing.Dummy
			// mdToken:         0000000002000005
			// File:            C:\Users\Viper\RiderProjects\RazorSharp\Test\bin\x64\Debug\Test.exe
			// Parent Class:    00007ff819616d60
			// Module:          00007ff7bdb34118
			// Method Table:    00007ff7bdce6c18
			// Vtable Slots:    4
			// Total Method Slots:  5
			// Class Attributes:    100001
			// Transparency:        Critical
			// NumStaticFields:     0

			//Debug.Assert(mt->Parent == (MethodTable*) 0x00007ff819616d60);
			//Debug.Assert(mt->EEClass->MethodTable == (MethodTable*) 0x00007ff7bdce6c18);
			Debug.Assert(mt->NumInstanceFields == 17);
			Debug.Assert(mt->NumStaticFields == 0);
			Debug.Assert(mt->EEClass->Attributes == 0x100001);
		}

	}

}