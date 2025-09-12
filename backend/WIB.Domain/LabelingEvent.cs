namespace WIB.Domain;

public class LabelingEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ProductId { get; set; }
    public string LabelRaw { get; set; } = string.Empty;
    public Guid? PredictedTypeId { get; set; }
    public Guid? PredictedCategoryId { get; set; }
    public Guid FinalTypeId { get; set; }
    public Guid FinalCategoryId { get; set; }
    public decimal Confidence { get; set; }
    public DateTime WhenUtc { get; set; } = DateTime.UtcNow;
}
