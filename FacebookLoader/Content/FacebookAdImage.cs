namespace FacebookLoader.Content;

public class FacebookAdImage
{
	public string Id { get; }
	public string Name { get; }
	public string AccountId { get; }
	public List<string> Creatives { get; }
	public string Hash { get; }
	public bool IsAssociatedCreativesInAdgroups { get; }
	public string PermalinkUrl { get; }
	public string Status { get; }
	public string CreatedTime { get; }
	public string UpdatedTime { get; }
	public string Url { get; }
	public string Url128 { get; }

	public FacebookAdImage(string Id, string Name, string AccountId, List<string> Creatives,
		string Hash, bool IsAssociatedCreativesInAdgroups, string PermalinkUrl,
		string Status, string CreatedTime, string UpdatedTime, string Url, string Url128)
	{
		this.Id = Id;
		this.Name = Name;
		this.AccountId = AccountId;
		this.Creatives = Creatives;
		this.Hash = Hash;
		this.IsAssociatedCreativesInAdgroups = IsAssociatedCreativesInAdgroups;
		this.PermalinkUrl = PermalinkUrl;
		this.Status = Status;
		this.CreatedTime = CreatedTime;
		this.UpdatedTime = UpdatedTime;
		this.Url = Url;
		this.Url128 = Url128;
	}
}
