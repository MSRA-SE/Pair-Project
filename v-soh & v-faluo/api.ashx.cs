using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;

namespace _2048AI
{
    /// <summary>
    /// Summary description for api
    /// </summary>
    public class api : IHttpHandler
    {
        public static int targetdepth;
        public int[,] MoveResult = new int[4, 4];
        public int[, ,] ThreadMoveResult = new int[4, 4, 4];
        public int[,] Current = new int[4, 4];
        public float[] ThreadResult= new float[4];

        public int[] StepNum = new int[4];
        public int globald = 0;
        public static int PredictStep=0;
        public int snakevalue = 50;
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

        private int EvaluateCurrent_Base(int[,] grids)
        {
            int v = 0;
            int maxvalue;
            int[] list = new int[16];

            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4;y++ )
                {
                    if (grids[x, y] == 0) v = v + 20000;
                    list[x * 4 + y] = grids[x, y];
                    v += list[x * 4 + y] * snakevalue * 2;
                }

            
            for (var x = 0; x < 4;x++ )
            {
                maxvalue = 0;
                for (var y = 0; y < 4; y++)
                    if (grids[x,y] > maxvalue) maxvalue = grids[x,y];

                if(x==0 || x==2)
                    if (maxvalue == grids[x, 3]) v += 20000;
                if (x == 1 || x == 3)
                    if (maxvalue == grids[x, 0]) v += 20000;   

                //if (maxvalue == grids[x, 3]) v += 20000;
            }
             
            
            for (var y = 0; y < 4; y++)
            {
                maxvalue = 0;
                for (var x = 0; x < 4; x++)
                    if (grids[x, y] > maxvalue) maxvalue = grids[x, y];

                //if (maxvalue == grids[0, y]) v += 20000;
                if (maxvalue == grids[3, y]) v += 20000;
            }
            

            Array.Sort(list);
            // Smooth
            
            for (var y = 0; y < 4;y++)
            {

                v = v - Math.Abs(list[y] - grids[0, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[7 - y] - grids[1, y]) * snakevalue;
            }

            for (var y = 0; y < 4; y++)
            {
                v = v - Math.Abs(list[y + 8] - grids[2, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[15 - y] - grids[3, y]) * snakevalue;
            }
             

            
            // Range
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    var dist = 3 - x + y;
                    var rank = 0;
                    for(var z=15;z>=0;z--)
                    {
                        if(list[z]==grids[x,y])
                        {
                            rank = 15-z;
                            break;
                        }
                    }
                    if (dist == 0 && rank == 0) v = v + 16000;
                    if (dist == 1 && rank < 3 ) v = v + 8000;
                    if (dist == 2 && rank < 6 ) v = v + 2000;
                    if (dist == 3 && rank < 10 ) v = v + 1000;

                } 
            
            
            //Consistent
            int flag = 1;
            for (var y = 0; y < 4;y++)
            {
                if(list[15-y]!=grids[3,y])
                {
                    flag = 0; break;
                }
                v = v + 20000;
            }
            
            if(flag==1)
            {
                for(var y=3;y>=0;y--)
                {
                    if(list[12+y]!=grids[2,y])
                    {
                        flag = 0;
                        break;
                    }
                    if (y == 3) v = v + 30000;
                    v = v + 20000;
                }
            }
            

                return v;
        }

        private int EvaluateCurrent_Base2(int[,] grids)
        {
            int v = 0;
            int maxvalue;
            int[] list = new int[16];
            int flag;

            // blank
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (grids[x, y] == 0) v = v + 20000;
                    list[x * 4 + y] = grids[x, y];
                    v += list[x * 4 + y] * snakevalue * 2;
                }

            // max row column
            for (var x = 0; x < 4; x++)
            {
                maxvalue = 0;
                for (var y = 0; y < 4; y++)
                    if (grids[x, y] > maxvalue) maxvalue = grids[x, y];

                
                if (maxvalue == grids[x, 0]) v += 20000;
                if (maxvalue == grids[x, 3]) v += 20000;
            }


