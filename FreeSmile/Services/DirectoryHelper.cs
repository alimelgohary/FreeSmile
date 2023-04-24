namespace FreeSmile.Services
{
    public class DirectoryHelper
    {
        public const string IMAGES_PATH = "Images";

        //"Images\profilePics\{id[0]}\{id}\{size}"
        public static string GetProfilePicturesUser(int id) => Path.Combine(IMAGES_PATH, "profilePics", id.ToString().First().ToString(), $"{id}");
        public static string GetProfilePicturesPath(int id, byte size) => Path.Combine(GetProfilePicturesUser(id), $"{size}");

        //"Images\verificationRequests\{id[0]}\{id}\{1|2}"
        public static string GetVerificationPathUser(int id) => Path.Combine(IMAGES_PATH, "verificationRequests", id.ToString().First().ToString(), $"{id}");
        public static string GetVerificationImgPath(int id, VerificationType type) => Path.Combine(GetVerificationPathUser(id), $"{(int)type}");
        public static string GetPostsPathPost(int id) => Path.Combine(IMAGES_PATH, "posts", id.ToString().First().ToString(), $"{id}");
        public static string GetPostsPathImg(int id, int imgNum) => Path.Combine(GetPostsPathPost(id), $"{imgNum}");
        public enum VerificationType
        {
            Nat = 1,
            Proof = 2
        }
    }
}
