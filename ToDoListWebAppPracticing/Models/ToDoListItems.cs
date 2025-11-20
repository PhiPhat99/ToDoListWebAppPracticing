using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Specialized;

namespace ToDoListWebAppPracticing.Models
{
    public class ToDoListItems
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "กรุณาใส่คำอธิบาย/รายละเอียด")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
        public string CategoryId { get; set; } = string.Empty;
        [ValidateNever]
        public Category Category { get; set; } = null!;
        
        [Required(ErrorMessage = "กรุณาเลือกสถานะ")]
        public string StatusId { get; set; } = string.Empty;
        [ValidateNever]
        public Status Status { get; set; } = null!;
        
        public DateTime? Duedate { get; set; }
        public bool OverDue => StatusId == "Open" && Duedate < DateTime.Today;


    }
}
