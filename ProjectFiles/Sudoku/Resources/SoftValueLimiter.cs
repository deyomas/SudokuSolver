using System;
using System.Collections;

public class SoftValueLimiter
{
	public bool vertical;
	public int idx;
	public int value;

	public SoftValueLimiter(bool v, int i, int val)
	{
		vertical = v;
		idx = i;
		value = val;
	}

	public string GetValueString()
	{
		return
			(vertical ? "Vert:" : "Hor:") +
			idx + " - " + value;
	}

	public override string ToString()
	{
		return GetValueString();
	}

	public override bool Equals(object o)
	{
		if (o == null) return false;
		if (!(o is SoftValueLimiter)) return false;
		return ((SoftValueLimiter)o).GetHashCode().Equals(GetHashCode());
	}

	public override int GetHashCode()
	{
		return
			vertical.GetHashCode() +
			idx.GetHashCode() +
			value.GetHashCode();
	}
}