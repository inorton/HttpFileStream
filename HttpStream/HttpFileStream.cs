using System;
using System.IO;

namespace HttpStream
{
	public class HttpFileStream : Stream
	{
		HttpTool tool;

		public HttpFileStream ( Uri webfile )
		{
			tool = new HttpTool(webfile);
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
				tool.Position = offset;
				break;
			case SeekOrigin.Current:
				tool.Position += offset;
				break;
			case SeekOrigin.End:
				if ( offset > 0 ) 
					throw new NotSupportedException();
				tool.Position = tool.Length - offset;
				break;

			default:
				break;
			}
			return tool.Position;
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return tool.Read ( buffer, offset, count );
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
				return tool.Length;
			}
		}

		public override long Position {
			get {
				return tool.Position;
			}
			set {
				tool.Position = value;
			}
		}

		public override void Close ()
		{
			tool.Close();
		}

		#endregion
	}
}

