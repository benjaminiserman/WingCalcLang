﻿namespace WingCalculatorShared;

internal record PromptNode(INode A, INode B) : INode
{
	public double Solve()
	{
		double a = A.Solve();

		if (a != 0) return a;
		else return B.Solve();
	}
}