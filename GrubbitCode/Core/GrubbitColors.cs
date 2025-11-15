using UnityEngine;

namespace Grubbit
{
	public static class GrubbitColors
	{
		public enum ColorType
		{
			White,
			Black,
			Orange,
			Green,
			Red,
			Blue,
			Grey,
			DarkGrey
		}

		public static Color ReturnColorByColorType(ColorType desiredColorType)
		{
			switch (desiredColorType)
			{
				default:
					return White;
				case ColorType.Black:
					return Black;
				case ColorType.Orange:
					return Orange;
				case ColorType.Green:
					return Green;
				case ColorType.Red:
					return Red;
				case ColorType.Blue:
					return Blue;
				case ColorType.Grey:
					return Grey;
				case ColorType.DarkGrey:
					return DarkGrey;
			}
		}

		public static string ReturnColorRawByColorType(ColorType desiredColorType)
		{
			switch (desiredColorType)
			{
				default:
					return WhiteRaw;
				case ColorType.Black:
					return BlackRaw;
				case ColorType.Orange:
					return OrangeRaw;
				case ColorType.Green:
					return GreenRaw;
				case ColorType.Red:
					return RedRaw;
				case ColorType.Blue:
					return BlueRaw;
				case ColorType.Grey:
					return GreyRaw;
				case ColorType.DarkGrey:
					return DarkGreyRaw;
			}
		}

		public static Color White => Utility.GetColorFromString(WhiteRaw);
		public static Color Black => Utility.GetColorFromString(BlackRaw);
		public static Color Orange => Utility.GetColorFromString(OrangeRaw);
		public static Color Green => Utility.GetColorFromString(GreenRaw);
		public static Color Red => Utility.GetColorFromString(RedRaw);
		public static Color Blue => Utility.GetColorFromString(BlueRaw);
		public static Color Grey => Utility.GetColorFromString(GreyRaw);
		public static Color DarkGrey => Utility.GetColorFromString(DarkGreyRaw);

		public static string WhiteRaw => "FFFFFFFF";
		public static string BlackRaw => "000000FF";
		public static string OrangeRaw => "DC4E37FF";
		public static string GreenRaw => "39B44AFF";
		public static string RedRaw => "C8102EFF";
		public static string BlueRaw => "00ABFFFF";
		public static string GreyRaw => "5E5F5EFF";
		public static string DarkGreyRaw => "333333FF";
	}
}