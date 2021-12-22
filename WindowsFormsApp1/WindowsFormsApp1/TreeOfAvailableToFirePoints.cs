using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    partial class Density_Shoot
    {
        //класс дерева с четырьмя ветьвями от каждого узла, которые делят с каждым уровнем
        //глубины игровое поле на все большое число частей
        unsafe class TreeOfAvailableToFirePoints
        {
            Node ROOT;
            public TreeOfAvailableToFirePoints()
            {
                this.Recurs_Constructor((0, 9), (0, 9), ref this.ROOT);
            }
            void Recurs_Constructor((int, int)? XCoords, (int, int)? YCoords, ref Node node)
            {

                if (XCoords.HasValue && YCoords.HasValue)
                {

                    int x1 = XCoords.Value.Item1, x2 = XCoords.Value.Item2,
                        y1 = YCoords.Value.Item1, y2 = YCoords.Value.Item2;
                    int MASS = (x2 - x1 + 1) * (y2 - y1 + 1);
                    node = new Node(MASS, XCoords, YCoords);
                    if (x2 - x1 == 0 && y2 - y1 == 0)
                    {
                        node.LXLY = node.LXMY = node.MXLY = node.MXMY = null;
                    }
                    else if (x2 - x1 < 0 || y2 - y1 < 0)
                    {
                        node = null;
                    }
                    else
                    {
                        int limX, limY;
                        limX = (x2 - x1) / 2 + x1; limY = (y2 - y1) / 2 + y1;
                        this.Recurs_Constructor((x1, limX), (y1, limY), ref node.LXLY);
                        this.Recurs_Constructor((x1, limX), (limY + 1, y2), ref node.LXMY);
                        this.Recurs_Constructor((limX + 1, x2), (y1, limY), ref node.MXLY);
                        this.Recurs_Constructor((limX + 1, x2), (limY + 1, y2), ref node.MXMY);
                    }
                }
            }
            public void Decrement_Mass(Point_Vector vect)
            {
                foreach (Point p in vect)
                {
                    if (this.Contains(p.X, p.Y))
                    {
                        this.Decrement_Mass(p.X, p.Y, ref this.ROOT);
                    }
                }
            }
            public Point Choose_Point() //переделать под указатели
            {
                Node cur_node = this.ROOT;
                int max; Node max_node;
                Random rand = new Random((int)(DateTime.Now.Ticks % 1000000));
                while (true)
                {
                    cur_node.MASS--;
                    max = 0; max_node = null;
                    this.CompareMassAndChange(ref max, ref max_node, cur_node.LXLY, rand);
                    this.CompareMassAndChange(ref max, ref max_node, cur_node.LXMY, rand);
                    this.CompareMassAndChange(ref max, ref max_node, cur_node.MXLY, rand);
                    this.CompareMassAndChange(ref max, ref max_node, cur_node.MXMY, rand);
                    if (max_node.IsLeaf)
                    {
                        this.CheckAndChangeToNull(max_node, ref cur_node.LXLY);
                        this.CheckAndChangeToNull(max_node, ref cur_node.LXMY);
                        this.CheckAndChangeToNull(max_node, ref cur_node.MXLY);
                        this.CheckAndChangeToNull(max_node, ref cur_node.MXMY);
                        break;
                    }
                    cur_node = max_node;
                }
                Point result = new Point(max_node.XCoords.Value.Item1, max_node.YCoords.Value.Item1);
                return result;

            }
            void CheckAndChangeToNull(Node ValueForCheck, ref Node ValueToChange)
            {
                if (ValueForCheck == ValueToChange)
                {
                    ValueToChange = null;
                }
            }
            void CompareMassAndChange(ref int mass, ref Node max_node,
                Node cur_node, Random random)
            {
                if (cur_node != null && 
                    (cur_node.MASS > mass || (cur_node.MASS == mass && random.Next() % 2 == 0)))
                {
                    mass = cur_node.MASS; max_node = cur_node;
                }
            }
            public void Decrement_Mass(int x, int y)
            {
                if (this.ROOT == null)
                {
                    throw new NullReferenceException("А корня то нет");
                }
                this.Decrement_Mass(x, y, ref this.ROOT);
            }
            public bool Contains(int x, int y)
            {
                Node cur_node = this.ROOT;
                Node next = null;
                bool result = true;
                int limX, limY, x1, x2, y1, y2;
                while (true)
                {
                    x1 = cur_node.XCoords.Value.Item1; x2 = cur_node.XCoords.Value.Item2;
                    y1 = cur_node.YCoords.Value.Item1; y2 = cur_node.YCoords.Value.Item2;
                    limX = (x2 - x1) / 2 + x1; limY = (y2 - y1) / 2 + y1;
                    if (x2 - x1 == 0 && y2 - y1 == 0 && x1 == x && y1 == y)
                    {
                        break;
                    }
                    this.CompareCoordsAndInit(x, y, limX, limY, cur_node, ref next);
                    if (next == null)
                    {
                        result = false;
                        break;
                    }
                    cur_node = next;
                }
                return result;
            }
            void Decrement_Mass(int x, int y, ref Node node) // переделать под метод,
                //возвращающий ссылку
            {
                int limX, limY;
                int x1 = node.XCoords.Value.Item1, x2 = node.XCoords.Value.Item2,
                        y1 = node.YCoords.Value.Item1, y2 = node.YCoords.Value.Item2;
                limX = (x2 - x1) / 2 + x1; limY = (y2 - y1) / 2 + y1;
                if (x2 - x1 == 0 && y2 - y1 == 0)
                {
                    node = null; return;
                }
                ref Node next = ref this.CompareCoordsAndInit(x, y, limX, limY, node);
                if (next == null || next.MASS == 0 || x2 - x1 < 0 || y2 - y1 < 0)
                {
                    throw new ArgumentException("Неправильная точка");
                }
                this.Decrement_Mass(x, y, ref next);
                node.MASS--;
                if (node.MASS == 0)
                {
                    node = null;
                }
            }
            ref Node CompareCoordsAndInit(int x, int y, int limX, int limY, Node cur_node)
            {
                if (cur_node == null)
                {
                    throw new ArgumentNullException("Узел не может отсутствовать при сравнении");
                }
                if (x <= limX)
                {
                    if (y <= limY)
                    {
                        return ref cur_node.LXLY;
                    }
                    else
                    {
                        return ref cur_node.LXMY;
                    }
                }
                else
                {
                    if (y <= limY)
                    {
                        return ref cur_node.MXLY;
                    }
                    else
                    {
                        return ref cur_node.MXMY;
                    }
                }
            }
            void CompareCoordsAndInit(int x, int y, int limX, int limY, Node cur_node,
                ref Node next)
            {
                if (cur_node == null)
                {
                    throw new ArgumentNullException("Узел не может отсутствовать при сравнении");
                }
                if (x <= limX)
                {
                    if (y <= limY)
                    {
                        next = cur_node.LXLY;
                    }
                    else
                    {
                        next = cur_node.LXMY;
                    }
                }
                else
                {
                    if (y <= limY)
                    {
                        next = cur_node.MXLY;
                    }
                    else
                    {
                        next = cur_node.MXMY;
                    }
                }
            }
            class Node
            {
                int mass;
                public bool IsLeaf;
                public (int, int)? XCoords, YCoords;
                public Node LXLY, LXMY, MXLY, MXMY;
                public Node(int mass, (int, int)? XCoords, (int, int)? YCoords)
                {
                    this.MASS = mass;
                    this.LXLY = this.LXMY = this.MXLY = this.MXMY = null;
                    this.XCoords = XCoords;
                    this.YCoords = YCoords;
                    int x1, x2, y1, y2;
                    x1 = this.XCoords.Value.Item1; x2 = this.XCoords.Value.Item2;
                    y1 = this.YCoords.Value.Item1; y2 = this.YCoords.Value.Item2;
                    if (x2 - x1 == 0 && y2 - y1 == 0)
                    {
                        this.IsLeaf = true;
                    }
                    else
                    {
                        this.IsLeaf = false;
                    }
                }
                public Node() : this(0, null, null) { }
                public int MASS
                {
                    get => this.mass;
                    set
                    {
                        if (this.mass < 0)
                        {
                            throw new ArgumentException("ТАКИ НЕ МОЖЕТ БЫТЬ КОЛ-ВО ОТРИЦАТЕЛЬНЫМ");
                        }
                        this.mass = value;
                    }
                }
            }
        }
    }
}
