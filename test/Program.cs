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
			Run ();
		}

		public static void Run(  )
		{
            var file = Environment.GetEnvironmentVariable("TEST_FILE_PREFIX");

            if ( file == null ) {
                Console.Error.WriteLine("you need to set TEST_FILE_PREFIX");
                return;
            }
            var resource = new Uri(file);
            var start = DateTime.Now;
			var t = new HttpFileStream( resource );
            t.AdjustHTTPReadSize(16 * 1024 * 1024 ); // make range requests this big rather than the size of reads.
			
            if ( Environment.GetEnvironmentVariable("TEST_COMPUTE_SHA1") != null ){
                var sha1 = new SHA1Managed();
                Console.Error.WriteLine( new SoapHexBinary( sha1.ComputeHash( t ) ).ToString() );
            } else {
                // just transfer data
                var tmp = new byte[1024*1024];
                int rc = 0;
                do { 
                    rc = t.Read( tmp, 0, tmp.Length );
                    var rdur = DateTime.Now.Subtract(start).TotalSeconds;
                    var rrate = (int)(t.Position / rdur);
                    Console.Error.WriteLine("\r{0} bytes transferred at {1} KBytes/s      ", t.Length, rrate / 1024 );
                } while ( rc > 0 );
            }

			t.Close();
            var dur = DateTime.Now.Subtract(start).TotalSeconds;
            var rate = (int)(t.Length / dur);
            Console.Error.WriteLine("{0} bytes transferred at {1} KBytes/s", t.Length, rate / 1024 );
		}
	}

}
