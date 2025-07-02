using Rafeeq.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int? BookingId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? AmountPaid { get; set; }

    [StringLength(50)]
    public string PaymentMethod { get; set; }

    [StringLength(255)]
    public string TransactionId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaymentDate { get; set; }

  

    [ForeignKey("BookingId")]
    [InverseProperty("Payments")]
    public virtual Booking Booking { get; set; }
}
