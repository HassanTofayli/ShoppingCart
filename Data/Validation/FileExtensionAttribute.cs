﻿using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Data.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Console.WriteLine("I am In FileExtentionAttribute Class");
            Console.WriteLine(value is IFormFile mfile);
            Console.WriteLine("here is the test " + value?.GetType().FullName?.ToString());
            if (value is IFormFile file)
            {
                Console.WriteLine("I am In FileExtentionAttribute Class in If");
                Console.WriteLine(file.FileName + " is my file name");

                var extension = Path.GetExtension(file.FileName);

                string[] extensions = { "jpg", "png" };
                bool result = extensions.Any(x => extension.EndsWith(x));

                if (!result)
                {
                    return new ValidationResult("Allowed extensions are jpg and png");
                }
            }

            return ValidationResult.Success;
        }
    }
}
