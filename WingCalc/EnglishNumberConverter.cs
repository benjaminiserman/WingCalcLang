namespace WingCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

internal static class EnglishNumberConverter
{
	private static readonly Dictionary<double, string> _constants = new()
	{
		[0] = "zero",
		[double.PositiveInfinity] = "infinity",
		[double.NegativeInfinity] = "negative infinity",
		[double.NaN] = "not a number",
	};

	private static readonly string[] _definedWords = new string[]
	{
		"zero",
		"one",
		"two",
		"three",
		"four",
		"five",
		"six",
		"seven",
		"eight",
		"nine",
		"ten",
		"eleven",
		"twelve",
		"thirteen",
		"fourteen",
		"fifteen",
		"sixteen",
		"seventeen",
		"eighteen",
		"nineteen",
	};

	private static readonly string[] _definedTens = new string[]
	{
		"zero",
		"ten",
		"twenty",
		"thirty",
		"fourty",
		"fifty",
		"sixty",
		"seventy",
		"eighty",
		"ninety",
	};

	public static string English(double x, BigInteger integer, BigInteger fraction, int integerExponent, int fractionExponent)
	{
		if (_constants.ContainsKey(x))
		{
			return _constants[x];
		}

		return English(integer, fraction, integerExponent, fractionExponent, x < 0);
	}

	public static string English(BigInteger integer, BigInteger fraction, int integerExponent, int fractionExponent, bool negative)
	{
		StringBuilder sb = new();

		if (integer == 0 && fraction == 0) return "zero";

		if (negative)
		{
			sb.Append("negative ");
			integer *= -1;
		}

		for (int i = integerExponent / 3; i >= 0; i--)
		{
			int hundred = (int)(integer / BigInteger.Pow(1000, i) % 1000);

			if (hundred != 0)
			{
				if (hundred / 100 != 0)
				{
					sb.Append($"{_definedWords[hundred / 100]} hundred ");
				}

				if (hundred % 100 != 0)
				{
					if (hundred % 100 < 20)
					{
						sb.Append($"{_definedWords[hundred % 100]} ");
					}
					else
					{
						if (hundred % 100 / 10 != 0)
						{
							if (hundred % 10 == 0)
							{
								sb.Append($"{_definedTens[hundred % 100 / 10]} ");
							}
							else
							{
								sb.Append($"{_definedTens[hundred % 100 / 10]}-{_definedWords[hundred % 10]} ");
							}
						}
						else
						{
							sb.Append($"{_definedWords[hundred]} ");
						}
					}
				}

				if (i != 0)
				{
					sb.Append($"{ConwayGuySystem.GetShortScale(i)} ");
				}
			}
		}

		if (fraction != 0)
		{
			BigInteger denominator = BigInteger.Pow(10, fractionExponent);

			BigInteger gcd = Factorizer.GCD(fraction, denominator);
			fraction /= gcd;
			denominator /= gcd;

			if (integer != 0) sb.Append("and ");

			sb.Append($"{English(fraction, 0, (int)Math.Floor(BigInteger.Log10(fraction)), 0, false)} over {English(denominator, 0, (int)Math.Floor(BigInteger.Log10(denominator)), 0, false)}");
		}

		return sb.ToString().Trim();
	}
}
