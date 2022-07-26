# SudokuSolver

This was a fun little project to attempt to recreate a logic-based sudoku solver. It implemented the tricks I knew how to employ, such as:

 - Row, Column, and 3x3 Box eliminations, when two identical numbers cannot exist in the same collection.
 - 'Soft' eliminations, when the potential values of a square give enough info to limit a number's placement elsewhere.
 - Last Available Location, when there is no other available location for a number in a given collection.

I also implemented a brute-force backtracking approach, which is the more computer-oriented solution, and compared the two on speed and success rate.

To run, simply launch the .exe found in the Executable folder. The application will attempt to solve the included sudoku boards (the .txt files in the same folder) both by my initial implementation through logic a person might use and compares the solve time against a brute force method that's more suited for a computer.

If you'd like, you can also launch the executable through the command line and add the command line argument "debug-#", with # being 1,2, or 3. Doing so will output a bunch of debug gibberish that years later I can barely understand, but it's there if you want to see it.
