using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace MapRenderer
{
    internal class TunnelingGenerator
    {
        int _WIDTH;
        int _HEIGHT;

        int _TUNN_MAX_SIZE;
        int _TUNN_MIN_SIZE;
        double _TUNN_CHANGE_CHANCE;
        Random random;

        public List<int[,]> _MAP_STAGES;

        public TunnelingGenerator(int seed = 0)
        {
            _MAP_STAGES = new List<int[,]>();

            //seeded if non-zero initialization
            if (seed != 0)
            {
                random = new Random(seed);
            }
            else
            {
                random = new Random();
            }
        }

        public int[,] CreateMap(int WIDTH, int HEIGHT, int ROOM_MAX_SIZE, int ROOM_MIN_SIZE, int MAX_ROOMS, int TUNN_MAX_SIZE, int TUNN_MIN_SIZE, double TUNN_CHANGE_CHANCE)
        {
            int width = WIDTH;
            int height = HEIGHT;
            _WIDTH = WIDTH;
            _HEIGHT = HEIGHT;

            int room_max_size = ROOM_MAX_SIZE;
            int room_min_size = ROOM_MIN_SIZE;
            int max_rooms = MAX_ROOMS;

            int tunn_max_size = TUNN_MAX_SIZE;
            int tunn_min_size = TUNN_MIN_SIZE;
            _TUNN_MAX_SIZE = TUNN_MAX_SIZE;
            _TUNN_MIN_SIZE = TUNN_MIN_SIZE;
            _TUNN_CHANGE_CHANCE = TUNN_CHANGE_CHANCE;

            int[,] level = new int[width, height];

            List<System.Drawing.Rectangle> rooms = new List<System.Drawing.Rectangle>();
            int num_rooms = 0;

            //random = new Random(0);

            //set level to all walls:
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    level[x, y] = 1;
                }
            }
            OutMap(level);

            for (int r = 0; r < max_rooms; r++)
            {
                //random width and height
                int w = random.Next(room_min_size, room_max_size);
                int h = random.Next(room_min_size, room_max_size);
                //random position:
                int x = random.Next(0, width  - w - 1);
                int y = random.Next(0, height - h - 1);

                System.Drawing.Rectangle new_room = new System.Drawing.Rectangle(x, y, w, h);

                //check overlap with previous rooms:
                bool overlap = false;
                foreach(System.Drawing.Rectangle room in rooms)
                {
                    if (new_room.IntersectsWith(room))
                    {
                        overlap = true;
                        break;
                    }
                }

                if(!overlap)
                {
                    CreateRoom(new_room, level);

                    (int newX, int newY) = RectCenter(new_room);

                    if (num_rooms != 0)
                    {
                        (int prevX, int prevY) = RectCenter(rooms[num_rooms - 1]);

                        //50% chance tunnel begins Horizontally:
                        if(random.NextDouble() > 0.5)
                        {
                            CreateHorTunnel(prevX, newX, prevY, level);
                            CreateVerTunnel(prevY, newY, newX, level);
                        }
                        else
                        {
                            CreateVerTunnel(prevY, newY, prevX, level);
                            CreateHorTunnel(prevX, newX, newY, level);
                        }
                    }

                    //append room to list:
                    rooms.Add(new_room);
                    num_rooms++;
                }
            }

            OutMap(level);
            return level;
        }

        private void OutMap(int[,] level)
        {
            int[,] stage = new int[_WIDTH, _HEIGHT];
            for(int x = 0; x < _WIDTH; x++)
            {
                for(int y = 0; y < _HEIGHT; y++)
                {
                    stage[x,y] = level[x,y];
                }
            }
            _MAP_STAGES.Add(stage);
        }

        (int, int) RectCenter(System.Drawing.Rectangle rectanlge)
        {
            int x = rectanlge.X;
            int y = rectanlge.Y;
            int w = rectanlge.Width;
            int h = rectanlge.Height;

            return (x + (w/2), y + (h/2));
        }

        void CreateRoom(System.Drawing.Rectangle room, int[,] level)
        {
            for(int x = room.X; x < room.X + room.Width; x++)
            {
                for(int y = room.Y; y < room.Y + room.Height; y++)
                {
                    if(coordsValid(x,y))
                    {
                        level[x, y] = 0;
                    }
                }
            }
            OutMap(level);
        }

        void CreateHorTunnel(int x1, int x2, int y, int[,] level)
        {
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);

            int width = random.Next(_TUNN_MIN_SIZE, _TUNN_MAX_SIZE);
            bool foundTunneler = false;

            for (int x = minX; x < maxX + 1; x++)
            {
                if(width >= 1)
                {
                    int startY = y - (width / 2);
                    int endY = y + (width / 2);
                    for(int ys = startY; ys <= endY + width; ys++)
                    {
                        if(coordsValid(x,ys))
                        {
                            if(level[x, ys] == 1)
                            {
                                level[x, ys] = -1;
                            }
                            else if (level[x, ys] == -1)
                            {
                                foundTunneler = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (coordsValid(x, y))
                    {
                        if (level[x,y] == 1)
                        {
                            level[x, y] = -1;
                        }
                    }
                }

                //change width of tunnel chance:
                double wChange = random.NextDouble();
                if (wChange < _TUNN_CHANGE_CHANCE)
                {
                    if (width > _TUNN_MIN_SIZE)
                    {
                        width--;
                    }
                }
                else if (wChange >= _TUNN_CHANGE_CHANCE && wChange < _TUNN_CHANGE_CHANCE * 2)
                {
                    if (width < _TUNN_MAX_SIZE)
                    {
                        width++;
                    }
                }

                //stop if met another tunneler:
                if(foundTunneler)
                {
                    //break;
                }
            }
            OutMap(level);
        }

        void CreateVerTunnel(int y1, int y2, int x, int[,] level)
        {
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            int width = random.Next(_TUNN_MIN_SIZE, _TUNN_MAX_SIZE);
            bool foundTunneler = false;

            for (int y = minY; y < maxY + 1; y++)
            {
                if (width >= 1)
                {
                    int startX = x - (width / 2);
                    int endX = x + (width / 2);
                    for (int xs = startX; xs <= endX + width; xs++)
                    {
                        if (coordsValid(xs, y))
                        {
                            if (level[xs,y] == 1)
                            {
                                level[xs, y] = -1;
                            }
                            else if (level[xs, y] == -1)
                            {
                                foundTunneler = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (coordsValid(x, y))
                    {
                        if (level[x, y] == 1)
                        {
                            level[x, y] = -1;
                        }
                        else if(level[x, y] == -1)
                        {
                            foundTunneler = true;
                        }
                    }
                }

                //change width of tunnel chance:
                double wChange = random.NextDouble();
                if (wChange < _TUNN_CHANGE_CHANCE)
                {
                    if (width > _TUNN_MIN_SIZE)
                    {
                        width--;
                    }
                }
                else if (wChange >= _TUNN_CHANGE_CHANCE && wChange < _TUNN_CHANGE_CHANCE * 2)
                {
                    if (width < _TUNN_MAX_SIZE)
                    {
                        width++;
                    }
                }

                //stop if met another tunneler:
                if (foundTunneler)
                {
                    //break;
                }
            }
            OutMap(level);
        }

        bool coordsValid(int x, int y)
        {
            if(x < 0 || x >= _WIDTH)
            {
                return false;
            }
            if(y < 0 || y >= _HEIGHT)
            {
                return false;
            }
            return true;
        }

    }
}
