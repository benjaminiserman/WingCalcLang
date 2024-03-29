﻿namespace WingCalc;
using System.Collections.Generic;
using System.Linq;
using WingCalc.Exceptions;

internal class LocalList
{
	private readonly Dictionary<string, INode> _nodes = new(StringComparer.OrdinalIgnoreCase);

	public LocalList() { }

	public LocalList(ICollection<INode> nodes)
	{
		_nodes = nodes.Select((n, i) => (n, i)).ToDictionary(x => x.i.ToString(), x => x.n, StringComparer.OrdinalIgnoreCase);
	}

	public INode this[string name, Scope scope]
	{
		get => Get(name, scope);
		set => Set(name, value);
	}

	public INode this[double name, Scope scope] => this[name.ToString(), scope];

	public double Set(string name, INode a)
	{
		if (_nodes.ContainsKey(name)) _nodes[name] = a;
		else _nodes.Add(name, a);

		return 1;
	}

	public INode Get(string name, Scope scope)
	{
		if (_nodes.ContainsKey(name)) return _nodes[name];
		else
		{
			throw new WingCalcException($"LocalList does not contain element #{name}.", scope);
		}
	}

	public bool Contains(string name) => _nodes.ContainsKey(name);

	public static explicit operator List<INode>(LocalList x)
	{
		List<INode> list = new();
		for (double i = 0; true; i++)
		{
			if (x._nodes.TryGetValue(i.ToString(), out INode node))
			{
				list.Add(node);
			}
			else break;
		}

		return list;
	}

	public int Count => _nodes.Count;
}
