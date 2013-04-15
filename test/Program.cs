using System;
using System.Linq;
using HttpStream;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;

namespace test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Run (4096);
			Console.Error.WriteLine("----");
			Run (4097);
			Console.Error.WriteLine("----");
		}

		public static void Run( int len )
		{
            var file = Environment.GetEnvironmentVariable("TEST_FILE_PREFIX");

			var resource = new Uri(file + len.ToString() + ".bin");
			var t = new HttpFileStream( resource );
			var sha1 = new SHA1Managed();
			Console.Error.WriteLine( new SoapHexBinary( sha1.ComputeHash( t ) ).ToString() );
			t.Close();

		}
	}

}
