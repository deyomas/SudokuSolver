/**
Make an efficient sudoku solver
*/

/**
 * Arguments:
 *	n1: The filename containing the sudoku map
 *	n2: ...
 */

using System;
using System.Text;
using System.Numerics;
using System.Collections.Generic;
public class SudokuSolver
{
	//1,4,9,16
	const int width = 9;

	#region classes
	class Square
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
				return r + "-" + ListToString<int>(softValues);
			} else {
				return r+value;
			}
		}

		public void SetValue(int v)
		{
			value = v;
			softValues.Clear();
			map[row][column] = value;
		}

		public override string ToString()
		{
			return GetValueString();
		}
	}

	class SoftValueLimiter
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

	struct POS
	{
		public int pos;
		public List<int> nums;

		public POS(int position, params int[] n)
		{
			pos = position; nums = new List<int>(n);
		}
	}

	class Vector2<T>
	{
		public T x;
		public T y;

		public Vector2(T x, T y) {
			this.x = x; this.y=y;
		}
	}

	abstract class SquareContainer
	{
		public List<Square> values;

		public bool Contains(Square val)
		{
			return values.Contains(val);
		}

		#region testValidSet
		private bool CheckValidSetTest(List<int> testSet, int L, List<int> validNumbers, List<List<int>> locIdx, int layer)
		{
			string indent = "";
			for (int i = 0; i < layer; i++) {
				indent += "| ";
			}
			Console.WriteLine(indent + "Layer {0}, testSet ({1}), validSet ({3}), L {2}", layer, ListToString<int>(testSet), L, ListToString<int>(validNumbers));
			
			//Going down
			if (testSet.Count != L) {
				//Add
				//int i = testSet.Count;
				int i = 0;
				if (testSet.Count > 0) {
					i = validNumbers.IndexOf(testSet[testSet.Count - 1])+1;
				}
				Console.WriteLine("{0}i = {1}",indent, i);
				if (i >= validNumbers.Count) return false;
				testSet.Add(validNumbers[i]);
				layer++;
				Console.WriteLine("{0}-testSet: ({1})",
									indent, ListToString<int>(testSet));
				//while (testSet.Count == layer && i < validNumbers.Count) {
				while (!CheckValidSetTest(testSet, L, validNumbers, locIdx, layer)) {
					Console.WriteLine(indent + "Returned false at layer " + layer);
					testSet.Remove(validNumbers[i]);
					i++;
					if (i >= validNumbers.Count) return false;
					testSet.Add(validNumbers[i]);
				}
			}

			//Reached the bottom of recursion
			//Check
			HashSet<int> idx = new HashSet<int>();
			foreach(int i in testSet) {
				foreach(int j in locIdx[i-1]) {
					idx.Add(j);
				}
			}

			//Passing test
			if (idx.Count == L) {
				Console.WriteLine(indent + "index count {0}, passed", idx.Count);
				return true;
			}

			//Failing test
			Console.WriteLine(indent + "index count {0}, failed", idx.Count);
			testSet.RemoveAt(testSet.Count - 1);
			return false;
		}

		private List<List<int>> Test(int L, int N, List<POS> squares)
		{
			//Return the sets in which any subset of length L of numbers
			//anywhere from 1-N resides only in those L sets
			List<int> testSet = new List<int>();
			List<int> validNumbers = new List<int>();

			List<List<int>> locIdx = new List<List<int>>();
			for (int i = 1; i <= N; i++) {
				List<int> m = new List<int>();
				foreach (POS p in squares) {
					if (p.nums.Contains(i)) m.Add(p.pos);
				}
				locIdx.Add(m);
			}

			for(int i = 0; i < locIdx.Count; i++) {
				if (locIdx[i].Count <= L && locIdx[i].Count > 0) {
					if (!validNumbers.Contains(i+1))
						validNumbers.Add(i+1);
				} else {
					//locIdx.RemoveAt(i);
					//i--;
				}
			}

			string s = "locIdx:\n";
			int asdf = 1;
			foreach (List<int> l in locIdx) {
				s += asdf.ToString() + ": (" + ListToString<int>(l) + ")\n";
				asdf++;
			}
			s += "---";
			Console.WriteLine(s);
			List<List<int>> list = new List<List<int>>();
			while (CheckValidSetTest(testSet, L, validNumbers, locIdx, 0)) {
				Console.WriteLine("Valid set:" + ListToString<int>(testSet));
				list.Add(new List<int>(testSet));
				foreach (int i in testSet) {
					validNumbers.Remove(i);
				}
				testSet.Clear();
			}
			Console.WriteLine("Completed");
			return list;
		}
		#endregion

		private bool CheckValidSet(List<int> testSet, int L, List<int> validNumbers, List<List<Square>> locIdx, int layer)
		{
			if (validNumbers.Count < L) return false;

			string indent = "";
			if (debug >= DEBUG_ALL) {
				for (int i = 0; i < layer; i++) {
					indent += "| ";
				}
				Console.WriteLine(indent + "Layer {0}, testSet ({1}), validSet ({3}), L {2}",
					layer, ListToString<int>(testSet), L, ListToString<int>(validNumbers));
			}

			//Going down
			if (testSet.Count != L) {
				//Add
				//int i = testSet.Count;
				int i = 0;
				if (testSet.Count > 0) {
					i = validNumbers.IndexOf(testSet[testSet.Count - 1]) + 1;
				}
				if (debug >= DEBUG_ALL) Console.WriteLine("{0}i = {1}", indent, i);
				if (i > (validNumbers.Count - L) + layer) return false;
				testSet.Add(validNumbers[i]);
				layer++;
				if (debug >= DEBUG_ALL) Console.WriteLine("{0}-testSet: ({1})",	indent, ListToString<int>(testSet));
				//while (testSet.Count == layer && i < validNumbers.Count) {
				while (!CheckValidSet(testSet, L, validNumbers, locIdx, layer)) {
					if (debug >= DEBUG_ALL) Console.WriteLine(indent + "Returned false at layer " + layer);
					testSet.Remove(validNumbers[i]);
					i++;
					if (i >= validNumbers.Count - L + layer) return false;
					testSet.Add(validNumbers[i]);
				}
			}



			//Reached the bottom of recursion
			//Check
			HashSet<Square> idx = new HashSet<Square>();
			foreach (int i in testSet) {
				foreach (Square j in locIdx[i - 1]) {
					idx.Add(j);
				}
			}

			//Passing test
			if (idx.Count == L) {
				if (debug >= DEBUG_ALL) Console.WriteLine(indent + "index count {0}, passed", idx.Count);
				return true;
			}

			//Failing test
			if (debug >= DEBUG_ALL) Console.WriteLine(indent + "index count {0}, failed", idx.Count);
			testSet.RemoveAt(testSet.Count - 1);
			return false;
		}

		private List<List<int>> SoftsRecursion(int L, List<Square> squares)
		{
			//Return the sets in which any subset of length L of numbers
			//anywhere from 1-N resides only in those L sets
			List<int> testSet = new List<int>();
			List<int> validNumbers = new List<int>();

			List<List<Square>> locIdx = new List<List<Square>>();
			for (int i = 1; i <= width; i++) {
				List<Square> m = new List<Square>();
				foreach (Square p in squares) {
					if (p.softValues.Contains(i)) m.Add(p);
				}
				locIdx.Add(m);
			}

			for (int i = 0; i < locIdx.Count; i++) {
				if (locIdx[i].Count <= L && locIdx[i].Count > 0) {
					if (!validNumbers.Contains(i + 1))
						validNumbers.Add(i + 1);
				} else {
					//locIdx.RemoveAt(i);
					//i--;
				}
			}

			if (debug >= DEBUG_MOST) {
				string s = "locIdx:\n";
				int asdf = 1;
				foreach (List<Square> l in locIdx) {
					s += asdf.ToString() + ": (" + ListToString<Square>(l) + ")\n";
					asdf++;
				}
				s += "---";
				Console.WriteLine(s);
			}
			if (debug >= DEBUG_MOST) Console.WriteLine("L: {0}", L);
			List<List<int>> list = new List<List<int>>();
			while (CheckValidSet(testSet, L, validNumbers, locIdx, 0)) {
				if (debug >= DEBUG_MOST) Console.WriteLine("Valid set:" + ListToString<int>(testSet));
				list.Add(new List<int>(testSet));
				foreach (int i in testSet) {
					validNumbers.Remove(i);
				}
				testSet.Clear();
			}
			if (debug >= DEBUG_MOST) Console.WriteLine("Completed");
			return list;
		}

		public virtual void CalculateSofts()
		{
			//Return the squares in which a set of length L of numbers
			//anywhere from 1-N resides only in those L squares
			int L;
			int N;
			//Setup
			bool test = false;
			if (test) {
				#region TEST
				L = 2; N = 4;
				//Passing: 0,1
				List<POS> p = new List<POS>() {
				new POS(0, 1,2),
				new POS(1, 1,2,3),
				new POS(2, 3),
				new POS(3)
			};
				List<List<int>> returns = Test(L, N, p);
				foreach (List<int> l in returns) {
					Console.WriteLine(ListToString<int>(l));
				}
				Console.WriteLine("------");
				L = 2; N = 5;
				//Passing: 0,1 | 3,4
				List<POS> p2 = new List<POS>() {
				new POS(0, 1,2,5),
				new POS(1, 1,2,5),
				new POS(2, 5),
				new POS(3, 3,4,5),
				new POS(4, 3,4,5),
			};
				returns = Test(L, N, p2);
				foreach (List<int> l in returns) {
					Console.WriteLine(ListToString<int>(l));
				}
				Console.WriteLine("------");
				L = 3; N = 5;
				//Passing: 0,1,2
				List<POS> p3 = new List<POS>() {
				new POS(0, 1,3,5),
				new POS(1, 1,2,5),
				new POS(2, 2,3,5),
				new POS(3, 5),
				new POS(4, 5),
			};
				returns = Test(L, N, p3);
				foreach (List<int> l in returns) {
					Console.WriteLine(ListToString<int>(l));
				}
				Console.WriteLine("------");
				L = 3; N = 7;
				//Passing: 0,1,3
				List<POS> p4 = new List<POS>() {
				new POS(0, 1,5,6,7),
				new POS(1, 1,6,7),
				new POS(2, 5),
				new POS(3, 5,6)
			};
				returns = Test(L, N, p4);
				foreach (List<int> l in returns) {
					Console.WriteLine(ListToString<int>(l));
				}
				Console.WriteLine("------");
				#endregion
			}

			int openSpaces = 0;
			foreach(Square s in values) {
				if (s.value == 0) {
					openSpaces++;
				}
			}
			for(L = 2; L < openSpaces; L++) {
				List<List<int>> validSoftPairings = SoftsRecursion(L, this.values);
				foreach (List<int> l in validSoftPairings) {
					/*
					 * if a valid pairing has 1,2 then remove all other numbers in
					 * any squares that have a 1 or a 2
					 */
					

					foreach (Square s in values) {
						foreach(int i in l) {
							if (s.softValues.Contains(i)) {
								if (debug >= DEBUG_MOST) Console.WriteLine("REMOVING VALUES----------");
								if (debug >= DEBUG_MOST) Console.WriteLine(ListToString<int>(s.softValues));
								s.softValues.RemoveAll(val => !l.Contains(val));
								if (debug >= DEBUG_MOST) Console.WriteLine(ListToString<int>(s.softValues));
								break;
							}
						}
					}
					if (debug >= DEBUG_MOST) Console.WriteLine(ListToString<int>(l));
				}
				if (debug >= DEBUG_MOST) Console.WriteLine("------");
			}
		}

		public void RemoveSoftValue(int val)
		{
			foreach(Square s in values) {
				s.softValues.Remove(val);
			}
		}
	}

	class Box : SquareContainer
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

	class Row : SquareContainer
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
	#endregion

	#region Variables
	static List<Row> rows;
	static List<Column> columns;
	static List<Box> boxes;
	static List<int[]> map;

	const int DEBUG_NONE = 0;
	const int DEBUG_MILD = 1;
	const int DEBUG_MOST = 2;
	const int DEBUG_ALL = 3;

	static int debug = DEBUG_NONE;
	const int DEFAULT_ITERATIONS = 25;
	static int iterations = DEFAULT_ITERATIONS;
	#endregion

	private static void Main(string[] args)
	{
		int requiredArgsLength = 1;
		if (args.Length < requiredArgsLength) {
			Console.WriteLine("Error: Arguments not correct!\n" +
				"Correct usage: SudokuSolver [filename]");
			return;
		}

		for(int i = requiredArgsLength; i < args.Length; i++) {
			switch(args[i]) {
				case "debug-1":
					debug = DEBUG_MILD;
					break;
				case "debug-2":
					debug = DEBUG_MOST;
					break;
				case "debug-3":
					debug = DEBUG_ALL;
					break;
			}
			if (iterations == DEFAULT_ITERATIONS) int.TryParse(args[i], out iterations);
		}

		map = LoadMap(args[0]);

		LoadOthers();

		Console.WriteLine(Solve());
	}

	private static string Solve()
	{
		string result = "";
		/**
			* Starting smaller, like a box that's only 4x4 wide, would be best
			* However, in a 4x4, we don't get to see the results of held values (small numbers)
			* affecting other boxes
			* 
			* Brute force is the simplest and most straight forward answer to this
			* 
			* I would want to implement:
			*	CheckRow()
			*	CheckColumn()
			*	CheckBox()
			* to find easy cases where one number remains OR check every
			* empty space to look for easy fill in numbers
			* 
			* 
			* I also think that to make an efficient solver we need to collect
			* as much data as possible per iteration so we can work the most with 
			* what we have every iteration.
			* 

			  Two+ same options in same squares means nothing else can go there.
			  This one would be more difficult to implement
			*/

		//Brute Force iteration

		//This stores all possible numbers
		//List<HashSet<int>[]> possible = FillPossibilities();
		int i = 0;
		Console.WriteLine("--Iterating");
		while (IterateSolver()) {
			PrintMap(map);
			Console.WriteLine("----------------");
			i++;
			if (i > iterations) break;
		}

		result = ValidateMap().ToString() + ", " + i + " iterations taken";

		return result;

		/* Old Code

		*/
	}

	static void SetValue(Square s, int val)
	{
		s.SetValue(val);
		for(int i = 0; i < width; i++) {
			rows[i].RemoveSoftValue(val);
			columns[i].RemoveSoftValue(val);
			boxes[i].RemoveSoftValue(val);
		}
	}

	static bool IterateSolver()
	{
		//Autofill in any that only have one possible value
		//OR update the soft values
		bool filled = false;

		//Find all softs by hard values
		filled = FindSofts();

		//Handle soft limiters
		foreach(Box b in boxes) {
			foreach(SoftValueLimiter svl in b.softLimiters) {
				if (debug >= DEBUG_MILD) Console.WriteLine(svl.GetValueString());
				if (svl.vertical) {
					foreach (Square s in columns[svl.idx].values) {
						if (!b.Contains(s)) {
							s.softValues.Remove(svl.value);
							//if (debug >= DEBUG_MOST) Console.WriteLine("Removed {0} from row {1}, column {2}", svl.value, s.row, s.column);
						}
					}
				} else {
					foreach (Square s in rows[svl.idx].values) {
						if (!b.Contains(s)) {
							s.softValues.Remove(svl.value);
							//if (debug >= DEBUG_MOST) Console.WriteLine("Removed {0} from row {1}, column {2}", svl.value, s.row, s.column);
						}
					}
				}
			}
		}

		Square tempSquare;

		//Fill in any squares which only have a single soft value;
		for (int row = 0; row < width; row++) {
			for(int column = 0; column < width; column++) {
				tempSquare = GetSquare(row, column);
				if (tempSquare == null) continue;
				if (tempSquare.softValues.Count == 1) {
					filled = true;
					if (debug >= DEBUG_MILD) Console.WriteLine("Setting map[{0},{1}] to square: {2}", row, column, GetSquare(row, column).softValues[0]);
					SetValue(tempSquare, tempSquare.softValues[0]);
					//tempSquare.SetValue(tempSquare.softValues[0]);
				}
			}
		}

		//Fill any squares in a box which have the only instance
		//of a soft value in that box
		List<Square> softs;
		foreach(Box b in boxes) {
			for (int i = 1; i <= width; i++) {
				if ((softs = b.values.FindAll(s => s.softValues.Contains(i))).Count == 1) {
					filled = true;
					if (debug >= DEBUG_MOST) b.PrintValues();
					if (debug >= DEBUG_MILD) Console.WriteLine("Setting map[{0},{1}] to box: {2}", softs[0].row, softs[0].column, i);
					GetSquare(softs[0].row, softs[0].column).SetValue(i);
					//map[softs[0].row][softs[0].column] = i;
				}
			}
		}

		//Fill in any squares in a row or column which have the only
		//instance of a soft value in that row/column
		for(int i = 0; i < width; i++) {
			if ((softs = rows[i].values.FindAll(s => s.softValues.Contains(i))).Count == 1) {
				filled = true;
				if (debug >= DEBUG_MILD) Console.WriteLine("Setting map[{0},{1}] to row: {2}", softs[0].row, softs[0].column, i);
				GetSquare(softs[0].row, softs[0].column).SetValue(i);
				//map[softs[0].row][softs[0].column] = i;
			}
			if ((softs = columns[i].values.FindAll(s => s.softValues.Contains(i))).Count == 1) {
				filled = true;
				if (debug >= DEBUG_MILD) Console.WriteLine("Setting map[{0},{1}] to column: {2}", softs[0].row, softs[0].column, i);
				GetSquare(softs[0].row, softs[0].column).SetValue(i);
				//map[softs[0].row][softs[0].column] = i;
			}
		}
		
		return filled;
	}

	static bool firstSofts = false;
	/// <summary>
	/// Calculates all soft values
	/// </summary>
	static bool FindSofts()
	{
		//Move this to a load map function
		if (!firstSofts) {
			foreach (Row r in rows) {
				foreach (Square s in r.values) {
					s.softValues = new List<int>();
				}
			}

			//Add all available numbers
			for (int row = 0; row < width; row++) {
				for (int column = 0; column < width; column++) {
					if (map[row][column] == 0) {
						//Empty space
						for (int num = 1; num <= width; num++) {
							if (CheckSquareHard(num, column, row)) {
								GetSquare(row, column).softValues.Add(num);
							}
						}
					}
				}
			}
			firstSofts = true;
		}


		//Perform filtering calculations
		bool changed = false;
		List<List<int>> softsSaved = new List<List<int>>();
		foreach(Row r in rows) {
			//Store soft values for checking
			for (int i = 0; i < width; i++) {
				softsSaved.Add(r.values[i].softValues);
			}
			
			//Update soft values
			r.CalculateSofts();

			//Check to see if any soft values were updated
			for(int i = 0; i < width; i++) {
				if (!softsSaved[i].Equals(r.values[i].softValues)) {
					if (debug >= DEBUG_MOST) {
						Console.WriteLine("row change true");
						Console.WriteLine("{0}\n--Compared to--\n{1}",
								ListToString<int>(softsSaved[i]),
								ListToString<int>(r.values[i].softValues));
						r.PrintValues();
					}
					changed = true;
				}
			}
			softsSaved.Clear();
		}

		foreach(Column c in columns) {
			//Store soft values for checking
			if (debug >= DEBUG_MILD || !changed) {
				for (int i = 0; i < width; i++) {
					softsSaved.Add(c.values[i].softValues);
				}
			}

			//Update soft values
			c.CalculateSofts();

			//Check to see if any soft values were updated
			if (debug >= DEBUG_MILD || !changed) {
				for (int i = 0; i < width; i++) {
					if (!softsSaved[i].Equals(c.values[i].softValues)) {
						if (debug >= DEBUG_MOST) {
							Console.WriteLine("column change true");
							Console.WriteLine("{0}\n--Compared to--\n{1}",
								ListToString<int>(softsSaved[i]),
								ListToString<int>(c.values[i].softValues));
							c.PrintValues();
						}
						changed = true;
					}
				}
			}
			softsSaved.Clear();
		}

		//Also track softLimiters for boxes
		List<SoftValueLimiter> softLimiterTemp= new List<SoftValueLimiter>();
		foreach (Box b in boxes) {
			//Store soft values for checking
			if (debug >= DEBUG_MILD || !changed) {
				softLimiterTemp = new List<SoftValueLimiter>(b.softLimiters);
				for (int i = 0; i < width; i++) {
					softsSaved.Add(b.values[i].softValues);
				}
			}

			//Update soft values
			b.CalculateSofts();

			//Check to see if any soft values were updated
			if (debug >= DEBUG_MILD || !changed) {
				bool softChange = false;
				//Need to do it like this because .Equals on the lists wasn't working
				if (softLimiterTemp.Count != b.softLimiters.Count) {
					softChange = true;
				}
				for(int i = 0; i < softLimiterTemp.Count; i++) {
					if (!softLimiterTemp[i].Equals(b.softLimiters[i])) {
						softChange = true;
					}
				}
				if (softChange) {
					if (debug >= DEBUG_MILD) {
						Console.WriteLine("Box soft limiters updated at box ({0},{1})", b.x, b.y);
						Console.WriteLine("{0}\n--Compared to--\n{1}",
								ListToString<SoftValueLimiter>(softLimiterTemp),
								ListToString<SoftValueLimiter>(b.softLimiters));
					}
					changed = true;
				}

				for (int i = 0; i < width; i++) {
					if (!softsSaved[i].Equals(b.values[i].softValues)) {
						if (debug >= DEBUG_MILD) {
							Console.WriteLine("Box change true at box ({0},{1}), square {2}", b.x, b.y, b.values[i].GetValueString());
							Console.WriteLine("{0}\n--Compared to--\n{1}",
								ListToString<int>(softsSaved[i]),
								ListToString<int>(b.values[i].softValues));
						}
						changed = true;
					}
				}
			}
			softsSaved.Clear();
		}

		//Calculate anti-softlimiters
		//Horizontal
		int boxWidth = (int)Math.Sqrt(width);
		for (int i = 0; i < boxWidth; i++) {
			List<Box> adjacent = new List<Box>();
			for (int j = 0; j < boxWidth; j++) {
				adjacent.Add(boxes[j+(i*boxWidth)]);
			}
			CalculateBoxAnti(adjacent);
		}
		//Vertical
		return changed;
		for (int i = 0; i < boxWidth; i++) {
			List<Box> adjacent = new List<Box>();
			for (int j = 0; j < boxWidth; j++) {
				adjacent.Add(boxes[i+(j * boxWidth)]);
			}
			CalculateBoxAnti(adjacent);
		}
		return changed;
	}

	private static void CalculateBoxAnti(List<Box> b)
	{
		List<HashSet<int>[]> fullList = new List<HashSet<int>[]>();
		foreach (Box bo in b) {
			bo.PrintValues();
			HashSet<int>[] hsl = new HashSet<int>[width];
			foreach(Square s in bo.values) {
				if (s.value == 0) {
					foreach(int i in s.softValues) {
						//if (vertical)
						//else
						if (hsl[s.row] == null) {
							Console.WriteLine("Adding row at " + s.row);
							hsl[s.row] = new HashSet<int>();
						}
						hsl[s.row].Add(i);
					}
				}
			}
			fullList.Add(hsl);
		}

		foreach(HashSet<int>[] hsi in fullList) {
			string s = "";
			foreach (HashSet<int> hs in hsi) {
				if (hs == null) continue;
				foreach (int i in hs) {
					s += i + ",";
				}
				s += "\n";
			}
			Console.WriteLine("Box:\n" + s);
		}

		/*
		 * Check against SoftValueLimiters and hard values to save time?
		 * If two boxes contain a number then it's null. Choose active and
		 * Set it if no others available?
		X X X   X 9 9   X _ X
		9 9 X   9 X X   9 _ 9
		9 9 X   X 9 9   9 X X
		*/

		for (int k = 1; k < width; k++) {
			for(int i = 0; i < b.Count; i++) {
				for(int j = 0; i < b.Count; j++) {
					if (fullList[i][j].Contains(k)) {
						//Row is marked as full
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// Returns the square at row, column
	/// </summary>
	/// <param name="row"></param>
	/// <param name="column"></param>
	/// <returns></returns>
	private static Square GetSquare(int row, int column)
	{
		return rows[row].values[column];
	}

	private static List<int[]> LoadMap(string filename)
	{
		System.IO.StreamReader file = new System.IO.StreamReader(@filename);
		string line;
		int row = 0;
		int column = 0;
		List<int[]> m = new List<int[]>(width);
		while ((line = file.ReadLine()) != null) {
			int num;
			column = 0;
			m.Add(new int[width]);
			foreach (char c in line) {
				if (int.TryParse(c.ToString(), out num)) {
					m[row][column] = num;
				}
				column++;
				if (column >= width) break;
			}
			row++;
			if (row >= width) break;
		}

		Console.WriteLine("Loaded Map:");
		PrintMap(m);
		Console.WriteLine("===============");

		return m;
	}

	/// <summary>
	/// Loads hard values into rows, columns, and boxes
	/// </summary>
	private static void LoadOthers()
	{
		//Load rows
		rows = new List<Row>();
		int idx = 0;
		foreach (int[] row in map) {
			rows.Add(new Row(row, idx));
			idx++;
		}

		//Load columns
		columns = new List<Column>();
		for (int column = 0; column < width; column++) {
			columns.Add(new Column() {
				columnIdx = column
			});
			for (int row = 0; row < width; row++) {
				columns[column].values.Add(rows[row].values[column]);
			}
		}

		//Load boxes
		boxes = new List<Box>();
		int boxWidth = (int)Math.Sqrt(width);

		//Iterate through boxes
		for (int boxX = 0; boxX < boxWidth; boxX++) {
			for (int boxY = 0; boxY < boxWidth; boxY++) {
				boxes.Add(new Box(boxX, boxY));
				for (int i = 0; i < boxWidth; i++) {
					for (int j = 0; j < boxWidth; j++) {
						boxes[boxes.Count - 1].values.Add(
							rows[boxX * boxWidth + i].values[boxY * boxWidth + j]);
					}
				}
			}
		}
	}

	/// <summary>
	/// Prints out the map
	/// </summary>
	/// <param name="map"></param>
	private static void PrintMap(List<int[]> map)
	{
		int rows = map.Count;
		int cols = map[0].Length;
		for (int i = 0; i < rows; i++) {
			StringBuilder s = new StringBuilder();
			for (int j = 0; j < cols; j++) {
				if (map[i][j] > 0) {
					s.Append(map[i][j].ToString());
				} else {
					s.Append(" ");
				}
				if (j != cols - 1) {
					s.Append("|");
				}
			}
			Console.WriteLine(s.ToString());
			//Console.WriteLine("------------------");
		}
	}

	/// <summary>
	/// Returns if the map is solved or not
	/// </summary>
	/// <returns></returns>
	static bool ValidateMap()
	{
		foreach(int[] row in map) {
			HashSet<int> nums = new HashSet<int>();
			foreach(int n in row) {
				if (nums.Contains(n)) return false;
				if (n == 0) return false;
				nums.Add(n);
			}
		}
		return true;
	}

	/// <summary>
	/// Checks if a number is valid in a square against HARD values
	/// </summary>
	/// <param name="value">Value to check</param>
	/// <param name="column">Column</param>
	/// <param name="row">Row (From the top)</param>
	/// <returns></returns>
	private static bool CheckSquareHard(int value, int column, int row)
	{
		//Check column
		for (int i = 0; i < width; i++) {
			if (map[i][column] == value) return false;
		}

		//Check row
		for (int i = 0; i < width; i++) {
			if (map[row][i] == value) return false;
		}

		//CheckBox
		int boxWidth = (int)Math.Sqrt(width);
		int boxX = column / boxWidth;
		int boxY = row / boxWidth;
		for (int i = 0; i < boxWidth; i++) {
			for (int j = 0; j < boxWidth; j++) {
				if (map[boxY * boxWidth + i][boxX * boxWidth + j] == value) return false;
			}
		}

		return true;
	}


	#region HELPER FUNCTIONS

	private delegate T ParseMethod<T>(string val);

	private static T[] CastArgsToArray<T>(string[] args, ParseMethod<T> method)
	{
		T[] values = new T[args.Length];
		for (int i = 0; i < args.Length; i++) {
			try {
				values[i] = method(args[i]);
			} catch (System.Exception) {
				throw new System.Exception("Argument " + args[i] + " Cannot be converted to " + typeof(T));
			}
		}
		return values;
	}

	private static List<T> CastArgsToList<T>(string[] args, ParseMethod<T> method)
	{
		return new List<T>(CastArgsToArray<T>(args, method));
	}

	private static string ArrayToString<T>(T[] array, string divider = ", ")
	{
		StringBuilder s = new StringBuilder();
		for (int i = 0; i < array.Length; i++) {
			s.Append(array[i].ToString());
			if (i != array.Length - 1) {
				s.Append(divider);
			}
		}
		return s.ToString();
	}

	private static string ListToString<T>(List<T> list)
	{
		return ArrayToString<T>(list.ToArray());
	}

	#endregion
}

public static class ArrayExt
{
	public static T[] GetRow<T>(this T[,] array, int row)
	{
		if (!typeof(T).IsPrimitive)
			throw new InvalidOperationException("Not supported for managed types.");

		if (array == null)
			throw new ArgumentNullException("array");

		int cols = array.GetUpperBound(1) + 1;
		T[] result = new T[cols];

		int size;

		if (typeof(T) == typeof(bool))
			size = 1;
		else if (typeof(T) == typeof(char))
			size = 2;
		else
			size = System.Runtime.InteropServices.Marshal.SizeOf<T>();

		Buffer.BlockCopy(array, row * cols * size, result, 0, cols * size);

		return result;
	}
}
 