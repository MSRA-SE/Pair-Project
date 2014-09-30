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
            int dir = AINextMove1(grids);
            context.Response.Write(dir.ToString());
        }

        private static int[,] LastGrids;
        private static bool GoLeft;
        /// <summary>
        /// interface for your AI code 
        /// </summary>
        /// <param name="grids">array of the current state for 2048, 0 elment for empty grid</param>
        /// <returns>0 for up </returns>
        /// <returns>1 for right </returns>
        /// <returns>2 for down </returns>
        /// <returns>3 for left</returns>
        private int AINextMove1(int[,] grids)
        {
            return Core.GetProposeMove(grids);
            //return new Random(DateTime.Now.Millisecond).Next() % 4;
        }
        private int AINextMove2(int[,] grids)
        {
            int next = 1;
            if (LastGrids == null)
            {
                LastGrids = new int[4, 4];
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        LastGrids[i, j] = 0;
                GoLeft = true;
            }
            if (IsEqualtoLast(grids))
            {
                next = new Random(DateTime.Now.Millisecond).Next() % 4;
                //GoLeft = !GoLeft;
            }
               

            else
            {
                if (GoLeft)
                {
                    GoLeft = false;
                    next = 3;
                }
                else
                {
                    GoLeft = true;
                    next = 2;
                }
            }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    LastGrids[i, j] = grids[i, j];
                }
            return next;
            //return new Random(DateTime.Now.Millisecond).Next() % 4;
        }

        private bool IsEqualtoLast(int[,] grids)
        {
            for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        if (grids[i, j] != LastGrids[i, j])
                            return false;
                    }
            return true;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}