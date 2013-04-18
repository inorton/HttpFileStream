using System;
using System.IO;

namespace HttpStream
{
	public class HttpFileStream : Stream
	{
        HttpTool Tool { get; set; }

		public HttpFileStream ( Uri webfile )
		{
			Tool = new HttpTool(webfile);
		}

        public void AdjustHTTPReadSize( int bytes ) {
            Tool.SetDesiredHttpRangeSize( bytes );
        }

		#region implemented abstract members of Stream

		public override void Flush ()
		{
			throw new NotSupportedException ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			switch ( origin ){
			case SeekOrigin.Begin:
				Tool.Position = offset;
				break;
			case SeekOrigin.Current:
				Tool.Position += offset;
				break;
			case SeekOrigin.End:
				if ( offset > 0 ) 
					throw new NotSupportedException();
				Tool.Position = Tool.Length - offset;
				break;

			default:
				break;
			}
			return Tool.Position;
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return Tool.Read ( buffer, offset, count );
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return true;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				return Tool.Length;
			}
		}

		public override long Position {
			get {
				return Tool.Position;
			}
			set {
				Tool.Position = value;
			}
		}

		public override void Close ()
		{
			Tool.Close();
		}

		#endregion
	}
}

