using DG.OneDrive.Serialized.Resources;
using System;
using System.Runtime.Serialization;

namespace DG.OneDrive.Exceptions
{
    /// <summary>
    /// Represents errors that occur while creating an upload session.
    /// </summary>
    [Serializable]
    public sealed class OneDriveUploadSessionException : Exception
    {
        /// <summary>
        /// The path of the file for which the upload session was being created.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The name (with extension) of the file for which the upload session was being created.
        /// </summary>
        public string NameWithExtension { get; set; }

        /// <inheritdoc/>
        public override string Message => $"Error occured while creating an uploading session for file {Path?.TrimEnd('/')}/{NameWithExtension}.";

        internal OneDriveUploadSessionException(UploadMetaData data) : base()
        {
            Path = data?.Path;
            NameWithExtension = data?.NameWithExtension;
        }

        private OneDriveUploadSessionException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        {
            Path = info.GetString(nameof(Path));
            NameWithExtension = info.GetString(nameof(NameWithExtension));
        }
    }
}
