using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CalcEventsTFS
{
    class Preprocessing
    {
        public class WindowsResult
        {
            public Dictionary<string, float[][]> Windows;

            public int[] Dates;
        }

        public static WindowsResult PrepareWindows(string path)
        {
            var root = JObject.Parse(File.ReadAllText(path));

            return PrepareWindows(root, new string[] { "sp", "fuel" }, 300, 150, 96, 40);
        }


        public static WindowsResult PrepareWindows(JObject data, string[] inputs, int windowSeconds, int paddingSeconds, int width, float maxSpeed)
        {
            var sensors = data["sensors"];

            IEnumerable<int> allDates = null;
            foreach (var key in inputs)
            {
                var sel = sensors[key].Select(i => (int)i[0]);
                if (allDates == null)
                    allDates = sel;
                else
                    allDates = allDates.Concat(sel);
            }

            var dates = allDates.Distinct().OrderBy(i => i).ToArray();

            Dictionary<string, float> sensMin = new Dictionary<string, float>();
            Dictionary<string, float> sensMax = new Dictionary<string, float>();

            foreach (var key in inputs)
            {
                if (key == "sp")
                {
                    sensMin[key] = 0;
                    sensMax[key] = maxSpeed;
                }
                else
                {
                    var values = sensors[key].Select(i => (float)i[1]);

                    sensMin[key] = values.Min();
                    sensMax[key] = values.Max();
                }
            }

            var res = new Dictionary<string, float[][]>();
            foreach (var key in inputs)
            {
                var values = sensors[key].ToArray();
                var min = sensMin[key];
                var max = sensMax[key];
                int ind = 0;

                var windows = new List<float[]>();

                for (int i = 0; i < dates.Length; i++)
                {
                    var t = dates[i];
                    var t0 = t - paddingSeconds;
                    var t1 = t0 + windowSeconds;
                    var window = key == "sp" ? RasterWindowSq(ref ind, values, t0, t1, min, max, width)
                                             : RasterWindow(ref ind, values, t0, t1, min, max, width);
                    windows.Add(window.ToArray());
                }

                res[key] = windows.ToArray();
            }

            return new WindowsResult
            {
                Windows = res,
                Dates = dates
            };
        }

        private static List<float> RasterWindow(ref int ind, JToken[] data, int minX, int maxX, float minY, float maxY, int width)
        {
            var dx = (double)(maxX - minX) / width;
            var dy = maxY - minY;
            double currX = minX;

            var window = new List<float>();
            var first = true;

            for (int i = ind; i < data.Length - 1; i++)
            {
                int date0 = (int)data[i][0];
                float val0 = (float)data[i][1];
                int date1 = (int)data[i + 1][0];
                float val1 = (float)data[i + 1][1];

                if (date1 < currX) continue;

                if (date0 >= maxX) break;

                if (first)
                {
                    ind = i;
                    first = false;

                    while (currX <= date0)
                    {
                        var val = (val0 - minY) / dy;
                        window.Add(val);
                        currX += dx;
                    }
                }

                while (currX <= date1)
                {
                    var val = val0 + (currX - date0) * (val1 - val0) / (date1 - date0);
                    val = (val - minY) / dy;
                    window.Add((float)val);
                    currX += dx;
                }
            }

            if (window.Count < width)
            {
                var last = window.Count > 0 ? window[window.Count - 1] : 0;
                int c = width - window.Count;
                for (int i = 0; i < c; i++)
                {
                    window.Add(last);
                }

            }
            else while (window.Count > width)
                {
                    window.RemoveAt(window.Count - 1);
                }

            if (window.Count != width)
                throw new Exception();

            return window;
        }

        private static List<float> RasterWindowSq(ref int ind, JToken[] data, int minX, int maxX, float minY, float maxY, int width)
        {
            var dx = (double)(maxX - minX) / width;
            var dy = maxY - minY;
            double currX = minX;

            var window = new List<float>();
            var first = true;

            for (int i = ind; i < data.Length - 1; i++)
            {
                int date0 = (int)data[i][0];
                float val0 = (float)data[i][1];
                int date1 = (int)data[i + 1][0];
                float val1 = (float)data[i + 1][1];

                //if (date1 < currX) continue;

                if (date0 >= maxX) break;

                val0 = (val0 - minY) / dy;

                if (first)
                {
                    ind = i;
                    first = false;

                    while (currX <= date0)
                    {
                        window.Add(0);
                        currX += dx;
                    }
                }

                while (currX <= date1)
                {
                    window.Add(val0);
                    currX += dx;
                }
            }

            if (window.Count < width)
            {
                var last = window.Count > 0 ? window[window.Count - 1] : 0;
                int c = width - window.Count;
                for (int i = 0; i < c; i++)
                {
                    window.Add(last);
                }

            }
            else while (window.Count > width)
                {
                    window.RemoveAt(window.Count - 1);
                }

            if (window.Count != width)
                throw new Exception();

            return window;
        }
    }
}
