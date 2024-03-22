using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    // names in models do not have to follow nay conventions
    // models represent DB
    public class Category
    {
        // primary key
        // however .net deals with any prop that has "Id" as a key by default
        // so [Key] is not required here
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        // server side validation
        // maximum string length of 30 chars
        [MaxLength(30)]
        public string? Name { get; set; }

        [DisplayName("Display Order")]
        // display order number is from 1 to 100 only
        [Range(1, 100, ErrorMessage = "Hey DO must be between 1 and 100")]
        public int DisplayOrder { get; set; }
    }
}

// c# properties like: public int Id { get; set; } are something that are between fields and methods
// generally used for getters and setters. Here is an example:
/*
    public class Car{
        private int speed;
        private int Speed
        {
            get
            {
                return speed;
            }
            set
            {
                if (value > 500)
                {
                    speed = 500;
                }
                else{
                    speed = value
                }
            }
        }

    }
*/
