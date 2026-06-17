using System.ComponentModel.DataAnnotations;

namespace Tools.Base64.Models
{
    public class Base64EncodeRequest
    {
        [Required]
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the input is a file (base64 string) or plain text
        /// </summary>
        public bool IsFile { get; set; } = false;

        /// <summary>
        /// Original filename when encoding a file
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// MIME type when encoding a file
        /// </summary>
        public string? MimeType { get; set; }
    }

    public class Base64DecodeRequest
    {
        [Required]
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the output should be treated as a file
        /// </summary>
        public bool OutputAsFile { get; set; } = false;
    }

    public class Base64Response
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public int? FileSize { get; set; }
        public bool IsTextOutput { get; set; } = true;
    }

    public class Base64ValidateRequest
    {
        [Required]
        public string Input { get; set; } = string.Empty;
    }

    public class Base64ValidateResponse
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public int? DecodedSize { get; set; }
        public string? EstimatedMimeType { get; set; }
    }
}
