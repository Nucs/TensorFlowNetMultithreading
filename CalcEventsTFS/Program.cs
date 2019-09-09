using System;
using System.Threading.Tasks;

namespace CalcEventsTFS
{
    class Program
    {
        const int THREADS_COUNT = 10;
        static string modelLocation = @"../../../../model/";

        static void Main(string[] args)
        {
            if (args.Length > 0)
                modelLocation = args[0];

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
