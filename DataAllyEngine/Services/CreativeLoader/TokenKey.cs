namespace DataAllyEngine.Services.CreativeLoader;

public record TokenKey(int CompanyId, int ChannelId, string PageId)
{
	public override string ToString() => $"CO{CompanyId}/CH{ChannelId}/P{PageId}";
	public static implicit operator string(TokenKey key) => key.ToString();
}