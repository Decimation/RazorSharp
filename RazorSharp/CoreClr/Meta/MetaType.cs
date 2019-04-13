#region

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Utilities;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     Exposes metadata from:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="MethodTable" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="EEClass" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="EEClassLayoutInfo" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class MetaType : IMeta, IFormattable
	{
		/// <summary>
		///     Exhaustive
		/// </summary>
		private const string FMT_E = "E";

		/// <summary>
		///     Basic
		/// </summary>
		private const string FMT_B = "B";

		internal MetaType(Pointer<MethodTable> p)
		{
			Value = p;

			if (!p.Reference.Canon.IsNull && p.Reference.Canon.Address != p.Address)
				Canon = new MetaType(p.Reference.Canon);

			if (p.Reference.IsArray)
				ElementType = new MetaType(p.Reference.ElementTypeHandle);

			if (!p.Reference.Parent.IsNull) {
				Parent = new MetaType(p.Reference.Parent);
			}


			Fields    = new VirtualCollection<MetaField>(GetField, GetFields);
			Methods   = new VirtualCollection<MetaMethod>(GetMethod, GetMethods);
			AllFields = new VirtualCollection<MetaField>(GetAnyField, GetAllFields);
		}

		private unsafe Pointer<EEClassLayoutInfo> LayoutInfo => Value.Reference.EEClass.Reference.LayoutInfo;

		public IEnumerable<MetaField> InstanceFields {
			get {
				return Fields
				      .Where(f => !f.FieldInfo.IsStatic && !f.FieldInfo.IsLiteral)
				      .OrderBy(f => f.Offset);
			}
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
				format = FMT_B;

			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;


			switch (format.ToUpperInvariant()) {
				case FMT_B:
					return
						String.Format("{0} (token: {1}) (base size: {2}) (component size: {3}) (base fields size: {4})",
						              Name, Token, BaseSize, ComponentSize, BaseFieldsSize);
				case FMT_E:
					return ToTable().ToMarkDownString();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private MetaField GetAnyField(string name)
		{
			var field = RuntimeType.GetAnyField(name);
			Conditions.Require(!field.IsLiteral, "Field cannot be literal", nameof(field));
			return new MetaField(field.GetFieldDesc());
		}

		private MetaField[] GetAllFields()
		{
			FieldInfo[] fields     = RuntimeType.GetAllFields().Where(x => !x.IsLiteral).ToArray();
			var         metaFields = new MetaField[fields.Length];

			for (int i = 0; i < fields.Length; i++) {
				metaFields[i] = new MetaField(fields[i].GetFieldDesc());
			}

			return metaFields;
		}

		private MetaField GetField(string name)
		{
			return new MetaField(RuntimeType.GetFieldDesc(name));
		}

		private MetaField[] GetFields()
		{
			Pointer<FieldDesc>[] fields = RuntimeType.GetFieldDescs();
			var                  meta   = new MetaField[fields.Length];

			for (int i = 0; i < fields.Length; i++)
				meta[i] = new MetaField(fields[i]);

			return meta;
		}

		private MetaMethod GetMethod(string name)
		{
			return new MetaMethod(RuntimeType.GetMethodDesc(name));
		}

		private MetaMethod[] GetMethods()
		{
			Pointer<MethodDesc>[] methods = RuntimeType.GetMethodDescs();
			var                   meta    = new MetaMethod[methods.Length];

			for (int i = 0; i < meta.Length; i++)
				meta[i] = new MetaMethod(methods[i]);

			return meta;
		}

		private ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Info", "Value");
			table.AddRow("Name", Name);
			table.AddRow("Token", Token);

			/* -- Sizes -- */
			table.AddRow("Base size", BaseSize);
			table.AddRow("Component size", ComponentSize);
			table.AddRow("Base fields size", BaseFieldsSize);

			/* -- Flags -- */
			table.AddRow("Flags", EnumUtil.CreateString(Flags));
			table.AddRow("Flags 2", EnumUtil.CreateString(Flags2));
			table.AddRow("Low flags", EnumUtil.CreateString(FlagsLow));
			table.AddRow("Attributes", EnumUtil.CreateString(TypeAttributes));
			table.AddRow("Layout flags", HasLayout ? EnumUtil.CreateString(LayoutFlags) : "-");
			table.AddRow("VM Flags", EnumUtil.CreateString(VMFlags));

			/* -- Aux types -- */
			table.AddRow("Canon type", Canon?.Name);
			table.AddRow("Element type", ElementType?.Name);
			table.AddRow("Parent type", Parent.Name);

			/* -- Numbers -- */
			table.AddRow("Number instance fields", NumInstanceFields);
			table.AddRow("Number static fields", NumStaticFields);
			table.AddRow("Number non virtual slots", NumNonVirtualSlots);
			table.AddRow("Number methods", NumMethods);
			table.AddRow("Number instance field bytes", NumInstanceFieldBytes);
			table.AddRow("Number virtuals", NumVirtuals);
			table.AddRow("Number interfaces", NumInterfaces);

			table.AddRow("Blittable", IsBlittable);


			table.AddRow("Value", Value.ToString("P"));

			return table;
		}

		public override string ToString()
		{
			return ToString(FMT_B);
		}

		public string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		#region Accessors

		public MetaField this[string name] => AllFields[name];

		public VirtualCollection<MetaField> Fields { get; }

		public VirtualCollection<MetaField> AllFields { get; }

		public VirtualCollection<MetaMethod> Methods { get; }

		public MetaType Canon { get; }

		public MetaType ElementType { get; }

		public MetaType Parent { get; }


		public CorElementType NormalType => Value.Reference.EEClass.Reference.NormalType;

		public string Name => Value.Reference.Name;

		/// <summary>
		///     Metadata token
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpMT /d</c> <c>"mdToken"</c> value in hexadecimal format.</para>
		///         <para>Equals <see cref="Type.MetadataToken" /></para>
		///     </remarks>
		/// </summary>
		public int Token => Value.Reference.Token;

		/// <summary>
		///     <para>Corresponding <see cref="Type" /> of this <see cref="MethodTable" /></para>
		/// </summary>
		public Type RuntimeType => Value.Reference.RuntimeType;

		#region bool

		public bool IsZeroSized => LayoutInfo.Reference.ZeroSized;

		public bool HasLayout => Value.Reference.EEClass.Reference.HasLayout;

		public bool HasComponentSize => Value.Reference.HasComponentSize;

		public bool IsArray => Value.Reference.IsArray;

		public bool IsStringOrArray => Value.Reference.IsStringOrArray;

		public bool IsBlittable => Value.Reference.IsBlittable;

		public bool IsString => Value.Reference.IsString;

		public bool ContainsPointers => Value.Reference.ContainsPointers;

		#endregion

		#region Flags

		public LayoutFlags LayoutFlags => LayoutInfo.Reference.Flags;

		public TypeAttributes TypeAttributes => Value.Reference.EEClass.Reference.TypeAttributes;

		public VMFlags VMFlags => Value.Reference.EEClass.Reference.VMFlags;

		public MethodTableFlags Flags => Value.Reference.Flags;

		public MethodTableFlags2 Flags2 => Value.Reference.Flags2;

		public MethodTableFlagsLow FlagsLow => Value.Reference.FlagsLow;

		#endregion

		#region Size

		/// <summary>
		///     The base size of this class when allocated on the heap. Note that for value types
		///     <see cref="BaseSize" /> returns the size of instance fields for a boxed value, and
		///     <see cref="NumInstanceFieldBytes" /> for an unboxed value.
		/// </summary>
		public int BaseSize => Value.Reference.BaseSize;

		/// <summary>
		///     <para>The size of an individual element when this type is an array or string.</para>
		///     <example>
		///         If this type is a <c>string</c>, the component size will be <c>2</c>. (<c>sizeof(char)</c>)
		///     </example>
		///     <returns>
		///         <c>0</c> if <c>!</c><see cref="HasComponentSize" />, component size otherwise
		///     </returns>
		/// </summary>
		public int ComponentSize => Value.Reference.ComponentSize;

		public int ManagedSize => (int) LayoutInfo.Reference.ManagedSize;

		public int NativeSize => Value.Reference.EEClass.Reference.NativeSize;

		public int BaseSizePadding => Value.Reference.EEClass.Reference.BaseSizePadding;

		public int BaseFieldsSize => Value.Reference.NumInstanceFieldBytes;

		#endregion

		#region Num

		/// <summary>
		///     The number of instance fields in this type.
		/// </summary>
		public int NumInstanceFields => Value.Reference.NumInstanceFields;

		/// <summary>
		///     The number of <c>static</c> fields in this type.
		/// </summary>
		public int NumStaticFields => Value.Reference.NumStaticFields;

		public int NumNonVirtualSlots => Value.Reference.NumNonVirtualSlots;

		/// <summary>
		///     Number of methods in this type.
		/// </summary>
		public int NumMethods => Value.Reference.NumMethods;

		/// <summary>
		///     The size of the instance fields in this type. This is the unboxed size of the type if the object is boxed.
		///     (Minus padding and overhead of the base size.)
		/// </summary>
		public int NumInstanceFieldBytes => Value.Reference.NumInstanceFieldBytes;

		/// <summary>
		///     The number of virtual methods in this type (<c>4</c> by default; from <see cref="Object" />)
		/// </summary>
		public int NumVirtuals => Value.Reference.NumVirtuals;

		/// <summary>
		///     The number of interfaces this type implements
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpMT /d</c> <c>Number of IFaces in IFaceMap</c> value.</para>
		///     </remarks>
		/// </summary>
		public int NumInterfaces => Value.Reference.NumInterfaces;

		#endregion

		internal Pointer<MethodTable> Value { get; }

		#endregion
	}
}