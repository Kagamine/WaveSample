using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace System
{
    public class WaveStream : IDisposable
    {
        public WaveStream(Stream stream, int ByteSample = 2, int DataPosition = 40)
        {
            _baseStream = stream;
            _dataPosition = DataPosition;
            _byteSample = ByteSample;
            _length = (int)_baseStream.Length - DataPosition;
        }

        public WaveStream(string Path, int ByteSample = 2, int DataPosition = 40)
            : this(new FileStream(Path, FileMode.Open, FileAccess.Read), ByteSample, DataPosition)
        {
        }

        private Stream _baseStream;

        public Stream BaseStream { get { return _baseStream; } }

        private int _dataPosition, _byteSample, _length;
        
        private IEnumerable<double> _getSample(byte[] buffer, long cnt, long per)
        {
            for (var i = 0L; i < cnt; i+= per)
            {
                yield return BitConverter.ToInt32(buffer, (int)i) / 2147483648D;
            }
        }

        public async Task<IEnumerable<double>> GetSampleAsync(int Count)
        {
            var cnt = _length / _byteSample;
            var per = cnt / Count;
            if (per % _byteSample != 0)
                per = per - per % _byteSample;
            var buffer = new byte[_length];
            _baseStream.Position = _dataPosition;
            await _baseStream.ReadAsync(buffer, 0, _length);
            return _getSample(buffer, cnt, per);
        }

        public IEnumerable<double> GetSample(int Count)
        {
            var cnt = _length / _byteSample;
            var per = cnt / Count;
            var buffer = new byte[_length];
            _baseStream.Position = _dataPosition;
            _baseStream.Read(buffer, 0, _length);
            return _getSample(buffer, cnt, per);
        }

        public void Dispose()
        {
            _baseStream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
