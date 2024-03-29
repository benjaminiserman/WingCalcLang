﻿namespace WingCalc.Nodes;

internal interface ILocal : IAssignable, INode
{
	string GetName(Scope scope);

	INode GetNonLocal(Scope scope)
	{
		INode node = scope.LocalList[GetName(scope), scope];

		if (node is ILocal gotLocal) return gotLocal.GetNonLocal(scope.ParentScope);
		else return node;
	}
}