            for (var y = 0; y < 4; y++)
            {
                maxvalue = 0;
                for (var x = 0; x < 4; x++)
                    if (grids[x, y] > maxvalue) maxvalue = grids[x, y];

                if (maxvalue == grids[0, y]) v += 20000;
                if (maxvalue == grids[3, y]) v += 20000;
            }

            // adjacent
            for (var x = 0; x < 4; x++)
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] == grids[x, y - 1]) v += 10000;
                }

            for (var x = 1; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (grids[x, y] == grids[x-1, y]) v += 10000;
                }


            // increase decrease rom column
            int sum;
            for (var x = 0; x < 4; x++)
            {
                flag = 1;
                sum = grids[x,0];
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] < grids[x, y - 1]) flag =0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum*snakevalue;

                flag = 1;
                sum = grids[x, 0];
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] > grids[x, y - 1]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum*snakevalue;
            }

            for (var y = 0; y < 4; y++)
            {
                flag = 1;
                sum = grids[0, y];
                for (var x = 1; x < 4; x++)
                {
                    if (grids[x, y] < grids[x-1, y]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum*snakevalue;

                flag = 1;
                sum = grids[0, y];
                for (var x = 1; x < 4; x++)
                {
                    if (grids[x, y] > grids[x-1, y]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum*snakevalue;

            }

            

            Array.Sort(list);

            if(list[15]==grids[0,0])
            {
                v += 300000;
                if (list[14] == grids[1, 0] || list[14] == grids[0, 1]) v += 100000;
            }
            else
            if (list[15] == grids[3, 0])
            {
                v += 300000;
                if (list[14] == grids[3, 1] || list[14] == grids[2, 0]) v += 100000;
            }
            else
            if (list[15] == grids[0, 3])
            {
                v += 300000;
                if (list[14] == grids[1, 3] || list[14] == grids[0, 2]) v += 100000;
            }
            else
            if (list[15] == grids[3, 3])
            {
                v += 300000;
                if (list[14] == grids[2, 3] || list[14] == grids[3, 2]) v += 100000;
            }

            return v;
            // Smooth

            for (var y = 0; y < 4; y++)
            {

                v = v - Math.Abs(list[y] - grids[0, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[7 - y] - grids[1, y]) * snakevalue;
            }

            for (var y = 0; y < 4; y++)
            {
                v = v - Math.Abs(list[y + 8] - grids[2, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[15 - y] - grids[3, y]) * snakevalue;
            }



            // Range
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    var dist = 3 - x + y;
                    var rank = 0;
                    for (var z = 15; z >= 0; z--)
                    {
                        if (list[z] == grids[x, y])
                        {
                            rank = 15 - z;
                            break;
                        }
                    }
                    if (dist == 0 && rank == 0) v = v + 16000;
                    if (dist == 1 && rank < 3) v = v + 8000;
                    if (dist == 2 && rank < 6) v = v + 2000;
                    if (dist == 3 && rank < 10) v = v + 1000;

                }


            //Consistent
            flag = 1;
            for (var y = 0; y < 4; y++)
            {
                if (list[15 - y] != grids[3, y])
                {
                    flag = 0; break;
                }
                v = v + 20000;
            }

            if (flag == 1)
            {
                for (var y = 3; y >= 0; y--)
                {
                    if (list[12 + y] != grids[2, y])
                    {
                        flag = 0;
                        break;
                    }
                    if (y == 3) v = v + 30000;
                    v = v + 20000;
                }
            }


            return v;
        }

        private int EvaluateCurrent(int[,] grids)
        {
            int v = 0;
            int maxvalue;
            int[] list = new int[16];
            int flag;

            // blank
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (grids[x, y] == 0) v = v + 20000;
                    list[x * 4 + y] = grids[x, y];
                    v += list[x * 4 + y] * snakevalue * 2;
                }

            // max row column
            for (var x = 0; x < 4; x++)
            {
                maxvalue = 0;
                for (var y = 0; y < 4; y++)
                    if (grids[x, y] > maxvalue) maxvalue = grids[x, y];

                if (x == 0 || x == 2)
                    if (maxvalue == grids[x, 3]) v += 20000;
                if (x == 1 || x == 3)
                    if (maxvalue == grids[x, 0]) v += 20000;

                //if (maxvalue == grids[x, 3]) v += 20000;
            }

            // gravity
            for (var y = 0; y < 4; y++)
            {
                maxvalue = 0;
                for (var x = 0; x < 4; x++)
                    if (grids[x, y] > maxvalue) maxvalue = grids[x, y];

                //if (maxvalue == grids[0, y]) v += 20000;
                if (maxvalue == grids[3, y]) v += 20000;
            }

            // adjacent
            for (var x = 0; x < 4; x++)
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] == grids[x, y - 1]) v += 10000;
                }

            for (var x = 1; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (grids[x, y] == grids[x - 1, y]) v += 10000;
                }


            // increase decrease rom column
            int sum;
            for (var x = 0; x < 4; x++)
            {
                flag = 1;
                sum = grids[x, 0];
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] < grids[x, y - 1]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum * snakevalue/5;

                flag = 1;
                sum = grids[x, 0];
                for (var y = 1; y < 4; y++)
                {
                    if (grids[x, y] > grids[x, y - 1]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum * snakevalue/5;
            }

            for (var y = 0; y < 4; y++)
            {
                flag = 1;
                sum = grids[0, y];
                for (var x = 1; x < 4; x++)
                {
                    if (grids[x, y] < grids[x - 1, y]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum * snakevalue/5;

                flag = 1;
                sum = grids[0, y];
                for (var x = 1; x < 4; x++)
                {
                    if (grids[x, y] > grids[x - 1, y]) flag = 0;
                    sum += grids[x, y];
                }
                if (flag == 1) v += sum * snakevalue/5;

            }



            Array.Sort(list);
            // corner
            if (list[15] == grids[0, 0])
            {
                v += 300000;
                if (list[14] == grids[1, 0] || list[14] == grids[0, 1]) v += 100000;
                if (list[13] == grids[1, 0] || list[14] == grids[0, 1]) v += 40000;
            }
            else
                if (list[15] == grids[3, 0])
                {
                    v += 300000;
                    if (list[14] == grids[3, 1] || list[14] == grids[2, 0]) v += 100000;
                    if (list[13] == grids[3, 1] || list[14] == grids[2, 0]) v += 40000;
                }
                else
                    if (list[15] == grids[0, 3])
                    {
                        v += 300000;
                        if (list[14] == grids[1, 3] || list[14] == grids[0, 2]) v += 100000;
                        if (list[13] == grids[1, 3] || list[14] == grids[0, 2]) v += 40000;
                    }
                    else
                        if (list[15] == grids[3, 3])
                        {
                            v += 300000;
                            if (list[14] == grids[2, 3] || list[14] == grids[3, 2]) v += 100000;
                            if (list[13] == grids[2, 3] || list[14] == grids[3, 2]) v += 40000;
                        }

            
            // Smooth

            for (var y = 0; y < 4; y++)
            {

                v = v - Math.Abs(list[y] - grids[0, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[7 - y] - grids[1, y]) * snakevalue;
            }

            for (var y = 0; y < 4; y++)
            {
                v = v - Math.Abs(list[y + 8] - grids[2, y]) * snakevalue;
            }

            for (var y = 3; y >= 0; y--)
            {
                v = v - Math.Abs(list[15 - y] - grids[3, y]) * snakevalue;
            }



            // Range
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    var dist = 3 - x + y;
                    var rank = 0;
                    for (var z = 15; z >= 0; z--)
                    {
                        if (list[z] == grids[x, y])
                        {
                            rank = 15 - z;
                            break;
                        }
                    }
                    if (dist == 0 && rank == 0) v = v + 16000;
                    if (dist == 1 && rank < 3) v = v + 8000;
                    if (dist == 2 && rank < 6) v = v + 2000;
                    if (dist == 3 && rank < 10) v = v + 1000;

                }


            //Consistent
            flag = 1;
            for (var y = 0; y < 4; y++)
            {
                if (list[15 - y] != grids[3, y])
                {
                    flag = 0; break;
                }
                v = v + 20000;
            }

            if (flag == 1)
            {
                for (var y = 3; y >= 0; y--)
                {
                    if (list[12 + y] != grids[2, y])
                    {
                        flag = 0;
                        break;
                    }
                    if (y == 3) v = v + 30000;
                    v = v + 20000;
                }
            }


            return v;
        }
        private void copyto(int[,] S, int[,] D)
        {
            for(var x=0;x<4;x++)
                for(var y=0;y<4;y++)
                {
                    D[x, y] = S[x, y];
                }

        }

        private int checksame(int[,] S1,int[,] S2)
        {
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if(S1[x, y] != S2[x, y]) return 0;
                }
            return 1;
        }
        
        private int Move(int[,] grids, int direction)
        {
            //copyto(grids, MoveResult);
            int cm = 0;
            int[,] flag = new int[,]
                {
                  {0,0,0,0},  
                  {0,0,0,0},
                  {0,0,0,0},
                  {0,0,0,0}
                };

            copyto(flag, MoveResult);

            int[] temp = new int[4];

            if(direction == 3)  //up
            {
                for (var x = 0; x < 4; x++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var y = 0; y < 4; y++)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for(var y=1;y<4;y++)
                    {
                        if(temp[y]==temp[y-1] && temp[y]!=0)
                        {
                            temp[y - 1] = temp[y] * 2;
                            temp[y] = 0;
                            cm++;
                        }
                    }

                    p = 0;
                    for (var y = 0; y < 4; y++)
                        if (temp[y] != 0) MoveResult[x, p++] = temp[y];

                }

            }

            if (direction == 1)  //down
            {
                for (var x = 0; x < 4; x++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var y = 3; y >= 0; y--)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var y = 1; y < 4; y++)
                    {
                        if (temp[y] == temp[y - 1] && temp[y] != 0)
                        {
                            temp[y - 1] = temp[y] * 2;
                            temp[y] = 0;
                            cm++;
                        }
                    }

                    p = 3;
                    for (var y = 0; y < 4; y++)
                        if (temp[y] != 0) MoveResult[x, p--] = temp[y];

                }

            }

            if (direction == 2)  //right
            {
                for (var y = 0; y < 4; y++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var x = 3; x >= 0; x--)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var x = 1; x < 4; x++)
                    {
                        if (temp[x] == temp[x - 1] && temp[x] != 0)
                        {
                            temp[x - 1] = temp[x] * 2;
                            temp[x] = 0;
                            cm++;
                        }
                    }

                    p = 3;
                    for (var x = 0; x <4; x++)
                        if (temp[x] != 0) MoveResult[p--, y] = temp[x];

                }

            }

            if (direction == 0)  //left
            {
                for (var y = 0; y < 4; y++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var x = 0; x <4; x++)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var x = 1; x < 4; x++)
                    {
                        if (temp[x] == temp[x - 1] && temp[x] != 0)
                        {
                            temp[x - 1] = temp[x] * 2;
                            temp[x] = 0;
                            cm++;
                        }
                    }

                    p = 0;
                    for (var x = 0; x < 4; x++)
                        if (temp[x] != 0) MoveResult[p++, y] = temp[x];

                }

            }

            return cm;
        }

        private int Move(int[,] grids, int direction, int tid)
        {
            //copyto(grids, MoveResult);
            int cm = 0;
            int[,] flag = new int[,]
                {
                  {0,0,0,0},  
                  {0,0,0,0},
                  {0,0,0,0},
                  {0,0,0,0}
                };

            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                    ThreadMoveResult[tid, x, y] = 0;

             

            int[] temp = new int[4];

            if (direction == 3)  //up
            {
                for (var x = 0; x < 4; x++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var y = 0; y < 4; y++)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var y = 1; y < 4; y++)
                    {
                        if (temp[y] == temp[y - 1] && temp[y] != 0)
                        {
                            temp[y - 1] = temp[y] * 2;
                            temp[y] = 0;
                            cm++;
                        }
                    }

                    p = 0;
                    for (var y = 0; y < 4; y++)
                        if (temp[y] != 0) ThreadMoveResult[tid, x, p++] = temp[y];

                }

            }

            if (direction == 1)  //down
            {
                for (var x = 0; x < 4; x++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var y = 3; y >= 0; y--)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var y = 1; y < 4; y++)
                    {
                        if (temp[y] == temp[y - 1] && temp[y] != 0)
                        {
                            temp[y - 1] = temp[y] * 2;
                            temp[y] = 0;
                            cm++;
                        }
                    }

                    p = 3;
                    for (var y = 0; y < 4; y++)
                        if (temp[y] != 0) ThreadMoveResult[tid, x, p--] = temp[y];

                }

            }

            if (direction == 2)  //right
            {
                for (var y = 0; y < 4; y++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var x = 3; x >= 0; x--)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var x = 1; x < 4; x++)
                    {
                        if (temp[x] == temp[x - 1] && temp[x] != 0)
                        {
                            temp[x - 1] = temp[x] * 2;
                            temp[x] = 0;
                            cm++;
                        }
                    }

                    p = 3;
                    for (var x = 0; x < 4; x++)
                        if (temp[x] != 0) ThreadMoveResult[tid, p--, y] = temp[x];

                }

            }

            if (direction == 0)  //left
            {
                for (var y = 0; y < 4; y++)
                {
                    int p = 0;
                    temp[0] = temp[1] = temp[2] = temp[3] = 0;
                    for (var x = 0; x < 4; x++)
                        if (grids[x, y] != 0) temp[p++] = grids[x, y];

                    for (var x = 1; x < 4; x++)
                    {
                        if (temp[x] == temp[x - 1] && temp[x] != 0)
                        {
                            temp[x - 1] = temp[x] * 2;
                            temp[x] = 0;
                            cm++;
                        }
                    }

                    p = 0;
                    for (var x = 0; x < 4; x++)
                        if (temp[x] != 0) ThreadMoveResult[tid, p++, y] = temp[x];

                }

            }

            return cm;
        }

        public void Thread1()
        {
            float currentvalue = 0;
            int tid = 0;
            int counter = 0;
            int[,] tempgrid = new int[4, 4];
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    tempgrid[x, y] = ThreadMoveResult[tid, x, y];
                }


            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (tempgrid[x, y] == 0)
                    {
                        // generate 2  90%
                        tempgrid[x, y] = 2;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.9f;
                        // generate 4   10%
                        tempgrid[x, y] = 4;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.1f;
                        tempgrid[x, y] = 0;
                        counter++;
                    }

                }
            if (counter == 0)
            {
                //currentvalue += MyAI(tempgrid, depth + 1);
                currentvalue = 0;
            }
            else
            {
                currentvalue /= counter;
            }

            StepNum[tid] += counter;
            ThreadResult[tid] = currentvalue;

        }

        public void Thread2()
        {
            float currentvalue = 0;
            int tid = 1;
            int counter = 0;
            int[,] tempgrid = new int[4, 4];
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    tempgrid[x, y] = ThreadMoveResult[tid, x, y];
                }


            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (tempgrid[x, y] == 0)
                    {
                        // generate 2  90%
                        tempgrid[x, y] = 2;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.9f;
                        // generate 4   10%
                        tempgrid[x, y] = 4;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.1f;
                        tempgrid[x, y] = 0;
                        counter++;
                    }

                }
            if (counter == 0)
            {
                //currentvalue += MyAI(tempgrid, depth + 1);
                currentvalue = 0;
            }
            else
            {
                currentvalue /= counter;
            }

            StepNum[tid] += counter;
            ThreadResult[tid] = currentvalue;


        }

        public void Thread3()
        {
            float currentvalue = 0;
            int tid = 2;
            int counter = 0;
            int[,] tempgrid = new int[4, 4];
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    tempgrid[x, y] = ThreadMoveResult[tid, x, y];
                }


            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (tempgrid[x, y] == 0)
                    {
                        // generate 2  90%
                        tempgrid[x, y] = 2;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.9f;
                        // generate 4   10%
                        tempgrid[x, y] = 4;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.1f;
                        tempgrid[x, y] = 0;
                        counter++;
                    }

                }
            if (counter == 0)
            {
                //currentvalue += MyAI(tempgrid, depth + 1);
                currentvalue = 0;
            }
            else
            {
                currentvalue /= counter;
            }

            StepNum[tid] += counter;
            ThreadResult[tid] = currentvalue;


        }

        public void Thread4()
        {
            float currentvalue = 0;
            int tid = 3;
            int counter = 0;
            int[,] tempgrid = new int[4, 4];
            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    tempgrid[x, y] = ThreadMoveResult[tid, x, y];
                }


            for (var x = 0; x < 4; x++)
                for (var y = 0; y < 4; y++)
                {
                    if (tempgrid[x, y] == 0)
                    {
                        // generate 2  90%
                        tempgrid[x, y] = 2;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.9f;
                        // generate 4   10%
                        tempgrid[x, y] = 4;
                        currentvalue += MyAI(tempgrid, 1, tid) * 0.1f;
                        tempgrid[x, y] = 0;
                        counter++;
                    }

                }
            if (counter == 0)
            {
                //currentvalue += MyAI(tempgrid, depth + 1);
                currentvalue = 0;
            }
            else
            {
                currentvalue /= counter;
            }

            StepNum[tid] += counter;
            ThreadResult[tid] = currentvalue;


        }

        private float MyAI_Multithread(int[,] grids)
        {
            float bestchoice = 0;
            float maxvalue = 0;
            float currentvalue = 0;
            int[,] tempgrid = new int[4, 4];
            int[] flag = new int[] { 0, 0, 0, 0 };
            Thread t1 = new Thread(new ThreadStart(this.Thread1));
            Thread t2 = new Thread(new ThreadStart(this.Thread2));
            Thread t3 = new Thread(new ThreadStart(this.Thread3));
            Thread t4 = new Thread(new ThreadStart(this.Thread4));

            StepNum[0] = 0;
            StepNum[1] = 0;
            StepNum[2] = 0;
            StepNum[3] = 0;

            copyto(grids, Current);
            
            for (var d = 0; d < 4; d++)
            {
                int counter = 0;
                int cm;
                ThreadResult[d] = 0;

                cm = Move(grids, d);

                cm = checksame(grids, MoveResult);

                copyto(MoveResult, tempgrid);

                currentvalue = 0;

                if (cm == 0)
                {
                   // launch a new thread
                    for (var x = 0; x < 4; x++)
                        for (var y = 0; y < 4; y++)
                        {
                            ThreadMoveResult[d, x, y] = MoveResult[x, y];
                        }

                    flag[d] = 1;
                    if(d==0)
                    {
                        t1.Priority = ThreadPriority.Highest;
                        t1.Name = "2048 1";
                        t1.Start();
                        
                    }

                    if (d == 1)
                    {
                        t2.Priority = ThreadPriority.Highest;
                        t2.Name = "2048 2";
                        t2.Start();
                    }

                    if (d == 2)
                    {
                        t3.Priority = ThreadPriority.Highest;
                        t3.Name = "2048 3";
                        t3.Start();
                    }

                    if (d == 3)
                    {
                        t4.Priority = ThreadPriority.Highest;
                        t4.Name = "2048 4";
                        t4.Start();
                    }


                }
                
            }

            for (var i = 0; i < 4;i++ )
            {
                if(flag[i]==1)
                {
                    if (i == 0) t1.Join();
                    if (i == 1) t2.Join();
                    if (i == 2) t3.Join();
                    if (i == 3) t4.Join();

                   
                }

            }

            for (var i = 0; i < 4; i++)
            {
                if (flag[i] == 1)
                {
              
                    if (ThreadResult[i] > maxvalue)
                    {
                        bestchoice = (float)i;
                        maxvalue = ThreadResult[i];
                    }


                }

            }




            return bestchoice;
        }

        private float MyAI(int[,] grids, int depth, int tid)
        {
            float bestchoice = 0;
            float maxvalue = 0;
            float currentvalue = 0;
            var g1 = grids;

            int[,] tempgrid = new int[4, 4];

           

            if (depth == targetdepth) return EvaluateCurrent(grids);

            for (var d = 0; d < 4; d++)
            {
                int counter = 0;
                int cm;

                cm = Move(grids, d ,tid);

                for (var x = 0; x < 4; x++)
                    for (var y = 0; y < 4; y++)
                    {
                        tempgrid[x, y] = ThreadMoveResult[tid, x, y];
                    }

                cm = checksame(grids, tempgrid);

                currentvalue = 0;

                if (cm == 0)
                {
                    for (var x = 0; x < 4; x++)
                        for (var y = 0; y < 4; y++)
                        {
                            if (tempgrid[x, y] == 0)
                            {
                                // generate 2  90%
                                tempgrid[x, y] = 2;
                                currentvalue += MyAI(tempgrid, depth + 1,tid) * 0.9f;
                                // generate 4   10%
                                tempgrid[x, y] = 4;
                                currentvalue += MyAI(tempgrid, depth + 1,tid) * 0.1f;
                                tempgrid[x, y] = 0;
                                counter++;
                            }

                        }
                    if (counter == 0)
                    {
                        //currentvalue += MyAI(tempgrid, depth + 1);
                        currentvalue = 0;
                    }
                    else
                    {
                        StepNum[tid] += counter;
                        currentvalue /= counter;
                    }
                }

                if (cm == 0 && currentvalue > maxvalue)
                {
                    bestchoice = (float)d;
                    maxvalue = currentvalue;
                }
            }

            if (depth == 0)
                return bestchoice;
            else
                return maxvalue;
        }


        private float MyAI(int[,] grids, int depth)
        {
            float bestchoice = 0;
            float maxvalue = 0;
            float currentvalue = 0;
            var g1 = grids;

            int[,] tempgrid = new int[4, 4]; 

            if(depth ==0)
            {
                StepNum[0] = 0;
                StepNum[1] = 0;
                StepNum[2] = 0;
                StepNum[3] = 0;
            }

            if (depth == targetdepth) return EvaluateCurrent(grids);

            for(var d=0;d<4;d++)
            {
                if (depth == 0) globald = d;
                int counter = 0;
                int cm;

                cm=Move(grids,d);

                cm = checksame(grids, MoveResult);

                copyto(MoveResult, tempgrid);

                currentvalue = 0;
              
                if (cm == 0)
                {
                    for (var x = 0; x < 4; x++)
                        for (var y = 0; y < 4; y++)
                        {
                            if (tempgrid[x, y] == 0)
                            {
                                // generate 2  90%
                                tempgrid[x, y] = 2;
                                currentvalue += MyAI(tempgrid, depth + 1) * 0.9f;
                                // generate 4   10%
                                tempgrid[x, y] = 4;
                                currentvalue += MyAI(tempgrid, depth + 1) * 0.1f;
                                tempgrid[x, y] = 0;
                                counter++;
                            }

                        }
                    if (counter == 0)
                    {
                        //currentvalue += MyAI(tempgrid, depth + 1);
                        currentvalue = 0;
                    }
                    else
                    {
                        StepNum[globald] += counter;
                        currentvalue /= counter;
                    }
                }

                if (cm==0 && currentvalue > maxvalue)
                {
                     bestchoice = (float)d;
                     maxvalue = currentvalue;
                }
            }

            if (depth == 0)
                return bestchoice;
            else
                return maxvalue;
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
        
            int blank=0;
            int max = 0;
            int maxstep = 0;
            int minstep = 0;
            int is2048 = 0;

            for(var x=0;x<4;x++)
                for(var y=0;y<4;y++)
                {
                    if(grids[x,y]==0) blank++;
                    if (grids[x, y] > max) max = grids[x, y];
                    if (grids[x, y] == 2048) is2048 = 1;
                }

            /*
            if(blank>8) targetdepth = 2;
            else 
            {
                if(blank>6) targetdepth = 3;
                else
                {
                    if(blank>3) targetdepth = 4;
                    else
                    {
                        if(blank>1) targetdepth = 5;
                        else targetdepth = 6;
                    }
                }
            }
            */
            //if (max >= 4096) max = 2048;

            if (PredictStep != 0 && max >= 2048)
            {
                if (max == 2048)
                {
                    if (PredictStep > 1000000) maxstep = targetdepth;
                    else maxstep = 10;
                    if (PredictStep < 10000) minstep = targetdepth + 1;
                    if (PredictStep < 250) minstep = targetdepth + 2;
                }
                else
                {
                    if (PredictStep > 1000000) maxstep = targetdepth;
                    else maxstep = 10;
                    if (PredictStep < 200000) minstep = targetdepth + 1;
                    if (PredictStep < 5000) minstep = targetdepth + 1;
                    if (PredictStep < 250) minstep = targetdepth + 2;
                }
            }

            

            if (max >= 8192)
            {
                snakevalue = 25;
                if (blank > 8) targetdepth = 3;
                else
                {
                    if (blank > 6) targetdepth = 4;
                    else
                    {
                        if (blank > 3) targetdepth = 4;
                        else
                        {
                            if (blank > 1) targetdepth = 5;
                            else targetdepth = 6;
                        }
                    }
                }
            }
            else if(max>=2048)
            {
                snakevalue = 40;
                if (blank > 10) targetdepth = 2;
                else
                {
                    if (blank > 6) targetdepth = 3;
                    else
                    {
                        if (blank > 3) targetdepth = 4;
                        else
                        {
                            if (blank > 1) targetdepth = 4;
                            else targetdepth = 5;
                        }
                    }
                }

            }
            else
            {
                if (blank > 8) targetdepth = 2;
                else
                {
                    if (blank > 6) targetdepth = 3;
                    else
                    {
                        if (blank > 3) targetdepth = 3;
                        else
                        {
                            if (blank > 1) targetdepth = 4;
                            else targetdepth = 4;
                        }
                    }
                }

            }

            if (PredictStep != 0 && max >= 2048)
            {
                if (targetdepth > maxstep) targetdepth = maxstep;
                if (targetdepth < minstep) targetdepth = minstep;
            }

            //Console.Write(targetdepth + "\n");


            float result = 0 ;
            int sumstep = 0 ;

            

            if(max>=4096)
            {
                if (is2048==1)
                {
                    targetdepth = 3;
                    snakevalue = 30;
                    if (targetdepth > 4) targetdepth = 4;
                    
                    //while (sumstep < 5 && targetdepth <= 20)
                    while (sumstep < 1000000 && targetdepth <= 20)
                    {
                        if (targetdepth >= 2)
                            result = MyAI_Multithread(grids);
                        else
                            result = MyAI(grids, 0);

                        sumstep = StepNum[0] + StepNum[1] + StepNum[2] + StepNum[3];
                        targetdepth++;
                    }
                }
                else
                {
                    snakevalue = 40;
                    if (targetdepth > 3) targetdepth = 3;

                    //while (sumstep < 5 && targetdepth <= 12)
                    while (sumstep < 50000 && targetdepth <= 12)
                    {
                        if (targetdepth >= 2)
                            result = MyAI_Multithread(grids);
                        else
                            result = MyAI(grids, 0);

                        sumstep = StepNum[0] + StepNum[1] + StepNum[2] + StepNum[3];
                        targetdepth++;
                    }
                }

            }
            else if(max>=2048)
            {
                snakevalue = 50;
                //targetdepth = 3;
                if (targetdepth > 4) targetdepth = 4;
                //while (sumstep < 20000 && targetdepth <= 12)
                while (sumstep < 2 && targetdepth <= 12)
                {
                    if (targetdepth >= 2)
                        result = MyAI_Multithread(grids);
                    else
                        result = MyAI(grids, 0);

                    sumstep = StepNum[0] + StepNum[1] + StepNum[2] + StepNum[3];
                    targetdepth++;
                }

            }
            else
            {
                snakevalue = 30;
                if (targetdepth > 3) targetdepth = 3;
                if (targetdepth >= 2)
                    result = MyAI_Multithread(grids);
                else
                    result = MyAI(grids, 0);
            }

            targetdepth = targetdepth;

            if (result > 0.5)
            {
                if (result > 1.5)
                {
                    if (result > 2.5)
                    {
                        PredictStep = StepNum[3];
                        return 3;
                    }
                    else
                    {
                        
                        PredictStep = StepNum[2];
                        return 2;
                    }
                }
                else
                {
                    
                    PredictStep = StepNum[1];
                    return 1;
                }
            }
            else
            {
                
                PredictStep = StepNum[0];
                return 0;

            }

            //return (int)MyAI(grids, 0);
            //return new Random(DateTime.Now.Millisecond).Next() % 4;
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