using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace _2048AI
{
    /// <summary>
    /// Summary description for api
    /// </summary>
    public class api : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            var jsonStr = context.Request.QueryString["grid"];
            var cells = jsonStr.Split(new[] { ' ', '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            var grids = new int[4, 4];

            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                { 
                    grids[x, y] = cells[x * 4 + y];
                }
            }

            int dir = AINextMove(grids);
            context.Response.Write(dir.ToString());
        }

        /// <summary>
        /// interface for your AI code 
        /// </summary>
        /// <param name="grids">array of the current state for 2048, 0 elment for empty grid</param>
        /// <returns>0 for up </returns>
        /// <returns>1 for right </returns>
        /// <returns>2 for down </returns>
        /// <returns>3 for left</returns>
        private int AINextMove(int[,] grids)
        {
            return makeDecision(grids);
            return new Random(DateTime.Now.Millisecond).Next() % 4;
        }

        private bool equals(int[,] grids1, int[,] grids2)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids1[i, j] != grids2[i, j])
                        return false;
            return true;
        }

        private int[,] copy(int [,] grids)
        {
            int [,] g= new int[4,4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    g[i, j] = grids[i, j];
                }
            return g;
        }
        private int[,] human_move(int [,] grids, int move)
        {
            int[,] result = new int [4,4];
                
            if(move == 0)
            {
                for (int j = 0; j < 4;j++)
                {
                    int stack = grids[0,j];
                    int k=0;
                    for (int i = 1; i < 4;i++)
                    {
                        if (grids[i, j] != 0)
                        {
                            if (stack == 0)
                                stack = grids[i, j];
                            else if (stack == grids[i, j])
                            {
                                result[k, j] = stack << 1;
                                stack = 0;
                                k++;
                            }
                            else
                            {
                                result[k, j] = stack;
                                k++;
                                stack = grids[i, j];
                            }
                        }
                    }
                    if (stack != 0) result[k, j] = stack;
                }
            }
            else if(move == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    int stack = grids[i, 3];
                    int k = 3;
                    for (int j = 2; j >= 0; j--)
                    {
                        if (grids[i, j] != 0)
                        {
                            if (stack == 0)
                                stack = grids[i, j];
                            else if (stack == grids[i, j])
                            {
                                result[i, k] = stack << 1;
                                stack = 0;
                                k--;
                            }
                            else
                            {
                                result[i, k] = stack;
                                k--;
                                stack = grids[i, j];
                            }
                        }
                    }
                    if (stack != 0) result[i, k] = stack;
                }
            }
            else if(move == 2)
            {
                for (int j = 0; j < 4; j++)
                {
                    int stack = grids[3, j];
                    int k = 3;
                    for (int i = 2; i >= 0; i--)
                    {
                        if (grids[i, j] != 0)
                        {
                            if (stack == 0)
                                stack = grids[i, j];
                            else if (stack == grids[i, j])
                            {
                                result[k, j] = stack << 1;
                                stack = 0;
                                k--;
                            }
                            else
                            {
                                result[k, j] = stack;
                                k--;
                                stack = grids[i, j];
                            }
                        }
                    }
                    if (stack != 0) result[k, j] = stack;
                }
            }
            else if(move == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    int stack = grids[i, 0];
                    int k = 0;
                    for (int j = 1; j < 4; j++)
                    {
                        if (grids[i, j] != 0)
                        {
                            if (stack == 0)
                                stack = grids[i, j];
                            else if (stack == grids[i, j])
                            {
                                result[i, k] = stack << 1;
                                stack = 0;
                                k++;
                            }
                            else
                            {
                                result[i, k] = stack;
                                k++;
                                stack = grids[i, j];
                            }
                        }
                    }
                    if (stack != 0) result[i, k] = stack;
                }
            }
            return result;
        }

        private List<KeyValuePair<int[,], int>> getMoves(int [,] grids, bool human_turn)
        {
            List<KeyValuePair<int[,], int>> result = new List<KeyValuePair<int[,], int>>();
            int[,] next_grids;
            if(human_turn)
            {
                for (int k = 0; k < 4; k++)
                {
                    next_grids = human_move(grids, k);
                    if (!equals(next_grids, grids))
                        result.Add(new KeyValuePair<int[,], int>(next_grids, k));
                }
            }
            else
            {
                for(int i = 0 ; i < 4 ; i++)
                    for(int j = 0 ; j < 4 ; j++)
                        if(grids[i, j] == 0)
                        {
                            next_grids = copy(grids);
                            next_grids[i, j] = 2;
                            result.Add(new KeyValuePair<int[,], int>(next_grids, 0));

                            next_grids = copy(grids);
                            next_grids[i, j] = 4;
                            result.Add(new KeyValuePair<int[,], int>(next_grids, 0));
                        }
            }
            return result;
        }

        private bool isEnded(int[,] grids)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 0)
                        return false;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (j != 0 && grids[i, j] == grids[i, j - 1])
                        return false;
                    if (j != 3 && grids[i, j] == grids[i, j + 1])
                        return false;
                    if (i != 3 && grids[i, j] == grids[i + 1, j])
                        return false;
                    if (i != 0 && grids[i, j] == grids[i - 1, j])
                        return false;
                
                }
            return true;
        }

        private int evaluate(int[,] grids)
        {
            //TODO
            /*
            int ans = 0,count=0;
            int weight = 2048;
            for (int j = 0; j < 4; j++)
            {
                if (j % 2 == 0)
                {
                    for (int i = 3; i >= 0; i--)
                    {
                        if (grids[i, j] != 0)
                        {
                            count++;
                            ans += weight * grids[i, j];
                        }
                        weight = weight - 128;
                    }
                }
                else
                {
                    for (int i = 0; i <4; i++)
                    {
                        if (grids[i, j] != 0)
                        {
                            count++;
                            ans += weight * grids[i, j];
                        }
                        weight = weight - 128;
                    }
                }
            }
            ans /= count;
             * */

            int ans = 0;

            int mon = 0;
            //calculate monotonicity
            for (int i=0;i<4;i=i+2)
            {
                for (int j=0;j<4;j++)
                {
                    int tmp = grids[i, j] << j;
                    mon += tmp;
                }
                for (int j = 0; j < 4; j++)
                {
                    //int tmp = grids[i+1, j] << (4-j);
                    int tmp = grids[i + 1, j] << j;
                    mon += tmp;
                }
            }
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    int tmp = grids[i, j] << i;
                    mon += tmp;
                }
            }

            int smooth = 0;
            
            
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int tmp = grids[i, j] - grids[i-1, j];
                    if (grids[i, j] == 0) tmp = 0;
                    if (grids[i-1, j] == 0) tmp = 0;
                    if (tmp < 0) tmp = -tmp;
                    smooth += tmp;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 1; j < 4; j++)
                {
                    int tmp = grids[i, j] - grids[i, j - 1];
                    if (grids[i, j] == 0) tmp = 0;
                    if (grids[i, j-1] == 0) tmp = 0;
                    if (tmp < 0) tmp = -tmp;
                    smooth += tmp;
                }
            }

            int block = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (grids[i, j] == 0)
                    {
                        if (i > 0) block += grids[i - 1, j];
                        if (j > 0) block += grids[i, j - 1];
                        if (i < 3) block += grids[i + 1, j];
                        if (j < 3) block += grids[i, j + 1];
                    }
                }
            }

            ans = (int)(1.0 * mon - 0.1*smooth + 0.8*block);
            return ans;
        }
        private KeyValuePair<int, int> alphaBeta(int[,] grids , int depth, int alpha, int beta, bool human_turn)
        {
            int value;
            if(depth == 0 || isEnded(grids))
            {
                value = evaluate(grids);
                return new KeyValuePair<int, int>(-1, value);
            }
            var next_gridss = getMoves(grids, human_turn);

            int best = -INF - 1;
            int move = 0;
            
            foreach (KeyValuePair<int[,], int> next_grids in next_gridss)
            {
                value = -alphaBeta(next_grids.Key, depth - 1, -beta, -alpha, !human_turn).Value;
                if (value > best)
                {
                    best = value;
                    move = next_grids.Value;
                }
                if (best > alpha)
                    alpha = best;
                if (best >= beta)
                    break;
            }
            return new KeyValuePair<int,int>(move, best);
        }

        private int makeDecision(int[,] grids)
        {
            int count=0;
            for (int i=0;i<4;i++)
                for (int j=0;j<4;j++)
                {
                    if (grids[i, j] != 0) count++;
                }
            KeyValuePair<int, int> pair;
            if (count <=8)
                pair = alphaBeta(grids, 6, -INF, INF, true);
            else
                pair = alphaBeta(grids, 6, -INF, INF, true);
            return pair.Key;
        }

        const int INF = 99999999;
        
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}