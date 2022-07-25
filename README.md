# SudokuSolver

This was a fun little project to attempt to recreate a logic-based sudoku solver. It implemented the tricks I knew how to employ, such as:

 - Row, Column, and 3x3 Box eliminations, when two identical numbers cannot exist in the same collection.
 - 'Soft' eliminations, when the potential values of a square give enough info to limit a number's placement elsewhere.
 - Last Available Location, when there is no other available location for a number in a given collection.

I also implemented a brute-force backtracking approach, which is the more computer-oriented solution, and compared the two on speed and success rate.
