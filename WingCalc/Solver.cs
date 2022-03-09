namespace WingCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WingCalc.Exceptions;
using WingCalc.Nodes;

public class Solver
{
	private readonly Dictionary<string, double> _variables = new(StringComparer.OrdinalIgnoreCase)
	{
		["PI"] = Math.PI,
		["TAU"] = Math.Tau,
		["E"] = Math.E,

		["BYTEMIN"] = byte.MinValue,
		["BYTEMAX"] = byte.MaxValue,
		["SBYTEMIN"] = sbyte.MinValue,
		["SBYTEMAX"] = sbyte.MaxValue,
		["SHORTMIN"] = short.MinValue,
		["SHORTMAX"] = short.MaxValue,
		["USHORTMIN"] = ushort.MinValue,
		["USHORTMAX"] = ushort.MaxValue,
		["INTMIN"] = int.MinValue,
		["INTMAX"] = int.MaxValue,
		["UINTMIN"] = uint.MinValue,
		["UINTMAX"] = uint.MaxValue,
		["LONGMIN"] = long.MinValue,
		["LONGMAX"] = long.MaxValue,
		["ULONGMIN"] = ulong.MinValue,
		["ULONGMAX"] = ulong.MaxValue,

		["DOUBLEMIN"] = double.MinValue,
		["DOUBLEMAX"] = double.MaxValue,
		["INFINITY"] = double.PositiveInfinity,
		["EPSILON"] = double.Epsilon,
		["NAN"] = double.NaN,

		["NL"] = -1,
		["DEC"] = 0,
		["TXT"] = 1,
		["BIN"] = 2,
		["PCT"] = 3,
		["FRAC"] = 4,
		["EXACT"] = 5,
		["OCT"] = 8,
		["HEX"] = 16,

		["ANS"] = 0,

		["ඞ"] = 1337,
	};

	private readonly Dictionary<string, Macro> _macros = new(StringComparer.OrdinalIgnoreCase);
	private readonly Stack<List<string>> _localNameStack = new();

	public Action<string> WriteLine { get; set; } = Console.WriteLine;
	public Action<string> WriteError { get; set; } = Console.WriteLine;
	public Action<string> Write { get; set; } = Console.Write;
	public Func<string> ReadLine { get; set; } = Console.ReadLine;
	public Action Flush { get; set; } = Console.Clear;
	public Action Clear { get; set; } = Console.Clear;

	public double Solve(string s, bool setAns = true) => Solve(s, out _, setAns);

