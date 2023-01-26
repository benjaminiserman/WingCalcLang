namespace WingCalc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class FunStringsHelper
{
	public static char GetSmallCap(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "ᴀʙᴄᴅᴇꜰɢʜɪᴊᴋʟᴍɴᴏᴘǫʀsᴛᴜᴠᴡxʏᴢ"[c - 'a'];
		}
		else
		{
			return c;
		}
	}

	public static char Bubble(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "ⓐⓑⓒⓓⓔⓕⓖⓗⓘⓙⓚⓛⓜⓝⓞⓟⓠⓡⓢⓣⓤⓥⓦⓧⓨⓩ"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "ⒶⒷⒸⒹⒺⒻⒼⒽⒿⒾⓀⓁⓂⓃⓄⓅⓆⓇⓈⓉⓊⓋⓌⓍⓎⓏ"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char BlackBubble(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "🅐🅑🅒🅓🅔🅕🅖🅗🅘🅙🅚🅛🅜🅝🅞🅟🅠🅡🅢🅣🅤🅥🅦🅧🅨🅩"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "🅐🅑🅒🅓🅔🅕🅖🅗🅘🅙🅚🅛🅜🅝🅞🅟🅠🅡🅢🅣🅤🅥🅦🅧🅨🅩"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char Square(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "🄰🄱🄲🄳🄴🄵🄶🄷🄸🄹🄺🄻🄼🄽🄾🄿🅀🅁🅂🅃🅄🅅🅆🅇🅈🅉"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "🄰🄱🄲🄳🄴🄵🄶🄷🄸🄹🄺🄻🄼🄽🄾🄿🅀🅁🅂🅃🅄🅅🅆🅇🅈🅉"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char Cursive(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "𝒶𝒷𝒸𝒹ℯ𝒻ℊ𝒽𝒾𝒿𝓀𝓁𝓂𝓃ℴ𝓅𝓆𝓇𝓈𝓉𝓊𝓋𝓌𝓍𝓎𝓏"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "𝒜ℬ𝒞𝒟ℰℱ𝒢ℋ𝒥ℐ𝒦ℒℳ𝒩𝒪𝒫𝒬ℛ𝒮𝒯𝒰𝒱𝒲𝒳𝒴𝒵"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char Outline(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "𝕒𝕓𝕔𝕕𝕖𝕗𝕘𝕙𝕚𝕛𝕜𝕝𝕞𝕟𝕠𝕡𝕢𝕣𝕤𝕥𝕦𝕧𝕨𝕩𝕪𝕫"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "𝔸𝔹ℂ𝔻𝔼𝔽𝔾ℍ𝕁𝕀𝕂𝕃𝕄ℕ𝕆ℙℚℝ𝕊𝕋𝕌𝕍𝕎𝕏𝕐ℤ"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char Fraktur(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "𝔞𝔟𝔠𝔡𝔢𝔣𝔤𝔥𝔦𝔧𝔨𝔩𝔪𝔫𝔬𝔭𝔮𝔯𝔰𝔱𝔲𝔳𝔴𝔵𝔶𝔷"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "𝔄𝔅ℭ𝔇𝔈𝔉𝔊ℌ𝔍ℑ𝔎𝔏𝔐𝔑𝔒𝔓𝔔ℜ𝔖𝔗𝔘𝔙𝔚𝔛𝔜ℨ"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char Monospace(char c)
	{
		if (c is >= 'a' and <= 'z')
		{
			return "𝚊𝚋𝚌𝚍𝚎𝚏𝚐𝚑𝚒𝚓𝚔𝚕𝚖𝚗𝚘𝚙𝚚𝚛𝚜𝚝𝚞𝚟𝚠𝚡𝚢𝚣"[c - 'a'];
		}
		else if (c is >= 'A' and <= 'Z')
		{
			return "𝙰𝙱𝙲𝙳𝙴𝙵𝙶𝙷𝙹𝙸𝙺𝙻𝙼𝙽𝙾𝙿𝚀𝚁𝚂𝚃𝚄𝚅𝚆𝚇𝚈𝚉"[c - 'A'];
		}
		else
		{
			return c;
		}
	}

	public static char NameToSymbol(string s)
	{
		return s switch
		{
			"alpha" => 'α',
			"Alpha" => 'A',
			"beta" => 'β',
			"Beta" => 'B',
			"gamma" => 'γ',
			"Gamma" => 'Γ',
			"delta" => 'δ',
			"Delta" => 'Δ',
			"epsilon" => 'ϵ',
			"Epsilon" => 'E',
			"zeta" => 'ζ',
			"Zeta" => 'Z',
			"eta" => 'η',
			"Eta" => 'H',
			"theta" => 'θ',
			"Theta" => 'Θ',
			"iota" => 'ι',
			"Iota" => 'I',
			"kappa" => 'κ',
			"Kappa" => 'K',
			"lambda" => 'λ',
			"Lambda" => 'Λ',
			"mu" => 'μ',
			"Mu" => 'M',
			"nu" => 'ν',
			"Nu" => 'N',
			"xi" => 'ξ',
			"Xi" => 'Ξ',
			"omicron" => 'o',
			"Omicron" => 'O',
			"pi" => 'π',
			"Pi" => 'Π',
			"rho" => 'ρ',
			"Rho" => 'P',
			"sigma" => 'σ',
			"Sigma" => 'Σ',
			"tau" => 'τ',
			"Tau" => 'T',
			"upsilon" => 'υ',
			"Upsilon" => 'ϒ',
			"phi" => 'ϕ',
			"Phi" => 'Φ',
			"chi" => 'χ',
			"Chi" => 'X',
			"psi" => 'ψ',
			"Psi" => 'Ψ',
			"omega" => 'ω',
			"Omega" => 'Ω',

			_ => '?'
		};

		public static Func<List<INode>, Scope, double> GetFunction(Func<char, char> transform)
		{
			return (args, scope) =>
			{
				var s = ListHandler.PointerOrString(args[0], scope);

				var output = string.Join(string.Empty, s.Select(c => transform(c)));

				scope.Solve.WriteLine(output);

				return 1;
			}
		}
	}
}
