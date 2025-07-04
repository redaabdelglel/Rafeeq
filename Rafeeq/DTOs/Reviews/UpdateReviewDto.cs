using System.ComponentModel.DataAnnotations;

public class UpdateReviewDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(500)]
    public string Comment { get; set; }
}