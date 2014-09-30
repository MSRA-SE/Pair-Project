using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048AI
{
    /// <returns>0 for up </returns>
    /// <returns>1 for right </returns>
    /// <returns>2 for down </returns>
    /// <returns>3 for left</returns>
    public class AI
    {
        public static void print(int[,] grid)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    Console.Write(grid[i, j] + "\t");
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void Main()
        {
            Estimation.Wemptyblock = 0.0;
            Estimation.Wsmoothness = 0.0;
            Estimation.Wscore = 0.0;
            Estimation.Wmono = 1;
            int[,] grid = new int[,] {  { 1024, 0, 0, 0 }, 
                                        { 4, 2, 0, 0 }, 
                                        { 2, 0, 0, 0 }, 
                                        { 2, 0, 0, 0 } };

            //Estimation.aftermove(grid, 3);

            Searcher.step = 0;
            while(true)
            {
                print(grid);
                int move = Searcher.dfs(grid, Searcher.PLAYER, 0,Double.MaxValue).step;
                Console.WriteLine(move);
                Estimation.aftermove(grid, move);
                print(grid);
                Console.WriteLine("----------------------------");
                Console.Read();
            }

        }

       private int AINextMove(int[,] grids)
       {
           return 0;
       }
    }

    public class Searcher
    {
        public class Situation
        {
            public double value;
            public int step;
            public Situation(Double value)
            {
                this.value = value;
                this.step = -1;
            }
            public void min(double minimum,int step)
            {
                if (minimum < this.value)
                {
                    this.step = step;
                    this.value = minimum;
                }
            }
            public void max(double maximum, int step)
            {
                if (maximum > this.value)
                {
                    this.step = step;
                    this.value = maximum;
                }
            }
        };

        public static int step;
        public static int COMPUTER = 1;
        public static int PLAYER = 0;
        public static int[] randomWay()
        {
            Random ran = new Random();
            int[] way = new int[] { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++)
            {
                int j = ran.Next(0,3);
                int t = way[i];
                way[i] = way[j];
                way[j] = t;
            }
            /*
            for (int i = 0; i < 4; i++)
            {
                Console.Write(way[i]);
            }
            Console.WriteLine();
            */
            return way;
        }

        public static List<int[,]> computerAI(int[,] grids)
        {
            List<int[,]> next = new List<int[,]>();
            for (int i = 0; i < 4; i++)
            { 
                for(int j = 0;j<4;j++)
                    if (grids[i, j] == 0)
                    { 
                        grids[i,j] = 2;
                        next.Add((int[,])grids.Clone());
                        grids[i, j] = 4;
                        next.Add((int[,])grids.Clone());
                        grids[i, j] = 0;
                    }
            }

            Random ran = new Random();
            for (int i = 0; i < next.Count; i++)
            {
                int j = ran.Next(0, next.Count - 1);
                int[,] t = next[i];
                next[i] = next[j];
                next[j] = t;
            }
            return next;
        }

        public static Situation dfs(int[,] grids,int player,int deep,double value)
        {
            if (deep == step)
            {
                int[,] next = (int[,])grids.Clone();
                Situation fin = new Situation(Double.MinValue);
                Situation s = new Situation(Double.MinValue);
                bool flag = false;
                for (int i = 0; i < 4; i++)
                { 
                    int[,] nextMove = (int[,])grids.Clone();
                    double score = Estimation.aftermove(nextMove, i);
                    s.max(score, i);
                    if (Estimation.samestate(nextMove,next)) continue;
                    flag = true;
                    fin.max(score, i);
                }
                if (flag)
                {
                    //Console.WriteLine("player:" + fin.step + " deep:" + deep + " value:" + fin.value);
                    return fin;
                }
                else
                {
                    //Console.WriteLine("player:" + s.step + " deep:" + deep + " value:" + s.value);
                    return s;
                }
            }

            if (player == COMPUTER)
            {
                List<int[,]> next = computerAI(grids);
                Situation s = new Situation(Double.MaxValue);

                for (int i = 0; i < next.Count; i++)
                {
                    //Console.WriteLine("computer:"+i+" deep:"+deep);
                    Situation nextSituation = dfs(next[i], PLAYER, deep + 1,s.value);
                    if (nextSituation.value < value)
                    {
                        //Console.WriteLine("Computer" + " " + s.minimum + " " + value + " " + step+" "+i);
                        
                        //return nextSituation;
                    }
                    s.min(nextSituation.value, i);
                    //s.value += nextSituation.value;
                }
                //s.value /= next.Count;
               // Console.WriteLine("computer:" + s.step + " deep:" + deep + " value:" + s.value);
                return s;
            }

            if (player == PLAYER)
            {   
                int[,] next = (int[,])grids.Clone();
                Situation s = new Situation(Double.MinValue);
                Situation ss = new Situation(Double.MinValue);
                bool flag = false;
                int[] way = randomWay();
                for (int k = 0; k < 4; k++)
                {

                    int i = way[k];
                    //Console.WriteLine("player:"+i+" deep:"+deep);
                    int[,] nextMove = (int[,])grids.Clone();
                    double score = Estimation.aftermove(nextMove, i);
                    ss.max(score, i);
                    if (Estimation.samestate(next, nextMove)) continue;
                    flag = true;
                    Situation nextSituation = dfs(nextMove, COMPUTER, deep,s.value);
                    if (nextSituation.value > value)
                    {
                        //Console.WriteLine("Player" + " " + s.maximum + " " + value + " " + step+" "+i);
                        //return nextSituation;
                    }
                    s.max(nextSituation.value, i);
                }
                if (!flag)
                {
                    //Console.WriteLine("player:" + ss.step + " deep:" + deep + " value:" + ss.value);
                    return ss;
                }
                else
                {
                    //Console.WriteLine("player:" + s.step + " deep:" + deep + " value:" + s.value);
                    return s;
                }
            }


            return new Situation(Double.MinValue);
        }


    }
}
