using DTOs;

namespace FreeSmile.DTOs.Admins
{
    public class GetVerificationRequestDto : GetBasicUserInfo
    {
        public string RequestedUniversity { get; set; } = null!;
        public string RequestedDegree { get; set; } = null!;
        public List<byte[]> Images { get; set; } = null!;
        public bool IsLast { get; set; }

    }
}