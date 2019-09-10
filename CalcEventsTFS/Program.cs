using NumSharp;
using System;
using System.Linq;
using System.Threading;
using Tensorflow;
using Tensorflow.Util;

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

            for (int t = 0; t < THREADS_COUNT; t++)
            {
                new Thread(() =>
                {
                    Session sess;

                    lock (Locks.ProcessWide)
                        sess = Session.LoadFromSavedModel(modelLocation).as_default();

                    {
                        var inputs = new[] { "sp", "fuel" };

                        var inp = inputs.Select(name => sess.graph.OperationByName(name).output).ToArray();
                        var outp = sess.graph.OperationByName("softmax_tensor").output;

                        for (var i = 0; i < 1000; i++)
                        {
                            {
                                var data = new float[96];
                                FeedItem[] feeds = new FeedItem[2];

                                for (int f = 0; f < 2; f++)
                                    feeds[f] = new FeedItem(inp[f], new NDArray(data));

                                try
                                {
                                    sess.run(outp, feeds);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                            GC.Collect();
                        }
                    }
                }).Start();
            }
        }
    }
}
