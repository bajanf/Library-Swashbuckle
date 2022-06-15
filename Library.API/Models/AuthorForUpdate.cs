using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    /// <summary>
    /// An author with FirstName and LastName fields
    /// </summary>
    public class AuthorForUpdate
    {
        /// <summary>
        /// The FirstName of the author
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string FirstName { get; set; }

        /// <summary>
        /// The LastName of the author
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string LastName { get; set; }
    }
}
