using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.IO;

namespace _2048AI
{
    /// <summary>
    /// Summary description for api
    /// </summary>
    public class api : IHttpHandler
    {
        //Weights for heuristic
        private double Wemptyblock, Wsmoothness, Wscore, Wmono;
        private static int[,] gridcopy = new int[4, 4];
        private static int times = 0;
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
        private void printcheckboard(int[,] grids,String filepath)
        {
            StreamWriter sw = new StreamWriter(filepath,true);
            sw.WriteLine(grids.Length);
             for (int i = 0; i < 4; i++)
                 for (int j = 0; j < 4; j++)
                 {
                     if (j != 3)
                         sw.Write(grids[i, j] + "\t");
                     else
                         sw.WriteLine(grids[i, j]);
                 }
             sw.WriteLine("============================");
             sw.Close();
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
            Searcher.step = 2;
            Estimation.Wemptyblock = 0.32;
            Estimation.Wsmoothness = 0.3;
            Estimation.Wscore = 0.12;
            Estimation.Wmono = 0.33;
            bool _2048appear = false;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] >= 2048)
                        _2048appear = true;

            if (_2048appear)
            {
                Estimation.Wemptyblock = 0.32;
                Estimation.Wsmoothness = 0.3;
                Estimation.Wscore = 0.12;
                Estimation.Wmono = 0.43;
            }
            return Searcher.dfs(grids, Searcher.PLAYER, 0, Double.MinValue).step;
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