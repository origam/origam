#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System.IO;
using System.Security.Cryptography;

namespace Origam.Server
{
    public class MD5PassThroughStream : Stream
    {
        private Stream wrappedStream;

        private MD5 md5;

        public MD5PassThroughStream(Stream stream)
        {
            wrappedStream = stream;
            md5 = MD5CryptoServiceProvider.Create();
            md5.Initialize();
        }

        public override bool CanRead
        {
            get { return wrappedStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return wrappedStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return wrappedStream.CanWrite; }
        }

        public override void Flush()
        {
            wrappedStream.Flush();
        }

        public override long Length
        {
            get { return wrappedStream.Length; }
        }

        public override long Position
        {
            get
            {
                return wrappedStream.Position;
            }
            set
            {
                wrappedStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int returnValue = wrappedStream.Read(buffer, offset, count);
            if (returnValue != 0)
            {
                md5.TransformBlock(buffer, 0, returnValue, null, 0);
            }
            return returnValue;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return wrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            wrappedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            wrappedStream.Write(buffer, offset, count);
        }

        /**
         * Call when transfer is finished.
         */
        public byte[] GetMD5()
        {
            md5.TransformFinalBlock(new byte[0], 0, 0);
            return md5.Hash;
        }
    }
}
