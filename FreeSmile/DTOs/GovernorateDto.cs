﻿using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel;

namespace FreeSmile.DTOs
{
    public class GovernorateDto
    {
        [DisplayName(nameof(GovernorateId))]
        [ForeignKey(nameof(Governate), "gov_id", bypassZero: true)]
        public int GovernorateId { get; set; }
    }
}