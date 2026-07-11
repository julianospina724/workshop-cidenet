namespace Api.Contracts.Permissions;

public record PermissionMatrixResponse(IReadOnlyList<PermissionEntryResponse> Entries);
