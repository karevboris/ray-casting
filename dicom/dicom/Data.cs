using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace dicom
{
    public class Data
    {
        public short[] data;
        public int W;
        public int H;
        public int D;
        public float sX;
        public float sY;
        public float sZ;
        public Data(short[] dat, int w, int h, int d, float sx, float sy, float sz)
        {
            data = new short[w*h*d];
            data = dat;
            W = w;
            H = h;
            D = d;
            sX = sx;
            sY = sy;
            sZ = sz;
        }
        public Data readBinFile(string filename)
        {
            Data res;
            FileStream stream;
            stream = File.OpenRead(filename);

            byte[] data;
            byte[] width = new byte[sizeof(int)];
            byte[] height = new byte[4];
            byte[] depth = new byte[4];
            byte[] scaleX = new byte[4];
            byte[] scaleY = new byte[4];
            byte[] scaleZ = new byte[4];
            int sizeOfDataArray = 0;

            stream.Read(width, 0, 4);
            stream.Seek(4, SeekOrigin.Begin);
            stream.Read(height, 0, 4);
            stream.Seek(8, SeekOrigin.Begin);
            stream.Read(depth, 0, 4);

            stream.Seek(12, SeekOrigin.Begin);
            stream.Read(scaleX, 0, 4);
            stream.Seek(16, SeekOrigin.Begin);
            stream.Read(scaleY, 0, 4);
            stream.Seek(20, SeekOrigin.Begin);
            stream.Read(scaleZ, 0, 4);

            sizeOfDataArray = BitConverter.ToInt32(width, 0) * BitConverter.ToInt32(height, 0) * BitConverter.ToInt32(depth, 0);
            short[] dat =new short[sizeOfDataArray];

            data = new byte[sizeof(short) * sizeOfDataArray];
            stream.Seek(24, SeekOrigin.Begin);
            int readResult = stream.Read(data, 0, data.Length);
            for (int i = 0; i < sizeOfDataArray; i++)
            {
                byte[] tmp = new byte[2];
                tmp[0] = data[0+i*2];
                tmp[1] = data[1+i*2];
                dat[i] = BitConverter.ToInt16(tmp, 0);
            }

                res = new Data(dat, BitConverter.ToInt32(width, 0), BitConverter.ToInt32(height, 0), BitConverter.ToInt32(depth, 0), BitConverter.ToSingle(scaleX, 0), BitConverter.ToSingle(scaleY, 0), BitConverter.ToSingle(scaleZ, 0));
            return res;
        }
    }
}
