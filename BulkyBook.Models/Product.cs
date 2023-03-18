using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulkyBook.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1,10000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }   

        [Required]
        [Range(1,10000)]
        public double Price { get; set; }

        [Required]
        [Range(1, 10000)]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 10000)]
        public double Price100 { get; set; }

        [Display(Name ="Image Url")]
        [ValidateNever]
        public string ImageUrl { get; set; }

        //for foreign key (it will automatically generate foreign key because here both the names are same but if name is different then we can do it using [ForeignKey()] annotation )
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]   // optional
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        public int CoverTypeId { get; set; }
        [ValidateNever]
        public CoverType CoverType { get; set; }
    }
}
