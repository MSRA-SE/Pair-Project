using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace _2048AI
{
    public static class Core
    {
        public static UInt64[] CacheMoveUp;
        public static UInt64[] CacheMoveDown;
        public static UInt64[] CacheMoveLeft;
        public static UInt64[] CacheMoveRight;
        public static double[] CacheScore;
        public static int[] CacheEmpty;

        private static List<Dictionary<UInt64, double>> TransTable;

        public const double ProbThreshold = 0.0005;
        public const int LayerThreshold = 4;
        static Core()
        {
            CacheMoveUp = new UInt64[65536];
            CacheMoveDown = new UInt64[65536];
            CacheMoveLeft = new UInt64[65536];
            CacheMoveRight = new UInt64[65536];
            _CacheMove();

            CacheEmpty = new int[65536];
            _CacheEmpty();

            CacheScore = new double[65536];
            _CacheScore();

            TransTable = new List<Dictionary<ulong, double>>(4);
            TransTable.Add(new Dictionary<UInt64, double>());
            TransTable.Add(new Dictionary<UInt64, double>());
            TransTable.Add(new Dictionary<UInt64, double>());
            TransTable.Add(new Dictionary<UInt64, double>());
        }
        private static void _CacheEmpty()
        {
            for (int i = 0; i < 65536; i++)
            {
                UInt64[] num = new UInt64[4];
                for (int j = 0; j < 4; j++)
                {
                    num[j] = (UInt64)((i >> (j * 4)) & 0xf);
                }

                int empty = 0;
                foreach (int tmp in num)
                {
                    if (tmp == 0) empty++;
                }
                CacheEmpty[i] = empty;
            }
        }

        private static void _CacheScore()
        {
            for (int i = 0; i < 65536; i++)
            {
                int[] num = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    num[j] = (int)((i >> (j * 4)) & 0xf);
                }
               
                double score1 = 0;
                double score2 = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (num[j] == 0) continue;
                    int k = j + 1;

                    if (num[j] < num[k])
                    {
                        score1 += Math.Pow(2, num[k]) * (num[k] - num[j]) * 9.5;
                    }
                    else if (num[j] > num[k])
                    {
                        score2 += Math.Pow(2, num[j]) * (num[j] - num[k]) * 9.5;
                    }
                    else
                    {
                        score1 -= 32;
                        score2 -= 32;
                    }
                }
                CacheScore[i] = -Math.Min(score1, score2) * 48;
                for (int j = 0; j < 4; j++)
                {
                    CacheScore[i] -= num[j] * num[j] * 220;
                }
                CacheScore[i] += CacheEmpty[i] * 300;
            }
        }
        private static void _CacheMove()
        {
            for (int i = 0; i < 65536; i++)
            {
                UInt64[] num = new UInt64[4];
                for (int j = 0; j < 4; j++)
                {
                    num[j] = (UInt64)((i >> (j * 4)) & 0xf);
                }

                for (int j1 = 0; j1 < 4; j1++)
                {
                    int j2;
                    for (j2 = j1 + 1; j2 < 4; j2++)
                    {
                        if (num[j2] != 0) break;
                    }
                    if (j2 == 4) break;
                    if (num[j1] == 0)
                    {
                        num[j1] = num[j2];
                        num[j2] = 0;
                        j1--;
                    }
                    else if (num[j1] == num[j2])
                    {
                        num[j1]++;
                        num[j2] = 0;
                    }
                }

                int rev_i = ((i << 12) | ((i << 4) & 0x0f00) | ((i >> 4) & 0x00f0) | (i >> 12)) & 0xffff;

                CacheMoveLeft[i] = (num[0] << 0) | (num[1] << 4) | (num[2] << 8) | (num[3] << 12);
                CacheMoveRight[rev_i] = (num[0] << 12) | (num[1] << 8) | (num[2] << 4) | (num[3] << 0);
                CacheMoveUp[i] = (num[0] << 0) | (num[1] << 16) | (num[2] << 32) | (num[3] << 48);
                CacheMoveDown[rev_i] = (num[0] << 48) | (num[1] << 32) | (num[2] << 16) | (num[3] << 0);
            }
        }

        private static UInt64 Log2(int num)
        {
            if (num == 0) return 0;
            int tmp = 1;
            UInt64 result = 0;
            while (tmp < num)
            {
                tmp *= 2;
                result++;
            }
            return result;
        }

        public static UInt64 Convert(int[,] grid)
        {
            UInt64 result = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    result |= Log2(grid[i, j]) << (4 * (4 * i + j));
            return result;
        }

        public static UInt64 Move(UInt64 grid, int direction)
        {
            UInt64 result = 0;
            switch(direction)
            {
                case 0: //up
                    grid = Transpose(grid);
                    result |= CacheMoveUp[grid & 0xffff];
                    result |= CacheMoveUp[(grid >> 16) & 0xffff] << 4;
                    result |= CacheMoveUp[(grid >> 32) & 0xffff] << 8;
                    result |= CacheMoveUp[(grid >> 48) & 0xffff] << 12;
                    break;
                case 1: //right
                    result |= CacheMoveRight[grid & 0xffff];
                    result |= CacheMoveRight[(grid >> 16) & 0xffff] << 16;
                    result |= CacheMoveRight[(grid >> 32) & 0xffff] << 32;
                    result |= CacheMoveRight[(grid >> 48) & 0xffff] << 48;
                    break; 
                case 2: //down
                    grid = Transpose(grid);
                    result |= CacheMoveDown[grid & 0xffff];
                    result |= CacheMoveDown[(grid >> 16) & 0xffff] << 4;
                    result |= CacheMoveDown[(grid >> 32) & 0xffff] << 8;
                    result |= CacheMoveDown[(grid >> 48) & 0xffff] << 12;
                    break;
                case 3: //left
                    result |= CacheMoveLeft[grid & 0xffff];
                    result |= CacheMoveLeft[(grid >> 16) & 0xffff] << 16;
                    result |= CacheMoveLeft[(grid >> 32) & 0xffff] << 32;
                    result |= CacheMoveLeft[(grid >> 48) & 0xffff] << 48;
                    break;
            }
            return result;
        }

        private static UInt64 Transpose(UInt64 grid)
        {
            UInt64 tmp = (grid & 0xf0f00f0ff0f00f0f) | 
                ((grid & 0x0000f0f00000f0f0) << 12) |
                ((grid & 0x0f0f00000f0f0000) >> 12);
            UInt64 result = (tmp & 0xff00ff0000ff00ff) |
                ((tmp & 0x00000000ff00ff00) << 24) |
                ((tmp & 0x00ff00ff00000000) >> 24);
            return result;
        }

        private static double GetHeurScore(UInt64 grid)
        {
            double score = 0;
            score += CacheScore[grid & 0xffff];
            score += CacheScore[(grid >> 16) & 0xffff];
            score += CacheScore[(grid >> 32) & 0xffff];
            score += CacheScore[(grid >> 48) & 0xffff];
            grid = Transpose(grid);
            score += CacheScore[grid & 0xffff];
            score += CacheScore[(grid >> 16) & 0xffff];
            score += CacheScore[(grid >> 32) & 0xffff];
            score += CacheScore[(grid >> 48) & 0xffff];
            return score;
        }

        private static int GetEmpty(UInt64 grid)
        {
            int empty = 0;
            empty += CacheEmpty[grid & 0xffff];
            empty += CacheEmpty[(grid >> 16) & 0xffff];
            empty += CacheEmpty[(grid >> 32) & 0xffff];
            empty += CacheEmpty[(grid >> 48) & 0xffff];
            return empty;
        }
        private static double GetPanHeurScore(UInt64 grid)
        {
            double score = GetHeurScore(grid) ;
            return score - 20000000;
        }
        private static double GetAvgGridScore(UInt64 grid, double prob, int layer, Dictionary<UInt64, double> transTable)
        {
            int empty = GetEmpty(grid);
            if (empty == 0)
            {
                return GetMoveScore(grid, prob, layer, transTable);
            }
            if (prob < ProbThreshold || layer == 0)
            {
                return GetHeurScore(grid);
            }
            double score = 0;
            try
            {
                score = transTable[grid];
                return score;
            }
            catch { }
            

            prob /= empty;

            
            UInt64 tmp = grid;
            UInt64 tile2 = 1;
            while (tile2 != 0)
            {
                if ((tmp & 0xf) == 0)
                {
                    score += GetMoveScore(grid | tile2, prob * 0.9, layer, transTable) * 0.9;
                    score += GetMoveScore(grid | (tile2 << 1), prob * 0.1, layer, transTable) * 0.1;
                }
                tmp >>= 4;
                tile2 <<= 4;
            }
            score /= empty;
            try
            {
                transTable.Add(grid, score);
            }
            catch { }

            return score;
        }

        private static double GetMoveScore(UInt64 grid, double prob, int layer, Dictionary<UInt64, double> transTable)
        {
            double maxScore = double.MinValue;

            for (int i = 0; i < 4; i++)
            {
                UInt64 aftermove = Move(grid, i);
                
                if (aftermove == grid)
                    continue;
                double score = GetAvgGridScore(aftermove, prob, layer - 1, transTable);
                if (maxScore <= score)
                    maxScore = score;
            }
            if (maxScore == double.MinValue)
            {
                return GetPanHeurScore(grid);
            }
            return maxScore;
        }

        private static int PredictLayer(int[,] grids)
        {
            int[] count = new int[20];
            for (int i = 0; i < 20; i++)
            {
                count[i] = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    count[Log2(grids[i, j])]++;
                }
            }
            int n = 0;
            for (int i = 1; i < 20; i++)
            {
                if (count[i] != 0)
                    n++;
            }
            return n - 4;
        }

        public static int GetProposeMove(int[,] grids)
        {
            UInt64 grid = Convert(grids);
            int direction = -1;
            double maxScore = double.MinValue;
            double[] score = new double[4];
  
            int layers = Math.Max(PredictLayer(grids), LayerThreshold);

            Parallel.For(0, 4, i =>
            {
                score[i] = double.MinValue;
                UInt64 aftermove = Move(grid, i);
                if (aftermove != grid)
                {
                    score[i] = GetAvgGridScore(aftermove, 1, layers, TransTable[i]);
                }
            });

            for (int i = 0; i < 4; i++)
            {
                if (maxScore <= score[i])
                {
                    maxScore = score[i];
                    direction = i;
                }
            }

            for (int i = 0; i < 4; i++)
                TransTable[i].Clear();
            return direction;
        }


    }
}