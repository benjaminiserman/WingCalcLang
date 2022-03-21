namespace WingCalc.Nodes;

internal record LambdaNode(INode Node, List<string> Aliases) : INode, ICallable
{
	public double Solve(Scope scope) => Node.Solve(new(new(), scope, scope.Solver, $"Lambda"));

	public double Call(Scope scope, LocalList list)
	{
		for (double i = 0; i < Aliases.Count; i++)
		{
			list.Set(Aliases[(int)i], list[i.ToString(), scope]);
		}

		return Node.Solve(new(list, scope, scope.Solver, $"Lambda"));
	}
}