	public double Solve(string s, out bool impliedAns, bool setAns = true)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			impliedAns = false;
			return 0;
		}

		var tokens = Tokenizer.Tokenize(s).ToArray();

		_localNameStack.Clear();
		_localNameStack.Push(new());
		LocalList localScope = new();

		INode node = CreateTree(tokens, out impliedAns, mayImplyAns: true, topLevel: true);

		Scope scope = new(localScope, null, this, "Main");

		double solve = node.Solve(scope);

		if (setAns) SetVariable("ANS", solve);

		return solve;
	}

	private INode CreateTree(Span<Token> tokens, bool mayImplyAns = false, bool topLevel = false) => CreateTree(tokens, out _, mayImplyAns, topLevel);
	private INode CreateTree(Span<Token> tokens, out bool impliedAns, bool mayImplyAns = false, bool topLevel = false)
	{
		List<INode> availableNodes = new();
		bool isCoefficient = false;
		impliedAns = false;

		for (int i = 0; i < tokens.Length; i++)
		{
			switch (tokens[i].TokenType)
			{
				case TokenType.Number:
				{
					try
					{
						availableNodes.Add(new ConstantNode(double.Parse(tokens[i].Text)));
					}
					catch
					{
						throw new WingCalcException($"Unable to parse constant {tokens[i].Text}.");
					}

					isCoefficient = true;
					break;
				}
				case TokenType.Operator:
				{
					availableNodes.Add(new PreOperatorNode(tokens[i].Text));
					isCoefficient = false;

					if (Operators.IsBinary(tokens[i].Text) && Operators.GetPrecedence(tokens[i].Text) == Operators.GetPrecedence("=") && availableNodes[^2] is MacroNode mn) // macro assignment
					{
						_localNameStack.Push(new());
						int end = GetEnd(tokens, i + 1);

						foreach (var alias in mn.GetAliases())
						{
							_localNameStack.Peek().Add(alias);
						}

						INode tree = CreateTree(tokens[(i + 1)..end], topLevel: false);
						
						_localNameStack.Pop();

						i = end - 1;
						availableNodes.Add(tree);

						int GetEnd(Span<Token> tokens, int i)
						{
							int open = 0;
							for (int j = i; j < tokens.Length; j++)
							{
								switch (tokens[j].TokenType)
								{
									case TokenType.OpenParen:
									{
										open++;
										break;
									}
									case TokenType.CloseParen:
									{
										open--;
										break;
									}
									case TokenType.Operator:
									{
										if (Operators.IsBinary(tokens[j].Text) && open == 0)
										{
											int precedence = Operators.GetPrecedence(tokens[j].Text);
											int assignmentPrecedence = Operators.GetPrecedence("=");

											if (precedence > assignmentPrecedence)
											{
												return j;
											}
										}

										break;
									}
								}
							}

							return tokens.Length;
						}
					}

					break;
				}
				case TokenType.Name:
				{
					switch (GetNameType(tokens, i, topLevel, out int startIndex))
					{
						case NameType.Function:
						{
							if (isCoefficient)
							{
								if (tokens[i - 1].TokenType is TokenType.Hex or TokenType.Roman)
								{
									throw new WingCalcException($"Tokens of type {tokens[i - 1].TokenType} may not serve as function coefficients. Try adding parentheses around the function call.");
								}

								availableNodes.Add(new PreOperatorNode("coeff"));
							}

							if (i == tokens.Length - 1 || tokens[i + 1].TokenType != TokenType.OpenParen) throw new WingCalcException($"Function {tokens[i].Text} called but no opening bracket found.");

							int end = FindClosing(i + 1, tokens);

							availableNodes.Add(new FunctionNode(tokens[i].Text, CreateParams(tokens[(i + 2)..end], out int newPushed)));
							
							i = end;
							isCoefficient = true;
							break;
						}
						case NameType.Variable:
						{
							if (isCoefficient)
							{
								availableNodes.Add(new PreOperatorNode("coeff"));
							}

							if (tokens[i].Text == "$")
							{
								availableNodes.Add(new PreOperatorNode("$"));
								isCoefficient = false;
							}
							else
							{
								availableNodes.Add(new VariableNode(tokens[i].Text[startIndex..]));
								isCoefficient = true;
							}

							break;
						}
						case NameType.Macro:
						{
							if (isCoefficient)
							{
								availableNodes.Add(new PreOperatorNode("coeff"));
							}

							if (tokens[i].Text == "@")
							{
								if (i == tokens.Length - 1 || tokens[i + 1].TokenType != TokenType.OpenParen)
								{
									throw new WingCalcException("'@' found without a macro name and without an opening parenthesis.");
								}
								else
								{
									int end = FindClosing(i + 1, tokens);
									availableNodes.Add(new LambdaNode(CreateTree(tokens[(i + 2)..end])));
																		isCoefficient = true;

									i = end;
								}
							}
							else
							{
								if (i == tokens.Length - 1 || tokens[i + 1].TokenType != TokenType.OpenParen)
								{
									availableNodes.Add(new MacroNode(tokens[i].Text[startIndex..], new(), true));
									isCoefficient = false;
								}
								else
								{
									int end = FindClosing(i + 1, tokens);

									availableNodes.Add(new MacroNode(tokens[i].Text[startIndex..], CreateParams(tokens[(i + 2)..end], out int newPushed), true));
																		isCoefficient = true;

									i = end;
								}
							}

							break;
						}
						case NameType.Local:
						{
							if (isCoefficient)
							{
								availableNodes.Add(new PreOperatorNode("coeff"));
							}

							if (tokens[i].Text == "#")
							{
								availableNodes.Add(new PreOperatorNode("#"));
								isCoefficient = false;
							}
							else
							{
								_localNameStack.Peek().Add(tokens[i].Text[startIndex..]);
								availableNodes.Add(new LocalNode(tokens[i].Text[startIndex..]));
								isCoefficient = true;
							}

							break;
						}
					}

					break;
				}
				case TokenType.Hex:
				{
					if (tokens[i].Text.Length <= 1) throw new WingCalcException("Hex literals cannot be empty.");

					availableNodes.Add(new ConstantNode(Convert.ToInt32(tokens[i].Text[1..], 16)));
					isCoefficient = true;
					break;
				}
				case TokenType.OpenParen:
				{
					if (isCoefficient)
					{
						availableNodes.Add(new PreOperatorNode("coeff"));
					}

					int end = FindClosing(i, tokens);

					INode tree = CreateTree(tokens[(i + 1)..end], topLevel: topLevel);
					
					availableNodes.Add(tree);

					if (tree is null) throw new WingCalcException($"Empty brackets {tokens[i].Text}{tokens[end].Text} found.");

					i = end;
					isCoefficient = true;
					break;
				}
				case TokenType.CloseParen:
				{
					throw new WingCalcException($"Unexpected character '{tokens[i].Text}' found.");
				}
				case TokenType.Comma:
				{
					throw new WingCalcException($"Unexpected character '{tokens[i].Text}' found.");
				}
				case TokenType.Quote:
				{
					availableNodes.Add(new QuoteNode(Regex.Unescape(tokens[i].Text)));
					isCoefficient = true;
					break;
				}
				case TokenType.Char:
				{
					string unescaped = Regex.Unescape(tokens[i].Text);
					if (unescaped.Length > 1) throw new WingCalcException($"Character '{tokens[i].Text}' could not be resolved: too many characters found in character literal.");
					availableNodes.Add(new ConstantNode(unescaped[0]));
					isCoefficient = true;
					break;
				}
				case TokenType.Binary:
				{
					if (tokens[i].Text.Length <= 1) throw new WingCalcException("Binary literals cannot be empty.");

					availableNodes.Add(new ConstantNode(Convert.ToInt32(tokens[i].Text, 2)));
					isCoefficient = true;
					break;
				}
				case TokenType.Octal:
				{
					if (tokens[i].Text.Length <= 1) throw new WingCalcException("Octal literals cannot be empty.");

					availableNodes.Add(new ConstantNode(Convert.ToInt32(tokens[i].Text, 8)));
					isCoefficient = true;
					break;
				}
				case TokenType.Roman:
				{
					if (tokens[i].Text.Length <= 1) throw new WingCalcException("Roman numeral literals cannot be empty.");

					availableNodes.Add(new ConstantNode(RomanNumeralConverter.GetValue(tokens[i].Text[1..])));
					isCoefficient = true;
					break;
				}
				default:
				{
					throw new NotImplementedException($"Token type {tokens[i].TokenType} is not implemented.");
				}
			}
		}

		if (availableNodes.Count == 0) return null;
		if (mayImplyAns && availableNodes[0] is PreOperatorNode firstNode && firstNode.Text.Length >= 2)
		{
			impliedAns = true;
			availableNodes.Insert(0, new VariableNode("ANS")); // add $ANS at when start with binary operator
		}

		if (availableNodes.Count > 0 && availableNodes[^1] is PreOperatorNode semiNode && semiNode.Text == ";") availableNodes.RemoveAt(availableNodes.Count - 1); // remove trailing semicolons

		for (int i = availableNodes.Count - 1; i >= 1; i--) // handle unary operators
		{
			if (availableNodes[i] is not PreOperatorNode
				&& availableNodes[i - 1] is PreOperatorNode signNode
				&& (i == 1 || availableNodes[i - 2] is PreOperatorNode))
			{
				INode numberNode = availableNodes[i];

				if (Tokenizer._unaryOperators.Contains(signNode.Text))
				{
					availableNodes.RemoveAt(i - 1);
					switch (signNode.Text)
					{
						case "+":
						{
							break;
						}
						case "-":
						{
							availableNodes.RemoveAt(i - 1);
							availableNodes.Insert(i - 1, new ConstantNode(-1));
							availableNodes.Insert(i, new PreOperatorNode("coeff"));
							availableNodes.Insert(i + 1, numberNode);
							break;
						}
						case "$":
						{
							availableNodes.RemoveAt(i - 1);
							availableNodes.Insert(i - 1, new PointerNode(numberNode));
							break;
						}
						case "!":
						{
							availableNodes.RemoveAt(i - 1);
							availableNodes.Insert(i - 1, new UnaryNode(numberNode, x => x == 0 ? 1 : 0));
							break;
						}
						case "~":
						{
							availableNodes.RemoveAt(i - 1);
							availableNodes.Insert(i - 1, new UnaryNode(numberNode, x => ~(int)x));
							break;
						}
						case "#":
						{
							availableNodes.RemoveAt(i - 1);
							LocalPointerNode local = new(numberNode);
							availableNodes.Insert(i - 1, local);
							break;
						}
						default:
						{
							throw new WingCalcException($"\"{signNode.Text}\" is a valid unary operator but is not yet implemented.");
						}
					}
				}
				else throw new WingCalcException($"\"{signNode.Text}\" is not a valid unary operator.");
			}
		}

		while (true)
		{
			var preOperatorNodes = from x in availableNodes where x is PreOperatorNode select x as PreOperatorNode;

			if (!preOperatorNodes.Any()) break;
			else
			{
				int tier = preOperatorNodes.Min(x => x.Tier);

				switch (Operators.GetTierAssociativity(tier))
				{
					case Operators.Associativity.Left:
					{
						for (int i = 0; i < availableNodes.Count; i++)
						{
							CheckAndCollapseNode(ref i, x => --x);
						}

						break;
					}
					case Operators.Associativity.Right:
					{
						for (int i = availableNodes.Count - 1; i >= 0; i--)
						{
							CheckAndCollapseNode(ref i, x => ++x);
						}

						break;
					}
				}

				void CheckAndCollapseNode(ref int i, Func<int, int> increment)
				{
					if (i < 0 || i >= availableNodes.Count) return;

					if (availableNodes[i] is PreOperatorNode node && node.Tier == tier)
					{
						if (i == 0 || availableNodes[i - 1] is null or PreOperatorNode)
						{
							throw new WingCalcException($"Operator {node.Text} is missing a left-hand operand.");
						}
						else if (i == availableNodes.Count - 1 || availableNodes[i + 1] is null or PreOperatorNode)
						{
							throw new WingCalcException($"Operator {node.Text} is missing a right-hand operand.");
						}

						var binaryNode = Operators.CreateNode(availableNodes[i - 1], node, availableNodes[i + 1]);

						availableNodes.RemoveAt(i - 1);
						availableNodes.RemoveAt(i - 1);
						availableNodes.RemoveAt(i - 1);

						availableNodes.Insert(i - 1, binaryNode);

						i = increment(i);
					}
				}
			}
		}

		if (availableNodes.Count > 1)
		{
			throw new WingCalcException($"Extra node {availableNodes[1]} found, expression tree could not be made.");
		}
		else return availableNodes.First();
	}

	private LocalList CreateParams(Span<Token> tokens, out int pushed)
	{
		List<INode> nodes = new();
		int next = 0;
		pushed = 0;

		int level = 1;
		for (int i = 0; i < tokens.Length; i++)
		{
			switch (tokens[i].TokenType)
			{
				case TokenType.OpenParen:
				{
					level++;
					break;
				}
				case TokenType.CloseParen:
				{
					level--;
					break;
				}
				case TokenType.Comma:
				{
					if (level == 1)
					{
						Span<Token> treeSpan = tokens[next..i];
						if (treeSpan.Length < 1) throw new WingCalcException("Empty parameters are not allowed.");
						nodes.Add(CreateTree(treeSpan));
						next = i == tokens.Length - 1
							? -1
							: i + 1;
					}

					break;
				}
			}
		}

		if (next != -1)
		{
			nodes.Add(CreateTree(tokens[next..tokens.Length]));
		}

		return new(nodes);
	}

	private int FindClosing(int start, Span<Token> tokens)
	{
		int level = 0;
		for (int i = start; i < tokens.Length; i++)
		{
			switch (tokens[i].TokenType)
			{
				case TokenType.OpenParen:
				{
					level++;
					break;
				}
				case TokenType.CloseParen:
				{
					level--;

					if (level == 0)
					{
						if (_matches[tokens[start].Text[0]] == tokens[i].Text[0]) return i;
						else throw new WingCalcException($"Closing bracket {tokens[i].Text[0]} does not match opening bracket {tokens[start].Text[0]}");
					}

					break;
				}
			}
		}

		throw new WingCalcException($"Closing bracket for {tokens[start].Text} expected but not found.");
	}

	private readonly Dictionary<char, char> _matches = new() { ['('] = ')', ['['] = ']', ['{'] = '}' };

	public double GetVariable(string s)
	{
		if (!_variables.ContainsKey(s)) _variables.Add(s, 0);

		return _variables[s];
	}

	public double SetVariable(string s, double x)
	{
		if (_variables.ContainsKey(s)) _variables[s] = x;
		else _variables.Add(s, x);

		return x;
	}

	public double[] GetArray(double x) => ListHandler.Enumerate(new PointerNode(new ConstantNode(x)), new(null, null, this, "Out")).ToArray();

	public string GetString(double x) => new(ListHandler.Enumerate(new PointerNode(new ConstantNode(x)), new(null, null, this, "Out")).Select(x => (char)x).ToArray());

	internal Macro GetMacro(string s)
	{
		if (!_macros.ContainsKey(s)) throw new WingCalcException($"Macro {s} does not exist.");

		return _macros[s];
	}

	internal double SetMacro(string s, Macro x)
	{
		if (_macros.ContainsKey(s)) _macros[s] = x;
		else _macros.Add(s, x);

		return 1;
	}

	internal bool MacroExists(string s) => _macros.ContainsKey(s);

	internal record Macro(INode Node, List<string> Aliases);

	public IEnumerable<(string, double)> GetValues()
	{
		foreach (var kvp in _variables)
		{
			yield return (kvp.Key, kvp.Value);
		}
	}

	private NameType GetNameType(Span<Token> tokens, int i, bool topLevel, out int startIndex)
	{
		bool nextParen = i < tokens.Length - 1 && tokens[i + 1].TokenType == TokenType.OpenParen;
		startIndex = 1;

		string s = tokens[i].Text;

		return s[0] switch
		{
			'@' => NameType.Macro,
			'$' => NameType.Variable,
			'#' => NameType.Local,
			_ => Else(tokens, s, out startIndex)
		};

		NameType Else(Span<Token> tokens, string s, out int startIndex)
		{
			startIndex = 0;

			if (nextParen)
			{
				if (Functions.Exists(s)) return NameType.Function;
				if (MacroExists(s)) return NameType.Macro;
				if (IsAssignment(tokens, i)) return NameType.Macro;
			}

			if (topLevel) return NameType.Variable;
			else if (IsAssignment(tokens, i) || _localNameStack.Peek().Contains(s, StringComparer.OrdinalIgnoreCase)) return NameType.Local;
			else return NameType.Variable;
		}

		bool IsAssignment(Span<Token> tokens, int i)
		{
			int open = 0;
			for (int j = i + 1; j < tokens.Length; j++)
			{
				switch (tokens[j].TokenType)
				{
					case TokenType.OpenParen:
					{
						open++;
						break;
					}
					case TokenType.CloseParen:
					{
						open--;
						break;
					}
					case TokenType.Operator:
					{
						if (open == 0)
						{
							int precedence = Operators.GetPrecedence(tokens[j].Text);
							int assignmentPrecedence = Operators.GetPrecedence("=");

							if (precedence == assignmentPrecedence)
							{
								return true;
							}
							else if (precedence > assignmentPrecedence)
							{
								return false;
							}
						}

						break;
					}
				}
			}

			return false;
		}
	}

	enum NameType { Variable, Macro, Local, Function }
}
