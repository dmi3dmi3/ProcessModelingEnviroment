using System;
using System.Collections.Generic;
using System.Linq;

namespace CellarAutomatonLib
{
    public class Neighbors
    {
        public Neighbors()
        {
            NeighborsCell = new Cell[8];
        }
        public Cell[] NeighborsCell { get; set; }
        public int StateCount(int state) => NeighborsCell.Count(_ => _ != null && _.State == state);

        public Cell NW => NeighborsCell[0];
        public Cell W => NeighborsCell[1];
        public Cell SW => NeighborsCell[2];
        public Cell N => NeighborsCell[3];
        public Cell S => NeighborsCell[4];
        public Cell NE => NeighborsCell[5];
        public Cell E => NeighborsCell[6];
        public Cell SE => NeighborsCell[7];

        private static readonly List<(int, int)> _shiftList = new List<(int, int)>
        {
            (-1, -1),//nw
            (-1, 0), //w
            (-1, 1), //sw
            (0, -1), //n
            (0, 1),  //s
            (1, -1), //ne
            (1, 0),  //e
            (1, 1)   //se
        };

        public  static Neighbors GetNeighbors(int x, int y, Cell[,] board, Random random, Config config)
        {
            var nb = new Neighbors();
            var c = -1;
            if (config.LoopEdges)
            {
                foreach (var (i, j) in _shiftList)
                {
                    c++;
                    if (random.Next(100) < config.IsolationPercent)
                        continue;
                    var k = (x + i + config.Height) % config.Height;
                    var h = (y + j + config.Width) % config.Width;
                    nb.NeighborsCell[c] = board[k, h].GetCopy();
                }
            }
            else
            {
                foreach (var (i, j) in _shiftList)
                {
                    c++;
                    if (random.Next(100) < config.IsolationPercent)
                        continue;
                    var k = x + i;
                    if (-1 == k || k == config.Height)
                        continue;
                    var h = y + j;
                    if (-1 == h || h == config.Width)
                        continue;

                    nb.NeighborsCell[c] = board[k, h].GetCopy();
                }
            }

            return nb;
        }
    }
}