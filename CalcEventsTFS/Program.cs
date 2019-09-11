using NumSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Tensorflow;
using Tensorflow.Util;
using System.Runtime.CompilerServices;

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
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    Session sess;
                    Tensor[] inp = null;
                    Tensor outp = null;
                    lock (Locks.ProcessWide) sess = LoadFromSavedModel(modelLocation);
                    var inputs = new[] {"sp", "fuel"};
                    inp = inputs.Select(name => sess.graph.OperationByName(name).output).ToArray();
                    outp = sess.graph.OperationByName("softmax_tensor").output;
                    {
                        while (true)
                        {
                            {
                                var data = new float[96];
                                FeedItem[] feeds = new FeedItem[2];

                                for (int f = 0; f < 2; f++)
                                    feeds[f] = new FeedItem(inp[f], new NDArray(data));

                                try
                                {
                                    sess.run(outp, feeds);
                                } catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    throw;
                                }
                            }
                        }

                        Console.WriteLine($"TID done {Thread.CurrentThread.ManagedThreadId}");
                    }
                }).Start();
            }

            Console.WriteLine("Done Starting Threads");
            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static Session LoadFromSavedModel(string path)
        {
            lock (Locks.ProcessWide)
            {
                var graph = c_api.TF_NewGraph();
                var status = new Status();
                var opt = new SessionOptions();

                var tags = new string[] {"serve"};
                var buffer = new TF_Buffer();

                IntPtr sess;
                try
                {
                    sess = c_api.TF_LoadSessionFromSavedModel(opt,
                        IntPtr.Zero,
                        path,
                        tags,
                        tags.Length,
                        graph,
                        ref buffer,
                        status);
                    status.Check(true);
                } catch (TensorflowException ex) when (ex.Message.Contains("Could not find SavedModel"))
                {
                    status = new Status();
                    sess = c_api.TF_LoadSessionFromSavedModel(opt,
                        IntPtr.Zero,
                        Path.GetFullPath(path),
                        tags,
                        tags.Length,
                        graph,
                        ref buffer,
                        status);
                    status.Check(true);
                }

                return new Session(sess, g: new Graph(graph)).as_default();
            }
        }
    }
}