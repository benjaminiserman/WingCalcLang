namespace WingCalc.Nodes;

internal interface ICallable : INode
{
	double Call(Scope scope, LocalList list);
}