namespace WingCalc;
using WingCalc.Exceptions;
using WingCalc.Nodes;

internal static class BytesHelper
{
	internal static byte[] GetBytes<T>(T item) => item switch
	{
		int x => BitConverter.GetBytes(x),
		float x => BitConverter.GetBytes(x),
		double x => BitConverter.GetBytes(x),
		bool x => BitConverter.GetBytes(x),
		char x => BitConverter.GetBytes(x),
		_ => throw new WingCalcException($"Invalid type specified: {item.GetType()}")
	};

	internal static double DoByteConversion<T>(List<INode> args, Scope scope, Func<double, T> conversion)
	{
		if (args.Count == 0)
		{
			throw new WingCalcException("Byte Conversion function expected at least 1 argument.");
		}

		if (args[0] is IPointer pointer)
		{
			ListHandler.Allocate(pointer, GetBytes(conversion(args[1].Solve(scope)))
				.Select(x => (double)x)
				.ToList(), scope);
		}
		else
		{
			foreach (var b in GetBytes(conversion(args[0].Solve(scope))))
			{
				scope.Solver.Write($"{b} ");
			}
		}
			
		return args[0].Solve(scope);
	}

	internal static Functions.Function GetBytesFromDouble() => (args, scope) => DoByteConversion(args, scope, x => x);
	internal static Functions.Function GetBytesFromFloat() => (args, scope) => DoByteConversion(args, scope, x => (float)x);
	internal static Functions.Function GetBytesFromInt() => (args, scope) => DoByteConversion(args, scope, x => (int)x);
}
