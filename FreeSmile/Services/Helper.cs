﻿using Microsoft.Extensions.Localization;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text.Json.Serialization;
using static FreeSmile.Services.MyConstants;

namespace FreeSmile.Services
{
    public class Helper
    {
        public static string GetEnvVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key)!;
        }
        public static void CheckEnvironmentVariables(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(GetEnvVariable(key)))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Fatal Error: {key} is not found in Environment Variables.");
                    Environment.Exit(1);
                }
            }
        }
        public static bool ValidPageNumber(int count, int pageRequested, int sizeRequested)
        {
            int lastPossiblePage = count / sizeRequested;
            if (count % sizeRequested != 0)
                lastPossiblePage++;

            if (pageRequested > lastPossiblePage)
            {
                return false;
            }
            return true;
        }
        public static async Task ResizeSaveImage(string path, IFormFile file)
        {
            byte[] originalImage;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                originalImage = memoryStream.ToArray();
            }

            var Ext = Path.GetExtension(file.FileName).ToLower();
            using (var image = Image.Load(originalImage))
            {
                var encoder = ExtensionToEncoder(Ext);

                while (file.Length >= MAX_IMAGE_SIZE)
                {
                    image.Mutate(x => x.Resize(image.Width * 70 / 100, 0));
                    using (var ms = new MemoryStream())
                    {
                        await image.SaveAsync(ms, encoder);
                        if (ms.Length <= MAX_IMAGE_SIZE)
                            break;
                    }
                }
                await image.SaveAsync(path, encoder);
            }
        }
        public async static Task SaveToDisk(IFormFile? file, string path)
        {
            //DO NOT REMOVE THIS CHECK
            if (file is null)
                return;
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
        }
        public static IImageEncoder ExtensionToEncoder(string ext)
        {
            ext = ext.ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" or ".jfif" or ".jpe" => new JpegEncoder(),
                ".png" => new PngEncoder(),
                ".gif" => new GifEncoder(),
                ".webp" => new WebpEncoder(),
                ".bm" or ".bmp" or ".dip" => new BmpEncoder(),
                ".ppm" or ".pbm" or ".pgm" => new PbmEncoder(),
                ".tga" or ".vda" or ".icb" or ".vst" => new TgaEncoder(),
                ".tiff" or ".tif" => new TiffEncoder(),
                _ => new JpegEncoder(),
            };
        }
        public class RegularResponse
        {
            //public int Id { get; set; }
            public string? Token { get; set; }
            public string? Error { get; set; }
            public string? Message { get; set; }
            public string? NextPage { get; set; }
            [JsonIgnore]
            public int StatusCode { get; set; }

            public static RegularResponse UnknownError(IStringLocalizer _localizer)
            {
                return new RegularResponse()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Error = _localizer["UnknownError"],
                    NextPage = Pages.same.ToString()
                };
            }
            public static RegularResponse Success(string? token = null, string? message = null, string? nextPage = "same")
            {
                return new RegularResponse()
                {
                    //Id = id,
                    Token = token,
                    Message = message,
                    NextPage = nextPage,
                    StatusCode = StatusCodes.Status200OK,
                };
            }
            public static RegularResponse BadRequestError(string? error = null, string? nextPage = "same")
            {
                return new RegularResponse()
                {
                    //Id = id,
                    Error = error,
                    NextPage = nextPage,
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }

        }
        
        public enum Pages
        {
            home, // homeAdmin, homeDentist, homePatient, homeSuperAdmin
            login,
            register,
            verifyEmail,
            verifyDentist,
            registerAdmin,
            registerDentist,
            registerPatient,
            pendingVerificationAcceptance,
            same,
            postFullPage_, // postFullPage_{post_id}  postFullPage_25 postFullPage_5154
            articleFullPage_ // articleFullPage_{article_id}  articleFullPage_25 articleFullPage_5154
        }
        public enum NotificationTemplates
        {
            Incorrect_Info = 1,
            Photos_Not_Clear,
            Missing_Photos,
            Verification_Success,
            Article_Like,
            Article_Comment,
            Reset_Password,
            Changed_Password,
            Article_Like_Others,
            Article_Comment_Others
        }

    }
}
