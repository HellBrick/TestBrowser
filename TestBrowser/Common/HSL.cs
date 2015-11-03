using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace HellBrick.TestBrowser.Common
{
	//[TypeConverter]
	public class HSL : MarkupExtension
	{
		public HSL()
		{
		}

		private double _hue = 0.0;
		public double Hue
		{
			get { return _hue; }
			set { _hue = value % 360.0; }
		}

		private double _saturation = 0.0;
		public double Saturation
		{
			get { return _saturation; }
			set { _saturation = ClamBetweenZeroAndOne( value ); }
		}

		private double _lightness = 0.0;
		public double Lightness
		{
			get { return _lightness; }
			set { _lightness = ClamBetweenZeroAndOne( value ); }
		}

		private double _alpha = 1.0;
		public double Alpha
		{
			get { return _alpha; }
			set { _alpha = ClamBetweenZeroAndOne( value ); }
		}

		private static double ClamBetweenZeroAndOne( double value )
		{
			return Math.Min( 1.0, Math.Max( 0.0, value ) );
		}

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var result = HslToRgb( Hue, Saturation, Lightness );
			return Color.FromArgb( (byte) ( Alpha * 255.0 ), (byte) ( result.Red * 255.0 ), (byte) ( result.Green * 255.0 ), (byte) ( result.Blue * 255.0 ) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hue">Hue ∈ [0°, 360°)</param>
		/// <param name="saturation">Saturation ∈ [0, 1]</param>
		/// <param name="lightness">Lightness ∈ [0, 1]</param>
		/// <returns></returns>
		private NormalizedColor HslToRgb( double hue, double saturation, double lightness )
		{
			double chroma = saturation * ( 1.0 - Math.Abs( 2.0 * lightness - 1.0 ) );
			double hueSector = hue / 60.0;
			double secondLargestColor = chroma * ( 1.0 - Math.Abs( hueSector % 2 - 1 ) );

			double minColor = lightness - chroma / 2.0;
			double maxColor = chroma + minColor;
			double middleColor = secondLargestColor + minColor;

			switch ( (int) Math.Floor( hueSector ) )
			{
				case 0: //	R -> G -> B
					return new NormalizedColor( red: maxColor, green: middleColor, blue: minColor );

				case 1: //	G -> R -> B
					return new NormalizedColor( green: maxColor, red: middleColor, blue: minColor );

				case 2: //	G -> B -> R
					return new NormalizedColor( green: maxColor, blue: middleColor, red: minColor );

				case 3: //	B -> G -> R
					return new NormalizedColor( blue: maxColor, green: middleColor, red: minColor );

				case 4: //	B -> R -> G
					return new NormalizedColor( blue: maxColor, red: middleColor, green: minColor );

				case 5: //	R -> B -> G
					return new NormalizedColor( red: maxColor, blue: middleColor, green: minColor );

				default:
					return new NormalizedColor();
			}
		}

		private struct NormalizedColor
		{
			public NormalizedColor( double red, double green, double blue )
			{
				Red = red;
				Green = green;
				Blue = blue;
			}

			public double Red { get; }
			public double Green { get; }
			public double Blue { get; }
		}
	}
}