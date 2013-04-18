using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text;

using System.Reflection;

namespace HttpStream
{
	internal class HttpTool
	{
        public const int DefaultRangeSize = 16384;
        public const int MinRangeSize = 2048;
		public Uri RemoteFile { get; private set; }

        long position;
        public long Position
        {
            get
            {
                return position;
            }
            set
            {
                if ( position != value ) Close(); // caller must have seek'd need a new request
                position = value;
            }
        }

		public HttpTool (Uri file)
		{
			RemoteFile = file;
			var h = GetHead();
			Length = h.ContentLength;
		}

		static MethodInfo AddWithoutValidateMethod = typeof(WebHeaderCollection).GetMethod ("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);

		HttpWebRequest Request ()
		{
			return WebRequest.Create (RemoteFile) as HttpWebRequest;
		}

		public HttpWebResponse GetHead ()
		{
			lock (RemoteFile) {
				var req = Request ();
				req.Method = "HEAD";
				var resp = req.GetResponse () as HttpWebResponse;
				resp.Close ();
				if (resp.ContentLength < 0)
					throw new FileLoadException ();
				return resp;
			}
		}

		public void Close ()
		{
            try {
                currentResponse.Close();
            } catch {}
            currentResponse = null;
		}

        int rangesize = DefaultRangeSize;

        public void SetDesiredHttpRangeSize( int count )
        {
            if ( count < MinRangeSize ) count = MinRangeSize;
            rangesize = count;
        }

        HttpWebResponse currentResponse = null;

        void BeginReadRequest( int count )
        {
            if ( count < rangesize ) count = rangesize;

            var end = Position + count; // read ahead some extra
            if (end > Length) {
                end = Length;
            }
            if ( end < Length ) end--;
            
            var req = Request ();
            var range = String.Format ("bytes={0}-{1}", Position, end);
            req.Method = "GET";
            AddWithoutValidateMethod.Invoke (req.Headers, new string[] { 
                HttpRequestHeader.Range.ToString (),  range });

            var resp = req.GetResponse() as HttpWebResponse;
            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent) {
                currentResponse = resp;
            } else {
                throw new EndOfStreamException ();
            }
        }

		public int Read (byte[] buf, int offset, int count)
		{
			lock (RemoteFile) {

				if (count + offset > buf.Length) {
					count = buf.Length - offset;
				}

                if ( Position >= Length ) return 0;

                int readbytes = 0;
                int chunkbytes = 0;
                do {
                    if ( currentResponse == null ) BeginReadRequest( count );
                    var str = currentResponse.GetResponseStream ();

                    var off = offset + readbytes;
                    var size = count - readbytes;
                    chunkbytes = str.Read (buf, off, size);
                    readbytes += chunkbytes;
                    position += chunkbytes;

                    if ( chunkbytes == 0 ) {
                        if ( Position < Length ) {
                            // there is some left, make another request and repeat
                            Close();
                            continue; // read the next range
                        } else {
                            break; // end of file on server
                        }
                    }

                } while ( readbytes < count );


                if ( readbytes == 0 ) {
                    Close(); // make a fresh request for the next chunk
                }


                return readbytes;
			}
		}

        public long Length { get; private set; }
	}
}
