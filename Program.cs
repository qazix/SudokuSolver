using System;
using System.Collections.Generic;

namespace SudokuSolver
{
   class Solver
   {
      /// <summary>
      /// Main create a board and tries to solve it
      /// </summary>
      /// <param name="args">Filename from the command line</param>
      public static void Main(string[] args)
      {
         if (args.Length > 0)
         {
            Board iniBoard = new Board(args[0]);

//            System.Console.WriteLine(iniBoard);

            if(solve(iniBoard) && !checkCompleteness(iniBoard))
            {
               iniBoard = backTrack(iniBoard);
            }

            if (iniBoard != null)
               System.Console.Write(iniBoard);
            else
               System.Console.Write("Board is unsolvable");

//            System.Console.Read();
         }
         else
         {
            System.Console.WriteLine("Requires a file to read from");
         }
      }
   
      /// <summary>
      /// Attempts to solve the board
      /// </summary>
      /// <param name="pBoard">Board to be solved</param>
      /// <returns>Whether or not the board given is valid</returns>
      private static bool solve(Board pBoard)
      {
         bool update;
         bool valid = true;

         do
         {
            update = false;
            for (int i = 0; valid && i < 9; ++i)
               for (int j = 0; valid && j < 9; ++j)
               {
                  //If there is only one option and it's not already locked in
                  if (pBoard.getHeat(i, j) == 1 && !pBoard.isLocked(i, j))
                  {
                     update = true;
                     //Update the board
                     valid = pBoard.update(i, j, pBoard.getFirstVal(i, j));
                  }
               }
         }
         while(update && valid);

         return valid;
      }

      private static Board backTrack(Board pBoard)
      {
         int coolestNode = pBoard.getLowestHeat();
         int row = coolestNode / 9;
         int col = coolestNode % 9;
         Board copy = null;

         for (int i = 1; i < 10; ++i)
         {
            copy = pBoard.getCopy();
            i = copy.getFirstVal(row, col, i);

/*            System.Console.WriteLine(copy);
            System.Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++\n" + 
                                     "row=" + row + " col=" + col + "val=" + i + 
                                     "\n+++++++++++++++++++++++++++++++++++++++++++++\n");
            System.Console.Read();*/

            if (i < 10 && copy.update(row, col, i) && solve(copy))
               if (checkCompleteness(copy))
                  return copy;
               else
               {
                  copy = backTrack(copy);
                  if (copy != null && checkCompleteness(copy))
                     return copy;
               }
         }

         return null;
      }

      /// <summary>
      /// We want to know if we found a partial or complete solution
      /// </summary>
      /// <param name="pBoard">Board to check</param>
      /// <returns>Whether the Board is complete</returns>
      private static bool checkCompleteness(Board pBoard)
      {
         bool complete = true;

         for (int i = 0; i < 9 && complete; ++i)
            for (int j = 0; j < 9 && complete; ++j)
            {
               //The board is not complete if there is ever more than one possible value
               if (pBoard.getHeat(i, j) > 1)
                  complete = false;
            }

         return complete;
      }
   }

   class Board
   {
      private bool [,,] mPosValues;
      private int [,] mHeatMap;
      private String mText;

      public int getHeat(int pRow, int pCol)
      { return mHeatMap[pRow, pCol]; }

      public bool isLocked(int pRow, int pCol)
      { return mPosValues[pRow, pCol, 0]; }

      public int getFirstVal(int pRow, int pCol, int index = 1)
      {
         int val = index;

         //loop till we find the value that is locked in
         for (val = index; val < 10 && !mPosValues[pRow, pCol, val]; ++val)
            ;

         return val;
      }

      public int getLowestHeat()
      {
         List<int>[] heats = new List<int>[10];
         for (int i = 0; i < 10; ++i)
            heats[i] = new List<int>();

         for (int i = 0; i < 9; ++i)
            for (int j = 0; j < 9; ++j)
            {
               heats[mHeatMap[i, j]].Add((i * 9) + j);
            }

         for (int i = 2; i < 10; ++i)
            if (heats[i].Count > 0)
               return heats[i][0];

         return 0;
      }
      public string getText()
      { return mText; }

