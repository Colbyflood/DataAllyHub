namespace DataAllyEngine.Configuration;

public interface IConfigurationLoader
{
	string GetKeyValueFor(string elementName);
}