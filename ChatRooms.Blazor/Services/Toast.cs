namespace ChatRooms.Blazor.Services;

public sealed record Toast(Guid Id, string Title, string? Message, ToastVariant Variant);
