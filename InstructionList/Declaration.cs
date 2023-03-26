namespace InstructionList;

public class Declaration
{
    // DEC NAME : TYPE : INIT : STORAGE_LOCATION; || Optional Comment

    public string Name;
    public string Type;
    public string InitalValue;
    public string StorageLocation;
    public string Comment;
    public const string Symbol = "DEC";

    public override string ToString()
    {
        var comment = Comment == "" ? "" : $" || {Comment}";
        return $"{Symbol} {Name} : {Type} : {InitalValue} : {StorageLocation};\t{comment}";
    }

    public static bool TryParse(string line, out Declaration declaration, out string e)
    {
        e = "";
        declaration = new Declaration();

        if (!line.StartsWith(Symbol))
        {
            e = $"Line does not start with {Symbol}";
            return false;
        }
        
        var parts = line.Split(new[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);

        if(parts.Length < 3)
        {
            e = "Declaration does not have enough parts, Expected\n```DEC NAME : TYPE : INIT : STORAGE_LOCATION;```\n"+
                $"FOUND: {line}";
            return false;
        }
        
        declaration.Name = parts[0].Replace(Symbol, "").Trim();
        declaration.Type = parts[1].Trim();
        declaration.InitalValue = parts[2].Trim();
        declaration.StorageLocation = parts[3].Trim();
        
        if(parts.Length > 4)
        {
            declaration.Comment = parts[4].Trim().Replace("||", "");
        }
        
        
        return true;
    }

    public static bool ParseFile(string programText, out Declaration[] programDeclarations, out string[] errorOut, out string withoutDeclarations)
    {
        var declarations = new List<Declaration>();
        List<string> errors = new();
        var notDec = new List<string>();
        
        var fileLines = programText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in fileLines)
        {
            if (!line.StartsWith(Declaration.Symbol))
            {
                notDec.Add(line);
                continue;
            }
            if (!TryParse(line, out var declaration, out string error))
            {
                errors.Add(error);
                continue;
            }

            declarations.Add(declaration);
        }
        
        
        errorOut = errors.ToArray();
        programDeclarations = declarations.ToArray();
        withoutDeclarations = notDec.Aggregate((a, b) => $"{a}{Environment.NewLine}{b}");
        return true;
    }
}