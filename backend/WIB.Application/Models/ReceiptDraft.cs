namespace WIB.Application.Models;

public record ReceiptDraft(
    string? Store,
    DateTime Date,
    string Currency,
    IReadOnlyCollection<ReceiptLineDraft> Lines,
    decimal Total,
    decimal? TaxTotal);

public record ReceiptLineDraft(
    string Label,
    decimal Qty,
    decimal UnitPrice,
    decimal LineTotal,
    decimal? VatRate,
    string? Brand);
