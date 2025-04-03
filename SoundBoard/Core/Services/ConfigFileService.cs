using System.Collections.Concurrent;
using SoundBoard.Models.UI;

namespace SoundBoard.Core.Services;

public class ConfigFileService(string workingPath)
{
    private readonly ConcurrentDictionary<string, SoundItemConfig> _soundItems = [];
    
    public IReadOnlyDictionary<string, SoundItemConfig> ReadConfig => _soundItems;

    public SoundItemConfig GetOrDefault(string name, Func<string, SoundItemConfig> factory)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        // Try to get the sound item from the dictionary
        if (_soundItems.TryGetValue(name, out var soundItem))
            return soundItem;

        // If not found, create a new one using the factory function
        soundItem = factory(name);
        _soundItems.TryAdd(name, soundItem);
        return soundItem;
    }

    public void UpdateSoundItem(string name, SoundItemConfig soundItem)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (soundItem is null)
            throw new ArgumentNullException(nameof(soundItem));

        // Update the sound item in the dictionary
        _soundItems.AddOrUpdate(name, soundItem, (key, oldValue) => soundItem);
        
        // Save the updated sound items to the file
        Save();
    }
    
    public void Load()
    {
        if (!System.IO.File.Exists(workingPath))
            System.IO.File.WriteAllBytes(workingPath, []);
        
        // Deserialize the byte array into a list of SoundItemConfig objects
        var fileData = System.IO.File.ReadAllBytes(workingPath);
        if (fileData.Length == 0)
            return;
        
        var soundItems = System.Text.Json.JsonSerializer.Deserialize<SoundItemConfig[]>(fileData);
        if (soundItems is null)
            throw new InvalidOperationException("Failed to deserialize sound items from file data.");

        // Clear the existing sound items and add the new ones
        _soundItems.Clear();
        foreach (var item in soundItems)
            _soundItems.TryAdd(item.Name, item);  
    }
    
    public void Save()
    {
        // Serialize the sound items to a byte array
        var soundItems = _soundItems.Values.ToArray();
        var fileData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(soundItems);

        // Write the byte array to the specified file path
        System.IO.File.WriteAllBytes(workingPath, fileData);
    }
}