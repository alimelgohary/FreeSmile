using FreeSmile.CustomValidations;
using FreeSmile.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FreeSmile.DTOs
{
    public class UserId
    {
        [DisplayName(nameof(Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(User), "id")]
        [Range(1, int.MaxValue)]
        public int? Id { get; set; }
    }
    public class DentistId
    {
        [DisplayName(nameof(Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(Dentist), "dentist_id")]
        [Range(1, int.MaxValue)]
        public int? Id { get; set; }
    }
    public class PatientId
    {
        [DisplayName(nameof(Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(Patient), "patient_id")]
        [Range(1, int.MaxValue)]
        public int? Id { get; set; }
    }
    public class AdminId
    {
        [DisplayName(nameof(Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(Admin), "admin_id")]
        [Range(1, int.MaxValue)]
        public int? Id { get; set; }
    }
    public class SuperAdminId
    {
        [DisplayName(nameof(Id))]
        [Required(ErrorMessage = "required")]
        [ForeignKey(nameof(SuperAdmin), "super_admin_id")]
        [Range(1, int.MaxValue)]
        public int? Id { get; set; }
    }
}