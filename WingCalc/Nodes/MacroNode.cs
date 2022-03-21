namespace WingCalc.Nodes;
using WingCalc.Exceptions;

internal record MacroNode(string Name, LocalList LocalList, bool Assignable) : INode, IAssignable, ICallable
{
	public double Solve(Scope scope)
	{
		Solver.Macro macro = scope.Solver.GetMacro(Name);

		for (double i = 0; i < macro.Aliases.Count; i++)
		{
			LocalList.Set(macro.Aliases[(int)i], LocalList[i.ToString(), scope]);
		}

		return macro.Node.Solve(new(LocalList, scope, scope.Solver, Name));
	}

	public double Assign(INode a, Scope scope)
	{
		if (Assignable)
		{
			return scope.Solver.SetMacro(Name, new(a, GetAliases()));
		}
		else
		{
			throw new WingCalcException("Macros with arguments cannot be assigned to.", scope);
		}
	}

	public double DeepAssign(INode a, Scope scope)
	{
		INode node = scope.Solver.GetMacro(Name).Node;

		if (node is IAssignable ia) return ia.DeepAssign(a, scope);
		else return Assign(a.GetAssign(scope), scope);
	}

	public double Call(Scope scope, LocalList list)
	{
		for (double i = 0; true; i++)
		{
			if (list.Contains(i.ToString()))
			{
				LocalList.Set(i.ToString(), list[i, scope]);
			}
			else break;
		}

		return Solve(scope);
	}

	public List<string> GetAliases()
	{
		List<string> aliases = new();
	
		for (double i = 0; LocalList.Contains(i.ToString()); i++)
		{
			aliases.Add(LocalList[i.ToString(), null] switch
			{
				VariableNode vn => vn.Name,
				LocalNode ln => ln.Name,
				MacroNode mn => mn.Name,
				_ => throw new WingCalcException($"{LocalList[i.ToString(), null].GetType().Name} is not a valid alias for a macro argument.")
			});
		}

		return aliases;
	}
}
