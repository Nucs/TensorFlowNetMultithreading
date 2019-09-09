using NumSharp;
using System;
using Tensorflow;

namespace CalcEventsTFS
{
    class TestData
    {
        public void Run(string modelLocation)
        {
            var res = Preprocessing.PrepareWindows(@"d:\Projects\ML\events_categorize\fillings_test.json");

            using (var sess = Session.LoadFromSavedModel(modelLocation))
            {
                var sp = sess.graph.OperationByName("sp").output;
                var fuel = sess.graph.OperationByName("fuel").output;

                var softmax_tensor = sess.graph.OperationByName("softmax_tensor").output;

                int lastPredict = -1;

                for (int i = 0; i < res.Dates.Length; i++)
                {
                    var sp_wind = res.Windows["sp"][i];
                    var fuel_wind = res.Windows["fuel"][i];

                    var o = sess.run(softmax_tensor,
                                     new FeedItem(sp, new NDArray(sp_wind)),
                                     new FeedItem(fuel, new NDArray(fuel_wind)));
                    var r = o.argmax();

                    if (lastPredict != r)
                    {
                        Console.WriteLine("{0} - {1}", res.Dates[i], r);
                        lastPredict = r;
                    }
                    //Console.WriteLine($"Result: {o}");
                }
            }
        }
    }
}
