using System;
using System.Collections;

namespace Origam.Schema.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ExpressionBrowserTreeSortAtribute : Attribute
{
	private readonly Type ttype;
	public ExpressionBrowserTreeSortAtribute(Type t)
	{
				ttype = t;
			}

	public IComparer GetComparator()
	{
				return Activator.CreateInstance(ttype) as IComparer;
			}
}