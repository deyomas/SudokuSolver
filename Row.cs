using System;
using System.Collections;

public class Row : SquareContainer
{
	//public List<Square> values;
	public int rowIdx;

	public Row()
	{
		values = new List<Square>();
	}

	public Row(Square[] squares, int rowIdx)
	{
		values = new List<Square>(squares);
		this.rowIdx = rowIdx;
	}

	public Row(int[] squares, int rowIdx)
	{
		values = new List<Square>();
		for (int i = 0; i < squares.Length; i++) {
			Square s = new Square(squares[i], rowIdx, i);
			values.Add(s);
		}
	}

	public void PrintValues()
	{
		Console.WriteLine("Index: " + rowIdx);
		foreach (Square s in values) {
			Console.WriteLine(s.GetValueString());
		}
	}
}