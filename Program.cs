using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryComparer
{
    class Program
    {
        // http://stackoverflow.com/a/7931353/25868
        // This method accepts two strings the represent two files to 
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the 
        // files are not the same.
        private static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }    
        public static IEnumerable<string> AllFiles(string directoryPath)
        {
            foreach (var file in Directory.GetFiles(directoryPath, "*.*"))
            {
                yield return file;
            }

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                foreach (var file in AllFiles(dir))
                {
                    yield return file;
                }
            }
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: DirectoryComparer <dir1> <dir2>");
                return 1;
            }

            var leftDir = args[0];
            var rightDir = args[1];

            if (!Directory.Exists(leftDir))
            {
                Console.WriteLine("Left directory doesn't exist: " + leftDir);
                return 1;
            }

            if (!Directory.Exists(rightDir))
            {
                Console.WriteLine("Right directory doesn't exist: " + rightDir);
                return 1;
            }

            Console.WriteLine("Comparing directories:");
            Console.WriteLine("    " + leftDir);
            Console.WriteLine("    " + rightDir);

            var leftOnlyFiles = new List<string>();
            var differentFiles = new List<string>();
            var rightOnlyFiles = new List<string>();

            var leftFiles = 0;

            foreach (var leftPath in AllFiles(leftDir).AsParallel())
            {
                ++leftFiles;

                var relativePath = leftPath.Substring(leftDir.Length+1);
                var rightPath = Path.Combine(rightDir, relativePath);

                if (!File.Exists(rightPath))
                {
                    leftOnlyFiles.Add(relativePath);
                }
                else
                {
                    if (!FileCompare(leftPath, rightPath))
                    {
                        differentFiles.Add(relativePath);
                    }                    
                }
            }

            var rightFiles = 0;

            foreach (var rightPath in AllFiles(rightDir).AsParallel())
            {
                ++rightFiles;

                var relativePath = rightPath.Substring(rightDir.Length + 1);
                var leftPath = Path.Combine(leftDir, relativePath);

                if (!File.Exists(leftPath))
                {
                    rightOnlyFiles.Add(relativePath);
                }
            }

            Console.WriteLine("== Summary == ");
            Console.WriteLine("Total left files: " + leftFiles);
            Console.WriteLine("Total right files: " + rightFiles);
            Console.WriteLine("Left only: " + leftOnlyFiles.Count);
            Console.WriteLine("Different: " + differentFiles.Count);
            Console.WriteLine("Right only: " + rightOnlyFiles.Count);

            Console.WriteLine("== Left only == ");

            foreach (var file in leftOnlyFiles)
            {
                Console.WriteLine(file);
            }

            Console.WriteLine("== Different == ");

            foreach (var file in differentFiles)
            {
                Console.WriteLine("    " + file);
            }

            Console.WriteLine("== Right only == ");

            foreach (var file in rightOnlyFiles)
            {
                Console.WriteLine(file);
            }

            return 0;
        }
    }
}
