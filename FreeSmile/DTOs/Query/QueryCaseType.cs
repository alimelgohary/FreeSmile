using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FreeSmile.DTOs.Query
{
    public class QueryCaseType
    {
        [DisplayName(nameof(CaseTypeId))]
        [ForeignKey(nameof(CaseType), "case_type_id", bypassZero: true)]
        public int CaseTypeId { get; set; }
    }
}