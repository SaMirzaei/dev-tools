using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tools.Base64.Abstractions;
using Tools.Base64.Models;

namespace Tools.Base64.Services
{
    public class Base64Service : IBase64Service
    {
        private readonly Dictionary<string, string> _mimeTypeMap = new Dictionary<string, string>()
        {
            // Images
            { "iVBORw0KGgo", "image/png" },
            { "/9j/", "image/jpeg" },
            { "R0lGODlh", "image/gif" },
            { "UklGRg", "image/webp" }, // More specific WebP signature
            { "PHN2Zw", "image/svg+xml" },
            { "SUkq", "image/tiff" },
            { "Qk0", "image/bmp" },
            // Documents
            { "JVBERi0", "application/pdf" },
            { "UEsDBBQA", "application/vnd.openxmlformats-officedocument" },
            { "UEsDBAoA", "application/zip" },
            { "0M8R4KGx", "application/vnd.ms-office" },
            // Audio/Video
            { "SUQz", "audio/mp3" },
            { "ID3", "audio/mp3" },
            { "T2dnUw", "audio/ogg" },
            { "ZmtycA", "video/mp4" },
            { "UklGRiI", "audio/wav" }, // More specific WAV signature
            // Archives
            { "504b0304", "application/zip" },
            { "526172211a0700", "application/x-rar-compressed" },
            { "1f8b08", "application/gzip" },
        };

        public Base64Response Encode(Base64EncodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Input))
                {
                    return new Base64Response
                    {
                        Success = false,
                        Error = "Input cannot be empty"
                    };
                }

                string result;

                if (request.IsFile)
                {
                    // Input is already base64 from file upload, just validate and return
                    if (!IsValidBase64(request.Input))
                    {
                        return new Base64Response
                        {
                            Success = false,
                            Error = "Invalid base64 file data"
                        };
                    }
                    result = request.Input;
                }
                else
                {
                    // Encode plain text to base64
                    var bytes = Encoding.UTF8.GetBytes(request.Input);
                    result = Convert.ToBase64String(bytes);
                }

                return new Base64Response
                {
                    Success = true,
                    Output = result,
                    FileName = request.FileName,
                    MimeType = request.MimeType,
                    FileSize = request.IsFile ? EstimateFileSize(result) : (int?)null,
                    IsTextOutput = false
                };
            }
            catch (Exception ex)
            {
                return new Base64Response
                {
                    Success = false,
                    Error = $"Encoding error: {ex.Message}"
                };
            }
        }

        public Base64Response Decode(Base64DecodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Input))
                {
                    return new Base64Response
                    {
                        Success = false,
                        Error = "Input cannot be empty"
                    };
                }

                // Clean up the input (remove whitespace, newlines)
                var cleanInput = Regex.Replace(request.Input, @"[\s\r\n]", "");

                if (!IsValidBase64(cleanInput))
                {
                    return new Base64Response
                    {
                        Success = false,
                        Error = "Invalid base64 string"
                    };
                }

                var bytes = Convert.FromBase64String(cleanInput);

                if (request.OutputAsFile)
                {
                    // Return as base64 for file download
                    var mimeType = DetectMimeType(cleanInput);
                    return new Base64Response
                    {
                        Success = true,
                        Output = cleanInput,
                        MimeType = mimeType,
                        FileSize = bytes.Length,
                        IsTextOutput = false
                    };
                }
                else
                {
                    // Try to decode as text
                    try
                    {
                        var text = Encoding.UTF8.GetString(bytes);

                        // Check if it's valid UTF-8 text
                        if (IsValidText(text))
                        {
                            return new Base64Response
                            {
                                Success = true,
                                Output = text,
                                IsTextOutput = true
                            };
                        }
                        else
                        {
                            // Not valid text, suggest file download
                            var mimeType = DetectMimeType(cleanInput);
                            return new Base64Response
                            {
                                Success = true,
                                Output = cleanInput,
                                MimeType = mimeType,
                                FileSize = bytes.Length,
                                IsTextOutput = false,
                                Error = "Content appears to be binary data. Consider downloading as file."
                            };
                        }
                    }
                    catch
                    {
                        // Decoding as UTF-8 failed, treat as binary
                        var mimeType = DetectMimeType(cleanInput);
                        return new Base64Response
                        {
                            Success = true,
                            Output = cleanInput,
                            MimeType = mimeType,
                            FileSize = bytes.Length,
                            IsTextOutput = false,
                            Error = "Content is binary data. Consider downloading as file."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Base64Response
                {
                    Success = false,
                    Error = $"Decoding error: {ex.Message}"
                };
            }
        }

        public Base64ValidateResponse Validate(Base64ValidateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Input))
                {
                    return new Base64ValidateResponse
                    {
                        IsValid = false,
                        Error = "Input cannot be empty"
                    };
                }

                var cleanInput = Regex.Replace(request.Input, @"[\s\r\n]", "");

                if (!IsValidBase64(cleanInput))
                {
                    return new Base64ValidateResponse
                    {
                        IsValid = false,
                        Error = "Invalid base64 string format"
                    };
                }

                var bytes = Convert.FromBase64String(cleanInput);
                var mimeType = DetectMimeType(cleanInput);

                return new Base64ValidateResponse
                {
                    IsValid = true,
                    DecodedSize = bytes.Length,
                    EstimatedMimeType = mimeType
                };
            }
            catch (Exception ex)
            {
                return new Base64ValidateResponse
                {
                    IsValid = false,
                    Error = ex.Message
                };
            }
        }

        private bool IsValidBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Remove whitespace
            var cleanInput = Regex.Replace(input, @"[\s\r\n]", "");

            // Check length (must be multiple of 4)
            if (cleanInput.Length % 4 != 0)
                return false;

            // Check characters (only A-Z, a-z, 0-9, +, /, =)
            if (!Regex.IsMatch(cleanInput, @"^[A-Za-z0-9+/]*={0,2}$"))
                return false;

            try
            {
                Convert.FromBase64String(cleanInput);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string DetectMimeType(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length < 8)
                return "application/octet-stream";

            var prefix = base64String.Substring(0, Math.Min(20, base64String.Length));

            // Check for longer, more specific signatures first
            var sortedSignatures = _mimeTypeMap.OrderByDescending(kvp => kvp.Key.Length);

            foreach (var kvp in sortedSignatures)
            {
                if (prefix.StartsWith(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            // Additional manual checks for RIFF-based formats (WebP, WAV)
            if (prefix.StartsWith("UklGR"))
            {
                // RIFF format - need to check more bytes to distinguish
                if (base64String.Length >= 16)
                {
                    var decoded = Convert.FromBase64String(base64String.Substring(0, 16));
                    var header = Encoding.ASCII.GetString(decoded);

                    if (header.Contains("WEBP"))
                        return "image/webp";
                    if (header.Contains("WAVE"))
                        return "audio/wav";
                }
                return "application/octet-stream";
            }

            return "application/octet-stream";
        }

        private bool IsValidText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            // Check for common control characters that indicate binary data
            foreach (char c in text)
            {
                if (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t')
                {
                    return false;
                }
            }

            return true;
        }

        private int EstimateFileSize(string base64String)
        {
            // Base64 encoding increases size by ~33%
            // So original size = (base64_length * 3) / 4
            var cleanLength = Regex.Replace(base64String, @"[\s\r\n=]", "").Length;
            return (cleanLength * 3) / 4;
        }
    }
}