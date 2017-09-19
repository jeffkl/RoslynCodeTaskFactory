using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCodeTaskFactory.UnitTests
{
    public sealed class DisposableDirectory : TempDirectory, IDisposable
    {
        public DisposableDirectory(TempRoot root)
            : base(root)
        {
        }

        public void Dispose()
        {
            if (Path != null && Directory.Exists(Path))
            {
                try
                {
                    Directory.Delete(Path, recursive: true);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    public sealed class DisposableFile : TempFile, IDisposable
    {
        private const int FileDispositionInfo = 4;

        public DisposableFile(string path)
            : base(path)
        {
        }

        public DisposableFile(string prefix = null, string extension = null, string directory = null, string callerSourcePath = null, int callerLineNumber = 0)
            : base(prefix, extension, directory, callerSourcePath, callerLineNumber)
        {
        }

        public void Dispose()
        {
            if (Path != null)
            {
                try
                {
                    File.Delete(Path);
                }
                catch (UnauthorizedAccessException)
                {
                    try
                    {
                        // the file might still be memory-mapped, delete on close:
                        DeleteFileOnClose(Path);
                    }
                    catch (IOException ex)
                    {
                        throw new InvalidOperationException(string.Format(@"
The file '{0}' seems to have been opened in a way that prevents us from deleting it on close.
Is the file loaded as an assembly (e.g. via Assembly.LoadFile)?

{1}: {2}", Path, ex.GetType().Name, ex.Message), ex);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //  We should ignore this exception if we got it the second time,
                        //  the most important reason is that the file has already been
                        //  scheduled for deletion and will be deleted when all handles
                        //  are closed.
                    }
                }
            }
        }

        /// <summary>
        /// Marks given file for automatic deletion when all its handles are closed.
        /// Note that after doing this the file can't be opened again, not even by the same process.
        /// </summary>
        internal static void DeleteFileOnClose(string fullPath)
        {
            using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete | FileShare.ReadWrite, 8, FileOptions.DeleteOnClose))
            {
                PrepareDeleteOnCloseStreamForDisposal(stream);
            }
        }

        internal static void PrepareDeleteOnCloseStreamForDisposal(FileStream stream)
        {
            uint trueValue = 1;
            SetFileInformationByHandle(stream.SafeFileHandle, FileDispositionInfo, ref trueValue, sizeof(uint));
        }

        [DllImport("kernel32.dll", PreserveSig = false)]
        private static extern void SetFileInformationByHandle(SafeFileHandle handle, int fileInformationClass, ref uint fileDispositionInfoDeleteFile, int bufferSize);
    }

    public class TempDirectory
    {
        private readonly string _path;
        private readonly TempRoot _root;

        protected TempDirectory(TempRoot root)
            : this(CreateUniqueDirectory(TempRoot.Root), root)
        {
        }

        private TempDirectory(string path, TempRoot root)
        {
            Debug.Assert(path != null);
            Debug.Assert(root != null);

            _path = path;
            _root = root;
        }

        public string Path => _path;

        /// <summary>
        /// Creates a file in this directory that is a copy of the specified file.
        /// </summary>
        public TempFile CopyFile(string originalPath)
        {
            string name = System.IO.Path.GetFileName(originalPath);
            string filePath = System.IO.Path.Combine(_path, name);
            File.Copy(originalPath, filePath);
            return _root.AddFile(new DisposableFile(filePath));
        }

        /// <summary>
        /// Creates a subdirectory in this directory.
        /// </summary>
        /// <param name="name">Directory name or unrooted directory path.</param>
        public TempDirectory CreateDirectory(string name)
        {
            string dirPath = System.IO.Path.Combine(_path, name);
            Directory.CreateDirectory(dirPath);
            return new TempDirectory(dirPath, _root);
        }

        /// <summary>
        /// Creates a file in this directory.
        /// </summary>
        /// <param name="name">File name.</param>
        public TempFile CreateFile(string name)
        {
            string filePath = System.IO.Path.Combine(_path, name);
            TempRoot.CreateStream(filePath);
            return _root.AddFile(new DisposableFile(filePath));
        }

        public override string ToString()
        {
            return _path;
        }

        private static string CreateUniqueDirectory(string basePath)
        {
            while (true)
            {
                string dir = System.IO.Path.Combine(basePath, Guid.NewGuid().ToString());
                try
                {
                    Directory.CreateDirectory(dir);
                    return dir;
                }
                catch (IOException)
                {
                    // retry
                }
            }
        }
    }

    public class TempFile
    {
        private readonly string _path;

        internal TempFile(string path)
        {
            _path = path;
        }

        internal TempFile(string prefix, string extension, string directory, string callerSourcePath, int callerLineNumber)
        {
            while (true)
            {
                if (prefix == null)
                {
                    prefix = System.IO.Path.GetFileName(callerSourcePath) + "_" + callerLineNumber.ToString() + "_";
                }

                _path = System.IO.Path.Combine(directory ?? TempRoot.Root, prefix + Guid.NewGuid() + (extension ?? ".tmp"));

                try
                {
                    TempRoot.CreateStream(_path);
                    break;
                }
                catch (PathTooLongException)
                {
                    throw;
                }
                catch (DirectoryNotFoundException)
                {
                    throw;
                }
                catch (IOException)
                {
                    // retry
                }
            }
        }

        public string Path => _path;

        public TempFile CopyContentFrom(string path)
        {
            return WriteAllBytes(File.ReadAllBytes(path));
        }

        public FileStream Open(FileAccess access = FileAccess.ReadWrite) => new FileStream(_path, FileMode.Open, access);

        public string ReadAllText()
        {
            return File.ReadAllText(_path);
        }

        public override string ToString()
        {
            return _path;
        }

        public TempFile WriteAllBytes(byte[] content)
        {
            File.WriteAllBytes(_path, content);
            return this;
        }

        public TempFile WriteAllText(string content, Encoding encoding)
        {
            File.WriteAllText(_path, content, encoding);
            return this;
        }

        public TempFile WriteAllText(string content)
        {
            File.WriteAllText(_path, content);
            return this;
        }

        public async Task<TempFile> WriteAllTextAsync(string content, Encoding encoding)
        {
            using (var sw = new StreamWriter(File.Create(_path), encoding))
            {
                await sw.WriteAsync(content).ConfigureAwait(false);
            }

            return this;
        }

        public Task<TempFile> WriteAllTextAsync(string content)
        {
            return WriteAllTextAsync(content, Encoding.UTF8);
        }
    }

    public sealed class TempRoot : IDisposable
    {
        public static readonly string Root;
        private readonly List<IDisposable> _temps = new List<IDisposable>();

        static TempRoot()
        {
            Root = Path.Combine(Path.GetTempPath(), "RoslynTests");
            Directory.CreateDirectory(Root);
        }

        public DisposableFile AddFile(DisposableFile file)
        {
            _temps.Add(file);
            return file;
        }

        public TempDirectory CreateDirectory()
        {
            var dir = new DisposableDirectory(this);
            _temps.Add(dir);
            return dir;
        }

        public TempFile CreateFile(string prefix = null, string extension = null, string directory = null, [CallerFilePath] string callerSourcePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            return AddFile(new DisposableFile(prefix, extension, directory, callerSourcePath, callerLineNumber));
        }

        public void Dispose()
        {
            if (_temps == null)
            {
                return;
            }

            DisposeAll(_temps);

            _temps.Clear();
        }

        internal static void CreateStream(string fullPath)
        {
            using (new FileStream(fullPath, FileMode.CreateNew))
            {
            }
        }

        private static void DisposeAll(IEnumerable<IDisposable> temps)
        {
            foreach (var temp in temps)
            {
                try
                {
                    temp?.Dispose();
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    public abstract class TestBase : IDisposable
    {
        private TempRoot _temp;

        public TempRoot Temp => _temp ?? (_temp = new TempRoot());

        public static string GetUniqueName() => Guid.NewGuid().ToString("D");

        public virtual void Dispose() => _temp?.Dispose();
    }
}