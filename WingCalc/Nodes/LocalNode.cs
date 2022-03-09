namespace WingCalc.Nodes;
using WingCalc.Exceptions;

internal record LocalNode(string Name) : INode, IAssignable, IPointer, ILocal, ICallable
{
	public double Solve(Scope scope)
	{
		try
		{
			return scope.LocalList[Name, scope].Solve(scope.ParentScope);
		}
		catch (NullReferenceException)
		{
			throw new WingCalcException($"Scope {scope.Name} does not contain variable {Name}.");
		}
	}

	public string GetName(Scope scope) => Name;

	public double Assign(INode b, Scope scope)
	{
		if (b is ILocal local) b = local.GetNonLocal(scope);
		else b = new ConstantNode(b.Solve(scope));

		scope.LocalList[Name, scope] = b;
		return 1;
	}

	public double DeepAssign(INode b, Scope scope)
	{
		string address = Name;
		INode a = scope.LocalList[address, scope];

		if (b is ILocal local) b = local.GetNonLocal(scope);
		else b = new ConstantNode(b.Solve(scope));

		if (a is IAssignable ia) return ia.DeepAssign(b, a is ILocal ? scope.ParentScope : scope);
		else
		{
			scope.LocalList[address, scope] = b;
			return 1;
		}
	}

	public double Address(Scope scope)
	{
		INode node = scope.LocalList[Name, scope];

		if (node is IPointer pointer and not ILocal) return pointer.Address(scope);
		else throw new WingCalcException($"#{Name} could not be interpreted as a pointer.", scope);
	}

	public double Set(string address, INode a, Scope scope)
	{
		string myAddress = Address(scope).ToString();

		if (scope.LocalList.Contains(myAddress))
		{
			INode node = scope.LocalList[myAddress, scope];

			if (node is IPointer pointer and not ILocal) return pointer.Set(address, a, scope);
			else throw new WingCalcException($"#{Name} could not be interpreted as a pointer.", scope);
		}
		else throw new WingCalcException($"#{Name} could not be interpreted as a pointer.", scope);
	}

	public double Get(string address, Scope scope)
	{
		INode node = scope.LocalList[Address(scope), scope];

		if (node is IPointer pointer and not ILocal) return pointer.Get(address, scope);
		else throw new WingCalcException($"#{Name} could not be interpreted as a pointer.", scope);
	}

	public INode GetAssign(Scope scope) => scope.LocalList[GetName(scope), scope];

	public double Call(Scope scope, LocalList list)
	{
		INode node = scope.LocalList[Address(scope), scope];

		if (node is ICallable callable and not ILocal) return callable.Call(scope, list);
		else throw new WingCalcException($"#{Name} could not be interpreted as callable.", scope);
	}
}
