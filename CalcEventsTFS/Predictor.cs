using NumSharp;
using System;
using System.Linq;
using Tensorflow;
using Tensorflow.Util;

namespace CalcEventsTFS
{
    class Predictor : IDisposable
    {
        private readonly Session _session;
        private readonly Tensor[] _inputs;
        private readonly Tensor _output;

        public Predictor(string modelLocation)
        {
            lock (Locks.ProcessWide)
                _session = Session.LoadFromSavedModel(modelLocation).as_default();
            var inputs = new[] { "sp", "fuel" };

            _inputs = inputs.Select(name => _session.graph.OperationByName(name).output).ToArray();
            _output = _session.graph.OperationByName("softmax_tensor").output;
        }

        public int Predict(params float[][] inputs)
        {
            FeedItem[] feeds = new FeedItem[inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
                feeds[i] = new FeedItem(_inputs[i], new NDArray(inputs[i]));

            return _session.run(_output, feeds).argmax();
        }

        public void Dispose()
        {
            _session.close();
        }
    }
}
