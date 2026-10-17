
namespace AiOrderAgent.Services;

public class DocumentSecurityFilter
{
    public List<DocumentChunk> FilterByTenant(
        IEnumerable<DocumentChunk> documents,
        string authenticatedTenantId)
    {
        if (string.IsNullOrWhiteSpace(authenticatedTenantId))
        {
            throw new UnauthorizedAccessException(
                "Tenant identity is required.");
        }

        // Chỉ lọc các tài liệu thuộc tenant đã xác thực.
        // Tenant ID phải được lấy từ server-side identity.
        return documents
            .Where(d => d.TenantId == authenticatedTenantId)
            .ToList();
    }
}
