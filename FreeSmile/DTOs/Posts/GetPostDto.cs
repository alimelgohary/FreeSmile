﻿using DTOs;

namespace FreeSmile.DTOs.Posts
{
    public class GetPostDto
    {
        public GetBasicUserInfo UserInfo { get; set; } = null!;
        public GetBasicDentistInfo? DentistInfo { get; set; } = null;
        public int PostId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string TimeWritten { get; set; } = null!;
        public string? TimeUpdated { get; set; } = null;
        public List<byte[]>? Images { get; set; } = null;
        public string? Phone { get; set; } = null;
        public bool IsOwner { get; set; }
        public DateTime Written { get; set; }
        public DateTime? Updated { get; set; }
        public string? Governorate { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsCase { get; set; }
        public bool IsListing { get; set; }
        public bool IsArticle { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
    }
}