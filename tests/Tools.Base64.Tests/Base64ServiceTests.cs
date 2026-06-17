using Microsoft.Extensions.DependencyInjection;
using Tools.Base64.Abstractions;
using Tools.Base64.Models;
using Tools.Base64.Services;

namespace Tools.Base64.Tests
{
    public class Base64ServiceTests
    {
        private readonly IBase64Service _service = new Base64Service();

        [Fact]
        public void Encode_TextInput_ReturnsExpectedBase64()
        {
            var resp = _service.Encode(new Base64EncodeRequest
            {
                Input = "Hello, Base64!"
            });

            Assert.True(resp.Success);
            Assert.Equal("SGVsbG8sIEJhc2U2NCE=", resp.Output);
            Assert.Null(resp.Error);
            Assert.Null(resp.FileSize);
            Assert.False(resp.IsTextOutput);
        }

        [Fact]
        public void Encode_EmptyInput_ReturnsError()
        {
            var resp = _service.Encode(new Base64EncodeRequest
            {
                Input = string.Empty
            });

            Assert.False(resp.Success);
            Assert.Equal("Input cannot be empty", resp.Error);
        }

        [Fact]
        public void Encode_FileInput_ValidBase64_PreservesContentAndMetadata()
        {
            var resp = _service.Encode(new Base64EncodeRequest
            {
                Input = "SGVsbG8=",
                IsFile = true,
                FileName = "hello.txt",
                MimeType = "text/plain"
            });

            Assert.True(resp.Success);
            Assert.Equal("SGVsbG8=", resp.Output);
            Assert.Equal("hello.txt", resp.FileName);
            Assert.Equal("text/plain", resp.MimeType);
            Assert.Equal(5, resp.FileSize);
            Assert.False(resp.IsTextOutput);
        }

        [Fact]
        public void Encode_FileInput_InvalidBase64_ReturnsError()
        {
            var resp = _service.Encode(new Base64EncodeRequest
            {
                Input = "not base64",
                IsFile = true
            });

            Assert.False(resp.Success);
            Assert.Equal("Invalid base64 file data", resp.Error);
        }

        [Fact]
        public void Decode_EmptyInput_ReturnsError()
        {
            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = string.Empty
            });

            Assert.False(resp.Success);
            Assert.Equal("Input cannot be empty", resp.Error);
        }

        [Fact]
        public void Decode_InvalidBase64_ReturnsError()
        {
            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = "%%%"
            });

            Assert.False(resp.Success);
            Assert.Equal("Invalid base64 string", resp.Error);
        }

        [Fact]
        public void Decode_TextOutput_ReturnsUtf8Text()
        {
            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = "SGVsbG8sIEJhc2U2NCE="
            });

            Assert.True(resp.Success);
            Assert.True(resp.IsTextOutput);
            Assert.Equal("Hello, Base64!", resp.Output);
            Assert.Null(resp.Error);
        }

        [Fact]
        public void Decode_IgnoresWhitespaceAndNewlines()
        {
            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = "SGVs\r\nbG8s IEJh c2U2NCE="
            });

            Assert.True(resp.Success);
            Assert.True(resp.IsTextOutput);
            Assert.Equal("Hello, Base64!", resp.Output);
        }

        [Fact]
        public void Decode_OutputAsFile_ReturnsMetadataAndDetectedMimeType()
        {
            const string pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO5WZr0AAAAASUVORK5CYII=";

            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = pngBase64,
                OutputAsFile = true
            });

            Assert.True(resp.Success);
            Assert.False(resp.IsTextOutput);
            Assert.Equal(pngBase64, resp.Output);
            Assert.Equal("image/png", resp.MimeType);
            Assert.True(resp.FileSize > 0);
            Assert.Null(resp.Error);
        }

        [Fact]
        public void Decode_BinaryContent_ReturnsHintInsteadOfText()
        {
            var resp = _service.Decode(new Base64DecodeRequest
            {
                Input = "AA=="
            });

            Assert.True(resp.Success);
            Assert.False(resp.IsTextOutput);
            Assert.Equal("AA==", resp.Output);
            Assert.Equal("application/octet-stream", resp.MimeType);
            Assert.Equal(1, resp.FileSize);
            Assert.Equal("Content appears to be binary data. Consider downloading as file.", resp.Error);
        }

        [Fact]
        public void Validate_EmptyInput_ReturnsError()
        {
            var resp = _service.Validate(new Base64ValidateRequest
            {
                Input = string.Empty
            });

            Assert.False(resp.IsValid);
            Assert.Equal("Input cannot be empty", resp.Error);
        }

        [Fact]
        public void Validate_InvalidBase64_ReturnsError()
        {
            var resp = _service.Validate(new Base64ValidateRequest
            {
                Input = "invalid"
            });

            Assert.False(resp.IsValid);
            Assert.Equal("Invalid base64 string format", resp.Error);
        }

        [Fact]
        public void Validate_ValidBase64_ReturnsDecodedSizeAndMimeType()
        {
            const string pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO5WZr0AAAAASUVORK5CYII=";

            var resp = _service.Validate(new Base64ValidateRequest
            {
                Input = pngBase64
            });

            Assert.True(resp.IsValid);
            Assert.Equal("image/png", resp.EstimatedMimeType);
            Assert.True(resp.DecodedSize > 0);
            Assert.Null(resp.Error);
        }

        [Fact]
        public void Validate_IgnoresWhitespaceAndNewlines()
        {
            var resp = _service.Validate(new Base64ValidateRequest
            {
                Input = "SGVs\r\nbG8="
            });

            Assert.True(resp.IsValid);
            Assert.Equal(5, resp.DecodedSize);
            Assert.Equal("application/octet-stream", resp.EstimatedMimeType);
        }

        [Fact]
        public void AddBase64Tools_RegistersService()
        {
            var services = new ServiceCollection();

            services.AddBase64Tools();

            var descriptor = Assert.Single(services, service => service.ServiceType == typeof(IBase64Service));
            Assert.Equal(typeof(Base64Service), descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }
}