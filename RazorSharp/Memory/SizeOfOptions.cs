using System;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;

namespace RazorSharp.Memory
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion
	
	/// <summary>
	/// Specifies how sizes are calculated
	/// </summary>
	public enum SizeOfOptions
	{
		/// <summary>
		///     <para>Returns the native (<see cref="Marshal" />) size of a type.</para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		///     <para> Returned from <see cref="EEClass.NativeSize" /> </para>
		///     <para> Equals <see cref="Marshal.SizeOf(Type)" /></para>
		///     <para> Equals <see cref="StructLayoutAttribute.Size" /> when type isn't zero-sized.</para>
		/// </remarks>
		/// <returns>The native size if the type has a native representation; <see cref="Constants.INVALID_VALUE" /> otherwise</returns>
		Native,
		
		/// <summary>
		///     Returns the managed size of an object.
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		///     <para>Returned from <see cref="EEClassLayoutInfo.ManagedSize" /></para>
		/// </remarks>
		/// <returns>
		///     Managed size if the type has an <see cref="EEClassLayoutInfo" />; <see cref="Constants.INVALID_VALUE" />
		///     otherwise
		/// </returns>
		Managed,
		
		
		/// <summary>
		///     <para>Returns the normal size of a type in memory.</para>
		///     <para>Call to <see cref="CSUnsafe.SizeOf{T}()" /></para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed</para>
		/// </remarks>
		/// <returns><see cref="IntPtr.Size" /> for reference types, size for value types</returns>
		Intrinsic,
		
		/// <summary>
		///     <para>Returns the base size of the fields (data) in the heap.</para>
		///     <para>This follows the formula of:</para>
		///     <para><see cref="MethodTable.BaseSize" /> - <see cref="EEClass.BaseSizePadding" /></para>
		/// <para>
		///         If a value is supplied, this manually reads the <c>MethodTable*</c>, making
		///         this work for boxed values.
		///     </para>
		/// <code>
		/// object o = 123;
		/// Unsafe.BaseFieldsSize(ref o);  // == 4 (boxed int, base fields size of int (sizeof(int)))
		/// Unsafe.BaseFieldsSize&lt;object&gt;; // == 0 (base fields size of object)
		/// </code>
		///     <remarks>
		/// <para>Only a type parameter is needed, or a value can be supplied</para>
		///         <para>Supply a value if the value may be boxed.</para>
		///         <para>Returned from <see cref="MethodTable.NumInstanceFieldBytes" /></para>
		///         <para>This includes field padding.</para>
		///     </remarks>
		/// </summary>
		/// <returns><see cref="Constants.MinObjectSize" /> if type is an array, fields size otherwise</returns>
		BaseFields,
		
		/// <summary>
		///     <para>Returns the base instance size according to the TypeHandle (<c>MethodTable</c>).</para>
		///     <para>This is the minimum heap size of a type.</para>
		///     <para>By default, this equals <see cref="Constants.MinObjectSize" /> (<c>24</c> (x64) or <c>12</c> (x84)).</para>
		/// </summary>
		/// <remarks>
		/// <para>Only a type parameter is needed, or a value can be supplied</para>
		///     <para>Returned from <see cref="MethodTable.BaseSize" /></para>
		/// </remarks>
		/// <returns>
		///     <see cref="MethodTable.BaseSize" />
		/// </returns>
		BaseInstance,
		
		/// <summary>
		///     <para>Calculates the complete size of a reference type in heap memory.</para>
		///     <para>This is the most accurate size calculation.</para>
		///     <para>
		///         This follows the size formula of: (<see cref="MethodTable.BaseSize" />) + (length) *
		///         (<see cref="MethodTable.ComponentSize" />)
		///     </para>
		///     <para>where:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>
		///                 <see cref="MethodTable.BaseSize" /> = The base instance size of a type
		///                 (<c>24</c> (x64) or <c>12</c> (x86) by default) (<see cref="Constants.MinObjectSize" />)
		///             </description>
		///         </item>
		///         <item>
		///             <description>length	= array or string length; <c>1</c> otherwise</description>
		///         </item>
		///         <item>
		///             <description><see cref="MethodTable.ComponentSize" /> = element size, if available; <c>0</c> otherwise</description>
		///         </item>
		///     </list>
		/// </summary>
		/// <remarks>
		/// <para>A value must be supplied.</para>
		///     <para>Source: /src/vm/object.inl: 45</para>
		///     <para>Equals the Son Of Strike "!do" command.</para>
		///     <para>Equals <see cref="BaseInstance" /> for objects that aren't arrays or strings.</para>
		///     <para>Note: This also includes padding and overhead (<see cref="ObjHeader" /> and <see cref="MethodTable" /> ptr.)</para>
		/// </remarks>
		/// <returns>The size of the type in heap memory, in bytes</returns>
		Heap,
		
		
		
		/// <summary>
		///     Calculates the complete size of the value's data. If the type parameter is
		///     a value type, this is equal to option <see cref="Intrinsic"/>. If the type parameter is a
		///     reference type, this is equal to <see cref="Heap"/>.
		/// </summary>
		Auto,
		
		
	}
}