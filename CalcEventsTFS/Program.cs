using System;
using System.Threading.Tasks;

namespace CalcEventsTFS
{
    class Program
    {
        static int THREADS_COUNT = 10;
        static string modelLocation = @"../../../../model/";
        static object predictLock = new object();
        static bool USE_LOCK = true;

        static void Main(string[] args)
        {
            if (args.Length > 0)
                modelLocation = args[0];

            if (args.Length > 1)
                THREADS_COUNT = int.Parse(args[1]);

            var tasks = new Task[THREADS_COUNT];
            for (int t = 0; t < THREADS_COUNT; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    var pr = new Predictor(modelLocation);
                    var inputs = new float[2][];
                    for (int i = 0; i < inputs.Length; i++)
                        inputs[i] = new float[96];

                    while (true)
                    {
                        try
                        {
                            if (USE_LOCK)
                            {
                                lock(predictLock)
                                    pr.Predict(inputs);
                            }
                            else
                                pr.Predict(inputs);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }

            Console.WriteLine("Wait");
            Task.WaitAll(tasks);

            Console.WriteLine("Complete");
            Console.ReadKey();
        }
    }
}
