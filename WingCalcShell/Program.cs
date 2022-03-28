using WingCalc;
using System.Text;

Solver solver = new();

Console.WriteLine("Enter an expression folowed by two empty lines:");

string lastEntry = string.Empty;

while (true)
{
	try
	{
		StringBuilder sb = new();
		int nl = 0;
		while (true)
		{
			string s = Console.ReadLine();
			sb.AppendLine(s);
			if (string.IsNullOrWhiteSpace(s))
			{
				nl++;
				if (nl == 2)
				{
					break;
				}
			}
			else nl = 0;
		}

		string entry = sb.ToString();
		if (string.IsNullOrWhiteSpace(entry)) entry = lastEntry;

		double ans = solver.Solve(entry);

		lastEntry = entry;
		solver.SetVariable("ANS", ans);

		Console.WriteLine($"> Solution: {ans}");
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
	}
}