using System;
using System.Collections.Generic;

class Column : SquareContainer
{
	//public List<Square> values;
	public int columnIdx;

	public Column()
	{
		values = new List<Square>();
	}

	public Column(int[] squares, int columnIdx)
	{
		values = new List<Square>();
		for (int i = 0; i < squares.Length; i++) {
			Square s = new Square(squares[i], i, columnIdx);
			values.Add(s);
		}
		this.columnIdx = columnIdx;
	}

	public void PrintValues()
	{
		Console.WriteLine("Index: " + columnIdx);
		foreach (Square s in values) {
			Console.WriteLine(s.GetValueString());
		}
	}
}