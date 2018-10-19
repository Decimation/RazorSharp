#region

#region

using System;
using System.Globalization;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Common;
using RazorSharp.Pointers;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.CLR.Meta
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
		private readonly Pointer<MethodTable> m_value;

		internal MetaType(Pointer<MethodTable> p)
		{
			m_value = p;

			if (!p.Reference.Canon.IsNull && p.Reference.Canon.Address != p.Address) {
				Canon = new MetaType(p.Reference.Canon);
			}

			if (p.Reference.IsArray) {
				ElementType = new MetaType(p.Reference.ElementTypeHandle);
			}

			if (!p.Reference.Parent.IsNull) {
				Parent = new MetaType(p.Reference.Parent);
			}

			Fields  = new VirtualCollection<MetaField>(GetField, GetFields);
			Methods = new VirtualCollection<MetaMethod>(GetMethod, GetMethods);
		}

		private unsafe Pointer<EEClassLayoutInfo> LayoutInfo => m_value.Reference.EEClass.Reference.LayoutInfo;

		private MetaField GetField(string name)
		{
			return new MetaField(Runtime.GetFieldDesc(RuntimeType, name));
		}

		private MetaField[] GetFields()
		{
			Pointer<FieldDesc>[] fields = Runtime.GetFieldDescs(RuntimeType);
			MetaField[]          meta   = new MetaField[fields.Length];
			for (int i = 0; i < fields.Length; i++) {
				meta[i] = new MetaField(fields[i]);
			}

			return meta;
		}

		private MetaMethod GetMethod(string name)
		{
			return new MetaMethod(Runtime.GetMethodDesc(RuntimeType, name));
		}

		private MetaMethod[] GetMethods()
		{
			Pointer<MethodDesc>[] methods = Runtime.GetMethodDescs(RuntimeType);
			MetaMethod[]          meta    = new MetaMethod[methods.Length];

			for (int i = 0; i < meta.Length; i++) {
				meta[i] = new MetaMethod(methods[i]);
			}

			return meta;
		}

		#region Accessors

		public MetaField this[string name] => Fields[name];

		public VirtualCollection<MetaField> Fields { get; }

		public VirtualCollection<MetaMethod> Methods { get; }

		public MetaType Canon { get; }

		public MetaType ElementType { get; }

		public MetaType Parent { get; }

		public CorElementType NormalType => m_value.Reference.EEClass.Reference.NormalType;

		public string Name => m_value.Reference.Name;

		public int Token => m_value.Reference.Token;

		public Type RuntimeType => m_value.Reference.RuntimeType;

		#region bool

		public bool IsZeroSized => LayoutInfo.Reference.ZeroSized;

		public bool HasLayout => m_value.Reference.EEClass.Reference.HasLayout;

		public bool HasComponentSize => m_value.Reference.HasComponentSize;

		public bool IsArray => m_value.Reference.IsArray;

		public bool IsStringOrArray => m_value.Reference.IsStringOrArray;

		public bool IsBlittable => m_value.Reference.IsBlittable;

		public bool IsString => m_value.Reference.IsString;

		public bool ContainsPointers => m_value.Reference.ContainsPointers;

		#endregion

		#region Flags

		public LayoutFlags LayoutFlags => LayoutInfo.Reference.Flags;

		public TypeAttributes TypeAttributes => m_value.Reference.EEClass.Reference.TypeAttributes;

		public VMFlags VMFlags => m_value.Reference.EEClass.Reference.VMFlags;

		public MethodTableFlags Flags => m_value.Reference.Flags;

		public MethodTableFlags2 Flags2 => m_value.Reference.Flags2;

		public MethodTableFlagsLow FlagsLow => m_value.Reference.FlagsLow;

		#endregion

		#region Size

		public int BaseSize => m_value.Reference.BaseSize;

		public int ComponentSize => m_value.Reference.ComponentSize;

		public int ManagedSize => (int) LayoutInfo.Reference.ManagedSize;

		public int NativeSize => m_value.Reference.EEClass.Reference.NativeSize;

		public int BaseSizePadding => m_value.Reference.EEClass.Reference.BaseSizePadding;

		public int BaseFieldsSize => m_value.Reference.NumInstanceFieldBytes;

		#endregion

		#region Num

		public int NumInstanceFields => m_value.Reference.NumInstanceFields;

		public int NumStaticFields => m_value.Reference.NumStaticFields;

		public int NumNonVirtualSlots => m_value.Reference.NumNonVirtualSlots;

		public int NumMethods => m_value.Reference.NumMethods;

		public int NumInstanceFieldBytes => m_value.Reference.NumInstanceFieldBytes;

		public int NumVirtuals => m_value.Reference.NumVirtuals;

		public int NumInterfaces => m_value.Reference.NumInterfaces;

		#endregion

		#endregion

		/// <summary>
		/// Exhaustive
		/// </summary>
		private const string FMT_E = "E";

		/// <summary>
		/// Basic
		/// </summary>
		private const string FMT_B = "B";

		private ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Info", "Value");
			table.AddRow("Name", Name);

			table.AddRow("Base size", BaseSize);
			table.AddRow("Component size", ComponentSize);
			table.AddRow("Base fields size", BaseFieldsSize);

			table.AddRow("Flags", Flags);
			table.AddRow("Flags 2", Flags2);
			table.AddRow("Low flags", FlagsLow);
			table.AddRow("Attributes", TypeAttributes);

			table.AddRow("Token", Token);

			table.AddRow("Canon type", Canon?.Name);
			table.AddRow("Element type", ElementType?.Name);
			table.AddRow("Parent type", Parent.Name);


			table.AddRow("Number instance fields", NumInstanceFields);
			table.AddRow("Number static fields", NumStaticFields);
			table.AddRow("Number non virtual slots", NumNonVirtualSlots);
			table.AddRow("Number methods", NumMethods);
			table.AddRow("Number instance field bytes", NumInstanceFieldBytes);
			table.AddRow("Number virtuals", NumVirtuals);
			table.AddRow("Number interfaces", NumInterfaces);

			table.AddRow("Blittable", IsBlittable);

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

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format)) {
				format = FMT_B;
			}

			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}


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


	}

}