using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace _2048AI
{
    public class Estimation
    {
        public static double Wemptyblock = 0.35;
        public static double Wsmoothness = 0.3;
        public static double Wscore = 0.15;
        public static double Wmono = 0.2;
        private static int[,] gridcopy = new int[4, 4];
        //private static int times = 0;
        private static void printcheckboard(int[,] grids, String filepath)
        {
            StreamWriter sw = new StreamWriter(filepath, true);
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

        private static double monotonicity(int[,] grids)
        {
            double monii, monid, mondd, mondi;
            monii = monid = mondd = mondi = 0;
            double rowi, coli;
            //double maindiai, vicediai;
            rowi = 0;
            coli = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 0)
                        grids[i, j] = 1;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (grids[i, j] == 0)
                        continue;
                    if (i < 3 && grids[i + 1, j] != 0)
                        coli += (Math.Sqrt(grids[i + 1, j]) - Math.Sqrt(grids[i, j])) * (j == 0 || j == 3 ? 1 : 0.95);
                    if (j < 3 && grids[i, j + 1] != 0)
                        rowi += (Math.Sqrt(grids[i, j + 1]) - Math.Sqrt(grids[i, j])) * (i == 0 || i == 3 ? 1 : 0.95);

                }
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 1)
                        grids[i, j] = 0;
            monii = coli + rowi;
            monid = coli - rowi;
            mondd = -coli - rowi;
            mondi = -coli + rowi;
            return Math.Max(Math.Max(monii, monid), Math.Max(mondd, mondi)) - Math.Min(Math.Min(monii, monid), Math.Min(mondd, mondi));
        }

        private static double smoothness(int[,] grids)
        {
            double smooth = 0;
            /*
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 0)
                        grids[i, j] = 1;*/
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    if (grids[i, j] == 0)
                        continue;
                    if (i > 0 && grids[i - 1, j] != 0)
                        smooth += Math.Abs(Math.Log(grids[i, j]) / Math.Log(2) - Math.Log(grids[i - 1, j]) / Math.Log(2));
                    if (i < 3 && grids[i + 1, j] != 0)
                        smooth += Math.Abs(Math.Log(grids[i, j]) / Math.Log(2) - Math.Log(grids[i + 1, j]) / Math.Log(2));
                    if (j > 0 && grids[i, j - 1] != 0)
                        smooth += Math.Abs(Math.Log(grids[i, j]) / Math.Log(2) - Math.Log(grids[i, j - 1]) / Math.Log(2));
                    if (j < 3 && grids[i, j + 1] != 0)
                        smooth += Math.Abs(Math.Log(grids[i, j]) / Math.Log(2) - Math.Log(grids[i, j + 1]) / Math.Log(2));
                }
            /*
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 1)
                        grids[i, j] = 0;
             */
            smooth *= -1;
            return smooth;
        }
        private static int emptyblocknum(int[,] grids)
        {
            int sum = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grids[i, j] == 0 || grids[i, j] == 1)
                        sum++;
            return sum;
        }
        public static double aftermove(int[,] grids, int move)
        {
            //grids[1, 1] = 9;
            int score = 0;
            switch (move)
            {
                case 0:
                    //up
                    for (int t = 0; t < 3; t++)
                        for (int i = 0; i < 4; i++)
                            for (int j = 0; j < 3; j++)
                            {
                                if (grids[j, i] == 0)
                                {
                                    grids[j, i] = grids[j + 1, i];
                                    grids[j + 1, i] = 0;
                                }
                            }
                    for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (grids[j, i] == 0)
                                break;
                            if (grids[j, i] == grids[j + 1, i])
                            {
                                score += grids[j, i];
                                grids[j, i] += grids[j, i];
                                grids[j + 1, i] = 0;
                                for (int k = j + 1; k < 3; k++)
                                {
                                    grids[k, i] = grids[k + 1, i];
                                    grids[k + 1, i] = 0;
                                }
                            }
                        }
                    break;
                case 1:
                    //right
                    for (int t = 0; t < 3; t++)
                        for (int i = 0; i < 4; i++)
                            for (int j = 3; j > 0; j--)
                            {
                                if (grids[i, j] == 0)
                                {
                                    grids[i, j] = grids[i, j - 1];
                                    grids[i, j - 1] = 0;
                                }
                            }
                    for (int i = 0; i < 4; i++)
                        for (int j = 3; j > 0; j--)
                        {
                            if (grids[i, j] == 0)
                                break;
                            if (grids[i, j] == grids[i, j - 1])
                            {
                                score += grids[i, j];
                                grids[i, j] += grids[i, j];
                                grids[i, j - 1] = 0;
                                for (int k = j - 1; k > 0; k--)
                                {
                                    grids[i, k] = grids[i, k - 1];
                                    grids[i, k - 1] = 0;
                                }
                            }
                        }
                    break;
                case 2:
                    //down
                    for (int t = 0; t < 3; t++)
                        for (int i = 0; i < 4; i++)
                            for (int j = 3; j > 0; j--)
                            {
                                if (grids[j, i] == 0)
                                {
                                    grids[j, i] = grids[j - 1, i];
                                    grids[j - 1, i] = 0;
                                }
                            }
                    for (int i = 0; i < 4; i++)
                        for (int j = 3; j > 0; j--)
                        {
                            if (grids[j, i] == 0)
                                break;
                            if (grids[j, i] == grids[j - 1, i])
                            {
                                score += grids[j, i];
                                grids[j, i] += grids[j, i];
                                grids[j - 1, i] = 0;
                                for (int k = j - 1; k > 0; k--)
                                {
                                    grids[k, i] = grids[k - 1, i];
                                    grids[k - 1, i] = 0;
                                }
                            }
                        }
                    break;
                case 3:
                    //left
                    for (int t = 0; t < 3; t++)
                        for (int i = 0; i < 4; i++)
                            for (int j = 0; j < 3; j++)
                            {
                                if (grids[i, j] == 0)
                                {
                                    grids[i, j] = grids[i, j + 1];
                                    grids[i, j + 1] = 0;
                                }
                            }
                    for (int i = 0; i < 4; i++)
                        for (int j = 0; j < 3; j++)
                        {
                            if (grids[i, j] == 0)
                                break;
                            if (grids[i, j] == grids[i, j + 1])
                            {
                                score += grids[i, j];
                                grids[i, j] += grids[i, j];
                                grids[i, j + 1] = 0;
                                for (int k = j + 1; k < 3; k++)
                                {
                                    grids[i, k] = grids[i, k + 1];
                                    grids[i, k + 1] = 0;
                                }
                            }
                        }
                    break;
            }

            double logscore = 0;
            if (score > 0) logscore = Math.Log(score) / Math.Log(2);
            double emptyblock = emptyblocknum(grids);
            double smoothnessval = smoothness(grids);
            double monotocityval = monotonicity(grids);
            double heurval = Wemptyblock * emptyblock + Wsmoothness * smoothnessval + Wscore * logscore + Wmono * monotocityval;
            //Console.WriteLine("score: " + logscore + " empty:" + Wemptyblock * emptyblock + " emptyNum:" + emptyblock + " smooth:" + Wsmoothness * smoothnessval + " monoto:" + Wmono * monotocityval);

            return heurval;
        }
        public static bool samestate(int[,] grid1, int[,] grid2)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (grid1[i, j] != grid2[i, j])
                        return false;
            return true;
        }
    }
}
