using System;
using System.ComponentModel.DataAnnotations;

namespace CsvReader.Models
{
    public class Transaction
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Account { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Currency Code")]
        public string CurrencyCode  { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime DateUploaded { get; set; }
    }
}