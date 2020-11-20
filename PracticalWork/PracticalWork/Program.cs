using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using System.Linq;



namespace PracticalWork
{
    enum Consts
    {
        separator = ' '
    }
    abstract class V3Data
    {
        public V3Data(string data, DateTime time)
        {
            Data = data;
            Time = time;
        }
        public V3Data(string filename)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(filename);
            } catch (FileNotFoundException)
            {
                Console.WriteLine("No such file exists.");
                return;
            } catch (ArgumentException)
            {
                Console.WriteLine("File name is empty string or null.");
                return;
            } catch (IOException)
            {
                Console.WriteLine("Incorrect filename.");
                return;
            }
            try
            {
                string temp;
                temp = sr.ReadLine();
                if (temp != null)
                {
                    string[] bases = temp.Split();
                    Data = bases[0];
                    Time = DateTime.ParseExact(bases[1], "dd-MM-yyyy", new CultureInfo("ru-ru"));
                }
            } catch (OutOfMemoryException)
            {
                Console.WriteLine("Not enough memory to allocate string.");
            } catch (IOException)
            {
                Console.WriteLine("Input/output error.");
            } catch (ArgumentNullException)
            {
                Console.WriteLine("Incorrect data in file.");
            } catch (FormatException)
            {
                Console.WriteLine("Incorrect data in file.");
            } finally
            {
                sr.Close();
            }
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
        public abstract string ToLongString(string format);
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
            return "Coords = (" + Coord.X.ToString() + ", " + Coord.Y.ToString() + "); Module = " +
                   Module.ToString();
        }
        public string ToLongString(string format)
        {
            return "Coords = (" + Coord.X.ToString(format) + ", " + Coord.Y.ToString(format) + "); Module = " +
                    Module.ToString(format);
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
        public string ToString(string format)
        {
            return "Step = " + Step.ToString(format) + "; Count = " + Count.ToString(format) + "\n";
        }
    }
    class V3DataOnGrid : V3Data, IEnumerable<DataItem>
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
        public override string ToLongString(string format)
        {
            string res = this.ToString();
            for (int i = 0; i < Ox.Count; ++i)
            {
                for (int j = 0; j < Oy.Count; ++j)
                {
                    res += "Field(" + (i * Ox.Step).ToString(format) + " " + (j * Oy.Step).ToString(format) +
                           ") = " + Field[i, j].ToString(format) + "\n";
                }
            }
            return res + "\n";
        }
        public IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < Ox.Count; ++i)
            {
                for (int j = 0; j < Oy.Count; ++j)
                {
                    DataItem item = new DataItem(new Vector2(Ox.Step * i, Oy.Step * j), Field[i, j]);
                    yield return item;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    class V3DataCollection: V3Data, IEnumerable<DataItem>
    {
        public V3DataCollection(string data, DateTime time) : base(data, time)
        {
            List = new List<DataItem>();
        }
        public V3DataCollection(string filename): base(filename)
        {
            List = new List<DataItem>();
            StreamReader sr;
            try
            {
                sr = new StreamReader(filename);
            } catch (FileNotFoundException)
            {
                Console.WriteLine("No such file exists.");
                return;
            } catch (ArgumentException)
            {
                Console.WriteLine("File name is empty string or null.");
                return;
            } catch (IOException)
            {
                Console.WriteLine("Incorrect filename.");
                return;
            }
            try
            {
                string temp;
                sr.ReadLine();
                while ((temp = sr.ReadLine()) != null)
                {
                    string[] nums = temp.Split((char)Consts.separator);
                    DataItem item = new DataItem();
                    item.Coord = new Vector2(float.Parse(nums[0]), float.Parse(nums[1]));
                    item.Module = float.Parse(nums[2]);
                    List.Add(item);
                }
            } catch (OutOfMemoryException)
            {
                Console.WriteLine("Not enough memory to allocate string.");
                List.Clear();
            } catch (IOException)
            {
                Console.WriteLine("Input/output error.");
                List.Clear();
            } catch (ArgumentNullException)
            {
                Console.WriteLine("Incorrect data in file.");
                List.Clear();
            } catch (FormatException)
            {
                Console.WriteLine("Incorrect data in file.");
                List.Clear();
            } catch (OverflowException)
            {
                Console.WriteLine("Some numbers in file are too big.");
                List.Clear();
            }
            finally
            {
                sr.Close();
            }
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
        public override string ToLongString(string format)
        {
            string res = this.ToString();
            for (int i = 0; i < List.Count; ++i)
            {
                res += "Field(" + List[i].Coord.X.ToString(format) + " " + List[i].Coord.Y.ToString(format) +
                       ") = " + List[i].Module.ToString(format) + "\n";
            }
            return res + "\n";
        }
        public IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < List.Count; ++i)
            {
                yield return List[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
        public int MinNumberOfCalcs
        {
            get
            {
                var V3Grid = from item in list where item.GetType() == typeof(V3DataOnGrid) select item;
                var V3Collection = from item in list where item.GetType() == typeof(V3DataCollection) select item;
                var V3GridCount = from V3DataOnGrid item in V3Grid select item.Field.Length;
                var V3CollectionCount = from V3DataCollection item in V3Collection select item.List.Count;
                var V3GridCountMin = V3GridCount.Min();
                var V3GridCollectionMin = V3CollectionCount.Min();
                return (V3GridCountMin < V3GridCollectionMin) ? (V3GridCountMin) : (V3GridCollectionMin);
            }
        }
        public float MaxDistance
        {
            get
            {
                var V3Grid = from item in list where item.GetType() == typeof(V3DataOnGrid) select item;
                var V3Collection = from item in list where item.GetType() == typeof(V3DataCollection) select item;
                var V3GridMaxDistances = from V3DataOnGrid item in V3Grid select Vector2.Distance(new Vector2(item.Ox.Step * (item.Ox.Count - 1), item.Oy.Step * (item.Oy.Count - 1)), new Vector2(0, 0));
                var V3CollectionDistances = from V3DataCollection item in V3Collection from elem1 in item.List from elem2 in item.List select Vector2.Distance(new Vector2(elem1.Coord.X, elem1.Coord.Y), new Vector2(elem2.Coord.X, elem2.Coord.Y));
                float V3GridMaxDistance = V3GridMaxDistances.Max();
                float V3CollectionMaxDistance = V3CollectionDistances.Max();
                return (V3GridMaxDistance > V3CollectionMaxDistance) ? V3GridMaxDistance : V3CollectionMaxDistance;
            }
        }
        public IEnumerable<DataItem> GetMultiplePoints
        {
            get
            {
                var V3Grid = from item in list where item.GetType() == typeof(V3DataOnGrid) select item;
                var V3Collection = from item in list where item.GetType() == typeof(V3DataCollection) select item;
                var V3GridCollection = from V3DataOnGrid item in V3Grid select (V3DataCollection)item;

                var V3CollectionLists = from V3DataCollection item in V3Collection select item.List;
                var V3GridCollectionLists = from V3DataCollection item in V3GridCollection select item.List;

                var V3CollectionDataItems = from List<DataItem> item in V3CollectionLists from elem in item select elem;
                var V3GridCollectionDataItems = from List<DataItem> item in V3GridCollectionLists from elem in item select elem;

                var dataItems = V3CollectionDataItems.Concat(V3GridCollectionDataItems);

                var res = from item1 in dataItems from item2 in dataItems where item1.Coord == item2.Coord where item1.Module != item2.Module select item1;

                return res;
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
                new V3DataOnGrid("Uniform grid; Axis X: step = 0.1, count = 11; Axis Y: step = 0.1; count = 11", DateTime.Now, new Grid1D(0.1f, 11), new Grid1D(0.1f, 11));
            item.InitRandom(0f, 10f);
            list.Add(item);

            V3DataOnGrid item1 =
                new V3DataOnGrid("Uniform grid; Axis X: step = 0.1, count = 11; Axis Y: step = 0.1; count = 11", DateTime.Now, new Grid1D(0.1f, 0), new Grid1D(0.1f, 0));
            item1.InitRandom(0f, 10f);
            list.Add(item1);

            V3DataCollection item2 = new V3DataCollection("Test.txt");
            list.Add(item2);

            V3DataCollection item3 = new V3DataCollection("Data", DateTime.Now);
            item3.InitRandom(0, 0.1f, 0.1f, 0.1, 0.1);
            list.Add(item3);
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
        public string ToString(string format)
        {
            string res = "";
            foreach (V3Data item in list)
            {
                res += item.ToLongString(format);
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
            string format = "F5";
            V3DataCollection obj1 = new V3DataCollection("Test.txt");
            Console.WriteLine(obj1.ToLongString(format));

            V3MainCollection obj2 = new V3MainCollection();
            obj2.AddDefaults();
            Console.WriteLine("Minimum number of calculations = " + obj2.MinNumberOfCalcs);
            Console.WriteLine("Maximum distance between points = " + obj2.MaxDistance);
            Console.WriteLine("Information of multiple DataItems:");
            IEnumerable<DataItem> mulpiplePoints = obj2.GetMultiplePoints;
            foreach (DataItem i in mulpiplePoints)
            {
                Console.WriteLine(i.ToLongString(format));
            }
        }
    }
}