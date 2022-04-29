namespace WingCalc;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

internal class ConwayGuySystem
{
	private static readonly string[] _shortScales = new string[]
	{
		"",
		"thousand",
		"mill",
		"bill",
		"trill",
		"quadrill",
		"quintill",
		"sextill",
		"septill",
		"octill",
		"nonill",
	};

	private static readonly string[] _units = new string[]
	{
		"", "un", "duo", "tre", "quattuor", "quinqua", "se", "septe", "octo", "nove"
	};

	private static readonly string[] _tens = new string[]
	{
		"", "deci", "viginti", "triginta", "quadraginta", "quinquaginta", "sexaginta", "septuaginta", "octoginta", "nonaginta"
	};

	private static readonly string[] _hundreds = new string[]
	{
		"", "centi", "ducenti", "trecenti", "quadringenti", "quingenti", "sescenti", "septingenti", "octingenti", "nongenti"
	};

	private static readonly int[] _tenN = { 1, 3, 4, 5, 6, 7 };
	private static readonly int[] _tenM = { 2, 8 };
	private static readonly int[] _tenS = { 2, 3, 4, 5 };
	private static readonly int[] _tenX = { 8 };

	private static readonly int[] _hunN = { 1, 2, 3, 4, 5, 6, 7 };
	private static readonly int[] _hunM = { 8 };
	private static readonly int[] _hunS = { 3, 4, 5 };
	private static readonly int[] _hunX = { 1, 8 };

	public static string GetShortScale(BigInteger x)
	{
		if (x == 1) return "thousand";

		x -= 1;
		StringBuilder sb = new();
		int exponent = ((int)Math.Floor(BigInteger.Log10(x)) + 1) / 3;

		for (int i = exponent; i >= 0; i--)
		{
			int hundred = (int)(x / BigInteger.Pow(1000, i) % 1000);

			if (hundred == 0)
			{
				if (i == exponent) continue;
				sb.Append("nilli");
			}
			else if (hundred <= 9)
			{
				sb.Append(_shortScales[hundred + 1]);
			}
			else
			{
				int u = hundred % 10;
				int t = hundred / 10 % 10;
				int h = hundred / 100;

				if (u != 0)
				{
					sb.Append(_units[u]);
					Morph(u, t, sb);
				}

				if (t != 0)
				{
					if (h == 0)
					{
						if (_tens[t][^1] == 'a')
						{
							sb.Append($"{_tens[t][..^1]}i");
						}
						else
						{
							sb.Append(_tens[t]);
						}
					}
					else
					{
						sb.Append(_tens[t]);
					}
				}

				if (h != 0)
				{
					sb.Append(_hundreds[h]);
				}

				sb.Append("lli");
			}
		}

		sb.Append("on");

		return sb.ToString();
	}

	private static void Morph(int u, int t, StringBuilder sb)
	{
		if (t != 0)
		{
			if (u is 3 && (_tenS.Contains(t) || _tenX.Contains(t)))
			{
				sb.Append('s');
			}
			else if (u is 6)
			{
				if (_tenS.Contains(t)) sb.Append('s');
				else if (_tenX.Contains(t)) sb.Append('x');
			}
			else if (u is 7 or 9)
			{
				if (_tenN.Contains(t)) sb.Append('n');
				else if (_tenM.Contains(t)) sb.Append('m');
			}
		}
	}
}