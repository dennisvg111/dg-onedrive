using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DG.OneDrive.Serialized.Resources
{
    /// <summary>
    /// Represents an upload session, which allows users to upload a file in multiple parts.
    /// </summary>
    public class UploadSession
    {
        [JsonProperty("uploadUrl")]
        private readonly Uri _uploadUri;

        [JsonProperty("expirationDateTime")]
        private readonly DateTimeOffset _expiration;

        [JsonProperty("nextExpectedRanges")]
        private readonly string[] _nextExpectedRanges;

        /// <summary>
        /// The URL endpoint that accepts <c>PUT</c> requests for byte ranges of the file.
        /// </summary>
        public Uri UploadUri => _uploadUri;

        /// <summary>
        /// The date and time in UTC that the upload session will expire. The complete file must be uploaded before this expiration time is reached.
        /// </summary>
        public DateTimeOffset Expiration => _expiration;

        /// <summary>
        /// A collection of byte ranges that the server is missing for the file.
        /// These ranges are zero indexed and of the format "start-end" (e.g. "0-26" to indicate the first 27 bytes of the file). 
        /// </summary>
        public IReadOnlyList<string> NextExpectedRanges => _nextExpectedRanges;
    }
}
