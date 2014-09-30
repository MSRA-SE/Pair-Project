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
            return Core.GetProposeMove(grids);
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