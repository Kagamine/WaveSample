using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace System
{
    public class WaveStream : IDisposable
    {
        public WaveStream(Stream stream, int ByteSample = 4, int DataPosition = 40)
        {
            _baseStream = stream;
            _dataPosition = DataPosition;
            _byteSample = ByteSample;
            _length = (int)_baseStream.Length - DataPosition;
        }

        public WaveStream(string Path, int ByteSample = 4, int DataPosition = 40)
            : this(new FileStream(Path, FileMode.Open, FileAccess.Read), ByteSample, DataPosition)
        {
        }

        private Stream _baseStream;

        public Stream BaseStream { get { return _baseStream; } }

        private int _dataPosition, _byteSample, _length;
        
        private IEnumerable<double> _getSample(byte[] buffer, long cnt, long per, bool ave)
        {
            var tmp = new List<double>();
            for (var i = 0L; i < cnt; i+= per)
            {
                if (ave)
                {
                    for (var j = i; j < cnt && j < i + 10; j += _byteSample * 2)
                    {
                        if (_byteSample == 1)
                        {
                            tmp.Add(buffer[j] / 256D);
                        }
                        else if (_byteSample == 2)
                        {
                            tmp.Add(BitConverter.ToInt16(buffer, (int)j) / 32768D);
                        }
                        else if (_byteSample == 4)
                        {
                            tmp.Add(BitConverter.ToInt32(buffer, (int)j) / 2147483648D);
                        }
                        else // _byteSample == 8
                        {
                            tmp.Add(BitConverter.ToInt64(buffer, (int)j) / 9223372036854775808D);
                        }
                    }
                    yield return tmp.Average();
                    tmp.Clear();
                }
                else
                {
                    if (_byteSample == 1)
                    {
                        yield return buffer[i] / 256D;
                    }
                    else if (_byteSample == 2)
                    {
                        yield return BitConverter.ToInt16(buffer, (int)i) / 32768D;
                    }
                    else if (_byteSample == 4)
                    {
                        yield return BitConverter.ToInt32(buffer, (int)i) / 2147483648D;
                    }
                    else // _byteSample == 8
                    {
                        yield return BitConverter.ToInt64(buffer, (int)i) / 9223372036854775808D;
                    }
                }
            }
        }

        public async Task<IEnumerable<double>> GetSampleAsync(int Count, bool Average = false)
        {
            var cnt = _length / _byteSample;
            var per = cnt / Count;
            if (per % _byteSample != 0)
                per = per - per % _byteSample;
            var buffer = new byte[_length];
            _baseStream.Position = _dataPosition;
            await _baseStream.ReadAsync(buffer, 0, _length);
            return _getSample(buffer, cnt, per, Average);
        }

        public IEnumerable<double> GetSample(int Count, bool Average = false)
        {
            var cnt = _length / _byteSample;
            var per = cnt / Count;
            var buffer = new byte[_length];
            _baseStream.Position = _dataPosition;
            _baseStream.Read(buffer, 0, _length);
            return _getSample(buffer, cnt, per, Average);
        }

        public void Dispose()
        {
            _baseStream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
