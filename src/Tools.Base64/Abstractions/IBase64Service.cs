using Tools.Base64.Models;

namespace Tools.Base64.Abstractions
{
    public interface IBase64Service
    {
        Base64Response Encode(Base64EncodeRequest request);
        Base64Response Decode(Base64DecodeRequest request);
        Base64ValidateResponse Validate(Base64ValidateRequest request);
    }
}
