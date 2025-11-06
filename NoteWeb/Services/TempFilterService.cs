using System.Text.Json;
using CodeLab.Share.Contrib.StopWords;

namespace NoteWeb.Services;

public class TempFilterService {
    private readonly StopWordsToolkit _toolkit;

    public StopWordsToolkit Toolkit => _toolkit;

    public TempFilterService() {
        var words = JsonSerializer.Deserialize<IEnumerable<Word>>(File.ReadAllText("words.json"));
        _toolkit = new StopWordsToolkit(words!.Select(a => a.Value));
    }

    public bool CheckBadWord(string word) {
        return _toolkit.CheckBadWord(word);
    }
}