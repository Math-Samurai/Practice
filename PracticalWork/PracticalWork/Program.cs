using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace PracticalWork
{
    abstract class V3Data
    {
        public V3Data(string data, DateTime time)
        {
            Data = data;
            Time = time;
        }
        public string Data
        {
            get; set;
        }
        public DateTime Time
        {
            get; set;
        }
        public abstract Vector2[] Nearest(Vector2 v);
        public abstract string ToLongString();
        public override string ToString()
        {
            return Data + "\n" + Time.ToString();
        }
    }
    struct DataItem
    {
        public DataItem(Vector2 coord, double module)
        {
            Coord = coord;
            Module = module;
        }
        public Vector2 Coord
        {
            get; set;
        }
        public double Module
        {
            get; set;
        }
        public override string ToString()
        {
            return "Coords = (" + Coord.X.ToString() + "," + Coord.Y.ToString() + "); Module = " +
                   Module.ToString();
        }
    }

    struct Grid1D
    {
        public Grid1D(float step, int count)
        {
            Step = step;
            Count = count;
        }

        public float Step
        {
            get; set;
        }
        public int Count
        {
            get; set;
        }
        public override string ToString()
        {
            return "Step = " + Step.ToString() + "; Count = " + Count.ToString() + "\n";
        }
    }
    class V3DataOnGrid : V3Data
    {
        public V3DataOnGrid(string data, DateTime time, Grid1D ox, Grid1D oy): base(data, time)
        {
            Ox = ox;
            Oy = oy;
        }
        public Grid1D Ox
        {
            get; set;
        }
        public Grid1D Oy
        {
            get; set;
        }
        public double[,] Field
        {
            get; set;
        }
        public static explicit operator V3DataCollection(V3DataOnGrid obj)
        {
            V3DataCollection res = new V3DataCollection(obj.Data, obj.Time);
            for (int i = 0; i < obj.Ox.Count; ++i)
            {
                for (int j = 0; j < obj.Oy.Count; ++j)
                {
                    Vector2 vec = new Vector2(i * obj.Ox.Step, j * obj.Oy.Step);
                    res.Add(new DataItem(vec, obj.Field[i, j]));
                }
            }
            return res;
        }
        public void InitRandom(double minValue, double maxValue)
        {
            Field = new double[Ox.Count, Oy.Count];
            Random rnd = new Random();
            for (int i = 0; i < Ox.Count; ++i)
            {
                for (int j = 0; j < Oy.Count; ++j)
                {
                    Field[i, j] = minValue + rnd.NextDouble() * (maxValue - minValue);
                }
            }
        }
        public override Vector2[] Nearest(Vector2 v)
        {
            List<Vector2> points = new List<Vector2>();
            double min_distance = Double.MaxValue;
            for (int i = 0; i < Ox.Count; ++i)
            {
                for (int j = 0; j < Oy.Count; ++j)
                {
                    Vector2 point = new Vector2(i * Ox.Step, j * Oy.Step);
                    double distance = Vector2.Distance(v, point);
                    int res = distance.CompareTo(min_distance);
                    if (res == 0)
                    {
                        points.Add(point);
                    } else if (res < 0)
                    {
                        min_distance = distance;
                        points.Clear();
                        points.Add(point);
                    }
                }
            }
            return points.ToArray();
        }
        public override string ToString()
        {
            return "V3DataOnGrid\n" + base.ToString() + "\nOx: " + Ox.ToString() + "Oy: " +
                   Oy.ToString();
        }
        public override string ToLongString()
        {
            string res = this.ToString();
            for (int i = 0; i < Ox.Count; ++i)
            {
                for (int j = 0; j < Oy.Count; ++j)
                {
                    res += "Field(" + (i * Ox.Step).ToString() + " " + (j * Oy.Step).ToString() +
                           ") = " + Field[i, j].ToString() + "\n";
                }
            }
            return res + "\n";
        }
    }
    class V3DataCollection: V3Data
    {
        public V3DataCollection(string data, DateTime time) : base(data, time)
        {
            List = new List<DataItem>();
        }
        public List<DataItem> List
        {
            get; set;
        }
        public void Add(DataItem item)
        {
            List.Add(item);
        }
        public void InitRandom(int nItems, float xmax, float ymax, double minValue, double maxValue)
        {
            Random rand = new Random();
            for (int i = 0; i < nItems; ++i)
            {
                DataItem item = new DataItem(new Vector2((float)rand.NextDouble() * xmax,
                                             (float)rand.NextDouble() * ymax),
                                             minValue + rand.NextDouble() * (maxValue - minValue));
                List.Add(item);
            }
        }
        public override Vector2[] Nearest(Vector2 v)
        {
            List<Vector2> points = new List<Vector2>();
            double min_distance = Double.MaxValue;
            foreach (DataItem item in List)
            {
                Vector2 point = new Vector2(item.Coord.X, item.Coord.Y);
                double distance = Vector2.Distance(v, point);
                int res = distance.CompareTo(min_distance);
                if (res < 0)
                {
                    min_distance = distance;
                    points.Clear();
                    points.Add(point);
                } else if (res == 0)
                {
                    points.Add(point);
                }
            }
            return points.ToArray();
        }
        public override string ToString()
        {
            return "V3DataCollection\n" + base.ToString() + "\nLength = " + List.Count.ToString() +
                   "\n";
        }
        public override string ToLongString()
        {
            string res = this.ToString();
            for (int i = 0; i < List.Count; ++i)
            {
                res += "Field(" + List[i].Coord.X.ToString() + " " + List[i].Coord.Y.ToString() +
                       ") = " + List[i].Module + "\n";
            }
            return res + "\n";
        }
    }
    class V3MainCollection: IEnumerable<V3Data>
    {
        private List<V3Data> list;
        public int Count
        {
            get
            {
                return list.Count;
            }
        }
        public void Add(V3Data item)
        {
            if (list == null)
            {
                list = new List<V3Data>();
            }
            list.Add(item);
        }
        private static bool ToRemove(string id, DateTime date, V3Data obj)
        {
            return obj.Data == id && obj.Time == date;
        }
        public IEnumerator<V3Data> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return list[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        public bool Remove(string id, DateTime date)
        {
            if (list.RemoveAll(x =>  id == x.Data && date == x.Time) > 0)
            {
                return true;
            }
            return false;
        }
        public void AddDefaults()
        {
            list = new List<V3Data>();
            V3DataOnGrid item =
                new V3DataOnGrid("Uniform;0.1f;11;0.1f,11", new DateTime(1), new Grid1D(0.1f, 11), new Grid1D(0.1f, 11));
            item.InitRandom(0f, 10f);
            list.Add(item);

            item =
                new V3DataOnGrid("Uniform;0.05f;21;0.05f;21", new DateTime(2), new Grid1D(0.05f, 21), new Grid1D(0.05f, 21));
            item.InitRandom(0f, 10f);
            list.Add(item);

            item =
                new V3DataOnGrid("Uniform;0.5f;3;0.5f;3", new DateTime(3), new Grid1D(0.5f, 3), new Grid1D(0.5f, 3));
            item.InitRandom(0f, 10f);
            list.Add(item);

            V3DataCollection item1 = new V3DataCollection("Ununiform", new DateTime(4));
            item1.InitRandom(10, 1, 1, 0, 10);
            list.Add(item1);

            item1 = new V3DataCollection("Ununiform", new DateTime(5));
            item1.InitRandom(20, 1, 1, 0, 10);
            list.Add(item1);

            item1 = new V3DataCollection("Ununiform", new DateTime(5));
            item1.InitRandom(4, 1, 1, 0, 10);
            list.Add(item1);
        }
        public override string ToString()
        {
            string res = "";
            foreach(V3Data item in list)
            {
                res += item.ToLongString();
            }
            return res;
        }
        public string Nearest(Vector2 vec)
        {
            string res = "";
            foreach (V3Data item in list)
            {
                Vector2[] near = item.Nearest(vec);
                foreach (Vector2 v in near)
                {
                    res += "The Nearest point is (" + v.X.ToString() + " " + v.Y.ToString() + ")\n";
                }
            }
            return res;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            V3DataOnGrid obj1 = new V3DataOnGrid("test", new DateTime(1), new Grid1D(0.1f, 11),
                                                 new Grid1D(0.1f, 11));
            obj1.InitRandom(0.0f, 10.0f);
            Console.WriteLine(obj1.ToLongString());
            V3DataCollection obj2 = (V3DataCollection)obj1;

            V3MainCollection obj3 = new V3MainCollection();
            obj3.AddDefaults();
            Console.WriteLine(obj3.ToString());

            Vector2 vec = new Vector2(0.32f, 0.32f);
            Console.WriteLine(obj3.Nearest(vec));
        }
    }
}