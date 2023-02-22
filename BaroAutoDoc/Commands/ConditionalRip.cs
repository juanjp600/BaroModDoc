﻿using System.Reflection;
using BaroAutoDoc.SyntaxWalkers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BaroAutoDoc.Commands;

sealed class ConditionalRip : Command
{
    public void Invoke()
    {
        Directory.SetCurrentDirectory(GlobalConfig.RepoPath);

        var page = new Page(); page.Title = "Conditionals";
        var intro = page.Body;

        const string srcPathFmt = "Barotrauma/Barotrauma{0}/{0}Source";
        var typeRipper = new ArbitraryTypeRipper("PropertyConditional");
        typeRipper.VisitAllInDirectory(string.Format(srcPathFmt, "Shared"));

        var declaration = typeRipper.Types.Single().Declaration as ClassDeclarationSyntax ?? throw new Exception("Type is not class");

        intro.Components.Add(new Page.InlineMarkdown(declaration.FindCommentAttachedToMember().Text));
        intro.Components.Add(new Page.NewLine());
        intro.Components.Add(new Page.NewLine());
        intro.Components.Add(new Page.RawText("Classes that use Conditionals: "));

        ArbitraryTypeRipper userFinder = new(syntax => syntax.ToString().Contains("PropertyConditional.FromXElement"));
        List<(string File, BaseTypeDeclarationSyntax Declaration)> users = new();
        void findUsers(string projectDir)
        {
            userFinder.Types.Clear();
            userFinder.VisitAllInDirectory(string.Format(srcPathFmt, projectDir));
            users.AddRange(userFinder.Types);
        }
        findUsers("Shared");
        findUsers("Client");
        findUsers("Server");
        var classNames = users.Select(c => c.Declaration.Identifier.Text).Distinct().ToArray();
        bool first = true;
        foreach (var className in classNames)
        {
            if (!first) { intro.Components.Add(new Page.RawText(", ")); }
            first = false;
            intro.Components.Add(new Page.RawText(className));

            void addLink(string file)
            {
                var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file).Replace("\\", "/");
                string url = $"https://github.com/Regalis11/Barotrauma/blob/master/{relativePath}";
                var sup = new Page.Superscript();
                string[] toMatch = { "Client", "Shared", "Server" };
                sup.Children.Add(new Page.Hyperlink(Url: url, Text: $"{toMatch.First(m => url.Contains($"{m}/{m}"))}", AltText: relativePath));
                intro!.Components.Add(sup);
            }
            foreach (var pair in users.Where(u => u.Declaration.Identifier.Text == className))
            {
                addLink(pair.File);
            }
        }

        EnumDeclarationSyntax findEnum(string enumName)
            => declaration.DescendantNodes()
                .OfType<EnumDeclarationSyntax>()
                .First(eds => eds.Identifier.Text.Equals(enumName));

        var conditionTypeEnum = findEnum("ConditionType");
        var comparisonOperatorEnum = findEnum("ComparisonOperatorType");
        //var logicalOperatorEnum = findEnum("LogicalOperatorType");

        static Page.Table createEnumTable(EnumDeclarationSyntax enumSyntax)
        {
            Page.Table table = new()
            {
                HeadRow = new Page.Table.Row("Value", "Description")
            };

            foreach (var member in enumSyntax.Members)
            {
                var comment = member.FindCommentAttachedToMember();
                var autoDocEntryName = comment.Element.ElementOfName("AutoDocEntryName") is { } entryNameElem
                    ? entryNameElem.GetAttributeValue("value")
                    : member.Identifier.Text;
                // TODO: fix Page.Table to be able to handle code blocks in cells
                /*var example = comment.Element.ElementOfName("example") is { } exampleElem
                    ? Page.CodeBlock.FromXElement(exampleElem).ToMarkdown()
                    : "";*/
                table.BodyRows.Add(new Page.Table.Row(autoDocEntryName, comment.Text));
            }

            return table;
        }

        var classParser = new ClassParser(new ClassParsingOptions());
        classParser.ParseType(declaration);
        
        var cursedAttributes = declaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "FromXElement")
            .Body!.Statements.OfType<LocalDeclarationStatementSyntax>()
            .Where(s => s.ToString().Contains("GetAttribute"))
            .Select(s => s.Declaration.Variables.First().Initializer)
            .OfType<EqualsValueClauseSyntax>()
            .Select(s => s.Value)
            .OfType<InvocationExpressionSyntax>()
            .Select(s => s.ArgumentList.Arguments.First().ToString().EvaluateAsCSharpExpression())
            .ToArray();

        var typesSection = new Page.Section(); page.Subsections.Add(typesSection);
        typesSection.Title = "Attributes";
        var typesTable = createEnumTable(conditionTypeEnum);
        typesTable.HeadRow!.Values[0] = "Attribute name";
        foreach (var cursedAttribute in cursedAttributes)
        {
            var field = classParser.DeclaredFields.First(f => f.Name == cursedAttribute);
            typesTable.BodyRows.Add(new Page.Table.Row(field.Name, field.Description));
        }
        typesSection.Body.Components.Add(typesTable);

        var comparisonOperatorSection = new Page.Section(); page.Subsections.Add(comparisonOperatorSection);
        comparisonOperatorSection.Title = "Comparison operators";
        comparisonOperatorSection.Body.Components.Add(new Page.InlineMarkdown(comparisonOperatorEnum.FindCommentAttachedToMember().Text));
        var comparisonOperatorTable = createEnumTable(comparisonOperatorEnum);
        comparisonOperatorTable.HeadRow!.Values[0] = "Operator";
        comparisonOperatorSection.Body.Components.Add(comparisonOperatorTable);

        var comparisonOperatorPrefixSwitch = declaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "GetComparisonOperatorType")
            .DescendantNodes().OfType<SwitchStatementSyntax>().First();
        foreach (var member in comparisonOperatorEnum.Members)
        {
            var matchingSection = comparisonOperatorPrefixSwitch.Sections
                .FirstOrDefault(s => s.Statements.Any(stmt => stmt.ToString().Contains(member.Identifier.Text)));
            if (matchingSection is null) { continue; }
            var labels = matchingSection.Labels
                .OfType<CaseSwitchLabelSyntax>()
                .Select(c => c.Value.ToString())
                .ToArray();
            comparisonOperatorTable.BodyRows.First(r => r.Values[0] == member.Identifier.Text).Values[0] = string.Join(", ", labels);
        }
        comparisonOperatorTable.BodyRows.RemoveAll(r => string.IsNullOrWhiteSpace(r.Values[0]));

        /*var logicalOperatorSection = new Page.Section(); page.Subsections.Add(logicalOperatorSection);
        logicalOperatorSection.Title = "Logical operators";
        logicalOperatorSection.Body.Components.Add(new Page.InlineMarkdown("Logical operators determine how multiple conditionals are combined."));
        logicalOperatorSection.Body.Components.Add(createEnumTable(logicalOperatorEnum));*/
        
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!);

        File.WriteAllText("Conditional.md", page.ToMarkdown());
    }
}
