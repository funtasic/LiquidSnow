﻿using System;
using System.IO;
using System.Linq;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Threading;
using Thismaker.Core.Utilities;
using Thismaker.Horus.Symmetrics;
using System.Text;

namespace Thismaker.Horus.IO
{
    /// <summary>
    /// A package that allows for manipulation and use of several <see cref="CryptoFile"/>s
    /// Useful where several files need to be encrypted under the same roof using the same key
    /// </summary>
    public class CryptoContainer : IDisposable
    {
        #region Private Fields

        private readonly Package _package;

        private readonly Stream _source;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new container with the provided key, 
        /// with the underlying stream being a <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="key"></param>
        public CryptoContainer(string key)
        {
            Key = key;
            _source = new MemoryStream();
            _package = Package.Open(_source, FileMode.OpenOrCreate);
        }

        /// <summary>
        /// Creates a new container with the provided key and stream.
        /// </summary>
        /// <param name="stream">The stream with the container data 
        /// and/or where the date will be saved</param>
        /// <param name="key"></param>
        public CryptoContainer(Stream stream, string key)
        {
            Key = key;
            _source = stream;
            _package = Package.Open(stream, FileMode.OpenOrCreate);
        }

        #endregion

        #region Properties
        /// <summary>
        /// The key/Password used in encrypting, decrypting contents of the container.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The symmetric algorithm to use, if not provided,
        /// the process will use <see cref="PredefinedSymmetric.AesFixed"/>
        /// </summary>
        public Symmetric Alogrithm { get; set; }
        = PredefinedSymmetric.AesFixed;
        #endregion

        #region Stream IO
        /// <summary>
        /// Adds an item to the container, overwriting it if requested and it exists
        /// </summary>
        /// <param name="filename">The name of the file to add to the container.</param>
        /// <param name="input">The source of the data to add to the stream</param>
        /// <param name="overwrite">If true, overwrites the file, 
        /// otherwise a <see cref="UnauthorizedAccessException"/> will be thrown if the part exists
        /// and is false</param>
        /// <param name="progress">If provided, reports on the progress of the operation</param>
        /// <param name="cancellationToken">If provided, can be used to cancel the operation</param>
        /// <returns></returns>
        public async Task AddAsync(string filename, Stream input, bool overwrite=false, IProgress<float> progress=null, CancellationToken cancellationToken=default)
        {
            try
            {
                var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));

                PackagePart part;
                if (_package.PartExists(uri))
                {
                    if (overwrite)
                    {
                        part = _package.GetPart(uri);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Part already exists");
                    }
                }
                else
                {
                    part = _package.CreatePart(uri, "");
                }
                var output = part.GetStream();
                using var ef = new CryptoFile(output, Key);
                ef.Algorithm = Alogrithm;
                await ef.WriteAsync(input, progress, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Copies the unencrypted contents of a file to the provided stream
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="output">The destination of the unencrypted data</param>
        /// <param name="progress">If provided, reports on the progress of the operation</param>
        /// <param name="cancellationToken">If provided, can be used to cancel the operation</param>
        /// <returns></returns>
        public async Task GetAsync(string filename, Stream output, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));

                if (!_package.PartExists(uri))
                {
                    throw new FileNotFoundException("The specified item does not exist");
                }

