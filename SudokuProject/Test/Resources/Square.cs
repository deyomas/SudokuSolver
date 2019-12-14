using System;
using System.Collections.Generic;

public class Square
{
	public int value;
	public List<int> softValues;
	public int row;
	public int column;

	public Square()
	{
		softValues = new List<int>();
	}

	public Square(int v, int r, int c)
	{
		value = v; row = r; column = c;
		softValues = new List<int>();
	}

	public bool HasSoftValue(int v)
	{
		return softValues.Contains(v);
	}

	public string GetValueString()
	{
		string r = "[" + row + "," + column + "]:";
		if (value == 0) {
			return r + "-" + SudokuSolver.ListToString<int>(softValues);
		} else {
			return r + value;
		}
	}

	public void SetValue(int v)
	{
		value = v;
		softValues.Clear();
	}

	public override string ToString()
	{
		return GetValueString();
	}
}