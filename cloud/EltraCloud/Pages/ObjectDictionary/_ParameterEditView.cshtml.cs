using System;
using System.ComponentModel.DataAnnotations;
using EltraCloud.Pages.ObjectDictionary.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable CS1591

namespace EltraCloud.Pages.ObjectDictionary
{
    public class ParameterEditViewModel : PageModel
    {
        [Required]
        public ParameterModel Parameter { get; set; }

        public void OnGet()
        {
        }

        public void OnPostAsync(ParameterEditViewModel model)
        {
            Console.WriteLine(model.Parameter.Value);
        }
    }
}