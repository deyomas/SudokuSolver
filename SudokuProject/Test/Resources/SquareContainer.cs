using System;
using System.Collections.Generic;

public abstract class SquareContainer
{
	public List<Square> values;

	public bool Contains(Square val)
	{
		return values.Contains(val);
	}

	private delegate bool DebugDel(int val);
	private DebugDel Debug = SudokuSolver.Debug;

	struct POS
	{
		public int pos;
		public List<int> nums;

		public POS(int position, params int[] n)
		{
			pos = position; nums = new List<int>(n);
		}
	}

	#region testValidSet
	private bool CheckValidSetTest(List<int> testSet, int L, List<int> validNumbers, List<List<int>> locIdx, int layer)
	{
		string indent = "";
		for (int i = 0; i < layer; i++) {
			indent += "| ";
		}
		Console.WriteLine(indent + "Layer {0}, testSet ({1}), validSet ({3}), L {2}",
			layer, SudokuSolver.ListToString(testSet),
			L, SudokuSolver.ListToString(validNumbers));

		//Going down
		if (testSet.Count != L) {
			//Add
			//int i = testSet.Count;
			int i = 0;
			if (testSet.Count > 0) {
				i = validNumbers.IndexOf(testSet[testSet.Count - 1]) + 1;
			}
			Console.WriteLine("{0}i = {1}", indent, i);
			if (i >= validNumbers.Count) return false;
			testSet.Add(validNumbers[i]);
			layer++;
			Console.WriteLine("{0}-testSet: ({1})",
								indent, SudokuSolver.ListToString(testSet));
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
		foreach (int i in testSet) {
			foreach (int j in locIdx[i - 1]) {
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

		for (int i = 0; i < locIdx.Count; i++) {
			if (locIdx[i].Count <= L && locIdx[i].Count > 0) {
				if (!validNumbers.Contains(i + 1))
					validNumbers.Add(i + 1);
			} else {
				//locIdx.RemoveAt(i);
				//i--;
			}
		}

		string s = "locIdx:\n";
		int asdf = 1;
		foreach (List<int> l in locIdx) {
			s += asdf.ToString() + ": (" + SudokuSolver.ListToString(l) + ")\n";
			asdf++;
		}
		s += "---";
		Console.WriteLine(s);
		List<List<int>> list = new List<List<int>>();
		while (CheckValidSetTest(testSet, L, validNumbers, locIdx, 0)) {
			Console.WriteLine("Valid set:" + SudokuSolver.ListToString(testSet));
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
		if (Debug(SudokuSolver.DEBUG_ALL)) {
			for (int i = 0; i < layer; i++) {
				indent += "| ";
			}
			Console.WriteLine(indent + "Layer {0}, testSet ({1}), validSet ({3}), L {2}",
				layer, SudokuSolver.ListToString(testSet), L, SudokuSolver.ListToString(validNumbers));
		}

		//Going down
		if (testSet.Count != L) {
			//Add
			//int i = testSet.Count;
			int i = 0;
			if (testSet.Count > 0) {
				i = validNumbers.IndexOf(testSet[testSet.Count - 1]) + 1;
			}
			if (Debug(SudokuSolver.DEBUG_ALL)) Console.WriteLine("{0}i = {1}", indent, i);
			if (i > (validNumbers.Count - L) + layer) return false;
			testSet.Add(validNumbers[i]);
			layer++;
			if (Debug(SudokuSolver.DEBUG_ALL)) Console.WriteLine("{0}-testSet: ({1})", indent, SudokuSolver.ListToString(testSet));
			//while (testSet.Count == layer && i < validNumbers.Count) {
			while (!CheckValidSet(testSet, L, validNumbers, locIdx, layer)) {
				if (Debug(SudokuSolver.DEBUG_ALL)) Console.WriteLine(indent + "Returned false at layer " + layer);
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
			if (Debug(SudokuSolver.DEBUG_ALL)) Console.WriteLine(indent + "index count {0}, passed", idx.Count);
			return true;
		}

		//Failing test
		if (Debug(SudokuSolver.DEBUG_ALL)) Console.WriteLine(indent + "index count {0}, failed", idx.Count);
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
		for (int i = 1; i <= SudokuSolver.width; i++) {
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

		if (Debug(SudokuSolver.DEBUG_MOST)) {
			string s = "locIdx:\n";
			int asdf = 1;
			foreach (List<Square> l in locIdx) {
				s += asdf.ToString() + ": (" + SudokuSolver.ListToString(l) + ")\n";
				asdf++;
			}
			s += "---";
			Console.WriteLine(s);
		}
		if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine("L: {0}", L);
		List<List<int>> list = new List<List<int>>();
		while (CheckValidSet(testSet, L, validNumbers, locIdx, 0)) {
			if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine("Valid set:" + SudokuSolver.ListToString(testSet));
			list.Add(new List<int>(testSet));
			foreach (int i in testSet) {
				validNumbers.Remove(i);
			}
			testSet.Clear();
		}
		if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine("Completed");
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
				Console.WriteLine(SudokuSolver.ListToString(l));
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
				Console.WriteLine(SudokuSolver.ListToString(l));
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
				Console.WriteLine(SudokuSolver.ListToString(l));
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
				Console.WriteLine(SudokuSolver.ListToString(l));
			}
			Console.WriteLine("------");
			#endregion
		}

		int openSpaces = 0;
		foreach (Square s in values) {
			if (s.value == 0) {
				openSpaces++;
			}
		}

		for (L = 2; L < openSpaces; L++) {
			List<List<int>> validSoftPairings = SoftsRecursion(L, this.values);
			foreach (List<int> l in validSoftPairings) {
				/*
				 * if a valid pairing has 1,2 then remove all other numbers in
				 * any squares that have a 1 or a 2
				 */


				foreach (Square s in values) {
					foreach (int i in l) {
						if (s.softValues.Contains(i)) {
							if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine("REMOVING VALUES----------");
							if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine(SudokuSolver.ListToString(s.softValues));
							s.softValues.RemoveAll(val => !l.Contains(val));
							if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine(SudokuSolver.ListToString(s.softValues));
							break;
						}
					}
				}
				if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine(SudokuSolver.ListToString(l));
			}
			if (Debug(SudokuSolver.DEBUG_MOST)) Console.WriteLine("------");
		}
	}

	public void RemoveSoftValue(int val)
	{
		foreach (Square s in values) {
			s.softValues.Remove(val);
		}
	}
}