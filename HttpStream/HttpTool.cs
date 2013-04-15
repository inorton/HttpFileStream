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

		public Uri RemoteFile { get; private set; }

		public long Position { get; set; }

		public HttpTool (Uri file)
		{
			RemoteFile = file;
			var h = GetHead();

			Length = h.ContentLength;

			Console.Error.WriteLine( h.Headers );
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

		}

		public int Read (byte[] buf, int offset, int count)
		{
			lock (RemoteFile) {
				if (count + offset > buf.Length) {
					count = buf.Length - offset;
				}

				var end = Position + count;
				if (end > Length) {
					end = Length;
				}

				//end--;

				if ( end == Position ) return 0;

				var req = Request ();
				var range = String.Format ("bytes={0}-{1}", Position, end);
				req.Method = "GET";
				AddWithoutValidateMethod.Invoke (req.Headers, new string[] { 
				HttpRequestHeader.Range.ToString (),  range });

				Console.Error.WriteLine ("requested range {0}", range);
				var resp = req.GetResponse () as HttpWebResponse;

				Console.Error.WriteLine( "got : \n" + resp.Headers );

				try {
					if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent) {
						var str = resp.GetResponseStream ();
						int readbytes = 0;
						int chunkbytes = 0;
						Console.Error.WriteLine("Position {0}", Position );
						do {
							var off = offset + readbytes;
							var size = count - readbytes;
							chunkbytes = str.Read (buf, off, size);
							readbytes += chunkbytes;
							Console.Error.WriteLine("total {0} chunk {1}, offset {2}", readbytes, chunkbytes, off );
						} while ( (chunkbytes > 0) && ( readbytes < count ) );

						Position += readbytes;// - 1;

						return readbytes;
					} 
					throw new EndOfStreamException ();
				} finally {
					resp.Close ();
					Console.Error.WriteLine("Position {0}", Position );
				}
			}
		}

		long len = -1;

		public long Length {
			get {
				return len;
			}
			private set {
				len = value;
			}
		}
	}
}
