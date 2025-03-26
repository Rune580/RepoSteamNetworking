namespace RepoSteamNetworking.SourceGenerator.Utils;

internal static class StringUtils
{
    public static string ToPascalCase(this string text)
    {
        var result = "";
        var capitalizeNextChar = false;
        
        foreach (var character in text)
        {
            if (character is '_' or ' ')
            {
                capitalizeNextChar = true;
                continue;
            }
            
            if (string.IsNullOrWhiteSpace(result) || capitalizeNextChar)
            {
                result += char.ToUpper(character);
                capitalizeNextChar = false;
                continue;
            }
            
            result += character;
        }
        
        return result;
    }

    public static string ToLowerCamelCaseWithUnderscore(this string text)
    {
        var result = "_";
        var capitalizeNextChar = false;

        foreach (var character in text)
        {
            if (result == "_")
            {
                if (character is '_' or ' ')
                    continue;
                
                capitalizeNextChar = false;
                result += char.ToLower(character);
                continue;
            }
            
            if (character is '_' or ' ')
            {
                capitalizeNextChar = true;
                continue;
            }

            if (result.Length >= 2 && capitalizeNextChar)
            {
                result += char.ToUpper(character);
                capitalizeNextChar = false;
                continue;
            }
            
            result += character;
        }

        return result;
    }
}