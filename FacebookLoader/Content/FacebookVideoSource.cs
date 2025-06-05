namespace FacebookLoader.Content;

public record FacebookVideoSource(string Id, string Source)
{
	public string Url => Source;
}
