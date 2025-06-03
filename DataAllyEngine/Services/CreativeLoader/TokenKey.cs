namespace DataAllyEngine.Services.CreativeLoader;

public record TokenKey(int CompanyId, int ChannelId)
{
	public override string ToString() => $"{CompanyId}/{ChannelId}";
	public static implicit operator string(TokenKey key) => key.ToString();
}