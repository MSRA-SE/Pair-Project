using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _2048AI
{
    class Const
    {
        public const int INF = 10000000;
    }

    enum Step
    {
        up, right, down, left
    }

    struct Node
    {
        public bool IsMax;
        public int[,] grids;
        public Node[] Childs;
        public double data;
        public bool IsValid;
    }

    public class TreeSearch
    {
        private int Depth;
        Node node;
        // private int NextMoveDirection;

        private TreeSearch()
        {

        }
        public TreeSearch(int[,] gridsIn, int depthIn)
        {
            // NextMoveDirection = 0;
            Depth = depthIn;
            node.IsMax = true;
            node.IsValid = true;
            node.grids = new int[4, 4];
            for(int i = 0; i < 4; ++i)
            {
                for(int j = 0; j < 4; ++j)
                {
                    node.grids[i, j] = gridsIn[i, j];
                }
            }
        }

        public int NextMove()
        {
            double Res = alphabeta(ref node, Depth, -Const.INF, Const.INF);
            double[] Result = new double[] { -Const.INF, -Const.INF, -Const.INF, -Const.INF };
            int MaxInd = 0;
            // return NextMoveDirection;
            for(int i = 0; i < 4; ++i)
            {
                if(node.Childs[i].IsValid)
                {
                    Result[i] = MinMax(node.Childs[i], Depth - 1);
                }
            }
            double MaxVal = Result[MaxInd];
            for(int i = 1 ; i < 4; ++i)
            {
                if (Result[i] > MaxVal)
                {
                    MaxInd = i;
                    MaxVal = Result[i];
                }
            }

            /*
            for (int i = 0; i < 4; i++)
            {
                System.Diagnostics.Debugger.Log(1, "1", i + ": " + Result[i] + " ");
            }
            System.Diagnostics.Debugger.Log(1, "1", "\n");
            */

            return MaxInd;
        }

        private double alphabeta(ref Node CurrentNode, int depthIn, double alpha, double beta)
        {
            if(depthIn == 0)
            {
                return CurrentNode.data;
            }
            double tmp;
            if (CurrentNode.IsMax)
            {
                CurrentNode.Childs = new Node[4];
                for (int i = 0; i < 4; ++i)
                {
                    CurrentNode.Childs[i].IsMax = false;
                    Step step = (Step)i;
                    CurrentNode.Childs[i].grids = new int[4, 4];
                    if (AITryNextMove(CurrentNode.grids, step, CurrentNode.Childs[i].grids, ref CurrentNode.Childs[i].data))
                    {

                        CurrentNode.Childs[i].IsValid = true;
                    }
                    else
                    {
                        CurrentNode.Childs[i].IsValid = false;
                    }
                    if(CurrentNode.Childs[i].IsValid)
                    {
                        tmp = alphabeta(ref CurrentNode.Childs[i], depthIn - 1, alpha, beta);
                        alpha = Math.Max(alpha, tmp);
                        if (beta <= alpha)
                        {
                            for(int j = i + 1; j < 4; ++j)
                            {
                                CurrentNode.Childs[j].IsValid = false;
                            }
                            break;
                        }
                    }
                }
                return alpha;
            }
            else
            {
                // get the most ugly insertion
                CurrentNode.Childs = new Node[1];
                CurrentNode.Childs[0].grids = new int[4, 4];
                CurrentNode.Childs[0].IsMax = true;
                CurrentNode.Childs[0].IsValid = true;
                AIGetNextINsertion(CurrentNode.grids, CurrentNode.Childs[0].grids, ref CurrentNode.Childs[0].data);
                tmp = alphabeta(ref CurrentNode.Childs[0], depthIn - 1, alpha, beta);
                return Math.Min(beta, tmp);
            }
        }


        // get the best move using MinMax
        private double MinMax(Node currentnode, int depthIn)
        {
            if(depthIn == 0)
            {
                return currentnode.data;
            }

            if (currentnode.IsMax)
            {
                double Res = -Const.INF;
                for (int i = 0; i < 4; ++i)
                {
                    if (currentnode.Childs[i].IsValid)
                    {
                        Res = Math.Max(MinMax(currentnode.Childs[i], depthIn - 1), Res);
                    }
                }

                if (Res == -Const.INF) return currentnode.data;
				
                return Res;
            }
            else
            {
                return MinMax(currentnode.Childs[0], depthIn - 1);
            }
        }
        // evaluation to a certain result
        private double GetResult(int[,] grids_out)
        {
            double WeightSmooth = 0 * GetSmoothness(grids_out);
            double WeightEmpty = 0.5 * GetEmptyNum(grids_out);
            double WeightMax = 0 * GetMaxNum(grids_out);
            double WeightMono = 1.5 * GetMono(grids_out);
			double WeightPos = 7 * WhereIsMax(grids_out);
			double WeightDistinct = 0.15 * DistinctMax(grids_out);

            /*
            System.Diagnostics.Debugger.Log(1, "1", "----------------------------------------\n");

			for(int i = 0; i < 4; i++)
			{
				for(int j = 0; j < 4; j++)
				{
					System.Diagnostics.Debugger.Log(1, "1", grids_out[i, j] + " ");
				}
				System.Diagnostics.Debugger.Log(1, "1", "\n");
			}
			System.Diagnostics.Debugger.Log(1, "1", "E:" + WeightEmpty + " M:" + WeightMono + " P:" + WeightPos + " D:" + WeightDistinct + "\n");

            System.Diagnostics.Debugger.Log(1, "1", "----------------------------------------\n");
            */

            return WeightSmooth
                + WeightEmpty
                + WeightMax
                + WeightMono
                + WeightPos
				+ WeightDistinct;

        }
        // get evaluation result
        private bool AITryNextMove(int[,] grids, Step step, int[,] grids_out, ref double Res)
        {
            Res = 0;
            bool canMove = false;
            // result = new int[2];
            // Rotate
            int[,] _grid = new int[4, 4];
            // int[,] _grid_back = new int[4, 4];

            switch (step)
            {
                case Step.up:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            _grid[x, y] = grids[x, y];
                        }
                    }
                    break;
                case Step.down:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            _grid[x, y] = grids[3 - x, y];
                        }
                    }
                    break;
                case Step.left:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            _grid[x, y] = grids[3 - y, x];
                        }
                    }
                    break;
                case Step.right:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            _grid[x, y] = grids[y, 3 - x];
                        }
                    }
                    break;
            }

			for(int j = 0; j < 4; j++)
			{
				int last = -1, pos = 0;
				for(int i = 0; i < 4; i++)
				{
					if(_grid[i, j] == 0)continue;
					if(last == -1 || _grid[last, j] != _grid[i, j])
					{
						if(last != -1)_grid[pos++, j] = _grid[last, j];
						last = i;
						continue;
					}
					_grid[pos++, j] = 2 * _grid[last, j];
					last = -1;
				}
					
				if(last != -1)_grid[pos++, j] = _grid[last, j];
					
				for(int i = pos; i < 4; i++)
					_grid[i, j] = 0;
			}

            // Undo rotate : 
            switch (step)
            {
                case Step.up:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            grids_out[x, y] = _grid[x, y];
                        }
                    }
                    break;
                case Step.down:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            grids_out[x, y] = _grid[3 - x, y];
                        }
                    }
                    break;
                case Step.left:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            grids_out[x, y] = _grid[y, 3 - x];
                        }
                    }
                    break;
                case Step.right:
                    for (var x = 0; x < 4; x++)
                    {
                        for (var y = 0; y < 4; y++)
                        {
                            grids_out[x, y] = _grid[3 - y, x];
                        }
                    }
                    break;
            }

            // check
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if (grids_out[i, j] != grids[i, j])
                    {
                        canMove = true;
                        break;
                    }
                }
				
				if (canMove) break;
            }

            Res = GetResult(grids_out);

            return canMove;
        }

        // get ugly insertion
        private void AIGetNextINsertion(int[,] grids, int[,] grids_out, ref double Res)
        {
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    grids_out[i, j] = grids[i, j];
                }
            }

            double tmp;
            double minVal = 1.0 * Const.INF;
            int[] ind = new int[3];
            for(int i = 0; i < 4; ++i)
            {
                for(int j = 0; j < 4; ++j)
                {
                    if (grids_out[i, j] != 0)
                        continue;
                    else
                    {
                        grids_out[i, j] = 2;
                        tmp = GetResult(grids_out);
                        if(tmp < minVal)
                        {
                            minVal = tmp;
                            ind[0] = i;
                            ind[1] = j;
                            ind[2] = 2;
                        }

                        grids_out[i, j] = 4;
                        tmp = GetResult(grids_out);
                        if (tmp < minVal)
                        {
                            minVal = tmp;
                            ind[0] = i;
                            ind[1] = j;
                            ind[2] = 4;
                        }

                        grids_out[i, j] = 0;
                    }
                }
            }

            // The worst insertion
            grids_out[ind[0], ind[1]] = ind[2];
            Res = minVal;
        }

        // Evaluations


        // Measure how smmothness the grid is
        private double GetSmoothness(int[,] gridIIn)
        {
            double Res = 0;
            double ThisVal = 0;
            double NextVal = 0;
            for(int i = 0; i < 4; ++i)
            {
                for(int j = 0 ; j < 4; ++j)
                {
                    if(gridIIn[i, j] != 0)
                    {
                        ThisVal = Math.Log(gridIIn[i, j], 2);
                        for(int inext = i + 1; inext < 4; ++inext)
                        {
                            if(gridIIn[inext, j] != 0)
                            {
                                NextVal = Math.Log(gridIIn[inext, j], 2);
                                Res -= Math.Abs(ThisVal - NextVal);
                                break;
                            }
                        }
                        //for (int jnext = i + 1; jnext < 4; ++jnext)
                        for (int jnext = j + 1; jnext < 4; ++jnext)
                        {
                            if (gridIIn[i, jnext] != 0)
                            {
                                NextVal = Math.Log(gridIIn[i, jnext], 2);
                                Res -= Math.Abs(ThisVal - NextVal);
                                break;
                            }
                        }
                    }
                }
            }
            return Res;
        }

        // how many empty spaces
        private double GetEmptyNum(int[,] gridIn) // 0 - 16
        {
            double Res = 0;
            for(int i = 0; i < 4; ++i)
            {
                for(int j = 0; j < 4; ++j)
                {
                    if(gridIn[i, j] == 0)
                    {
                        Res++;
                    }
                }
            }
            return Res;
        }

        // the maxNum
        private double GetMaxNum(int[,] gridIn)
        {
            double Res = gridIn[0, 0];
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if (gridIn[i, j] > Res)
                    {
                        Res = gridIn[i, j];
                    }
                }
            }
            return Math.Log(Res, 2);
        }

        // The monotonic
        private double GetMono(int[,] gridIn)
        {
            double ThisVal = 0;
            double NextVal = 0;
            double[] Result = new double[] { 0, 0, 0, 0 };
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    if (gridIn[i, j] != 0)
                    {
                        ThisVal = Math.Log(gridIn[i, j], 2);
                        for (int inext = i + 1; inext < 4; ++inext)
                        {
                            if (gridIn[inext, j] != 0)
                            {
                                NextVal = Math.Log(gridIn[inext, j], 2);
                                if (ThisVal > NextVal)
                                    Result[0] += NextVal - ThisVal;
                                else
                                    Result[1] += ThisVal - NextVal;
                                break;
                            }
                        }
                        //for (int jnext = i + 1; jnext < 4; ++jnext)
						for (int jnext = j + 1; jnext < 4; ++jnext)
                        {
                            if (gridIn[i, jnext] != 0)
                            {
                                NextVal = Math.Log(gridIn[i, jnext], 2);
                                if (ThisVal > NextVal)
                                    Result[2] += NextVal - ThisVal;
                                else
                                    Result[3] += ThisVal - NextVal;
                                break;
                            }
                        }
                    }
                }
            }
            return Math.Max(Result[0], Result[1]) + Math.Max(Result[2], Result[3]);
        }

        private double WhereIsMax(int[,] gridIn)
        {
            int maxi = 0, maxj = 0;
            for(int i = 0; i < 4; i++)
                for(int j = 0; j < 4; j++)
                {
                    if(gridIn[maxi, maxj] <= gridIn[i, j])
                    {
						if(gridIn[maxi, maxj] == gridIn[i, j])
						{
							if (maxi == 0 && (maxj == 0 || maxj == 3)) continue;
							if (maxi == 3 && (maxj == 0 || maxj == 3)) continue;
							if (i != 0 && i != 3)continue;
							if (j != 0 && j != 3)continue;
						}
                        maxi = i;
                        maxj = j;
                    }
                }
            if (maxi == 0 && (maxj == 0 || maxj == 3)) return 15;
            if (maxi == 3 && (maxj == 0 || maxj == 3)) return 15;
            if (maxi == 0 || maxi == 3 || maxj == 0 || maxj == 3) return 1;
            return 0;
        }

        private double DistinctMax(int[,] gridIn)
        {
            int[] num = new int[16];

            for (int i = 0; i < 16; i++)
                num[i] = 0;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    int res = gridIn[i, j], idx = 0;
                    while(res != 0)
                    {
                        idx++;
                        res >>= 1;
                    }
                    num[idx]++;
                }

            double cost = 0;

            for (int i = 0; i < 16; i++)
                cost -= num[i] * i * i * i;

            return cost * 0.1;
        }
    }
}