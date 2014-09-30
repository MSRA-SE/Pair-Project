Our programs are mainly in folder AIfor2048_SRC.
Estimation.cs & Searcher.cs
We generate dynamic link library using these two source file.

In execute project, you should add our dll file as reference.
Also you should modify the function AINextMove in api.ashx.cs

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