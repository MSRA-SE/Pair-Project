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
    /// 

    public class move
    {
        public int[,] grid = new int[4, 4];
        public int[,] grid_copy = new int[4, 4];   

        public move(int[,] x)
        {
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    grid[i, j] = x[i, j];
                    grid_copy[i, j] = x[i, j];
                }
            }          
        }

        //将运算中主要用的数组grid做一个备份，以便在走完一步后回到未走之前的状态。
        public void copy()
        {
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    grid[i, j] = grid_copy[i, j];
                }
            }
        }

        //模拟人走一步时用到的函数，以单独一行为例:需要先压缩其中的空格，然后再进行合并（在能合并的前提下）
        //其中compress进行压缩，而combine进行合并。
        public void compress(int[] tmp)  
        {
            int[] tmp1 = { 0, 0, 0, 0 };
            int k = 0, i;
            for (i = 0; i < 4; i++)
            {
                if (tmp[i] != 0)
                {
                    tmp1[k] = tmp[i];
                    k++;
                }
            }
            for (i = 0; i < 4; i++)
            {
                tmp[i] = tmp1[i];
            }
        }

        public int combine(int[] temp)
        {
            int score1 = 0;
            compress(temp);
            for (int i = 0; i < 3; i++)
            {
                if (temp[i] != 0 && temp[i] == temp[i + 1])
                {
                    temp[i] = 2 * temp[i];
                    score1 += temp[i];
                    temp[i + 1] = 0;
                }
            }
            compress(temp);
            return (score1);
        }

        // 用户朝各个方向走所用的函数。
        public int up()
        {
            int score1 = 0;
            int[] tmp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tmp[j] = grid[j, i];
                }
                score1 += combine(tmp);
                for (int j = 0; j < 4; j++)
                {
                    grid[j, i] = tmp[j];
                }
            }
            return (score1);
        }

        public int down()
        {
            int score1 = 0;
            int[] tmp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tmp[j] = grid[3 - j, i];
                }
                score1 += combine(tmp);
                for (int j = 0; j < 4; j++)
                {
                    grid[3 - j, i] = tmp[j];
                }
            }
            return (score1);
        }

        public int left()
        {
            int score1 = 0;
            int[] tmp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tmp[j] = grid[i, j];
                }
                score1 += combine(tmp);
                for (int j = 0; j < 4; j++)
                {
                    grid[i, j] = tmp[j];
                }
            }
            return (score1);
        }

        public int right()
        {
            int score1 = 0;
            int[] tmp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tmp[j] = grid[i, 3 - j];
                }
                score1 += combine(tmp);
                for (int j = 0; j < 4; j++)
                {
                    grid[i, 3 - j] = tmp[j];
                }
            }
            return (score1);
        }
       
        //统计grid中的空格数
        public int empty_num()
        {
            int num = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    num += grid[i, j] == 0 ? 1 : 0;
                }
            }
            return num;
        }

        //计算单调性，首先对grid的个元素取log，原因如下：在游戏中2048与1024之间的差距 和 512与256之间的差距
        //是一样的，但这两组数之间的差值却相差极大，取完log之后他们才是在同一标准下，平滑性也是同样的道理。
        public double monotonicity()
        {
            int[,] log_of_grid = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
            int[] tmp1 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] tmp2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int k = 0;
            double mono1 = 0, mono2 = 0;
            double w1 = 0.2, w2 = 0.8;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    log_of_grid[i, j] = grid[i, j] == 0 ? 0 : (int)(Math.Log(grid[i, j]) / Math.Log(2));
                }
            }
            //tmp_zigzag为将grid从左下角开始按zigzag的方式排列一遍，数据不多，手工填写。 
            int[] tmp_zigzag = {   log_of_grid[3,0], log_of_grid[2,0], log_of_grid[3,1], log_of_grid[3,2],
                                   log_of_grid[2,1], log_of_grid[1,0], log_of_grid[0,0], log_of_grid[1,1],
                                   log_of_grid[2,2], log_of_grid[3,3], log_of_grid[2,3], log_of_grid[1,2],
                                   log_of_grid[0,1], log_of_grid[0,2], log_of_grid[1,3], log_of_grid[0,3]
                               };
            //tmp_up为将grid从左下角开始依次按列衔接排列。
            int[] tmp_up = {   log_of_grid[3,0], log_of_grid[2,0], log_of_grid[1,0], log_of_grid[0,0],
                               log_of_grid[0,1], log_of_grid[1,1], log_of_grid[2,1], log_of_grid[3,1],
                               log_of_grid[3,2], log_of_grid[2,2], log_of_grid[1,2], log_of_grid[0,2],
                               log_of_grid[0,3], log_of_grid[1,3], log_of_grid[2,3], log_of_grid[3,3]
                           };
            //压缩tmp_zigzag中的空格
            for (int i = 0; i < 15; i++)
            {
                if (tmp_zigzag[i] != 0) 
                {
                    tmp1[k] = tmp_zigzag[i];
                    k++;
                }
            }
            for (int i = 0; i < 15; i++)
            {
                tmp_zigzag[i] = tmp1[i];
            }
            //压缩tmp_up中的空格
            k = 0;
            for (int i = 0; i < 15; i++)
            {
                if (tmp_up[i] != 0) ;
                {
                    tmp2[k] = tmp_up[i];
                    k++;
                }
            }
            for (int i = 0; i < 15; i++)
            {
                tmp_up[i] = tmp2[i];
            }
            //邻位依次相减并累加，作为单调性的值。
            for (int i = 0; i < 15; i++)
            {
                mono1 += tmp_zigzag[i] - tmp_zigzag[i + 1];
                mono2 += tmp_up[i] - tmp_up[i + 1];
            }
            //如果tmp_zigzag的最大值不在首位，做一个惩罚,相对于保持最大值在左下角不变
            if (tmp_zigzag.Max() != tmp_zigzag[0])
                mono1 -= 16 - empty_num();
            //如果tmp_up的最大值不在首位，做一个惩罚相对于保持最大值在左下角不变
            if (tmp_up.Max() != tmp_up[0])
                mono2 -= 16 - empty_num();
            //tmp_zigzag和tmp_up按不同的权重对评估造成影响。
            return (mono1 * w1 + mono2 * w2);
        }

        //平滑性计算，两两相邻的数据之间只计算一次差值并取绝对值，然后累加，这样值越大说明平滑性越差，为了和单调
        //性中保持相同的越大越好，所以取个负号。
        public double smooth()
        {
            int[,] log_of_grid = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
            int i, j;
            double tmp = 0;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    log_of_grid[i, j] = grid[i, j] == 0 ? 0 : (int)(Math.Log(grid[i, j]) / Math.Log(2));
                }
            }
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    tmp += -(Math.Abs(log_of_grid[i, j] - log_of_grid[i + 1, j]) + Math.Abs(log_of_grid[i, j] - log_of_grid[i, j + 1]));
                }
            }
            for (i = 0; i < 3; i++)
            {
                tmp += -(Math.Abs(log_of_grid[3, i] - log_of_grid[3, i + 1]) + Math.Abs(log_of_grid[i, 3] - log_of_grid[i + 1, 3]));
            }
            return tmp;
        }

        //得到最大值的变化，主要是用在有合并后最大值发生变化时，如果合并会使得最大值变化，这一般是有利的。合并时
        //只变化grid，而grid_copy未变，所以可以通过这两个这两个来验证变化与否。
        public double  get_max_change()
        {
            int max1 = 0, max2 = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    if (grid[i, j] > max1)
                        max1 = grid[i,j];
                    if (grid_copy[i, j] > max2)
                        max2 = grid_copy[i, j];
                }
            }
            return (max1 == max2 ? 0 : Math.Log(max1-max2)/Math.Log(2));
            
        }
       
        //评估函数
        public double evaluate()
        {
            const double smooth_weight = 1.0;
            const double monotonicity_weight = 1.5;
            const double empty_weight = 2.7;
            const double maxweigth = 3;
            return smooth_weight * smooth()
                + monotonicity() * monotonicity_weight
                + empty_num() * empty_weight
                + get_max_change() * maxweigth;            
        }

        //用户移动函数
        public void user_move(int dirc)
        {
            switch (dirc)
            {
                case 0:
                    up();
                    break;
                case 1:
                    right();
                    break;
                case 2:
                    down();
                    break;
                case 3:
                    left();
                    break;
                default:
                    break;
            }
        }

        //判断x与y是不是完全相同，主要用在判断用户能否向某个方向移动一次，如果完成一次移动后
        //grid和grid_copy完全一样说明该方向的移动无效
        public bool equal(int[,] x, int[,] y)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (x[i, j] != y[i, j])
                        return false;
                }
            }
            return true;
        }

        //这一函数用在计算机走最后一次迭代时对场面的评估
        public double Com_last_move_value()
        {
            double ret = 0;
            double tmp1, tmp2;
            if (empty_num() != 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (grid[i, j] == 0)
                        {                            
                            grid[i, j] = 2;
                            tmp1 = evaluate();
                            grid[i, j] = 4;
                            tmp2 = evaluate();
                            grid[i, j] = 0;
                            ret += 0.1 * tmp2 + 0.9 * tmp1;
                        }                             
                    }
                }
                return ret / empty_num();
            }
            else
            {
                return evaluate();
            }
        }
    }


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
        /// 
              
        public double Cal_value(int dept, move x, bool turn)
        {
            //该函数迭代计算评估值
            turn = !turn;      //每迭代一次换一下对象（人还是机器）
            double aver = 0.0 ;
            if (turn)           //turn为真则表示这一步该人走，否则机器走
            {
                if (dept == 0)
                {
                    double[] goal = { 0, 0, 0, 0 };
                    for (int k = 0; k < 4; k++)
                    {
                        x.user_move(k);
                        goal[k] = x.evaluate();
                        x.copy();      //朝某个方向移动一次以后要还原grid
                    }                   
                    return (goal.Max());
                }
                else
                {                    
                    double[] goal = { 0, 0, 0, 0 };
                    for (int k = 0; k < 4; k++)
                    {
                        x.user_move(k);
                        if(x.equal(x.grid,x.grid_copy))   //当前走法不可走
                        {
                            goal[k] = -1000;
                        }
                        else 
                        {
                            move t = new move(x.grid);                        
                            goal[k] += Cal_value(dept - 1, t, turn);                            
                        }
                        x.copy(); 
                    }                   
                    return (goal.Max());
                }
            }

            else
            {
                if (dept == 0)
                {
                    return x.Com_last_move_value();
                }
                else
                {
                    if (x.empty_num() == 0)   //无法再填入新的值
                    {
                        aver = x.evaluate();
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (x.grid[i, j] == 0)
                                {
                                    x.grid[i, j] = 2;
                                    move tmp1 = new move(x.grid);
                                    x.grid[i, j] = 0;
                                    aver += 0.9 * Cal_value(dept - 1, tmp1, turn);
                                    x.grid[i, j] = 4;
                                    move tmp2 = new move(x.grid);
                                    x.grid[i, j] = 0;
                                    aver += 0.1 * Cal_value(dept - 1, tmp2, turn);
                                }
                            }
                        }
                        aver /= x.empty_num();
                    }
                    return aver;
                }
            }
        }

        public int get_direction(move x,int dep)
        {
            int i, k;
            int dir = 0;
            double[] goal = { 0, 0, 0, 0 };
            for (k = 0; k < 4; k++)          //分别计算朝四个方向移动的评估值
            {
                x.user_move(k);                
                if (x.equal(x.grid, x.grid_copy))
                    goal[k] = -1000;
                else
                {
                    move y = new move(x.grid);
                    goal[k] = Cal_value(dep, y, true);
                }                                   
                x.copy();
            }
            
            if (goal[0] == goal[1] && goal[1] == goal[2] && goal[2] == goal[3])
                dir = new Random(DateTime.Now.Millisecond).Next() % 4;  //朝四个方向的评估都相同，则随机走
            else
            {
                for (i = 1; i < 4; i++)
                {
                    if (goal[i] > goal[0])
                    {
                        goal[0] = goal[i];  //选取评估值最大的方向。
                        dir = i;
                    }
                }
            }
            return (dir);
        }

        private int AINextMove(int[,] grids)
        {           
          
            move once = new move(grids);
            int dir = get_direction(once, 4);           
            return dir;                      
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