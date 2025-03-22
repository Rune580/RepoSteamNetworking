using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace RepoSteamNetworking.SourceGenerator.Utils;

internal class CodeBuilder
{
    private readonly StringBuilder _body = new();

    private readonly HashSet<string> _imports = [];
    private string _currentNamespace = string.Empty;
    private int _indentLevel;

    public CodeBuilder WithImport(string import)
    {
        _imports.Add(import);
        return this;
    }

    public CodeBuilder WithImports(params string[] imports)
    {
        foreach (var import in imports)
            _imports.Add(import);
        return this;
    }

    public CodeBuilder WithNamespace(string @namespace)
    {
        _currentNamespace = @namespace;
        _indentLevel = 1;
        return this;
    }

    public CodeBuilder IncreaseIndent()
    {
        _indentLevel++;
        return this;
    }

    public CodeBuilder DecreaseIndent()
    {
        _indentLevel--;
        if (_indentLevel < 0)
            _indentLevel = 0;
        
        return this;
    }

    public CodeBuilder AppendLine(string text)
    {
        var newLines = new[] { "\r\n", "\n" };
        var lines = text.Split(newLines, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine == "}")
                DecreaseIndent();
            
            AppendLineInternal(trimmedLine);

            if (trimmedLine == "{")
                IncreaseIndent();
        }
        
        return this;
    }

    private void AppendLineInternal(string line)
    {
        for (int i = 0; i < _indentLevel; i++)
            _body.Append('\t');

        _body.AppendLine($"{line}");
    }

    public override string ToString()
    {
        while (_indentLevel > 1)
            AppendLine("}");
        
        var code = new StringBuilder();

        foreach (var import in _imports)
            code.AppendLine($"using {import};");

        code.AppendLine($"namespace {_currentNamespace}\n{{");
        
        code.Append(_body);

        code.Append("}");
        
        return code.ToString();
    }

    public SourceText ToSourceText(Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        
        return SourceText.From(ToString(), encoding);
    }
}