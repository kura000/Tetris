using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;

class Tetris
{
    // 定数定義
    static class Constants
    {
        public const int fieldWidth = 10;
        public const int fieldHeight = 20;
        public const int shapeWidthMax = 4;
        public const int shapeHeightMax = 4;
        public const int perDecreseInterval = 100;
        public const int perDecreseLine = 5;
        public const int maxInterval = 2000;
        public const int minInterval = 100;
    }

    // テトリミノの列挙
    enum blockTypes
    {
        shapeI,
        shapeO,
        shapeS,
        shapeZ,
        shapeJ,
        shapeL,
        shapeT,
        shapeMax
    }

    // Class：テトリミノの形状
    class Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[,] Pattern { get; set; }

        public Shape(int width, int height, int[,] pattern)
        {
            Width = width;
            Height = height;
            Pattern = pattern;
        }
    }

    // Class：テトリミノの本体
    class Mino
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Shape Shape { get; set; }

        public Mino(int x, int y, Shape shape)
        {
            X = x;
            Y = y;
            Shape = shape;
        }
    }

    // Main
    static void Main()
    {
        // フィールドとスクリーンを定義する
        var screen = new int[Constants.fieldHeight, Constants.fieldWidth];
        var field = new int[Constants.fieldHeight, Constants.fieldWidth];

        // 新しいテトリミノを生成
        var mino = GenerateMino();

        // フラグ、変数定義
        var gamePlaying = true;
        var countDeleteLine = 0;
        var lastDeleteLine = 0;

        // フィールドを描画（引数に副作用あり）
        DrawScreen(screen, field, mino, countDeleteLine);

        // タイマーによる自動落下
        var timer = new System.Timers.Timer();
        var interval = Constants.maxInterval;
        timer.Interval = interval;
        timer.Elapsed += (sender, e) =>
        {
            Mino lastMino = new Mino(mino.X, mino.Y, CopyMinoShape(mino));
            mino.Y++;
            if (MinoIntersectField(field, mino))
            {
                mino = lastMino;

                // 落下した時に一番下ならフィールドにテトリミノを書き写す
                for (int y = 0; y < mino.Shape.Height; y++)
                    for (int x = 0; x < mino.Shape.Width; x++)
                        if (mino.Shape.Pattern[y, x] == 1)
                            field[mino.Y + y, mino.X + x] = 1;

                // GameOver判定
                gamePlaying = checkStillAlive(field);

                // テトリミノが揃っている列がある場合、その列を削除
                var completeFlg = true;
                for (int y = 0; y < Constants.fieldHeight; y++)
                {
                    completeFlg = true;
                    // 列が揃っているかのチェック
                    for (int x = 0; x < Constants.fieldWidth; x++)
                    {
                        if (field[y, x] == 0)
                        {
                            completeFlg = false;
                            break;
                        }
                    }
                    // 揃っている場合列を削除
                    if (completeFlg)
                    {
                        for (int x = 0; x < Constants.fieldWidth; x++)
                            field[y, x] = 0;

                        for (int y2 = y; y2 >= 1; y2--)
                            for (int x = 0; x < Constants.fieldWidth; x++)
                            {
                                field[y2, x] = field[y2 - 1, x];
                                field[y2 - 1, x] = 0;
                            }
                        countDeleteLine++;
                    }
                }

                // 消去したライン数に応じて落下速度を加速させる
                if (countDeleteLine - lastDeleteLine >= Constants.perDecreseLine && interval > Constants.minInterval)
                {
                    interval -= Constants.perDecreseInterval;
                    timer.Interval = interval;
                    lastDeleteLine = countDeleteLine;
                }

                mino = GenerateMino();

            }
            DrawScreen(screen, field, mino, countDeleteLine);
        };
        timer.AutoReset = true;
        timer.Enabled = true;

        // 無限ループ
        Console.WriteLine("Game　Start...");
        while (gamePlaying)
        {
            var input = Console.ReadKey(true);
            // 終了コマンド
            if (input.Key.ToString() == "Q")
            {
                Console.WriteLine("Quit Game...");
                timer.Close();
                break;
            }
            // 操作コマンド
            var copyShape = CopyMinoShape(mino);
            Mino lastMino = new Mino(mino.X, mino.Y, copyShape);
            switch (input.Key.ToString())
            {
                case "W":
                    break;

                case "S":
                    mino.Y++;
                    break;

                case "A":
                    mino.X--;
                    break;

                case "D":
                    mino.X++;
                    break;

                default:
                    // 上記以外のキーが入力された場合テトリミノを反時計回りに回転させる（引数に副作用あり）
                    RotateMino(mino);
                    break;
            }

            if (MinoIntersectField(field, mino))
            {
                mino = lastMino;
            }

            // 画面の再描画
            DrawScreen(screen, field, mino, countDeleteLine);
        }
        // GameOver判定
        if (!gamePlaying)
        {
            Console.WriteLine("Game Over.....");
            timer.Close();
        }
    }

    // テトリミノの形状パターンが格納された配列を生成する
    private static Shape[] CreatePatternList()
    {
        // 1.patternI
        var patternI = new int[,]
        {
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0},
            {0,0,0,0}
        };
        Shape shapeI = new Shape(4, 4, patternI);

        // 2.patternO
        var patternO = new int[,]
        {
            {1,1},
            {1,1}
        };
        Shape shapeO = new Shape(2, 2, patternO);

        // 3.patternS
        var patternS = new int[,]
        {
            {0,1,1},
            {1,1,0},
            {0,0,0}

        };
        Shape shapeS = new Shape(3, 3, patternS);

        // 4.patternZ
        var patternZ = new int[,]
        {
            {1,1,0},
            {0,1,1},
            {0,0,0}
        };
        Shape shapeZ = new Shape(3, 3, patternZ);

        // 5.patternJ
        var patternJ = new int[,]
        {
            {1,0,0},
            {1,1,1},
            {0,0,0}
        };
        Shape shapeJ = new Shape(3, 3, patternJ);

        // 6.patternL
        var patternL = new int[,]
        {
            {0,0,1},
            {1,1,1},
            {0,0,0}
        };
        Shape shapeL = new Shape(3, 3, patternL);

        // 6.patternT
        var patternT = new int[,]
        {
            {0,1,0},
            {1,1,1},
            {0,0,0}
        };
        Shape shapeT = new Shape(3, 3, patternT);

        var shapes = new Shape[] { shapeI, shapeO, shapeS, shapeZ, shapeJ, shapeL, shapeT };
        return shapes;
    }

    // 新しいテトリミノを生成する
    private static Mino GenerateMino()
    {
        var shapes = CreatePatternList();
        var rand = new Random();
        var rNumber = rand.Next(0, (int)blockTypes.shapeMax);
        var mino = new Mino(0, 0, shapes[rNumber]);
        mino.X = (Constants.fieldWidth - mino.Shape.Width) / 2;        
        return mino;
    }

    // フィールドの描画
    static void DrawScreen(int[,] screen, int[,] field, Mino mino, int countDeleteLine)
    {
        // スクリーンにフィールドをコピーする
        screen = (int[,])field.Clone();

        // スクリーン上のテトリミノと重なっている部分を描画する
        for (int y = 0; y < mino.Shape.Height; y++)
        {
            for (int x = 0; x < mino.Shape.Width; x++)
            {

                if (mino.Shape.Pattern[y, x] == 1)
                {
                    screen[mino.Y + y, mino.X + x] |= 1;
                }
            }
        }

        // コンソールのリセット（既存の描画を削除）
        Console.Clear();

        // スクリーンを描画（壁とテトリミノ）
        Console.WriteLine("");
        Console.WriteLine("");
        for (int y = 0; y < Constants.fieldHeight; y++)
        {
            Console.Write("□");
            for (int x = 0; x < Constants.fieldWidth; x++)
            {
                Console.Write(screen[y, x] == 1 ? "■" : " ");
            }
            Console.WriteLine("□");
        }

        // スクリーン外の底を描画
        for (int x = 0; x < Constants.fieldWidth + 2; x++)
        {
            Console.Write("=");
        }
        Console.WriteLine();
        Console.WriteLine($"Line:{countDeleteLine}");


    }

    // テトリミノの回転
    static void RotateMino(Mino mino)
    {
        // 引数で受け取ったテトリミノのX座標、Y座標は保持する。
        // 形状を管理しているShapeのPatternを更新する。
        var copyShape = CopyMinoShape(mino);
        Mino newMino = new Mino(mino.X, mino.Y, copyShape);

        for (int y = 0; y < mino.Shape.Height; y++)
            for (int x = 0; x < mino.Shape.Width; x++)
            {
                if (mino.Shape.Pattern[y, x] == 1)
                {
                    newMino.Shape.Pattern[mino.Shape.Width - 1 - x, y] = 1;
                }
                if (mino.Shape.Pattern[y, x] == 0)
                {
                    newMino.Shape.Pattern[mino.Shape.Width - 1 - x, y] = 0;
                }
            }
        mino.Shape.Pattern = newMino.Shape.Pattern;

    }

    // テトリミノ形状パターンコピー
    static Shape CopyMinoShape(Mino origin)
    {
        var copyPattern = new int[origin.Shape.Height, origin.Shape.Width];
        Array.Copy(origin.Shape.Pattern, copyPattern, origin.Shape.Pattern.Length);
        var copyShape = new Shape(origin.Shape.Width, origin.Shape.Height, copyPattern);
        return copyShape;
    }

    // テトリミノ当たり判定
    static bool MinoIntersectField(int[,] field, Mino mino)
    {
        for (int y = 0; y < mino.Shape.Height; y++)
            for (int x = 0; x < mino.Shape.Width; x++)
                if (mino.Shape.Pattern[y,x] == 1)
                {
                    if (
                        mino.Y+y < 0
                        || mino.Y+y >= Constants.fieldHeight
                        || mino.X+x < 0
                        || mino.X+x >= Constants.fieldWidth
                        )
                    {
                        return true;
                    }
                    if (field[mino.Y+y, mino.X+x] == 1)
                    {
                        return true;
                    }
                }
        return false;
    }

    // GameOver判定
    static bool checkStillAlive(int[,] field)
    {
        for (int x = 0; x < Constants.fieldWidth; x++)
            if (field[0, x] == 1)
            {
                return false;
            }
        return true;
    }

    // debug：テトリミノパターン出力
    static void PrintPattern(Mino mino, string title)
    {
        for (int y = 0; y < mino.Shape.Height; y++)
            for (int x = 0; x < mino.Shape.Width; x++)
                Console.WriteLine($"{title} {x}, {y} : {mino.Shape.Pattern[y, x]}");
    }

}
