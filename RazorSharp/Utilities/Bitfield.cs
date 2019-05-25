using System;
using System.Reflection;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Utilities
{
	public static class Bitfield
	{
		public static int GetValue<T>(T value, string fieldName, string bitfieldName)
		{
			var type     = value.GetType();
			var field    = type.GetAnyField(fieldName);
			var attrType = typeof(BitfieldAttribute);

			Conditions.Require(Attribute.IsDefined(field, attrType));

			if (field.FieldType != typeof(int) && field.FieldType != typeof(uint)) {
				throw new InvalidOperationException("Field must be of type int");
			}

			var attrValue  = (BitfieldAttribute[]) field.GetCustomAttributes(attrType);
			var fieldValue = field.GetValue(value);

			int bitPos = 0;

			foreach (var attribute in attrValue) {
				if (bitPos >= Constants.BITS_PER_DWORD) {
					throw new OverflowException("Bit count exceeded the bit size of int");
				}

				if (attribute.Name == bitfieldName) {
					if (field.FieldType == typeof(int)) {
						return Bits.ReadBits((int) fieldValue, attribute.Count, bitPos);
					}

					if (field.FieldType == typeof(uint)) {
						return Bits.ReadBits((uint) fieldValue, attribute.Count, bitPos);
					}
				}

				bitPos += attribute.Count;
			}


			return default;
		}
	}
}