namespace Ptr.Shared.Misc;

public class ChatMessageFormatter
{
    private string _prefix = string.Empty;
    private bool _shouldApplyPrefix = true;

    public void SetPrefix(string prefix)
    {
        _prefix = prefix;
    }

    public string Format(string message)
    {
        var formatted = _shouldApplyPrefix && !string.IsNullOrEmpty(_prefix)
            ? $" {_prefix} {message}"
            : message;

        return formatted.ProcessColorString();
    }

    public void ShouldApplyPrefix(bool shouldApply)
    {
        _shouldApplyPrefix = shouldApply;
    }
}

internal static class ChatMessageFormatterExtensions
{
    // Dictionary for color code replacements - using only the colors from ChatColor class
    private static readonly Dictionary<string, string> ColorMap = new()
    {
        { "{normal}", "\x01" },
        { "{default}", "\x01" },
        { "{white}", "\x01" },
        { "{darkred}", "\x02" },
        { "{pink}", "\x03" },
        { "{green}", "\x04" },
        { "{lightgreen}", "\x05" },
        { "{lime}", "\x06" },
        { "{red}", "\x07" },
        { "{grey}", "\x08" },
        { "{gray}", "\x08" },
        { "{yellow}", "\x09" },
        { "{gold}", "\x10" },
        { "{orange}", "\x10" }, // Map orange to gold
        { "{silver}", "\x0A" },
        { "{lightblue}", "\x0B" },
        { "{blue}", "\x0B" },
        { "{darkblue}", "\x0C" },
        { "{purple}", "\x0E" },
        { "{lightred}", "\x0F" },
        { "{darkorange}", "\x10" }, // Map darkorange to gold
        { "{olive}", "\x09" } // Map olive to yellow (closest match)
    };

    extension(string self)
    {
        public string ProcessColorString()
        {
            if (string.IsNullOrEmpty(self))
            {
                return self;
            }

            var result = self;

            foreach (var (placeholder, code) in ColorMap)
            {
                result = result.Replace(placeholder, code);
            }

            return result;
        }

        public string RemoveAllColors()
        {
            if (string.IsNullOrEmpty(self))
            {
                return self;
            }

            var result = ColorMap.Keys.Aggregate(self,
                (current, placeholder) => current.Replace(placeholder, string.Empty));

            // Remove color placeholders

            // Remove control codes
            var controlCodes = new[]
            {
                "\x01", "\x02", "\x03", "\x04", "\x05", "\x06", "\x07", "\x08",
                "\x09", "\x0A", "\x0B", "\x0C", "\x0D", "\x0E", "\x0F", "\x10"
            };

            foreach (var code in controlCodes)
            {
                result = result.Replace(code, string.Empty);
            }

            return result;
        }
    }
}