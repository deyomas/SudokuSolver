using System;
using System.Collections;

public class Box : SquareContainer
{
	//public List<Square> values;
	public List<SoftValueLimiter> softLimiters;
	public int x;
	public int y;

	public Box(int x, int y)
	{
		values = new List<Square>();
		softLimiters = new List<SoftValueLimiter>();
		this.x = x; this.y = y;
	}

	public override void CalculateSofts()
	{
		base.CalculateSofts();

		//if (debug) PrintValues();
		softLimiters = new List<SoftValueLimiter>();
		for (int i = 1; i <= width; i++) {
			List<Square> found = values.FindAll((Square s) => s != null && s.softValues.Contains(i));
			if (found.Count > 1) {
				int count = found.Count;
				int r = found[0].row; int c = found[0].column;
				if (found.TrueForAll(b => b.row == r)) {
					softLimiters.Add(new SoftValueLimiter(false, r, i));
				}
				if (found.TrueForAll(b => b.column == c)) {
					softLimiters.Add(new SoftValueLimiter(true, c, i));
				}
			}
		}

		foreach (SoftValueLimiter svl in softLimiters) {
			if (debug >= DEBUG_MOST) Console.WriteLine(svl.GetValueString());
		}
	}

	public void PrintValues()
	{
		Console.WriteLine("Index: [{0},{1}]", x, y);
		foreach (Square s in values) {
			Console.WriteLine(s.GetValueString());
		}
	}
}