                var input = _package.GetPart(uri).GetStream();
                using var ef = new CryptoFile(input, Key);
                ef.Algorithm = Alogrithm;
                await ef.ReadAsync(output, progress, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Object IO
        /// <summary>
        /// Adds a <see cref="object"/> to the container by means of XML serialization.
        /// Therefore the object, and the data you wish to persist, must be XML serializable
        /// </summary>
        /// <param name="filename">The name to give the file that will host the object</param>
        /// <param name="o">The object to serailzie</param>
        /// <param name="overwrite">If true, any previous existing records will be overwritten</param>
        public async Task AddAsync(string filename, object o, bool overwrite = false)
        {
            try
            {
                var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));

                PackagePart part;
                if (_package.PartExists(uri))
                {
                    if (overwrite)
                    {
                        part = _package.GetPart(uri);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Part already exists");
                    }
                }
                else
                {
                    part = _package.CreatePart(uri, "");
                }
                var output = part.GetStream();
                using var ef = new CryptoFile(output, Key);
                ef.Algorithm = Alogrithm;
                await ef.SerializeAsync(o).ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Can be used to quickly obtain an object that was saved into the container.
        /// </summary>
        /// <typeparam name="T">The type of object</typeparam>
        /// <param name="filename">The name of the file to get</param>
        /// <returns>A deserialized copy of the object</returns>
        public async Task<T> GetAsync<T>(string filename)
        {
            try
            {
                var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));

                if (!_package.PartExists(uri))
                {
                    throw new FileNotFoundException("The specified item does not exist");
                }

                var input = _package.GetPart(uri).GetStream();
                using var ef = new CryptoFile(input, Key);
                ef.Algorithm = Alogrithm;
                return await ef.DeserializeAsync<T>().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Byte IO
        /// <summary>
        /// Adds byte content to the container under the specified name
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="data">The byte array data to add to the file</param>
        /// <param name="overwrite">If true, overwrites an exisitng file, otherwise throws an exception</param>
        /// <param name="progress">Reports the progress of the operation</param>
        /// <param name="cancellationToken">Allows cancellation fo the operation</param>
        /// <returns></returns>
        public async Task AddBytesAsync(string filename, byte[] data, bool overwrite = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            using var msData = new MemoryStream(data);
            await AddAsync(filename, msData, overwrite, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the byte content of a file in the container
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="progress">If provided, reports on the progress of the operation</param>
        /// <param name="cancellationToken">Allows cancelling the operation</param>
        /// <returns></returns>
        public async Task<byte[]> GetBytesAsync(string filename, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            using var msData = new MemoryStream();
            await GetAsync(filename, msData, progress, cancellationToken).ConfigureAwait(false);
            return msData.ToArray();
        }
        #endregion

        #region String IO
        /// <summary>
        /// Adds text contents to the container that will be accessable through the provided filename
        /// </summary>
        /// <param name="filename">The name of the file in the cryptocontainer</param>
        /// <param name="contents">The string contents of the file</param>
        /// <param name="overwrite">If true, overwrites an exisitng file, otherwise, an exception is thrown if the file already exists</param>
        /// <param name="progress">If provided, reports the progress of the process</param>
        /// <param name="cancellationToken">If provided, alows the canceling of the task</param>
        public async Task AddTextAsync(string filename, string contents, bool overwrite = false, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            await AddBytesAsync(filename, contents.GetBytes<UTF8Encoding>(), overwrite, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the contents of a file as a UTF8 string
        /// </summary>
        /// <param name="filename">The name of the file</param>
        /// <param name="progress">Reports the progress if provided</param>
        /// <param name="cancellationToken">Cancels the task when called</param>
        public async Task<string> GetTextAsync(string filename, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            var bytes = await GetBytesAsync(filename, progress, cancellationToken).ConfigureAwait(false);
            return bytes.GetString<UTF8Encoding>();
        }
        #endregion

        #region Misc Methods
        /// <summary>
        /// Extracts the contents of the container to the provided directory
        /// </summary>
        /// <param name="outputDirecotry"></param>
        /// <param name="progress">If provided, can be used to report progress of operation</param>
        /// <param name="cancellationToken">Can be used to cancel the operation</param>
        /// <returns></returns>
        public async Task ExtractContainerAsync(string outputDirecotry, IProgress<float> progress=null, CancellationToken cancellationToken=default)
        {
            if (!Directory.Exists(outputDirecotry))
            {
                Directory.CreateDirectory(outputDirecotry);
            }
            var parts = _package.GetParts();

            int index = 0;
            int count = parts.Count();
                
            var innerProgress = new Progress<float>{};
            innerProgress.ProgressChanged += (o, e) => 
            {
                var percent = (index + e) / count;
                progress.Report(percent);
            };

            if (progress == null)
                innerProgress = null;

            foreach(var part in parts)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                var name = part.Uri.ToString();
                var path=IOUtility.CombinePath(outputDirecotry, name);
                using var fs = File.OpenWrite(path);
                await GetAsync(name, fs, innerProgress);
                index++;
            }
        }

        /// <summary>
        /// Determines if a file exists in the container
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool Exists(string filename)
        {
            var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));
            return _package.PartExists(uri);
        }

        /// <summary>
        /// Deletes the specified file, returning true if it was successful
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>True if the file was deleted</returns>
        public bool Delete(string filename)
        {
            var uri = PackUriHelper.CreatePartUri(new Uri(filename, UriKind.Relative));
            if (_package.PartExists(uri))
            {
                try
                {
                    _package.DeletePart(uri);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// Writes any pending data to the underlying stream, disposing of it therafter.
        /// </summary>
        public void Dispose()
        {
            _package.Close();
            _source.Dispose();
        }

        #endregion
    }
}
