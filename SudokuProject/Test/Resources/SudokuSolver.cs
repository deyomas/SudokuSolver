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
using System.Collections.Generic;
public class SudokuSolver
{
	//1,4,9,16
	public const int width = 9;

	#region classes

	

	class Vector2<T>
	{
		public T x;
		public T y;

		public Vector2(T x, T y) {
			this.x = x; this.y=y;
		}
	}

	
	#endregion

	#region Variables
	static List<Row> rows;
	static List<Column> columns;
	static List<Box> boxes;
	static List<int[]> map;

	public const int DEBUG_NONE = 0;
	public const int DEBUG_MILD = 1;
	public const int DEBUG_MOST = 2;
	public const int DEBUG_ALL = 3;

	public static int debug = DEBUG_NONE;
	const int DEFAULT_ITERATIONS = 25;
	static int iterations = DEFAULT_ITERATIONS;

	public static bool Debug(int val)
	{
		return debug >= val;
	}
	#endregion

	private static void Main(string[] args)
	{
		args = new string[] {
			"Sudoku Example Very Hard.txt"
			//"debug-1"
		};

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
		debug = DEBUG_MILD;
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
		PrintMap(map);
		result = ValidateMap().ToString() + ", " + i + " iterations taken";

		return result;

		/* Old Code

		*/
	}

	static void SetValue(Square s, int val)
	{
		Console.WriteLine("Setting [{0},{1}] to {2}", s.row, s.column, val);
		s.SetValue(val);
		map[s.row][s.column] = val;
		rows[s.row].RemoveSoftValue(val);
		columns[s.column].RemoveSoftValue(val);
		int boxWidth = (int)Math.Sqrt(width);
		int i = boxWidth * (s.row / boxWidth) + (s.column / boxWidth);
		boxes[i].RemoveSoftValue(val);
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

		foreach(Box b in boxes) {
			b.PrintValues();
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
					SetValue(GetSquare(softs[0].row, softs[0].column),i);
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
				SetValue(GetSquare(softs[0].row, softs[0].column),i);
				//map[softs[0].row][softs[0].column] = i;
			}
			if ((softs = columns[i].values.FindAll(s => s.softValues.Contains(i))).Count == 1) {
				filled = true;
				if (debug >= DEBUG_MILD) Console.WriteLine("Setting map[{0},{1}] to column: {2}", softs[0].row, softs[0].column, i);
				SetValue(GetSquare(softs[0].row, softs[0].column),i);
				//map[softs[0].row][softs[0].column] = i;
			}
		}
		
		return filled;
	}

	/// <summary>
	/// Calculates all soft values. Soft values are only
	/// removed, never added
	/// </summary>
	static bool FindSofts()
	{
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
					Console.WriteLine("CHANGED IS TRUE --------");
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
					Console.WriteLine("CHANGED IS TRUE --------");
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
				if (!softChange) {
					for (int i = 0; i < softLimiterTemp.Count; i++) {
						if (!softLimiterTemp[i].Equals(b.softLimiters[i])) {
							softChange = true;
						}
					}
				}
				if (softChange) {
					if (debug >= DEBUG_MILD) {
						Console.WriteLine("Box soft limiters updated at box ({0},{1})", b.x, b.y);
						Console.WriteLine("{0}\n--Compared to--\n{1}",
								ListToString<SoftValueLimiter>(softLimiterTemp),
								ListToString<SoftValueLimiter>(b.softLimiters));
					}
					Console.WriteLine("CHANGED IS TRUE --------");
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
						Console.WriteLine("CHANGED IS TRUE --------");
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
			if (CalculateBoxAnti(adjacent)) {
				Console.WriteLine("CHANGED IS TRUE --------");
				changed = true;
			}
		}
		//Vertical
		//return changed;
		for (int i = 0; i < boxWidth; i++) {
			List<Box> adjacent = new List<Box>();
			for (int j = 0; j < boxWidth; j++) {
				adjacent.Add(boxes[i+(j * boxWidth)]);
			}
			if (CalculateBoxAnti(adjacent)) {
				Console.WriteLine("CHANGED IS TRUE --------");
				changed = true;
			}
		}
		return changed;
	}

	/// <summary>
	/// Could optimize to avoid duplicate checking
	/// </summary>
	/// <param name="b"></param>
	/// <returns></returns>
	private static bool CalculateBoxAnti(List<Box> b)
	{
		if (b.Count < 2) return false;
		bool vertical = (b[0].y == b[1].y);

		List<HashSet<int>[]> fullList = new List<HashSet<int>[]>();
		foreach (Box bo in b) {
			bo.PrintValues();
			HashSet<int>[] hsl = new HashSet<int>[width];
			foreach(Square s in bo.values) {
				if (s.value == 0) {
					foreach(int i in s.softValues) {
						int idx = vertical ? s.column : s.row;
						if (hsl[idx] == null) {
							Console.WriteLine("Adding {0} at {1}",
								vertical ? "column" : "row", idx);
							hsl[idx] = new HashSet<int>();
						}
						hsl[idx].Add(i);
					}
				}
			}
			fullList.Add(hsl);
		}

		foreach(HashSet<int>[] hsi in fullList) {
			string s = "";
			foreach (HashSet<int> hs in hsi) {
				if (hs == null) {
					s += "\n";
					continue;
				}
				foreach (int i in hs) {
					s += i + ",";
				}
				s += "\n";
			}
			Console.WriteLine("Box:\n" + s);
		}


		bool anyRemoved = false;
		int EMPTY = -1;
		int MORE_THAN_ONE = -2;
		for (int k = 1; k <= width; k++) {
			for(int j = 0; j < width; j++) {
				int chosenX = EMPTY;
				int chosenY = EMPTY;
				for (int i = 0; i < b.Count; i++) {
					if (fullList[i][j] != null &&
						fullList[i][j].Contains(k)) {
						if (chosenX == EMPTY) {
							chosenX = i; chosenY = j;
						} else {
							chosenX = MORE_THAN_ONE;
							chosenY = MORE_THAN_ONE;
						}
					}
				}
				if (chosenX != EMPTY && chosenX != MORE_THAN_ONE) {
					if (Debug(DEBUG_MOST)) Console.WriteLine("ChosenXY: {0},{1} for val {2}", chosenX, chosenY, k);
					foreach (Square s in b[chosenX].values) {
						
						if (vertical) {
							if (s.column != chosenY) {
								if (s.softValues.Contains(k)) {
									if (Debug(DEBUG_MOST)) Console.WriteLine("Removed {0} from {1}", k, s.ToString());
									s.softValues.Remove(k);
									anyRemoved = true;
								}
							}
						} else {
							if (s.row != chosenY) {
								if (s.softValues.Contains(k)) {
									if (Debug(DEBUG_MOST)) Console.WriteLine("Removed {0} from {1}", k, s.ToString());
									s.softValues.Remove(k);
									anyRemoved = true;
								}
							}
						}
					}
				}
			}
		}
		return anyRemoved;
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

		//Load in all soft values
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

	public delegate T ParseMethod<T>(string val);

	public static T[] CastArgsToArray<T>(string[] args, ParseMethod<T> method)
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

	public static List<T> CastArgsToList<T>(string[] args, ParseMethod<T> method)
	{
		return new List<T>(CastArgsToArray<T>(args, method));
	}

	public static string ArrayToString<T>(T[] array, string divider = ", ")
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

	public static string ListToString<T>(List<T> list)
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
 