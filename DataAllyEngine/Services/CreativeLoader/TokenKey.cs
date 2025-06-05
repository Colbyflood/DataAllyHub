namespace DataAllyEngine.Services.CreativeLoader;

public record TokenKey(int CompanyId, string PageId)
{
	public override string ToString() => $"CO{CompanyId}/P{PageId}";
	public static implicit operator string(TokenKey key) => key.ToString();
}