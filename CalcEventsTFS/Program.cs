using System;
using System.Linq;
using System.Threading;

namespace CalcEventsTFS
{
    class Program
    {
        static int THREADS_COUNT = 10;
        static string modelLocation = @"../../../../model/";

        static void Main(string[] args)
        {
            if (args.Length > 0)
                modelLocation = args[0];

            if (args.Length > 1)
                THREADS_COUNT = int.Parse(args[1]);

            var tasks = new Thread[THREADS_COUNT];
            for (int t = 0; t < THREADS_COUNT; t++)
            {
                tasks[t] = new Thread(() =>
                {
                    var pr = new Predictor(modelLocation);
                    var inputs = new float[2][];
                    for (int i = 0; i < inputs.Length; i++)
                        inputs[i] = new float[96];

                    while (true)
                    {
                        try
                        {
                            pr.Predict(inputs);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }

            foreach (var t in tasks)
                t.Start();

            Console.WriteLine("Wait");
            foreach (var t in tasks)
                t.Join();

            Console.WriteLine("Complete");
            Console.ReadKey();
        }
    }
}