      public Board getCopy()
      {
         Board copy = new Board(this);
         return copy;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="pFileName">Path to file</param>
      public Board(string pFileName)
      {
         mPosValues = new bool[9, 9, 10];
         mHeatMap = new int[9, 9];

         try
         {
            mText = System.IO.File.ReadAllText(@pFileName);

            initValues();
            initHeatMap();
         }
         catch(Exception e)
         {
            System.Console.WriteLine(e);
         }
      }

      public Board(Board pBoard)
      {
         mPosValues = new bool[9, 9, 10];
         mHeatMap = new int[9, 9];

         for (int i = 0; i < 9; ++i)
            for (int j = 0; j < 9; ++j)
            {
               mHeatMap[i, j] = pBoard.mHeatMap[i, j];
               for (int k = 0; k < 10; ++k)
               {
                  mPosValues[i, j, k] = pBoard.mPosValues[i, j, k];
               }
            }
      }

      /// <summary>
      /// Initializes the values for board based off the text from the file
      /// </summary>
      /// <param name="text">Text from file</param>
      private void initValues()
      {
         int index;
         for (int i = 0; i < 9; ++i)
         {
            for (int j = 0; j < 9; ++j)
            {
               index = 0;
               //Add the i again at the end to represent that each line has a newline char
               if (Char.IsNumber(mText[9 * i + j + i]))
                  index = Convert.ToInt32(new string(mText[9 * i + j + i], 1));

//               System.Console.WriteLine(index);

               //This lead bit represents whether a value has been locked in
               mPosValues[i, j, 0] = index > 0;

               //We can use the locked in bit to perform logic representing that only 
               // the value locked in is the only posibble value
               for (int k = 1; k < 10; ++k)
                  mPosValues[i, j, k] = !mPosValues[i, j, 0] || k == index;

               //We want some starter values here it was just convenient since we were running
               // through this doubley nested loop
               if (mPosValues[i, j, 0])
                  mHeatMap[i, j] = 1;
               else
                  mHeatMap[i, j] = 9;
            }
         }
      }

      /// <summary>
      /// If the value is locked in we make the heat 1 and calculate the heat for the neighboring values
      /// </summary>
      private void initHeatMap()
      {
         int val;
         for (int i = 0; i < 9; ++i)
            for (int j = 0; j < 9; ++j)
            {
               //If the locked in bit is true
               if (mPosValues[i, j, 0])
               {
                  val = getFirstVal(i, j);

                  update(i, j, val);
               }
            }
      }

      /// <summary>
      /// This is where the rules checking happens
      /// </summary>
      /// <param name="pRow">row of update source</param>
      /// <param name="pCol">column of update source</param>
      /// <param name="val">value to update</param>
      public bool update(int pRow, int pCol, int val)
      {
         bool valid = true;

         //These represent the big rows and columns starting index
         int bRow = (pRow / 3) * 3;
         int bCol = (pCol / 3) * 3;

//         System.Console.WriteLine("val=" + val + " r=" + bRow + " c=" + bCol);

         //lock in the value and set the Heat for that position to one
         mPosValues[pRow, pCol, 0] = true;
         for (int i = 1; i < 10; ++i)
            mPosValues[pRow, pCol, i] = (i == val);
         
         mHeatMap[pRow, pCol] = 1;  

         for (int i = 0; valid && i < 9; ++i)
         {
            //This is checking for duplicates in the same column as the update position
            if (i != pRow && mPosValues[i, pCol, val])
            {
               mPosValues[i, pCol, val] = false;
               --mHeatMap[i, pCol];
               valid = mHeatMap[i, pCol] > 0;
            }

            //This is checking for duplicates in the same row as the update position
            if (valid && i != pCol && mPosValues[pRow, i, val])
            {
               mPosValues[pRow, i, val] = false;
               --mHeatMap[pRow, i];
               valid = mHeatMap[pRow, i] > 0;
            }

            //This checks for duplicates in the same unit as the update position
            if (valid && ((i / 3) + bRow != pRow || (i % 3) + bCol != pCol) &&
               mPosValues[i / 3 + bRow, i % 3 + bCol, val])
            {
               mPosValues[i / 3 + bRow, i % 3 + bCol, val] = false;
               --mHeatMap[i / 3 + bRow, i % 3 + bCol];
               valid = mHeatMap[i / 3 + bRow, i % 3 + bCol] > 0;
            }
         }

/*         System.Console.Write(this);
         System.Console.WriteLine("valid=" + valid);
         System.Console.Read();*/

         return valid;
      }

      /// <summary>
      /// Builds a string that looks like the one retrieved from the file.
      /// </summary>
      /// <returns>text in the same format as file</returns>
      public override string ToString()
      {
         string vals = "";
         string heat = "";
         for (int i = 0; i < 9; ++i)
         {
            for (int j = 0; j < 9; ++j)
            {
               if (mPosValues[i, j, 0])
                  vals += getFirstVal(i, j).ToString();
               else
                  vals += "_";

               heat += mHeatMap[i, j];
            }

             vals += "\n";
             heat += "\n";
         }

         return vals;// +'\n' + heat;
      }
   }
}
